<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Project Module</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable', readonlyProject != '' ? 'edit_projectModule' : '']">
          <div class="q-gutter-y-md">
            <q-card>
              <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
                <q-tab v-if="!isCharter" name="1_tab" label="Project Module Info." class="q-px-lg q-mr-md" />
                <q-tab name="2_tab" label="Project Module Charter" class="q-px-lg" :disable="disableTab" />
              </q-tabs>
              <q-separator />
              <q-tab-panels v-model="tab" animated>
                <q-tab-panel v-if="!isCharter" name="1_tab">
                  <fieldset>
                    <legend>Module Info</legend>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <formSingleSelectDropdown
                        v-model="model.projectId"
                        label="Project Name"
                        :readonly="!!readonlyProject"
                        :class="readonlyProject !== '' ? 'edit_tasks' : ''"
                        :options="projectNameDropdownSingleSelect.list.value"
                        :filter="projectNameDropdownSingleSelect.filter"
                        @update:model-value="getSortOrderByProjectId(model.projectId)"
                        :wrapperClass="'col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12'"
                        :error="v$.projectId.$error"
                        :error-message="v$.projectId.$errors[0]?.$message"
                      >
                        <template #after>
                          <q-icon
                            v-if="!readonlyProject && props.id == ''"
                            name="o_add"
                            color="primary"
                            class="cursor-pointer q-ml-xs add-icon"
                            @click="onProjectAdd(null, refreshProjectDropdownList)"
                          >
                            <q-tooltip>Add new Project</q-tooltip>
                          </q-icon>
                        </template>
                      </formSingleSelectDropdown>
                      <div class="col-12 col-md-6">
                        <label class="label q-mb-xs text-black">Project Module<span class="required">*</span></label>
                        <div>
                          <q-input
                            v-model="model.name" outlined stack-label hide-bottom-space :dense="true"
                            :error="v$.name.$error" :error-message="v$.name.$errors[0]?.$message" @blur="v$.name.$touch"
                          />
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <formDate
                        v-model="model.startDateStr"
                        label="Start Date"
                        :error="v$.startDateStr.$error"
                        :error-message="v$.startDateStr.$errors[0]?.$message"
                        :onBlur="() => v$.startDateStr.$touch()"
                        :dateOptions="optionsFn"
                      />
                      <formDate
                        v-model="model.endDateStr"
                        label="Due Date"
                        :error="v$.endDateStr.$error"
                        :error-message="v$.endDateStr.$errors[0]?.$message"
                        :onBlur="() => v$.endDateStr.$touch()"
                        :dateOptions="disableBeforeStartDate"
                      />
                      <formSingleSelectDropdown
                        v-model="model.projectModuleStatusId"
                        label="Project Module Status"
                        class="project-module-status-dropdown"
                        :wrapperClass="'col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12'"
                        :options="projectModuleStatusForDropdownSingleSelect.list.value"
                        :filter="projectModuleStatusForDropdownSingleSelect.filter"
                        :error="v$.projectModuleStatusId.$error"
                        :error-message="v$.projectModuleStatusId.$errors[0]?.$message"
                      />
                      <div class="col-12 col-sm-3 col-md-3 col-lg-3">
                        <div class="text-black">Sort Order<span class="required">*</span></div>
                        <div>
                          <q-input
                            v-model="model.sortOrder"
                            :placeholder="nestSortOrderByLastSortOrder"
                            outlined
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            maxlength="5"
                            :rules="[val => validateSortOrder(val) || 'Enter a valid sort order']"
                            :error="v$.sortOrder.$error"
                            :error-message="v$.sortOrder.$errors[0]?.$message"
                            @blur="v$.sortOrder.$touch"
                          />
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12">
                        <div class="form-group">
                          <label class="label q-mb-xs text-black">Module Description</label>
                          <q-editor
                            v-model="model.description"
                            :dense="$q.screen.lt.md"
                            :toolbar="toolbar"
                            :fonts="fonts"
                            class="relative-position"
                          />
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-12 col-md-12 col-lg-12">
                        <div>
                          <div class="q-mb-xs text-black">Notes</div>
                          <div class="form-group">
                            <q-input v-model="model.notes" outlined autogrow hint="The maximum length allowed is 500." maxlength="500" />
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-lg">
                      <div class="col-12 q-mb-xs text-black">Project Module Files</div>
                      <div class="col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12">
                        <div class="form-group">
                          <multiFileUploader
                            :initialFiles="model.projectModuleFiles"
                            :allowedExtensions="[
                              '.pdf','.xls','.xlsx','.doc','.docx','.jpeg','.jpg','.png','.ppt','.pptx'
                            ]"
                            :maxSizeInMb="25"
                            label="Drag files here or (+) to upload."
                            @files-selected="handleFiles"
                          />
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-lg">
                      <div v-if="model.projectModuleFiles && model.projectModuleFiles.length > 0" class="row q-gutter-md">
                        <div
                          v-for="(file, index) in model.projectModuleFiles"
                          :key="index"
                          class="col-3 position-relative file-card text-center"
                          style="max-width: 140px; min-width: 140px;"
                        >
                          <div class="file-preview square-box">
                            <template v-if="isImageFile(file)">
                              <img
                                :src="file.file.virtualPath ? file.file.virtualPath : getFilePreview(file.file)"
                                alt="File Preview"
                                class="square-content centered-image"
                              >
                            </template>
                            <template v-else>
                              <q-icon
                                :name="getFileIcon(file.file?.mimeType)"
                                class="file-icon square-content"
                                size="70px"
                              />
                            </template>
                            <div class="file-name q-mt-sm">
                              <q-btn
                                v-if="file.file?.virtualPath || file?.name"
                                class="bg-primary text-white q-pa-xs"
                                no-caps
                                @click="viewFile(file)"
                              >
                                <span class="truncate-text">
                                  {{ file.file?.name || file.name || extractFileName(file.file?.seoFilename) }}
                                </span>
                              </q-btn>
                            </div>
                          </div>
                          <!-- Remove Button -->
                          <q-btn
                            color="negative"
                            flat
                            round
                            dense
                            icon="o_close"
                            class="remove-file-icon"
                            @click="removeFile(index)"
                          />
                        </div>
                      </div>
                    </div>
                    <div align="center" class="q-gutter-sm justify-center">
                      <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                      <q-btn v-if="tab === '1_tab'" label="Save & Next" type="submit" color="primary" class="actionBtn" :loading="processing" :disable="processing" no-caps />
                      <q-btn label="Save & Close" type="button" color="primary" class="actionBtn hidden" :loading="processingClose" :disable="processingClose" no-caps @click="onSubmitClose()" />
                    </div>
                  </fieldset>
                </q-tab-panel>
                <q-tab-panel name="2_tab">
                  <fieldset class="q-mb-lg">
                    <div>
                      <q-banner
                        dense
                        rounded
                        class="bg-blue-1 text-primary q-mb-md"
                      >
                        <template #avatar>
                          <q-icon
                            name="o_info"
                            size="sm"
                            color="primary"
                          />
                        </template>

                        <div class="text-body2">
                          <strong>Task List Security:</strong> These permissions apply only to the Task List and do not change project-level security.
                        </div>
                      </q-banner>
                      <q-table
                        ref="tableRef"
                        v-model:pagination="pagination"
                        bordered
                        class="no-shadow"
                        :loading="loading"
                        :rows="rows"
                        :columns="columns"
                        row-key="employeeId"
                        separator="cell"
                        binary-state-sort
                        :rows-per-page-options="[20, 50, 100, 200, 500]"
                      >
                        <template #header-cell="props">
                          <q-th
                            :props="props"
                            class="bg-primary text-white"
                            :style="props.col.width ? `width: ${props.col.width};` : ''"
                          >
                            <q-checkbox
                              v-if="props.col.isSelection"
                              :model-value="allSelected"
                              :indeterminate="someSelected"
                              color="primary"
                              @update:model-value="toggleSelectAll"
                            />

                            <template v-else>
                              {{ props.col.label }}
                            </template>
                          </q-th>
                        </template>

                        <template #body-cell="props">
                          <q-td
                            :props="props"
                            :style="props.col.width ? `width: ${props.col.width};` : ''"
                          >
                            <!-- Employee selection -->
                            <q-checkbox
                              v-if="props.col.isSelection"
                              :model-value="props.row.selected"
                              color="primary"
                              @update:model-value="value => toggleEmployee(props.row, value)"
                            />

                            <!-- Manage / View / Notes -->
                            <q-checkbox
                              v-else-if="props.col.type === 'checkbox'"
                              :model-value="
                                props.row.selected &&
                                props.row[props.col.field]
                              "
                              color="primary"
                              :disable="!props.row.selected"
                              @update:model-value="
                                value => props.row[props.col.field] = value
                              "
                            />

                            <!-- Employee Name -->
                            <template v-else>
                              {{ props.row[props.col.field] }}
                            </template>
                          </q-td>
                        </template>
                      </q-table>
                    </div>
                    <div align="center" class="q-gutter-sm justify-center q-mt-sm">
                      <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                      <q-btn v-if="tab === '1_tab'" label="Save & Next" type="submit" color="primary" class="actionBtn" :loading="processing" :disable="processing" no-caps />
                      <q-btn label="Save & Close" type="button" color="primary" class="actionBtn" :loading="processing" :disable="processing" no-caps @click="onSubmitClose()" />
                    </div>
                  </fieldset>
                </q-tab-panel>
              </q-tab-panels>
            </q-card>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import _ from "lodash";
import { isDate } from "validators/zw_validators.js";
import useFilters from "composables/useFilters";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, watch, onMounted, computed, toRaw } from "vue";
import { notifySuccess, notifyError, zwConfirm, notifyWarning } from "assets/utils";
import { parse } from "date-fns"; // Standard TimeZone Conversion

import projectModuleService from "modules/project-modules/projectModules.service";
import projectService from "modules/project/projects.service";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Project Dialogs
import {
  onProjectAdd
} from "src/modules/project/utils/dialogs.js";

// Shared Inputs
import multiFileUploader from "src/components/form-inputs/_multiFileUpload.vue";

// Define emits
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();
defineEmits([...useDialogPluginComponent.emits]);

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" },
  projectName: { type: String, default: "" },
  projectIdAttr: { type: String, default: "" },
  projectIdValue: { type: String, default: "" },
  startDate: { type: String, default: "" },
  endDate: { type: String, default: "" },
  isCharter: { type: Boolean, default: false }
});

// Common variables
const tab = ref(props.isCharter ? "2_tab" : "1_tab");
const { toDate } = useFilters();
const loading = ref(true);
const processing = ref(false);
const processingClose = ref(false);
let oldStatus = null;
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);

const today = new Date();
today.setHours(0, 0, 0, 0);
const optionsFn = (date) => {
  const dateObj = new Date(date);
  return dateObj >= today;
};

const readonlyProject = props.projectIdAttr ? "readonly" : "";
const rows = ref([]);
let projectModuleId = props.id;
let disableTab = true;
if (projectModuleId) {
  disableTab = false;
}

let previousProjectId = null;
const isLoadingEditModule = ref(false);

// local storage values
// const localStorageKey = "Project Modules";
// const filterLocalStorage = getLocalStorage(localStorageKey);
// const projectIds = filterLocalStorage ? filterLocalStorage.projectIds[0] : [];

// Define model values
const model = ref({
  projectId: props.projectIdAttr !== "" ? props.projectIdAttr : (props.projectIdValue !== "" ? props.projectIdValue : null),
  name: "",
  startDateStr: props.startDate ? props.startDate : "",
  endDateStr: props.endDate ? props.endDate : "",
  description: "",
  active: true,
  notes: ""
});

const pagination = ref({ sortBy: "employeeName", descending: false, rowsPerPage: 20, page: 1 });
const columns = [
  {
    name: "employeeSelection",
    label: "",
    field: "selected",
    align: "center",
    sortable: false,
    isSelection: true,
    width: "80px"
  },
  {
    name: "employeeName",
    label: "Employee Name",
    field: "employeeName",
    align: "left",
    sortable: true
  },
  {
    name: "manage",
    label: "Manage",
    field: "fullAccess",
    align: "center",
    sortable: false,
    type: "checkbox",
    width: "8%"
  },
  {
    name: "view",
    label: "View",
    field: "viewOnly",
    align: "center",
    sortable: false,
    type: "checkbox",
    width: "8%"
  },
  {
    name: "notes",
    label: "Notes",
    field: "notes",
    align: "center",
    sortable: false,
    type: "checkbox",
    width: "8%"
  }
];

let nestSortOrderByLastSortOrder = 1;
// get project details on edit mode
const getNextSortOrderOfProjectModuleAndTask = (projectId) => {
  loading.value = true;
  projectModuleService.getNextSortOrderOfProjectModuleAndTask(projectId).then((resp) => {
    nestSortOrderByLastSortOrder = resp.nextSortOrderOfProjectModule;
    model.value.sortOrder = resp.nextSortOrderOfProjectModule;
  }).finally(() => {
    loading.value = false;
  });
};

const allSelected = computed(() => {
  return (
    rows.value.length > 0 &&
    rows.value.every(row => row.selected)
  );
});

const someSelected = computed(() => {
  const selectedCount = rows.value.filter(
    row => row.selected
  ).length;

  return (
    selectedCount > 0 &&
    selectedCount < rows.value.length
  );
});

const toggleEmployee = (row, selected) => {
  row.selected = selected;

  row.fullAccess = selected && row.securityFullAccess;
  row.viewOnly = selected && row.securityViewOnly;
  row.notes = selected && row.securityNotes;
};

const toggleSelectAll = (value) => {
  rows.value.forEach(row => {
    toggleEmployee(row, value);
  });
};

const projectDateCache = new Map();

const cacheProjectDates = (projectId) => {
  if (!projectId) {
    return;
  }

  projectDateCache.set(String(projectId), {
    startDate: model.value.startDateStr || "",
    endDate: model.value.endDateStr || ""
  });
};

const getCachedProjectDates = (projectId) => {
  if (!projectId) {
    return null;
  }

  return projectDateCache.get(String(projectId)) || null;
};

const setProjectDates = (startDate, endDate) => {
  model.value.startDateStr = startDate || "";
  model.value.endDateStr = endDate || "";
};

const calculateProjectDates = (projectStart, projectEnd) => {
  const propStart = props.startDate
    ? toDate(props.startDate)
    : null;

  const propEnd = props.endDate
    ? toDate(props.endDate)
    : null;

  const isBetween = (date, start, end) => {
    if (!date || !start || !end) {
      return false;
    }

    return date >= start && date <= end;
  };

  if (
    !projectEnd &&
    propStart &&
    propEnd &&
    projectStart &&
    propStart >= projectStart &&
    propEnd >= projectStart
  ) {
    return {
      startDate: propStart,
      endDate: propEnd
    };
  }

  return {
    startDate: isBetween(propStart, projectStart, projectEnd)
      ? propStart
      : projectStart || "",
    endDate: isBetween(propEnd, projectStart, projectEnd)
      ? propEnd
      : projectEnd || ""
  };
};

// get project details on edit mode
const getProjectModule = async (projectModuleId) => {
  loading.value = true;
  isLoadingEditModule.value = true;

  try {
    const resp =
      await projectModuleService.getProjectModuleDetails(
        projectModuleId
      );

    model.value = _.cloneDeep(resp);

    oldStatus =
      model.value.projectModuleStatus?.dropDownValue || "";

    model.value.description = resp.description || "";
    model.value.notes = resp.notes || "";

    model.value.startDateStr = resp.startDate
      ? toDate(resp.startDate)
      : "";

    model.value.endDateStr = resp.endDate
      ? toDate(resp.endDate)
      : "";

    model.value.projectModuleFiles =
      resp.projectModuleFilesList || [];

    const projectEmployees =
      resp.project?.projectEmployeeMappings || [];

    const moduleEmployees =
      resp.projectModuleEmployeeMappings || [];

    loadProjectEmployees(
      projectEmployees,
      moduleEmployees
    );

    if (model.value.projectId) {
      cacheProjectDates(model.value.projectId);

      await getProject(model.value.projectId, {
        restoreCachedDates: true,
        isInitialEditLoad: true
      });
    }
  } catch (error) {
    console.error(
      "Error while loading project module:",
      error
    );

    notifyError({
      message: "Unable to load project module details."
    });
  } finally {
    isLoadingEditModule.value = false;
    loading.value = false;
  }
};

const getProject = async (projectId, options = {}) => {
  const {
    restoreCachedDates = true,
    isInitialEditLoad = false
  } = options;

  if (!projectId) {
    rows.value = [];
    setProjectDates("", "");
    return;
  }

  loading.value = true;

  try {
    const resp = await projectService.getProject(projectId);

    const projectStart = resp.startDate
      ? toDate(resp.startDate)
      : null;

    const projectEnd = resp.goLiveDate
      ? toDate(resp.goLiveDate)
      : null;

    const cachedDates = getCachedProjectDates(projectId);

    if (isInitialEditLoad) {
      if (
        model.value.startDateStr ||
        model.value.endDateStr
      ) {
        cacheProjectDates(projectId);
      } else {
        const projectDates = calculateProjectDates(
          projectStart,
          projectEnd
        );

        setProjectDates(
          projectDates.startDate,
          projectDates.endDate
        );

        cacheProjectDates(projectId);
      }
    } else if (restoreCachedDates && cachedDates) {
      setProjectDates(
        cachedDates.startDate,
        cachedDates.endDate
      );
    } else {
      const projectDates = calculateProjectDates(
        projectStart,
        projectEnd
      );

      setProjectDates(
        projectDates.startDate,
        projectDates.endDate
      );

      cacheProjectDates(projectId);
    }

    const projectEmployees =
      resp.projectEmployeeMappings || [];

    if (props.id) {
      const moduleEmployees =
        model.value.projectModuleEmployeeMappings || [];

      loadProjectEmployees(
        projectEmployees,
        moduleEmployees
      );
    } else {
      loadProjectEmployees(projectEmployees, []);
    }
  } catch (error) {
    console.error("Error while loading project:", error);

    rows.value = [];

    notifyError({
      message: "Unable to load project employees."
    });
  } finally {
    loading.value = false;
  }
};

// const createEmployeeRows = (projectEmployees, moduleEmployees = []) => {
//   const moduleEmployeeMap = new Map(
//     moduleEmployees
//       .filter(mapping => mapping.employee?.id && !mapping.deleted)
//       .map(mapping => [mapping.employee.id, mapping])
//   );

//   return projectEmployees
//     .map(mapping => {
//       const employee = mapping.employee;

//       if (!employee?.id) {
//         return null;
//       }

//       const moduleMapping = moduleEmployeeMap.get(employee.id);

//       return {
//         id: moduleMapping?.id || "",
//         // This is Employee.Id
//         employeeId: employee.id,

//         employeeName: employee.person?.fullName || "",

//         // Checked if an active mapping exists
//         selected: !!moduleMapping && !moduleMapping.deleted
//       };
//     })
//     .filter(Boolean);
// };

const createEmployeeRows = (
  projectEmployees,
  moduleEmployees = [],
  isNewModule = false
) => {
  const moduleEmployeeMap = new Map(
    moduleEmployees
      .filter(mapping => {
        const employeeId =
          mapping.employeeId || mapping.employee?.id;

        return employeeId && !mapping.deleted;
      })
      .map(mapping => [
        String(mapping.employeeId || mapping.employee.id),
        mapping
      ])
  );

  return projectEmployees
    .map(mapping => {
      const employee = mapping.employee;

      if (!employee?.id) {
        return null;
      }

      const employeeId = String(employee.id);
      const moduleMapping =
        moduleEmployeeMap.get(employeeId);

      const moduleFullAccess =
        !!moduleMapping?.fullAccess;

      const moduleViewOnly =
        !!moduleMapping?.viewOnly;

      const moduleNotes =
        !!moduleMapping?.notes;

      // Module security is considered available only when
      // at least one module permission is enabled.
      const hasModuleSecurity =
        !!moduleMapping &&
        (
          moduleFullAccess ||
          moduleViewOnly ||
          moduleNotes
        );

      const securityFullAccess =
        !!mapping.manage;

      const securityViewOnly =
        !!mapping.view;

      const securityNotes =
        !!mapping.notes;

      const selected = isNewModule
        ? true
        : hasModuleSecurity;

      return {
        id: moduleMapping?.id || "",
        employeeId: employee.id,
        employeeName:
          employee.person?.fullName || "",

        selected,

        fullAccess: hasModuleSecurity
          ? moduleFullAccess
          : selected && securityFullAccess,

        viewOnly: hasModuleSecurity
          ? moduleViewOnly
          : selected && securityViewOnly,

        notes: hasModuleSecurity
          ? moduleNotes
          : selected && securityNotes,

        securityFullAccess,
        securityViewOnly,
        securityNotes,

        moduleFullAccess,
        moduleViewOnly,
        moduleNotes,

        hasModuleSecurity
      };
    })
    .filter(Boolean);
};

const loadProjectEmployees = (
  projectEmployees,
  moduleEmployees = []
) => {
  rows.value = createEmployeeRows(
    projectEmployees,
    moduleEmployees,
    !props.id
  );
};

// const loadProjectEmployees = (projectEmployees, moduleEmployees = []) => {
//   rows.value = createEmployeeRows(projectEmployees, moduleEmployees);
// };

// Validation rules
const rules = {
  projectId: { required: helpers.withMessage("Project is required", required) },
  projectModuleStatusId: { required: helpers.withMessage("Project module status is required", required) },
  name: { required: helpers.withMessage("Module name is required", required), minLength: minLength(1), maxLength: maxLength(100) },
  sortOrder: { required: helpers.withMessage("Sort Order is required", required) },
  startDateStr: {
    required: helpers.withMessage("Start date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  endDateStr: {
    required: helpers.withMessage("End date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate),
    afterStartDate: helpers.withMessage("End date must occur after the start date", (value, startDate) => {
      return new Date(value) > new Date(startDate.startDateStr);
    })
  }
};
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

function validateSortOrder (value) {
  if (typeof value !== "string" && typeof value !== "number") return false;
  const str = String(value).trim();

  // Must be digits only
  if (!/^\d+$/.test(str)) {
    return false;
  }

  // Convert to number and check > 0
  return Number(str) > 0;
}

const disableBeforeStartDate = (startDate) => {
  if (!model.value.startDateStr) {
    return true;
  }

  // Convert MM/dd/yyyy string to Date
  const start = parse(model.value.startDateStr, "MM/dd/yyyy", new Date());
  const currentDate = parse(startDate, "yyyy/MM/dd", new Date());

  return currentDate >= start;
};

function getSortOrderByProjectId (projectId) {
  model.value.sortOrder = "";
  getNextSortOrderOfProjectModuleAndTask(projectId);
}

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const refreshProjectDropdownList = () => {
  projectNameDropdownSingleSelect.load();
};

const { projectModuleStatusForDropdownSingleSelect } = projectModuleOfProjectModule();

// -------------------------------------------------------------------------------------------------------
// Upload Image
// -------------------------------------------------------------------------------------------------------
function handleFiles (files) {
  model.value.projectModuleFiles = files;
  model.value.projectFileFlag = "edit";
}

function getFilePreview (file) {
  return file && file instanceof File ? URL.createObjectURL(file) : "";
}

function isImageFile (file) {
  if (file.file instanceof File) {
    return file.file.type.startsWith("image/");
  } else if (file.file && file.file.mimeType) {
    return file.file.mimeType.startsWith("image/");
  }
  return false;
}

function viewFile (file) {
  let fileUrl; // Declare fileUrl before using it

  if (file?.file?.virtualPath) {
    fileUrl = new URL(file.file.virtualPath).href; // For uploaded files
  } else if (file?.file instanceof File) {
    fileUrl = URL.createObjectURL(file.file); // For newly added files
  } else if (file instanceof File) {
    fileUrl = URL.createObjectURL(file); // Direct File object case
  }

  // const fileUrl = new URL(file, baseURL).href;
  const fileExtension = fileUrl.split(".").pop().toLowerCase();
  const supportedFormats = ["pdf", "docx", "xlsx", "pptx"];
  const imageFormats = ["jpg", "jpeg", "png", "gif", "svg"];

  let viewerUrl = fileUrl;

  // Use Google Docs Viewer for Documents
  if (supportedFormats.includes(fileExtension)) {
    viewerUrl = `https://docs.google.com/gview?url=${encodeURIComponent(fileUrl)}&embedded=true`;
  }

  // Open new window
  const newWindow = window.open("", "_blank");

  // Check if the popup is blocked
  if (!newWindow) {
    alert("Popup blocked! Please allow popups for this site.");
    return;
  }

  // Delay to avoid null reference issues
  setTimeout(() => {
    newWindow.document.write(`
<html>
<head>
<title>${fileUrl.split("/").pop()}</title>
<style>
              * { margin: 0; padding: 0; box-sizing: border-box; }
              body, html { width: 100vw; height: 100vh; display: flex; align-items: center; justify-content: center; background-color: #f4f4f4; overflow: hidden; }
              .top-right {
                position: fixed;
                top: 10px;
                right: 10px;
                background: #007bff;
                color: white;
                padding: 10px 15px;
                border-radius: 5px;
                font-size: 16px;
                text-decoration: none;
                z-index: 10;
              }
              .top-right:hover {
                background: #0056b3;
              }

              iframe, img {
                width: 100%;
                height: 100%;
                border: none;
                display: block;
                object-fit: contain; /* Ensures images fit properly */
              }

              /* Responsive Fixes */
              @media (max-width: 768px) {
                .top-right {
                  top: 5px;
                  right: 5px;
                  padding: 8px 12px;
                  font-size: 14px;
                }
              }
</style>
</head>
<body>
<a class="top-right" href="${fileUrl}" download>Download</a>
            ${
  imageFormats.includes(fileExtension)
    ? `<img src="${fileUrl}" alt="Image Preview">` // Show image directly
    : `<iframe src="${viewerUrl}"></iframe>` // Show document using iframe
}
</body>
</html>
        `);
  }, 100);
}

function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
}

function getFileIcon (mimeType) {
  const mimeToIconMap = {
    "application/pdf": "o_picture_as_pdf",
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": "o_insert_chart",
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document": "o_description",
    "application/vnd.openxmlformats-officedocument.presentationml.presentation": "o_slideshow", // PPTX MIME type
    "application/vnd.ms-powerpoint": "o_slideshow", // PPT MIME type
    "application/zip": "o_folder_zip",
    "text/plain": "o_article",
    "image/png": "o_image",
    "image/jpeg": "o_image",
    "image/gif": "o_image",
    // Default icon for unknown MIME types
    default: "o_insert_drive_file"
  };

  return mimeToIconMap[mimeType] || mimeToIconMap.default;
}

function removeFile (index) {
  const file = model.value.projectModuleFiles[index];

  if (file && file.name && file.type) {
    URL.revokeObjectURL(file); // Clean up object URL
  }

  if (file && file.file && file.file.virtualPath) {
    // If it's an existing file, mark it as "remove" instead of deleting from array
    file.flag = "remove";
    model.value.projectModuleFiles.splice(index, 1);
  } else {
    // For new files, just remove them from the list
    model.value.projectModuleFiles.splice(index, 1);
  }

  if (model.value.projectModuleFiles.length === 0) {
    // model.value.projectFileFlag = "remove";
  }
}
// -------------------------------------------------------------------------------------------------------

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { projectNameDropdownSingleSelect
} = projectModule();

// const validateProjectModuleForm = async () => {
//   // Always validate Project Module Info first
//   const isValid = await v$.value.$validate();

//   if (!isValid) {
//     tab.value = "1_tab";

//     notifyError({
//       message: "Please fill all required fields."
//     });

//     return false;
//   }

//   // Only validate employee selection when on Tab 2
//   if (tab.value === "2_tab") {
//     const selectedEmployees = rows.value.filter(
//       row => row.selected
//     );

//     if (selectedEmployees.length === 0) {
//       notifyError({
//         message: "Please assign at least one employee."
//       });

//       return false;
//     }
//   }

//   return true;
// };
const validateEmployeeSecurity = () => {
  const selectedEmployees = rows.value.filter(
    row => row.selected
  );

  if (selectedEmployees.length === 0) {
    notifyError({
      message: "Please assign at least one employee."
    });
    return false;
  }

  const invalidEmployee = selectedEmployees.find(
    row =>
      !row.fullAccess &&
      !row.viewOnly &&
      !row.notes
  );

  if (invalidEmployee) {
    notifyError({
      message: `Please select at least one permission for ${invalidEmployee.employeeName}.`
    });
    return false;
  }

  return true;
};

const validateProjectModuleForm = async () => {
  const isValid = await v$.value.$validate();

  if (!isValid) {
    tab.value = "1_tab";

    notifyError({
      message: "Please fill all required fields."
    });

    return false;
  }

  if (tab.value === "2_tab") {
    if (!validateEmployeeSecurity()) {
      return false;
    }
  }

  return true;
};

const onSubmitClose = () => {
  onSubmit(1);
};

// Submit form
const onSubmit = async (isClose = 0) => {
  if (isClose === 1) {
    processingClose.value = true;
    processing.value = false;
  } else {
    processing.value = true;
    processingClose.value = false;
  }

  try {
    const formData = new FormData();

    if (!(await validateProjectModuleForm())) {
      return;
    }

    // Status confirmation
    if (props.id) {
      const selected = projectModuleStatusForDropdownSingleSelect.list.value.find(
        item => item.value === model.value.projectModuleStatusId
      );

      if (
        oldStatus.toLowerCase() !== "close" &&
        selected?.text.toLowerCase() === "close"
      ) {
        const resp = await projectModuleService.checkModuleCanBeDeleted(props.id);
        const canClose = resp?.canDelete;

        if (!canClose) {
          zwConfirm({
            title: "Active Tasks or Activities Found",
            message: "This module has active tasks or activities. You cannot close it.",
            okLabel: "OK",
            cancel: false
          }, () => {});

          return;
        }
      }
    }

    // Form data
    formData.append("projectId", model.value.projectId);
    formData.append("name", model.value.name);
    formData.append("startDateStr", model.value.startDateStr);
    formData.append("endDateStr", model.value.endDateStr);
    formData.append("sortOrder", model.value.sortOrder);
    formData.append("projectModuleStatusId", model.value.projectModuleStatusId);
    formData.append("description", model.value.description);
    formData.append("notes", model.value.notes);
    formData.append("tab", tab.value);

    if (tab.value === "2_tab") {
      model.value.projectModuleEmployeeMappings = rows.value.map(emp => ({
        id: emp.id ?? "",
        employeeId: emp.employeeId ?? "",
        selected: !!emp.selected,
        fullAccess: !!emp.fullAccess,
        viewOnly: !!emp.viewOnly,
        notes: !!emp.notes
      }));

      model.value.projectModuleEmployeeMappings.forEach((emp, index) => {
        formData.append(
          `projectModuleEmployeeMappings[${index}].id`,
          emp.id ?? ""
        );

        formData.append(
          `projectModuleEmployeeMappings[${index}].employeeId`,
          emp.employeeId ?? ""
        );

        formData.append(
          `projectModuleEmployeeMappings[${index}].fullAccess`,
          String(emp.fullAccess)
        );

        formData.append(
          `projectModuleEmployeeMappings[${index}].viewOnly`,
          String(emp.viewOnly)
        );

        formData.append(
          `projectModuleEmployeeMappings[${index}].notes`,
          String(emp.notes)
        );

        formData.append(
          `projectModuleEmployeeMappings[${index}].deleted`,
          String(!emp.selected)
        );
      });
    }
    toRaw(model.value.projectModuleFiles || []).forEach((file) => {
      if (file.file && file.file.virtualPath) {
        formData.append(
          "ExistingFiles",
          JSON.stringify({
            id: file.id,
            virtualPath: file.file.virtualPath
          })
        );
      } else {
        formData.append("projectModuleFiles", file);
      }
    });

    // Save
    const resp = await projectModuleService.saveProjectModule(
      projectModuleId,
      formData
    );

    projectModuleId = resp.id;
    disableTab = false;

    if (resp.warning) {
      notifyWarning({ message: resp.warning });
    }
    notifySuccess({ message: resp.message });

    // Save & Close
    if (isClose === 1) {
      getProjectModule(projectModuleId);
      onDialogOK();
    } else {
      const currentTab = tab.value;
      switch (currentTab) {
      case "1_tab":
        tab.value = "2_tab";
        getProjectModule(projectModuleId);
        break;
      default:
        break;
      }
    }
  } catch (error) {
    console.error("Error while saving project module:", error);
    notifyError({
      message: "An error occurred while saving the project module."
    });
  } finally {
    processing.value = false;
    processingClose.value = false;
  }
};

// watches a data property with the same name i.e. immediate effect
watch(
  () => props.id,
  (newValue) => {
    if (newValue) {
      getProjectModule(newValue);
    }
  },
  {
    immediate: true
  }
);

watch(
  () => model.value.projectId,
  async (newProjectId, oldProjectId) => {
    if (isLoadingEditModule.value) {
      return;
    }

    if (!newProjectId) {
      rows.value = [];
      setProjectDates("", "");
      previousProjectId = null;
      return;
    }

    const newId = String(newProjectId);
    const oldId = oldProjectId
      ? String(oldProjectId)
      : null;

    if (
      oldProjectId === undefined ||
      oldProjectId === null
    ) {
      previousProjectId = newId;

      await getProject(newProjectId, {
        restoreCachedDates: true,
        isInitialEditLoad: false
      });

      return;
    }

    if (newId !== oldId) {
      if (oldId) {
        cacheProjectDates(oldId);
      }

      previousProjectId = newId;

      await getProject(newProjectId, {
        restoreCachedDates: true,
        isInitialEditLoad: false
      });

      if (!props.id) {
        await getNextSortOrderOfProjectModuleAndTask(
          newProjectId
        );
      }
    }
  },
  {
    immediate: true
  }
);

watch(
  [
    () => model.value.startDateStr,
    () => model.value.endDateStr
  ],
  ([startDate, endDate]) => {
    if (!model.value.projectId) {
      return;
    }

    const projectId = String(model.value.projectId);

    projectDateCache.set(projectId, {
      startDate: startDate || "",
      endDate: endDate || ""
    });
  }
);

// On page rendering
onMounted(() => {
  projectNameDropdownSingleSelect.load();
  projectModuleStatusForDropdownSingleSelect.load("WO Status");
});
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_projectModule .q-select__dropdown-icon{
  display: none;
}
.edit_projectModule .project-module-status-dropdown .q-select__dropdown-icon {
  display: inline-flex !important;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
</style>
