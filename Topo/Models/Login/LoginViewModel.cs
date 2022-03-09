using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Topo.Models.Login
{
    public class LoginViewModel
    {
        public IEnumerable<SelectListItem>? Branches { get; set; }
        [Display(Name = "Branch")]
        public string SelectedBranch { get; set; } = string.Empty;

        [Display(Name = "Member Number")]
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
