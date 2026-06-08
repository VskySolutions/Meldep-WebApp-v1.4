<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card
      class="q-dialog-plugin PersonMain card-header with-tools headerBasic"
      style="width: 50vw; max-width: 50vw;"
    >
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white q-mr-lg">{{ id ? "Edit" : "Add" }} Weekly Saturday's Off</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                <div class="col-12  q-mb-md">
                  <div class="label q-mb-xs text-black">Title</div>
                  <q-input v-model="model.title" outlined stack-label hide-bottom-space :dense="true" maxlength="500" readonly autofocus />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                <div class="col-lg-3 col-md-4 col-sm-6 col-12 q-mb-md">
                  <label class="label q-mb-xs text-black">Target Month<span class="required">*</span></label>
                  <div class="input-group">
                    <q-input v-model="model.date" outlined stack-label hide-bottom-space dense>
                      <template v-if="!props.id" #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy
                            ref="qDateProxy" v-model="isPopupVisible" transition-show="scale"
                            transition-hide="scale"
                          >
                            <q-date
                              v-model="model.date" default-view="Years" ref="date3ref"
                              emit-immediately minimal mask="MMMM-YYYY" class="myDate"
                              @update:model-value="onUpdateMv2"
                            />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                      <template v-else #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date
                              v-model="model.date" mask="MM/DD/YYYY"
                              :options="optionsDate" @update:model-value="() => $refs.qDateProxy.hide()"
                            />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
                <!-- Dynamically Generated Radio Buttons -->
                <div v-if="!props.id" class="col-lg-9 col-md-8 col-sm-6 q-mb-md">
                  <label class="label q-mb-xs text-black q-ml-sm">Saturday's<span class="required">*</span></label>
                  <div class="row q-gutter-x-md">
                    <q-checkbox v-for="(saturday, index) in saturdays" :key="index" v-model="selectedSaturday" :val="saturday.value" :label="saturday.label" color="primary" />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn
            color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps
            @click="onDialogCancel"
          />
          <q-btn
            color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing"
            no-caps
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent } from "quasar";
import { ref, watch, onMounted, nextTick } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import yearlyLeaveService from "modules/leave-yearly-schedule/leaveYearlySchedule.service";
import _ from "lodash";

const $emit = defineEmits(["hide", "ok"]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();
const isPopupVisible = ref(false);
const date3ref = ref(null);
// Selected Saturday
const selectedSaturday = ref([]); // For multiple selection (checkbox)
const loading = ref(true);
// const currentView = ref("Years");
// Saturdays options
const saturdays = ref([]);
const lastSelectedMonth = ref(null);
// const targetMonth = ref("");
const props = defineProps({
  id: { type: String, default: "" },
  leaveRuleId: { type: String, default: "" }
});

// Define model values
const model = ref({
  title: "Weekly Saturday's Off",
  leaveRuleId: props.leaveRuleId,
  date: "",
  description: "",
  selectedDates: []
});

const getEvents = () => {
  if (!props.id) return; // If no ID, do nothing (Add mode)
  loading.value = true;
  yearlyLeaveService.getEvents(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.id = resp.id;
    model.value.leaveRuleId = resp.leaveRuleId;
    model.value.title = resp.title;
    model.value.description = resp.description;
    model.value.date = resp.date;
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getEvents();
  }
}, { immediate: true });

watch(() => props.leaveRuleId, (newValue) => {
  model.value.leaveRuleId = newValue;
}, { immediate: true });

// Function to format the month and year as "Month-Year"
function tosetMonthYear (date) {
  const parsedDate = new Date(date); // Ensure it's a Date object
  const month = parsedDate.toLocaleString("en-US", { month: "long" });
  const year = parsedDate.getFullYear();
  return `${month}-${year}`; // Example: "April-2025"
}

// Function to get all Saturdays of the selected month
function getSaturdays (year, month) {
  const saturdaysList = [];
  const firstDayOfMonth = new Date(year, month - 1, 1);
  const daysInMonth = new Date(year, month, 0).getDate(); // Total days in month

  for (let day = 1; day <= daysInMonth; day++) {
    firstDayOfMonth.setDate(day); // Move to the current day

    if (firstDayOfMonth.getDay() === 6) { // Check if it's Saturday
      saturdaysList.push({
        label: `${day} ${tosetMonthYear(firstDayOfMonth)}`,
        value: day
      });
    }
  }
  return saturdaysList;
}

const onUpdateMv2 = (val) => {
  model.value.date = val; // Store selected value
  localStorage.setItem("selectedMonthYear", val); // Save to LocalStorage
  const [monthName, year] = val.split("-");
  const month = new Date(`${monthName} 1, ${year}`).getMonth() + 1;

  // Update Saturdays for the selected month-year
  saturdays.value = getSaturdays(year, month);

  // Ensure the Month Picker is displayed after selecting a year
  nextTick(() => {
    if (lastSelectedMonth.value === month) {
      // If the same month is selected twice, close popup
      isPopupVisible.value = false;
      lastSelectedMonth.value = null;
    } else {
      // Store the selected month
      lastSelectedMonth.value = month;
    }
  });
};

onMounted(() => {
  const savedMonth = localStorage.getItem("selectedMonthYear");
  if (savedMonth) {
    model.value.date = tosetMonthYear(new Date(savedMonth)); // Convert MM/DD/YYYY to "Month-Year"
  }
});

// Initialize Saturdays for the stored month-year
const initSaturdays = () => {
  const savedMonthYear = localStorage.getItem("selectedMonthYear");
  let dateObj;

  if (savedMonthYear) {
    const [monthName, year] = savedMonthYear.split("-");
    const month = new Date(`${monthName} 1, ${year}`).getMonth() + 1;
    dateObj = new Date(year, month - 1, 1);
  } else {
    dateObj = new Date(); // Default to current month
  }
  // Get Saturdays for the selected month-year
  saturdays.value = getSaturdays(dateObj.getFullYear(), dateObj.getMonth() + 1);
};

// Call the function on component mount
onMounted(() => {
  initSaturdays();
});

// Watch for changes in selected month
watch(() => model.value.date, (newDate) => {
  if (newDate) {
    onUpdateMv2(newDate);
  }
});

function optionsDate (date) {
  const selectedDate = new Date(date);
  const dayOfWeek = selectedDate.getDay(); // 6 = Saturday

  return dayOfWeek === 6;
}

async function onSubmit () {
  // if (!await v$.value.$validate()) {
  //   return;
  // }
  // if (!selectedSaturday.value.length) {
  //   notifyError({ message: "Please select at least one Saturday." });
  //   return;
  // }
  // Get selected Saturdays in mm/dd/yyyy format
  if (!model.value.date) {
    notifyError({ message: "Please select a Target Month." });
    return;
  }
  model.value.selectedDates = selectedSaturday.value.map(day => {
    const [monthName, year] = model.value.date.split("-");
    const month = new Date(`${monthName} 1, ${year}`).getMonth() + 1;
    return `${month.toString().padStart(2, "0")}/${day.toString().padStart(2, "0")}/${year}`;
  });
  model.value.selectedDates = props.id ? null : model.value.selectedDates;
  model.value.leaveRuleId = props.leaveRuleId;
  model.value.date = props.id ? model.value.date : null;
  yearlyLeaveService.saveLeaveSaturdayEvents(props.id, model.value).then(resp => {
    notifySuccess({ message: "Saturday's Off saved successfully." });
    $emit("ok");
    $emit("hide");
    setTimeout(() => {
      window.location.reload();
    }, 1000);
  });
}
</script>
