namespace Topo.Models.Wallchart
{
    public class WallchartItemModel
    {
        public string MemberName { get; set; } = string.Empty;
        public DateTime? IntroToScouting { get; set; }
        public DateTime? IntroToSection { get; set; }
        public int Milestone1Community { get; set; }
        public int Milestone1Outdoors { get; set; }
        public int Milestone1Creative { get; set; }
        public int Milestone1PersonalGrowth { get; set; }
        public int Milestone1Assist { get; set; }
        public int Milestone1Lead { get; set; }
        public int Milestone2Community { get; set; }
        public int Milestone2Outdoors { get; set; }
        public int Milestone2Creative { get; set; }
        public int Milestone2PersonalGrowth { get; set; }
        public int Milestone2Assist { get; set; }
        public int Milestone2Lead { get; set; }
        public int Milestone3Community { get; set; }
        public int Milestone3Outdoors { get; set; }
        public int Milestone3Creative { get; set; }
        public int Milestone3PersonalGrowth { get; set; }
        public int Milestone3Assist { get; set; }
        public int Milestone3Lead { get; set; }
        public int OASBushcraftStage { get; set; }
        public int OASBushwalkingStage { get; set; }
        public int OASCampingStage { get; set; }
        public int OASAlpineStage { get; set; }
        public int OASCyclingStage { get; set; }
        public int OASVerticalStage { get; set; }
        public int OASAquaticsStage { get; set; }
        public int OASBoatingStage { get; set; }
        public int OASPaddlingStage { get; set; }
        public string SIAAdventureSport { get; set; } = string.Empty;
        public string SIAArtsLiterature { get; set; } = string.Empty;
        public string SIAEnvironment { get; set; } = string.Empty;
        public string SIAStemInnovation { get; set; } = string.Empty;
        public string SIAGrowthDevelopment { get; set; } = string.Empty;
        public string SIACreatingABetterWorld { get; set; } = string.Empty;
        public DateTime? LeadershipCourse { get; set; }
        public DateTime? AdventurousJourney { get; set; }
        public DateTime? PersonalReflection { get; set; }
        public double PeakAward { get; set; }
    }
}
