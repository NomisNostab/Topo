using System.ComponentModel.DataAnnotations;

namespace Topo.Models.Login
{
    public class LoginViewModel
    {
        [Display(Name = "Member Number")]
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
