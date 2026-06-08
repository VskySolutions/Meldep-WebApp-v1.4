using AutoMapper;
using Vsky.Api.Models;
using Vsky.Api.Models.ExpenseModels;
using Vsky.Api.Models.ProjectSwimlane;
using Vsky.Models;
using Vsky.Models.Expens;

namespace Vsky.Api.AutoMapper
{
    public class AutoMapperConfiguration : Profile
    {
        public AutoMapperConfiguration()
        {
            CreateMap<ApplicationUser, UserModel>();
            CreateMap<UserModel, ApplicationUser>();

            CreateMap<Contact, ContactModel>();
            CreateMap<ContactModel, Contact>();

            CreateMap<Country, CountryModel>();
            CreateMap<CountryModel, Country>();

            CreateMap<StateProvince, StateProvinceModel>();
            CreateMap<StateProvinceModel, StateProvince>();

            CreateMap<Company, CompanyModel>();
            CreateMap<CompanyModel, Company>();

            CreateMap<CustomerFiles, CustomerFilesModel>();
            CreateMap<CustomerFilesModel, CustomerFiles>();

            CreateMap<Project, ProjectModel>();
            CreateMap<ProjectModel, Project>();

            CreateMap<Site, SiteModel>();
            CreateMap<SiteModel, Site>();

            CreateMap<SitesModifiedLogs, SitesModifiedLogsModel>();
            CreateMap<SitesModifiedLogsModel, SitesModifiedLogs>();

            CreateMap<Department, DepartmentModel>();
            CreateMap<DepartmentModel, Department>();

            CreateMap<DropDown, DropDownModel>();
            CreateMap<DropDownModel, DropDown>();

            CreateMap<DropDown, DropDownViewModel>();
            CreateMap<DropDown, DropDownViewModel>();

            CreateMap<DropDownType, DropDownTypeModel>();
            CreateMap<DropDownTypeModel, DropDownType>();

            CreateMap<DropDownType, DropDownTypeViewModel>();
            CreateMap<DropDownType, DropDownTypeViewModel>();

            CreateMap<Employee, EmployeeModel>();
            CreateMap<EmployeeModel, Employee>();

            CreateMap<ProjectActivity, ProjectActivityModel>();
            CreateMap<ProjectActivityModel, ProjectActivity>();

            CreateMap<ProjectTask, ProjectTaskModel>();
            CreateMap<ProjectTaskModel, ProjectTask>();

            CreateMap<Tags, TagModels>();
            CreateMap<TagModels, Tags>();

            CreateMap<ProjectTask_Tags, ProjectTask_TagModels>();
            CreateMap<ProjectTask_TagModels, ProjectTask_Tags>();

            CreateMap<ProjectTags, ProjectTagsModel>();
            CreateMap<ProjectTagsModel, ProjectTags>();

            CreateMap<ProjectModule, ProjectModuleModel>();
            CreateMap<ProjectModuleModel, ProjectModule>();

            CreateMap<ProjectEmployeeMapping, ProjectEmployeeMappingModel>();
            CreateMap<ProjectEmployeeMappingModel, ProjectEmployeeMapping>();

            CreateMap<Lead, LeadModels>();
            CreateMap<LeadModels, Lead>();

            CreateMap<ApplicationRole, RoleModel>();
            CreateMap<RoleModel, ApplicationRole>();

            CreateMap<LeadActivityLogs, LeadActivityLogsModel>();
            CreateMap<LeadActivityLogsModel, LeadActivityLogs>();

            CreateMap<LeadStages, LeadStagesModel>();
            CreateMap<LeadStagesModel, LeadStages>();

            CreateMap<LeadActivities, LeadActivitiesModel>();
            CreateMap<LeadActivitiesModel, LeadActivities>();

            CreateMap<LeadUserGroupMapping, LeadUserGroupMappingModel>();
            CreateMap<LeadUserGroupMappingModel, LeadUserGroupMapping>();

            CreateMap<SetReminder, SetReminderModels>();
            CreateMap<SetReminderModels, SetReminder>();
            CreateMap<CompanyContacts, CompanyContactsModels>();
            CreateMap<CompanyContactsModels, CompanyContacts>();

            CreateMap<Address, AddressModels>();
            CreateMap<AddressModels, Address>();

            CreateMap<Person, PersonModel>();
            CreateMap<PersonModel, Person>();

            CreateMap<PersonSitesMapping, PersonSitesMappingModel>();
            CreateMap<PersonSitesMappingModel, PersonSitesMapping>();

            CreateMap<DailyPlanner, DailyPlannerModel>();
            CreateMap<DailyPlannerModel, DailyPlanner>();

            CreateMap<DailyPlannerLine, DailyPlannerLineModel>();
            CreateMap<DailyPlannerLineModel, DailyPlannerLine>();

            CreateMap<Timesheet, TimesheetModel>();
            CreateMap<TimesheetModel, Timesheet>();

            CreateMap<TimesheetLines, TimesheetLinesModel>();
            CreateMap<TimesheetLinesModel, TimesheetLines>();

            CreateMap<TimesheetAISummary, TimesheetAISummaryModel>();
            CreateMap<TimesheetAISummaryModel, TimesheetAISummary>();

            CreateMap<EmployeeType, EmployeeTypeModel>();
            CreateMap<EmployeeTypeModel, EmployeeType>();            
            
            CreateMap<SalesPerson, SalesPersonModel>();
            CreateMap<SalesPersonModel, SalesPerson>();            
            
            CreateMap<CompanyClients, CompanyClientsModel>();
            CreateMap<CompanyClientsModel, CompanyClients>();

            CreateMap<EmployeeStatus, EmployeeStatusModel>();
            CreateMap<EmployeeStatusModel, EmployeeStatus>();

            CreateMap<EmployeeDepartment, EmployeeDepartmentModel>();
            CreateMap<EmployeeDepartmentModel, EmployeeDepartment>();

            CreateMap<EmployeeDesignation, EmployeeDesignationModel>();
            CreateMap<EmployeeDesignationModel, EmployeeDesignation>();

            CreateMap<EmployeeClientLocation, EmployeeClientLocationModel>();
            CreateMap<EmployeeClientLocationModel, EmployeeClientLocation>();

            CreateMap<EmployeeOrgLocation, EmployeeOrgLocationModel>();
            CreateMap<EmployeeOrgLocationModel, EmployeeOrgLocation>();

            CreateMap<Notes, NoteModel>();
            CreateMap<NoteModel, Notes>();

            CreateMap<LeaveCredit, LeaveCreditModel>();
            CreateMap<LeaveCreditModel, LeaveCredit>();

            CreateMap<LeaveRules, LeaveRulesModel>();
            CreateMap<LeaveRulesModel, LeaveRules>();

            CreateMap<LeaveRuleLines, LeaveRuleLinesModel>();
            CreateMap<LeaveRuleLinesModel, LeaveRuleLines>();

            CreateMap<LeaveSchedules, LeaveScheduleModels>();
            CreateMap<LeaveScheduleModels, LeaveSchedules>();

            CreateMap<EmployeeLeave, EmployeeLeaveModel>();
            CreateMap<EmployeeLeaveModel, EmployeeLeave>();

            CreateMap<TestPlan, TestPlanModel>();
            CreateMap<TestPlanModel, TestPlan>();

            CreateMap<TestCase, TestCaseModel>();
            CreateMap<TestCaseModel, TestCase>();

            CreateMap<Issue, IssueModel>();
            CreateMap<IssueModel, Issue>();

            CreateMap<Modules, ModulesModel>();
            CreateMap<ModulesModel, Modules>();

            CreateMap<ModulesMenus, ModulesMenusModel>();
            CreateMap<ModulesMenusModel, ModulesMenus>();            
            
            CreateMap<SitesModules, SitesModulesModel>();
            CreateMap<SitesModulesModel, SitesModules>();

            CreateMap<Requirement, RequirementModel>();
            CreateMap<RequirementModel, Requirement>();

            CreateMap<RequirementGroup, RequirementGroupModel>();
            CreateMap<RequirementGroupModel, RequirementGroup>();

            CreateMap<RequirementChangeLog, RequirementChangeLogModel>();
            CreateMap<RequirementChangeLogModel, RequirementChangeLog>();

            CreateMap<FilePathDetails, FilePathDetailsModel>();
            CreateMap<FilePathDetailsModel, FilePathDetails>();

            CreateMap<AdPost, AdPostModel>();
            CreateMap<AdPostModel, AdPost>();

            CreateMap<AdPostChannel, AdPostChannelModel>();
            CreateMap<AdPostChannelModel, AdPostChannel>();

            CreateMap<AdPostingStatus, AdPostingStatusModel>();
            CreateMap<AdPostingStatusModel, AdPostingStatus>();

            CreateMap<Server, ServerModel>();
            CreateMap<ServerModel, Server>();

            CreateMap<ServerPayments, ServerPaymentsModel>();
            CreateMap<ServerPaymentsModel, ServerPayments>();

            CreateMap<Domain, DomainModel>();
            CreateMap<DomainModel, Domain>();

            CreateMap<DomainAttributes, DomainAttributesModel>();
            CreateMap<DomainAttributesModel, DomainAttributes>();

            CreateMap<InfraAccount, InfraAccountModel>();
            CreateMap<InfraAccountModel, InfraAccount>();

            CreateMap<InfraFTP, InfraFTPModel>();
            CreateMap<InfraFTPModel, InfraFTP>();

            CreateMap<InfraDatabase, InfraDatabaseModel>();
            CreateMap<InfraDatabaseModel, InfraDatabase>();

            CreateMap<InfraProjectInstance, InfraProjectInstanceModel>();
            CreateMap<InfraProjectInstanceModel, InfraProjectInstance>();

            CreateMap<TimeInTimeOut, TimeInTimeOutModel>();
            CreateMap<TimeInTimeOutModel, TimeInTimeOut>();

            CreateMap<TimeInTimeOutBreakDetail, TimeInTimeOutBreakDetailModel>();
            CreateMap<TimeInTimeOutBreakDetailModel, TimeInTimeOutBreakDetail>();

            CreateMap<Inventory, InventoryModel>();
            CreateMap<InventoryModel, Inventory>();

            CreateMap<InventoryAssignment, InventoryAssignmentModel>();
            CreateMap<InventoryAssignmentModel, InventoryAssignment>();

            CreateMap<InventoryItemType, InventoryItemTypeModels>();
            CreateMap<InventoryItemTypeModels, InventoryItemType>();

            CreateMap<TrainingPortal, TrainingPortalModel>();
            CreateMap<TrainingPortalModel, TrainingPortal>();

            CreateMap<Training_Portal_Mapping, TrainingPortalMappingModels>();
            CreateMap<TrainingPortalMappingModels, Training_Portal_Mapping>();

            //CreateMap<TaskChangeLog, TaskChangeLogModel>();
            //CreateMap<TaskChangeLogModel, TaskChangeLog>();

            CreateMap<ProjectTaskStatusLog, ProjectTaskStatusLogModel>();
            CreateMap<ProjectTaskStatusLogModel, ProjectTaskStatusLog>();

            CreateMap<IssueStatusChangedLog, IssueStatusChangedLogModel>();
            CreateMap<IssueStatusChangedLogModel, IssueStatusChangedLog>();

            CreateMap<IssueActivity, IssueActivityModel>();
            CreateMap<IssueActivityModel, IssueActivity>();            
            
            CreateMap<ReportSettings, ReportSettingsModel>();
            CreateMap<ReportSettingsModel, ReportSettings>();

            CreateMap<Notification, NotificationsModel>();
            CreateMap<NotificationsModel, Notification>();

            CreateMap<NotificationDetails, NotificationDetailsModel>();
            CreateMap<NotificationDetailsModel, NotificationDetails>();

            CreateMap<NotificationPermissions, NotificationPermissionsModel>();
            CreateMap<NotificationPermissionsModel, NotificationPermissions>();

            CreateMap<MasterNotification, MasterNotificationModel>();
            CreateMap<MasterNotificationModel, MasterNotification>();

            CreateMap<JobCreate, JobCreateModel>();
            CreateMap<JobCreateModel, JobCreate>();

            CreateMap<Candidate, CandidateModels>();
            CreateMap<CandidateModels, Candidate>();

            CreateMap<CandidateActivities, CandidateActivitiesModels>();
            CreateMap<CandidateActivitiesModels, CandidateActivities>();

            CreateMap<CandidateFeedback, CandidateFeedbackModel>();
            CreateMap<CandidateFeedbackModel, CandidateFeedback>();

            CreateMap<CandidateDepartments, CandidateDepartmentModel>();
            CreateMap<CandidateDepartmentModel, CandidateDepartments>();

            CreateMap<CandidateNotes, CandidateNotesModel>();
            CreateMap<CandidateNotesModel, CandidateNotes>();

            CreateMap<HelpDeskTopic, HelpDeskTopicModel>();
            CreateMap<HelpDeskTopicModel, HelpDeskTopic>();

            CreateMap<HelpDeskTopicQuestions, HelpDeskTopicQuestionsModel>();
            CreateMap<HelpDeskTopicQuestionsModel, HelpDeskTopicQuestions>();

            CreateMap<HelpDesk, HelpDeskModel>();
            CreateMap<HelpDeskModel, HelpDesk>();

            CreateMap<HelpDeskStatusLog, HelpDeskStatusLogModel>();
            CreateMap<HelpDeskStatusLogModel, HelpDeskStatusLog>();

            CreateMap<HelpDeskFiles, HelpDeskFilesModel>();
            CreateMap<HelpDeskFilesModel, HelpDeskFiles>();

            CreateMap<ReportSettingsDetails, ReportSettingsDetailsModel>();
            CreateMap<ReportSettingsDetailsModel, ReportSettingsDetails>();

            CreateMap<Expense_BankAccounts, Expense_BankAccountsModel>();
            CreateMap<Expense_BankAccountsModel, Expense_BankAccounts>();

            CreateMap<Expense, ExpenseModel>();
            CreateMap<ExpenseModel, Expense>();

            CreateMap<Expense_Files, ExpenseFilesModel>();
            CreateMap<ExpenseFilesModel, Expense_Files>();

            CreateMap<Expense_Lines, Expense_LinesModel>();
            CreateMap<Expense_LinesModel, Expense_Lines>();

            CreateMap<ExpenseVendors, ExpenseVendorsModel>();
            CreateMap<ExpenseVendorsModel, ExpenseVendors>();

            CreateMap<ExpenseVendorBankAccounts, ExpenseVendorBankAccountsModel>();
            CreateMap<ExpenseVendorBankAccountsModel, ExpenseVendorBankAccounts>();

            CreateMap<EmployeeOrgStructure, EmployeeOrgStructureModel>();
            CreateMap<EmployeeOrgStructureModel, EmployeeOrgStructure>();
            
            // SwimLanes
            //CreateMap<ProjectSwimLane, ProjectSwimLaneModel>();
            //CreateMap<ProjectSwimLaneModel, ProjectSwimLane>();

            //CreateMap<ProjectSubSwimLane, ProjectSubSwimLaneModel>();
            //CreateMap<ProjectSubSwimLaneModel, ProjectSubSwimLane>();

            //CreateMap<ProjectSubSwimLaneTasks, ProjectSubSwimLaneTasksModel>();
            //CreateMap<ProjectSubSwimLaneTasksModel, ProjectSubSwimLaneTasks>();

            CreateMap<ProjectFiles, ProjectFilesModel>();
            CreateMap<ProjectFilesModel, ProjectFiles>();

            CreateMap<ProjectModuleFiles, ProjectModuleFilesModel>();
            CreateMap<ProjectModuleFilesModel, ProjectModuleFiles>();

            CreateMap<ProjectTaskFiles, ProjectTaskFilesModel>();
            CreateMap<ProjectTaskFilesModel, ProjectTaskFiles>();

            CreateMap<ProjectActivityFiles, ProjectActivityFilesModel>();
            CreateMap<ProjectActivityFilesModel, ProjectActivityFiles>();

            CreateMap<ProjectsMessages, ProjectsMessagesModel>();
            CreateMap<ProjectsMessagesModel, ProjectsMessages>();

            CreateMap<Picture, PicturesModel>();
            CreateMap<PicturesModel, Picture>();

            CreateMap<VW_CustomerFiles, VW_CustomerFilesModel>();
            CreateMap<VW_CustomerFilesModel, VW_CustomerFiles>();
            
            CreateMap<CustomerFilesLines, CustomerFilesLinesModel>();
            CreateMap<CustomerFilesLinesModel, CustomerFilesLines>(); 
            
            CreateMap<SitesRoles, SitesRolesModels>();
            CreateMap<SitesRolesModels, SitesRoles>();

            CreateMap<SitesModulesMenus, SitesModulesMenusModel>();
            CreateMap<SitesModulesMenusModel, SitesModulesMenus>();

            CreateMap<SitesModulesMenusPermissions, SitesModulesMenusPermissionsModel>();
            CreateMap<SitesModulesMenusPermissionsModel, SitesModulesMenusPermissions>();

            CreateMap<ProjectSwimLanes, ProjectSwimLanesModel>();
            CreateMap<ProjectSwimLanesModel, ProjectSwimLanes>();

            CreateMap<ProjectUserMapping, ProjectUserMappingModel>();
            CreateMap<ProjectUserMappingModel, ProjectUserMapping>();
            
            CreateMap<Expense_Advance_Requests, Expense_Advance_Requests_Models>();
            CreateMap<Expense_Advance_Requests_Models, Expense_Advance_Requests>();

            CreateMap<Expense_Purchase_Requests, Expense_Purchase_Requests_Models>();
            CreateMap<Expense_Purchase_Requests_Models, Expense_Purchase_Requests>();

            CreateMap<Website_Demos, Website_DemosModel>();
            CreateMap<Website_DemosModel, Website_Demos>();

            CreateMap<Website_Demo_Modules, Website_Demo_ModulesModel>();
            CreateMap<Website_Demo_ModulesModel, Website_Demo_Modules>();

            CreateMap<ReportUserMapping, ReportUserMappingModel>();
            CreateMap<ReportUserMappingModel, ReportUserMapping>();

            CreateMap<ReportRoleGroupMapping, ReportRoleGroupMappingModel>();
            CreateMap<ReportRoleGroupMappingModel, ReportRoleGroupMapping>();

            CreateMap<ExpensePurchaseRequestFiles, ExpensePurchaseRequestFilesModel>();
            CreateMap<ExpensePurchaseRequestFilesModel, ExpensePurchaseRequestFiles>();

            CreateMap<ExpenseAdvanceRequestFiles, ExpenseAdvanceRequestFilesModel>();
            CreateMap<ExpenseAdvanceRequestFilesModel, ExpenseAdvanceRequestFiles>();

            CreateMap<MessageTemplate, MessageTemplateModel>();
            CreateMap<MessageTemplateModel, MessageTemplate>();

            CreateMap<SOPTemplate, SOPTemplateModel>();
            CreateMap<SOPTemplateModel, SOPTemplate>();

            CreateMap<SOPTemplateSection, SOPTemplateSectionModel>();
            CreateMap<SOPTemplateSectionModel, SOPTemplateSection>();

            CreateMap<SOPTemplateSectionItems, SOPTemplateSectionItemsModel>();
            CreateMap<SOPTemplateSectionItemsModel, SOPTemplateSectionItems>();

            CreateMap<SOPAssignment, SOPAssignmentModel>();
            CreateMap<SOPAssignmentModel, SOPAssignment>();

            CreateMap<SOPAssignmentResponse, SOPAssignmentResponseModel>();
            CreateMap<SOPAssignmentResponseModel, SOPAssignmentResponse>();

            CreateMap<SOPAssignmentResponseEvidences, SOPAssignmentResponseEvidencesModel>();
            CreateMap<SOPAssignmentResponseEvidencesModel, SOPAssignmentResponseEvidences>();

            CreateMap<SOPProcess, SOPProcessModel>();
            CreateMap<SOPProcessModel, SOPProcess>();
        }
    }
}