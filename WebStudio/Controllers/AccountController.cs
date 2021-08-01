using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _environment;
        private readonly FileUploadService _uploadService;
        private ILogger<AccountController> _iLogger;
        Logger _nLogger = LogManager.GetCurrentClassLogger();
        

        public AccountController(WebStudioContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, 
            SignInManager<User> signInManager, IWebHostEnvironment environment, FileUploadService uploadService, 
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
                        _nLogger.Info($"Вход в личный кабинет пользователем {user.Surname} {user.Name} - успешно");
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
            try
            {
                if (ModelState.IsValid)
                {
                    //string path = Path.Combine(_environment.ContentRootPath, "wwwroot/Images/Avatars");
                    string avatarPath = $"/Images/Avatars/defaultavatar.jpg";
                    if (model.File != null)
                    {
                        avatarPath = $"/Images/Avatars/{model.File.FileName}";
                        using (var fileStream = new FileStream(_environment.WebRootPath + avatarPath, FileMode.Create))
                        {
                            await model.File.CopyToAsync(fileStream);
                        }

                        FileModel file = new FileModel
                        {
                            Name = model.File.FileName,
                            Path = avatarPath
                        };

                        _db.Files.Add(file);
                        _db.SaveChanges();
                        //_uploadService.Upload(path, model.File.FileName, model.File);
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
                    await _userManager.AddToRoleAsync(user, "user");
                    if (result.Succeeded)
                    {
                        await SendLinkForConfirmEmail(user.Email);
                        _nLogger.Info($"Регистрация пользователя {user.Surname} {user.Name} - успешно. " +
                                      $"Идет отправка ссылки для подтверждения email");
                        return RedirectToAction("Index", new {userId = user.Id});
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            _nLogger.Warn(
                                $"Регистрация пользователя: ошибка при регистрации {user.Surname} {user.Name}");
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }

                return View(model);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }


        public async Task<IActionResult> SendLinkForConfirmEmail(string email)
        {
            try
            {
                User user = _userManager.FindByEmailAsync(email).Result;
                if (user == null)
                {
                    _nLogger.Warn($"Отправка ссылки для подтверждения email: пользователь с email {email} не найден");
                    return Ok("Неверная эл.почта");
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", new {token, email = user.Email},
                    Request.Scheme);
                EmailService emailService = new EmailService();
                await emailService.SendEmailAfterRegister(user.Email, confirmationLink);

                _nLogger.Info(
                    $"Отправлена ссылка пользователю {user.Surname} {user.Name} на эл.почту {user.Email} для подтверждения");
                return RedirectToAction("Index", new {userId = user.Id});
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }
        

        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            try
            {
                User user = _userManager.FindByEmailAsync(email).Result;
                if (user == null)
                {
                    _nLogger.Warn("Подтверждение email: пользователь не найден");
                    return View("ErrorInConfirmEmail");
                }

                var result = await _userManager.ConfirmEmailAsync(user, token);
                _nLogger.Info($"Подтверждение email: пользователем {user.Surname} {user.Name} подтвержден адрес эл.почты");
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
                        _nLogger.Warn("Попытка входа заблокированным пользователем");
                        return View("ErrorLockedUser");
                    }
                    
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        _nLogger.Info($"Вход в приложение пользователем {user.Surname} {user.Name} - успешно");
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
                            _nLogger.Info($"Попытка входа в приложение пользователем {user.Surname} {user.Name}, " +
                                          $"у которого эл.почта не подтверждена");
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
                _nLogger.Info($"Попытка входв пользователя с эл.почтой {model.Email}");
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
                _nLogger.Info($"Открыта страница редактирования профиля пользователя {user.Surname} {user.Name}");
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
                        string oldEmail = !user.Email.Equals(model.Email) ? user.Email : "";
                        string oldName = !user.Name.Equals(model.Name) ? user.Name : "";
                        string oldSurName = !user.Surname.Equals(model.Surname) ? user.Surname : "";
                        string oldPhone = !user.PhoneNumber.Equals(model.PhoneNumber) ? user.PhoneNumber : "";
                        string oldAvatarPath = "";
                        
                        user.Name = model.Name;
                        user.Surname = model.Surname;
                        user.Email = model.Email;
                        user.UserName = model.Email;
                        user.PhoneNumber = model.PhoneNumber;

                        if (model.File != null)
                        {
                            string avatarPath = $"/Images/Avatars/{model.File.FileName}";
                            using (var fileStream = new FileStream(_environment.WebRootPath + avatarPath, FileMode.Create))
                            {
                                await model.File.CopyToAsync(fileStream);
                            }

                            FileModel file = new FileModel
                            {
                                Name = model.File.FileName,
                                Path = avatarPath
                            };

                            _db.Files.Add(file);
                            _db.SaveChanges();
                            
                            model.AvatarPath = avatarPath;
                            oldAvatarPath = !user.AvatarPath.Equals(model.AvatarPath) ? user.AvatarPath : "";
                            user.AvatarPath = model.AvatarPath;
                        }

                        var result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            _nLogger.Info($"Редактирование профиля пользователя {user.Surname} {user.Name} - успешно");
                            if(!string.IsNullOrEmpty(oldEmail))
                                _nLogger.Info($"Эл.почта:  до редактирования {oldEmail}, после редактирования {model.Email}");
                            if(!string.IsNullOrEmpty(oldName))
                                _nLogger.Info($"Имя: до редактирования {oldName}, после редактирования {model.Name}");
                            if(!string.IsNullOrEmpty(oldSurName))
                                _nLogger.Info($"Фамилия: до редактирования {oldSurName}, после редактирования {model.Surname}");
                            if(!string.IsNullOrEmpty(oldEmail))
                                _nLogger.Info($"Телефон: до редактирования {oldPhone}, после редактирования {model.PhoneNumber}");
                            if(!string.IsNullOrEmpty(oldAvatarPath))
                                _nLogger.Info($"Изменено фото пользователя");
                            
                            return RedirectToAction("Index", new {userId = model.Id});
                        }

                        foreach (var error in result.Errors)
                        {
                            _nLogger.Warn($"Редактирование профиля пользователя {user.Surname} {user.Name} - " +
                                          $"ошибка при добавлении измененых данных");
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                _nLogger.Info($"Редактирование профиля пользователя: ошибка валидации данных пользователя с эл.почтой {model.Email}");
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
            try
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
                        _nLogger.Info($"Открыта страница изменения пароля пользователя {user.Surname} {user.Name}");
                        return View(model);
                    }
                    _nLogger.Warn($"Изменение пароля: пользователь не найден по id {userId}");
                    return NotFound();
                }
                _nLogger.Warn($"Изменение пароля: в id пользователя передано пустое значение");
                return NotFound();
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
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            try
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
                            _nLogger.Info($"Изменение пароля пользователя {user.Surname} {user.Name} - успешно");
                            return View("SuccessChangePassword");
                        }

                        foreach (var error in result.Errors)
                        {
                            _nLogger.Warn($"Изменение пароля пользователя {user.Surname} {user.Name} - ошибка " +
                                          $"валидации пользователя при изменении пароля");
                            ModelState.AddModelError("", "Пользователь не существует");
                        }
                    }
                }
                _nLogger.Warn($"Изменение пароля пользователя с email {model.Email} - неудачно");
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
        public async Task<IActionResult> ChangeRole(string userId, string loginUserId, string roleName)
        {
            try
            {
                if (userId == null)
                {
                    _nLogger.Warn("Изменение роли пользователя: в id пользователя передано пустое значение");
                    return NotFound();
                }
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
                    _nLogger.Info($"Изменение роли пользователя {user.Surname} {user.Name} - успешно");
                    return RedirectToAction("Index", "Account", new {userId = loginUserId});
                }
                _nLogger.Warn("Изменение роли пользователя: пользователь не найден");
                return NotFound();
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _nLogger.Info("Выход из учетной записи: успешно");
                return RedirectToAction("Index", "Cards");
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> LockOrUnlockUser(string userId, string adminId)
        {
            try
            {
                if (userId == null || adminId == null)
                {
                    _nLogger.Warn("Блокировка учетной записи: в id передано пустое значение");
                    return NotFound();
                }

                User user = _userManager.FindByIdAsync(userId).Result;
                User admin = _userManager.FindByIdAsync(adminId).Result;

                if (user == null || admin == null)
                {
                    _nLogger.Warn($"Блокировка учетной записи: пользователь и админ не найдены по id = \n{userId}\n{adminId}");
                    return NotFound();
                }

                if (user.LockoutEnabled == false)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTime.Now.AddYears(100);    
                    _nLogger.Info($"Разблокировка учетной записи {user.Surname} {user.Name}");
                }
                else
                {
                    user.LockoutEnabled = false;
                    user.LockoutEnd = DateTime.Now.AddMinutes(-1);
                    _nLogger.Info($"Блокировка учетной записи {user.Surname} {user.Name}");
                }
            
                await _userManager.UpdateAsync(user);
                _nLogger.Info($"УСПЕШНО");
                return RedirectToAction("Index", new {userId = adminId});
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            try
            {
                _nLogger.Info("Открыта страница восстановления пароля");
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
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        _nLogger.Warn($"Восстановление пароля: пользователь не найден по адресу эл.почты {model.Email}");
                        return View("ErrorUserNotFound");
                    }

                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var callBackUrl = Url.Action("ResetPassword", "Account", new {userId = user.Id, code = code},
                        protocol: HttpContext.Request.Scheme);
                    EmailService emailService = new EmailService();
                    await emailService.SendEmailForResetPassword(model.Email, "Сброс пароля", callBackUrl);
                    
                    _nLogger.Info($"Восстановление пароля: отправлена ссылка на эл.почту {model.Email}");
                    return View("ForgotPasswordConfirm");
                }
                _nLogger.Info($"Восстановление пароля: данные пользователя не валидны {model.Email}");
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
        public IActionResult ResetPassword(string code = null)
        {
            try
            {
                return code == null ? View("Error") : View();
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    User user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        _nLogger.Warn($"Сброс пароля: пользователь c почтой {model.Email} не найден");
                        return View("ErrorUserNotFound");
                    }

                    var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                    if (result.Succeeded)
                    {
                        _nLogger.Info($"Сброс пароля пользователем {user.Surname} {user.Name}: успешно ");
                        return View("ResetPasswordConfirm");
                    }

                    foreach (var error in result.Errors)
                    {
                        _nLogger.Warn($"Сброс пароля: ошибка при сбросе пароля пользователем {user.Surname} {user.Name}");
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                return View(model);
            }
            catch (Exception e)
            {
                _nLogger.Error($"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                _iLogger.Log(LogLevel.Error, $"Внимание, ошибка: {e.Message} => {e.StackTrace}");
                throw;
            }
        }
    }
}