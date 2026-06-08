<template>
  <q-dialog class="customDialog dialog-scrollable-content" ref="dialogRef" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1000px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Leave Credit</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable', readonlyProject != '' ? 'edit_projectModule' : '']">
          <div class="q-gutter-y-md">
              <fieldset>
                <legend>Leave Credit Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-3">
                    <label class="label q-mb-xs text-black">Employee Name</label>
                    <div>
                      <q-input
                        v-model="leaveModel.employeeName"
                        outlined
                        stack-label
                        hide-bottom-space
                        readonly
                        :dense="true"
                      />
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-3">
                    <label class="label q-mb-xs text-black">Credited Leaves</label>
                    <div>
                      <q-input
                        v-model="leaveModel.creditLeaves"
                        outlined
                        stack-label
                        hide-bottom-space
                        readonly
                        :dense="true"
                      />
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-3">
                    <label class="label q-mb-xs text-black">Used Leaves</label>
                    <div>
                      <q-input
                        v-model="leaveModel.usedLeaves"
                        outlined
                        stack-label
                        hide-bottom-space
                        readonly
                        :dense="true"
                      />
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-3">
                    <label class="label q-mb-xs text-black">Remaining Leaves</label>
                    <div>
                      <q-input
                        v-model="leaveModel.remainingLeaves"
                        outlined
                        stack-label
                        hide-bottom-space
                        readonly
                        :dense="true"
                      />
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-3">
                    <label class="label q-mb-xs text-black">Leave Type<span class="required">*</span></label>
                    <q-select
                      v-model="model.leaveTypeId"
                      clearable
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="bonusLeavesList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      @filter="getAllLeaveTypeListDropDownForFilter"
                      :error="v$.leaveTypeId.$error"
                      :error-message="v$.leaveTypeId.$errors[0]?.$message"
                      @blur="v$.leaveTypeId.$touch"
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
                  <div class="col-12 col-sm-6 col-md-3">
                    <label class="label q-mb-xs text-black">Casual Leaves<span class="required"></span></label>
                    <div>
                      <q-input
                        v-model="model.casualLeaves"
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        mask="##.##"
                        hint="00.00"
                        :error="v$.casualLeaves.$error"
                        :error-message="v$.casualLeaves.$errors[0]?.$message"
                        @blur="v$.casualLeaves.$touch"
                      />
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-3">
                    <label class="label q-mb-xs text-black">Sick Leaves<span class="required"></span></label>
                    <div>
                      <q-input
                        v-model="model.sickLeaves"
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        mask="##.##"
                        hint="00.00"
                        :error="v$.sickLeaves.$error"
                        :error-message="v$.sickLeaves.$errors[0]?.$message"
                        @blur="v$.sickLeaves.$touch"
                      />
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-xs-12 col-md-6">
                    <div>
                      <div class="q-mb-xs text-black">Reason<span class="required">*</span></div>
                      <div class="form-group">
                        <q-input
                          outlined
                          v-model="model.creditReason"
                          autogrow
                          hint="The maximum length allowed is 250."
                          :error="v$.creditReason.$error"
                          :error-message="v$.creditReason.$errors[0]?.$message"
                          @blur="v$.creditReason.$touch"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md"></div><br>
          <h1 class="text-dark">Bonus Leaves List</h1><br>
          <q-table
            ref="tableRef"
            :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable'"
            v-model:pagination="pagination"
            :loading="loading"
            :rows="rows"
            :columns="columns"
            row-key="id"
            separator="cell"
            no-data-label="No data available"
            binary-state-sort
            @request="getLeaveCredits"
          >
            <template #header="props">
              <q-tr :props="props" class="bg-primary text-white">
                <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
              </q-tr>
            </template>
            <template #body="props">
              <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                <q-td>
                  {{ toDate(props.row.createdOnUtc) }}
                </q-td>
                <q-td>
                  {{ props.row.casualLeaves }}
                </q-td>
                <q-td>
                  {{ props.row.sickLeaves }}
                </q-td>
                <q-td>
                  {{ props.row.leaveTypes.dropDownValue }}
                </q-td>
                <q-td>
                  {{ props.row.createdBy.person.firstName + " "+ props.row.createdBy.person.lastName }}
                </q-td>
                <q-td>
                  <span style="display: block; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                    {{ props.row.creditReason }}
                  </span>
                </q-td>
              </q-tr>
              <q-separator/>
            </template>
          </q-table>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import _ from "lodash";
import useVuelidate from "@vuelidate/core";
import useFilters from "composables/useFilters";
import commonService from "services/common.service";
import leaveService from "../leave.service";

// define props
const props = defineProps({ id: { type: String, default: "" }, projectIdAttr: { type: String, default: "" } });
const readonlyProject = props.projectIdAttr ? "readonly" : "";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Common variables
const { toDate } = useFilters();
const loading = ref(true);
const processing = ref(false);

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "createdOnUtc", label: "Date", field: "createdOnUtc", align: "left", sortable: true },
  { name: "casualLeaves", label: "Casual Leaves", field: "casualLeaves", align: "left", sortable: true },
  { name: "sickLeaves", label: "Sick Leaves", field: "sickLeaves", align: "left", sortable: true },
  { name: "leaveTypes.dropDownValue", label: "Leave Type", field: "leaveTypes.dropDownValue", align: "left", sortable: true },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left", sortable: true },
  { name: "creditReason", label: "Reason", field: "creditReason", align: "left", sortable: true }
]);

// Define model values
const model = ref({
  casualLeaves: "",
  sickLeaves: "",
  leaveTypeId: "",
  creditReason: ""
});

// Define model values
const leaveModel = ref({});

const atLeastOneLeave = helpers.withMessage(
  "At least one Casual Leave or Sick Leave is required",
  (value, leaves) => {
    return !!(leaves.casualLeaves || leaves.sickLeaves);
  }
);

// Validation rules
const rules = {
  // casualLeaves: { required: helpers.withMessage("Casual Leave is required", required) },
  // sickLeaves: { required: helpers.withMessage("Sick Leave is required", required) },
  casualLeaves: { required: atLeastOneLeave },
  sickLeaves: { required: atLeastOneLeave },
  creditReason: { required: helpers.withMessage("Credit Reason is required", required), minLength: minLength(1), maxLength: maxLength(250) },
  leaveTypeId: { required: helpers.withMessage("Leave Type is required", required) }
};
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// get project details on edit mode
const getLeaveCredits = () => {
  loading.value = true;
  leaveService.getLeaveCredit(props.id).then((resp) => {
    leaveModel.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// Get/Map project list to table
const getEmployeeLeaveData = (employeeId) => {
  leaveService.getLeaveCreditByEmployeeId(employeeId, leaveModel.value.leaveCreditsforYear).then((resp) => {
    rows.value = resp.data;
  }).finally(() => {
    loading.value = false;
  });
};

// Get all project Module Type List
const bonusLeavesList = ref([]);
const bonusLeavesListFilter = ref([]);
function getAllLeaveTypeListForDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    bonusLeavesList.value = responseData;
    bonusLeavesListFilter.value = responseData;
  });
}

// Search project Module Type List for dropdown
function getAllLeaveTypeListDropDownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      bonusLeavesList.value = bonusLeavesListFilter.value;
    } else {
      bonusLeavesList.value = bonusLeavesListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    model.value.employeeId = leaveModel.value.employeeId;
    model.value.leaveCreditId = props.id;
    // Convert empty values to null
    model.value.casualLeaves = model.value.casualLeaves || 0;
    model.value.sickLeaves = model.value.sickLeaves || 0;
    if (await v$.value.$validate()) {
      processing.value = true;
      leaveService.saveLeaveCredit(model.value).then((resp) => {
        notifySuccess({ message: "Leave Credit is saved successfully." });
        onDialogOK();
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
};

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getLeaveCredits();
  }
}, { immediate: true });

// watches a data property with the same name i.e. immediate effect
watch(() => leaveModel.value.employeeId, (newValue, oldValue) => {
  if (newValue) {
    getEmployeeLeaveData(newValue);
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  getAllLeaveTypeListForDropDown("Leave Type");
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
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
</style>
