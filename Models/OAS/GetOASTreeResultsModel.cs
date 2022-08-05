namespace Topo.Models.OAS
{
    public class GetOASTreeResultsModel
    {
        public string title { get; set; }
        public string stream_id { get; set; }
        public string description { get; set; }
        public string achievement_type { get; set; }
        public Tree tree { get; set; }
    }

    public class Tree
    {
        public string branch_id { get; set; }
        public string title { get; set; }
        public int stage { get; set; }
        public string template_link { get; set; }
        public Child[] children { get; set; }
    }

    public class Child
    {
        public string branch_id { get; set; }
        public string title { get; set; }
        public int stage { get; set; }
        public string template_link { get; set; }
        public Child[] children { get; set; }
    }


}
