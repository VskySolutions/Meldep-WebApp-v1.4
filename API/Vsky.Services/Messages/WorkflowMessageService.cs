using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Candidates;
using Vsky.Services.Contacts;
using Vsky.Services.EmailNotifications;
using Vsky.Services.EmailReply;
using Vsky.Services.Employees;
using Vsky.Services.HelpDesks;
using Vsky.Services.Persons;
using Vsky.Services.Sites;
using Vsky.Services.Users;

namespace Vsky.Services.Messages
{
    public class WorkflowMessageService : IWorkflowMessageService
    {
        #region Fields
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly ITokenizer _tokenizer;
        private readonly IPersonService _personService;
        private readonly ICandidateService _candidateService;
        private readonly IContactService _contactService;
        private readonly IEmailRepliesServices _emailRepliesServices;
        private readonly IHelpDeskEmailRepliesMappingService _helpDeskEmailRepliesMappingService;
        private readonly ISiteService _siteService;
        private readonly IHelpDeskReminderLogService _helpDeskReminderLogService;
        private readonly IEmployeeService _employeeService;
        private readonly ISitesEmailNotificationsServices _sitesEmailNotificationsServices;
        private readonly IUserService _userService;
        #endregion

        #region Ctor
        public WorkflowMessageService(
            ApplicationDbContext db, 
            IEmailSender emailSender, 
            IMessageTokenProvider messageTokenProvider,
            ITokenizer tokenizer,
            IPersonService personService, 
            ICandidateService candidateService,
            IContactService contactService,
            IEmailRepliesServices emailRepliesServices,
            IHelpDeskEmailRepliesMappingService helpDeskEmailRepliesMappingService,
            ISiteService siteService,
            IHelpDeskReminderLogService helpDeskReminderLogService,
            IEmployeeService employeeService,
            ISitesEmailNotificationsServices sitesEmailNotificationsServices,
            IUserService userService
        )
        {
            _db = db;
            _emailSender = emailSender;
            _messageTokenProvider = messageTokenProvider;
            _tokenizer = tokenizer;
            _personService = personService;
            _candidateService = candidateService;
            _contactService = contactService;
            _emailRepliesServices = emailRepliesServices;
            _helpDeskEmailRepliesMappingService = helpDeskEmailRepliesMappingService;
            _siteService = siteService;
            _helpDeskReminderLogService = helpDeskReminderLogService;
            _employeeService = employeeService;
            _sitesEmailNotificationsServices = sitesEmailNotificationsServices;
            _userService = userService;
        }
        #endregion

        #region Methods
        public async Task SendWelcomeEmail(ApplicationUser user, string password)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.UserWelcomeMessage && x.Active);

            if (template != null)
            {
                // tokens
                var tokens = new List<Token>();

                _messageTokenProvider.AddUserTokens(tokens, user);

                var person = await _personService.GetPersonById(user.PersonId);

                // add password to the token list
                tokens.Add(new Token("User.Password", password));
                tokens.Add(new Token("User.FirstName", person.FirstName));
                tokens.Add(new Token("User.LastName", person.LastName));

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId) ?? await _db.EmailAccounts.FirstOrDefaultAsync();
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(template.Body, tokens, true);

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, user.Email, null);
            }
        }
        public async Task SendResetPasswordEmail(ApplicationUser user, string password)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.UserResetPassword && x.Active);

            if (template != null)
            {
                // tokens
                var tokens = new List<Token>();

                _messageTokenProvider.AddUserTokens(tokens, user);

                var person = await _personService.GetPersonById(user.PersonId);
                // add password to the token list
                tokens.Add(new Token("User.Password", password));
                tokens.Add(new Token("User.FirstName", person.FirstName));
                tokens.Add(new Token("User.LastName", person.LastName));

                //var AppUrl = "";
                var WEB_Domain = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["WEB_Domain"];
                string AppUrl = WEB_Domain;
                //template.Body += "<br><br><p style='align:center'><a href='" + AppUrl + "' target='_blank'>Click here to Login</a></p>";
                var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";
                var Body = "<div style='text-align: center; margin-bottom: 30px;'><p style='color:black;text-align: left; padding: 30px; margin: 0 auto; font-size: 17px;'>"
                    + _tokenizer.Replace(template.Body, tokens, true)
                    + " <br /></p><p><a href='" + AppUrl + "' style='text-decoration: none; background-color: #378bbd; border: 1px solid #000; font-size: 15px; padding: 10px 20px; border-radius: 5px; color: white;'>Click here to Login</a></p></div>";
                var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                    "<h3 style='margin: 0px; color: white; font-size: 25px;'>" + "Meldep" + "</h3>" +
                    "<div style='padding: 10px 0;'>" +
                    "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                    "<a href='mailto:" + "info@vskysolutions.com" + "' style='color: #fff; text-decoration: none;'>" + "info@vskysolutions.com" + " </a>" +
                    "</div>" +
                    "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                    "<a href='tel:" + "+1 904-914-8759" + "' style='color: #fff; text-decoration: none;'>" + "+1 904-914-8759" + "</a>" +
                    "</div>" +
                    "</div>" +
                    "<div style='padding: 0 10px 10px;'><p style='margin: 0px; color: white; font-size: 15px;'>" + "9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States" + "</p></div>" +
                    "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>Copyright ©2024 " + "Vsky Solutions" +
                    ". Application Designed and Developed by <a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                    "</div>";

                // Combine Header, Body, Footer
                var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId) ?? await _db.EmailAccounts.FirstOrDefaultAsync();
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(FullEmail, tokens, true);

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, user.Email, null);
            }
        }
        public async Task SendTwoFactorToken(ApplicationUser user, string code)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.UserTwoFactorToken && x.Active);

            if (template != null)
            {
                // tokens
                var tokens = new List<Token>();

                _messageTokenProvider.AddUserTokens(tokens, user);

                // add code to the token list
                tokens.Add(new Token("User.TwoFactorToken", code));

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId) ?? await _db.EmailAccounts.FirstOrDefaultAsync();
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(template.Body, tokens, true);

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, user.Email, null);
            }
        }
        public async Task SendMailToHr(Employee employee, EmployeeLeave code)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.LeaveSendToHRToken && x.Active);

            if (template != null)
            {
                // tokens
                var tokens = new List<Token>();

                //_messageTokenProvider.AddEmployeeTokens(tokens, employee);

                // add code to the token list
                tokens.Add(new Token("Leave.SendToHR", code));
                tokens.Add(new Token("Leave.HrFirstName", employee.Person.FirstName));
                tokens.Add(new Token("Leave.HrLastName", employee.Person.LastName));
                tokens.Add(new Token("Leave.FirstName", code.Employee.Person.FirstName));
                tokens.Add(new Token("Leave.LastName", code.Employee.Person.LastName));
                tokens.Add(new Token("Leave.FromDate", code.FromDate.ToString("MM-dd-yyyy")));
                tokens.Add(new Token("Leave.ToDate", code.ToDate.ToString("MM-dd-yyyy")));
                tokens.Add(new Token("Leave.Reason", code.Reason));
                tokens.Add(new Token("Leave.LeaveType", code.LeaveCategories.DropDownValue));
                tokens.Add(new Token("Leave.NoofLeaves", code.NoofLeaves));

                var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";
                var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                template.Body + " <br /></p></div>";
                var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                             "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                             "<div style='padding: 10px 0;'>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                             "</div>" +
                             "<div style='padding: 0 10px 10px;'>" +
                             "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                             "</div>" +
                             "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                             "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                             "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                             "</div>";

                var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == employee.SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == employee.SiteId);
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(FullEmail, tokens, true);

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, employee.Person.PrimaryEmailAddress, null);
            }
        }
        public async Task SendMailToApprover(Employee employee, EmployeeLeave code)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.LeaveForwardToken && x.Active);

            if (template != null)
            {
                // tokens
                var tokens = new List<Token>();

                // add code to the token list
                tokens.Add(new Token("Leave.Forward", code));
                tokens.Add(new Token("Leave.FirstName", code.Employee.Person.FirstName));
                tokens.Add(new Token("Leave.LastName", code.Employee.Person.LastName));
                tokens.Add(new Token("Leave.FromDate", code.FromDate.ToString("MM-dd-yyyy")));
                tokens.Add(new Token("Leave.ToDate", code.ToDate.ToString("MM-dd-yyyy")));
                tokens.Add(new Token("Leave.Reason", code.Reason));
                tokens.Add(new Token("Leave.HRNote", code.HRNote));
                tokens.Add(new Token("Leave.NoofLeaves", code.NoofLeaves));

                //var AppUrl = "";
                var WEB_Domain = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["WEB_Domain"];
                string AppUrl = WEB_Domain + "/approve-leaves?id=" + code.Id;

                tokens.Add(new Token("Leave.URL", AppUrl));

                var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";
                var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                template.Body + " <br /></p></div>";
                var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                             "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                             "<div style='padding: 10px 0;'>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                             "</div>" +
                             "<div style='padding: 0 10px 10px;'>" +
                             "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                             "</div>" +
                             "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                             "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                             "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                             "</div>";

                var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == employee.SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == employee.SiteId);
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(FullEmail, tokens, true);

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, employee.Person.PrimaryEmailAddress, null);
            }
        }
        public async Task SendLeaveStatusEmailToEmployee(Employee employee, EmployeeLeave code)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync();
            if (code.LeaveStatuses.DropDownValue == "Approved")
                template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.LeaveApprovalSendToEmployeeToken && x.Active);
            else if (code.LeaveStatuses.DropDownValue == "Decline")
                template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.LeaveStatusSendToEmployeeToken && x.Active);
            else if ((code.LeaveStatuses.DropDownValue == "Cancelled"))
                template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.SendLeaveCancellationToEmployeeToken && x.Active);


            if (template != null)
            {
                // tokens
                var tokens = new List<Token>();

                // add code to the token list
                tokens.Add(new Token("Leave.SendLeaveApprovalToEmployee", code));
                tokens.Add(new Token("Leave.SendLeaveStatusToEmployee", code));
                tokens.Add(new Token("Leave.SendLeaveCancellationToEmployee", code));
                tokens.Add(new Token("Leave.ApproverFirstName", code.LeaveApprover.Person.FirstName));
                tokens.Add(new Token("Leave.ApproverLastName", code.LeaveApprover.Person.LastName));
                tokens.Add(new Token("Leave.FirstName", employee.Person.FirstName));
                tokens.Add(new Token("Leave.LastName", employee.Person.LastName));
                tokens.Add(new Token("Leave.FromDate", code.FromDate.ToString("MM-dd-yyyy")));
                tokens.Add(new Token("Leave.ToDate", code.ToDate.ToString("MM-dd-yyyy")));

                var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";
                var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                template.Body + " <br /></p></div>";
                var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                             "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                             "<div style='padding: 10px 0;'>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                             "</div>" +
                             "<div style='padding: 0 10px 10px;'>" +
                             "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                             "</div>" +
                             "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                             "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                             "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                             "</div>";

                var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == employee.SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == employee.SiteId);
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(FullEmail, tokens, true);

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, employee.Person.PrimaryEmailAddress, null);
            }
        }
        public async Task SendPasswordResetEmail(ApplicationUser user, string code)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.UserResetPassword && x.Active);
            if (template != null)
            {
                // Tokens
                var tokens = new List<Token>();

                // Add user information to tokens
                var person = await _personService.GetPersonById(user.PersonId);
                tokens.Add(new Token("User.FirstName", person.FirstName));

                // URL Link
                var WEB_Domain = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["WEB_Domain"];
                string AppUrl = WEB_Domain + "/auth/reset-password/" + user.Id;

                // Header, Body, Footer
                var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";
                var Body = "<div style='text-align: center; margin-bottom: 30px;'><p style='color:black;text-align: left; padding: 30px; margin: 0 auto; font-size: 17px;'>"
                    + template.Body
                    + " <br /></p><p><a href='" + AppUrl + "' style='text-decoration: none; background-color: #378bbd; border: 1px solid #000; font-size: 15px; padding: 10px 20px; border-radius: 5px; color: white;'>Reset Password</a></p></div>";
                var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                    "<h3 style='margin: 0px; color: white; font-size: 25px;'>" + "Meldep" + "</h3>" +
                    "<div style='padding: 10px 0;'>" +
                    "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                    "<a href='mailto:" + "info@vskysolutions.com" + "' style='color: #fff; text-decoration: none;'>" + "info@vskysolutions.com" + " </a>" +
                    "</div>" +
                    "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                    "<a href='tel:" + "+1 904-914-8759" + "' style='color: #fff; text-decoration: none;'>" + "+1 904-914-8759" + "</a>" +
                    "</div>" +
                    "</div>" +
                    "<div style='padding: 0 10px 10px;'><p style='margin: 0px; color: white; font-size: 15px;'>" + "9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States" + "</p></div>" +
                    "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>Copyright ©2024 " + "Vsky Solutions" +
                    ". Application Designed and Developed by <a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                    "</div>";

                // Full Email
                var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                // Replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(FullEmail, tokens, true);

                // Email Account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId) ?? await _db.EmailAccounts.FirstOrDefaultAsync();
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // Send Email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, user.Email, null);
            }
        }
        #endregion

        #region Help Desk
        public async Task SendRequestReceivedMailToRequester(Employee employee, HelpDesk code, string twilioEmailId, string requesterEmailId, bool isEmailSent, string prefixNo, string requesterName, DateTime GetDateTime)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.RequestReceivedToken && x.Active);
            string prefixTicketNo = $"#{prefixNo}-{code.TicketNo}";

            if (template != null)
            {
                // tokens
                var tokens = new List<Token>();

                string requesterFullName = !string.IsNullOrEmpty(requesterName) ? requesterName : requesterEmailId;
                var complainerFirstName = employee?.Person?.FirstName ?? requesterFullName;
                var complainerLastName = employee?.Person?.LastName ?? "";

                // add code to the token list
                tokens.Add(new Token("HelpDesk.RequestReceived", code));
                tokens.Add(new Token("HelpDesk.ComplainerFirstName", complainerFirstName));
                tokens.Add(new Token("HelpDesk.ComplainerLastName", complainerLastName));
                tokens.Add(new Token("HelpDesk.TicketNo", prefixTicketNo));
                tokens.Add(new Token("HelpDesk.Title", code.Title));

                //var AppUrl = "";
                var WEB_Domain = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["WEB_Domain"];
                string AppUrl = WEB_Domain + "/account/help-desk";

                tokens.Add(new Token("HelpDesk.URL", AppUrl));
              
                // Build Email Layout
                var FullEmail = BuildEmailLayout(template.Body, WEB_Domain, true);

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == code.SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == code.SiteId);
                if(emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(FullEmail, tokens, true);

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, requesterEmailId, null);

                // Save email repy
                if (isEmailSent)
                {
                    InsertSystemEmailReplyAsync(requesterEmailId, emailAccount.Email, subject, body, employee?.Id, twilioEmailId, code.Id, GetDateTime);
                }
            }
        }
        public async Task SendHelpDeskMailToAdmin(Employee employee, HelpDesk code, bool isEmailSent, string twilioEmailId, string prefixNo, List<string> emailAddresses, string requesterName, string requesterEmailId, DateTime GetDateTime)
        {
            try
            {
                var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.HelpDeskMailSendToAdminToken && x.Active);
                string prefixTicketNo = $"#{prefixNo}-{code.TicketNo}";
                string requesterFullName = !string.IsNullOrEmpty(requesterName) ? requesterName : requesterEmailId;
                string complainerName = !string.IsNullOrEmpty(requesterFullName) ? requesterFullName : code.Employee.Person.FullName;

                string toEmailCsv = PrepareRecipientsCsv(employee, emailAddresses);
                if (string.IsNullOrEmpty(toEmailCsv))
                    return;

                if (template != null)
                {
                    // tokens
                    var tokens = new List<Token>();
                    var (safeHtml, isHtml) = SanitizeDescription(code.Description);

                    // add code to the token list
                    tokens.Add(new Token("HelpDesk.HelpDeskMailSendToAdmin", code));
                    tokens.Add(new Token("HelpDesk.AdministratorFirstName", ""));
                    tokens.Add(new Token("HelpDesk.AdministratorLastName", ""));
                    tokens.Add(new Token("HelpDesk.ComplainerName", complainerName));
                    tokens.Add(new Token("HelpDesk.Description", safeHtml, isHtml));
                    tokens.Add(new Token("HelpDesk.Title", code.Title));
                    tokens.Add(new Token("HelpDesk.TicketNo", prefixTicketNo));
                    tokens.Add(new Token("HelpDesk.Date", code.CreatedOnUtc));

                    //var AppUrl = "";
                    var WEB_Domain = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["WEB_Domain"];
                    string AppUrl = WEB_Domain + "/account/help-desk";
                    tokens.Add(new Token("HelpDesk.URL", AppUrl));
                    
                    // Build Email Layout
                    var FullEmail = BuildEmailLayout(template.Body, WEB_Domain, true);

                    // email account
                    var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == code.SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == code.SiteId);
                    if (emailAccount == null)
                    {
                        throw new NullReferenceException("Email account not found");
                    }

                    // replace subject and body tokens
                    var subject = _tokenizer.Replace(template.Subject, tokens, false);
                    var body = _tokenizer.Replace(FullEmail, tokens, true);

                    // send email
                    await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, toEmailCsv, null);

                    // Save email repy
                    if (isEmailSent)
                    {
                        InsertSystemEmailReplyAsync(toEmailCsv, emailAccount.Email, subject, body, null, twilioEmailId, code.Id, GetDateTime);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        public async Task SendHelpDeskReplyMail(Employee employee, HelpDesk code, EmailReplies reply, string twilioEmailId, List<string> toEmailList, List<string> actualToEmails, List<string> ccEmailList, string prefixNo, DateTime GetDateTime)
        {
            try
            {
                var toEmailCsv = PrepareRecipientsCsv(null, toEmailList);
                if (string.IsNullOrEmpty(toEmailCsv))
                    return;

                // only actual/system emails for DB save
                var actualToEmailCsv = PrepareRecipientsCsv(null, actualToEmails);

                // CC CSV
                var ccEmailCsv = ccEmailList != null && ccEmailList.Any()
                    ? string.Join(",",
                        ccEmailList
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Distinct(StringComparer.OrdinalIgnoreCase))
                    : "";

                var subject = $"#{prefixNo}-{code.TicketNo} {reply.Subject} - New Reply";

                var WEB_Domain = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["WEB_Domain"];

                string AppUrl = WEB_Domain + "/account/help-desk";

                var redirectionUrl = WEB_Domain == "https://www.meldep.com" ? "live4-0.meldep.com" : "dev4-0.meldep.com";

                // Build Email Layout
                var systemNote = "<div style='margin:30px 0; padding:12px; border-top:1px solid #ddd; border-bottom:1px solid #ddd;" +
                                    "background-color:#f9f9f9; text-align:center; font-size:14px; color:#555;'>" +
                                    "<strong>Note:</strong> You may reply directly to this email. Your response will be automatically recorded under your ticket in the system." +
                                    "</div>";

                var FullEmail = "<div>" + reply.Body + systemNote + "</div>";

                // Send Twilio Email
                string EmailStatus = await _emailSender.TwilioSendEmailsAsync(subject, FullEmail, toEmailCsv, "", redirectionUrl, null, null, null, ccEmailList, null, null, null, twilioEmailId, code.SiteId);

                InsertSystemEmailReplyAsync(actualToEmailCsv, reply.FromEmail, reply.Subject, reply.Body, reply.OwnerId, twilioEmailId, code.Id, GetDateTime, false, reply.ExternalToEmail, ccEmailCsv);
            }
            catch (Exception)
            {
            }
        }
        public async Task SendHelpDeskStatusMail(Employee employee, HelpDesk code, List<string> emailAddresses, string twilioEmailId, string prefixNo, string siteName, string requesterEmail, string assignedToName, DateTime GetDateTime)
        {
            try
            {
                var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.HelpDeskStatusMail && x.Active);
                string prefixTicketNo = $"#{prefixNo}-{code.TicketNo}";
                string statusName = !string.IsNullOrEmpty(assignedToName) ? $"{code.StatusText} ({assignedToName})" : code.StatusText;

                #region Prepare TO emails           

                string toEmailCsv = PrepareRecipientsCsv(employee, emailAddresses, requesterEmail);

                if (string.IsNullOrEmpty(toEmailCsv))
                    return;

                #endregion

                if (template != null)
                {
                    // tokens
                    var tokens = new List<Token>();

                    // add code to the token list
                    tokens.Add(new Token("HelpDesk.HelpDeskStatusMail", code));
                    tokens.Add(new Token("HelpDesk.TicketNo", prefixTicketNo)); // Ticket no
                    tokens.Add(new Token("HelpDesk.StatusName", statusName)); // changed status name with assignedTo person Name
                    tokens.Add(new Token("HelpDesk.TicketName", code.Title)); // title
                    tokens.Add(new Token("HelpDesk.Priority", code.Priority.DropDownValue)); // priority name
                    tokens.Add(new Token("HelpDesk.Category", code.Category.DropDownValue)); // category name
                    tokens.Add(new Token("HelpDesk.FromPersonName", siteName)); // person name for status change 
                    tokens.Add(new Token("HelpDesk.CreatedBy", code.CreatedBy.Person.FullName)); // Craeted By
                    tokens.Add(new Token("HelpDesk.CreatedDate", code.CreatedOnUtc)); // Craeted date

                    //var AppUrl = "";
                    var WEB_Domain = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["WEB_Domain"];
                    string AppUrl = WEB_Domain + "/account/help-desk";

                    tokens.Add(new Token("HelpDesk.URL", AppUrl));
                  
                    // Build Email Layout
                    var FullEmail = BuildEmailLayout(template.Body, WEB_Domain, true);

                    // email account
                    var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == code.SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == code.SiteId);
                    if (emailAccount == null)
                    {
                        throw new NullReferenceException("Email account not found");
                    }

                    // replace subject and body tokens
                    var subject = _tokenizer.Replace(template.Subject, tokens, false);
                    var body = _tokenizer.Replace(FullEmail, tokens, true);

                    await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, toEmailCsv, null);

                    // Save email repy
                    InsertSystemEmailReplyAsync(toEmailCsv, emailAccount.Email, subject, body, null, twilioEmailId, code.Id, GetDateTime);
                }
            }
            catch (Exception)
            {
            }
        }
        public async Task SendHelpDeskAssignedToMail(Employee employee, HelpDesk code, string assignedToName, string twilioEmailId, string prefixNo, string siteName, string requesterEmail, DateTime GetDateTime)
        {
            try
            {
                var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.HelpDeskAssignedToMail && x.Active);
                var empEmail = employee?.Person?.PrimaryEmailAddress ?? requesterEmail ?? string.Empty;
                string prefixTicketNo = $"#{prefixNo}-{code.TicketNo}";

                if (template != null)
                {
                    // tokens
                    var tokens = new List<Token>();

                    // add code to the token list
                    tokens.Add(new Token("HelpDesk.HelpDeskAssignedToMail", code));
                    tokens.Add(new Token("HelpDesk.TicketName", code.Title)); // ticket name
                    tokens.Add(new Token("HelpDesk.AssignedToName", assignedToName)); // Assigned to person name
                    tokens.Add(new Token("HelpDesk.TicketId", prefixTicketNo)); // Ticket Id
                    tokens.Add(new Token("HelpDesk.Priority", code.Priority.DropDownValue)); // Priority
                    tokens.Add(new Token("HelpDesk.CreatedBy", code.CreatedBy.Person.FullName)); // Craeted By
                    tokens.Add(new Token("HelpDesk.CreatedDate", code.CreatedOnUtc)); // Craeted date
                    tokens.Add(new Token("HelpDesk.FromPersonName", siteName)); // person name for Assigned to

                    //var AppUrl = "";
                    var WEB_Domain = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["WEB_Domain"];
                    string AppUrl = WEB_Domain + "/account/help-desk";

                    tokens.Add(new Token("HelpDesk.URL", AppUrl));
                  
                    // Build Email Layout
                    var FullEmail = BuildEmailLayout(template.Body, WEB_Domain, true);

                    // email account
                    var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == code.SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == code.SiteId);
                    if (emailAccount == null)
                    {
                        throw new NullReferenceException("Email account not found");
                    }

                    // replace subject and body tokens
                    var subject = _tokenizer.Replace(template.Subject, tokens, false);
                    var body = _tokenizer.Replace(FullEmail, tokens, true);

                    // send email
                    await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, empEmail, null);

                    // Save email repy
                    InsertSystemEmailReplyAsync(empEmail, emailAccount.Email, subject, body, employee?.Id, twilioEmailId, code.Id, GetDateTime);
                }
            }
            catch (Exception)
            {
            }
        }       
        public async Task SendHelpDeskReminderMail(HelpDesk helpdesk, string title, string templateName, string prefixNo, DateTime GetDateTime)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == templateName && x.Active);

            if (template != null)
            {
                var SiteEmailNotification = await _sitesEmailNotificationsServices.GetSiteEmailNotificationByMessageTemplateId(template.Id);
                string prefixTicketNo = $"#{prefixNo}-{helpdesk.TicketNo}";
                var createdByFullName = helpdesk.CreatedBy?.Person?.FirstName + " " + helpdesk.CreatedBy?.Person?.LastName;

                // tokens
                var tokens = new List<Token>();

                // add code to the token list
                tokens.Add(new Token("HelpDesk.TicketName", helpdesk.Title)); // ticket name
                tokens.Add(new Token("HelpDesk.TicketNo", prefixTicketNo)); // Ticket Id
                tokens.Add(new Token("HelpDesk.Priority", helpdesk.Priority.DropDownValue)); // Priority
                tokens.Add(new Token("HelpDesk.CreatedBy", createdByFullName)); // Craeted By
                tokens.Add(new Token("HelpDesk.CreatedDate", helpdesk.CreatedOnUtc)); // Craeted date
                tokens.Add(new Token("HelpDesk.ReminderTitle", title));

                // Build Email Layout
                var FullEmail = BuildEmailLayout(template.Body, null, true);

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == helpdesk.SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == helpdesk.SiteId);
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(FullEmail, tokens, true);

                // Email receiver logic
                List<string> toEmails = new List<string>();
                string receiverType = "";

                // ================= Assigned User =================
                string assignedUserEmail = null;

                if (!string.IsNullOrEmpty(helpdesk.AssignedToId))
                {
                    var assignedUser = await _employeeService.GetEmployeeDetailsById(helpdesk.AssignedToId);
                    if (assignedUser != null && !string.IsNullOrEmpty(assignedUser.OfficialEmail))
                        assignedUserEmail = assignedUser?.OfficialEmail;
                }

                // ================= Admin =================
                var admins = await _userService.GetUsersByRole(helpdesk.SiteId, "Admin");
                var adminEmails = admins?
                       .Where(x => !string.IsNullOrEmpty(x.Email))
                       .Select(x => x.Email)
                       .ToList() ?? new List<string>();

                // ================= Receiver filter based on title =================
                if (templateName == MessageTemplateSystemNames.TicketUnassignedHours)
                {
                    // only admins
                    toEmails.AddRange(adminEmails);
                    receiverType = "Admin";
                }
                else if (templateName == MessageTemplateSystemNames.MoveAssignedToInProgress
                   || templateName == MessageTemplateSystemNames.CompletedToClosed
                   || templateName == MessageTemplateSystemNames.PendingFirstEmailReply
                )
                {
                    // Assigned user + admin
                    if (!string.IsNullOrEmpty(assignedUserEmail))
                        toEmails.Add(assignedUserEmail);

                    toEmails.AddRange(adminEmails);
                    receiverType = "Support Team/Admin";
                }
                else if (templateName == MessageTemplateSystemNames.InProgressNoActivity)
                {
                    // Assigned user only
                    if (!string.IsNullOrEmpty(assignedUserEmail))
                        toEmails.Add(assignedUserEmail);

                    receiverType = "Support Team";
                }
                else
                {
                    receiverType = "Requester";
                }

                var toEmailString = string.Join(",", toEmails.Distinct());

                if (!string.IsNullOrEmpty(toEmailString))
                {
                    await _emailSender.SendEmailAsync(
                        emailAccount,
                        subject,
                        body,
                        emailAccount.Email,
                        emailAccount.DisplayName,
                        toEmailString,
                        null);
                }

                var log = new HelpDeskReminderLog
                {
                    Id = Guid.NewGuid().ToString(),
                    HelpDeskId = helpdesk.Id,
                    SiteEmailNotificationId = SiteEmailNotification.Id,
                    Date = GetDateTime,
                    ToEmail = toEmailString,
                    IsRequesterOrSupport = receiverType,
                    CreatedOnUtc = GetDateTime,
                    Deleted = false
                };

                _helpDeskReminderLogService.InsertHelpDeskReminderLogs(log);
            }
        }
        #endregion

        #region send contact email form vsky website application and meldep
        public async Task SendContactUserEmail(Contact contact)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.UserWelcomeMessage && x.Active);
            if (template != null)
            {
                var title = !string.IsNullOrEmpty(contact.Title) ? contact.Title : "";

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId) ?? await _db.EmailAccounts.FirstOrDefaultAsync();
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = "New Contact Form Submission from " + contact.FullName + "";
                var body = "Hello Vsky Team, <br /><br />A new inquiry has been submitted through the contact us form. <br />Here are the details:<br />" +
                           "Name : " + contact.FullName + "<br />" +
                           "Email : " + contact.Email + "<br />" +
                           "Subject : " + title + "<br />" +
                           "Message : " + contact.Message + "<br /><br />" +
                           "Please review and respond as needed.<br /><br />" +
                           "Thanks and Regards,<br />" +
                           "Vsky Solutions Team <br />" +
                           "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a>";

                var fullEmailBody = BuildEmailLayout(body, null, true);

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, fullEmailBody, emailAccount.Email, emailAccount.DisplayName, "info@vskysolutions.com", null);
            }
        }
        public async Task SendContactCandidateEmail(Contact contact)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.UserWelcomeMessage && x.Active);
            if (template != null)
            {
                var candidate = await _contactService.GetContactById(contact.Id);

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId) ?? await _db.EmailAccounts.FirstOrDefaultAsync();
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = "Thank You for Contacting Vsky Solutions!";
                var body = "Dear " + contact.FullName + ",<br /><br />Thank you for reaching out to Vsky Solutions. We have received your inquiry and will get back to you as soon as possible. <br />Here are the details you provided:<br />" +
                           "Name : " + contact.FullName + "<br />" +
                           "Email : " + contact.Email + "<br />" +
                           "Message : " + contact.Message + "<br /><br />" +
                           "Our team will review your request and respond promptly.<br /><br />" +
                           "Thanks and Regards,<br />" +
                           "Vsky Solutions Team <br />" +
                            "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a>" +
                           "<a href='https://vskywebsite.vskyapplications.com/' style='color: #fff; text-decoration: none;'>https://vskywebsite.vskyapplications.com/</a>";

                var fullEmailBody = BuildEmailLayout(body, null, true);

                await _emailSender.SendEmailAsync(emailAccount, subject, fullEmailBody, emailAccount.Email, emailAccount.DisplayName, contact.Email, null);
            }
        }
        #endregion

        #region send career form email form vsky website application
        public async Task SendRegisterUserEmail(string Id)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.UserWelcomeMessage && x.Active);
            if (template != null)
            {
                var candidateData = await _candidateService.GetCandidateDetailsById(Id);

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == candidateData.SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == candidateData.SiteId);
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = "New Candidate Application - " + candidateData.Person.FullName + " for " + candidateData.Job.JobTitle + "";
                var body = "Hello HR Team,<br /><br />" +
                           "We have received a new application from " + candidateData.Person.FullName + " for the role of " + candidateData.Job.JobTitle + " position at Vsky Solutions." +
                           " <br />Below are the details of the candidate:<br />" +
                           "Full Name : " + candidateData.Person.FullName + "<br />" +
                           "Email Address : " + candidateData.Person.PrimaryEmailAddress + "<br />" +
                           "Phone Number : " + candidateData.Person.PrimaryPhoneNumber + "<br />" +
                           "LinkedIn Profile : " + candidateData.LinkedInProfile + "<br />" +
                           "Location Preference : " + candidateData.AppliedWorkLocations.DropDownValue + "<br />" +
                           "Please review the application and take necessary action. Let us know if further details are required.<br /><br />" +
                           "Thanks and Regards,<br />" +
                           "Vsky Solutions <br />" +
                            "<a href='mailto:hr@vskysolutions.com' style='color: #fff; text-decoration: none;'>hr@vskysolutions.com</a>" +
                           "<a href='https://vskywebsite.vskyapplications.com/' style='color: #fff; text-decoration: none;'>https://vskywebsite.vskyapplications.com/</a>";

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, "hr@vskysolutions.com", null);
            }
        }
        public async Task SendRegisterCandidateEmail(string Id)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.UserWelcomeMessage && x.Active);
            if (template != null)
            {
                var candidateData = await _candidateService.GetCandidateDetailsById(Id);

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == candidateData.SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == candidateData.SiteId);
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = "Your Application at VSky Solutions - Confirmation";

                var body = "Dear " + candidateData.Person.FullName + ",<br /><br />" +
                          "Thank you for applying to VSKY Solutions. <br />" +
                          "We would like to inform you that we have successfully received your resume and it is currently under review by our team. <br />" +
                          "If your profile matches our requirements, our team will contact you within 3-5 working days regarding the next steps in the selection process. <br />" +
                          "In case you do not hear from us within this timeframe, we will retain your resume in our database and reach out if a suitable opportunity arises in the future. <br />" +
                          "We appreciate your interest in joining our organization. <br />" +
                          "Best regards,<br />" +
                          "Vsky Solutions Team <br />" +
                           "<a href='mailto:hr@vskysolutions.com' style='color: #fff; text-decoration: none;'>hr@vskysolutions.com</a>" +
                          "<a href='https://vskywebsite.vskyapplications.com/' style='color: #fff; text-decoration: none;'>https://vskywebsite.vskyapplications.com/</a>";

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, candidateData.Person.PrimaryEmailAddress, null);
            }
        }
        #endregion

        public async Task SendWebsiteDemoMailToUser(Website_Demos website_Demos)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.WebsiteDemoMailToUserToken && x.Active);

            if (template != null)
            {
                // tokens
                var tokens = new List<Token>();
                tokens.Add(new Token("WebsiteDemo.WebsiteDemoMailToUser", website_Demos));
                tokens.Add(new Token("WebsiteDemo.FullName", website_Demos.FullName));

                var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";

                var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                template.Body +
                            " <br /></p></div>";

                var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                             "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                             "<div style='padding: 10px 0;'>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                             "</div>" +
                             "<div style='padding: 0 10px 10px;'>" +
                             "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                             "</div>" +
                             "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                             "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                             "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                             "</div>";

                var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId) ?? await _db.EmailAccounts.FirstOrDefaultAsync();
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(FullEmail, tokens, true);

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, website_Demos.EmailAddress, null);
            }
        }
        public async Task SendDemoRequestSubmittedMailToVsky(Website_Demos website_Demos)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.WebsiteDemoRequestSubmittedMailToken && x.Active);

            if (template != null)
            {
                // tokens
                var tokens = new List<Token>();
                tokens.Add(new Token("WebsiteDemo.WebsiteDemoRequestSubmitted", website_Demos));
                tokens.Add(new Token("WebsiteDemo.FullName", website_Demos.FullName));
                tokens.Add(new Token("WebsiteDemo.EmailAddress", website_Demos.EmailAddress));
                tokens.Add(new Token("WebsiteDemo.CompanyName", website_Demos.CompanyName));
                tokens.Add(new Token("WebsiteDemo.BusinessSize", website_Demos.BusinessSize.DropDownValue));
                string moduleList = "";

                foreach (var item in website_Demos.Website_Demo_Modules)
                {
                    if (item.Modules != null && !string.IsNullOrWhiteSpace(item.Modules.Name))
                    {
                        moduleList += "• " + item.Modules.Name + " ";
                    }
                }

                tokens.Add(new Token("WebsiteDemo.Modules", moduleList));


                var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";

                var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                template.Body +
                            " <br /></p></div>";


                var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                             "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                             "<div style='padding: 10px 0;'>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                             "</div>" +
                             "<div style='padding: 0 10px 10px;'>" +
                             "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                             "</div>" +
                             "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                             "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                             "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                             "</div>";

                var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId) ?? await _db.EmailAccounts.FirstOrDefaultAsync();
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(FullEmail, tokens, true);

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, "info@vskysolutions.com", null);
            }
        }
        public async Task SendExpenseMailToApprovers(ApplicationUser User, decimal totalAmount, string redirectUrl, string role, string status, string preApproverNote, string SiteId)
        {
            string messageToken = "";

            if (!string.IsNullOrEmpty(role))
            {
                messageToken = role == "Finance" && status == "Submitted" ? MessageTemplateSystemNames.ExpensePreApproveRequestToken
                               : role == "Finance-Preapprove" && status == "Pre-Approved" ? MessageTemplateSystemNames.ExpenseApproveRequestToken
                               : role == "Finance-Approver" && status == "Approved" ? MessageTemplateSystemNames.ExpensePaymentRequestToken
                               : "";
            }
            if (!string.IsNullOrEmpty(messageToken))
            {
                var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == messageToken && x.Active);

                if (template != null)
                {
                    // tokens
                    var tokens = new List<Token>();
                    tokens.Add(new Token("Expense.FirstName", User.Person.FirstName + " " + User.Person.LastName));
                    tokens.Add(new Token("Expense.TotalAmount", totalAmount));
                    if (!string.IsNullOrEmpty(preApproverNote))
                        tokens.Add(new Token("Expense.Note", preApproverNote));
                    else
                        tokens.Add(new Token("Expense.Note", "--"));

                    var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";

                    var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                    template.Body +
                                "<br><br> <a href=" + redirectUrl + " target='_blank'>Click Here</a> <br /></p></div>";


                    var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                                 "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                                 "<div style='padding: 10px 0;'>" +
                                 "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                                 "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                                 "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                                 "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                                 "</div>" +
                                 "<div style='padding: 0 10px 10px;'>" +
                                 "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                                 "</div>" +
                                 "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                                 "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                                 "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                                 "</div>";

                    var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                    // email account
                    var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == SiteId);
                    if (emailAccount == null)
                    {
                        throw new NullReferenceException("Email account not found");
                    }

                    // replace subject and body tokens
                    var subject = _tokenizer.Replace(template.Subject, tokens, false);
                    var body = _tokenizer.Replace(FullEmail, tokens, true);

                    await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, User.Email, null);
                }
            }
        }
        public async Task SendExpenseMailToUsers(ApplicationUser User, decimal totalAmount, string redirectUrl, string role, string status, string postApproverNote, string paidByNote, string SiteId)
        {
            string messageToken = "";
            // Get full host name, e.g., sub.example.com
            if (!string.IsNullOrEmpty(role))
            {
                messageToken = role == "Finance-Preapprove" && status == "Pre-Approved" ? MessageTemplateSystemNames.ExpensePreApprovedToken
                               : role == "Finance-Approver" && status == "Approved" ? MessageTemplateSystemNames.ExpenseApprovedToken
                               : role == "Finance-PaidBy" && status == "Paid" ? MessageTemplateSystemNames.ExpensePaidToken : "";
            }
            if (!string.IsNullOrEmpty(messageToken))
            {
                var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == messageToken && x.Active);

                if (template != null)
                {
                    // tokens
                    var tokens = new List<Token>();
                    tokens.Add(new Token("Expense.FirstName", User.Person.FirstName + " " + User.Person.LastName));
                    tokens.Add(new Token("Expense.TotalAmount", totalAmount));
                    if (!string.IsNullOrEmpty(postApproverNote) && status == "Approved")
                        tokens.Add(new Token("Expense.Note", postApproverNote));
                    else if (!string.IsNullOrEmpty(paidByNote) && status == "Paid")
                        tokens.Add(new Token("Expense.Note", paidByNote));
                    else
                        tokens.Add(new Token("Expense.Note", "--"));

                    var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";

                    var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                    template.Body +
                                "<br><br> <a href=" + redirectUrl + " target='_blank'>Click Here</a> <br /></p></div>";

                    var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                                 "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                                 "<div style='padding: 10px 0;'>" +
                                 "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                                 "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                                 "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                                 "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                                 "</div>" +
                                 "<div style='padding: 0 10px 10px;'>" +
                                 "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                                 "</div>" +
                                 "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                                 "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                                 "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                                 "</div>";

                    var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                    // email account
                    var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == SiteId);
                    if (emailAccount == null)
                    {
                        throw new NullReferenceException("Email account not found");
                    }

                    // replace subject and body tokens
                    var subject = _tokenizer.Replace(template.Subject, tokens, false);
                    var body = _tokenizer.Replace(FullEmail, tokens, true);

                    await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, User.Email, null);
                }
            }
        }
        public async Task SendAdvanceExpenseMailToApprovers(ApplicationUser User, decimal totalAmount, string redirectUrl, string role, string status, string preApproverNote, string SiteId)
        {
            string messageToken = "";

            if (!string.IsNullOrEmpty(role))
            {
                messageToken = role == "Finance" && status == "Submitted" ? MessageTemplateSystemNames.AdvanceExpensePreApproveRequestToken
                               : role == "Finance-Preapprove" && status == "Pre-Approved" ? MessageTemplateSystemNames.AdvanceExpenseApproveRequestToken
                               : role == "Finance-Approver" && status == "Approved" ? MessageTemplateSystemNames.AdvanceExpensePaymentRequestToken
                               : "";
            }
            if (!string.IsNullOrEmpty(messageToken))
            {
                var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == messageToken && x.Active);

                if (template != null)
                {
                    // tokens 
                    var tokens = new List<Token>();
                    tokens.Add(new Token("AdvanceExpense.FirstName", User.Person.FirstName + " " + User.Person.LastName));
                    tokens.Add(new Token("AdvanceExpense.TotalAmount", totalAmount));
                    if (!string.IsNullOrEmpty(preApproverNote))
                        tokens.Add(new Token("AdvanceExpense.Note", preApproverNote));
                    else
                        tokens.Add(new Token("AdvanceExpense.Note", "--"));

                    var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";

                    var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                    template.Body +
                                "<br><br> <a href=" + redirectUrl + " target='_blank'>Click Here</a> <br /></p></div>";

                    var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                                 "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                                 "<div style='padding: 10px 0;'>" +
                                 "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                                 "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                                 "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                                 "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                                 "</div>" +
                                 "<div style='padding: 0 10px 10px;'>" +
                                 "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                                 "</div>" +
                                 "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                                 "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                                 "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                                 "</div>";

                    var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                    // email account
                    var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == SiteId);
                    if (emailAccount == null)
                    {
                        throw new NullReferenceException("Email account not found");
                    }

                    // replace subject and body tokens
                    var subject = _tokenizer.Replace(template.Subject, tokens, false);
                    var body = _tokenizer.Replace(FullEmail, tokens, true);

                    await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, User.Email, null);
                }
            }
        }
        public async Task SendAdvanceExpenseMailToUsers(ApplicationUser User, decimal totalAmount, string redirectUrl, string role, string status, string postApproverNote, string paidByNote, string SiteId)
        {
            string messageToken = "";
            if (!string.IsNullOrEmpty(role))
            {
                messageToken = role == "Finance-Preapprove" && status == "Pre-Approved" ? MessageTemplateSystemNames.AdvanceExpensePreApprovedToken
                               : role == "Finance-Approver" && status == "Approved" ? MessageTemplateSystemNames.AdvanceExpenseApprovedToken
                               : role == "Finance-PaidBy" && status == "Paid" ? MessageTemplateSystemNames.AdvanceExpensePaidToken
                               : "";
            }
            if (!string.IsNullOrEmpty(messageToken))
            {
                var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == messageToken && x.Active);

                if (template != null)
                {
                    // tokens
                    var tokens = new List<Token>();
                    tokens.Add(new Token("AdvanceExpense.FirstName", User.Person.FirstName + " " + User.Person.LastName));
                    tokens.Add(new Token("AdvanceExpense.TotalAmount", totalAmount));
                    if (!string.IsNullOrEmpty(postApproverNote) && status == "Approved")
                        tokens.Add(new Token("AdvanceExpense.Note", postApproverNote));
                    else if (!string.IsNullOrEmpty(paidByNote) && status == "Paid")
                        tokens.Add(new Token("AdvanceExpense.Note", paidByNote));
                    else
                        tokens.Add(new Token("AdvanceExpense.Note", "--"));

                    var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";

                    var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                    template.Body +
                                "<br><br> <a href=" + redirectUrl + " target='_blank'>Click Here</a> <br /></p></div>";

                    var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                                 "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                                 "<div style='padding: 10px 0;'>" +
                                 "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                                 "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                                 "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                                 "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                                 "</div>" +
                                 "<div style='padding: 0 10px 10px;'>" +
                                 "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                                 "</div>" +
                                 "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                                 "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                                 "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                                 "</div>";

                    var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                    // email account
                    var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == SiteId);
                    if (emailAccount == null)
                    {
                        throw new NullReferenceException("Email account not found");
                    }

                    // replace subject and body tokens
                    var subject = _tokenizer.Replace(template.Subject, tokens, false);
                    var body = _tokenizer.Replace(FullEmail, tokens, true);

                    await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, User.Email, null);
                }
            }
        }
        public async Task SendPurchaseExpenseMailToApprovers(ApplicationUser User, decimal totalAmount, string redirectUrl, string role, string status, string preApproverNote, string SiteId)
        {
            string messageToken = "";

            if (!string.IsNullOrEmpty(role))
            {
                messageToken = role == "Finance" && status == "Submitted" ? MessageTemplateSystemNames.PurchaseExpensePreApproveRequestToken
                               : role == "Finance-Preapprove" && status == "Pre-Approved" ? MessageTemplateSystemNames.PurchaseExpenseApproveRequestToken
                               : role == "Finance-Approver" && status == "Approved" ? MessageTemplateSystemNames.PurchaseExpensePaymentRequestToken
                               : "";
            }
            if (!string.IsNullOrEmpty(messageToken))
            {
                var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == messageToken && x.Active);

                if (template != null)
                {
                    // tokens
                    var tokens = new List<Token>();
                    tokens.Add(new Token("PurchaseExpense.FirstName", User.Person.FirstName + " " + User.Person.LastName));
                    tokens.Add(new Token("PurchaseExpense.TotalAmount", totalAmount));
                    if (!string.IsNullOrEmpty(preApproverNote))
                        tokens.Add(new Token("PurchaseExpense.Note", preApproverNote));
                    else
                        tokens.Add(new Token("PurchaseExpense.Note", "--"));

                    var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";

                    var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                    template.Body +
                                "<br><br> <a href=" + redirectUrl + " target='_blank'>Click Here</a> <br /></p></div>";


                    var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                                 "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                                 "<div style='padding: 10px 0;'>" +
                                 "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                                 "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                                 "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                                 "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                                 "</div>" +
                                 "<div style='padding: 0 10px 10px;'>" +
                                 "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                                 "</div>" +
                                 "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                                 "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                                 "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                                 "</div>";

                    var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                    // email account
                    var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == SiteId);
                    if (emailAccount == null)
                    {
                        throw new NullReferenceException("Email account not found");
                    }

                    // replace subject and body tokens
                    var subject = _tokenizer.Replace(template.Subject, tokens, false);
                    var body = _tokenizer.Replace(FullEmail, tokens, true);

                    await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, User.Email, null);
                }
            }
        }
        public async Task SendPurchaseExpenseMailToUsers(ApplicationUser User, decimal totalAmount, string redirectUrl, string role, string status, string postApproverNote, string paidByNote, string SiteId)
        {
            string messageToken = "";
            if (!string.IsNullOrEmpty(role))
            {
                messageToken = role == "Finance-Preapprove" && status == "Pre-Approved" ? MessageTemplateSystemNames.PurchaseExpensePreApprovedToken
                               : role == "Finance-Approver" && status == "Approved" ? MessageTemplateSystemNames.PurchaseExpenseApprovedToken
                               : role == "Finance-PaidBy" && status == "Paid" ? MessageTemplateSystemNames.PurchaseExpensePaidToken
                               : "";
            }
            if (!string.IsNullOrEmpty(messageToken))
            {
                var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == messageToken && x.Active);

                if (template != null)
                {
                    // tokens
                    var tokens = new List<Token>();
                    tokens.Add(new Token("PurchaseExpense.FirstName", User.Person.FirstName + " " + User.Person.LastName));
                    tokens.Add(new Token("PurchaseExpense.TotalAmount", totalAmount));
                    if (!string.IsNullOrEmpty(postApproverNote) && status == "Approved")
                        tokens.Add(new Token("PurchaseExpense.Note", postApproverNote));
                    else if (!string.IsNullOrEmpty(paidByNote) && status == "Paid")
                        tokens.Add(new Token("PurchaseExpense.Note", paidByNote));
                    else
                        tokens.Add(new Token("PurchaseExpense.Note", "--"));

                    var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";

                    var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                    template.Body +
                                "<br><br> <a href=" + redirectUrl + " target='_blank'>Click Here</a> <br /></p></div>";

                    var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                                 "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                                 "<div style='padding: 10px 0;'>" +
                                 "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                                 "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                                 "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                                 "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                                 "</div>" +
                                 "<div style='padding: 0 10px 10px;'>" +
                                 "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                                 "</div>" +
                                 "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                                 "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                                 "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                                 "</div>";

                    var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                    // email account
                    var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == SiteId) ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == SiteId);
                    if (emailAccount == null)
                    {
                        throw new NullReferenceException("Email account not found");
                    }

                    // replace subject and body tokens
                    var subject = _tokenizer.Replace(template.Subject, tokens, false);
                    var body = _tokenizer.Replace(FullEmail, tokens, true);

                    await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, User.Email, null);
                }
            }
        }
        public async Task SendTimeOutStatusEmailToApprovers(Employee employee, Employee lead, string redirectUrl, TimeInTimeOut code)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.MissingTimeOutSubmitToken && x.Active);


            if (template != null)
            {
                // tokens
                var tokens = new List<Token>();

                // add code to the token list
                tokens.Add(new Token("Employee.FirstName", employee.Person.FirstName));
                tokens.Add(new Token("Employee.LastName", employee.Person.LastName));
                tokens.Add(new Token("Lead.FirstName", lead.Person.FirstName));
                tokens.Add(new Token("Lead.LastName", lead.Person.LastName));
                tokens.Add(new Token("TimeInDate", code.TimeInDate?.ToString("MM-dd-yyyy") ?? ""));
                tokens.Add(new Token("TimeOutDate", code.TimeOutDate?.ToString("MM-dd-yyyy") ?? ""));
                tokens.Add(new Token("TimeIn", DateTime.Today.Add(code.TimeIn).ToString("hh:mm tt", CultureInfo.InvariantCulture)));
                tokens.Add(new Token("TimeOut", DateTime.Today.Add(code.TimeOut).ToString("hh:mm tt", CultureInfo.InvariantCulture)));

                var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";
                var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                template.Body + "<br><br> <a href=" + redirectUrl + " target='_blank'>Click Here</a> <br /></p></div>";

                var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                             "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                             "<div style='padding: 10px 0;'>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                             "</div>" +
                             "<div style='padding: 0 10px 10px;'>" +
                             "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                             "</div>" +
                             "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                             "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                             "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                             "</div>";

                var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId) ?? await _db.EmailAccounts.FirstOrDefaultAsync();
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(FullEmail, tokens, true);

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, lead.Person.PrimaryEmailAddress, null);
            }
        }
        public async Task SendTimeOutStatusEmailToEmployee(Employee employee, Employee lead, string status, TimeInTimeOut code)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync();
            if (status == "Approved")
                template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.TimeOutApprovedEmailToEmployeeToken && x.Active);
            else
                template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.TimeOutDeclinedEmailToEmployeeToken && x.Active);

            if (template != null)
            {
                // tokens
                var tokens = new List<Token>();

                // add code to the token list
                tokens.Add(new Token("Employee.FirstName", employee.Person.FirstName));
                tokens.Add(new Token("Employee.LastName", employee.Person.LastName));
                tokens.Add(new Token("Lead.FirstName", lead.Person.FirstName));
                tokens.Add(new Token("Lead.LastName", lead.Person.LastName));
                tokens.Add(new Token("TimeInDate", code.TimeInDate?.ToString("MM-dd-yyyy") ?? ""));
                tokens.Add(new Token("TimeOutDate", code.TimeOutDate?.ToString("MM-dd-yyyy") ?? ""));
                tokens.Add(new Token("TimeIn", DateTime.Today.Add(code.TimeIn).ToString("hh:mm tt", CultureInfo.InvariantCulture)));
                tokens.Add(new Token("TimeOut", DateTime.Today.Add(code.TimeOut).ToString("hh:mm tt", CultureInfo.InvariantCulture)));

                //tokens.Add(new Token("Leave.URL", AppUrl));

                var Header = "<div style='text-align:center;padding:10px;background-color:#000;'><img src='" + "https://www.meldep.com/assets/meldep_logo.a392f709.png" + "' style='width:8%; height: 8%;'/></div>";
                var Body = "<div style='text-align: center; margin-bottom: 30px;'><div style='text-align: left; padding: 10px; margin: 0 auto; font-size: 17px;'><p style='color:black;text-align: left; margin: 0 auto; font-size: 17px;'>" +
                template.Body + " <br /></p></div>";
                var Footer = "<div style='text-align:center;background-color:#000;padding:20px;'>" +
                             "<h3 style='margin: 0px; color: white; font-size: 25px;'>Meldep</h3>" +
                             "<div style='padding: 10px 0;'>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='mailto:info@vskysolutions.com' style='color: #fff; text-decoration: none;'>info@vskysolutions.com</a></div>" +
                             "<div style='display: inline-block; width: 49%; font-size: 15px;'>" +
                             "<a href='tel:+19049148759' style='color: #fff; text-decoration: none;'>+1 904-914-8759</a></div>" +
                             "</div>" +
                             "<div style='padding: 0 10px 10px;'>" +
                             "<p style='margin: 0px; color: white; font-size: 15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                             "</div>" +
                             "<div style='padding-top: 10px;color: white; font-size: 13px; text-align: center; border-top: 2px solid #000;'>" +
                             "Copyright ©2024 Vsky Solutions. Application Designed and Developed by " +
                             "<a href='https://vskysolutions.com' target='_blank' style='color: white; font-size: 15px; font-weight: bold; text-decoration: none;'>VSky Solutions.</a></div>" +
                             "</div>";

                var FullEmail = "<div style='margin: 30px 50px; border: 5px solid #000; border-radius: 10px;'>" + Header + Body + Footer + "</div>";

                // email account
                var emailAccount = await _db.EmailAccounts.FirstOrDefaultAsync(x => x.Id == template.EmailAccountId) ?? await _db.EmailAccounts.FirstOrDefaultAsync();
                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace subject and body tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(FullEmail, tokens, true);

                // send email
                await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, employee.Person.PrimaryEmailAddress, null);
            }
        }
        public async Task SendAnniversaryEmailToHr(List<Employee> employees, string type, List<string> emailAddresses, string siteId = null)
        {
            if (employees == null || !employees.Any())
                return;

            string templateName =
                type == "1 week" ? "Employee.EmployeeAnniversary1WeekToHR" :
                type == "2 weeks" ? "Employee.EmployeeAnniversary2WeekToHR" :
                "Employee.EmployeeAnniversaryToHR";

            var template = await _db.MessageTemplates
                .FirstOrDefaultAsync(x => x.Name == templateName && x.Active);

            if (template == null)
                return;

            var recipients = new List<string>();
            //Hr team / multiple emails
            if (emailAddresses != null && emailAddresses.Any())
                recipients.AddRange(emailAddresses);

            // remove duplicates & nulls
            recipients = recipients
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();

            if (!recipients.Any())
                return;

            string toEmailCsv = string.Join(",", recipients);

            var employeeListText = "<ul>";
            foreach (var employee in employees)
            {
                var years = employee.YearsCompleted;
                var yearText = years == 1 ? "year" : "years";
                employeeListText += $"<li>{employee.Person.FullName} - {years} {yearText}</li>";
            }
            employeeListText += "</ul>";

            var tokens = new List<Token>
            {
               new Token("AnniversaryEmployees.List", employeeListText)
            };

            // Build Email Layout
            var FullEmail = BuildEmailLayout(template.Body, null, true);

            var subject = _tokenizer.Replace(template.Subject, tokens, false);
            var body = _tokenizer.Replace(FullEmail, tokens, false);

            var emailAccount = await _db.EmailAccounts
                .FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == siteId)
                ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == siteId);

            if (emailAccount == null)
            {
                throw new NullReferenceException("Email account not found");
            }

            await _emailSender.SendEmailAsync(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, toEmailCsv, null);
        }

        #region Share My Tenant
        public async Task SendSiteShareInvitationMail(Person person, string email, string siteName, string invitedByName, string siteId)
        {
            var template = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Name == MessageTemplateSystemNames.SiteShareInvitationToken && x.Active);
            if (template != null)
            {
                // tokens
                var tokens = new List<Token>();

                var fullName = person?.FirstName + " " + person?.LastName;

                // domain
                var WEB_Domain = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AppSettings")["WEB_Domain"];
                string appUrl = WEB_Domain + "/dashboard";

                tokens.Add(new Token("SiteShare.FullName", fullName));
                tokens.Add(new Token("SiteShare.SiteName", siteName));
                tokens.Add(new Token("SiteShare.InvitedBy", invitedByName));
                tokens.Add(new Token("SiteShare.InvitationLink", appUrl));

                // Build Email Layout
                var fullEmail = BuildEmailLayout(template.Body, null, true);

                // email account
                var emailAccount = await _db.EmailAccounts
                    .FirstOrDefaultAsync(x => x.Id == template.EmailAccountId && x.SiteId == siteId)
                    ?? await _db.EmailAccounts.FirstOrDefaultAsync(x => x.SiteId == siteId);

                if (emailAccount == null)
                {
                    throw new NullReferenceException("Email account not found");
                }

                // replace tokens
                var subject = _tokenizer.Replace(template.Subject, tokens, false);
                var body = _tokenizer.Replace(fullEmail, tokens, true);

                // send email
                await _emailSender.SendEmailAsync(
                    emailAccount,
                    subject,
                    body,
                    emailAccount.Email,
                    emailAccount.DisplayName,
                    email,
                    null
                );
            }        
        }
        #endregion

        public string BuildEmailLayout(string bodyContent, string webDomain = null, bool includeSystemNote = false)
        {
            var header =
                "<div style='text-align:center;padding:10px;background-color:#000;'>" +
                "<img src='https://www.meldep.com/assets/meldep_logo.a392f709.png' style='width:8%; height:8%;'/>" +
                "</div>";

            string viewRequestButton = "";

            if (!string.IsNullOrWhiteSpace(webDomain))
            {
                viewRequestButton =
                    "<p>" +
                        "<a href='" + webDomain + "/account/help-desk' target='_blank' " +
                        "style='background-color:#1b75ab; color:white; padding:10px 20px; " +
                        "text-decoration:none; border-radius:5px; display:inline-block; " +
                        "font-size:16px; font-family:Arial, sans-serif; float:left; margin-left:1%;'>" +
                        "View Request" +
                        "</a>" +
                    "</p>";
            }

            var body =
                "<div style='text-align:center; margin-bottom:30px;'>" +
                    "<div style='text-align:left; padding:10px; margin:0 auto; font-size:17px;'>" +
                        "<p style='color:black; margin:0; font-size:17px;'>" +
                            bodyContent +
                        "</p>" +
                    "</div><br/>" +
                    viewRequestButton +
                "</div>";

            var systemNote = includeSystemNote ?
                "<div style='margin:30px 0; padding:12px; border-top:1px solid #ddd; border-bottom:1px solid #ddd;" +
                "background-color:#f9f9f9; text-align:center; font-size:14px; color:#555;'>" +
                "<strong>Note:</strong> This is a system generated mail. Please do not reply." +
                "</div>"
                : "<div style='margin:30px 0; padding:12px; border-top:1px solid #ddd; border-bottom:1px solid #ddd;" +
                "background-color:#f9f9f9; text-align:center; font-size:14px; color:#555;'>" +
                "<strong>Note:</strong> You may reply directly to this email. Your response will be automatically recorded under your ticket in the system." +
                "</div>";

            var footer =
                "<div style='text-align:center;background-color:#000;padding:20px;margin-top:7%;'>" +
                    "<h3 style='margin:0;color:white;font-size:25px;'>Meldep</h3>" +
                    "<div style='padding:10px 0;'>" +
                        "<div style='display:inline-block;width:49%;font-size:15px;'>" +
                            "<a href='mailto:info@vskysolutions.com' style='color:#fff;text-decoration:none;'>info@vskysolutions.com</a>" +
                        "</div>" +
                        "<div style='display:inline-block;width:49%;font-size:15px;'>" +
                            "<a href='tel:+19049148759' style='color:#fff;text-decoration:none;'>+1 904-914-8759</a>" +
                        "</div>" +
                    "</div>" +
                    "<p style='margin:0;color:white;font-size:15px;'>9140, Baymeadows Park Drive, Suite 10S, Jacksonville, FL, 32256, United States</p>" +
                    "<div style='padding-top:10px;color:white;font-size:13px;'>" +
                    "Copyright ©2024 Vsky Solutions. Designed by " +
                    "<a href='https://vskysolutions.com' target='_blank' style='color:white;font-weight:bold;text-decoration:none;'>VSky Solutions</a>" +
                    "</div>" +
                "</div>";

            return
                "<div style='margin:30px 50px; border:5px solid #000; border-radius:10px;'>" +
                header + body + systemNote + footer +
                "</div>";
        }

        #region Private methods
        public static bool ContainsHtml(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            try
            {
                // Fast pre-check to avoid unnecessary parsing
                if (!input.Contains("<") || !input.Contains(">"))
                    return false;

                var doc = new HtmlAgilityPack.HtmlDocument
                {
                    OptionFixNestedTags = true
                };

                doc.LoadHtml(input);

                // Detect real HTML nodes (ignore text-only)
                return doc.DocumentNode.Descendants().Any(n => n.NodeType == HtmlAgilityPack.HtmlNodeType.Element);
            }
            catch
            {
                return false;
            }
        }
        private (string SafeHtml, bool IsHtml) SanitizeDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return (description, false);

            var sanitizer = new HtmlSanitizer();

            // Text formatting
            sanitizer.AllowedTags.UnionWith(new[]
            {
                "p", "br", "b", "strong", "i", "em",
                "ul", "ol", "li", "div", "span"
            });

            // Image support
            sanitizer.AllowedTags.Add("img");
            sanitizer.AllowedAttributes.UnionWith(new[]
            {
                "src", "alt", "width", "height", "style"
            });

            // Allow base64 images
            sanitizer.AllowedSchemes.Add("data");

            var isHtml = ContainsHtml(description);
            var safeHtml = isHtml ? sanitizer.Sanitize(description) : description;

            return (safeHtml, isHtml);
        }
        private string PrepareRecipientsCsv(Employee employee, List<string> emailAddresses, string requesterEmail = null)
        {
            var recipients = new List<string>();

            // Employee email (AssignedTo / Requester employee)
            if (!string.IsNullOrEmpty(employee?.Person?.PrimaryEmailAddress))
                recipients.Add(employee.Person.PrimaryEmailAddress);

            // Support team / multiple emails
            if (emailAddresses != null && emailAddresses.Any())
                recipients.AddRange(emailAddresses);

            // External requester email (optional)
            if (!string.IsNullOrEmpty(requesterEmail))
                recipients.Add(requesterEmail);

            // remove duplicates & nulls
            recipients = recipients
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            if (!recipients.Any())
                return null;

            return string.Join(",", recipients);
        }
        private void InsertSystemEmailReplyAsync(string toEmailCsv, string fromEmail, string subject, string body, string ownerId, string twilioEmailId, string helpDeskId, DateTime GetDateTime, bool IsSystemEmail = true, string externalToEmail = null, string ccEmailCsv = null)
        {
            var emailReply = new EmailReplies
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = !string.IsNullOrEmpty(ownerId) ? ownerId : null,
                ToEmail = toEmailCsv,
                FromEmail = fromEmail,
                ExternalToEmail = externalToEmail,
                CCEmail = ccEmailCsv,
                TwilioEmailId = twilioEmailId,
                Subject = subject,
                Body = body,
                IsSystemEmail = IsSystemEmail,
                CreatedOnUtc = GetDateTime,
                UpdatedOnUtc = GetDateTime,
                Deleted = false
            };

            _emailRepliesServices.InsertEmailReplies(emailReply);

            var mapping = new HelpDeskEmailRepliesMapping
            {
                Id = Guid.NewGuid().ToString(),
                HelpDeskId = helpDeskId,
                EmailRepliesId = emailReply.Id
            };

            _helpDeskEmailRepliesMappingService.InsertHelpDeskEmailRepliesMapping(mapping);
        }
        #endregion
    }
}