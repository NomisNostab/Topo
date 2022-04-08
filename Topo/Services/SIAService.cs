﻿using FastReport;
using System.Globalization;
using Topo.Models.SIA;

namespace Topo.Services
{
    public interface ISIAService
    {
        public Task<Report> GenerateSIAReport();
    }

    public class SIAService : ISIAService
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly ITerrainAPIService _terrainAPIService;

        public SIAService(IMemberListService memberListService,
            ITerrainAPIService terrainAPIService, 
            StorageService storageService)
        {
            _memberListService = memberListService;
            _terrainAPIService = terrainAPIService;
            _storageService = storageService;
        }
        public async Task<Report> GenerateSIAReport()
        {

            var unit = _storageService.SelectedUnitName.ToLower().Replace(" unit", "");
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            var unitSiaProjects = new List<SIAProjectListModel>();
            var members = await _memberListService.GetMembersAsync();
            var sortedMemberList = members.Where(m => m.isAdultLeader == 0).OrderBy(m => m.first_name).ThenBy(m => m.last_name).ToList();
            foreach (var member in sortedMemberList)
            {
                var siaResultModel = await _terrainAPIService.GetSIAResultsForMember(member.id);
                var memberSiaProjects = siaResultModel.results.Where(r => r.section == unit)
                    .Select(r => new SIAProjectListModel
                    {
                        memberName = $"{member.first_name} {member.last_name}",
                        area = myTI.ToTitleCase(r.answers.special_interest_area_selection.Replace("sia_", "").Replace("_", " ")),
                        projectName = r.answers.project_name,
                        status = myTI.ToTitleCase(r.status.Replace("_", " ")),
                        statusUpdated = r.status_updated
                    })
                    .ToList();
                if (memberSiaProjects.Count > 0)
                    unitSiaProjects = unitSiaProjects.Concat(memberSiaProjects).ToList();
            }

            var groupName = _storageService.GroupName;
            var unitName = _storageService.SelectedUnitName ?? "";
            var report = new Report();
            var directory = Directory.GetCurrentDirectory();
            report.Load(@$"{directory}\Reports\SIAProjectReport.frx");
            report.SetParameterValue("GroupName", groupName);
            report.SetParameterValue("UnitName", unitName);
            report.SetParameterValue("ReportDate", DateTime.Now.ToShortDateString());
            report.RegisterData(unitSiaProjects, "SIAProjects");

            return report;

        }
    }
}