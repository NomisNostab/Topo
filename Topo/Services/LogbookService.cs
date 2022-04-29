using FastReport;
using System.Globalization;
using Topo.Models.Logbook;

namespace Topo.Services
{
    public interface ILogbookService
    {
        public Task<Report> GenerateLogbookReport(List<KeyValuePair<string, string>> selectedMembers);
    }
    public class LogbookService : ILogbookService
    {
        private readonly StorageService _storageService;
        private readonly ITerrainAPIService _terrainAPIService;

        public LogbookService(StorageService storageService, ITerrainAPIService terrainAPIService)
        {
            _storageService = storageService;
            _terrainAPIService = terrainAPIService;
        }

        public async Task<Report> GenerateLogbookReport(List<KeyValuePair<string, string>> selectedMembers)
        {
            var memberLogbooks = new List<MemberLogbookReportViewModel>();
            foreach (var memberKVP in selectedMembers)
            {
                await _terrainAPIService.RevokeAssumedProfiles();
                await _terrainAPIService.AssumeProfile(memberKVP.Key);
                var getMemberLogbookMetrics = await _terrainAPIService.GetMemberLogbookMetrics(memberKVP.Key);
                var totalNightsCamped = getMemberLogbookMetrics.results.Where(r => r.name == "total_nights_camped").FirstOrDefault()?.value ?? 0;
                var totalKmsHiked = (getMemberLogbookMetrics.results.Where(r => r.name == "total_distance_hiked").FirstOrDefault()?.value ?? 0) / 1000.0f;
                var getMemberLogbookSummary = await _terrainAPIService.GetMemberLogbookSummary(memberKVP.Key);
                if (getMemberLogbookSummary != null)
                {
                    foreach (var summaryResult in getMemberLogbookSummary.results)
                    {
                        var getLogbookDetail = await _terrainAPIService.GetMemberLogbookDetail(memberKVP.Key, summaryResult.id);
                        if (getLogbookDetail != null)
                        {
                            var activityDate = DateTime.ParseExact(getLogbookDetail.start_date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var endDate = DateTime.ParseExact(getLogbookDetail.end_date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var nightsCamped = getLogbookDetail.categories.Any(c => c == "camping") ?
                                (endDate - activityDate).TotalDays :
                                0;
                            var kmsHiked = getLogbookDetail.categories.Any(c => c == "walking_hike") ?
                                getLogbookDetail.distance_walkabout / 1000.0f :
                                0;
                            memberLogbooks.Add(new MemberLogbookReportViewModel
                            {
                                MemberName = memberKVP.Value,
                                TotalKilometersHiked = totalKmsHiked,
                                TotalNightsCamped = totalNightsCamped,
                                ActivityName = getLogbookDetail.title,
                                ActivityArea = getLogbookDetail.achievement_meta.branch,
                                ActivityDate = activityDate,
                                ActivityLead = getLogbookDetail.details.who_lead,
                                MemberRole = getLogbookDetail.details.your_role,
                                KilometersHiked = kmsHiked,
                                NightsCamped = (float)nightsCamped,
                                Verifier = getLogbookDetail.details.verifier.name
                            });
                        }
                    }
                }
            }
            await _terrainAPIService.RevokeAssumedProfiles();

            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var report = new Report();
            var directory = Directory.GetCurrentDirectory();
            report.Load(@$"{directory}/Reports/Logbook.frx");
            report.SetParameterValue("GroupName", groupName);
            report.SetParameterValue("UnitName", unitName);
            report.SetParameterValue("ReportDate", DateTime.Now.ToShortDateString());
            report.RegisterData(memberLogbooks, "MemberLogbooks");

            return report;

        }
    }
}


////Build report template for model
//report.Dictionary.RegisterBusinessObject(
//        new List<MemberLogbookReportViewModel>(),
//        "MemberLogbooks",
//        2,
//        true
//    );
//report.Save(@$"{directory}/Reports/Logbook.frx");
