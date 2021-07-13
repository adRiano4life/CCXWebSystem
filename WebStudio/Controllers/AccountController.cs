using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using WebStudio.Models;
using WebStudio.Services;
using WebStudio.ViewModels;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace WebStudio.Controllers
{
    public class AccountController : Controller
    {
        private WebStudioContext _db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IHostEnvironment _environment;
        private readonly FileUploadService _uploadService;
        private ILogger<AccountController> _iLogger;
        Logger _nLogger = LogManager.GetCurrentClassLogger();
        

        public AccountController(WebStudioContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, 
            SignInManager<User> signInManager, IHostEnvironment environment, FileUploadService uploadService, 
            ILogger<AccountController> iLogger)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _environment = environment;
            _uploadService = uploadService;
            _iLogger = iLogger;
        }
        
        
        [HttpGet]
        public async Task<IActionResult> Index(string userId)
        {
            try
            {
                if (userId != null)
                {
                    User user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        IndexUsersViewModel model = new IndexUsersViewModel{User = user};
                        if (User.IsInRole("admin"))
                        {
                            model.Users = _db.Users.ToList();
                        }
                        _nLogger.Info($"Осуществлен вход в личный кабинет пользователем id = {user.Id}");
                        return View(model);
                    }
                    _nLogger.Warn("Вход в личный кабинет: пользователь не найден по id");
                    return NotFound();
                }
                _nLogger.Warn("Вход в личный кабинет: в Id пользователя не передано значение");
                return NotFound();
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        
        [HttpGet]
        public IActionResult Register()
        {
            try
            {
                _nLogger.Info($"Открыта форма регистрации нового пользователя");
                return View();
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string path = Path.Combine(_environment.ContentRootPath, "wwwroot/Images/Avatars");
                string avatarPath = $"/Images/Avatars/defaultavatar.jpg";
                if (model.File != null)
                {
                    avatarPath = $"/Images/Avatars/{model.File.FileName}";
                    _uploadService.Upload(path, model.File.FileName, model.File);
                }

                model.AvatarPath = avatarPath;

                User user = new User
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    UserName = model.Email,
                    AvatarPath = model.AvatarPath,
                    RoleDisplay = "user"
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    SendLinkForConfirmEmail(user.Email);
                    return RedirectToAction("Index", new{userId = user.Id});
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
            
        }

        
        public async Task<IActionResult> SendLinkForConfirmEmail(string email)
        {
            User user = _userManager.FindByEmailAsync(email).Result;
            if (user == null) return Ok("Неверная  эл.почта");
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", new {token, email = user.Email},
                Request.Scheme);
            EmailService emailService = new EmailService();
            bool emailResponse = emailService.SendEmailAfterRegister(user.Email, confirmationLink);
                
            if (emailResponse)
                return RedirectToAction("Index", new {userId = user.Id});
            else
            {
                // log email failed
            }
            return View("Login");
        }

        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            try
            {
                User user = _userManager.FindByEmailAsync(email).Result;
                if (user == null)
                {
                    _nLogger.Warn("Пользователь не найден");
                    return View("ErrorInConfirmEmail");
                }

                var result = await _userManager.ConfirmEmailAsync(user, token);
                _nLogger.Info($"Пользователем с id = {user.Id} подтвержден адрес эл.почты");
                return View(result.Succeeded ? "ConfirmEmail" : "ErrorInConfirmEmail");
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            try
            {
                _nLogger.Info($"Открыта форма входа");
                return View(new LoginViewModel {ReturnUrl = returnUrl});
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        _nLogger.Warn("Попытка входа: пользователь не найден");
                        return View("ErrorUserNotFound");
                    }
                    if (user.LockoutEnabled)
                    {
                        _nLogger.Warn("Попытка аутентификации заблокированным пользователем");
                        return View("ErrorLockedUser");
                    }
                    
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        _nLogger.Info($"Вход в приложение пользователем с id {user.Id}");
                        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }
                        return RedirectToAction("Index", "Cards");
                    }
                    else
                    {
                        if (result.IsNotAllowed)
                        {
                            _nLogger.Info($"Попытка входа в приложение пользователем с id {user.Id}, у которого эл.почта не подтверждена");
                            ModelState.AddModelError("", 
                                "Вход возможен после подтверждения вашей эл.почты");
                            model.RepeatLinkForConfirmEmail = "repeat";
                        }
                        else
                        {
                            _nLogger.Info($"Неудачная попытка входа в приложение пользователем с эл.почтой {model.Email}");
                            ModelState.AddModelError("", "Неверный логин или пароль");    
                        }
                    }
                }
                _nLogger.Info($"Вход пользователем с эл.почтой {model.Email}");
                return View(model);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }
        

        [HttpGet]
        [Authorize]
        public IActionResult Edit(string userId = null)
        {
            try
            {
                User user = _userManager.FindByIdAsync(userId).Result;

                EditUserViewModel model = new EditUserViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    PhoneNumber =  user.PhoneNumber,
                    AvatarPath = user.AvatarPath
                };
                _nLogger.Info($"Открыта страница редактирования профиля пользователя с id {user.Id}");
                return View(model);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }
        

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User user = await _userManager.FindByIdAsync(model.Id);
                    if (user != null)
                    {
                        user.Name = model.Name;
                        user.Surname = model.Surname;
                        user.Email = model.Email;
                        user.UserName = model.Email;
                        user.PhoneNumber = model.PhoneNumber;

                        if (model.File != null)
                        {
                            string path = Path.Combine(_environment.ContentRootPath, "wwwroot\\Images\\Avatars");
                            string avatarPath = $"/Images/Avatars/{model.File.FileName}";
                            _uploadService.Upload(path, model.File.FileName, model.File);
                            model.AvatarPath = avatarPath;
                            user.AvatarPath = model.AvatarPath;
                        }

                        var result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            _nLogger.Info($"Профиля пользователя с id {user.Id} успешно отредактирован");
                            return RedirectToAction("Index", new {userId = model.Id});
                        }

                        foreach (var error in result.Errors)
                        {
                            _nLogger.Warn($"Возникла ошибка при редактировании профиля пользователя с id {user.Id}");
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                _nLogger.Info($"Возникла ошибка при редактировании профиля пользователя с эл.почтой {model.Email}");
                return View(model);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }
        

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string userId)
        {
            if (userId != null)
            {
                User user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    ChangePasswordViewModel model = new ChangePasswordViewModel
                    {
                        Id = user.Id,
                        Email = user.Email
                    };
                    return View(model);
                }

                return NotFound();
            }

            return NotFound();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    var passwordValidator =
                        HttpContext.RequestServices.GetService(typeof(IPasswordValidator<User>)) as
                            IPasswordValidator<User>;
                    var passwordHasher =
                        HttpContext.RequestServices.GetService(typeof(IPasswordHasher<User>)) as IPasswordHasher<User>;
                    var result = await passwordValidator.ValidateAsync(_userManager, user, model.NewPassword);
                    if (result.Succeeded)
                    {
                        user.PasswordHash = passwordHasher.HashPassword(user, model.NewPassword);
                        await _userManager.UpdateAsync(user);
                        return RedirectToAction("Index");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", "Пользователь не существует");
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string loginUserId, string roleName)
        {
            User user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                switch (roleName)
                {
                    case "admin":
                        await _userManager.RemoveFromRoleAsync(user, "user");
                        await _userManager.AddToRoleAsync(user, "admin");
                        user.RoleDisplay = roleName;
                        break;
                    case "user":
                        await _userManager.RemoveFromRoleAsync(user, "admin");
                        await _userManager.AddToRoleAsync(user, "user");
                        user.RoleDisplay = roleName;
                        break;
                }
                await _userManager.UpdateAsync(user);
                return RedirectToAction("Index", "Account", new {userId = loginUserId});
            }

            return NotFound();
        }

        
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Cards");
        }

        
        [HttpPost]
        public async Task<IActionResult> LockOrUnlockUser(string userId, string adminId)
        {
            if(userId == null || adminId == null) return NotFound();

            User user = _userManager.FindByIdAsync(userId).Result;
            User admin = _userManager.FindByIdAsync(adminId).Result;

            if (user == null || admin == null) return NotFound();

            if (user.LockoutEnabled == false)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTime.Now.AddYears(100);    
            }
            else
            {
                user.LockoutEnabled = false;
                user.LockoutEnd = DateTime.Now.AddMinutes(-1);
            }
            
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index", new {userId = adminId});
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return View("ErrorUserNotFound");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callBackUrl = Url.Action("ResetPassword", "Account", new {userId = user.Id, code = code},
                    protocol: HttpContext.Request.Scheme);
                EmailService emailService = new EmailService();
                await emailService.SendEmailForResetPassword(model.Email, "Сброс пароля", callBackUrl);
                return View("ForgotPasswordConfirm");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return View("ErrorUserNotFound");
                }

                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return View("ResetPasswordConfirm");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}