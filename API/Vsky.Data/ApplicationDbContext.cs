using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vsky.Models;
using Vsky.Models.Expens;

namespace Vsky.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser,
        ApplicationRole,
        string,
        ApplicationUserClaim,
        ApplicationUserRole,
        ApplicationUserLogin,
        ApplicationRoleClaim,
        ApplicationUserToken>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Address
        public virtual DbSet<Address> Address { get; set; }

        // Person
        public virtual DbSet<Person> Person { get; set; }
        public virtual DbSet<PersonSitesMapping> PersonSitesMapping { get; set; }

        // Sites
        public virtual DbSet<Site> Sites { get; set; }
        public virtual DbSet<SitesRoles> SitesRoles { get; set; }
        public virtual DbSet<SitesEmailNotifications> SitesEmailNotifications { get; set; }
        public virtual DbSet<SitesEmailNotificationsPermission> SitesEmailNotificationsPermission { get; set; }
        public virtual DbSet<SitesModules> SitesModules { get; set; }
        public virtual DbSet<SitesModulesMenus> SitesModulesMenus { get; set; }
        public virtual DbSet<SitesModulesMenusPermissions> SitesModulesMenusPermissions { get; set; }
        public virtual DbSet<SitesModifiedLogs> SitesModifiedLogs { get; set; }
        public virtual DbSet<TimeZones> TimeZone { get; set; }

        // Websites
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<Website_Demos> Website_Demos { get; set; }
        public virtual DbSet<Website_Demo_Modules> Website_Demo_Modules { get; set; }

        // Country & States
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<StateProvince> StateProvinces { get; set; }

        // Dropdowns
        public virtual DbSet<DropDown> DropDowns { get; set; }
        public virtual DbSet<DropDownType> DropDownTypes { get; set; }

        // Email Notification
        public virtual DbSet<EmailAccount> EmailAccounts { get; set; }
        public virtual DbSet<MessageTemplate> MessageTemplates { get; set; }

        // System Notification
        public virtual DbSet<MasterNotification> MasterNotification { get; set; }
        public virtual DbSet<Notification> Notification { get; set; }
        public virtual DbSet<NotificationDetails> NotificationDetails { get; set; }
        public virtual DbSet<NotificationPermissions> NotificationPermissions { get; set; }

        // Common
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<SetReminder> SetReminders { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<Picture> Pictures { get; set; }
        public virtual DbSet<Tags> Tags { get; set; }
        public virtual DbSet<FilePathDetails> FilePathDetails { get; set; }
        public virtual DbSet<Notes> Notes { get; set; }

        // Employee
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<EmployeeType> EmployeeType { get; set; }
        public virtual DbSet<EmployeeStatus> EmployeeStatus { get; set; }
        public virtual DbSet<EmployeeDepartment> EmployeeDepartment { get; set; }
        public virtual DbSet<EmployeeDesignation> EmployeeDesignation { get; set; }
        public virtual DbSet<EmployeeClientLocation> EmployeeClientLocation { get; set; }
        public virtual DbSet<EmployeeOrgLocation> EmployeeOrgLocation { get; set; }
        public virtual DbSet<EmployeeLeave> EmployeeLeave { get; set; }
        public virtual DbSet<EmployeeOrgStructure> EmployeeOrgStructure { get; set; }
        public virtual DbSet<EmployeeOrgStructureDesignationMapping> EmployeeOrgStructureDesignationMapping { get; set; }

        // Employee Training
        public virtual DbSet<TrainingPortal> TrainingPortal { get; set; }
        public virtual DbSet<Training_Portal_Mapping> Training_Portal_Mapping { get; set; }

        // Project
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<ProjectsMessages> ProjectMessages { get; set; }
        public virtual DbSet<ProjectFiles> ProjectFiles { get; set; }
        public virtual DbSet<ProjectUserMapping> ProjectUserMapping { get; set; }
        public virtual DbSet<ProjectTags> Project_Tags { get; set; }
        public virtual DbSet<ProjectPinned> ProjectPinned { get; set; }
        public virtual DbSet<ProjectColor> ProjectColor { get; set; }
        public virtual DbSet<ProjectEmployeeMapping> ProjectEmployeeMappings { get; set; }
        public virtual DbSet<ProjectTaskRelatedMapping> ProjectTaskRelatedMapping { get; set; }

        // Project Release Tracking
        public virtual DbSet<ProjectReleaseTracking> ProjectReleaseTracking { get; set; }
        public virtual DbSet<ProjectReleaseTrackingReqPlanTaskIssueMapping> ProjectReleaseTrackingReqPlanTaskIssueMapping { get; set; }
        public virtual DbSet<ProjectReleaseTrackingStatusLog> ProjectReleaseTrackingStatusLog { get; set; }

        // Project Modules
        public virtual DbSet<ProjectModule> ProjectModules { get; set; }
        public virtual DbSet<ProjectModulesUserMapping> ProjectModulesUserMapping { get; set; }
        public virtual DbSet<ProjectModuleFiles> ProjectModuleFiles { get; set; }

        // Project Task
        public virtual DbSet<ProjectTask> ProjectTasks { get; set; }
        public virtual DbSet<ProjectTaskFiles> ProjectTaskFiles { get; set; }
        public virtual DbSet<ProjectTask_Tags> ProjectTask_Tags { get; set; }
        public virtual DbSet<ProjectTaskStatusLog> ProjectTaskStatusLog { get; set; }

        // Project Activities
        public virtual DbSet<ProjectActivity> ProjectActivities { get; set; }
        public virtual DbSet<ProjectActivityFiles> ProjectActivityFiles { get; set; }

        // ProjectSwimLanes
        public virtual DbSet<ProjectSwimLanes> ProjectSwimLanes { get; set; }
        public virtual DbSet<ProjectSwimLanesList> ProjectSwimLanesList { get; set; }
        public virtual DbSet<ProjectSwimLanesListsTasks> ProjectSwimLanesListsTasks { get; set; }
        public virtual DbSet<MasterProjectSwimlaneLists> MasterProjectSwimlaneLists { get; set; }


        // Project Weekly/Monthly Plan 
        public virtual DbSet<ProjectWeeklyPlan> ProjectWeeklyPlan { get; set; }
        public virtual DbSet<ProjectWeeklyPlanDates> ProjectWeeklyPlanDates { get; set; }
        public virtual DbSet<ProjectWeeklyPlanDatesLines> ProjectWeeklyPlanDatesLines { get; set; }
        public virtual DbSet<EmployeeEstimatedHoursDropdownList> EmployeeEstimatedHoursDropdownList { get; set; }

        // CRM - Leads
        public virtual DbSet<Lead> Leads { get; set; }
        public virtual DbSet<LeadActivityLogs> LeadActivityLogs { get; set; }
        public virtual DbSet<LeadActivities> LeadActivities { get; set; }
        public virtual DbSet<LeadStages> LeadStages { get; set; }
        public virtual DbSet<LeadActivities> LeadActivitiess { get; set; }
        public virtual DbSet<LeadUserGroupMapping> LeadUserGroupMapping { get; set; }

        // CRM - Company
        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<CompanyContacts> CompanyContact { get; set; }

        // CRM - Customer
        public virtual DbSet<CompanyClients> CompanyClients { get; set; }
        public virtual DbSet<CustomerFiles> CustomerFiles { get; set; }
        public virtual DbSet<CustomerFilesLines> CustomerFilesLines { get; set; }

        // CRM
        public virtual DbSet<SalesPerson> SalesPerson { get; set; }

        // My Work - Daily Planner
        public virtual DbSet<DailyPlanner> DailyPlanner { get; set; }
        public virtual DbSet<DailyPlannerLine> DailyPlannerLines { get; set; }

        // My Work - Timesheet
        public virtual DbSet<Timesheet> Timesheet { get; set; }
        public virtual DbSet<TimesheetLines> TimesheetLines { get; set; }
        public virtual DbSet<TimesheetAISummary> TimesheetAISummary { get; set; }

        // My Work -> Movement Register
        public virtual DbSet<MovementRegister> MovementRegister { get; set; }
        public virtual DbSet<MovementRegisterDetails> MovementRegisterDetails { get; set; }

        // Leave
        public virtual DbSet<LeaveCredit> LeaveCredit { get; set; }
        public virtual DbSet<LeaveRules> LeaveRules { get; set; }
        public virtual DbSet<LeaveRuleLines> LeaveRuleLines { get; set; }
        public virtual DbSet<LeaveSchedules> LeaveSchedules { get; set; }

        // SDLC -> Test Plan & Test Cases
        public virtual DbSet<TestPlan> TestPlan { get; set; }
        public virtual DbSet<TestCase> TestCase { get; set; }

        // SDLC -> Issues
        public virtual DbSet<Issue> Issue { get; set; }
        public virtual DbSet<ImageMigration> ImageMigration { get; set; }
        public virtual DbSet<IssueStatusChangedLog> IssueStatusChangedLog { get; set; }
        public virtual DbSet<IssueActivity> IssueActivity { get; set; }

        // SDLC -> Requirement
        public virtual DbSet<Requirement> Requirement { get; set; }
        public virtual DbSet<RequirementGroup> RequirementGroup { get; set; }
        public virtual DbSet<RequirementChangeLog> RequirementChangeLog { get; set; }
        public virtual DbSet<RequirementTags> RequirementTags { get; set; }
        public virtual DbSet<RequirementPinned> RequirementPinned { get; set; }

        //Marketing -> Ad Post & Job Create
        public virtual DbSet<AdPost> AdPost { get; set; }
        public virtual DbSet<AdPostChannel> AdPostChannel { get; set; }
        public virtual DbSet<AdPostingStatus> AdPostingStatus { get; set; }
        public virtual DbSet<JobCreate> JobCreate { get; set; }

        //Master Modules
        public virtual DbSet<Modules> Modules { get; set; }
        public virtual DbSet<ModulesMenus> ModulesMenus { get; set; }

        //Infrastructure Ver 1
        public virtual DbSet<Server> Server { get; set; }
        public virtual DbSet<ServerPayments> ServerPayments { get; set; }
        public virtual DbSet<Domain> Domain { get; set; }
        public virtual DbSet<DomainAttributes> DomainAttributes { get; set; }
        public virtual DbSet<TimeInTimeOut> TimeInTimeOut { get; set; }
        public virtual DbSet<TimeInTimeOutBreakDetail> TimeInTimeOutBreakDetail { get; set; }

        // Infrastructure -> Inventory Ver 1
        public virtual DbSet<Inventory> Inventory { get; set; }
        public virtual DbSet<InventoryAssignment> InventoryAssignment { get; set; }
        public virtual DbSet<InventoryItemType> InventoryItemType { get; set; }

        // Infrastructure Ver 2
        public virtual DbSet<InfraAccount> InfraAccount { get; set; }
        public virtual DbSet<InfraAccountServices> InfraAccountServices { get; set; }
        public virtual DbSet<InfraAccountServicesPriceHistory> InfraAccountServicesPriceHistory { get; set; }
        public virtual DbSet<InfraProjectServices> InfraProjectServices { get; set; }
        public virtual DbSet<InfraFTP> InfraFTP { get; set; }
        public virtual DbSet<InfraFTPsProjectInstanceMapping> InfraFTPsProjectInstanceMapping { get; set; }
        public virtual DbSet<InfraDatabase> InfraDatabase { get; set; }
        public virtual DbSet<InfraDatabaseProjectInstanceMapping> InfraDatabaseProjectInstanceMapping { get; set; }
        public virtual DbSet<InfraProjectInstance> InfraProjectInstance { get; set; }
        public virtual DbSet<InfraProjectInstanceRole> InfraProjectInstanceRole { get; set; }
        public virtual DbSet<InfraProjectInstanceRoleUsers> InfraProjectInstanceRoleUsers { get; set; }

        // ReportSettings
        public virtual DbSet<ReportSettings> ReportSettings { get; set; }
        public virtual DbSet<ReportSettingsDetails> ReportSettingsDetails { get; set; }
        public virtual DbSet<ReportUserMapping> ReportUserMapping { get; set; }
        public virtual DbSet<ReportRoleGroupMapping> ReportRoleGroupMapping { get; set; }

        //Candidiate
        public virtual DbSet<Candidate> Candidate { get; set; }
        public virtual DbSet<CandidateActivities> CandidateActivities { get; set; }
        public virtual DbSet<CandidateFeedback> CandidateFeedback { get; set; }
        public virtual DbSet<CandidateDepartments> CandidateDepartments { get; set; }
        public virtual DbSet<CandidateNotes> CandidateNotes { get; set; }

        //Help Desk
        public virtual DbSet<HelpDeskTopic> HelpDeskTopic { get; set; }
        public virtual DbSet<HelpDeskTopicQuestions> HelpDeskTopicQuestions { get; set; }
        public virtual DbSet<HelpDesk> HelpDesk { get; set; }
        public virtual DbSet<HelpDeskStatusLog> HelpDeskStatusLog { get; set; }
        public virtual DbSet<HelpDeskFiles> HelpDeskFiles { get; set; }
        public virtual DbSet<HelpDeskReminderLog> HelpDeskReminderLog { get; set; }
        public virtual DbSet<HelpDeskEmailRepliesMapping> HelpDeskEmailRepliesMapping { get; set; }

        // Twilio EmailReplies
        public virtual DbSet<TwilioEmailSettings> TwilioEmailSettings { get; set; }
        public virtual DbSet<EmailReplies> EmailReplies { get; set; }

        //Expense
        public virtual DbSet<Expense> Expense { get; set; }
        public virtual DbSet<Expense_Lines> ExpenceLines { get; set; }
        public virtual DbSet<Expense_Files> Expense_Files { get; set; }
        public virtual DbSet<ExpenseVendors> ExpenseVendors { get; set; }
        public virtual DbSet<Expense_BankAccounts> ExpenseBankAccounts { get; set; }
        public virtual DbSet<ExpenseVendorBankAccounts> ExpenseVendorBankAccounts { get; set; }
        public virtual DbSet<ExpenseAdvanceRequestFiles> ExpenseAdvanceRequestFiles { get; set; }
        public virtual DbSet<Expense_Advance_Requests> Expense_Advance_Requests { get; set; }
        public virtual DbSet<ExpensePurchaseRequestFiles> ExpensePurchaseRequestFiles { get; set; }
        public virtual DbSet<Expense_Purchase_Requests> Expense_Purchase_Requests { get; set; }

        //Item
        public virtual DbSet<ItemCategory> ItemCategory { get; set; }
        public virtual DbSet<ItemSubcategory> ItemSubcategory { get; set; }
        public virtual DbSet<ItemSubCategoryAttributes> ItemSubCategoryAttributes { get; set; }
        public virtual DbSet<ItemSubCategoryAttributesValues> ItemSubCategoryAttributesValues { get; set; }

        //Site Items
        public virtual DbSet<SitesItemSubCategoryAttributesMapping> SitesItemSubCategoryAttributesMapping { get; set; }
        public virtual DbSet<SitesItems> SitesItems { get; set; }
        public virtual DbSet<SitesItemsAttributes> SitesItemsAttributes { get; set; }

        //SOP Templates
        public virtual DbSet<SOPTemplate> SOPTemplate { get; set; }
        public virtual DbSet<SOPTemplateSection> SOPTemplateSection { get; set; }
        public virtual DbSet<SOPTemplateSectionItems> SOPTemplateSectionItems { get; set; }

        //SOP Assignments
        public virtual DbSet<SOPAssignment> SOPAssignment { get; set; }
        public virtual DbSet<SOPAssignmentResponse> SOPAssignmentResponse { get; set; }
        public virtual DbSet<SOPAssignmentResponseEvidences> SOPAssignmentResponseEvidences { get; set; }

        //SOP Process
        public virtual DbSet<SOPProcess> SOPProcess { get; set; }
        public virtual DbSet<SOPProcessStatusLog> SOPProcessStatusLog { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region ASPNet

            builder.Entity<ApplicationUser>(b =>
            {
                b.HasMany(e => e.Claims).WithOne(e => e.User).HasForeignKey(uc => uc.UserId).IsRequired();
                b.HasMany(e => e.Logins).WithOne(e => e.User).HasForeignKey(ul => ul.UserId).IsRequired();
                b.HasMany(e => e.Tokens).WithOne(e => e.User).HasForeignKey(ut => ut.UserId).IsRequired();
                b.HasMany(e => e.UserRoles).WithOne(e => e.User).HasForeignKey(ur => ur.UserId).IsRequired();
            });

            builder.Entity<ApplicationRole>(b =>
            {
                b.HasMany(e => e.UserRoles).WithOne(e => e.Role).HasForeignKey(ur => ur.RoleId).IsRequired();
                b.HasMany(e => e.RoleClaims).WithOne(e => e.Role).HasForeignKey(rc => rc.RoleId).IsRequired();
            });

            builder.Entity<ApplicationUserRole>(b =>
            {
                b.HasKey(ur => new { ur.UserId, ur.RoleId, ur.SiteId });
                b.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).IsRequired();
                b.HasOne(ur => ur.User).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.UserId).IsRequired();
                b.HasOne(ur => ur.Site).WithMany().HasForeignKey(ur => ur.SiteId);
            });

            builder.Entity<Log>(entity =>
            {
                entity.ToTable("Log");
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.IpAddress).HasMaxLength(200);
                entity.Property(e => e.ShortMessage).IsRequired();

                entity.Ignore(e => e.LogLevel);

                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            });

            #endregion

            #region Sites

            builder.Entity<Site>(entity =>
            {
                entity.ToTable("Sites");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.AddressId).HasMaxLength(450);
                entity.Property(e => e.PersonId).HasMaxLength(450);
                entity.Property(e => e.UserName).HasMaxLength(200);

                entity.Property(e => e.SiteLogoId).HasMaxLength(450);
                entity.Property(e => e.SiteLogoPath);
                entity.Property(e => e.SiteFaviconId).HasMaxLength(450);
                entity.Property(e => e.SiteFaviconPath);
                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(e => e.Address).WithMany().HasForeignKey(e => e.AddressId);
                entity.HasOne(e => e.Person).WithMany().HasForeignKey(e => e.PersonId);
            });

            builder.Entity<SitesRoles>(entity =>
            {
                entity.ToTable("SitesRoles");
                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(200);
                entity.Property(e => e.RoleId).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);

                entity.HasOne(d => d.Site).WithMany(d => d.SitesRoles).HasForeignKey(d => d.SiteId);
                entity.HasOne(e => e.ApplicationRole).WithMany().HasForeignKey(e => e.RoleId);
            });

            builder.Entity<SitesEmailNotifications>(entity =>
            {
                entity.ToTable("Sites_EmailNotifications");

                entity.HasOne(e => e.Sites).WithMany().HasForeignKey(e => e.SiteId);
                entity.HasOne(e => e.MessageTemplate).WithMany().HasForeignKey(e => e.MessageTemplateId);
            });

            builder.Entity<SitesEmailNotificationsPermission>(entity =>
            {
                entity.ToTable("Sites_EmailNotifications_Permission");

                entity.HasOne(e => e.Sites).WithMany().HasForeignKey(e => e.SiteId);
                entity.HasOne(e => e.SitesEmailNotifications).WithMany().HasForeignKey(e => e.SiteEmailNotificationId);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            });

            builder.Entity<SitesModules>(entity =>
            {
                entity.ToTable("SitesModules");
                entity.Property(e => e.SiteId).HasMaxLength(450);
                entity.Property(e => e.ModuleId).HasMaxLength(450);
                entity.Property(e => e.Active);
                entity.Property(e => e.SortOrder);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
                entity.HasOne(d => d.Modules).WithMany(m => m.SiteModules).HasForeignKey(d => d.ModuleId);
            });

            builder.Entity<SitesModulesMenus>(entity =>
            {
                entity.ToTable("SitesModulesMenus");
                entity.Property(e => e.SiteId).HasMaxLength(450);
                entity.Property(e => e.SiteModuleId).HasMaxLength(450);
                entity.Property(e => e.MenuId).HasMaxLength(450);
                entity.Property(e => e.Active);
                entity.Property(e => e.SortOrder);
                entity.Property(e => e.SetAsLanding);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
                entity.HasOne(d => d.SitesModules).WithMany(x => x.SitesModulesMenus).HasForeignKey(d => d.SiteModuleId);
                entity.HasOne(d => d.ModulesMenus).WithMany(m => m.SitesModulesMenus).HasForeignKey(d => d.MenuId);
            });

            builder.Entity<SitesModulesMenusPermissions>(entity =>
            {
                entity.ToTable("SitesModulesMenusPermissions");
                entity.Property(e => e.SiteId).HasMaxLength(450);
                entity.Property(e => e.SiteRoleId).HasMaxLength(450);
                entity.Property(e => e.SiteModuleMenuId).HasMaxLength(450);
                entity.Property(e => e.IsShowMenu);
                entity.Property(e => e.IsManage);
                entity.Property(e => e.IsView);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
                entity.HasOne(d => d.SitesRoles).WithMany().HasForeignKey(d => d.SiteRoleId);
                entity.HasOne(d => d.SitesModulesMenus).WithMany(x => x.SitesModulesMenusPermissions).HasForeignKey(d => d.SiteModuleMenuId);
            });

            builder.Entity<SitesModifiedLogs>(entity =>
            {
                entity.ToTable("SitesModifiedLogs");

                entity.HasOne(d => d.user).WithMany().HasForeignKey(d => d.LastModifiedBy);
                entity.HasOne(d => d.Sites).WithMany().HasForeignKey(d => d.SiteId);
            });

            builder.Entity<TimeZones>(entity =>
            {
                entity.ToTable("TimeZone");

            });

            #endregion

            #region Country State 

            builder.Entity<Country>(entity =>
            {
                entity.ToTable("Country");

                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ThreeLetterIsoCode).HasMaxLength(3);
                entity.Property(e => e.TwoLetterIsoCode).HasMaxLength(2);
            });

            builder.Entity<StateProvince>(entity =>
            {
                entity.ToTable("StateProvince");

                entity.Property(e => e.Abbreviation).HasMaxLength(100);
                entity.Property(e => e.CountryId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

                entity.HasOne(d => d.Country).WithMany(p => p.StateProvinces).HasForeignKey(d => d.CountryId);
            });

            #endregion

            #region Address

            builder.Entity<Address>(entity =>
            {
                entity.ToTable("Address");
                entity.Property(e => e.City).HasMaxLength(128);
                entity.Property(e => e.ZipCode).HasMaxLength(6);
                entity.Property(e => e.AddressLine1).HasMaxLength(500);
                entity.Property(e => e.AddressLine2).HasMaxLength(500);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.CreatedById).IsRequired().HasPrecision(450);
                entity.Property(e => e.UpdatedById).HasPrecision(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
                entity.Property(e => e.CountryId).HasMaxLength(450);
                entity.Property(e => e.StateProvinceId).HasMaxLength(450);

                entity.HasOne(d => d.AddressCountry).WithMany().HasForeignKey(d => d.CountryId);
                entity.HasOne(d => d.AddressStateProvince).WithMany().HasForeignKey(d => d.StateProvinceId);
            });

            #endregion

            #region Person

            builder.Entity<PersonSitesMapping>(entity =>
            {
                entity.ToTable("PersonSitesMapping");

                entity.HasOne(d => d.CreatedBy).WithMany().HasForeignKey(d => d.CreatedById);
                entity.HasOne(d => d.UpdatedBy).WithMany().HasForeignKey(d => d.UpdatedById);
            });

            builder.Entity<Person>(entity =>
            {
                entity.ToTable("Person");

                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MiddleName).HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PictureId).HasMaxLength(450);
                entity.Property(e => e.AddressId).HasMaxLength(450);
                entity.Property(e => e.AddressTypeId).HasMaxLength(450);
                entity.Property(e => e.GenderId).HasMaxLength(450);
                entity.Property(e => e.PrimaryPhoneNumber).IsRequired().HasMaxLength(30);
                entity.Property(e => e.PrimaryEmailAddress).IsRequired().HasMaxLength(300);
                entity.Property(e => e.IdentifiedDate).HasPrecision(6);
                entity.Property(e => e.DOB);
                entity.Property(e => e.IdentifiedById).HasMaxLength(450);
                entity.Property(e => e.IdentificationNote).HasMaxLength(500);
                entity.Property(e => e.Relation);
                entity.Property(e => e.RelationFullName);
                entity.Property(e => e.PhoneNumber);
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.BgColor).HasMaxLength(50);
                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
                entity.Property(e => e.Title).HasMaxLength(200); // from BS
                entity.Property(e => e.ProfileLink).HasMaxLength(500); // from BS

                entity.HasOne(e => e.Address).WithMany().HasForeignKey(e => e.AddressId);
                entity.HasOne(e => e.AddressType).WithMany().HasForeignKey(e => e.AddressTypeId);
                entity.HasOne(e => e.Picture).WithMany().HasForeignKey(e => e.PictureId);
                entity.HasOne(e => e.Gender).WithMany().HasForeignKey(e => e.GenderId);
                entity.HasOne(e => e.IdentifiedBy).WithMany().HasForeignKey(e => e.IdentifiedById);
            });

            #endregion

            #region Email

            builder.Entity<EmailAccount>(entity =>
            {
                entity.ToTable("EmailAccount");
                entity.Property(e => e.DisplayName).HasMaxLength(255);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Host).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(255);

                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
            });

            builder.Entity<MessageTemplate>(entity =>
            {
                entity.ToTable("MessageTemplate");
                entity.Property(e => e.BccEmailAddresses).HasMaxLength(200);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Subject).HasMaxLength(1000);

                entity.HasOne(d => d.EmailAccount).WithMany().HasForeignKey(d => d.EmailAccountId);
            });

            #endregion

            #region Pictures (Media)

            builder.Entity<Picture>(entity =>
            {
                entity.ToTable("Picture");

                entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SeoFilename).HasMaxLength(300);
                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
            });

            #endregion

            #region Common

            builder.Entity<SetReminder>(entity =>
            {
                entity.ToTable("SetReminder");
                entity.Property(e => e.LeadActivityLogId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ReminderAfterDays);
                entity.Property(e => e.Time).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Note);
                entity.Property(e => e.ReminderDateTime);
                entity.Property(e => e.IsMailStatus);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.CreatedById).IsRequired().HasPrecision(450);
                entity.Property(e => e.UpdatedById).HasPrecision(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.LeadActivityLogs).WithMany().HasForeignKey(d => d.LeadActivityLogId);
            });

            builder.Entity<Notes>(entity =>
            {
                entity.ToTable("Notes");
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.CreatedById);

            });

            builder.Entity<ImageMigration>(entity =>
            {
                entity.ToTable("ImageMigration");
            });

            builder.Entity<FilePathDetails>(entity =>
            {
                entity.ToTable("FilePathDetails");
                entity.Property(e => e.ModuleId).HasMaxLength(450);
                entity.Property(e => e.ModuleName).HasMaxLength(250);
                entity.Property(e => e.FilePath).HasMaxLength(250);
                entity.Property(e => e.FileName).HasMaxLength(250);
                entity.Property(e => e.Note).HasMaxLength(250);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Requirement).WithMany(d => d.FilePathDetails).HasForeignKey(d => d.ModuleId);
            });

            #endregion

            #region Master Tags

            builder.Entity<Tags>(entity =>
            {
                entity.ToTable("Tags");
                entity.Property(e => e.Name).HasMaxLength(450);
                entity.Property(e => e.BgColor).HasMaxLength(50);
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.SortOrder);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);

                entity.HasOne(d => d.CreatedBy).WithMany().HasForeignKey(d => d.CreatedById);
                entity.HasOne(d => d.Sites).WithMany().HasForeignKey(d => d.SiteId);
            });

            #endregion

            #region Master - Modules & Menus

            builder.Entity<Modules>(entity =>
            {
                entity.ToTable("Modules");
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
            });

            builder.Entity<ModulesMenus>(entity =>
            {
                entity.ToTable("ModulesMenus");
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Modules).WithMany(m => m.ModulesMenus).HasForeignKey(d => d.ModuleId);
            });

            #endregion

            #region Masters -> System Notifications

            builder.Entity<MasterNotification>(entity =>
            {
                entity.ToTable("Master_Notification");
                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);

                entity.HasOne(e => e.Site).WithMany().HasForeignKey(e => e.SiteId);
            });

            builder.Entity<NotificationPermissions>(entity =>
            {
                entity.ToTable("NotificationPermissions");
                entity.Property(e => e.NotificationId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.AspNetUserId).IsRequired().HasMaxLength(450);

                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.AspNetUserId);
            });

            #endregion

            #region DropDowns

            builder.Entity<DropDown>(entity =>
            {
                entity.ToTable("DropDown");

                entity.Property(e => e.DropDownTypeId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.DropDownValue).IsRequired().HasMaxLength(128);
                entity.Property(e => e.DropDownText).HasMaxLength(128);
                entity.Property(e => e.Description);
                entity.Property(e => e.BgColor).HasMaxLength(50);
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.SortOrder);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.DropDownType).WithMany().HasForeignKey(d => d.DropDownTypeId);
            });

            builder.Entity<DropDownType>(entity =>
            {
                entity.ToTable("DropDownType");

                entity.Property(e => e.Type).IsRequired().HasMaxLength(128);
                entity.Property(e => e.SiteId).HasMaxLength(128);
                entity.Property(e => e.DisplayName).HasMaxLength(128);
                entity.Property(e => e.GroupName).HasMaxLength(128);
                entity.Property(e => e.ModuleName).HasMaxLength(128);
                entity.Property(e => e.SortOrder);
                entity.Property(e => e.IsAlphabeticalOrNumerical);
                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
            });

            #endregion

            #region CRM -> Company

            builder.Entity<Company>(entity =>
            {
                entity.ToTable("Company");

                entity.Property(e => e.AlternativeEmailAddress).HasMaxLength(64);
                entity.Property(e => e.CreatedById);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.ServiceProviderDate).HasPrecision(6);
                entity.Property(e => e.ComapnyCreatedDate).HasPrecision(6);
                entity.Property(e => e.EmailAddress);
                entity.Property(e => e.ServiceProvidedDetails);
                entity.Property(e => e.ProductDetails);
                entity.Property(e => e.Name);
                entity.Property(e => e.AlternativePhoneNumber);
                entity.Property(e => e.PhoneNumber);
                entity.Property(e => e.UpdatedById);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
                entity.Property(e => e.Website);
                entity.Property(e => e.AddressId);
                entity.Property(e => e.ProfileLink).HasMaxLength(500); // from BS
            });

            builder.Entity<CompanyContacts>(entity =>
            {
                entity.ToTable("CompanyContacts");
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.CreatedById).HasPrecision(450);
                entity.Property(e => e.UpdatedById).HasPrecision(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
                entity.Property(e => e.CompanyId).HasMaxLength(450);
            });

            #endregion

            #region CRM -> Customers

            builder.Entity<CompanyClients>(entity =>
            {
                entity.ToTable("CompanyCustomers");
                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.AssignedTo).WithMany().HasForeignKey(d => d.AssignedToId);
            });

            builder.Entity<CustomerFiles>(entity =>
            {
                entity.ToTable("CustomerFiles");
                entity.Property(e => e.CustomerId).HasMaxLength(450);
                entity.Property(e => e.SiteId).HasMaxLength(450);
                entity.Property(e => e.Year);
                entity.Property(e => e.SortOrder);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.CreatedById).HasPrecision(450);
                entity.Property(e => e.UpdatedById).HasPrecision(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(c => c.CompanyClients).WithMany().HasForeignKey(c => c.CustomerId);
                entity.HasOne(c => c.CreatedBy).WithMany().HasForeignKey(c => c.CreatedById);
                entity.HasOne(c => c.UpdatedBy).WithMany().HasForeignKey(c => c.UpdatedById);
            });

            builder.Entity<CustomerFilesLines>(entity =>
            {
                entity.ToTable("CustomerFilesLines");
                entity.HasOne(d => d.CustomerFiles).WithMany(m => m.CustomerFilesLines).HasForeignKey(d => d.CustomerFileId);
            });

            #endregion

            #region CRM - Leads

            builder.Entity<Lead>(entity =>
            {
                entity.ToTable("Lead");
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.CreatedById).IsRequired().HasPrecision(450);
                entity.Property(e => e.UpdatedById).HasPrecision(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
                entity.Property(e => e.LeadSourceId).IsRequired().HasMaxLength(450);

                entity.HasOne(d => d.Person).WithMany().HasForeignKey(d => d.PersonId);
                entity.HasOne(d => d.LeadSources).WithMany().HasForeignKey(d => d.LeadSourceId);
                entity.HasOne(d => d.Client).WithMany().HasForeignKey(d => d.ClientId);
                entity.HasOne(d => d.Company).WithMany().HasForeignKey(d => d.CompanyId);

            });

            builder.Entity<LeadActivityLogs>(entity =>
            {
                entity.ToTable("LeadActivityLogs");
                entity.Property(e => e.LeadsId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.LeadActivityId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.LeadStageId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ActivityDate).HasPrecision(6);
                entity.Property(e => e.ActivityNote).HasMaxLength(500);
                entity.Property(e => e.IsFutureActivity);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.CreatedById).IsRequired().HasPrecision(450);
                entity.Property(e => e.UpdatedById).HasPrecision(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Leads).WithMany(d => d.LeadActivityLogs).HasForeignKey(d => d.LeadsId);
                entity.HasOne(d => d.LeadActivity).WithMany(d => d.LeadActivityLogss).HasForeignKey(d => d.LeadActivityId);
                entity.HasOne(d => d.LeadStage).WithMany(d => d.LeadActivityLogss).HasForeignKey(d => d.LeadStageId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.CreatedById);
            });

            builder.Entity<LeadStages>(entity =>
            {
                entity.ToTable("LeadStages");

                entity.Property(e => e.StageName).HasMaxLength(200);
                entity.Property(e => e.StageDescription);
            });

            builder.Entity<LeadActivities>(entity =>
            {
                entity.ToTable("LeadActivities");

                entity.Property(e => e.ActivityName).HasMaxLength(200);
                entity.Property(e => e.ActivityDescription);
            });

            builder.Entity<LeadUserGroupMapping>(entity =>
            {
                entity.ToTable("Lead_User_Group_Mapping");
            });

            builder.Entity<SalesPerson>(entity =>
            {
                entity.ToTable("SalesPerson");
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
                entity.HasOne(d => d.Employee).WithMany().HasForeignKey(d => d.EmployeeId);

            });

            #endregion

            #region User Management -> Employee

            builder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employee");

                entity.Property(e => e.SiteId).HasMaxLength(450);
                entity.Property(e => e.PersonId).HasMaxLength(450);
                entity.Property(e => e.EmployeeCode).HasMaxLength(32);
                entity.Property(e => e.OfficialEmail).HasMaxLength(50);
                entity.Property(e => e.EmergencyContactName).HasMaxLength(50);
                entity.Property(e => e.EmergencyPhoneNo).HasMaxLength(32);
                entity.Property(e => e.SameASPermanentAddress).HasMaxLength(32);
                entity.Property(e => e.AddressId).HasMaxLength(450);
                entity.Property(e => e.AadhaarCardNo).HasMaxLength(12);
                entity.Property(e => e.PanCardNo).HasMaxLength(10);
                entity.Property(e => e.EPFUANNo).HasMaxLength(12);
                entity.Property(e => e.JoiningDate).HasPrecision(6);
                entity.Property(e => e.ReleaseDate).HasPrecision(6);
                entity.Property(e => e.EducationDetail).HasMaxLength(500);
                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
            });

            builder.Entity<EmployeeType>(entity =>
            {
                entity.ToTable("EmployeeType");

                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.EmployeeTypeId).HasMaxLength(450);
                entity.Property(e => e.StartDate).HasPrecision(6);
                entity.Property(e => e.EndDate).HasPrecision(6);
                entity.Property(e => e.Duration).HasMaxLength(50);
                entity.Property(e => e.Note).HasMaxLength(300);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Employee).WithMany(m => m.EmployeeType).HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.EmployeeTypeDropdown).WithMany().HasForeignKey(d => d.EmployeeTypeId);
            });

            builder.Entity<EmployeeStatus>(entity =>
            {
                entity.ToTable("EmployeeStatus");

                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.EmployeeStatusId).HasMaxLength(450);
                entity.Property(e => e.StartDate).HasPrecision(6);
                entity.Property(e => e.EndDate).HasPrecision(6);
                entity.Property(e => e.Duration).HasMaxLength(50);
                entity.Property(e => e.Note).HasMaxLength(300);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Employee).WithMany(m => m.EmployeeStatuses).HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.Status).WithMany().HasForeignKey(d => d.EmployeeStatusId);
            });

            builder.Entity<EmployeeDepartment>(entity =>
            {
                entity.ToTable("EmployeeDepartment");

                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.EmployeeDepartmentId).HasMaxLength(450);
                entity.Property(e => e.StartDate).HasPrecision(6);
                entity.Property(e => e.EndDate).HasPrecision(6);
                entity.Property(e => e.Duration).HasMaxLength(50);
                entity.Property(e => e.Note).HasMaxLength(300);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Employee).WithMany(m => m.EmployeeDepartment).HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.Department).WithMany().HasForeignKey(d => d.EmployeeDepartmentId);
            });

            builder.Entity<EmployeeDesignation>(entity =>
            {
                entity.ToTable("EmployeeDesignation");

                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.EmployeeDesignationId).HasMaxLength(450);
                entity.Property(e => e.ShiftId).HasMaxLength(450);
                entity.Property(e => e.LeaveApproverId).HasMaxLength(450);
                entity.Property(e => e.StartDate).HasPrecision(6);
                entity.Property(e => e.EndDate).HasPrecision(6);
                entity.Property(e => e.Duration).HasMaxLength(50);
                entity.Property(e => e.Note).HasMaxLength(300);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Employee).WithMany(m => m.EmployeeDesignation).HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.Designation).WithMany().HasForeignKey(d => d.EmployeeDesignationId);
                entity.HasOne(d => d.Shift).WithMany().HasForeignKey(d => d.ShiftId);
                entity.HasOne(d => d.LeaveApprover).WithMany().HasForeignKey(d => d.LeaveApproverId);
            });

            builder.Entity<EmployeeOrgLocation>(entity =>
            {
                entity.ToTable("EmployeeOrgLocation");

                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.OrgLocationId).HasMaxLength(450);
                entity.Property(e => e.StartDate).HasPrecision(6);
                entity.Property(e => e.EndDate).HasPrecision(6);
                entity.Property(e => e.Duration).HasMaxLength(50);
                entity.Property(e => e.Note).HasMaxLength(300);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Employee).WithMany(m => m.EmployeeOrgLocation).HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.OrgLocation).WithMany().HasForeignKey(d => d.OrgLocationId);
            });

            builder.Entity<EmployeeClientLocation>(entity =>
            {
                entity.ToTable("EmployeeClientLocation");

                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.ClientLocationId).HasMaxLength(450);
                entity.Property(e => e.StartDate).HasPrecision(6);
                entity.Property(e => e.EndDate).HasPrecision(6);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Employee).WithMany(m => m.EmployeeClientLocation).HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.ClientLocation).WithMany().HasForeignKey(d => d.ClientLocationId);
            });

            builder.Entity<EmployeeOrgStructure>(entity =>
            {
                entity.ToTable("EmployeeOrgStructure");

                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ManagerId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.DepartmentId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.RoleId).HasMaxLength(450);
                entity.Property(e => e.Year).IsRequired();
                entity.Property(e => e.Level);
                entity.Property(e => e.SortOrder);
                entity.Property(e => e.Responsibilities);
                entity.Property(e => e.Color);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).IsRequired().HasPrecision(6);

                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId);
                entity.HasOne(e => e.Manager).WithMany().HasForeignKey(e => e.ManagerId);
                entity.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId);
                entity.HasOne(e => e.Role).WithMany().HasForeignKey(e => e.RoleId);
                entity.HasOne(e => e.Site).WithMany().HasForeignKey(e => e.SiteId);
            });

            builder.Entity<EmployeeOrgStructureDesignationMapping>(entity =>
            {
                entity.ToTable("EmployeeOrgStructure_Designation_Mapping");

                entity.HasOne(d => d.EmployeeOrgStructure).WithMany(d => d.EmployeeOrgStructureDesignationMapping).HasForeignKey(d => d.EmployeeOrgStructureId);
                entity.HasOne(d => d.EmployeeDesignation).WithMany().HasForeignKey(d => d.EmployeeDesignationId);

            });

            builder.Entity<EmployeeLeave>(entity =>
            {
                entity.ToTable("EmployeeLeave");

                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.LeaveApproverId).HasMaxLength(450);
                entity.Property(e => e.FileId).HasMaxLength(450);
                entity.Property(e => e.LeaveStatusId).HasMaxLength(450);
                entity.Property(e => e.LeaveCategoryId).HasMaxLength(450);
                entity.Property(e => e.FromDate).HasPrecision(6);
                entity.Property(e => e.ToDate).HasPrecision(6);
                entity.Property(e => e.NoofLeaves).HasColumnType("decimal(18, 1)");
                entity.Property(e => e.Reason);
                entity.Property(e => e.ApproverNote);
                entity.Property(e => e.HRNote);
                entity.Property(e => e.IsPaidLeave);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Employee).WithMany().HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.LeaveApprover).WithMany().HasForeignKey(d => d.LeaveApproverId);
                entity.HasOne(d => d.File).WithMany().HasForeignKey(d => d.FileId);
                entity.HasOne(d => d.LeaveStatuses).WithMany().HasForeignKey(d => d.LeaveStatusId);
                entity.HasOne(d => d.LeaveCategories).WithMany().HasForeignKey(d => d.LeaveCategoryId);
            });

            #endregion

            #region Organization Management ->  Department

            builder.Entity<Department>(entity =>
            {
                entity.ToTable("Department");

                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
            });

            #endregion

            #region Organization Management -> Leave

            builder.Entity<LeaveCredit>(entity =>
            {
                entity.ToTable("LeaveCredit");

                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.LeaveTypeId).HasMaxLength(450);
                entity.Property(e => e.CasualLeaves).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.SickLeaves).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.CreditReason).HasMaxLength(250);
                entity.Property(e => e.LeaveCreditsforYear);
                entity.Property(e => e.Note).HasMaxLength(500);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Employee).WithMany().HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.LeaveTypes).WithMany().HasForeignKey(d => d.LeaveTypeId);
            });

            builder.Entity<LeaveRules>(entity =>
            {
                entity.ToTable("LeaveRules");

                entity.Property(e => e.Year);
                entity.Property(e => e.IsGenerated);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
            });

            builder.Entity<LeaveRuleLines>(entity =>
            {
                entity.ToTable("LeaveRuleLines");

                entity.Property(e => e.LeaveRuleId).HasMaxLength(450);
                entity.Property(e => e.EmploymentTypeId).HasMaxLength(450);
                entity.Property(e => e.CasualLeaves).HasColumnType("decimal(16, 2)");
                entity.Property(e => e.SickLeaves).HasColumnType("decimal(16, 2)");

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.LeaveRules).WithMany(m => m.LeaveRuleLinesList).HasForeignKey(d => d.LeaveRuleId);
                entity.HasOne(d => d.EmploymentType).WithMany().HasForeignKey(d => d.EmploymentTypeId);
            });

            builder.Entity<LeaveSchedules>(entity =>
            {
                entity.ToTable("LeaveSchedule");
                entity.Property(e => e.SiteId).HasMaxLength(450);
                entity.Property(e => e.LeaveRuleId).HasMaxLength(450);
                entity.Property(e => e.Date).HasPrecision(6);
                entity.Property(e => e.Title);
                entity.Property(e => e.Description);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);

                entity.HasOne(d => d.LeaveRules).WithMany().HasForeignKey(d => d.LeaveRuleId);
                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
            });

            #endregion

            #region Organization Management -> Training Portal

            builder.Entity<TrainingPortal>(entity =>
            {
                entity.ToTable("TrainingPortal");
                entity.Property(e => e.TrainingFileId).HasMaxLength(450);
                entity.Property(e => e.SiteId).HasMaxLength(450);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Description);
                entity.Property(e => e.Url);
                entity.Property(e => e.Description);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.File).WithMany().HasForeignKey(d => d.TrainingFileId);
            });

            builder.Entity<Training_Portal_Mapping>(entity =>
            {
                entity.ToTable("Training_Portal_Mapping");
                entity.Property(e => e.TrainingId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.EmployeeDesignationId).HasMaxLength(450);

                entity.HasOne(d => d.Training).WithMany(d => d.TrainingPortalMappings).HasForeignKey(d => d.TrainingId);
                entity.HasOne(d => d.EmployeeDesignationType).WithMany().HasForeignKey(d => d.EmployeeDesignationId);
            });

            #endregion

            #region Project Management -> Project

            builder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(128);
                entity.Property(e => e.Code).HasMaxLength(64);
                entity.Property(e => e.CustomerId).HasMaxLength(450);
                entity.Property(e => e.CompanyContactId).HasMaxLength(450);
                entity.Property(e => e.EndDate).HasPrecision(6);
                entity.Property(e => e.GoLiveDate).HasPrecision(6);
                entity.Property(e => e.ProjectPriorityId).HasMaxLength(450);
                entity.Property(e => e.ProjectStatusId).HasMaxLength(450);
                entity.Property(e => e.ProjectTypeId).HasMaxLength(450);
                entity.Property(e => e.StartDate).HasPrecision(6);
                entity.Property(e => e.Website).HasMaxLength(128);
                entity.Property(e => e.SiteId).HasMaxLength(450);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.CompanyContact).WithMany().HasForeignKey(d => d.CompanyContactId);
                entity.HasOne(d => d.ProjectPriority).WithMany().HasForeignKey(d => d.ProjectPriorityId);
                entity.HasOne(d => d.ProjectType).WithMany().HasForeignKey(d => d.ProjectTypeId);
                entity.HasOne(d => d.ProjectCoordinator).WithMany().HasForeignKey(d => d.ProjectCoordinatorId);
                entity.HasOne(d => d.PlanApprover).WithMany().HasForeignKey(d => d.PlanApproverId);
                entity.HasOne(d => d.ProjectCategories).WithMany().HasForeignKey(d => d.ProjectCategoryId);
                entity.HasOne(d => d.ProjectCategoriesSubCategories).WithMany().HasForeignKey(d => d.ProjectSubcategoryId);
            });

            builder.Entity<ProjectFiles>(entity =>
            {
                entity.ToTable("Project_Files");

                entity.Property(e => e.ProjectId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.FileId).IsRequired().HasMaxLength(450);
                entity.HasOne(d => d.File).WithMany().HasForeignKey(d => d.FileId);
                entity.HasOne(d => d.CreatedBy).WithMany().HasForeignKey(d => d.CreatedById);
            });

            builder.Entity<ProjectEmployeeMapping>(entity =>
            {
                entity.ToTable("Project_Employee_Mapping");

                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.ProjectId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.EmployeeDesignationId).HasMaxLength(256);
                entity.Property(e => e.ProductivityFactor);

                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);


                entity.HasOne(d => d.Employee).WithMany(p => p.ProjectEmployeeMappings).HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.Project).WithMany(p => p.ProjectEmployeeMappings).HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.VW_Project).WithMany(p => p.ProjectEmployeeMappings).HasForeignKey(d => d.ProjectId);

                entity.HasOne(d => d.EmployeeRoleDropdown).WithMany().HasForeignKey(d => d.EmployeeDesignationId);
            });

            builder.Entity<ProjectTags>(entity =>
            {
                entity.ToTable("Project_Tags");
                entity.Property(e => e.ProjectId).HasMaxLength(450);
                entity.Property(e => e.TagId).HasMaxLength(450);
                entity.Property(e => e.AspNetUserId).HasMaxLength(450);


                entity.HasOne(d => d.Projects).WithMany(m => m.ProjectTags).HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.Tags).WithMany().HasForeignKey(d => d.TagId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.AspNetUserId);
            });

            builder.Entity<ProjectPinned>(entity =>
            {
                entity.ToTable("Project_Pinned");

                entity.HasOne(d => d.Project).WithMany(m => m.ProjectPinned).HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.AspNetUserId);
            });

            builder.Entity<ProjectColor>(entity =>
            {
                entity.ToTable("Project_Color");

                entity.HasOne(d => d.Project).WithMany(m => m.ProjectColors).HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.AspNetUserId);
            });

            builder.Entity<ProjectsMessages>(entity =>
            {
                entity.ToTable("ProjectsMessages");

                entity.HasOne(e => e.Sites).WithMany().HasForeignKey(e => e.SiteId);
                entity.HasOne(e => e.Project).WithMany(x => x.ProjectsMessages).HasForeignKey(e => e.ProjectId);
                entity.HasOne(e => e.SentByUser).WithMany().HasForeignKey(e => e.SentBy);
            });

            builder.Entity<ProjectUserMapping>(entity =>
            {
                entity.ToTable("Project_User_Mapping");

                entity.Property(e => e.ProjectId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.AspNetUserId).HasMaxLength(256);
                entity.Property(e => e.FullAccess);
                entity.Property(e => e.ViewOnly);
                entity.Property(e => e.Notes);

                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Project).WithMany(p => p.ProjectUserMappings).HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.VW_Project).WithMany(p => p.ProjectUserMappings).HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.AspNetUserId);
            });

            #endregion
             
            #region Project Release Tracking

            builder.Entity<ProjectReleaseTracking>(entity =>
            {
                entity.ToTable("ProjectReleaseTracking");

                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
                entity.HasOne(d => d.Project).WithMany().HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.InfraInstance).WithMany().HasForeignKey(d => d.InfraInstanceId);
                entity.HasOne(d => d.DeploymentOwner).WithMany().HasForeignKey(d => d.DeploymentOwnerId);
                entity.HasOne(d => d.Approver).WithMany().HasForeignKey(d => d.ApproverId);
                entity.HasOne(d => d.Tester).WithMany().HasForeignKey(d => d.TesterId);
                entity.HasOne(d => d.ReleaseType).WithMany().HasForeignKey(d => d.ReleaseTypeId);
                entity.HasOne(d => d.CreatedBy).WithMany().HasForeignKey(d => d.CreatedById);
                entity.HasOne(d => d.UpdatedBy).WithMany().HasForeignKey(d => d.UpdatedById);
            });

            builder.Entity<ProjectReleaseTrackingReqPlanTaskIssueMapping>(entity =>
            {
                entity.ToTable("ProjectReleaseTracking_ReqPlanTaskIssueMapping");

                entity.HasOne(d => d.ReleaseTracking).WithMany().HasForeignKey(d => d.ReleaseTrackingId);
            });

            builder.Entity<ProjectReleaseTrackingStatusLog>(entity =>
            {
                entity.ToTable("ProjectReleaseTracking_StatusLog");

                entity.HasOne(d => d.ReleaseTracking).WithMany(x => x.ProjectReleaseTrackingStatusLog).HasForeignKey(d => d.ReleaseTrackingId);
                entity.HasOne(d => d.Status).WithMany().HasForeignKey(d => d.StatusId);
            });
            #endregion

            #region Project Management -> Project Modules

            builder.Entity<ProjectModule>(entity =>
            {
                entity.ToTable("ProjectModules");

                entity.Property(e => e.CloseDate).HasPrecision(6);
                entity.Property(e => e.StartDate).HasPrecision(6);
                entity.Property(e => e.EndDate).HasPrecision(6);
                entity.Property(e => e.ProjectId).IsRequired();
                entity.Property(e => e.TargetDate).HasPrecision(6);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
                entity.Property(e => e.ProjectModuleNumber);
                entity.Property(e => e.Description);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
                entity.Property(e => e.ProjectModuleStatusId).HasMaxLength(450);
                entity.Property(e => e.ProjectModuleTypeId).HasMaxLength(450);

                entity.HasOne(d => d.ProjectModuleStatus).WithMany().HasForeignKey(d => d.ProjectModuleStatusId);
                entity.HasOne(d => d.ProjectModuleType).WithMany().HasForeignKey(d => d.ProjectModuleTypeId);
            });

            builder.Entity<ProjectModulesUserMapping>(entity =>
            {
                entity.ToTable("ProjectModules_User_Mapping");

                entity.Property(e => e.ProjectModuleId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.AspNetUserId).HasMaxLength(256);
                entity.Property(e => e.FullAccess);
                entity.Property(e => e.ViewOnly);
                entity.Property(e => e.Notes);

                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.ProjectModule).WithMany(x => x.ProjectModulesUserMappings).HasForeignKey(d => d.ProjectModuleId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.AspNetUserId);
            });

            builder.Entity<ProjectModuleFiles>(entity =>
            {
                entity.ToTable("Project_Module_Files");

                entity.Property(e => e.ProjectModuleId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.FileId).IsRequired().HasMaxLength(450);
                entity.HasOne(d => d.File).WithMany().HasForeignKey(d => d.FileId);
            });

            #endregion

            #region Project Management -> Project Tasks

            builder.Entity<ProjectTask>(entity =>
            {
                entity.ToTable("ProjectTasks");

                entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
                entity.Property(e => e.ProjectId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.AssignedToId).HasMaxLength(450);
                entity.Property(e => e.DueDate).HasPrecision(6);
                entity.Property(e => e.EndDate).HasPrecision(6);
                entity.Property(e => e.EstimateTime).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.SortOrder).HasColumnType("decimal(18, 3)");
                entity.Property(e => e.PriorityId).HasMaxLength(450);
                entity.Property(e => e.StartDate).HasPrecision(6);
                entity.Property(e => e.StatusId).HasMaxLength(450);
                entity.Property(e => e.TaskMonth).HasPrecision(6);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
                entity.Property(e => e.ProjectModuleId).HasMaxLength(450);

                entity.HasOne(d => d.Priority).WithMany().HasForeignKey(d => d.PriorityId);
                entity.HasOne(d => d.Status).WithMany().HasForeignKey(d => d.StatusId);
                entity.HasOne(d => d.Project).WithMany(m => m.ProjectTasks).HasForeignKey(d => d.ProjectId);
            });

            builder.Entity<ProjectTaskStatusLog>(entity =>
            {
                entity.ToTable("ProjectTaskStatusLog");

                entity.Property(e => e.TaskId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.StatusId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.StatusChangedBy).HasMaxLength(450);
                entity.Property(e => e.StatusChangedDate).HasPrecision(6);

                entity.HasOne(d => d.Task).WithMany(m => m.ProjectTaskStatusLog).HasForeignKey(d => d.TaskId);
                entity.HasOne(d => d.Status).WithMany().HasForeignKey(d => d.StatusId);
                entity.HasOne(d => d.StatusChangedByEmployee).WithMany().HasForeignKey(d => d.StatusChangedBy);
            });

            builder.Entity<ProjectTaskRelatedMapping>(entity =>
            {
                entity.ToTable("ProjectTask_RelatedMapping");

                entity.Property(e => e.TaskId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.IssueId).HasMaxLength(450);
                entity.Property(e => e.RequirementId).HasMaxLength(450);

                entity.HasOne(d => d.ProjectTask).WithMany(x => x.ProjectTaskRelatedMappings).HasForeignKey(d => d.TaskId);
                entity.HasOne(d => d.Issue).WithMany(x => x.ProjectTaskRelatedMappings).HasForeignKey(d => d.IssueId);
                entity.HasOne(d => d.Requirement).WithMany(x => x.ProjectTaskRelatedMappings).HasForeignKey(d => d.RequirementId);
            });

            builder.Entity<ProjectTask_Tags>(entity =>
            {
                entity.ToTable("ProjectTask_Tags");
                entity.Property(e => e.TaskId).HasMaxLength(450);
                entity.Property(e => e.TagId).HasMaxLength(450);
                entity.Property(e => e.AspNetUserId).HasMaxLength(450);

                entity.HasOne(d => d.Task).WithMany(m => m.ProjectTask_Tags).HasForeignKey(d => d.TaskId);
                entity.HasOne(d => d.Tags).WithMany().HasForeignKey(d => d.TagId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.AspNetUserId);
            });

            builder.Entity<ProjectTaskFiles>(entity =>
            {
                entity.ToTable("Project_Task_Files");

                entity.Property(e => e.ProjectTaskId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.FileId).IsRequired().HasMaxLength(450);
                entity.HasOne(d => d.File).WithMany().HasForeignKey(d => d.FileId);
            });

            #endregion

            #region Project Task Activities

            builder.Entity<ProjectActivity>(entity =>
            {
                entity.ToTable("ProjectActivities");

                entity.Property(e => e.ProjectId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.TaskId).HasMaxLength(450);
                entity.Property(e => e.ProjectModuleId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ActivityStatusId).HasMaxLength(450);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Description).HasMaxLength(450);
                entity.Property(e => e.AssignedToId).HasMaxLength(450);
                entity.Property(e => e.DueDate).HasPrecision(6);
                entity.Property(e => e.StartDate).HasPrecision(6);
                entity.Property(e => e.EndDate).HasPrecision(6);
                entity.Property(e => e.EstimateHours);
                entity.Property(e => e.TargetMonth).HasPrecision(6);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Project).WithMany(m => m.ProjectActivities).HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.AssignedTo).WithMany(m => m.ProjectActivities).HasForeignKey(d => d.AssignedToId);
                entity.HasOne(d => d.Task).WithMany(m => m.ProjectActivities).HasForeignKey(d => d.TaskId);
                entity.HasOne(d => d.ProjectModule).WithMany(m => m.ProjectActivities).HasForeignKey(d => d.ProjectModuleId);
                entity.HasOne(d => d.ActivityStatus).WithMany().HasForeignKey(d => d.ActivityStatusId);
                entity.HasOne(d => d.CreatedByUser).WithMany().HasForeignKey(d => d.CreatedById);
                entity.HasOne(d => d.UpdatedByUser).WithMany().HasForeignKey(d => d.UpdatedById);

            });

            builder.Entity<ProjectActivityFiles>(entity =>
            {
                entity.ToTable("Project_Activity_Files");

                entity.Property(e => e.ProjectActivityId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.FileId).IsRequired().HasMaxLength(450);
                entity.HasOne(d => d.File).WithMany().HasForeignKey(d => d.FileId);
            });

            #endregion

            #region Project Target Plan - Weekly/Monthly Planner

            builder.Entity<ProjectWeeklyPlan>(entity =>
            {
                entity.ToTable("Project_WeeklyPlan");
                entity.HasOne(e => e.Project).WithMany(x => x.ProjectWeeklyPlans).HasForeignKey(e => e.ProjectId);
            });

            builder.Entity<ProjectWeeklyPlanDates>(entity =>
            {
                entity.ToTable("Project_WeeklyPlan_Dates");
                entity.HasOne(d => d.ProjectWeeklyPlan).WithMany(d => d.ProjectWeeklyPlanDates).HasForeignKey(d => d.ProjectWeeklyPlanId);
                entity.HasOne(e => e.PlanType).WithMany().HasForeignKey(e => e.PlanTypeId);
            });

            builder.Entity<ProjectWeeklyPlanDatesReqTaskIssueMapping>(entity =>
            {
                entity.ToTable("Project_WeeklyPlan_Dates_ReqTaskIssueMapping");
                entity.HasOne(d => d.ProjectWeeklyPlanDates).WithMany(d => d.ProjectWeeklyPlanDatesReqTaskIssueMapping).HasForeignKey(d => d.ProjectWeeklyPlanDatesId);
                entity.HasOne(e => e.Requirement).WithMany().HasForeignKey(e => e.RequirementId);
                entity.HasOne(e => e.Task).WithMany(m => m.ProjectWeeklyPlanDatesReqTaskIssueMappingList).HasForeignKey(e => e.TaskId);
                entity.HasOne(e => e.Issue).WithMany().HasForeignKey(e => e.IssueId);
            });

            builder.Entity<ProjectWeeklyPlanDatesLines>(entity =>
            {
                entity.ToTable("Project_WeeklyPlan_Dates_Lines");
                entity.HasOne(d => d.ProjectWeeklyPlanDates).WithMany(d => d.ProjectWeeklyPlanDatesLines).HasForeignKey(d => d.ProjectWeeklyPlanDatesId);
            });

            builder.Entity<ProjectWeeklyPlanDatesLinesAssignedTo>(entity =>
            {
                entity.ToTable("Project_WeeklyPlan_Dates_Lines_AssignedTo");
                entity.HasOne(d => d.ProjectWeeklyPlanDatesLine).WithMany(d => d.ProjectWeeklyPlanDatesLinesAssignedTo).HasForeignKey(d => d.ProjectWeeklyPlanDatesLineId);
            });

            builder.Entity<EmployeeEstimatedHoursDropdownList>().HasNoKey(); // SP result has no primary key

            #endregion

            #region Project WorkBoard

            builder.Entity<ProjectSwimLanes>(entity =>
            {
                entity.ToTable("Project_SwimLanes");
                entity.HasOne(d => d.Project).WithMany(d => d.ProjectSwimLanes).HasForeignKey(d => d.ProjectId);
                entity.HasOne(e => e.SwimlaneType).WithMany().HasForeignKey(e => e.SwimlaneTypeId);
            });

            builder.Entity<ProjectSwimLanesList>(entity =>
            {
                entity.ToTable("Project_Swimlanes_Lists");
                entity.HasOne(d => d.ProjectSwimlane).WithMany(d => d.ProjectSwimLanesList).HasForeignKey(d => d.ProjectSwimlaneId);
            });

            builder.Entity<ProjectSwimLanesListsTasks>(entity =>
            {
                entity.ToTable("Project_Swimlanes_Lists_Tasks");
                entity.HasOne(d => d.ProjectSwimlaneList).WithMany(d => d.ProjectSwimLanesListsTasks).HasForeignKey(d => d.ProjectSwimlaneListId);
            });

            builder.Entity<MasterProjectSwimlaneLists>(entity =>
            {
                entity.ToTable("Master_ProjectSwimlane_Lists");
                entity.HasOne(e => e.SwimlaneType).WithMany().HasForeignKey(e => e.SwimlaneTypeId);
            });

            #endregion

            #region My Work -> Daily Planner

            builder.Entity<DailyPlanner>(entity =>
            {
                entity.ToTable("DailyPlanner");

                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.DailyPlannerDate).IsRequired();
                entity.Property(e => e.IsForwordedToTimesheet).IsRequired();
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Sites).WithMany().HasForeignKey(d => d.SiteId);
                entity.HasOne(d => d.Employee).WithMany().HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.CreatedById);
            });

            builder.Entity<DailyPlannerLine>(entity =>
            {
                entity.ToTable("DailyPlannerLines");
                entity.Property(e => e.DailyPlannerId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Hours).IsRequired();
                entity.Property(e => e.ProjectId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ProjectModuleId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ProjectTaskId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.DailyPlanner).WithMany(p => p.DailyPlannerLines).HasForeignKey(d => d.DailyPlannerId);
                entity.HasOne(d => d.Project).WithMany().HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.ProjectModule).WithMany().HasForeignKey(d => d.ProjectModuleId);
                entity.HasOne(d => d.ProjectTask).WithMany().HasForeignKey(d => d.ProjectTaskId);
                entity.HasOne(d => d.ProjectActivity).WithMany().HasForeignKey(d => d.ProjectActivityId);

            });

            #endregion

            #region My Work -> Timesheet

            builder.Entity<Timesheet>(entity =>
            {
                entity.ToTable("Timesheet");
                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.TimesheetDate).IsRequired().HasPrecision(6);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(e => e.Sites).WithMany().HasForeignKey(e => e.SiteId);
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.CreatedById);
            });

            builder.Entity<TimesheetLines>(entity =>
            {
                entity.ToTable("TimesheetLines");
                entity.Property(e => e.TimesheetId).HasMaxLength(450);
                entity.Property(e => e.ProjectId).HasMaxLength(450);
                entity.Property(e => e.ProjectModuleId).HasMaxLength(450);
                entity.Property(e => e.ProjectTaskId).HasMaxLength(450);
                entity.Property(e => e.ProjectActivityId).HasMaxLength(450);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Hours).IsRequired().HasColumnType("decimal(18, 2)");
                entity.Property(e => e.BillableHours).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.BillableCreatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(e => e.Timesheet).WithMany(e => e.TimesheetLines).HasForeignKey(e => e.TimesheetId);
                entity.HasOne(e => e.Project).WithMany(m => m.TimesheetLine).HasForeignKey(e => e.ProjectId);
                entity.HasOne(e => e.ProjectModule).WithMany().HasForeignKey(e => e.ProjectModuleId);
                entity.HasOne(d => d.Task).WithMany().HasForeignKey(d => d.ProjectTaskId);
                entity.HasOne(d => d.ProjectActivity).WithMany().HasForeignKey(d => d.ProjectActivityId);
                 entity.HasOne(d => d.BillableCreatedBy).WithMany().HasForeignKey(d => d.BillableCreatedById);
            });

            builder.Entity<TimesheetAISummary>(entity =>
            {
                entity.ToTable("Timesheet_AISummary");

                entity.HasOne(e => e.Task).WithMany().HasForeignKey(e => e.TaskId);
                entity.HasOne(e => e.TimesheetLine).WithMany().HasForeignKey(e => e.TimesheetLineId);
            });

            #endregion

            #region My Work -> TimeIn-TimeOut

            builder.Entity<TimeInTimeOut>(entity =>
            {
                entity.ToTable("TimeInTimeOut");
                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.WorkHoursApprovalStatusId).HasMaxLength(450);
                entity.Property(e => e.TimeIn);
                entity.Property(e => e.TimeOut);
                entity.Property(e => e.TimeInDate).HasPrecision(6);
                entity.Property(e => e.TimeOutDate).HasPrecision(6);
                entity.Property(e => e.ActualHours);
                entity.Property(e => e.TotalHours);
                entity.Property(e => e.TotalBreak);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
                entity.HasOne(d => d.Employee).WithMany().HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.WorkHoursApprovalStatus).WithMany().HasForeignKey(d => d.WorkHoursApprovalStatusId);
                entity.HasOne(d => d.CreatedBy).WithMany().HasForeignKey(d => d.CreatedById);
            });

            builder.Entity<TimeInTimeOutBreakDetail>(entity =>
            {
                entity.ToTable("TimeInTimeOutBreakDetail");
                entity.Property(e => e.TimeInTimeOutId).HasMaxLength(450);
                entity.Property(e => e.TotalBreak);
                entity.Property(e => e.BreakReason).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.TimeInTimeOut).WithMany(d => d.TimeInTimeOutBreakDetailList).HasForeignKey(d => d.TimeInTimeOutId);
            });

            #endregion

            #region My Work -> Movement Register

            builder.Entity<MovementRegister>(entity =>
            {
                entity.ToTable("MovementRegister");

                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Date).HasPrecision(6); ;
                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Sites).WithMany().HasForeignKey(d => d.SiteId);
            });

            builder.Entity<MovementRegisterDetails>(entity =>
            {
                entity.ToTable("MovementRegisterDetails");

                entity.Property(e => e.MomentRegisterId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ApproverById).HasMaxLength(450);
                entity.Property(e => e.TypeId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Message);
                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.MomentRegisters).WithMany(p => p.MovementRegisterDetails).HasForeignKey(d => d.MomentRegisterId);
                entity.HasOne(d => d.Employees).WithMany().HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.Approvers).WithMany().HasForeignKey(d => d.ApproverById);
                entity.HasOne(d => d.BreakTime).WithMany().HasForeignKey(d => d.BreakTimeId);
            });

            #endregion

            #region SDLC -> Test Plan & Test Cases

            builder.Entity<TestPlan>(entity =>
            {
                entity.ToTable("TestPlan");
                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.PlanMakerId).HasMaxLength(450);
                entity.Property(e => e.PlanReviewerId).HasMaxLength(450);
                entity.Property(e => e.ProjectId).HasMaxLength(450);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Description);
                entity.Property(e => e.TestPlanNumber);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(e => e.Site).WithMany().HasForeignKey(e => e.SiteId);
                entity.HasOne(e => e.PlanMaker).WithMany().HasForeignKey(e => e.PlanMakerId);
                entity.HasOne(e => e.PlanReviewer).WithMany().HasForeignKey(e => e.PlanReviewerId);
                entity.HasOne(d => d.Project).WithMany(m => m.TestPlans).HasForeignKey(d => d.ProjectId);
            });

            builder.Entity<TestCase>(entity =>
            {
                entity.ToTable("TestCase");
                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.PlanId).HasMaxLength(450);
                entity.Property(e => e.StatusId).HasMaxLength(450);
                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.ProjectId).HasMaxLength(450);
                entity.Property(e => e.TestedBy).HasMaxLength(450);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Description);
                entity.Property(e => e.Steps);
                entity.Property(e => e.ExpectedResult);
                entity.Property(e => e.ActualResult);
                entity.Property(e => e.TestResult);
                entity.Property(e => e.TestedDate).IsRequired().HasPrecision(6);
                entity.Property(e => e.TestCaseNumber);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(e => e.Site).WithMany().HasForeignKey(e => e.SiteId);
                entity.HasOne(e => e.TestPlan).WithMany(m => m.TestCases).HasForeignKey(e => e.PlanId);
                entity.HasOne(e => e.Status).WithMany().HasForeignKey(e => e.StatusId);
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId);
                entity.HasOne(d => d.Project).WithMany().HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.CreatedByUser).WithMany().HasForeignKey(d => d.CreatedById);
                entity.HasOne(d => d.TestedByEmployee).WithMany().HasForeignKey(d => d.TestedBy);
            });

            #endregion

            #region SDLC -> Issue

            builder.Entity<Issue>(entity =>
            {
                entity.ToTable("Issue");
                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ProjectId).HasMaxLength(450);
                entity.Property(e => e.ProjectModuleId).HasMaxLength(450);
                entity.Property(e => e.PriorityId).HasMaxLength(450);
                entity.Property(e => e.StatusId).HasMaxLength(450);
                entity.Property(e => e.TypeId).HasMaxLength(450);
                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.TestCaseId).HasMaxLength(450);
                entity.Property(e => e.ClosedBy).HasMaxLength(450);
                entity.Property(e => e.LastModifiedBy).HasMaxLength(450);
                entity.Property(e => e.ReportedById).HasMaxLength(450);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Description);
                entity.Property(e => e.LastUpdatedDate).HasPrecision(6);
                entity.Property(e => e.CloseDate).HasPrecision(6);
                entity.Property(e => e.DueDate).HasPrecision(6);
                entity.Property(e => e.IsTaskCreated);
                entity.Property(e => e.IssueNumber);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(e => e.Site).WithMany().HasForeignKey(e => e.SiteId);
                entity.HasOne(d => d.Project).WithMany(m => m.Issue).HasForeignKey(d => d.ProjectId);
                entity.HasOne(e => e.ProjectModule).WithMany().HasForeignKey(e => e.ProjectModuleId);
                entity.HasOne(e => e.Priority).WithMany().HasForeignKey(e => e.PriorityId);
                entity.HasOne(e => e.Status).WithMany().HasForeignKey(e => e.StatusId);
                entity.HasOne(e => e.Type).WithMany().HasForeignKey(e => e.TypeId);
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId);
                entity.HasOne(e => e.TestCase).WithMany().HasForeignKey(e => e.TestCaseId);
                entity.HasOne(e => e.ClosedByEmployee).WithMany().HasForeignKey(e => e.ClosedBy);
                entity.HasOne(e => e.LastModifiedByEmployee).WithMany().HasForeignKey(e => e.LastModifiedBy);
                entity.HasOne(e => e.ReportedBy).WithMany().HasForeignKey(e => e.ReportedById);
            });

            builder.Entity<IssueStatusChangedLog>(entity =>
            {
                entity.ToTable("IssueStatusChangedLog");

                entity.Property(e => e.IssueId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.StatusId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.StatusChangedBy).HasMaxLength(450);
                entity.Property(e => e.StatusChangedDate).HasPrecision(6);

                entity.HasOne(d => d.Issue).WithMany(m => m.IssueStatusChangedLog).HasForeignKey(d => d.IssueId);
                entity.HasOne(d => d.Status).WithMany().HasForeignKey(d => d.StatusId);
                entity.HasOne(d => d.StatusChangedByEmployee).WithMany().HasForeignKey(d => d.StatusChangedBy);
            });

            builder.Entity<IssueActivity>(entity =>
            {
                entity.ToTable("IssueActivity");

                entity.Property(e => e.IssueId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ActivityName).HasMaxLength(200);
                entity.Property(e => e.DueDate).HasPrecision(6);
                entity.Property(e => e.PriorityId).HasMaxLength(450);
                entity.Property(e => e.AssignedTo).HasMaxLength(450);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Issue).WithMany(m => m.IssueActivity).HasForeignKey(d => d.IssueId);
                entity.HasOne(d => d.Priority).WithMany().HasForeignKey(d => d.PriorityId);
                entity.HasOne(d => d.AssignedToEmployee).WithMany().HasForeignKey(d => d.AssignedTo);
            });

            #endregion

            #region SDLC -> Requirement

            builder.Entity<Requirement>(entity =>
            {
                entity.ToTable("Requirement");
                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ProjectId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.RequirementGroupId).HasMaxLength(450);
                entity.Property(e => e.ProjectModuleId).HasMaxLength(450);
                entity.Property(e => e.ApprovalStatus).HasMaxLength(450);
                entity.Property(e => e.RequirementEnteredBy).HasMaxLength(450);
                entity.Property(e => e.StatusId).HasMaxLength(450);
                entity.Property(e => e.IdentifiedUserType).HasMaxLength(450);
                entity.Property(e => e.IdentifiedEmployeeId).HasMaxLength(450);
                entity.Property(e => e.IdentifiedCustomerId).HasMaxLength(450);
                entity.Property(e => e.PriorityId).HasMaxLength(450);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Description);
                entity.Property(e => e.IdentifiedDate).HasPrecision(6);
                entity.Property(e => e.CloseDate).HasPrecision(6);
                entity.Property(e => e.Notes);
                entity.Property(e => e.EditingStatus);
                entity.Property(e => e.RequirementNumber);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(e => e.Site).WithMany().HasForeignKey(e => e.SiteId);
                entity.HasOne(d => d.Project).WithMany(x => x.Requirement).HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.ProjectModule).WithMany().HasForeignKey(d => d.ProjectModuleId);
                entity.HasOne(d => d.RequirementGroup).WithMany().HasForeignKey(d => d.RequirementGroupId);
                entity.HasOne(d => d.ApprovalStatusDropDown).WithMany().HasForeignKey(d => d.ApprovalStatus);
                entity.HasOne(d => d.RequirementEntered).WithMany().HasForeignKey(d => d.RequirementEnteredBy);
                entity.HasOne(d => d.RequirementType).WithMany().HasForeignKey(d => d.RequirementTypeId);
                entity.HasOne(d => d.Status).WithMany().HasForeignKey(d => d.StatusId);
                entity.HasOne(d => d.UserType).WithMany().HasForeignKey(d => d.IdentifiedUserType);
                entity.HasOne(d => d.Employee).WithMany().HasForeignKey(d => d.IdentifiedEmployeeId);
                entity.HasOne(d => d.Customer).WithMany().HasForeignKey(d => d.IdentifiedCustomerId);
                entity.HasOne(d => d.Priority).WithMany().HasForeignKey(d => d.PriorityId);
            });

            builder.Entity<RequirementGroup>(entity =>
            {
                entity.ToTable("RequirementGroup");
                entity.Property(e => e.ProjectId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Description);
                entity.Property(e => e.RequirementGroupNumber);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Project).WithMany().HasForeignKey(d => d.ProjectId);
            });

            builder.Entity<RequirementChangeLog>(entity =>
            {
                entity.ToTable("RequirementChangeLog");
                entity.Property(e => e.RequirementId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.RequirementLogDate).HasPrecision(6);
                entity.Property(e => e.Description);
                entity.Property(e => e.RequirementName).HasMaxLength(200);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Employee).WithMany().HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.Requirement).WithMany(d => d.RequirementChangeLog).HasForeignKey(d => d.RequirementId);
            });

            builder.Entity<RequirementTags>(entity =>
            {
                entity.ToTable("Requirement_Tags");

                entity.HasOne(d => d.Requirement).WithMany(d => d.RequirementTags).HasForeignKey(d => d.RequirementId);
                entity.HasOne(d => d.Tags).WithMany().HasForeignKey(d => d.TagId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.AspNetUserId);
            });

            builder.Entity<RequirementPinned>(entity =>
            {
                entity.ToTable("Requirement_Pinned");

                entity.HasOne(d => d.Requirement).WithMany(m => m.RequirementPinned).HasForeignKey(d => d.RequirementId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.AspNetUserId);
            });

            builder.Entity<RequirementColor>(entity =>
            {
                entity.ToTable("Requirement_Color");

                entity.HasOne(d => d.Requirement).WithMany(m => m.RequirementColors).HasForeignKey(d => d.RequirementId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.AspNetUserId);
            });

            #endregion

            #region Marketing -> Ad Post & Job Post

            builder.Entity<AdPost>(entity =>
            {
                entity.ToTable("AdPost");
                entity.Property(e => e.PictureId).HasMaxLength(450);
                entity.Property(e => e.ProjectId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CustomerId).HasMaxLength(450);
                entity.Property(e => e.ImageType).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ImageProviderClientId).HasMaxLength(450);
                entity.Property(e => e.ImageProviderEmpId).HasMaxLength(450);
                entity.Property(e => e.ContentType).HasMaxLength(450);
                entity.Property(e => e.ContentProviderClientId).HasMaxLength(450);
                entity.Property(e => e.ContentProviderEmpId).HasMaxLength(450);
                entity.Property(e => e.AdNumber);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.URL);
                entity.Property(e => e.Description);
                entity.Property(e => e.Caption);
                entity.Property(e => e.Tags);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Picture).WithMany().HasForeignKey(d => d.PictureId);
                entity.HasOne(d => d.Project).WithMany().HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.Customer).WithMany().HasForeignKey(d => d.CustomerId);
                entity.HasOne(d => d.ImageTypeDropDown).WithMany().HasForeignKey(d => d.ImageType);
                entity.HasOne(d => d.ImageProviderClient).WithMany().HasForeignKey(d => d.ImageProviderClientId);
                entity.HasOne(d => d.ImageProviderEmp).WithMany().HasForeignKey(d => d.ImageProviderEmpId);
                entity.HasOne(d => d.ContentTypeDropDown).WithMany().HasForeignKey(d => d.ContentType);
                entity.HasOne(d => d.ContentProviderClient).WithMany().HasForeignKey(d => d.ContentProviderClientId);
                entity.HasOne(d => d.ContentProviderEmp).WithMany().HasForeignKey(d => d.ContentProviderEmpId);
            });

            builder.Entity<AdPostChannel>(entity =>
            {
                entity.ToTable("AdPostChannel");
                entity.Property(e => e.ProjectId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CustomerId).HasMaxLength(450);
                entity.Property(e => e.ChannelNumber);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.GroupMemberCount);
                entity.Property(e => e.Description);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Project).WithMany().HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.Customer).WithMany().HasForeignKey(d => d.CustomerId);
            });

            builder.Entity<AdPostingStatus>(entity =>
            {
                entity.ToTable("AdPostingStatus");
                entity.Property(e => e.AdId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.AdPostChannelId).HasMaxLength(450);
                entity.Property(e => e.Date).HasPrecision(6);
                entity.Property(e => e.Likes);
                entity.Property(e => e.Comments);
                entity.Property(e => e.Shares);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.AdPost).WithMany(d => d.AdPostingStatusList).HasForeignKey(d => d.AdId);
                entity.HasOne(d => d.AdPostChannel).WithMany().HasForeignKey(d => d.AdPostChannelId);
            });

            builder.Entity<JobCreate>(entity =>
            {
                entity.ToTable("JobCreate");
                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.JobDescription);
                entity.Property(e => e.Criteria);
                entity.Property(e => e.JobTitle);
                entity.Property(e => e.JobCreatedDate).HasPrecision(6);
                entity.Property(e => e.PublishedJobDate).HasPrecision(6);
                entity.Property(e => e.JobReference).HasMaxLength(100);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).IsRequired().HasPrecision(6);

                entity.HasOne(e => e.Site).WithMany().HasForeignKey(e => e.SiteId);
            });

            #endregion

            #region Infrastructure Ver-1

            builder.Entity<Server>(entity =>
            {
                entity.ToTable("Server");
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Provider).WithMany().HasForeignKey(d => d.ProviderId);
                entity.HasOne(d => d.Sites).WithMany().HasForeignKey(d => d.SiteId);
            });

            builder.Entity<ServerPayments>(entity =>
            {
                entity.ToTable("ServerPayments");
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Server).WithMany(m => m.ServerPayments).HasForeignKey(d => d.ServerId);
            });

            builder.Entity<Domain>(entity =>
            {
                entity.ToTable("Domain");
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
                entity.HasOne(d => d.Project).WithMany().HasForeignKey(d => d.ProjectId);
                entity.HasOne(d => d.DomainServer).WithMany().HasForeignKey(d => d.DomainServerId);
                entity.HasOne(d => d.HostingServer).WithMany().HasForeignKey(d => d.HostingServerId);
                entity.HasOne(d => d.DomainType).WithMany().HasForeignKey(d => d.DomainTypeId);
                entity.HasOne(d => d.DomainMapping).WithMany().HasForeignKey(d => d.DomainMappingId);
            });

            builder.Entity<DomainAttributes>(entity =>
            {
                entity.ToTable("DomainAttributes");
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Domain).WithMany(m => m.DomainAttributes).HasForeignKey(d => d.DomainId);
                entity.HasOne(d => d.DomainAttribute).WithMany().HasForeignKey(d => d.DomainAttributeId);
            });

            #endregion

            #region Infrastructure Ver-2

            builder.Entity<InfraAccount>(entity =>
            {
                entity.ToTable("Infra_Account");

                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
                entity.HasOne(d => d.Provider).WithMany().HasForeignKey(d => d.ProviderId);
                entity.HasOne(d => d.WalletType).WithMany().HasForeignKey(d => d.WalletTypeId);
                entity.HasOne(d => d.CreatedBy).WithMany().HasForeignKey(d => d.CreatedById);
                entity.HasOne(d => d.UpdatedBy).WithMany().HasForeignKey(d => d.UpdatedById);
            });

            builder.Entity<InfraAccountServices>(entity =>
            {
                entity.ToTable("Infra_Account_Services_History");

                entity.HasOne(d => d.InfraAccount).WithMany(x => x.InfraAccountServices).HasForeignKey(d => d.InfraAccountId);
                entity.HasOne(d => d.ItemType).WithMany().HasForeignKey(d => d.ItemTypeId);
                entity.HasOne(d => d.WalletType).WithMany().HasForeignKey(d => d.WalletTypeId);
                entity.HasOne(d => d.OwnerShipType).WithMany().HasForeignKey(d => d.OwnerShipTypeId);
                entity.HasOne(d => d.PaymentTerm).WithMany().HasForeignKey(d => d.PaymentTermId);
                entity.HasOne(d => d.InfraAccountService).WithMany().HasForeignKey(d => d.InfraAccountServiceId);
            });

            builder.Entity<InfraAccountServicesPriceHistory>(entity =>
            {
                entity.ToTable("Infra_Account_Services_PriceHistory");

                entity.HasOne(d => d.InfraAccountServicesHistory).WithMany(p => p.PriceHistories).HasForeignKey(d => d.InfraAccountServiceId);
            });

            builder.Entity<InfraProjectServices>(entity =>
            {
                entity.ToTable("Infra_ProjectServices");

                entity.HasOne(d => d.InfraAccountServices).WithMany(x => x.InfraProjectServices).HasForeignKey(d => d.InfraServiceId);
                entity.HasOne(d => d.Project).WithMany(x => x.InfraProjectServices).HasForeignKey(d => d.InfraProjectId);
            });


            builder.Entity<InfraFTP>(entity =>
            {
                entity.ToTable("Infra_FTP");

                entity.HasOne(d => d.InfraService).WithMany(x => x.InfraFTPList).HasForeignKey(d => d.InfraServiceId);
                entity.HasOne(d => d.ProtocolType).WithMany().HasForeignKey(d => d.ProtocolTypeId);
                entity.HasOne(d => d.EncryptionType).WithMany().HasForeignKey(d => d.EncryptionTypeId);
                entity.HasOne(d => d.WalletType).WithMany().HasForeignKey(d => d.WalletTypeId);
            });

            builder.Entity<InfraFTPsProjectInstanceMapping>(entity =>
            {
                entity.ToTable("Infra_FTPs_ProjectInstance_Mapping");

                entity.HasOne(d => d.InfraFTP).WithMany(x => x.InfraFTPsProjectInstanceMapping).HasForeignKey(d => d.InfraFTPId);
                entity.HasOne(d => d.InfraProjectInstance).WithMany().HasForeignKey(d => d.ProjectInstanceId);
            });


            builder.Entity<InfraDatabase>(entity =>
            {
                entity.ToTable("Infra_Database");

                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
                entity.HasOne(d => d.InfraService).WithMany(x => x.InfraDatabaseList).HasForeignKey(d => d.InfraServiceId);
                entity.HasOne(d => d.WalletType).WithMany().HasForeignKey(d => d.WalletTypeId);
            });

            builder.Entity<InfraDatabaseProjectInstanceMapping>(entity =>
            {
                entity.ToTable("Infra_Database_ProjectInstance_Mapping");

                entity.HasOne(d => d.InfraDatabase).WithMany(x => x.InfraDatabaseProjectInstanceMapping).HasForeignKey(d => d.InfraDatabaseId);
                entity.HasOne(d => d.InfraProjectInstance).WithMany().HasForeignKey(d => d.ProjectInstanceId);
            });


            builder.Entity<InfraProjectInstance>(entity =>
            {
                entity.ToTable("Infra_ProjectInstance");

                entity.HasOne(d => d.InfraProject).WithMany().HasForeignKey(d => d.InfraProjectId);
                entity.HasOne(d => d.InstanceType).WithMany().HasForeignKey(d => d.InstanceTypeId);
                entity.HasOne(d => d.Platform).WithMany().HasForeignKey(d => d.PlatformId);
            });

            builder.Entity<InfraProjectInstanceRole>(entity =>
            {
                entity.ToTable("Infra_ProjectInstance_Role");

                entity.HasOne(d => d.ProjectInstance).WithMany(x => x.InfraProjectInstanceRole).HasForeignKey(d => d.ProjectInstanceId);
            });

            builder.Entity<InfraProjectInstanceRoleUsers>(entity =>
            {
                entity.ToTable("Infra_ProjectInstance_Role_Users");

                entity.HasOne(d => d.ProjectInstanceRole).WithMany(x => x.InfraProjectInstanceRoleUsers).HasForeignKey(d => d.ProjectInstanceRoleId);
            });

            #endregion

            #region Infrastrcuture - Inventory - Ver 1

            builder.Entity<Inventory>(entity =>
            {
                entity.ToTable("Inventory");
                entity.Property(e => e.ItemTypeId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.InventoryStatusId).HasMaxLength(450);
                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.AssignmentTypeId).HasMaxLength(450);
                entity.Property(e => e.Inventorycode);
                entity.Property(e => e.DateofPurchase).HasPrecision(6);
                entity.Property(e => e.Warranty).HasMaxLength(50);
                entity.Property(e => e.Guaranty).HasMaxLength(50);
                entity.Property(e => e.InventoryAssignId).HasMaxLength(450);
                entity.Property(e => e.ServiceCode).HasMaxLength(50);
                entity.Property(e => e.Notes);
                entity.Property(e => e.OperatingSystem);
                entity.Property(e => e.ProcessorType);
                entity.Property(e => e.MemoryORRAM);
                entity.Property(e => e.HardDriveORStorageCapacity);
                entity.Property(e => e.GraphicsCard);
                entity.Property(e => e.VirusProtection);
                entity.Property(e => e.ModelNameORNumber);
                entity.Property(e => e.Description);
                entity.Property(e => e.WarrantyExpiryDate).HasPrecision(6);
                entity.Property(e => e.AssignDate).HasPrecision(6);
                entity.Property(e => e.ReturnDate).HasPrecision(6);
                entity.Property(e => e.ReturnReson);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.ItemType).WithMany().HasForeignKey(d => d.ItemTypeId);
                entity.HasOne(d => d.InventoryStatus).WithMany().HasForeignKey(d => d.InventoryStatusId);
                entity.HasOne(d => d.Employee).WithMany().HasForeignKey(d => d.EmployeeId);
                entity.HasOne(d => d.AssignmentType).WithMany().HasForeignKey(d => d.AssignmentTypeId);
                entity.HasOne(d => d.InventoryAssign).WithMany().HasForeignKey(d => d.InventoryAssignId);
                entity.HasOne(d => d.CreatedBy).WithMany().HasForeignKey(d => d.CreatedById);
                entity.HasOne(d => d.UpdatedBy).WithMany().HasForeignKey(d => d.UpdatedById);
            });

            builder.Entity<InventoryAssignment>(entity =>
            {
                entity.ToTable("InventoryAssignment");
                entity.Property(e => e.InventoryId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.EmployeeId).HasMaxLength(450);
                entity.Property(e => e.AssignDate).HasPrecision(6);
                entity.Property(e => e.ReturnDate).HasPrecision(6);
                entity.Property(e => e.ReturnReson);

                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Inventory).WithMany(d => d.InventoryAssignmentList).HasForeignKey(d => d.InventoryId);
                entity.HasOne(d => d.Employee).WithMany().HasForeignKey(d => d.EmployeeId);
            });

            builder.Entity<InventoryItemType>(entity =>
            {
                entity.ToTable("InventoryItemType");
                entity.Property(e => e.SiteId).HasMaxLength(450);
                entity.Property(e => e.Name);
                entity.Property(e => e.Prefix);
                entity.Property(e => e.SortOrder);

                entity.HasOne(d => d.Sites).WithMany().HasForeignKey(d => d.SiteId);
            });

            #endregion

            #region Reports

            builder.Entity<ReportSettings>(entity =>
            {
                entity.ToTable("ReportSettings");
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);

                entity.HasOne(e => e.Sites).WithMany().HasForeignKey(e => e.SiteId);
            });

            builder.Entity<ReportSettingsDetails>(entity =>
            {
                entity.ToTable("ReportSettingsDetails");

                entity.Property(e => e.ReportId).HasMaxLength(450);
                entity.Property(e => e.ReportGroupId).HasMaxLength(450);

                entity.HasOne(e => e.ReportSetting).WithMany().HasForeignKey(e => e.ReportSettingId);
                entity.HasOne(e => e.ReportGroup).WithMany().HasForeignKey(e => e.ReportGroupId);
                entity.HasOne(e => e.Sites).WithMany().HasForeignKey(e => e.SiteId);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById);
                entity.HasOne(e => e.UpdatedBy).WithMany().HasForeignKey(e => e.UpdatedById);
            });


            builder.Entity<ReportUserMapping>(entity =>
            {
                entity.ToTable("Report_User_Mapping");

                entity.Property(e => e.ReportSettingsDetailId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.AspNetUserId).HasMaxLength(256);
                entity.Property(e => e.FullAccess);
                entity.Property(e => e.ViewOnly);

                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.ReportSettingsDetail).WithMany(p => p.ReportUserMapping).HasForeignKey(d => d.ReportSettingsDetailId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.AspNetUserId);
            });

            builder.Entity<ReportRoleGroupMapping>(entity =>
            {
                entity.ToTable("Report_Role_Group_Mapping");

                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.SiteRoleId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ReportGroupId).IsRequired().HasMaxLength(450);

                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);

                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
                entity.HasOne(d => d.SitesRoles).WithMany().HasForeignKey(d => d.SiteRoleId);
                entity.HasOne(d => d.ReportGroup).WithMany().HasForeignKey(d => d.ReportGroupId);
            });

            #endregion

            #region System Notifications

            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");
                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(200);
                entity.Property(e => e.FromUserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.RecordId).HasMaxLength(450);
                entity.Property(e => e.RedirectURL).HasMaxLength(500);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);

                entity.HasOne(e => e.Site).WithMany().HasForeignKey(e => e.SiteId);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.FromUserId);
            });

            builder.Entity<NotificationDetails>(entity =>
            {
                entity.ToTable("NotificationDetails");
                entity.Property(e => e.NotificationId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ToUserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.IsRead);

                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.ToUserId);
            });

            #endregion

            #region Candidate

            builder.Entity<Candidate>(entity =>
            {
                entity.ToTable("Candidate");
                entity.Property(e => e.SearchNumber).ValueGeneratedOnAdd().HasAnnotation("SqlServer:Identity", "20000, 1").Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
                entity.Property(e => e.SiteId).HasMaxLength(450);
                entity.Property(e => e.JobId).HasMaxLength(450);
                entity.Property(e => e.PersonId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.AddressId).HasMaxLength(450);
                entity.Property(e => e.EnglishFluencyId).HasMaxLength(450);
                entity.Property(e => e.LanguageId).HasMaxLength(450);
                entity.Property(e => e.SourceId).HasMaxLength(450);
                entity.Property(e => e.DepartmentId).HasMaxLength(450);
                entity.Property(e => e.AppliedJobPositionId).HasMaxLength(450);
                entity.Property(e => e.RecruiterId).HasMaxLength(450);
                entity.Property(e => e.AppliedWorkLocationId).HasMaxLength(450);
                entity.Property(e => e.StatusId).HasMaxLength(450);
                entity.Property(e => e.AvailabilityWorkId).HasMaxLength(100);
                entity.Property(e => e.Source).HasMaxLength(450);
                entity.Property(e => e.JobApplyDate).HasPrecision(6);
                entity.Property(e => e.ReferenceName).HasMaxLength(450);
                entity.Property(e => e.Qualification).HasMaxLength(450);
                entity.Property(e => e.ExperienceYears);
                entity.Property(e => e.ExperienceMonths);
                entity.Property(e => e.ExpectedSalaryFrom).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.ExpectedSalaryTo).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.ExperienceDetails);
                entity.Property(e => e.IsTransportration);
                entity.Property(e => e.IsReadyToRelocate);
                entity.Property(e => e.IsCandidateOwnSystem);
                entity.Property(e => e.IsExperienced);
                entity.Property(e => e.CandidateResumeFileId).HasMaxLength(450);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).IsRequired().HasPrecision(6);

                entity.HasOne(e => e.Sites).WithMany().HasForeignKey(e => e.SiteId);
                entity.HasOne(e => e.Job).WithMany().HasForeignKey(e => e.JobId);
                entity.HasOne(e => e.Person).WithMany().HasForeignKey(e => e.PersonId);
                entity.HasOne(e => e.Address).WithMany().HasForeignKey(e => e.AddressId);
                entity.HasOne(e => e.EnglishFluencies).WithMany().HasForeignKey(e => e.EnglishFluencyId);
                entity.HasOne(e => e.Language).WithMany().HasForeignKey(e => e.LanguageId);
                entity.HasOne(e => e.Sources).WithMany().HasForeignKey(e => e.SourceId);
                entity.HasOne(e => e.Departments).WithMany().HasForeignKey(e => e.DepartmentId);
                entity.HasOne(e => e.AppliedJobPositions).WithMany().HasForeignKey(e => e.AppliedJobPositionId);
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.RecruiterId);
                entity.HasOne(e => e.AppliedWorkLocations).WithMany().HasForeignKey(e => e.AppliedWorkLocationId);
                entity.HasOne(e => e.AvailabilityWorks).WithMany().HasForeignKey(e => e.AvailabilityWorkId);
                entity.HasOne(e => e.Status).WithMany().HasForeignKey(e => e.StatusId);
                entity.HasOne(d => d.File).WithMany().HasForeignKey(d => d.CandidateResumeFileId);
            });

            builder.Entity<CandidateActivities>(entity =>
            {
                entity.ToTable("CandidateActivities");

                entity.Property(e => e.CandidateId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.PriorityId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.EmployeeOwnerId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ActivityName).HasMaxLength(450);
                entity.Property(e => e.DueDate).HasPrecision(6);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).IsRequired().HasPrecision(6);

                entity.HasOne(e => e.Priority).WithMany().HasForeignKey(e => e.PriorityId);
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeOwnerId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.CreatedById);
            });

            builder.Entity<CandidateFeedback>(entity =>
            {
                entity.ToTable("CandidateFeedback");

                entity.Property(e => e.CandidateId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.EmployeeOwnerId).HasMaxLength(450);
                entity.Property(e => e.QuestionId).HasMaxLength(450);
                entity.Property(e => e.QuestionTypeId).HasMaxLength(450);
                entity.Property(e => e.Answer);
                entity.Property(e => e.Description);
                entity.Property(e => e.DueDate).HasPrecision(6);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).IsRequired().HasPrecision(6);

                entity.HasOne(e => e.CandidateQuestions).WithMany().HasForeignKey(e => e.QuestionId);
                entity.HasOne(e => e.CandidateQuestionType).WithMany().HasForeignKey(e => e.QuestionTypeId);
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeOwnerId);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.CreatedById);
            });

            builder.Entity<CandidateDepartments>(entity =>
            {
                entity.ToTable("CandidateDepartments");

                entity.Property(e => e.CandidateId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.DepartmentsId).IsRequired().HasMaxLength(450);

                entity.HasOne(e => e.Departments).WithMany().HasForeignKey(e => e.DepartmentsId);
            });

            builder.Entity<CandidateNotes>(entity =>
            {
                entity.ToTable("CandidateNotes");

                entity.Property(e => e.CandidateId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.NoteId).IsRequired().HasMaxLength(450);

                entity.HasOne(e => e.Note).WithMany().HasForeignKey(e => e.NoteId);
            });

            #endregion

            #region HelpDesk

            builder.Entity<HelpDesk>(entity =>
            {
                entity.ToTable("HelpDesk");
                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.RequesterId).HasMaxLength(450);
                entity.Property(e => e.TopicId).HasMaxLength(450);
                entity.Property(e => e.QuestionId).HasMaxLength(450);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Description);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).IsRequired().HasPrecision(6);

                entity.HasOne(e => e.Site).WithMany().HasForeignKey(e => e.SiteId);
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.RequesterId);
                entity.HasOne(e => e.HelpDeskTopic).WithMany().HasForeignKey(e => e.TopicId);
                entity.HasOne(e => e.HelpDeskTopicQuestions).WithMany().HasForeignKey(e => e.QuestionId);
                entity.HasOne(e => e.Priority).WithMany().HasForeignKey(e => e.PriorityId);
                entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId);
                entity.HasOne(e => e.AssignedTo).WithMany().HasForeignKey(e => e.AssignedToId);
                entity.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById);
                entity.HasOne(e => e.UpdatedBy).WithMany().HasForeignKey(e => e.UpdatedById);
            });

            builder.Entity<HelpDeskStatusLog>(entity =>
            {
                entity.ToTable("HelpDesk_StatusLog");
                entity.Property(e => e.HelpDeskId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.StatusId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.DurationInMinutes).IsRequired();

                entity.HasOne(e => e.Status).WithMany().HasForeignKey(e => e.StatusId);
            });

            builder.Entity<HelpDeskFiles>(entity =>
            {
                entity.ToTable("HelpDesk_Files");

                entity.Property(e => e.HelpDeskId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.FileId).IsRequired().HasMaxLength(450);
                entity.HasOne(d => d.File).WithMany().HasForeignKey(d => d.FileId);
            });

            builder.Entity<HelpDeskReminderLog>(entity =>
            {
                entity.ToTable("HelpDesk_ReminderLog");

                entity.HasOne(d => d.HelpDesk).WithMany().HasForeignKey(d => d.HelpDeskId);
                entity.HasOne(d => d.SitesEmailNotifications).WithMany().HasForeignKey(d => d.SiteEmailNotificationId);
            });
            
            builder.Entity<HelpDeskEmailRepliesMapping>(entity =>
            {
                entity.ToTable("HelpDesk_EmailReplies_Mapping");
                entity.Property(e => e.HelpDeskId).HasMaxLength(450);
                entity.Property(e => e.EmailRepliesId).HasMaxLength(450);

                entity.HasOne(e => e.HelpDesk).WithMany().HasForeignKey(e => e.HelpDeskId);
                entity.HasOne(e => e.EmailReplies).WithMany().HasForeignKey(e => e.EmailRepliesId);
            });

            builder.Entity<EmailReplies>(entity =>
            {
                entity.ToTable("EmailReplies");
            });

            builder.Entity<HelpDeskTopic>(entity =>
            {
                entity.ToTable("HelpDesk_Topic");
                entity.Property(e => e.SiteId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Description);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).IsRequired().HasPrecision(6);

                entity.HasOne(e => e.Site).WithMany().HasForeignKey(e => e.SiteId);
            });

            builder.Entity<HelpDeskTopicQuestions>(entity =>
            {
                entity.ToTable("HelpDesk_Topic_Questions");
                entity.Property(e => e.TopicId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Question);
                entity.Property(e => e.Description);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).IsRequired().HasPrecision(6);

                entity.HasOne(e => e.HelpDeskTopic).WithMany().HasForeignKey(e => e.TopicId);
            });

            #endregion

            #region Finance

            builder.Entity<Expense>(entity =>
            {
                entity.ToTable("Expense");

                entity.Property(e => e.BackAccountId).HasMaxLength(450);
                entity.Property(e => e.PayeeId).HasMaxLength(450);
                entity.Property(e => e.StatusId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedById).IsRequired().HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).IsRequired().HasPrecision(6);
                entity.Property(e => e.UpdatedById).HasMaxLength(450);
                entity.Property(e => e.UpdatedOnUtc).HasPrecision(6);
                entity.HasOne(e => e.ExpenseVendors).WithMany().HasForeignKey(e => e.PayeeId);
                entity.HasOne(e => e.ExpenseVendorBankAccounts).WithMany().HasForeignKey(e => e.ExpenseVendorBankAccountId);
                entity.HasOne(e => e.ExpenseStatus).WithMany().HasForeignKey(e => e.StatusId);
                entity.HasOne(e => e.Location).WithMany().HasForeignKey(e => e.LocationId);
                entity.HasOne(e => e.RecurringInterval).WithMany().HasForeignKey(e => e.RecurringIntervalId);
                entity.HasOne(e => e.Picture).WithMany().HasForeignKey(e => e.Attachment);
                entity.HasOne(e => e.ExpenseBankAccounts).WithMany(x => x.ExpenseList).HasForeignKey(e => e.BackAccountId);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById);
                entity.HasOne(e => e.UpdatedBy).WithMany().HasForeignKey(e => e.UpdatedById);
                entity.HasOne(e => e.Expense_Advance_Requests).WithMany().HasForeignKey(e => e.Ref_no);
            });

            builder.Entity<Expense_Lines>(entity =>
            {
                entity.ToTable("Expense_Lines");

                entity.Property(e => e.Amount).IsRequired();
                entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.ExpenseCategoryId);
                entity.HasOne(e => e.ExpenseCategorySubcategory).WithMany().HasForeignKey(e => e.ExpenseSubcategoryId);
            });

            builder.Entity<Expense_BankAccounts>(entity =>
            {
                entity.ToTable("Expense_BankAccounts");

                entity.Property(e => e.AccountNumber).HasMaxLength(200);
                entity.HasOne(e => e.AccountTypeDropDown).WithMany().HasForeignKey(e => e.AccountTypeId);
            });

            builder.Entity<ExpenseVendors>(entity =>
            {
                entity.ToTable("Expense_Vendors");

                entity.HasOne(e => e.Address).WithMany().HasForeignKey(e => e.AddressId);
                entity.HasOne(e => e.Person).WithMany().HasForeignKey(e => e.PersonId);
            });

            builder.Entity<ExpenseVendorBankAccounts>(entity =>
            {
                entity.ToTable("Expense_Vendor_BankAccounts");

                entity.HasOne(e => e.AccountType).WithMany().HasForeignKey(e => e.AccountTypeId);
                entity.HasOne(e => e.PaymentType).WithMany().HasForeignKey(e => e.PaymentTypeId);
            });

            builder.Entity<Expense_Files>(entity =>
            {
                entity.ToTable("Expense_Files");

                entity.Property(e => e.ExpenseId).HasMaxLength(450);
                entity.Property(e => e.FileId).HasMaxLength(450);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById);
            });

            builder.Entity<Expense_Advance_Requests>(entity =>
            {
                entity.ToTable("Expense_Advance_Requests");

                entity.HasOne(e => e.RequestedEmployee).WithMany().HasForeignKey(e => e.RequestedBy);
                entity.HasOne(e => e.Location).WithMany().HasForeignKey(e => e.LocationId);
                entity.HasOne(e => e.AdvanceExpenseStatus).WithMany().HasForeignKey(e => e.StatusId);
                entity.HasOne(e => e.CreatedBy).WithMany().HasForeignKey(e => e.CreatedById);
                entity.HasOne(e => e.PaymentType).WithMany().HasForeignKey(e => e.PaymentTypeId);
                entity.HasOne(e => e.ItemCategory).WithMany().HasForeignKey(e => e.ItemCategoryId);
                entity.HasOne(e => e.ItemSubCategory).WithMany().HasForeignKey(e => e.ItemSubCategoryId);
            });


            builder.Entity<Expense_Purchase_Requests>(entity =>
            {
                entity.ToTable("Expense_Purchase_Requests");

                entity.HasOne(e => e.RequestedEmployee).WithMany().HasForeignKey(e => e.RequestedById);
                entity.HasOne(e => e.ItemCategory).WithMany().HasForeignKey(e => e.ItemCategoryId);
                entity.HasOne(e => e.ItemSubCategory).WithMany().HasForeignKey(e => e.ItemSubCategoryId);
                entity.HasOne(e => e.PurchaserEmployee).WithMany().HasForeignKey(e => e.PurchaserId);
                entity.HasOne(e => e.ExpenseVendors).WithMany().HasForeignKey(e => e.VendorId);
                entity.HasOne(e => e.PurchaseRequestStatus).WithMany().HasForeignKey(e => e.StatusId);
            });

            builder.Entity<ExpensePurchaseRequestFiles>(entity =>
            {
                entity.ToTable("Expense_Purchase_Request_Files");

                entity.HasOne(d => d.Expense_Purchase_Requests).WithMany(p => p.ExpensePurchaseRequestFileList).HasForeignKey(d => d.ExpensePurchaseRequestId);
                entity.HasOne(d => d.File).WithMany().HasForeignKey(d => d.FileId);
            });

            builder.Entity<ExpenseAdvanceRequestFiles>(entity =>
            {
                entity.ToTable("Expense_Advance_Request_Files");

                entity.HasOne(d => d.Expense_Advance_Requests).WithMany(p => p.ExpenseAdvanceRequestFileList).HasForeignKey(d => d.ExpenseAdvanceRequestId);
                entity.HasOne(d => d.File).WithMany().HasForeignKey(d => d.FileId);
            });

            #endregion

            #region Items

            builder.Entity<ItemCategory>(entity =>
            {
                entity.ToTable("ItemCategory");
            });

            builder.Entity<ItemSubcategory>(entity =>
            {
                entity.ToTable("ItemSubcategory");

                entity.HasOne(d => d.ItemCategory).WithMany(p => p.ItemSubcategory).HasForeignKey(d => d.ItemCategoryId);
            });

            builder.Entity<ItemSubCategoryAttributes>(entity =>
            {
                entity.ToTable("Item_SubCategory_Attributes");

            });

            builder.Entity<ItemSubCategoryAttributesValues>(entity =>
            {
                entity.ToTable("Item_SubCategory_Attributes_Values");

                entity.HasOne(d => d.ItemSubcategory).WithMany().HasForeignKey(d => d.ItemSubCategoryId);
                entity.HasOne(d => d.ItemSubCategoryAttributes).WithMany(p => p.ItemSubCategoryAttributesValues).HasForeignKey(d => d.ItemSubCategoryAttributeId);
            });

            #endregion

            #region Sites -> Items

            builder.Entity<SitesItemSubCategoryAttributesMapping>(entity =>
            {
                entity.ToTable("Sites_Item_SubCategory_Attributes_Mapping");

                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
                entity.HasOne(d => d.ItemSubcategory).WithMany(p => p.SitesItemSubCategoryAttributesMapping).HasForeignKey(d => d.ItemSubCategoryId);
                entity.HasOne(d => d.ItemSubCategoryAttributes).WithMany(p => p.SitesItemSubCategoryAttributesMapping).HasForeignKey(d => d.ItemSubCategoryAttributeId);
            });

            builder.Entity<SitesItems>(entity =>
            {
                entity.ToTable("Sites_Items");
                entity.HasOne(d => d.Site).WithMany().HasForeignKey(d => d.SiteId);
                entity.HasOne(d => d.ItemSubcategory).WithMany().HasForeignKey(d => d.ItemSubCategoryId);
            });

            builder.Entity<SitesItemsAttributes>(entity =>
            {
                entity.ToTable("Sites_Items_Attributes");

                entity.HasOne(d => d.ItemSubCategoryAttributes).WithMany().HasForeignKey(d => d.ItemSubCategoryAttributeId);
                entity.HasOne(d => d.SitesItems).WithMany(p => p.SitesItemsAttributeList).HasForeignKey(d => d.SiteItemId);
            });

            #endregion

            #region Websites

            builder.Entity<Contact>(entity =>
            {
                entity.ToTable("Contact");
                entity.Property(e => e.FullName).HasMaxLength(128);
                entity.Property(e => e.Email).HasMaxLength(128);
                entity.Property(e => e.PhoneNo).HasMaxLength(16);
                entity.Property(e => e.MobileNo).HasMaxLength(16);
                entity.Property(e => e.Title).HasMaxLength(450);
                entity.Property(e => e.Message);
                entity.Property(e => e.ContactedDate).HasPrecision(6);
            });

            builder.Entity<Website_Demos>(entity =>
            {
                entity.ToTable("Website_Demos");

                entity.Property(e => e.BusinessSizeId).HasMaxLength(450);
                entity.Property(e => e.FullName);
                entity.Property(e => e.EmailAddress);
                entity.Property(e => e.CompanyName);
                entity.Property(e => e.CreatedById).HasMaxLength(450);
                entity.Property(e => e.CreatedOnUtc).HasPrecision(6);

                entity.HasOne(e => e.BusinessSize).WithMany().HasForeignKey(e => e.BusinessSizeId);

            });
            builder.Entity<Website_Demo_Modules>(entity =>
            {
                entity.ToTable("Website_Demo_Modules");
                entity.Property(e => e.WebsiteDemoId).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ModuleId).IsRequired().HasMaxLength(200);

                entity.HasOne(d => d.Website_Demos).WithMany(d => d.Website_Demo_Modules).HasForeignKey(d => d.WebsiteDemoId);
                entity.HasOne(e => e.Modules).WithMany().HasForeignKey(e => e.ModuleId);
            });

            #endregion

            #region SOP Template & SOP Assignment & SOP Process

            builder.Entity<SOPTemplate>(entity =>
            {
                entity.ToTable("SOPTemplate");

                entity.HasOne(d => d.CreatedBy).WithMany().HasForeignKey(d => d.CreatedById);
                entity.HasOne(d => d.UpdatedBy).WithMany().HasForeignKey(d => d.UpdatedById);
            });

            builder.Entity<SOPTemplateSection>(entity =>
            {
                entity.ToTable("SOPTemplate_Section");

                //entity.HasOne(d => d.Template).WithMany(x => x.SOPTemplateSections).HasForeignKey(d => d.TemplateId);

                entity.HasOne(d => d.CreatedBy).WithMany().HasForeignKey(d => d.CreatedById);
                entity.HasOne(d => d.UpdatedBy).WithMany().HasForeignKey(d => d.UpdatedById);
            });

            builder.Entity<SOPTemplateSectionItems>(entity =>
            {
                entity.ToTable("SOPTemplate_SectionItems");

                //entity.HasOne(d => d.Template).WithMany().HasForeignKey(d => d.TemplateId);
                //entity.HasOne(d => d.Section).WithMany(x => x.SOPTemplateSectionItems).HasForeignKey(d => d.SectionId);

                entity.HasOne(d => d.InputType).WithMany().HasForeignKey(d => d.InputTypeId);

                entity.HasOne(d => d.CreatedBy).WithMany().HasForeignKey(d => d.CreatedById);
                entity.HasOne(d => d.UpdatedBy).WithMany().HasForeignKey(d => d.UpdatedById);
            });

            builder.Entity<SOPAssignment>(entity =>
            {
                entity.ToTable("SOPAssignment");
            });

            builder.Entity<SOPAssignmentResponse>(entity =>
            {
                entity.ToTable("SOPAssignment_Response");

                entity.HasOne(d => d.CreatedBy).WithMany().HasForeignKey(d => d.CreatedById);
                entity.HasOne(d => d.UpdatedBy).WithMany().HasForeignKey(d => d.UpdatedById);
            });

            builder.Entity<SOPAssignmentResponseEvidences>(entity =>
            {
                entity.ToTable("SOPAssignment_ResponseEvidences");

                entity.HasOne(d => d.CreatedBy).WithMany().HasForeignKey(d => d.CreatedById);
            });

            builder.Entity<SOPProcess>(entity =>
            {
                entity.ToTable("SOPProcess");
            });

            //builder.Entity<SOPProcessItems>(entity =>
            //{
            //    entity.ToTable("SOPProcess_Items");
            //});

            builder.Entity<SOPProcessStatusLog>(entity =>
            {
                entity.ToTable("SOPProcess_StatusLog");
            });

            #endregion

            #region MS-SQL Views

            builder.Entity<VW_Customer>(entity =>
            {
                entity.ToView("VW_CustomerList");
            });

            builder.Entity<VW_CustomerFiles>(entity =>
            {
                entity.ToView("VW_CustomerFiles");
            });


            builder.Entity<VW_Project>(entity =>
            {
                entity.ToView("VW_CustomerProjectList");
                entity.HasOne(d => d.Customer).WithMany(d => d.Projects).HasForeignKey(d => d.CustomerId);
            });

            builder.Entity<VW_UserProjectPinned>(entity =>
            {
                entity.ToView("VW_UserProjectPinned");
            });

            builder.Entity<VW_UserProjectColor>(entity =>
            {
                entity.ToView("VW_UserProjectColor");
            });

            builder.Entity<VW_ProjectSwimLane>(entity =>
            {
                entity.ToView("VW_ProjectSwimLaneList");
            });

            builder.Entity<VW_ProjectModules>(entity =>
            {
                entity.ToView("VW_ProjectModulesList");
            });

            builder.Entity<VW_ProjectTask>(entity =>
            {
                entity.ToView("VW_ProjectTaskList");
            });

            builder.Entity<VW_UserTaskTags>(entity =>
            {
                entity.ToView("VW_UserTaskTags");
            });

            builder.Entity<VW_ProjectTaskActivities>(entity =>
            {
                entity.ToView("VW_ProjectTaskActivities");
                entity.HasOne(d => d.ProjectTask).WithMany(d => d.ProjectActivities).HasForeignKey(d => d.ProjectTaskId);
            });


            builder.Entity<VWProjectTaskStatusSummary>(entity =>
            {
                entity.ToView("VW_ProjectTaskStatusSummary");
                entity.HasOne(v => v.Project)
                      .WithOne(p => p.ProjectTaskStatusSummary)
                      .HasForeignKey<VWProjectTaskStatusSummary>(v => v.ProjectId)
                      .HasPrincipalKey<VW_Project>(p => p.Id);
            });

            builder.Entity<VWProjectIssueStatusSummary>(entity =>
            {
                entity.ToView("VW_ProjectIssueStatusSummary");
                entity.HasOne(v => v.Project)
                      .WithOne(p => p.ProjectIssueStatusSummary)
                      .HasForeignKey<VWProjectIssueStatusSummary>(v => v.ProjectId)
                      .HasPrincipalKey<VW_Project>(p => p.Id);
            });

            builder.Entity<VWCustomerTaskStatusSummary>(entity =>
            {
                entity.ToView("VW_CustomerTaskStatusSummary");
                entity.HasOne(v => v.Customer)
                      .WithOne(p => p.CustomerTaskStatusSummary)
                      .HasForeignKey<VWCustomerTaskStatusSummary>(v => v.CustomerId)
                      .HasPrincipalKey<VW_Customer>(p => p.Id);
            });

            builder.Entity<VW_EmployeeTaskActivitySummary>(entity =>
            {
                entity.ToView("VW_EmployeeTaskActivitySummary");
            });

            builder.Entity<VWProjectRequirementStatusSummary>(entity =>
            {
                entity.ToView("VW_ProjectRequirementStatusSummary");
                entity.HasOne(v => v.Project)
                      .WithOne(p => p.ProjectRequirementStatusSummary)
                      .HasForeignKey<VWProjectRequirementStatusSummary>(v => v.ProjectId)
                      .HasPrincipalKey<VW_Project>(p => p.Id);
            });

            builder.Entity<VWEmployeeAssignedHours>(entity =>
            {
                entity.ToView("VW_EmployeeAssignedHours");
                entity.HasOne(d => d.Employee).WithMany(d => d.EmployeeAssignedHours).HasForeignKey(d => d.EmployeeId);
            });

            #endregion
        }
    }
}