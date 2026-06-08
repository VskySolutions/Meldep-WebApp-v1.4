import { useSingleSelectDropdown, useMultiSelectDropdown } from "composables/form-inputs/useDropdown.js";
import personService from "modules/person/person.service";

export default function personModule () {
  const personNameDropdown = useMultiSelectDropdown(personService.getAllPersonListForDropdown, {
    labelKey: "fullName",
    valueKey: "id"
  });

  const isSharedPersonNameForDropdown = useMultiSelectDropdown(personService.getAllIsSharedPersonListForDropdown, {
    labelKey: "fullName",
    valueKey: "id"
  });

  const personNameDropdownSingleSelect = useSingleSelectDropdown(personService.getAllPersonListForDropdown, {
    labelKey: "fullName",
    valueKey: "id",
    labelFn: (item) => {
      return item.fullName + (item.primaryEmailAddress ? ` (${item.primaryEmailAddress})` : "");
    }
  });
  
  const personPrimaryEmailAddressDropdown = useMultiSelectDropdown(personService.getAllPersonPrimaryEmailAddressListForDropdown, {
    labelKey: "primaryEmailAddress",
    valueKey: "primaryEmailAddress"
  });

  return {
    personNameDropdown,
    personNameDropdownSingleSelect,
    isSharedPersonNameForDropdown,
    personPrimaryEmailAddressDropdown
  };
}
