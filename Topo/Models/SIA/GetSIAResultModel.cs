namespace Topo.Models.SIA
{
    public class GetSIAResultModel
    {
        public string id { get; set; }
        public string member_id { get; set; }
        public string template { get; set; }
        public int version { get; set; }
        public string section { get; set; }
        public string type { get; set; }
        public Answers answers { get; set; }
        public string status { get; set; }
        public DateTime status_updated { get; set; }
    }

}
