using Syncfusion.XlsIO;
using Topo.Images;
using Topo.Models.AditionalAwards;
using Topo.Models.Events;
using Topo.Models.MemberList;
using Topo.Models.OAS;

namespace Topo.Services
{
    public interface IReportService
    {
        public IWorkbook BuildAdditionalAwardsWorkbook(List<AdditionalAwardSpecificationListModel> awardSpecificationsList, List<AdditionalAwardListModel> sortedAdditionalAwardsList, List<string>? distinctAwards, string groupName, string section, string unitName);
        public IWorkbook GeneratePatrolListWorkbook(List<MemberListModel> sortedPatrolList, string groupName, string section, string unitName, bool includeLeaders);
        public IWorkbook GenerateMemberListWorkbook(List<MemberListModel> sortedMemberList, string groupName, string section, string unitName);
        public IWorkbook GeneratePatrolSheetsWorkbook(List<MemberListModel> sortedPatrolList, string section);
        public IWorkbook GenerateSignInSheetWorkbook(List<MemberListModel> memberListModel, string groupName, string section, string unitName, string eventName);
        public IWorkbook GenerateEventAttendanceWorkbook(EventListModel eventListModel, string groupName, string section, string unitName);
        public IWorkbook GenerateAttendanceReportWorkbook(AttendanceReportModel attendanceReportData, string groupName, string section, string unitName, DateTime fromDate, DateTime toDate, bool forPdfOutput);
        public IWorkbook GenerateOASWorksheetWorkbook(List<OASWorksheetAnswers> worksheetAnswers, string groupName, string section, string unitName, string templateTitle, bool forPdfOutput);
    }
    public class ReportService : IReportService
    {
        private readonly IImages _images;

        public ReportService(IImages images)
        {
            _images = images;
        }

        private IWorkbook CreateWorkbookWithLogo(string groupName, string section, int lastHeadingCol)
        {
            //Step 1 : Instantiate the spreadsheet creation engine.
            ExcelEngine excelEngine = new ExcelEngine();
            //Step 2 : Instantiate the excel application object.
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2016;

            // Creating new workbook
            IWorkbook workbook = application.Workbooks.Create(1);
            IWorksheet sheet = workbook.Worksheets[0];

            // Add Logo
            var directory = Directory.GetCurrentDirectory();
            var logoName = _images.GetLogoForSection(section);
            FileStream imageStream = new FileStream($@"{directory}/Images/{logoName}", FileMode.Open, FileAccess.Read);
            IPictureShape logo = sheet.Pictures.AddPicture(1, 1, imageStream);
            var aspectRatio = (double)logo.Height / logo.Width;
            logo.Width = 100;
            logo.Height = (int)(100 * aspectRatio);

            //Adding cell style.               
            IStyle headingStyle = workbook.Styles.Add("headingStyle");
            headingStyle.Font.Bold = true;
            headingStyle.Font.Size = 14;
            headingStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            headingStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;

            // Add Group Name
            var groupNameCell = sheet.Range[1, 2];
            groupNameCell.Text = groupName;
            groupNameCell.CellStyle = headingStyle;
            sheet.Range[1, 2, 1, lastHeadingCol].Merge();
            sheet.SetRowHeight(1, 30);

            return workbook;
        }
        public IWorkbook BuildAdditionalAwardsWorkbook(List<AdditionalAwardSpecificationListModel> awardSpecificationsList, List<AdditionalAwardListModel> sortedAdditionalAwardsList, List<string>? distinctAwards, string groupName, string section, string unitName)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 16);
            IWorksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;

            IStyle headingStyle = workbook.Styles["headingStyle"];

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = $"Additional Badges Awarded as at {DateTime.Now.ToShortDateString()}";
            title.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 16].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 16].Merge();
            sheet.SetRowHeight(rowNumber, 40);

            //Add Heading Row
            rowNumber++;
            var columnNumber = 1;
            sheet.Range[rowNumber, 1].Text = "Scout";
            sheet.Range[rowNumber, 1].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            var usedAwards = awardSpecificationsList.Where(x => distinctAwards.Contains(x.id));
            foreach (var award in usedAwards.OrderBy(x => x.additionalAwardSortIndex))
            {
                columnNumber = distinctAwards.IndexOf(award.id) + 1;
                var cell = sheet.Range[rowNumber, columnNumber + 1];
                cell.Text = award.name;
                cell.CellStyle.Rotation = 90;
                cell.CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                cell.CellStyle.VerticalAlignment = ExcelVAlign.VAlignBottom;
                cell.BorderAround();
            }
            rowNumber++;
            // Add detail rows
            foreach (var additionalAward in sortedAdditionalAwardsList.GroupBy(x => x.MemberName))
            {
                sheet.SetRowHeight(rowNumber, 15);
                sheet.Range[rowNumber, 1].Text = additionalAward.Key;
                sheet.Range[rowNumber, 1].BorderAround();
                sheet.Range[rowNumber, 1].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                // Set style for date boxes
                for (int i = 0; i < distinctAwards.Count(); i++)
                {
                    var cell = sheet.Range[rowNumber, i + 2];
                    cell.CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    cell.CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    cell.BorderAround();
                }
                // Populate award dates
                foreach (var x in additionalAward)
                {
                    columnNumber = distinctAwards.IndexOf(x.AwardId) + 1;
                    var cell = sheet.Range[rowNumber, columnNumber + 1];
                    cell.Text = x.AwardDate.Value.ToString("dd/MM/yy");
                }
                rowNumber++;
            }
            sheet.UsedRange.AutofitColumns();
            return workbook;
        }

        public IWorkbook GeneratePatrolListWorkbook(List<MemberListModel> sortedPatrolList,string groupName, string section, string unitName, bool includeLeaders)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 9);
            IWorksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;

            IStyle headingStyle = workbook.Styles["headingStyle"];

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = $"Patrols as at {DateTime.Now.ToShortDateString()}";
            title.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 9].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 9].Merge();
            sheet.SetRowHeight(rowNumber, 40);

            var groupedMembersList = sortedPatrolList.Where(pl => pl.isAdultLeader == 0).GroupBy(pl => pl.patrol_name).ToList();
            foreach (var patrol in groupedMembersList)
            {
                // Add Patrol Name
                rowNumber++;
                sheet.Range[rowNumber, 1].Text = patrol.Key;
                sheet.Range[rowNumber, 1].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 1].CellStyle.Font.Size = 14;
                sheet.Range[rowNumber, 1].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[rowNumber, 1, rowNumber, 3].Merge();
                sheet.SetRowHeight(rowNumber, 20);

                // Add Patrol members
                foreach (var member in patrol.OrderByDescending(p => p.patrol_duty))
                {
                    rowNumber++;
                    sheet.Range[rowNumber, 1].Text = member.first_name;
                    sheet.Range[rowNumber, 1].BorderAround();
                    sheet.Range[rowNumber, 2].Text = member.last_name;
                    sheet.Range[rowNumber, 2].BorderAround();
                    sheet.Range[rowNumber, 3].Text = member.member_number;
                    sheet.Range[rowNumber, 3].BorderAround();
                    sheet.Range[rowNumber, 4].Text = member.patrol_duty;
                    sheet.Range[rowNumber, 4].BorderAround();
                    if (!string.IsNullOrEmpty(member.patrol_duty))
                    {
                        sheet.Range[rowNumber, 1, rowNumber, 4].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    }
                }
                rowNumber++;
            }

            rowNumber = 3;
            var unitCouncilList = sortedPatrolList.Where(pl => pl.unit_council)
                .OrderBy(uc => uc.isAdultLeader)
                .ThenBy(uc => uc.first_name)
                .ThenBy(uc => uc.last_name)
                .ToList();
            // Add Unit Council Name
            rowNumber++;
            sheet.Range[rowNumber, 6].Text = "Unit Council";
            sheet.Range[rowNumber, 6].CellStyle.Font.Bold = true;
            sheet.Range[rowNumber, 6].CellStyle.Font.Size = 14;
            sheet.Range[rowNumber, 6].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[rowNumber, 6, rowNumber, 9].Merge();
            sheet.SetRowHeight(rowNumber, 20);

            foreach (var member in unitCouncilList)
            {
                // Add Unit Council members
                rowNumber++;
                sheet.Range[rowNumber, 6].Text = member.first_name;
                sheet.Range[rowNumber, 6].BorderAround();
                sheet.Range[rowNumber, 7].Text = member.last_name;
                sheet.Range[rowNumber, 7].BorderAround();
                sheet.Range[rowNumber, 8].Text = member.member_number;
                sheet.Range[rowNumber, 8].BorderAround();
                sheet.Range[rowNumber, 9].Text = member.patrol_duty;
                sheet.Range[rowNumber, 9].BorderAround();
            }
            rowNumber++;

            if (includeLeaders)
            {
                var adultLeaderList = sortedPatrolList
                    .Where(uc => uc.isAdultLeader == 1)
                    .OrderBy(uc => uc.first_name)
                    .ThenBy(uc => uc.last_name)
                    .ToList();
                // Add Adult Leaders Name
                rowNumber++;
                sheet.Range[rowNumber, 6].Text = "Adult Leaders";
                sheet.Range[rowNumber, 6].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 6].CellStyle.Font.Size = 14;
                sheet.Range[rowNumber, 6].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[rowNumber, 6, rowNumber, 9].Merge();
                sheet.SetRowHeight(rowNumber, 20);

                foreach (var member in adultLeaderList)
                {
                    // Add Unit Council members
                    rowNumber++;
                    sheet.Range[rowNumber, 6].Text = member.first_name;
                    sheet.Range[rowNumber, 6].BorderAround();
                    sheet.Range[rowNumber, 7].Text = member.last_name;
                    sheet.Range[rowNumber, 7].BorderAround();
                    sheet.Range[rowNumber, 8].Text = member.member_number;
                    sheet.Range[rowNumber, 8].BorderAround();
                    sheet.Range[rowNumber, 9].Text = member.patrol_duty;
                    sheet.Range[rowNumber, 9].BorderAround();
                }
            }

            sheet.UsedRange.AutofitColumns();

            return workbook;
        }

        public IWorkbook GenerateMemberListWorkbook(List<MemberListModel> sortedMemberList, string groupName, string section, string unitName)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 7);
            IWorksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;

            IStyle headingStyle = workbook.Styles["headingStyle"];

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = $"Members as at {DateTime.Now.ToShortDateString()}";
            title.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 7].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 7].Merge();
            sheet.SetRowHeight(rowNumber, 40);

            // Add Header
            rowNumber++;
            sheet.Range[rowNumber, 2, rowNumber, 7].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[rowNumber, 2].Text = "First Name";
            sheet.Range[rowNumber, 2].BorderAround();
            sheet.Range[rowNumber, 3].Text = "Last Name";
            sheet.Range[rowNumber, 3].BorderAround();
            sheet.Range[rowNumber, 4].Text = "Birthday";
            sheet.Range[rowNumber, 4].BorderAround();
            sheet.Range[rowNumber, 5].Text = "Age";
            sheet.Range[rowNumber, 5].BorderAround();
            sheet.Range[rowNumber, 6].Text = "Member";
            sheet.Range[rowNumber, 6].BorderAround();
            sheet.Range[rowNumber, 7].Text = "Duty";
            sheet.Range[rowNumber, 7].BorderAround();

            foreach (var member in sortedMemberList)
            {
                rowNumber++;
                sheet.Range[rowNumber, 2].Text = member.first_name;
                sheet.Range[rowNumber, 2].BorderAround();
                sheet.Range[rowNumber, 3].Text = member.last_name;
                sheet.Range[rowNumber, 3].BorderAround();
                sheet.Range[rowNumber, 4].Text = member.date_of_birth.ToShortDateString();
                sheet.Range[rowNumber, 4].BorderAround();
                sheet.Range[rowNumber, 5].Text = member.age;
                sheet.Range[rowNumber, 5].BorderAround();
                sheet.Range[rowNumber, 6].Text = member.member_number;
                sheet.Range[rowNumber, 6].BorderAround();
                sheet.Range[rowNumber, 7].Text = member.patrol_duty;
                sheet.Range[rowNumber, 7].BorderAround();

                var dateDiff = DateTime.Now - member.date_of_birth;
                var approxYears = dateDiff.Days / 365.0;
                if (approxYears > UnitMaxAge(section) - 2)
                    sheet.Range[rowNumber, 5].CellStyle.ColorIndex = ExcelKnownColors.Yellow;
                if (approxYears > UnitMaxAge(section) - 1)
                    sheet.Range[rowNumber, 5].CellStyle.ColorIndex = ExcelKnownColors.Rose;
            }

            sheet.UsedRange.AutofitColumns();

            return workbook;
        }

        public IWorkbook GeneratePatrolSheetsWorkbook(List<MemberListModel> sortedPatrolList, string section)
        {
            //Step 1 : Instantiate the spreadsheet creation engine.
            ExcelEngine excelEngine = new ExcelEngine();
            //Step 2 : Instantiate the excel application object.
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2016;


            var groupedMembersList = sortedPatrolList.Where(pl => pl.isAdultLeader == 0).GroupBy(pl => pl.patrol_name).ToList();

            // Creating new workbook
            IWorkbook workbook = application.Workbooks.Create(groupedMembersList.Count);

            //Adding cell style.               
            IStyle headingStyle = workbook.Styles.Add("headingStyle");
            headingStyle.Font.Bold = true;
            headingStyle.Font.Size = 40;
            headingStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            headingStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
            headingStyle.WrapText = true;

            IStyle rowStyle = workbook.Styles.Add("rowStyle");
            rowStyle.Font.Bold = true;
            rowStyle.Font.Size = 20;
            rowStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;

            var sheetIndex = 0;
            foreach (var patrol in groupedMembersList)
            {
                IWorksheet sheet = workbook.Worksheets[sheetIndex];

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

                // Add Patrol Name
                sheet.Range[rowNumber, 2].Text = patrol.Key;
                sheet.Range[rowNumber, 2].CellStyle = headingStyle;
                sheet.Range[rowNumber, 2, rowNumber, 6].Merge();
                sheet.SetRowHeight(rowNumber, 100);

                // Add Patrol members
                foreach (var member in patrol.OrderByDescending(p => p.patrol_duty))
                {
                    rowNumber++;
                    sheet.Range[rowNumber, 1, rowNumber, 5].CellStyle = rowStyle;
                    sheet.Range[rowNumber, 2].Text = member.first_name;
                    sheet.Range[rowNumber, 2].BorderAround();
                    sheet.Range[rowNumber, 3].Text = member.last_name;
                    sheet.Range[rowNumber, 3].BorderAround();
                    sheet.Range[rowNumber, 4].Text = member.member_number;
                    sheet.Range[rowNumber, 4].BorderAround();
                    sheet.Range[rowNumber, 5].Text = member.patrol_duty;
                    sheet.Range[rowNumber, 5].BorderAround();
                    if (!string.IsNullOrEmpty(member.patrol_duty))
                    {
                        sheet.Range[rowNumber, 2, rowNumber, 5].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    }
                }
                rowNumber++;

                sheet.UsedRange.AutofitColumns();
                sheetIndex++;
            }


            return workbook;

        }

        public IWorkbook GenerateSignInSheetWorkbook(List<MemberListModel> memberListModel,string groupName, string section, string unitName, string eventName)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 7);
            IWorksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;

            IStyle headingStyle = workbook.Styles["headingStyle"];

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
            sheet.Range[rowNumber, 5, rowNumber, 7].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
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
            sheet.Range[rowNumber, 5, rowNumber, 7].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
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

        public IWorkbook GenerateEventAttendanceWorkbook(EventListModel eventListModel, string groupName, string section, string unitName)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 7);
            IWorksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;

            IStyle headingStyle = workbook.Styles["headingStyle"];

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
            sheet.Range[rowNumber, 2, rowNumber, 6].CellStyle.Font.Bold = true;
            sheet.Range[rowNumber, 2].Text = "First Name";
            sheet.Range[rowNumber, 2].BorderAround();
            sheet.Range[rowNumber, 3].Text = "Last Name";
            sheet.Range[rowNumber, 3].BorderAround();
            sheet.Range[rowNumber, 4].Text = "Number";
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
                sheet.Range[rowNumber, 6].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
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
                sheet.Range[rowNumber, 6].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }

            sheet.UsedRange.AutofitColumns();

            return workbook;
        }

        public IWorkbook GenerateAttendanceReportWorkbook(AttendanceReportModel attendanceReportData, string groupName, string section, string unitName, DateTime fromDate, DateTime toDate, bool forPdfOutput)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 10);
            IWorksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;

            IStyle headingStyle = workbook.Styles["headingStyle"];

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 10].Merge();
            sheet.SetRowHeight(rowNumber, 25);

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = $"Attendance between {fromDate.ToShortDateString()} and {toDate.ToShortDateString()}";
            title.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 10].Merge();
            sheet.SetRowHeight(rowNumber, 25);

            var totalEvents = attendanceReportData.attendanceReportChallengeAreaSummaries.FirstOrDefault()?.TotalEvents ?? 0;
            var communityEvents = attendanceReportData.attendanceReportChallengeAreaSummaries.Where(c => c.ChallengeArea == "Community").FirstOrDefault()?.EventCount ?? 0;
            var creativeEvents = attendanceReportData.attendanceReportChallengeAreaSummaries.Where(c => c.ChallengeArea == "Creative").FirstOrDefault()?.EventCount ?? 0;
            var outdoorsEvents = attendanceReportData.attendanceReportChallengeAreaSummaries.Where(c => c.ChallengeArea == "Outdoors").FirstOrDefault()?.EventCount ?? 0;
            var personalGrowthEvents = attendanceReportData.attendanceReportChallengeAreaSummaries.Where(c => c.ChallengeArea == "Personal Growth").FirstOrDefault()?.EventCount ?? 0;

            // Add Challenge Area Summary
            rowNumber++;
            sheet.Range[rowNumber, 2].Text = $"Community {communityEvents} / {totalEvents}";
            sheet.Range[rowNumber, 2, rowNumber, 3].Merge();
            sheet.Range[rowNumber, 2, rowNumber, 3].BorderAround();
            sheet.Range[rowNumber, 2, rowNumber, 3].CellStyle.ColorIndex = GetChallengeAreaColour("Community");
            sheet.Range[rowNumber, 4].Text = $"Creative {creativeEvents} / {totalEvents}";
            sheet.Range[rowNumber, 4, rowNumber, 5].Merge();
            sheet.Range[rowNumber, 4, rowNumber, 5].BorderAround();
            sheet.Range[rowNumber, 4, rowNumber, 5].CellStyle.ColorIndex = GetChallengeAreaColour("Creative");
            sheet.Range[rowNumber, 6].Text = $"Outdoors {outdoorsEvents} / {totalEvents}";
            sheet.Range[rowNumber, 6, rowNumber, 7].Merge();
            sheet.Range[rowNumber, 6, rowNumber, 7].BorderAround();
            sheet.Range[rowNumber, 6, rowNumber, 7].CellStyle.ColorIndex = GetChallengeAreaColour("Outdoors");
            sheet.Range[rowNumber, 8].Text = $"Personal Growth {personalGrowthEvents} / {totalEvents}";
            sheet.Range[rowNumber, 8, rowNumber, 10].Merge();
            sheet.Range[rowNumber, 8, rowNumber, 10].BorderAround();
            sheet.Range[rowNumber, 8, rowNumber, 10].CellStyle.ColorIndex = GetChallengeAreaColour("Personal Growth");
            sheet.Range[rowNumber, 2, rowNumber, 10].CellStyle.Font.Bold = true;
            rowNumber++;
            // Group attendance by member for youth
            var groupedAttendances = attendanceReportData.attendanceReportItems.Where(m => m.IsAdultMember == 0).GroupBy(wa => wa.MemberName).ToList();

            // Add Event Details
            var columnNumber = 1;
            rowNumber++;
            var firstGroupedAttendance = groupedAttendances.FirstOrDefault();
            foreach (var eventAttendance in firstGroupedAttendance)
            {
                columnNumber++;
                if (forPdfOutput)
                {
                    // Name
                    sheet.Range[rowNumber, columnNumber].Text = eventAttendance.EventNameDisplay;
                    sheet.Range[rowNumber, columnNumber].CellStyle.ColorIndex = GetChallengeAreaColour(eventAttendance.EventChallengeArea);
                    sheet.Range[rowNumber, columnNumber].BorderAround();
                    sheet.Range[rowNumber, columnNumber].CellStyle.Rotation = 90;
                    sheet.Range[rowNumber, columnNumber].CellStyle.WrapText = true;
                    sheet.Range[rowNumber, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[rowNumber, columnNumber].CellStyle.Font.Bold = true;
                    sheet.SetColumnWidth(columnNumber, 8);
                }
                else
                {
                    // Challenge Area
                    sheet.Range[rowNumber, columnNumber].Text = eventAttendance.EventChallengeArea;
                    sheet.Range[rowNumber, columnNumber].CellStyle.ColorIndex = GetChallengeAreaColour(eventAttendance.EventChallengeArea);
                    sheet.Range[rowNumber, columnNumber].BorderAround();

                    // Date
                    sheet.Range[rowNumber + 1, columnNumber].DateTime = eventAttendance.EventStartDate;
                    sheet.Range[rowNumber + 1, columnNumber].CellStyle.ColorIndex = GetChallengeAreaColour(eventAttendance.EventChallengeArea);
                    sheet.Range[rowNumber + 1, columnNumber].BorderAround();

                    // Name
                    sheet.Range[rowNumber + 2, columnNumber].Text = eventAttendance.EventName;
                    sheet.Range[rowNumber + 2, columnNumber].CellStyle.ColorIndex = GetChallengeAreaColour(eventAttendance.EventChallengeArea);
                    sheet.Range[rowNumber + 2, columnNumber].BorderAround();
                }
            }

            if (forPdfOutput)
            {
                sheet.SetRowHeight(rowNumber, 175);
            }
            else
            {
                rowNumber = rowNumber + 2;
            }
            columnNumber++;
            sheet.Range[rowNumber, columnNumber].Text = "Total";
            sheet.Range[rowNumber, columnNumber].BorderAround();
            sheet.Range[rowNumber, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[rowNumber, columnNumber].CellStyle.Font.Bold = true;
            sheet.SetColumnWidth(columnNumber, 8);


            // Add youth member rows
            var sumStartRow = rowNumber + 1;
            foreach (var groupedAttendance in groupedAttendances)
            {
                rowNumber++;
                sheet.Range[rowNumber, 1].Text = forPdfOutput ? groupedAttendance.FirstOrDefault().MemberNameAndRate : groupedAttendance.FirstOrDefault().MemberName;
                sheet.Range[rowNumber, 1].BorderAround();
                columnNumber = 1;
                foreach (var eventAttendance in groupedAttendance)
                {
                    columnNumber++;
                    sheet.Range[rowNumber, columnNumber].Number = eventAttendance.Attended;
                    sheet.Range[rowNumber, columnNumber].BorderAround();
                    sheet.Range[rowNumber, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                }
                // Row total
                var sumRange = sheet.Range[rowNumber, 2, rowNumber, columnNumber].AddressLocal;
                columnNumber++;
                sheet.Range[rowNumber, columnNumber].Formula = $"=SUM({sumRange})";
                sheet.Range[rowNumber, columnNumber].BorderAround();
                sheet.Range[rowNumber, columnNumber].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, columnNumber].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[rowNumber, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }

            // Add total row
            var sumEndRow = rowNumber;
            rowNumber++;
            var youthTotalRow = rowNumber;
            sheet.Range[rowNumber, 1].Text = "Youth Total";
            sheet.Range[rowNumber, 1].BorderAround();
            for (int i = 2; i <= columnNumber; i++)
            {
                var sumRange = sheet.Range[sumStartRow, i, sumEndRow, i].AddressLocal;
                sheet.Range[rowNumber, i].Formula = $"=SUM({sumRange})";
                sheet.Range[rowNumber, i].BorderAround();
                sheet.Range[rowNumber, i].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, i].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }
            sheet.Range[rowNumber, 1, rowNumber, columnNumber].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;

            // Group attendance by member for adults
            groupedAttendances = attendanceReportData.attendanceReportItems.Where(m => m.IsAdultMember == 1).GroupBy(wa => wa.MemberName).ToList();

            // Add adult member rows
            sumStartRow = rowNumber + 1;
            foreach (var groupedAttendance in groupedAttendances)
            {
                rowNumber++;
                sheet.Range[rowNumber, 1].Text = groupedAttendance.FirstOrDefault().MemberName;
                sheet.Range[rowNumber, 1].BorderAround();
                columnNumber = 1;
                foreach (var eventAttendance in groupedAttendance)
                {
                    columnNumber++;
                    sheet.Range[rowNumber, columnNumber].Number = eventAttendance.Attended;
                    sheet.Range[rowNumber, columnNumber].BorderAround();
                    sheet.Range[rowNumber, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                }
                // Row total
                var sumRange = sheet.Range[rowNumber, 2, rowNumber, columnNumber].AddressLocal;
                columnNumber++;
                sheet.Range[rowNumber, columnNumber].Formula = $"=SUM({sumRange})";
                sheet.Range[rowNumber, columnNumber].BorderAround();
                sheet.Range[rowNumber, columnNumber].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, columnNumber].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[rowNumber, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }

            // Add total row
            sumEndRow = rowNumber;
            rowNumber++;
            var adultTotalRow = rowNumber;
            sheet.Range[rowNumber, 1].Text = "Adult Total";
            sheet.Range[rowNumber, 1].BorderAround();
            for (int i = 2; i <= columnNumber; i++)
            {
                var sumRange = sheet.Range[sumStartRow, i, sumEndRow, i].AddressLocal;
                sheet.Range[rowNumber, i].Formula = $"=SUM({sumRange})";
                sheet.Range[rowNumber, i].BorderAround();
                sheet.Range[rowNumber, i].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, i].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }
            sheet.Range[rowNumber, 1, rowNumber, columnNumber].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;

            // Add Grand Total row
            rowNumber++;
            sheet.Range[rowNumber, 1].Text = "Event Total";
            sheet.Range[rowNumber, 1].BorderAround();
            for (int i = 2; i <= columnNumber; i++)
            {
                var youthTotalCell = sheet.Range[youthTotalRow, i].AddressLocal;
                var adultTotalCell = sheet.Range[adultTotalRow, i].AddressLocal;
                sheet.Range[rowNumber, i].Formula = $"={youthTotalCell}+{adultTotalCell}";
                sheet.Range[rowNumber, i].BorderAround();
                sheet.Range[rowNumber, i].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, i].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }
            sheet.Range[rowNumber, 1, rowNumber, columnNumber].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;

            sheet.Range[1, 1, rowNumber, 1].AutofitColumns();

            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA3;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
            sheet.PageSetup.CenterHorizontally = true;
            sheet.PageSetup.CenterVertically = true;
            sheet.PageSetup.BottomMargin = 0;
            sheet.PageSetup.TopMargin = 0;
            sheet.PageSetup.LeftMargin = 0;
            sheet.PageSetup.RightMargin = 0;
            sheet.PageSetup.HeaderMargin = 0;
            sheet.PageSetup.FooterMargin = 0;
            sheet.PageSetup.IsFitToPage = true;

            return workbook;
        }

        public IWorkbook GenerateOASWorksheetWorkbook(List<OASWorksheetAnswers> worksheetAnswers, string groupName, string section, string unitName, string templateTitle, bool forPdfOutput)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 10);
            IWorksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;
            int columnNumber = 1;

            IStyle headingStyle = workbook.Styles["headingStyle"];

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 10].Merge();
            sheet.SetRowHeight(rowNumber, 25);

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = $"{templateTitle} as at {DateTime.Now.ToShortDateString()}";
            title.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 10].Merge();
            sheet.SetRowHeight(rowNumber, 25);

            rowNumber++;
            var pdrText = "";
            int pdrStartCol = 99;
            var groupedAnswers = worksheetAnswers.GroupBy(x => x.MemberName).ToList();
            foreach (var plan in groupedAnswers.FirstOrDefault().OrderBy(ga => ga.InputTitleSortIndex).ThenBy(ga => ga.InputSortIndex).ToList())
            {
                // Plan Do Review
                columnNumber++;
                if (pdrText != plan.InputTitle)
                {
                    pdrText = plan.InputTitle;
                    sheet.Range[rowNumber, columnNumber].Text = plan.InputTitle;
                    sheet.Range[rowNumber, columnNumber].CellStyle.ColorIndex = GetInputTitleColour(plan.InputTitle);
                    sheet.Range[rowNumber, columnNumber].BorderAround();
                    sheet.Range[rowNumber, columnNumber].CellStyle.Font.Bold = true;
                    sheet.Range[rowNumber, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    if (pdrStartCol > columnNumber)
                        pdrStartCol = columnNumber;
                    else
                    {
                        sheet.Range[rowNumber, pdrStartCol, rowNumber, columnNumber - 1].Merge();
                        sheet.Range[rowNumber, pdrStartCol, rowNumber, columnNumber - 1].BorderAround();
                        pdrStartCol = columnNumber;
                    }
                }

                // iStatement
                sheet.Range[rowNumber + 1, columnNumber].Text = plan.InputLabel;
                sheet.Range[rowNumber + 1, columnNumber].BorderAround();
                sheet.Range[rowNumber + 1, columnNumber].CellStyle.Font.Bold = true;
                if (forPdfOutput)
                {
                    sheet.Range[rowNumber + 1, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet.Range[rowNumber + 1, columnNumber].CellStyle.Rotation = 90;
                    sheet.Range[rowNumber + 1, columnNumber].CellStyle.WrapText = true; 
                    sheet.SetRowHeight(rowNumber + 1, 200);
                }
                sheet.SetColumnWidth(columnNumber, 10);
            }

            // Member Name and answers
            rowNumber++;
            rowNumber++;
            columnNumber = 0;
            foreach (var groupedAnswer in groupedAnswers.OrderBy(ga => ga.Key))
            {
                columnNumber++;
                sheet.Range[rowNumber, columnNumber].Text = groupedAnswer.Key;
                sheet.Range[rowNumber, columnNumber].BorderAround();
                sheet.Range[rowNumber, columnNumber].CellStyle.Font.Bold = true;

                foreach(var answer in groupedAnswer.OrderBy(ga => ga.InputTitleSortIndex).ThenBy(ga => ga.InputSortIndex))
                {
                    columnNumber++;
                    if (answer.MemberAnswer.HasValue)
                    {
                        sheet.Range[rowNumber, columnNumber].DateTime = answer.MemberAnswer.Value;
                        sheet.Range[rowNumber, columnNumber].CellStyle.ColorIndex = ExcelKnownColors.Sea_green;
                    }
                    sheet.Range[rowNumber, columnNumber].BorderAround();
                }
                rowNumber++;
                columnNumber = 0;
            }

            sheet.Range[1, 1, rowNumber, 1].AutofitColumns();

            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA3;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
            sheet.PageSetup.BottomMargin = 0.25;
            sheet.PageSetup.TopMargin = 0.25;
            sheet.PageSetup.LeftMargin = 0.25;
            sheet.PageSetup.RightMargin = 0.25;
            sheet.PageSetup.HeaderMargin = 0;
            sheet.PageSetup.FooterMargin = 0;
            sheet.PageSetup.IsFitToPage = true;

            return workbook;
        }

        private int UnitMaxAge(string unit)
        {
            switch (unit)
            {
                case "joey":
                    return 8;
                case "cub":
                    return 11;
                case "scout":
                    return 15;
                case "venturer":
                    return 18;
                case "rover":
                default:
                    return 26;
            }
        }

        private ExcelKnownColors GetChallengeAreaColour(string challengeArea)
        {
            switch (challengeArea)
            {
                case "Community":
                    return ExcelKnownColors.Grey_25_percent;
                case "Creative":
                    return ExcelKnownColors.Rose;
                case "Outdoors":
                    return ExcelKnownColors.Sea_green;
                case "Personal Growth":
                    return ExcelKnownColors.Gold;
                default:
                    return ExcelKnownColors.None;
            }
        }

        private ExcelKnownColors GetInputTitleColour(string inputTitle)
        {
            switch (inputTitle)
            {
                case "Plan":
                    return ExcelKnownColors.Pale_blue;
                case "Do":
                    return ExcelKnownColors.Sky_blue;
                case "Review":
                    return ExcelKnownColors.Light_blue;
                case "Verify":
                    return ExcelKnownColors.Blue_grey;
                default:
                    return ExcelKnownColors.None;
            }
        }
    }
}
