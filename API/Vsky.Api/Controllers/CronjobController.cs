using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vsky.Models;
using Vsky.Services.AzureBlobImage;
using Vsky.Services.Common;
using Vsky.Services.EmailReply;
using Vsky.Services.EmployeeLeaves;
using Vsky.Services.EmailReply;
using Vsky.Services.Employees;
using Vsky.Services.HelpDesks;
using Vsky.Services.ImageMigration;
using Vsky.Services.Issues;
using Vsky.Services.LeaveSchedule;
using Vsky.Services.Messages;
using Vsky.Services.Note;
using Vsky.Services.Notifications;
using Vsky.Services.ProjectTasks;
using Vsky.Services.Requirements;
using Vsky.Services.Sites;
using Vsky.Services.TestCases;
using Vsky.Services.Timesheets;
using Vsky.Services.Users;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Vsky.Api.Controllers
{
    [AllowAnonymous]
    [Route("cron-job")]
    public class CronjobController : BaseController
    {
        #region Define Services
        private readonly IEmployeeService _employeeService;
        private readonly ICommonService _commonService;
        private readonly ISiteService _siteService;
        private readonly IUserService _userService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IAzureBlobImageServices _azureBlobImageServices;
        private readonly IImageMigrationService _imageMigrationService;
        private readonly IIssueService _issueService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IRequirementService _requirementService;
        private readonly INoteService _noteService;
        private readonly ITestCaseService _testCaseService;
        private readonly IHelpDeskService _helpDeskService;
        private readonly IHelpDeskReminderLogService _helpDeskReminderLogService;
        private readonly IHelpDeskEmailRepliesMappingService _helpDeskEmailRepliesMappingService;
        private readonly IMasterNotificationService _masterNotificationService;
        private readonly INotificationService _notificationService;
        private readonly ITimesheetService _timesheetService;
        private readonly ILeaveScheduleService _leaveScheduleService;
        private readonly IEmployeeLeaveService _employeeLeaveService;
        #endregion

        #region Services Initializations 
        public CronjobController(
             IEmployeeService employeeService,
             ICommonService commonService,
             ISiteService siteService,
             IWorkflowMessageService workflowMessageService,
             IUserService userService,
             IAzureBlobImageServices azureBlobImageServices,
             IImageMigrationService imageMigrationService,
             IIssueService issueService,
             IProjectTaskService projectTaskService,
             IRequirementService requirementService,
             INoteService noteService,
             ITestCaseService testCaseService,
             IHelpDeskService helpDeskService,
             IHelpDeskReminderLogService helpDeskReminderLogService,
             IHelpDeskEmailRepliesMappingService helpDeskEmailRepliesMappingService,
             IMasterNotificationService masterNotificationService,
             INotificationService notificationService,
             ITimesheetService timesheetService,
             ILeaveScheduleService leaveScheduleService,
             IEmployeeLeaveService employeeLeaveService

        )
        {
            _employeeService = employeeService;
            _commonService = commonService;
            _siteService = siteService;
            _workflowMessageService = workflowMessageService;
            _userService = userService;
            _azureBlobImageServices = azureBlobImageServices;
            _imageMigrationService = imageMigrationService;
            _issueService = issueService;
            _projectTaskService = projectTaskService;
            _requirementService = requirementService;
            _noteService = noteService;
            _testCaseService = testCaseService;
            _helpDeskService = helpDeskService;
            _helpDeskReminderLogService = helpDeskReminderLogService;
            _helpDeskEmailRepliesMappingService = helpDeskEmailRepliesMappingService;
            _masterNotificationService = masterNotificationService;
            _notificationService = notificationService;
            _timesheetService = timesheetService;
            _leaveScheduleService = leaveScheduleService;
            _employeeLeaveService = employeeLeaveService;
        }
        #endregion

        #region SendAnniversaryEmailToHr
        // Title: SendAnniversaryEmailToHr
        // Description:This method retrieves all users with the HR role for the site and collects their registered email addresses. It then identifies employees who have work anniversaries today, one week before, and two weeks before, and sends consolidated anniversary notification emails only to HR users for each period.
        [AllowAnonymous]
        [HttpGet("send-anniversary-email-to-hr")]
        public async Task<IActionResult> SendAnniversaryEmailToHr()
        {
            try
            {
                var sites = await _siteService.GetAllSitesList();
                foreach (var site in sites)
                {
                    // send meail to hr
                    var hrs = await _userService.GetUsersByRole(site.Id, "hr");
                    var hrEmails = new List<string>();

                    foreach (var user in hrs)
                    {
                        //var employeeId = _commonService.GetEmployeeIdByUserId(site.Id, user.Id);

                        var employeeId = _commonService.GetEmployeeIdByUserIdAndEmail(site.Id, user.Id);
                        var employeeData = await _employeeService.GetEmployeeDetailsById(employeeId);

                        if (!string.IsNullOrEmpty(employeeData?.Person?.PrimaryEmailAddress))
                        {
                            hrEmails.Add(employeeData.Person.PrimaryEmailAddress);
                        }
                    }

                    if (hrEmails.Any())
                    {
                        // Employees whose work anniversary is today
                        var list = await _employeeService.GetEmployeesForAnniversary(site.Id, 0);
                        if (list?.Count > 0)
                            await _workflowMessageService.SendAnniversaryEmailToHr(list, "today", hrEmails.Distinct().ToList(), site.Id);

                        // 1 week before
                        list = await _employeeService.GetEmployeesForAnniversary(site.Id, 7);
                        if (list?.Count > 0)
                            await _workflowMessageService.SendAnniversaryEmailToHr(list, "1 week", hrEmails.Distinct().ToList(), site.Id);

                        // 2 weeks before
                        list = await _employeeService.GetEmployeesForAnniversary(site.Id, 14);
                        if (list?.Count > 0)
                            await _workflowMessageService.SendAnniversaryEmailToHr(list, "2 weeks", hrEmails.Distinct().ToList(), site.Id);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Issue Image Migration
        [HttpGet("send-images-to-azure-blob")]
        public async Task<IActionResult> MigrateImages()
        {
            try
            {
                int batchSize = 20;
                int processed = 0;

                var images = await _imageMigrationService.GetAllImageMigrations("", "", false, batchSize);

                if (!images.Any())
                    return Ok();

                foreach (var img in images)
                {
                    try
                    {
                        // data:image/png;base64,xxxx
                        var parts = img.Base64Data.Split(',');
                        var header = parts[0]; // data:image/png;base64
                        var base64 = parts[1];
                        var extension = GetExtension(header);
                        var fileName = $"{(img.TableNumber > 0 ? img.TableNumber : img.TableId)}-{img.ImageNo}{extension}";
                        byte[] bytes = Convert.FromBase64String(base64);

                        using var ms = new MemoryStream(bytes);
                        IFormFile file = new FormFile(ms, 0, ms.Length, "file", fileName);

                        var url = await _azureBlobImageServices.UploadImageAsync("site-vsky-solutions", img.TableName, file);

                        img.BlobURL = url;
                        img.IsProcessed = 1;
                        _imageMigrationService.UpdateImageMigration(img);

                        if (img.TableName == "issue")
                        {
                            var IssueData = await _issueService.GetIssueById(img.TableId);
                            IssueData.Description = IssueData.Description.Replace(img.Base64Data, img.BlobURL, StringComparison.Ordinal);
                            _issueService.UpdateIssue(IssueData);
                        }
                        else if (img.TableName == "project-tasks")
                        {
                            var ProjectTaskData = await _projectTaskService.GetById(img.TableId);
                            ProjectTaskData.Description = ProjectTaskData.Description.Replace(img.Base64Data, img.BlobURL, StringComparison.Ordinal);
                            _projectTaskService.UpdateProjectTask(ProjectTaskData);
                        }
                        else if (img.TableName == "requirements")
                        {
                            var RequirementData = await _requirementService.GetRequirementById(img.TableId);
                            RequirementData.Description = RequirementData.Description.Replace(img.Base64Data, img.BlobURL, StringComparison.Ordinal);
                            _requirementService.UpdateRequirement(RequirementData);
                        }
                        else if (img.TableName == "notes")
                        {
                            var NoteData = await _noteService.GetById(img.TableId);
                            NoteData.Note = NoteData.Note.Replace(img.Base64Data, img.BlobURL, StringComparison.Ordinal);
                            _noteService.UpdateNote(NoteData);
                        }
                        else if (img.TableName == "test-cases")
                        {
                            var TestCaseData = await _testCaseService.GetTestCaseById(img.TableId);
                            TestCaseData.Description = TestCaseData.Description.Replace(img.Base64Data, img.BlobURL, StringComparison.Ordinal);
                            _testCaseService.UpdateTestCase(TestCaseData);
                        }
                        else
                        {

                        }
                        processed++;

                        await Task.Delay(50);
                    }
                    catch (Exception ex)
                    {
                        // Optional: log failure
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static string GetExtension(string header)
        {
            if (header.Contains("image/png")) return ".png";
            if (header.Contains("image/jpeg")) return ".jpg";
            if (header.Contains("image/jpg")) return ".jpg";
            if (header.Contains("image/gif")) return ".gif";
            if (header.Contains("image/webp")) return ".webp";

            return ".png"; // fallback
        }
        #endregion

        #region Auto-Generated System Reminders for Helpdesk Requests

        /// <summary>
        /// This cron job for send email notifications reminders
        /// </summary>
        /// <returns></returns>
       
        [AllowAnonymous]
        [HttpGet("send-helpdesk-reminder-system-notifications")]
        public async Task<IActionResult> SendHelpDeskReminderSystemNotifications()
        {
            try
            {
                // var GetDateTime = DateTime.UtcNow;

                var reminderStatuses = new List<(string StatusName, string NotificationNumber, double Hours)>
                {
                    ("New", "HelpDeskNewReminder", 4),
                    ("Assigned", "HelpDeskAssignedReminder", 8),
                    ("In Progress", "HelpDeskInProgressReminder", 24),
                    ("Completed", "HelpDeskCompletedReminder", 24)
                };

                var sites = await _siteService.GetAllSitesList();

                foreach (var site in sites)
                {
                    var GetDateTime = _siteService.GetDateTime(site.TimeZone);
                    var siteInfo = await GetSiteTicketInfoAsync(site.Id);
                    string prefixNo = siteInfo != null ? siteInfo.TicketNoPrefix : "HR";

                    var systemUserId = await _userService.GetUserIdByRole(site.Id, "Site Super Admin");
                    var adminUsers = await _userService.GetUsersByRole(site.Id, "Site Super Admin");

                    string createdById = systemUserId;
                    var closedStatusId = _commonService.GetDrownValueIdByTypeandValue(site.Id, "HelpDesk Status", "Closed");

                    foreach (var status in reminderStatuses)
                    {
                        var statusId = _commonService.GetDrownValueIdByTypeandValue(site.Id, "HelpDesk Status", status.StatusName);
                        if (statusId == null) continue;

                        var helpDeskList = await _helpDeskService.GetAllHelpDesksByStatusId(statusId);

                        // ================= New Tickets =================
                        if (status.StatusName == "New")
                        {
                            var filteredNewTickets = new List<HelpDesk>();

                            foreach (var ticket in helpDeskList.Where(x => x.AssignedToId == null))
                            {
                                // Get latest status from status log
                                var (lastStatusLog, logCount) = await _helpDeskService.GetLatestStatusLogByTicketId(site.Id, ticket.Id);

                                // Skip if already action done (logCount > 1)
                                if (logCount > 1) continue;

                                // Skip CLOSED tickets
                                if (lastStatusLog?.StatusId == closedStatusId)
                                    continue;

                                // Time condition
                                var diffHours = (GetDateTime - ticket.CreatedDate).TotalHours;

                                if (diffHours >= status.Hours)
                                {
                                    filteredNewTickets.Add(ticket);
                                }
                            }

                            if (!filteredNewTickets.Any())
                                continue;

                            string ticketList = "";
                            foreach (var ticket in filteredNewTickets)
                            {
                                ticketList += $"{prefixNo}-{ticket.TicketNo}, ";
                            }

                            // Admin only
                            foreach (var user in adminUsers)
                            {
                                var masterNotification =
                                    await _masterNotificationService.GetMasterNotificationByNumber(
                                        site.Id,
                                        "HelpDeskNewReminder",
                                        user.Id);

                                if (masterNotification == null)
                                    continue;

                                string message = masterNotification.Message + " " + ticketList;

                                _notificationService.AddNotification(
                                    site.Id,
                                    masterNotification.Title,
                                    message,
                                    masterNotification.Type,
                                    createdById,
                                    null,
                                    "/help-desk",
                                    user.Id,
                                    createdById,
                                    GetDateTime
                                    );
                            }
                            continue;
                        }

                        // ================= Assigned / In Progress / Completed =================
                        var groupedTickets = helpDeskList
                            .Where(x => !string.IsNullOrEmpty(x.AssignedToId))
                            .GroupBy(x => x.AssignedToId);

                        foreach (var group in groupedTickets)
                        {
                            var LoggedUserId = _commonService.GetLoggeduserIdByEmployeeId(site.Id, group.Key);
                            if (string.IsNullOrEmpty(LoggedUserId)) continue;

                            string ticketList = "";

                            foreach (var ticket in group)
                            {
                                // Get latest status from status log
                                var (lastStatusLog, logCount) = await _helpDeskService.GetLatestStatusLogByTicketId(site.Id, ticket.Id);

                                // Skip if already action done (logCount > 1)
                                if (logCount > 1) continue;

                                // Skip CLOSED tickets
                                if (lastStatusLog?.StatusId == closedStatusId)
                                    continue;

                                // Use last activity date from status log
                                var lastActivityDate = !string.IsNullOrEmpty(lastStatusLog?.CreatedDate)
                                            ? Convert.ToDateTime(lastStatusLog.CreatedDate)
                                            : ticket.CreatedDate;

                                var diffHours = (GetDateTime - lastActivityDate).TotalHours;

                                if (status.StatusName == "Assigned" && diffHours >= status.Hours)
                                {
                                    ticketList += $"{prefixNo}-{ticket.TicketNo}, ";
                                }

                                if (status.StatusName == "In Progress" && diffHours >= status.Hours)
                                {
                                    ticketList += $"{prefixNo}-{ticket.TicketNo}, ";
                                }

                                if (status.StatusName == "Completed" && diffHours >= status.Hours)
                                {
                                    ticketList += $"{prefixNo}-{ticket.TicketNo}, ";
                                }
                            }

                            if (string.IsNullOrEmpty(ticketList)) continue;

                            // Determine recipients
                            var recipients = new List<string>();
                            if (status.StatusName == "In Progress")
                                recipients.Add(LoggedUserId); // Only assigned user
                            else
                            {
                                recipients.Add(LoggedUserId); // Assigned + Admin
                                recipients.AddRange(adminUsers.Select(x => x.Id));
                            }

                            foreach (var recipient in recipients)
                            {
                                var masterNotification =
                                    await _masterNotificationService.GetMasterNotificationByNumber(
                                        site.Id,
                                        status.NotificationNumber,
                                        recipient);

                                if (masterNotification == null)
                                    continue;

                                string message = masterNotification.Message + " " + ticketList;

                                _notificationService.AddNotification(
                                    site.Id,
                                    masterNotification.Title,
                                    message,
                                    masterNotification.Type,
                                    createdById,
                                    null,
                                    "/help-desk",
                                    recipient,
                                    createdById,
                                    GetDateTime
                                    );
                            }
                        }
                    }

                    // ================= First Reply Reminder =================
                    var assignedStatusId = _commonService.GetDrownValueIdByTypeandValue(site.Id, "HelpDesk Status", "Assigned");
                    if (assignedStatusId != null)
                    {
                        var assignedTickets = (await _helpDeskService.GetAllHelpDesksByStatusId(assignedStatusId)).ToList();

                        foreach (var ticket in assignedTickets)
                        {
                            // Get latest status from status log
                            var (lastStatusLog, logCount) = await _helpDeskService.GetLatestStatusLogByTicketId(site.Id, ticket.Id);

                            // Skip if already action done (logCount > 1)
                            if (logCount > 1) continue;

                            // Skip CLOSED tickets
                            if (lastStatusLog?.StatusId == closedStatusId)
                                continue;

                            var diffHours = (GetDateTime - ticket.CreatedDate).TotalHours;
                            if (diffHours < 4) continue;

                            var replies = await _helpDeskEmailRepliesMappingService.GetAllHelpDeskEmailRepliesMappingList(site.Id, ticket.Id, 0, 1);
                            if (replies.Any()) continue;

                            var LoggedUserId = _commonService.GetLoggeduserIdByEmployeeId(site.Id, ticket.AssignedToId);
                            if (string.IsNullOrEmpty(LoggedUserId)) continue;

                            var recipients = new List<string> { LoggedUserId };
                            recipients.AddRange(adminUsers.Select(x => x.Id));

                            foreach (var recipient in recipients)
                            {
                                var masterNotification =
                                    await _masterNotificationService
                                        .GetMasterNotificationByNumber(
                                            site.Id,
                                            "HelpDeskFirstReplyReminder",
                                            recipient);

                                if (masterNotification == null)
                                    continue;

                                string ticketNo = $"{prefixNo}-{ticket.TicketNo}";
                                string message = masterNotification.Message.Replace("[TicketNo]", ticketNo);

                                _notificationService.AddNotification(
                                    site.Id,
                                    masterNotification.Title,
                                    message,
                                    masterNotification.Type,
                                    ticket.CreatedById,
                                    ticket.Id,
                                    "/help-desk",
                                    recipient,
                                    ticket.CreatedById,
                                    GetDateTime
                                    );
                            }
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // get site TicketNOPrefix
        private async Task<Site> GetSiteTicketInfoAsync(string siteId)
        {
            return await _siteService.GetSiteTicketNoPrefixById(siteId);
        }
        #endregion

        [AllowAnonymous]
        [HttpGet("send-pending-weekly-timesheet-notification")]
        public async Task SendPendingWeeklyTimesheetNotification()
        {

            var today = DateTime.UtcNow.Date;

            // Find Monday of the current week
            int daysFromMonday = today.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)today.DayOfWeek - (int)DayOfWeek.Monday;

            var currentWeekMonday = today.AddDays(-daysFromMonday);

            // Previous week's Monday
            var weekStartDate = currentWeekMonday.AddDays(-7);

            var sites = await _siteService.GetAllSitesList();
            foreach (var site in sites)
            {
                if (site.NumberOfWorkingDays <= 0)
                    continue;

                var GetDateTime = _siteService.GetDateTime(site.TimeZone);
                var weekEndDate = weekStartDate.AddDays(site.NumberOfWorkingDays - 1);
                var systemUserId = await _userService.GetUserIdByRole(site.Id, "Site Super Admin");

                // Generate working dates based on site's working days
                var siteWorkingDates = Enumerable.Range(0, site.NumberOfWorkingDays)
                    .Select(i => weekStartDate.AddDays(i).Date)
                    .ToList();

                var activeEmployees = await _employeeService.GetAllActiveEmployeeListForDropdown(site.Id);

                var siteLeaveSchedules = site.CheckLeavesForTimesheetReminder
                    ? await _leaveScheduleService.GetAllLeaveEvents(site.Id, weekStartDate, weekEndDate)
                    : new List<LeaveSchedules>();

                var pendingTimesheetEmployees = new List<Employee>();

                foreach (var employee in activeEmployees)
                {
                    // Dates for which employee is expected to submit a timesheet
                    var requiredTimesheetDates = new List<DateTime>(siteWorkingDates);

                    if (site.CheckLeavesForTimesheetReminder)
                    {
                        // Remove site holidays
                        requiredTimesheetDates.RemoveAll(date => siteLeaveSchedules.Any(x => x.Date.HasValue && x.Date.Value.Date == date));

                        // Get approved leave dates
                        var employeeApprovedLeaveDates = await _employeeLeaveService.GetEmployeeApprovedLeaveDates(employee.Id, weekStartDate, weekEndDate);

                        // Remove employee leave dates
                        requiredTimesheetDates.RemoveAll(date => employeeApprovedLeaveDates.Any(x => x.Date.Date == date));
                    }

                    // No working dates remain
                    if (!requiredTimesheetDates.Any())
                        continue;

                    bool hasPendingTimesheet = false;

                    foreach (var requiredDate in requiredTimesheetDates)
                    {
                        bool hasTimesheet = await _timesheetService.HasTimesheetForDate(
                            site.Id,
                            employee.Id,
                            requiredDate
                        );

                        if (!hasTimesheet)
                        {
                            hasPendingTimesheet = true;
                            break;
                        }
                    }

                    if (hasPendingTimesheet)
                    {
                        pendingTimesheetEmployees.Add(employee);
                    }
                }

                if (!pendingTimesheetEmployees.Any())
                    continue;

                var timesheetApprovers = await _userService.GetUsersByRole(site.Id, "timesheet approver");

                foreach (var timesheetApprover in timesheetApprovers)
                {
                    var masterNotificationData =
                        await _masterNotificationService.GetMasterNotificationByNumber(
                            site.Id,
                            "PendingWeeklyTimesheets1",
                            timesheetApprover.Id
                        );

                    if (masterNotificationData == null)
                        continue;

                    string message = masterNotificationData.Message
                        .Replace("[Week End Date]", weekEndDate.ToString("MM/dd/yyyy"))
                        .Replace("[Employee Names]", string.Join(", ", pendingTimesheetEmployees.Select(x => x.Person.FullName)));

                    _notificationService.AddNotification(
                        site.Id,
                        masterNotificationData.Title,
                        message,
                        masterNotificationData.Type,
                        systemUserId,
                        null,
                        "/timesheet",
                        timesheetApprover.Id,
                        systemUserId,
                        GetDateTime
                    );
                }
            }

        }
    }
}
