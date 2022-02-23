namespace Topo.Models.Events
{
    public class GetEventsResultModel
    {
        public Event[] results { get; set; }
    }

    public class Event
    {
        public string id { get; set; }
        public DateTime start_datetime { get; set; }
        public DateTime end_datetime { get; set; }
        public string title { get; set; }
        public string invitee_type { get; set; }
        public string status { get; set; }
        public string challenge_area { get; set; }
        public string section { get; set; }
        public string invitee_id { get; set; }
        public string invitee_name { get; set; }
        public string group_id { get; set; }
    }


}
