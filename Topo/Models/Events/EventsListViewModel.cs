using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Topo.Models.Events
{
    public class EventsListViewModel
    {
        public IEnumerable<SelectListItem>? Calendars { get; set; }

        [Display(Name = "Unit Calendar")]
        public string SelectedCalendar { get; set; } = "";
        public IEnumerable<EventListModel>? Events { get; set; }
        public string SelectedEvent { get; set; } = "";

        [Display(Name = "Search Date")]
        [DataType(DataType.Date, ErrorMessage = "Date only")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CalendarSearchDate { get; set; } = DateTime.Now;
    }
}
