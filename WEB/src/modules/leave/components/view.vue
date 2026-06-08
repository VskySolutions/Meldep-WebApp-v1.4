<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1000px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Leave Details</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
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
                <div class="q-mb-xs">Leave Approver Name</div>
                <div class="text-black">{{ model.leaveApprover.person.firstName ? model.leaveApprover.person.firstName + " " + model.leaveApprover.person.lastName : "-" }}</div>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <legend>Leave Request Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">From Date </div>
                <div class="text-black">{{ model.fromDate ? model.fromDate : "-" }}</div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">To Date </div>
                <div class="text-black">{{ model.toDate ? model.toDate : "-" }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div v-if="approveleaveId !== ''" class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Leave Status</div>
                <div class="text-black">{{ model.leaveStatuses.dropDownValue == 'Sent to Approver' ? "Waiting for Approval" : model.leaveStatuses.dropDownValue }}</div>
              </div>
              <div v-else-if="forwardleaveid !== ''" class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Leave Status</div>
                <div class="text-black">{{ model.leaveStatuses.dropDownValue == 'Applied' ? "Waiting to Send for Approver" : model.leaveStatuses.dropDownValue }}</div>
              </div>
              <div v-else class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Leave Status</div>
                <div class="text-black">{{ model.leaveStatuses.dropDownValue ? model.leaveStatuses.dropDownValue : "-" }}</div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Leave Type</div>
                <div class="text-black">{{ model.leaveCategories.dropDownValue ? model.leaveCategories.dropDownValue : "-" }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">No of Leaves </div>
                <div class="text-black">{{ model.noofLeaves ? model.noofLeaves : "-" }}</div>
              </div>
              <div v-if="model.noofLeaves < 1" class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Half Day Type</div>
                <div class="text-black">{{ model.halfDayType ? model.halfDayType : "-" }}</div>
              </div>
              <div v-if="model.isSandwich" class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Is Sandwich?</div>
                <div class="text-black">{{ model.isSandwich ? "Yes" : "No" }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Paid/Unpaid Status</div>
                <div class="text-black">{{ model.isPaidLeave ? "Paid" : "Unpaid" }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-lg-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Reason</div>
                <div class="text-black"><p v-html="model.reason ? model.reason : '-'" /></div>
              </div>
            </div>
          </fieldset>
          <fieldset v-if="approveleaveId !== ''">
            <legend>HR Note</legend>
            <div class="row q-col-gutter-x-md">
              <div class="col-lg-12 col-sm-6 col-md-6">
                <div class="text-black break-word"><p v-html="model.hrNote ? model.hrNote : '-'" /></div>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <legend>Approver Note</legend>
            <div class="row q-col-gutter-x-md">
              <div class="col-lg-12 col-sm-6 col-md-6">
                <div class="text-black break-word"><p v-html="model.approverNote ? model.approverNote : '-'" /></div>
              </div>
              <!-- <div class="col-lg-12">
                    <p class="text-black leavenote break-word" v-html="model.approverNote ? model.approverNote : '-'"></p>
                  </div> -->
            </div>
          </fieldset>
          <fieldset v-if="model.file.virtualPath != null">
            <legend>Proof Of Medical</legend>
            <div class="row justify-center">
              <!-- Display Image -->
              <img v-if="isImage(model.file.virtualPath)" :src="model.file.virtualPath" alt="Uploaded File" style="width: 30%;">
              <!-- Display File Link -->
              <a v-else :href="model.file.virtualPath" target="_blank" rel="noopener noreferrer" style="text-decoration: none; text-align: center; display: inline-block;">
                <i class="fa fa-file" style="font-size: 30px; color: gray; transition: transform 0.2s, color 0.2s;" />
                <span style="display: block; font-size: 14px; color: #555; margin-top: 8px;">
                  View File
                </span>
              </a>
            </div>
          </fieldset>
        </div>
      </div>
      <q-card-actions v-if="canCancelLeave" align="center" class="stickyFooter">
        <q-btn color="red-6" push outline label="Cancel Leave" type="submit" class="actionBtn" :loading="processing" no-caps @click="onCancel" />
      </q-card-actions>
      <!-- </q-card-section> -->
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted, computed } from "vue";
import { notifySuccess, zwConfirm } from "assets/utils";
import { useAuthStore } from "stores/auth";
import _ from "lodash";
import leaveService from "../leave.service";

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" },
  leaveId: { type: String, default: "" },
  approveleaveId: { type: String, default: "" },
  forwardleaveid: { type: String, default: "" }
});

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK } = useDialogPluginComponent();

const loading = ref(true);
const processing = ref(false);
const authStore = useAuthStore();
const user = authStore.user;

// const baseURL = process.env.API_BASE_URL;
// Define model values
const model = ref({
  halfDayType: "",
  fromDate: "",
  toDate: "",
  reason: "",
  noofLeaves: "",
  leaveApproverId: "",
  employee: {
    person: {
      firstName: "",
      lastName: ""
    }
  },
  leaveApprover: {
    person: {
      lastName: ""
    }
  },
  leaveStatuses: {
    dropDownValue: ""
  },
  leaveCategories: {
    dropDownValue: ""
  },
  file: {
    virtualPath: ""
  }
});

const getEmployeeLeave = () => {
  loading.value = true;
  leaveService.getEmployeeLeave(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

const canCancelLeave = computed(() => {
  const roles = (user?.roles || []).map(r => r.toLowerCase());
  const currentEmployeeId = user?.employeeId;

  const isAdminOrHR =
    roles.includes("admin") || roles.includes("hr");

  const isLeaveApproverOfThisEmployee = model.value?.leaveApproverId === currentEmployeeId;

  const isApproved = model.value?.leaveStatuses?.dropDownValue === "Approved";

  return isApproved && (isAdminOrHR || isLeaveApproverOfThisEmployee);
});

const onCancel = async () => {
  processing.value = true;
  try {
    zwConfirm({ data: `${model.value.employee.person.firstName}` }, () => {
      leaveService.cancelEmployeeLeave(props.id).then(resp => {
        notifySuccess({ message: "Leave Cancelled successfully." });
        onDialogOK();
      });
    }, () => {
    });
  } catch (error) {
    console.error("Error in canceling the leave:", error);
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

function isImage (path) {
  const imageExtensions = ["jpg", "jpeg", "png", "gif", "bmp", "webp"];
  const extension = path.split(".").pop().toLowerCase();
  return imageExtensions.includes(extension);
}

onMounted(() => {
  getEmployeeLeave();
});
</script>

<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.leavenote {
  width: 900px !important;
  max-width: 900px !important;
  white-space: normal;
  overflow-wrap: break-word;
}
</style>
