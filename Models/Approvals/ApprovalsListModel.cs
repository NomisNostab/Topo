namespace Topo.Models.Approvals
{
    public class ApprovalsListModel
    {
        public string member_id { get; set; } = string.Empty;
        public string member_first_name { get; set; } = string.Empty;
        public string member_last_name { get; set; } = string.Empty;
        public string member_display_name => $"{member_first_name} {member_last_name}";
        public string achievement_id { get; set; } = string.Empty;
        public string achievement_type { get; set; } = string.Empty;
        public string achievement_name { get; set; } = string.Empty;
        public string submission_status { get; set; } = string.Empty;
        public string submission_outcome { get; set; } = string.Empty;
        public string submission_type { get; set; } = string.Empty;
        public DateTime submission_date { get; set; }
        public DateTime? awarded_date { get; set; }
        public DateTime? presented_date { get; set; }
    }
}
