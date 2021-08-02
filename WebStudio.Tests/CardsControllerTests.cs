using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
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
        private static User _user = new User() { Email = "testUser@test.test"};
        private Card _card = new Card() { Name = "testCard", Number = "T-000/1", DateOfProcessingEnd = DateTime.Now, DateOfAuctionStartUpdated = DateTime.Now.AddDays(1) };

        
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
        public async Task ChangeCardStatusFromNewToProcessPostMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            _userManager = CreateUserManagerMock();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            var model = ReturnDetailCardViewModel();
            string cardStateProcess = CardState.Проработка.ToString();
            model.Card.ExecutorId = _user.Id;
            
            //Act
            var taskResult = await controller.ChangeCardStatus(model: model, cardState: cardStateProcess, bid: 1, cardId: model.Card.Id);
            var bdResult = db.Cards.FirstOrDefault(c => c.Id == model.Card.Id);

            //Assert
            Assert.NotNull(taskResult);
            Assert.Equal(bdResult?.CardState.ToString(), cardStateProcess);
            Assert.Equal(bdResult?.ExecutorId, model.UserId);
            db.Users.Remove(_user);
            db.Cards.Remove(bdResult);
            await db.SaveChangesAsync();
        }
        
        
        [Fact]
        public async Task ChangeCardStatusFromProcessToPkoPostMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            var model = ReturnDetailCardViewModel();
            model.Card.CardState = CardState.Проработка;
            string cardStateUpdate = CardState.ПКО.ToString();
            
            //Act
            var taskResult = await controller.ChangeCardStatus(model: model, cardState: cardStateUpdate, bid: 1, cardId: model.Card.Id);
            var bdResult = await db.Cards.FirstOrDefaultAsync(c => c.Id == model.Card.Id);
            
            //Assert
            Assert.NotNull(taskResult);
            Assert.Equal(bdResult?.CardState.ToString(), cardStateUpdate);
            Assert.Equal(bdResult?.DateOfProcessingEnd, model.Card.DateOfProcessingEnd);
            Assert.Equal(bdResult?.DateOfAuctionStartUpdated, model.Card.DateOfAuctionStartUpdated);
            db.Users.Remove(_user);
            db.Cards.Remove(bdResult);
            await db.SaveChangesAsync();
        }
        
        
        [Fact]
        public async Task ChangeCardStatusFromPkoToAuctionPostMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            var model = ReturnDetailCardViewModel();
            model.Card.CardState = CardState.ПКО;
            string cardStateUpdate = CardState.Торги.ToString();
            
            //Act
            var taskResult = await controller.ChangeCardStatus(model: model, cardState: cardStateUpdate, bid: 1, cardId: model.Card.Id);
            var bdResult = await db.Cards.FirstOrDefaultAsync(c => c.Id == model.Card.Id);
            
            //Assert
            Assert.NotNull(taskResult);
            Assert.Equal(bdResult?.CardState.ToString(), cardStateUpdate);
            Assert.Equal(bdResult?.DateOfProcessingEnd, model.Card.DateOfProcessingEnd);
            Assert.Equal(bdResult?.DateOfAuctionStartUpdated, model.Card.DateOfAuctionStartUpdated);
            db.Users.Remove(_user);
            db.Cards.Remove(bdResult);
            await db.SaveChangesAsync();
        }

     
        [Fact]
        public async Task ChangeCardStatusFromAuctionToDeletePostMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            var model = ReturnDetailCardViewModel();
            model.Card.CardState = CardState.Торги;
            string cardStateUpdate = CardState.Удалена.ToString();
            
            //Act
            var taskResult = await controller.ChangeCardStatus(model: model, cardState: cardStateUpdate, bid: 1, cardId: model.Card.Id);
            var bdResult = await db.Cards.FirstOrDefaultAsync(c => c.Id == model.Card.Id);
            
            //Assert
            Assert.NotNull(taskResult);
            Assert.Equal(bdResult?.CardState.ToString(), cardStateUpdate);
            db.Users.Remove(_user);
            db.Cards.Remove(bdResult);
            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task ChangeCardStatusFromDeleteToNewPostMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            var model = ReturnDetailCardViewModel();
            model.Card.CardState = CardState.Удалена;
            string cardStateUpdate = CardState.Новая.ToString();
            
            //Act
            var taskResult = await controller.ChangeCardStatus(model: model, cardState: cardStateUpdate, bid: 1, cardId: model.Card.Id);
            var bdResult = await db.Cards.FirstOrDefaultAsync(c => c.Id == model.Card.Id);
            
            //Assert
            Assert.NotNull(taskResult);
            Assert.Equal(bdResult?.CardState.ToString(), cardStateUpdate);
            db.Users.Remove(_user);
            db.Cards.Remove(bdResult);
            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task ChangeCardStatusFromAuctionToLostPostMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            var model = ReturnDetailCardViewModel();
            model.Card.CardState = CardState.Торги;
            string cardStateUpdate = CardState.Проиграна.ToString();
            
            //Act
            var taskResult = await controller.ChangeCardStatus(model: model, cardState: cardStateUpdate, bid: 555, cardId: model.Card.Id);
            var bdResult = await db.Cards.FirstOrDefaultAsync(c => c.Id == model.Card.Id);
            var cloneCardFromBd =
                await db.HistoryOfVictoryAndLosing.FirstOrDefaultAsync(c => c.Number == bdResult.Number);
            
            //Assert
            Assert.NotNull(taskResult);
            Assert.NotNull(cloneCardFromBd);
            Assert.Equal(cardStateUpdate, bdResult?.CardState.ToString());
            Assert.Equal(555, bdResult?.Bidding);
            Assert.Equal(555, cloneCardFromBd.Bidding);
            db.Users.Remove(_user);
            db.Cards.Remove(bdResult);
            db.HistoryOfVictoryAndLosing.Remove(cloneCardFromBd);
            await db.SaveChangesAsync();
        }
        
        
        [Fact]
        public async Task ChangeCardStatusFromAuctionToWonPostMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            var model = ReturnDetailCardViewModel();
            model.Card.CardState = CardState.Торги;
            string cardStateUpdate = CardState.Выиграна.ToString();
            
            //Act
            var taskResult = await controller.ChangeCardStatus(model: model, cardState: cardStateUpdate, bid: 1, cardId: model.Card.Id);
            var bdResult = await db.Cards.FirstOrDefaultAsync(c => c.Id == model.Card.Id);
            var cloneCardFromBd =
                await db.HistoryOfVictoryAndLosing.FirstOrDefaultAsync(c => c.Number == bdResult.Number);
            
            //Assert
            Assert.NotNull(taskResult);
            Assert.NotNull(cloneCardFromBd);
            Assert.Equal(cardStateUpdate, bdResult?.CardState.ToString());
            Assert.Equal(model.Card.Number, cloneCardFromBd.Number);
            db.Users.Remove(_user);
            db.Cards.Remove(bdResult);
            db.HistoryOfVictoryAndLosing.Remove(cloneCardFromBd);
            await db.SaveChangesAsync();
        }

        
        [Fact]
        public async Task ChangeCardStatusFromWonToActivePostMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            var model = ReturnDetailCardViewModel();
            model.Card.CardState = CardState.Выиграна;
            string cardStateUpdate = CardState.Активна.ToString();
            
            //Act
            var taskResult = await controller.ChangeCardStatus(model: model, cardState: cardStateUpdate, bid: 1, cardId: model.Card.Id);
            var bdResult = await db.Cards.FirstOrDefaultAsync(c => c.Id == model.Card.Id);
            
            //Assert
            Assert.NotNull(taskResult);
            Assert.Equal(cardStateUpdate, bdResult?.CardState.ToString());
            db.Users.Remove(_user);
            db.Cards.Remove(bdResult);
            await db.SaveChangesAsync();
        }
        
        
        [Fact]
        public async Task ChangeCardStatusFromActiveToClosedPostMethodTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            var model = ReturnDetailCardViewModel();
            model.Card.CardState = CardState.Активна;
            string cardStateUpdate = CardState.Закрыта.ToString();
            
            //Act
            var taskResult = await controller.ChangeCardStatus(model: model, cardState: cardStateUpdate, bid: 1, cardId: model.Card.Id);
            var bdResult = await db.Cards.FirstOrDefaultAsync(c => c.Id == model.Card.Id);
            
            //Assert
            Assert.NotNull(taskResult);
            Assert.Equal(cardStateUpdate, bdResult?.CardState.ToString());
            db.Users.Remove(_user);
            db.Cards.Remove(bdResult);
            await db.SaveChangesAsync();
        }

        //Отложено до момента закрытия тикета 134
        //т.к. экшн будет изменен после данного тикета
        // аналогично с тикетом 132
        // [Fact]
        // public void AllCardsListMethodGetTest()
        // {
        //     //Arrange
        //     var db = ReturnsWebStudioDbContext();
        //     var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
        //     CardState state = CardState.Новая;
        //     //Act
        //     var taskResult = controller.AllCardsList(page: 1, from: DateTime.Now, to: DateTime.Now.AddDays(1), filter: state.ToString(), sort: CardState.Новая );
        //     var bdResult = db.Cards.All(c => c.CardState == state);
        // }

        [Fact]
        public async Task CommentMethodPostTest()
        {
            //Arrange
            var db = ReturnsWebStudioDbContext();
            _userManager = CreateUserManagerMock();
            var controller = new CardsController(db, _userManager, _appEnvironment, _iLogger);
            var model = ReturnDetailCardViewModel();
            model.Comment.Message = "test comment";
            model.Comment.CardId = model.CardId;
            model.Comment.UserId = model.UserId;
            
            //Act
            var taskResult = await controller.Comment(model: model);
            var bdResult = db.Comments.FirstOrDefault(c => c.Message == model.Comment.Message);

            //Assert
            Assert.NotNull(taskResult);
            Assert.NotNull(bdResult);
            Assert.Equal(bdResult?.Message, model.Comment.Message);
            db.Comments.FirstOrDefaultAsync(c => c.Message == model.Comment.Message).Result.CardId = null;
            db.Comments.FirstOrDefaultAsync(c => c.Message == model.Comment.Message).Result.UserId = null;
            await db.SaveChangesAsync();
            db.Comments.Remove(bdResult);
            
            await db.SaveChangesAsync();
            db.Users.Remove(_user);
            await db.SaveChangesAsync();
            db.Cards.Remove(model.Card);// не удаляется из бд
            await db.SaveChangesAsync();
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

        
        private UserManager<User> CreateUserManagerMock()
        {
            //
            Mock<IUserPasswordStore<User>> userPasswordStore = new Mock<IUserPasswordStore<User>>();
            userPasswordStore.Setup(s => s.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(IdentityResult.Success));

            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();

            //this should be keep in sync with settings in ConfigureIdentity in WebApi -> Startup.cs
            idOptions.Lockout.AllowedForNewUsers = false;
            idOptions.Password.RequireDigit = true;
            idOptions.Password.RequireLowercase = true;
            idOptions.Password.RequireNonAlphanumeric = true;
            idOptions.Password.RequireUppercase = true;
            idOptions.Password.RequiredLength = 8;
            idOptions.Password.RequiredUniqueChars = 1;

            idOptions.SignIn.RequireConfirmedEmail = false;

            // Lockout settings.
            idOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            idOptions.Lockout.MaxFailedAccessAttempts = 5;
            idOptions.Lockout.AllowedForNewUsers = true;


            options.Setup(o => o.Value).Returns(idOptions);
            var userValidators = new List<IUserValidator<User>>();
            UserValidator<User> validator = new UserValidator<User>();
            userValidators.Add(validator);

            var passValidator = new PasswordValidator<User>();
            var pwdValidators = new List<IPasswordValidator<User>>();
            pwdValidators.Add(passValidator);
            var userManager = new UserManager<User>(userPasswordStore.Object, options.Object, new PasswordHasher<User>(),
                userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<User>>>().Object);
            
            return userManager;
        }
    }
}