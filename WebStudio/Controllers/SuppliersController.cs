using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebStudio.Models;
using WebStudio.ViewModels;
using X.PagedList;

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
        [Authorize]
        public  IActionResult Index(string searchByName, string searchByTag, int? page)
        {
            List<Supplier> suppliers = _db.Suppliers.ToList();
            
            if (searchByName != null)
            {
                suppliers = suppliers.Where(s => s.Name.ToLower().Contains(searchByName.ToLower())).ToList();
                ViewBag.searchByName = searchByName;
            }
            
            if (searchByTag != null)
            {
                List<Supplier> search = new List<Supplier>();
                foreach (var supplier in suppliers)
                {
                    foreach (var tag in supplier.Tags)
                    {
                        if(tag.ToLower().Contains(searchByTag.ToLower()))
                            search.Add(supplier);
                    }
                }
                suppliers = search;
                ViewBag.searchByTag = searchByTag;
            }
            
            int pageSize = 2;
            int pageNumber = (page ?? 1);
            return View(suppliers.OrderBy(s=>s.Name).ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            CreateSupplierViewModel model = new CreateSupplierViewModel();
            return View(model);
        }
        
        [HttpPost]
        [Authorize]
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
                return RedirectToAction("Index", "Suppliers"); 
            }
            return View(model);
        }

        
        [HttpGet]
        [Authorize]
        public IActionResult Edit(string id)
        {
            if (id == null) return NotFound();
            Supplier supplier = _db.Suppliers.FirstOrDefault(r => r.Id == id);
            if (supplier == null) return NotFound();

           CreateSupplierViewModel model = new CreateSupplierViewModel
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Email = supplier.Email,
                PhoneNumber = supplier.PhoneNumber,
                Website = supplier.Website,
                Address = supplier.Address
            };
           
           if (supplier.Tags.Count != 0)
           {
               model.Tags = String.Join(" ", supplier.Tags.ToArray());;
           }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Edit(CreateSupplierViewModel model)
        {
            if (model == null) return NotFound();
            if (ModelState.IsValid && model.Id != null)
            {
                Supplier supplier = _db.Suppliers.FirstOrDefault(s => s.Id == model.Id);
                if (supplier == null) return NotFound();
                supplier.Name = model.Name;
                supplier.Email = model.Email;
                supplier.Website = model.Website;
                supplier.PhoneNumber = model.PhoneNumber;
                supplier.Address = model.Address;
                supplier.Tags = new List<string>();
                
                if (!string.IsNullOrEmpty(model.Tags))
                {
                    string[] tagsString = model.Tags.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    supplier.Tags.AddRange(tagsString.ToList());
                }
                
                _db.Suppliers.Update(supplier);
                _db.SaveChanges();
                return RedirectToAction("Index", "Suppliers");
            }

            return View(model);
        }

        
        // [HttpGet]
        // public IActionResult Delete(string id)
        // {
        //     if (id is not null)
        //     {
        //         Supplier supplier = _db.Suppliers.FirstOrDefault(s => s.Id == id);
        //         if (supplier != null)
        //         {
        //             return View(supplier);
        //         }
        //         return NotFound();
        //     }
        //     return NotFound();
        // }


        [HttpPost]
        [ActionName("Delete")]
        public IActionResult ConfirmDelete(string id)
        {
            Supplier supplier = _db.Suppliers.FirstOrDefault(t => t.Id == id);
            if (supplier != null)
            {
                _db.Entry(supplier).State = EntityState.Deleted;
                _db.SaveChanges();
            }
            return RedirectToAction("Index", "Suppliers");
        }
        
        
       
    }
}