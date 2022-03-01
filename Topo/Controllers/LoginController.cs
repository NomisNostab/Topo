using Microsoft.AspNetCore.Mvc;
using Topo.Models.Login;
using Topo.Services;
using Topo.Data;

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
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind("UserName, Password")] LoginViewModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var authenticationResult = await _terrainAPIService.LoginAsync(loginModel.UserName, loginModel.Password);
            if (authenticationResult != null)
            {
                await _terrainAPIService.GetUserAsync();
                await _terrainAPIService.GetProfilesAsync();
                if (_storageService.GetProfilesResult != null && _storageService.GetProfilesResult.profiles != null && _storageService.GetProfilesResult.profiles.Length > 0)
                    _storageService.MemberName = _storageService.GetProfilesResult.profiles[0].member?.name ?? "";
                _storageService.Units = _terrainAPIService.GetUnits();
            }
            var authentication = _dbContext.Authentications.FirstOrDefault();
            if (authentication == null)
            {
                authentication = new Data.Models.Authentication();
                _dbContext.Authentications.Add(authentication);
            }
            authentication.AccessToken = authenticationResult.AuthenticationResult.AccessToken;
            authentication.IdToken = authenticationResult.AuthenticationResult.IdToken;
            authentication.RefreshToken = authenticationResult.AuthenticationResult.RefreshToken;
            authentication.TokenType = authenticationResult.AuthenticationResult.TokenType;
            authentication.ExpiresIn = authenticationResult.AuthenticationResult.ExpiresIn;
            authentication.MemberName = _storageService.MemberName;
            authentication.TokenExpiry = _storageService.TokenExpiry;
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
    }
}
