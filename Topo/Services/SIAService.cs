using System.Globalization;
using Topo.Models.SIA;

namespace Topo.Services
{
    public interface ISIAService
    {
        public Task<List<SIAProjectListModel>> GenerateSIAReportData(List<KeyValuePair<string, string>> selectedMembers, string section, string selectedUnitId);
    }

    public class SIAService : ISIAService
    {
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly ILogger<ISIAService> _logger;
        private readonly IApprovalsService _approvalsService;

        public SIAService(ITerrainAPIService terrainAPIService,
            ILogger<ISIAService> logger,
            IApprovalsService approvalsService)
        {
            _terrainAPIService = terrainAPIService;
            _logger = logger;
            _approvalsService = approvalsService;
        }
        public async Task<List<SIAProjectListModel>> GenerateSIAReportData(List<KeyValuePair<string, string>> selectedMembers, string section, string selectedUnitId)
        {
            var approvals = await _approvalsService.GetApprovalListItems(selectedUnitId);
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
                            statusUpdated = r.status_updated,
                            achievement_id = r.id
                        })
                        .ToList();
                    foreach (var memberProject in memberSiaProjects)
                    {
                        var approval = approvals.Where(a => a.achievement_id == memberProject.achievement_id && a.submission_type.ToLower() == "review").FirstOrDefault();
                        if (approval != null)
                        {
                            if (approval.presented_date.HasValue)
                            {
                                memberProject.status = "Presented";
                                memberProject.statusUpdated = approval.presented_date.Value.ToLocalTime();
                            }
                        }
                    }
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

            return unitSiaProjects;
        }
    }
}
