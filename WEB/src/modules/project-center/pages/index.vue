<template>
  <q-page padding>
    <q-card class="breadcrumSection project6 flex justify-between items-center">
      <!-- Breadcrumb Section -->
      <q-card-section class="card-header with-tools flex justify-between items-center">
        <q-breadcrumbs class="text-brown text-weight-bold text-h3">
          <template #separator>
            <q-icon size="1.5em" name="o_chevron_right" color="primary" />
          </template>
          <q-breadcrumbs-el label="Project Management" />
          <q-breadcrumbs-el label="Project" clickable :to="fromPage" />
          <q-breadcrumbs-el :label="'Project Center - ' + (model.name)" />
        </q-breadcrumbs>
      </q-card-section>
      <!-- Chat Popup Button -->
      <div>
        <q-btn round color="primary" class="projectchatbox q-mr-sm" @click="toggleProjectChatBox(projectId)"> <i class="fa-brands fa-facebook-messenger" /><q-tooltip>Message</q-tooltip></q-btn>
        <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-mr-lg no-space-between" @click="$router.back()" />
      </div>
    </q-card>
    <div class="items-center">
      <div class="q-pt-md cardTable">
        <div class="q-gutter-y-md">
          <q-card class="PersonMain card-header with-tools headerBasic">
            <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
              <q-tab name="1_tab" label="Project Info." class="q-px-lg q-mr-md" />
              <q-tab name="2_tab" label="Modules" class="q-px-lg hidden" />
              <q-tab name="3_tab" label="Tasks" class="q-px-lg" />
              <q-tab name="4_tab" label="Task Activities" class="q-px-lg hidden" />
              <q-tab name="6_tab" label="Weekly Project Plans" class="q-px-lg" />
              <q-tab name="7_tab" label="Monthly Project Plans" class="q-px-lg" />
              <q-tab name="8_tab" label="Test Plans" class="q-px-lg" />
              <q-tab name="9_tab" label="Test Cases" class="q-px-lg" />
              <q-tab name="11_tab" label="Requirements" class="q-px-lg" />
              <q-tab name="12_tab" label="Issues" class="q-px-lg" />
              <q-tab v-if="role === 'admin'" name="13_tab" label="Infra Services" class="q-px-lg" />
              <q-tab name="14_tab" label="Notes" class="q-px-lg" />
              <q-tab name="15_tab" label="Files" class="q-px-lg" />
            </q-tabs>
            <q-separator />
            <q-tab-panels v-model="tab" animated class="flex justify-center">
              <q-tab-panel name="1_tab" class="items-center q-pa-md q-mx-auto">
                <fieldset>
                  <legend class="text-primary text-h5">Project Info</legend>
                  <div class="row q-col-gutter-x-md q-mb-md q-mt-md">
                    <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12">
                      <div class="form-group">
                        <div class="q-mb-xs">Customer</div>
                        <div class="text-black q-mb-sm"><q-badge v-if="!model.customer.company.name" color="red" square>No Data</q-badge>
                          {{ model.customer.company.name }}</div>
                      </div>
                    </div>
                    <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12">
                      <div class="form-group">
                        <div class="q-mb-xs">Company Contact</div>
                        <div class="text-black q-mb-sm"><q-badge v-if="!model.companyContact.person.fullName" color="red" square>No Data</q-badge>
                          {{ model.companyContact.person.fullName }}</div>
                      </div>
                    </div>
                    <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12">
                      <div class="form-group">
                        <div class="q-mb-xs">Start Date</div>
                        <div class="text-black q-mb-sm"><q-badge v-if="!model.startDate" color="red" square>No Data</q-badge>
                          {{ model.startDate }}</div>
                      </div>

                    </div>
                    <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12">
                      <div class="form-group">
                        <div class="q-mb-xs">Completion Date</div>
                        <div class="text-black q-mb-sm">
                          <q-badge v-if="!model.goLiveDate" color="red" square>No Data</q-badge>
                          {{ model.goLiveDate }}
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md q-mt-md">
                    <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12">
                      <div class="form-group">
                        <div class="q-mb-xs">Project Priority</div>
                        <div class="text-black q-mb-sm">
                          <q-badge v-if="!model.projectPriority.dropDownValue" color="red" square>No Data</q-badge>
                          {{ model.projectPriority.dropDownValue }}
                        </div>
                      </div>
                    </div>
                    <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12">
                      <div class="form-group">
                        <div class="q-mb-xs">Project Type</div>
                        <div class="text-black q-mb-sm">
                          <q-badge v-if="!model.projectType.dropDownValue" color="red" square>No Data</q-badge>
                          {{ model.projectType.dropDownValue }}
                        </div>
                      </div>

                    </div>
                    <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12">
                      <div class="form-group">
                        <div class="q-mb-xs">Status</div>
                        <div class="text-black q-mb-sm">
                          <q-badge v-if="!model.projectStatus.dropDownValue" color="red" square>No Data</q-badge>
                          {{ model.projectStatus.dropDownValue }}
                        </div>
                      </div>
                    </div>
                    <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12">

                      <div class="form-group">
                        <div class="q-mb-xs">Active</div>
                        <div class="text-black q-mb-sm">
                          {{ model.active ? "Yes" : "No" }}
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md q-mt-md">
                    <div class="col-3 ">
                      <div class="form-group">
                        <div class="q-mb-xs">Project Leads</div>
                        <div class="text-black q-mb-sm">
                          <q-badge v-if="!model.projectLeads || model.projectLeads.length === 0" color="red" square>No Data</q-badge>
                          <span v-else>{{ model.projectLeads.join(", ") }}</span>
                        </div>
                      </div>
                    </div>
                    <div class="col-3 ">
                      <div class="form-group">
                        <div class="q-mb-xs">Project Coordinator</div>
                        <div class="text-black q-mb-sm">
                          <q-badge v-if="!model.projectCoordinators || model.projectCoordinators.length === 0" color="red" square>No Data</q-badge>
                          <q-badge v-if="!model.projectCoordinators" color="red" square>No Data</q-badge>
                          <span v-if="model.projectCoordinators">
                            <span v-for="(coordinator, index) in model.projectCoordinators" :key="index">
                              {{ coordinator }}
                              <span v-if="index !== model.projectCoordinators.length - 1"><br></span>
                            </span>
                          </span>
                        </div>
                      </div>
                    </div>
                    <div class="col-3">
                      <div class="form-group">
                        <div class="q-mb-xs q-mr-lg">System Analyst</div>
                        <div class="text-black">
                          <q-badge v-if="!model.projectSystemAnalyst || model.projectSystemAnalyst.length === 0" color="red" square>No Data</q-badge>
                          <span v-else>{{ model.projectSystemAnalyst.join(", ") }}</span>
                        </div>
                      </div>
                    </div>
                    <div class="col-3">
                      <div class="form-group">
                        <div class="q-mb-xs q-mr-lg">System Architect</div>
                        <div class="text-black">
                          <q-badge v-if="!model.projectSystemArchitect || model.projectSystemArchitect.length === 0" color="red" square>No Data</q-badge>
                          <span v-else>{{ model.projectSystemArchitect.join(", ") }}</span>
                        </div>
                      </div>
                    </div>
                    <div class="col-3">
                      <div class="form-group">
                        <div class="q-mb-xs q-mr-lg">Project Billing Admin:</div>
                        <div class="text-black">
                          <q-badge v-if="!model.projectBillingAdmin || model.projectBillingAdmin.length === 0" color="red" square>No Data</q-badge>
                          <span v-else>{{ model.projectBillingAdmin.join(", ") }}</span>
                        </div>
                      </div>
                    </div>
                    <div class="col-8">
                      <div class="form-group">
                        <div class="q-mb-xs q-mr-lg">Team Members:</div>
                        <div class="text-black">
                          <q-badge v-if="!model.projectTeamMembers || model.projectTeamMembers.length === 0" color="red" square>No Data</q-badge>
                          <span v-else>{{ model.projectTeamMembers.join(", ") }}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md q-mt-md">
                    <div class="form-group">
                      <div class="col-12">
                        <div class="q-mb-xs q-mr-lg">Description:</div>
                        <div class="text-black q-mt-sm RichTextEditor">
                          <q-badge v-if="!model.description" color="red" square>No Data</q-badge>
                          <p v-html="model.description" />
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md q-mt-md">
                    <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12 flex items-center">
                      <div class="q-mb-xs q-mr-lg">Created By:</div>
                      <div class="text-black">
                        <q-badge v-if="!model.createdBy.person.fullName" color="red" square>No Data</q-badge>
                        {{ model.createdBy.person.fullName }}
                      </div>
                    </div>
                    <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12 flex items-center">
                      <div class="q-mb-xs q-mr-lg">Created Date:</div>
                      <div class="text-black">
                        <q-badge v-if="!model.createdOnUtc" color="red" square>No Data</q-badge>
                        {{ model.createdOnUtc }}
                      </div>
                    </div>
                    <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12 flex items-center">
                      <div class="q-mb-xs q-mr-lg">Updated By:</div>
                      <div class="text-black">
                        <q-badge v-if="!model.updatedBy.person.fullName" color="red" square>No Data</q-badge>
                        {{ model.updatedBy.person.fullName }}
                      </div>
                    </div>
                    <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12 flex items-center">
                      <div class="q-mb-xs q-mr-lg">Updated Date:</div>
                      <div class="text-black">
                        <q-badge v-if="!model.updatedOnUtc" color="red" square>No Data</q-badge>
                        {{ model.updatedOnUtc }}
                      </div>
                    </div>
                  </div>
                </fieldset>
              </q-tab-panel>

              <q-tab-panel name="2_tab" class="items-center q-pa-md q-mx-auto">
                <projectModules :project-id="projectId" />
              </q-tab-panel>

              <q-tab-panel name="3_tab" class="items-center q-pa-md q-mx-auto">
                <projectTasks :project-id="projectId" />
              </q-tab-panel>

              <q-tab-panel name="4_tab" class="items-center q-pa-md q-mx-auto">
                <projectTaskActivity :project-id="projectId" />
              </q-tab-panel>

              <q-tab-panel name="6_tab" class="items-center q-pa-md q-mx-auto">
                <weeklyProjectPlan :project-id="projectId" />
              </q-tab-panel>
              <q-tab-panel name="7_tab" class="items-center q-pa-md q-mx-auto">
                <monthlyProjectPlan :project-id="projectId" />
              </q-tab-panel>

              <q-tab-panel name="8_tab" class="items-center q-pa-md q-mx-auto">
                <projectTestPlans :project-id="projectId" />
              </q-tab-panel>

              <q-tab-panel name="9_tab" class="items-center q-pa-md q-mx-auto">
                <projectTestCases :project-id="projectId" />
              </q-tab-panel>

              <q-tab-panel name="11_tab" class="items-center q-pa-md q-mx-auto">
                <fieldset>
                  <legend>Project Requirements</legend>
                  <div class="q-mb-sm q-gutter-sm flex justify-end">
                    <q-input v-model="filterRequirement" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search" dense clearable>
                      <template #prepend>
                        <q-icon name="o_search" />
                      </template>
                    </q-input>
                  </div>
                  <q-table
                    ref="reqTableRef" v-model:pagination="paginationRequirements" :class="rowsRequirements.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="filteredRequirement" :columns="columnsRequirements" row-key="id" separator="cell"
                    no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getAllRequirement"
                  >
                    <template #header="propsRequirements">
                      <q-tr :props="propsRequirements" class="bg-primary text-white">
                        <q-th v-for="col in propsRequirements.cols" :key="col.name" :props="propsRequirements">{{ col.label }}</q-th>
                      </q-tr>
                    </template>
                    <template #body="propsRequirements">
                      <q-tr :props="propsRequirements" :class="activeRowIdRequirements == propsRequirements.row.id ? 'highlight' : ''" :set="(preProjectName = null)">
                        <q-td style="width: 3%;" class="hidden">{{ propsRequirements.row.requirementNumber }}</q-td>
                        <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;" class="hidden">{{ propsRequirements.row.requirementGroup.name }}</q-td>
                        <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">{{ propsRequirements.row.title }}</q-td>
                        <q-td style="width: 5%;">
                          <formSingleSelectDropdown
                            v-model="propsRequirements.row.status.id"
                            :options="requirementStatusDropdownSingleSelect.list.value"
                            :filter="requirementStatusDropdownSingleSelect.filter"
                            :bg-color="getStatusColorRequirement(propsRequirements.row.status.dropDownValue)"
                            @update:model-value="onSubmitRequirementStatus(propsRequirements.row.id, propsRequirements.row.status.id, refreshRequirementList, refreshRequirementStatusDropdown)"
                          />
                        </q-td>
                        <q-td style="width: 5%;">
                          <formSingleSelectDropdown
                            v-model="propsRequirements.row.priority.id"
                            :options="requirementPriorityDropdownSingleSelect.list.value"
                            :filter="requirementPriorityDropdownSingleSelect.filter"
                            @update:model-value="onSubmitRequirementPriority(propsRequirements.row.id, propsRequirements.row.priority.id, refreshRequirementList, refreshRequirementPriorityDropdown)"
                          />
                        </q-td>
                        <q-td style="width: 5%;">{{ propsRequirements.row.userType.dropDownValue }}</q-td>
                        <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 8%;">{{ propsRequirements.row.userType.dropDownValue === 'Customer' ? (propsRequirements.row.customer && propsRequirements.row.customer.fullName ? propsRequirements.row.customer.fullName : 'N/A') : (propsRequirements.row.employee && propsRequirements.row.employee.person && propsRequirements.row.employee.person.fullName ? propsRequirements.row.employee.person.fullName : 'N/A') }}</q-td>
                        <q-td style="width: 5%;">{{ propsRequirements.row.createdOnUtc }}</q-td>
                      </q-tr><q-separator />
                    </template>
                  </q-table>
                </fieldset>
              </q-tab-panel>

              <q-tab-panel name="12_tab" class="items-center q-pa-md q-mx-auto">
                <fieldset>
                  <legend>Project Issues</legend>
                  <div class="q-mb-sm q-gutter-sm flex justify-end">
                    <q-input v-model="filterIssue" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search" dense clearable>
                      <template #prepend>
                        <q-icon name="o_search" />
                      </template>
                    </q-input>
                  </div>
                  <q-table
                    ref="issueTableRef" v-model:pagination="paginationIssue" :class="rowsIssue.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="filteredIssue" :columns="columnsIssue" row-key="id" separator="cell"
                    no-data-label="No data available" :filter="filterIssue" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getAllIssue"
                  >
                    <template #header="propsIssue">
                      <q-tr :props="propsIssue" class="bg-primary text-white">
                        <q-th v-for="col in propsIssue.cols" :key="col.name" :props="propsIssue">{{ col.label }}</q-th>
                      </q-tr>
                    </template>
                    <template #body="propsIssue">
                      <q-tr :props="propsIssue" :class="activeRowIdIssue == propsIssue.row.id ? 'highlight' : ''">
                        <q-td style="width: 3%;">{{ propsIssue.row.issueNumber }}</q-td>
                        <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">{{ propsIssue.row.projectModule.name ? propsIssue.row.projectModule.name : "-" }}</q-td>
                        <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">{{ propsIssue.row.name }}</q-td>
                        <q-td style="width: 5%;">{{ propsIssue.row.priority.dropDownValue }}</q-td>
                        <q-td style="width: 5%;">{{ propsIssue.row.type.dropDownValue }}</q-td>
                        <q-td style="width: 5%;">
                          <formSingleSelectDropdown
                            v-model="propsIssue.row.status.id"
                            :options="issueStatusDropdownSingleSelect.list.value"
                            :filter="issueStatusDropdownSingleSelect.filter"
                            :bg-color="getIssueStatusColor(propsIssue.row.status.dropDownValue)"
                            @update:model-value="onSubmitIssueStatus(propsIssue.row.id, propsIssue.row.status.id, refreshIssueList)"
                          />
                        </q-td>
                        <q-td style="width: 8%;">{{ propsIssue.row.employee.person.fullName }}</q-td>
                        <q-td style="width: 8%;">{{ propsIssue.row.reportedBy.person.fullName }}</q-td>
                        <q-td style="width: 5%;">{{ propsIssue.row.createdOnUtc }}</q-td>
                      </q-tr>
                      <q-tr v-if="propsIssue.pageIndex === rowsIssue.length - 1" class="hidden">
                        <q-td colspan="8" class="text-right"><b>Total  Issues:</b></q-td>
                        <q-td class="text-center"><b>{{ rows.length }}</b></q-td>
                      </q-tr>
                      <q-separator />
                    </template>
                  </q-table>
                </fieldset>
              </q-tab-panel>
              <q-tab-panel v-if="role === 'admin'" name="13_tab">
                <AccountServicesTab
                  :rows="servicesRows"
                  :loading="loading"
                />
              </q-tab-panel>
              <q-tab-panel name="14_tab" class="items-center q-pa-md q-mx-auto">
                <fieldset>
                  <legend>Project Notes</legend>
                  <div class="q-px-sm">
                    <q-timeline color="secondary">
                      <q-timeline-entry
                        v-for="(notes, index) in rowsNotes"
                        :key="index"
                        :subtitle="`${notes.createdOnUtc} - ${notes.user.person.fullName}`"
                        :icon="done_all"
                        :color="'primary'"
                      >
                        <div class="fs-14 RichTextEditor">Notes: <span class="text-black" v-html="notes.note" /></div>
                        <div v-if="notes.type !== 'Projects'" class="q-mt-xs fs-13"><i>{{ notes.type }}: <span class="text-black">{{ notes.sub_Module }}</span></i></div>
                      </q-timeline-entry>
                    </q-timeline>
                  </div>
                </fieldset>
              </q-tab-panel>
              <q-tab-panel name="15_tab" class="items-center q-pa-md q-mx-auto">
                <projectFiles :project-id="projectId" />
              </q-tab-panel>
            </q-tab-panels>
          </q-card>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import _ from "lodash";
import { useQuasar } from "quasar";
import { ref, watch, onMounted, computed } from "vue";
import { useAuthStore } from "stores/auth";

import projectService from "modules/project/projects.service";

import projectChatBox from "modules/project/components/_projectChat.vue";
import projectModules from "modules/project-center/components/_projectModulesTab.vue";
import projectTasks from "modules/project-center/components/_projectTasksTab.vue";
import projectTaskActivity from "modules/project-center/components/_projectTaskandActivitiesTab.vue";
import weeklyProjectPlan from "modules/project-center/components/_weeklyPlannerTab.vue";
import monthlyProjectPlan from "modules/project-center/components/_monthlyPlannerTab.vue";
import projectFiles from "modules/project-center/components/_filesTab.vue";
import projectTestPlans from "modules/project-center/components/_testPlansTab.vue";
import projectTestCases from "modules/project-center/components/_testCasesTab.vue";
import AccountServicesTab from "modules/infra-account/components/_accountServicesTab.vue";

import requirementModule from "src/modules/requirement/utils/dropdowns.js";
import issueModule from "src/modules/issue/utils/dropdowns.js";

// SOP Change :- Shared Project Actions
import {
  initRequirementActions,
  onSubmitRequirementStatus,
  onSubmitRequirementPriority
} from "src/modules/requirement/utils/actions.js";

// SOP Change :- Shared Project Actions
import {
  initIssueActions,
  onSubmitIssueStatus
} from "src/modules/issue/utils/actions.js";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
// login user role
const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "infrastructureadmin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

const $q = useQuasar();
const selectedProjectId = history.state?.projectId;
const fromPage = computed(() => "/project" || "/project");
let projectId = selectedProjectId;
const activeRowId = ref(null);
const loading = ref(true);

// Project ChatBox
const toggleProjectChatBox = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: projectChatBox,
    componentProps: { id }
  }).onOk(() => {
    activeRowId.value = null;
  }).onCancel(() => {
    activeRowId.value = null;
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// for project module
const tab = ref("1_tab");
const rows = ref([]);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// for issues
const issueTableRef = ref();
const rowsIssue = ref([]);
const servicesRows = ref([]);
const activeRowIdIssue = ref(null);
const filterIssue = ref("");
const paginationIssue = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columnsIssue = ref([
  { name: "issueNumber", label: "Id", field: "issueNumber", align: "left", sortable: true },
  { name: "projectModule.name", label: "Module", field: "projectModule.name", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "priority.dropDownValue", label: "Priority", field: "priority.dropDownValue", align: "left", sortable: true },
  { name: "type.dropDownValue", label: "Type", field: "type.dropDownValue", align: "left", sortable: true },
  { name: "status.dropDownValue", label: "Status", field: "status.dropDownValue", align: "left", sortable: true },
  { name: "employee.person.fullName", label: "Assign To", field: "employee.person.fullName", align: "left", sortable: false },
  { name: "reportedBy.person.fullName", label: "Reported By", field: "reportedBy.person.fullName", align: "left", sortable: false },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true }
]);

// for Requirements
const reqTableRef = ref();
const rowsRequirements = ref([]);
const activeRowIdRequirements = ref(null);
const filterRequirement = ref("");
const paginationRequirements = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columnsRequirements = ref([
  // { name: "requirementGroup.name", label: "Requirement Group", field: "requirementGroup.name", align: "left", sortable: true },
  { name: "title", label: "Title", field: "title", align: "left", sortable: true },
  { name: "status.dropDownValue", label: "Status", field: "status.dropDownValue", align: "left", sortable: true },
  { name: "priority.dropDownValue", label: "Priority", field: "priority.dropDownValue", align: "left", sortable: true },
  { name: "userType.dropDownValue", label: "User Type", field: "userType.dropDownValue", align: "left", sortable: true },
  { name: "employee.person.fullName", label: "Identified By", field: "employee.person.fullName", align: "left", sortable: false },
  { name: "createdOnUtc", label: "Date", field: "createdOnUtc", align: "left", sortable: true }
]);

// for notes
const rowsNotes = ref([]);
const paginationNotes = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Define model values
const model = ref({
  name: "-",
  projectStatusId: "",
  startDate: "-",
  goLiveDate: "-",
  description: "",
  projectStatus: "-",
  projectPriority: "-",
  projectType: "-",
  createdOnUtc: "",
  updatedOnUtc: "",
  customer: {
    company: {
      name: ""
    }
  },
  companyContact: {
    person: {
      fullName: ""
    }
  },
  createdBy: {
    person: {
      fullName: ""
    }
  },
  updatedBy: {
    person: {
      fullName: ""
    }
  }
});

// get project details
const getProject = () => {
  loading.value = true;
  projectService.getProject(selectedProjectId).then((resp) => {
    model.value = {
      ..._.cloneDeep(resp),
      projectLeads: resp.projectEmployeeMappings ? resp.projectEmployeeMappings.filter(mapping => mapping.employeeRoleDropdown.dropDownValue === "Project Lead")
        .map(mapping => mapping.employee.person.fullName) : [],
      projectCoordinators: resp.projectEmployeeMappings ? resp.projectEmployeeMappings.filter(mapping => mapping.employeeRoleDropdown.dropDownValue === "Project Coordinator")
        .map(mapping => mapping.employee.person.fullName) : [],
      projectTeamMembers: resp.projectEmployeeMappings ? resp.projectEmployeeMappings.filter(mapping => mapping.employeeRoleDropdown.dropDownValue === "Team Members")
        .map(mapping => mapping.employee.person.fullName) : [],
      projectSystemAnalyst: resp.projectEmployeeMappings ? resp.projectEmployeeMappings.filter(mapping => mapping.employeeRoleDropdown.dropDownValue === "System Analyst")
        .map(mapping => mapping.employee.person.fullName) : [],
      projectSystemArchitect: resp.projectEmployeeMappings ? resp.projectEmployeeMappings.filter(mapping => mapping.employeeRoleDropdown.dropDownValue === "System Architect")
        .map(mapping => mapping.employee.person.fullName) : [],
      projectBillingAdmin: resp.projectEmployeeMappings ? resp.projectEmployeeMappings.filter(mapping => mapping.employeeRoleDropdown.dropDownValue === "Project Billing Admin")
        .map(mapping => mapping.employee.person.fullName) : []
    };
    servicesRows.value = (resp.infraProjectServices ?? []).map(row => {
      const service = row.infraAccountServices || {};
      return {
        ...service,
        itemTypeId: service.itemType?.id ?? null,
        ownerShipTypeId: service.ownerShipType?.id ?? null,
        paymentTermId: service.paymentTerm?.id ?? null,
        walletTypeId: service.walletType?.id ?? null,
        startDateStr: service.startDate ?? null,
        instructions: service.instructions ?? null,
        deleted: false
      };
    });
  }).finally(() => {
    loading.value = false;
  });
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const refreshRequirementList = () => {
  getAllRequirement({ pagination: paginationRequirements.value });
};

const refreshRequirementStatusDropdown = () => {
  requirementStatusDropdownSingleSelect.load("Requirement Status");
};

const refreshRequirementPriorityDropdown = () => {
  requirementPriorityDropdownSingleSelect.load("Requirement Priority");
};

const refreshIssueList = () => {
  getAllIssue({ pagination: paginationIssue.value });
};

const filterRows = (data, searchTerm, columns) => {
  if (!searchTerm) return data; // If no filter, return all data
  const lowerCaseTerm = searchTerm.toLowerCase();
  return data.filter(row =>
    columns.some(column => {
      const value = column.field.split(".").reduce((obj, key) => obj?.[key], row); // Handle nested fields
      return String(value || "").toLowerCase().includes(lowerCaseTerm);
    })
  );
};

const issueColumns = columnsIssue.value;
const requirementColumns = columnsRequirements.value;

// Computed properties for each table’s filtered data
const filteredIssue = computed(() => filterRows(rowsIssue.value, filterIssue.value, issueColumns));
const filteredRequirement = computed(() => filterRows(rowsRequirements.value, filterRequirement.value, requirementColumns));

// Added colors for task status dropdown list
function getIssueStatusColor (statusText) {
  if (statusText) {
    switch (statusText) {
    case "Done":
      return "purple-4";
    case "Reopen":
      return "purple-4";
    case "To Do":
      return "deep-orange-4";
    case "Closed":
      return "grey-4";
    case "Converted to Task":
      return "green-4";
    case "In Development":
      return "yellow-4";
    case "In Review":
      return "cyan-4";
    case "In Testing":
      return "deep-orange-4";
    case "New":
      return "blue-4";
    case "New from Test Plan":
      return "blue-4";
    case "On Hold":
      return "brown-4";
    case "In UAT":
      return "blue-grey-4";
    case "UAT Passed":
      return "green-9";
    default:
      return "#ffffff";
    }
  }
}

const getAllIssue = (propsIssue) => {
  const { page, rowsPerPage, sortBy, descending } = propsIssue.pagination;
  loading.value = true;
  const payloadIssue = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    projectId
  };
  projectService.getAllIssuesForDashboard(payloadIssue).then((resp) => {
    rowsIssue.value = resp.data;
    paginationIssue.value.page = page;
    paginationIssue.value.rowsPerPage = rowsPerPage;
    paginationIssue.value.sortBy = sortBy;
    paginationIssue.value.descending = descending;
    paginationIssue.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

// for site info
// const getOrganizations = () => {
//   loading.value = true;
//   projectService.getOrganizationForDashboard(selectedSiteId.value).then((resp) => {
//     siteModel.value = _.cloneDeep(resp);
//   }).finally(() => {
//     loading.value = false;
//   });
// };

const getAllNoteByTypeProjectId = (propsNotes) => {
  const { page, rowsPerPage, sortBy, descending } = propsNotes.pagination;
  loading.value = true;
  const payloadNotes = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    projectId
  };
  projectService.getAllNotesByProjectId(payloadNotes).then((resp) => {
    rowsNotes.value = resp.data;
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

// for requirements
const getAllRequirement = (propsRequirements) => {
  const { page, rowsPerPage, sortBy, descending } = propsRequirements.pagination;
  loading.value = true;
  const payloadRequirementGroup = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    projectId
  };
  projectService.getAllRequirementsForDashboard(payloadRequirementGroup).then((resp) => {
    rowsRequirements.value = resp.data;
    paginationRequirements.value.page = page;
    paginationRequirements.value.rowsPerPage = rowsPerPage;
    paginationRequirements.value.sortBy = sortBy;
    paginationRequirements.value.descending = descending;
    paginationRequirements.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------
const {
  requirementStatusDropdownSingleSelect,
  requirementPriorityDropdownSingleSelect
} = requirementModule();

const {
  issueStatusDropdownSingleSelect
} = issueModule();

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initRequirementActions(activeRowId);
initIssueActions(activeRowId);

// Added colors for dropdown list
function getStatusColorRequirement (statusText) {
  if (statusText) {
    switch (statusText) {
    case "Close":
      return "grey-4";
    case "In Progress":
      return "yellow-4";
    case "On Hold":
      return "brown-4";
    case "Open":
      return "purple-4";
    case "New":
      return "blue-4";
    default:
      return "#ffffff";
    }
  }
}

// On page rendering
onMounted(() => {
  getProject();
  issueStatusDropdownSingleSelect.load("Issue Status");
  requirementStatusDropdownSingleSelect.load("Requirement Status");
  requirementPriorityDropdownSingleSelect.load("Requirement Priority");
  const propsIssue = { pagination: paginationIssue.value };
  const propsNotes = { pagination: paginationNotes.value };
  getAllIssue(propsIssue);
  // getOrganizations();
  refreshRequirementList();
  getAllNoteByTypeProjectId(propsNotes);
});

watch(() => history.state, (query) => {
  if (projectId) {
    projectId = history.state?.projectId;
  }
}, { immediate: true });

watch(tab, (newTab) => {
  if (newTab === "1_tab") {
    getProject();
  } else if (newTab === "11_tab") {
    refreshRequirementList();
  } else if (newTab === "12_tab") {
    getAllIssue({ pagination: paginationIssue.value });
  } else if (newTab === "14_tab") {
    getAllNoteByTypeProjectId({ pagination: paginationNotes.value });
  }
});

</script>
<style>
.projectchatbox i {
    font-size: 20px;
    width: 32px;
    height: 32px;
    text-align: center;
    padding: 0px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.projectchatbox {
    min-width: auto;
    min-height: auto;
}
</style>
