using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using Topo.Models;
using Topo.Models.Home;
using Topo.Services;
using Topo.Data;

namespace Topo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StorageService _storageService;
        private readonly TopoDBContext _dbContext;
        private readonly ILoginService _loginService;

        public HomeController(ILogger<HomeController> logger,
            StorageService storageService,
            TopoDBContext dbContext, ILoginService loginService)
        {
            _logger = logger;
            _storageService = storageService;
            _dbContext = dbContext;
            _loginService = loginService;
        }

        public IActionResult Index()
        {
            var model = PopulateModelFromStorage().Result;
            return View(model);
        }


        private async Task<HomeViewModel> PopulateModelFromStorage()
        {
            if (_dbContext.Authentications.Any())
            {
                var authentication = _dbContext.Authentications.FirstOrDefault();
                if (authentication != null)
                {
                    if (authentication.TokenExpiry < DateTime.Now) // Token expired, login again
                    {
                        _dbContext.Remove(authentication);
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        _storageService.IsAuthenticated = true;
                        _storageService.AuthenticationResult = new Models.Login.AuthenticationResult();
                        _storageService.AuthenticationResult.AccessToken = authentication.AccessToken;
                        _storageService.AuthenticationResult.IdToken = authentication.IdToken;
                        _storageService.AuthenticationResult.ExpiresIn = authentication.ExpiresIn;
                        _storageService.AuthenticationResult.TokenType = authentication.TokenType;
                        _storageService.AuthenticationResult.RefreshToken = authentication.RefreshToken;
                        _storageService.MemberName = authentication.MemberName;
                        _storageService.TokenExpiry = authentication.TokenExpiry ?? DateTime.Now.AddMinutes(-5);
                        await _loginService.GetUserAsync();
                        await _loginService.GetProfilesAsync();
                        _storageService.Units = _loginService.GetUnits();
                    }
                }
            }
            var model = new HomeViewModel();
            model.IsAuthenticated = _storageService.IsAuthenticated;
            model.FullName = _storageService.MemberName;
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            return model;
        }
        public IActionResult Privacy()
        {
            var model = PopulateModelFromStorage().Result;
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}