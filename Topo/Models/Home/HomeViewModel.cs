using Microsoft.AspNetCore.Mvc.Rendering;

namespace Topo.Models.Home
{
    public class HomeViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
