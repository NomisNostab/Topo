using System.ComponentModel.DataAnnotations;

namespace Topo.Models.Events
{
    public class EventListModel
    {
        public string Id { get; set; }
        [Display(Name = "Program Name")]
        public string EventName { get; set; } = "";

        [Display(Name = "Date")]
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string EventDisplay => $"{EventName} {StartDateTime.ToShortDateString()}";

        [Display(Name = "Date")]
        public string EventDate => StartDateTime.ToShortDateString();

        public Attendee_Members[] attendees;
    }
}
