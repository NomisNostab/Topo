using Syncfusion.XlsIO;
using System.Globalization;
using Topo.Models.AditionalAwards;

namespace Topo.Services
{
    public interface IAdditionalAwardService
    {
        public Task<IWorkbook> GenerateAdditionalAwardReport(string selectedUnitId, List<KeyValuePair<string, string>> selectedMembers);
    }
    public class AdditionalAwardService : IAdditionalAwardService
    {
        private readonly StorageService _storageService;
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly ILogger<ISIAService> _logger;
        private readonly IReportService _reportService;
        private readonly IApprovalsService _approvalsService;

        public AdditionalAwardService(ITerrainAPIService terrainAPIService,
            StorageService storageService, ILogger<ISIAService> logger,
            IReportService reportService, IApprovalsService approvalsService)
        {
            _terrainAPIService = terrainAPIService;
            _storageService = storageService;
            _logger = logger;
            _reportService = reportService;
            _approvalsService = approvalsService;
        }
        public async Task<IWorkbook> GenerateAdditionalAwardReport(string selectedUnitId, List<KeyValuePair<string, string>> selectedMembers)
        {
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var section = _storageService.SelectedSection;
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
            var unitAchievementsResult = await _terrainAPIService.GetUnitAdditionalAwardAchievements(selectedUnitId ?? "");
            var approvedAwards = await _approvalsService.GetApprovalListItems(selectedUnitId ?? "");
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
                    var award = approvedAwards.Where(a => a.achievement_id == result.id && a.submission_type.ToLower() == "review").FirstOrDefault();
                    DateTime? awardPresentedDate = new DateTime?();
                    if (award != null)
                    {
                        if (award.presented_date.HasValue)
                        {
                            awardPresentedDate = award.presented_date.Value.ToLocalTime();
                        }
                    }
                    additionalAwardsList.Add(new AdditionalAwardListModel
                    {
                        MemberName = memberName,
                        AwardId = awardSpecification?.id ?? "",
                        AwardName = awardSpecification?.name ?? "",
                        AwardSortIndex = awardSpecification?.additionalAwardSortIndex ?? 0,
                        AwardDate = awardStatusDate,
                        PresentedDate = awardPresentedDate ?? null
                    });
                }
            }
            await _terrainAPIService.RevokeAssumedProfiles();
            var sortedAdditionalAwardsList = additionalAwardsList.OrderBy(a => a.MemberName).ThenBy(a => a.AwardSortIndex).ToList();
            var distinctAwards = sortedAdditionalAwardsList.OrderBy(x => x.AwardSortIndex).Select(x => x.AwardId).Distinct().ToList();
            var workbook = _reportService.GenerateAdditionalAwardsWorkbook(awardSpecificationsList, sortedAdditionalAwardsList, distinctAwards, groupName, section, unitName);

            return workbook;
        }
    }
}
