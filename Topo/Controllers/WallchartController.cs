using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;
using System.Text;
using Topo.Models.Wallchart;
using Topo.Services;

namespace Topo.Controllers
{
    public class WallchartController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly IWallchartService _wallchartService;

        public WallchartController(StorageService storageService, IMemberListService memberListService, IWallchartService wallchartService)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _wallchartService = wallchartService;
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

        public async Task<ActionResult> WallchartReport(string selectedUnitId)
        {
            var report = await _wallchartService.GenerateWallchartReport(selectedUnitId);
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
                return File(strm, "application/pdf", $"Wallchart_Report_{unitName.Replace(' ', '_')}.pdf");
            }
            else
            {
                var model = SetModel();
                SetViewBag();
                return View(model);
            }
        }

        public async Task<ActionResult> WallchartCSV(string selectedUnitId)
        {
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";

            var wallchartItems = await _wallchartService.GetWallchartItems(selectedUnitId);

            MemoryStream ms = new MemoryStream();
            // Encoding.UTF8 produces stream with BOM, new UTF8Encoding(false) - without BOM
            using (StreamWriter sw = new StreamWriter(ms, new UTF8Encoding(false), 8192, true))
            {
                sw.WriteLine(groupName);
                sw.WriteLine(unitName);
                sw.WriteLine("Wallchart");
                sw.WriteLine();
                PropertyInfo[] properties = typeof(WallchartItemModel).GetProperties();
                sw.WriteLine(string.Join(",", properties.Select(x => x.Name)));

                foreach (var member in wallchartItems.OrderBy(m => m.MemberName))
                {
                    sw.WriteLine(string.Join(",", properties.Select(prop => prop.GetValue(member))));
                }
            }
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", $"Wallchart_Report_{unitName.Replace(' ', '_')}.csv");
        }
    }
}
