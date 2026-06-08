<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Release</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <q-card>
              <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
                <q-tab name="1_tab" label="Release Info." class="q-px-lg q-mr-md" />
                <q-tab name="2_tab" label="Release Tracking Items" class="q-px-lg" :disable="disableTab" />
              </q-tabs>
              <q-separator />
              <q-tab-panels v-model="tab" animated>
                <q-tab-panel name="1_tab">
                  <fieldset>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <formSingleSelectDropdown
                        v-model="model.projectId"
                        label="Project Name"
                        :options="projectNameDropdownSingleSelect.list.value"
                        :filter="projectNameDropdownSingleSelect.filter"
                        :readonly="isReadOnlyMode"
                        :error="v$.projectId.$error"
                        :error-message="v$.projectId.$errors[0]?.$message"
                      />
                      <formSingleSelectDropdown
                        v-model="model.infraInstanceId"
                        label="Infra Instance"
                        :readonly="isReadOnlyMode"
                        :disable="!model.projectId"
                        :options="infraProjectInstanceTypeDropdownSingleSelect.list.value"
                        :filter="infraProjectInstanceTypeDropdownSingleSelect.filter"
                        :error="v$.infraInstanceId.$error"
                        :error-message="v$.infraInstanceId.$errors[0]?.$message"
                      />
                      <formSingleSelectDropdown
                        v-model="model.deploymentOwnerId"
                        label="Deployment Owner"
                        :readonly="isReadOnlyMode"
                        :disable="!model.projectId"
                        :options="projectEmployeeDropdownSingleSelect.list.value"
                        :filter="projectEmployeeDropdownSingleSelect.filter"
                        :error="v$.deploymentOwnerId.$error"
                        :error-message="v$.deploymentOwnerId.$errors[0]?.$message"
                      />
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <formSingleSelectDropdown
                        v-model="model.approverId"
                        label="Approver"
                        :readonly="isReadOnlyMode"
                        :disable="!model.projectId"
                        :options="projectEmployeeDropdownSingleSelect.list.value"
                        :filter="projectEmployeeDropdownSingleSelect.filter"
                        :error="v$.approverId.$error"
                        :error-message="v$.approverId.$errors[0]?.$message"
                      />
                      <formSingleSelectDropdown
                        v-model="model.testerId"
                        label="Tester"
                        :readonly="isReadOnlyMode"
                        :disable="!model.projectId"
                        :options="projectEmployeeDropdownSingleSelect.list.value"
                        :filter="projectEmployeeDropdownSingleSelect.filter"
                        :error="v$.testerId.$error"
                        :error-message="v$.testerId.$errors[0]?.$message"
                      />
                      <formSingleSelectDropdown
                        v-model="model.releaseTypeId"
                        label="Release Type"
                        :readonly="isReadOnlyMode"
                        :disable="!model.projectId"
                        :options="releaseTrackingTypeDropdownSingleSelect.list.value"
                        :filter="releaseTrackingTypeDropdownSingleSelect.filter"
                        :error="v$.releaseTypeId.$error"
                        :error-message="v$.releaseTypeId.$errors[0]?.$message"
                      />
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12">
                        <div class="text-black">Version Number<span class="required">*</span></div>
                        <q-input
                          v-model="model.versionNumber"
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          :readonly="isReadOnlyMode"
                          :disable="!isAdmin || !model.projectId"
                          :error="v$.versionNumber.$error"
                          :error-message="v$.versionNumber.$errors[0]?.$message"
                          @click="v$.versionNumber.$touch"
                        />
                      </div>
                      <div class="col-xxl-8 col-lg-8 col-md-8 col-sm-8 col-xs-12">
                        <div class="text-black">Name<span class="required">*</span></div>
                        <q-input
                          v-model="model.name"
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          :readonly="isReadOnlyMode"
                          :error="v$.name.$error"
                          :error-message="v$.name.$errors[0]?.$message"
                          @click="v$.name.$touch"
                        />
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <formDate
                        v-model="model.plannedReleaseDateStr"
                        label="Planned Release Date"
                        :readonly="isReadOnlyMode"
                        :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                        :error="v$.plannedReleaseDateStr.$error"
                        :error-message="v$.plannedReleaseDateStr.$errors[0]?.$message"
                        :onBlur="() => v$.plannedReleaseDateStr.$touch()"
                      />
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-12 col-md-12 col-lg-12">
                        <div class="text-black"><label>Description</label></div>
                        <div class="form-group">
                          <q-editor
                            v-model="model.description"
                            :disable="isReadOnlyMode"
                            :dense="$q.screen.lt.md"
                            :toolbar="toolbar"
                            :fonts="fonts"
                          />
                        </div>
                      </div>
                    </div>
                    <div align="center" class="q-gutter-sm justify-center">
                      <!-- CLOSE -->
                      <q-btn
                        color="grey-4"
                        outline
                        label="Close"
                        class="text-grey-9 actionBtn same-size-btn"
                        no-caps
                        @click="onDialogCancel"
                      />
                      <q-btn
                        :label="firstTabDraftLabel"
                        color="primary"
                        outline
                        class="actionBtn same-size-btn"
                        no-caps
                        :disable="isReadOnlyMode"
                        :loading="processingClose"
                        @click="onSubmitDraftClose"
                      />
                      <q-btn
                        :label="isEdit ? 'Update & Next' : 'Save & Next'"
                        color="primary"
                        outline
                        class="actionBtn same-size-btn"
                        :loading="processing"
                        :disable="isReadOnlyMode"
                        no-caps
                        @click="onSubmitNext"
                      />
                    </div>
                  </fieldset>
                </q-tab-panel>
                <q-tab-panel name="2_tab">
                  <div>
                    <div class="q-mb-sm q-gutter-sm flex justify-end">
                      <q-input
                        v-model="filterItems" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search"
                        dense clearable
                      >
                        <template #prepend>
                          <q-icon name="o_search" />
                        </template>
                      </q-input>
                    </div>
                    <q-table
                      ref="tableRef"
                      v-model:pagination="pagination"
                      bordered
                      class="no-shadow"
                      virtual-scroll
                      :loading="loading"
                      :rows="filteredRows"
                      :columns="columns"
                      row-key="id"
                      separator="cell"
                      binary-state-sort
                      :rows-per-page-options="[20, 50, 100, 200, 500]"
                    >
                      <template #header="props">
                        <q-tr :props="props" class="bg-primary text-white">
                          <q-th
                            v-for="col in props.cols"
                            :key="col.name"
                            :props="props"
                          >
                            {{ col.label }}
                          </q-th>
                        </q-tr>
                      </template>
                      <template #body="props">
                        <q-tr>
                          <q-td class="text-center" style="width: 10%;">
                            <q-checkbox :disable="isReadOnlyMode" v-model="props.row.checkboxStatus" />
                          </q-td>
                          <q-td style="width: 12%;">{{ props.row.type }}</q-td>
                          <q-td class="text-right" style="width: 8%;">#{{ props.row.number }}</q-td>
                          <q-td class="ellipsis-cell" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 60%;">
                            <div>
                              {{ isExpanded(props.row.id) ? props.row.name : truncateText(props.row.name) }}

                              <span
                                v-if="props.row.name?.length > 80"
                                class="text-primary cursor-pointer q-ml-xs"
                                @click="toggleExpand(props.row.id)"
                              >
                                {{ isExpanded(props.row.id) ? 'less' : '...more' }}
                              </span>
                            </div>
                          </q-td>
                          <q-td style="width: 10%;">{{ props.row.date }}</q-td>
                        </q-tr>
                      </template>
                    </q-table>
                  </div>
                  <div align="center" class="q-gutter-sm justify-center q-mt-sm">
                    <q-btn
                      color="grey-4"
                      outline
                      label="Close"
                      class="text-grey-9 actionBtn same-size-btn"
                      no-caps
                      @click="onDialogCancel"
                    />
                    <q-btn
                      v-if="!isEdit || isDraft"
                      :label="draftCloseLabel"
                      color="primary"
                      outline
                      class="actionBtn same-size-btn"
                      no-caps
                      :disable="isReadOnlyMode"
                      :loading="processingClose"
                      @click="onSubmitDraftClose"
                    />
                    <q-btn
                      :label="progressCloseLabel"
                      color="primary"
                      outline
                      class="actionBtn same-size-btn"
                      :disable="isReadOnlyMode"
                      :loading="processing"
                      no-caps
                      @click="onSubmitClose"
                    />
                  </div>
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
import _ from "lodash";
import { isDate } from "validators/zw_validators.js";
import { notifySuccess, notifyError } from "assets/utils";
import { useAuthStore } from "stores/auth";
import useVuelidate from "@vuelidate/core";
import { ref, onMounted, watch, computed } from "vue";
import { useQuasar, useDialogPluginComponent } from "quasar";
import { required, helpers } from "@vuelidate/validators";

import releaseTrackingService from "modules/project-release-tracking/projectReleaseTracking.service";

// SOP Change :- Shared Dropdowns
import releaseTrackingModule from "src/modules/project-release-tracking/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";
import infraProjectInstanceModule from "src/modules/infra-project-instance/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";
import { format } from "date-fns";

const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const filterItems = ref("");
const isEdit = computed(() => !!props.id);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();
defineEmits([...useDialogPluginComponent.emits]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Common variables
const tab = ref("1_tab");
const loading = ref(true);
const processing = ref(false);
const processingClose = ref(false);
let previousVersion = null;

// check login user role
const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin"];
const isAdmin = computed(() =>
  user?.roles?.some(role => adminRoles.includes(role?.toLowerCase()))
);

const rows = ref([]);
const pagination = ref({ sortBy: "", descending: false, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "selection", label: "Selection", field: "checkboxStatus", align: "center", sortable: true },
  { name: "type", label: "Type", field: "type", align: "left", sortable: true },
  { name: "number", label: "Number", field: "number", align: "left" },
  { name: "name", label: "Name", field: "name", align: "left" },
  { name: "date", label: "Date", field: "date", align: "left" }
]);

// Define model values
const model = ref({
  id: "",
  name: "",
  projectStatusId: "",
  startDateStr: format(new Date(), "MM/dd/yyyy"),
  goLiveDateStr: "",
  description: "",
  active: true,
  employeeId: null,
  projectFiles: [],
  projectFileFlag: "edit"
});

let initialProjectId = null;
let initialReleaseTypeId = null;
let releaseTrackingId = props.id;
let disableTab = true;
if (releaseTrackingId) {
  disableTab = false;
}
const expandedRows = ref(new Set());

const toggleExpand = (id) => {
  if (expandedRows.value.has(id)) {
    expandedRows.value.delete(id);
  } else {
    expandedRows.value.add(id);
  }
};

const isExpanded = (id) => {
  return expandedRows.value.has(id);
};

const truncateText = (text, length = 80) =>
  text?.length > length ? text.slice(0, length) + "..." : text || "";

async function generateVersion (projectId, typeText) {
  try {
    previousVersion =
      await releaseTrackingService.generateVersionNumber(
        projectId,
        typeText
      );

    model.value.versionNumber = previousVersion;
  } catch (err) {
    console.error(err);
  }
}

// ==================================================================
// button labels
// ==================================================================
const status = computed(() =>
  (model.value.statusText || "").trim().toLowerCase()
);

const isDraft = computed(() => status.value === "draft");
const isReadOnlyMode = computed(() => {
  // Allow edit in Add mode
  if (!props.id) return false;

  // For Edit mode, lock if not draft
  return status.value !== 'draft';
});

const firstTabDraftLabel = computed(() => {
  if (isEdit.value) {
    return status.value !== "draft"
      ? "Update & Close"
      : "Update as Draft & Close";
  }
  return "Save as Draft & Close";
});

const draftCloseLabel = computed(() => {
  return isEdit.value
    ? "Update as Draft & Close"
    : "Save as Draft & Close";
});

const progressCloseLabel = computed(() => {
  if (isEdit.value) {
    return isDraft.value
      ? "Update as In Progress & Close"
      : "Update & Close";
  }
  return "Save as In Progress & Close";
});
// ==================================================================

const getReleaseTrackingInDetailsById = (releaseTrackingId) => {
  loading.value = true;
  releaseTrackingService.getReleaseTrackingInDetailsById(releaseTrackingId).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.projectId = resp.project?.id;
    model.value.infraInstanceId = resp.infraInstance?.id;
    model.value.deploymentOwnerId = resp.deploymentOwner?.id;
    model.value.approverId = resp.approver?.id;
    model.value.testerId = resp.tester?.id;
    model.value.releaseTypeId = resp.releaseType?.id;
    model.value.plannedReleaseDateStr = resp.plannedReleaseDate ? format(resp.plannedReleaseDate, "MM/dd/yyyy") : "";
    model.value.description = resp.description ? resp.description : "";
    previousVersion = resp.versionNumber;
    initialProjectId = model.value.projectId;
    initialReleaseTypeId = model.value.releaseTypeId;
  }).finally(() => {
    loading.value = false;
  });
};

const getAllReqPlanTaskIssuesByProjectId = async (projectId) => {
  if (!projectId) return;

  try {
    loading.value = true;

    const resp = await releaseTrackingService.getAllReqPlanTaskIssuesByProjectId(projectId);

    rows.value = resp.map(x => ({
      id: x.id,
      type: x.type,
      number: x.number,
      name: x.name,
      date: x.date,
      checkboxStatus: false
    }));
  } catch (err) {
    console.error(err);
    notifyError({ message: "Failed to load tracking items" });
  } finally {
    loading.value = false;
  }
};

const loadMappedItems = async () => {
  try {
    const mappedList = await releaseTrackingService.getMappingByReleaseTrackingId(releaseTrackingId);

    const mappedSet = new Set(
      mappedList.map(x => `${x.type?.toLowerCase()}_${x.refId}`)
    );
    rows.value.forEach(row => {
      const key = `${row.type?.toLowerCase()}_${row.id}`;
      row.checkboxStatus = mappedSet.has(key);
    });
  } catch (err) {
    console.error(err);
  }
};

const filteredRows = computed(() => {
  if (!filterItems.value) return rows.value;

  const search = filterItems.value.toLowerCase();

  return rows.value.filter(row =>
    row.type?.toLowerCase().includes(search) ||
    row.name?.toLowerCase().includes(search) ||
    row.number?.toString().includes(search) ||
    row.date?.toLowerCase().includes(search)
  );
});

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectNameDropdownSingleSelect,
  projectEmployeeDropdownSingleSelect
} = projectModule();

const {
  infraProjectInstanceTypeDropdownSingleSelect
} = infraProjectInstanceModule();

const {
  releaseTrackingTypeDropdownSingleSelect
} = releaseTrackingModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// version number validation
// --------------------------------------------------------------------------------------------------------------------------------------------------
function compareVersions (v1, v2) {
  if (!v1 || !v2) return null;

  const a = v1.split(".").map(Number);
  const b = v2.split(".").map(Number);

  const length = Math.max(a.length, b.length);

  for (let i = 0; i < length; i++) {
    const num1 = a[i] || 0;
    const num2 = b[i] || 0;

    if (num1 > num2) return 1;
    if (num1 < num2) return -1;
  }

  return 0;
}

function validateVersion (newVersion, previousVersion, isAdmin) {
  if (!previousVersion) return true;

  if (!newVersion) {
    notifyError({ message: "Version is required." });
    return false;
  }

  if (isAdmin) {
    const result = compareVersions(newVersion, previousVersion);

    if (result === null) {
      notifyError({ message: "Invalid version format." });
      return false;
    }

    if (result < 0) {
      notifyError({
        message: `Version must be greator or equal to ${previousVersion}`
      });
      return false;
    }
  }

  return true;
}
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Validation Rules
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Release Tracking Info - Validation Rules
const rules = {
  plannedReleaseDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    required: helpers.withMessage("Planned release date is required", required)
  },
  projectId: { required: helpers.withMessage("Project is required", required) },
  infraInstanceId: { required: helpers.withMessage("Infra Instance is required", required) },
  deploymentOwnerId: { required: helpers.withMessage("Deployment Owner is required", required) },
  approverId: { required: helpers.withMessage("Approver is required", required) },
  testerId: { required: helpers.withMessage("Tester is required", required) },
  releaseTypeId: { required: helpers.withMessage("Release Type is required", required) },
  versionNumber: { required: helpers.withMessage("Version Number is required", required) },
  name: { required: helpers.withMessage("Name is required", required) }
};
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Submit form
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onSubmitDraftClose = () => onSubmit("draftClose", 1, true);
const onSubmitClose = () => onSubmit("progressClose", 1, false);
const onSubmitNext = () => onSubmit("next");

const onSubmit = async (type, isClose = 0, isDraft = null) => {
  model.value.isDraft = isDraft;

  processing.value = false;
  processingClose.value = false;

  if (type === "next") {
    processing.value = true;
    model.value.isDraft = true;
  }

  if (type === "draftClose") {
    processingClose.value = true;
    model.value.isDraft = true;
  }

  if (type === "progressClose") {
    processing.value = true;
    model.value.isDraft = false;
  }

  try {
    if (
      !(await v$.value.$validate()) ||
      !validateVersion(model.value.versionNumber, previousVersion, isAdmin)
    ) {
      return;
    }

    // Tab 2 validation
    if (tab.value === "2_tab" && rows.value.length === 0) {
      notifyError({ message: "Add at least one item in release tracking." });
      return;
    }
    const selected = projectNameDropdownSingleSelect.value
      ?.find(p => p.value === model.value.projectId);

    model.value.projectName = selected?.text || "";

    model.value.tab = tab.value;
    model.value.projectReleaseTrackingReqPlanTaskIssueList = rows.value
      .filter(x => x.checkboxStatus)
      .map(x => ({
        id: x.id,
        refId: x.id,
        type: x.type
      }));
    const resp = await releaseTrackingService.saveReleaseTracking(
      releaseTrackingId,
      model.value
    );

    notifySuccess({
      message: isDraft
        ? "Draft saved successfully."
        : "Release Tracking is saved successfully."
    });

    // Update state
    releaseTrackingId = resp.id;
    disableTab = false;

    await getReleaseTrackingInDetailsById(releaseTrackingId);
    await getAllReqPlanTaskIssuesByProjectId(model.value.projectId);

    if (isClose === 1) {
      onDialogOK();
    } else if (tab.value === "1_tab") {
      tab.value = "2_tab";
    }
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    // Reset loaders
    processing.value = false;
    processingClose.value = false;
  }
};

watch(
  () => [model.value.projectId, model.value.releaseTypeId],
  async ([projectId, releaseTypeId]) => {
    if (!projectId || !releaseTypeId) return;

    const selectedType = releaseTrackingTypeDropdownSingleSelect.list.value.find(
      x => x.value === releaseTypeId
    );

    if (!selectedType?.text) return;

    // ADD MODE
    if (!props.id) {
      await generateVersion(projectId, selectedType.text);
      return;
    }

    // EDIT MODE, only if projectId or releaseTypeId is changed
    const isProjectChanged = projectId !== initialProjectId;
    const isTypeChanged = releaseTypeId !== initialReleaseTypeId;

    if (isProjectChanged || isTypeChanged) {
      await generateVersion(projectId, selectedType.text);
    }
  }
);

watch(() => tab.value, async (newTab) => {
  if (newTab !== "2_tab") return;

  if (model.value.projectId) await getAllReqPlanTaskIssuesByProjectId(model.value.projectId);

  if (releaseTrackingId) {
    await loadMappedItems();
  }
});

watch(() => releaseTrackingId, (newValue, oldValue) => {
  if (newValue) {
    getReleaseTrackingInDetailsById(releaseTrackingId);
  }
}, { immediate: true });

const isInitialLoad = ref(true);

watch(
  () => model.value.projectId,
  async (newValue, oldValue) => {
    if (!newValue) return;

    // Always load dropdowns
    await infraProjectInstanceTypeDropdownSingleSelect.load(newValue);
    await projectEmployeeDropdownSingleSelect.load(newValue);

    if (props.id && isInitialLoad.value) {
      isInitialLoad.value = false;
      return;
    }

    if (newValue === oldValue) return;

    model.value.infraInstanceId = null;
    model.value.deploymentOwnerId = null;
    model.value.approverId = null;
    model.value.testerId = null;
    model.value.releaseTypeId = null;
    model.value.versionNumber = "";
  }
);
// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------
onMounted(() => {
  projectNameDropdownSingleSelect.load();
  releaseTrackingTypeDropdownSingleSelect.load("Release Tracking Type");

  if (model.value.projectId) {
    infraProjectInstanceTypeDropdownSingleSelect.load(model.value.projectId);
    projectEmployeeDropdownSingleSelect.load(model.value.projectId);
  }
});

</script>
<style>
.ellipsis-cell {
  max-width: 260px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.same-size-btn {
  min-width: 150px;
  height: 50px;
}
</style>
