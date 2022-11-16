using Syncfusion.Drawing;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation;
using Topo.Models.AditionalAwards;
using Topo.Models.Approvals;
using Topo.Models.Events;
using Topo.Models.Logbook;
using Topo.Models.MemberList;
using Topo.Models.Milestone;
using Topo.Models.OAS;
using Topo.Models.SIA;
using Topo.Models.Wallchart;

namespace Topo.Services
{
    public interface IReportService
    {
        public IWorkbook GenerateAdditionalAwardsWorkbook(List<AdditionalAwardSpecificationListModel> awardSpecificationsList, List<AdditionalAwardListModel> sortedAdditionalAwardsList, List<string>? distinctAwards, string groupName, string section, string unitName);
        public IWorkbook GeneratePatrolListWorkbook(List<MemberListModel> sortedPatrolList, string groupName, string section, string unitName, bool includeLeaders);
        public IWorkbook GenerateMemberListWorkbook(List<MemberListModel> sortedMemberList, string groupName, string section, string unitName);
        public IWorkbook GeneratePatrolSheetsWorkbook(List<MemberListModel> sortedPatrolList, string section);
        public IWorkbook GenerateSignInSheetWorkbook(List<MemberListModel> memberListModel, string groupName, string section, string unitName, string eventName);
        public IWorkbook GenerateEventAttendanceWorkbook(EventListModel eventListModel, string groupName, string section, string unitName);
        public IWorkbook GenerateAttendanceReportWorkbook(AttendanceReportModel attendanceReportData, string groupName, string section, string unitName, DateTime fromDate, DateTime toDate, bool forPdfOutput);
        public IWorkbook GenerateOASWorksheetWorkbook(List<OASWorksheetAnswers> worksheetAnswers, string groupName, string section, string unitName, bool forPdfOutput, bool formatLikeTerrain, bool showByPatrol = false);
        public IWorkbook GenerateSIAWorkbook(List<SIAProjectListModel> siaProjects, string groupName, string section, string unitName, bool forPdfOutput);
        public IWorkbook GenerateMilestoneWorkbook(List<MilestoneSummaryListModel> milestoneSummaries, string groupName, string section, string unitName, bool forPdfOutput);
        public IWorkbook GenerateLogbookWorkbook(List<MemberLogbookReportViewModel> logbookEntries, string groupName, string section, string unitName, bool forPdfOutput);
        public IWorkbook GenerateWallchartWorkbook(List<WallchartItemModel> wallchartEntries, string groupName, string section, string unitName, bool forPdfOutput);
        public IWorkbook GenerateApprovalsWorkbook(List<ApprovalsListModel> selectedApprovals, string groupName, string section, string unitName, DateTime approvalSearchFromDate, DateTime approvalSearchToDate, bool groupByMember, bool forPdfOutput);
        public IWorkbook CreateWorkbookWithSheets(int sheetsToCreate);
    }
    public class ReportService : IReportService
    {
        private enum participateAssistLead
        {
            participate,
            assist,
            lead
        }

        private enum wallchartGroups
        {
            intro,
            participate,
            assist,
            lead,
            oasCore,
            oasLand,
            oasWater,
            oasProgression,
            siaOdd,
            siaEven,
            leadershipCourse,
            adventurousJourney,
            personalReflection,
            peakAward
        }

        private Color[] Milestone1ParticipateColours = new Color[]
        {
            Color.FromArgb(236, 82, 82),
            Color.FromArgb(241, 129, 57),
            Color.FromArgb(248, 194, 23),
            Color.FromArgb(253, 238, 0),
            Color.FromArgb(218, 240, 29),
            Color.FromArgb(183, 241, 58),
            Color.FromArgb(156, 242, 80)
         };

        private Color[] Milestone1AssistColours = new Color[]
        {
            Color.FromArgb(236, 82, 82),
            Color.FromArgb(253, 238, 0),
            Color.FromArgb(156, 242, 80)
        };

        private Color[] Milestone1LeadColours = new Color[]
        {
            Color.FromArgb(236, 82, 82),
            Color.FromArgb(156, 242, 80)
        };

        private Color[] Milestone2ParticipateColours = new Color[]
        {
            Color.FromArgb(236, 82, 82),
            Color.FromArgb(241, 131, 57),
            Color.FromArgb(248, 189, 26),
            Color.FromArgb(221, 239, 26),
            Color.FromArgb(184, 241, 57),
            Color.FromArgb(156, 242, 80)
        };

        private Color[] Milestone2AssistColours = new Color[]
        {
            Color.FromArgb(236, 82, 82),
            Color.FromArgb(246, 176, 33),
            Color.FromArgb(216, 240, 30),
            Color.FromArgb(156, 242, 80)
        };

        private Color[] Milestone3Colours = new Color[]
        {
            Color.FromArgb(236, 82, 82),
            Color.FromArgb(244, 157, 43),
            Color.FromArgb(253, 238, 0),
            Color.FromArgb(206, 240, 38),
            Color.FromArgb(156, 242, 80)
        };

        public ReportService()
        {
        }

        public IWorkbook CreateWorkbookWithSheets(int sheetsToCreate)
        {
            //Step 1 : Instantiate the spreadsheet creation engine.
            ExcelEngine excelEngine = new ExcelEngine();
            //Step 2 : Instantiate the excel application object.
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2016;

            //Initializes the SubstituteFont event to perform font substitution in Excel-to-PDF conversion.
            application.SubstituteFont += new SubstituteFontEventHandler(SubstituteFont);

            // Creating new workbook
            IWorkbook workbook = application.Workbooks.Create(sheetsToCreate);

            //Adding cell style.               
            IStyle headingStyle = workbook.Styles.Add("headingStyle");
            headingStyle.Font.Bold = true;
            headingStyle.Font.Size = 14;
            headingStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            headingStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;

            return workbook;
        }

        private static void SubstituteFont(object sender, SubstituteFontEventArgs args)
        {
            var directory = Directory.GetCurrentDirectory();
            args.AlternateFontStream = File.OpenRead($@"{directory}/Fonts/carlito.regular.ttf");
        }

        private IWorksheet AddLogoToSheet(IWorkbook workbook, int worksheetIndex, string groupName, string section, int lastHeadingCol)
        {
            IWorksheet sheet = workbook.Worksheets[worksheetIndex];

            // Add Logo
            var directory = Directory.GetCurrentDirectory();
            var logoName = GetLogoForSection(section);
            var logoFullPathName = $@"{directory}/Images/{logoName}";
            if (File.Exists(logoFullPathName))
            {
                FileStream imageStream = new FileStream(logoFullPathName, FileMode.Open, FileAccess.Read);
                IPictureShape logo = sheet.Pictures.AddPicture(1, 1, imageStream);
                var aspectRatio = (double)logo.Height / logo.Width;
                logo.Width = 100;
                logo.Height = (int)(100 * aspectRatio);
            }

            // Add Group Name
            var groupNameCell = sheet.Range[1, 2];
            groupNameCell.Text = groupName;
            groupNameCell.CellStyle = workbook.Styles["headingStyle"];
            sheet.Range[1, 2, 1, lastHeadingCol].Merge();
            sheet.SetRowHeight(1, 30);

            return sheet;
        }

        public string GetLogoForSection(string section)
        {
            var logoName = "";
            switch (section)
            {
                case "joey":
                    logoName = "Joey Scouts Full Col Vertical.jpg";
                    break;
                case "cub":
                    logoName = "Cub Scouts Full Col Vertical.png";
                    break;
                case "scout":
                    logoName = "Scouts Full Col Vertical.jpg";
                    break;
                case "venturer":
                    logoName = "Venturer Scouts Full Col Vertical.jpg";
                    break;
                case "rover":
                    logoName = "Rover Scouts Full Col Vertical.jpg";
                    break;
            }
            return logoName;
        }

        private IWorkbook CreateWorkbookWithLogo(string groupName, string section, int lastHeadingCol)
        {
            var workbook = CreateWorkbookWithSheets(1);
            AddLogoToSheet(workbook, 0, groupName, section, lastHeadingCol);
            return workbook;
        }
        public IWorkbook GenerateAdditionalAwardsWorkbook(List<AdditionalAwardSpecificationListModel> awardSpecificationsList, List<AdditionalAwardListModel> sortedAdditionalAwardsList, List<string>? distinctAwards, string groupName, string section, string unitName)
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
                    cell.DateTime = x.AwardDate.Value;
                    cell.CellStyle.Color = Color.Orange;
                    if (x.PresentedDate.HasValue)
                    {
                        cell.DateTime = x.PresentedDate.Value;
                        cell.CellStyle.Color = Color.LightGreen;
                    }
                    cell.NumberFormat = "dd/MM/yy";
                }
                rowNumber++;
            }
            sheet.UsedRange.AutofitColumns();

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

        public IWorkbook GeneratePatrolListWorkbook(List<MemberListModel> sortedPatrolList, string groupName, string section, string unitName, bool includeLeaders)
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
                var patrolName = string.IsNullOrEmpty(patrol.Key) ? "Unassigned" : patrol.Key;
                rowNumber++;
                sheet.Range[rowNumber, 1].Text = $"{patrolName} ({patrol.Count()})";
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
                    sheet.Range[rowNumber, 3].Text = member.patrol_duty;
                    sheet.Range[rowNumber, 3].BorderAround();
                    if (!string.IsNullOrEmpty(member.patrol_duty))
                    {
                        sheet.Range[rowNumber, 1, rowNumber, 3].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
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
                sheet.Range[rowNumber, 8].Text = member.patrol_duty;
                sheet.Range[rowNumber, 8].BorderAround();
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
                    sheet.Range[rowNumber, 8].Text = member.patrol_duty;
                    sheet.Range[rowNumber, 8].BorderAround();
                }
            }

            sheet.UsedRange.AutofitColumns();

            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
            sheet.PageSetup.BottomMargin = 0.25;
            sheet.PageSetup.TopMargin = 0.25;
            sheet.PageSetup.LeftMargin = 0.25;
            sheet.PageSetup.RightMargin = 0.25;
            sheet.PageSetup.HeaderMargin = 0;
            sheet.PageSetup.FooterMargin = 0;
            sheet.PageSetup.IsFitToPage = true;

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
            sheet.Range[rowNumber, 2, rowNumber, 8].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 8].Merge();
            sheet.SetRowHeight(rowNumber, 40);

            // Add Header
            rowNumber++;
            sheet.Range[rowNumber, 2].Text = "First Name";
            sheet.Range[rowNumber, 2].BorderAround();
            sheet.Range[rowNumber, 3].Text = "Last Name";
            sheet.Range[rowNumber, 3].BorderAround();
            sheet.Range[rowNumber, 4].Text = "Age";
            sheet.Range[rowNumber, 4].BorderAround();
            sheet.Range[rowNumber, 5].Text = "Member";
            sheet.Range[rowNumber, 5].BorderAround();
            sheet.Range[rowNumber, 6].Text = "Duty";
            sheet.Range[rowNumber, 6].BorderAround();
            sheet.Range[rowNumber, 7].Text = "Patrol";
            sheet.Range[rowNumber, 7].BorderAround();
            sheet.Range[rowNumber, 2, rowNumber, 7].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;

            foreach (var member in sortedMemberList)
            {
                rowNumber++;
                sheet.Range[rowNumber, 2].Text = member.first_name;
                sheet.Range[rowNumber, 2].BorderAround();
                sheet.Range[rowNumber, 3].Text = member.last_name;
                sheet.Range[rowNumber, 3].BorderAround();
                sheet.Range[rowNumber, 4].Text = member.age;
                sheet.Range[rowNumber, 4].BorderAround();
                sheet.Range[rowNumber, 5].Text = member.member_number;
                sheet.Range[rowNumber, 5].BorderAround();
                sheet.Range[rowNumber, 6].Text = member.patrol_duty;
                sheet.Range[rowNumber, 6].BorderAround();
                sheet.Range[rowNumber, 7].Text = member.patrol_name;
                sheet.Range[rowNumber, 7].BorderAround();

                decimal.TryParse(member.age, out decimal approxYears);
                if (member.age.StartsWith((UnitMaxAge(section) - 2).ToString()))
                    sheet.Range[rowNumber, 4].CellStyle.ColorIndex = ExcelKnownColors.Yellow;
                if (member.age.StartsWith(UnitMaxAge(section).ToString()) || member.age.StartsWith((UnitMaxAge(section) - 1).ToString()))
                    sheet.Range[rowNumber, 4].CellStyle.ColorIndex = ExcelKnownColors.Rose;
            }

            rowNumber++;
            rowNumber++;
            sheet.Range[rowNumber, 2].Text = "Second last year in section";
            sheet.Range[rowNumber, 2, rowNumber, 3].Merge();
            sheet.Range[rowNumber, 4].Text = "";
            sheet.Range[rowNumber, 4].BorderAround();
            sheet.Range[rowNumber, 4].CellStyle.ColorIndex = ExcelKnownColors.Yellow;
            rowNumber++;
            sheet.Range[rowNumber, 2].Text = "Last year in section";
            sheet.Range[rowNumber, 2, rowNumber, 3].Merge();
            sheet.Range[rowNumber, 4].Text = "";
            sheet.Range[rowNumber, 4].BorderAround();
            sheet.Range[rowNumber, 4].CellStyle.ColorIndex = ExcelKnownColors.Rose;

            sheet.UsedRange.AutofitColumns();

            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
            sheet.PageSetup.BottomMargin = 0.25;
            sheet.PageSetup.TopMargin = 0.25;
            sheet.PageSetup.LeftMargin = 0.25;
            sheet.PageSetup.RightMargin = 0.25;
            sheet.PageSetup.HeaderMargin = 0;
            sheet.PageSetup.FooterMargin = 0;

            return workbook;
        }

        public IWorkbook GeneratePatrolSheetsWorkbook(List<MemberListModel> sortedPatrolList, string section)
        {
            //Step 1 : Instantiate the spreadsheet creation engine.
            ExcelEngine excelEngine = new ExcelEngine();
            //Step 2 : Instantiate the excel application object.
            IApplication application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2016;

            //Initializes the SubstituteFont event to perform font substitution in Excel-to-PDF conversion.
            application.SubstituteFont += new SubstituteFontEventHandler(SubstituteFont);

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
                var logoName = GetLogoForSection(section);
                var logoFullPathName = $@"{directory}/Images/{logoName}";
                if (File.Exists(logoFullPathName))
                {
                    FileStream imageStream = new FileStream(logoFullPathName, FileMode.Open, FileAccess.Read);
                    IPictureShape logo = sheet.Pictures.AddPicture(rowNumber, 1, imageStream);
                    var aspectRatio = (double)logo.Height / logo.Width;
                    logo.Width = 100;
                    logo.Height = (int)(100 * aspectRatio);
                }

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
                    sheet.Range[rowNumber, 4].Text = member.patrol_duty;
                    sheet.Range[rowNumber, 4].BorderAround();
                    if (!string.IsNullOrEmpty(member.patrol_duty))
                    {
                        sheet.Range[rowNumber, 2, rowNumber, 4].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    }
                }
                rowNumber++;

                sheet.UsedRange.AutofitColumns();

                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
                sheet.PageSetup.BottomMargin = 0.25;
                sheet.PageSetup.TopMargin = 0.25;
                sheet.PageSetup.LeftMargin = 0.25;
                sheet.PageSetup.RightMargin = 0.25;
                sheet.PageSetup.HeaderMargin = 0;
                sheet.PageSetup.FooterMargin = 0;
                sheet.PageSetup.IsFitToPage = true;

                sheetIndex++;
            }


            return workbook;

        }

        public IWorkbook GenerateSignInSheetWorkbook(List<MemberListModel> memberListModel, string groupName, string section, string unitName, string eventName)
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
            sheet.Range[rowNumber, 2].Text = "Patrol";
            sheet.Range[rowNumber, 3].Text = "Role";
            sheet.Range[rowNumber, 4].Text = " Registered";
            sheet.Range[rowNumber, 5].Text = " Paid";
            sheet.Range[rowNumber, 6].Text = " Attended";
            sheet.Range[rowNumber, 4, rowNumber, 6].CellStyle.Rotation = 90;
            sheet.Range[rowNumber, 4, rowNumber, 6].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[rowNumber, 7].Text = "Name";
            sheet.Range[rowNumber, 1, rowNumber, 7].CellStyle.Font.Bold = true;

            foreach (var member in memberListModel.Where(m => m.isAdultLeader == 0))
            {
                rowNumber++;
                sheet.Range[rowNumber, 1].Text = $"{member.first_name} {member.last_name}";
                sheet.Range[rowNumber, 1].BorderAround();
                sheet.Range[rowNumber, 2].Text = member.patrol_name;
                sheet.Range[rowNumber, 2].BorderAround();
                sheet.Range[rowNumber, 3].Text = member.patrol_duty;
                sheet.Range[rowNumber, 3].BorderAround();
                sheet.Range[rowNumber, 4].Text = "";
                sheet.Range[rowNumber, 4].BorderAround();
                sheet.Range[rowNumber, 5].Text = "";
                sheet.Range[rowNumber, 5].BorderAround();
                sheet.Range[rowNumber, 6].Text = "";
                sheet.Range[rowNumber, 6].BorderAround();
                sheet.Range[rowNumber, 7].Text = member.first_name;
                sheet.Range[rowNumber, 7].BorderAround();
                sheet.Range[rowNumber, 8].Text = "";
                sheet.Range[rowNumber, 8].BorderAround();
            }

            rowNumber++;
            // Add Header
            rowNumber++;
            sheet.Range[rowNumber, 1].Text = "Name";
            sheet.Range[rowNumber, 2].Text = "Patrol";
            sheet.Range[rowNumber, 3].Text = "Role";
            sheet.Range[rowNumber, 4].Text = " Registered";
            sheet.Range[rowNumber, 5].Text = " Paid";
            sheet.Range[rowNumber, 6].Text = " Attended";
            sheet.Range[rowNumber, 4, rowNumber, 6].CellStyle.Rotation = 90;
            sheet.Range[rowNumber, 4, rowNumber, 6].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[rowNumber, 7].Text = "Name";
            sheet.Range[rowNumber, 1, rowNumber, 7].CellStyle.Font.Bold = true;

            foreach (var member in memberListModel.Where(m => m.isAdultLeader == 1))
            {
                rowNumber++;
                sheet.Range[rowNumber, 1].Text = $"{member.first_name} {member.last_name}";
                sheet.Range[rowNumber, 1].BorderAround();
                sheet.Range[rowNumber, 2].Text = "";
                sheet.Range[rowNumber, 2].BorderAround();
                sheet.Range[rowNumber, 3].Text = member.patrol_duty;
                sheet.Range[rowNumber, 3].BorderAround();
                sheet.Range[rowNumber, 4].Text = "";
                sheet.Range[rowNumber, 4].BorderAround();
                sheet.Range[rowNumber, 5].Text = "";
                sheet.Range[rowNumber, 5].BorderAround();
                sheet.Range[rowNumber, 6].Text = "";
                sheet.Range[rowNumber, 6].BorderAround();
                sheet.Range[rowNumber, 7].Text = member.first_name;
                sheet.Range[rowNumber, 7].BorderAround();
            }

            sheet.UsedRange.AutofitColumns();

            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
            sheet.PageSetup.BottomMargin = 0.25;
            sheet.PageSetup.TopMargin = 0.25;
            sheet.PageSetup.LeftMargin = 0.25;
            sheet.PageSetup.RightMargin = 0.25;
            sheet.PageSetup.HeaderMargin = 0;
            sheet.PageSetup.FooterMargin = 0;
            sheet.PageSetup.IsFitToPage = true;

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
            sheet.Range[rowNumber, 2, rowNumber, 5].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[rowNumber, 2, rowNumber, 5].CellStyle.Font.Bold = true;
            sheet.Range[rowNumber, 2].Text = "First Name";
            sheet.Range[rowNumber, 2].BorderAround();
            sheet.Range[rowNumber, 3].Text = "Last Name";
            sheet.Range[rowNumber, 3].BorderAround();
            sheet.Range[rowNumber, 4].Text = "Patrol";
            sheet.Range[rowNumber, 4].BorderAround();
            sheet.Range[rowNumber, 5].Text = "Attended";
            sheet.Range[rowNumber, 5].BorderAround();

            foreach (var attendee in eventListModel.attendees.Where(a => !a.isAdultMember).OrderBy(a => a.last_name))
            {
                rowNumber++;
                sheet.Range[rowNumber, 2].Text = attendee.first_name;
                sheet.Range[rowNumber, 2].BorderAround();
                sheet.Range[rowNumber, 3].Text = attendee.last_name;
                sheet.Range[rowNumber, 3].BorderAround();
                sheet.Range[rowNumber, 4].Text = attendee.patrol_name;
                sheet.Range[rowNumber, 4].BorderAround();
                sheet.Range[rowNumber, 5].Text = attendee.attended ? "Y" : "";
                sheet.Range[rowNumber, 5].BorderAround();
                sheet.Range[rowNumber, 5].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }

            rowNumber++;

            foreach (var attendee in eventListModel.attendees.Where(a => a.isAdultMember).OrderBy(a => a.last_name))
            {
                rowNumber++;
                sheet.Range[rowNumber, 2].Text = attendee.first_name;
                sheet.Range[rowNumber, 2].BorderAround();
                sheet.Range[rowNumber, 3].Text = attendee.last_name;
                sheet.Range[rowNumber, 3].BorderAround();
                sheet.Range[rowNumber, 4].Text = attendee.patrol_name;
                sheet.Range[rowNumber, 4].BorderAround();
                sheet.Range[rowNumber, 5].Text = attendee.attended ? "Y" : "";
                sheet.Range[rowNumber, 5].BorderAround();
                sheet.Range[rowNumber, 5].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }

            sheet.UsedRange.AutofitColumns();

            return workbook;
        }

        public IWorkbook GenerateAttendanceReportWorkbook(AttendanceReportModel attendanceReportData, string groupName, string section, string unitName, DateTime fromDate, DateTime toDate, bool forPdfOutput)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 10);
            IWorksheet sheet = workbook.Worksheets[0];
            sheet.EnableSheetCalculations();

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
                    sheet.Range[rowNumber, columnNumber].Text = $"{eventAttendance.EventNameDisplay} ({eventAttendance.EventStatus})";
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

                    // State
                    sheet.Range[rowNumber + 2, columnNumber].Text = eventAttendance.EventStatus;
                    sheet.Range[rowNumber + 2, columnNumber].CellStyle.ColorIndex = GetChallengeAreaColour(eventAttendance.EventChallengeArea);
                    sheet.Range[rowNumber + 2, columnNumber].BorderAround();

                    // Name
                    sheet.Range[rowNumber + 3, columnNumber].Text = eventAttendance.EventName;
                    sheet.Range[rowNumber + 3, columnNumber].CellStyle.ColorIndex = GetChallengeAreaColour(eventAttendance.EventChallengeArea);
                    sheet.Range[rowNumber + 3, columnNumber].BorderAround();
                }
            }

            if (forPdfOutput)
            {
                sheet.SetRowHeight(rowNumber, 175);
            }
            else
            {
                rowNumber = rowNumber + 3;
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
                    sheet.Range[rowNumber, columnNumber].Text = eventAttendance.Pal;
                    sheet.Range[rowNumber, columnNumber].BorderAround();
                    sheet.Range[rowNumber, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                }
                // Row total
                var sumRange = sheet.Range[rowNumber, 2, rowNumber, columnNumber].AddressLocal;
                columnNumber++;
                sheet.Range[rowNumber, columnNumber].Formula = @$"=COUNTIFS({sumRange}, ""P"")+COUNTIFS({sumRange}, ""A"")+COUNTIFS({sumRange}, ""L"")";
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
            for (int i = 2; i <= columnNumber - 1; i++)
            {
                var sumRange = sheet.Range[sumStartRow, i, sumEndRow, i].AddressLocal;
                sheet.Range[rowNumber, i].Formula = @$"=COUNTIFS({sumRange}, ""P"")+COUNTIFS({sumRange}, ""A"")+COUNTIFS({sumRange}, ""L"")";
                sheet.Range[rowNumber, i].BorderAround();
                sheet.Range[rowNumber, i].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, i].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }
            sheet.Range[rowNumber, columnNumber].Formula = $"SUM({sheet.Range[sumStartRow, columnNumber, sumEndRow, columnNumber].AddressLocal})";
            sheet.Range[rowNumber, columnNumber].Number = sheet.Range[rowNumber, columnNumber].FormulaNumberValue;
            sheet.Range[rowNumber, columnNumber].BorderAround();
            sheet.Range[rowNumber, columnNumber].CellStyle.Font.Bold = true;
            sheet.Range[rowNumber, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
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
                    sheet.Range[rowNumber, columnNumber].Text = eventAttendance.Attended > 0 ? "Y" : "";
                    sheet.Range[rowNumber, columnNumber].BorderAround();
                    sheet.Range[rowNumber, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                }
                // Row total
                var sumRange = sheet.Range[rowNumber, 2, rowNumber, columnNumber].AddressLocal;
                columnNumber++;
                sheet.Range[rowNumber, columnNumber].Formula = @$"=COUNTIFS({sumRange}, ""Y"")";
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
            for (int i = 2; i <= columnNumber - 1; i++)
            {
                var sumRange = sheet.Range[sumStartRow, i, sumEndRow, i].AddressLocal;
                sheet.Range[rowNumber, i].Formula = @$"=COUNTIFS({sumRange}, ""Y"")";
                sheet.Range[rowNumber, i].BorderAround();
                sheet.Range[rowNumber, i].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, i].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }
            sheet.Range[rowNumber, columnNumber].Formula = $"SUM({sheet.Range[sumStartRow, columnNumber, sumEndRow, columnNumber].AddressLocal})";
            sheet.Range[rowNumber, columnNumber].Number = sheet.Range[rowNumber, columnNumber].FormulaNumberValue;
            sheet.Range[rowNumber, columnNumber].BorderAround();
            sheet.Range[rowNumber, columnNumber].CellStyle.Font.Bold = true;
            sheet.Range[rowNumber, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[rowNumber, 1, rowNumber, columnNumber].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;

            // Add Grand Total row
            rowNumber++;
            sheet.Range[rowNumber, 1].Text = "Event Total";
            sheet.Range[rowNumber, 1].BorderAround();
            for (int i = 2; i <= columnNumber; i++)
            {
                var youthTotalCell = int.Parse(sheet.Range[youthTotalRow, i].CalculatedValue);
                var adultTotalCell = int.Parse(sheet.Range[adultTotalRow, i].CalculatedValue);
                sheet.Range[rowNumber, i].Number = youthTotalCell + adultTotalCell;
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

        private void GenerateOASWorksheetBodyLikeTerrain(IWorksheet sheet, IList<IGrouping<string, OASWorksheetAnswers>> groupedAnswers, ref int rowNumber, ref int columnNumber)
        {
            var pdrText = "";
            var titleRow = rowNumber;
            int pdrStartRow = 99;
            foreach (var plan in groupedAnswers.FirstOrDefault().OrderBy(ga => ga.InputTitleSortIndex).ThenBy(ga => ga.InputSortIndex).ToList())
            {
                // Plan Do Review
                rowNumber++;
                if (pdrText != plan.InputTitle)
                {
                    pdrText = plan.InputTitle;
                    sheet.Range[rowNumber, columnNumber].Text = plan.InputTitle;
                    sheet.Range[rowNumber, columnNumber].CellStyle.Color = GetInputTitleColour(plan.InputTitle);
                    sheet.Range[rowNumber, columnNumber].BorderAround();
                    sheet.Range[rowNumber, columnNumber].CellStyle.Rotation = 0;
                    sheet.Range[rowNumber, columnNumber].CellStyle.Font.Bold = true;
                    sheet.Range[rowNumber, columnNumber].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                    if (pdrStartRow > rowNumber)
                        pdrStartRow = rowNumber;
                    else
                    {
                        sheet.Range[pdrStartRow, columnNumber, rowNumber - 1, columnNumber].Merge();
                        sheet.Range[pdrStartRow, columnNumber, rowNumber - 1, columnNumber].BorderAround();
                        pdrStartRow = rowNumber;
                    }
                }

                // iStatement
                sheet.Range[rowNumber, columnNumber + 1].Text = plan.InputLabel;
                sheet.Range[rowNumber, columnNumber + 1].BorderAround();
                sheet.Range[rowNumber, columnNumber + 1].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, columnNumber + 1].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rowNumber, columnNumber + 1].CellStyle.Rotation = 0;
                sheet.Range[rowNumber, columnNumber + 1].CellStyle.WrapText = true;
                sheet.SetColumnWidth(columnNumber + 1, 50);
                sheet.AutofitRow(rowNumber);
            }

            // Member Name and answers
            columnNumber++;
            sheet.Range[titleRow + 1, columnNumber + 1].FreezePanes();
            rowNumber = titleRow;
            foreach (var groupedAnswer in groupedAnswers.OrderBy(ga => ga.Key))
            {
                columnNumber++;
                sheet.Range[rowNumber, columnNumber].Text = groupedAnswer.Key;
                sheet.Range[rowNumber, columnNumber].BorderAround();
                sheet.Range[rowNumber, columnNumber].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, columnNumber].CellStyle.Rotation = 90;
                sheet.Range[rowNumber, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;

                foreach (var answer in groupedAnswer.OrderBy(ga => ga.InputTitleSortIndex).ThenBy(ga => ga.InputSortIndex))
                {
                    rowNumber++;
                    if (answer.MemberAnswer.HasValue)
                    {
                        sheet.Range[rowNumber, columnNumber].DateTime = answer.MemberAnswer.Value;
                        sheet.Range[rowNumber, columnNumber].CellStyle.Color = Color.DarkSeaGreen;
                    }
                    sheet.Range[rowNumber, columnNumber].BorderAround();
                }
                rowNumber = titleRow;
                sheet.AutofitColumn(columnNumber);
            }

//            sheet.Range[1, 1, rowNumber, columnNumber].AutofitColumns();

        }
        private void GenerateOASWorksheetBodyOriginal(IWorksheet sheet, IList<IGrouping<string, OASWorksheetAnswers>> groupedAnswers, ref int rowNumber, ref int columnNumber)
        {
            var pdrText = "";
            int pdrStartCol = 99;
            foreach (var plan in groupedAnswers.FirstOrDefault().OrderBy(ga => ga.InputTitleSortIndex).ThenBy(ga => ga.InputSortIndex).ToList())
            {
                // Plan Do Review
                columnNumber++;
                if (pdrText != plan.InputTitle)
                {
                    pdrText = plan.InputTitle;
                    sheet.Range[rowNumber, columnNumber].Text = plan.InputTitle;
                    sheet.Range[rowNumber, columnNumber].CellStyle.Color = GetInputTitleColour(plan.InputTitle);
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
                sheet.Range[rowNumber + 1, columnNumber].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rowNumber + 1, columnNumber].CellStyle.Rotation = 90;
                sheet.Range[rowNumber + 1, columnNumber].CellStyle.WrapText = true;
                sheet.SetRowHeight(rowNumber + 1, 200);
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

                foreach (var answer in groupedAnswer.OrderBy(ga => ga.InputTitleSortIndex).ThenBy(ga => ga.InputSortIndex))
                {
                    columnNumber++;
                    if (answer.MemberAnswer.HasValue)
                    {
                        sheet.Range[rowNumber, columnNumber].DateTime = answer.MemberAnswer.Value;
                        sheet.Range[rowNumber, columnNumber].CellStyle.Color = Color.DarkSeaGreen;
                    }
                    sheet.Range[rowNumber, columnNumber].BorderAround();
                }
                rowNumber++;
                columnNumber = 0;
            }

            sheet.Range[1, 1, rowNumber, 1].AutofitColumns();
        }

        public IWorkbook GenerateOASWorksheetWorkbook(List<OASWorksheetAnswers> worksheetAnswers, string groupName, string section, string unitName, bool forPdfOutput, bool formatLikeTerrain, bool breakByPatrol = false)
        {
            var worksheetAnswersGroupedByTemplate = worksheetAnswers.GroupBy(wa => wa.TemplateTitle + (breakByPatrol ? " - " + wa.MemberPatrol : ""));
            var workbook = CreateWorkbookWithSheets(worksheetAnswersGroupedByTemplate.Count());
            var worksheetIndex = 0;
            foreach (var templatAnswerGroup in worksheetAnswersGroupedByTemplate.OrderBy(a => a.Key))
            {
                IWorksheet sheet = AddLogoToSheet(workbook, worksheetIndex, groupName, section, 10);
                sheet.Name = templatAnswerGroup.Key;
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
                string templateTitle = templatAnswerGroup.Key ?? "";
                rowNumber++;
                var title = sheet.Range[rowNumber, 2];
                title.Text = $"{templateTitle} as at {DateTime.Now.ToShortDateString()}";
                title.CellStyle = headingStyle;
                sheet.Range[rowNumber, 2, rowNumber, 10].Merge();
                sheet.SetRowHeight(rowNumber, 25);

                rowNumber++;
                IList<IGrouping<string, OASWorksheetAnswers>> groupedAnswers = templatAnswerGroup.GroupBy(x => x.MemberName).ToList();

                if (formatLikeTerrain)
                    GenerateOASWorksheetBodyLikeTerrain(sheet, groupedAnswers, ref rowNumber, ref columnNumber);
                else
                    GenerateOASWorksheetBodyOriginal(sheet, groupedAnswers, ref rowNumber, ref columnNumber);

                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA3;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.BottomMargin = 0.25;
                sheet.PageSetup.TopMargin = 0.25;
                sheet.PageSetup.LeftMargin = 0.25;
                sheet.PageSetup.RightMargin = 0.25;
                sheet.PageSetup.HeaderMargin = 0;
                sheet.PageSetup.FooterMargin = 0;
                sheet.PageSetup.IsFitToPage = true;

                worksheetIndex++;
            }

            return workbook;
        }


        public IWorkbook GenerateSIAWorkbook(List<SIAProjectListModel> siaProjects, string groupName, string section, string unitName, bool forPdfOutput)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 6);
            IWorksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;

            IStyle headingStyle = workbook.Styles["headingStyle"];

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 6].Merge();
            sheet.SetRowHeight(rowNumber, 40);

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = $"SIA Project List as at {DateTime.Now.ToShortDateString()}";
            title.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 6].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            foreach (var memberSiaProject in siaProjects.GroupBy(sp => sp.memberName))
            {
                rowNumber++;
                sheet.Range[rowNumber, 1].Text = memberSiaProject.Key;
                sheet.Range[rowNumber, 1].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 1].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[rowNumber, 1, rowNumber, 2].Merge();
                sheet.SetRowHeight(rowNumber, 20);
                foreach (var siaProject in memberSiaProject.OrderBy(sia => sia.statusUpdated))
                {
                    rowNumber++;
                    sheet.Range[rowNumber, 2].Text = siaProject.area;
                    sheet.Range[rowNumber, 3].Text = siaProject.projectName;
                    sheet.Range[rowNumber, 3].CellStyle.WrapText = true;
                    sheet.Range[rowNumber, 4].Text = siaProject.status;
                    sheet.Range[rowNumber, 5].DateTime = siaProject.statusUpdated;
                }
            }

            sheet.SetColumnWidth(1, 10);
            sheet.SetColumnWidth(2, 21);
            sheet.SetColumnWidth(3, 65);
            sheet.SetColumnWidth(4, 20);
            sheet.SetColumnWidth(5, 15);

            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
            sheet.PageSetup.BottomMargin = 0.25;
            sheet.PageSetup.TopMargin = 0.25;
            sheet.PageSetup.LeftMargin = 0.25;
            sheet.PageSetup.RightMargin = 0.25;
            sheet.PageSetup.HeaderMargin = 0;
            sheet.PageSetup.FooterMargin = 0;
            sheet.PageSetup.IsFitToPage = true;

            return workbook;
        }

        public IWorkbook GenerateMilestoneWorkbook(List<MilestoneSummaryListModel> milestoneSummaries, string groupName, string section, string unitName, bool forPdfOutput)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 8);
            IWorksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;
            int averageStartRow = 0;
            int averageEndRow = 0;

            IStyle headingStyle = workbook.Styles["headingStyle"];

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 8].Merge();
            sheet.SetRowHeight(rowNumber, 40);

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = $"Milestone Summary as at {DateTime.Now.ToShortDateString()}";
            title.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 8].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            foreach (var milestoneSummary in milestoneSummaries.GroupBy(ms => ms.currentLevel).OrderBy(ms => ms.Key))
            {
                // Headings
                rowNumber++;
                sheet.Range[rowNumber, 3].Text = $"Milestone {milestoneSummary.Key}";
                sheet.Range[rowNumber, 3].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 3].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[rowNumber, 3].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rowNumber, 3, rowNumber, 8].Merge();
                sheet.Range[rowNumber, 3, rowNumber, 8].BorderAround();
                sheet.Range[rowNumber, 3, rowNumber, 8].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.SetRowHeight(rowNumber, 20);

                rowNumber++;
                sheet.Range[rowNumber, 3].Text = GetParticipateHeadingText(milestoneSummary.Key);
                sheet.Range[rowNumber, 3].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 3].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[rowNumber, 3].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rowNumber, 3, rowNumber, 6].Merge();
                sheet.Range[rowNumber, 3, rowNumber, 6].BorderAround();
                sheet.Range[rowNumber, 3, rowNumber, 6].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.SetRowHeight(rowNumber, 20);

                rowNumber++;
                sheet.Range[rowNumber, 1].Text = "Name";
                sheet.Range[rowNumber, 1].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 1].BorderAround();
                sheet.Range[rowNumber, 2].Text = "Percent Complete";
                sheet.Range[rowNumber, 2].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 2].BorderAround();
                sheet.Range[rowNumber, 2].CellStyle.WrapText = true;
                sheet.Range[rowNumber, 3].Text = "Community";
                sheet.Range[rowNumber, 3].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 3].BorderAround();
                sheet.Range[rowNumber, 4].Text = "Outdoors";
                sheet.Range[rowNumber, 4].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 4].BorderAround();
                sheet.Range[rowNumber, 5].Text = "Creative";
                sheet.Range[rowNumber, 5].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 5].BorderAround();
                sheet.Range[rowNumber, 6].Text = "Personal Growth";
                sheet.Range[rowNumber, 6].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 6].BorderAround();
                sheet.Range[rowNumber, 6].CellStyle.WrapText = true;
                sheet.Range[rowNumber, 7].Text = GetAssistHeadingText(milestoneSummary.Key);
                sheet.Range[rowNumber, 7].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 7].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rowNumber - 1, 7, rowNumber, 7].Merge();
                sheet.Range[rowNumber - 1, 7, rowNumber, 7].BorderAround();
                sheet.Range[rowNumber - 1, 7, rowNumber, 7].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.Range[rowNumber, 8].Text = GetLeadHeadingText(milestoneSummary.Key);
                sheet.Range[rowNumber, 8].CellStyle.Font.Bold = true;
                sheet.Range[rowNumber, 8].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rowNumber - 1, 8, rowNumber, 8].Merge();
                sheet.Range[rowNumber - 1, 8, rowNumber, 8].BorderAround();
                sheet.Range[rowNumber - 1, 8, rowNumber, 8].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;

                sheet.Range[rowNumber, 1, rowNumber, 8].CellStyle.VerticalAlignment = ExcelVAlign.VAlignBottom;
                sheet.Range[rowNumber, 1, rowNumber, 8].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet.Range[rowNumber, 1, rowNumber, 8].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                sheet.SetRowHeight(rowNumber, 35);

                averageStartRow = rowNumber + 1;
                foreach (var memberSummary in milestoneSummary.OrderBy(ms => ms.memberName))
                {
                    rowNumber++;
                    sheet.Range[rowNumber, 1].Text = memberSummary.memberName;
                    sheet.Range[rowNumber, 1].BorderAround();
                    sheet.Range[rowNumber, 2].Number = memberSummary.percentComplete / 100.00;
                    sheet.Range[rowNumber, 2].NumberFormat = "0%";
                    sheet.Range[rowNumber, 2].BorderAround();
                    sheet.Range[rowNumber, 2].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
                    switch (milestoneSummary.Key)
                    {
                        case 1:
                            SetMilestoneCell(sheet.Range[rowNumber, 3], 1, participateAssistLead.participate, memberSummary.milestone1ParticipateCommunity);
                            SetMilestoneCell(sheet.Range[rowNumber, 4], 1, participateAssistLead.participate, memberSummary.milestone1ParticipateOutdoors);
                            SetMilestoneCell(sheet.Range[rowNumber, 5], 1, participateAssistLead.participate, memberSummary.milestone1ParticipateCreative);
                            SetMilestoneCell(sheet.Range[rowNumber, 6], 1, participateAssistLead.participate, memberSummary.milestone1ParticipatePersonalGrowth);
                            SetMilestoneCell(sheet.Range[rowNumber, 7], 1, participateAssistLead.assist, memberSummary.milestone1Assist);
                            SetMilestoneCell(sheet.Range[rowNumber, 8], 1, participateAssistLead.lead, memberSummary.milestone1Lead);
                            break;
                        case 2:
                            SetMilestoneCell(sheet.Range[rowNumber, 3], 2, participateAssistLead.participate, memberSummary.milestone2ParticipateCommunity);
                            SetMilestoneCell(sheet.Range[rowNumber, 4], 2, participateAssistLead.participate, memberSummary.milestone2ParticipateOutdoors);
                            SetMilestoneCell(sheet.Range[rowNumber, 5], 2, participateAssistLead.participate, memberSummary.milestone2ParticipateCreative);
                            SetMilestoneCell(sheet.Range[rowNumber, 6], 2, participateAssistLead.participate, memberSummary.milestone2ParticipatePersonalGrowth);
                            SetMilestoneCell(sheet.Range[rowNumber, 7], 2, participateAssistLead.assist, memberSummary.milestone2Assist);
                            SetMilestoneCell(sheet.Range[rowNumber, 8], 2, participateAssistLead.lead, memberSummary.milestone2Lead);
                            break;
                        case 3:
                            SetMilestoneCell(sheet.Range[rowNumber, 3], 3, participateAssistLead.participate, memberSummary.milestone3ParticipateCommunity);
                            SetMilestoneCell(sheet.Range[rowNumber, 4], 3, participateAssistLead.participate, memberSummary.milestone3ParticipateOutdoors);
                            SetMilestoneCell(sheet.Range[rowNumber, 5], 3, participateAssistLead.participate, memberSummary.milestone3ParticipateCreative);
                            SetMilestoneCell(sheet.Range[rowNumber, 6], 3, participateAssistLead.participate, memberSummary.milestone3ParticipatePersonalGrowth);
                            SetMilestoneCell(sheet.Range[rowNumber, 7], 3, participateAssistLead.assist, memberSummary.milestone3Assist);
                            SetMilestoneCell(sheet.Range[rowNumber, 8], 3, participateAssistLead.lead, memberSummary.milestone3Lead);
                            break;
                    }
                    sheet.Range[rowNumber, 3].BorderAround();
                    sheet.Range[rowNumber, 4].BorderAround();
                    sheet.Range[rowNumber, 5].BorderAround();
                    sheet.Range[rowNumber, 6].BorderAround();
                    sheet.Range[rowNumber, 7].BorderAround();
                    sheet.Range[rowNumber, 8].BorderAround();
                    sheet.Range[rowNumber, 2, rowNumber, 8].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                }

                // Add average row
                averageEndRow = rowNumber;
                rowNumber++;
                sheet.Range[rowNumber, 2].Text = "Average";
                sheet.Range[rowNumber, 2].BorderAround();
                for (int i = 3; i <= 8; i++)
                {
                    var avgRange = sheet.Range[averageStartRow, i, averageEndRow, i].AddressLocal;
                    sheet.Range[rowNumber, i].Formula = $"=AVERAGE({avgRange})";
                    sheet.Range[rowNumber, i].NumberFormat = "0.0";
                    sheet.Range[rowNumber, i].BorderAround();
                    sheet.Range[rowNumber, i].CellStyle.Font.Bold = true;
                    sheet.Range[rowNumber, i].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
                }
                sheet.Range[rowNumber, 2, rowNumber, 8].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;

                rowNumber++;
                rowNumber++;
            }

            sheet.Range[1, 1, rowNumber, 1].AutofitColumns();
            sheet.SetColumnWidth(2, 10);
            sheet.SetColumnWidth(3, 10);
            sheet.SetColumnWidth(4, 10);
            sheet.SetColumnWidth(5, 10);
            sheet.SetColumnWidth(6, 10);
            sheet.SetColumnWidth(7, 10);
            sheet.SetColumnWidth(8, 10);

            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
            sheet.PageSetup.BottomMargin = 0.25;
            sheet.PageSetup.TopMargin = 0.25;
            sheet.PageSetup.LeftMargin = 0.25;
            sheet.PageSetup.RightMargin = 0.25;
            sheet.PageSetup.HeaderMargin = 0;
            sheet.PageSetup.FooterMargin = 0;
            sheet.PageSetup.IsFitToPage = true;

            return workbook;
        }

        public IWorkbook GenerateLogbookWorkbook(List<MemberLogbookReportViewModel> logbookEntries, string groupName, string section, string unitName, bool forPdfOutput)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 8);
            IWorksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;

            IStyle headingStyle = workbook.Styles["headingStyle"];

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 8].Merge();
            sheet.SetRowHeight(rowNumber, 40);

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = $"Logbook entries as at {DateTime.Now.ToShortDateString()}";
            title.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 8].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            foreach (var logbookGroupEntry in logbookEntries.GroupBy(lb => lb.MemberName).OrderBy(lb => lb.Key))
            {
                rowNumber++;
                sheet.Range[rowNumber, 1].Text = logbookGroupEntry.Key;
                sheet.Range[rowNumber, 1, rowNumber, 2].Merge();
                sheet.Range[rowNumber, 3].Text = $"{logbookGroupEntry.FirstOrDefault()?.TotalKilometersHiked ?? 0} KMs";
                sheet.Range[rowNumber, 4].Text = $"{logbookGroupEntry.FirstOrDefault()?.TotalNightsCamped ?? 0} Nights";
                sheet.Range[rowNumber, 1, rowNumber, 4].CellStyle.Font.Bold = true;

                sheet.SetRowHeight(rowNumber, 20);

                foreach (var logbookEntry in logbookGroupEntry)
                {
                    rowNumber++;
                    sheet.Range[rowNumber, 2].Text = logbookEntry.ActivityName;
                    sheet.Range[rowNumber, 3].Text = logbookEntry.ActivityArea;
                    sheet.Range[rowNumber, 4].DateTime = logbookEntry.ActivityDate;
                    sheet.Range[rowNumber, 5].Text = logbookEntry.MemberRole;
                    sheet.Range[rowNumber, 6].Text = $"{logbookEntry.KilometersHiked} KMs";
                    sheet.Range[rowNumber, 7].Text = $"{logbookEntry.NightsCamped} Nights";
                    sheet.Range[rowNumber, 8].Text = logbookEntry.Verifier;
                }
            }

            sheet.SetColumnWidth(1, 10);
            sheet.SetColumnWidth(2, 40);
            sheet.Range[1, 2, rowNumber, 2].CellStyle.WrapText = true;
            sheet.SetColumnWidth(3, 15);
            sheet.SetColumnWidth(4, 15);
            sheet.SetColumnWidth(5, 15);
            sheet.SetColumnWidth(6, 10);
            sheet.SetColumnWidth(7, 10);
            sheet.Range[1, 8, rowNumber, 8].AutofitColumns();

            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
            sheet.PageSetup.BottomMargin = 0.5;
            sheet.PageSetup.TopMargin = 0.25;
            sheet.PageSetup.LeftMargin = 0.25;
            sheet.PageSetup.RightMargin = 0.25;
            sheet.PageSetup.HeaderMargin = 0;
            sheet.PageSetup.FooterMargin = 0.25;
            sheet.PageSetup.RightFooter = "&P of &N";

            return workbook;

        }

        public IWorkbook GenerateWallchartWorkbook(List<WallchartItemModel> wallchartEntries, string groupName, string section, string unitName, bool forPdfOutput)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 41);
            IWorksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;

            IStyle headingStyle = workbook.Styles["headingStyle"];

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 41].Merge();
            sheet.SetRowHeight(rowNumber, 25);

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = $"Group Life Wallchart as at {DateTime.Now.ToShortDateString()}";
            title.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 41].Merge();
            sheet.SetRowHeight(rowNumber, 25);

            // Add Heading 1
            rowNumber++;
            sheet.Range[rowNumber, 2, rowNumber, 3].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 2, rowNumber, 3].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 4].Text = "MILESTONE 1";
            sheet.Range[rowNumber, 4, rowNumber, 9].Merge();
            sheet.Range[rowNumber, 4, rowNumber, 9].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 4, rowNumber, 9].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 10].Text = "MILESTONE 2";
            sheet.Range[rowNumber, 10, rowNumber, 15].Merge();
            sheet.Range[rowNumber, 10, rowNumber, 15].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 10, rowNumber, 15].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 16].Text = "MILESTONE 3";
            sheet.Range[rowNumber, 16, rowNumber, 21].Merge();
            sheet.Range[rowNumber, 16, rowNumber, 21].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 16, rowNumber, 21].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 22].Text = "OUTDOOR ADVENTURE SKILLS";
            sheet.Range[rowNumber, 22, rowNumber, 33].Merge();
            sheet.Range[rowNumber, 22, rowNumber, 33].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 22, rowNumber, 33].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 34].Text = "SPECIAL INTEREST AREAS";
            sheet.Range[rowNumber, 34, rowNumber, 39].Merge();
            sheet.Range[rowNumber, 34, rowNumber, 39].CellStyle.WrapText = true;
            sheet.Range[rowNumber, 34, rowNumber, 39].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 34, rowNumber, 39].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 40, rowNumber, 43].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 40, rowNumber, 43].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 2, rowNumber, 43].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[rowNumber, 2, rowNumber, 43].CellStyle.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[rowNumber, 2, rowNumber, 43].CellStyle.Font.Bold = true;
            sheet.SetRowHeight(rowNumber, 30);

            // Add Heading 2
            rowNumber++;
            sheet.Range[rowNumber, 2, rowNumber, 21].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 2, rowNumber, 21].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 22].Text = "CORE";
            sheet.Range[rowNumber, 22, rowNumber, 24].Merge();
            sheet.Range[rowNumber, 22, rowNumber, 24].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 22, rowNumber, 24].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 25].Text = "LAND";
            sheet.Range[rowNumber, 25, rowNumber, 27].Merge();
            sheet.Range[rowNumber, 25, rowNumber, 27].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 25, rowNumber, 27].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 28].Text = "WATER";
            sheet.Range[rowNumber, 28, rowNumber, 30].Merge();
            sheet.Range[rowNumber, 28, rowNumber, 30].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 28, rowNumber, 30].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 31, rowNumber, 43].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 31, rowNumber, 43].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 2, rowNumber, 43].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[rowNumber, 2, rowNumber, 43].CellStyle.Font.Bold = true;

            // Add Heading 3
            rowNumber++;
            sheet.Range[rowNumber, 2, rowNumber, 43].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 2, rowNumber, 43].CellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
            sheet.Range[rowNumber, 2, rowNumber, 43].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[rowNumber, 2, rowNumber, 43].CellStyle.VerticalAlignment = ExcelVAlign.VAlignBottom;
            sheet.Range[rowNumber, 2, rowNumber, 43].CellStyle.Rotation = 90;
            sheet.Range[rowNumber, 1, rowNumber, 43].CellStyle.Font.Bold = true;
            sheet.Range[rowNumber, 1].Text = " Name";
            sheet.Range[rowNumber, 2].Text = " Intro To Scouting";
            sheet.Range[rowNumber, 3].Text = " Intro To Section";
            sheet.Range[rowNumber, 4].Text = " Community (6)";
            sheet.Range[rowNumber, 5].Text = " Creative (6)";
            sheet.Range[rowNumber, 6].Text = " Outdoors (6)";
            sheet.Range[rowNumber, 7].Text = " Personal Growth (6)";
            sheet.Range[rowNumber, 8].Text = " Assist (2)";
            sheet.Range[rowNumber, 9].Text = " Lead (1)";
            sheet.Range[rowNumber, 10].Text = " Community (5)";
            sheet.Range[rowNumber, 11].Text = " Creative (5)";
            sheet.Range[rowNumber, 12].Text = " Outdoors (5)";
            sheet.Range[rowNumber, 13].Text = " Personal Growth (5)";
            sheet.Range[rowNumber, 14].Text = " Assist (3)";
            sheet.Range[rowNumber, 15].Text = " Lead (2)";
            sheet.Range[rowNumber, 16].Text = " Community (4)";
            sheet.Range[rowNumber, 17].Text = " Creative (4)";
            sheet.Range[rowNumber, 18].Text = " Outdoors (4)";
            sheet.Range[rowNumber, 19].Text = " Personal Growth (4)";
            sheet.Range[rowNumber, 20].Text = " Assist (4)";
            sheet.Range[rowNumber, 21].Text = " Lead (4)";
            sheet.Range[rowNumber, 22].Text = " Bushcraft";
            sheet.Range[rowNumber, 23].Text = " Bushwalking";
            sheet.Range[rowNumber, 24].Text = " Camping";
            sheet.Range[rowNumber, 25].Text = " Alpine";
            sheet.Range[rowNumber, 26].Text = " Cycling";
            sheet.Range[rowNumber, 27].Text = " Vertical";
            sheet.Range[rowNumber, 28].Text = " Aquatics";
            sheet.Range[rowNumber, 29].Text = " Boating";
            sheet.Range[rowNumber, 30].Text = " Paddling";
            sheet.Range[rowNumber, 31].Text = " Total Progressions";
            sheet.Range[rowNumber, 32].Text = " Total Nights Camped";
            sheet.Range[rowNumber, 33].Text = " Total KMs Hiked";
            sheet.Range[rowNumber, 34].Text = " Adventure & Sport";
            sheet.Range[rowNumber, 35].Text = " Arts & Literature";
            sheet.Range[rowNumber, 36].Text = " Environment";
            sheet.Range[rowNumber, 37].Text = " STEM & Innovation";
            sheet.Range[rowNumber, 38].Text = " Growth & Development";
            sheet.Range[rowNumber, 39].Text = " Creating a Better World";
            sheet.Range[rowNumber, 40].Text = " Leadership Course";
            sheet.Range[rowNumber, 41].Text = " Adventurous Journey";
            sheet.Range[rowNumber, 42].Text = " Personal Reflection";
            sheet.Range[rowNumber, 43].Text = " PEAK AWARD";
            sheet.SetRowHeight(rowNumber, 120);

            foreach (var wallchartEntry in wallchartEntries)
            {
                rowNumber++;
                sheet.Range[rowNumber, 1].Text = wallchartEntry.MemberName;
                SetWallchartCell(sheet.Range[rowNumber, 2], 0, wallchartGroups.intro, wallchartEntry.IntroToScouting);
                SetWallchartCell(sheet.Range[rowNumber, 3], 0, wallchartGroups.intro, wallchartEntry.IntroToSection);
                if (wallchartEntry.Milestone1Presented.HasValue)
                {
                    sheet.Range[rowNumber, 4].DateTime = wallchartEntry.Milestone1Presented.Value;
                    sheet.Range[rowNumber, 4, rowNumber, 9].Merge();
                    sheet.Range[rowNumber, 4, rowNumber, 9].CellStyle.Color = Milestone1LeadColours[1];
                }
                else if (wallchartEntry.Milestone1Awarded.HasValue)
                {
                    sheet.Range[rowNumber, 4].Text = $"{wallchartEntry.Milestone1Awarded.Value.ToShortDateString()}*";
                    sheet.Range[rowNumber, 4, rowNumber, 9].Merge();
                    sheet.Range[rowNumber, 4, rowNumber, 9].CellStyle.Color = Milestone1LeadColours[1];
                }
                else
                {
                    SetWallchartCell(sheet.Range[rowNumber, 4], 1, wallchartGroups.participate, wallchartEntry.Milestone1Community);
                    SetWallchartCell(sheet.Range[rowNumber, 5], 1, wallchartGroups.participate, wallchartEntry.Milestone1Creative);
                    SetWallchartCell(sheet.Range[rowNumber, 6], 1, wallchartGroups.participate, wallchartEntry.Milestone1Outdoors);
                    SetWallchartCell(sheet.Range[rowNumber, 7], 1, wallchartGroups.participate, wallchartEntry.Milestone1PersonalGrowth);
                    SetWallchartCell(sheet.Range[rowNumber, 8], 1, wallchartGroups.assist, wallchartEntry.Milestone1Assist);
                    SetWallchartCell(sheet.Range[rowNumber, 9], 1, wallchartGroups.lead, wallchartEntry.Milestone1Lead);
                }

                if (wallchartEntry.Milestone2Presented.HasValue)
                {
                    sheet.Range[rowNumber, 10].DateTime = wallchartEntry.Milestone2Presented.Value;
                    sheet.Range[rowNumber, 10, rowNumber, 15].Merge();
                    sheet.Range[rowNumber, 10, rowNumber, 15].CellStyle.Color = Milestone1LeadColours[1];
                }
                else if (wallchartEntry.Milestone2Awarded.HasValue)
                {
                    sheet.Range[rowNumber, 10].Text = $"{wallchartEntry.Milestone2Awarded.Value.ToShortDateString()}*";
                    sheet.Range[rowNumber, 10, rowNumber, 15].Merge();
                    sheet.Range[rowNumber, 10, rowNumber, 15].CellStyle.Color = Milestone1LeadColours[1];
                }
                else
                {
                    SetWallchartCell(sheet.Range[rowNumber, 10], 2, wallchartGroups.participate, wallchartEntry.Milestone2Community);
                    SetWallchartCell(sheet.Range[rowNumber, 11], 2, wallchartGroups.participate, wallchartEntry.Milestone2Creative);
                    SetWallchartCell(sheet.Range[rowNumber, 12], 2, wallchartGroups.participate, wallchartEntry.Milestone2Outdoors);
                    SetWallchartCell(sheet.Range[rowNumber, 13], 2, wallchartGroups.participate, wallchartEntry.Milestone2PersonalGrowth);
                    SetWallchartCell(sheet.Range[rowNumber, 14], 2, wallchartGroups.assist, wallchartEntry.Milestone2Assist);
                    SetWallchartCell(sheet.Range[rowNumber, 15], 2, wallchartGroups.lead, wallchartEntry.Milestone2Lead);
                }

                if (wallchartEntry.Milestone3Presented.HasValue)
                {
                    sheet.Range[rowNumber, 16].DateTime = wallchartEntry.Milestone3Presented.Value;
                    sheet.Range[rowNumber, 16, rowNumber, 21].Merge();
                    sheet.Range[rowNumber, 16, rowNumber, 21].CellStyle.Color = Milestone1LeadColours[1];
                }
                else if (wallchartEntry.Milestone3Awarded.HasValue)
                {
                    sheet.Range[rowNumber, 16].Text = $"{wallchartEntry.Milestone3Awarded.Value.ToShortDateString()}*";
                    sheet.Range[rowNumber, 16, rowNumber, 21].Merge();
                    sheet.Range[rowNumber, 16, rowNumber, 21].CellStyle.Color = Milestone1LeadColours[1];
                }
                else
                {
                    SetWallchartCell(sheet.Range[rowNumber, 16], 3, wallchartGroups.participate, wallchartEntry.Milestone3Community);
                    SetWallchartCell(sheet.Range[rowNumber, 17], 3, wallchartGroups.participate, wallchartEntry.Milestone3Creative);
                    SetWallchartCell(sheet.Range[rowNumber, 18], 3, wallchartGroups.participate, wallchartEntry.Milestone3Outdoors);
                    SetWallchartCell(sheet.Range[rowNumber, 19], 3, wallchartGroups.participate, wallchartEntry.Milestone3PersonalGrowth);
                    SetWallchartCell(sheet.Range[rowNumber, 20], 3, wallchartGroups.assist, wallchartEntry.Milestone3Assist);
                    SetWallchartCell(sheet.Range[rowNumber, 21], 3, wallchartGroups.lead, wallchartEntry.Milestone3Lead);
                }
                SetWallchartCell(sheet.Range[rowNumber, 22], 0, wallchartGroups.oasCore, wallchartEntry.OASBushcraftStage);
                SetWallchartCell(sheet.Range[rowNumber, 23], 0, wallchartGroups.oasCore, wallchartEntry.OASBushwalkingStage);
                SetWallchartCell(sheet.Range[rowNumber, 24], 0, wallchartGroups.oasCore, wallchartEntry.OASCampingStage);
                SetWallchartCell(sheet.Range[rowNumber, 25], 0, wallchartGroups.oasLand, wallchartEntry.OASAlpineStage);
                SetWallchartCell(sheet.Range[rowNumber, 26], 0, wallchartGroups.oasLand, wallchartEntry.OASCyclingStage);
                SetWallchartCell(sheet.Range[rowNumber, 27], 0, wallchartGroups.oasLand, wallchartEntry.OASVerticalStage);
                SetWallchartCell(sheet.Range[rowNumber, 28], 0, wallchartGroups.oasWater, wallchartEntry.OASAquaticsStage);
                SetWallchartCell(sheet.Range[rowNumber, 29], 0, wallchartGroups.oasWater, wallchartEntry.OASBoatingStage);
                SetWallchartCell(sheet.Range[rowNumber, 30], 0, wallchartGroups.oasWater, wallchartEntry.OASPaddlingStage);
                SetWallchartCell(sheet.Range[rowNumber, 31], 0, wallchartGroups.oasProgression, wallchartEntry.OASStageProgressions);
                SetWallchartCell(sheet.Range[rowNumber, 32], 0, wallchartGroups.oasProgression, wallchartEntry.NightsCamped);
                SetWallchartCell(sheet.Range[rowNumber, 33], 0, wallchartGroups.oasProgression, wallchartEntry.KMsHiked);
                SetWallchartCell(sheet.Range[rowNumber, 34], 0, wallchartGroups.siaOdd, wallchartEntry.SIAAdventureSport);
                SetWallchartCell(sheet.Range[rowNumber, 35], 0, wallchartGroups.siaEven, wallchartEntry.SIAArtsLiterature);
                SetWallchartCell(sheet.Range[rowNumber, 36], 0, wallchartGroups.siaOdd, wallchartEntry.SIAEnvironment);
                SetWallchartCell(sheet.Range[rowNumber, 37], 0, wallchartGroups.siaEven, wallchartEntry.SIAStemInnovation);
                SetWallchartCell(sheet.Range[rowNumber, 38], 0, wallchartGroups.siaOdd, wallchartEntry.SIAGrowthDevelopment);
                SetWallchartCell(sheet.Range[rowNumber, 39], 0, wallchartGroups.siaEven, wallchartEntry.SIACreatingABetterWorld);
                SetWallchartCell(sheet.Range[rowNumber, 40], 0, wallchartGroups.leadershipCourse, wallchartEntry.LeadershipCourse);
                SetWallchartCell(sheet.Range[rowNumber, 41], 0, wallchartGroups.adventurousJourney, wallchartEntry.AdventurousJourney);
                SetWallchartCell(sheet.Range[rowNumber, 42], 0, wallchartGroups.personalReflection, wallchartEntry.PersonalReflection);
                SetWallchartCell(sheet.Range[rowNumber, 43], 0, wallchartGroups.intro, wallchartEntry.PeakAward);
                sheet.Range[rowNumber, 1, rowNumber, 43].BorderAround();
                sheet.Range[rowNumber, 1, rowNumber, 43].BorderInside();
                sheet.Range[rowNumber, 2, rowNumber, 43].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
            }


            sheet.Range[1, 1, rowNumber, 43].AutofitColumns();

            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
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


        public IWorkbook GenerateApprovalsWorkbook(List<ApprovalsListModel> selectedApprovals, string groupName, string section, string unitName, DateTime approvalSearchFromDate, DateTime approvalSearchToDate, bool groupByMember, bool forPdfOutput)
        {
            var workbook = CreateWorkbookWithLogo(groupName, section, 8);
            IWorksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;
            int averageStartRow = 0;
            int averageEndRow = 0;

            IStyle headingStyle = workbook.Styles["headingStyle"];

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 8].Merge();
            sheet.SetRowHeight(rowNumber, 40);

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = $"Award Approvals between {approvalSearchFromDate.ToShortDateString()} and {approvalSearchToDate.ToShortDateString()}";
            title.CellStyle = headingStyle;
            sheet.Range[rowNumber, 2, rowNumber, 8].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            var groupedApprovals = new List<IGrouping<string, ApprovalsListModel>>();
            if (groupByMember)
                groupedApprovals = selectedApprovals.GroupBy(a => a.member_display_name).ToList();
            else
                groupedApprovals = selectedApprovals.GroupBy(a => a.achievement_name).ToList();

            foreach (var approvalGroup in groupedApprovals.OrderBy(a => a.Key))
            {
                rowNumber++;
                sheet.Range[rowNumber, 1].Text = approvalGroup.Key;
                sheet.Range[rowNumber, 1, rowNumber, 4].Merge();
                if (rowNumber == 4)
                {
                    sheet.Range[rowNumber, 6].Text = "Awarded";
                    sheet.Range[rowNumber, 6].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[rowNumber, 7].Text = "Presented";
                    sheet.Range[rowNumber, 7].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                }
                sheet.Range[rowNumber, 1, rowNumber, 7].CellStyle.Font.Bold = true;

                sheet.SetRowHeight(rowNumber, 20);

                foreach (var approval in approvalGroup.OrderBy(a => a.member_display_name).ThenBy(a => a.achievement_name))
                {
                    rowNumber++;
                    sheet.Range[rowNumber, 2].Text = groupByMember ? approval.achievement_name : approval.member_display_name;
                    sheet.Range[rowNumber, 3].Text = approval.submission_type;
                    sheet.Range[rowNumber, 4].Text = approval.submission_status;
                    sheet.Range[rowNumber, 5].Text = approval.submission_outcome;
                    if (approval.awarded_date.HasValue)
                        sheet.Range[rowNumber, 6].DateTime = approval.awarded_date.Value;
                    if (approval.presented_date.HasValue)
                        sheet.Range[rowNumber, 7].DateTime = approval.presented_date.Value;
                }
            }

            sheet.Range[4, 2, rowNumber, 7].AutofitColumns();

            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
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

        private Color GetInputTitleColour(string inputTitle)
        {
            switch (inputTitle)
            {
                case "Plan":
                    return Color.LightBlue;
                case "Do":
                    return Color.SkyBlue;
                case "Review":
                    return Color.CornflowerBlue;
                case "Verify":
                    return Color.RoyalBlue;
                default:
                    return Color.White;
            }
        }

        private string GetParticipateHeadingText(int currentLevel)
        {
            switch (currentLevel)
            {
                case 1:
                    return "Participate - 6";
                case 2:
                    return "Participate - 5";
                case 3:
                    return "Participate - 4";
                default:
                    return "Participate";
            }
        }

        private string GetAssistHeadingText(int currentLevel)
        {
            switch (currentLevel)
            {
                case 1:
                    return "Assist - 2";
                case 2:
                    return "Assist - 3";
                case 3:
                    return "Assist - 4";
                default:
                    return "Assist";
            }
        }

        private string GetLeadHeadingText(int currentLevel)
        {
            switch (currentLevel)
            {
                case 1:
                    return "Lead - 1";
                case 2:
                    return "Lead - 2";
                case 3:
                    return "Lead - 4";
                default:
                    return "Lead";
            }
        }

        private void SetMilestoneCell(IRange cell, int currentLevel, participateAssistLead pal, int count)
        {
            cell.Number = count;
            cell.CellStyle.Color = GetMilestoneProgressColour(currentLevel, pal, count);
        }
        private Color GetMilestoneProgressColour(int currentLevel, participateAssistLead pal, int count)
        {
            // Skipped milestones have a count of -1
            if (count < 0)
                return Color.White;

            if (currentLevel == 1)
            {
                switch (pal)
                {
                    case participateAssistLead.participate:
                        return Milestone1ParticipateColours[Math.Min(count, 6)];
                    case participateAssistLead.assist:
                        return Milestone1AssistColours[Math.Min(count, 2)];
                    case participateAssistLead.lead:
                        return Milestone1LeadColours[Math.Min(count, 1)];
                    default:
                        return Color.White;
                }
            }

            if (currentLevel == 2)
            {
                switch (pal)
                {
                    case participateAssistLead.participate:
                        return Milestone2ParticipateColours[Math.Min(count, 5)];
                    case participateAssistLead.assist:
                        return Milestone2AssistColours[Math.Min(count, 3)];
                    case participateAssistLead.lead:
                        return Milestone1AssistColours[Math.Min(count, 2)];
                    default:
                        return Color.White;
                }
            }

            if (currentLevel == 3)
            {
                switch (pal)
                {
                    case participateAssistLead.participate:
                        return Milestone3Colours[Math.Min(count, 4)];
                    case participateAssistLead.assist:
                        return Milestone3Colours[Math.Min(count, 4)];
                    case participateAssistLead.lead:
                        return Milestone3Colours[Math.Min(count, 4)];
                    default:
                        return Color.White;
                }
            }

            return Color.White;
        }

        private void SetWallchartCell(IRange cell, int currentLevel, wallchartGroups wallchartGroups, int count)
        {
            if (count > 0)
                cell.Number = count;
            SetWallchartCellBackground(cell, currentLevel, wallchartGroups, count);
        }

        private void SetWallchartCell(IRange cell, int currentLevel, wallchartGroups wallchartGroups, DateTime? dateAwarded)
        {
            if (dateAwarded.HasValue)
            {
                cell.DateTime = dateAwarded.Value;
                cell.NumberFormat = "dd/MM/yy";
            }
            SetWallchartCellBackground(cell, currentLevel, wallchartGroups, 0);
        }

        private void SetWallchartCell(IRange cell, int currentLevel, wallchartGroups wallchartGroups, double percent)
        {
            cell.Number = percent;
            cell.NumberFormat = "0%";
            SetWallchartCellBackground(cell, currentLevel, wallchartGroups, 0);
        }

        private void SetWallchartCellBackground(IRange cell, int currentLevel, wallchartGroups wallchartGroup, int count)
        {
            switch (wallchartGroup)
            {
                case wallchartGroups.intro:
                    cell.CellStyle.Color = Color.Gainsboro;
                    break;
                case wallchartGroups.participate:
                    switch (currentLevel)
                    {
                        case 1:
                            if (count == 6)
                                cell.CellStyle.Color = Milestone1ParticipateColours[6];
                            break;
                        case 2:
                            if (count == 5)
                                cell.CellStyle.Color = Milestone2ParticipateColours[5];
                            break;
                        case 3:
                            if (count >= 4)
                                cell.CellStyle.Color = Milestone3Colours[4];
                            break;
                    }
                    break;
                case wallchartGroups.assist:
                    switch (currentLevel)
                    {
                        case 1:
                            if (count == 2)
                                cell.CellStyle.Color = Milestone1AssistColours[2];
                            else
                                cell.CellStyle.Color = Color.Gainsboro;
                            break;
                        case 2:
                            if (count == 3)
                                cell.CellStyle.Color = Milestone2AssistColours[3];
                            else
                                cell.CellStyle.Color = Color.Gainsboro;
                            break;
                        case 3:
                            if (count >= 4)
                                cell.CellStyle.Color = Milestone3Colours[4];
                            else
                                cell.CellStyle.Color = Color.Gainsboro;
                            break;
                    }
                    break;
                case wallchartGroups.lead:
                    switch (currentLevel)
                    {
                        case 1:
                            if (count == 1)
                                cell.CellStyle.Color = Milestone1LeadColours[1];
                            else
                                cell.CellStyle.Color = Color.Gainsboro;
                            break;
                        case 2:
                            if (count == 2)
                                cell.CellStyle.Color = Milestone1AssistColours[2];
                            else
                                cell.CellStyle.Color = Color.Gainsboro;
                            break;
                        case 3:
                            if (count >= 4)
                                cell.CellStyle.Color = Milestone3Colours[4];
                            else
                                cell.CellStyle.Color = Color.Gainsboro;
                            break;
                    }
                    break;
                case wallchartGroups.oasCore:
                case wallchartGroups.adventurousJourney:
                    cell.CellStyle.Color = Color.DarkSeaGreen;
                    break;
                case wallchartGroups.oasLand:
                case wallchartGroups.personalReflection:
                    cell.CellStyle.Color = Color.LightSalmon;
                    break;
                case wallchartGroups.oasWater:
                case wallchartGroups.leadershipCourse:
                    cell.CellStyle.Color = Color.LightBlue;
                    break;
                case wallchartGroups.oasProgression:
                    cell.CellStyle.Color = Color.Gainsboro;
                    break;
                case wallchartGroups.siaOdd:
                    cell.CellStyle.Color = Color.MistyRose;
                    break;
                case wallchartGroups.siaEven:
                    cell.CellStyle.Color = Color.LightYellow;
                    break;
                case wallchartGroups.peakAward:
                    cell.CellStyle.Color = Color.Thistle;
                    break;
            }
        }

    }
}
