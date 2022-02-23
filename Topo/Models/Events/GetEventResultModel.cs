using System.ComponentModel.DataAnnotations;

namespace Topo.Models.Events
{
    public class GetEventResultModel
    {
        public string id { get; set; }
        public string status { get; set; }
        public string title { get; set; }
        public string location { get; set; }
        public Organiser organiser { get; set; }
        public Organiser1[] organisers { get; set; }
        public string challenge_area { get; set; }
        public DateTime start_datetime { get; set; }
        public DateTime end_datetime { get; set; }
        public Attendance attendance { get; set; }
        public Invitee[] invitees { get; set; }
        public Review review { get; set; }
        public string owner_type { get; set; }
        public string owner_id { get; set; }
        public Achievement_Pathway_Oas_Data achievement_pathway_oas_data { get; set; }
        public Achievement_Pathway_Logbook_Data achievement_pathway_logbook_data { get; set; }
    }

    public class Organiser
    {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string member_number { get; set; }
        public string patrol_name { get; set; }
    }

    public class Attendance
    {
        public Leader_Members[] leader_members { get; set; }
        public Assistant_Members[] assistant_members { get; set; }
        public Participant_Members[] participant_members { get; set; }
        public Attendee_Members[] attendee_members { get; set; }
    }

    public class Leader_Members
    {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string member_number { get; set; }
        public string patrol_name { get; set; }
    }

    public class Assistant_Members
    {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string member_number { get; set; }
        public string patrol_name { get; set; }
    }

    public class Participant_Members
    {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string member_number { get; set; }
        public string patrol_name { get; set; }
    }

    public class Attendee_Members
    {

        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string member_number { get; set; }
        public string patrol_name { get; set; }
    }

    public class Review
    {
        public string general_rating { get; set; }
        public string[] general_tags { get; set; }
        public string[] scout_method_elements { get; set; }
        public string[] scout_spices_elements { get; set; }
    }

    public class Achievement_Pathway_Oas_Data
    {
        public string award_rule { get; set; }
        public Verifier verifier { get; set; }
        public object[] groups { get; set; }
    }

    public class Verifier
    {
        public string name { get; set; }
        public string contact { get; set; }
        public string type { get; set; }
    }

    public class Achievement_Pathway_Logbook_Data
    {
        public float distance_travelled { get; set; }
        public float distance_walkabout { get; set; }
        public Achievement_Meta achievement_meta { get; set; }
        public object[] categories { get; set; }
        public Details details { get; set; }
        public string title { get; set; }
    }

    public class Achievement_Meta
    {
        public string stream { get; set; }
        public string branch { get; set; }
    }

    public class Details
    {
        public string activity_time_length { get; set; }
        public string activity_grade { get; set; }
    }

    public class Organiser1
    {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string member_number { get; set; }
        public string patrol_name { get; set; }
    }

    public class Invitee
    {
        public string invitee_id { get; set; }
        public string invitee_type { get; set; }
        public string invitee_name { get; set; }
        public string id { get; set; }
    }


}
