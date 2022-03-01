using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Topo.Services;
using Topo.Models.OAS;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Topo.Controllers
{
    public class OASController : Controller
    {
        private readonly StorageService _storageService;
        private readonly IOASService _oasService;

        public OASController(StorageService storageService, IOASService oasService)
        {
            _storageService = storageService;
            _oasService = oasService;
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
            if (oasIndexViewModel.SelectedStage != null && !oasIndexViewModel.SelectedStage.Contains(oasIndexViewModel.SelectedStream))
                ModelState.AddModelError("SelectedStage", "Select new stage");

            if (ModelState.IsValid)
            {
                _storageService.SelectedUnitId = oasIndexViewModel.SelectedUnitId;
                if (_storageService.Units != null)
                    _storageService.SelectedUnitName = _storageService.Units.Where(u => u.Value == oasIndexViewModel.SelectedUnitId)?.SingleOrDefault()?.Text;
                model = SetUpViewModel();
                model.Stages = _oasService.GetOASStageListItems(_storageService.OASStageList);
                model.SelectedStream = oasIndexViewModel.SelectedStream;
                var selectedStage = _storageService.OASStageList.Where(s => s.TemplateLink == oasIndexViewModel.SelectedStage).SingleOrDefault();
                await _oasService.GetUnitAchievements(oasIndexViewModel.SelectedUnitId, selectedStage.Stream.ToLower(), selectedStage.Branch, selectedStage.Stage);
                model.SelectedStage = oasIndexViewModel.SelectedStage;
            }
            else
            {
                model = SetUpViewModel();

                if (oasIndexViewModel.SelectedStream != null)
                {
                    _storageService.OASStageList = await _oasService.GetOASStageList(oasIndexViewModel.SelectedStream);
                    model.Stages = _oasService.GetOASStageListItems(_storageService.OASStageList);
                    model.SelectedStage = oasIndexViewModel.SelectedStage;
                    model.SelectedStream = oasIndexViewModel.SelectedStream;
                }
            }
            SetViewBag();
            return View(model);
        }



        // POST: OAS/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
