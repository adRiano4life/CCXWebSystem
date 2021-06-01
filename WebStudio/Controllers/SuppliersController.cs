using System;
using System.Collections.Generic;
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
                    Address = model.Address
                };

                if (string.IsNullOrEmpty(model.Tags))
                {
                    string[] tagsString = model.Tags.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    supplier.Tags.AddRange(tagsString);
                    
                    // for (int i = 0; i < tagsString.Length; i++)
                    // {
                    //     supplier.Tags.Add(tagsString[i]);    
                    // }
                }
                
                _db.Suppliers.Add(supplier);
                _db.SaveChanges();
                return RedirectToAction("Index", "Cards"); //Index/Suppliers
            }

            return View(model);
            //return RedirectToAction("Create");
        }

        
    }
}