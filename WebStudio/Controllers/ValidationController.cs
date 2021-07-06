using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebStudio.Models;

namespace WebStudio.Controllers
{
    public class ValidationController : Controller
    {
        private WebStudioContext _db { get; set; }

        public ValidationController(WebStudioContext db)
        {
            _db = db;
        }

        public bool CheckCardNumber(string cardId)
        {
            if (cardId == null)
                return true;

            return (_db.Cards.Any(c => c.Id == cardId));
        }
    }
}