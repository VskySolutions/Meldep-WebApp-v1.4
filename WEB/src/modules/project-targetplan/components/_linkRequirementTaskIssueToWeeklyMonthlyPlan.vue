<template>
  <q-dialog ref="linkContainer" v-model="linkRequirementTaskIssueModal" class="project-message-dialog customDialog dialog-scrollable-content" persistent position="right">
    <q-card style="width: 500px; max-width: 500px;">
      <q-card-section class="bg-primary text-white flex items-center q-pa-sm">
        <div class="q-space flex  fs-16">Link To Weekly/Monthly Plan?</div>
        <q-btn dense flat icon="o_close" @click="toggleLinkRequirementTaskIssueModal" />
      </q-card-section>
      <q-card-section class="row q-pa-md">
        <div class="row full-width q-mb-md">
          <div class="col-12 q-mb-sm">
            <label class="Cutomlabel" style="margin: 0px !important;">Project Name<span class="required">*</span></label>
            <q-input v-model="model.projectName" outlined hide-bottom-space :dense="true" :readonly="true" />
          </div>
          <div class="col-6">
            <label class="Cutomlabel" style="margin: 0px !important;">Plan Type<span class="required">*</span></label>
            <div class="q-gutter-sm">
              <q-radio
                v-for="plan in ProjectPlanTypeDropdownSingleSelect.list.value"
                :key="plan.id"
                v-model="model.planTypeId"
                :val="plan.value"
                :label="plan.text"
                @update:model-value="(val) => onProjectPlanChanged(plan)"
              />
            </div>
          </div>
          <div v-if="!showWeeklyMonthlyCalendar">
            <formDate
              v-model="model.date"
              label="Week End"
              :dateOptions="isSunday"
              :navigationMinYearMonth="minMonth"
              :minimal="true"
              :firstDayOfWeek="1"
              :error="dateError"
              :error-message="dateErrorMessage"
            />
          </div>
          <div v-if="showWeeklyMonthlyCalendar">
            <formMonthYearPicker
              v-model="model.date"
              label="Month"
              :default-view="currentView"
              :navigation-min-year-month="minMonth"
              :error="dateError"
              :error-message="dateErrorMessage"
              @update:model-value="onUpdateMonth"
            />
          </div>
        </div>
        <div class="row full-width q-mb-md">
          <div class="col-12">Selected {{ model.type }}<span class="required">*</span></div>
          <div v-for="(name, index) in model.names" :key="name" class="col-12 q-px-sm q-mb-xs">
            {{ (index+1) + ") " + name }}
          </div>
        </div>
        <div class="row full-width flex justify-center q-px-sm">
          <q-btn size="md" color="grey-4" outline label="Close" type="button" class="text-grey-9 q-mr-xs" style="width: 120px;" no-caps @click="toggleLinkRequirementTaskIssueModal" />
          <q-btn size="md" color="primary" outline label="Link Now" type="button" :loading="processing" :disable="disableActionButton" style="width: 120px;" no-caps @click="OnSave()" />
        </div>
      </q-card-section>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import { useQuasar, date } from "quasar";
import { notifySuccess } from "assets/utils";

import projectService from "modules/project/projects.service";

// SOP Change :- Shared Inputs
import formDate from "src/components/form-inputs/_formDate.vue";
import formMonthYearPicker from "src/components/form-inputs/_formMonthYearPicker.vue";

// SOP Change :- Shared Dropdowns
import projectTargetPlanModule from "src/modules/project-targetplan/utils/dropdowns.js";

const $q = useQuasar();

const today = new Date();
const minMonth = date.formatDate(today, "YYYY/MM");

const processing = ref(false);
const showWeeklyMonthlyCalendar = ref(false);
const disableActionButton = ref(true);

const props = defineProps({
  projectId: { type: String, default: "" },
  projectName: { type: String, default: "" },
  type: { type: String, default: "" },
  ids: { type: Array, default: () => [] },
  names: { type: Array, default: () => [] },
  hasTaskLink: { type: Number, default: 0 }
});

const linkRequirementTaskIssueModal = ref(false);
const toggleLinkRequirementTaskIssueModal = () => { linkRequirementTaskIssueModal.value = !linkRequirementTaskIssueModal.value; };

const model = ref({
  projectId: props?.projectId ? props.projectId : null,
  projectName: props?.projectName ? props.projectName : null,
  planTypeId: null,
  date: null,
  type: props.type,
  ids: props?.ids ? props.ids : null,
  names: props?.names ? props.names : null
});

const onProjectPlanChanged = (selected) => {
  showWeeklyMonthlyCalendar.value = selected.text === "Monthly";
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Weekly Date
// --------------------------------------------------------------------------------------------------------------------------------------------------

// Only allow Sundays and today/future
const isSunday = (dateStr) => {
  const day = new Date(dateStr);
  const todayStart = new Date();
  todayStart.setHours(0, 0, 0, 0);

  return (
    day.getDay() === 0 // && day >= todayStart
  );
};

// Validation state
const dateError = ref(false);
const dateErrorMessage = ref("");

// Regex to match MM/DD/YYYY format
const validDateRegex = /^(0[1-9]|1[0-2])\/(0[1-9]|[12]\d|3[01])\/\d{4}$/;

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Monthly Date
// --------------------------------------------------------------------------------------------------------------------------------------------------

const calendarRef = ref(null);
const qDateProxy = ref(null);
const currentView = ref("Years");
const formattedMonthDate = ref("");

const onUpdateMonth = (val) => {
  if (currentView.value === "Years") {
    currentView.value = "Months";
    calendarRef.value?.setView("Months");
  } else {
    formattedMonthDate.value = val;
    // Convert 'June-2025' or 'MMMM-YYYY' to Date object (1st of month)
    const parsedDate = new Date(val);
    const formatted = date.formatDate(parsedDate, "MM/01/YYYY"); // Day is fixed to 01

    model.value.date = formatted; // Save Date object
    currentView.value = "Years";
    qDateProxy.value?.hide(); // Close popup
  }
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  ProjectPlanTypeDropdownSingleSelect
} = projectTargetPlanModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Save: Link Requirement/Task/Issue
// --------------------------------------------------------------------------------------------------------------------------------------------------

const OnSave = () => {
  const title = `Link ${model.value.type} To ${showWeeklyMonthlyCalendar.value ? "Monthly" : "Weekly"} Plan`;
  // show a warning message advising the user to create a task first before linking
  const message = props.hasTaskLink > 0
    ? `It is not advisable to link ${model.value.type.toLowerCase()} directly here.Please create a task first and then link it. Do you still want to proceed?`
    : "Are you sure you want to link this to plan?";

  $q.dialog({
    title,
    message,
    ok: { label: "Yes", color: "primary" },
    cancel: { label: "No", color: "negative" }
  }).onOk(() => {
    $q.loading.show();
    projectService.linkRequirementTaskIssueToWeeklyPlanDate(model.value).then((resp) => {
      toggleLinkRequirementTaskIssueModal();
      notifySuccess({
        message: `${model.value.type} Linked To ${showWeeklyMonthlyCalendar.value ? "Monthly" : "Weekly"} Plan`
      });
    }).finally(() => {
      $q.loading.hide();
    });
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Search: When user types in search
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => model.value,
  (newModelValue) => {
    // Validate date format
    if (!newModelValue?.date || !validDateRegex.test(newModelValue.date)) {
      dateError.value = true;
      dateErrorMessage.value = "Please enter date in MM/DD/YYYY format.";
    } else if (!showWeeklyMonthlyCalendar.value && !isSunday(newModelValue.date)) {
      dateError.value = true;
      dateErrorMessage.value = "Date must be a Sunday.";
    } else {
      dateError.value = false;
      dateErrorMessage.value = "";
    }

    // Disable action if any required field is missing or date has error
    const hasMissingData =
      !newModelValue?.projectId ||
      !newModelValue?.date ||
      !newModelValue?.planTypeId ||
      !newModelValue?.type ||
      !newModelValue?.ids ||
      newModelValue?.ids.length === 0 ||
      dateError.value;

    disableActionButton.value = hasMissingData;
  },
  { deep: true }
);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Page Load
// --------------------------------------------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  await ProjectPlanTypeDropdownSingleSelect.load("Project Weekly Target Planning");
  // set default in type
  const weeklyValue = await ProjectPlanTypeDropdownSingleSelect.getValueByLabel("weekly");
  if (weeklyValue) {
    model.value.planTypeId = weeklyValue;
  }
});

</script>
