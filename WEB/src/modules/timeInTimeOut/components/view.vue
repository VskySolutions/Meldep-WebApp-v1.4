<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 800px; height: 100% !important;max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Time In Time Out</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Time In Time Out Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Date</div>
                <div class="text-black">
                  {{ model.timeInDate ? model.timeInDate : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Employee Name</div>
                <div class="text-black">
                  {{ model.employee.person.fullName ? model.employee.person.fullName : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Employee Shift</div>
                <div class="text-black">
                  {{ model.employeeShift || '-' }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Time In</div>
                <div class="text-black">
                  {{ model.timeIn ? model.timeIn : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Time Out</div>
                <div class="text-black">
                  {{ model.timeOut ? model.timeOut : '-' }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Total Hours</div>
                <div class="text-black">
                  {{ model.totalHours ? formatTimeHoursMinutes(model.totalHours) : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Total Break Hours</div>
                <div class="text-black">
                  {{ model.totalBreak ? formatTimeHoursMinutes(model.totalBreak) : '-' }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Actual Working Hours</div>
                <div class="text-black">
                  {{ model.actualHoursStr ? formatTimeHoursMinutes(model.actualHoursStr) : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created By</div>
                <div class="text-black">
                  {{ model.createdBy.person.fullName ? model.createdBy.person.fullName : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created Date</div>
                <div class="text-black">
                  {{ model.createdOnUtc ? model.createdOnUtc : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Updated By</div>
                <div class="text-black">
                  {{ model.updatedBy.person.fullName ? model.updatedBy.person.fullName : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Updated Date</div>
                <div class="text-black">
                  {{ model.updatedOnUtc ? model.updatedOnUtc : "-" }}
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset class="q-mb-lg hidden">
            <legend>Breaks Info</legend>
            <q-table
              ref="tableRef"
              v-model:pagination="pagination"
              class="no-shadow"
              bordered
              :loading="loading"
              :rows="rows"
              :columns="columns"
              row-key="id"
              separator="cell"
              no-data-label="No data available"
              binary-state-sort
              :rows-per-page-options="[20, 50, 100, 200, 500]"
            >
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                  <q-td>
                    {{ props.rowIndex + 1 }}
                  </q-td>
                  <q-td>
                    {{ props.row.breakOutStr }}
                  </q-td>
                  <q-td>
                    {{ props.row.breakInStr }}
                  </q-td>
                  <q-td>
                    {{ props.row.breakReason }}
                  </q-td>
                </q-tr>
              </template>
            </q-table>
          </fieldset>
          <q-card-actions
            v-if="model.employee?.employeeDesignation?.some(
              d => d.leaveApproverId === storedUser.employeeId
            ) &&
              model.workHoursApprovalStatus.dropDownValue !== 'Approved' &&
              model.workHoursApprovalStatus.dropDownValue !== 'Decline'" align="center" class="stickyFooter q-gutter-sm justify-center"
          >
            <q-btn color="primary" push outline label="Approve" type="submit" class="actionBtn" :loading="loading" no-caps @click="submitTimeOutApproval('Approved')" />
            <q-btn color="red-6" push outline label="Decline" type="button" class="actionBtn" :loading="declineLoading" no-caps @click="submitTimeOutApproval('Decline')" />
          </q-card-actions>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";
import { notifySuccess, notifyError, zwConfirm, getLocalStorage } from "assets/utils";
// import { useRouter } from "vue-router";

import timeInTimeOutService from "modules/timeInTimeOut/timeInTimeOut.service";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Common variables
const loading = ref(true);
const declineLoading = ref(false);
// const router = useRouter();

// local storage values
const storedUser = getLocalStorage("user");

// Define model values
const model = ref({
  timeInDate: "-",
  timeIn: "-",
  timeOut: "",
  totalHours: "",
  totalBreak: "",
  actualHours: "",
  employee: {
    person: {
      fullName: ""
    }
  },
  workHoursApprovalStatus: {
    dropDownValue: ""
  },
  employeeShift: "",
  createdBy: {
    person: {
      fullName: ""
    }
  },
  updatedBy: {
    person: {
      fullName: ""
    }
  },
  createdOnUtc: "",
  updatedOnUtc: ""
});

// table variables
const rows = ref([]);
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "breakReason", label: "No.", field: "breakReason", align: "left", sortable: false },
  { name: "breakOutStr", label: "Break Out", field: "breakOutStr", align: "left", sortable: false },
  { name: "breakInStr", label: "Break In", field: "breakInStr", align: "left", sortable: false },
  { name: "breakReason", label: "Break Reason", field: "breakReason", align: "left", sortable: false }
]);

// getTimeInTimeOutDetails
const getTimeInTimeOutDetails = () => {
  loading.value = true;
  timeInTimeOutService.getTimeInTimeOutDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    // Convert employeeDesignation shift names to a comma-separated string
    model.value.employeeShift = (resp.employee.employeeDesignation || [])
      .map(item => item.shift?.dropDownValue)
      .filter(name => !!name)
      .join(", ");
    rows.value = resp.timeInTimeOutBreakDetailList.map((lines, index) => {
      return {
        ...lines,
        // breakInStr: convertDecimalToTimeStr(lines.breakIn),
        breakInStr: lines.breakInStr,
        breakOutStr: lines.breakOutStr,
        breakReason: lines.breakReason
      };
    });
  }).finally(() => {
    loading.value = false;
  });
};

function formatTimeHoursMinutes (timeString) {
  if (!timeString) return "0 mins";

  // Split the time string into hours, minutes, seconds
  const [hoursStr, minsStr] = timeString.split(":");
  const hours = parseInt(hoursStr, 10);
  const mins = parseInt(minsStr, 10);

  let output = "";
  if (hours > 0) output += `${hours} hr${hours > 1 ? "s" : ""}`;
  if (mins > 0) output += (output ? " " : "") + `${mins} min${mins > 1 ? "s" : ""}`;

  return output || "0 mins";
}

const submitTimeOutApproval = (approvalStatus) => {
  approvalStatus === "Approved"
    ? (loading.value = true)
    : (declineLoading.value = true);
  model.value.approvalStatus = approvalStatus;

  try {
    zwConfirm({ data: `${model.value}` }, () => {
      loading.value = true;
      timeInTimeOutService.saveTimeInTimeOut(props.id, model.value).then((resp) => {
        notifySuccess({
          message:
            approvalStatus === "Approved"
              ? "Time Out entry approved successfully"
              : "Time Out entry declined successfully"
        });
        onDialogOK();

        // if (props.approve === "approve") {
        //   router.push({ name: "login" });
        // }
      });
    }, () => {
      loading.value = false;
      declineLoading.value = false;
    });
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    setTimeout(() => {
      loading.value = false;
      declineLoading.value = false;
    }, 1500);
  }
};

// On page rendering
onMounted(() => {
  getTimeInTimeOutDetails();
});

</script>

<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
