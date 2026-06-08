<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1100px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Assign Bulk</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable']">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>User assign Info</legend>
              <div class="row flex items-center q-my-md">
                <div class="col-4">
                  <label class="q-mb-xs q-ml-sm text-black">Employee Name<span class="required">*</span></label>
                  <q-select
                    v-model="model.employeeIds"
                    push
                    class="q-mx-sm w-100 h-auto"
                    clearable
                    use-input
                    outlined
                    use-chips
                    transition-show="jump-up"
                    transition-hide="jump-up"
                    hide-bottom-space
                    :dense="true"
                    multiple
                    fill-input
                    input-debounce="0"
                    :options="employeeList"
                    option-value="value"
                    option-label="text"
                    emit-value map-options
                    :popup-content-class="customPopupContentClass"
                    :error="v$.employeeIds.$error"
                    :error-message="v$.employeeIds.$errors[0]?.$message"
                    @filter="getAllActiveEmployeesListForFilter"
                    @blur="v$.employeeIds.$touch"
                  >
                    <template #option="{ itemProps, opt, selected, toggleOption }">
                      <q-item v-bind="itemProps">
                        <q-item-section>
                          <div class="row q-col-gutter-x-md items-center selection-drodown-item">
                            <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                            <span>{{ opt.text }}</span>
                          </div>
                        </q-item-section>
                      </q-item>
                    </template>
                  </q-select>
                </div>
                <div class="col-4">
                  <label class="q-mb-xs q-ml-sm text-black">Report Name<span class="required">*</span></label>
                  <q-select
                    v-model="model.reportIds"
                    push
                    class="q-mx-sm w-100 h-auto"
                    clearable
                    use-input
                    outlined
                    use-chips
                    transition-show="jump-up"
                    transition-hide="jump-up"
                    hide-bottom-space
                    :dense="true"
                    multiple
                    fill-input
                    input-debounce="0"
                    :options="reportNameList"
                    option-value="value"
                    option-label="text"
                    emit-value map-options
                    :popup-content-class="customPopupContentClass"
                    :error="v$.reportIds.$error"
                    :error-message="v$.reportIds.$errors[0]?.$message"
                    @filter="getAllReportListForFilter"
                    @blur="v$.reportIds.$touch"
                  >
                    <template #option="{ itemProps, opt, selected, toggleOption }">
                      <q-item v-bind="itemProps">
                        <q-item-section>
                          <div class="row q-col-gutter-x-md items-center selection-drodown-item">
                            <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                            <span>{{ opt.text }}</span>
                          </div>
                        </q-item-section>
                      </q-item>
                    </template>
                  </q-select>
                </div>
                <div class="col-3 q-mb-sm justify-end" align="right">
                  <label class="q-mb-xs q-ml-sm text-black">Select All</label>
                  <q-toggle
                    v-model="selectAllPermissions"
                    color="primary"
                    no-caps :disable="!model.reportIds || model.reportIds.length === 0"
                    @update:model-value="toggleAllPermissions"
                  />
                </div>
              </div>
              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                virtual-scroll bordered
                :loading="loading"
                :rows="UserRows"
                :columns="UserColumns"
                row-key="id"
                separator="cell"
                :rows-per-page-options="[20, 50, 100, 200, 500]"
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th
                      v-for="col in props.cols"
                      :key="col.name"
                      class="text-center"
                    >
                      {{ col.label }}
                      <template v-if="['fullAccess', 'viewOnly'].includes(col.name)">
                        <q-checkbox
                          v-model="selectAll[col.name]"
                          dense
                          color="primary"
                          @update:model-value="toggleAll(col.name, $event)"
                        />
                      </template>
                    </q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :class="props.row.deleted ? 'hidden' : 'edit_tasks'">
                    <q-td style="width: 35%;">
                      <q-select
                        v-model="props.row.reportSettingsDetailId"
                        use-input
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        readonly
                        :options="reportNameList"
                        option-value="value"
                        option-label="text"
                        emit-value
                        map-options
                        :error="rowValidations[props.rowIndex]?.value?.aspNetUserId.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.aspNetUserId.$errors[0]?.$message" @filter="getAllReportListForFilter" @blur="rowValidations[props.rowIndex]?.value?.aspNetUserId.$touch"
                      >
                        <template #option="{ itemProps, opt }">
                          <q-item v-bind="itemProps">
                            <q-item-section>
                              <div class="row q-col-gutter-x-md items-center">
                                <span>{{ opt.text }}</span>
                              </div>
                            </q-item-section>
                          </q-item>
                        </template>
                      </q-select>
                    </q-td>
                    <td class="text-center" style="width: 20%; vertical-align: middle;">
                      <q-checkbox v-model="props.row.fullAccess" color="primary" />
                    </td>
                    <td class="text-center" style="width: 20%; vertical-align: middle;">
                      <q-checkbox v-model="props.row.viewOnly" color="primary" />
                    </td>
                  </q-tr>
                </template>
              </q-table></fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { uid, useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import { notifySuccess, notifyError } from "assets/utils";

import reportService from "modules/reports/reports.service";
import employeesService from "src/modules/employee/employee.service";

// Common variables
const loading = ref(true);
const processing = ref(false);
const UserRows = ref([]);
const model = ref({});
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel, onDialogOK } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const reportSettingsDetailId = props.id;

// get report user on edit mode
const getReportUserByReportSettingsDetailId = () => {
  loading.value = true;
  reportService.getReportUserByReportSettingsDetailId(reportSettingsDetailId).then((resp) => {
    UserRows.value = resp.map(item => ({
      ...item,
      id: item.id,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

const UserColumns = ref([
  { name: "aspNetUserId", label: "Report", field: "aspNetUserId", sortable: false },
  { name: "fullAccess", label: "Manage Permission", field: "fullAccess" },
  { name: "viewOnly", label: "View Permission", field: "viewOnly" }
]);

const selectAll = ref({
  fullAccess: false,
  viewOnly: false
});
const selectAllPermissions = ref(false);

function toggleAll (field, value) {
  UserRows.value.forEach(row => {
    row[field] = value;
  });
}

function toggleAllPermissions (value) {
  UserRows.value.forEach(row => {
    row.fullAccess = value;
    row.viewOnly = value;
    row.notes = value;
  });
  selectAll.value.fullAccess = value;
  selectAll.value.viewOnly = value;
}

// Get all report name list for dropdown
const reportNameList = ref([]);
const reportNameListFilter = ref([]);
function getAllReportListForDropdown () {
  reportService.getAllReportListForDropdown().then((resp) => {
    const responseData = resp
      .map((item) => ({ text: item.reportName, value: item.id }))
      .sort((a, b) => a.text.localeCompare(b.text));

    reportNameList.value = responseData;
    reportNameListFilter.value = responseData;
  });
}

// Search report name for dropdown
function getAllReportListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      reportNameList.value = reportNameListFilter.value;
    } else {
      reportNameList.value = reportNameListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all active employee List for dropdown
const employeeList = ref([]);
const employeeListFilter = ref([]);
function getAllActiveEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id }));
    employeeList.value = responseData;
    employeeListFilter.value = responseData;
  });
}

// Search active employee List for filter
function getAllActiveEmployeesListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = employeeListFilter.value;
    } else {
      employeeList.value = employeeListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Validation rules
const rules = {
  employeeIds: { required: helpers.withMessage("Employee is required", required) },
  reportIds: { required: helpers.withMessage("Report is required", required) }
};
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });
const rowValidations = ref([]);

// compute invalid row indexes
const invalidRows = computed(() =>
  UserRows.value.map((row, idx) =>
    !(row.fullAccess || row.viewOnly || row.notes) ? idx : null
  ).filter(i => i !== null)
);

// Submit form
const onSubmit = async () => {
  if (invalidRows.value.length > 0) {
    notifyError({ message: "Select at least one permission (Manage, View) for each row." });
    return;
  }
  if (!await v$.value.$validate()) { return; }
  processing.value = true;

  // Prepare the payload as a plain object
  const payload = {
    reportUserMapping: UserRows.value
  };
  // Submit the payload
  reportService.assignBulk(model.value.employeeIds, payload).then((resp) => {
    notifySuccess({ message: "User assigned successfully." });
    onDialogOK();
  }).catch((err) => {
    if (!err.response?.status === 404) {
      notifyError({ message: "Something went wrong." });
    }
  }).finally(() => {
    processing.value = false;
  });
};

// On page rendering
onMounted(() => {
  getAllReportListForDropdown();
  getAllActiveEmployeesListForDropdown();
});

// watches a data property with the same name i.e. immediate effect
watch(() => reportSettingsDetailId, (newValue, oldValue) => {
  if (newValue) {
    getReportUserByReportSettingsDetailId();
  }
}, { immediate: true });

const previousReportSettingsIds = ref([]);
watch(
  () => model.value.reportIds || [],
  (newIds = []) => {
    // Add new reports
    const added = newIds.filter(id => !previousReportSettingsIds.value.includes(id));
    for (const reportId of added) {
      const report = reportNameList.value.find(p => p.value === reportId);
      if (!report) continue;

      const alreadyExists = UserRows.value.some(r => r.reportId === reportId);
      if (alreadyExists) continue;

      UserRows.value.push({
        id: uid(),
        reportSettingsDetailId: reportId,
        reportName: report.text,
        aspNetUserId: "",
        fullAccess: false,
        viewOnly: false,
        deleted: false
      });
    }
    // Remove deselected reports
    const removed = previousReportSettingsIds.value.filter(id => !newIds.includes(id));
    if (removed.length > 0) {
      UserRows.value = UserRows.value.filter(row => !removed.includes(row.reportSettingsDetailId));
    }

    // Update tracking
    previousReportSettingsIds.value = [...newIds];
  },
  { immediate: true }
);
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_tasks .q-select__dropdown-icon{
  display: none;
}
</style>
