namespace Topo.Models.SIA
{
    public class SIAProjectListModel
    {
        public string memberName { get; set; } = string.Empty;
        public string area { get; set; } = string.Empty;
        public string projectName { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public DateTime statusUpdated { get; set; }
    }
}
