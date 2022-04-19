using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Topo.Models.Milestone;
using Topo.Services;

namespace Topo.Controllers
{
    public class MilestoneController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly IMilestoneService _MilestoneService;

        public MilestoneController(StorageService storageService, IMemberListService memberListService, IMilestoneService MilestoneService)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _MilestoneService = MilestoneService;
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
        public ActionResult Index(MilestoneIndexViewModel MilestoneIndexViewModel)
        {
            var model = SetModel();
            if (ModelState.IsValid)
            {
                _storageService.SelectedUnitId = MilestoneIndexViewModel.SelectedUnitId;
                if (_storageService.Units != null)
                    _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == MilestoneIndexViewModel.SelectedUnitId)?.FirstOrDefault()?.Text;
                model.SelectedUnitId = _storageService.SelectedUnitId;
                model.SelectedUnitName = _storageService.SelectedUnitName;
            }
            SetViewBag();
            return View(model);
        }

        public async Task<ActionResult> MilestoneReport(string selectedUnitId)
        {
            var report = await _MilestoneService.GenerateMilestoneReport(selectedUnitId);
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
    }
}

////Build report template for model
//var report1 = new FastReport.Report();
//report1.Dictionary.RegisterBusinessObject(
//        new List<Models.Milestone.MilestoneSummaryListModel>(),
//        "MilestoneSummary",
//        2,
//        true
//    );
//report1.Save(@"C:\Users\simon\Documents\MilestoneSummary.frx");
