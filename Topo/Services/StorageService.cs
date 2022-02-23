using Microsoft.AspNetCore.Mvc.Rendering;
using Topo.Models.Login;
using Topo.Models.Events;

namespace Topo.Services
{
    public class StorageService
    {
        public bool IsAuthenticated { get; set; }
        public string? Email { get; set; }
        public AuthenticationResult? AuthenticationResult { get; set; }
        public GetUserResultModel? GetUserResult { get; set; } = null;
        public GetProfilesResultModel? GetProfilesResult { get; set; }
        public List<SelectListItem>? Units { get; set; }
        public string? SelectedUnitName { get; set; }
        public string? SelectedUnitId { get; set; }
        public List<SelectListItem>? Events { get; set; }
        public List<SelectListItem>? Calendars { get; set; }
        public GetCalendarsResultModel? GetCalendarsResult { get; set;} = null;


    }
}
