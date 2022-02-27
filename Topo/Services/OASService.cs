using Microsoft.AspNetCore.Mvc.Rendering;
using Topo.Models.OAS;


namespace Topo.Services
{
    public interface IOASService
    {
        public Task<List<OASStageListModel>> GetOASStageList(string stream);
        public List<SelectListItem> GetOASStreamList();
        public List<SelectListItem> GetOASStageListItems(List<OASStageListModel> oasStageListModels);
        public Task GetUnitAchievements(string unit, string stream, string branch, int stage);
    }
    public class OASService : IOASService
    {
        private readonly StorageService _storageService;
        private readonly ITerrainAPIService _terrainAPIService;

        public OASService(StorageService storageService, ITerrainAPIService terrainAPIService)
        {
            _storageService = storageService;
            _terrainAPIService = terrainAPIService;
        }
        public async Task<List<OASStageListModel>> GetOASStageList(string stream)
        {
            //var eventListModel = new EventListModel();
            var getEventResultModel = await _terrainAPIService.GetOASTreeAsync(stream);
            var streamTitle = getEventResultModel.title;
            var oasStageListModels = new List<OASStageListModel>();
            ProcessTree(streamTitle, getEventResultModel.tree, oasStageListModels);
            return oasStageListModels;
        }
        private void ProcessTree(string streamTitle, Tree treeNode, List<OASStageListModel> oasStageListModels)
        {
            oasStageListModels.Add(new OASStageListModel
            {
                Stream = streamTitle,
                Branch = treeNode.branch_id,
                StageTitle = treeNode.title,
                Stage = treeNode.stage,
                TemplateLink = treeNode.template_link,
                SelectListItemText = FormatStageText(streamTitle, treeNode.title, treeNode.stage)
            });
            if (treeNode.children != null)
            {
                foreach (var child in treeNode.children)
                {
                    ProcessChild(streamTitle, child, oasStageListModels);
                }
            }
        }
        private void ProcessChild(string streamTitle, Child childNode, List<OASStageListModel> oasStageListModels)
        {
            oasStageListModels.Add(new OASStageListModel {
                Stream = streamTitle,
                Branch = childNode.branch_id,
                StageTitle = childNode.title,
                Stage = childNode.stage,
                TemplateLink = childNode.template_link,
                SelectListItemText = FormatStageText(streamTitle, childNode.title, childNode.stage)
            });
            if (childNode.children != null)
            {
                foreach (var child in childNode.children)
                {
                    ProcessChild(streamTitle, child, oasStageListModels);
                }
            }
        }

        private string FormatStageText(string streamTitle, string nodeTitle, int nodeStage)
        {
            return streamTitle == nodeTitle ? $"{streamTitle} {nodeStage}" : $"{streamTitle} {nodeTitle} {nodeStage}";
        }

        public List<SelectListItem> GetOASStreamList()
        {
            var oasStreams = new List<SelectListItem>();

            oasStreams.Add(new SelectListItem("Bushcraft", "bushcraft"));
            oasStreams.Add(new SelectListItem("Bushwalking", "bushwalking"));
            oasStreams.Add(new SelectListItem("Camping", "camping"));
            oasStreams.Add(new SelectListItem("Alpine", "alpine"));
            oasStreams.Add(new SelectListItem("Aquatics", "aquatics"));
            oasStreams.Add(new SelectListItem("Boating", "boating"));
            oasStreams.Add(new SelectListItem("Cycling", "cycling"));
            oasStreams.Add(new SelectListItem("Paddling", "paddling"));
            oasStreams.Add(new SelectListItem("Vertical", "vertical"));

            return oasStreams;
        }

        public List<SelectListItem> GetOASStageListItems(List<OASStageListModel> oasStageListModels)
        {
            var oasStages = new List<SelectListItem>();
            foreach (var stage in oasStageListModels)
            {
                oasStages.Add(new SelectListItem(stage.SelectListItemText, stage.TemplateLink));
            }
            return oasStages;
        }

        public async Task GetUnitAchievements(string unit, string stream, string branch, int stage)
        {
            var achievementsResultModel = await _terrainAPIService.GetUnitAchievements(unit, stream, branch, stage);
        }
    }
}
