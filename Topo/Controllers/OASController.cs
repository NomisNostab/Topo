using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Topo.Services;
using Topo.Models.MemberList;
using Topo.Models.OAS;
using Microsoft.AspNetCore.Mvc.Rendering;
using FastReport;
using FastReport.Table;
using FastReport.Export.PdfSimple;
using FastReport.Utils;
using System.Drawing;
using Topo.Data;
using Topo.Data.Models;
using System.Globalization;

namespace Topo.Controllers
{
    public class OASController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IOASService _oasService;
        private readonly IMemberListService _memberListService;
        private readonly TopoDBContext _dbContext;

        public OASController(StorageService storageService,
            IOASService oasService,
            IMemberListService memberListService,
            TopoDBContext dBContext)
        {
            _storageService = storageService;
            _oasService = oasService;
            _memberListService = memberListService;
            _dbContext = dBContext;
        }

        private void SetViewBag()
        {
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            ViewBag.Unit = _storageService.SelectedUnitName;
        }

        private OASIndexViewModel SetUpViewModel()
        {
            var model = new OASIndexViewModel();
            model.Units = new List<SelectListItem>();
            if (_storageService.Units != null)
                model.Units = _storageService.Units;
            if (_storageService.SelectedUnitId != null)
                model.SelectedUnitId = _storageService.SelectedUnitId;
            if (_storageService.SelectedUnitName != null)
                model.SelectedUnitName = _storageService.SelectedUnitName;
            model.Streams = _oasService.GetOASStreamList();
            if (model.Stages == null)
                model.Stages = new List<SelectListItem>();
            return model;
        }


        // GET: OAS
        public ActionResult Index()
        {
            var model = SetUpViewModel();
            SetViewBag();
            return View(model);
        }


        [HttpPost]
        public async Task<ActionResult> Index(OASIndexViewModel oasIndexViewModel)
        {
            var model = new OASIndexViewModel();
            if (oasIndexViewModel.SelectedStream != null && oasIndexViewModel.SelectedStage != null && !oasIndexViewModel.SelectedStage.Contains(oasIndexViewModel.SelectedStream))
                ModelState.AddModelError("SelectedStage", "Select new stage");

            if (ModelState.IsValid)
            {
                _storageService.SelectedUnitId = oasIndexViewModel.SelectedUnitId;
                if (_storageService.Units != null)
                    _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == oasIndexViewModel.SelectedUnitId)?.FirstOrDefault()?.Text;
                model = SetUpViewModel();
                model.Stages = _oasService.GetOASStageListItems(_storageService.OASStageList);
                model.SelectedStream = oasIndexViewModel.SelectedStream;
                model.SelectedStage = oasIndexViewModel.SelectedStage;
            }
            else
            {
                model = SetUpViewModel();

                if (oasIndexViewModel.SelectedStream != null)
                {
                    _storageService.OASStageList = await _oasService.GetOASStageList(oasIndexViewModel.SelectedStream);
                    model.Stages = _oasService.GetOASStageListItems(_storageService.OASStageList);
                    model.SelectedStream = oasIndexViewModel.SelectedStream;
                    model.SelectedStage = "";
                }
            }
            SetViewBag();
            return View(model);
        }

        public async Task<ActionResult> OASReport(string selectedUnitId, string selectedStageTemplate)
        {
            var report = await _oasService.GenerateOASWorksheet(selectedUnitId, selectedStageTemplate);
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
                var templateName = selectedStageTemplate.Replace("/latest.json", "").Replace('/', '_');
                var unitName = _storageService.SelectedUnitName ?? "";
                var fileName = $"OAS_Worksheet_{unitName.Replace(' ', '_')}_{templateName}.pdf";
                return File(strm, "application/pdf", fileName);
            }
            return null;
        }

    }
}
