using Microsoft.EntityFrameworkCore;
using YazlabBirSonProje.Models;

namespace YazlabBirSonProje.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Logs> Logs { get; set; }
        public DbSet<Admin> Admins { get; set; }
    }
}