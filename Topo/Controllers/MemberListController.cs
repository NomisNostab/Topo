using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using Topo.Models.MemberList;
using Topo.Services;

namespace Topo.Controllers
{
    public class MemberListController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly IReportService _reportService;

        public MemberListController(StorageService storageService, IMemberListService memberListService, IReportService reportService)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _reportService = reportService;
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
                var allMembers = await _memberListService.GetMembersAsync(_storageService.SelectedUnitId);
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
                        case "PatrolListPdf":
                            return await GeneratePatrolReport(memberListViewModel.IncludeLeaders, Topo.Constants.OutputType.pdf);
                        case "PatrolListXlsx":
                            return await GeneratePatrolReport(memberListViewModel.IncludeLeaders, Topo.Constants.OutputType.xlsx);
                        case "PatrolSheetPdf":
                            return await GeneratePatrolSheets(Topo.Constants.OutputType.pdf);
                        case "PatrolSheetXlsx":
                            return await GeneratePatrolSheets(Topo.Constants.OutputType.xlsx);
                        case "MemberListXlsx":
                            return await GenerateMemberList(Topo.Constants.OutputType.xlsx);
                        case "MemberListPdf":
                            return await GenerateMemberList(Topo.Constants.OutputType.pdf);
                    }
                }
            }
            else
            {
                model = await SetUpViewModel();
            }
            return View(model);
        }

        private async Task<ActionResult> GeneratePatrolReport(bool includeLeaders, Topo.Constants.OutputType outputType)
        {
            var model = await _memberListService.GetMembersAsync(_storageService.SelectedUnitId);
            var reportDownloadName = "Patrol_List";
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var unit = _storageService.GetProfilesResult.profiles.FirstOrDefault(u => u.unit.name == unitName);
            if (unit == null)
                throw new IndexOutOfRangeException($"No unit found with name {unitName}. You may not have permissions to this section");
            var section = unit.unit.section;
            var sortedPatrolList = new List<MemberListModel>();
            if (includeLeaders)
                sortedPatrolList = model.OrderBy(m => m.patrol_name).ToList();
            else
                sortedPatrolList = model.Where(m => m.isAdultLeader == 0).OrderBy(m => m.patrol_name).ToList();
            var workbook = _reportService.GeneratePatrolListWorkbook(sortedPatrolList, groupName, section, unitName, includeLeaders);

            MemoryStream strm = new MemoryStream();
            workbook.Version = ExcelVersion.Excel2016;

            if (outputType == Topo.Constants.OutputType.pdf)
            {
                //Stream as Excel file
                var sheet = workbook.Worksheets[0];
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;

                //Initialize XlsIO renderer.
                XlsIORenderer renderer = new XlsIORenderer();

                //Convert Excel document into PDF document 
                PdfDocument pdfDocument = renderer.ConvertToPDF(sheet);
                pdfDocument.Save(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/pdf", $"{reportDownloadName}_{unitName.Replace(' ', '_')}.pdf");
            }
            if (outputType == Topo.Constants.OutputType.xlsx)
            {
                //Stream as Excel file
                workbook.SaveAs(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/vnd.ms-excel", $"{reportDownloadName}_{unitName.Replace(' ', '_')}.xlsx");
            }

            var viewModel = await SetUpViewModel();
            return View(viewModel);
        }

        private async Task<ActionResult> GenerateMemberList(Topo.Constants.OutputType outputType)
        {
            var model = await _memberListService.GetMembersAsync(_storageService.SelectedUnitId);
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var unit = _storageService.GetProfilesResult.profiles.FirstOrDefault(u => u.unit.name == unitName);
            if (unit == null)
                throw new IndexOutOfRangeException($"No unit found with name {unitName}. You may not have permissions to this section");
            var section = unit.unit.section;
            var sortedMemberList = model.Where(m => m.isAdultLeader == 0).OrderBy(m => m.first_name).ThenBy(m => m.last_name).ToList();
            var workbook = _reportService.GenerateMemberListWorkbook(sortedMemberList, groupName, section, unitName);

            MemoryStream strm = new MemoryStream();
            workbook.Version = ExcelVersion.Excel2016;

            if (outputType == Topo.Constants.OutputType.pdf)
            {
                //Initialize XlsIO renderer.
                XlsIORenderer renderer = new XlsIORenderer();

                //Convert Excel document into PDF document 
                PdfDocument pdfDocument = renderer.ConvertToPDF(workbook);
                pdfDocument.Save(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/pdf", $"Members_{unitName.Replace(' ', '_')}.pdf");
            }

            if (outputType == Topo.Constants.OutputType.xlsx)
            {
                //Stream as Excel file
                workbook.SaveAs(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/vnd.ms-excel", $"Members_{unitName.Replace(' ', '_')}.xlsx");
            }

            var viewModel = await SetUpViewModel();
            return View(viewModel);

        }

        private async Task<ActionResult> GeneratePatrolSheets(Topo.Constants.OutputType outputType)
        {
            var model = await _memberListService.GetMembersAsync(_storageService.SelectedUnitId);
            var reportDownloadName = "Patrol_Sheets";
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var section = _storageService.SelectedSection;
            var sortedPatrolList = new List<MemberListModel>();
            sortedPatrolList = model.Where(m => m.isAdultLeader == 0).OrderBy(m => m.patrol_name).ToList();
            var workbook = _reportService.GeneratePatrolSheetsWorkbook(sortedPatrolList, section);

            MemoryStream strm = new MemoryStream();
            workbook.Version = ExcelVersion.Excel2016;

            if (outputType == Topo.Constants.OutputType.pdf)
            {
                //Initialize XlsIO renderer.
                XlsIORenderer renderer = new XlsIORenderer();

                //Convert Excel document into PDF document 
                PdfDocument pdfDocument = renderer.ConvertToPDF(workbook);
                pdfDocument.Save(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/pdf", $"{reportDownloadName}_{unitName.Replace(' ', '_')}.pdf");
            }

            if (outputType == Topo.Constants.OutputType.xlsx)
            {
                //Stream as Excel file
                workbook.SaveAs(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/vnd.ms-excel", $"{reportDownloadName}_{unitName.Replace(' ', '_')}.xlsx");
            }


            var viewModel = await SetUpViewModel();
            return View(viewModel);
        }

    }
}


