namespace Topo.Models.Logbook
{
    public class GetMemberLogbookMetricsResultModel
    {
        public MetricsResult[] results { get; set; } = new MetricsResult[0];
    }

    public class MetricsResult
    {
        public string name { get; set; } = string.Empty;
        public float value { get; set; }
        public DateTime last_updated { get; set; }
    }

}
