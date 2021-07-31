using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NLog;
using WebStudio.Models;

namespace WebStudio.Controllers
{
    public class ValidationController : Controller
    {
        private WebStudioContext _db;
        private Logger _nLogger = LogManager.GetCurrentClassLogger();

        public ValidationController(WebStudioContext db)
        {
            _db = db;
        }

        public bool CheckCardNumber(string cardNumber)
        {
            try
            {
                if (cardNumber == null)
                    return true;

                return (_db.Cards.Any(c => c.Number == cardNumber));
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
            
        }
    }
}