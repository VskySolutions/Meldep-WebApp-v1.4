<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1000px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Leave Details</div>
        <!-- <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense /> -->
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogClose()" />
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
                  <div class="text-black">{{ model.employee.person.firstName ? model.employee.person.firstName + " " + model.employee.person.lastName : "-" }}</div>
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Email Address</div>
                  <div class="text-black">{{ model.employee.person.primaryEmailAddress ? model.employee.person.primaryEmailAddress : "-" }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Phone Number</div>
                  <div class="text-black">{{ model.employee.person.primaryPhoneNumber ? model.employee.person.primaryPhoneNumber : "-" }}</div>
                </div>
              </div>
            </fieldset>
            <fieldset>
              <legend>Leave Request Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6">
                  <div class="q-mb-xs">From Date</div>
                  <div class="text-black">{{ model.fromDate ? model.fromDate : "-" }}</div>
                </div>
                <div class="col-12 col-sm-6">
                  <div class="q-mb-xs">To Date</div>
                  <div class="text-black">{{ model.toDate ? model.toDate : "-" }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6">
                  <div class="q-mb-xs">Leave Status</div>
                  <div class="text-black">{{ model.leaveStatuses.dropDownValue == 'Sent to Approver' ? "Waiting for Approval" : model.leaveStatuses.dropDownValue }}</div>
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Leave Type</div>
                  <div class="text-black">{{ model.leaveCategories.dropDownValue ? model.leaveCategories.dropDownValue : "-" }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6">
                  <div class="q-mb-xs">No. of Leaves</div>
                  <div class="text-black">{{ model.noofLeaves ? model.noofLeaves : "-" }}</div>
                </div>
                <div v-if="model.noofLeaves < 1" class="col-12 col-sm-6">
                  <div class="q-mb-xs">Half Day Type</div>
                  <div class="text-black">{{ model.halfDayType ? model.halfDayType : "-" }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div v-if="model.isSandwich" class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Is Sandwich?</div>
                  <div class="text-black">{{ model.isSandwich ? "Yes" : "No" }}</div>
                </div>
                <div v-if="model.leaveStatuses.dropDownValue !== 'Approved' && model.leaveStatuses.dropDownValue !== 'Decline'" class="col-12 col-sm-6 col-md-6">
                  <div class="row q-gutter-sm">
                    <div class="">Paid/Unpaid Status :</div>
                    <q-radio
                      v-model="model.isPaidLeave"
                      label="Paid"
                      dense
                      class="text-black"
                      :val="true"
                    />
                    <q-radio
                      v-model="model.isPaidLeave"
                      label="Unpaid"
                      dense
                      class="text-black"
                      :val="false"
                    />
                  </div>
                </div>
                <div v-else class="col-12 col-md-6">
                  <div class="q-mb-xs">Paid/Unpaid Status</div>
                  <div class="text-black">{{ model.isPaidLeave ? "Paid" : "Unpaid" }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="q-mb-xs">Reason</div>
                  <div class="text-black"><p v-html="model.reason" /></div>
                </div>
              </div>
            </fieldset>
            <fieldset>
              <legend>HR Note</legend>
              <div class="row q-col-gutter-x-md">
                <div class="col-12">
                  <div class="text-black"><p v-html="model.hrNote ? model.hrNote : '-'" /></div>
                </div>
              </div>
            </fieldset>
            <fieldset v-if="model.leaveStatuses.dropDownValue !== 'Approved' && model.leaveStatuses.dropDownValue !== 'Decline'">
              <legend>Approver Note</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="form-group">
                    <q-input
                      v-model="model.approverNote"
                      outlined
                      autogrow
                      hint="The maximum length allowed is 500."
                      maxlength="500"
                      :error="v$.approverNote.$error"
                      :error-message="v$.approverNote.$errors[0]?.$message"
                      @blur="v$.approverNote.$touch"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset v-else>
              <legend>Approver Note</legend>
              <div class="row q-col-gutter-x-md">
                <div class="col-12">
                  <div class="text-black leavenote">
                    <p v-html="model.approverNote ? model.approverNote : '-'" />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions v-if="model.leaveStatuses.dropDownValue !== 'Approved' && model.leaveStatuses.dropDownValue !== 'Decline'" align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="red-6" push outline label="Decline" type="button" class="actionBtn" :loading="processing" no-caps @click="onDecline" />
          <q-btn color="primary" push outline label="Approve" type="submit" class="actionBtn" :loading="processing1" :disable="processing1" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent } from "quasar";
import { minLength, maxLength } from "@vuelidate/validators";
import { ref, watch } from "vue";
import { notifySuccess, notifyError, zwConfirm } from "assets/utils";
import { useRouter } from "vue-router";
import _ from "lodash";
import useVuelidate from "@vuelidate/core";
import leaveService from "../leave.service";

// define props
const props = defineProps({ id: { type: String, default: "" }, approve: { type: String, default: "" } });

// define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK } = useDialogPluginComponent();

// common variables
const loading = ref(true);
const processing = ref(false);
const processing1 = ref(false);
const router = useRouter();

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
  }
});

// Validation rules
const rules = {
  approverNote: { minLength: minLength(1), maxLength: maxLength(500) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const getEmployeeLeave = () => {
  try {
    loading.value = true;
    leaveService.getEmployeeLeave(props.id).then((resp) => {
      model.value = _.cloneDeep(resp);
      if (resp === "Error") {
        window.location.href = "/auth/login";
      }
    }).finally(() => {
      loading.value = false;
    });
  } catch (error) {
    console.error("Error fetching user:", error);
    if (error.response || error.response.status === 401 || error.response.status === 400) {
      console.error("Unauthorized access. Redirecting to login.");
      window.location.href = "/auth/login";
    }
  }
};

function onDialogClose () {
  if (props.approve === "approve") {
    onDialogOK();
    router.push({ name: "login", params: {} });
  } else {
    onDialogOK();
  }
}

const onDecline = async () => {
  // if (await v$.value.$validate()) {
  //   return;
  // }
  processing.value = true;
  model.value.leaveStatusFlag = "DC";
  model.value.flag = "AV";
  zwConfirm({ data: `${model.value}` }, () => {
    leaveService.saveEmployeeLeaveDetails(props.id, model.value).then((resp) => {
      notifySuccess({ message: "Leave Decline." });
      if (props.approve === "approve") {
        onDialogOK();
        router.push({ name: "login", params: {} });
      } else {
        onDialogOK();
      }
    }).finally(() => {
      processing.value = false;
    });
  }, () => {
    processing.value = false;
  });
};

const onSubmit = async () => {
  processing1.value = true;
  try {
    model.value.flag = "AV";
    zwConfirm({ data: `${model.value}` }, () => {
      processing1.value = true;
      leaveService.saveEmployeeLeaveDetails(props.id, model.value).then((resp) => {
        notifySuccess({ message: "Leave Approved" });
        if (props.approve === "approve") {
          onDialogOK();
          router.push({ name: "login", params: {} });
        } else {
          onDialogOK();
        }
      });
    }, () => {
      processing1.value = false;
    });
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing1.value = true;
    setTimeout(() => {
      processing1.value = false;
    }, 1500);
  }
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getEmployeeLeave();
  }
}, { immediate: true });

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
.leavenote {
  width: 900px !important;
  max-width: 900px !important;
  white-space: normal;
  overflow-wrap: break-word;
}
</style>
