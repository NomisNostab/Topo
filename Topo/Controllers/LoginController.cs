using Microsoft.AspNetCore.Mvc;
using Topo.Models.Login;
using Topo.Services;
using Topo.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Topo.Controllers
{
    public class LoginController : Controller
    {
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly StorageService _storageService;
        private readonly TopoDBContext _dbContext;

        public LoginController(ITerrainAPIService terrainAPIService,
            StorageService storageService,
            TopoDBContext topoDBContext)
        {
            _terrainAPIService = terrainAPIService;
            _storageService = storageService;
            _dbContext = topoDBContext;
        }
        public IActionResult Login()
        {
            LoginViewModel model = SetUpViewModel();
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            return View(model);
        }

        private static LoginViewModel SetUpViewModel()
        {
            var model = new LoginViewModel();
            model.Branches = new List<SelectListItem>()
            {
                new SelectListItem(){ Text = "ACT", Value = "act" },
                new SelectListItem(){ Text = "NSW", Value = "nsw"},
                new SelectListItem(){ Text = "NT", Value = "nt"},
                new SelectListItem(){ Text = "QLD", Value = "qld"},
                new SelectListItem(){ Text = "SA", Value = "sa"},
                new SelectListItem(){ Text = "TAS", Value = "tas"},
                new SelectListItem(){ Text = "VIC", Value = "vic"},
                new SelectListItem(){ Text = "WA", Value = "wa"}
            };
            return model;
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("SelectedBranch, UserName, Password")] LoginViewModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                LoginViewModel model = SetUpViewModel();
                model.SelectedBranch = loginModel.SelectedBranch;
                model.UserName = loginModel.UserName;
                model.Password = loginModel.Password;
                return View(model);
            }
            var authenticationResult = await _terrainAPIService.LoginAsync(loginModel.SelectedBranch, loginModel.UserName, loginModel.Password);
            if (authenticationResult != null && authenticationResult.AuthenticationSuccessResultModel.AuthenticationResult != null)
            {
                await _terrainAPIService.GetUserAsync();
                await _terrainAPIService.GetProfilesAsync();
                if (_storageService.GetProfilesResult != null && _storageService.GetProfilesResult.profiles != null && _storageService.GetProfilesResult.profiles.Length > 0)
                    _storageService.MemberName = _storageService.GetProfilesResult.profiles[0].member?.name ?? "";
                _storageService.Units = _terrainAPIService.GetUnits();
                var authentication = _dbContext.Authentications.FirstOrDefault();
                if (authentication == null)
                {
                    authentication = new Data.Models.Authentication();
                    _dbContext.Authentications.Add(authentication);
                }
                authentication.AccessToken = authenticationResult.AuthenticationSuccessResultModel.AuthenticationResult.AccessToken;
                authentication.IdToken = authenticationResult.AuthenticationSuccessResultModel.AuthenticationResult.IdToken;
                authentication.RefreshToken = authenticationResult.AuthenticationSuccessResultModel.AuthenticationResult.RefreshToken;
                authentication.TokenType = authenticationResult.AuthenticationSuccessResultModel.AuthenticationResult.TokenType;
                authentication.ExpiresIn = authenticationResult.AuthenticationSuccessResultModel.AuthenticationResult.ExpiresIn;
                authentication.MemberName = _storageService.MemberName;
                authentication.TokenExpiry = _storageService.TokenExpiry;
                _dbContext.SaveChanges();
            }
            if (authenticationResult != null && authenticationResult.AuthenticationErrorResultModel.message != null)
            {
                LoginViewModel model = SetUpViewModel();
                model.SelectedBranch = loginModel.SelectedBranch;
                model.UserName = loginModel.UserName;
                model.Password = loginModel.Password;
                ModelState.AddModelError("Password", authenticationResult.AuthenticationErrorResultModel.message);
                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            _storageService.ClearStorage();
            var authentication = _dbContext.Authentications.FirstOrDefault();
            if (authentication != null)
            {
                _dbContext.Remove(authentication);
                _dbContext.SaveChanges();
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
