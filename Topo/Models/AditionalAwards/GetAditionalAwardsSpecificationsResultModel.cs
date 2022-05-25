namespace Topo.Models.AditionalAwards
{
    public class GetAditionalAwardsSpecificationsResultModel
    {
        public AwardDescription[] AwardDescriptions { get; set; } = new AwardDescription[0];
    }

    public class AwardDescription
    {
        public string title { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
        public string[] sections { get; set; } = new string[0];
        public string description { get; set; } = string.Empty;
        public Expiry expiry { get; set; } = new Expiry();
    }

    public class Expiry
    {
        public string from_date_awarded { get; set; } = string.Empty;
        public string yearly_on_date { get; set; } = string.Empty;
    }

}
