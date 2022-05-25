namespace Topo.Models.AditionalAwards
{
    public class AdditionalAwardListModel
    {
        public string MemberName { get; set; } = string.Empty;
        public string AwardId { get; set; } = string.Empty;
        public string AwardName { get; set; } = string.Empty;
        public int AwardSortIndex { get; set; } = 0;
        public DateTime? AwardDate { get; set; }
    }
}
