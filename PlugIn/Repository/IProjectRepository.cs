using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlugIn.Contracts;
using PlugIn.Contracts.Responses;
using PlugIn.Dal.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PlugIn.Repository
{
    public interface IProjectRepository
    {
        public Task<IdentityResult> Register(RegisterForm form);
        public Task<SignInResult> LoginAsync(LoginForm form);
        public Task LogOutAsync();
        public Task<IdentityResult> AddProjectAsync(ProjectForm form);
        public Task<BaseResponse> SendEmailAsync(string email);
        public Task<BaseResponse> ConfirmEmailAsync(string email, string token);
        public Task<string> GenerateEmailConfirmationToken(ApplicationUser user);

	}
}
