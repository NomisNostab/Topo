using Newtonsoft.Json;
using System.Globalization;
using Topo.Images;
using Topo.Models.Events;

namespace Topo.Services
{
    public interface IEventService
    {
        public Task<List<CalendarListModel>> GetCalendars();
        public Task SetCalendar(string calendarId);
        public Task ResetCalendar();
        public Task<List<EventListModel>> GetEventsForDates(DateTime fromDate, DateTime toDate);
        public Task<EventListModel> GetAttendanceForEvent(string eventId);
        public Task<AttendanceReportModel> GenerateAttendanceReportData(DateTime fromDate, DateTime toDate, string selectedCalendar);
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
            //Deep copy calendar to allow reset
            var serialisedCalendar = JsonConvert.SerializeObject(_storageService.GetCalendarsResult);
            var calendars = JsonConvert.DeserializeObject<GetCalendarsResultModel>(serialisedCalendar);

            foreach (var calendar in calendars.own_calendars)
            {
                calendar.selected = calendar.id == calendarId;
            }

            foreach (var calendar in calendars.other_calendars)
            {
                calendar.selected = false;
            }

            await _terrainAPIService.PutCalendarsAsync(GetUser(), calendars);
        }

        public async Task ResetCalendar()
        {
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
                    ChallengeArea = myTI.ToTitleCase(e.challenge_area.Replace("_", " ")),
                    EventStatus = myTI.ToTitleCase(e.status)
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
            var members = await _memberListService.GetMembersAsync(_storageService.SelectedUnitId);
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

            if (getEventResultModel != null && getEventResultModel.attendance != null && getEventResultModel.attendance.attendee_members != null && getEventResultModel.attendance.attendee_members.Any())
            {
                eventListModel.Id = eventId;
                eventListModel.EventName = getEventResultModel.title;
                eventListModel.StartDateTime = getEventResultModel.start_datetime;
                foreach (var attended in getEventResultModel.attendance.attendee_members)
                {
                    if (eventAttendance.Any(a => a.member_number == attended.member_number))
                    {
                        eventAttendance.Where(a => a.member_number == attended.member_number).Single().attended = true;
                    }
                }
                eventListModel.attendees = eventAttendance.ToArray();
                return eventListModel;
            }

            // for older events participant_members seems to be used  not attendee_members
            if (getEventResultModel != null && getEventResultModel.attendance != null && getEventResultModel.attendance.participant_members != null && getEventResultModel.attendance.participant_members.Any())
            {
                eventListModel.Id = eventId;
                eventListModel.EventName = getEventResultModel.title;
                eventListModel.StartDateTime = getEventResultModel.start_datetime;
                foreach (var attended in getEventResultModel.attendance.participant_members)
                {
                    if (eventAttendance.Any(a => a.member_number == attended.member_number))
                    {
                        eventAttendance.Where(a => a.member_number == attended.member_number).Single().attended = true;
                    }
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
            var members = await _memberListService.GetMembersAsync(_storageService.SelectedUnitId);
            //
            var programEvents = await GetEventsForDates(fromDate, toDate);
            await ResetCalendar();
            foreach (var programEvent in programEvents.OrderBy(pe => pe.StartDateTime))
            {
                var eventListModel = await GetAttendanceForEvent(programEvent.Id);
                programEvent.attendees = eventListModel.attendees;
                foreach (var member in members)
                {
                    var attended = programEvent.attendees.Where(a => a.member_number == member.member_number).SingleOrDefault()?.attended ?? false;
                    attendanceReportItems.Add(new AttendanceReportItemModel
                    {
                        MemberId = member.id,
                        MemberName = $"{member.first_name} {member.last_name}",
                        EventName = programEvent.EventName,
                        EventNameDisplay = programEvent.EventDisplay,
                        EventChallengeArea = programEvent.ChallengeArea,
                        EventStartDate = programEvent.StartDateTime,
                        Attended = attended ? 1 : 0,
                        IsAdultMember = member.isAdultLeader,
                        EventStatus = programEvent.EventStatus
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
                var attendanceRate = totalEvents == 0 ? 0 : (decimal)attendanceCount / totalEvents * 100m;
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

