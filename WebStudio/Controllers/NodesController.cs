using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebStudio.Models;

namespace WebStudio.Controllers
{
    public class NodesController : Controller
    {
        private WebStudioContext _db;

        public NodesController(WebStudioContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Node> nodes = _db.Nodes.ToList();
            if (nodes != null)
            {
                return View(nodes);
            }

            return NotFound();
        }
    }
}