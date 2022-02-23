using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using Topo.Models;
using Topo.Models.Home;
using Topo.Services;

namespace Topo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StorageService _storageService;

        public HomeController(ILogger<HomeController> logger, 
            StorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        public IActionResult Index()
        {
            var model = PopulateModelFromStorage();
            return View(model);
        }


        private HomeViewModel PopulateModelFromStorage()
        {
            var model = new HomeViewModel();
            model.IsAuthenticated = _storageService.IsAuthenticated;
            if (_storageService.GetProfilesResult != null && _storageService.GetProfilesResult.profiles != null && _storageService.GetProfilesResult.profiles.Length > 0)
                model.FullName = _storageService.GetProfilesResult.profiles[0].member?.name ?? "";
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            return model;
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}