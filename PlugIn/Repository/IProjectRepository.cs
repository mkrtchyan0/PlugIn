﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlugIn.Contracts;
using PlugIn.Contracts.Responses;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PlugIn.Repository
{
    public interface IProjectRepository
    {
        public Task<IdentityResult> Register(RegisterForm form);
        public Task<SignInResult> LoginAsync(LoginForm form);
        public Task LogOutAsync();
        public Task<IdentityResult> AddProjectAsync(ProjectForm form);
    }
}