using Newtonsoft.Json;
using Topo.Models.Approvals;
using System.Linq;
using System.Globalization;

namespace Topo.Services
{
    public interface IApprovalsService
    {
        Task<List<ApprovalsListModel>> GetApprovalListItems(string unitId);
        void UpdateApproval(string unitId, ApprovalsListModel approval);
        List<ApprovalsListModel> ReadApprovalListFromFileSystem(string unitId);
    }
    public class ApprovalsService : IApprovalsService
    {
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly IMemberListService _memberService;

        public ApprovalsService(ITerrainAPIService terrainAPIService, IMemberListService memberService)
        { 
            _terrainAPIService = terrainAPIService;
            _memberService = memberService;
        }

        public List<ApprovalsListModel> ReadApprovalListFromFileSystem(string unitId)
        {
            var list = new List<ApprovalsListModel>();
            string path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Topo");
            using (StreamReader r = new StreamReader($@"{path}\{unitId}_ApprovalsList.json", new FileStreamOptions() { Mode = FileMode.OpenOrCreate }))
            {
                string json = r.ReadToEnd();
                list = JsonConvert.DeserializeObject<List<ApprovalsListModel>>(json);
            }

            return list ?? new List<ApprovalsListModel>();
        }

        private void WriteApprovalsListToFileSystem(List<ApprovalsListModel> approvalsList, string unitId)
        {
            //open file stream
            string path = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Topo");
            using (StreamWriter file = File.CreateText($@"{path}\{unitId}_ApprovalsList.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, approvalsList);
            }
        }

        private async Task<List<ApprovalsListModel>> GetApprovalList(string unitId, string status)
        {
            TextInfo myTI = new CultureInfo("en-AU", false).TextInfo;
            var members = await _memberService.GetMembersAsync(unitId);
            var approvalList = await _terrainAPIService.GetUnitApprovals(unitId, status);
            var approvals = new List<ApprovalsListModel>();
            foreach (var approval in approvalList.results)
            {
                var member = members.Where(m => m.id == approval.member.id).FirstOrDefault();
                if (member != null)
                {
                    approvals.Add(new ApprovalsListModel()
                    {
                        member_id = approval.member.id,
                        member_first_name = approval.member.first_name,
                        member_last_name = approval.member.last_name,
                        achievement_id = approval.achievement.id,
                        achievement_type = approval.achievement.type,
                        submission_type = myTI.ToTitleCase(approval.submission.type),
                        submission_status = myTI.ToTitleCase(approval.submission.status),
                        submission_outcome = myTI.ToTitleCase(approval.submission.outcome),
                        submission_date = approval.submission.date,
                        awarded_date = (approval.submission.status == "finalised" && approval.submission.outcome == "approved") ? approval.submission.date : null
                    });
                }
            }

            return approvals;
        }

        private async Task<List<ApprovalsListModel>> GetAdditionalAwardList(string unitId)
        {
            TextInfo myTI = new CultureInfo("en-AU", false).TextInfo;
            var members = await _memberService.GetMembersAsync(unitId);
            var unitAchievementsResult = await _terrainAPIService.GetUnitAdditionalAwardAchievements(unitId);
            var approvals = new List<ApprovalsListModel>();
            foreach (var additionalAward in unitAchievementsResult.results)
            {
                var member = members.Where(m => m.id == additionalAward.member_id).FirstOrDefault();
                if (member != null)
                {
                    var awardStatusDate = additionalAward.status_updated;
                    if (additionalAward.imported != null)
                        awardStatusDate = DateTime.ParseExact(additionalAward.imported.date_awarded, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    approvals.Add(new ApprovalsListModel()
                    {
                        member_id = additionalAward.member_id,
                        member_first_name = member?.first_name ?? "",
                        member_last_name = member?.last_name ?? "",
                        achievement_id = additionalAward.id,
                        achievement_type = additionalAward.achievement_meta.additional_award_id,
                        submission_type = "Review",
                        submission_status = "Finalised",
                        submission_outcome = myTI.ToTitleCase(additionalAward.status),
                        submission_date = awardStatusDate,
                        awarded_date = awardStatusDate
                    });
                }
            }

            return approvals;
        }



        private async Task<string> GetAchievementName(string member_id, string achievement_id, string achievement_type)
        {
            var name = "";
            switch (achievement_type)
            {
                case "intro_scouting":
                    name = "intro to scouting";
                    break;
                case "intro_section":
                    name = "intro to section";
                    break;
                case "milestone":
                    var memberMilestoneAchievementResult = await _terrainAPIService.GetMemberAchievementResult(member_id, achievement_id, achievement_type);
                    name = memberMilestoneAchievementResult != null ? $"milestone {memberMilestoneAchievementResult.achievement_meta.stage}" : "milestone not found";
                    break;
                case "outdoor_adventure_skill":
                    var memberAchievementResult = await _terrainAPIService.GetMemberAchievementResult(member_id, achievement_id, achievement_type);
                    if (memberAchievementResult != null && !string.IsNullOrEmpty(memberAchievementResult.template))
                    {
                        var templateParts = memberAchievementResult.template.Split("/");
                        name = string.Join(" ", templateParts);
                    }
                    else
                    {
                        name = "outdoor adventure skill not found";
                    }
                    break;
                case "special_interest_area":
                    var siaResult = await _terrainAPIService.GetSIAResultForMember(member_id, achievement_id);
                    if (siaResult != null && siaResult.answers != null && !string.IsNullOrEmpty(siaResult.answers.special_interest_area_selection) && !string.IsNullOrEmpty(siaResult.answers.project_name))
                        name = $"{siaResult.answers.special_interest_area_selection.Replace("_", " ")} {siaResult.answers.project_name}";
                    else
                        name = "SIA Not Found";
                    break;
                default:
                    name = achievement_type.Replace("_", " ");
                    break;
            }
            TextInfo myTI = new CultureInfo("en-AU", false).TextInfo;
            return myTI.ToTitleCase(name);
        }

        public async Task<List<ApprovalsListModel>> GetApprovalListItems(string unitId)
        {
            var savedApprovalItems = ReadApprovalListFromFileSystem(unitId);
            var initialLoad = savedApprovalItems.Count == 0;
            var pendingApprovals = await GetApprovalList(unitId, "pending");
            var finalisedApprovals = await GetApprovalList(unitId, "finalised");
            var additionalAwards = await GetAdditionalAwardList(unitId);
            var allTerrainApprovals = finalisedApprovals.Concat(pendingApprovals).Concat(additionalAwards).OrderBy(a => a.submission_date);
            // Remove pending approvals from savedApprovalItems
            var oldPendingApprovalItems = savedApprovalItems.Where(s => s.submission_status.ToLower() == "pending").ToList();
            if (oldPendingApprovalItems != null && oldPendingApprovalItems.Any())
            {
                foreach(var pendingItem in oldPendingApprovalItems)
                {
                    savedApprovalItems.Remove(pendingItem);
                }
            }
            // Get items in allTerrainApprovals that are not in savedApprovalItems, these are new since last time
            var newSubmissions = allTerrainApprovals.Where(all => savedApprovalItems.Count(x => x.achievement_id == all.achievement_id) == 0).ToList();

            foreach (var newApproval in newSubmissions)
            {
                newApproval.achievement_name = await GetAchievementName(newApproval.member_id, newApproval.achievement_id, newApproval.achievement_type);
                if (initialLoad)
                    newApproval.presented_date = newApproval.awarded_date;
            }

            savedApprovalItems.AddRange(newSubmissions);

            WriteApprovalsListToFileSystem(savedApprovalItems.OrderBy(a => a.submission_date).ToList(), unitId);

            return savedApprovalItems ?? new List<ApprovalsListModel>();
        }

        public void UpdateApproval (string unitId, ApprovalsListModel approval)
        {
            var savedApprovalItems = ReadApprovalListFromFileSystem(unitId);
            var approvalItem = savedApprovalItems.Where(a => a.achievement_id == approval.achievement_id && a.submission_type == approval.submission_type).FirstOrDefault();
            if (approvalItem != null)
            {
                approvalItem.presented_date = approval.presented_date.HasValue ? approval.presented_date.Value.ToLocalTime() : null;
            }
            WriteApprovalsListToFileSystem(savedApprovalItems.OrderBy(a => a.submission_date).ToList(), unitId);
        }
    }
}
