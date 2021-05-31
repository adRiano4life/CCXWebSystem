using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using WebStudio.Models;
using WebStudio.Services;
using WebStudio.ViewModels;

namespace WebStudio.Controllers
{
    public class RequestsController : Controller
    {

        private WebStudioContext _db;
        private UserManager<User> _userManager;
        private readonly IHostEnvironment _environment;
        private readonly FileUploadService _uploadService;

        public RequestsController(WebStudioContext db, 
            UserManager<User> userManager,
            IHostEnvironment environment,
            FileUploadService uploadService)
        {
            _db = db;
            _userManager = userManager;
            _environment = environment;
            _uploadService = uploadService;
        }

        // GET
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create(string cardId)
        {
            CreateRequestViewModel model = new CreateRequestViewModel
            {
                Text = "    В связи с производственной необходимостью, просим Вас рассмотреть возможность предоставления коммерческого предложения в максимально короткие сроки. " +
                       "При составлении коммерческого предложения просим Вас учесть следующее:\n" +
                       "    1. Соответствие запрашиваемого ТМЦ с Вашим предложением: марка, модель, модификации, ГОСТ , ТУ и т.д. В случае не “критичного” расхождения, просим указать данный факт в КП.\n" +
                       "    2. Количество: в случае несоответствия запрашиваемого кол-ва нормам отгрузки ТМЦ, просим указать данный факт.\n" +
                       "    3. Качество: предлагать только новый товар, не бывшим в использовании, в ином случае предупреждать в КП.\n" +
                       "    4. Отгрузка: просим указывать варианты отгрузки, которыми располагаете (автотранспорт, жд/транспорт, авиатранспорт)\n" +
                       "    5. Условия оплаты.\n" +
                       "    6. В случае габаритного груза, просим указывать полные габариты в упакованном виде.\n",
                Card = _db.Cards.FirstOrDefault(c=>c.Id == cardId)
            };
            return View(model);
        }

        [HttpPost]
        //[Authorize]
        public async Task<IActionResult> Create(CreateRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                string path = Path.Combine(_environment.ContentRootPath, "wwwroot\\Files\\Requests");
                string filePath = $"\\Files\\Requests\\{model.File.FileName}";
                _uploadService.Upload(path, model.File.FileName, model.File);
                model.FilePath = filePath;

                Request request = new Request
                {
                    Text = model.Text,
                    DateOfCreate = model.DateOfCreate,
                    ExecutorId = model.ExecutorId,
                    Executor = await _userManager.FindByIdAsync(model.ExecutorId),
                    CardId = model.CardId,
                    Card = _db.Cards.FirstOrDefault(c => c.Id == model.CardId),
                    FilePath = model.FilePath
                };
                var result = _db.Requests.AddAsync(request);
                if (result.IsCompleted)
                {
                    await _db.SaveChangesAsync();
                    return RedirectToAction("DetailCard", "Cards");
                }
            }

            return View(model);
        }
    }
}