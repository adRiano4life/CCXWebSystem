using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
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
            return View(offer);
        }
        
        [HttpPost]
        public IActionResult Create(Offer offer)
        {
            if (ModelState.IsValid)
            {
                if (offer.File != null)
                {
                    string rootDirName = "wwwroot";
                    DirectoryInfo dirInfo = new DirectoryInfo(rootDirName);
                    foreach (var dir in dirInfo.GetDirectories())
                    {
                        if (!Directory.Exists("Offers"))
                            dirInfo.CreateSubdirectory("Offers");
                    }
                    rootDirName = String.Concat(rootDirName, "\\Offers");
                    string rootDirPath = Path.Combine(_environment.ContentRootPath, rootDirName);

                    offer.Path = $"/Offers/{offer.SupplierName} - {offer.DateOfIssue}";
                    _uploadService.Upload(rootDirPath, offer.File.FileName, offer.File);
                }
                
                _db.Offers.Add(offer);
                _db.SaveChanges();
                return RedirectToAction("Index");
            } 
            return View(offer);
        }
        
        
        [HttpGet]
        public IActionResult AddPosition(string offerId)
        {
            if (offerId == null)
                return NotFound();
            OfferPosition offerPosition = new OfferPosition(){OfferId =  offerId};
            return View(offerPosition);
        }
        
        [HttpPost]
        public IActionResult AddPosition(OfferPosition offerPosition)
        {
            if (ModelState.IsValid)
            {
                
                _db.OfferPositions.Add(offerPosition);
                _db.SaveChanges();
                return RedirectToAction("Index");
            } 
            return View(offerPosition);
        }

        
    }
}