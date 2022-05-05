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
using System.Text;

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
            SetViewBag();
            return model;
        }


        // GET: OAS
        public ActionResult Index()
        {
            var model = SetUpViewModel();
            return View(model);
        }


        [HttpPost]
        public async Task<ActionResult> Index(OASIndexViewModel oasIndexViewModel, string button)
        {
            var model = new OASIndexViewModel();
            ModelState.Remove("button");
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
                if (button == "OASReport")
                {
                    return await OASReport(oasIndexViewModel.SelectedUnitId, oasIndexViewModel.SelectedStage, oasIndexViewModel.HideCompletedMembers);
                }
                if (button == "OASCSV")
                {
                    return await OASCSV(oasIndexViewModel.SelectedUnitId, oasIndexViewModel.SelectedStage, oasIndexViewModel.HideCompletedMembers);
                }
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
            return View(model);
        }

        private async Task<ActionResult> OASReport(string selectedUnitId, string selectedStageTemplate, bool hideCompletedMembers)
        {
            var report = await _oasService.GenerateOASWorksheet(selectedUnitId, selectedStageTemplate, hideCompletedMembers);
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
                var templateName = selectedStageTemplate.Replace("/latest.json", "").Replace('/', '_');
                var unitName = _storageService.SelectedUnitName ?? "";
                var fileName = $"OAS_Worksheet_{unitName.Replace(' ', '_')}_{templateName}.pdf";
                return File(strm, "application/pdf", fileName);
            }
            return null;
        }

        private async Task<ActionResult> OASCSV(string selectedUnitId, string selectedStageTemplate, bool hideCompletedMembers)
        {
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var templateName = selectedStageTemplate.Replace("/latest.json", "").Replace('/', '_');

            var worksheetAnswers = await _oasService.GenerateOASWorksheetCSV(selectedUnitId, selectedStageTemplate, hideCompletedMembers);
            var groupedAnswers = worksheetAnswers.GroupBy(wa => wa.MemberName).ToList();
            var quote = '"';
            MemoryStream ms = new MemoryStream();
            // Encoding.UTF8 produces stream with BOM, new UTF8Encoding(false) - without BOM
            using (StreamWriter sw = new StreamWriter(ms, new UTF8Encoding(false), 8192, true))
            {
                sw.WriteLine(groupName);
                sw.WriteLine(unitName);
                sw.WriteLine($"{templateName.Replace('_', ' ').ToUpper()}");
                sw.Write(",");
                sw.WriteLine(string.Join(",", groupedAnswers.FirstOrDefault().Select(a => a.InputTitle)));
                sw.Write("Scout,");
                sw.WriteLine(string.Join(",", groupedAnswers.FirstOrDefault().Select(a => quote + a.InputLabel + quote)));

                foreach (var groupedAnswer in groupedAnswers.OrderBy(ga => ga.Key))
                {
                    var member = groupedAnswer.Key;
                    var answerCount = groupedAnswer.Count();
                    sw.Write($"{member},");
                    sw.WriteLine(string.Join(", ", groupedAnswer.OrderBy(ga => ga.InputTitleSortIndex).ThenBy(ga => ga.InputSortIndex).Select(a => a.MemberAnswer)));
                }
            }
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", $"OAS_Worksheet_{unitName.Replace(' ', '_')}_{templateName}.csv");
        }

    }
}
