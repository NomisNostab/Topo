namespace Topo.Models.OAS
{
    public class GetUnitAchievementsResultsModel
    {
        public Result[] results { get; set; }
    }


    public class Rootobject
    {
        public Result[] results { get; set; }
    }

    public class Result
    {
        public string id { get; set; }
        public string member_id { get; set; }
        public string template { get; set; }
        public int version { get; set; }
        public string section { get; set; }
        public string type { get; set; }
        public Dictionary<string, string> answers { get; set; }
        public string status { get; set; }
        public DateTime status_updated { get; set; }
        public Achievement_Meta achievement_meta { get; set; }
        public DateTime last_updated { get; set; }
        public Imported imported { get; set; }
    }

    public class Achievement_Meta
    {
        public string stream { get; set; }
        public string branch { get; set; }
        public int stage { get; set; }
    }

    public class Imported
    {
        public string date_awarded { get; set; }
        public Awarded_By awarded_by { get; set; }
    }

    public class Awarded_By
    {
        public string id { get; set; }
        public string name { get; set; }
    }

}
