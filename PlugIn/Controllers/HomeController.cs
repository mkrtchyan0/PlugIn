using Microsoft.AspNetCore.Mvc;
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
        public IActionResult ConfirmEmail()
        {
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
                    await SendEmailAsync(form.Email);

                    return RedirectToAction("Index");
                }
                else
                    return RedirectToAction("index");
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

        [HttpPost]
        public async Task SendEmailAsync(string toEmail)
        {
            var html = "< !DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n<head>\r\n<meta name=\"viewport\" content=\"width=device-width\" />\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />\r\n<title>Really Simple HTML Email Template</title>\r\n<style>\r\n/* ------------------------------------- \r\n\t\tGLOBAL \r\n------------------------------------- */\r\n* { \r\n\tmargin:0;\r\n\tpadding:0;\r\n\tfont-family: \"Helvetica Neue\", \"Helvetica\", Helvetica, Arial, sans-serif; \r\n\tfont-size: 100%;\r\n\tline-height: 1.6;\r\n}\r\n\r\nimg { \r\n\tmax-width: 100%; \r\n}\r\n\r\nbody {\r\n\t-webkit-font-smoothing:antialiased; \r\n\t-webkit-text-size-adjust:none; \r\n\twidth: 100%!important; \r\n\theight: 100%;\r\n}\r\n\r\n\r\n/* ------------------------------------- \r\n\t\tELEMENTS \r\n------------------------------------- */\r\na { \r\n\tcolor: #348eda;\r\n}\r\n\r\n.btn-primary{\r\n\ttext-decoration:none;\r\n\tcolor: #FFF;\r\n\tbackground-color: #348eda;\r\n\tborder:solid #348eda;\r\n\tborder-width:10px 20px;\r\n\tline-height:2;\r\n\tfont-weight:bold;\r\n\tmargin-right:10px;\r\n\ttext-align:center;\r\n\tcursor:pointer;\r\n\tdisplay: inline-block;\r\n\tborder-radius: 25px;\r\n}\r\n\r\n.btn-secondary {\r\n\ttext-decoration:none;\r\n\tcolor: #FFF;\r\n\tbackground-color: #aaa;\r\n\tborder:solid #aaa;\r\n\tborder-width:10px 20px;\r\n\tline-height:2;\r\n\tfont-weight:bold;\r\n\tmargin-right:10px;\r\n\ttext-align:center;\r\n\tcursor:pointer;\r\n\tdisplay: inline-block;\r\n\tborder-radius: 25px;\r\n}\r\n\r\n.last { \r\n\tmargin-bottom: 0;\r\n}\r\n\r\n.first{\r\n\tmargin-top: 0;\r\n}\r\n\r\n.padding{\r\n\tpadding:10px 0;\r\n}\r\n\r\n\r\n/* ------------------------------------- \r\n\t\tBODY \r\n------------------------------------- */\r\ntable.body-wrap { \r\n\twidth: 100%;\r\n\tpadding: 20px;\r\n}\r\n\r\ntable.body-wrap .container{\r\n\tborder: 1px solid #f0f0f0;\r\n}\r\n\r\n\r\n/* ------------------------------------- \r\n\t\tFOOTER \r\n------------------------------------- */\r\ntable.footer-wrap { \r\n\twidth: 100%;\t\r\n\tclear:both!important;\r\n}\r\n\r\n.footer-wrap .container p {\r\n\tfont-size:12px;\r\n\tcolor:#666;\r\n\t\r\n}\r\n\r\ntable.footer-wrap a{\r\n\tcolor: #999;\r\n}\r\n\r\n\r\n/* ------------------------------------- \r\n\t\tTYPOGRAPHY \r\n------------------------------------- */\r\nh1,h2,h3{\r\n\tfont-family: \"Helvetica Neue\", Helvetica, Arial, \"Lucida Grande\", sans-serif; line-height: 1.1; margin-bottom:15px; color:#000;\r\n\tmargin: 40px 0 10px;\r\n\tline-height: 1.2;\r\n\tfont-weight:200; \r\n}\r\n\r\nh1 {\r\n\tfont-size: 36px;\r\n}\r\nh2 {\r\n\tfont-size: 28px;\r\n\ttext-align:center;\r\n}\r\nh3 {\r\n\tfont-size: 22px;\r\n}\r\n\r\np, ul, ol { \r\n\tmargin-bottom: 10px; \r\n\tfont-weight: normal; \r\n\tfont-size:15px;\r\n}\r\n\r\nul li, ol li {\r\n\tmargin-left:5px;\r\n\tlist-style-position: inside;\r\n}\r\n\r\nstrong{\r\n\tfont-size:18px;\r\n\tfont-weight:normal;\r\n}\r\n\r\n/* --------------------------------------------------- \r\n\t\tRESPONSIVENESS\r\n\t\tNuke it from orbit. It's the only way to be sure. \r\n------------------------------------------------------ */\r\n\r\n/* Set a max-width, and make it display as block so it will automatically stretch to that width, but will also shrink down on a phone or something */\r\n.container {\r\n\tdisplay:block!important;\r\n\tmax-width:600px!important;\r\n\tmargin:0 auto!important; /* makes it centered */\r\n\tclear:both!important;\r\n}\r\n\r\n/* Set the padding on the td rather than the div for Outlook compatibility */\r\n.body-wrap .container{\r\n\tpadding:20px;\r\n}\r\n\r\n/* This should also be a block element, so that it will fill 100% of the .container */\r\n.content {\r\n\tmax-width:600px;\r\n\tmargin:0 auto;\r\n\tdisplay:block; \r\n}\r\n\r\n/* Let's make sure tables in the content area are 100% wide */\r\n.content table { \r\n\twidth: 100%; \r\n}\r\n\r\n.center{\r\n\ttext-align:center;\r\n}\r\n\r\n.logo{\r\n\tdisplay:inline-block;\r\n\twidth:399px;\r\n\theight:85px;\r\n\tmax-width:90%;\r\n}\r\n\r\n.footnote{\r\n\tfont-size:14px;\r\n\tcolor:#444;\r\n}\r\n\r\n@media all and (min-resolution: 192dpi), (-webkit-min-device-pixel-ratio: 2), (min--moz-device-pixel-ratio: 2), (-o-min-device-pixel-ratio: 2/1), (min-device-pixel-ratio: 2), (min-resolution: 2dppx){\r\n\t.logo{\r\n\t\tbackground-image:url(chartblocks@2x.png);\r\n\t\tbackground-size:100% auto;\r\n\t\tbackground-repeat:no-repeat;\r\n\t}\r\n\t.logo img{\r\n\t\tvisibility:hidden;\r\n\t}\r\n}\r\n\r\n</style>\r\n</head>\r\n \r\n<body bgcolor=\"#f6f6f6\">\r\n\r\n<!-- body -->\r\n<table class=\"body-wrap\">\r\n\t<tr>\r\n\t\t<td></td>\r\n\t\t<td class=\"container\" bgcolor=\"#FFFFFF\">\r\n\t\t\t<div class=\"content\">\r\n\t\t\t<table>\r\n\t\t\t\t<tr>\r\n\t\t\t\t\t<td class=\"center\">\r\n\t\t\t\t\t\t<div class=\"logo\">\r\n\t\t\t\t\t\t\t<img src=\"chartblocks.png\">\r\n\t\t\t\t\t\t</div>\r\n\t\t\t\t\t</td>\r\n\t\t\t\t</tr>\r\n\t\t\t\t<tr>\r\n\t\t\t\t\t<td>\r\n\t\t\t\t\t\t<h2>Activate your account</h2>\r\n\t\t\t\t\t\t<p>Hi Sam,\r\n\t\t\t\t\t\t<p>Thanks for creating a ChartBlocks account. To use the email address test@test.com to log in to ChartBlocks please verify your email address by clicking the button below.</p>\r\n\t\t\t\t\t\t<table>\r\n\t\t\t\t\t\t\t<tr>\r\n\t\t\t\t\t\t\t\t<td class=\"padding center\">\r\n\t\t\t\t\t\t\t\t\t<p><a href=\"https://github.com/leemunroe/html-email-template\" class=\"btn-primary\">Verify my email address</a></p>\r\n\t\t\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t\t</tr>\r\n\t\t\t\t\t\t</table>\r\n\t\t\t\t\t\t<p>Thanks,<br><strong>Sam</strong><br> ChartBlocks Support.</p>\r\n\t\t\t\t\t\t<p><a href=\"http://twitter.com/chartblocks\">Follow @chartblocks on Twitter</a>\r\n\t\t\t\t\t</td>\r\n\t\t\t\t</tr>\r\n\t\t\t\t<tr>\r\n\t\t\t\t\t<td>\r\n\t\t\t\t\t\t<p class=\"footnote\">We hope you find ChartBlocks easy to use but if you have any problems drop an email to support@chartblocks.com and we'll get straight back to you.</p>\r\n\t\t\t\t\t</td>\r\n\t\t\t\t</tr>\r\n\t\t\t</table>\r\n\t\t\t</div>\r\n\t\t\t<!-- /content -->\r\n\t\t\t\t\t\t\t\t\t\r\n\t\t</td>\r\n\t\t<td></td>\r\n\t</tr>\r\n</table>\r\n<!-- /body -->\r\n\r\n<!-- footer -->\r\n<table class=\"footer-wrap\">\r\n\t<tr>\r\n\t\t<td></td>\r\n\t\t<td class=\"container\">\r\n\t\t\t\r\n\t\t\t<!-- content -->\r\n\t\t\t<div class=\"content\">\r\n\t\t\t\t<table>\r\n\t\t\t\t\t<tr>\r\n\t\t\t\t\t\t<td align=\"center\">\r\n\t\t\t\t\t\t\t<p>Don't like these annoying emails? <a href=\"#\"><unsubscribe>Unsubscribe</unsubscribe></a>.\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table>\r\n\t\t\t</div><!-- /content -->\r\n\t\t\t\t\r\n\t\t</td>\r\n\t\t<td></td>\r\n\t</tr>\r\n</table>\r\n<!-- /footer -->\r\n\r\n</body>\r\n</html>";
            string verifyemail = "vardanmkrtchyan171@gmail.com";
            var verifytoken = "804430";

            var url = $"https://localhost:44394/confirmemail?email={verifyemail}&token={verifytoken}/";

            var fullname = "Vardan Mkrtchyan";
            var firmName = "Bim Consulting";

            var body2 = $"<!DOCTYPE>\r\n<html>\r\n<head>\r\n<meta name=\"viewport\" content=\"width=device-width\" />\r\n<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />\r\n<title>Really Simple HTML Email Template</title>\r\n<style>\r\n/* ------------------------------------- \r\n\t\tGLOBAL \r\n------------------------------------- */\r\n* {{ \r\n\tmargin:0;\r\n\tpadding:0;\r\n\tfont-family: \"Helvetica Neue\", \"Helvetica\", Helvetica, Arial, sans-serif; \r\n\tfont-size: 100%;\r\n\tline-height: 1.6;\r\n}}\r\n\r\nimg {{ \r\n\tmax-width: 100%; \r\n}}\r\n\r\nbody {{\r\n\t-webkit-font-smoothing:antialiased; \r\n\t-webkit-text-size-adjust:none; \r\n\twidth: 100%!important; \r\n\theight: 100%;\r\n}}\r\n\r\n\r\n/* ------------------------------------- \r\n\t\tELEMENTS \r\n------------------------------------- */\r\na {{ \r\n\tcolor: #348eda;\r\n}}\r\n\r\n.btn-primary{{\r\n\ttext-decoration:none;\r\n\tcolor: #FFF;\r\n\tbackground-color: #348eda;\r\n\tborder:solid #348eda;\r\n\tborder-width:10px 20px;\r\n\tline-height:2;\r\n\tfont-weight:bold;\r\n\tmargin-right:10px;\r\n\ttext-align:center;\r\n\tcursor:pointer;\r\n\tdisplay: inline-block;\r\n\tborder-radius: 25px;\r\n}}\r\n\r\n.btn-secondary {{\r\n\ttext-decoration:none;\r\n\tcolor: #FFF;\r\n\tbackground-color: #aaa;\r\n\tborder:solid #aaa;\r\n\tborder-width:10px 20px;\r\n\tline-height:2;\r\n\tfont-weight:bold;\r\n\tmargin-right:10px;\r\n\ttext-align:center;\r\n\tcursor:pointer;\r\n\tdisplay: inline-block;\r\n\tborder-radius: 25px;\r\n}}\r\n\r\n.last {{ \r\n\tmargin-bottom: 0;\r\n}}\r\n\r\n.first{{\r\n\tmargin-top: 0;\r\n}}\r\n\r\n.padding{{\r\n\tpadding:10px 0;\r\n}}\r\n\r\n\r\n/* ------------------------------------- \r\n\t\tBODY \r\n------------------------------------- */\r\ntable.body-wrap {{ \r\n\twidth: 100%;\r\n\tpadding: 20px;\r\n}}\r\n\r\ntable.body-wrap .container{{\r\n\tborder: 1px solid #f0f0f0;\r\n}}\r\n\r\n\r\n/* ------------------------------------- \r\n\t\tFOOTER \r\n------------------------------------- */\r\ntable.footer-wrap {{ \r\n\twidth: 100%;\t\r\n\tclear:both!important;\r\n}}\r\n\r\n.footer-wrap .container p {{\r\n\tfont-size:12px;\r\n\tcolor:#666;\r\n\t\r\n}}\r\n\r\ntable.footer-wrap a{{\r\n\tcolor: #999;\r\n}}\r\n\r\n\r\n/* ------------------------------------- \r\n\t\tTYPOGRAPHY \r\n------------------------------------- */\r\nh1,h2,h3{{\r\n\tfont-family: \"Helvetica Neue\", Helvetica, Arial, \"Lucida Grande\", sans-serif; line-height: 1.1; margin-bottom:15px; color:#000;\r\n\tmargin: 40px 0 10px;\r\n\tline-height: 1.2;\r\n\tfont-weight:200; \r\n}}\r\n\r\nh1 {{\r\n\tfont-size: 36px;\r\n}}\r\nh2 {{\r\n\tfont-size: 28px;\r\n\ttext-align:center;\r\n}}\r\nh3 {{\r\n\tfont-size: 22px;\r\n}}\r\n\r\np, ul, ol {{ \r\n\tmargin-bottom: 10px; \r\n\tfont-weight: normal; \r\n\tfont-size:15px;\r\n}}\r\n\r\nul li, ol li {{\r\n\tmargin-left:5px;\r\n\tlist-style-position: inside;\r\n}}\r\n\r\nstrong{{\r\n\tfont-size:18px;\r\n\tfont-weight:normal;\r\n}}\r\n\r\n/* --------------------------------------------------- \r\n\t\tRESPONSIVENESS\r\n\t\tNuke it from orbit. It's the only way to be sure. \r\n------------------------------------------------------ */\r\n\r\n/* Set a max-width, and make it display as block so it will automatically stretch to that width, but will also shrink down on a phone or something */\r\n.container {{\r\n\tdisplay:block!important;\r\n\tmax-width:600px!important;\r\n\tmargin:0 auto!important; /* makes it centered */\r\n\tclear:both!important;\r\n}}\r\n\r\n/* Set the padding on the td rather than the div for Outlook compatibility */\r\n.body-wrap .container{{\r\n\tpadding:20px;\r\n}}\r\n\r\n/* This should also be a block element, so that it will fill 100% of the .container */\r\n.content {{\r\n\tmax-width:600px;\r\n\tmargin:0 auto;\r\n\tdisplay:block; \r\n}}\r\n\r\n/* Let's make sure tables in the content area are 100% wide */\r\n.content table {{ \r\n\twidth: 100%; \r\n}}\r\n\r\n.center{{\r\n\ttext-align:center;\r\n}}\r\n\r\n.logo{{\r\n\tdisplay:inline-block;\r\n\twidth:399px;\r\n\theight:85px;\r\n\tmax-width:90%;\r\n}}\r\n\r\n.footnote{{\r\n\tfont-size:14px;\r\n\tcolor:#444;\r\n}}\r\n\r\n@media all and (min-resolution: 192dpi), (-webkit-min-device-pixel-ratio: 2), (min--moz-device-pixel-ratio: 2), (-o-min-device-pixel-ratio: 2/1), (min-device-pixel-ratio: 2), (min-resolution: 2dppx){{\r\n\t.logo{{\r\n\t\tbackground-image:url(chartblocks@2x.png);\r\n\t\tbackground-size:100% auto;\r\n\t\tbackground-repeat:no-repeat;\r\n\t}}\r\n\t.logo img{{\r\n\t\tvisibility:hidden;\r\n\t}}\r\n}}\r\n\r\n</style>\r\n</head>\r\n \r\n<body bgcolor=\"#f6f6f6\">\r\n\r\n<!-- body -->\r\n<table class=\"body-wrap\">\r\n\t<tr>\r\n\t\t<td></td>\r\n\t\t<td class=\"container\" bgcolor=\"#FFFFFF\">\r\n\t\t\t<div class=\"content\">\r\n\t\t\t<table>\r\n\t\t\t\t<tr>\r\n\t\t\t\t\t<td class=\"center\">\r\n\t\t\t\t\t\t<div class=\"logo\">\r\n\t\t\t\t\t\t\t<img src=\"https://img.icons8.com/?size=100&id=68406&format=png&color=000000\">\r\n\t\t\t\t\t\t</div>\r\n\t\t\t\t\t</td>\r\n\t\t\t\t</tr>\r\n\t\t\t\t<tr>\r\n\t\t\t\t\t<td>\r\n\t\t\t\t\t\t<h2>Activate your account</h2>\r\n\t\t\t\t\t\t<p>Hi {fullname},\r\n\t\t\t\t\t\t<p>Thanks for creating a ChartBlocks account. To use the email address test@test.com to log in to ChartBlocks please verify your email address by clicking the button below.</p>\r\n\t\t\t\t\t\t<table>\r\n\t\t\t\t\t\t\t<tr>\r\n\t\t\t\t\t\t\t\t<td class=\"padding center\">\r\n\t\t\t\t\t\t\t\t\t<p><a href=\"{url}\" class=\"btn-primary\">Verify my email address</a></p>\r\n\t\t\t\t\t\t\t\t</td>\r\n\t\t\t\t\t\t\t</tr>\r\n\t\t\t\t\t\t</table>\r\n\t\t\t\t\t\t<p>Thanks,<br><strong>{firmName}</strong><br> ChartBlocks Support.</p>\r\n\t\t\t\t\t\t<p><a href=\"http://twitter.com/chartblocks\">Follow @prince999 on Twitter</a>\r\n\t\t\t\t\t</td>\r\n\t\t\t\t</tr>\r\n\t\t\t\t<tr>\r\n\t\t\t\t\t<td>\r\n\t\t\t\t\t\t<p class=\"footnote\">We hope you find ChartBlocks easy to use but if you have any problems drop an email to support@chartblocks.com and we'll get straight back to you.</p>\r\n\t\t\t\t\t</td>\r\n\t\t\t\t</tr>\r\n\t\t\t</table>\r\n\t\t\t</div>\r\n\t\t\t<!-- /content -->\r\n\t\t\t\t\t\t\t\t\t\r\n\t\t</td>\r\n\t\t<td></td>\r\n\t</tr>\r\n</table>\r\n<!-- /body -->\r\n\r\n<!-- footer -->\r\n<table class=\"footer-wrap\">\r\n\t<tr>\r\n\t\t<td></td>\r\n\t\t<td class=\"container\">\r\n\t\t\t\r\n\t\t\t<!-- content -->\r\n\t\t\t<div class=\"content\">\r\n\t\t\t\t<table>\r\n\t\t\t\t\t<tr>\r\n\t\t\t\t\t\t<td align=\"center\">\r\n\t\t\t\t\t\t\t<p>Don't like these annoying emails? <a href=\"#\"><unsubscribe>Unsubscribe</unsubscribe></a>.\r\n\t\t\t\t\t\t\t</p>\r\n\t\t\t\t\t\t</td>\r\n\t\t\t\t\t</tr>\r\n\t\t\t\t</table>\r\n\t\t\t</div><!-- /content -->\r\n\t\t\t\t\r\n\t\t</td>\r\n\t\t<td></td>\r\n\t</tr>\r\n</table>\r\n<!-- /footer -->\r\n\r\n</body>\r\n</html>";
            var b = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n\r\n  <meta charset=\"utf-8\">\r\n  <meta http-equiv=\"x-ua-compatible\" content=\"ie=edge\">\r\n  <title>Email  Confirmation</title>\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\r\n  <style type=\"text/css\">\r\n  /**\r\n   * Google webfonts. Recommended to include the .woff version for cross-client compatibility.\r\n   */\r\n  @media screen {{\r\n    @font-face {{\r\n      font-family: 'Source Sans Pro';\r\n      font-style: normal;\r\n      font-weight: 400;\r\n      src: local('Source Sans Pro Regular'), local('SourceSansPro-Regular'), url(https://fonts.gstatic.com/s/sourcesanspro/v10/ODelI1aHBYDBqgeIAH2zlBM0YzuT7MdOe03otPbuUS0.woff) format('woff');\r\n    }}\r\n    @font-face {{\r\n      font-family: 'Source Sans Pro';\r\n      font-style: normal;\r\n      font-weight: 700;\r\n      src: local('Source Sans Pro Bold'), local('SourceSansPro-Bold'), url(https://fonts.gstatic.com/s/sourcesanspro/v10/toadOcfmlt9b38dHJxOBGFkQc6VGVFSmCnC_l7QZG60.woff) format('woff');\r\n    }}\r\n  }}\r\n  /**\r\n   * Avoid browser level font resizing.\r\n   * 1. Windows Mobile\r\n   * 2. iOS / OSX\r\n   */\r\n  body,\r\n  table,\r\n  td,\r\n  a {{\r\n    -ms-text-size-adjust: 100%; /* 1 */\r\n    -webkit-text-size-adjust: 100%; /* 2 */\r\n  }}\r\n  /**\r\n   * Remove extra space added to tables and cells in Outlook.\r\n   */\r\n  table,\r\n  td {{\r\n    mso-table-rspace: 0pt;\r\n    mso-table-lspace: 0pt;\r\n  }}\r\n  /**\r\n   * Better fluid images in Internet Explorer.\r\n   */\r\n  img {{\r\n    -ms-interpolation-mode: bicubic;\r\n  }}\r\n  /**\r\n   * Remove blue links for iOS devices.\r\n   */\r\n  a[x-apple-data-detectors] {{\r\n    font-family: inherit !important;\r\n    font-size: inherit !important;\r\n    font-weight: inherit !important;\r\n    line-height: inherit !important;\r\n    color: inherit !important;\r\n    text-decoration: none !important;\r\n  }}\r\n  /**\r\n   * Fix centering issues in Android 4.4.\r\n   */\r\n  div[style*=\"margin: 16px 0;\"] {{\r\n    margin: 0 !important;\r\n  }}\r\n  body {{\r\n    width: 100% !important;\r\n    height: 100% !important;\r\n    padding: 0 !important;\r\n    margin: 0 !important;\r\n  }}\r\n  /**\r\n   * Collapse table borders to avoid space between cells.\r\n   */\r\n  table {{\r\n    border-collapse: collapse !important;\r\n  }}\r\n  a {{\r\n    color: #1a82e2;\r\n  }}\r\n  img {{\r\n    height: auto;\r\n    line-height: 100%;\r\n    text-decoration: none;\r\n    border: 0;\r\n    outline: none;\r\n  }}\r\n  </style>\r\n\r\n</head>\r\n<body style=\"background-color: #e9ecef;\">\r\n\r\n  <!-- start preheader -->\r\n  <div class=\"preheader\" style=\"display: none; max-width: 0; max-height: 0; overflow: hidden; font-size: 1px; line-height: 1px; color: #fff; opacity: 0;\">\r\n    A preheader is the short summary text that follows the subject line when an email is viewed in the inbox.\r\n  </div>\r\n  <!-- end preheader -->\r\n\r\n  <!-- start body -->\r\n  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">\r\n\r\n    <!-- start logo -->\r\n    <tr>\r\n      <td align=\"center\" bgcolor=\"#e9ecef\">\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"600\">\r\n        <tr>\r\n        <td align=\"center\" valign=\"top\" width=\"600\">\r\n        <![endif]-->\r\n        <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\r\n          <tr>\r\n            <td align=\"center\" valign=\"top\" style=\"padding: 36px 24px;\">\r\n              <a href=\"https://www.blogdesire.com\" target=\"_blank\" style=\"display: inline-block;\">\r\n                <img src=\"https://www.blogdesire.com/wp-content/uploads/2019/07/blogdesire-1.png\" alt=\"Logo\" border=\"0\" width=\"48\" style=\"display: block; width: 48px; max-width: 48px; min-width: 48px;\">\r\n              </a>\r\n            </td>\r\n          </tr>\r\n        </table>\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        </td>\r\n        </tr>\r\n        </table>\r\n        <![endif]-->\r\n      </td>\r\n    </tr>\r\n    <!-- end logo -->\r\n\r\n    <!-- start hero -->\r\n    <tr>\r\n      <td align=\"center\" bgcolor=\"#e9ecef\">\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"600\">\r\n        <tr>\r\n        <td align=\"center\" valign=\"top\" width=\"600\">\r\n        <![endif]-->\r\n        <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\r\n          <tr>\r\n            <td align=\"left\" bgcolor=\"#ffffff\" style=\"padding: 36px 24px 0; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; border-top: 3px solid #d4dadf;\">\r\n              <h1 style=\"margin: 0; font-size: 32px; font-weight: 700; letter-spacing: -1px; line-height: 48px;\">{fullname} Confirm Your Email Address</h1>\r\n            </td>\r\n          </tr>\r\n        </table>\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        </td>\r\n        </tr>\r\n        </table>\r\n        <![endif]-->\r\n      </td>\r\n    </tr>\r\n    <!-- end hero -->\r\n\r\n    <!-- start copy block -->\r\n    <tr>\r\n      <td align=\"center\" bgcolor=\"#e9ecef\">\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"600\">\r\n        <tr>\r\n        <td align=\"center\" valign=\"top\" width=\"600\">\r\n        <![endif]-->\r\n        <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\r\n\r\n          <!-- start copy -->\r\n          <tr>\r\n            <td align=\"left\" bgcolor=\"#ffffff\" style=\"padding: 24px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px;\">\r\n              <p style=\"margin: 0;\">Tap the button below to confirm your email address. If you didn't create an account with <a href=\"https://blogdesire.com\">Paste</a>, you can safely delete this email.</p>\r\n            </td>\r\n          </tr>\r\n          <!-- end copy -->\r\n\r\n          <!-- start button -->\r\n          <tr>\r\n            <td align=\"left\" bgcolor=\"#ffffff\">\r\n              <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">\r\n                <tr>\r\n                  <td align=\"center\" bgcolor=\"#ffffff\" style=\"padding: 12px;\">\r\n                    <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">\r\n                      <tr>\r\n                        <td align=\"center\" bgcolor=\"#1a82e2\" style=\"border-radius: 6px;\">\r\n                          <a href=\"https://www.blogdesire.com\" target=\"_blank\" style=\"display: inline-block; padding: 16px 36px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 6px;\">Do Something Sweet</a>\r\n                        </td>\r\n                      </tr>\r\n                    </table>\r\n                  </td>\r\n                </tr>\r\n              </table>\r\n            </td>\r\n          </tr>\r\n          <!-- end button -->\r\n\r\n          <!-- start copy -->\r\n          <tr>\r\n            <td align=\"left\" bgcolor=\"#ffffff\" style=\"padding: 24px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px;\">\r\n              <p style=\"margin: 0;\">If that doesn't work, copy and paste the following link in your browser:</p>\r\n              <p style=\"margin: 0;\"><a href=\"https://blogdesire.com\" target=\"_blank\">https://blogdesire.com/xxx-xxx-xxxx</a></p>\r\n            </td>\r\n          </tr>\r\n          <!-- end copy -->\r\n\r\n          <!-- start copy -->\r\n          <tr>\r\n            <td align=\"left\" bgcolor=\"#ffffff\" style=\"padding: 24px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px; border-bottom: 3px solid #d4dadf\">\r\n              <p style=\"margin: 0;\">Cheers,<br> Paste</p>\r\n            </td>\r\n          </tr>\r\n          <!-- end copy -->\r\n\r\n        </table>\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        </td>\r\n        </tr>\r\n        </table>\r\n        <![endif]-->\r\n      </td>\r\n    </tr>\r\n    <!-- end copy block -->\r\n\r\n    <!-- start footer -->\r\n    <tr>\r\n      <td align=\"center\" bgcolor=\"#e9ecef\" style=\"padding: 24px;\">\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"600\">\r\n        <tr>\r\n        <td align=\"center\" valign=\"top\" width=\"600\">\r\n        <![endif]-->\r\n        <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\r\n\r\n          <!-- start permission -->\r\n          <tr>\r\n            <td align=\"center\" bgcolor=\"#e9ecef\" style=\"padding: 12px 24px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 14px; line-height: 20px; color: #666;\">\r\n              <p style=\"margin: 0;\">You received this email because we received a request for [type_of_action] for your account. If you didn't request [type_of_action] you can safely delete this email.</p>\r\n            </td>\r\n          </tr>\r\n          <!-- end permission -->\r\n\r\n          <!-- start unsubscribe -->\r\n          <tr>\r\n            <td align=\"center\" bgcolor=\"#e9ecef\" style=\"padding: 12px 24px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 14px; line-height: 20px; color: #666;\">\r\n              <p style=\"margin: 0;\">To stop receiving these emails, you can <a href=\"https://www.blogdesire.com\" target=\"_blank\">unsubscribe</a> at any time.</p>\r\n              <p style=\"margin: 0;\">Paste 1234 S. Broadway St. City, State 12345</p>\r\n            </td>\r\n          </tr>\r\n          <!-- end unsubscribe -->\r\n\r\n        </table>\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        </td>\r\n        </tr>\r\n        </table>\r\n        <![endif]-->\r\n      </td>\r\n    </tr>\r\n    <!-- end footer -->\r\n\r\n  </table>\r\n  <!-- end body -->\r\n\r\n</body>\r\n</html>";
            await emailService.SendEmailAsync(toEmail, b);
        }
    }
 }
