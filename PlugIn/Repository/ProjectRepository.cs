using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlugIn.Contracts;
using PlugIn.Contracts.Responses;
using PlugIn.Dal.AppContext;
using PlugIn.Dal.Models;
using PlugIn.JwtTokenProviders;
using PlugIn.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PlugIn.Repository
{
    public class ProjectRepository(SignInManager<ApplicationUser> signInManager,
                                UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager,
                                JwtTokenProvider jwtToken, PlugInDbContext context) : IProjectRepository
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly JwtTokenProvider _jwtToken = jwtToken;
        private readonly PlugInDbContext _context = context;

        readonly EmailService emailService = new
            ("smtp.gmail.com",
            587,
            "smarttaxam@gmail.com",
            "qmkcevtbftufhlrm");

        public async Task LogOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
        public async Task<SignInResult> LoginAsync(LoginForm form)
        {
            return await _signInManager.PasswordSignInAsync(form.Email, form.Password, isPersistent: true, lockoutOnFailure: false);
        }
        public async Task<IdentityResult> Register(RegisterForm form)
        {
            IdentityResult result;
            var user = await _userManager.FindByEmailAsync(form.Email);

            if (user != null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Code = "400",
                        Description = $"The User by {form.Email} already exists!"
                    });
            }

            user = new ApplicationUser
            {
                FirstName = form.FirstName,
                UserName = form.Email,
                LastName = form.LastName,
                Email = form.Email,
                PasswordHash = form.Password
            };

            try
            {
                result = await _userManager.CreateAsync(user, form.Password);
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Code = "500", Description = ex.Message });
            }
            var roleResult = await _roleManager.RoleExistsAsync("user");

            if (!roleResult)
                await _roleManager.CreateAsync(new IdentityRole<Guid>("user"));

            var addToRoleResult = await _userManager.AddToRoleAsync(user, "user");

            if (!addToRoleResult.Succeeded)
                return addToRoleResult;

            var token = _jwtToken.Create("user", user.Email);

            var tokenresult = await _userManager.SetAuthenticationTokenAsync(user, "AuthenticationToken", "Bearer", token);

            if (!tokenresult.Succeeded)
                return tokenresult;

            return IdentityResult.Success;
        }
        public async Task<IdentityResult> AddProjectAsync(ProjectForm form)
        {
            var project = new ApplicationProject
            {
                UserLogin = form.UserLogin,
                Name = form.Name,
                Description = form.Description
            };
            try
            {
                var result = _context.AppProject.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Code = "500", Description = ex.Message });
            }
            return IdentityResult.Success;
        }        
        public async Task<string> GenerateEmailConfirmationToken(ApplicationUser user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }
    }
}