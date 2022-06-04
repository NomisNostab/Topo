using FastReport;
using Syncfusion.XlsIO;
using System.Globalization;
using Topo.Images;
using Topo.Models.Events;
using Topo.Models.MemberList;

namespace Topo.Services
{
    public interface IEventService
    {
        public Task<List<CalendarListModel>> GetCalendars();
        public Task SetCalendar(string calendarId);
        public Task<List<EventListModel>> GetEventsForDates(DateTime fromDate, DateTime toDate);
        public Task<EventListModel> GetAttendanceForEvent(string eventId);
        public Task<AttendanceReportModel> GenerateAttendanceReportData(DateTime fromDate, DateTime toDate, string selectedCalendar);
        public IWorkbook GenerateEventAttendanceXlsx(EventListModel eventListModel, string section, string unitName);
        public IWorkbook GenerateSignInSheet(List<MemberListModel> memberListModel, string section, string unitName, string eventName);
    }

    public class EventService : IEventService
    {
        private readonly StorageService _storageService;
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly IMemberListService _memberListService;
        private readonly IImages _images;
        public EventService(StorageService storageService, ITerrainAPIService terrainAPIService, IMemberListService memberListService, IImages images)
        {
            _storageService = storageService;
            _terrainAPIService = terrainAPIService;
            _memberListService = memberListService;
            _images = images;
        }

        public async Task<List<CalendarListModel>> GetCalendars()
        {
            var getCalendarsResultModel = await _terrainAPIService.GetCalendarsAsync(GetUser());
            if (getCalendarsResultModel != null && getCalendarsResultModel.own_calendars != null)
            {
                var calendars = getCalendarsResultModel.own_calendars.Where(c => c.type == "unit")
                    .Select(e => new CalendarListModel()
                    {
                        Id = e.id,
                        Title = e.title
                    })
                    .ToList();
                _storageService.GetCalendarsResult = getCalendarsResultModel;
                return calendars;
            }
            return new List<CalendarListModel>();
        }

        public async Task SetCalendar(string calendarId)
        {
            foreach (var calendar in _storageService.GetCalendarsResult.own_calendars)
            {
                calendar.selected = calendar.id == calendarId;
            }
            await _terrainAPIService.PutCalendarsAsync(GetUser(), _storageService.GetCalendarsResult);
        }

        public async Task<List<EventListModel>> GetEventsForDates(DateTime fromDate, DateTime toDate)
        {
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            var getEventsResultModel = await _terrainAPIService.GetEventsAsync(GetUser(), fromDate, toDate);
            if (getEventsResultModel != null && getEventsResultModel.results != null)
            {
                var events = getEventsResultModel.results.Select(e => new EventListModel()
                {
                    Id = e.id,
                    EventName = e.title,
                    StartDateTime = e.start_datetime,
                    EndDateTime = e.end_datetime,
                    ChallengeArea = myTI.ToTitleCase(e.challenge_area.Replace("_", " "))
                })
                    .ToList();
                return events;
            }
            return new List<EventListModel>();
        }

        public async Task<EventListModel> GetAttendanceForEvent(string eventId)
        {
            var eventListModel = new EventListModel();
            var getEventResultModel = await _terrainAPIService.GetEventAsync(eventId);
            var eventAttendance = new List<EventAttendance>();
            var members = await _memberListService.GetMembersAsync();
            foreach (var member in members)
            {
                eventAttendance.Add(new EventAttendance
                {
                    first_name = member.first_name,
                    last_name = member.last_name,
                    member_number = member.member_number,
                    patrol_name = member.patrol_name,
                    isAdultMember = member.isAdultLeader == 1,
                    attended = false
                });
            }

            if (getEventResultModel != null && getEventResultModel.attendance != null && getEventResultModel.attendance.attendee_members != null)
            {
                eventListModel.Id = eventId;
                eventListModel.EventName = getEventResultModel.title;
                eventListModel.StartDateTime = getEventResultModel.start_datetime;
                foreach (var attended in getEventResultModel.attendance.attendee_members)
                {
                    eventAttendance.Where(a => a.member_number == attended.member_number).Single().attended = true;
                }
                eventListModel.attendees = eventAttendance.ToArray();
                return eventListModel;
            }
            return new EventListModel();
        }


        public async Task<AttendanceReportModel> GenerateAttendanceReportData(DateTime fromDate, DateTime toDate, string selectedCalendar)
        {
            var attendanceReport = new AttendanceReportModel();
            var attendanceReportItems = new List<AttendanceReportItemModel>();
            await SetCalendar(selectedCalendar);
            var members = await _memberListService.GetMembersAsync();
            //
            var programEvents = await GetEventsForDates(fromDate, toDate);
            foreach (var programEvent in programEvents.OrderBy(pe => pe.StartDateTime))
            {
                var eventListModel = await GetAttendanceForEvent(programEvent.Id);
                programEvent.attendees = eventListModel.attendees;
                foreach (var member in members)
                {
                    var attended = programEvent.attendees.Any(a => a.id == member.id);
                    attendanceReportItems.Add(new AttendanceReportItemModel
                    {
                        MemberId = member.id,
                        MemberName = $"{member.first_name} {member.last_name}",
                        EventName = programEvent.EventName,
                        EventNameDisplay = programEvent.EventDisplay,
                        EventChallengeArea = programEvent.ChallengeArea,
                        EventStartDate = programEvent.StartDateTime,
                        Attended = attended ? 1 : 0,
                        IsAdultMember = member.isAdultLeader
                    });
                }
            }
            attendanceReport.attendanceReportItems = attendanceReportItems;

            var memberSummaries = new List<AttendanceReportMemberSummaryModel>();
            var attendanceReportItemsGroupedByMember = attendanceReportItems.GroupBy(a => a.MemberId);
            foreach (var memberAttendance in attendanceReportItemsGroupedByMember)
            {
                var attendedCount = memberAttendance.Where(ma => ma.EventStartDate <= DateTime.Now).Sum(ma => ma.Attended);
                var totalEvents = memberAttendance.Where(ma => ma.EventStartDate <= DateTime.Now).Count();
                memberSummaries.Add(new AttendanceReportMemberSummaryModel
                {
                    MemberId = memberAttendance.Key,
                    MemberName = memberAttendance.FirstOrDefault()?.MemberName ?? "",
                    IsAdultMember = memberAttendance.FirstOrDefault()?.IsAdultMember ?? 0,
                    AttendanceCount = attendedCount,
                    TotalEvents = totalEvents
                });
            }

            foreach (var attendanceItem in attendanceReportItems)
            {
                var attendanceCount = memberSummaries.Where(ms => ms.MemberId == attendanceItem.MemberId).FirstOrDefault()?.AttendanceCount ?? 0;
                var totalEvents = memberSummaries.Where(ms => ms.MemberId == attendanceItem.MemberId).FirstOrDefault()?.TotalEvents ?? 0;
                var attendanceRate = (decimal)attendanceCount / totalEvents * 100m;
                attendanceItem.MemberNameAndRate = $"{attendanceItem.MemberName} ({Math.Round(attendanceRate, 0)}%)";
            }

            var challengeAreaSummaries = new List<AttendanceReportChallengeAreaSummaryModel>();
            var programEventsGroupedByChallengeArea = programEvents.GroupBy(a => a.ChallengeArea);
            foreach (var challengeArea in programEventsGroupedByChallengeArea)
            {
                challengeAreaSummaries.Add(new AttendanceReportChallengeAreaSummaryModel
                {
                    ChallengeArea = challengeArea.Key,
                    EventCount = challengeArea.Count(),
                    TotalEvents = programEvents.Count()
                });
            }
            attendanceReport.attendanceReportChallengeAreaSummaries = challengeAreaSummaries;

            return attendanceReport;
        }

        public IWorkbook GenerateSignInSheet(List<MemberListModel> memberListModel, string section, string unitName, string eventName)
        {
            //Step 1 : Instantiate the spreadsheet creation engine.
            ExcelEngine excelEngine = new ExcelEngine();
            //Step 2 : Instantiate the excel application object.
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2016;

            // Creating new workbook
            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet sheet = workbook.Worksheets[0];

            int rowNumber = 1;
            int columnNumber = 1;

            // Add Logo
            var directory = Directory.GetCurrentDirectory();
            var logoName = _images.GetLogoForSection(section);
            FileStream imageStream = new FileStream($@"{directory}/Images/{logoName}", FileMode.Open, FileAccess.Read);
            IPictureShape logo = sheet.Pictures.AddPicture(rowNumber, 1, imageStream);
            var aspectRatio = (double)logo.Height / logo.Width;
            logo.Width = 100;
            logo.Height = (int)(100 * aspectRatio);
            sheet.SetColumnWidthInPixels(1, 100);

            //Adding cell style.               
            IStyle headingStyle = workbook.Styles.Add("headingStyle");
            headingStyle.Font.Bold = true;
            headingStyle.Font.Size = 14;
            headingStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            headingStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;

            // Add Group Name
            var groupName = sheet.Range[rowNumber, 2];
            groupName.Text = _storageService.GroupName;
            groupName.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 7].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 7].Merge();
            sheet.SetRowHeight(rowNumber, 40);

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = eventName;
            title.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 7].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            // Add Header
            rowNumber++;
            sheet.Range[rowNumber, 1].Text = "Name";
            sheet.Range[rowNumber, 2].Text = "Number";
            sheet.Range[rowNumber, 3].Text = "Patrol";
            sheet.Range[rowNumber, 4].Text = "Role";
            sheet.Range[rowNumber, 5].Text = "Registered";
            sheet.Range[rowNumber, 6].Text = "Paid";
            sheet.Range[rowNumber, 7].Text = "Attended";
            sheet.Range[rowNumber, 5, rowNumber, 7].CellStyle.Rotation = 90;
            sheet.Range[rowNumber, 8].Text = "Name";
            sheet.Range[rowNumber, 1, rowNumber, 8].CellStyle.Font.Bold = true;

            foreach (var member in memberListModel.Where(m => m.isAdultLeader == 0))
            {
                rowNumber++;
                sheet.Range[rowNumber, 1].Text = $"{member.first_name} {member.last_name}";
                sheet.Range[rowNumber, 1].BorderAround();
                sheet.Range[rowNumber, 2].Text = member.member_number;
                sheet.Range[rowNumber, 2].BorderAround();
                sheet.Range[rowNumber, 3].Text = member.patrol_name;
                sheet.Range[rowNumber, 3].BorderAround();
                sheet.Range[rowNumber, 4].Text = member.patrol_duty;
                sheet.Range[rowNumber, 4].BorderAround();
                sheet.Range[rowNumber, 5].Text = "";
                sheet.Range[rowNumber, 5].BorderAround();
                sheet.Range[rowNumber, 6].Text = "";
                sheet.Range[rowNumber, 6].BorderAround();
                sheet.Range[rowNumber, 7].Text = "";
                sheet.Range[rowNumber, 7].BorderAround();
                sheet.Range[rowNumber, 8].Text = member.first_name;
                sheet.Range[rowNumber, 8].BorderAround();
            }

            rowNumber++;
            // Add Header
            rowNumber++;
            sheet.Range[rowNumber, 1].Text = "Name";
            sheet.Range[rowNumber, 2].Text = "Number";
            sheet.Range[rowNumber, 3].Text = "Patrol";
            sheet.Range[rowNumber, 4].Text = "Role";
            sheet.Range[rowNumber, 5].Text = "Registered";
            sheet.Range[rowNumber, 6].Text = "Paid";
            sheet.Range[rowNumber, 7].Text = "Attended";
            sheet.Range[rowNumber, 5, rowNumber, 7].CellStyle.Rotation = 90;
            sheet.Range[rowNumber, 8].Text = "Name";
            sheet.Range[rowNumber, 1, rowNumber, 8].CellStyle.Font.Bold = true;

            foreach (var member in memberListModel.Where(m => m.isAdultLeader == 1))
            {
                rowNumber++;
                sheet.Range[rowNumber, 1].Text = $"{member.first_name} {member.last_name}";
                sheet.Range[rowNumber, 1].BorderAround();
                sheet.Range[rowNumber, 2].Text = member.member_number;
                sheet.Range[rowNumber, 2].BorderAround();
                sheet.Range[rowNumber, 3].Text = "";
                sheet.Range[rowNumber, 3].BorderAround();
                sheet.Range[rowNumber, 4].Text = "";
                sheet.Range[rowNumber, 4].BorderAround();
                sheet.Range[rowNumber, 5].Text = "";
                sheet.Range[rowNumber, 5].BorderAround();
                sheet.Range[rowNumber, 6].Text = "";
                sheet.Range[rowNumber, 6].BorderAround();
                sheet.Range[rowNumber, 7].Text = "";
                sheet.Range[rowNumber, 7].BorderAround();
                sheet.Range[rowNumber, 8].Text = member.first_name;
                sheet.Range[rowNumber, 8].BorderAround();
            }

            sheet.UsedRange.AutofitColumns();

            return workbook;
        }
        public IWorkbook GenerateEventAttendanceXlsx(EventListModel eventListModel, string section, string unitName)
        {
            //Step 1 : Instantiate the spreadsheet creation engine.
            ExcelEngine excelEngine = new ExcelEngine();
            //Step 2 : Instantiate the excel application object.
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2016;

            // Creating new workbook
            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet sheet = workbook.Worksheets[0];

            int rowNumber = 1;
            int columnNumber = 1;

            // Add Logo
            var directory = Directory.GetCurrentDirectory();
            var logoName = _images.GetLogoForSection(section);
            FileStream imageStream = new FileStream($@"{directory}/Images/{logoName}", FileMode.Open, FileAccess.Read);
            IPictureShape logo = sheet.Pictures.AddPicture(rowNumber, 1, imageStream);
            var aspectRatio = (double)logo.Height / logo.Width;
            logo.Width = 100;
            logo.Height = (int)(100 * aspectRatio);
            sheet.SetColumnWidthInPixels(1, 100);

            //Adding cell style.               
            IStyle headingStyle = workbook.Styles.Add("headingStyle");
            headingStyle.Font.Bold = true;
            headingStyle.Font.Size = 14;
            headingStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            headingStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;

            // Add Group Name
            var groupName = sheet.Range[rowNumber, 2];
            groupName.Text = _storageService.GroupName;
            groupName.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 7].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 7].Merge();
            sheet.SetRowHeight(rowNumber, 40);

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = eventListModel.EventDisplay;
            title.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 7].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            // Add Header
            rowNumber++;
            sheet.Range[rowNumber, 2, rowNumber, 6].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[rowNumber, 2].Text = "First Name";
            sheet.Range[rowNumber, 2].BorderAround();
            sheet.Range[rowNumber, 3].Text = "Last Name";
            sheet.Range[rowNumber, 3].BorderAround();
            sheet.Range[rowNumber, 4].Text = "Member";
            sheet.Range[rowNumber, 4].BorderAround();
            sheet.Range[rowNumber, 5].Text = "Patrol";
            sheet.Range[rowNumber, 5].BorderAround();
            sheet.Range[rowNumber, 6].Text = "Attended";
            sheet.Range[rowNumber, 6].BorderAround();

            foreach (var attendee in eventListModel.attendees.Where(a => !a.isAdultMember).OrderBy(a => a.last_name))
            {
                rowNumber++;
                sheet.Range[rowNumber, 2].Text = attendee.first_name;
                sheet.Range[rowNumber, 2].BorderAround();
                sheet.Range[rowNumber, 3].Text = attendee.last_name;
                sheet.Range[rowNumber, 3].BorderAround();
                sheet.Range[rowNumber, 4].Text = attendee.member_number;
                sheet.Range[rowNumber, 4].BorderAround();
                sheet.Range[rowNumber, 5].Text = attendee.patrol_name;
                sheet.Range[rowNumber, 5].BorderAround();
                sheet.Range[rowNumber, 6].Text = attendee.attended ? "Y" : "";
                sheet.Range[rowNumber, 6].BorderAround();
            }

            rowNumber++;

            foreach (var attendee in eventListModel.attendees.Where(a => a.isAdultMember).OrderBy(a => a.last_name))
            {
                rowNumber++;
                sheet.Range[rowNumber, 2].Text = attendee.first_name;
                sheet.Range[rowNumber, 2].BorderAround();
                sheet.Range[rowNumber, 3].Text = attendee.last_name;
                sheet.Range[rowNumber, 3].BorderAround();
                sheet.Range[rowNumber, 4].Text = attendee.member_number;
                sheet.Range[rowNumber, 4].BorderAround();
                sheet.Range[rowNumber, 5].Text = attendee.patrol_name;
                sheet.Range[rowNumber, 5].BorderAround();
                sheet.Range[rowNumber, 6].Text = attendee.attended ? "Y" : "";
                sheet.Range[rowNumber, 6].BorderAround();
            }

            sheet.UsedRange.AutofitColumns();

            return workbook;

        }
        private string GetUser()
        {
            var userId = "";
            if (_storageService.GetProfilesResult != null && _storageService.GetProfilesResult.profiles != null && _storageService.GetProfilesResult.profiles.Length > 0)
            {
                userId = _storageService.GetProfilesResult.profiles[0].member?.id;
            }
            return userId;
        }
    }
}

