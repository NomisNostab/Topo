using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            if (ModelState.IsValid)
            {
                _storageService.SelectedUnitId = memberListViewModel.SelectedUnitId;
                if (_storageService.Units != null)
                    _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == memberListViewModel.SelectedUnitId)?.FirstOrDefault()?.Text;
                model = await SetUpViewModel();
                model.Approvals = model.Approvals?.Where(a => a.submission_date >= memberListViewModel.ApprovalSearchFromDate && a.submission_date <= memberListViewModel.ApprovalSearchToDate).ToList() ?? new List<ApprovalsListModel>();
                ViewBag.dataSource = model.Approvals.ToList();
                if (!string.IsNullOrEmpty(button))
                {
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
