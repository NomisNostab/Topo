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

        private async Task<SIAIndexViewModel> SetUpViewModel()
        {
            var model = new SIAIndexViewModel();
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

        // GET: SIAController
        public async Task<ActionResult> Index()
        {
            var model = await SetUpViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(SIAIndexViewModel siaIndexViewModel, string button)
        {
            var model = new SIAIndexViewModel();
            ModelState.Remove("button");
            if (ModelState.IsValid)
            {
                if (_storageService.SelectedUnitId != siaIndexViewModel.SelectedUnitId)
                {
                    _storageService.SelectedUnitId = siaIndexViewModel.SelectedUnitId;
                    return RedirectToAction("Index", "SIA");
                }
                _storageService.SelectedUnitId = siaIndexViewModel.SelectedUnitId;
                model = await SetUpViewModel();
                if (!string.IsNullOrEmpty(button))
                {
                    var selectedMembers = siaIndexViewModel.getSelectedMembers();
                    if (selectedMembers != null)
                    {
                        var memberKVP = new List<KeyValuePair<string, string>>();
                        foreach (var member in selectedMembers)
                        {
                            var memberName = siaIndexViewModel.Members.Where(m => m.id == member).Select(m => m.first_name + " " + m.last_name).FirstOrDefault();
                            memberKVP.Add(new KeyValuePair<string, string>(member, memberName ?? ""));
                        }
                        var report = await _SIAService.GenerateSIAReport(memberKVP);
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
                            return View(model);
                        }
                    }
                }
            }
            model = await SetUpViewModel();
            return View(model);
        }

        //public async Task<ActionResult> SIAReport()
        //{
        //    var report = await _SIAService.GenerateSIAReport();
        //    if (report.Prepare())
        //    {
        //        // Set PDF export props
        //        PDFSimpleExport pdfExport = new PDFSimpleExport();
        //        pdfExport.ShowProgress = false;

        //        MemoryStream strm = new MemoryStream();
        //        report.Report.Export(pdfExport, strm);
        //        report.Dispose();
        //        pdfExport.Dispose();
        //        strm.Position = 0;

        //        // return stream in browser
        //        var unitName = _storageService.SelectedUnitName ?? "";
        //        return File(strm, "application/pdf", $"SIA_Projects_{unitName.Replace(' ', '_')}.pdf");
        //    }
        //    else
        //    {
        //        var model = await SetUpViewModel();
        //        SetViewBag();
        //        return View(model);
        //    }
        //}
    }
}
