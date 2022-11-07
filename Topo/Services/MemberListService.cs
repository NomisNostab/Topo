using System.Globalization;
using Topo.Models.MemberList;

namespace Topo.Services
{
    public interface IMemberListService
    {
        public Task<List<MemberListModel>> GetMembersAsync(string unitId);
    }

    public class MemberListService : IMemberListService
    {
        private readonly StorageService _storageService;
        private readonly ITerrainAPIService _terrainAPIService;

        public MemberListService(StorageService storageService, ITerrainAPIService terrainAPIService)
        {
            _storageService = storageService;
            _terrainAPIService = terrainAPIService;
        }

        public async Task<List<MemberListModel>> GetMembersAsync(string unitId)
        {
            var cachedMembersList = _storageService.CachedMembers.Where(cm => cm.Key == unitId).FirstOrDefault().Value;
            if (cachedMembersList != null)
                return cachedMembersList;

            var getMembersResultModel = await _terrainAPIService.GetMembersAsync(unitId ?? "");
            var memberList = new List<MemberListModel>();
            if (getMembersResultModel != null && getMembersResultModel.results != null)
            {
                memberList = getMembersResultModel.results
                    .Select(m => new MemberListModel
                    {
                        id = m.id,
                        member_number = m.member_number,
                        first_name = m.first_name,
                        last_name = m.last_name,
                        age = GetAgeFromBirthdate(m.date_of_birth),
                        unit_council = m.unit.unit_council,
                        patrol_name = m.patrol == null ? "" : m.patrol.name,
                        patrol_duty = GetPatrolDuty(m.unit.duty, m.patrol?.duty ?? ""),
                        patrol_order = GetPatrolOrder(m.unit.duty, m.patrol?.duty ?? ""),
                        isAdultLeader = m.unit.duty == "adult_leader" ? 1 : 0,
                        status = m.status
                    })
                    .ToList();
                _storageService.CachedMembers.Add(new KeyValuePair<string, List<MemberListModel>>(unitId, memberList));
            }
            return memberList;
        }

        private string GetAgeFromBirthdate(string dateOfBirth)
        {
            var birthday = DateTime.ParseExact(dateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture); // Date in AU format
            DateTime now = DateTime.Today;
            int months = now.Month - birthday.Month;
            int years = now.Year - birthday.Year;

            if (now.Day < birthday.Day)
            {
                months--;
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }
            return $"{years}y {months}m";
        }



        private string GetPatrolDuty(string unitDuty, string patrolDuty)
        {
            if (unitDuty == "adult_leader")
                return "SL";
            if (unitDuty == "unit_leader")
                return "UL";
            if (patrolDuty == "assistant_patrol_leader")
                return "APL";
            if (patrolDuty == "patrol_leader")
                return "PL";
            return "";
        }

        private int GetPatrolOrder(string unitDuty, string patrolDuty)
        {
            if (unitDuty == "unit_leader")
                return 0;
            if (patrolDuty == "assistant_patrol_leader")
                return 2;
            if (patrolDuty == "patrol_leader")
                return 1;
            return 3;
        }

    }
}
