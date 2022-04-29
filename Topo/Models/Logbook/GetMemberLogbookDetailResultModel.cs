namespace Topo.Models.Logbook
{
    public class GetMemberLogbookDetailResultModel
    {
        public string id { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string start_date { get; set; } = string.Empty;
        public Achievement_Meta achievement_meta { get; set; } = new Achievement_Meta();
        public string end_date { get; set; } = string.Empty;
        public float distance_travelled { get; set; }
        public float distance_walkabout { get; set; }
        public string[] categories { get; set; } = new string[0];
        public Details details { get; set; } = new Details();
    }

    public class Details
    {
        public string other_participants { get; set; } = string.Empty;
        public string activity_time_length { get; set; } = string.Empty;
        public string activity_grade { get; set; } = string.Empty;
        public string event_id { get; set; } = string.Empty;
        public string purpose { get; set; } = string.Empty;
        public string who_lead { get; set; } = string.Empty;
        public Verifier verifier { get; set; } = new Verifier();
        public string weather { get; set; } = string.Empty;
        public string location { get; set; } = string.Empty;
        public string event_title { get; set; } = string.Empty;
        public string your_role { get; set; } = string.Empty;
    }

    public class Verifier
    {
        public string name { get; set; } = string.Empty;
        public string contact { get; set; } = string.Empty;
    }

}
