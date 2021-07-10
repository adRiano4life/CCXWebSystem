using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using WebStudio.Models;
using WebStudio.Services;
using WebStudio.ViewModels;

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
        

        public AccountController(WebStudioContext db, 
            UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager, 
            SignInManager<User> signInManager, 
            IHostEnvironment environment, 
            FileUploadService uploadService)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _environment = environment;
            _uploadService = uploadService;
            

        }
        
        
        [HttpGet]
        public async Task<IActionResult> Index(string userId)
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
                    
                    return View(model);
                }

                return NotFound();
            }
            return NotFound();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
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
            User user = _userManager.FindByEmailAsync(email).Result;
            if(user == null)
                return View("ErrorInConfirmEmail");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return View(result.Succeeded ? "ConfirmEmail" : "ErrorInConfirmEmail");
        }

        
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel {ReturnUrl = returnUrl});
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return View("ErrorUserNotFound");
                }
                if (user.LockoutEnabled)
                {
                    return View("ErrorLockedUser");
                }
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
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
                        ModelState.AddModelError("", 
                            "Вход возможен после подтверждения вашей эл.почты");
                        model.RepeatLinkForConfirmEmail = "repeat";
                    }
                    else
                    {
                        ModelState.AddModelError("", "Неверный логин или пароль");    
                    }
                }
            }
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Edit(string userId = null)
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
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(EditUserViewModel model)
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
                        return RedirectToAction("Index", new {userId = model.Id});
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);
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