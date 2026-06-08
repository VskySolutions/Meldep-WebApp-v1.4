<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent @hide="onDialogHide">
    <q-card style="width: 400px; max-width: 90vw;">
      <q-card-section class="card-header row items-center q-pa-sm">
        <div class="text-h2">{{ detailId ? "Edit" : "Add" }} Time Adjustment Details</div>
        <q-btn v-close-popup icon="o_close" class="q-ml-auto close" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="submitTimeAdjustment">
        <q-card-section class="card-body scroll">
          <div class="col-12 q-mb-sm">
            <div class="q-mb-xs text-black"> Time Adjustment Time <span class="required">*</span></div>
            <div>
              <q-select
                v-model="timeAdjustmentModel.breakTimeId"
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
                :error="timeAdjustmentModelV$.breakTimeId.$error"
                :error-message="timeAdjustmentModelV$.breakTimeId.$errors[0]?.$message"
                @blur="timeAdjustmentModelV$.breakTimeId.$touch"
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
            <label class="label text-black">Time Adjustment Explanation<span class="required">*</span></label>
            <q-input
              v-model="timeAdjustmentModel.message"
              outlined
              stack-label
              hide-bottom-space
              autogrow
              :error="timeAdjustmentModelV$.message.$error"
              :error-message="timeAdjustmentModelV$.message.$errors[0]?.$message"
              @blur="timeAdjustmentModelV$.message.$touch"
            />
          </div>
          <div class="col-12 q-mb-sm">
            <label class="label q-mb-xs text-black">Time Adjustment Date<span class="required">*</span></label>
            <q-input
              v-model="timeAdjustmentModel.date"
              outlined
              stack-label
              hide-bottom-space
              mask="##/##/####"
              :disable="isEditMode"
              :error="timeAdjustmentModelV$.date.$error"
              :error-message="timeAdjustmentModelV$.date.$errors[0]?.$message"
              @blur="timeAdjustmentModelV$.date.$touch"
            >
              <template #append>
                <q-icon name="o_calendar_month" class="cursor-pointer">
                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                    <q-date
                      v-model="timeAdjustmentModel.date"
                      mask="MM/DD/YYYY"
                      :options="disableFutureDates"
                      @update:model-value="() => $refs.qDateProxy.hide()"
                    />
                  </q-popup-proxy>
                </q-icon>
              </template>
            </q-input>
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
              <div class="q-pa-md column q-gutter-md" style="max-width: 450px">
                <div class="text-body2">
                  <div class="text-red-10 text-weight-bold">
                    <b>Important: DO NOT use Time Adjustment unless the process below is followed.</b>
                  </div><br>

                  <b>When to Use</b><br>
                  Only for genuine attendance corrections such as:
                  <ul>
                    <li>Early login</li>
                    <li>If an employee <b>arrives late</b>, they should enter the <b>actual late time</b> using the Time Adjustment option.</li>
                    <li>If an employee <b>changes their shift time,</b> they may select a <b>5-minute adjustment</b> and clearly mention the <b>reason or shift change details</b> in the message section.</li>
                  </ul>

                  <b>How to Use</b>
                  <ol>
                    <li>Click <b>Time Adjustment</b> on the dashboard.</li>
                    <li>Fill in the required details:
                      <ol type="a">
                        <li><b>Time Adjustment Time:</b> Select the applicable adjustment time.</li>
                        <li><b>Time Adjustment Explanation:</b> Clearly mention the reason (e.g., late arrival, early login, shift change).</li>
                        <li><b>Time Adjustment Date:</b> Auto-selected by the system; update if required.</li>
                      </ol>
                    </li>
                    <li>Click <b>Save</b> to submit for approval.</li>
                  </ol>

                  <b>Do Not Use</b>
                  <ul>
                    <li>To bypass attendance discipline</li>
                    <li>As a substitute for Leave, Movement, or WFH</li>
                  </ul>

                  <b>Important Notes</b>
                  <ul>
                    <li>For shift changes, only <b>minor adjustments (up to 5 minutes)</b> are permitted and must be justified in the explanation.</li>
                    <li>All Time Adjustments are subject to <b>Project Lead approval.</b></li>
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
import { isDate } from "validators/zw_validators.js";
import { useDialogPluginComponent } from "quasar";

import movementRegisterService from "src/modules/movementRegister/movementRegister.service";
import commonService from "services/common.service";

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

const timeAdjustmentModel = ref({
  timeInMinutes: "",
  breakTimeId: "",
  message: "",
  date: toDate(currentDate)
});

const timeAdjustmentModelRules = {
  breakTimeId: { required: helpers.withMessage("Time Adjustment Time is required", required) },
  message: { required: helpers.withMessage("Time Adjustment Explanation is required", required) },
  date: {
    required: helpers.withMessage("Time Adjustment Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate),
    afterCurrentDate: helpers.withMessage("Time Adjustment Date cannot be greater than today.", (value) => {
      return new Date(value) <= currentDate;
    })
  }
};
const timeAdjustmentModelV$ = useVuelidate(timeAdjustmentModelRules, timeAdjustmentModel);

const breakTimeList = ref([]);
const breakTimeListFilter = ref([]);
function getAllBreakTimesListForDropdown (buttonType, typeName) {
  commonService.getDropdownByButton(buttonType, typeName).then((resp) => {
    const responseData = resp.map((item) => ({ id: item.id, text: item.dropDownText, value: Number(item.dropDownValue) }));
    breakTimeList.value = responseData;
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

// get Movement Register details
const getMovementRegisterDetails = () => {
  loading.value = true;
  movementRegisterService
    .getMovementRegisterDetails(props.id, props.detailId)
    .then((resp) => {
      const movementRegisterDetail = resp.movementRegisterDetails?.[0];
      if (!movementRegisterDetail) return;
      timeAdjustmentModel.value = {
        breakTimeId: movementRegisterDetail.breakTimeId,
        timeInMinutes: movementRegisterDetail.timeInMinutes,
        message: movementRegisterDetail.message,
        date: resp.date
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
  await submitTimeAdjustment();
  processing.value = false;
}
// =============================================================

async function submitTimeAdjustment () {
  processing.value = true;
  try {
    const isValid = await timeAdjustmentModelV$.value.$validate();
    if (!isValid) {
      processing.value = false;
      return;
    }
    const timeAdjustmentPayload = {
      breakTimeId: timeAdjustmentModel.value.breakTimeId,
      timeInMinutes: timeAdjustmentModel.value.timeInMinutes,
      type: "Time Adjustment",
      message: timeAdjustmentModel.value.message,
      date: timeAdjustmentModel.value.date
    };
    await movementRegisterService.saveMovementRegister(props.detailId, timeAdjustmentPayload);
    notifySuccess({ message: "Time Adjustment is saved successfully." });
    onDialogOK();
  } catch (err) {
    console.error(err);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

function disableFutureDates (date) {
  const today = new Date();
  const current = new Date(date);
  return current <= today;
}

const onBreakTimeChange = (id) => {
  const selected = breakTimeList.value.find(item => item.id === id);
  timeAdjustmentModel.value.timeInMinutes = selected
    ? selected.value
    : 0;
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getMovementRegisterDetails();
  }
}, { immediate: true });

onMounted(() => {
  getAllBreakTimesListForDropdown("Time Adjustment", "Break Time");
});
</script>
