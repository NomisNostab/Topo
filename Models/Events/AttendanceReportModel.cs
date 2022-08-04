namespace Topo.Models.Events
{
    public class AttendanceReportModel
    {
        public List<AttendanceReportItemModel> attendanceReportItems { get; set; } = new List<AttendanceReportItemModel>();
        public List<AttendanceReportMemberSummaryModel> attendanceReportMemberSummaries { get; set; } = new List<AttendanceReportMemberSummaryModel>();
        public List<AttendanceReportChallengeAreaSummaryModel> attendanceReportChallengeAreaSummaries { get; set; } = new List<AttendanceReportChallengeAreaSummaryModel>();
    }

    public class AttendanceReportItemModel
    {
        public string MemberId { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string MemberNameAndRate { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string EventNameDisplay { get; set; } = string.Empty;
        public string EventChallengeArea { get; set; } = string.Empty;
        public DateTime EventStartDate { get; set; }
        public int Attended { get; set; } = 0;
        public int IsAdultMember { get; set; } = 0;
        public string EventStatus { get; set; } = string.Empty;
    }
    
    public class AttendanceReportMemberSummaryModel
    {
        public string MemberId { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public int IsAdultMember { get; set; } = 0;
        public int AttendanceCount { get; set; } = 0;
        public int TotalEvents { get; set; } = 0;
    }

    public class AttendanceReportChallengeAreaSummaryModel
    {
        public string ChallengeArea { get; set; } = string.Empty;
        public int EventCount { get; set; } = 0;
        public int TotalEvents { get; set; } = 0;
    }
}
