namespace Topo.Models.Milestone
{
    public class GetGroupLifeResultModel
    {
        public Result[] results { get; set; } = new Result[0];
        public int total { get; set; }
        public DateTime retrieved_date { get; set; }
    }

    public class Result
    {
        public string member_id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string age { get; set; } = string.Empty;
        public DateTime? intro_to_scouts { get; set; }
        public DateTime? intro_to_section { get; set; }
        public DateTime? personal_development { get; set; }
        public DateTime? adventurous_journey { get; set; }
        public DateTime? personal_reflection { get; set; }
        public Sia sia { get; set; } = new Sia();
        public Peak_Award peak_award { get; set; } = new Peak_Award();
        public int unit_attendance { get; set; }
        public int total_attendance { get; set; }
        public Milestone milestone { get; set; } = new Milestone();
        public Milestone[] milestones { get; set; } = new Milestone[0];
        public Oas oas { get; set; } = new Oas();
    }

    public class Sia
    {
        public int completed_projects { get; set; }
        public string[] completed_areas { get; set; } = new string[0];
        public int in_progress { get; set; }
    }

    public class Peak_Award
    {
        public float adventurous_journey { get; set; }
        public float personal_development { get; set; }
        public float personal_reflection { get; set; }
        public float milestones { get; set; }
        public float oas { get; set; }
        public float sia { get; set; }
        public float total { get; set; }
    }

    public class Milestone
    {
        public int milestone { get; set; }
        public bool awarded { get; set; }
        public Participate[] participates { get; set; } = new Participate[0];
        public DateTime? status_updated { get; set; }
        public int total_assists { get; set; }
        public int total_leads { get; set; }
    }

    public class Participate
    {
        public string challenge_area { get; set; } = string.Empty;
        public int total { get; set; }
    }

    public class Oas
    {
        public int total_progressions { get; set; }
        public Highest[] highest { get; set; } = new Highest[0];
    }

    public class Highest
    {
        public string stream { get; set; } = string.Empty;
        public int stage { get; set; }
        public string branch { get; set; } = string.Empty;
    }


}
