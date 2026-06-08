import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import commonService from "services/common.service";
import helpDeskService from "modules/helpdesk/helpDesk.service";
import helpDeskTopicsQuestionsService from "modules/helpdesk/helpDeskTopicsQuestions.service.js";
import usersService from "modules/user-management/userManagement.service";

export default function helpDeskModule () {
  const requesterNameForDropdown = useMultiSelectDropdown(helpDeskService.getRequesterDropdown, {
    labelKey: "email",
    valueKey: "email"
  });

  const helpDeskStatusDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id",
    colorKey: "color",
    bgColorKey: "bgColor"
  });

  const helpDeskPriorityDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id",
    colorKey: "color",
    bgColorKey: "bgColor"
  });

  const helpDeskActiveWorkspaceDropdown = useMultiSelectDropdown(helpDeskTopicsQuestionsService.getAllHelpDeskTopicListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });

  const helpDeskAllWorkspaceDropdown = useMultiSelectDropdown(helpDeskTopicsQuestionsService.getAllHelpDeskTopicList, {
    labelKey: "text",
    valueKey: "value"
  });

  const helpDeskMenusDropdown = useMultiSelectDropdown(helpDeskService.getAllHelpDeskTopicQuestionsListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });

  const helpDeskCategoryDropdown = useMultiSelectDropdown(commonService.getDropDown, {
    labelKey: "dropdownValue",
    valueKey: "id"
  });

  const supportTeamUserForDropdown = useSingleSelectDropdown(usersService.getSupportTeamUsersDataForDropdown, {
    labelKey: "supportTeamUserName",
    valueKey: "employeeId"
  });

  return {
    requesterNameForDropdown,
    helpDeskStatusDropdown,
    helpDeskPriorityDropdown,
    helpDeskActiveWorkspaceDropdown,
    helpDeskAllWorkspaceDropdown,
    helpDeskMenusDropdown,
    helpDeskCategoryDropdown,
    supportTeamUserForDropdown
  };
}
