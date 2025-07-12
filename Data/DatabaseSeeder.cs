using YazlabBirSonProje.Models;

namespace YazlabBirSonProje.Data
{
    public static class DatabaseSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product { ProductID = 1, ProductName = "Product1", Stock = 500, Price = 100 },
                    new Product { ProductID = 2, ProductName = "Product2", Stock = 10, Price = 50 },
                    new Product { ProductID = 3, ProductName = "Product3", Stock = 200, Price = 45 },
                    new Product { ProductID = 4, ProductName = "Product4", Stock = 75, Price = 75 },
                    new Product { ProductID = 5, ProductName = "Product5", Stock = 0, Price = 500 }
                };

                context.Products.AddRange(products);
                context.SaveChanges();
            }

            if (!context.Customers.Any())
            {
                var random = new Random();

                int totalCustomers = random.Next(5, 11);
                int premiumCustomerCount = random.Next(2, totalCustomers - 1);

                var customers = new List<Customer>();

                for (int i = 0; i < premiumCustomerCount; i++)
                {
                    customers.Add(new Customer
                    {
                        CustomerName = $"PremiumCustomer_{i + 1}",
                        Budget = random.Next(2000, 3001),
                        CustomerType = "Premium",
                        TotalSpent = 0
                    });
                }

                int standardCustomerCount = totalCustomers - premiumCustomerCount;
                for (int i = 0; i < standardCustomerCount; i++)
                {
                    customers.Add(new Customer
                    {
                        CustomerName = $"Customer_{i + 1}",
                        Budget = random.Next(500, 3001),
                        CustomerType = "Standard",
                        TotalSpent = 0
                    });
                }

                context.Customers.AddRange(customers);
                context.SaveChanges();
            }

            if (!context.Admins.Any())
            {
                context.Admins.Add(new Admin
                {
                    Username = "Osman",
                    Password = "123456"
                });
                context.SaveChanges();
            }
        }
    }
}
