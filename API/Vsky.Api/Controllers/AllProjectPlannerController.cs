using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Vsky.Api.Extensions;
using Vsky.Api.Models;
using System.Linq;
using Vsky.Services.Common;
using Vsky.Services.DropDowns;
using Vsky.Services.Dashboard;
using Vsky.Models;
using Vsky.Services.DropDownTypes;

namespace Vsky.Api.Controllers
{
    [Route("all-project-planner")]
    public class AllProjectPlannerController : BaseController
    {
        #region Define Services
        private readonly GlobalVariable _globalVariable;
        private readonly ICommonService _commonService;
        private readonly IVWDashboardServices _vWDashboardServices;
        private readonly IDropDownService _dropDownService;
        private readonly IDropDownTypeService _dropDownTypeService;
        #endregion

        #region Services Initializations
        public AllProjectPlannerController(
            GlobalVariable globalVariable,
            ICommonService commonService,
            IVWDashboardServices vWDashboardServices, 
            IDropDownService dropDownService,
            IDropDownTypeService dropDownTypeService)
        {
            _globalVariable = globalVariable;
            _commonService = commonService;
            _vWDashboardServices = vWDashboardServices;
            _dropDownService = dropDownService;
            _dropDownTypeService = dropDownTypeService;
        }
        #endregion

        #region GetAllVWCustomerList
        [HttpPost("get-all-vw-customer-list")]
        public async Task<IActionResult> GetAllVWCustomerList(VW_CustomerSearchModel model)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                int Status = 2;
                if (!string.IsNullOrWhiteSpace(model.StatusId))
                {
                    Status = model.StatusId == "Active" ? 1 : (model.StatusId == "Inactive" ? 0 : 2);
                }

                var List = await _vWDashboardServices.GetAllCustomerList(
                    SiteId,
                    model.SearchText,
                    logginuser: LoggedUserId,
                    model.ProjectId,
                    model.CustomerIds,
                    model.CustomerTypeIds,
                    model.CustomerAssignToIds,
                    model.ParentCustomerIds,
                    model.ProjectTypeIds,
                    model.ProjectIds,
                    model.ProjectStatusIds,
                    model.ProjectPriorityIds,
                    model.ProjectCoordinatorIds,
                    model.ProjectLeadsIds,
                    Status,
                    model.CompanyContactIds,
                    model.Year,
                    model.SortBy,
                    model.Descending,
                    model.Page,
                    model.PageSize
                );
                var Data = new VW_CustomerList
                {
                    CustomerList = List,
                    Total = List.TotalCount
                };
                return Ok(Data);

            }
            catch (Exception ex)
            {
                return Ok(ex.Message + ":-" + ex.InnerException);
            }
        }
        #endregion

        #region GetAllProjectsPlannerList
        // Title: GetAllProjectsPlannerList
        [HttpPost("get-all-project-planner-list")]
        public async Task<IActionResult> GetAllProjectsPlannerList(AllProjectPlannerSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;
                var year =  searchModel.Year == "" ? 0 : Convert.ToUInt16(searchModel.Year);

                int Status = 2;
                //if (!string.IsNullOrWhiteSpace(searchModel.StatusId))
                //{
                //    var activeStatus = _dropDownService.GetDropDownById(searchModel.StatusId).GetAwaiter().GetResult();
                //    Status = activeStatus.DropDownValue == "Active" ? 1 : (activeStatus.DropDownValue == "Inactive" ? 0 : 2);
                //}
                //if (!string.IsNullOrWhiteSpace(searchModel.StatusId))
                //{
                //    //var activeStatus = _dropDownService.GetDropDownById(model.StatusId).GetAwaiter().GetResult();
                //    //Status = activeStatus.DropDownValue == "Active" ? 1 : (activeStatus.DropDownValue == "Inactive" ? 0 : 2);D
                //    Status = searchModel.StatusId == "Active" ? 1 : (searchModel.StatusId == "Inactive" ? 0 : 2);
                //}
                if (!string.IsNullOrWhiteSpace(searchModel.StatusId))
                {
                    var activeStatus = _dropDownService.GetDropDownById(searchModel.StatusId).GetAwaiter().GetResult();
                    Status = activeStatus.DropDownValue == "Active" ? 1 : (activeStatus.DropDownValue == "Inactive" ? 0 : 2);
                }

                var list = await _vWDashboardServices.GetAllCustomerProjectList(
                    SiteId: SiteId, 
                    filterProject: searchModel.filterProject, 
                    logginuser: LoggedUserId,
                    searchModel.SearchText,
                    CustomerIds: searchModel.CustomerIds, 
                    ProjectTypeIds: searchModel.ProjectTypeIds,
                    ProjectIds:searchModel.ProjectIds, 
                    ProjectStatusIds: searchModel.ProjectStatusIds,
                    ProjectPriorityIds: searchModel.ProjectPriorityIds,
                    ProjectCoordinatorIds: searchModel.ProjectCoordinatorIds,
                    ProjectLeadsIds: searchModel.ProjectLeadsIds,
                    Status: Status,
                    CompanyContactIds: searchModel.CompanyContactIds,
                    Year:searchModel.Year,  
                    SortBy:searchModel.SortBy, 
                    Descending: searchModel.Descending, 
                    page: searchModel.Page, 
                    pageSize: searchModel.PageSize
                );

                return Ok(new
                {
                    Data = list,
                    Total = list.TotalCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllProjectsModulesPlannerList
        // Title: GetAllProjectsModulesPlannerList
        [HttpPost("get-all-project-module-planner-list")]
        public async Task<IActionResult> GetAllProjectsModulesPlannerList(ProjectModuleSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var projectIdList = new List<string>();
                if (!string.IsNullOrEmpty(searchModel.ProjectId))
                    projectIdList.Add(searchModel.ProjectId);

                var list =await _vWDashboardServices.GetAllProjectModulesList(SiteId, isShowCloseStatus: searchModel.isShowCloseStatus, searchModel.filterModule,   LoggedUserId: LoggedUserId, ProjectModuleIds:searchModel.ProjectModuleIds, projectIdList, ProjectModuleStatusIds: searchModel.ProjectModuleStatusIds, searchModel.SortBy, searchModel.Descending, searchModel.Page, searchModel.PageSize);
                return Ok(new
                {
                    Data = list,
                    Total = list.TotalCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllProjectsTaskPlannerList
        // Title: GetAllProjectsTaskPlannerList
        [HttpPost("get-all-project-task-planner-list")]
        public async Task<IActionResult> GetAllProjectsTaskPlannerList(ProjectTaskSearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;

                var projectIdList = new List<string>();
                var projectSwimlaneIdList = new List<string>();
                var projectModuleIdList = new List<string>();

                if (!string.IsNullOrEmpty(searchModel.ProjectId))
                    projectIdList.Add(searchModel.ProjectId);

                if (!string.IsNullOrEmpty(searchModel.ProjectSwimlaneId))
                    projectSwimlaneIdList.Add(searchModel.ProjectSwimlaneId);

                if (!string.IsNullOrEmpty(searchModel.ProjectModuleId))
                    projectModuleIdList.Add(searchModel.ProjectModuleId);

                var list =await _vWDashboardServices.GetAllProjectTaskList(
                    SiteId: SiteId, 
                    isShowCloseStatus:searchModel.isShowCloseStatus, 
                    taskName:searchModel.Name ,
                    filterTask: searchModel.filterTask, 
                    LoggedUserId: LoggedUserId, 
                    ProjectId: projectIdList, 
                    ProjectSwimlaneId: projectSwimlaneIdList, 
                    ProjectModuleId: projectModuleIdList,
                    StatusId:searchModel.StatusIds,
                    PriorityId:searchModel.PriorityIds,
                    AssignedToId: searchModel.AssignedToIds,
                    tagIds:searchModel.TaskTagsIds, 
                    SortBy: searchModel.SortBy, 
                    Descending: searchModel.Descending, 
                    page: searchModel.Page, 
                    pageSize: searchModel.PageSize);
                return Ok(new
                {
                    Data = list,
                    Total = list.TotalCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region GetAllProjectsActivityPlannerList
        // Title: GetAllProjectsActivityPlannerList
        [HttpPost("get-all-project-activity-planner-list")]
        public async Task<IActionResult> GetAllProjectsActivityPlannerList(ProjectActivitySearchModel searchModel)
        {
            try
            {
                var LoggedUserId = User.GetLoggedInUserId<string>();
                var SiteId = _globalVariable.SiteId;


                var list =await _vWDashboardServices.GetAllProjectActivitiesList(SiteId,
                    searchModel.isShowCloseStatus, 
                    searchModel.isShowCompleteStatus, 
                    searchModel.filterActivity, 
                    searchModel.ProjectId, 
                    LoggedUserId, 
                    searchModel.ProjectSwimlaneId,
                    searchModel.ProjectModuleId,
                    searchModel.ProjectTaskId, 
                    searchModel.SortBy, 
                    searchModel.Descending,
                    searchModel.Page,
                    searchModel.PageSize
                );

                var activityDropDownType = await _dropDownTypeService.GetDropDownType(SiteId, "Project Activities");
                var activityDropdowns = await _dropDownService.GetDropDowns(activityDropDownType.Id);

                foreach (var item in list)
                {
                    if (!string.IsNullOrEmpty(item.ActivityName))
                    {
                        var activityDropdownItem = activityDropdowns.FirstOrDefault(d => d.DropDownValue == item.ActivityName);
                        if (activityDropdownItem != null)
                        {
                            item.ActivityNameDescription = activityDropdownItem.Description;
                        }
                    }
                }

                return Ok(new
                {
                    Data = list,
                    Total = list.TotalCount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
