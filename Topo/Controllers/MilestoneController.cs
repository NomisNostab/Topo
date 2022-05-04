using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public MilestoneController(StorageService storageService, IMemberListService memberListService, IMilestoneService milestoneService)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _milestoneService = milestoneService;
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

        public async Task<ActionResult> MilestoneCSV(string selectedUnitId)
        {
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";

            var milestoneSummaries = await _milestoneService.GetMilestoneSummaries(selectedUnitId);

            MemoryStream ms = new MemoryStream();
            // Encoding.UTF8 produces stream with BOM, new UTF8Encoding(false) - without BOM
            using (StreamWriter sw = new StreamWriter(ms, new UTF8Encoding(false), 8192, true))
            {
                sw.WriteLine(groupName);
                sw.WriteLine(unitName);
                sw.WriteLine("Milestones");
                sw.WriteLine();
                PropertyInfo[] properties = typeof(MilestoneSummaryListModel).GetProperties();
                sw.WriteLine(string.Join(",", properties.Select(x => x.Name)));

                foreach (var member in milestoneSummaries.OrderBy(m => m.currentLevel).ThenBy(m => m.memberName))
                {
                    sw.WriteLine(string.Join(",", properties.Select(prop => prop.GetValue(member))));
                }
            }
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", $"Milestone_Report_{unitName.Replace(' ', '_')}.csv");
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
