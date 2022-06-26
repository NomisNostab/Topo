using Topo.Models.Milestone;

namespace Topo.Services
{
    public interface IMilestoneService
    {
        Task<List<MilestoneSummaryListModel>> GetMilestoneSummaries(string selectedUnitId);
    }
    public class MilestoneService : IMilestoneService
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly ILogger<ISIAService> _logger;

        public MilestoneService(IMemberListService memberListService,
            ITerrainAPIService terrainAPIService,
            StorageService storageService, ILogger<ISIAService> logger)
        {
            _memberListService = memberListService;
            _terrainAPIService = terrainAPIService;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<List<MilestoneSummaryListModel>> GetMilestoneSummaries(string selectedUnitId)
        {
            var unitMilestoneSummary = new List<MilestoneSummaryListModel>();
            var getGroupLifeResultModel = await _terrainAPIService.GetGroupLifeForUnit(selectedUnitId);
            foreach (var result in getGroupLifeResultModel.results)
            {
                var milestone1 = result.milestones.Where(m => m.milestone == 1).FirstOrDefault();
                var milestone1Awarded = result.milestone.milestone > 1 && (milestone1?.awarded ?? false);
                var milestone1Skipped = result.milestone.milestone > 1 && !(milestone1?.awarded ?? false);
                var milestone2 = result.milestones.Where(m => m.milestone == 2).FirstOrDefault();
                var milestone2Awarded = result.milestone.milestone > 2 && (milestone2?.awarded ?? false);
                var milestone2Skipped = result.milestone.milestone > 2 && !(milestone2?.awarded ?? false);
                var milestone3 = result.milestones.Where(m => m.milestone == 3).FirstOrDefault();
                var milestone3Awarded = milestone3?.awarded ?? false;
                unitMilestoneSummary.Add(
                    new MilestoneSummaryListModel
                    {
                        memberName = result.name,
                        currentLevel = result.milestone.milestone,
                        percentComplete = CalculateMilestonePercentComplete(result.milestone),
                        milestone1ParticipateCommunity = milestone1Skipped ? -1 : (milestone1Awarded ? 6 : milestone1?.participates.Where(p => p.challenge_area == "community").FirstOrDefault()?.total ?? 0),
                        milestone1ParticipateOutdoors = milestone1Skipped ? -1 : (milestone1Awarded ? 6 : milestone1?.participates.Where(p => p.challenge_area == "outdoors").FirstOrDefault()?.total ?? 0),
                        milestone1ParticipateCreative = milestone1Skipped ? -1 : (milestone1Awarded ? 6 : milestone1?.participates.Where(p => p.challenge_area == "creative").FirstOrDefault()?.total ?? 0),
                        milestone1ParticipatePersonalGrowth = milestone1Skipped ? -1 : (milestone1Awarded ? 6 : milestone1?.participates.Where(p => p.challenge_area == "personal_growth").FirstOrDefault()?.total ?? 0),
                        milestone1Assist = milestone1Skipped ? -1 : (milestone1Awarded ? 2 : milestone1?.total_assists ?? 0),
                        milestone1Lead = milestone1Skipped ? -1 : (milestone1Awarded ? 1 : milestone1?.total_leads ?? 0),
                        milestone2ParticipateCommunity = milestone2Skipped ? -1 : (milestone2Awarded ? 5 : milestone2?.participates.Where(p => p.challenge_area == "community").FirstOrDefault()?.total ?? 0),
                        milestone2ParticipateOutdoors = milestone2Skipped ? -1 : (milestone2Awarded ? 5 : milestone2?.participates.Where(p => p.challenge_area == "outdoors").FirstOrDefault()?.total ?? 0),
                        milestone2ParticipateCreative = milestone2Skipped ? -1 : (milestone2Awarded ? 5 : milestone2?.participates.Where(p => p.challenge_area == "creative").FirstOrDefault()?.total ?? 0),
                        milestone2ParticipatePersonalGrowth = milestone2Skipped ? -1 : (milestone2Awarded ? 5 : milestone2?.participates.Where(p => p.challenge_area == "personal_growth").FirstOrDefault()?.total ?? 0),
                        milestone2Assist = milestone2Skipped ? -1 : (milestone2Awarded ? 3 : milestone2?.total_assists ?? 0),
                        milestone2Lead = milestone2Skipped ? -1 : (milestone2Awarded ? 2 : milestone2?.total_leads ?? 0),
                        milestone3ParticipateCommunity = milestone3Awarded ? 4 : milestone3?.participates.Where(p => p.challenge_area == "community").FirstOrDefault()?.total ?? 0,
                        milestone3ParticipateOutdoors = milestone3Awarded ? 4 : milestone3?.participates.Where(p => p.challenge_area == "outdoors").FirstOrDefault()?.total ?? 0,
                        milestone3ParticipateCreative = milestone3Awarded ? 4 : milestone3?.participates.Where(p => p.challenge_area == "creative").FirstOrDefault()?.total ?? 0,
                        milestone3ParticipatePersonalGrowth = milestone3Awarded ? 4 : milestone3?.participates.Where(p => p.challenge_area == "personal_growth").FirstOrDefault()?.total ?? 0,
                        milestone3Assist = milestone3Awarded ? 4 : milestone3?.total_assists ?? 0,
                        milestone3Lead = milestone3Awarded ? 4 : milestone3?.total_leads ?? 0
                    });
            }
            return unitMilestoneSummary;
        }

        private int CalculateMilestonePercentComplete(Milestone milestone)
        {
            int participateTotal = 0;
            int target = 0;
            foreach (var participate in milestone.participates)
            {
                participateTotal += milestone.milestone == 3
                                    ? Math.Min(4, participate.total)
                                    : participate.total;
            }
            participateTotal += milestone.milestone == 3
                                ? Math.Min(4, milestone.total_assists) + Math.Min(4, milestone.total_leads)
                                : milestone.total_assists + milestone.total_leads;

            switch (milestone.milestone)
            {
                case 1:
                    target = 27;
                    break;
                case 2:
                    target = 25;
                    break;
                case 3:
                    target = 24;
                    break;
            }

            var percentComplete = participateTotal * 100 / target;
            return percentComplete;
        }
    }
}
