﻿using Microsoft.AspNetCore.Mvc;
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
            var model = PopulateModelFromStorage();
            return View(model);
        }


        private HomeViewModel PopulateModelFromStorage()
        {
            var model = new HomeViewModel();
            model.IsAuthenticated = _storageService.IsAuthenticated;
            model.FullName = _storageService.MemberName;
            ViewBag.IsAuthenticated = _storageService.IsAuthenticated;
            return model;
        }
        public IActionResult Privacy()
        {
            var model = PopulateModelFromStorage();
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}