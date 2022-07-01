using System.ComponentModel.DataAnnotations;

namespace Topo.Models.OAS
{
    public class OASWorksheetAnswers
    {
        public string TemplateTitle { get; set; } = string.Empty;
        public string InputId { get; set; } = string.Empty;
        public string InputTitle { get; set; } = string.Empty;
        public int InputTitleSortIndex { get; set; }
        public string InputLabel { get; set; } = string.Empty;
        public int InputSortIndex { get; set; }
        public string MemberId { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public DateTime? MemberAnswer { get; set; }
        public string MemberPatrol { get; set; } = string.Empty;
    }
}
