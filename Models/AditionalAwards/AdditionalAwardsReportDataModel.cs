namespace Topo.Models.AditionalAwards
{
    public class AdditionalAwardsReportDataModel
    {
        public List<AdditionalAwardSpecificationListModel> AwardSpecificationsList { get; set; } = new List<AdditionalAwardSpecificationListModel>();
        public List<AdditionalAwardListModel> SortedAdditionalAwardsList { get; set; } = new List<AdditionalAwardListModel>();
        public List<string> DistinctAwards { get; set; } = new List<string>();
    }
}
