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
       private readonly ISuppliersService _suppliersService;


       [Fact]
       public void ShouldFailValidationModel()
       {
           //Arrange
           //var controller = new SuppliersController();
           // var viewModel = new CreateSupplierViewModel();
           //
           // //Act
           // var viewResult = controller.Create(viewModel) as ViewResult;
           //
           // //Assert
           // Assert.NotNull(viewModel);
           // Assert.Equal(string.Empty, viewResult.ViewName);
           // Assert.False(viewResult.ViewData.ModelState.IsValid);
       } 

       
       // [Fact]
       // public void AddSupplierReturnsARedirectAndAddsSupplier()
       // {
       //     var mock = new Mock<ISuppliersService>();
       //     var controller = new SuppliersController(mock.Object);
       //     var model = new CreateSupplierViewModel
       //     {
       //         Id = Guid.NewGuid().ToString(),
       //         Name = "testSupplier",
       //         Email = "testSupplier@gmail.com",
       //         PhoneNumber = "8777",
       //         Website = "www.tengrinews.kz",
       //         Address = "г.Алматы",
       //         Tags = "testTagOne testTagTwo"
       //     };
       //     
       //     ViewResult result = controller.Create(model) as ViewResult;
       //     Assert.NotNull(result);
       //     Assert.NotNull(result.Model);
       //     //Assert.Equal(model, result.Model);
       //     
       //     if (!string.IsNullOrEmpty(model.Tags))
       //     {
       //         string[] tagsString =
       //             model.Tags.ToLower().Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
       //
       //         // var supplier = new Supplier
       //         // {
       //         //     Name = model.Name, Email = model.Email, PhoneNumber = model.PhoneNumber,
       //         //     Website = model.Website, Tags = new List<string>()
       //         // };
       //         // supplier.Tags.AddRange(tagsString.ToList());
       //     }
       //    
       //     var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
       //     Assert.Equal("Index", redirectToActionResult.ActionName);
       //
       //     //mock.Verify(r=>r.AddSupplier(supplier));
       //
       //     var viewResult = controller.Create(model) as ViewResult;
       //     Assert.Equal("Create", viewResult.ViewName);
       //     Assert.Equal(typeof(CreateSupplierViewModel), viewResult.Model.GetType());
       // }


    }
}