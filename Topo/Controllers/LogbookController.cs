using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Topo.Models.Logbook;
using Topo.Models.MemberList;
using Topo.Services;

namespace Topo.Controllers
{
    public class LogbookController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private ILogbookService _logbookService;

        public LogbookController(StorageService storageService,
            IMemberListService memberListService, ILogbookService logbookService)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _logbookService = logbookService;
        }

        private async Task<LogbookListViewModel> SetUpViewModel()
        {
            var model = new LogbookListViewModel();
            model.Units = new List<SelectListItem>();
            if (_storageService.Units != null)
                model.Units = _storageService.Units;
            if (_storageService.SelectedUnitId != null)
            {
                model.SelectedUnitId = _storageService.SelectedUnitId;
                var allMembers = await _memberListService.GetMembersAsync();
                var members = allMembers.Where(m => m.isAdultLeader == 0).OrderBy(m => m.first_name).ThenBy(m => m.last_name).ToList();
                foreach (var member in members)
                {
                    var editorViewModel = new MemberListEditorViewModel
                    {
                        id = member.id,
                        first_name = member.first_name,
                        last_name = member.last_name,
                        member_number = member.member_number,
                        patrol_name = member.patrol_name,
                        patrol_duty = string.IsNullOrEmpty(member.patrol_duty) ? "-" : member.patrol_duty,
                        unit_council = member.unit_council,
                        selected = false
                    };
                    model.Members.Add(editorViewModel);
                }
            }
            if (_storageService.Units != null)
            {
                _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == _storageService.SelectedUnitId)?.FirstOrDefault()?.Text;
                model.SelectedUnitName = _storageService.SelectedUnitName ?? "";
            }
            SetViewBag();
            return model;
        }

        private void SetViewBag()
        {
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            ViewBag.Unit = _storageService.SelectedUnitName;
        }

        public async Task<ActionResult> Index()
        {
            var model = await SetUpViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(LogbookListViewModel logbookListViewModel, string button)
        {
            var model = new LogbookListViewModel();
            ModelState.Remove("button");
            if (ModelState.IsValid)
            {
                if (_storageService.SelectedUnitId != logbookListViewModel.SelectedUnitId)
                {
                    _storageService.SelectedUnitId = logbookListViewModel.SelectedUnitId;
                    return RedirectToAction("Index", "Logbook");
                }
                _storageService.SelectedUnitId = logbookListViewModel.SelectedUnitId;
                model = await SetUpViewModel();
                if (!string.IsNullOrEmpty(button))
                {
                    var selectedMembers = logbookListViewModel.getSelectedMembers();
                    if (selectedMembers != null)
                    {
                        var memberKVP = new List<KeyValuePair<string, string>>();
                        foreach (var member in selectedMembers)
                        {
                            var memberName = logbookListViewModel.Members.Where(m => m.id == member).Select(m => m.first_name + " " + m.last_name).FirstOrDefault();
                            memberKVP.Add(new KeyValuePair<string, string>(member, memberName ?? ""));
                        }
                        var report = await _logbookService.GenerateLogbookReport(memberKVP);
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
                            return File(strm, "application/pdf", $"Logbook_Report_{unitName.Replace(' ', '_')}.pdf");
                        }
                        else
                        {
                            return View(model);
                        }
                    }
                }
            }
            else
            {
                model = await SetUpViewModel();
            }
            return View(model);
        }

    }
}
