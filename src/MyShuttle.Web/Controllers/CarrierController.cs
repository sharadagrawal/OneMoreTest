
namespace MyShuttle.Web.Controllers
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Mvc;
    using Models;
    using Model;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Authorization;

    [Authorize]
    public class CarrierController : Controller
    {
        public SignInManager<ApplicationUser> SignInManager { get; private set; }

        public CarrierController(SignInManager<ApplicationUser> signInManager)
        {
            SignInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var signInStatus = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
                if (signInStatus.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }
                else
                { 
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        public IActionResult LogOff()
        {
            SignInManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Carrier");
            }
        }
    }
}