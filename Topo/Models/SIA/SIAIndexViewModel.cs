using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Topo.Models.SIA
{
    public class SIAIndexViewModel
    {
        public IEnumerable<SelectListItem>? Units { get; set; }
        [Display(Name = "Unit")]
        public string SelectedUnitId { get; set; } = string.Empty;
        public string SelectedUnitName { get; set; } = string.Empty;
    }
}
