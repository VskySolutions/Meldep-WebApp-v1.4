<template>
  <q-page class="q-pa-md">
    <!-- Breadcrumb -->
     <div class="row q-col-gutter-x-md">
      <div class="col">
        <q-card class="breadcrumSection project6 flex justify-between items-center">
          <q-card-section class="card-header with-tools flex justify-between items-center">
            <div class="flex items-center">
              <q-breadcrumbs class="text-brown text-weight-bold text-h3">
                <template #separator>
                  <q-icon size="1.5em" name="o_chevron_right" color="primary" />
                </template>
                <q-breadcrumbs-el label="SDLC" />
                <q-breadcrumbs-el label="Requirements" clickable to="/requirement" />
                <q-breadcrumbs-el label="Requirement Center" />
                <q-breadcrumbs-el :label="requirement.project?.name" />
                <q-breadcrumbs-el :label="requirement.title" />
                
              </q-breadcrumbs>
            </div>
          </q-card-section>
          <div>
            <q-btn
              icon="o_chevron_left"
              outline
              label="Back"
              no-caps
              class="text-primary btnRounded q-mr-lg"
              @click="$router.back()"
            />
          </div>
        </q-card>
      </div>
     </div>

    <!-- Requirement Header -->
    <q-card flat bordered class="q-mt-md q-pa-lg requirement-header hidden">
      <div class="row no-wrap items-start">
        <q-avatar
          rounded
          size="48px"
          color="blue-1"
          text-color="primary"
          icon="o_view_kanban"
        />
        <div class="q-ml-md col">
          <div class="text-caption text-primary text-weight-medium">
            REQ-{{ requirement.requirementNumber }} • REQUIREMENT CENTER
          </div>
          <div class="text-h5 text-weight-bold q-mt-xs">
            {{ requirement.title }}
          </div>
          <div class="row q-col-gutter-lg q-mt-xs">
            <div>
              <div class="text-caption text-grey-6 text-uppercase text-weight-medium">PROJECT</div>
              <div class="text-body2 text-grey-10 text-weight-medium">
                {{ requirement.project?.name || '-' }}
              </div>
            </div>
            <div>
              <div class="text-caption text-grey-6 text-uppercase text-weight-medium">TYPE</div>
              <div class="text-body2 text-grey-10 text-weight-medium">
                {{ requirement.requirementType?.dropDownValue || '-' }}
              </div>
            </div>
            <div>
              <div class="text-caption text-grey-6 text-uppercase text-weight-medium">AREA</div>
              <div class="text-body2 text-grey-10 text-weight-medium">
                {{ requirement.area?.dropDownValue || '-' }}
              </div>
            </div>
            <div>
              <div class="text-caption text-grey-6 text-uppercase text-weight-medium">WORKSPACE</div>
              <div class="text-body2 text-grey-10 text-weight-medium">
                {{ requirement.workspace?.dropDownValue || '-' }}
              </div>
            </div>
            <div>
              <div class="text-caption text-grey-6 text-uppercase text-weight-medium">STATUS</div>
              <q-chip
                dense
                :style="{
                  backgroundColor: requirement.status.bgColor,
                  color: requirement.status.color
                }"
              >
                {{ requirement.status.dropDownValue || '-' }}
              </q-chip>
            </div>
            <div>
              <div class="text-caption text-grey-6 text-uppercase text-weight-medium">PRIORITY</div>
              <q-chip
                dense
                :style="{
                  backgroundColor: requirement.priority.bgColor,
                  color: requirement.priority.color
                }"
              >
                {{ requirement.priority.dropDownValue || '-' }}
              </q-chip>
            </div>
            <div>
              <div class="text-caption text-grey-6 text-uppercase text-weight-medium">REQUIREMENT ENTERED BY</div>
              <div class="text-body2 text-grey-10 text-weight-medium">
                {{ requirement.requirementEntered.person.fullName || '-' }}
              </div>
            </div>
            <div>
              <div class="text-caption text-grey-6 text-uppercase text-weight-medium">
                ACTUAL END DATE
              </div>
              <div class="text-body2 text-grey-10 text-weight-medium">
                {{ requirement.actualEndDate || '-' }}
              </div>
            </div>
            <div>
              <div class="text-caption text-grey-6 text-uppercase text-weight-medium">
                CREATED BY
              </div>
              <div class="text-body2 text-grey-10 text-weight-medium">
                {{ requirement.createdBy.person.firstName + " "+ requirement.createdBy.person.lastName || '-' }}
              </div>
            </div>
            <div>
              <div class="text-caption text-grey-6 text-uppercase text-weight-medium">
                CREATED DATE
              </div>
              <div class="text-body2 text-grey-10 text-weight-medium">
                {{ requirement.createdOnUtc || '-' }}
              </div>
            </div>
            <div>
              <div class="text-caption text-grey-6 text-uppercase text-weight-medium">
                UPDATED BY
              </div>
              <div class="text-body2 text-grey-10 text-weight-medium">
                {{ requirement.updatedBy.person.firstName + " " + requirement.updatedBy.person.lastName || '-' }}
              </div>
            </div>
            <div>
              <div class="text-caption text-grey-6 text-uppercase text-weight-medium">
                UPDATED DATE
              </div>
              <div class="text-body2 text-grey-10 text-weight-medium">
                {{ requirement.updatedOnUtc || '-' }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </q-card>

    <!-- Tabs -->
    <q-tabs
      v-model="tab"
      dense
      active-color="primary"
      indicator-color="primary"
      class="q-mt-md"
    >
      <q-tab
        name="dashboard"
        label="Dashboard"
        icon="o_dashboard"
      />
      <q-tab
        name="workspace"
        label="Workspace"
        icon="o_workspaces"
      />
      <q-tab
        name="workbench"
        label="workbench"
        icon="o_view_list"
      />
    </q-tabs>

    <q-separator />

    <q-tab-panels
      v-model="tab"
      animated
    >
      <q-tab-panel name="dashboard">
        <DashboardTab
          :requirement-id="requirementId"
          @loaded="onDashboardLoaded"
        />
      </q-tab-panel>

      <q-tab-panel name="workspace">
        <WorkspaceTab
          :requirement-id="requirementId"
          :project-id="requirement.projectId"
          :task-count="counts.taskCount"
          :test-case-count="counts.testCaseCount"
          :issue-count="counts.issueCount"
          :timesheet-count="counts.timesheetCount"
          :projectQA-count="counts.projectQACount"
          :projectActionItems-count="counts.projectActionItemsCount"
        />
      </q-tab-panel>

      <q-tab-panel name="workbench">
        <WorkbenchTabTab
          :requirement-id="requirementId"
          :project-id="requirement.projectId"
          :task-count="counts.taskCount"
          :test-case-count="counts.testCaseCount"
          :issue-count="counts.issueCount"
          :timesheet-count="counts.timesheetCount"
          :projectQA-count="counts.projectQACount"
          :projectActionItems-count="counts.projectActionItemsCount"
        />
      </q-tab-panel>
    </q-tab-panels>
  </q-page>
</template>

<script setup>
import { ref, watch } from 'vue';
import _ from "lodash";

import DashboardTab from "modules/requirement-center/components/_dashboardTab.vue";
import WorkspaceTab from "modules/requirement-center/components/_workspaceTab.vue";
import WorkbenchTabTab from "modules/requirement-center/components/_workbenchTab.vue";
import requirementService from "modules/requirement/requirement.service";

// Props values i.e. come from query string
const requirementId = history.state?.requirementId;
const tab = ref('workbench');
const requirement = ref({
  status:{
    dropDownValue: ""
  },
  priority: {
    dropDownValue: ""
  },
  requirementEntered: {
    person: {
      fullName: ""
    }
  },
  createdBy: {
    person: {
      firstName: "",
      lastName: ""
    }
  },
  updatedBy: {
    person: {
      firstName: "",
      lastName: ""
    }
  }
});

const counts = ref({
  taskCount: 0,
  testCaseCount: 0,
  issueCount: 0,
  timesheetCount: 0,
  projectQACount: 0,
  projectActionItemsCount: 0
});

const onDashboardLoaded = (data) => {
  counts.value = data;
};

const getRequirement = async (requirementId) => {
  requirementService.getRequirementDetails(requirementId).then((resp) => {
    requirement.value = _.cloneDeep(resp);
  }).finally(() => {
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => requirementId,
    async (requirementId) => {
      await getRequirement(requirementId);
    },
  {
    immediate: true
  }
);
</script>

<style>
.requirement-header {
  border-radius: 12px;
}

.requirement-header .text-caption {
  font-size: 11px;
  letter-spacing: 1px;
  font-weight: 600;
}
</style>
