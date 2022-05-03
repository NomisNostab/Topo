using FastReport;
using System.Globalization;
using Topo.Models.SIA;

namespace Topo.Services
{
    public interface ISIAService
    {
        public Task<Report> GenerateSIAReport(List<KeyValuePair<string, string>> selectedMembers);
    }

    public class SIAService : ISIAService
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly ILogger<ISIAService> _logger;

        public SIAService(IMemberListService memberListService,
            ITerrainAPIService terrainAPIService,
            StorageService storageService, ILogger<ISIAService> logger)
        {
            _memberListService = memberListService;
            _terrainAPIService = terrainAPIService;
            _storageService = storageService;
            _logger = logger;
        }
        public async Task<Report> GenerateSIAReport(List<KeyValuePair<string, string>> selectedMembers)
        {
            var unitName = _storageService.SelectedUnitName ?? "";
            var unit = _storageService.GetProfilesResult.profiles.FirstOrDefault(u => u.unit.name == unitName);
            if (unit == null)
                throw new IndexOutOfRangeException($"No unit found with name {unitName}. You may not have permissions to this section");
            var section = unit.unit.section;
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            var unitSiaProjects = new List<SIAProjectListModel>();
            foreach (var member in selectedMembers)
            {
                try
                {
                    var siaResultModel = await _terrainAPIService.GetSIAResultsForMember(member.Key);
                    var memberSiaProjects = siaResultModel.results.Where(r => r.section == section)
                        .Select(r => new SIAProjectListModel
                        {
                            memberName = member.Value,
                            area = myTI.ToTitleCase(r.answers.special_interest_area_selection.Replace("sia_", "").Replace("_", " ")),
                            projectName = r.answers.project_name,
                            status = myTI.ToTitleCase(r.status.Replace("_", " ")),
                            statusUpdated = r.status_updated
                        })
                        .ToList();
                    if (memberSiaProjects != null && memberSiaProjects.Count > 0)
                        unitSiaProjects = unitSiaProjects.Concat(memberSiaProjects).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"GenerateSIAReport: Exception: {ex.Message}");
                    _logger.LogError($"GenerateSIAReport: Member: {member.Value} {member.Key}");
                    _logger.LogError(ex, "GenerateSIAReport:");
                    unitSiaProjects.Add(new SIAProjectListModel
                    { 
                        memberName = $"{member.Value}",
                        area = "Error",
                        projectName = "Error",
                        status = "Error",
                        statusUpdated = DateTime.Now
                    });
                }
            }

            var groupName = _storageService.GroupName;
            var report = new Report();
            var directory = Directory.GetCurrentDirectory();
            report.Load(@$"{directory}/Reports/SIAProjectReport.frx");
            report.SetParameterValue("GroupName", groupName);
            report.SetParameterValue("UnitName", unitName);
            report.SetParameterValue("ReportDate", DateTime.Now.ToShortDateString());
            report.RegisterData(unitSiaProjects, "SIAProjects");

            return report;
        }
    }
}
