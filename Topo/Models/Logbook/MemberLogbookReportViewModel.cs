namespace Topo.Models.Logbook
{
    public class MemberLogbookReportViewModel
    {
        public string MemberName { get; set; } = string.Empty;
        public float TotalKilometersHiked { get; set; } = 0;
        public float TotalNightsCamped { get; set; } = 0;
        public string ActivityName { get; set; } = string.Empty;    
        public string ActivityArea { get; set; } = string.Empty;
        public DateTime ActivityDate { get; set; }
        public string ActivityLead { get; set; } = string.Empty;
        public string MemberRole { get; set; } = string.Empty;
        public float KilometersHiked { get; set; } = 0;
        public float NightsCamped { get; set; } = 0;
        public string Verifier { get; set; } = string.Empty;
    }
}
