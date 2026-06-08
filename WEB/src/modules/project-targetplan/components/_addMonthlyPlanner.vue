<template>
  <q-dialog ref="chatContainer" v-model="monthPlannerOpen" class="project-message-dialog customDialog dialog-scrollable-content" persistent position="right">
    <q-card style="width: 60vw; max-width: 60vw; height: 100vh;">
      <q-card-section class="bg-primary text-white flex items-center q-pa-sm">
        <div class="q-space flex  fs-16">Add Monthly Plan</div>
        <q-btn dense flat icon="o_close" @click="toggleProjectChatBox" />
      </q-card-section>
      <q-card-section>
        <fieldset>
          <div class="row full-width q-mb-md q-mt-sm">
            <formSingleSelectDropdown
              v-model="model.projectId"
              label="Project Name"
              :options="projectNameDropdownSingleSelect.list.value"
              :filter="projectNameDropdownSingleSelect.filter"
              :wrapperClass="'col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12'"
            />
            <formMonthYearPicker
              v-model="formattedMonthDate"
              label="Month"
              :default-view="currentView"
              :navigation-min-year-month="minMonth"
              wrapper-class="col-md-4 col-sm-6 q-px-sm"
              @update:model-value="onUpdateMonth"
            />
          </div>
          <div class="row full-width">
            <div class="col-12 flex justify-between q-px-sm q-my-md">
              <h3>Planned<span class="required">*</span></h3>
              <q-btn icon="o_add" outline label="Add Lines" size="sm" no-caps class="text-primary btnRounded" @click="onAddProjectPlanLines()" />
            </div>
          </div>
          <div class="row full-width q-px-sm">
            <div v-if="model.weekDateLines?.length > 0" class="col-12 full-width">
              <table class="WhiteTable full-width">
                <thead>
                  <tr>
                    <th class="text-start">Expected Plan<span class="required">*</span></th>
                    <th width="50px">Action</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="(line, lineIndex) in model.weekDateLines" :key="line.id">
                    <td>
                      <div class="row q-mb-sm">
                        <q-editor
                          v-model="line.description"
                          class="full-width "
                          :dense="$q.screen.lt.md"
                          :toolbar="toolbar"
                          :fonts="fonts"
                        />
                      </div>
                      <div class="row">
                        <div class="col-xxl-1 col-xl-1 col-lg-2 col-md-2 col-sm-2 col-xs-2 flex items-center">
                          <q-input v-model="line.expectedHours" outlined hide-bottom-space :dense="true" :readonly="true" label="Est. Hrs*" class="q-mr-sm" />
                        </div>
                        <div class="col-xxl-11 col-xl-11 col-lg-10 col-md-10 col-sm-10 col-xs-10 flex items-center">
                          <div class="TaskActivity flex items-center">
                            <!-- Task Assignment - Action Button -->
                            <div class="q-mr-sm">
                              <q-icon name="o_group_add" color="grey-8" size="sm" class="cursor-pointer" @click="resetResourceModel" />
                              <q-tooltip>Assign Resource?</q-tooltip>
                              <q-popup-edit
                                class="small-popup-title"
                                style="width: 280px;"
                                @before-show="() => {addResourceBtn = true}"
                              >
                                <div class="row justify-between items-center q-mb-sm">
                                  <h3>Add Resource</h3>
                                  <q-btn v-close-popup icon="o_close" size="sm" color="black" flat round dense />
                                </div>
                                <formSingleSelectDropdown
                                  v-model="selectedResourcesOptions"
                                  label="Select Employee"
                                  :options="activeEmployeesDropdownSingleSelect.list.value"
                                  :filter="activeEmployeesDropdownSingleSelect.filter"
                                  @update:model-value="(val) => checkIfEmployeeAlreadyExistsInPlanLine(line.saveWeeklyLinesAssignTos, val)"
                                />
                                <label class="q-mt-sm">Est. Hrs <span class="text-red">*</span></label>
                                <q-input
                                  v-model="resourceModel.estimatedHours"
                                  outlined
                                  hide-bottom-space
                                  dense
                                  :rules="[validateTaskEstimatedHours]"
                                  maxlength="5"
                                  class="full-width"
                                  @keyup="checkAddResourceFields(resourceModel)"
                                />
                                <div class="row justify-end q-gutter-sm q-mt-sm">
                                  <q-btn v-close-popup label="Cancel" color="grey" flat dense />
                                  <q-btn v-close-popup label="Set" color="primary" dense :disable="addResourceBtn" :loading="resourceProcessing" @click="onAddResourceToLine(line, resourceModel)" />
                                </div>
                              </q-popup-edit>
                            </div>
                            <!-- Task Assignment - Labels -->
                            <div v-for="(resource, resourceIndex) in line.saveWeeklyLinesAssignTos" :key="resource.id" class="Person position-relative q-mr-xs">
                              {{ (resource?.firstName?.trim()?.[0] || '') + (resource?.lastName?.trim()?.[0] || '') + "(" + resource.estimatedHours + ")" }}
                              <q-icon class="delete" name="o_close" color="red" size="xs" @click="onDeleteResourceToLine(line, resource, resourceIndex)" />
                              <q-tooltip>
                                <div>
                                  <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                                  <span>{{ resource.firstName + ' ' + resource?.lastName }}</span>
                                </div>
                              </q-tooltip>
                            </div>
                          </div>
                        </div>
                      </div>
                    </td>
                    <td class="text-center">
                      <q-btn :disable="model.weekDateLines?.length > 1 ? false : true" icon="o_delete" outline size="sm" color="red" no-caps class="q-ml-sm" @click="onDeleteProjectPlanLines(lineIndex)" />
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
            <div v-else class="col-12 full-width q-my-md">
              <h6 class="text-red text-center">No plan added</h6>
            </div>
          </div>
          <div class="row full-width flex justify-center q-px-sm q-mt-sm">
            <q-btn size="md" color="grey-4" outline label="Close" type="button" class="text-grey-9 q-mr-xs" style="width: 120px;" no-caps @click="toggleProjectChatBox" />
            <q-btn size="md" color="primary" outline label="Save & Close" type="button" :loading="processing" :disable="disableActionButton" style="width: 120px;" no-caps @click="OnSaveProjectPlan()" />
          </div>
        </fieldset>
      </q-card-section>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import { useQuasar, date, uid } from "quasar";
import { notifyError } from "assets/utils";

import projectService from "modules/project/projects.service";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formMonthYearPicker from "src/components/form-inputs/_formMonthYearPicker.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const resourceProcessing = ref(false);
const processing = ref(false);
const disableActionButton = ref(true);

const props = defineProps({ projectId: { type: String, default: "" } });

const monthPlannerOpen = ref(false);
const toggleProjectChatBox = () => { monthPlannerOpen.value = !monthPlannerOpen.value; };

const model = ref({
  projectId: props?.projectId ? props.projectId : null,
  weekDate: null,
  planTypeId: null,
  weekDateLines: []
});

const getProjectWeeklyPlanType = () => {
  projectService.getProjectWeeklyPlanTypeId("Project Weekly Target Planning", "Monthly").then((resp) => {
    model.value.planTypeId = resp;
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Plan:- Add new project week.
// --------------------------------------------------------------------------------------------------------------------------------------------------
const calendarRef = ref(null);
const qDateProxy = ref(null);
const today = new Date();
const currentView = ref("Years");
const formattedMonthDate = ref("");

// Disable past navigation
const minMonth = date.formatDate(today, "YYYY/MM");

const onUpdateMonth = (val) => {
  if (currentView.value === "Years") {
    currentView.value = "Months";
    calendarRef.value?.setView("Months");
  } else {
    formattedMonthDate.value = val;
    // Convert 'June-2025' or 'MMMM-YYYY' to Date object (1st of month)
    const parsedDate = new Date(val);
    const formatted = date.formatDate(parsedDate, "MM/01/YYYY"); // Day is fixed to 01

    model.value.weekDate = formatted; // Save Date object
    currentView.value = "Years";
    qDateProxy.value?.hide(); // Close popup
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Plan Lines
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onAddProjectPlanLines = () => {
  const lineModel = ref({
    id: uid(),
    description: null,
    saveWeeklyLinesAssignTos: []
  });
  model.value.weekDateLines.unshift(lineModel.value);
};

const onDeleteProjectPlanLines = (lineIndex) => {
  $q.dialog({
    title: "Delete Line?",
    message: "Are you sure you want to delete monthly plan line?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    model.value.weekDateLines?.splice(lineIndex, 1);
  });
};

const OnSaveProjectPlan = async () => {
  processing.value = true;
  try {
    processing.value = true;
    await projectService.addProjectToWeeklyPlan(model.value);
  } catch (error) {
    console.error("Error in submitting the monthly plan:", error);
    notifyError({ message: "An error occurred while saving the monthly plan." });
  } finally {
    setTimeout(() => {
      processing.value = false;
    }, 1500);
    toggleProjectChatBox();
  }
};

const validateTaskEstimatedHours = (value) => {
  const regex = /^(\d+(\.\d{1,2})?)?$/;
  if (!value || (regex.test(value) && value.length <= 5)) {
    return true; // Valid input
  }
  return "Invalid activity hours format.";
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Plan:- Add resource to line under project weeklines.
// --------------------------------------------------------------------------------------------------------------------------------------------------
const addResourceBtn = ref(true);
const selectedResourcesOptions = ref([]);

const resourceModel = ref({
  id: uid(),
  projectWeeklyPlanDatesLineId: "",
  employeeId: null,
  estimatedHours: "",
  firstName: "",
  lastName: ""
});

const resetResourceModel = () => {
  resourceModel.value = {
    id: uid(),
    projectWeeklyPlanDatesLineId: "",
    employeeId: null,
    estimatedHours: "",
    firstName: "",
    lastName: ""
  };
  selectedResourcesOptions.value = [];
};

const checkIfEmployeeAlreadyExistsInPlanLine = async (existingResources, selectOption) => {
  const isEmployeeAdded = existingResources?.some(r => r.employeeId === selectOption.value);
  if (isEmployeeAdded) {
    resourceModel.value.employeeId = null;
    resourceModel.value.firstName = "";
    resourceModel.value.lastName = "";
    selectedResourcesOptions.value = [];
    notifyError({ message: "Cannot Add Duplicate Employee" });
  } else {
    const selectedEmployee = activeEmployeesDropdownSingleSelect.list.value.find(
      (employee) => employee.value === selectOption
    );

    if (!selectedEmployee) {
      notifyError({ message: "Employee not found" });
      return;
    }

    const fullName = selectedEmployee.text?.trim?.() || "";
    const nameParts = fullName.trim().split(" ");
    resourceModel.value.employeeId = selectedResourcesOptions.value;
    resourceModel.value.firstName = nameParts[0];
    resourceModel.value.lastName = nameParts.length > 1 ? nameParts.slice(1).join(" ") : "";
    checkAddResourceFields(resourceModel.value);
  }
};

const checkAddResourceFields = (resource) => {
  const hasMissingData =
    !resource?.employeeId ||
    !resource?.estimatedHours ||
    !isPositiveNumberOrDecimal(resource.estimatedHours);
  addResourceBtn.value = hasMissingData;
};

const isPositiveNumberOrDecimal = (value) => {
  const regex = /^\d+(\.\d+)?$/;
  return regex.test(String(value));
};

const onAddResourceToLine = async (line, resource) => {
  resourceProcessing.value = true;
  try {
    if (!line.saveWeeklyLinesAssignTos) {
      line.saveWeeklyLinesAssignTos = [];
    }
    resource.projectWeeklyPlanDatesLineId = line.id;
    line.saveWeeklyLinesAssignTos.push({ ...resource });
    getTotalWeekLineExpectedHours(line);
  } catch (error) {
    console.error("Error in submitting the resource:", error);
  } finally {
    resourceProcessing.value = true;
    setTimeout(() => {
      resourceProcessing.value = false;
    }, 1500);
  }
};

const onDeleteResourceToLine = (line, resource, resourceIndex) => {
  $q.dialog({
    title: "Remove Employee From Plan?",
    message: "Are you sure you want to remove this employee from plan?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    line.saveWeeklyLinesAssignTos.splice(resourceIndex, 1);
    getTotalWeekLineExpectedHours(line);
    notifyError({ message: "Resource Deleted" });
  });
};

const getTotalWeekLineExpectedHours = (line = []) => {
  const totalWeekHours = line.saveWeeklyLinesAssignTos.reduce((total, assignTo) => {
    const hours = parseFloat(assignTo.estimatedHours);
    return total + (isNaN(hours) ? 0 : hours);
  }, 0).toFixed(2);
  line.expectedHours = totalWeekHours;
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { projectNameDropdownSingleSelect
} = projectModule();

const { activeEmployeesDropdownSingleSelect } = employeeModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Search: When user types in search
// --------------------------------------------------------------------------------------------------------------------------------------------------
watch(
  () => model.value,
  (newModelValue) => {
    const hasMissingData =
      !newModelValue?.projectId?.length ||
      !newModelValue?.weekDate?.length ||
      newModelValue?.weekDateLines?.some(line => !line?.expectedHours || line.expectedHours.trim() === "") ||
      newModelValue?.weekDateLines?.some(line => !line?.description || line.description.trim() === "" || line.description.trim() === "<br>")
    ;
    disableActionButton.value = hasMissingData;
  },
  { deep: true }
);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Page Load
// --------------------------------------------------------------------------------------------------------------------------------------------------
onMounted(() => {
  projectNameDropdownSingleSelect.load();
  onAddProjectPlanLines();
  getProjectWeeklyPlanType();
  activeEmployeesDropdownSingleSelect.load();
});

</script>
<style scoped>
.TaskActivity .Person {
  border-radius: 10%;
  background-color: #5d5d5d;
  color: white;
  font-size: 12px;
  padding: 2px 3px;
}
</style>
