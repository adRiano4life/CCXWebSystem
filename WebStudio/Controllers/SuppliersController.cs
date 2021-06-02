using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebStudio.Models;
using WebStudio.ViewModels;

namespace WebStudio.Controllers
{
    public class SuppliersController : Controller
    {
        private WebStudioContext _db;

        public SuppliersController(WebStudioContext db)
        {
            _db = db;
        }

        
        [HttpGet]
        //[Authorize]
        public IActionResult Create()
        {
            CreateSupplierViewModel model = new CreateSupplierViewModel();
            return View(model);
        }
        
        [HttpPost]
        //[Authorize]
        public IActionResult Create(CreateSupplierViewModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                Supplier supplier = new Supplier
                {
                    Name = model.Name,
                    Email = model.Email,
                    Website = model.Website,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Tags = new List<string>()
                };

                if (!string.IsNullOrEmpty(model.Tags))
                {
                    string[] tagsString = model.Tags.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    supplier.Tags.AddRange(tagsString.ToList());
                }
                
                _db.Suppliers.Add(supplier);
                _db.SaveChanges();
                return RedirectToAction("Index", "Cards"); //Index/Suppliers
            }

            return View(model);
        }

        
    }
}