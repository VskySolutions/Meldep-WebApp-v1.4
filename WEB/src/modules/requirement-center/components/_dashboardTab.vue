<template>
  <div class="summary-row q-mb-md">
    <q-card flat bordered class="summary-card card-border">
      <q-card-section>
        <div class="text-h4 text-weight-bold text-primary">
          {{ taskSummary.open }}
        </div>
        <div class="text-caption text-uppercase">
          Open Tasks
        </div>
      </q-card-section>
    </q-card>

    <q-card flat bordered class="summary-card card-border">
      <q-card-section>
        <div class="text-h4 text-weight-bold text-primary">
          {{ timesheetSummary.totalHours }}
        </div>
        <div class="text-caption text-uppercase">
          Hours Logged
        </div>
      </q-card-section>
    </q-card>

    <!-- <q-card flat bordered class="summary-card hidden">
      <q-card-section>
        <div class="text-h4 text-weight-bold text-primary">
          {{ monthlyPlanSummary.utilized }}%
        </div>
        <div class="text-caption text-uppercase">
          July Plan Utilised
        </div>
      </q-card-section>
    </q-card> -->

    <q-card flat bordered class="summary-card card-border">
      <q-card-section>
        <div class="text-h4 text-weight-bold text-primary">
          {{ testCaseSummary.passed }} / {{ testCaseSummary.total }}
        </div>
        <div class="text-caption text-uppercase">
          Test Cases Passed
        </div>
      </q-card-section>
    </q-card>

    <q-card flat bordered class="summary-card card-border">
      <q-card-section>
        <div class="text-h4 text-weight-bold text-primary">
          {{ issueSummary.open }}
        </div>
        <div class="text-caption text-uppercase">
          Open Issues
        </div>
      </q-card-section>
    </q-card>
  </div>
  <div class="row q-col-gutter-md q-mt-md">
    <div class="col-12 col-md-6">
      <TaskCard
        :requirement-id="props.requirementId"
        @summary="updateTaskSummary"
      />
    </div>

    <div class="col-12 col-md-6">
      <TimesheetCard
        :requirement-id="props.requirementId"
        @summary="updateTimesheetSummary"
      />
    </div>

    <!-- <div class="col-12 col-md-6 hidden">
      <MonthlyPlanCard
        :requirement-id="props.requirementId"
        @summary="updateMonthlyPlanSummary"
      />
    </div>

    <div class="col-12 col-md-6 hidden">
      <WeeklyPlanCard
        :requirement-id="props.requirementId"
        @summary="updateWeeklyPlanSummary"
      />
    </div> -->

    <div class="col-12 col-md-6">
      <TestCaseCard
        :requirement-id="props.requirementId"
        @summary="updateTestCaseSummary"
      />
    </div>

    <div class="col-12 col-md-6">
      <IssueCard
        :requirement-id="props.requirementId"
        @summary="updateIssueSummary"
      />
    </div>
  </div>
</template>

<script setup>
import { reactive } from 'vue';

import TaskCard from "./cards/_taskCard.vue";
import TimesheetCard from "./cards/_timesheetCard.vue";
// import MonthlyPlanCard from "./cards/_monthlyPlanCard.vue";
// import WeeklyPlanCard from "./cards/_weeklyPlanCard.vue";
import TestCaseCard from "./cards/_testCaseCard.vue";
import IssueCard from "./cards/_issueCard.vue";

const emit = defineEmits(["loaded"]);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

const taskSummary = reactive({
  open: 0
});

const timesheetSummary = reactive({
  totalHours: 0
});

// const monthlyPlanSummary = reactive({
//   utilized: 0
// })

// const weeklyPlanSummary = reactive({
//   completed: 0
// })

const testCaseSummary = reactive({
  total: 0,
  passed: 0
});

const issueSummary = reactive({
  open: 0
});

const emitSummary = () => {
  emit("loaded", {
    taskCount: taskSummary.total,
    timesheetCount: timesheetSummary.totalHours,
    testCaseCount: testCaseSummary.total,
    issueCount: issueSummary.total
  })
}

const updateTaskSummary = (summary) => {
  taskSummary.total = summary.total;
  taskSummary.open = summary.open;
  emitSummary();
}

const updateTimesheetSummary = (summary) => {
  timesheetSummary.total = summary.total;
  timesheetSummary.totalHours = summary.totalHours;
  emitSummary();
}

// const updateMonthlyPlanSummary = (summary) => {
//   monthlyPlanSummary.utilized = summary.utilized;
//   emitSummary();
// }

// const updateWeeklyPlanSummary = (summary) => {
//   weeklyPlanSummary.completed = summary.completed;
//   emitSummary();
// }

const updateTestCaseSummary = (summary) => {
  testCaseSummary.total = summary.total;
  testCaseSummary.passed = summary.passed;
  emitSummary();
}

const updateIssueSummary = (summary) => {
  issueSummary.total = summary.total;
  issueSummary.open = summary.open;
  emitSummary();
}
</script>

<style>
.summary-row {
  display: flex;
  gap: 16px;
  width: 100%;
}

.summary-card {
  flex: 1 1 20%;
  min-width: 0;
  border-radius: 12px;
}
.card-border {
  border: 0.5px solid #1b75ab;
}
</style>
