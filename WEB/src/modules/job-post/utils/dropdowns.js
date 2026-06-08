import { useMultiSelectDropdown, useSingleSelectDropdown } from "composables/form-inputs/useDropdown.js";
import jobPostService from "modules/job-post/jobPost.service";

export default function jobPostModule () {
  const jobPostNameDropdown = useMultiSelectDropdown(jobPostService.getAllJobPostListForDropdown, {
    labelKey: "text",
    valueKey: "value"
  });

  const jobPostNameDropdownSingleSelect = useSingleSelectDropdown(jobPostService.getAllJobPostListForVskyWebsite, {
    labelKey: "text",
    valueKey: "value"
  });
  
  return {
    jobPostNameDropdown,
    jobPostNameDropdownSingleSelect
  };
}
