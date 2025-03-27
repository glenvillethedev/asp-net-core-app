using ExpensesTracker.Models.Enums;
using ExpensesTracker.Models.IdentityEntities;
using ExpensesTracker.Services.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace ExpensesTracker.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = "NotLoggedIn")]
        public IActionResult SignIn()
        {
            try
            {
                ViewBag.SignInModel = new SignInRequest();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while rendering the SignIn view.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" 
                    , nameof(AccountController), nameof(SignIn));

                throw;
            }
        }

        [HttpPost]
        [Authorize(Policy = "NotLoggedIn")]
        public async Task<IActionResult> SignIn(SignInRequest request, string? returnUrl)
        {
            try
            {
                List<string> errors = new List<string>();
                ViewBag.SignInModel = request;
                if (!this.CredentialIsValid(request, out errors))
                {
                    ViewBag.ErrorList = errors;

                    return View();
                }
                else
                {
                    var result = await _signInManager.PasswordSignInAsync(request.Username, request.Password, request.IsPersistent, false);

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            return User.IsInRole(AppUserRoles.Admin.ToString()) ? RedirectToAction("Index", "Entry", new { area = "Admin" }) : RedirectToAction("Index", "Entry");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorList = new string[] { "Invalid Username or Password." };

                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while signing in.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" +
                    "Username: {Username}\n"
                    , nameof(AccountController), nameof(SignIn), request.Username);

                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("SignIn");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while signing out.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" +
                    "UserId: {Id}\n"
                    , nameof(AccountController), nameof(SignOut), User.FindFirstValue(ClaimTypes.NameIdentifier));

                throw;
            }
        }

        [HttpGet]
        [Authorize(Policy = "NotLoggedIn")]
        public IActionResult Register()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured rendering the Register view.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n"
                    , nameof(AccountController), nameof(Register));

                throw;
            }
        }

        [HttpPost]
        [Authorize(Policy = "NotLoggedIn")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {
                List<string> errors = new List<string>();
                ViewBag.RequestModel = request;

                if (!this.UserIsValid(request, out errors))
                {
                    ViewBag.ErrorList = errors;
                    return View();
                }
                else
                {
                    AppUser user = new AppUser()
                    {
                        Name = request.Name,
                        UserName = request.Email,
                        Email = request.Email,
                        PhoneNumber = request.Phone,
                        CreateDt = DateTime.Now
                    };

                    IdentityResult result = await _userManager.CreateAsync(user, request.Password);

                    if (result.Succeeded)
                    {
                        // Create and Assign Role to User
                        if (request.Role == AppUserRoles.Admin)
                        {
                            if (await _roleManager.FindByNameAsync(AppUserRoles.Admin.ToString()) is null)
                            {
                                await _roleManager.CreateAsync(new AppRole { Name = AppUserRoles.Admin.ToString() });
                            }
                            await _userManager.AddToRoleAsync(user, AppUserRoles.Admin.ToString());
                        }
                        else if (request.Role == AppUserRoles.User)
                        {

                            if (await _roleManager.FindByNameAsync(AppUserRoles.User.ToString()) is null)
                            {
                                await _roleManager.CreateAsync(new AppRole { Name = AppUserRoles.User.ToString() });
                            }
                            await _userManager.AddToRoleAsync(user, AppUserRoles.User.ToString());
                        }

                        // SignIn
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        return request.Role == AppUserRoles.Admin ? RedirectToAction("Index", "Home", new { area = "Admin" }) : RedirectToAction("Index", "Entry");
                    }
                    else
                    {
                        ViewBag.ErrorList = result.Errors.Select(x => x.Description);
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== ERROR ==========\n" +
                    "Message: An error occured while registering the user.\n" +
                    "Controller: {ControllerName}\n" +
                    "Action: {ActionName}\n" +
                    "Username: {Username}\n"
                    , nameof(AccountController), nameof(SignIn), request.Email);

                throw;
            }

        }

        public IActionResult AccessDenied()
        {
            return RedirectToAction("Index", "Entry");
        }

        [Route("/Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }

        #region Private Methods
        private bool UserIsValid(RegisterRequest registerRequest, out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(registerRequest.Name))
            {
                errors.Add("Name must not be empty.");
            }
            if (string.IsNullOrWhiteSpace(registerRequest.Email))
            {
                errors.Add("Email Address must not be empty.");
            }
            if (string.IsNullOrWhiteSpace(registerRequest.Phone))
            {
                errors.Add("Phone Number must not be empty.");
            }
            if (string.IsNullOrWhiteSpace(registerRequest.Password))
            {
                errors.Add("Password must not be empty.");
            }

            return errors.IsNullOrEmpty();
        }

        private bool CredentialIsValid(SignInRequest signInRequest, out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(signInRequest.Username))
            {
                errors.Add("Email must not be empty.");
            }
            if (string.IsNullOrWhiteSpace(signInRequest.Password))
            {
                errors.Add("Password must not be empty.");
            }

            return errors.IsNullOrEmpty();
        }
        #endregion
    }
}
