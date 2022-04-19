using FastReport;
using Topo.Models.Milestone;

namespace Topo.Services
{
    public interface IMilestoneService
    {
        Task<Report> GenerateMilestoneReport(string selectedUnitId);
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

        public async Task<Report> GenerateMilestoneReport(string selectedUnitId)
        {
            var unitMilestoneSummary = new List<MilestoneSummaryListModel>();
            var getGroupLifeResultModel = _terrainAPIService.GetGroupLifeForUnit(selectedUnitId);
            foreach (var result in getGroupLifeResultModel.Result.results)
            {
                var milestone1 = result.milestones.Where(m => m.milestone == 1).FirstOrDefault();
                var milestone2 = result.milestones.Where(m => m.milestone == 2).FirstOrDefault();
                var milestone3 = result.milestones.Where(m => m.milestone == 3).FirstOrDefault();
                unitMilestoneSummary.Add(
                    new MilestoneSummaryListModel
                    {
                        memberName = result.name,
                        currentLevel = $"Milestone {result.milestone.milestone}",
                        percentComplete = CalculateMilestonePercentComplete(result.milestone),
                        milestone1ParticipateCommunity = milestone1?.awarded ?? false ? 6 : milestone1?.participates.Where(p => p.challenge_area == "community").FirstOrDefault()?.total ?? 0,
                        milestone1ParticipateOutdoors = milestone1?.awarded ?? false ? 6 : milestone1?.participates.Where(p => p.challenge_area == "outdoors").FirstOrDefault()?.total ?? 0,
                        milestone1ParticipateCreative = milestone1?.awarded ?? false ? 6 : milestone1?.participates.Where(p => p.challenge_area == "creative").FirstOrDefault()?.total ?? 0,
                        milestone1ParticipatePersonalGrowth = milestone1?.awarded ?? false ? 6 : milestone1?.participates.Where(p => p.challenge_area == "personal_growth").FirstOrDefault()?.total ?? 0,
                        milestone1Assist = milestone1?.awarded ?? false ? 2 : milestone1?.total_assists ?? 0,
                        milestone1Lead = milestone1?.awarded ?? false ? 1 : milestone1?.total_leads ?? 0,
                        milestone2ParticipateCommunity = milestone2?.awarded ?? false ? 5 : milestone2?.participates.Where(p => p.challenge_area == "community").FirstOrDefault()?.total ?? 0,
                        milestone2ParticipateOutdoors = milestone2?.awarded ?? false ? 5 : milestone2?.participates.Where(p => p.challenge_area == "outdoors").FirstOrDefault()?.total ?? 0,
                        milestone2ParticipateCreative = milestone2?.awarded ?? false ? 5 : milestone2?.participates.Where(p => p.challenge_area == "creative").FirstOrDefault()?.total ?? 0,
                        milestone2ParticipatePersonalGrowth = milestone2?.awarded ?? false ? 5 : milestone2?.participates.Where(p => p.challenge_area == "personal_growth").FirstOrDefault()?.total ?? 0,
                        milestone2Assist = milestone2?.awarded ?? false ? 3 : milestone2?.total_assists ?? 0,
                        milestone2Lead = milestone2?.awarded ?? false ? 2 : milestone2?.total_leads ?? 0,
                        milestone3ParticipateCommunity = milestone3?.awarded ?? false ? 4 : milestone3?.participates.Where(p => p.challenge_area == "community").FirstOrDefault()?.total ?? 0,
                        milestone3ParticipateOutdoors = milestone3?.awarded ?? false ? 4 : milestone3?.participates.Where(p => p.challenge_area == "outdoors").FirstOrDefault()?.total ?? 0,
                        milestone3ParticipateCreative = milestone3?.awarded ?? false ? 4 : milestone3?.participates.Where(p => p.challenge_area == "creative").FirstOrDefault()?.total ?? 0,
                        milestone3ParticipatePersonalGrowth = milestone3?.awarded ?? false ? 4 : milestone3?.participates.Where(p => p.challenge_area == "personal_growth").FirstOrDefault()?.total ?? 0,
                        milestone3Assist = milestone3?.awarded ?? false ? 4 : milestone3?.total_assists ?? 0,
                        milestone3Lead = milestone3?.awarded ?? false ? 4 : milestone3?.total_leads ?? 0
                    }
                    );
            }

            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var report = new Report();
            var directory = Directory.GetCurrentDirectory();
            report.Load(@$"{directory}\Reports\MilestoneSummary.frx");
            report.SetParameterValue("GroupName", groupName);
            report.SetParameterValue("UnitName", unitName);
            report.SetParameterValue("ReportDate", DateTime.Now.ToShortDateString());
            report.RegisterData(unitMilestoneSummary, "MilestoneSummary");

            return report;

        }

        private int CalculateMilestonePercentComplete(Milestone milestone)
        {
            int participateTotal = 0;
            int target = 0;
            foreach (var participate in milestone.participates)
            {
                participateTotal += participate.total;
            }
            participateTotal += milestone.total_assists + milestone.total_leads;

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
