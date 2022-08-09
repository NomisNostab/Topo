using Topo.Models.Approvals;
using Topo.Models.Wallchart;

namespace Topo.Services
{
    public interface IWallchartService
    {
        Task<List<WallchartItemModel>> GetWallchartItems(string selectedUnitId);
    }
    public class WallchartService : IWallchartService
    {
        private readonly StorageService _storageService;
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly ILogger<IWallchartService> _logger;
        private readonly IApprovalsService _approvalService;
        private List<ApprovalsListModel> approvals = new List<ApprovalsListModel>();

        public WallchartService(StorageService storageService, ITerrainAPIService terrainAPIService, ILogger<IWallchartService> logger, IApprovalsService approvalService)
        {
            _storageService = storageService;
            _terrainAPIService = terrainAPIService;
            _logger = logger;
            _approvalService = approvalService;
        }

        public async Task<List<WallchartItemModel>> GetWallchartItems(string selectedUnitId)
        {
            var cachedWallchartItems = _storageService.CachedWallchartItems.Where(cm => cm.Key == selectedUnitId).FirstOrDefault().Value;
            if (cachedWallchartItems != null)
                return cachedWallchartItems;

            var section = _storageService.SelectedSection;
            approvals = _approvalService.ReadApprovalListFromFileSystem(selectedUnitId);
            await _terrainAPIService.RevokeAssumedProfiles();

            var wallchartItems = new List<WallchartItemModel>();
            var getGroupLifeResultModel = await _terrainAPIService.GetGroupLifeForUnit(selectedUnitId);
            foreach (var result in getGroupLifeResultModel.results)
            {
                await _terrainAPIService.AssumeProfile(result.member_id);
                var getMemberLogbookMetrics = await _terrainAPIService.GetMemberLogbookMetrics(result.member_id);
                var totalNightsCamped = getMemberLogbookMetrics.results.Where(r => r.name == "total_nights_camped").FirstOrDefault()?.value ?? 0;
                var totalKmsHiked = (getMemberLogbookMetrics.results.Where(r => r.name == "total_distance_hiked").FirstOrDefault()?.value ?? 0) / 1000.0f;

                var wallchartItem = new WallchartItemModel();
                wallchartItem.MemberName = result.name;
                wallchartItem.IntroToScouting = result.intro_to_scouts;
                wallchartItem.IntroToSection = result.intro_to_section;
                var currentMilestone = result.milestone.milestone;
                foreach (var milestone in result.milestones.OrderBy(m => m.milestone))
                {
                    if (milestone.milestone == 1)
                    {
                        if (milestone.awarded)
                        {
                            wallchartItem.Milestone1Community = 6;
                            wallchartItem.Milestone1Creative = 6;
                            wallchartItem.Milestone1Outdoors = 6;
                            wallchartItem.Milestone1PersonalGrowth = 6;
                            wallchartItem.Milestone1Assist = 2;
                            wallchartItem.Milestone1Lead = 1;
                            wallchartItem.Milestone1Awarded = milestone.status_updated;
                            wallchartItem.Milestone1Presented = GetApprovalDate(result.member_id, "Milestone 1");
                        }
                        else
                        {
                            wallchartItem.Milestone1Community = milestone.participates.Where(p => p.challenge_area == "community").FirstOrDefault()?.total ?? 0;
                            wallchartItem.Milestone1Creative = milestone.participates.Where(p => p.challenge_area == "creative").FirstOrDefault()?.total ?? 0;
                            wallchartItem.Milestone1Outdoors = milestone.participates.Where(p => p.challenge_area == "outdoors").FirstOrDefault()?.total ?? 0;
                            wallchartItem.Milestone1PersonalGrowth = milestone.participates.Where(p => p.challenge_area == "personal_growth").FirstOrDefault()?.total ?? 0;
                            wallchartItem.Milestone1Assist = milestone.total_assists;
                            wallchartItem.Milestone1Lead = milestone.total_leads;
                        }
                    }
                    if (milestone.milestone == 2)
                    {
                        if (milestone.awarded)
                        {
                            wallchartItem.Milestone2Community = 5;
                            wallchartItem.Milestone2Creative = 5;
                            wallchartItem.Milestone2Outdoors = 5;
                            wallchartItem.Milestone2PersonalGrowth = 5;
                            wallchartItem.Milestone2Assist = 3;
                            wallchartItem.Milestone2Lead = 2;
                            wallchartItem.Milestone2Awarded = milestone.status_updated;
                            wallchartItem.Milestone2Presented = GetApprovalDate(result.member_id, "Milestone 2");
                        }
                        else
                        {
                            wallchartItem.Milestone2Community = milestone.participates.Where(p => p.challenge_area == "community").FirstOrDefault()?.total ?? 0;
                            wallchartItem.Milestone2Creative = milestone.participates.Where(p => p.challenge_area == "creative").FirstOrDefault()?.total ?? 0;
                            wallchartItem.Milestone2Outdoors = milestone.participates.Where(p => p.challenge_area == "outdoors").FirstOrDefault()?.total ?? 0;
                            wallchartItem.Milestone2PersonalGrowth = milestone.participates.Where(p => p.challenge_area == "personal_growth").FirstOrDefault()?.total ?? 0;
                            wallchartItem.Milestone2Assist = milestone.total_assists;
                            wallchartItem.Milestone2Lead = milestone.total_leads;
                        }
                    }
                    if (milestone.milestone == 3)
                    {
                        wallchartItem.Milestone3Community = milestone.participates.Where(p => p.challenge_area == "community").FirstOrDefault()?.total ?? 0;
                        wallchartItem.Milestone3Creative = milestone.participates.Where(p => p.challenge_area == "creative").FirstOrDefault()?.total ?? 0;
                        wallchartItem.Milestone3Outdoors = milestone.participates.Where(p => p.challenge_area == "outdoors").FirstOrDefault()?.total ?? 0;
                        wallchartItem.Milestone3PersonalGrowth = milestone.participates.Where(p => p.challenge_area == "personal_growth").FirstOrDefault()?.total ?? 0;
                        wallchartItem.Milestone3Assist = milestone.total_assists;
                        wallchartItem.Milestone3Lead = milestone.total_leads;
                        wallchartItem.Milestone3Awarded = milestone.awarded ? milestone.status_updated : null;
                        wallchartItem.Milestone3Presented = GetApprovalDate(result.member_id, "Milestone 3");
                    }
                }
                foreach (var oas in result.oas.highest.OrderBy(h => h.stream).ThenBy(h => h.stage))
                {
                    switch (oas.stream)
                    {
                        case "bushcraft":
                            wallchartItem.OASBushcraftStage = oas.stage;
                            break;
                        case "bushwalking":
                            wallchartItem.OASBushwalkingStage = oas.stage;
                            break;
                        case "camping":
                            wallchartItem.OASCampingStage = oas.stage;
                            break;
                        case "alpine":
                            wallchartItem.OASAlpineStage = oas.stage;
                            break;
                        case "cycling":
                            wallchartItem.OASCyclingStage = oas.stage;
                            break;
                        case "vertical":
                            wallchartItem.OASVerticalStage = oas.stage;
                            break;
                        case "aquatics":
                            wallchartItem.OASAquaticsStage = oas.stage;
                            break;
                        case "boating":
                            wallchartItem.OASBoatingStage = oas.stage;
                            break;
                        case "paddling":
                            wallchartItem.OASPaddlingStage = oas.stage;
                            break;
                    }
                }
                wallchartItem.NightsCamped = (int)totalNightsCamped;
                wallchartItem.KMsHiked = (int)totalKmsHiked;
                wallchartItem.OASStageProgressions = result.oas.total_progressions;
                var siaResultModel = await _terrainAPIService.GetSIAResultsForMember(result.member_id);
                var awardedSIAProjects = siaResultModel.results.Where(r => r.section == section && r.status == "awarded");
                var awardedSIAProjectsByArea = awardedSIAProjects.GroupBy(p => p.answers.special_interest_area_selection);
                foreach (var area in awardedSIAProjectsByArea)
                {
                    switch (area.Key)
                    {
                        case "sia_adventure_sport":
                            wallchartItem.SIAAdventureSport = area.Count();
                            break;
                        case "sia_art_literature":
                            wallchartItem.SIAArtsLiterature = area.Count();
                            break;
                        case "sia_environment":
                            wallchartItem.SIAEnvironment = area.Count();
                            break;
                        case "sia_stem_innovation":
                            wallchartItem.SIAStemInnovation = area.Count();
                            break;
                        case "sia_growth_development":
                            wallchartItem.SIAGrowthDevelopment = area.Count();
                            break;
                        case "sia_better_world":
                            wallchartItem.SIACreatingABetterWorld = area.Count();
                            break;
                    }
                }
                wallchartItem.LeadershipCourse = result.personal_development;
                wallchartItem.AdventurousJourney = result.adventurous_journey;
                wallchartItem.PersonalReflection = result.personal_reflection;
                wallchartItem.PeakAward = Math.Round(result.peak_award.total / 100d, 2);
                wallchartItems.Add(wallchartItem);

                await _terrainAPIService.RevokeAssumedProfiles();
            }
            _storageService.CachedWallchartItems.Add(new KeyValuePair<string, List<WallchartItemModel>>(selectedUnitId, wallchartItems));
            return wallchartItems;
        }

        private DateTime? GetApprovalDate(string member_id, string achievement_name)
        {
            var approval = approvals.Where(a => a.member_id == member_id && a.achievement_name == achievement_name).FirstOrDefault();
            return approval?.presented_date;
        }
    }
}
