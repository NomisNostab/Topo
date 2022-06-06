using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using Syncfusion.XlsIORenderer;
using Topo.Models.Events;
using Topo.Services;

namespace Topo.Controllers
{
    public class EventsController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly IEventService _eventService;
        private readonly IReportService _reportService;
        public EventsController(StorageService storageService, IMemberListService memberListService, IEventService eventService, IReportService reportService)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _eventService = eventService;
            _reportService = reportService;
        }
        private void SetViewBag()
        {
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            ViewBag.Unit = _storageService.SelectedUnitName;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new EventsListViewModel();
            var calendars = await _eventService.GetCalendars();
            _storageService.Calendars = calendars.Select(e => new SelectListItem { Text = e.Title, Value = e.Id }).ToList();
            viewModel.Calendars = _storageService.Calendars;
            if (!string.IsNullOrEmpty(_storageService.SelectedUnitName))
            {
                viewModel.SelectedCalendar = _storageService.Calendars.Where(c => c.Text == _storageService.SelectedUnitName).FirstOrDefault()?.Value ?? "";
            }
            viewModel.CalendarSearchFromDate = DateTime.Now;
            viewModel.CalendarSearchToDate = DateTime.Now.AddMonths(4);

            SetViewBag();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index([Bind("SelectedCalendar, SelectedEvent, CalendarSearchFromDate, CalendarSearchToDate")] EventsListViewModel eventViewModel, string button)
        {
            var viewModel = new EventsListViewModel();
            viewModel.Events = new List<EventListModel>();
            if (eventViewModel.CalendarSearchFromDate != null && eventViewModel.CalendarSearchToDate != null && eventViewModel.CalendarSearchFromDate > eventViewModel.CalendarSearchToDate)
            {
                ModelState.AddModelError("CalendarSearchToDate", "The to date must be after the from date");
            }
            ModelState.Remove("button");
            if (ModelState.IsValid)
            {
                var calendarTitle = _storageService.Calendars.FirstOrDefault(c => c.Value == eventViewModel.SelectedCalendar).Text;
                var selectedUnit = _storageService.Units.Where(u => u.Text == calendarTitle)?.FirstOrDefault();
                _storageService.SelectedUnitName = selectedUnit.Text;
                _storageService.SelectedUnitId = selectedUnit.Value;
                if (button == "AttendanceReportPdf")
                {
                    return await AttendanceReport(eventViewModel.CalendarSearchFromDate, eventViewModel.CalendarSearchToDate, eventViewModel.SelectedCalendar, Topo.Constants.OutputType.pdf);
                }
                if (button == "AttendanceReportXlsx")
                {
                    return await AttendanceReport(eventViewModel.CalendarSearchFromDate, eventViewModel.CalendarSearchToDate, eventViewModel.SelectedCalendar, Topo.Constants.OutputType.xlsx);
                }
                await _eventService.SetCalendar(eventViewModel.SelectedCalendar);
                var events = await _eventService.GetEventsForDates(eventViewModel.CalendarSearchFromDate, eventViewModel.CalendarSearchToDate);
                viewModel.Events = events;
                _storageService.Events = events.Select(e => new SelectListItem { Text = e.EventDisplay, Value = e.Id }).ToList();
            }
            viewModel.Calendars = _storageService.Calendars;
            viewModel.SelectedCalendar = eventViewModel.SelectedCalendar;
            viewModel.CalendarSearchFromDate = eventViewModel.CalendarSearchFromDate;

            SetViewBag();
            return View(viewModel);
        }

        public async Task<ActionResult> SignInSheet(string eventId)
        {
            var viewModel = new EventsListViewModel();
            var selectedEvent = _storageService.Events?.Where(e => e.Value == eventId).FirstOrDefault().Text;
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var unit = _storageService.GetProfilesResult.profiles.FirstOrDefault(u => u.unit.name == unitName);
            if (unit == null)
                throw new IndexOutOfRangeException($"No unit found with name {unitName}. You may not have permissions to this section");
            var section = unit.unit.section;

            var model = await _memberListService.GetMembersAsync();

            var workbook = _reportService.GenerateSignInSheetWorkbook(model, groupName, section, unitName, selectedEvent);

            //Stream as Excel file
            MemoryStream strm = new MemoryStream();

            //Initialize XlsIO renderer.
            XlsIORenderer renderer = new XlsIORenderer();

            //Convert Excel document into PDF document 
            var sheet = workbook.Worksheets[0];
            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Portrait;
            PdfDocument pdfDocument = renderer.ConvertToPDF(sheet);
            pdfDocument.Save(strm);

            // return stream in browser
            return File(strm.ToArray(), "application/pdf", $"SignInSheet_{unitName.Replace(' ', '_')}_{selectedEvent.Replace(' ', '_')}.pdf");

        }

        public async Task<ActionResult> AttendanceList(string eventId)
        {
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var unit = _storageService.GetProfilesResult.profiles.FirstOrDefault(u => u.unit.name == unitName);
            if (unit == null)
                throw new IndexOutOfRangeException($"No unit found with name {unitName}. You may not have permissions to this section");
            var section = unit.unit.section;

            var eventListModel = await _eventService.GetAttendanceForEvent(eventId);
            var workbook = _reportService.GenerateEventAttendanceWorkbook(eventListModel, groupName, section, unitName);

            MemoryStream strm = new MemoryStream();
            //Stream as Excel file
            workbook.SaveAs(strm);

            // return stream in browser
            return File(strm.ToArray(), "application/vnd.ms-excel", $"Attendance_{unitName.Replace(' ', '_')}_{eventListModel.EventDisplay.Replace(' ', '_')}.xlsx");
        }

        private async Task<ActionResult> AttendanceReport(DateTime fromDate, DateTime toDate, string selectedCalendar, Topo.Constants.OutputType outputType)
        {
            var attendanceReportData = await _eventService.GenerateAttendanceReportData(fromDate, toDate, selectedCalendar);

            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var unit = _storageService.GetProfilesResult.profiles.FirstOrDefault(u => u.unit.name == unitName);
            if (unit == null)
                throw new IndexOutOfRangeException($"No unit found with name {unitName}. You may not have permissions to this section");
            var section = unit.unit.section;

            var workbook = _reportService.GenerateAttendanceReportWorkbook(attendanceReportData, groupName, section, unitName, fromDate, toDate, outputType == Constants.OutputType.pdf);

            //Stream as Excel file
            MemoryStream strm = new MemoryStream();

            if (outputType == Constants.OutputType.pdf)
            {
                //Initialize XlsIO renderer.
                XlsIORenderer renderer = new XlsIORenderer();

                //Convert Excel document into PDF document 
                var sheet = workbook.Worksheets[0];

                PdfDocument pdfDocument = renderer.ConvertToPDF(sheet);
                pdfDocument.Save(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/pdf", $"Attendance_{unitName.Replace(' ', '_')}.pdf");
            }

            if (outputType == Constants.OutputType.xlsx)
            {
                //Stream as Excel file
                workbook.SaveAs(strm);

                // return stream in browser
                return File(strm.ToArray(), "application/vnd.ms-excel", $"Attendance_{unitName.Replace(' ', '_')}.xlsx");
            }
            return View();
        }
    }
}
