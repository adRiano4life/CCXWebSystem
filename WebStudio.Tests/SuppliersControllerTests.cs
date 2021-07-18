using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using WebStudio.Controllers;
using WebStudio.Models;
using WebStudio.ViewModels;
using Xunit;

namespace WebStudio.Tests
{
    public class SuppliersControllerTests
    {

       
       [NonAction]
       private CreateSupplierViewModel ReturnCreateSupplierModel()
       {
           
           var model = new CreateSupplierViewModel
           {
               Id = Guid.NewGuid().ToString(),
               Name = "testSupplier",
               Email = "testSupplier@gmail.com",
               PhoneNumber = "8777",
               Website = "www.tengrinews.kz",
               Address = "г.Алматы",
               Tags = "testTagOne testTagTwo".ToLower()
           };
           return model;
       }

       [NonAction]
       private WebStudioContext ReturnsWebStudioDbContext()
       {
           string DefaultConnection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=QWEqwe123@";
           var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
           var options = optionsBuilder.UseNpgsql(DefaultConnection).Options;
           var db = new WebStudioContext(options);
           return db;
       }
       
       
       [Fact]
       public void IndexSupplierTest()
       {
           //Arrange
           var db = ReturnsWebStudioDbContext();
           var controller = new SuppliersController(db);
           string searchByName = "", searchByTag= "";
           int? page = 1;
           
           //Act
           ViewResult result = controller.Index(searchByName : searchByName, searchByTag : searchByTag, page : page) as ViewResult;

           //Assert
           Assert.Equal("Index", result?.ViewName);
       }
       
       [Fact]
       public void CreateSupplierGetMethodTest()
       {
           //Arrange
           var db = ReturnsWebStudioDbContext();
           var controller = new SuppliersController(db);
           
           //Act
           ViewResult result = controller.Create() as ViewResult;

           //Assert
           Assert.Equal("Create", result?.ViewName);
       }
       
       [Fact]
       public void CreateSupplierPostMethodTest()
       {
           //Arrange
           var db = ReturnsWebStudioDbContext();
           var controller = new SuppliersController(db);
           var model = ReturnCreateSupplierModel();
           
           //Act
           var result = controller.Create(model: model); 
           var bdresult = db.Suppliers.FirstOrDefault(s => s.Name == model.Name);
           
           //Assert
           Assert.NotNull(bdresult);
           Assert.Equal(model.Name, bdresult.Name);
           Assert.Equal(model.Email, bdresult.Email);
           Assert.Equal(model.Website, bdresult.Website);
           Assert.Equal(model.PhoneNumber, bdresult.PhoneNumber);
           Assert.Equal(model.Address, bdresult.Address);
           
           //bdresult tags from list to string for assert with model tags
           StringBuilder bdresultTags = new StringBuilder();

           if (model.Tags.Length > 0 && bdresult.Tags.Count > 0)
           {
               for(int i = 0; i < bdresult.Tags.Count; i++)
               {
                   bdresultTags.Append(bdresult.Tags[i] + " ");
               }
               Assert.Equal(model.Tags, bdresultTags.ToString().TrimEnd());
           }

           db.Suppliers.Remove(bdresult);
           db.SaveChanges();
       }

       
       [Fact]
       public void EditSupplierGetMethodTest()
       {
           //Arrange
           var db = ReturnsWebStudioDbContext();
           var controller = new SuppliersController(db);
           
           CreateSupplierViewModel testModel = ReturnCreateSupplierModel();
           Supplier testSupplier = new Supplier
           {
               Name = testModel.Name, Email = testModel.Email, PhoneNumber = testModel.PhoneNumber,
               Website = testModel.Website, Address = testModel.Address, Tags = new List<string>()
           }; 
           
           if (!string.IsNullOrEmpty(testModel.Tags))
           {
               string[] tagsString = testModel.Tags.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
               testSupplier.Tags.AddRange(tagsString.ToList());
           }
           
           db.Suppliers.Add(testSupplier);
           db.SaveChanges();
           
           //Act
           ViewResult result = controller.Edit(id : testSupplier.Id) as ViewResult;

           //Assert
           Assert.Equal("Edit", result?.ViewName);
           
           db.Suppliers.Remove(testSupplier);
           db.SaveChanges();
       }
       
       [Fact]
       public void EditSupplierPostMethodTest()
       {
           //Arrange
           var db = ReturnsWebStudioDbContext();
           var controller = new SuppliersController(db);
           var model = ReturnCreateSupplierModel();

           //Act
           var resultCreate = controller.Create(model);
           Assert.NotNull(resultCreate);
           
           var resultEdit = controller.Edit(model); 
           var bdresult = db.Suppliers.FirstOrDefault(s => s.Name == model.Name);
           
           //Assert
           Assert.NotNull(resultEdit);
           Assert.Equal(model.Name, bdresult.Name);
           Assert.Equal(model.Email, bdresult.Email);
           Assert.Equal(model.Website, bdresult.Website);
           Assert.Equal(model.PhoneNumber, bdresult.PhoneNumber);
           Assert.Equal(model.Address, bdresult.Address);
           
           //bdresult tags from list to string for assert with model tags
           StringBuilder bdresultTags = new StringBuilder();

           if (model.Tags.Length > 0 && bdresult.Tags.Count > 0)
           {
               for(int i = 0; i < bdresult.Tags.Count; i++)
               {
                   bdresultTags.Append(bdresult.Tags[i] + " ");
               }
               Assert.Equal(model.Tags, bdresultTags.ToString().TrimEnd());
           }

           db.Suppliers.Remove(bdresult);
           db.SaveChanges();
       }


       [Fact]
       public void DeleteSupplierTest()
       {
           //Arrange
           var db = ReturnsWebStudioDbContext();
           var controller = new SuppliersController(db);
           
           CreateSupplierViewModel testModel = ReturnCreateSupplierModel();
           Supplier testSupplier = new Supplier { Name = testModel.Name, Email = testModel.Email, 
               PhoneNumber = testModel.PhoneNumber, Website = testModel.Website, Address = testModel.Address, 
               Tags = new List<string>()}; 
           
           if (!string.IsNullOrEmpty(testModel.Tags))
           {
               string[] tagsString = testModel.Tags.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
               testSupplier.Tags.AddRange(tagsString.ToList());
           }
           
           db.Suppliers.Add(testSupplier);
           db.SaveChanges();

           //Act
           ViewResult result = controller.Delete(id : testSupplier.Id) as ViewResult;

           //Assert
           Assert.Equal("Delete", result?.ViewName);

           db.Suppliers.Remove(testSupplier);
           db.SaveChanges();
       }
       
       
       [Fact]
       public void ConfirmDeleteSupplierTest()
       {
           //Arrange
           var db = ReturnsWebStudioDbContext();
           var controller = new SuppliersController(db);
           var model = ReturnCreateSupplierModel();
       
           //Act
           var resultAdd = controller.Create(model); 
           var bdresultAdd = db.Suppliers.FirstOrDefault(s => s.Name == model.Name);
           
           //Assert
           Assert.NotNull(resultAdd);
           
           string id = bdresultAdd.Id;
           
           var resultDelete = controller.ConfirmDelete(id); 
           var bdresultDelete = db.Suppliers.FirstOrDefault(s => s.Name != bdresultAdd.Name);

           Assert.Null(bdresultDelete);
       }

       [Fact]
       public async Task AddSupplierAjaxTest()
       {
           //Arrange
           var db = ReturnsWebStudioDbContext();
           var controller = new SuppliersController(db);
           var model = ReturnCreateSupplierModel();
           Card testCard = new Card{ Number = "T-000", Name = "testCard"};
           await db.Cards.AddAsync(testCard);
           await db.SaveChangesAsync();
           
           string supplierName = model.Name, supplierEmail = model.Email, supplierSite = model.Website,
           supplierPhone = model.PhoneNumber, supplierAddress = model.Address, supplierTags = model.Tags, 
           supplierCardId = db.Cards.FirstOrDefault(c=>c.Number == testCard.Number).Id;

           //Act
           var resultAddSupplierAjax = controller.AddSupplierAjax( supplierName,  supplierEmail,  supplierSite,
                supplierPhone,  supplierAddress,  supplierTags, supplierCardId);
           var bdResultSupplier = db.Suppliers.FirstOrDefault(s => s.Name == supplierName);
           var bdResultSearchSupplier = db.SearchSuppliers.FirstOrDefault(s => s.Name == supplierName);
           
           //Assert
           Assert.NotNull(resultAddSupplierAjax);
           Assert.NotNull(bdResultSupplier);
           Assert.NotNull(bdResultSearchSupplier);

           db.Cards.Remove(testCard);
           db.Suppliers.Remove(bdResultSupplier);
           db.SearchSuppliers.Remove(bdResultSearchSupplier);
           await db.SaveChangesAsync();
       }
    }
}