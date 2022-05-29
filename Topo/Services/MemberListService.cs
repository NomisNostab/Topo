using Newtonsoft.Json;
using System.Text;
using Topo.Models.MemberList;
using Syncfusion.XlsIO;
using Topo.Images;
using System.Globalization;

namespace Topo.Services
{
    public interface IMemberListService
    {
        public Task<List<MemberListModel>> GetMembersAsync();
        public IWorkbook GeneratePatrolList(List<MemberListModel> sortedPatrolList, string section, string unitName, bool includeLeaders);
        public IWorkbook GenerateMemberList(List<MemberListModel> sortedMemberList, string section, string unitName);
        public IWorkbook GeneratePatrolSheets(List<MemberListModel> sortedPatrolList, string section, string unitName);
    }

    public class MemberListService : IMemberListService
    {
        private readonly StorageService _storageService;
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly IImages _images;

        public MemberListService(StorageService storageService, ITerrainAPIService terrainAPIService, IImages images)
        {
            _storageService = storageService;
            _terrainAPIService = terrainAPIService;
            _images = images;
        }

        public async Task<List<MemberListModel>> GetMembersAsync()
        {
            var cachedMembersList = _storageService.CachedMembers.Where(cm => cm.Key == _storageService.SelectedUnitId).FirstOrDefault().Value;
            if (cachedMembersList != null)
                return cachedMembersList;

            var getMembersResultModel = await _terrainAPIService.GetMembersAsync(_storageService.SelectedUnitId ?? "");
            var memberList = new List<MemberListModel>();
            if (getMembersResultModel != null && getMembersResultModel.results != null)
            {
                memberList = getMembersResultModel.results
                    .Select(m => new MemberListModel
                    {
                        id = m.id,
                        member_number = m.member_number,
                        first_name = m.first_name,
                        last_name = m.last_name,
                        date_of_birth = DateTime.ParseExact(m.date_of_birth, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        age = GetAgeFromBirthdate(m.date_of_birth),
                        unit_council = m.unit.unit_council,
                        patrol_name = m.patrol == null ? "" : m.patrol.name,
                        patrol_duty = m.patrol == null ? "" : GetPatrolDuty(m.unit.duty, m.patrol.duty),
                        patrol_order = m.patrol == null ? 3 : GetPatrolOrder(m.unit.duty, m.patrol.duty),
                        isAdultLeader = m.unit.duty == "adult_leader" ? 1 : 0,
                        status = m.status
                    })
                    .ToList();
                _storageService.CachedMembers.Add(new KeyValuePair<string, List<MemberListModel>>(_storageService.SelectedUnitId, memberList));
            }
            return memberList;
        }

        private string GetAgeFromBirthdate(string dateOfBirth)
        {
            var birthday = DateTime.ParseExact(dateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture); // Date in AU format
            DateTime now = DateTime.Today;
            int months = now.Month - birthday.Month;
            int years = now.Year - birthday.Year;

            if (now.Day < birthday.Day)
            {
                months--;
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }
            return $"{years}y {months}m";
        }

        public IWorkbook GeneratePatrolList(List<MemberListModel> sortedPatrolList, string section, string unitName, bool includeLeaders)
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
            sheet.Range[rowNumber, 2, rowNumber, 9].Merge();
            sheet.SetRowHeight(rowNumber, 30);

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

        public IWorkbook GenerateMemberList(List<MemberListModel> sortedMemberList, string section, string unitName)
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

        public IWorkbook GeneratePatrolSheets(List<MemberListModel> sortedPatrolList, string section, string unitName)
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

        private string GetPatrolDuty(string unitDuty, string patrolDuty)
        {
            if (unitDuty == "unit_leader")
                return "UL";
            if (patrolDuty == "assistant_patrol_leader")
                return "APL";
            if (patrolDuty == "patrol_leader")
                return "PL";
            return "";
        }

        private int GetPatrolOrder(string unitDuty, string patrolDuty)
        {
            if (unitDuty == "unit_leader")
                return 0;
            if (patrolDuty == "assistant_patrol_leader")
                return 2;
            if (patrolDuty == "patrol_leader")
                return 1;
            return 3;
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
    }
}
