using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

//using Microsoft.AspNetCore.Mvc;
using PlugIn.Contracts;
using PlugIn.Contracts.Responses;
using PlugIn.Dal.AppContext;
using PlugIn.Dal.Models;
using PlugIn.JwtTokenProviders;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PlugIn.Repository
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtTokenProvider _jwtToken;
        private readonly PlugInDbContext _context;

        public ProjectRepository(IConfiguration configuration, SignInManager<ApplicationUser> signInManager,
                                UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, 
                                JwtTokenProvider jwtToken, PlugInDbContext context)
        {
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtToken = jwtToken;
            _context = context;
        }
        public async Task LogOutAsync()
        {
            await _signInManager.SignOutAsync();
        }
        public async Task<SignInResult> LoginAsync(LoginForm form)
        {
            return  await _signInManager.PasswordSignInAsync(form.Email, form.Password, isPersistent: true, lockoutOnFailure: false);
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
                result =  await _userManager.CreateAsync(user, form.Password);
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Code = "500", Description = ex.Message});
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
                UserLogin  = form.UserLogin,
                Name = form.Name,
                Description = form.Description
            };

            try
            {
                var result = _context.AppProject.Add(project);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Code = "500", Description = ex.Message});
            }
            return IdentityResult.Success;
        }
    }
}