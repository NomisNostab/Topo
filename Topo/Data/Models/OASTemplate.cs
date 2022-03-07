using System.ComponentModel.DataAnnotations;

namespace Topo.Data.Models
{
    public class OASTemplate
    {
        [Key]
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public string TemplateTitle { get; set; }
        public string InputGroup { get; set; }
        public int InputGroupSort { get; set; }
        public string InputId { get; set; }
        public string InputLabel { get; set; }
    }
}
