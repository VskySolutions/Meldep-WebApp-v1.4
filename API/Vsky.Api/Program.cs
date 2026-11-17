using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Ical.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using StackExchange.Profiling;
using Swashbuckle.AspNetCore.SwaggerUI;
using Vsky.Api.ApiErrors;
using Vsky.Api.Converter;
using Vsky.Api.Extensions;
using Vsky.Api.Helpers;
using Vsky.Api.Models;
using Vsky.Core.Configuration;
using Vsky.Core.Infrastructure;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.AdPosts;
using Vsky.Services.ApplicationUserRoles;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Candidates;
using Vsky.Services.Common;
using Vsky.Services.Companies;
using Vsky.Services.Configuration;
using Vsky.Services.Contacts;
using Vsky.Services.CustomersFile;
using Vsky.Services.DailyPlanners;
using Vsky.Services.Dashboard;
using Vsky.Services.Departments;
using Vsky.Services.Domains;
using Vsky.Services.DropDowns;
using Vsky.Services.DropDownTypes;
using Vsky.Services.EmailNotifications;
using Vsky.Services.EmailReply;
using Vsky.Services.EmployeeClientLocations;
using Vsky.Services.EmployeeDepartments;
using Vsky.Services.EmployeeDesignations;
using Vsky.Services.EmployeeLeaves;
using Vsky.Services.EmployeeOrgLocations;
using Vsky.Services.EmployeeOrgStructureDesignationMappings;
using Vsky.Services.EmployeeOrgStructures;
using Vsky.Services.Employees;
using Vsky.Services.EmployeeStatuses;
using Vsky.Services.EmployeeTypes;
using Vsky.Services.Expences;
using Vsky.Services.ExpenseExpenseBankAccounts;
using Vsky.Services.ExpenseExpensExpense_Lines;
using Vsky.Services.ExpenseLines;
using Vsky.Services.ExpenseVendorBankAccount;
using Vsky.Services.FilePathDetail;
using Vsky.Services.GlobalVariables;
using Vsky.Services.HelpDesks;
using Vsky.Services.ImageMigration;
using Vsky.Services.InfraAccounts;
using Vsky.Services.InfraDashboard;
using Vsky.Services.InfraDatabases;
using Vsky.Services.InfraFTPs;
using Vsky.Services.InfraProjectInstances;
using Vsky.Services.Inventories;
using Vsky.Services.IssueActivitys;
using Vsky.Services.Issues;
using Vsky.Services.ItemCategories;
using Vsky.Services.ItemSubCategoryAttribute;
using Vsky.Services.JobsCreate;
using Vsky.Services.LeadActivityLogss;
using Vsky.Services.Leads;
using Vsky.Services.LeadUserGroupMappings;
using Vsky.Services.LeaveCredits;
using Vsky.Services.LeaveRule;
using Vsky.Services.LeaveRuleLine;
using Vsky.Services.LeaveSchedule;
using Vsky.Services.Logging;
using Vsky.Services.Messages;
using Vsky.Services.Module;
using Vsky.Services.MovementRegisters;
using Vsky.Services.Note;
using Vsky.Services.Notifications;
using Vsky.Services.Persons;
using Vsky.Services.PowerBI;
using Vsky.Services.ProjectActionItem;
using Vsky.Services.ProjectActivities;
using Vsky.Services.ProjectEmployeeMappings;
using Vsky.Services.ProjectMessage;
using Vsky.Services.ProjectModuleEmployeeMappings;
using Vsky.Services.ProjectModules;
using Vsky.Services.ProjectModulesUserMappings;
using Vsky.Services.ProjectQuestionsAnswer;
using Vsky.Services.ProjectReleaseTrackings;
using Vsky.Services.Projects;
using Vsky.Services.ProjectsColor;
using Vsky.Services.ProjectsPinned;
using Vsky.Services.ProjectsTag;
using Vsky.Services.ProjectSwimLanes;
using Vsky.Services.ProjectTasks;
using Vsky.Services.ProjectUserMappings;
using Vsky.Services.ProjectWeeklyPlan;
using Vsky.Services.ReportRoleGroupMappings;
using Vsky.Services.ReportSetting;
using Vsky.Services.ReportSettingDetail;
using Vsky.Services.ReportUserMappings;
using Vsky.Services.RequirementDataMappings;
using Vsky.Services.RequirementStatusTrackings;
using Vsky.Services.Requirements;
using Vsky.Services.RequirementsColor;
using Vsky.Services.RequirementsPinned;
using Vsky.Services.SalesPersons;
using Vsky.Services.Security;
using Vsky.Services.Servers;
using Vsky.Services.SetReminders;
using Vsky.Services.Sites;
using Vsky.Services.SitesItem;
using Vsky.Services.SitesItemSubCategoryAttributesMappings;
using Vsky.Services.SitesModifiedLog;
using Vsky.Services.SitesModule;
using Vsky.Services.SitesModulesMenu;
using Vsky.Services.SitesModulesMenusPermission;
using Vsky.Services.SitesRole;
using Vsky.Services.SOPAssignments;
using Vsky.Services.SOPProcesses;
using Vsky.Services.SOPTemplates;
using Vsky.Services.TestCases;
using Vsky.Services.TestPlans;
using Vsky.Services.TimeInTimeOuts;
using Vsky.Services.TimesheetAISummarys;
using Vsky.Services.Timesheets;
using Vsky.Services.Analytics;
using Vsky.Services.TimeZone;
using Vsky.Services.TrainingPortalMappings;
using Vsky.Services.TrainingPortals;
using Vsky.Services.Users;
using Vsky.Services.Website_Demo;
using Vsky.Services.Website_Demo_Module;
using Microsoft.AspNetCore.Authorization;

namespace Vsky.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
             var builder = WebApplication.CreateBuilder(args);

            // add services to the container
            var mvcBuilder = builder.Services.AddControllers(options =>
            {
                options.MaxModelValidationErrors = 50;

                // Registered once here, so it covers every action, including any added later.
                options.Filters.Add<AuthorizationEnforcementFilter>();
                options.Filters.Add<SiteScopeActionFilter>();
                options.Filters.Add<PermissionCacheInvalidationFilter>();
                options.Filters.Add<ActivityLogFilter>();
            });

            // Off unless configured, so file URLs stay exactly as they are until this is switched on.
            var blobUrlSigner = BlobUrlSigner.IsSignedMode(builder.Configuration)
                ? new BlobUrlSigner(builder.Configuration.GetConnectionString("AzureBlob"), builder.Configuration.GetValue("BlobAccess:SignatureMinutes", 60))
                : null;
            if (blobUrlSigner != null)
            {
                mvcBuilder.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new BlobUrlJsonConverter(blobUrlSigner)));
            }

            mvcBuilder.AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver { Modifiers = { SensitiveUserFields.Hide } };
                options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
                options.JsonSerializerOptions.Converters.Add(new CustomTimeSpanConverter());
            });

            // register all available validators from vsky assemblies
            var assemblies = mvcBuilder.PartManager.ApplicationParts
                .OfType<AssemblyPart>()
                .Where(part => part.Name.StartsWith("Vsky", StringComparison.InvariantCultureIgnoreCase))
                .Select(part => part.Assembly);

            builder.Services.AddValidatorsFromAssemblies(assemblies);

            // configure invalid model error model and formats
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    // Field names only: the values can hold personal data or passwords.
                    RequestLogState.For(actionContext.HttpContext).InvalidFields = actionContext.ModelState
                        .Where(field => field.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                        .Select(field => field.Key)
                        .Take(20)
                        .ToArray();

                    return new BadRequestObjectResult(new ValidationError(actionContext.ModelState));
                };
            });

            //builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            //{
            //    options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
            //    options.JsonSerializerOptions.Converters.Add(new CustomTimeSpanConverter());
            //});

            // add fluent validation
            builder.Services.AddFluentValidationAutoValidation();

            // db context
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(60); // 1 minutes
                    });
            }, ServiceLifetime.Scoped);

            // configure the data protection system to persist keys to the specified directory
            var dataProtectionKeysFolder = new DirectoryInfo("App_Data/DataProtectionKeys");
            builder.Services.AddDataProtection().PersistKeysToFileSystem(dataProtectionKeysFolder);

            // add identity
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedAccount = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            // identity options
            builder.Services.Configure<IdentityOptions>(options =>
            {
                // password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8; // new passwords only; existing ones keep working
                options.Password.RequiredUniqueChars = 1;

                // lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // user settings
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // sign in
                options.SignIn.RequireConfirmedEmail = true;
            });

            // jwt token config
            var jwtTokenConfig = builder.Configuration.GetSection("JwtTokenConfig").Get<JwtTokenConfig>();
            builder.Services.AddSingleton(jwtTokenConfig);

            // add authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtTokenConfig.Issuer,
                    ValidAudience = jwtTokenConfig.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtTokenConfig.SecurityKey)),
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    NameClaimType = JwtRegisteredClaimNames.Sub
                };
                cfg.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async ctx =>
                    {
                        var userManager = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                        var subClaim = ctx.Principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                                    ?? ctx.Principal.FindFirst(ClaimTypes.Email)?.Value;

                        if (string.IsNullOrEmpty(subClaim))
                        {
                            ctx.Fail("Missing subject (sub) or email claim in token.");
                            return;
                        }

                        // Try to find the user based on email (since you changed Sub to store Email)
                        var user = await userManager.FindByEmailAsync(subClaim);

                        if (user == null || !user.Active || user.Deleted)
                        {
                            ctx.Fail("No active user account found.");
                            return;
                        }

                        // A password change updates the stamp, which ends the user's older sessions.
                        var sessionStamp = ctx.Principal.FindFirst(SessionStamp.ClaimType)?.Value;
                        if (sessionStamp != null && sessionStamp != SessionStamp.For(user.SecurityStamp))
                        {
                            ctx.Fail("This session has ended.");
                            return;
                        }

                        // The web app sends the site it got at login on every request. A client that sends no
                        // X-Site-Id (Swagger, Postman, scripts) gets that same site, chosen by the same rule, so
                        // one user and token see one site everywhere. Only the user's own accessible sites can be
                        // chosen, and every site, menu and record check still runs. A site that IS sent is never
                        // replaced, so a wrong value is still denied.
                        var tenant = ctx.HttpContext.RequestServices.GetRequiredService<GlobalVariable>();
                        if (string.IsNullOrWhiteSpace(tenant.SiteId))
                        {
                            var defaultSiteId = await ctx.HttpContext.RequestServices
                                .GetRequiredService<Vsky.Services.Authorization.ISiteAccessService>()
                                .GetDefaultSiteIdAsync(user.Id);
                            if (defaultSiteId != null)
                            {
                                tenant.SiteId = defaultSiteId;
                                if (string.IsNullOrWhiteSpace(tenant.TimeZone))
                                    tenant.TimeZone = (await ctx.HttpContext.RequestServices.GetRequiredService<ISiteService>().GetById(defaultSiteId))?.TimeZone;
                            }
                        }
                    },
                    // Runs only when a protected endpoint rejects the request, so an expired
                    // token sent to an anonymous endpoint no longer starts a 401 response.
                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();
                        if (ctx.Response.HasStarted)
                            return;

                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        ctx.Response.ContentType = "application/json";

                        var error = new AuthError(StatusCodes.Status401Unauthorized, "Authentication failed.");
                        await System.Text.Json.JsonSerializer.SerializeAsync(ctx.Response.Body, error);
                        await ctx.Response.Body.FlushAsync();
                    }
                };

            });

            // mini profiler
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddMiniProfiler(options =>
                {
                    options.RouteBasePath = "/profiler";
                    options.IgnoredPaths.Add("/swagger");
                    options.ResultsAuthorize = request => IsUserAuthorized(request);
                    options.ResultsListAuthorize = request => IsUserAuthorized(request);
                    options.ShouldProfile = request => ShouldProfile(request);
                    options.ColorScheme = ColorScheme.Dark;
                }).AddEntityFramework();
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            builder.Services.AddResponseCompression();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;

            // services
            builder.Services.AddScoped<IAppFileProvider, AppFileProvider>();
            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            //builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

            // Change MT
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<GlobalVariable>();
            // Slows password guessing and form abuse. Per client address, on anonymous endpoints only.
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy(PublicEndpointLimit.PolicyName, context =>
                    System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                        {
                            PermitLimit = PublicEndpointLimit.RequestsPerMinute,
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });
            builder.Services.AddScoped<AdminAccess>();
            builder.Services.AddScoped<Vsky.Services.Authorization.ISiteAccessService, Vsky.Services.Authorization.SiteAccessService>();
            builder.Services.Configure<Vsky.Services.Authorization.SecurityAuthorizationOptions>(builder.Configuration.GetSection("Authorization"));
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<Vsky.Services.Authorization.PermissionCache>();
            builder.Services.AddScoped<Vsky.Services.Authorization.IPermissionSource, Vsky.Services.Authorization.RolePermissionSource>();
            builder.Services.AddScoped<Vsky.Services.Authorization.IPermissionSource, Vsky.Services.Authorization.GroupPermissionSource>();
            builder.Services.AddScoped<Vsky.Services.Authorization.IPermissionSource, Vsky.Services.Authorization.UserPermissionSource>();
            builder.Services.AddScoped<Vsky.Services.Authorization.IPermissionService, Vsky.Services.Authorization.PermissionService>();
            builder.Services.AddSingleton<SecurityLog>();
            builder.Services.AddSingleton<Vsky.Services.Authorization.ISecurityLog>(provider => provider.GetRequiredService<SecurityLog>());
            builder.Services.AddSingleton<ActivityLogQueue>();
            builder.Services.AddHostedService(provider => provider.GetRequiredService<ActivityLogQueue>());
            builder.Services.AddScoped<Vsky.Services.SecurityAdmins.ISecurityAdminService, Vsky.Services.SecurityAdmins.SecurityAdminService>();
            builder.Services.AddScoped<AuthorizationAuditLog>();
            builder.Services.AddScoped<Vsky.Services.Authorization.ProjectRecordAccess>();
            builder.Services.AddSingleton<EndpointClassificationScan>();
            builder.Services.AddHostedService(provider => provider.GetRequiredService<EndpointClassificationScan>());
            builder.Services.AddSingleton<MicrosoftIdTokenValidator>();

            builder.Services.AddScoped<ILogger, DefaultLogger>();
            builder.Services.AddScoped<IApplicationUserRoleService, ApplicationUserRoleService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IWorkflowMessageService, WorkflowMessageService>();
            builder.Services.AddScoped<ISmtpBuilder, SmtpBuilder>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IEmailAccountService, EmailAccountService>();
            builder.Services.AddScoped<ITokenizer, Tokenizer>();
            builder.Services.AddScoped<IMessageTokenProvider, MessageTokenProvider>();
            builder.Services.AddScoped<IEncryptionService, EncryptionService>();
            builder.Services.AddScoped<IContactService, ContactService>();
            builder.Services.AddScoped<ICommonService, CommonService>();
            builder.Services.AddScoped<ISiteService, SiteService>();
            builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
            builder.Services.AddScoped<ICompanyService, CompanyService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IDropDownService, DropDownService>();
            builder.Services.AddScoped<IDropDownTypeService, DropDownTypeService>();
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();
            builder.Services.AddScoped<IProjectActivityService, ProjectActivityService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<IProjectTaskService, ProjectTaskService>();
            builder.Services.AddScoped<ITagService, TagService>();
            builder.Services.AddScoped<IProjectTaskTagService, ProjectTaskTagService>();
            builder.Services.AddScoped<IProjectsTagService, ProjectsTagService>();
            builder.Services.AddScoped<IProjectsPinnedService, ProjectsPinnedService>();
            builder.Services.AddScoped<IProjectsColorService, ProjectsColorService>();
            builder.Services.AddScoped<IProjectTaskRelatedMappingsService, ProjectTaskRelatedMappingsService>();
            builder.Services.AddScoped<IProjectModuleService, ProjectModuleService>();
            builder.Services.AddScoped<ILeadService, LeadService>();
            builder.Services.AddScoped<ILeadUserGroupMappingService, LeadUserGroupMappingService>();
            builder.Services.AddScoped<ICompanyContactsService, CompanyContactsService>();
            builder.Services.AddScoped<ILeadActivityLogsService, LeadActivityLogsService>();
            builder.Services.AddScoped<ISetReminderService, SetReminderService>();
            builder.Services.AddScoped<IProjectEmployeeMappingService, ProjectEmployeeMappingService>();
            builder.Services.AddScoped<IProjectEmployeeRoleMappingService, ProjectEmployeeRoleMappingService>();
            builder.Services.AddScoped<IPersonService, PersonService>();
            builder.Services.AddScoped<IPersonSitesMappingService, PersonSitesMappingService>();
            builder.Services.AddScoped<IDailyPlannerService, DailyPlannerService>();
            builder.Services.AddScoped<IDailyPlannerLineService, DailyPlannerLineService>();
            builder.Services.AddScoped<ITimesheetService,TimesheetService>();
            builder.Services.AddScoped<ITimesheetRegisterService, TimesheetRegisterService>();
            builder.Services.AddScoped<IAttendanceRegisterService, AttendanceRegisterService>();
            builder.Services.AddScoped<ITimesheetLinesService, TimesheetLinesService>();
            builder.Services.AddScoped<ITimesheetAISummaryService, TimesheetAISummaryService>();
            builder.Services.AddScoped<IEmployeeTypeService, EmployeeTypeService>();
            builder.Services.AddScoped<IEmployeeStatusService, EmployeeStatusService>();
            builder.Services.AddScoped<IEmployeeDepartmentService, EmployeeDepartmentService>();
            builder.Services.AddScoped<IEmployeeDesignationService, EmployeeDesignationService>();
            builder.Services.AddScoped<IEmployeeOrgLocationService, EmployeeOrgLocationService>();
            builder.Services.AddScoped<IEmployeeOrgStructureDesignationMappingService, EmployeeOrgStructureDesignationMappingService>();
            builder.Services.AddScoped<IEmployeeClientLocationService, EmployeeClientLocationService>();
            builder.Services.AddScoped<ISalesPersonService, SalesPersonService>();
            builder.Services.AddScoped<ICompanyClientsService, CompanyClientsService>();
            builder.Services.AddScoped<ILeaveCreditService, LeaveCreditService>();
            builder.Services.AddScoped<ILeaveRulesService, LeaveRulesService>();
            builder.Services.AddScoped<ILeaveRuleLinesService, LeaveRuleLinesService>();
            builder.Services.AddScoped<IEmployeeLeaveService, EmployeeLeaveService>();
            builder.Services.AddScoped<ILeaveScheduleService, LeaveScheduleService>();
            builder.Services.AddScoped<INoteService, NoteService>();
            builder.Services.AddScoped<ITestPlanService, TestPlanService>();
            builder.Services.AddScoped<ITestCaseService, TestCaseService>();
            builder.Services.AddScoped<ITestCaseExecutionLogExecutionLogService, TestCaseExecutionLogExecutionLogService>();
            builder.Services.AddScoped<IIssueService, IssueService>();
            builder.Services.AddScoped<IImageMigrationService, ImageMigrationService>();
            builder.Services.AddScoped<IIssueStatusChangedLogService, IssueStatusChangedLogService>();
            builder.Services.AddScoped<IIssueActivityService, IssueActivityService>();
            builder.Services.AddScoped<IModulesService, ModulesService>();
            builder.Services.AddScoped<IModulesMenusService, ModulesMenusService>();
            builder.Services.AddScoped<ISitesModulesService, SitesModulesService>();
            builder.Services.AddScoped<ISitesModifiedLogsService, SitesModifiedLogsService>();

            builder.Services.AddScoped<IRequirementService, RequirementService>();
            builder.Services.AddScoped<IRequirementGroupService, RequirementGroupService>();
            builder.Services.AddScoped<IRequirementsPinnedService, RequirementsPinnedService>();
            builder.Services.AddScoped<IRequirementsColorService, RequirementsColorService>();
            builder.Services.AddScoped<IFilePathDetailsService, FilePathDetailsService>();
            builder.Services.AddScoped<IRequirementChangeLogService, RequirementChangeLogService>();
            builder.Services.AddScoped<IRequirementTagService, RequirementTagService>();
            builder.Services.AddScoped<IRequirementDataMappingService, RequirementDataMappingService>();
            builder.Services.AddScoped<IRequirementDataMappingNotesService, RequirementDataMappingNotesService>();
            builder.Services.AddScoped<IRequirementStatusTrackingService, RequirementStatusTrackingService>();

            builder.Services.AddScoped<IAdPostService, AdPostService>();
            builder.Services.AddScoped<IAdPostChannelService, AdPostChannelService>();
            builder.Services.AddScoped<IAdPostingStatusService, AdPostingStatusService>();
            builder.Services.AddScoped<IServerService, ServerService>();
            builder.Services.AddScoped<IServerPaymentsService, ServerPaymentsService>();
            builder.Services.AddScoped<ITrainingPortalService, TrainingPortalService>();
            builder.Services.AddScoped<ITrainingPortalMappingService, TrainingPortalMappingService>();
            builder.Services.AddScoped<IDomainService, DomainService>();
            builder.Services.AddScoped<IDomainAttributeService, DomainAttributeService>();
            builder.Services.AddScoped<IInfraAccountService, InfraAccountService>();
            builder.Services.AddScoped<IInfraAccountServiceCalculationService, InfraAccountServiceCalculationService>();
            builder.Services.AddScoped<IInfraAccountServicesService, InfraAccountServicesService>();
            builder.Services.AddScoped<IInfraProjectServicesService, InfraProjectServicesService>();
            builder.Services.AddScoped<IInfraDashboardAggregationService, InfraDashboardAggregationService>();
            builder.Services.AddScoped<IInfraFTPService, InfraFTPService>();
            builder.Services.AddScoped<IInfraFTPsProjectInstanceMappingService, InfraFTPsProjectInstanceMappingService>();
            builder.Services.AddScoped<IInfraDatabaseService, InfraDatabaseService>();
            builder.Services.AddScoped<IInfraDatabaseProjectInstanceMappingService, InfraDatabaseProjectInstanceMappingService>();
            builder.Services.AddScoped<IInfraProjectInstanceService, InfraProjectInstanceService>();
            builder.Services.AddScoped<IInfraProjectInstanceRoleService, InfraProjectInstanceRoleService>();
            builder.Services.AddScoped<IInfraProjectInstanceRoleUsersService, InfraProjectInstanceRoleUsersService>();
            builder.Services.AddScoped<ITimeInTimeOutService, TimeInTimeOutService>();
            builder.Services.AddScoped<ITimeInTimeOutBreakDetailService, TimeInTimeOutBreakDetailService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<IInventoryAssignmentService, InventoryAssignmentService>();
            builder.Services.AddScoped<IInventoryItemTypeService, InventoryItemTypeService>();
            builder.Services.AddScoped<IItemCategoriesService, ItemCategoriesService>();
            builder.Services.AddScoped<IItemSubcategoriesService, ItemSubcategoriesService>();
            builder.Services.AddScoped<IItemSubCategoryAttributesService, ItemSubCategoryAttributesService>();
            builder.Services.AddScoped<IItemSubCategoryAttributesValuesService, ItemSubCategoryAttributesValuesService>();
            builder.Services.AddScoped<ISitesItemSubCategoryAttributesMappingService, SitesItemSubCategoryAttributesMappingService>();
            builder.Services.AddScoped<IProjectTaskStatusLogService, ProjectTaskStatusLogService>();
            builder.Services.AddScoped<IIssueStatusChangedLogService, IssueStatusChangedLogService>();
            builder.Services.AddScoped<IIssueActivityService, IssueActivityService>();
            builder.Services.AddScoped<IReportSettingsService, ReportSettingsService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IJobCreateService, JobCreateService>();
            builder.Services.AddScoped<ICandidateService, CandidateService>();
            builder.Services.AddScoped<ICandidateDepartmentService, CandidateDepartmentService>();
            builder.Services.AddScoped<ICandidateNoteService, CandidateNoteService>();
            builder.Services.AddScoped<ICandidateActivityService, CandidateActivityService>();
            builder.Services.AddScoped<ICandidateFeedbacksService, CandidateFeedbacksService>();
            builder.Services.AddScoped<IReportSettingDetailService, ReportSettingDetailService>();
            builder.Services.AddScoped<IExpense_BankAccountsService, Expense_BankAccountsService>();
            builder.Services.AddScoped<IExpenseService, ExpenseService>();
            builder.Services.AddScoped<IExpenseFileService, ExpenseFileService>();
            builder.Services.AddScoped<IExpense_LinesService, Expense_LinesService>();
            builder.Services.AddScoped<IExpenseVendorBankAccountsService, ExpenseVendorBankAccountsService>();
            builder.Services.AddScoped<IExpenseVendorsService, ExpenseVendorsService>();
            
            builder.Services.AddScoped<AadService>();
            builder.Services.AddScoped<EmbedService>();
            builder.Services.AddScoped<ConfigValidatorService>();
            builder.Services.AddScoped<IEmployeeOrgStructureService, EmployeeOrgStructureService>();
            builder.Services.AddScoped<IProjectFilesService, ProjectFilesService>();
            builder.Services.AddScoped<IProjectModuleFilesService, ProjectModuleFilesService>();
            builder.Services.AddScoped<IProjectTaskFilesService, ProjectTaskFilesService>();
            builder.Services.AddScoped<IProjectActivityFilesService, ProjectActivityFilesService>();
            builder.Services.AddScoped<IProjectMessageService, ProjectMessageService>();
            builder.Services.AddScoped<IProjectSwimLanesService, ProjectSwimLanesService>();
            builder.Services.AddScoped<IProjectSwimLanesListServices, ProjectSwimLanesListServices>();
            builder.Services.AddScoped<IProjectSwimLanesListsTasksServices, ProjectSwimLanesListsTasksServices>();
            builder.Services.AddScoped<IMasterProjectSwimlaneListsServices, MasterProjectSwimlaneListsServices>();
            builder.Services.AddScoped<ICustomerFilesService, CustomerFilesService>();
            builder.Services.AddScoped<ICustomerFilesLinesService, CustomerFilesLinesService>();
            builder.Services.AddScoped<ISitesRolesService, SitesRolesService>();
            builder.Services.AddScoped<ISitesModulesMenusService, SitesModulesMenusService>();
            builder.Services.AddScoped<ISitesModulesMenusPermissionsService, SitesModulesMenusPermissionsService>();
            builder.Services.AddScoped<IProjectWeeklyService, ProjectWeeklyService>();
            builder.Services.AddScoped<IProjectWeeklyDatesService, ProjectWeeklyDatesService>();
            builder.Services.AddScoped<IProjectWeeklyDatesLinesService, ProjectWeeklyDatesLinesService>();
            builder.Services.AddScoped<IProjectWeeklyPlanDatesLinesAssignedToService, ProjectWeeklyPlanDatesLinesAssignedToService>();
            builder.Services.AddScoped<IProjectUserMappingService, ProjectUserMappingService>();
            builder.Services.AddScoped<IVWDashboardServices, VWDashboardServices>();
            builder.Services.AddScoped<INotificationPermissionsService, NotificationPermissionsService>();
            builder.Services.AddScoped<IMasterNotificationService, MasterNotificationService>();
            builder.Services.AddScoped<IWebsite_DemosService, Website_DemosService>();
            builder.Services.AddScoped<IWebsite_Demo_ModulesService, Website_Demo_ModulesService>();
            builder.Services.AddScoped<IExpense_Advance_Requests_Service, Expense_Advance_Requests_Service>();
            builder.Services.AddScoped<IExpense_Purchase_Requests_Service, Expense_Purchase_Requests_Service>();
            builder.Services.AddScoped<IReportUserMappingService, ReportUserMappingService>();
            builder.Services.AddScoped<IReportRoleGroupMappingService, ReportRoleGroupMappingService>();
            builder.Services.AddScoped<IMovementRegisterServices, MovementRegisterServices>();
            builder.Services.AddScoped<IMovementRegisterDetailsService, MovementRegisterDetailsService>();
            builder.Services.AddScoped<IProjectModulesUserMappingService, ProjectModulesUserMappingService>();
            builder.Services.AddScoped<IProjectModuleEmployeeMappingService, ProjectModuleEmployeeMappingService>();
            builder.Services.AddScoped<IHelpDeskFilesService, HelpDeskFilesService>();
            builder.Services.AddScoped<IExpensePurchaseRequestFilesService, ExpensePurchaseRequestFilesService>();
            builder.Services.AddScoped<IExpenseAdvanceRequestFilesService, ExpenseAdvanceRequestFilesService>();
            builder.Services.AddScoped<IHelpDeskService, HelpDeskService>();
            builder.Services.AddScoped<IHelpDeskTopicService, HelpDeskTopicService>();
            builder.Services.AddScoped<IHelpDeskReminderLogService, HelpDeskReminderLogService>();
            builder.Services.AddScoped<IEmailRepliesServices, EmailRepliesServices>();
            builder.Services.AddScoped<IHelpDeskEmailRepliesMappingService, HelpDeskEmailRepliesMappingService>();
            builder.Services.AddScoped<ISitesEmailNotificationsServices, SitesEmailNotificationsServices>();
            builder.Services.AddScoped<ISitesEmailNotificationsPermissionServices, SitesEmailNotificationsPermissionServices>();
            builder.Services.AddScoped<IMessageTemplateService, MessageTemplateService>();
            builder.Services.AddScoped<IAzureBlobImageServices, AzureBlobImageServices>();
            builder.Services.AddScoped<ISitesItemsService, SitesItemsService>();
            builder.Services.AddScoped<ISitesItemsAttributesService, SitesItemsAttributesService>();
            builder.Services.AddScoped<ISOPTemplateService, SOPTemplateService>();
            builder.Services.AddScoped<ISOPTemplateSectionService, SOPTemplateSectionService>();
            builder.Services.AddScoped<ISOPTemplateSectionItemsService, SOPTemplateSectionItemsService>();
            builder.Services.AddScoped<ISOPAssignmentService, SOPAssignmentService>();
            builder.Services.AddScoped<ISOPAssignmentResponseService, SOPAssignmentResponseService>();
            builder.Services.AddScoped<ISOPAssignmentResponseEvidencesService, SOPAssignmentResponseEvidencesService>();
            builder.Services.AddScoped<ISOPProcessService, SOPProcessService>();
            builder.Services.AddScoped<ISOPProcessStatusLogService, SOPProcessStatusLogService>();
            builder.Services.AddScoped<ITimeZoneService, TimeZoneService>();
            builder.Services.AddScoped<IProjectActionItemsService, ProjectActionItemsService>();

            // Release Tracking
            builder.Services.AddScoped<IProjectReleaseTrackingService, ProjectReleaseTrackingService>();
            builder.Services.AddScoped<IProjectReleaseTrackingStatusLogService, ProjectReleaseTrackingStatusLogService>();
            builder.Services.AddScoped<IProjectReleaseTrackingReqPlanTaskIssueMappingService, ProjectReleaseTrackingReqPlanTaskIssueMappingService>();

            builder.Services.AddScoped<IProjectQuestionsAnswersService, ProjectQuestionsAnswersService>();
            builder.Services.AddScoped<IProjectQuestionsAnswersResponseLogService, ProjectQuestionsAnswersResponseLogService>();
            builder.Services.AddScoped<IProjectQuestionsAnswersContributorsService, ProjectQuestionsAnswersContributorsService>();

            builder.Services.AddScoped<ISitesProjectRolesService, SitesProjectRolesService>();

            builder.Services.ConfigureCors(builder.Configuration);
            builder.Services.ConfigureIISIntegration();
            builder.Services.AddAutoMapper(typeof(Program).Assembly);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Meldep 3.2 APIs",
                    Version = "v1"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter your token in the text input below.\r\n\r\nExample: \"1safsfsdfdfd\"",
                });
                // The web app sends the active site on every request (boot/axios.js), and the API reads it only
                // from these headers (GlobalVariableMiddleware). Declaring them here makes Swagger's Authorize
                // dialog send them too, so a Swagger call is checked exactly like the same call from the app.
                // GET /authorization/my-sites lists the site IDs a token can use.
                c.AddSecurityDefinition("SiteId", new OpenApiSecurityScheme()
                {
                    Name = "X-Site-Id",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Description = "Optional. The active site ID (a GUID, not the token). Leave empty to use the site the app opens at login (your last-used site). To work in another of your sites, enter its ID: GET /authorization/my-sites lists them."
                });
                c.AddSecurityDefinition("SiteTimezone", new OpenApiSecurityScheme()
                {
                    Name = "X-Site-Timezone",
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Description = "Optional. The active site's time zone, as the web app sends it (for example \"India Standard Time\"), so dates match the app."
                });
                OpenApiSecurityScheme SchemeReference(string id) => new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = id }
                };
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    { SchemeReference("Bearer"), new string[] {} },
                    { SchemeReference("SiteId"), new string[] {} },
                    { SchemeReference("SiteTimezone"), new string[] {} }
                });
            });
            // Add CORS services
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                builder =>
                {
                    builder.WithOrigins(
                        "https://meldep.com/",
                        "https://dev4-0.meldep.com",
                        "http://localhost:9000",
                        "https://meldepv3-2-reporting.vskyapplications.com",
                        "https://vskywebsite.vskyapplications.com",
                        "https://www.vskysolutions.com",
                        "https://vskysolutions.com",
                        "https://meldepwindows.vskyapplications.com",
                        "http://localhost:9300",
                        "https://n8nworkflow.vskyapplications.com",
                        "https://hrlens.vskyapplications.com")
                    .AllowAnyHeader()   // Allow any headers (for access to images)
                    .AllowAnyMethod()   // Allow any HTTP methods (GET, POST, etc.)
                    .AllowCredentials(); // Allow cookies and credentials if needed
                });
            });

            var app = builder.Build();

            // First, so it sees the final result of every request.
            app.UseMiddleware<ActivityLoggingMiddleware>();

            // configure the http request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseMiniProfiler();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    var path = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";
                    c.SwaggerEndpoint($"{path}/swagger/v1/swagger.json", "Meldep 3.2 v1");
                    c.DocExpansion(DocExpansion.None);
                });
            }
            else
                app.MapGet("/", () => "Meldep 3.2");

            // configure the http request pipeline
            if (app.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseResponseCompression();
            app.UseExceptionHandler(handler =>
            {
                handler.Run(async ctx =>
                {
                    var exception = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;

                    if (exception == null)
                    {
                        return;
                    }

                    // This writer never throws. The old logger could fail too (for example if the
                    // database is down), which crashed this handler and dropped the connection.
                    var logId = ctx.RequestServices.GetRequiredService<SecurityLog>().WriteError(null, exception);

                    // The client has gone, so there's no error to report and nobody to reply to.
                    if (ctx.RequestAborted.IsCancellationRequested)
                    {
                        return;
                    }

                    ctx.Response.ContentType = "application/json";
                    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    // Never send exception details to the client; they stay in the log under this id.
                    var error = new AuthError(StatusCodes.Status500InternalServerError, $"An error occurred while processing your request. Reference id is {logId}");
                    // The app's JSON options, so the client gets "message", which is the name the web app reads.
                    var jsonOptions = ctx.RequestServices.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>().Value.JsonSerializerOptions;
                    await System.Text.Json.JsonSerializer.SerializeAsync(ctx.Response.Body, error, jsonOptions);
                    await ctx.Response.Body.FlushAsync();
                });
            });

            if (blobUrlSigner != null)
            {
                blobUrlSigner.UseDataProtection(app.Services.GetRequiredService<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>());
                app.UseSignedLocalUploads(blobUrlSigner);
            }

            app.UseStaticFiles();
            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });
            app.UseCors("CorsPolicy");
            app.UseRouting();
            app.UseRateLimiter();

            HttpContextHelper.Accessor = app.Services.GetRequiredService<IHttpContextAccessor>(); // Change MT
            app.UseMiddleware<GlobalVariableMiddleware>(); // Change MT

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            
            app.Run();
        }

        public static bool IsUserAuthorized(HttpRequest request)
        {
            return request.HttpContext.User.Identity.IsAuthenticated;
        }

        public static bool ShouldProfile(HttpRequest request)
        {
            return request.HttpContext.User.Identity.IsAuthenticated;
        }
    }
}