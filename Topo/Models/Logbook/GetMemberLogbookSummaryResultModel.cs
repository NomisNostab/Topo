namespace Topo.Models.Logbook
{
    public class GetMemberLogbookSummaryResultModel
    {
        public Result[] results { get; set; } = new Result[0];
    }

    public class Result
    {
        public string id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string start_date { get; set; } = string.Empty; 
        public Achievement_Meta achievement_meta { get; set; } = new Achievement_Meta();
    }

    public class Achievement_Meta
    {
        public string stream { get; set; } = string.Empty; 
        public string branch { get; set; } = string.Empty; 
    }

}
