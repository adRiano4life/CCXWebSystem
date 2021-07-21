using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using WebStudio.Models;
using WebStudio.ViewModels;
using X.PagedList;

namespace WebStudio.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private WebStudioContext _db;
        private Logger _logger = LogManager.GetCurrentClassLogger();

        public SuppliersController(WebStudioContext db)
        {
            _db = db;
        }


        [HttpGet]
        [Authorize]
        public  IActionResult Index(string searchByName, string searchByTag, int? page)
        {
            try
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
                
                _logger.Info("Открыта таблица внесенных в базу поставщиков");
            
                int pageSize = 20;
                int pageNumber = (page ?? 1);
                return View("Index",suppliers.OrderBy(s=>s.Name).ToPagedList(pageNumber, pageSize));
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }

        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            try
            {
                CreateSupplierViewModel model = new CreateSupplierViewModel();
                _logger.Info("Открыта форма создания поставщика");
                return View("Create", model);
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
            
        }
        
        [HttpPost]
        [Authorize]
        public IActionResult Create(CreateSupplierViewModel model)
        {
            try
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
                    _logger.Info($"Поставщик {supplier.Name} добавлен в базу данных");
                    return RedirectToAction("Index", "Suppliers"); 
                }
                
                return View(model);
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }

        }

        
        [HttpGet]
        [Authorize]
        public IActionResult Edit(string id)
        {
            try
            {
                if (id == null)
                {
                    _logger.Warn("Не найден ID для поиска поставщика для редактирования в базе данных");
                    return NotFound();
                }
                
                Supplier supplier = _db.Suppliers.FirstOrDefault(r => r.Id == id);
                if (supplier == null)
                {
                    _logger.Warn("Поставщик для редактирования не найден в базе данных");
                    return NotFound();
                }

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
                
                _logger.Info("Открыта форма редактирования поставщика");
                return View("Edit", model);
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }

        }

        [HttpPost]
        [Authorize]
        public IActionResult Edit(CreateSupplierViewModel model)
        {
            try
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
                    _logger.Info($"Поставщик {supplier.Name} отредактирован");
                    return RedirectToAction("Index", "Suppliers");
                }

                return View(model);
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        
        [HttpGet]
        public IActionResult Delete(string id)
        {
            try
            {
                if (id is not null)
                {
                    Supplier supplier = _db.Suppliers.FirstOrDefault(s => s.Id == id);
                    if (supplier != null)
                    {
                        return View("Delete", supplier);
                    }
                    return NotFound();
                }
                return NotFound();
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }

        }


        [HttpPost]
        [ActionName("Delete")]
        public IActionResult ConfirmDelete(string id)
        {
            try
            {
                Supplier supplier = _db.Suppliers.FirstOrDefault(t => t.Id == id);
                if (supplier != null)
                {
                    _db.Entry(supplier).State = EntityState.Deleted;
                    _db.SaveChanges();
                    _logger.Info($"Поставщик {supplier.Name} удален из базы данных");
                }
                return RedirectToAction("Index", "Suppliers");
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> AddSupplierAjax(string supplierName, string supplierEmail, string supplierSite,
            string supplierPhone, string supplierAddress, string supplierTags, string supplierCardId)
        {
            try
            {
                List<string> tags = supplierTags.ToLower().Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
                Supplier supplier = new Supplier
                {
                    Name = supplierName,
                    Email = supplierEmail,
                    Website = supplierSite,
                    PhoneNumber = supplierPhone,
                    Address = supplierAddress,
                    Tags = tags
                };
                SearchSupplier searchSupplier = new SearchSupplier
                {
                    Name = supplierName,
                    Email = supplierEmail,
                    Website = supplierSite,
                    PhoneNumber = supplierPhone,
                    Address = supplierAddress,
                    Tags = tags,
                    CardId = supplierCardId,
                    Card = _db.Cards.FirstOrDefault(c=>c.Id == supplierCardId)
                };

                await _db.Suppliers.AddAsync(supplier);
                await _db.SearchSuppliers.AddAsync(searchSupplier);
                await _db.SaveChangesAsync();
                _logger.Info($"Поставщик {supplier.Name} добавлен в базу данных поставщиков через форму запроса");
                return PartialView("SuppliersAddPartialView",searchSupplier.Card);
            }
            catch (Exception e)
            {
                _logger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }
       
    }
}