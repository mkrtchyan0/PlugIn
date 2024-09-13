using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using PlugIn.Contracts;
using PlugIn.Dal.AppContext;
using PlugIn.Models;
using PlugIn.Repository;
using System.Diagnostics;

namespace PlugIn.Controllers
{
    public class HomeController(IProjectRepository projectRepository, PlugInDbContext context) : Controller
    {
        EmailService emailService = new EmailService
            ("smtp.gmail.com",
            587,
            "smarttaxam@gmail.com",
            "qmkcevtbftufhlrm");

        private readonly IProjectRepository _projectRepository = projectRepository;
        private readonly PlugInDbContext _context = context;

        public IActionResult WrongPassword()
        {
            return View();
        }
        //[HttpGet("confirmemail/{email}/{token}")]
        [Route("ConfirmEmail")]
        public IActionResult ConfirmEmail(string email, string token)
        {
                _projectRepository.ConfirmEmailAsync(email, token);

            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult InvalidPassword()
        {
            return View();
        }
        public IActionResult IndexAuthorized()
        {
            if (ModelState.IsValid)
            {
                if (Request.Cookies["UserLogin"] != null)
                {
                    var userlogin = Request.Cookies["UserLogin"];

                    var user = _context.Users.FirstOrDefault(x => x.Email == userlogin);

                    return View(user);
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult UserLogOut()
        {
            if (ModelState.IsValid)
            {
                _projectRepository.LogOutAsync();

                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public IActionResult UserRegister()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserRegister(RegisterForm form)
        {
            if (ModelState.IsValid)
            {
                var result = await _projectRepository.Register(form);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                    return RedirectToAction("Index");
            }
            return View("WrongPassword");
        }
        public IActionResult UsersHomePage()
        {
            return View();
        }
        public IActionResult AddApplication()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddApplication(ProjectForm form)
        {
            if (ModelState.IsValid)
            {
                if (Request.Cookies["UserLogin"] != null)
                {
                    var userlogin = Request.Cookies["UserLogin"];

                    form.UserLogin = userlogin;
                }
                var result = await _projectRepository.AddProjectAsync(form);

                if (result.Succeeded)
                {
                    return RedirectToAction("IndexAuthorized");
                }
                else
                    ModelState.AddModelError("",
                        result.Errors.Select(e => "Code: " + e.Description + " Description: " + e.Code + " ,")
                .Aggregate((e, delimiter) => e + delimiter));
            }
            return View(form);
        }
        [HttpGet]
        public IActionResult UserLogin()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLogin(LoginForm form)
        {
            if (ModelState.IsValid)
            {
                var result = await _projectRepository.LoginAsync(form);

                if (!result.Succeeded)
                {
                    return RedirectToAction("InvalidPassword");
                }
                else
                {
                    CookieOptions options = new()
                    {
                        Expires = DateTimeOffset.UtcNow.AddMinutes(15),

                        IsEssential = true
                    };
                    Response.Cookies.Append("UserLogin", form.Email, options);

                    return RedirectToAction("IndexAuthorized");
                }
            }
            return RedirectToAction("Index");
        }
        public IActionResult MyProjects()
        {
            if (Request.Cookies["UserLogin"] != null)
            {
                var userlogin = Request.Cookies["UserLogin"];
                var projects = _context.AppProject.Where(x => x.UserLogin == userlogin).ToList();
                return View(projects);
            }
            return View("IndexAuthorized");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
 }
