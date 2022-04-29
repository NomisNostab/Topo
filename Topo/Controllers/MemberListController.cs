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
                model.Members = allMembers.Where(m => m.isAdultLeader == 0).OrderBy(m => m.first_name).ThenBy(m => m.last_name).ToList();
            }
            if (_storageService.SelectedUnitName != null)
                model.SelectedUnitName = _storageService.SelectedUnitName;
            SetViewBag();
            return model;
        }

        [HttpPost]
        public async Task<ActionResult> Index(MemberListViewModel memberListViewModel, string button)
        {
            var model = new MemberListViewModel();
            ModelState.Remove("button");
            if (ModelState.IsValid)
            {
                _storageService.SelectedUnitId = memberListViewModel.SelectedUnitId;
                if (_storageService.Units != null)
                    _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == memberListViewModel.SelectedUnitId)?.FirstOrDefault()?.Text;
                model = await SetUpViewModel();
                if (!string.IsNullOrEmpty(button))
                {
                    switch (button)
                    {
                        case "PatrolList":
                            return await GeneratePatrolReport("Patrols", "Patrol_List", memberListViewModel.IncludeLeaders);
                        case "PatrolSheet":
                            return await GeneratePatrolReport("PatrolSheets", "Patrol_Sheets");
                    }
                }
            }
            else
            {
                model = await SetUpViewModel();
            }
            return View(model);
        }

        private async Task<ActionResult> GeneratePatrolReport(string reportDefinitionName, string reportDownloadName, bool includeLeaders = false)
        {
            var model = await _memberListService.GetMembersAsync();
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var sortedPatrolList = new List<MemberListModel>();
            if (includeLeaders)
                sortedPatrolList = model.OrderBy(m => m.patrol_name).ToList();
            else
                sortedPatrolList = model.Where(m => m.isAdultLeader == 0).OrderBy(m => m.patrol_name).ToList();
            var patrolListReport = new Report();
            var directory = Directory.GetCurrentDirectory();
            patrolListReport.Load($@"{directory}/Reports/{reportDefinitionName}.frx");
            patrolListReport.SetParameterValue("GroupName", groupName);
            patrolListReport.SetParameterValue("UnitName", unitName);
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
                return File(strm, "application/pdf", $"{reportDownloadName}_{unitName.Replace(' ', '_')}.pdf");
            }
            else
            {
                SetViewBag();
                return View();
            }
        }
    }
}


