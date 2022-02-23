﻿using Microsoft.AspNetCore.Mvc;
using Topo.Models.Login;
using Topo.Services;

namespace Topo.Controllers
{
    public class LoginController : Controller
    {
        private readonly ITerrainAPIService _terrainAPIService;
        private readonly StorageService _storageService;

        public LoginController(ITerrainAPIService terrainAPIService,
            StorageService storageService)
        {
            _terrainAPIService = terrainAPIService;
            _storageService = storageService;
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
                if (_storageService.GetUserResult != null && _storageService.GetUserResult.UserAttributes != null)
                {
                    foreach (var userAttribute in _storageService.GetUserResult.UserAttributes)
                    {
                        if (userAttribute.Name == "email")
                            _storageService.Email = userAttribute.Value;
                    }
                }
                await _terrainAPIService.GetProfilesAsync();
                _storageService.Units = _terrainAPIService.GetUnits();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
