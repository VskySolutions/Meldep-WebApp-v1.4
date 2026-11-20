namespace Vsky.Api.Authorization
{
    /// <summary>
    /// One constant per ModulesMenus.Link value in the database. A link edited in the admin screen
    /// must be changed here too, or the rule stops matching and access is denied.
    /// A value with | means "any of these menus", because attribute arguments must be constants.
    /// </summary>
    public static class MenuLinks
    {
        public const string Dashboard = "/dashboard";

        // CRM
        public const string Company = "/company";
        public const string CompanyContacts = "/company-contacts";
        public const string Customer = "/customer";
        public const string CustomerFiles = "/customer-files";
        public const string Lead = "/lead";
        public const string LeadGroupUsers = "/lead-group-user-assignment/list";

        // Expenses
        public const string AdvanceExpense = "/finance-expense-advance";
        public const string ApproveAdvanceExpense = "/approve-advance-expense";
        public const string ApproveExpense = "/approve-expense";
        public const string ApprovePurchaseExpense = "/approve-purchase-expense";
        public const string BankAccounts = "/finance-bank-account";
        public const string Expenses = "/finance-expense";
        public const string PurchaseExpense = "/finance-expense-purchase-request";
        public const string Vendors = "/finance-expense-vendors";

        // Infrastructure
        public const string InfraAccountServices = "/infra-account-services";
        public const string InfraAccount = "/infra-account";
        public const string InfraDatabase = "/infra-database/list";
        public const string Domain = "/domain";
        public const string InfraFtp = "/infra-ftp/list";
        public const string FtpList = "/ftp-list";
        public const string Inventory = "/inventory";
        public const string InfraProjectInstance = "/infra-project-instance/list";
        public const string Server = "/server";

        // Marketing
        public const string AdPost = "/marketing-ad-post";
        public const string AdPostChannel = "/marketing-ad-post-channel";

        // My work
        public const string DailyPlanner = "/my-daily-planner";
        public const string MyTasks = "/my-task-and-activities";
        public const string Timesheet = "/timesheet";

        // Organization and leave
        public const string ApplyLeave = "/apply-leave";
        public const string ApproveLeaves = "/approve-leaves";
        public const string Department = "/department";
        public const string OrgStructure = "/employee-org-structure";
        public const string ForwardLeaves = "/forward-leaves";
        public const string LeaveCredit = "/leave-credit";
        public const string TrainingPortal = "/training-portal";

        // Project management
        public const string AllProjectPlanner = "/all-project-planner";
        public const string BillingProcessing = "/billing-processing-employee";
        public const string MonthlyPlan = "/project-targetplan/monthlyplanner";
        public const string ProjectQuestions = "/project-questions-answers/list";
        public const string ProjectRelease = "/project-release-tracking/list";
        public const string ProjectTasks = "/project-tasks";
        public const string Project = "/project";
        public const string WeeklyPlan = "/project-targetplan/weeklyplanner";

        public const string Reports = "/reports";

        // SDLC
        public const string Issue = "/issue";
        public const string RequirementGroup = "/requirement-group";
        public const string RequirementDataMapping = "/requirement-data-mapping";
        public const string Requirement = "/requirement";
        public const string TestCase = "/test-case";
        public const string TestPlan = "/test-plan";

        // Settings
        public const string DropdownTypes = "/dropdowntypes";
        public const string Dropdowns = "/dropdown";
        public const string LeaveRules = "/leave-rules";
        public const string MasterRoles = "/master-roles";
        public const string Modules = "/modules";
        public const string ProjectSecurity = "/project/assign-users-to-project";
        public const string SiteRoles = "/site-roles";
        public const string Sites = "/sites";
        public const string SwimlaneTemplates = "/master-project-template";
        public const string Tags = "/tags";

        // Analytics: one menu per register, so View and Manage are granted separately for each.
        public const string TimesheetRegister = "/analytics/timesheet-register";
        public const string AttendanceRegister = "/analytics/attendance-register";

        // SOP
        public const string SopAssignment = "/sop-assignment";
        public const string SopProcess = "/sop-process";
        public const string SopTemplate = "/sop-template";

        // Talent and users
        public const string Candidate = "/candidate";
        public const string JobPost = "/job-post";
        public const string Employee = "/employee";
        public const string Person = "/person";
        public const string UserManagement = "/user-management";

        // Groups: screens that share one set of APIs
        public const string ProjectWork = Project + "|" + ProjectTasks + "|" + MyTasks + "|" + AllProjectPlanner
            + "|" + WeeklyPlan + "|" + MonthlyPlan + "|" + BillingProcessing + "|" + ProjectSecurity
            + "|" + ProjectQuestions + "|" + ProjectRelease + "|" + Timesheet + "|" + DailyPlanner;
        public const string LeaveAny = ApplyLeave + "|" + ApproveLeaves + "|" + ForwardLeaves + "|" + LeaveCredit;
        public const string ExpenseAny = Expenses + "|" + ApproveExpense;
        public const string AdvanceExpenseAny = AdvanceExpense + "|" + ApproveAdvanceExpense;
        public const string PurchaseExpenseAny = PurchaseExpense + "|" + ApprovePurchaseExpense;
        public const string InfraAny = InfraAccount + "|" + InfraAccountServices + "|" + InfraDatabase + "|" + InfraFtp
            + "|" + InfraProjectInstance + "|" + Server + "|" + Domain;
    }
}
