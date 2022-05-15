﻿using FastReport;
using Topo.Models.Wallchart;

namespace Topo.Services
{
    public interface IWallchartService
    {
        Task<Report> GenerateWallchartReport(string selectedUnitId);
        Task<List<WallchartItemModel>> GetWallchartItems(string selectedUnitId);
    }
    public class WallchartService : IWallchartService
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly ILogger<IWallchartService> _logger;

        public WallchartService(StorageService storageService, IMemberListService memberListService, ITerrainAPIService terrainAPIService, ILogger<IWallchartService> logger)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _terrainAPIService = terrainAPIService;
            _logger = logger;
        }

        public async Task<Report> GenerateWallchartReport(string selectedUnitId)
        {
            var wallchartItems = await GetWallchartItems(selectedUnitId);
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var report = new Report();
            var directory = Directory.GetCurrentDirectory();
            report.Load(@$"{directory}/Reports/Wallchart.frx");
            report.SetParameterValue("GroupName", groupName);
            report.SetParameterValue("UnitName", unitName);
            report.SetParameterValue("ReportDate", DateTime.Now.ToShortDateString());
            report.RegisterData(wallchartItems, "Wallchart");

            return report;
        }

        public async Task<List<WallchartItemModel>> GetWallchartItems(string selectedUnitId)
        {
            var wallchartItems = new List<WallchartItemModel>();
            var getGroupLifeResultModel = await _terrainAPIService.GetGroupLifeForUnit(selectedUnitId);
            foreach (var result in getGroupLifeResultModel.results)
            {
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
                    }
                }
                foreach (var oas in result.oas.highest)
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
                            wallchartItem.OASAquaticsStage = oas.stage;
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
                foreach (var sia in result.sia.completed_areas)
                {
                    switch (sia)
                    {
                        case "sia_adventure_sport":
                            wallchartItem.SIAAdventureSport = "Y";
                            break;
                        case "sia_art_literature":
                            wallchartItem.SIAArtsLiterature = "Y";
                            break;
                        case "sia_environment":
                            wallchartItem.SIAEnvironment = "Y";
                            break;
                        case "sia_stem_innovation":
                            wallchartItem.SIAStemInnovation = "Y";
                            break;
                        case "sia_growth_development":
                            wallchartItem.SIAGrowthDevelopment = "Y";
                            break;
                        case "sia_better_world":
                            wallchartItem.SIACreatingABetterWorld = "Y";
                            break;
                    }
                }
                wallchartItem.LeadershipCourse = result.personal_development;
                wallchartItem.AdventurousJourney = result.adventurous_journey;
                wallchartItem.PersonalReflection = result.personal_reflection;
                wallchartItem.PeakAward = Math.Round(result.peak_award.total / 100d, 2);
                wallchartItems.Add(wallchartItem);
            }

            return wallchartItems;
        }
    }
}