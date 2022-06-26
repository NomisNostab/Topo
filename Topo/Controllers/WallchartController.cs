using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.Pdf;
using Syncfusion.XlsIORenderer;
using Topo.Models.Wallchart;
using Topo.Services;

namespace Topo.Controllers
{
    public class WallchartController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly IWallchartService _wallchartService;
        private readonly IReportService _reportService;
        public WallchartController(StorageService storageService, IMemberListService memberListService, IWallchartService wallchartService, IReportService reportService)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _wallchartService = wallchartService;
            _reportService = reportService;
        }

        private void SetViewBag()
        {
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            ViewBag.Unit = _storageService.SelectedUnitName;
        }

        private WallchartIndexViewModel SetModel()
        {
            WallchartIndexViewModel model = new WallchartIndexViewModel();
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
        public IActionResult Index()
        {
            var model = SetModel();
            SetViewBag();

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(WallchartIndexViewModel wallchartIndexViewModel)
        {
            var model = SetModel();
            if (ModelState.IsValid)
            {
                _storageService.SelectedUnitId = wallchartIndexViewModel.SelectedUnitId;
                if (_storageService.Units != null)
                    _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == _storageService.SelectedUnitId)?.FirstOrDefault()?.Text;
                model.SelectedUnitId = _storageService.SelectedUnitId;
                model.SelectedUnitName = _storageService.SelectedUnitName ?? "";
            }
            SetViewBag();
            return View(model);
        }

        public async Task<ActionResult> WallchartPdf(string selectedUnitId)
        {
            return await WallchartReport(selectedUnitId, Constants.OutputType.pdf);
        }

        public async Task<ActionResult> WallchartXlsx(string selectedUnitId)
        {
            return await WallchartReport(selectedUnitId, Constants.OutputType.xlsx);
        }

        public async Task<ActionResult> WallchartReport(string selectedUnitId, Constants.OutputType outputType)
        {
            var groupName = _storageService.GroupName;
            var section = _storageService.SelectedSection;
            var unitName = _storageService.SelectedUnitName ?? "";

            var wallchartItems = await _wallchartService.GetWallchartItems(selectedUnitId);
            var workbook = _reportService.GenerateWallchartWorkbook(wallchartItems, groupName, section, unitName, true);

            //Stream 
            MemoryStream strm = new MemoryStream();

            if (outputType == Constants.OutputType.xlsx)
            {
                //Stream as Excel file
                workbook.SaveAs(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/vnd.ms-excel", $"Wallchart_Report_{unitName.Replace(' ', '_')}.xlsx");
            }
            else
            {
                //Stream as PDF

                //Initialize XlsIO renderer.
                XlsIORenderer renderer = new XlsIORenderer();

                //Convert Excel document into PDF document 
                PdfDocument pdfDocument = renderer.ConvertToPDF(workbook);
                pdfDocument.Save(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/pdf", $"Wallchart_Report_{unitName.Replace(' ', '_')}.pdf");
            }

        }
    }
}
