using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NLog;
using WebStudio.Models;
using WebStudio.ViewModels;

namespace WebStudio.Controllers
{
    public class CalculationsController : Controller
    {
        
        private WebStudioContext _db;
        private ILogger<CardsController> _iLogger;
        Logger _nLogger = LogManager.GetCurrentClassLogger();
        

        public CalculationsController(WebStudioContext db, UserManager<User> userManager, IWebHostEnvironment appEnvironment, ILogger<CardsController> iLogger)
        {
            _db = db;
            _iLogger = iLogger;
        }



        
    }
}