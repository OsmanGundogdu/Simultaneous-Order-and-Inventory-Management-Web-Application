using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YazlabBirSonProje.Data;
using YazlabBirSonProje.Models;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public IActionResult BuyProduct(int productId, int quantity)
    {
        var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);
        if (product == null)
        {
            TempData["Error"] = "Ürün bulunamadı.";
            return RedirectToAction("Index", "Product");
        }

        if (product.Stock < quantity)
        {
            TempData["Error"] = "Yeterli stok bulunmamaktadır.";
            return RedirectToAction("Index", "Product");
        }

        try
        {
            product.Stock -= quantity;
            _context.SaveChanges();
            TempData["Success"] = $"{quantity} adet {product.ProductName} satın alındı.";
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "Başka bir işlem stoğu güncelledi. Lütfen tekrar deneyin.";
        }

        return RedirectToAction("Index", "Product");
    }

    public IActionResult Index()
    {
        var products = _context.Products.ToList();
        return View(products);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Product product)
    {
        _context.Products.Add(product);
        _context.SaveChanges();
        return RedirectToAction("AdminProducts", "Admin");
    }

    public IActionResult Delete(int id)
    {
        var product = _context.Products.Find(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
        }
        return RedirectToAction("AdminProducts", "Admin");
    }
}
