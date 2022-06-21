using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.Pdf;
using Syncfusion.XlsIORenderer;
using System.Reflection;
using System.Text;
using Topo.Models.Milestone;
using Topo.Services;

namespace Topo.Controllers
{
    public class MilestoneController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly IMilestoneService _milestoneService;
        private readonly IReportService _reportService;

        public MilestoneController(StorageService storageService, IMemberListService memberListService, IMilestoneService milestoneService, IReportService reportService)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _milestoneService = milestoneService;
            _reportService = reportService;
        }

        private void SetViewBag()
        {
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            ViewBag.Unit = _storageService.SelectedUnitName;
        }

        private MilestoneIndexViewModel SetModel()
        {
            MilestoneIndexViewModel model = new MilestoneIndexViewModel();
            model.Units = new List<SelectListItem>();
            if (_storageService.Units != null)
                model.Units = _storageService.Units;
            if (_storageService.Units != null && _storageService.SelectedUnitId != null)
            {
                _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == _storageService.SelectedUnitId)?.FirstOrDefault()?.Text;
                model.SelectedUnitId = _storageService.SelectedUnitId;
                model.SelectedUnitName = _storageService.SelectedUnitName ?? string.Empty;
            }
            return model;
        }

        // GET: MilestoneController
        public ActionResult Index()
        {
            var model = SetModel();
            SetViewBag();

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(MilestoneIndexViewModel milestoneIndexViewModel)
        {
            var model = SetModel();
            if (ModelState.IsValid)
            {
                _storageService.SelectedUnitId = milestoneIndexViewModel.SelectedUnitId;
                if (_storageService.Units != null)
                    _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == _storageService.SelectedUnitId)?.FirstOrDefault()?.Text;
                model.SelectedUnitId = _storageService.SelectedUnitId;
                model.SelectedUnitName = _storageService.SelectedUnitName ?? "";
            }
            SetViewBag();
            return View(model);
        }

        public async Task<ActionResult> MilestoneReport(string selectedUnitId)
        {
            var report = await _milestoneService.GenerateMilestoneReport(selectedUnitId);
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
                var unitName = _storageService.SelectedUnitName ?? "";
                return File(strm, "application/pdf", $"Milestone_Report_{unitName.Replace(' ', '_')}.pdf");
            }
            else
            {
                var model = SetModel();
                SetViewBag();
                return View(model);
            }
        }

        public async Task<ActionResult> MilestonePdf(string selectedUnitId)
        {
            return await MilestoneWorkbook(selectedUnitId, Constants.OutputType.pdf);
        }

        public async Task<ActionResult> MilestoneXlsx(string selectedUnitId)
        {
            return await MilestoneWorkbook(selectedUnitId, Constants.OutputType.xlsx);
        }

        public async Task<ActionResult> MilestoneWorkbook(string selectedUnitId, Constants.OutputType outputType)
        {
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var section = _storageService.SelectedSection;

            var milestoneSummaries = await _milestoneService.GetMilestoneSummaries(selectedUnitId);

            var workbook = _reportService.GenerateMilestoneWorkbook(milestoneSummaries, groupName, section, unitName, true);

            MemoryStream strm = new MemoryStream();
            if (outputType == Constants.OutputType.xlsx)
            {
                //Stream as Excel file
                workbook.SaveAs(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/vnd.ms-excel", $"Milestone_Report_{unitName.Replace(' ', '_')}.xlsx");
            }
            else
            {
                //Stream as Excel file

                //Initialize XlsIO renderer.
                XlsIORenderer renderer = new XlsIORenderer();

                //Convert Excel document into PDF document 
                PdfDocument pdfDocument = renderer.ConvertToPDF(workbook);
                pdfDocument.Save(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/pdf", $"Milestone_Report_{unitName.Replace(' ', '_')}.pdf");
            }

        }

    }
}


