namespace Topo.Models.SIA
{
    public class GetSIAResultsModel
    {
        public Result[] results { get; set; } = new Result[0];
    }

    public class Result
    {
        public string id { get; set; } = string.Empty;
        public string member_id { get; set; } = string.Empty;
        public string section { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public Answers answers { get; set; } = new Answers();
        public string status { get; set; } = string.Empty;
        public DateTime status_updated { get; set; }
        public Imported imported { get; set; } = new Imported();
        public string template { get; set; } = string.Empty;
        public int version { get; set; }
        public Upload[] uploads { get; set; } = new Upload[0];
        public bool can_archive { get; set; }
        public DateTime last_updated { get; set; }
    }

    public class Answers
    {
        public string project_name { get; set; } = string.Empty;
        public string special_interest_area_selection { get; set; } = string.Empty;
        public string need_support { get; set; } = string.Empty;
        public string how_know_complete { get; set; } = string.Empty;
        public string who_is_involved { get; set; } = string.Empty;
        public string achieve_goals_change { get; set; } = string.Empty;
        public string List_everyone_involved { get; set; } = string.Empty;
        public string who_you_need { get; set; } = string.Empty;
        public string steps { get; set; } = string.Empty;
        public string how_you_adapt_your_plans { get; set; } = string.Empty;
        public string Goal_vision { get; set; } = string.Empty;
        public string goals_sustainable { get; set; } = string.Empty;
        public string main_activity { get; set; } = string.Empty;
        public string risks { get; set; } = string.Empty;
        public string highlight_SIA { get; set; } = string.Empty;
        public string continue_extend_SIA { get; set; } = string.Empty;
        public string[] attach_documentation { get; set; } = new string[0];
        public string what_ways_have_you_developed_by_doing_project { get; set; } = string.Empty;
        public string success_of_SIA { get; set; } = string.Empty;
        public string how_develop { get; set; } = string.Empty;
        public string strengths_improvements { get; set; } = string.Empty;
    }

    public class Imported
    {
        public string date_awarded { get; set; } = string.Empty;
        public Awarded_By awarded_by { get; set; } = new Awarded_By();
    }

    public class Awarded_By
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
    }

    public class Upload
    {
        public string id { get; set; } = string.Empty;
        public string filename { get; set; } = string.Empty;
        public string bucket { get; set; } = string.Empty;
        public string key { get; set; } = string.Empty;
        public string url { get; set; } = string.Empty;
        public DateTime uploaded_on { get; set; }
    }

}
