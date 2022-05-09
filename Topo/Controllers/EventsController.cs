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
                if (button == "AttendanceReport")
                {
                    return await AttendanceReport(eventViewModel.CalendarSearchFromDate, eventViewModel.CalendarSearchToDate, eventViewModel.SelectedCalendar);
                }
                if (button == "AttendanceCSV")
                {
                    return await AttendanceCSV(eventViewModel.CalendarSearchFromDate, eventViewModel.CalendarSearchToDate, eventViewModel.SelectedCalendar);
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

        private async Task<ActionResult> AttendanceReport(DateTime fromDate, DateTime toDate, string selectedCalendar)
        {
            var attendanceReportData = await _eventService.GenerateAttendanceReportData(fromDate, toDate, selectedCalendar);

            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var report = new Report();
            var directory = Directory.GetCurrentDirectory();
            report.Load($@"{directory}/Reports/Attendance.frx");
            report.SetParameterValue("GroupName", groupName);
            report.SetParameterValue("UnitName", unitName);
            report.SetParameterValue("FromDate", fromDate.ToShortDateString());
            report.SetParameterValue("ToDate", toDate.ToShortDateString());
            report.SetParameterValue("TotalEvents", attendanceReportData.attendanceReportChallengeAreaSummaries.FirstOrDefault()?.TotalEvents ?? 0);
            report.SetParameterValue("CommunityEvents", attendanceReportData.attendanceReportChallengeAreaSummaries.Where(c => c.ChallengeArea == "Community").FirstOrDefault()?.EventCount ?? 0);
            report.SetParameterValue("CreativeEvents", attendanceReportData.attendanceReportChallengeAreaSummaries.Where(c => c.ChallengeArea == "Creative").FirstOrDefault()?.EventCount ?? 0);
            report.SetParameterValue("OutdoorsEvents", attendanceReportData.attendanceReportChallengeAreaSummaries.Where(c => c.ChallengeArea == "Outdoors").FirstOrDefault()?.EventCount ?? 0);
            report.SetParameterValue("PersonalGrowthEvents", attendanceReportData.attendanceReportChallengeAreaSummaries.Where(c => c.ChallengeArea == "Personal Growth").FirstOrDefault()?.EventCount ?? 0);
            report.RegisterData(attendanceReportData.attendanceReportItems.ToList(), "MemberAttendance");


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
                return File(strm, "application/pdf", $"Attendance_{unitName.Replace(' ', '_')}.pdf");
            }
            else
            {
                SetViewBag();
                return View();
            }
        }

        private async Task<ActionResult> AttendanceCSV(DateTime fromDate, DateTime toDate, string selectedCalendar)
        {

            var attendanceReportData = await _eventService.GenerateAttendanceReportData(fromDate, toDate, selectedCalendar);

            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var groupedAttendances = attendanceReportData.attendanceReportItems.GroupBy(wa => $"{wa.IsAdultMember} {wa.MemberName}").ToList();
            var quote = '"';
            MemoryStream ms = new MemoryStream();
            // Encoding.UTF8 produces stream with BOM, new UTF8Encoding(false) - without BOM
            using (StreamWriter sw = new StreamWriter(ms, new UTF8Encoding(false), 8192, true))
            {
                sw.WriteLine(groupName);
                sw.WriteLine(unitName);
                sw.WriteLine($"Attendance from {fromDate.ToShortDateString()} to {toDate.ToShortDateString()}");
                sw.WriteLine();
                sw.Write(",,");
                sw.WriteLine(string.Join(",", groupedAttendances.FirstOrDefault().Select(a => a.EventChallengeArea)));
                sw.Write("Adult/Youth,Scout,");
                sw.WriteLine(string.Join(",", groupedAttendances.FirstOrDefault().Select(a => a.EventName)));

                foreach (var groupedAttendance in groupedAttendances.OrderBy(ga => ga.Key))
                {
                    var member = groupedAttendance.FirstOrDefault()?.MemberName ?? "";
                    var isAdult = groupedAttendance.FirstOrDefault()?.IsAdultMember ?? 0;
                    sw.Write($"{isAdult},{member},");
                    sw.WriteLine(string.Join(", ", groupedAttendance.OrderBy(ga => ga.EventStartDate).Select(a => a.Attended)));
                }
            }
            ms.Position = 0;
            return File(ms, "application/vnd.ms-excel", $"Attendance{unitName.Replace(' ', '_')}.csv");
        }

    }
}

////Build report template for model
//var reportDS = new Report();
//var directoryDS = Directory.GetCurrentDirectory();
//reportDS.Dictionary.RegisterBusinessObject(
//        new List<AttendanceReportItemModel>(),
//        "MemberAttendance",
//        2,
//        true
//    );
//reportDS.Dictionary.RegisterBusinessObject(
//        new List<AttendanceReportMemberSummaryModel>(),
//        "MemberSummaries",
//        2,
//        true
//    );
//reportDS.Dictionary.RegisterBusinessObject(
//        new List<AttendanceReportChallengeAreaSummaryModel>(),
//        "ChallengeSummaries",
//        2,
//        true
//    );
//reportDS.Save(@$"{directoryDS}/Reports/AttendanceSummary.frx");
