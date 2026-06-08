<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:900px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Leave Details</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <!-- <q-card-section class="card-header with-tools"> -->
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Employee Info</legend>
              <!-- <q-card class="card-header with-tools headerBasic"> -->
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Employee Name</div>
                  <div class="text-black">{{ model.employee.person.firstName + " " + model.employee.person.lastName }}</div>
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Leave Approver Name</div>
                  <div class="text-black">{{ model.approverName ? model.approverName : "-" }}</div>
                </div>
              </div>
            </fieldset>
            <fieldset>
              <legend>Leave Request Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-4">
                  <div class="q-mb-xs">From Date</div>
                  <div class="text-black">{{ model.fromDate }}</div>
                </div>
                <div class="col-12 col-sm-6 col-md-4">
                  <div class="q-mb-xs">To Date</div>
                  <div class="text-black">{{ model.toDate }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-4">
                  <div class="q-mb-xs">No of Leaves</div>
                  <div class="text-black">{{ model.noofLeaves }}</div>
                </div>
                <div class="col-12 col-sm-6 col-md-4">
                  <div class="q-mb-xs">Remaining Leave Balance</div>
                  <div class="text-black">{{ empModel }}</div>
                </div>
                <div v-if="model.isSandwich" class="col-12 col-sm-6 col-md-4">
                  <div class="q-mb-xs">Is Sandwich?</div>
                  <div class="text-black">{{ model.isSandwich ? "Yes" : "No" }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-4">
                  <div class="q-mb-xs">Leave Status</div>
                  <div class="text-black">{{ model.leaveStatuses.dropDownValue == 'Applied' ? "Waiting to Send for Approver" : model.leaveStatuses.dropDownValue }}</div>
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Leave Type</div>
                  <div class="text-black">{{ model.leaveCategories.dropDownValue ? model.leaveCategories.dropDownValue : "-" }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-lg-12 col-sm-12 col-md-12">
                  <div class="q-mb-xs">Reason :</div>
                  <div class="text-black RichTextEditor"><p v-html="model.reason" /></div>
                </div>
              </div>
              <!-- <div class="row q-col-gutter-x-md">
                <div class="col-3">
                  <div class="q-mb-xs text-black text-black">Remaining Leave Balance :</div>
                </div>
                <div class="col-9">
                  <div class="q-mb-xs text-primary">{{ model.noofLeaves }}</div>
                </div>
              </div> -->
            </fieldset>
            <fieldset>
              <legend>Approver Info</legend>
              <div class="row q-col-gutter-x-md">
                <!-- <div class="col-3">
                  <div class="q-mb-xs text-black text-black">Approver Name :</div>
                </div>
                <div class="col-9">
                  <div class="q-mb-xs text-primary">{{ model.noofLeaves }}</div>
                </div> -->
                <div class="col-12 col-md-6">
                  <div class="row items-center">
                    <div class="col-12 col-sm-4">
                      <div class="q-mb-xs">
                        Approver Name <span class="required">*</span>
                      </div>
                    </div>
                    <div class="col-12 col-sm-8">
                      <q-select
                        v-model="model.leaveApproverId"
                        clearable
                        use-input
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        :options="personList"
                        option-value="value"
                        option-label="text"
                        emit-value
                        map-options
                        :error="v$.leaveApproverId.$error"
                        :error-message="v$.leaveApproverId.$errors[0]?.$message"
                        @blur="v$.leaveApproverId.$touch"
                        @filter="getAllEmployeesListDropdownForFilter"
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
                  </div>
                </div>
                <div class="col-12 col-md-6">
                  <div class="row items-center q-mt-xs">
                    <div>Paid/Unpaid Status :</div>
                    <q-radio
                      v-model="model.isPaidLeave"
                      label="Paid"
                      dense
                      class="text-black q-ml-sm"
                      :val="true"
                    />
                    <q-radio
                      v-model="model.isPaidLeave"
                      label="Unpaid"
                      dense
                      class="text-black q-ml-sm"
                      :val="false"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset>
              <legend>HR Note</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-12 col-md-12 col-lg-12">
                  <div>
                    <div class="form-group">
                      <q-input
                        v-model="model.hRNote"
                        outlined
                        autogrow
                        hint="The maximum length allowed is 500."
                        maxlength="500"
                        :error="v$.hRNote.$error"
                        :error-message="v$.hRNote.$errors[0]?.$message"
                        @blur="v$.hRNote.$touch"
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
          <q-btn color="primary" push outline label="Send" type="submit" class="actionBtn" :loading="processing" :disable="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent } from "quasar";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { onMounted, ref, watch } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import _ from "lodash";
import leaveService from "../leave.service";
import employeesService from "modules/employee/employee.service";

// define props
const props = defineProps({ id: { type: String, default: "" } });

// define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// common variables
const loading = ref(true);
const processing = ref(false);
const empModel = ref({});

// define model
const model = ref({
  employee: {
    person: {
      firstName: "",
      lastName: "",
      fromDate: "",
      toDate: "",
      reason: "",
      noofLeaves: ""
    }
  },
  leaveStatuses: {
    dropDownValue: ""
  },
  leaveCategories: {
    dropDownValue: ""
  },
  leaveApproverId: "",
  isPaidLeave: false
});

// Validation rules
const rules = {
  leaveApproverId: { required: helpers.withMessage("Approver name is required", required) },
  hRNote: { minLength: minLength(1), maxLength: maxLength(500) }
};

// get Employee Leave Balance
const getEmployeeLeaveBalance = (employeeId) => {
  leaveService.getEmployeeLeaveBalance(employeeId).then((resp) => {
    empModel.value = resp;
  }).finally(() => {
  });
};

// Get all employee list for dropdown
const personList = ref([]);
const personListFilter = ref([]);
function getAllEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id }));
    personList.value = responseData;
    personListFilter.value = responseData;
  });
}

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Search person for dropdown
function getAllEmployeesListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      personList.value = personListFilter.value;
    } else {
      personList.value = personListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const getEmployeeLeave = () => {
  loading.value = true;
  leaveService.getEmployeeLeave(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    getEmployeeLeaveBalance(model.value.employee.id);
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getEmployeeLeave();
  }
}, { immediate: true });

const onSubmit = async () => {
  processing.value = true;
  try {
    if (await v$.value.$validate()) {
      processing.value = true;
      model.value.flag = "FD";
      leaveService.saveEmployeeLeaveDetails(props.id, model.value).then((resp) => {
        notifySuccess({ message: "Leave Forwarded successfully." });
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
    }, 3000);
  }
};

onMounted(() => {
  getAllEmployeesListForDropdown();
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
