using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Topo.Models.MemberList;
using Topo.Models.SIA;
using Topo.Services;

namespace Topo.Controllers
{
    public class SIAController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly ISIAService _SIAService;

        public SIAController(StorageService storageService, IMemberListService memberListService, ISIAService sIAService)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _SIAService = sIAService;
        }

        private void SetViewBag()
        {
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            ViewBag.Unit = _storageService.SelectedUnitName;
        }

        // GET: SIAController
        public async Task<ActionResult> Index()
        {
            SIAIndexViewModel model = new SIAIndexViewModel();
            model.Units = new List<SelectListItem>();
            if (_storageService.Units != null)
                model.Units = _storageService.Units;
            SetViewBag();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(SIAIndexViewModel siaIndexViewModel)
        {
            var model = new SIAIndexViewModel();
            if (ModelState.IsValid)
            {
                _storageService.SelectedUnitId = siaIndexViewModel.SelectedUnitId;
                if (_storageService.Units != null)
                    _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == siaIndexViewModel.SelectedUnitId)?.SingleOrDefault()?.Text;
                model.SelectedUnitId = _storageService.SelectedUnitId;
                model.SelectedUnitName = _storageService.SelectedUnitName;
                model.Units = new List<SelectListItem>();
                if (_storageService.Units != null)
                    model.Units = _storageService.Units;
            }
            SetViewBag();
            return View(model);
        }

        public async Task<ActionResult> SIAReport()
        {
            var report = await _SIAService.GenerateSIAReport();
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
                return File(strm, "application/pdf", $"SIA_Projects_{unitName.Replace(' ', '_')}.pdf");
            }
            else
            {
                SetViewBag();
                return View();
            }
        }
    }
}
