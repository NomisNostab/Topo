using Newtonsoft.Json;
using System.Text;
using Topo.Models.MemberList;

namespace Topo.Services
{
    public interface IMemberListService
    {
        public Task<List<MemberListModel>> GetMembersAsync();
        
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

        public async Task<List<MemberListModel>> GetMembersAsync()
        {
            var getMembersResultModel = await _terrainAPIService.GetMembersAsync();
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
                        date_of_birth = m.date_of_birth,
                        unit_council = m.unit.unit_council,
                        patrol_name = m.patrol == null ? "" : m.patrol.name,
                        patrol_duty = m.patrol == null ? "" : GetPatrolDuty(m.unit.duty, m.patrol.duty),
                        patrol_order = m.patrol == null ? 3 : GetPatrolOrder(m.unit.duty, m.patrol.duty),
                        isAdultLeader = m.unit.duty == "adult_leader" ? 1 : 0
                    })
                    .ToList();
            }
            return memberList;
        }

        private string GetPatrolDuty(string unitDuty, string patrolDuty)
        {
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
