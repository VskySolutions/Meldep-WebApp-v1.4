using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Extensions.DependencyInjection;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
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
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting(PublicEndpointLimit.PolicyName)]
    public class AuthController : BaseController
    {
        #region Fields
        private const string NoSiteAccessMessage = "Your account isn't assigned to an active site. Please contact your administrator.";
        private const string InvalidResetLinkMessage ="This password reset link is invalid or has expired. Please request a new one.";
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
        private readonly MicrosoftIdTokenValidator _microsoftIdTokenValidator;
        private readonly Vsky.Services.Authorization.ISiteAccessService _siteAccessService;
        private readonly Vsky.Services.Authorization.SecurityAuthorizationOptions _authorizationOptions;
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
            IPersonSitesMappingService personSitesMappingService,
            MicrosoftIdTokenValidator microsoftIdTokenValidator,
            Vsky.Services.Authorization.ISiteAccessService siteAccessService,
            Microsoft.Extensions.Options.IOptions<Vsky.Services.Authorization.SecurityAuthorizationOptions> authorizationOptions
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
            _microsoftIdTokenValidator = microsoftIdTokenValidator;
            _siteAccessService = siteAccessService;
            _authorizationOptions = authorizationOptions.Value;
        }
        #endregion

        #region Utilities
        private static long ToUnixEpochDate(DateTime date)
        {
            return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        }

        // One rule for every login path: the last-used site if the user may still act there,
        // otherwise their oldest valid site. Returns null when enforcing and none is valid.
        private async Task<Site> ResolveLoginSite(ApplicationUser user, PersonSitesMapping defaultMapping)
        {
            // The same rule the API uses for a request that names no site, so both open the same site.
            var defaultSiteId = await _siteAccessService.GetDefaultSiteIdAsync(user.Id);
            if (defaultSiteId != null)
                return await _siteService.GetById(defaultSiteId);

            if (_authorizationOptions.SiteGateMode == Vsky.Services.Authorization.AuthorizationMode.Enforce)
                return null;

            var lastUsedSite = await _personSitesMappingService.GetLastUsedSiteByPersonId(defaultMapping.PersonId);
            return lastUsedSite != null ? lastUsedSite.Sites : defaultMapping.Sites;
        }

        private void LogAuthEvent(string message, string userId = null)
        {
            HttpContext.RequestServices.GetRequiredService<SecurityLog>().Write(Vsky.Models.LogLevel.Information, message, string.Empty, userId);
        }

        // Records that the user chose to sign out. It only records: the token stays valid until it expires.
        [HttpPost("logout")]
        [AuthenticatedOnly]
        public IActionResult Logout()
        {
            LogAuthEvent("Logout", User.GetLoggedInUserId<string>());
            return NoContent();
        }

        // Keeps an active user signed in, so tokens can be short-lived without extra logins. The
        // current token must still be valid, and the password must not have changed since.
        [HttpPost("refresh")]
        [AuthenticatedOnly]
        public async Task<IActionResult> RefreshToken()
        {
            var user = await _userManager.FindByIdAsync(User.GetLoggedInUserId<string>());
            if (user == null || user.Deleted || !user.Active)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { token = GenerakeToken(user, _jwtTokenConfig, roles), expiresIn = (int)_jwtTokenConfig.ValidFor.TotalSeconds });
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
                new(SessionStamp.ClaimType, SessionStamp.For(user.SecurityStamp))
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

        #region Get user by token
        [HttpGet("user-by-token")]
        [AllowAnonymous]
        public Task<IActionResult> getUserByToken(string token)
        {
            return GetUserFromValidatedToken(token);
        }

        // Same as the GET above, but keeps the token out of URLs and server logs.
        [HttpPost("user-by-token")]
        [AllowAnonymous]
        public Task<IActionResult> PostUserByToken(UserByTokenModel model)
        {
            return GetUserFromValidatedToken(model?.Token);
        }

        // A token not signed by this API could never authenticate a later request,
        // so it must not reveal any user data here either.
        private JwtSecurityToken ValidateOwnToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtTokenConfig.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtTokenConfig.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenConfig.SecurityKey))
                }, out var validatedToken);

                return validatedToken as JwtSecurityToken;
            }
            catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
            {
                return null;
            }
        }

        private async Task<IActionResult> GetUserFromValidatedToken(string token)
        {
            var jwtToken = ValidateOwnToken(token);
            if (jwtToken != null)
            {
                var userEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user != null && !user.Deleted && user.Active)
                {
                    //Get user roles
                    var roles = await _userManager.GetRolesAsync(user);

                    //Find Person Info
                    var personMappingData = await _personsService.GetPersonSiteMappingByPersonId(user.PersonId, "");
                    if (personMappingData == null || personMappingData.Person == null || personMappingData.Sites == null)
                    {
                        LogAuthEvent("Login denied: no site access", user.Id);
                        return BadRequest(new BadRequestError("Invalid User or Site Information."));
                    }

                    //Find Site details
                    var personData = personMappingData.Person;
                    var SiteData = await ResolveLoginSite(user, personMappingData);
                    if (SiteData == null)
                    {
                        LogAuthEvent("Login denied: no site access", user.Id);
                        return BadRequest(new BadRequestError(NoSiteAccessMessage));
                    }
                    var LandingPageLink = await _sitesModulesMenusService.GetLandingPageBySiteId(SiteData.Id);
                    var EmployeeId = _commonService.GetEmployeeIdByUserIdAndEmail(SiteData.Id, user.Id);
                    var GetDateTime = _siteService.GetDateTime(SiteData.TimeZone);

                    // Fetch the normalized (site-specific) role names
                    var normalizedRoles = await _applicationUserRoleService.GetNormalizedRoleNamesByUserAndSite(user.Id, SiteData.Id);

                    //Collect users info and token data (mirror the /auth/login response so
                    //the SPA can set the X-Site-Id tenant header and resolve permissions)
                    var tokenResult = new TokenResultModel
                    {
                        SiteId = SiteData.Id,
                        UserId = user.Id,
                        PersonId = personData.Id,
                        EmployeeId = EmployeeId,
                        SiteName = SiteData.Name,
                        SiteLandingPageLink = LandingPageLink,
                        SiteTimeZone = SiteData.TimeZone,
                        Username = user.UserName,
                        UserEmail = user.Email,
                        Email = user.Email,
                        FirstName = personData.FirstName,
                        LastName = personData.LastName,
                        Roles = roles.Any() ? normalizedRoles.ToArray() : null,
                        Token = token,
                        ExpiresIn = (int)_jwtTokenConfig.ValidFor.TotalSeconds,
                        CreatedAt = GetDateTime,
                        IsFileUploadOrExternal = SiteData.IsFileUploadOrExternal
                    };
                    LogAuthEvent("Login succeeded", tokenResult.UserId); return Ok(tokenResult);
                }
                else
                {
                    LogAuthEvent("Login failed"); return BadRequest(new BadRequestError("Invalid login attempt."));
                }
            }
            else
            {
                LogAuthEvent("Login failed"); return BadRequest(new BadRequestError("Invalid login attempt."));
            }
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
                                LogAuthEvent("Login denied: no site access", userData.Id);
                                return BadRequest(new BadRequestError("Invalid User or Site Information."));
                            }

                            //Find Site details
                            var personData = personMappingData.Person;
                            var SiteData = await ResolveLoginSite(userData, personMappingData);
                            if (SiteData == null)
                            {
                                await _signInManager.SignOutAsync();
                                LogAuthEvent("Login denied: no site access", userData.Id);
                                return BadRequest(new BadRequestError(NoSiteAccessMessage));
                            }
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
                                IsFileUploadOrExternal = SiteData.IsFileUploadOrExternal
                            };

                            LogAuthEvent("Login succeeded", tokenResult.UserId); return Ok(tokenResult);                          
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
                            LogAuthEvent("Login blocked: account locked", userData.Id); return BadRequest(new BadRequestError("User account locked out."));
                        }
                        else
                        {
                            LogAuthEvent("Login failed", userData.Id); return BadRequest(new BadRequestError("Invalid login attempt."));
                        }
                    }
                }
                LogAuthEvent("Login failed"); return BadRequest(new BadRequestError("Invalid login attempt."));
            }
            catch (Exception ex)
            {
                return BadRequest(SafeErrorMessage(ex));
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
                // Validate signature, lifetime, audience and tenant before trusting any claim
                var authtoken = Authorization.Token.Replace("Bearer ", "");
                var email = await _microsoftIdTokenValidator.ValidateAndGetEmailAsync(authtoken);
                if (string.IsNullOrEmpty(email))
                {
                    LogAuthEvent("Microsoft login rejected"); return Unauthorized("Invalid token.");
                }

                var userData = await _userManager.FindByEmailAsync(email);
                if (userData == null || userData.Deleted || !userData.Active)
                {
                    LogAuthEvent("Microsoft login rejected", userData?.Id);
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
                        LogAuthEvent("Login denied: no site access", userData.Id);
                        return BadRequest(new BadRequestError("Invalid User or Site Information."));
                    }

                    //Find Site details
                    var personData = personMappingData.Person;
                    var SiteData = await ResolveLoginSite(userData, personMappingData);
                    if (SiteData == null)
                    {
                        await _signInManager.SignOutAsync();
                        LogAuthEvent("Login denied: no site access", userData.Id);
                        return BadRequest(new BadRequestError(NoSiteAccessMessage));
                    }
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
                        IsMsLogin = true,
                        IsFileUploadOrExternal = SiteData.IsFileUploadOrExternal
                    };
                    //Collect user roles
                    if (roles != null && roles.Count > 0)
                    {
                        tokenResult.Roles = normalizedRoles.ToArray();
                        tokenResult.RolesName = roles.ToArray();
                    }

                    LogAuthEvent("Login succeeded", tokenResult.UserId); return Ok(tokenResult);
                }
            }
            catch (Exception)
            {
                return Unauthorized("Token validation failed.");
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
                // The response is the same in every case, so this endpoint can't be used
                // to find out whether an email address has an account.
                var genericResponse = new { success = true, message = "If an account exists for this email, a password reset link has been sent." };

                var user = await _userManager.FindByEmailAsync(model.Email);
                // Check if user exists, is not deleted, and has a confirmed email
                if (user == null || user.Deleted || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return Ok(genericResponse);
                }

                var SiteId = _globalVariable.SiteId;

                //Check Email Notification Permission
                var canSend = await _sitesEmailNotificationsPermissionServices.ShouldSendNotification(SiteId, user.Id, "User.ResetPassword");

                if (!canSend)
                    return Ok(genericResponse);

                // Generate a password reset token for the user
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Email the reset link, which carries this token
                await _workflowMessageService.SendPasswordResetEmail(user, code); LogAuthEvent("Password reset requested", user.Id);

                return Ok(genericResponse);
            }
            // Return model state error response if the model state is invalid
            return ModelStateError(ModelState);
        }

        #endregion

        #region Get reset link user
        // Returns only the email address, and only for a valid reset link.
        [HttpPost("user")]
        [AllowAnonymous]
        public async Task<IActionResult> GetResetLinkUser(ResetLinkModel model)
        {
            var user = string.IsNullOrEmpty(model?.UserId) ? null : await _userManager.FindByIdAsync(model.UserId);

            var linkIsValid = user != null
                && !user.Deleted
                && !string.IsNullOrEmpty(model.Token)
                && await _userManager.VerifyUserTokenAsync(
                    user,
                    _userManager.Options.Tokens.PasswordResetTokenProvider,
                    UserManager<ApplicationUser>.ResetPasswordTokenPurpose,
                    model.Token);

            if (!linkIsValid)
                return BadRequest(new BadRequestError(InvalidResetLinkMessage));

            return Ok(new { email = user.Email });
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
                // Check if user exists, is not deleted, has a confirmed email, and sent the emailed token
                if (user == null || user.Deleted || string.IsNullOrEmpty(model.Token) || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    return BadRequest(new BadRequestError(InvalidResetLinkMessage));
                }
                // The token must be the one from the emailed link. It expires and works only once.
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
                if (result.Succeeded)
                {
                    LogAuthEvent("Password reset completed", user.Id); return Ok("Your password has been reset successfully. Log in with your new password.");
                }
                else if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken)))
                {
                    return BadRequest(new BadRequestError(InvalidResetLinkMessage));
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