using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
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
       //private readonly ISuppliersService _suppliersService;

       
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
       
       
       [Fact]
       public void CreateSupplierTest()
       {
           string DefaultConnection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=QWEqwe123@";
           var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
           var options = optionsBuilder.UseNpgsql(DefaultConnection).Options;
           var db = new WebStudioContext(options);
           //var mock = new Mock<ISuppliersService>();
           var controller = new SuppliersController(db);
           
           var model = ReturnCreateSupplierModel();
           
           var result = controller.Create(model); 
           var bdresult = db.Suppliers.FirstOrDefault(s => s.Name == model.Name);
           
           Assert.NotNull(result);
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
       public void EditSupplierTest()
       {
           string DefaultConnection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=QWEqwe123@";
           var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
           var options = optionsBuilder.UseNpgsql(DefaultConnection).Options;
           var db = new WebStudioContext(options);
           var controller = new SuppliersController(db);

           var model = ReturnCreateSupplierModel();

           var resultCreate = controller.Create(model);
           Assert.NotNull(resultCreate);
           
           var resultEdit = controller.Edit(model); 
           var bdresult = db.Suppliers.FirstOrDefault(s => s.Name == model.Name);
           
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
           string DefaultConnection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=QWEqwe123@";
           var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
           var options = optionsBuilder.UseNpgsql(DefaultConnection).Options;
           var db = new WebStudioContext(options);
           var controller = new SuppliersController(db);

           var model = ReturnCreateSupplierModel();
           var resultAdd = controller.Create(model); 
           var bdresultAdd = db.Suppliers.FirstOrDefault(s => s.Name == model.Name);
           
           Assert.NotNull(resultAdd);
           
           string id = bdresultAdd.Id;
           
           var resultDelete = controller.ConfirmDelete(id); 
           var bdresultDelete = db.Suppliers.FirstOrDefault(s => s.Name != bdresultAdd.Name);

           Assert.Null(bdresultDelete);
           
       }

    }
}