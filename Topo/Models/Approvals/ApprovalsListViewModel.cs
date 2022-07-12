using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Topo.Models.Approvals
{
    public class ApprovalsListViewModel
    {
        [Display(Name = "Unit")]
        public string SelectedUnitId { get; set; } = string.Empty;
        public string SelectedUnitName { get; set; } = string.Empty;
        public IEnumerable<SelectListItem>? Units { get; set; }
        public List<ApprovalsListModel>? Approvals { get; set; }

        [Display(Name = "Search From Date")]
        [DataType(DataType.Date, ErrorMessage = "Date only")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ApprovalSearchFromDate { get; set; } = DateTime.Now;

        [Display(Name = "Search To Date")]
        [DataType(DataType.Date, ErrorMessage = "Date only")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ApprovalSearchToDate { get; set; } = DateTime.Now;
    }
}
