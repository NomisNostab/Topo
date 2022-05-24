using Spire.Xls;
using System.Globalization;
using Topo.Images;
using Topo.Models.AditionalAwards;

namespace Topo.Services
{
    public interface IAdditionalAwardService
    {
        public Task<Workbook> GenerateAdditionalAwardReport(List<KeyValuePair<string, string>> selectedMembers);
    }
    public class AdditionalAwardService : IAdditionalAwardService
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly ILogger<ISIAService> _logger;
        private readonly IImages _images;

        public AdditionalAwardService(IMemberListService memberListService,
            ITerrainAPIService terrainAPIService,
            StorageService storageService, ILogger<ISIAService> logger,
            IImages images)
        {
            _memberListService = memberListService;
            _terrainAPIService = terrainAPIService;
            _storageService = storageService;
            _logger = logger;
            _images = images;
        }
        public async Task<Workbook> GenerateAdditionalAwardReport(List<KeyValuePair<string, string>> selectedMembers)
        {
            var unitName = _storageService.SelectedUnitName ?? "";
            var unit = _storageService.GetProfilesResult.profiles.FirstOrDefault(u => u.unit.name == unitName);
            if (unit == null)
                throw new IndexOutOfRangeException($"No unit found with name {unitName}. You may not have permissions to this section");
            var section = unit.unit.section;
            var awardSpecificationsList = _storageService.AdditionalAwardSpecifications;
            if (awardSpecificationsList == null || awardSpecificationsList.Count == 0)
            {
                var additionalAwardsSpecifications = await _terrainAPIService.GetAditionalAwardSpecifications();
                var additionalAwardSortIndex = 0;
                awardSpecificationsList = additionalAwardsSpecifications.AwardDescriptions
                    .Select(x => new AdditionalAwardSpecificationListModel()
                    {
                        id = x.id,
                        name = x.title,
                        additionalAwardSortIndex = additionalAwardSortIndex++,
                    })
                    .ToList();
                _storageService.AdditionalAwardSpecifications = awardSpecificationsList;
            }
            var unitAchievementsResult = await _terrainAPIService.GetUnitAdditionalAwardAchievements(_storageService.SelectedUnitId ?? "");
            var additionalAwardsList = new List<AdditionalAwardListModel>();
            var lastMemberProcessed = "";
            var memberName = "";
            foreach (var result in unitAchievementsResult.results)
            {
                var memberKVP = selectedMembers.Where(m => m.Key == result.member_id).FirstOrDefault();
                if (memberKVP.Key != null)
                {
                    if (memberKVP.Key != lastMemberProcessed)
                    {
                        await _terrainAPIService.RevokeAssumedProfiles();
                        await _terrainAPIService.AssumeProfile(memberKVP.Key);
                        var getMemberLogbookMetrics = await _terrainAPIService.GetMemberLogbookMetrics(memberKVP.Key);
                        var totalNightsCamped = getMemberLogbookMetrics.results.Where(r => r.name == "total_nights_camped").FirstOrDefault()?.value ?? 0;
                        var totalKmsHiked = (getMemberLogbookMetrics.results.Where(r => r.name == "total_distance_hiked").FirstOrDefault()?.value ?? 0) / 1000.0f;
                        memberName = $"{memberKVP.Value} ({totalNightsCamped} Nights, {totalKmsHiked} KMs)";
                        lastMemberProcessed = memberKVP.Key;
                    }
                    var awardSpecification = awardSpecificationsList.Where(a => a.id == result.achievement_meta.additional_award_id).FirstOrDefault();
                    var awardStatus = result.status;
                    var awardStatusDate = result.status_updated;
                    if (result.imported != null)
                        awardStatusDate = DateTime.ParseExact(result.imported.date_awarded, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    additionalAwardsList.Add(new AdditionalAwardListModel
                    {
                        MemberName = memberName,
                        AwardId = awardSpecification?.id ?? "",
                        AwardName = awardSpecification?.name ?? "",
                        AwardSortIndex = awardSpecification?.additionalAwardSortIndex ?? 0,
                        AwardDate = awardStatusDate
                    });
                }
            }
            await _terrainAPIService.RevokeAssumedProfiles();
            var sortedAdditionalAwardsList = additionalAwardsList.OrderBy(a => a.MemberName).ThenBy(a => a.AwardSortIndex).ToList();
            var distinctAwards = sortedAdditionalAwardsList.OrderBy(x => x.AwardSortIndex).Select(x => x.AwardId).Distinct().ToList();
            var workbook = BuildXLS(awardSpecificationsList, sortedAdditionalAwardsList, distinctAwards, section, unitName);

            return workbook;
        }

        private Workbook BuildXLS(List<AdditionalAwardSpecificationListModel> awardSpecificationsList, List<AdditionalAwardListModel> sortedAdditionalAwardsList, List<string>? distinctAwards, string section, string unitName)
        {
            //Create a new workbook
            Workbook workbook = new Workbook();
            //Initialize worksheet        
            Worksheet sheet = workbook.Worksheets[0];
            int rowNumber = 1;

            // Add Logo
            var directory = Directory.GetCurrentDirectory();
            var logoName = _images.GetLogoForSection(section);
            var logo = sheet.Pictures.Add(rowNumber, 1, $@"{directory}/Images/{logoName}");
            var aspectRatio = (double)logo.Height / logo.Width;
            logo.Width = 100;
            logo.Height = (int)(100 * aspectRatio);
            
            // Add Group Name
            var groupName = sheet.Range[rowNumber, 2];
            groupName.Text = _storageService.GroupName;
            groupName.Style.Font.IsBold = true;
            groupName.Style.Font.Size = 14;
            groupName.Style.HorizontalAlignment = HorizontalAlignType.Center;
            groupName.Style.VerticalAlignment = VerticalAlignType.Center;
            sheet.Range[rowNumber, 2, rowNumber, 16].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            // Add Title
            rowNumber++;
            var title = sheet.Range[rowNumber, 2];
            title.Text = $"Additional Badges Awarded as at {DateTime.Now.ToShortDateString()}";
            title.Style.Font.IsBold = true;
            title.Style.Font.Size = 14;
            title.Style.HorizontalAlignment = HorizontalAlignType.Center;
            title.Style.VerticalAlignment = VerticalAlignType.Center;
            sheet.Range[rowNumber, 2, rowNumber, 16].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            // Add Unit name
            rowNumber++;
            var unit = sheet.Range[rowNumber, 2];
            unit.Text = unitName;
            unit.Style.Font.IsBold = true;
            unit.Style.Font.Size = 14;
            unit.Style.HorizontalAlignment = HorizontalAlignType.Center;
            unit.Style.VerticalAlignment = VerticalAlignType.Center;
            sheet.Range[rowNumber, 2, rowNumber, 16].Merge();
            sheet.SetRowHeight(rowNumber, 30);

            //Add Heading Row
            rowNumber++;
            var columnNumber = 1;
            sheet.Range[rowNumber, 1].Text = "Scout";
            sheet.Range[rowNumber, 1].Style.HorizontalAlignment = HorizontalAlignType.Center;
            var usedAwards = awardSpecificationsList.Where(x => distinctAwards.Contains(x.id));
            foreach (var award in usedAwards.OrderBy(x => x.additionalAwardSortIndex))
            {
                columnNumber = distinctAwards.IndexOf(award.id) + 1;
                var cell = sheet.Range[rowNumber, columnNumber + 1];
                cell.Text = award.name;
                cell.Style.Rotation = 90;
                cell.Style.HorizontalAlignment = HorizontalAlignType.Center;
                cell.Style.VerticalAlignment = VerticalAlignType.Bottom;
                cell.BorderAround(LineStyleType.Thin);
            }
            rowNumber++;
            // Add detail rows
            foreach (var additionalAward in sortedAdditionalAwardsList.GroupBy(x => x.MemberName))
            {
                sheet.SetRowHeight(rowNumber, 15);
                sheet.Range[rowNumber, 1].Text = additionalAward.Key;
                sheet.Range[rowNumber, 1].BorderAround(LineStyleType.Thin);
                sheet.Range[rowNumber, 1].Style.VerticalAlignment = VerticalAlignType.Center;
                // Set style for date boxes
                for (int i = 0; i < distinctAwards.Count(); i++)
                {
                    var cell = sheet.Range[rowNumber, i + 2];
                    cell.Style.HorizontalAlignment = HorizontalAlignType.Center;
                    cell.Style.VerticalAlignment = VerticalAlignType.Center;
                    cell.BorderAround(LineStyleType.Thin);
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
            sheet.AllocatedRange.AutoFitColumns();
            return workbook;
        }
    }
}
