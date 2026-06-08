<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <!-- <q-breadcrumbs-el label="Dashboard" icon="o_home" clickable to="/dashboard" /> -->
              <q-breadcrumbs-el label="SDLC" />
              <q-breadcrumbs-el label="Issues" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center q-mr-xs">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    class="search-bar"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Issue Id</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.issueNumber" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.projectIds"
                        label="Project Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.projectModuleIds"
                        label="Project Module"
                        :options="projectModulesByProjectIdForDropdown.list.value"
                        :filter="projectModulesByProjectIdForDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Issue Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.name"
                            class="q-mx-sm w-100 h-auto"
                            fill-input
                            :dense="true"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.priorityIds"
                        label="Issue Priority"
                        :options="issuePriorityForDropdown.list.value"
                        :filter="issuePriorityForDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="search.statusIds"
                        label="Status"
                        :options="issueStatusForDropdown.list.value"
                        :filter="issueStatusForDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="search.issueTypeIds"
                        label="Issue Type"
                        :options="issueTypeForDropdown.list.value"
                        :filter="issueTypeForDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="search.employeeIds"
                        label="Assign To"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onAdvanceSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onAdvanceClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
              <q-menu v-model="showManageDropdownOptions" anchor="bottom right" self="top right" no-parent-event style="width: 320px;">
                <q-card class="q-pa-sm">
                  <div class="text-subtitle2 q-mt-sm q-mb-sm">Manage Dropdown Options</div>
                  <q-separator />
                  <q-list style="min-width: 200px">
                    <q-item
                      v-for="opt in dropdownTypes"
                      :key="opt.id"
                      clickable
                      :active="selectedField === opt.id"
                      active-class="bg-primary text-white"
                      @click="$router.push({ path: '/manage-dropdowns', state: { id: opt.id, groupName: opt.groupName, moduleName: opt.moduleName } })"
                    >
                      <q-item-section>{{ opt.type }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-card>
              </q-menu>
              <q-menu v-model="showMultiSelectOptions" anchor="bottom right" self="top right" persistent no-parent-event style="width: 320px;">
                <q-card class="q-pa-sm">
                  <div class="text-subtitle2 q-mt-sm q-mb-sm">Click one of the options below</div><q-separator />
                  <q-list style="min-width: 200px">
                    <q-item
                      v-for="opt in selectedFieldOptions"
                      :key="opt.value"
                      clickable
                      :active="selectedField === opt.value"
                      active-class="bg-primary text-white"
                      @click="selectedField = opt.value"
                    >
                      <q-item-section avatar>
                        <q-icon
                          :name="opt.icon"
                          size="xs"
                          class="cursor-pointer"
                          :color="selectedField === opt.value ? 'primary' : 'grey-7'"
                        />
                      </q-item-section>
                      <q-item-section>{{ opt.label }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-card>
              </q-menu>
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Create Issue" no-caps class="text-primary btnRounded" @click="onIssueAdd(refreshIssueList)" />
                <q-btn icon="o_checklist" outline no-caps class="text-primary btnRounded q-ml-sm" :disabled="multiSelectIssueIds.length === 0" @click.stop="showMultiSelectOptions = !showMultiSelectOptions">
                  <q-badge v-if="multiSelectIssueIds?.length > 0" :label="multiSelectIssueIds.length" class="primary" floating />
                  <q-tooltip>Multi Actions</q-tooltip>
                </q-btn>
                <q-btn v-if="role === 'admin'" icon="o_playlist_add" outline no-caps class="text-primary btnRounded q-ml-sm" @click="showManageDropdownOptions = !showManageDropdownOptions">
                  <q-tooltip>Manage Dropdowns</q-tooltip>
                </q-btn>
                <q-btn v-if="selectedProjectId" icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded no-space-between q-ml-sm" @click="$router.back()" />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-table
        ref="tableRef"
        v-model:pagination="pagination"
        :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
        :loading="loading"
        :rows="rows"
        :columns="columns"
        row-key="id"
        separator="cell"
        no-data-label="No data available"
        binary-state-sort
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        @request="getAllIssue"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th auto-width class="text-center" />
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="highlightedId == props.row.id ? 'highlight' : ''">
            <q-td auto-width class="text-center hidden">
              <q-icon :name="isExpanded(props.row.id) ? '-' : '+'" class="cursor-pointer custom-plus-minus-icon" @click="toggleExpand(props.row.id)">
                <q-tooltip>{{ isExpanded(props.row.id) ? 'Collapse' : 'Expand' }}</q-tooltip>
              </q-icon>
            </q-td>
            <q-td class="text-center" style="width: 3%;"><q-checkbox v-model="props.row.checkboxStatus" @update:model-value="onSelectCheckbox(props.row.project.id, props.row.project.name, props.row.id, props.row.name, $event)" /></q-td>
            <q-td style="width: 3%;" class="text-right">#{{ props.row.issueNumber }}</q-td>
            <q-td style="width: 20%; white-space: normal;" class="hoverable-cell">
              <div class="row no-wrap items-center justify-between">
                <span style="flex: 1; word-break: break-word; white-space: normal;" @click="onProjectView(props.row.project.id)">{{ props.row.project.name }}</span>
                <div class="row items-center q-gutter-sm q-ml-sm" style="flex-shrink: 0;">
                  <q-icon
                    name="o_radio_button_checked" size="xs"
                    class="cursor-pointer"
                    @click="setActiveRowIdInLocalStorage(props.row.id);
                            $router.push({ path: '/project-center', state: { projectId: props.row.project.id } })"
                  >
                    <q-tooltip>Project Center</q-tooltip>
                  </q-icon>
                  <q-icon
                    v-if="props.row.isEditable"
                    name="o_developer_board" size="xs"
                    class="cursor-pointer"
                    @click="setActiveRowIdInLocalStorage(props.row.id);
                            $router.push({ path: '/project-planning/workboard', state: {projectId: props.row.project.id } })"
                  >
                    <q-tooltip>Work Board</q-tooltip>
                  </q-icon>
                  <!-- New info icon for issue status -->
                  <!-- <q-icon name="o_info" size="xs" class="cursor-pointer text-primary" v-if="statusSummary.find(x => x.projectId === props.row.project.id)"> -->
                  <q-icon name="o_info" size="xs" class="cursor-pointer text-primary">
                    <q-tooltip anchor="bottom middle" self="top middle" class="bg-grey-8 text-white shadow-2">
                      <div class="text-caption">
                        <table class="table boarded statusTable">
                          <thead>
                            <tr>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.noStatus > 0">No Status</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.new > 0">New</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.newFromTestPlan > 0">Test Plan</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.toDo > 0">To Do</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.inDevelopment > 0">In Development</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.inReview > 0">In Review</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.inTesting > 0">In Testing</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.reopen > 0">Reopen</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.inUAT > 0">In UAT</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.uatPassed > 0">UAT Passed</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.convertedToTask > 0">Converted To Task</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.onHold > 0">On Hold</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.closed > 0">Closed</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.done > 0">Done</th>
                              <th v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.total > 0">Total</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.noStatus > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.noStatus }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.new > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.new }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.newFromTestPlan > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.newFromTestPlan }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.toDo > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.toDo }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.inDevelopment > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.inDevelopment }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.inReview > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.inReview }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.inTesting > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.inTesting }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.reopen > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.reopen }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.inUAT > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.inUAT }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.uatPassed > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.uatPassed }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.convertedToTask > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.convertedToTask }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.onHold > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.onHold }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.closed > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.closed }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.done > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.done }}</td>
                              <td v-if="statusSummary.find(x => x.projectId === props.row.project.id)?.total > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.project.id)?.total }}</td>
                            </tr>
                          </tbody>
                        </table>
                      </div>
                    </q-tooltip>
                  </q-icon>
                </div>
              </div>
            </q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;">{{ props.row.projectModule.name }}</q-td>
            <q-td style="width: 3%;">
              <span v-if="props.row.projectTaskRelatedMappings?.length">
                <template v-for="(item, index) in props.row.projectTaskRelatedMappings" :key="index">
                  <span class="hoverable-cell" style="cursor: pointer;" @click="onProjectTaskView(item.taskId)">#{{ item.projectTask?.projectTaskNumber }}
                    <span v-if="item.projectTask?.status">
                      ({{ item.projectTask.status.dropDownValue }})
                    </span>
                  </span>
                  <span v-if="index < props.row.projectTaskRelatedMappings.length - 1">, </span>
                  <br>
                </template>
              </span>
            </q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;">{{ props.row.name }}</q-td>
            <q-td style="width: 5%;">{{ props.row.priority.dropDownValue }}</q-td>
            <q-td style="width: 5%;">{{ props.row.type.dropDownValue }}</q-td>
            <q-td
              class="common-q-td"
              :class="{ 'hoverable-cell' : props.row.isEditable }"
              @click="activeEdit = { rowId: props.row.id, field: 'status' }"
              style="width: 5%;"
            >
              <quickEditSingleSelect
                field="status"
                :row-id="props.row.id"
                :value="props.row.status.id"
                :display-value="props.row.status.dropDownValue"
                :editable="props.row.isEditable"
                :options="issueStatusDropdownSingleSelect.list.value"
                :active-edit="activeEdit"
                :show-history="false"
                @cancel="activeEdit = { rowId: null, field: null }"
                @submit="({ rowId, value }) => onSubmitIssueStatus(rowId, value, refreshIssueList)"
              />
            </q-td>
            <q-td style="width: 8%;">{{ props.row.employee.person.fullName }}</q-td>
            <q-td style="width: 8%;">{{ props.row.reportedBy.person.fullName }}</q-td>
            <q-td class="text-center" style="width: 5%;">{{ props.row.createdOnUtc }}</q-td>
            <q-td style="width: 5%;" class="text-center actions">
              <q-icon
                name="o_visibility"
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="onIssueView(props.row.id)"
              >
                <q-tooltip>View</q-tooltip>
              </q-icon>
                <a
                  v-if="props.row.isEditable || props.row.isNotes"
                  style="position: relative;"
                  class="q-icon notranslate cursor-pointer q-ml-sm q-mr-sm"
                  @click="onNoteAdd(props.row.id, 'Issue', props.row.project.id, props.row.project.name, props.row.name, refreshIssueList)"
                >
                  <q-tooltip anchor="bottom middle" self="top middle">
                    Note
                  </q-tooltip>
                  <q-icon name="o_assignment" />
                  <q-badge
                    v-if="props.row.issueNotesCount > 0"
                    style="position: absolute; right: -16px; top: -15px;"
                    color="green"
                    text-color="white"
                    :label="props.row.issueNotesCount"
                  />
                </a>
                <q-btn dense flat icon="o_more_vert" color="primary">
                  <q-tooltip>More Options</q-tooltip>
                  <q-menu auto-close>
                    <q-list style="min-width: 180px">
                      <q-item
                        v-if="props.row.isEditable"
                        v-ripple clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id); onIssueEdit(props.row.id, refreshIssueList)"
                      >
                        <q-item-section avatar><q-icon name="o_edit" size="xs" /></q-item-section>
                        <q-item-section>Edit</q-item-section>
                      </q-item>
                      <q-item
                        v-if="props.row.isEditable"
                        v-ripple clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id); onIssueStatusLog(props.row.id)"
                      >
                        <q-item-section avatar><q-icon name="o_groups" size="xs" /></q-item-section>
                        <q-item-section>Issue Status change log</q-item-section>
                      </q-item>
                      <q-item
                        v-if="props.row.isEditable"
                        v-ripple clickable
                        :class="{ 'disabled-icon': props.row.status.dropDownValue === 'Converted to Task' }"
                        @click="setActiveRowIdInLocalStorage(props.row.id); onConvertToTask(props.row.id, props.row.projectId, props.row.projectModuleId, props.row.name, props.row.description, true)"
                      >
                        <q-item-section avatar><q-icon name="o_add" size="xs" /></q-item-section>
                        <q-item-section>Convert into Task</q-item-section>
                      </q-item>
                      <q-item
                        v-if="props.row.isEditable"
                        v-ripple clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id); onIssueAddActivity(props.row.id, refreshIssueList)"
                      >
                        <q-item-section avatar><q-icon name="o_description" size="xs" /></q-item-section>
                        <q-item-section>Add Activities</q-item-section>
                      </q-item>
                      <q-item
                        v-if="props.row.isEditable"
                        v-ripple
                        clickable
                        @click="onSubmitIssueDelete(props.row.id, props.row.name, refreshIssueList)"
                      >
                        <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                        <q-item-section class="text-negative">Delete</q-item-section>
                      </q-item>
                    </q-list>
                  </q-menu>
                </q-btn>
            </q-td>
          </q-tr>
          <q-tr v-if="props.pageIndex === rows.length - 1" class="hidden">
            <q-td colspan="9" class="text-right"><b>Total  Issues:</b></q-td>
            <q-td class="text-center"><b>{{ rows.length }}</b></q-td>
            <q-td />
          </q-tr><q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed, onBeforeUnmount } from "vue";
import { useQuasar } from "quasar";
import { notifySuccess, notifyError, getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";
import { useAuthStore } from "stores/auth";
import issuesService from "modules/issue/issue.service";
import taskService from "modules/project-tasks/projectTasks.service";

import editProjectTask from "modules/project-tasks/components/addEdit.vue";
import selectMultiIssue from "modules/issue/components/_multiIssueQuickActions.vue";
import linkTaskToPlan from "modules/project-targetplan/components/_linkRequirementTaskIssueToWeeklyMonthlyPlan.vue";
import manageDropdownsService from "modules/dropdown/dropdown.service";

// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import issueModule from "src/modules/issue/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";

// Shared Issue Dialogs
import {
  initIssueDialogs,
  onIssueView,
  onIssueAdd,
  onIssueEdit,
  onIssueStatusLog,
  onIssueAddActivity
} from "src/modules/issue/utils/dialogs.js";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView
} from "src/modules/project/utils/dialogs.js";

// Shared Project Task Dialogs
import {
  initProjectTaskDialogs,
  onProjectTaskView
} from "src/modules/project-tasks/utils/dialogs.js";

// Shared Common Dialogs
import {
  initCommonDialogs,
  onNoteAdd
} from "src/modules/common/utils/dialogs.js";

// Shared Issue Actions
import {
  initIssueActions,
  onSubmitIssueStatus,
  onSubmitIssueDelete
} from "src/modules/issue/utils/actions.js";

// Common variables
const $q = useQuasar();
const expandedRows = ref([]);
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const statusSummary = ref([]);
const showMultiSelectOptions = ref(false);
const selectedField = ref(null);
const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";
const selectedProjectId = history.state?.projectId;
const processing = ref(false);
const dropdownTypes = ref([]);
const showManageDropdownOptions = ref(false);
const activeEdit = ref({ rowId: null, field: null });

// local storage values
const localStorageKey = "Issue";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

const highlightIssueId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightIssueId);
const highlightedId = computed(() => {
  return activeRowId.value;
});

function setActiveRowIdInLocalStorage (id) {
  const storedData = getLocalStorage(localStorageKey) || {};
  setLocalStorage(localStorageKey, { ...storedData, activeRowId: id });
  activeRowId.value = id;
}

const isExpanded = (rowId) => {
  return expandedRows.value.includes(rowId);
};

// Table variables
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "issueNumber", label: "Id", field: "issueNumber", align: "left", sortable: true },
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true },
  { name: "projectModule.name", label: "Project Module", field: "projectModule.name", align: "left", sortable: true },
  { name: "projectTaskRelatedMappings", label: "Task", field: "projectTaskRelatedMappings", align: "left", sortable: false },
  { name: "name", label: "Issue Name", field: "name", align: "left", sortable: true },
  { name: "priority.dropDownValue", label: "Priority", field: "priority.dropDownValue", align: "left", sortable: true },
  { name: "type.dropDownValue", label: "Type", field: "type.dropDownValue", align: "left", sortable: true },
  { name: "status.dropDownValue", label: "Status", field: "status.dropDownValue", align: "left", sortable: true },
  { name: "employee.person.firstName", label: "Assign To", field: "employee.person.firstName", align: "left", sortable: true },
  { name: "reportedBy.person.firstName", label: "Reported By", field: "reportedBy.person.firstName", align: "left", sortable: true },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "center", sortable: true }
]);

const handleDocumentClick = (event) => {
  const highlightElement = document.querySelector(".highlight");
  // Check if clicked inside the highlighted row or icons
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;
    const storedData = getLocalStorage(localStorageKey) || {};
    setLocalStorage(localStorageKey, { ...storedData, activeRowId: null });
  }
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const refreshIssueList = () => {
  getAllIssue({ pagination: pagination.value });
};

// ------------------------------------------------------------------------------------
// Advance Filter :- On Submit & Cancel
// ------------------------------------------------------------------------------------

const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

// Search variables
const search = ref({
  searchText: getFilterValue("searchText", ""),
  projectIds: getFilterValue("projectIds", selectedProjectId ? [selectedProjectId] : (filterLocalStorage?.projectIds || [])),
  projectModuleIds: getFilterValue("projectModuleIds", []),
  issueTypeIds: getFilterValue("issueTypeIds", []),
  priorityIds: getFilterValue("priorityIds", []),
  statusIds: getFilterValue("statusIds", []),
  employeeIds: getFilterValue("employeeIds", []),
  issueNumber: getFilterValue("issueNumber", 0),
  name: getFilterValue("name", "")
});

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshIssueList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.issueNumber = undefined;
  search.value.projectIds = [];
  search.value.projectModuleIds = [];
  search.value.name = "";
  search.value.priorityIds = [];
  search.value.statusIds = [];
  search.value.issueTypeIds = [];
  search.value.employeeIds = [];
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// DataTable:- Get Issue List
// --------------------------------------------------------------------------------------------------------------------------------------------------

const getAllIssue = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const number = search.value.issueNumber ? search.value.issueNumber.replace(/[^0-9]/g, "").replace(/^0+(?!$)/, "") : "";
  search.value.issueNumber = number || "0";
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
  const storedIssueIds = localStorage.getItem("selectedIssueIds") || [];
  issuesService.getAllIssue(payload).then((resp) => {
    rows.value = resp.data.map(requirement => {
      const hasFullAccess = requirement.project?.projectUserMappings[0]?.fullAccess ?? false;
      return {
        ...requirement,
        checkboxStatus: storedIssueIds.includes(requirement.id), // Initialize checkboxStatus for each row
        isNotes: requirement.project?.projectUserMappings[0]?.notes ?? false,
        isEditable: role === "admin" || hasFullAccess
      };
    });
    statusSummary.value = resp.statusSummary;
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

function getDropdownTypeByModuleName (moduleName) {
  manageDropdownsService.getDropdownTypeByModuleName(moduleName).then((resp) => {
    dropdownTypes.value = resp;
  });
}

function disableOption (option) {
  return option.text && (option.text.toLowerCase() === "new" || option.text.toLowerCase() === "new from test plan");
}

const toggleExpand = (rowId) => {
  if (expandedRows.value.includes(rowId)) {
    expandedRows.value = expandedRows.value.filter(id => id !== rowId);
  } else {
    expandedRows.value.push(rowId);
  }
};

const onConvertToTask = (id, projectId, projectModuleId, name, description, isIssueConverted) => {
  activeRowId.value = id;
  // Collect created task numbers from related mappings
  const taskNumbers = [];
  rows.value.filter(row => id === row.id).forEach(req => {
    if (req.projectTaskRelatedMappings?.length) {
      req.projectTaskRelatedMappings.forEach(mapping => {
        if (mapping.projectTask?.projectTaskNumber) {
          taskNumbers.push(mapping.projectTask.projectTaskNumber);
        }
      });
    }
  });
  $q.dialog({
    component: editProjectTask,
    componentProps: { issueId: id, issueProjectId: projectId, issueModuleId: projectModuleId, name, description, isIssueConverted, taskNumbers }
  }).onOk(() => {
    refreshIssueList();
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = id;
  });
};

// Added colors for task status dropdown list
// function getStatusColor (statusText) {
//   if (statusText) {
//     switch (statusText) {
//     case "Done":
//       return "purple-4";
//     case "Reopen":
//       return "purple-4";
//     case "To Do":
//       return "deep-orange-4";
//     case "Closed":
//       return "grey-4";
//     case "Converted to Task":
//       return "green-4";
//     case "In Development":
//       return "yellow-4";
//     case "In Review":
//       return "cyan-4";
//     case "In Testing":
//       return "deep-orange-4";
//     case "New":
//       return "blue-4";
//     case "New from Test Plan":
//       return "blue-4";
//     case "On Hold":
//       return "brown-4";
//     case "In UAT":
//       return "blue-grey-4";
//     case "UAT Passed":
//       return "green-9";
//     default:
//       return "#ffffff";
//     }
//   }
// }

const multiSelectRequirementProjectMap = ref([]);
const multiSelectProjectIds = ref([]);
const multiSelectProjectName = ref([]);
const multiSelectIssueIds = ref([]);
const multiSelectIssueNames = ref([]);

const onSelectCheckbox = (projectId, projectName, issueId, issueName, flag) => {
  if (flag === true) {
    // Add the issueId to the multiSelectIssueIds array if it's not already present
    if (!multiSelectIssueIds.value.includes(issueId)) {
      multiSelectIssueIds.value.push(issueId);
      multiSelectIssueNames.value.push(issueName);
      multiSelectRequirementProjectMap.value[issueId] = projectId;

      // Add projectId only if not already present
      if (!multiSelectProjectIds.value.includes(projectId)) {
        multiSelectProjectIds.value.push(projectId);
        multiSelectProjectName.value.push(projectName);
      }
    }
  } else {
    // Remove issue
    const index = multiSelectIssueIds.value.indexOf(issueId);
    const removedProjectId = multiSelectRequirementProjectMap.value[issueId];

    if (index !== -1) {
      multiSelectIssueIds.value.splice(index, 1);
      multiSelectIssueNames.value.splice(index, 1);
    }

    delete multiSelectRequirementProjectMap.value[issueId];
    // If no other selected task belongs to that project, remove the projectId
    const stillHasTaskForProject = Object.values(multiSelectRequirementProjectMap.value).some(pid => pid === removedProjectId);
    if (!stillHasTaskForProject) {
      multiSelectProjectIds.value = multiSelectProjectIds.value.filter(x => x !== removedProjectId);
      multiSelectProjectName.value = multiSelectProjectName.value.filter(x => x !== projectName);
    }
  }
  localStorage.setItem("selectedIssueIds", multiSelectIssueIds.value);
};

const selectedFieldOptions = [
  { label: "Link Issue To Plan", value: "linkToPlan", icon: "o_calendar_view_week" },
  { label: "Change Status", value: "Status", icon: "o_flag" },
  { label: "Change Priority", value: "Priority", icon: "o_priority_high" },
  { label: "Convert into Task", value: "convertIntoTask", icon: "o_swap_horiz" }
];

const onSelectMultiOptions = () => {
  activeRowId.value = multiSelectIssueIds.value;
  $q.dialog({
    component: selectMultiIssue,
    componentProps: { issueIds: multiSelectIssueIds.value, selectedField: selectedField.value }
  }).onOk(() => {
    setDefaultsForMultiSelects();
    refreshIssueList();
  }).onCancel(() => {
    selectedField.value = null;
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

const onLinkTaskToPlan = () => {
  const props = {
    projectId: multiSelectProjectIds.value[0],
    projectName: multiSelectProjectName.value[0],
    type: "Issues",
    ids: multiSelectIssueIds.value,
    names: multiSelectIssueNames.value,
    hasTaskLink: rows.value.some(
      r => multiSelectIssueIds.value.includes(r.id) &&
  (!r.projectTaskRelatedMappings?.length))
  };
  $q.dialog({
    component: linkTaskToPlan,
    componentProps: props
  }).onOk(() => {
    setDefaultsForMultiSelects();
    refreshIssueList();
  }).onCancel(() => {
    selectedField.value = null;
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

function onBulkIssuesConvertToTask (issueIds) {
  const selectedIssues = rows.value.filter(row => issueIds.includes(row.id));
  if (!selectedIssues.length) return;

  const hasMissingModule = selectedIssues.some(issue => !issue.projectModuleId);
  if (hasMissingModule) {
    notifyError({ message: "Please select a project module for this issue before converting." });
    selectedField.value = null;
    return;
  }
  processing.value = true;
  // Collect created tasks grouped by task name
  const createdTasks = {};

  selectedIssues.forEach(issue => {
    if (issue.projectTaskRelatedMappings?.length) {
      issue.projectTaskRelatedMappings.forEach(mapping => {
        const issueName = issue.name || "Unnamed Issue";
        const taskNumber = mapping.projectTask?.projectTaskNumber;

        if (taskNumber) {
          if (!createdTasks[issueName]) {
            createdTasks[issueName] = [];
          }
          createdTasks[issueName].push(taskNumber);
        }
      });
    }
  });

  const taskModels = selectedIssues.map(issue => {
    return {
      flag: "Add",
      issueId: issue.id
    };
  });
  const payload = {
    isIssueConverted: true,
    projectTaskModel: taskModels
  };

  let message = "<hr/>Are you sure you want to convert selected issues to task?";
  const taskNames = Object.keys(createdTasks);

  if (taskNames.length > 0) {
    message += "<br/><br/><b>Already created task(s):</b><br/>";
    message += "<table style='width:100%; border-collapse:collapse;' border='1' cellspacing='0' cellpadding='5'>";
    message += "<tr><th>Issue Title</th><th>Task Numbers</th></tr>";

    taskNames.forEach(name => {
      const taskNumbers = createdTasks[name].map(num => `#${num}`).join(", ");
      message += `<tr><td>${name}</td><td>${taskNumbers}</td></tr>`;
    });

    message += "</table>";
  } else {
    message += "<hr/>";
  }
  $q.dialog({
    title: "<span class='text-primary''>Confirmation</span>",
    message,
    html: true,
    ok: { label: "Yes", color: "primary" },
    cancel: { label: "No", color: "negative" }
  }).onOk(() => {
    taskService.saveBulkTasks(payload)
      .then(() => {
        notifySuccess({ message: "Tasks are saved successfully." });
        multiSelectIssueIds.value = [];
        selectedField.value = null;
        localStorage.removeItem("selectedIssueIds");
        refreshIssueList();
      })
      .finally(() => {
        processing.value = false;
      });
  }).onCancel(() => {
    selectedField.value = null;
  });
}

function setDefaultsForMultiSelects () {
  multiSelectProjectIds.value = [];
  multiSelectProjectName.value = [];
  multiSelectRequirementProjectMap.value = [];
  multiSelectIssueIds.value = [];
  multiSelectIssueNames.value = [];
  localStorage.removeItem("selectedIssueIds");
}

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initIssueDialogs(activeRowId);
initProjectDialogs(activeRowId);
initProjectTaskDialogs(activeRowId);
initCommonDialogs(activeRowId);
initIssueActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const { projectNameDropdown } = projectModule();
const { projectModulesByProjectIdForDropdown } = projectModuleOfProjectModule();
const { activeEmployeesDropdown } = employeeModule();
const {
  issueStatusForDropdown,
  issuePriorityForDropdown,
  issueTypeForDropdown,
  issueStatusDropdownSingleSelect
} = issueModule();

// ----------------------------
// Applied Filter Labels.
// ----------------------------
const mapFilterToLabel = (ids, list, label) => {
  if (!Array.isArray(ids) || !ids.length) return {};

  const text = ids
    .map(id => {
      const match = list.value.find(item => item.value === id);
      return match ? match.text : id;
    })
    .join(", ");

  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name"),
  ...mapFilterToLabel(search.value.projectModuleIds, projectModulesByProjectIdForDropdown.list, "Project Module"),
  ...mapFilterToLabel(search.value.priorityIds, issuePriorityForDropdown.list, "Issue Priority"),
  ...mapFilterToLabel(search.value.statusIds, issueStatusForDropdown.list, "Status"),
  ...mapFilterToLabel(search.value.issueTypeIds, issueTypeForDropdown.list, "Issue Type"),
  ...mapFilterToLabel(search.value.employeeIds, activeEmployeesDropdown.list, "Assign To"),
  ...(search.value.issueNumber > 0 ? { "Issue Id": search.value.issueNumber } : {}),
  ...(search.value.name ? { "Issue Name": search.value.name } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Project Module": return search.value.projectModuleIds?.length || 0;
  case "Issue Priority": return search.value.priorityIds?.length || 0;
  case "Status": return search.value.statusIds?.length || 0;
  case "Issue Type": return search.value.issueTypeIds?.length || 0;
  case "Assign To": return search.value.employeeIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Project Module") {
    search.value.projectModuleIds = [];
  } else if (key === "Issue Priority") {
    search.value.priorityIds = [];
  } else if (key === "Status") {
    search.value.statusIds = [];
  } else if (key === "Issue Type") {
    search.value.issueTypeIds = [];
  } else if (key === "Assign To") {
    search.value.employeeIds = [];
  } else if (key === "Issue Id") {
    search.value.issueNumber = "";
  } else if (key === "Issue Name") {
    search.value.name = "";
  }
  delete appliedFilters.value[key];
  refreshIssueList();
}

// ----------------------------
// Multi-Select change events
// ----------------------------
watch(selectedField, (newVal) => {
  if (newVal) {
    showMultiSelectOptions.value = false;
    if (newVal === "linkToPlan") {
      if (multiSelectProjectIds.value.length > 1) {
        $q.notify({
          type: "warning",
          message: "Cannot link plan: selected issues are of multiple projects."
        });
        selectedField.value = null;
        return;
      }
      onLinkTaskToPlan();
      return;
    } else if (newVal === "convertIntoTask") {
      onBulkIssuesConvertToTask(multiSelectIssueIds.value);
      return;
    }
    onSelectMultiOptions(); // This opens the dialog for the selected action
  }
});

watch(multiSelectIssueIds, () => {
  if (multiSelectIssueIds.value.length === 0) {
    showMultiSelectOptions.value = false;
  }
}, { deep: true });

watch(() => search.value.projectIds, (newValue, oldValue) => {
  if (newValue) {
    projectModulesByProjectIdForDropdown.load(false, false, search.value.projectIds);
  }
}, { immediate: true });

// Quick Search
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshIssueList();
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// ----------------------------------------------------------------------------------------------------------------
// On page rendering
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  tableRef.value.requestServerInteraction();
  activeEmployeesDropdown.load(user.siteId);
  projectNameDropdown.load();
  if (search.value.projectIds.length > 0) projectModulesByProjectIdForDropdown.load(false, false, search.value.projectIds);
  getDropdownTypeByModuleName("SDLC");
  issueStatusForDropdown.load("Issue Status");
  issuePriorityForDropdown.load("Issue Priority");
  issueStatusDropdownSingleSelect.load("Issue Status");
  issueTypeForDropdown.load("Issue Type");
  localStorage.removeItem("selectedIssueIds");
  if (!activeRowId.value && highlightIssueId) {
    activeRowId.value = highlightIssueId;
  }

  document.addEventListener("click", handleDocumentClick);
});

</script>
<style>
.disabled-icon {
  color: gray;
  pointer-events: none;
  opacity: 0.6;
}
</style>
