namespace Topo.Models.OAS
{
    public class OASStageListModel
    {
        public string Stream { get; set; } = string.Empty;
        public string StageTitle { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public int Stage { get; set; }
        public string TemplateLink { get; set; } = string.Empty;
        public string SelectListItemText { get; set; } = string.Empty;
    }
}
