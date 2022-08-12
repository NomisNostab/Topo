using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using Topo.Models.MemberList;
using Topo.Models.ReportGeneration;
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
            var serialiSedsortedMemberList = JsonSerializer.Serialize(sortedMemberList);
            var reportGenerationRequest = new ReportGenerationRequest()
            {
                ReportType = ReportType.MemberList,
                GroupName = groupName,
                Section = section,
                UnitName = unitName,
                OutputType = OutputType.PDF,
                ReportData = serialiSedsortedMemberList
            };
            var serialisedReportGenerationRequest = JsonSerializer.Serialize(reportGenerationRequest);

            var workbook = _reportService.GenerateMemberListWorkbook(sortedMemberList, groupName, section, unitName);

            MemoryStream strm = new MemoryStream();
            workbook.Version = ExcelVersion.Excel2016;

            if (outputType == Topo.Constants.OutputType.pdf)
            {
                //Convert Base64String into PDF document
                var document = "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";
                byte[] bytes = Convert.FromBase64String(document);
                // return stream in browser
                return File(bytes, "application/pdf", $"Members_{unitName.Replace(' ', '_')}.pdf");


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


