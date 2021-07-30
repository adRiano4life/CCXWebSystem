using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using NLog;
using WebStudio.Controllers;
using WebStudio.Models;
using WebStudio.ViewModels;
using Xunit;

namespace WebStudio.Tests
{
    public class CardsControllerTests
    {
        private WebStudioContext _db;
        private UserManager<User> _userManager;
        IWebHostEnvironment _appEnvironment;
        private ILogger<CardsController> _iLogger;
        Logger _nLogger = LogManager.GetCurrentClassLogger();
        private User _user = new User() { Email = "testUser@test.test"};
        private Card _card = new Card() { Name = "testCard", Number = "T-000/1"};


        // public CardsControllerTests(WebStudioContext db, UserManager<User> userManager, IWebHostEnvironment appEnvironment,
        //     ILogger<CardsController> iLogger)
        // {
        //     _db = ReturnsWebStudioDbContext();
        //     _userManager = userManager;
        //     _appEnvironment = appEnvironment;
        //     _iLogger = iLogger;
        // }
        
        [Fact]
        public void DetailCardMethodGetTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            db.Users.Add(_user);
            db.Cards.Add(_card);
            db.SaveChanges();
            DetailCardViewModel model = new DetailCardViewModel()
            {
                CardId = _card.Id, Card = _card,Comment = new Comment(),
                UserId = _user.Id, Users = db.Users.ToList(), FileModels = new List<FileModel>()
            };

            //Act
            var taskResult = controller.DetailCard2(cardId: _card.Id);
            taskResult.Wait();
            var result = taskResult.Result as ViewResult;
            var testModel = model; 
            
            //Assert
            Assert.NotNull(result);
            Assert.Equal("DetailCard2", result?.ViewName);
            Assert.Equal(model.CardId, testModel.CardId);
            db.Users.Remove(_user);
            db.Cards.Remove(_card);
            db.SaveChangesAsync();
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
        private void AddTestCardInDbContext()
        {
            Card _testCard = new Card
            {
                Number = "T-000/1", Name = "testCard", StartSumm = 1, DateOfAcceptingEnd = DateTime.Now,
                DateOfAuctionStart = DateTime.Now.AddDays(1), Initiator = "testInitiator", Broker = "testBroker",
                Auction = "test", State = "testState", BestPrice = "test", Links = new List<string>(), LinkNames = new List<string>(),
                Positions = new List<CardPosition>(), Bidding = 1, Comments = new List<Comment>(), ExecutorId = "",
                DateOfProcessingEnd = DateTime.Now, DateOfAuctionStartUpdated = DateTime.Now.AddDays(2)
            };
        }

    }
}