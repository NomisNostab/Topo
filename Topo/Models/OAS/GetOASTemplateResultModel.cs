namespace Topo.Models.OAS
{
    public class GetOASTemplateResultModel
    {
        public string template { get; set; }
        public Meta meta { get; set; }
        public int version { get; set; }
        public Document[] document { get; set; }
    }

    public class Meta
    {
        public int stage { get; set; }
        public string stream { get; set; }
    }

    public class Document
    {
        public string title { get; set; }
        public Input_Groups[] input_groups { get; set; }
    }

    public class Input_Groups
    {
        public string title { get; set; }
        public Input[] inputs { get; set; }
    }

    public class Input
    {
        public string id { get; set; }
        public string type { get; set; }
        public string label { get; set; }
        public string dialog_text { get; set; }
        public string alt { get; set; }
    }
}
