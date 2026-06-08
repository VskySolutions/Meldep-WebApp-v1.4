<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none" persistent position="right" @hide="onDialogHide">
    <q-card
      class="q-dialog-plugin PersonMain card-header with-tools headerBasic"
      style="width: 100%; max-height: 100% !important; max-width: 250vw; height: 100%;"
    >
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Add Candidate Activity</div>
        <q-btn v-close-popup icon="o_close" color="white" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Candidate Activity Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col">
                  <div class="q-mb-xs text-black">Activity Name<span class="required">*</span></div>
                  <q-input
                    v-model="model.activityName"
                    autogrow
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="false"
                    maxlength="450"
                    :error="v$.activityName.$error"
                    :error-message="v$.activityName.$errors[0]?.$message"
                    @blur="v$.activityName.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-lg-4 col-md-4 col-sm-6 col-xs-6">
                  <formDate
                    v-model="model.dueDate"
                    label="Due Date"
                    :error="v$.dueDate.$error"
                    :error-message="v$.dueDate.$errors[0]?.$message"
                    :onBlur="() => v$.dueDate.$touch()"
                  />
                </div>
                <div class="col-lg-4 col-md-4 col-sm-6 col-xs-6">
                  <formSingleSelectDropdown
                    v-model="model.priorityId"
                    label="Priority"
                    :options="candidatePriorityDropdownSingleSelect.list.value"
                    :filter="candidatePriorityDropdownSingleSelect.filter"
                    :error="v$.priorityId.$error"
                    :error-message="v$.priorityId.$errors[0]?.$message"
                  />
                </div>
                <div class="col-lg-4 col-md-4 col-sm-6 col-xs-6">
                  <formSingleSelectDropdown
                    v-model="model.employeeOwnerId"
                    label="Owner"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                    :error="v$.employeeOwnerId.$error"
                    :error-message="v$.employeeOwnerId.$errors[0]?.$message"
                  />
                </div>
              </div>
              <q-card-actions align="center" class="q-mt-md">
                <q-btn
                  color="grey-4"
                  push
                  outline
                  label="Close"
                  type="button"
                  class="text-grey-9 actionBtn"
                  no-caps
                  @click="onDialogCancel"
                />
                <q-btn
                  color="primary"
                  push
                  outline
                  label="Save"
                  type="submit"
                  class="actionBtn"
                  :loading="processing"
                  :disable="processing"
                  no-caps
                />
              </q-card-actions>
            </fieldset>
            <fieldset>
              <legend>Activity Log</legend>
              <div class="row">
                <div class="col">
                  <q-table
                    ref="tableRef"
                    v-model:pagination="pagination"
                    style="width: 1170px"
                    virtual-scroll
                    class="border Custom-DataTable"
                    :loading="loading"
                    :rows="rows"
                    :columns="columns"
                    row-key="id"
                    separator="cell"
                    no-data-label="No data available"
                    binary-state-sort
                    @request="getRoles"
                  >
                    <template #header="props">
                      <q-tr :props="props" class="bg-primary text-white">
                        <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                        <q-th auto-width class="text-center">Actions</q-th>
                      </q-tr>
                    </template>
                    <template #body="props">
                      <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                        <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;" class="RichTextEditor">
                          <div class="text-black" v-html="props.row.activityName" />
                        </q-td>
                        <q-td style="width: 5%;">
                          {{ props.row.dueDate }}
                        </q-td>
                        <q-td style="width: 5%;">
                          {{ props.row.priority.dropDownValue }}
                        </q-td>
                        <q-td style="width: 5%;">
                          {{ props.row.employee.person.fullName }}
                        </q-td>
                        <q-td style="width: 5%;" class="text-center actions">
                          <q-icon 
                            name="o_edit" 
                            class="cursor-pointer q-mr-sm" 
                            :class="storedUser.username === props.row.user.userName ? '' : 'hidden'" 
                            @click="onEdit(props.row)"
                          >
                            <q-tooltip>Edit</q-tooltip>
                          </q-icon>
                          <q-icon 
                            name="o_delete_outline" 
                            class="cursor-pointer" 
                            :class="storedUser.username === props.row.user.userName ? '' : 'hidden'" 
                            color="negative" 
                            @click="onSubmitCandidateActivityDelete(props.row.id, props.row.activityName, refreshCandidateActivityLog)"
                          >
                            <q-tooltip>Delete</q-tooltip>
                          </q-icon>
                        </q-td>
                      </q-tr>
                    </template>
                  </q-table>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent, date } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import { isDate } from "validators/zw_validators.js";
import { notifySuccess, notifyError, getLocalStorage, notifyWarning } from "assets/utils";
import { useAuthStore } from "stores/auth";
import _ from "lodash";
import { format } from "date-fns"; // Standard TimeZone Conversion

import candidateService from "../candidate.service";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// Shared Dropdowns
import candidateModule from "src/modules/candidate/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";

// Shared Candidate Actions
import {
  initCandidateActions,
  onSubmitCandidateActivityDelete
} from "src/modules/candidate/utils/actions.js";

// define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// define props
const props = defineProps({ id: { type: String, default: "" } });

// common variables
const storedUser = getLocalStorage("user");
const authStore = useAuthStore();
const user = authStore.user;
const loading = ref(true);
const processing = ref(false);
const rows = ref([]);
const activeRowId = ref(null);

// table variables
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "activityName", label: "Activity Name", field: "activityName", align: "left", sortable: true },
  { name: "dueDate", label: "Due date", field: "dueDate", align: "left", sortable: true },
  { name: "priority.dropDownValue", label: "Priority", field: "priority.dropDownValue", align: "left", sortable: true },
  { name: "employee.person.fullName", label: "Owner", field: "employee.person.fullName", align: "left", sortable: true }
]);

// define model
const model = ref({
  description: "",
  employeeOwnerId: user?.employeeId ? user.employeeId : "",
  dueDate: format(new Date(), "MM/dd/yyyy")
});

// getCandidateActivityLog
const getCandidateActivityLog = () => {
  loading.value = true;
  candidateService.getActivityLogById(props.id).then((resp) => {
    rows.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// edit activity log
const onEdit = (item) => {
  model.value = item;
  model.value.showCheckbox = true;
  activeRowId.value = item.id;
};

// validation rules
const rules = {
  priorityId: { required: helpers.withMessage("Priority is required", required) },
  dueDate: {
    required: helpers.withMessage("Due date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  employeeOwnerId: { required: helpers.withMessage("Owner is required", required) },
  activityName: { required: helpers.withMessage("Activity name is required", required) }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const refreshCandidateActivityLog = () => {
  getCandidateActivityLog({ pagination: pagination.value });
}

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initCandidateActions(activeRowId);

// ------------------------------------------------------------------------------------
// All Dropdowns
// ------------------------------------------------------------------------------------

const { candidatePriorityDropdownSingleSelect } = candidateModule();
const { activeEmployeesDropdownSingleSelect } = employeeModule();

async function onSubmit () {
  try {
    if (processing.value) {
      notifyWarning({ message: "Double click not allowed. Please wait..." });
      return; // stop further submit
    }
    if (await v$.value.$validate()) {
      processing.value = true;
      const payload = {
        id: activeRowId.value,
        candidateId: props.id,
        dueDate: model.value.dueDate,
        priorityId: model.value.priorityId,
        employeeOwnerId: model.value.employeeOwnerId,
        activityName: model.value.activityName
      };
      candidateService.saveCandidateActivityLogs(payload).then(resp => {
        notifySuccess({ message: "Candidate Activity is saved successfully." });
        // Clear form
        activeRowId.value = null;
        model.value.activityName = "";
        model.value.dueDate = format(new Date(), "MM/dd/yyyy");
        model.value.priorityId = "";
        model.value.employeeOwnerId = user?.employeeId ? user.employeeId : "";
        // Reset validation
        v$.value.$reset();
        refreshCandidateActivityLog();
      });
    }
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    refreshCandidateActivityLog();
  }
}, { immediate: true });

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  activeEmployeesDropdownSingleSelect.load();
  candidatePriorityDropdownSingleSelect.load("Candidate Priority");
});

</script>
