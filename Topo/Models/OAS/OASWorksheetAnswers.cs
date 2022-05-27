using System.ComponentModel.DataAnnotations;

namespace Topo.Models.OAS
{
    public class OASWorksheetAnswers
    {
        [Key]
        public int Id { get; set; }
        public string InputId { get; set; }
        public string InputTitle { get; set; }
        public int InputTitleSortIndex { get; set; }
        public string InputLabel { get; set; }
        public int InputSortIndex { get; set; }
        public string MemberId { get; set; }
        public string MemberName { get; set; }
        public DateTime? MemberAnswer { get; set; }
    }
}
