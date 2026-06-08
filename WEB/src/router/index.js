import { route } from "quasar/wrappers";
import { createRouter, createMemoryHistory, createWebHistory, createWebHashHistory } from "vue-router";
import routes from "./routes";

/*
 * If not building with SSR mode, you can
 * directly export the Router instantiation;
 *
 * The function below can be async too; either use
 * async/await or return a Promise which resolves
 * with the Router instance.
 */

import authRoutes from "modules/auth/routes";
import accountRoutes from "modules/account/routes";
import employeeRoutes from "modules/employee/routes";
import leaveRoutes from "modules/leave/routes";
import timeInTimeOutRoutes from "modules/timeInTimeOut/routes";
import projectRoutes from "modules/project/routes";
import projectCenterRoutes from "modules/project-center/routes";
import projectPlanningRoutes from "modules/project-planning/routes";
import projectTargetplanRoutes from "modules/project-targetplan/routes";
import tasksRoutes from "modules/project-tasks/routes";
import tasksActivitiesRoutes from "modules/project-tasks-activities/routes";
import issueRoutes from "modules/issue/routes";
import companyRoutes from "modules/company/routes";
import userManagementRoutes from "modules/user-management/routes";
import leadsRoutes from "modules/lead/routes";
import rolesRoutes from "modules/roles/routes";
import sitesRoutes from "modules/sites/routes";
import reportRoutes from "modules/reports/routes";
import dashboardRoutes from "modules/dashboard/routes";
import personRoutes from "modules/person/routes";
import projectModuleRoutes from "modules/project-modules/routes";
import departmentRoutes from "modules/department/routes";
import dropdownsModuleRoutes from "modules/dropdown/routes";
import notificationsTypeRoutes from "modules/notification/routes"; // Notifications pushed 16-09-24
import leaveRulesRoutes from "modules/leave-rules/routes";
import companyContactsRoutes from "modules/company-contacts/routes";
import customerRoutes from "modules/customer/routes";
import mydailyplannerRoutes from "modules/my-daily-planner/routes";
import timesheetRoutes from "modules/timesheet/routes";
import testplansRoutes from "modules/test-plan/routes";
import testcasesRoutes from "modules/test-case/routes";
import moduleRoutes from "modules/module/routes";
import requirementGroupRoutes from "modules/requirement-group/routes";
import requirementRoutes from "modules/requirement/routes";
import marketingAdPostRoutes from "modules/marketing-ad-post/routes";
import marketingAdPostChannelRoutes from "modules/marketing-ad-post-channel/routes";
import inventoryRoutes from "modules/inventory/routes";
import trainingPortalRoutes from "modules/training-portal/routes";
import jobPostRoutes from "modules/job-post/routes";
import candidateRoutes from "modules/candidate/routes";
import helpDeskRoutes from "modules/helpdesk/routes";
import leaveYearlyScheduleRoutes from "modules/leave-yearly-schedule/routes";
import financeExpenseRoutes from "modules/finance-expense/routes";
import financeBankAccountRoutes from "modules/finance-bank-account/routes";
import financeExpenseVendorsRoutes from "modules/finance-expense-vendors/routes";
import employeeOrgStructureRoutes from "modules/employee-org-structure/routes";
import tagRoutes from "modules/tags/routes";
import settingsRoutes from "modules/settings/routes";
import customerFileRoutes from "modules/customer-files/routes";
import financeExpenseAdvanceRequestsRoutes from "modules/finance-expense-advance/routes";
import financeExpensePurchaseRequestRoutes from "modules/finance-expense-purchase-request/routes";
import websiteRoutes from "modules/website/routes";
import movementRegister from "modules/movementRegister/routes";
import itemRoutes from "modules/items/routes";
import infraAccountRoutes from "modules/infra-account/routes";
import infraAccountServicesRoutes from "modules/infra-account-services/routes";
import infraFtpRoutes from "modules/infra-ftp/routes";
import infraProjectInstanceRoutes from "modules/infra-project-instance/routes";
import infraDatabaseRoutes from "modules/infra-database/routes";
import sitesItemsRoutes from "modules/sites-items/routes";
import releaseTrackingRoutes from "modules/project-release-tracking/routes";
import sopTemplateRoutes from "src/modules/sop-template/routes";
import sopAssignmentRoutes from "src/modules/sop-assignment/routes";
import siteSharing from "src/modules/sites-sharing/routes";
import sopProcessRoutes from "src/modules/sop-process/routes";

routes.push(...userManagementRoutes);
routes.push(...companyRoutes);
routes.push(...issueRoutes);
routes.push(...tasksActivitiesRoutes);
routes.push(...tasksRoutes);
routes.push(...projectRoutes);
routes.push(...projectCenterRoutes);
routes.push(...projectPlanningRoutes);
routes.push(...projectTargetplanRoutes);
routes.push(...employeeRoutes);
routes.push(...leaveRoutes);
routes.push(...timeInTimeOutRoutes);
routes.push(...authRoutes);
routes.push(...accountRoutes);
routes.push(...leadsRoutes);
routes.push(...rolesRoutes);
routes.push(...sitesRoutes);
routes.push(...reportRoutes);
routes.push(...dashboardRoutes);
routes.push(...personRoutes);
routes.push(...projectModuleRoutes);
routes.push(...dropdownsModuleRoutes);
routes.push(...departmentRoutes);
routes.push(...leaveRulesRoutes);
routes.push(...notificationsTypeRoutes);// Notifications pushed 16-09-24
routes.push(...companyContactsRoutes);
routes.push(...customerRoutes);
routes.push(...mydailyplannerRoutes);
routes.push(...timesheetRoutes);
routes.push(...testplansRoutes);
routes.push(...testcasesRoutes);
routes.push(...moduleRoutes);
routes.push(...requirementGroupRoutes);
routes.push(...requirementRoutes);
routes.push(...marketingAdPostRoutes);
routes.push(...marketingAdPostChannelRoutes);
routes.push(...inventoryRoutes);
routes.push(...trainingPortalRoutes);
routes.push(...jobPostRoutes);
routes.push(...candidateRoutes);
routes.push(...leaveYearlyScheduleRoutes);
routes.push(...helpDeskRoutes);
routes.push(...financeExpenseRoutes);
routes.push(...financeBankAccountRoutes);
routes.push(...financeExpenseVendorsRoutes);
routes.push(...employeeOrgStructureRoutes);
routes.push(...tagRoutes);
routes.push(...settingsRoutes);
routes.push(...customerFileRoutes);
routes.push(...financeExpenseAdvanceRequestsRoutes);
routes.push(...financeExpensePurchaseRequestRoutes);
routes.push(...websiteRoutes);
routes.push(...movementRegister);
routes.push(...itemRoutes);
routes.push(...infraAccountRoutes);
routes.push(...infraAccountServicesRoutes);
routes.push(...infraFtpRoutes);
routes.push(...infraProjectInstanceRoutes);
routes.push(...infraDatabaseRoutes);
routes.push(...sitesItemsRoutes);
routes.push(...releaseTrackingRoutes);
routes.push(...sopTemplateRoutes);
routes.push(...sopAssignmentRoutes);
routes.push(...siteSharing);
routes.push(...sopProcessRoutes);

export default route(function (/* { store, ssrContext } */) {
  const createHistory = process.env.SERVER
    ? createMemoryHistory
    : (process.env.VUE_ROUTER_MODE === "history" ? createWebHistory : createWebHashHistory);

  const Router = createRouter({
    scrollBehavior: () => ({ left: 0, top: 0 }),
    routes,

    // Leave this as is and make changes in quasar.conf.js instead!
    // quasar.conf.js -> build -> vueRouterMode
    // quasar.conf.js -> build -> publicPath
    history: createHistory(process.env.VUE_ROUTER_BASE)
  });
  Router.beforeEach((to, from, next) => {
    const token = localStorage.getItem("access_token");
    const lastRoute = localStorage.getItem("last_route");

    // Always allow login page
    if (to.path === "/auth/login") {
      return next();
    }

    // If user is NOT logged in and route requires auth
    if (to.meta.requiresAuth && !token) {
      return next("/auth/login");
    }

    // If user opens root `/`
    if (to.path === "/" && token) {
      return next(lastRoute || "/dashboard");
    }

    next();
  });
  return Router;
});
