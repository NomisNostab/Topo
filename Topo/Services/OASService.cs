using FastReport;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using Topo.Data;
using Topo.Data.Models;
using Topo.Models.OAS;

namespace Topo.Services
{
    public interface IOASService
    {
        public Task<List<OASStageListModel>> GetOASStageList(string stream);
        public List<SelectListItem> GetOASStreamList();
        public List<SelectListItem> GetOASStageListItems(List<OASStageListModel> oasStageListModels);
        public Task<GetUnitAchievementsResultsModel> GetUnitAchievements(string unit, string stream, string branch, int stage);
        public Task<List<OASTemplate>> GetOASTemplate(string templateName);
        public Task<Report> GenerateOASWorksheet(string selectedUnitId, string selectedStageTemplate);
    }
    public class OASService : IOASService
    {
        private readonly StorageService _storageService;
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly TopoDBContext _dbContext;
        private readonly IMemberListService _memberListService;

        public OASService(StorageService storageService, ITerrainAPIService terrainAPIService, TopoDBContext dBContext, IMemberListService memberListService)
        {
            _storageService = storageService;
            _terrainAPIService = terrainAPIService;
            _dbContext = dBContext;
            _memberListService = memberListService;
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

        public async Task<GetUnitAchievementsResultsModel> GetUnitAchievements(string unit, string stream, string branch, int stage)
        {
            return await _terrainAPIService.GetUnitAchievements(unit, stream, branch, stage);
        }

        public async Task<List<OASTemplate>> GetOASTemplate(string templateName)
        {
            var templateList = _dbContext.OASTemplates
                .Where(t => t.TemplateName == templateName && t.InputGroupSort < 4)
                .OrderBy(t => t.InputGroupSort)
                .ThenBy(t => t.Id)
                .ToList();
            if (templateList == null || templateList.Count == 0)
            {
                var templateTitle = "";
                var inputGroupTitle = "";
                var oasTemplateResult = await _terrainAPIService.GetOASTemplateAsync(templateName);
                foreach (var document in oasTemplateResult.document)
                {
                    templateTitle = document.title;
                    foreach (var inputGroup in document.input_groups)
                    { 
                        inputGroupTitle = inputGroup.title;
                        foreach (var input in inputGroup.inputs)
                        {
                            var oasTemplate = new OASTemplate { 
                                TemplateName = templateName,
                                TemplateTitle = templateTitle,
                                InputGroup = inputGroupTitle,
                                InputGroupSort = GetInputGroupSort(inputGroupTitle),
                                InputId = input.id,
                                InputLabel = input.label
                            };
                            _dbContext.OASTemplates.Add(oasTemplate);
                            templateList = _dbContext.OASTemplates
                                .Where(t => t.TemplateName == templateName && t.InputGroupSort < 4)
                                .OrderBy(t => t.InputGroupSort)
                                .ThenBy(t => t.Id)
                                .ToList();
                        }
                    }
                }
                _dbContext.SaveChanges();
                templateList = _dbContext.OASTemplates
                .Where(t => t.TemplateName == templateName && t.InputGroupSort < 4)
                .OrderBy(t => t.InputGroupSort)
                .ThenBy(t => t.Id)
                .ToList();
            }

            return templateList;
        }

        private int GetInputGroupSort(string inputGroup)
        {
            if (inputGroup == "Plan>")
                return 1;
            if (inputGroup == "Do>")
                return 2;
            if (inputGroup == "Review>")
                return 3;
            return 4;
        }

        public async Task<Report> GenerateOASWorksheet(string selectedUnitId, string selectedStageTemplate)
        {
            var selectedStage = _storageService.OASStageList.Where(s => s.TemplateLink == selectedStageTemplate).SingleOrDefault();
            var getUnitAchievementsResultsModel = await GetUnitAchievements(selectedUnitId, selectedStage.Stream.ToLower(), selectedStage.Branch, selectedStage.Stage);
            var members = await _memberListService.GetMembersAsync();
            var sortedMemberList = members.Where(m => m.isAdultLeader == 0).OrderBy(m => m.first_name).ThenBy(m => m.last_name).ToList();
            var templateList = await GetOASTemplate(selectedStageTemplate.Replace("/latest.json", ""));

            var templateTitle = templateList.Count > 0 ? templateList[0].TemplateTitle : "";

            _dbContext.OASWorksheetAnswers.RemoveRange(_dbContext.OASWorksheetAnswers);

            foreach (var item in templateList.Where(t => t.InputGroupSort < 4).OrderBy(t => t.InputGroupSort).ThenBy(t => t.Id))
            {
                foreach (var member in sortedMemberList)
                {
                    OASWorksheetAnswers oASWorksheetAnswers = new OASWorksheetAnswers()
                    {
                        InputId = item.InputId,
                        InputTitle = item.InputGroup.Replace(">", ""),
                        InputLabel = item.InputLabel,
                        InputTitleSortIndex = item.InputGroupSort,
                        InputSortIndex = item.Id,
                        MemberId = member.id,
                        MemberName = $"{member.first_name} {member.last_name}",
                        MemberAnswer = null
                    };
                    _dbContext.OASWorksheetAnswers.Add(oASWorksheetAnswers);
                }
            }
            _dbContext.SaveChanges();

            foreach (var memberAchievement in getUnitAchievementsResultsModel.results)
            {
                var verifiedAnswers = memberAchievement.answers.Where(a => a.Key.EndsWith("_verifiedDate")) ?? new Dictionary<string, string>();
                // In progress
                if (memberAchievement.status == "draft_review")
                {
                    if (verifiedAnswers.Any())
                    {
                        foreach (var answer in verifiedAnswers)
                        {
                            var worksheetAnswer = _dbContext.OASWorksheetAnswers
                                .Where(wa => wa.InputId == answer.Key.Replace("_verifiedDate", ""))
                                .Where(wa => wa.MemberId == memberAchievement.member_id)
                                .FirstOrDefault();
                            if (worksheetAnswer != null)
                                try
                                {
                                    worksheetAnswer.MemberAnswer = ConvertAnswerDate(answer.Value);
                                }
                                catch (Exception ex)
                                {
                                    worksheetAnswer.MemberAnswer = memberAchievement.status_updated;
                                }
                        }
                    }
                }

                // Awarded
                if (memberAchievement.status == "awarded")
                {
                    // Imported
                    if (memberAchievement.imported != null)
                    {
                        // Conver string date from yyyy-mm-dd format
                        var importedDate = DateTime.ParseExact(memberAchievement.imported.date_awarded, "yyyy-MM-dd", CultureInfo.InvariantCulture); 
                        // Get all answers
                        var worksheetAnswers = _dbContext.OASWorksheetAnswers
                            .Where(wa => wa.MemberId == memberAchievement.member_id)
                            .ToList();
                        // Set answer date for each answer
                        foreach (var worksheetAnswer in worksheetAnswers)
                        {
                            if (worksheetAnswer != null)
                                worksheetAnswer.MemberAnswer = importedDate;
                        }
                    }
                    // No answers
                    if (memberAchievement.answers == null || !verifiedAnswers.Any())
                    {
                        // Get all answers
                        var worksheetAnswers = _dbContext.OASWorksheetAnswers
                            .Where(wa => wa.MemberId == memberAchievement.member_id)
                            .ToList();
                        // Set answer date for each answer
                        foreach (var worksheetAnswer in worksheetAnswers)
                        {
                            if (worksheetAnswer != null)
                                worksheetAnswer.MemberAnswer = memberAchievement.status_updated;
                        }
                    }
                    // Answers
                    if (verifiedAnswers.Any())
                    {
                        foreach (var answer in verifiedAnswers)
                        {
                            var worksheetAnswer = _dbContext.OASWorksheetAnswers
                                .Where(wa => wa.InputId == answer.Key.Replace("_verifiedDate", ""))
                                .Where(wa => wa.MemberId == memberAchievement.member_id)
                                .FirstOrDefault();
                            if (worksheetAnswer != null)
                                try
                                {
                                    worksheetAnswer.MemberAnswer = ConvertAnswerDate(answer.Value);
                                }
                                catch (Exception ex)
                                {
                                    worksheetAnswer.MemberAnswer = memberAchievement.status_updated;
                                }
                        }
                    }
                }
            }
            _dbContext.SaveChanges();

            var sortedAnswers = _dbContext.OASWorksheetAnswers
                .OrderBy(owa => owa.InputTitleSortIndex)
                .ThenBy(owa => owa.InputSortIndex)
                .ToList();

            _dbContext.OASWorksheetAnswers.RemoveRange(_dbContext.OASWorksheetAnswers);

            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var report = new Report();
            var directory = Directory.GetCurrentDirectory();
            report.Load($@"{directory}\Reports\OASWorksheet.frx");
            report.SetParameterValue("GroupName", groupName);
            report.SetParameterValue("UnitName", unitName);
            report.SetParameterValue("OASStage", templateTitle);
            report.RegisterData(sortedAnswers, "OASWorksheets");

            return report;
        }

        private DateTime ConvertAnswerDate(string answerValue)
        {
            // Question answer dates seem to be either AU or US format. WTF!
            // "south_magnetic_find_electronic_means_compass_west_directions_e7e4fc_verifiedDate": "18/11/2020",
            // "least_activities_improved_bushcraft_learnt_from_enjoyed_talked_a5973d_verifiedDate": "11/18/2020",
            DateTime answerDate;
            try
            {
                answerDate = DateTime.ParseExact(answerValue, "dd/MM/yyyy", CultureInfo.InvariantCulture); // Date in AU format
                return answerDate;
            }
            catch
            {
                answerDate = DateTime.ParseExact(answerValue, "MM/dd/yyyy", CultureInfo.InvariantCulture); // Date in UA format
                return answerDate;
            }
        }
    }
}
