using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

       [Fact]
       public void AddSupplierTest()
       {
           string DefaultConnection = "Server=127.0.0.1;Port=5432;Database=WebStudio;User Id=postgres;Password=123";
           var optionsBuilder = new DbContextOptionsBuilder<WebStudioContext>();
           var options = optionsBuilder.UseNpgsql(DefaultConnection).Options;
           var db = new WebStudioContext(options);
           var mock = new Mock<ISuppliersService>();
           //var controller = new SuppliersController(mock.Object, db);
           var controller = new SuppliersController(db);
           
           var model = new CreateSupplierViewModel
           {
               Id = Guid.NewGuid().ToString(),
               Name = "testSupplier",
               Email = "testSupplier@gmail.com",
               PhoneNumber = "8777",
               Website = "www.tengrinews.kz",
               Address = "г.Алматы",
               Tags = "testTagOne testTagTwo"
           };

           var result = controller.Create(model); 
           var bdresult = db.Suppliers.FirstOrDefault(s => s.Name == model.Name);
           
           Assert.NotNull(result);
           Assert.Equal(model.Website, bdresult.Website);
           Assert.Equal(model.Website, bdresult.Website);

           db.Remove(bdresult);
           db.SaveChanges();
           
           //
           // //mock.Verify(r=>r.AddSupplier(supplier));
           
           // when model not valid
           // var viewResult = controller.Create(model) as ViewResult;
           // Assert.Equal("Create", viewResult.ViewName);
           // Assert.Equal(typeof(CreateSupplierViewModel), viewResult.Model.GetType());
       }


    }
}