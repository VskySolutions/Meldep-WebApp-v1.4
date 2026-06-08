using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.PowerBI.Api.Models;
using Vsky.Api.ApiErrors;
using Vsky.Api.Models;
using Vsky.Core.Configuration;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.Common;
using Vsky.Services.EmailNotifications;
using Vsky.Services.Messages;
using Vsky.Services.Persons;
using Vsky.Services.Sites;
using Vsky.Services.SitesModulesMenu;
using Vsky.Services.Users;

namespace Vsky.Api.Controllers
{
    [Route("auth")]
    public class AuthController : BaseController
    {
        #region Fields
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenConfig _jwtTokenConfig;
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly GlobalVariable _globalVariable;

        private readonly ISiteService _siteService;
        private readonly IPersonService _personsService;
        private readonly ICommonService _commonService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ISitesModulesMenusService _sitesModulesMenusService;
        private readonly ISitesEmailNotificationsPermissionServices _sitesEmailNotificationsPermissionServices;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        private readonly IPersonSitesMappingService _personSitesMappingService;
        #endregion

        #region Ctor
        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext db,
            JwtTokenConfig jwtTokenConfig,
            GlobalVariable globalVariable,
            IWorkflowMessageService workflowMessageService,
            IUserService userService, 
            ISiteService SiteService,
            IPersonService PersonsService, 
            IEmailSender emailSender, 
            ICommonService commonService,
            ISitesModulesMenusService sitesModulesMenusService,
            ISitesEmailNotificationsPermissionServices sitesEmailNotificationsPermissionServices,
            IApplicationUserRoleService applicationUserRoleService,
            IPersonSitesMappingService personSitesMappingService
        )
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _jwtTokenConfig = jwtTokenConfig;
            _globalVariable = globalVariable;
            _workflowMessageService = workflowMessageService;
            _siteService = SiteService;
            _personsService = PersonsService;
            _commonService = commonService;
            _sitesModulesMenusService = sitesModulesMenusService;
            _sitesEmailNotificationsPermissionServices = sitesEmailNotificationsPermissionServices;
            _applicationUserRoleService = applicationUserRoleService;
            _personSitesMappingService = personSitesMappingService;
        }
        #endregion

        #region Utilities
        private static long ToUnixEpochDate(DateTime date)
        {
            return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        }

        private static string GenerakeToken(ApplicationUser user, JwtTokenConfig jwtTokenConfig, IList<string> roles/*, string SiteId*/)
        {
            // default claims
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new(JwtRegisteredClaimNames.Jti, jwtTokenConfig.JtiGenerator().Result),
                new(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(jwtTokenConfig.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Iss, jwtTokenConfig.Issuer),
                //new Claim("SiteId", SiteId.ToString()),
            };

            // user roles as claims
            claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)));

            // the jwt security token and encode it
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenConfig.SecurityKey));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: jwtTokenConfig.Issuer,
                audience: jwtTokenConfig.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: jwtTokenConfig.Expiration,
                signingCredentials: signinCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        #endregion

        #region Login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(TokenModel model)
        {
            try
            {
                //Check model state is valid
                if (ModelState.IsValid)
                {
                    //Get user by username
                    var userData = await _userManager.FindByNameAsync(model.Username);
                    if (userData != null && !userData.Deleted && userData.Active)
                    {
                        //Check user with valid password
                        var result = await _signInManager.PasswordSignInAsync(userData, model.Password, false, true);
                        if (result.Succeeded)
                        {
                            //Get user roles
                            var roles = await _userManager.GetRolesAsync(userData);

                            // Fetch the NormalizedName of each role
                            //var normalizedRoles = _db.Roles.Where(r => roles.Contains(r.Name)).Select(r => r.NormalizedName).ToArray();

                            //Find Person Info
                            var personMappingData = await _personsService.GetPersonSiteMappingByPersonId(userData.PersonId, "");
                            if (personMappingData == null || personMappingData.Person == null || personMappingData.Sites == null)
                            {
                                await _signInManager.SignOutAsync();
                                return BadRequest(new BadRequestError("Invalid User or Site Information."));
                            }

                            // Get last used site
                            var lastUsedSite = await _personSitesMappingService.GetLastUsedSiteByPersonId(personMappingData.PersonId);

                            //Find Site details
                            var personData = personMappingData.Person;
                            var SiteData = lastUsedSite != null ? lastUsedSite.Sites: personMappingData.Sites;
                            var LandingPageLink = await _sitesModulesMenusService.GetLandingPageBySiteId(SiteData.Id);
                            //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteData.Id, userData.Id);
                            var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteData.Id, userData.Id);
                            var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                            // Fetch the NormalizedName of each role
                            var normalizedRole = await _applicationUserRoleService.GetNormalizedRoleNamesByUserAndSite(userData.Id, SiteData.Id);

                            //Generate login token
                            var token = GenerakeToken(userData, _jwtTokenConfig, roles);

                            //Collect users info and token data
                            var tokenResult = new TokenResultModel
                            {
                                SiteId = SiteData.Id,
                                UserId = userData.Id,
                                PersonId = personData.Id,
                                EmployeeId = EmployeeId,
                                SiteName = SiteData.Name,
                                SiteLandingPageLink = LandingPageLink,
                                SiteTimeZone = SiteData.TimeZone,
                                Username = userData.UserName,
                                UserEmail = userData.Email,
                                FirstName = personData.FirstName,
                                LastName = personData.LastName,
                                Roles = roles.Any() ? normalizedRole.ToArray() : null,
                                Token = token,
                                ExpiresIn = (int)_jwtTokenConfig.ValidFor.TotalSeconds,
                                CreatedAt = GetDateTime,
                            };

                            return Ok(tokenResult);

                          
                        }
                        else if (result.RequiresTwoFactor)
                        {
                            await _signInManager.SignOutAsync();

                            var code = await _userManager.GenerateTwoFactorTokenAsync(userData, "Email");

                            // send two factor code email
                            await _workflowMessageService.SendTwoFactorToken(userData, code);

                            // redirect to login with 2fa
                            return Ok(new AuthError((int)AuthErrorCodes.RequiresTwoFactor, userData.Email));

                        }
                        else if (result.IsLockedOut)
                        {
                            // redirect to lockout
                            return BadRequest(new BadRequestError("User account locked out."));
                        }
                        else
                        {
                            return BadRequest(new BadRequestError("Invalid login attempt."));
                        }
                    }
                }
                return BadRequest(new BadRequestError("Invalid login attempt."));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Microsoft Login
        [AllowAnonymous]
        [HttpPost("mslogin")]
        public async Task<IActionResult> GetSecureData(TokenResultModel Authorization)
        {
            try
            {
                if (string.IsNullOrEmpty(Authorization.Token))
                {
                    return Unauthorized("Token is missing.");
                }
                // Now validate the token and extract claims
                var handler = new JwtSecurityTokenHandler();
                var authtoken = Authorization.Token.Replace("Bearer ", "");
                var jsonToken = handler.ReadToken(authtoken) as JwtSecurityToken;
                if (jsonToken == null)
                {
                    return Unauthorized("Invalid token.");
                }

                // Extract user claims from the token
                var email = jsonToken?.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
                var userData = await _userManager.FindByEmailAsync(email);
                if (userData == null)
                {
                    return Ok(new { loginErrorMessage = "You are not authorized user to login into system, You need to register first" });
                }
                else
                {
                    //Get user roles
                    var roles = await _userManager.GetRolesAsync(userData);

                    // Fetch the NormalizedName of each role
                    //var normalizedRoles = _db.Roles.Where(r => roles.Contains(r.Name)).Select(r => r.NormalizedName).ToArray();

                    //Find Person Info
                    var personMappingData = await _personsService.GetPersonSiteMappingByPersonId(userData.PersonId, "");
                    if (personMappingData == null || personMappingData.Person == null || personMappingData.Sites == null)
                    {
                        await _signInManager.SignOutAsync();
                        return BadRequest(new BadRequestError("Invalid User or Site Information."));
                    }

                    //Find Site details
                    var personData = personMappingData.Person;
                    var SiteData = personMappingData.Sites;
                    var LandingPageLink = await _sitesModulesMenusService.GetLandingPageBySiteId(SiteData.Id);
                    //var EmployeeId = _commonService.GetEmployeeIdByUserId(SiteData.Id, userData.Id);

                    var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteData.Id, userData.Id);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the NormalizedName of each role
                    var normalizedRoles = await _applicationUserRoleService.GetNormalizedRoleNamesByUserAndSite(userData.Id, SiteData.Id);

                    //Generate login token
                    var token = GenerakeToken(userData, _jwtTokenConfig, roles);

                    //Collect users info and token data
                    var tokenResult = new TokenResultModel
                    {
                        SiteId = SiteData.Id,
                        UserId = userData.Id,
                        PersonId = personData.Id,
                        EmployeeId = EmployeeId,
                        SiteName = SiteData.Name,
                        SiteLandingPageLink = LandingPageLink,
                        SiteTimeZone = SiteData.TimeZone,
                        Username = userData.UserName,
                        UserEmail = userData.Email,
                        FirstName = personData.FirstName,
                        LastName = personData.LastName,
                        Roles = roles.Any() ? normalizedRoles.ToArray() : null,
                        Token = token,
                        ExpiresIn = (int)_jwtTokenConfig.ValidFor.TotalSeconds,
                        CreatedAt = GetDateTime,
                        IsMsLogin = true
                    };
                    //Collect user roles
                    if (roles != null && roles.Count > 0)
                    {
                        tokenResult.Roles = normalizedRoles.ToArray();
                        tokenResult.RolesName = roles.ToArray();
                    }

                    return Ok(tokenResult);
                }
            }
            catch (Exception ex)
            {
                return Unauthorized($"Token validation failed: {ex.Message}");
            }
            

        }
        #endregion

        #region Forgot Password
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                 var user = await _userManager.FindByEmailAsync(model.Email);
                // Check if user exists, is not deleted, and has a confirmed email
                if (user == null || user.Deleted || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return BadRequest(new BadRequestError("No user was found with the specified id."));
                }

                var SiteId = _globalVariable.SiteId;

                //Check Email Notification Permission
                var canSend = await _sitesEmailNotificationsPermissionServices.ShouldSendNotification(SiteId, user.Id, "User.ResetPassword");

                if (!canSend)
                    return Ok(new { success = false, message = "Email notification permission is currently disabled. Please enable the email permission from manage permission to receive the password reset link." });

                // Generate a password reset token for the user
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Reset the user's password using the generated token and new password
                await _workflowMessageService.SendPasswordResetEmail(user, code);

                return Ok(new { success = true, message = "Password reset link sent to email." });
            }
            // Return model state error response if the model state is invalid
            return ModelStateError(ModelState);
        }

        #endregion

        #region Get user by id
        [HttpGet("user")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(string userid)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(userid);

                return Ok(user);
            }
            // Return model state error response if the model state is invalid
            return ModelStateError(ModelState);
        }
        #endregion

        #region Reset Password
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                // Check if user exists, is not deleted, and has a confirmed email
                if (user == null || user.Deleted || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return BadRequest(new BadRequestError("No user was found with the specified id."));
                }
                // Decode the token from the URL
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                // Reset the user's password using the generated token and new password
                var result = await _userManager.ResetPasswordAsync(user, code, model.NewPassword);
                if (result.Succeeded)
                {
                    return Ok("Your password has been reset successfully. Log in with your new password.");
                }
                else
                {
                    // Return an internal server error response if there were issues resetting the password
                    return InternalServerError(result.Errors);
                }
            }
            // Return model state error response if the model state is invalid
            return ModelStateError(ModelState);
        }
        #endregion
    }
}