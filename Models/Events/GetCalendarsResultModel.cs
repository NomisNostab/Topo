namespace Topo.Models.Events
{
    public class GetCalendarsResultModel
    {
        public string member_id { get; set; }
        public Own_Calendars[] own_calendars { get; set; }
        public Other_Calendars[] other_calendars { get; set; }
    }

    public class Own_Calendars
    {
        public string id { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public bool selected { get; set; }
        public string section { get; set; }
    }

    public class Other_Calendars
    {
        public string id { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public bool selected { get; set; }
        public string section { get; set; }
    }

}
