<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent @hide="onDialogHide">
    <q-card style="width: 400px; max-width: 90vw;">
      <q-card-section class="card-header row items-center q-pa-sm">
        <div class="text-h2">{{ detailId ? "Edit" : "Add" }} Break Details</div>
        <q-btn v-close-popup icon="o_close" class="q-ml-auto close" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="submitBreak">
        <q-card-section class="card-body scroll">
          <div class="col-12 q-mb-sm">
            <div class="label text-black"> Break Time <span class="required">*</span></div>
            <div>
              <q-select
                v-model="breakDetailModel.breakTimeId"
                clearable use-input
                outlined
                stack-label
                hide-bottom-space
                option-value="id"
                option-label="text"
                emit-value
                map-options
                :dense="true"
                :options="breakTimeList"
                :error="breakV$.breakTimeId.$error"
                :error-message="breakV$.breakTimeId.$errors[0]?.$message"
                @blur="breakV$.breakTimeId.$touch"
                @update:model-value="onBreakTimeChange"
                @filter="getAllBreakTimesForFilter"
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
          <div class="col-12 q-mb-sm">
            <label class="label text-black">Break Explanation<span class="required">*</span></label>
            <q-select
              v-model="breakDetailModel.message"
              outlined
              dense
              hide-bottom-space
              use-input
              fill-input
              hide-selected
              input-debounce="0"
              :options="breakExplanationList"
              option-value="value"
              option-label="text"
              new-value-mode="add-unique"
              :error="breakV$.message.$error"
              :error-message="breakV$.message.$errors[0]?.$message"
              @filter="getAllBreakExplanationListForFilter"
              @blur="breakV$.message.$touch"
              @update:model-value="val => {
                breakDetailModel.message = val?.value || val
              }"
              @new-value="val => breakDetailModel.message = val"
              @input-value="val => breakDetailModel.message = val"
            />
          </div>
          <div class="col-12 q-mb-sm">
            <label class="label q-mb-xs text-black">Permission By (Permission taken from / informed to the following person, and acknowledgement received)<span class="required">*</span></label>
            <q-select
              v-model="breakDetailModel.approverById"
              clearable
              use-input
              stack-label
              outlined
              hide-bottom-space
              :dense="true"
              :options="employeeApproverList"
              option-value="value"
              option-label="text"
              emit-value
              map-options
              :disable="isEditMode"
              :error="breakV$.approverById.$error"
              :error-message="breakV$.approverById.$errors[0]?.$message"
              @blur="breakV$.approverById.$touch"
              @filter="getDefaultLeaveApproverNameForFilter"
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
          <div class="col-12 q-mb-sm">
            <label class="label q-mb-xs text-black">
              Informed to my project stakeholders (I have informed well in advance to everyone working with me & may need my attendance)
            </label>
            <div class="row items-center q-gutter-x-lg">
              <q-checkbox
                :model-value="breakDetailModel.notifyToStakeholders === true"
                label="Yes"
                :disable="isEditMode"
                @update:model-value="breakDetailModel.notifyToStakeholders = true"
              />
              <q-checkbox
                :model-value="breakDetailModel.notifyToStakeholders === false"
                label="No"
                :disable="isEditMode"
                @update:model-value="breakDetailModel.notifyToStakeholders = false"
              />
            </div>
          </div>
        </q-card-section>
        <q-separator />
        <q-card-actions align="right">
          <q-btn v-close-popup flat dense bordered color="grey-8" label="Close" @click="onDialogCancel" />
          <!-- <q-btn label="Save" type="submit" flat bordered color="primary" :loading="processing" /> -->
          <q-btn
            label="Save"
            flat
            bordered
            color="primary"
            type="button"
            :loading="processing"
            @click="openTooltip"
          >
            <q-tooltip
              v-model="showTooltip"
              persistent
              no-parent-event
              anchor="top middle"
              self="bottom middle"
            >
              <div class="q-pa-md column q-gutter-md" style="max-width: 380px">
                <div class="text-body2" style="font-size: 14px;">
                  <b>When to Use</b><br>
                  Short breaks taken during working hours (lunch, tea, personal, walk).<br><br>

                  <b>How to Use</b>
                  <ol>
                    <li>Click <b>Break</b> on the dashboard.</li>
                    <li>Enter the following details:
                      <ul>
                        <li><b>Break Time:</b> Select the applicable break duration.</li>
                        <li><b>Break Explanation:</b> Choose the reason for the break.</li>
                        <li><b>Permission By:</b> Select the authority who approved your break.</li>
                        <li><b>Stakeholder Intimation:</b> Select Yes/No to confirm whether project stakeholders have been informed.</li>
                      </ul>
                    </li>
                    <li>Click <b>Save</b> to submit the break entry.</li>
                  </ol>

                  <b>Do Not Use</b>
                  <ul>
                    <li>To extend break duration beyond approval.</li>
                    <li>To adjust login/logout times.</li>
                  </ul>
                </div>
                <div class="row items-center q-gutter-sm">
                  <q-btn
                    outline
                    dense
                    label="Close"
                    @click.stop="closeTooltip"
                  />
                  <q-space />
                  <q-btn
                    unelevated
                    color="primary"
                    label="Continue"
                    @click.stop="onContinue"
                  />
                </div>
              </div>
            </q-tooltip>
          </q-btn>
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>
<script setup>
import { ref, onMounted, watch } from "vue";
import useVuelidate from "@vuelidate/core";
import useFilters from "composables/useFilters";
import { required, helpers } from "@vuelidate/validators";
import { notifySuccess, notifyError } from "assets/utils";
import { useDialogPluginComponent } from "quasar";

import movementRegisterService from "src/modules/movementRegister/movementRegister.service";
import commonService from "services/common.service";
import employeesService from "src/modules/employee/employee.service";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, detailId: { type: String, default: "" } });

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const { toDate } = useFilters();
const loading = ref(true);
const processing = ref(false);
const currentDate = new Date();
const isEditMode = !!props.detailId;

const breakDetailModel = ref({
  timeInMinutes: null,
  breakTimeId: "",
  message: "",
  approverById: "",
  notifyToStakeholders: true
});

const breakRules = {
  breakTimeId: { required: helpers.withMessage("Break Time is required", required) },
  message: { required: helpers.withMessage("Break Explanation is required", required) },
  approverById: { required: helpers.withMessage("Permission By is required", required) }
};

const breakV$ = useVuelidate(breakRules, breakDetailModel);

const breakTimeList = ref([]);
const breakTimeListFilter = ref([]);
function getAllBreakTimesListForDropdown (buttonType, typeName) {
  commonService.getDropdownByButton(buttonType, typeName).then((resp) => {
    const responseData = resp.map((item) => ({ id: item.id, text: item.dropDownText, value: Number(item.dropDownValue) }));
    // breakTimeList.value = responseData;
    breakTimeList.value = isEditMode
      ? responseData
      : responseData.filter(item => item.value !== 0);
    breakTimeListFilter.value = responseData;
  });
}

function getAllBreakTimesForFilter (val, update, abort) {
  update(() => {
    const needle = val?.toLowerCase() || "";
    breakTimeList.value = breakTimeListFilter.value.filter(
      v => (isEditMode || Number(v.value) !== 0) && v.text.toLowerCase().includes(needle)
    );
  });
}

const breakExplanationList = ref([]);
const breakExplanationListFilter = ref([]);
function getAllBreakExplanationListForDropdown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.dropdownValue }));
    breakExplanationList.value = responseData;
    breakExplanationListFilter.value = responseData;
  });
}
function getAllBreakExplanationListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      breakExplanationList.value = breakExplanationListFilter.value;
    } else {
      breakExplanationList.value = breakExplanationListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const employeeApproverList = ref([]);
const employeeApproverFilter = ref([]);
function getDefaultLeaveApproverNameForDropdown () {
  employeesService.getDefaultLeaveApproverNameForDropdown().then((resp) => {
    const responseData = resp.employees.map(item => ({
      text: item.person.fullName,
      value: item.id
    }));
    employeeApproverList.value = responseData;
    employeeApproverFilter.value = responseData;
    if (!isEditMode) {
      breakDetailModel.value.approverById = resp.leaveApproverId;
    }
  });
}
function getDefaultLeaveApproverNameForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeApproverList.value = employeeApproverFilter.value;
    } else {
      employeeApproverList.value = employeeApproverFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// get Movement Register details
const getMovementRegisterDetails = () => {
  loading.value = true;
  movementRegisterService
    .getMovementRegisterDetails(props.id, props.detailId)
    .then((resp) => {
      const movementRegisterDetail = resp.movementRegisterDetails?.[0];
      if (!movementRegisterDetail) return;
      breakDetailModel.value = {
        breakTimeId: movementRegisterDetail.breakTimeId,
        timeInMinutes: movementRegisterDetail.timeInMinutes,
        message: movementRegisterDetail.message,
        approverById: movementRegisterDetail.approverById,
        notifyToStakeholders: movementRegisterDetail.notifyToStakeholders
      };
    })
    .finally(() => {
      loading.value = false;
    });
};

// ===========================================================
// Tooltip
// ===========================================================
const showTooltip = ref(false);

function openTooltip () {
  processing.value = true;
  showTooltip.value = true;
}

function closeTooltip () {
  showTooltip.value = false;
  processing.value = false;
}

async function onContinue () {
  showTooltip.value = false;
  processing.value = true;
  await submitBreak();
  processing.value = false;
}
// =============================================================

async function submitBreak () {
  processing.value = true;
  try {
    const isValid = await breakV$.value.$validate();
    if (!isValid) {
      processing.value = false;
      return;
    }
    const breakOutPayload = {
      breakTimeId: breakDetailModel.value.breakTimeId,
      timeInMinutes: breakDetailModel.value.timeInMinutes,
      message: breakDetailModel.value.message,
      approverById: breakDetailModel.value.approverById,
      notifyToStakeholders: breakDetailModel.value.notifyToStakeholders,
      Type: "Break",
      date: toDate(currentDate)
    };
    await movementRegisterService.saveMovementRegister(props.detailId, breakOutPayload);
    notifySuccess({ message: "Break is saved successfully." });
    onDialogOK();
  } catch (error) {
    console.error("Break Out submit error:", error);
    notifyError({ message: "An error occurred while submitting break out." });
  } finally {
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

const onBreakTimeChange = (id) => {
  const selected = breakTimeList.value.find(item => item.id === id);
  breakDetailModel.value.timeInMinutes = selected
    ? selected.value
    : 0;
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getMovementRegisterDetails();
  }
}, { immediate: true });

onMounted(() => {
  getAllBreakExplanationListForDropdown("Break Explanation");
  getAllBreakTimesListForDropdown("Break", "Break Time");
  getDefaultLeaveApproverNameForDropdown();
});
</script>
<style>
/* Enable interaction inside tooltip */
.q-tooltip {
  pointer-events: auto !important;
}

/* Ensure tooltip is above dialog content */
.q-tooltip__content {
  pointer-events: auto !important;
  z-index: 6000;
}
</style>
