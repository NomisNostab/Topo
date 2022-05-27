using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Topo.Models.Login;
using Topo.Services;

namespace Topo.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;
        private readonly StorageService _storageService;

        public LoginController(ILoginService loginService,
            StorageService storageService)
        {
            _loginService = loginService;
            _storageService = storageService;
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
            var authenticationResult = await _loginService.LoginAsync(loginModel.SelectedBranch, loginModel.UserName, loginModel.Password);
            if (authenticationResult != null && authenticationResult.AuthenticationSuccessResultModel.AuthenticationResult != null)
            {
                await _loginService.GetUserAsync();
                await _loginService.GetProfilesAsync();
                if (_storageService.GetProfilesResult != null && _storageService.GetProfilesResult.profiles != null && _storageService.GetProfilesResult.profiles.Length > 0)
                {
                    _storageService.MemberName = _storageService.GetProfilesResult.profiles[0].member?.name ?? "";
                    _storageService.GroupName = _storageService.GetProfilesResult.profiles[0].group?.name ?? "";
                }
                _storageService.Units = _loginService.GetUnits();
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

            return RedirectToAction("Index", "Home");
        }
    }
}
