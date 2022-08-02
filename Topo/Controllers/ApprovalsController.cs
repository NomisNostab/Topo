using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.Pdf;
using Syncfusion.XlsIORenderer;
using System.Text.Json;
using Topo.Models.Approvals;
using Topo.Services;

namespace Topo.Controllers
{
    public class ApprovalsController : Controller
    {

        private readonly StorageService _storageService;
        private readonly IApprovalsService _approvalsService;
        private readonly IReportService _reportService;

        public ApprovalsController(StorageService storageService, IApprovalsService approvalsService, IReportService reportService)
        {
            _storageService = storageService;
            _reportService = reportService;
            _approvalsService = approvalsService;
        }
        private async Task<ApprovalsListViewModel> SetUpViewModel()
        {
            var model = new ApprovalsListViewModel();
            model.Units = new List<SelectListItem>();
            model.Approvals = new List<ApprovalsListModel>();
            if (_storageService.Units != null)
                model.Units = _storageService.Units;
            if (_storageService.SelectedUnitId != null)
            {
                model.SelectedUnitId = _storageService.SelectedUnitId;
                model.Approvals = await _approvalsService.GetApprovalListItems(_storageService.SelectedUnitId);
                model.SelectedGroupingColumn = "achievement_name";
            }
            if (_storageService.SelectedUnitName != null)
                model.SelectedUnitName = _storageService.SelectedUnitName;
            SetViewBag();
            return model;
        }

        private void SetViewBag()
        {
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            ViewBag.Unit = _storageService.SelectedUnitName;
            ViewBag.dpParams = new { @params = new { format = "dd/MM/yyyy" } };
        }

        public async Task<ActionResult> Index()
        {
            ApprovalsListViewModel model = await SetUpViewModel();
            model.ApprovalSearchFromDate = DateTime.Now.AddMonths(-2);
            model.ApprovalSearchToDate = DateTime.Now;
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(ApprovalsListViewModel memberListViewModel, string button)
        {
            var model = new ApprovalsListViewModel();
            ModelState.Remove("button");
            ModelState.Remove("SelectedMembers");
            ModelState.Remove("SelectedMembersOperator");
            ModelState.Remove("SelectedGroupingColumn");
            if (ModelState.IsValid)
            {
                _storageService.SelectedUnitId = memberListViewModel.SelectedUnitId;
                if (_storageService.Units != null)
                    _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == memberListViewModel.SelectedUnitId)?.FirstOrDefault()?.Text;
                model = await SetUpViewModel();
                model.ToBePresented = memberListViewModel.ToBePresented;
                model.IsPresented = memberListViewModel.IsPresented;
                model.Approvals = model.Approvals?.Where(a => a.submission_date >= memberListViewModel.ApprovalSearchFromDate && a.submission_date <= memberListViewModel.ApprovalSearchToDate.AddDays(1)).ToList() ?? new List<ApprovalsListModel>();
                if (memberListViewModel.ToBePresented)
                    model.Approvals = model.Approvals.Where(a => !string.IsNullOrEmpty(a.submission_outcome) && !a.presented_date.HasValue).ToList();
                if (memberListViewModel.IsPresented)
                    model.Approvals = model.Approvals.Where(a => a.presented_date.HasValue && a.presented_date != a.awarded_date).ToList();
                ViewBag.dataSource = model.Approvals.ToList();
                if (!string.IsNullOrEmpty(button))
                {
                    var selectedMembers = (memberListViewModel.SelectedMembers ?? "").Split(",");
                    var selectedMembersOperator = memberListViewModel.SelectedMembersOperator ?? "";
                    var selectedApprovals = new List<ApprovalsListModel>();
                    if (selectedMembersOperator == "equal")
                        selectedApprovals = model.Approvals.Where(t2 => selectedMembers.Count(m => m == t2.member_display_name) != 0).ToList();
                    else
                        selectedApprovals = model.Approvals.Where(t2 => selectedMembers.Count(m => m == t2.member_display_name) == 0).ToList();

                    Constants.OutputType outputType;
                    if (button == "ApprovalsListPdf")
                        outputType = Constants.OutputType.pdf;
                    else
                        outputType = Constants.OutputType.xlsx;

                    var groupName = _storageService.GroupName;
                    var unitName = _storageService.SelectedUnitName ?? "";
                    var section = _storageService.SelectedSection;
                    var groupByMember = (memberListViewModel.SelectedGroupingColumn ?? "achievement_name") == "member_display_name";
                    var workbook = _reportService.GenerateApprovalsWorkbook(selectedApprovals, groupName, section, unitName, memberListViewModel.ApprovalSearchFromDate, memberListViewModel.ApprovalSearchToDate, groupByMember: groupByMember, forPdfOutput: outputType == Constants.OutputType.pdf);

                    //Stream 
                    MemoryStream strm = new MemoryStream();

                    if (outputType == Constants.OutputType.xlsx)
                    {
                        //Stream as Excel file
                        workbook.SaveAs(strm);

                        // return stream in browser
                        return File(strm.ToArray(), "application/vnd.ms-excel", $"Approvals_Report_{unitName.Replace(' ', '_')}.xlsx");
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
                        return File(strm.ToArray(), "application/pdf", $"Approvals_Report_{unitName.Replace(' ', '_')}.pdf");
                    }
                }
            }
            else
            {
                model = await SetUpViewModel();
            }
            return View(model);
        }

        public ActionResult Update([FromBody] CRUDModel<ApprovalsListModel> value)
        {
            var approval = value.value;
            _approvalsService.UpdateApproval(_storageService.SelectedUnitId, approval);
            return Json(value.value);
        }

        public class CRUDModel<T> where T : class
        {
            public T value { get; set; }
        }
    }
}
