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
using WebStudio.Enums;
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

        
        [Fact]
        public void DetailCardGetMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);

            //Act
            var taskResult = controller.DetailCard2(cardId: _card.Id);
            taskResult.Wait();
            var result = taskResult.Result as ViewResult;
            
            //Assert
            Assert.NotNull(result);
            Assert.Equal("DetailCard2", result?.ViewName);
            db.Users.Remove(_user);
            db.Cards.Remove(_card);
            db.SaveChangesAsync();
        }


        [Fact]
        public void DeleteCardPostMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            db.Users.Add(_user);
            db.Cards.Add(_card);
            db.SaveChanges();
            
            //Act
            var taskResult = controller.DeleteCard(cardId: _card.Id);
            var bdResult = db.Cards.FirstOrDefault(c => c.Id == _card.Id);

            //Assert
            Assert.NotNull(taskResult);
            Assert.Equal(bdResult?.CardState, _card.CardState);
            db.Users.Remove(_user);
            db.Cards.Remove(_card);
            db.SaveChangesAsync();
        }

        
        [Fact]
        public void ChangeCardStatusFromNewToProcessPostMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            var model = ReturnDetailCardViewModel();
            string cardStateUpdate = CardState.Проработка.ToString();
            
            //Act
            var taskResult = controller.ChangeCardStatus(model: model, cardState: cardStateUpdate, bid: 1, cardId: model.Card.Id);
            var bdResult = db.Cards.FirstOrDefault(c => c.Id == model.Card.Id);

            //Assert
            Assert.NotNull(taskResult);
            Assert.Equal(bdResult?.CardState.ToString(), cardStateUpdate);
            Assert.Equal(bdResult?.ExecutorId, model.UserId);
            db.Users.Remove(_user);
            db.Cards.Remove(bdResult);
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
        private DetailCardViewModel ReturnDetailCardViewModel()
        {
            var db = ReturnsWebStudioDbContext();
            db.Users.Add(_user);
            db.Cards.Add(_card);
            db.SaveChanges();
            
            DetailCardViewModel model = new DetailCardViewModel()
            {
                CardId = _card.Id, Card = _card,Comment = new Comment(),
                UserId = _user.Id, Users = db.Users.ToList(), FileModels = new List<FileModel>()
            };
            return model;
        }

    }
}