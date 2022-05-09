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

        [Display(Name = "Search From Date")]
        [DataType(DataType.Date, ErrorMessage = "Date only")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CalendarSearchFromDate { get; set; } = DateTime.Now;

        [Display(Name = "Search To Date")]
        [DataType(DataType.Date, ErrorMessage = "Date only")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CalendarSearchToDate { get; set; } = DateTime.Now;
    }
}
