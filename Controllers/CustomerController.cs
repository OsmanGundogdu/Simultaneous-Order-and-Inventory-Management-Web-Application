using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YazlabBirSonProje.Data;
using YazlabBirSonProje.Helpers;
using YazlabBirSonProje.Models;
using Microsoft.AspNetCore.Http;

public class CustomerController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomerController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CustomerID,CustomerName,Budget,CustomerType,TotalSpent")] Customer customer)
    {
        if (ModelState.IsValid)
        {
            _context.Add(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(customer);
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _context.Customers
            .OrderByDescending(c => c.PriorityScore)
            .ToListAsync();

        ViewBag.IsLoggedIn = HttpContext.Session.GetString("Token") != null;
        ViewBag.IsAdminLoggedIn = HttpContext.Session.GetString("AdminToken") != null;

        return View(customers);
    }

    public IActionResult Login(int id)
    {
        var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == id);
        if (customer == null)
        {
            return NotFound();
        }

        ViewBag.CustomerName = customer.CustomerName;
        ViewBag.CustomerID = customer.CustomerID;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(int id, string password)
    {
        if (password != "123456")
        {
            ViewBag.Error = "Hatalı şifre! Lütfen tekrar deneyin.";
            return View();
        }

        var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == id);
        if (customer == null)
        {
            ViewBag.Error = "Geçersiz müşteri ID. Lütfen tekrar deneyin.";
            return View();
        }

        var token = new Token
        {
            Value = TokenHelper.GenerateToken(),
            Expiration = DateTime.Now.AddMinutes(5)
        };

        HttpContext.Session.SetString("Token", token.Value);
        HttpContext.Session.SetString("TokenExpiration", token.Expiration.ToString());
        HttpContext.Session.SetInt32("LoggedInCustomerID", customer.CustomerID);
        HttpContext.Session.SetString("LoggedInCustomerName", customer.CustomerName);

        TempData["Success"] = $"Giriş başarılı! Hoş geldiniz, {customer.CustomerName}.";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "Çıkış başarılı!";
        return RedirectToAction(nameof(Index), "Home");
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound();
        }
        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CustomerID,CustomerName,Budget,CustomerType,TotalSpent")] Customer customer)
    {
        if (id != customer.CustomerID)
        {
            return NotFound();
        }

        if (customer.TotalSpent > 2000 && customer.CustomerType != "Premium")
        {
            customer.CustomerType = "Premium";
        }
        else if (customer.TotalSpent <= 2000 && customer.CustomerType != "Standard")
        {
            customer.CustomerType = "Standard";
        }

        UpdatePriorityScore(customer);

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(customer.CustomerID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(customer);
    }

    private bool CustomerExists(int id)
    {
        return _context.Customers.Any(e => e.CustomerID == id);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var customer = await _context.Customers
            .FirstOrDefaultAsync(m => m.CustomerID == id);
        if (customer == null)
        {
            return NotFound();
        }

        return View(customer);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public void UpdatePriorityScore(Customer customer)
    {
        int basePriorityScore = customer.CustomerType == "Premium" ? 20 : 10;
        decimal waitingTimeWeight = 0.5m;

        decimal priorityScore = basePriorityScore + (customer.TotalSpent * waitingTimeWeight);

        customer.PriorityScore = (int)priorityScore;
    }




}

public class SessionHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetSession(string key, string value)
    {
        _httpContextAccessor.HttpContext.Session.SetString(key, value);
    }

    public string GetSession(string key)
    {
        return _httpContextAccessor.HttpContext.Session.GetString(key);
    }
}
