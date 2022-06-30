using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Topo.Models.OAS
{
    public class OASIndexViewModel
    {
        public IEnumerable<SelectListItem>? Units { get; set; }
        [Display(Name = "Unit")]
        public string SelectedUnitId { get; set; } = string.Empty;
        public string SelectedUnitName { get; set; } = string.Empty;
        public IEnumerable<SelectListItem>? Stages { get; set; }
        [Display(Name = "Stage")]
        public string[] SelectedStages { get; set; } = new string[0];

        [Display(Name = "Hide Completed Members")]
        public bool HideCompletedMembers { get; set; }

    }
}
