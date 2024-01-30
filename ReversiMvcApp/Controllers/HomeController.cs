using Google.Authenticator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using Newtonsoft.Json.Linq;
using ReversiMvcApp.Data;
using ReversiMvcApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ReversiMvcApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ReversiDbContext _SpelerContext;


        public HomeController(ILogger<HomeController> logger,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ReversiDbContext SpelerContext)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _SpelerContext = SpelerContext;

        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Beheerder, Mediator")]
        public IActionResult Admin()
        {
            
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Beheerder, Mediator")]
        public async Task<ActionResult> Admin (string userEmailField)
        {
            
            if (userEmailField != null) {
                var user = await _userManager.FindByEmailAsync(userEmailField);
                

                ViewData["UserName"] = "";
                ViewData["UserID"] = "";
                ViewData["Message"] = "";
                ViewData["SpelerClaim"] = false;
                ViewData["MediatorClaim"] = false;
                ViewData["BeheerderClaim"] = false;
                bool status = false;


                if (user != null)
                {

                    var ListClaims = _userManager.GetClaimsAsync(user).Result.ToList();
                    var listRollClaims = ListClaims.FindAll(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

                    foreach (var Claim in listRollClaims)
                    {
                        if (Claim.Value == "Speler")
                        {
                            ViewData["SpelerClaim"] = true;
                        }
                        else if (Claim.Value == "Mediator")
                        {
                            ViewData["MediatorClaim"] = true;
                        }
                        else if (Claim.Value == "Beheerder")
                        {
                            ViewData["BeheerderClaim"] = true;
                        }
                    };


                    ViewData["UserName"] = user.UserName;
                    ViewData["UserID"] = user.Id;
                    ViewData["status"] = true;
                }
                else
                {
                    ViewData["Message"] = "Gebruiker niet gevonden";
                    ViewData["status"] = status;
                }
            }

            return View();
        }

        [Authorize(Roles = "Beheerder, Mediator")]
        public async Task<ActionResult> EditUser(string UserId, string Username, string RoleSpeler, string RoleMediator, string RoleBeheerder)
        {
            //throw new Exception(Username + " : " + RoleSpeler + " : " + RoleMediator + " : " + RoleBeheerder + " : " + UserId);
            var user = await _userManager.FindByIdAsync(UserId);

            var oldUserName = user.UserName;

            if(oldUserName != Username)
            {
                await _userManager.SetUserNameAsync(user, Username);
                var speler = await _SpelerContext.Spelers.FirstOrDefaultAsync(m => m.Guid == user.Id);
                speler.Naam = Username;
                await _SpelerContext.SaveChangesAsync();
            }

            var ListClaims = _userManager.GetClaimsAsync(user).Result.ToList();
            var listRollClaims = ListClaims.FindAll(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            if (listRollClaims.Find(x => x.Value == "Speler") == null && RoleSpeler == "on")
            {
                var claimSpeler = new Claim(ClaimTypes.Role, "Speler");
                await _userManager.AddClaimAsync(user, claimSpeler);
            }
            else if (listRollClaims.Find(x => x.Value == "Speler") != null && RoleSpeler == null)
            {
                await _userManager.RemoveClaimAsync(user, listRollClaims.Find(x => x.Value == "Speler"));
            }

            if (listRollClaims.Find(x => x.Value == "Mediator") == null && RoleMediator == "on")
            {
                var claimMediator = new Claim(ClaimTypes.Role, "Mediator");
                await _userManager.AddClaimAsync(user, claimMediator);
            }
            else if (listRollClaims.Find(x => x.Value == "Mediator") != null && RoleMediator == null)
            {
                await _userManager.RemoveClaimAsync(user, listRollClaims.Find(x => x.Value == "Mediator"));
            }

            if (listRollClaims.Find(x => x.Value == "Beheerder") == null && RoleBeheerder == "on")
            {
                var claimBeheerder = new Claim(ClaimTypes.Role, "Beheerder");
                await _userManager.AddClaimAsync(user, claimBeheerder);
            }
            else if (listRollClaims.Find(x => x.Value == "Beheerder") != null && RoleBeheerder == null)
            {
                await _userManager.RemoveClaimAsync(user, listRollClaims.Find(x => x.Value == "Beheerder"));
            }


            return RedirectToAction("Admin", "Home");
        }

        [Authorize(Roles = "Beheerder")]
        public async Task<ActionResult> DeleteUser(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            await _userManager.DeleteAsync(user);
            var speler = await _SpelerContext.Spelers.FirstOrDefaultAsync(m => m.Guid == user.Id);
            _SpelerContext.Remove(speler);
            await _SpelerContext.SaveChangesAsync();

            return RedirectToAction("Admin", "Home");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<ActionResult> Verify2FA(string rememberMe)
        {
            var token = Request.Form["passcode"];
            
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            string UserUniqueKey = HttpContext.Session.GetString("UserUniqueKey");
            bool isValid = tfa.ValidateTwoFactorPIN(UserUniqueKey, token);
            if (isValid)
            {
                string Email = HttpContext.Session.GetString("Email");
                string Password = HttpContext.Session.GetString("Password");
                bool RememberMe = Convert.ToBoolean(HttpContext.Session.GetString("RememberMe"));
                var user = await _userManager.FindByEmailAsync(Email);
                var result = await _signInManager.PasswordSignInAsync(user, Password, RememberMe, lockoutOnFailure: false);
                //var result = await _signInManager.PasswordSignInAsync(Email, Password, RememberMe, lockoutOnFailure: false);
                _logger.LogInformation("User logged in.");
                return RedirectToAction("Index", "Spellen");
            }
            else
            {
                
                return Redirect($"/Identity/Account/Login");
            }
        }
    }
}