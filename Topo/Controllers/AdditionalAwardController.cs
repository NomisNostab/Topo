﻿using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Spire.Xls;
using Topo.Models.AditionalAwards;
using Topo.Models.MemberList;
using Topo.Services;

namespace Topo.Controllers
{
    public class AdditionalAwardController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IMemberListService _memberListService;
        private readonly IAdditionalAwardService _additionalAwardService;
        private readonly ILogger<SIAController> _logger;

        public AdditionalAwardController(StorageService storageService, IMemberListService memberListService, IAdditionalAwardService addirionalAwardService, ILogger<SIAController> logger)
        {
            _storageService = storageService;
            _memberListService = memberListService;
            _additionalAwardService = addirionalAwardService;
            _logger = logger;
        }

        private void SetViewBag()
        {
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            ViewBag.Unit = _storageService.SelectedUnitName;
        }

        private async Task<AdditionalAwardIndexViewModel> SetUpViewModel()
        {
            var model = new AdditionalAwardIndexViewModel();
            model.Units = new List<SelectListItem>();
            if (_storageService.Units != null)
                model.Units = _storageService.Units;
            if (_storageService.SelectedUnitId != null)
            {
                model.SelectedUnitId = _storageService.SelectedUnitId;
                var allMembers = await _memberListService.GetMembersAsync();
                var members = allMembers.Where(m => m.isAdultLeader == 0).OrderBy(m => m.first_name).ThenBy(m => m.last_name).ToList();
                foreach (var member in members)
                {
                    var editorViewModel = new MemberListEditorViewModel
                    {
                        id = member.id,
                        first_name = member.first_name,
                        last_name = member.last_name,
                        member_number = member.member_number,
                        patrol_name = string.IsNullOrEmpty(member.patrol_name) ? "-" : member.patrol_name,
                        patrol_duty = string.IsNullOrEmpty(member.patrol_duty) ? "-" : member.patrol_duty,
                        unit_council = member.unit_council,
                        selected = false
                    };
                    model.Members.Add(editorViewModel);
                }
            }
            if (_storageService.Units != null)
            {
                _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == _storageService.SelectedUnitId)?.FirstOrDefault()?.Text;
                model.SelectedUnitName = _storageService.SelectedUnitName ?? "";
            }
            SetViewBag();
            return model;
        }

        // GET: AdditionalAwardController
        public async Task<ActionResult> Index()
        {
            var model = await SetUpViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Index(AdditionalAwardIndexViewModel additionalAwardIndexViewModel, string button)
        {
            if (string.IsNullOrEmpty(additionalAwardIndexViewModel.SelectedUnitId) || _storageService.SelectedUnitId != additionalAwardIndexViewModel.SelectedUnitId)
            {
                _storageService.SelectedUnitId = additionalAwardIndexViewModel.SelectedUnitId;
                return RedirectToAction("Index", "AdditionalAward");
            }

            var model = new AdditionalAwardIndexViewModel();
            if (!string.IsNullOrEmpty(additionalAwardIndexViewModel.SelectedUnitId))
            {
                _storageService.SelectedUnitId = additionalAwardIndexViewModel.SelectedUnitId;
                if (!string.IsNullOrEmpty(button))
                {
                    var selectedMembers = additionalAwardIndexViewModel.getSelectedMembers();
                    if (selectedMembers == null)
                    {
                        _logger.LogInformation("selectedMembers: null");
                        selectedMembers = new List<string>();
                    }
                    // No members selected, default to all
                    if (selectedMembers.Count() == 0)
                    {
                        selectedMembers = additionalAwardIndexViewModel.Members.Select(m => m.id).ToList();
                    }
                    if (selectedMembers != null)
                    {
                        _logger.LogInformation($"selectedMembers.Count: {selectedMembers.Count()}");
                        var memberKVP = new List<KeyValuePair<string, string>>();
                        foreach (var member in selectedMembers)
                        {
                            var memberName = additionalAwardIndexViewModel.Members.Where(m => m.id == member).Select(m => m.first_name + " " + m.last_name).FirstOrDefault();
                            memberKVP.Add(new KeyValuePair<string, string>(member, memberName ?? ""));
                        }
                        _logger.LogInformation($"memberKVP.Count: {memberKVP.Count()}");
                        var workbook = await _additionalAwardService.GenerateAdditionalAwardReport(memberKVP);
                        if (!string.IsNullOrEmpty(button))
                        {
                            if (button == "AdditionalAwardReport")
                            {
                                var sheet = workbook.Worksheets[0];
                                sheet.PageSetup.PaperSize = PaperSizeType.PaperA3;
                                sheet.PageSetup.Orientation = PageOrientationType.Landscape;
                                MemoryStream strm = new MemoryStream();
                                sheet.SaveToPdfStream(strm);

                                // return stream in browser
                                var unitName = _storageService.SelectedUnitName ?? "";
                                return File(strm, "application/pdf", $"Additional_Awards_{unitName.Replace(' ', '_')}.pdf");
                            }
                            if (button == "AdditionalAwardxls")
                            {
                                //Stream as Excel file
                                MemoryStream strm = new MemoryStream();
                                workbook.SaveToStream(strm, FileFormat.Version2016);

                                // return stream in browser
                                var unitName = _storageService.SelectedUnitName ?? "";
                                return File(strm.ToArray(), "application/vnd.ms-excel", $"Additional_Awards_{unitName.Replace(' ', '_')}.xlsx");
                            }
                        }
                    }
                }
            }
            model = await SetUpViewModel();
            return View(model);
        }

    }
}

////Build report template for model
//var directory = Directory.GetCurrentDirectory();
//var report1 = new FastReport.Report();
//report1.Dictionary.RegisterBusinessObject(
//        new List<Models.AditionalAwards.AdditionalAwardListModel>(),
//        "AdditionalAwards",
//        2,
//        true
//    );
//report1.Save($"{directory}/Reports/AdditionalAwards.frx");