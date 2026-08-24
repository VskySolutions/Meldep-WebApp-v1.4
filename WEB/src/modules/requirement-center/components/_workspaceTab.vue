<template>
  <div>
    <q-card flat bordered class="q-mb-md">
      <q-tabs
        v-model="leftTab"
        dense
        active-color="primary"
        indicator-color="primary"
        align="left"
        inline-label
      >
        <q-tab name="qAndA">
          <div class="row items-center no-wrap">
            Project Questions Answers 
            <q-badge
              rounded
              color="grey-3"
              text-color="black"
              class="q-ml-xs"
            >
              {{ props.projectQACount }}
            </q-badge>
          </div>
        </q-tab>
        
        <q-tab name="actionItems">
          <div class="row items-center no-wrap">
            Action Items
            <q-badge
              rounded
              color="grey-3"
              text-color="black"
              class="q-ml-xs"
            >
              {{ props.projectActionItemsCount }}
            </q-badge>
          </div>
        </q-tab>

        <q-tab name="tasks">
          <div class="row items-center no-wrap">
            Tasks
            <q-badge
              rounded
              color="grey-3"
              text-color="black"
              class="q-ml-xs"
            >
              {{ props.taskCount }}
            </q-badge>
          </div>
        </q-tab>

        <q-tab name="timesheet">
          <div class="row items-center no-wrap">
            Timesheet
            <q-badge rounded color="grey-3" text-color="black" class="q-ml-xs">
              {{ props.timesheetCount }}
            </q-badge>
          </div>
        </q-tab>

        <!-- <q-tab name="monthlyPlan">
          <div class="row items-center no-wrap">
            Monthly Plan
            <q-badge rounded color="grey-3" text-color="black" class="q-ml-xs">
              {{ monthlyPlanCount }}
            </q-badge>
          </div>
        </q-tab>

        <q-tab name="weeklyPlan">
          <div class="row items-center no-wrap">
            Weekly Plan
            <q-badge rounded color="grey-3" text-color="black" class="q-ml-xs">
              {{ weeklyPlanCount }}
            </q-badge>
          </div>
        </q-tab> -->

        <q-tab name="testCases">
          <div class="row items-center no-wrap">
            Test Cases
            <q-badge rounded color="grey-3" text-color="black" class="q-ml-xs">
              {{ props.testCaseCount }}
            </q-badge>
          </div>
        </q-tab>

        <q-tab name="issues">
          <div class="row items-center no-wrap">
            Issues
            <q-badge rounded color="grey-3" text-color="black" class="q-ml-xs">
              {{ props.issueCount }}
            </q-badge>
          </div>
        </q-tab>
      </q-tabs>
    </q-card>

    <!-- Content -->
    <div class="row q-col-gutter-md">

      <!-- Left List -->
      <div class="col-12 col-md-5">

        <q-tab-panels
          v-model="leftTab"
          animated
          keep-alive
        >
          <q-tab-panel name="qAndA" class="q-pa-none">
            <ProjectQAList
              :requirement-id="requirementId"
              :project-id="projectId"
              :active-tab="leftTab"
              @select="selectedQA = $event"
              @count="projectQACount = $event"
            />
          </q-tab-panel>
          
          <q-tab-panel name="actionItems" class="q-pa-none">
            <ProjectActionItemsList
              :requirement-id="requirementId"
              :project-id="projectId"
              :active-tab="leftTab"
              @select="selectedActionItems = $event"
              @count="projectActionItemsCount = $event"
            />
          </q-tab-panel>

          <q-tab-panel name="tasks" class="q-pa-none">
            <TaskList
              :requirement-id="requirementId"
              :project-id="projectId"
              :active-tab="leftTab"
              @select="selectedTask = $event"
              @count="taskCount = $event"
            />
          </q-tab-panel>

         <q-tab-panel name="timesheet" class="q-pa-none">
            <TimesheetList
              :requirement-id="requirementId"
              :project-id="projectId"
              :active-tab="leftTab"
              @select="onSelectGroup"
              @search-change="timesheetSearchModel = $event"
            />
          </q-tab-panel>

          <!-- <q-tab-panel name="monthlyPlan" class="q-pa-none">
            <MonthlyPlanList />
          </q-tab-panel>

          <q-tab-panel name="weeklyPlan" class="q-pa-none">
            <WeeklyPlanList />
          </q-tab-panel> -->

          <q-tab-panel name="testCases" class="q-pa-none">
            <TestCaseList
              :requirement-id="requirementId"
              :project-id="projectId"
              :active-tab="leftTab"
              @select="selectedTestCase = $event"
              @count="testCaseCount = $event"
            />
          </q-tab-panel>

          <q-tab-panel name="issues" class="q-pa-none">
            <IssueList
              :requirement-id="requirementId"
              :active-tab="leftTab"
              @select="selectedIssue = $event"
              @count="issueCount = $event"
            />
          </q-tab-panel>
        </q-tab-panels>

      </div>

      <!-- Right Details -->
      <div class="col-12 col-md-7">

        <q-tab-panels
          v-model="leftTab"
          animated
          keep-alive
        >
          <q-tab-panel name="qAndA" class="q-pa-none">
            <ProjectQADetails
              v-if="selectedQA"
              :id="selectedQA.id"
            />
          </q-tab-panel>
          
          <q-tab-panel name="actionItems" class="q-pa-none">
            <ProjectActionItems
              v-if="selectedActionItems"
              :id="selectedActionItems.id"
            />
          </q-tab-panel>

          <q-tab-panel name="tasks" class="q-pa-none">
            <TaskDetails
              v-if="selectedTask"
              :id="selectedTask.id"
            />
          </q-tab-panel>

          <q-tab-panel name="timesheet" class="q-pa-none">
            <TimesheetDetails
              v-if="selectedGroup"
              :requirement-id="requirementId"
              :search-model="timesheetSearchModel"
              :group="selectedGroup"
            />
          </q-tab-panel>

          <!-- <q-tab-panel name="monthlyPlan" class="q-pa-none">
            <MonthlyPlanDetails />
          </q-tab-panel>

          <q-tab-panel name="weeklyPlan" class="q-pa-none">
            <WeeklyPlanDetails />
          </q-tab-panel> -->

          <q-tab-panel name="testCases" class="q-pa-none">
            <TestCaseDetails
              v-if="selectedTestCase"
              :id="selectedTestCase.id"
              :test-plan-id="selectedTestCase.testPlanId"
            />
          </q-tab-panel>

          <q-tab-panel name="issues" class="q-pa-none">
            <IssueDetails
              v-if="selectedIssue"
              :id="selectedIssue.id"
            />
          </q-tab-panel>
        </q-tab-panels>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';

import TaskList from './list/_taskList.vue';
import TimesheetList from './list/_timesheetList.vue';
// import MonthlyPlanList from './list/_monthlyPlanList.vue';
// import WeeklyPlanList from './list/_weeklyPlanList.vue';
import TestCaseList from './list/_testCaseList.vue';
import IssueList from './list/_issueList.vue';
import ProjectQAList from './list/_projectQAList.vue';
import ProjectActionItemsList from "./list/_projectActionItemsList.vue";

import TaskDetails from './details/_taskDetails.vue';
import TimesheetDetails from './details/_timesheetDetails.vue';
// import MonthlyPlanDetails from './details/_monthlyPlanDetails.vue';
// import WeeklyPlanDetails from './details/_weeklyPlanDetails.vue';
import TestCaseDetails from './details/_testCaseDetails.vue';
import IssueDetails from './details/_issueDetails.vue';
import ProjectQADetails from './details/_projectQ&ADetails.vue';
import ProjectActionItems from './details/_projectActionItemsDetails.vue';

const props = defineProps({
  requirementId: String,
  projectId: String,
  taskCount: {
    type: Number,
    default: 0
  },
  testCaseCount: {
    type: Number,
    default: 0
  },
  issueCount: {
    type: Number,
    default: 0
  },
  timesheetCount: {
    type: Number,
    default: 0
  },
  projectQACount: {
    type: Number,
    default: 0
  },
  projectActionItemsCount: {
    type: Number,
    default: 0
  }
});

const leftTab = ref('qAndA');
const selectedTask = ref(null);
const selectedTestCase = ref(null);
const selectedIssue = ref(null);
const selectedGroup = ref(null);
const selectedQA = ref(null);
const selectedActionItems = ref(null);
const timesheetSearchModel = ref({});

const onSelectGroup = group => {
  selectedGroup.value = group;
};
</script>
<style>
.full-height {
  height: calc(100vh - 220px);
}

:deep(.q-tabs) {
  background: #fff;
}

:deep(.q-tab) {
  margin-right: 8px;
  border: 1px solid #dcdfe6;
  border-radius: 8px;
  min-height: 38px;
  padding: 0 14px;
}

:deep(.q-tab--active) {
  background: #1976d2;
  color: white;
}

:deep(.q-tab__indicator) {
  display: none;
}
</style>
