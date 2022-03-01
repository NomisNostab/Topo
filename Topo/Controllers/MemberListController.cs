using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Topo.Services;
using Topo.Models.MemberList;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Topo.Controllers
{
    public class MemberListController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;

        public MemberListController(StorageService storageService, IMemberListService memberListService)
        {
            _storageService = storageService;
            _memberListService = memberListService;
        }

        private void SetViewBag()
        {
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            ViewBag.Unit = _storageService.SelectedUnitName;
        }

        public async Task<ActionResult> Index()
        {
            MemberListViewModel model = await SetUpViewModel();
            SetViewBag();
            return View(model);
        }

        private async Task<MemberListViewModel> SetUpViewModel()
        {
            var model = new MemberListViewModel();
            model.Units = new List<SelectListItem>();
            if (_storageService.Units != null)
                model.Units = _storageService.Units;
            if (_storageService.SelectedUnitId != null)
            {
                model.SelectedUnitId = _storageService.SelectedUnitId;
                var allMembers = await _memberListService.GetMembersAsync();
                model.Members = allMembers.Where(m => m.unit_order == 0).OrderBy(m => m.first_name).ThenBy(m => m.last_name).ToList();
            }
            if (_storageService.SelectedUnitName != null)
                model.SelectedUnitName = _storageService.SelectedUnitName;
            return model;
        }

        [HttpPost]
        public async Task<ActionResult> Index(MemberListViewModel memberListViewModel)
        {
            var model = new MemberListViewModel();
            if (ModelState.IsValid)
            {
                _storageService.SelectedUnitId = memberListViewModel.SelectedUnitId;
                if (_storageService.Units != null)
                    _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == memberListViewModel.SelectedUnitId)?.SingleOrDefault()?.Text;
                model = await SetUpViewModel();
            }
            else
            {
                model = await SetUpViewModel();
            }
            SetViewBag();
            return View(model);
        }


        public async Task<ActionResult> PatrolList()
        {
            var model = await _memberListService.GetMembersAsync();
            var sortedPatrolList = model.Where(m => m.unit_order == 0).OrderBy(m => m.patrol_name).ToList();
            var patrolListReport = new Report();
            var directory = Directory.GetCurrentDirectory();
            patrolListReport.Load($@"{directory}\Reports\Patrols.frx");
            patrolListReport.SetParameterValue("ReportDate", DateTime.Now.ToShortDateString());
            patrolListReport.RegisterData(sortedPatrolList, "Members");

            if (patrolListReport.Prepare())
            {
                // Set PDF export props
                PDFSimpleExport pdfExport = new PDFSimpleExport();
                pdfExport.ShowProgress = false;

                MemoryStream strm = new MemoryStream();
                patrolListReport.Report.Export(pdfExport, strm);
                patrolListReport.Dispose();
                pdfExport.Dispose();
                strm.Position = 0;

                // return stream in browser
                return File(strm, "application/pdf", "PatrolList.pdf");
            }
            else
            {
                SetViewBag();
                return View();
            }
        }

        public async Task<ActionResult> PatrolSheet()
        {
            var model = await _memberListService.GetMembersAsync();
            var sortedPatrolList = model.Where(m => m.unit_order == 0).OrderBy(m => m.patrol_name).ToList();
            var patrolListReport = new Report();
            var directory = Directory.GetCurrentDirectory();
            patrolListReport.Load($@"{directory}\Reports\PatrolSheets.frx");
            patrolListReport.SetParameterValue("ReportDate", DateTime.Now.ToShortDateString());
            patrolListReport.RegisterData(sortedPatrolList, "Members");

            if (patrolListReport.Prepare())
            {
                // Set PDF export props
                PDFSimpleExport pdfExport = new PDFSimpleExport();
                pdfExport.ShowProgress = false;

                MemoryStream strm = new MemoryStream();
                patrolListReport.Report.Export(pdfExport, strm);
                patrolListReport.Dispose();
                pdfExport.Dispose();
                strm.Position = 0;

                // return stream in browser
                return File(strm, "application/pdf", "PatrolSheets.pdf");
            }
            else
            {
                SetViewBag();
                return View();
            }
        }
    }
}

//Build report template for model
//report1.Dictionary.RegisterBusinessObject(
//        new List<MemberListModel>(),
//        "Members",
//        2,
//        true
//    );
//report1.Save(@"C:\Users\simon\Documents\SignInSheet2.frx");
