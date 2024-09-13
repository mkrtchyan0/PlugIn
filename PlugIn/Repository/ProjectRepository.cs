using Azure.Core;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PlugIn.Contracts;
using PlugIn.Contracts.Responses;
using PlugIn.Dal.AppContext;
using PlugIn.Dal.Models;
using PlugIn.JwtTokenProviders;
using PlugIn.Models;
using System;
using System.Text;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace PlugIn.Repository
{
    public class ProjectRepository(SignInManager<ApplicationUser> signInManager,
                              UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole<Guid>> roleManager,
                              JwtTokenProvider jwtToken,
                              PlugInDbContext context,
                              IUrlHelperFactory urlHelperFactory,
                              IActionContextAccessor actionContextAccessor,
                              EmailService emailService) : IProjectRepository
    {
        private readonly RoleManager<IdentityRole<Guid>> _roleManager = roleManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly JwtTokenProvider _jwtToken = jwtToken;
        private readonly PlugInDbContext _context = context;
        private readonly IUrlHelper _UrlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);

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
            //var tokenResultConfigur = await _userManager.SetAuthenticationTokenAsync(user, "EmailConfirmation", "Confirmation", tokenForConfigur);

            if (!tokenresult.Succeeded)
                return tokenresult;
            var emailtoken = _userManager.GenerateEmailConfirmationTokenAsync(user).Result;
            try
            {
                await SendEmailAsync(user.Email, emailtoken);
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Code = "500", Description = ex.Message });
            }
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
        public async Task<bool> SendEmailAsync(string toEmail, string verifytoken)
        {
            var uTask = _userManager.FindByEmailAsync(toEmail);
            var user = await uTask;

            if (user == null)
                return false;
                var fullname = user.FirstName + " " + user.LastName;

            var confirmationLink = _UrlHelper.Action("ConfirmEmail", null, new { email = toEmail, token = verifytoken }, "https", "localhost:44321");

            var html = $"<!DOCTYPE html>\r\n<html>\r\n<head>\r\n\r\n  <meta charset=\"utf-8\">\r\n  <meta http-equiv=\"x-ua-compatible\" content=\"ie=edge\">\r\n  <title>Email  Confirmation</title>\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\r\n  <style type=\"text/css\">\r\n  /**\r\n   * Google webfonts. Recommended to include the .woff version for cross-client compatibility.\r\n   */\r\n  @media screen {{\r\n    @font-face {{\r\n      font-family: 'Source Sans Pro';\r\n      font-style: normal;\r\n      font-weight: 400;\r\n      src: local('Source Sans Pro Regular'), local('SourceSansPro-Regular'), url(https://fonts.gstatic.com/s/sourcesanspro/v10/ODelI1aHBYDBqgeIAH2zlBM0YzuT7MdOe03otPbuUS0.woff) format('woff');\r\n    }}\r\n    @font-face {{\r\n      font-family: 'Source Sans Pro';\r\n      font-style: normal;\r\n      font-weight: 700;\r\n      src: local('Source Sans Pro Bold'), local('SourceSansPro-Bold'), url(https://fonts.gstatic.com/s/sourcesanspro/v10/toadOcfmlt9b38dHJxOBGFkQc6VGVFSmCnC_l7QZG60.woff) format('woff');\r\n    }}\r\n  }}\r\n  /**\r\n   * Avoid browser level font resizing.\r\n   * 1. Windows Mobile\r\n   * 2. iOS / OSX\r\n   */\r\n  body,\r\n  table,\r\n  td,\r\n  a {{\r\n    -ms-text-size-adjust: 100%; /* 1 */\r\n    -webkit-text-size-adjust: 100%; /* 2 */\r\n  }}\r\n  /**\r\n   * Remove extra space added to tables and cells in Outlook.\r\n   */\r\n  table,\r\n  td {{\r\n    mso-table-rspace: 0pt;\r\n    mso-table-lspace: 0pt;\r\n  }}\r\n  /**\r\n   * Better fluid images in Internet Explorer.\r\n   */\r\n  img {{\r\n    -ms-interpolation-mode: bicubic;\r\n  }}\r\n  /**\r\n   * Remove blue links for iOS devices.\r\n   */\r\n  a[x-apple-data-detectors] {{\r\n    font-family: inherit !important;\r\n    font-size: inherit !important;\r\n    font-weight: inherit !important;\r\n    line-height: inherit !important;\r\n    color: inherit !important;\r\n    text-decoration: none !important;\r\n  }}\r\n  /**\r\n   * Fix centering issues in Android 4.4.\r\n   */\r\n  div[style*=\"margin: 16px 0;\"] {{\r\n    margin: 0 !important;\r\n  }}\r\n  body {{\r\n    width: 100% !important;\r\n    height: 100% !important;\r\n    padding: 0 !important;\r\n    margin: 0 !important;\r\n  }}\r\n  /**\r\n   * Collapse table borders to avoid space between cells.\r\n   */\r\n  table {{\r\n    border-collapse: collapse !important;\r\n  }}\r\n  a {{\r\n    color: #1a82e2;\r\n  }}\r\n  img {{\r\n    height: auto;\r\n    line-height: 100%;\r\n    text-decoration: none;\r\n    border: 0;\r\n    outline: none;\r\n  }}\r\n  </style>\r\n\r\n</head>\r\n<body style=\"background-color: #e9ecef;\">\r\n\r\n  <!-- start preheader -->\r\n  <div class=\"preheader\" style=\"display: none; max-width: 0; max-height: 0; overflow: hidden; font-size: 1px; line-height: 1px; color: #fff; opacity: 0;\">\r\n    A preheader is the short summary text that follows the subject line when an email is viewed in the inbox.\r\n  </div>\r\n  <!-- end preheader -->\r\n\r\n  <!-- start body -->\r\n  <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">\r\n\r\n    <!-- start logo -->\r\n    <tr>\r\n      <td align=\"center\" bgcolor=\"#e9ecef\">\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"600\">\r\n        <tr>\r\n        <td align=\"center\" valign=\"top\" width=\"600\">\r\n        <![endif]-->\r\n        <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\r\n          <tr>\r\n            <td align=\"center\" valign=\"top\" style=\"padding: 36px 24px;\">\r\n              <a href=\"https://www.blogdesire.com\" target=\"_blank\" style=\"display: inline-block;\">\r\n                <img src=\"https://www.blogdesire.com/wp-content/uploads/2019/07/blogdesire-1.png\" alt=\"Logo\" border=\"0\" width=\"48\" style=\"display: block; width: 48px; max-width: 48px; min-width; 48px;\">\r\n              </a>\r\n            </td>\r\n          </tr>\r\n        </table>\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        </td>\r\n        </tr>\r\n        </table>\r\n        <![endif]-->\r\n      </td>\r\n    </tr>\r\n    <!-- end logo -->\r\n\r\n    <!-- start hero -->\r\n    <tr>\r\n      <td align=\"center\" bgcolor=\"#e9ecef\">\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"600\">\r\n        <tr>\r\n        <td align=\"center\" valign=\"top\" width=\"600\">\r\n        <![endif]-->\r\n        <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\r\n          <tr>\r\n            <td align=\"left\" bgcolor=\"#ffffff\" style=\"padding: 36px 24px 0; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; border-top: 3px solid #d4dadf;\">\r\n              <h1 style=\"margin: 0; font-size: 32px; font-weight: 700; letter-spacing: -1px; line-height: 48px;\">{fullname} Confirm Your Email Address</h1>\r\n            </td>\r\n          </tr>\r\n        </table>\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        </td>\r\n        </tr>\r\n        </table>\r\n        <![endif]-->\r\n      </td>\r\n    </tr>\r\n    <!-- end hero -->\r\n\r\n    <!-- start copy block -->\r\n    <tr>\r\n      <td align=\"center\" bgcolor=\"#e9ecef\">\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"600\">\r\n        <tr>\r\n        <td align=\"center\" valign=\"top\" width=\"600\">\r\n        <![endif]-->\r\n        <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\r\n\r\n          <!-- start copy -->\r\n          <tr>\r\n            <td align=\"left\" bgcolor=\"#ffffff\" style=\"padding: 24px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px;\">\r\n              <p style=\"margin: 0;\">Tap the button below to confirm your email address. If you didn't create an account with <a href=\"https://blogdesire.com\">Paste</a>, you can safely delete this email.</p>\r\n            </td>\r\n          </tr>\r\n          <!-- end copy -->\r\n\r\n          <!-- start button -->\r\n          <tr>\r\n            <td align=\"left\" bgcolor=\"#ffffff\">\r\n              <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">\r\n                <tr>\r\n                  <td align=\"center\" bgcolor=\"#ffffff\" style=\"padding: 12px;\">\r\n                    <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\">\r\n                      <tr>\r\n                        <td align=\"center\" bgcolor=\"#1a82e2\" style=\"border-radius: 6px;\">\r\n                          <a href=\"{confirmationLink}\" target=\"_blank\" style=\"display: inline-block; padding: 16px 36px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 16px; color: #ffffff; text-decoration: none; border-radius: 6px;\">Click to verify email</a>\r\n                        </td>\r\n                      </tr>\r\n                    </table>\r\n                  </td>\r\n                </tr>\r\n              </table>\r\n            </td>\r\n          </tr>\r\n          <!-- end button -->\r\n\r\n          <!-- start copy -->\r\n          <tr>\r\n            <td align=\"left\" bgcolor=\"#ffffff\" style=\"padding: 24px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px;\">\r\n              <p style=\"margin: 0;\">If that doesn't work, copy and paste the following link in your browser:</ p >\r\n < p style =\"margin: 0;\"><a href=\"https://blogdesire.com\" target=\"_blank\">https://blogdesire.com/xxx-xxx-xxxx</a></p>\r\n            </td>\r\n          </tr>\r\n          <!-- end copy -->\r\n\r\n          <!-- start copy -->\r\n          <tr>\r\n            <td align=\"left\" bgcolor=\"#ffffff\" style=\"padding: 24px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 16px; line-height: 24px; border-bottom: 3px solid #d4dadf\">\r\n              <p style=\"margin: 0;\">Cheers,<br> Paste</p>\r\n            </td>\r\n          </tr>\r\n          <!-- end copy -->\r\n\r\n        </table>\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        </td>\r\n        </tr>\r\n        </table>\r\n        <![endif]-->\r\n      </td>\r\n    </tr>\r\n    <!-- end copy block -->\r\n\r\n    <!-- start footer -->\r\n    <tr>\r\n      <td align=\"center\" bgcolor=\"#e9ecef\" style=\"padding: 24px;\">\r\n        <!--[if (gte mso 9)|(IE)]>\r\n        <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"600\">\r\n        <tr>\r\n        <td align=\"center\" valign=\"top\" width=\"600\">\r\n        <![endif]-->\r\n        <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\r\n\r\n          <!-- start permission -->\r\n          <tr>\r\n            <td align=\"center\" bgcolor=\"#e9ecef\" style=\"padding: 12px 24px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 14px; line-height: 20px; color: #666;\">\r\n              <p style=\"margin: 0;\">You received this email because we received a request for [type_of_action] for your account. If you didn't request [type_of_action] you can safely delete this email.</p>\r\n            </td>\r\n          </tr>\r\n          <!-- end permission -->\r\n\r\n          <!-- start unsubscribe -->\r\n          <tr>\r\n            <td align=\"center\" bgcolor=\"#e9ecef\" style=\"padding: 12px 24px; font-family: 'Source Sans Pro', Helvetica, Arial, sans-serif; font-size: 14px; line-height: 20px; color: #666;\">\r\n              <p style=\"margin: 0;\">To stop receiving these emails, you can <a href=\"https://www.blogdesire.com\" target=\"_blank\">unsubscribe</a> at any time.</p>\r\n              <p style=\"margin: 0;\">Paste 1234 S. Broadway St. City, State 12345 </ p >\r\n </ td >\r\n </ tr >\r\n < !--end unsubscribe-- >\r\n\r\n </ table >\r\n < !-- [if (gte mso 9)| (IE)]>\r\n </ td >\r\n </ tr >\r\n </ table >\r\n < ! [endif]-- >\r\n </ td >\r\n </ tr >\r\n < !--end footer-- >\r\n\r\n </ table >\r\n < !--end body-- >\r\n\r\n </ body >\r\n </ html > ";

            return await emailService.SendEmailAsync(toEmail, html);
        }
        public Task<IdentityResult> ConfirmEmailAsync(string email, string token)
        {
            try
            {
                var user = _userManager.FindByEmailAsync(email).Result;

                if (user == null)
                    throw new Exception("User not found.");

                var result = _userManager.ConfirmEmailAsync(user, token).Result;
                if (!result.Succeeded)
                    throw new Exception("User not found.");
            }
            catch (Exception ex)
            {
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Code = "500", Description = ex.Message }));
            }
            return Task.FromResult(IdentityResult.Success);
        }
    }
}