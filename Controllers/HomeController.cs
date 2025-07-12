using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using YazlabBirSonProje.Models;
using YazlabBirSonProje.Data;

namespace YazlabBirSonProje.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        if (HttpContext.Session.GetString("Token") == null)
        {
            TempData["Error"] = "Lütfen önce giriş yapınız.";
            return RedirectToAction("Login", "Customer");
        }

        ViewData["Message"] = "Hoş geldiniz, kontrol panelindesiniz!";
        return View();
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
