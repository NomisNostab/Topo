using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.Pdf;
using Syncfusion.XlsIORenderer;
using Topo.Models.OAS;
using Topo.Services;

namespace Topo.Controllers
{
    public class OASController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IOASService _oasService;
        private readonly IMemberListService _memberListService;
        private readonly IReportService _reportService;
        public OASController(StorageService storageService,
            IOASService oasService,
            IMemberListService memberListService,
            IReportService reportService)
        {
            _storageService = storageService;
            _oasService = oasService;
            _memberListService = memberListService;
            _reportService = reportService;
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
                if (button == "OASReportPdf")
                {
                    return await OASWorkbook(oasIndexViewModel.SelectedUnitId, oasIndexViewModel.SelectedStage, oasIndexViewModel.HideCompletedMembers, Constants.OutputType.pdf);
                }
                if (button == "OASReportXlsx")
                {
                    return await OASWorkbook(oasIndexViewModel.SelectedUnitId, oasIndexViewModel.SelectedStage, oasIndexViewModel.HideCompletedMembers, Constants.OutputType.xlsx);
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

        private async Task<ActionResult> OASWorkbook(string selectedUnitId, string selectedStageTemplate, bool hideCompletedMembers, Constants.OutputType outputType)
        {
            var templateList = await _oasService.GetOASTemplate(selectedStageTemplate.Replace("/latest.json", ""));
            var templateName = selectedStageTemplate.Replace("/latest.json", "").Replace('/', '_');
            var templateTitle = templateList.Count > 0 ? templateList[0].TemplateTitle : "";
            if (hideCompletedMembers)
                templateTitle += " (in progress)";

            var sortedAnswers = await _oasService.GenerateOASWorksheetAnswers(selectedUnitId, selectedStageTemplate, hideCompletedMembers, templateList);

            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var section = _storageService.SelectedSection;

            var workbook = _reportService.GenerateOASWorksheetWorkbook(sortedAnswers, groupName, section, unitName, templateTitle, outputType == Constants.OutputType.pdf);

            //Stream 
            MemoryStream strm = new MemoryStream();

            if (outputType == Constants.OutputType.xlsx)
            {
                //Stream as Excel file
                workbook.SaveAs(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/vnd.ms-excel", $"OAS_Worksheet_{unitName.Replace(' ', '_')}_{templateName}.xlsx");
            }
            else
            {
                //Stream as Excel file
                var sheet = workbook.Worksheets[0];

                //Initialize XlsIO renderer.
                XlsIORenderer renderer = new XlsIORenderer();

                //Convert Excel document into PDF document 
                PdfDocument pdfDocument = renderer.ConvertToPDF(workbook);
                pdfDocument.Save(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/pdf", $"OAS_Worksheet_{unitName.Replace(' ', '_')}_{templateName}.pdf");
            }
        }

    }
}
