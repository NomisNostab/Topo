﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Topo.Models.Login;
using Topo.Models.Events;
using Topo.Models.OAS;
using Topo.Models.MemberList;
using Topo.Models.Wallchart;

namespace Topo.Services
{
    public class StorageService
    {
        public bool IsAuthenticated { get; set; }
        public string? MemberName { get; set; }
        public AuthenticationResult? AuthenticationResult { get; set; }
        public GetUserResultModel? GetUserResult { get; set; } = null;
        public GetProfilesResultModel? GetProfilesResult { get; set; }
        public List<SelectListItem>? Units { get; set; }
        public string? SelectedUnitName { get; set; }
        public string? SelectedUnitId { get; set; }
        public List<SelectListItem>? Events { get; set; }
        public List<SelectListItem>? Calendars { get; set; }
        public GetCalendarsResultModel? GetCalendarsResult { get; set;} = null;
        public DateTime TokenExpiry { get; set; }
        public List<OASStageListModel> OASStageList { get; set; } = new List<OASStageListModel>();
        public string GroupName { get; set; } = "";
        public List<KeyValuePair<string, List<MemberListModel>>> CachedMembers { get; set; } = new List<KeyValuePair<string, List<MemberListModel>>>();
        public List<KeyValuePair<string, List<WallchartItemModel>>> CachedWallchartItems { get; set; } = new List<KeyValuePair<string, List<WallchartItemModel>>>();
        public void ClearStorage()
        {
            IsAuthenticated = false;
            AuthenticationResult = null;
            MemberName = "";
            AuthenticationResult = null;
            GetUserResult = null;
            GetProfilesResult = null;
            Units = null;
            Events = null;
            Calendars = null;
            SelectedUnitName = null;
            SelectedUnitId = null;
            GetCalendarsResult = null;
            TokenExpiry = DateTime.MinValue;
            CachedMembers = new List<KeyValuePair<string, List<MemberListModel>>>();
            CachedWallchartItems = new List<KeyValuePair<string, List<WallchartItemModel>>>();
        }
    }
}
