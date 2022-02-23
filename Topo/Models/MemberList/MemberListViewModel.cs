using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Topo.Models.MemberList
{
    public class MemberListViewModel
    {
        [Display(Name = "Unit")]
        public string SelectedUnitId { get; set; } = string.Empty;
        public string SelectedUnitName { get; set; } = string.Empty;
        public IEnumerable<SelectListItem>? Units { get; set; }
        public List<MemberListModel>? Members { get; set; }

    }
}
