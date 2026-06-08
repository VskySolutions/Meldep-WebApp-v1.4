using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.PowerBI.Api.Models;
using Vsky.Models;

namespace Vsky.Services.Messages
{
    public interface IWorkflowMessageService
    {
        Task SendWelcomeEmail(ApplicationUser user, string password);
        Task SendResetPasswordEmail(ApplicationUser user, string password);
        Task SendTwoFactorToken(ApplicationUser user, string code);
        Task SendMailToHr(Employee employee, EmployeeLeave code);
        Task SendMailToApprover(Employee employee, EmployeeLeave code);
        Task SendLeaveStatusEmailToEmployee(Employee employee, EmployeeLeave code);
        Task SendPasswordResetEmail(ApplicationUser user, string code);
        Task SendContactUserEmail(Contact contact);
        Task SendContactCandidateEmail(Contact contact);
        Task SendRegisterUserEmail(string Id);
        Task SendRegisterCandidateEmail(string Id);

        #region Help Desk
        Task SendHelpDeskMailToAdmin(Employee employee, HelpDesk code, bool isEmailSent, string twilioEmailId, string prefixNo, List<string> emailAddresses, string requesterName, string requesterEmailId, DateTime GetDateTime);
        Task SendHelpDeskStatusMail(Employee employee, HelpDesk code, List<string> emailAddresses, string twilioEmailId, string prefixNo, string siteName, string requesterEmail, string assignedToName, DateTime GetDateTime);
        Task SendHelpDeskAssignedToMail(Employee employee, HelpDesk code, string assignedToName, string twilioEmailId, string prefixNo, string siteName, string requesterEmail, DateTime GetDateTime);
        Task SendHelpDeskReplyMail(Employee employee, HelpDesk code, EmailReplies reply, string twilioEmailId, List<string> toEmailList, List<string> actualToEmails, List<string> ccEmailList, string prefixNo, DateTime GetDateTime);
        Task SendRequestReceivedMailToRequester(Employee employee, HelpDesk code, string twilioEmailId, string requesterEmailId, bool isEmailSent, string prefixNo, string requesterName, DateTime GetDateTime);
        Task SendHelpDeskReminderMail(HelpDesk helpdesk, string title, string templateName, string prefixNo, DateTime GetDateTime);
        #endregion

        Task SendWebsiteDemoMailToUser(Website_Demos website_Demos);
        Task SendDemoRequestSubmittedMailToVsky(Website_Demos website_Demos);
        Task SendTimeOutStatusEmailToApprovers(Employee employee, Employee lead, string redirectUrl, TimeInTimeOut code);
        Task SendTimeOutStatusEmailToEmployee(Employee employee, Employee lead, string status, TimeInTimeOut code);

        Task SendExpenseMailToApprovers(ApplicationUser User, decimal totalAmount, string redirectUrl, string role, string status, string preApproverNote, string SiteId);
        Task SendExpenseMailToUsers(ApplicationUser User, decimal totalAmount, string redirectUrl, string role, string status, string preApproverNote, string paidByNote, string SiteId);

        Task SendAdvanceExpenseMailToApprovers(ApplicationUser User, decimal totalAmount, string redirectUrl, string role, string status, string preApproverNote, string SiteId);
        Task SendAdvanceExpenseMailToUsers(ApplicationUser User, decimal totalAmount, string redirectUrl, string role, string status, string postApproverNote, string paidByNote, string SiteId);

        Task SendPurchaseExpenseMailToApprovers(ApplicationUser User, decimal totalAmount, string redirectUrl, string role, string status, string preApproverNote, string SiteId);
        Task SendPurchaseExpenseMailToUsers(ApplicationUser User, decimal totalAmount, string redirectUrl, string role, string status, string postApproverNote, string paidByNote, string SiteId);

        Task SendAnniversaryEmailToHr(List<Employee> employees, string type, List<string> emailAddresses, string siteId = null);

        #region SendSiteShareInvitationMail
        Task SendSiteShareInvitationMail(Person person, string email, string siteName, string invitedByName, string siteId);
        #endregion

        string BuildEmailLayout(string bodyContent, string webDomain = null, bool includeSystemNote = false);
    }
        
}