using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YazlabBirSonProje.Data;
using YazlabBirSonProje.Models;
using YazlabBirSonProje.Services;

namespace YazlabBirSonProje.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LoggingService _loggingService;

        public AdminController(ApplicationDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Username == username && a.Password == password);
            if (admin != null)
            {
                HttpContext.Session.SetString("AdminLoggedIn", "true");
                HttpContext.Session.SetString("AdminToken", Guid.NewGuid().ToString());
                TempData["Success"] = "Admin olarak başarıyla giriş yaptınız!";
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Geçersiz kullanıcı adı veya şifre.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminToken");
            TempData["Success"] = "Başarıyla çıkış yapıldı.";
            return RedirectToAction("Login");
        }
        
        public IActionResult Dashboard()
        {
            var adminLoggedIn = HttpContext.Session.GetString("AdminLoggedIn");
            if (adminLoggedIn != "true")
            {
                TempData["Error"] = "Giriş yapmanız gerekiyor!";
                return RedirectToAction("Login");
            }

            return View("Dashboard");
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("AdminToken") != null;
        }

        public IActionResult AdminProducts()
        {
            if (!IsAdminLoggedIn())
            {
                TempData["Error"] = "Lütfen önce giriş yapınız.";
                return RedirectToAction("Login");
            }

            var products = _context.Products.ToList();
            ViewBag.Products = products;

            return View();
        }
        
        public IActionResult Logs()
        {
            var logs = _context.Logs
                .OrderByDescending(log => log.Timestamp)
                .Take(50)
                .ToList();

            return View(logs);
        }

        [HttpGet]
        public IActionResult GetLogs()
        {
            var logs = _context.Logs
                .OrderByDescending(log => log.Timestamp)
                .Take(50)
                .Select(log => new
                {
                    customerID = log.CustomerID.HasValue ? log.CustomerID.ToString() : "Admin islemi",
                    customerType = log.CustomerType ?? "N/A",
                    logID = log.LogID,
                    logType = log.LogType ?? "N/A",
                    productDetails = log.ProductDetails ?? "N/A",
                    result = log.Result ?? "N/A",
                    timestamp = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")                  
                })
                .ToList();

            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(logs));
            return Json(logs);
        }

        [HttpPost]
        public IActionResult ApproveOrder(int orderId)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var order = _context.Orders.Include(o => o.Product).FirstOrDefault(o => o.OrderID == orderId);
                if (order == null)
                {
                    TempData["Error"] = "Sipariş bulunamadı.";
                    return RedirectToAction("ApproveOrderList");
                }

                if (order.Product.Stock < order.Quantity)
                {
                    order.OrderStatus = "Reddedildi";
                    _context.SaveChanges();

                    _loggingService.Log(order.OrderID, order.CustomerID, "Hata", "Admin İşlemi", $"{order.Product.ProductName} - {order.Quantity}", "Stok yetersizliği nedeniyle sipariş reddedildi");
                    TempData["Error"] = "Stok yetersizliği nedeniyle sipariş reddedildi.";
                    transaction.Rollback();
                    return RedirectToAction("ApproveOrderList");
                }

                order.Product.Stock -= order.Quantity;

                var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == order.CustomerID);
                if (customer != null)
                {
                    customer.Budget -= order.TotalPrice;
                    customer.TotalSpent += order.TotalPrice;
                }

                if(customer.TotalSpent >= 2000)
                {
                    customer.CustomerType = "Premium";
                }

                order.OrderStatus = "Onaylandı";
                _context.SaveChanges();

                _loggingService.Log(order.OrderID, order.CustomerID, "Bilgilendirme", "Admin İşlemi", $"{order.Product.ProductName} - {order.Quantity}", "Sipariş başarıyla onaylandı");

                transaction.Commit();
                TempData["Success"] = "Sipariş başarıyla onaylandı.";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["Error"] = $"Sipariş onaylanırken bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction("ApproveOrderList");
        }

        [HttpGet]
        public IActionResult ApproveOrderList()
        {
            var pendingOrders = _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Customer)
                .Where(o => o.OrderStatus == "Bekliyor")
                .AsEnumerable()
                .Select(o =>
                {
                    double pendingTime = (DateTime.Now - o.OrderDate.Value).TotalSeconds;

                    double priorityScore = (o.Customer.CustomerType == "Premium" ? 20 : 10) +
                                        (pendingTime * 0.5);

                    o.Customer.PendingTime = pendingTime;
                    o.Customer.PriorityScore = priorityScore;

                    return new
                    {
                        Order = o,
                        PriorityScore = priorityScore
                    };
                })
                .OrderByDescending(o => o.Order.Customer.CustomerType == "Premium" ? 1 : 0)
                .ThenByDescending(o => o.PriorityScore)
                .Select(o => o.Order)
                .ToList();

            _context.SaveChanges();

            return View("ApproveOrder", pendingOrders);
        }

        [HttpPost]
        public IActionResult RejectOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (order == null)
            {
                TempData["Error"] = "Sipariş bulunamadı.";
                return RedirectToAction("ApproveOrderList");
            }

            order.OrderStatus = "Reddedildi";
            _context.SaveChanges();

            var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == order.CustomerID);
            _loggingService.Log(order.OrderID, order.CustomerID, "Hata", customer?.CustomerType == "Standard" ? "Standard" : "Premium", $"{order.Product?.ProductName} - {order.Quantity}", "Sipariş admin tarafından reddedildi");
            TempData["Success"] = "Sipariş başarıyla reddedildi.";
            return RedirectToAction("ApproveOrderList");
        }

        [HttpPost]
        public IActionResult ApproveAllOrders()
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
            var pendingOrders = _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Customer)
                .Where(o => o.OrderStatus == "Bekliyor")
                .OrderBy(o => o.OrderDate)
                .ToList();

            var customer = _context.Customers.FirstOrDefault(c => c.CustomerID == pendingOrders[0].CustomerID);
            foreach (var order in pendingOrders)
            {
                if (order.Product.Stock >= order.Quantity &&  customer.Budget >= order.TotalPrice)
                {
                order.Product.Stock -= order.Quantity;

                if (order.Customer != null)
                {
                    order.Customer.Budget -= order.TotalPrice;
                    order.Customer.TotalSpent += order.TotalPrice;
                }

                if(customer.TotalSpent >= 2000)
                {
                    customer.CustomerType = "Premium";
                }

                order.OrderStatus = "Onaylandı";

                _loggingService.Log(
                    order.OrderID,
                    order.CustomerID,
                    "Bilgilendirme",
                    "Admin İşlemi",
                    $"{order.Product.ProductName} - {order.Quantity}",
                    "Sipariş başarıyla onaylandı"
                );
                }
                else
                {
                order.OrderStatus = "Reddedildi";

                _loggingService.Log(
                    order.OrderID,
                    order.CustomerID,
                    "Hata",
                    "Admin İşlemi",
                    $"{order.Product.ProductName} - {order.Quantity}",
                    "Stok yetersizliği veya bütçe yetersizliği nedeniyle sipariş reddedildi"
                );
                }
            }

            _context.SaveChanges();
            transaction.Commit();

            TempData["Success"] = "Tüm bekleyen siparişler başarıyla işleme alındı.";
            }
            catch (Exception ex)
            {
            transaction.Rollback();
            TempData["Error"] = $"Siparişler işlenirken bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction("ApproveOrderList");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                TempData["Error"] = "Düzenlenecek ürün bulunamadı.";
                return RedirectToAction("AdminProducts");
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product updatedProduct)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductID == updatedProduct.ProductID);
            if (product == null)
            {
                TempData["Error"] = "Güncellenecek ürün bulunamadı.";
                return RedirectToAction("AdminProducts");
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                product.ProductName = updatedProduct.ProductName;
                product.Stock = updatedProduct.Stock;
                product.Price = updatedProduct.Price;
                _context.SaveChanges();

                _loggingService.Log(null, null, "Bilgilendirme", "Admin İşlemi", $"{product.ProductName} bilgileri güncellendi", "Ürün başarıyla güncellendi");

                transaction.Commit();
                TempData["Success"] = "Ürün başarıyla güncellendi.";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["Error"] = $"Ürün güncellenirken bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction("AdminProducts");
        }

        public IActionResult AddUsers()
        {
            try
            {
                DatabaseSeeder.Seed(_context);
                TempData["Success"] = "Kullanıcılar başarıyla eklendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Kullanıcılar eklenirken bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUsers(List<int> selectedUserIds)
        {
            if (selectedUserIds == null || !selectedUserIds.Any())
            {
                TempData["Error"] = "Silmek için en az bir kullanıcı seçmelisiniz.";
                return RedirectToAction("Index", "Customer");
            }

            try
            {
                foreach (var userId in selectedUserIds)
                {
                    var user = _context.Customers.FirstOrDefault(c => c.CustomerID == userId);
                    if (user != null)
                    {
                        var orders = _context.Orders.Where(o => o.CustomerID == userId).ToList();
                        _context.Orders.RemoveRange(orders);

                        _loggingService.Log(null, user.CustomerID, "Bilgilendirme", "Admin İşlemi", "N/A", $"Müşteri {user.CustomerName} ve ilgili siparişler silindi");

                        _context.Customers.Remove(user);
                    }
                }

                _context.SaveChanges();
                TempData["Success"] = "Seçilen kullanıcılar ve ilgili siparişler başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Kullanıcıları silerken bir hata oluştu: {ex.Message}";
            }
            return RedirectToAction("Index", "Customer");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProducts(List<int> selectedProductIds)
        {
            if (selectedProductIds == null || !selectedProductIds.Any())
            {
                TempData["Error"] = "Silmek için bir ürün seçmelisiniz.";
                return RedirectToAction("AdminProducts");
            }

            try
            {
                foreach (var productId in selectedProductIds)
                {
                    var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);
                    if (product != null)
                    {
                        var orders = _context.Orders.Where(o => o.ProductID == productId).ToList();
                        _context.Orders.RemoveRange(orders);

                        _context.Products.Remove(product);

                        _loggingService.Log(null, null, "Bilgilendirme", "Admin İşlemi", $"{product.ProductName}", $"Ürün ve ilgili siparişler silindi.");
                    }
                }

                _context.SaveChanges();
                TempData["Success"] = "Seçilen ürünler başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction("AdminProducts");
        }


    
    }
}
