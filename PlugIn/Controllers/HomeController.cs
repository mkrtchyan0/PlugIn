using Microsoft.AspNetCore.Mvc;
using PlugIn.Contracts;
using PlugIn.Dal.AppContext;
using PlugIn.Models;
using PlugIn.Repository;
using System.Diagnostics;

namespace PlugIn.Controllers
{
	public class HomeController : Controller
    {
        private readonly IProjectRepository _projectRepository;
		private readonly PlugInDbContext _context;

		public HomeController(IProjectRepository projectRepository, PlugInDbContext context)
        {
            _projectRepository = projectRepository;
            _context = context;
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
                    ModelState.AddModelError("",
                        result.Errors.Select(e => "Code: " + e.Description + " Description: " + e.Code + " ,")
                .Aggregate((e, delimiter) => e + delimiter));
            }
            return View(form);
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
                    CookieOptions options = new CookieOptions
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
