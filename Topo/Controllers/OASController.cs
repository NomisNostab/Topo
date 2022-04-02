using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Topo.Services;
using Topo.Models.MemberList;
using Topo.Models.OAS;
using Microsoft.AspNetCore.Mvc.Rendering;
using FastReport;
using FastReport.Table;
using FastReport.Export.PdfSimple;
using FastReport.Utils;
using System.Drawing;
using Topo.Data;
using Topo.Data.Models;
using System.Globalization;

namespace Topo.Controllers
{
    public class OASController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IOASService _oasService;
        private readonly IMemberListService _memberListService;
        private readonly TopoDBContext _dbContext;

        public OASController(StorageService storageService,
            IOASService oasService,
            IMemberListService memberListService,
            TopoDBContext dBContext)
        {
            _storageService = storageService;
            _oasService = oasService;
            _memberListService = memberListService;
            _dbContext = dBContext;
        }

        private void SetViewBag()
        {
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            ViewBag.Unit = _storageService.SelectedUnitName;
        }

        private OASIndexViewModel SetUpViewModel()
        {
            var model = new OASIndexViewModel();
            model.Units = new List<SelectListItem>();
            if (_storageService.Units != null)
                model.Units = _storageService.Units;
            if (_storageService.SelectedUnitId != null)
                model.SelectedUnitId = _storageService.SelectedUnitId;
            if (_storageService.SelectedUnitName != null)
                model.SelectedUnitName = _storageService.SelectedUnitName;
            model.Streams = _oasService.GetOASStreamList();
            if (model.Stages == null)
                model.Stages = new List<SelectListItem>();
            return model;
        }


        // GET: OAS
        public ActionResult Index()
        {
            var model = SetUpViewModel();
            SetViewBag();
            return View(model);
        }


        [HttpPost]
        public async Task<ActionResult> Index(OASIndexViewModel oasIndexViewModel)
        {
            var model = new OASIndexViewModel();
            if (oasIndexViewModel.SelectedStream != null && oasIndexViewModel.SelectedStage != null && !oasIndexViewModel.SelectedStage.Contains(oasIndexViewModel.SelectedStream))
                ModelState.AddModelError("SelectedStage", "Select new stage");

            if (ModelState.IsValid)
            {
                _storageService.SelectedUnitId = oasIndexViewModel.SelectedUnitId;
                if (_storageService.Units != null)
                    _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == oasIndexViewModel.SelectedUnitId)?.FirstOrDefault()?.Text;
                model = SetUpViewModel();
                model.Stages = _oasService.GetOASStageListItems(_storageService.OASStageList);
                model.SelectedStream = oasIndexViewModel.SelectedStream;
                model.SelectedStage = oasIndexViewModel.SelectedStage;
            }
            else
            {
                model = SetUpViewModel();

                if (oasIndexViewModel.SelectedStream != null)
                {
                    _storageService.OASStageList = await _oasService.GetOASStageList(oasIndexViewModel.SelectedStream);
                    model.Stages = _oasService.GetOASStageListItems(_storageService.OASStageList);
                    model.SelectedStream = oasIndexViewModel.SelectedStream;
                    model.SelectedStage = "";
                }
            }
            SetViewBag();
            return View(model);
        }

        public async Task<ActionResult> OASReport(string selectedUnitId, string selectedStageTemplate)
        {
            var selectedStage = _storageService.OASStageList.Where(s => s.TemplateLink == selectedStageTemplate).SingleOrDefault();
            var getUnitAchievementsResultsModel = await _oasService.GetUnitAchievements(selectedUnitId, selectedStage.Stream.ToLower(), selectedStage.Branch, selectedStage.Stage);
            var members = await _memberListService.GetMembersAsync();
            var sortedMemberList = members.Where(m => m.isAdultLeader == 0).OrderBy(m => m.first_name).ThenBy(m => m.last_name).ToList();
            var templateList = await _oasService.GetOASTemplate(selectedStageTemplate.Replace("/latest.json", ""));
            return GenerateAchievementResults(getUnitAchievementsResultsModel, sortedMemberList, templateList);
        }

        private FileStreamResult GenerateAchievementResults(GetUnitAchievementsResultsModel getUnitAchievementsResultsModel,
            List<MemberListModel> sortedMemberList,
            List<OASTemplate> templateList)
        {
            var templateTitle = templateList.Count > 0 ? templateList[0].TemplateTitle : "";
            var templateName = templateList.Count > 0 ? templateList[0].TemplateName : "";

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

            foreach (var member in getUnitAchievementsResultsModel.results)
            {
                DateTime? awardedDate = member.status == "awarded" ? member.status_updated : null;
                if (member.answers == null)
                {
                    var worksheetAnswers = _dbContext.OASWorksheetAnswers
                        .Where(wa => wa.MemberId == member.member_id)
                        .ToList();
                    foreach (var worksheetAnswer in worksheetAnswers)
                    {
                        if (worksheetAnswer != null)
                            worksheetAnswer.MemberAnswer = awardedDate;
                    }
                }
                else
                {

                    var verifiedAnswers = member.answers.Where(a => a.Key.EndsWith("_verifiedDate"));
                    if (verifiedAnswers.Any())
                    {
                        foreach (var answer in verifiedAnswers)
                        {
                            var worksheetAnswer = _dbContext.OASWorksheetAnswers
                                .Where(wa => wa.InputId == answer.Key.Replace("_verifiedDate", ""))
                                .Where(wa => wa.MemberId == member.member_id)
                                .FirstOrDefault();
                            if (worksheetAnswer != null)
                                try
                                {
                                    worksheetAnswer.MemberAnswer = awardedDate ?? DateTime.Parse(answer.Value, CultureInfo.InvariantCulture);
                                }
                                catch (Exception ex)
                                {

                                }
                        }
                    }
                    else
                    {
                        foreach (var answer in member.answers)
                        {
                            var worksheetAnswer = _dbContext.OASWorksheetAnswers
                                .Where(wa => wa.InputId == answer.Key)
                                .Where(wa => wa.MemberId == member.member_id)
                                .FirstOrDefault();
                            if (worksheetAnswer != null)
                                try
                                {
                                    worksheetAnswer.MemberAnswer = awardedDate;
                                }
                                catch (Exception ex)
                                {

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


            if (report.Prepare())
            {
                // Set PDF export props
                PDFSimpleExport pdfExport = new PDFSimpleExport();
                pdfExport.ShowProgress = false;

                MemoryStream strm = new MemoryStream();
                report.Report.Export(pdfExport, strm);
                report.Dispose();
                pdfExport.Dispose();
                strm.Position = 0;

                // return stream in browser
                var fileName = $"OAS_Worksheet_{_storageService.SelectedUnitName.Replace(' ', '_')}_{templateName.Replace('/', '_')}.pdf";
                return File(strm, "application/pdf", fileName);
            }
            return null;

        }


        // POST: OAS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
