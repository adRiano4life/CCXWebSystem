using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MimeKit;
using Org.BouncyCastle.Asn1.X509;
using WebStudio.Models;
using WebStudio.Services;

namespace WebStudio.Controllers
{
    [Authorize]
    public class OffersController : Controller
    {
        private WebStudioContext _db;
        
        private FileUploadService _uploadService;
        private IHostEnvironment _environment;

        public OffersController(WebStudioContext db, FileUploadService uploadService, IHostEnvironment environment)
        {
            _db = db;
            _uploadService = uploadService;
            _environment = environment;
        }
        
        public IActionResult Index()
        {
            return View(_db.Offers.ToList());
        }

        [HttpGet]
        public IActionResult Create(string userId)
        {
            if (userId == null)
                return NotFound();
            Offer offer = new Offer(){UserId = userId};
            return View("Create", offer);
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(Offer offer)
        {
            if (ModelState.IsValid && offer.File != null)
            {
                string rootDirName = Program.PathToFiles;
                DirectoryInfo dirInfo = new DirectoryInfo(rootDirName);
                foreach (var dir in dirInfo.GetDirectories())
                {
                    if (!Directory.Exists("Offers"))
                        dirInfo.CreateSubdirectory("Offers");
                }

                //rootDirName = rootDirName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                Console.WriteLine(_environment.ContentRootPath);
                string rootDirPath = Path.Combine(_environment.ContentRootPath, $"wwwroot\\Files\\Offers");
                string fileType = offer.File.FileName.Substring(offer.File.FileName.IndexOf('.'));

                var supplierName = new String(offer.SupplierName.Where(x => char.IsLetterOrDigit(x)
                                                                            || char.IsWhiteSpace(x)).ToArray());
                
                string fileName =
                    $"{offer.CardNumber.Substring(0, offer.CardNumber.IndexOf('/'))} - {supplierName}{fileType}";

                _uploadService.Upload(rootDirPath, fileName, offer.File);
                offer.Path = $"/Offers/{fileName}";
                offer.FileName = fileName;

                Card card = await _db.Cards.FirstOrDefaultAsync(c => c.Number == offer.CardNumber);
                if (card == null)
                    return NotFound();

                offer.CardId = card.Id;
                _db.Offers.Add(offer);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(offer);
        }
        
        
        public IActionResult DownloadFile(string path, string fileName)
        {
            if (path == null && fileName == null)
                return NotFound();

            Offer offer = _db.Offers.FirstOrDefault(o => o.Path == path && o.FileName == fileName);
            if (offer == null)
                return NotFound();
            string contentType = GetContentType(fileName);
            
            var filePath = Path.Combine("", "/Files" + path);
            return File(filePath, contentType, fileName);
        }
        
        [NonAction]
        private string GetContentType(string filename) {
            var dictionary = new Dictionary<string, string> {
                { ".doc", "application/ms-word" },
                { ".docx", "application/ms-word" },
                { ".xls", "application/ms-excel" },
                { ".xlsx", "application/ms-excel" },
                { ".pdf", "application/pdf" },
                { ".png", "image/png" },
                { ".jpg", "image/jpg" }
            };
            string contentType = "";
            string fileExtension = Path.GetExtension(filename);
            dictionary.TryGetValue(fileExtension, out contentType);
            return contentType;
        }
        
        
    }
}