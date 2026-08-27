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
      <div class="col-12 col-md-12">

        <q-tab-panels
          v-model="leftTab"
          animated
          keep-alive
        >
          <q-tab-panel name="qAndA" class="q-pa-none">
            <ProjectQATabularView
              :requirement-id="requirementId"
              :project-id="projectId"
              :active-tab="leftTab"
              @select="selectedQA = $event"
              @count="projectQACount = $event"
            />
          </q-tab-panel>
          
          <q-tab-panel name="actionItems" class="q-pa-none">
            <ProjectActionItemsTabularView
              :requirement-id="requirementId"
              :project-id="projectId"
              :active-tab="leftTab"
              @select="selectedActionItems = $event"
              @count="projectActionItemsCount = $event"
            />
          </q-tab-panel>

          <q-tab-panel name="tasks" class="q-pa-none">
            <TaskTabularView
              :requirement-id="requirementId"
              :project-id="projectId"
              :active-tab="leftTab"
              @select="selectedTask = $event"
              @count="taskCount = $event"
            />
          </q-tab-panel>

         <q-tab-panel name="timesheet" class="q-pa-none">
            <TimesheetTabularView
              :requirement-id="requirementId"
              :project-id="projectId"
              :active-tab="leftTab"
              @select="onSelectGroup"
              @search-change="timesheetSearchModel = $event"
            />
          </q-tab-panel>

          <q-tab-panel name="testCases" class="q-pa-none">
            <TestCaseTabularView
              :requirement-id="requirementId"
              :project-id="projectId"
              :active-tab="leftTab"
              @select="selectedTestCase = $event"
              @count="testCaseCount = $event"
            />
          </q-tab-panel>

          <q-tab-panel name="issues" class="q-pa-none">
            <IssueTabularView
              :requirement-id="requirementId"
              :active-tab="leftTab"
              @select="selectedIssue = $event"
              @count="issueCount = $event"
            />
          </q-tab-panel>
        </q-tab-panels>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue';

import IssueList from './list/_issueList.vue';
import ProjectQATabularView from './tabularView/_projectQ&ATabularView.vue';
import ProjectActionItemsTabularView from './tabularView/_projectActionItemsTabularView.vue';
import TaskTabularView from './tabularView/_taskTabularView.vue';
import TimesheetTabularView from './tabularView/_timesheetTabularView.vue';
import TestCaseTabularView from './tabularView/_testCaseTabularView.vue';
import IssueTabularView from './tabularView/_issueTabularView.vue';

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
