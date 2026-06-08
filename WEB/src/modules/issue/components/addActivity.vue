<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic"
      style="width: 60vw !important;max-width: 60vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Add Activity</div>
        <q-btn v-close-popup icon="o_close" color="white" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="q-mb-xs text-black">Activity Name<span class="required">*</span></div>
                  <div class="form-group">
                    <q-input
                      outlined
                      v-model="model.activityName"
                      autogrow
                      :error="v$.activityName.$error"
                      :error-message="v$.activityName.$errors[0]?.$message"
                      @blur="v$.activityName.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-12 col-md-6 col-lg-4">
                  <div class="q-mb-xs text-black">Due Date<span class="required">*</span></div>
                  <q-input
                    v-model="model.dueDate"
                    :dense="true"
                    outlined
                    stack-label
                    hide-bottom-space mask="##/##/####"
                    :error="v$.dueDate.$error"
                    :error-message="v$.dueDate.$errors[0]?.$message"
                    @blur="v$.dueDate.$touch"
                  >
                    <template #append>
                      <q-icon name="o_calendar_month" class="cursor-pointer">
                        <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                          <q-date
                            v-model="model.dueDate"
                            mask="MM/DD/YYYY"
                            @update:model-value="() => $refs.qDateProxy.hide()"
                          />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
                <div class="col-12 col-sm-12 col-md-6 col-lg-4">
                  <div class="q-mb-xs text-black">Priority<span class="required">*</span></div>
                  <q-select
                    v-model="model.priorityId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="priorityList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.priorityId.$error"
                    :error-message="v$.priorityId.$errors[0]?.$message"
                    @blur="v$.priorityId.$touch"
                    @filter="getAllPriorityLisDropDownForFilter"
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
                </div>
                <div class="col-12 col-sm-12 col-md-6 col-lg-4">
                  <div class="q-mb-xs text-black">Owner<span class="required">*</span></div>
                    <q-select
                      v-model="model.assignedTo" clearable use-input outlined stack-label hide-bottom-space :dense="true"
                      :options="employeeList" option-value="value" option-label="text" emit-value map-options :error="v$.assignedTo.$error" :error-message="v$.assignedTo.$errors[0]?.$message" @blur="v$.assignedTo.$touch" @filter="getAllEmployeesListDropdownForFilter">
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
                </div>
              </div>
              <q-card-actions align="center" class="q-mt-md q-gutter-sm justify-center">
                <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
              </q-card-actions>
            </fieldset>
            <fieldset>
              <legend>Activities</legend>
              <div class="row">
                <div class="col">
                  <q-table virtual-scroll class="border Custom-DataTable" ref="tableRef" v-model:pagination="pagination" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell" no-data-label="No data available" binary-state-sort>
                    <template #header="props">
                      <q-tr :props="props" class="bg-primary text-white">
                        <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                        <q-th auto-width class="text-center">Actions</q-th>
                      </q-tr>
                    </template>
                    <template #body="props">
                      <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                        <q-td>{{ props.row.activityName}}</q-td>
                        <q-td>{{ props.row.dueDate}}</q-td>
                        <q-td>{{ props.row.priority.dropDownValue }}</q-td>
                        <q-td>{{ props.row.assignedToEmployee.person.fullName  }}</q-td>
                        <q-td auto-width class="text-center actions">
                          <q-icon name="o_edit" class="cursor-pointer q-mr-sm" @click="onEdit(props.row)">
                            <q-tooltip>Edit</q-tooltip>
                          </q-icon>
                          <q-icon name="o_delete_outline" class="cursor-pointer" color="negative"
                            @click="onDelete(props.row)">
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
import { useDialogPluginComponent } from "quasar";
import issueService from "../issue.service";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import _ from "lodash";
import employeesService from "src/modules/employee/employee.service";
import commonService from "services/common.service";

// define props
const props = defineProps({ id: { type: String, default: "" } });

// define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// common variables
const loading = ref(true);
const processing = ref(false);
const rows = ref([]);
const activeRowId = ref(null);

const pagination = ref({ sortBy: "firstName", descending: false, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "activityName", label: "Activity Name", field: "activityName", align: "left", sortable: true },
  { name: "dueDate", label: "DueDate", field: "dueDate", align: "left", sortable: true },
  { name: "priority.dropDownValue", label: "Priority", field: "priority.dropDownValue", align: "left", sortable: true },
  { name: "assignedToEmployee.person.fullName", label: "Assign To", field: "assignedToEmployee.person.fullName", align: "left", sortable: true }
]);

const model = ref({
});

// validations
const rules = {
  activityName: { required: helpers.withMessage("Activity name is required", required) },
  dueDate: { required: helpers.withMessage("Due date is required", required) },
  priorityId: { required: helpers.withMessage("Priority is required", required) },
  assignedTo: { required: helpers.withMessage("Owner is required", required) }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// get issue and map
const getIssueActivity = () => {
  loading.value = true;
  issueService.getActivity(props.id).then((resp) => {
    rows.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// Get all Assign To list
const employeeList = ref([]);
const employeeListFilter = ref([]);
function getAllEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    employeeList.value = responseData;
    employeeListFilter.value = responseData;
  });
}

// Search Assign To for dropdown
function getAllEmployeesListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = employeeListFilter.value;
    } else {
      employeeList.value = employeeListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all issue priority List
const priorityList = ref([]);
const priorityListFilter = ref([]);
function getAllPriorityListForDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    priorityList.value = responseData;
    priorityListFilter.value = responseData;

    // Set "high" status as the default if it exists
    const newStatus = responseData.find(status => status.text.toLowerCase() === "high");
    if (newStatus && props.id === "") {
      model.value.priorityId = newStatus.value;
    }
  });
}

// Search issue priority List for dropdown
function getAllPriorityLisDropDownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      priorityList.value = priorityListFilter.value;
    } else {
      priorityList.value = priorityListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const onEdit = (item) => {
  model.value = item;
  activeRowId.value = item.id;
};

const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.activityName}` }, () => {
    issueService.deleteActivity(item.id).then(resp => {
      notifySuccess({ message: "Issue Activity is deleted successfully." });
      getIssueActivity({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};

async function onSubmit () {
  processing.value = true;
  try {
    if (!await v$.value.$validate()) {
      return;
    }

    const payload = {
      id: activeRowId.value,
      issueId: props.id,
      activityName: model.value.activityName,
      dueDate: model.value.dueDate,
      priorityId: model.value.priorityId,
      assignedTo: model.value.assignedTo
    };
    issueService.saveIssueActivity(payload).then(resp => {
      notifySuccess({ message: "Issue Activity is saved successfully." });
      getIssueActivity({ pagination: pagination.value });
    });
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
    getIssueActivity();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  getAllEmployeesListForDropdown();
  getAllPriorityListForDropDown("Issue Priority");
});

</script>
<style>
.hidden {
  display: none;
}
</style>
