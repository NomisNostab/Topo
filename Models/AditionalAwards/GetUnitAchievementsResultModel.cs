namespace Topo.Models.AditionalAwards
{
    public class GetUnitAchievementsResultModel
    {
        public Result[] results { get; set; } = new Result[0];
    }

    public class Result
    {
        public string id { get; set; } = string.Empty;
        public string member_id { get; set; } = string.Empty;
        public string section { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public DateTime status_updated { get; set; }
        public Imported imported { get; set; } = new Imported();
        public Achievement_Meta achievement_meta { get; set; } = new Achievement_Meta();
    }

    public class Imported
    {
        public string date_awarded { get; set; } = string.Empty;
    }

    public class Achievement_Meta
    {
        public string additional_award_id { get; set; } = string.Empty;
    }

}
