using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.Pdf;
using Syncfusion.XlsIORenderer;
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
        private IReportService _reportService;
        private ILogger<LogbookController> _logger;

        public LogbookController(StorageService storageService,
                                    IMemberListService memberListService, 
                                    ILogbookService logbookService, 
                                    ILogger<LogbookController> logger,
                                    IReportService reportService)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _logbookService = logbookService;
            _logger = logger;
            _reportService = reportService;
        }

        private async Task<LogbookListViewModel> SetUpViewModel(bool includeLeaders = false)
        {
            var model = new LogbookListViewModel();
            model.Units = new List<SelectListItem>();
            if (_storageService.Units != null)
                model.Units = _storageService.Units;
            if (_storageService.SelectedUnitId != null)
            {
                model.SelectedUnitId = _storageService.SelectedUnitId;
                var allMembers = await _memberListService.GetMembersAsync(_storageService.SelectedUnitId);
                var members = allMembers.Where(m => includeLeaders || m.isAdultLeader == 0).OrderBy(m => m.first_name).ThenBy(m => m.last_name).ToList();
                foreach (var member in members)
                {
                    var editorViewModel = new MemberListEditorViewModel
                    {
                        id = member.id,
                        first_name = member.first_name,
                        last_name = member.last_name,
                        member_number = member.member_number,
                        patrol_name = string.IsNullOrEmpty(member.patrol_name) ? "-" : member.patrol_name,
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

        public async Task<ActionResult> Index(bool includeLeaders = false)
        {
            var model = await SetUpViewModel(includeLeaders);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(LogbookListViewModel logbookListViewModel, string button)
        {
            if (string.IsNullOrEmpty(logbookListViewModel.SelectedUnitId) || _storageService.SelectedUnitId != logbookListViewModel.SelectedUnitId)
            {
                _storageService.SelectedUnitId = logbookListViewModel.SelectedUnitId;
                return RedirectToAction("Index", "Logbook", new { includeLeaders = logbookListViewModel.IncludeLeaders });
            }

            var model = new LogbookListViewModel();
            if (!string.IsNullOrEmpty(logbookListViewModel.SelectedUnitId))
            {
                _storageService.SelectedUnitId = logbookListViewModel.SelectedUnitId;
                if (!string.IsNullOrEmpty(button))
                {
                    var selectedMembers = logbookListViewModel.getSelectedMembers();
                    if(selectedMembers == null)
                    {
                        _logger.LogInformation("selectedMembers: null");
                        selectedMembers = new List<string>();
                    }
                    // No members selected, default to all
                    if (selectedMembers.Count() == 0)
                    {
                        selectedMembers = logbookListViewModel.Members.Select(m => m.id).ToList();
                    }
                    if (selectedMembers != null)
                    {
                        _logger.LogInformation($"selectedMembers.Count: {selectedMembers.Count()}");
                        var memberKVP = new List<KeyValuePair<string, string>>();
                        foreach (var member in selectedMembers)
                        {
                            var memberName = logbookListViewModel.Members.Where(m => m.id == member).Select(m => m.first_name + " " + m.last_name).FirstOrDefault();
                            memberKVP.Add(new KeyValuePair<string, string>(member, memberName ?? ""));
                        }
                        _logger.LogInformation($"memberKVP.Count: {memberKVP.Count()}");

                        Constants.OutputType outputType;
                        if (button == "LogbookReportPdf")
                            outputType = Constants.OutputType.pdf;
                        else
                            outputType = Constants.OutputType.xlsx;

                        var logbookEntries = await _logbookService.GenerateLogbookData(memberKVP);
                        var groupName = _storageService.GroupName;
                        var unitName = _storageService.SelectedUnitName ?? "";
                        var section = _storageService.SelectedSection;
                        var workbook = _reportService.GenerateLogbookWorkbook(logbookEntries, groupName, section, unitName, outputType == Constants.OutputType.pdf);

                        //Stream 
                        MemoryStream strm = new MemoryStream();

                        if (outputType == Constants.OutputType.xlsx)
                        {
                            //Stream as Excel file
                            workbook.SaveAs(strm);

                            // return stream in browser
                            return File(strm.ToArray(), "application/vnd.ms-excel", $"Logbook_Report_{unitName.Replace(' ', '_')}.xlsx");
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
                            return File(strm.ToArray(), "application/pdf", $"Logbook_Report_{unitName.Replace(' ', '_')}.pdf");
                        }
                    }
                }
            }
            return RedirectToAction("Index", "Logbook", new { includeLeaders = logbookListViewModel.IncludeLeaders });
        }

    }
}
