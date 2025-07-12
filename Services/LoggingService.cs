using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YazlabBirSonProje.Data;
using YazlabBirSonProje.Models;

namespace YazlabBirSonProje.Services
{
    public class LoggingService
    {
        private readonly ApplicationDbContext _context;

        public LoggingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Log(int? orderId, int? customerId, string logType, string customerType, string productDetails, string result)
        {
            var log = new Logs
            {
                OrderID = orderId,
                CustomerID = customerId,
                LogType = logType,
                CustomerType = customerType,
                ProductDetails = productDetails,
                Timestamp = DateTime.Now,
                Result = result
            };

            _context.Logs.Add(log);
            _context.SaveChanges();
        }
    }
}