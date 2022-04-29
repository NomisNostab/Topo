using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using Topo.Models;
using Topo.Models.Home;
using Topo.Models.Events;
using Topo.Services;
using FastReport;
using FastReport.Export.PdfSimple;
using System.Text;
using System.Reflection;

namespace Topo.Controllers
{
    public class EventsController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly IEventService _eventService;

        public EventsController(StorageService storageService, IMemberListService memberListService, IEventService eventService)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _eventService = eventService;
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

            SetViewBag();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index([Bind("SelectedCalendar, SelectedEvent, CalendarSearchDate")] EventsListViewModel eventViewModel)
        {
            var viewModel = new EventsListViewModel();
            viewModel.Events = new List<EventListModel>();
            if (ModelState.IsValid)
            {
                var calendarTitle = _storageService.Calendars.FirstOrDefault(c => c.Value == eventViewModel.SelectedCalendar).Text;
                var selectedUnit = _storageService.Units.Where(u => u.Text == calendarTitle)?.FirstOrDefault();
                _storageService.SelectedUnitName = selectedUnit.Text;
                _storageService.SelectedUnitId = selectedUnit.Value;
                await _eventService.SetCalendar(eventViewModel.SelectedCalendar);
                var events = await _eventService.GetEventsForDates(eventViewModel.CalendarSearchDate.AddMonths(-1), eventViewModel.CalendarSearchDate.AddMonths(1));
                viewModel.Events = events;
                _storageService.Events = events.Select(e => new SelectListItem { Text = e.EventDisplay, Value = e.Id }).ToList();
            }
            viewModel.Calendars = _storageService.Calendars;
            viewModel.SelectedCalendar = eventViewModel.SelectedCalendar;
            viewModel.CalendarSearchDate = eventViewModel.CalendarSearchDate;

            SetViewBag();
            return View(viewModel);
        }

        public async Task<ActionResult> SignInSheet(string eventId)
        {
            var viewModel = new EventsListViewModel();
            var selectedEvent = _storageService.Events?.Where(e => e.Value == eventId).FirstOrDefault().Text;
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var model = await _memberListService.GetMembersAsync();
            var signInSheetReport = new Report();
            var directory = Directory.GetCurrentDirectory();
            signInSheetReport.Load($@"{directory}/Reports/SignInSheet.frx");
            signInSheetReport.SetParameterValue("GroupName", groupName);
            signInSheetReport.SetParameterValue("UnitName", unitName);
            signInSheetReport.SetParameterValue("Event", selectedEvent);
            signInSheetReport.RegisterData(model, "Members");

            if (signInSheetReport.Prepare())
            {
                // Set PDF export props
                PDFSimpleExport pdfExport = new PDFSimpleExport();
                pdfExport.ShowProgress = false;

                MemoryStream strm = new MemoryStream();
                signInSheetReport.Report.Export(pdfExport, strm);
                signInSheetReport.Dispose();
                pdfExport.Dispose();
                strm.Position = 0;

                // return stream in browser
                return File(strm, "application/pdf", $"SignInSheet_{unitName.Replace(' ', '_')}_{selectedEvent.Replace(' ', '_')}.pdf");
            }
            else
            {
                SetViewBag();
                return View();
            }
        }

        public async Task<ActionResult> AttendanceList(string eventId)
        {
            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";

            var eventListModel = await _eventService.GetAttendanceForEvent(eventId);

            MemoryStream ms = new MemoryStream();
            // Encoding.UTF8 produces stream with BOM, new UTF8Encoding(false) - without BOM
            using (StreamWriter sw = new StreamWriter(ms, new UTF8Encoding(false), 8192, true))
            {
                sw.WriteLine(groupName);
                sw.WriteLine(unitName);
                sw.WriteLine(eventListModel.EventDisplay);
                sw.WriteLine();
                PropertyInfo[] properties = typeof(Attendee_Members).GetProperties();
                sw.WriteLine(string.Join(",", properties.Where(x => x.Name != "id").Select(x => x.Name)));

                foreach (Attendee_Members attendee in eventListModel.attendees.OrderBy(a => a.last_name))
                {
                    sw.WriteLine(string.Join(",", properties.Where(x => x.Name != "id").Select(prop => prop.GetValue(attendee))));
                }
            }
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", $"Attendance_{unitName.Replace(' ', '_')}_{eventListModel.EventDisplay.Replace(' ', '_')}.csv");
        }

    }
}
