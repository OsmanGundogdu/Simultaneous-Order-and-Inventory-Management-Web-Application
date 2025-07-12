using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
using YazlabBirSonProje.Data;
using YazlabBirSonProje.Models;
using YazlabBirSonProje.Services;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly LoggingService _loggingService;

    public OrderController(ApplicationDbContext context, LoggingService loggingService)
    {
        _context = context;
        _loggingService = loggingService;
    }

    public IActionResult Index()
    {
        var customerId = HttpContext.Session.GetInt32("LoggedInCustomerID");
        if (customerId == null)
        {
            TempData["Error"] = "Siparişleri görmek için giriş yapmalısınız.";
            return RedirectToAction("Login", "Customer");
        }

        var orders = _context.Orders
            .Where(o => o.CustomerID == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        if (!orders.Any())
        {
            TempData["Info"] = "Henüz bir siparişiniz yok.";
        }

        return View(orders);
    }

    public IActionResult Create()
    {
        ViewBag.Products = _context.Products.ToList();
        return View(new OrderViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(OrderViewModel orderViewModel)
    {
        var startTime = DateTime.Now;

        var customerId = HttpContext.Session.GetInt32("LoggedInCustomerID");
        if (customerId == null)
        {
            TempData["Error"] = "Sipariş oluşturmak için giriş yapmalısınız.";
            return RedirectToAction("Login", "Customer");
        }

        var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == customerId);
        if (customer == null)
        {
            TempData["Error"] = "Müşteri bulunamadı.";
            return RedirectToAction("Login", "Customer");
        }

        foreach (var (productId, quantity) in orderViewModel.ProductIDs.Zip(orderViewModel.Quantities))
        {
            if ((DateTime.Now - startTime).TotalSeconds > 60)
            {
                _loggingService.Log(null, customer.CustomerID, "Hata", customer.CustomerType, "N/A", "Zaman aşımı");
                TempData["Error"] = "Zaman aşımı! Sipariş oluşturma işlemi 60 saniyede tamamlanamadı.";
                return RedirectToAction("Create");
            }

            var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product == null || product.Stock <= 0)
            {
                _loggingService.Log(null, customer.CustomerID, "Hata", customer.CustomerType, $"{product?.ProductName} - {quantity}", "Ürün stoğu yetersiz");
                TempData["Error"] = $"{product?.ProductName ?? "Ürün"} stoğu tükenmiş.";
                ViewBag.Products = _context.Products.ToList();
                return View(orderViewModel);
            }

            if (quantity > 5)
            {
                _loggingService.Log(null, customer.CustomerID, "Hata", customer.CustomerType, $"{product.ProductName} - {quantity}", "Maksimum miktar aşıldı");
                TempData["Error"] = $"{product.ProductName}: Tek seferde en fazla 5 adet alabilirsiniz.";
                ViewBag.Products = _context.Products.ToList();
                return View(orderViewModel);
            }

            var totalPrice = quantity * product.Price;
            if (customer.Budget < totalPrice)
            {
                _loggingService.Log(null, customer.CustomerID, "Hata", customer.CustomerType, $"{product.ProductName} - {quantity}", "Müşteri bakiyesi yetersiz");
                TempData["Error"] = $"{product.ProductName}: Bütçeniz yeterli değil.";
                ViewBag.Products = _context.Products.ToList();
                return View(orderViewModel);
            }

            var order = new Order
            {
                CustomerID = customer.CustomerID,
                ProductID = product.ProductID,
                Quantity = quantity,
                TotalPrice = totalPrice,
                OrderStatus = "Bekliyor",
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            _loggingService.Log(order.OrderID, customer.CustomerID, "Bilgilendirme", customer.CustomerType, $"{product.ProductName} - {quantity}", "Sipariş başarıyla oluşturuldu");
        }

        System.Threading.Thread.Sleep(2000);

        _context.SaveChanges();
        TempData["Success"] = "Siparişler başarıyla oluşturuldu.";
        return RedirectToAction("Index", "Order");
    }

    [HttpGet]
    public JsonResult GetStockData()
    {
        var stockData = _context.Products
            .Select(p => new { p.ProductName, p.Stock })
            .ToList();

        return Json(stockData);
    }




}
