import { useTagsDropdown } from "composables/form-inputs/useDropdown.js";
import projectTaskService from "modules/project-tasks/projectTasks.service";

export default function tagModule () {
  const tagsDropdown = useTagsDropdown(projectTaskService.getAllTagsListForDropdown);

  return {
    tagsDropdown
  };
}
