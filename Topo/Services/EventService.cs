using Topo.Models.Events;

namespace Topo.Services
{
    public interface IEventService
    {
        public Task<List<CalendarListModel>> GetCalendars();
        public Task SetCalendar(string calendarId);
        public Task<List<EventListModel>> GetEventsForDates(DateTime fromDate, DateTime toDate);
        public Task<EventListModel> GetAttendanceForEvent(string eventId);
    }

    public class EventService : IEventService
    {
        private readonly StorageService _storageService;
        private readonly ITerrainAPIService _terrainAPIService;

        public EventService(StorageService storageService, ITerrainAPIService terrainAPIService)
        {
            _storageService = storageService;
            _terrainAPIService = terrainAPIService;
        }

        public async Task<List<CalendarListModel>> GetCalendars()
        {
            var getCalendarsResultModel = await _terrainAPIService.GetCalendarsAsync();
            if (getCalendarsResultModel != null && getCalendarsResultModel.own_calendars != null)
            {
                var calendars = getCalendarsResultModel.own_calendars.Where(c => c.type == "unit")
                    .Select(e => new CalendarListModel()
                {
                    Id = e.id,
                    Title = e.title
                })
                    .ToList();
                _storageService.GetCalendarsResult = getCalendarsResultModel;
                return calendars;
            }
            return new List<CalendarListModel>();
        }

        public async Task SetCalendar(string calendarId)
        {
            foreach (var calendar in _storageService.GetCalendarsResult.own_calendars)
            {
                calendar.selected = calendar.id == calendarId;
            }
            await _terrainAPIService.PutCalendarsAsync(_storageService.GetCalendarsResult);
        }

        public async Task<List<EventListModel>> GetEventsForDates(DateTime fromDate, DateTime toDate)
        {
            var getEventsResultModel = await _terrainAPIService.GetEventsAsync(fromDate, toDate);
            if (getEventsResultModel != null && getEventsResultModel.results != null)
            {
                var events = getEventsResultModel.results.Select(e => new EventListModel()
                {
                    Id = e.id,
                    EventName = e.title,
                    StartDateTime = e.start_datetime,
                    EndDateTime = e.end_datetime
                })
                    .ToList();
                return events;
            }
            return new List<EventListModel>();
        }

        public async Task<EventListModel> GetAttendanceForEvent(string eventId)
        {
            var eventListModel = new EventListModel();
            var getEventResultModel = await _terrainAPIService.GetEventAsync(eventId);
            if (getEventResultModel != null && getEventResultModel.attendance != null && getEventResultModel.attendance.attendee_members != null)
            {
                eventListModel.Id = eventId;
                eventListModel.EventName = getEventResultModel.title;
                eventListModel.StartDateTime = getEventResultModel.start_datetime;
                eventListModel.attendees = getEventResultModel.attendance.attendee_members;
                return eventListModel;
            }
            return new EventListModel();
        }

    }
}
