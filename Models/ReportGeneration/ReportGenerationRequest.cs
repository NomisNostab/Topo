using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Topo.Models.ReportGeneration
{
    public enum ReportType
    {
        MemberList,
        PatrolList,
        PatrolSheets,
        SignInSheet,
        EventAttendance,
        Attendance,
        OASWorksheet,
        SIA,
        Milestone,
        Logbook,
        Wallchart,
        AdditionalAwards,
        Aprovals
    }

    public enum OutputType
    {
        PDF,
        Excel
    }

    public class ReportGenerationRequest
    {
        public ReportType ReportType { get; set; } = ReportType.MemberList;
        public string GroupName { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string ReportData { get; set; } = string.Empty;
        public OutputType OutputType { get; set; } = OutputType.PDF;
        public bool IncludeLeaders { get; set; } = false;
        public string EventName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool BreakByPatrol { get; set; } = false ;
        public bool GroupByMember { get; set; } = false;
        public bool FormatLikeTerrain { get; set; } = false ;
    }
}
// sortedMemberList, groupName, section, unitName