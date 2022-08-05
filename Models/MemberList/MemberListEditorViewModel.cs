using System.ComponentModel.DataAnnotations;

namespace Topo.Models.MemberList
{
    public class MemberListEditorViewModel
    {
        public string id { get; set; }
        [Display(Name = "Member Number")]
        public string member_number { get; set; }

        [Display(Name = "First Name")] 
        public string first_name { get; set; }
        [Display(Name = "Family Name")]
        public string last_name { get; set; }
        [Display(Name = "Unit Council")]
        public bool unit_council { get; set; }
        [Display(Name = "Patrol")]
        public string patrol_name { get; set; }
        [Display(Name = "Role")]
        public string patrol_duty { get; set; }
        [Display(Name = "Select")]
        public bool selected { get; set; }
    }
}
