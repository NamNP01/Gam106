using bai1.Data;
using bai1.Models;
using bai1.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace bai1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Register()
        {
            var regions = _context.Regions.ToListAsync();
            RegisterVM registerVM = new()
            {
                Email = "",
                Password = "",
                Name = "",
                LinkAvatar= "",
                RegionId = 1,
                Regions = new SelectList(regions.Result, "RegionId", "Name"),
            };
            return View(registerVM);
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            ModelState.Remove("LinkAvatar");
            ModelState.Remove("Regions");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = registerVM.Email,
                    Email = registerVM.Email,
                    RegionId = registerVM.RegionId,
                    Name = registerVM.Name,
                };
                var result = await _userManager.CreateAsync(user, registerVM.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);

                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid");
            }
            var regions = await _context.Regions.ToListAsync();
            registerVM.Regions = new SelectList(regions, "RegionId", "Name");
            return View(registerVM);
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            ModelState.Remove("twoFactorCode");
            ModelState.Remove("twoFactorRecoveryCode");
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(loginRequest.Email, loginRequest.Password, true, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user =await _userManager.FindByEmailAsync(loginRequest.Email);
                    var roles =await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("AccessDenied", "Home");
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt");
            }
            return View(loginRequest);
        }
    }
}