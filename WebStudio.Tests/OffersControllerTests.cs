using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NLog;
using WebStudio.Controllers;
using WebStudio.Models;
using WebStudio.Services;
using Xunit;

namespace WebStudio.Tests
{
    public class OffersControllerTests
    {
        private FileUploadService _uploadService = new FileUploadService();
        private IHostEnvironment _environment = new HostingEnvironment();
        private User _user = new User() { Email = "testUser@test.test"};
        private Card _card = new Card() { Name = "testCard", Number = "T-000/1"};
        
        
        [NonAction]
        private Offer ReturnNewOffer()
        {
            var offer = new Offer
            {
                CardNumber = _card.Number,
                CardId = _card.Id, 
                SupplierName = "ТОО \"Тестовый поставщик\"",
                DateOfIssue = DateTime.MinValue,
                Number = "000",
                File = CreateFileMock(),
                FileName = "test.pdf",
                Note = "тестовый коммент",
                UserId = _user.Id,
            };
            return offer;
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

        
        [NonAction]
        public IFormFile CreateFileMock()
        {
            var fileMock = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);

            var file = fileMock.Object;
            return file;
        }

        
        [Fact]
        public void IndexOffersTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            _environment.ContentRootPath = "\\var\\www\\CCXWebSystem\\WebStudio\\";
            var controller = new OffersController(db);
            int? page = 1;
            db.Users.Add(_user);
            db.Cards.Add(_card);
            var offer = ReturnNewOffer();
            db.Offers.Add(offer);
            var position = new CardPosition { CardId = offer.CardId, Card = offer.Card, StockNumber = "test", CodTNVED = "test", 
                Name = "test", Amount = 1, UnitPrice = 1, Measure = "test", TotalPrice = 1, Currency = "test", };
            db.Positions.Add(position);
            db.SaveChanges();
            
            //Act
            ViewResult result = controller.Index(null, null, null, 
                DateTime.MinValue, null, page : page) as ViewResult;

            //Assert
            Assert.Equal("Index", result?.ViewName);
            db.Offers.Remove(offer);
            db.Positions.Remove(position);
            db.Users.Remove(_user);
            db.Cards.Remove(_card);
            db.SaveChanges();
        }
        
        [Fact]
        public void CreateOfferGetMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new OffersController(db);
            db.Cards.Add(_card);
            db.SaveChanges();
            db.Users.Add(_user);
            db.SaveChanges();
            
            //Act
            ViewResult result = controller.Create(userId : _user.Id) as ViewResult;

            //Assert
            Assert.NotNull(_user);
            Assert.NotNull(_card);
            Assert.Equal("testUser@test.test", _user.Email);
            Assert.Equal("Create", result?.ViewName);

            db.Users.Remove(_user);
            db.Cards.Remove(_card);
            db.SaveChanges();
        }

        
        [Fact]
        public void CreateOfferPostMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            _environment.ContentRootPath = "\\var\\www\\CCXWebSystem\\WebStudio\\";
            var controller = new OffersController(db);
            db.Cards.Add(_card);
            db.SaveChanges();
            db.Users.Add(_user);
            db.SaveChanges();
            var offer = ReturnNewOffer();
            IFormFileCollection formFileCollection = new FormFileCollection();
            
            //Act
            var result = controller.Create(offer: offer, formFileCollection); 
            var bdresult = db.Offers.FirstOrDefault(o => o.Id == offer.Id);

            //Assert
            Assert.NotNull(bdresult);
            Assert.Equal(offer.Id, bdresult.Id);
            Assert.Equal(offer.CardNumber, bdresult.CardNumber);
            Assert.Equal(offer.CardId, bdresult.CardId);
            Assert.Equal(offer.UserId, bdresult.UserId);
            Assert.Equal(offer.Path, bdresult.Path);
            Assert.Equal(offer.DateOfIssue, bdresult.DateOfIssue);
            Assert.Equal(offer.Number, bdresult.Number);

            db.Users.Remove(_user);
            db.Cards.Remove(_card);
            db.Offers.Remove(bdresult);
            db.SaveChanges();
        }
        
    }
}