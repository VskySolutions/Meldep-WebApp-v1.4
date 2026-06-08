<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-xs-2 col-sm-1 col-md-2 col-lg-3 col-xl-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Help Desk" />
              <q-breadcrumbs-el label="Ticket" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-xs-3 col-sm-2 col-md-2 col-lg-2 col-xl-3">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-xs-7 col-sm-9 col-md-8 col-lg-7 col-xl-7">
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
                          <label class="Cutomlabel q-mt-sm fs-13">Ticket No</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.ticketNo"
                            clearable
                            class="q-mx-sm w-100 h-auto"
                            mask="#####"
                            fill-input
                            :dense="true"
                          >
                            <template #prepend>
                              <span class="fs-13">{{ prefixRef }}</span>
                            </template>
                          </q-input>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Title</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.title"
                            clearable
                            class="q-mx-sm w-100 h-auto"
                            fill-input
                            :dense="true"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-if="role === 'admin' || roleSupportTeam"
                        v-model="search.employeeEmails"
                        label="Requester"
                        :options="requesterNameForDropdown.list.value"
                        :filter="requesterNameForDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Assigned To</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.assignedToId"
                            class="q-mx-sm w-100 h-auto"
                            stack-label
                            hide-bottom-space
                            use-input
                            clearable
                            :dense="true"
                            :options="assignedToList"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.statusIds"
                        label="Status"
                        :options="helpDeskStatusDropdown.list.value"
                        :filter="helpDeskStatusDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="search.priorityIds"
                        label="Priority"
                        :options="helpDeskPriorityDropdown.list.value"
                        :filter="helpDeskPriorityDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="search.topicIds"
                        label="Workspaces"
                        :options="helpDeskActiveWorkspaceDropdown.list.value"
                        :filter="helpDeskActiveWorkspaceDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.questionIds"
                        label="Menus"
                        :disable="!search.topicIds > 0"
                        :options="helpDeskMenusDropdown.list.value"
                        :filter="helpDeskMenusDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.categoryIds"
                        label="Category"
                        :options="helpDeskCategoryDropdown.list.value"
                        :filter="helpDeskCategoryDropdown.filter"
                        :isShowAll="true"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">From Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.ticketFromDate" fill-input dense mask="##/##/####">
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.ticketFromDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">To Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.ticketToDate" fill-input dense mask="##/##/####">
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.ticketToDate" mask="MM/DD/YYYY" :options="disableBeforeStartDate" @update:model-value="() => $refs.qDateProxy.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
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
              <q-menu v-model="showHelpDeskTopicsOptions" anchor="bottom right" self="top right" no-parent-event style="width: 320px;">
                <q-card class="q-pa-sm">
                  <div class="text-subtitle2 q-mt-sm q-mb-sm">Manage Ticket Workspace & Menu</div>
                  <q-separator />
                  <q-list style="min-width: 200px">
                    <q-item
                      v-for="opt in helpDeskTopics"
                      :key="opt.id"
                      clickable
                      :active="selectedField === opt.id"
                      active-class="bg-primary text-white"
                      @click="$router.push({ path: '/help-desk/topics-questions/list', state: { id: opt.id } })"
                    >
                      <q-item-section>{{ opt.title }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-card>
              </q-menu>
              <div class="q-gutter-sm">
                <q-btn
                  icon="o_grid_view"
                  class="text-white btnRounded bg-primary q-px-sm"
                  :disable="$route.path === '/help-desk/list'"
                  @click="$router.push({ path: '/help-desk/list' })"
                >
                  <q-tooltip>Card View</q-tooltip>
                </q-btn>
                <q-btn
                  v-if="user?.roles?.includes('admin')"
                  icon="o_playlist_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
                  @click="showHelpDeskTopicsOptions = true"
                >
                  <q-tooltip>Manage Ticket Workspace & Menu</q-tooltip>
                </q-btn>
                <q-btn
                  icon="o_add"
                  outline
                  label="Add"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onHelpDeskAdd(refreshHelpDeskList)"
                >
                  <q-tooltip>Add Ticket</q-tooltip>
                </q-btn>
                <!-- Reset Column Width -->
                <q-btn
                  icon="o_refresh"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="resetColumnsWidth"
                >
                  <q-tooltip>Reset Columns Width</q-tooltip>
                </q-btn>
                <!-- Column Hide/Show -->
                <columnVisibilityMenu
                  :all-column-names="allColumnNames"
                  :selected-column-names="selectedColumnNames"
                  @update:selected-column-names="selectedColumnNames = $event"
                  @select-all-columns="selectAllColumns"
                  @default-columns="defaultColumns"
                />
                <!-- Button to Open Sorting Dialog -->
                <q-btn
                  color="primary"
                  icon="o_sort"
                  class="btnRounded q-ml-xs"
                  @click="showSortDialog = true"
                >
                  <q-badge v-if="selectedSortCount > 0" color="green" floating class="q-ml-xs">
                    {{ selectedSortCount }}
                  </q-badge>
                  <q-tooltip>Sort</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="table-scroll-container">
        <q-table
          ref="tableRef"
          v-model:pagination="pagination"
          :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
          :loading="loading"
          :rows="rows"
          :columns="computedColumns"
          row-key="id"
          separator="cell"
          no-data-label="No data available"
          binary-state-sort
          :rows-per-page-options="[20, 50, 100, 200, 500]"
          @request="getAllHelpDesks"
        >
          <template #header="props">
            <q-tr :props="props" class="bg-primary text-white">
              <q-th
                v-for="(col, index) in props.cols"
                :key="col.name"
                :props="props"
                :style="{
                  width: (resizeWidths?.[col.name] || 120) + 'px',
                  minWidth: '80px',
                  position: 'relative'
                }"
                @click="!isResizing && col.sortable"
              >
                {{ col.label }}
                <div class="resize-handle" @mousedown="(e) => startResize(e, col.name)" />
              </q-th>
              <q-th auto-width class="text-center">Actions</q-th>
            </q-tr>
          </template>
          <template #body="props">
            <q-tr :props="props" :class="highlightedId == props.row.id ? 'highlight' : ''">
              <q-td v-if="(role === 'admin' || roleSupportTeam) && selectedColumnNames.includes('requesterEmail')">
                <div class="row items-center justify-between">
                  <div v-if="props.row.requesterEmail" class="text-black">
                    {{ props.row.requesterEmail }}
                  </div>
                  <div v-else-if="props.row.requesterId" class="text-black">
                    {{ props.row.employee?.person?.primaryEmailAddress }}
                  </div>
                  <q-icon
                    v-if="props.row.employee.person.id"
                    name="o_person"
                    class="cursor-pointer q-pl-sm"
                    size="xs"
                    @click="onPersonView(props.row.employee.person.id)"
                  >
                    <q-tooltip>View Requester</q-tooltip>
                  </q-icon>
                </div>
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('ticketNo')"
                class="hoverable-cell"
                @click="onHelpDeskView(props.row.id, props.row.title, props.row.employee.id, props.row.employee.person.primaryEmailAddress, props.row.twilioEmailId, 'Help Desk Notes', refreshHelpDeskList, refreshAllUserListByRoleForDropdown)"
              >
                #{{ props.row.displayTicketNo }}
                <q-tooltip>View Ticket</q-tooltip>
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('title')"
                class="common-q-td hoverable-cell"
                @click="onHelpDeskView(props.row.id, props.row.title, props.row.employee.id, props.row.employee.person.primaryEmailAddress, props.row.twilioEmailId, 'Help Desk Notes', refreshHelpDeskList, refreshAllUserListByRoleForDropdown)"
              >
                <q-tooltip>View Ticket</q-tooltip>
                {{ props.row.title }}
              </q-td>
              <q-td v-if="selectedColumnNames.includes('helpDeskTopic.title')">{{ props.row.helpDeskTopic?.title }}</q-td>
              <q-td v-if="selectedColumnNames.includes('helpDeskTopicQuestions.question')">{{ props.row.helpDeskTopicQuestions?.question }}</q-td>
              <q-td v-if="selectedColumnNames.includes('categoryId')">{{ props.row.category.dropDownValue }}</q-td>
              <q-td
                v-if="selectedColumnNames.includes('statusId')"
                class="common-q-td"
                :class="{ 'hoverable-cell' : props.row.id }"
                @click="activeEdit = { rowId: props.row.id, field: 'status' }"
              >
              <div class="row items-center justify-between q-pr-xs">
                <quickEditSingleSelect
                  field="status"
                  :row-id="props.row.id"
                  :value="props.row.statusId"
                  :display-value="props.row.statusText"
                  :editable="props.row.id"
                  :disable="['Closed','Cancelled'].includes(props.row.statusText)"
                  :options="getVisibleStatusOptions(props.row)"
                  :active-edit="activeEdit"
                  :show-history="false"
                  :loading="updatingRow.status === props.row.id"
                  @cancel="activeEdit = { rowId: null, field: null }"
                  @submit="({ rowId, value }) => onSubmitHelpDeskStatus(rowId, value, refreshHelpDeskList)"
                />
                <div v-if="role === 'admin' && (props.row.statusText == 'Closed' && props.row.previousStatusText == 'Open')" class="q-ml-sm">
                  <q-icon
                    name="o_comment"
                    size="xs"
                    class="cursor-pointer"
                  >
                    <q-tooltip>Add Comments</q-tooltip>
                    <q-popup-edit
                      v-model="props.row.closingComment"
                      anchor="top middle"
                      self="bottom middle"
                      buttons
                      persistent
                      label-set="Save"
                      label-cancel="Cancel"
                      @save="val => onSaveComment(props.row.id, val)"
                    >
                      <template #default="scope">
                        <div class="relative-position q-pa-sm" style="min-width: 260px;">
                          <q-btn
                            icon="o_close"
                            flat
                            round
                            dense
                            size="sm"
                            class="absolute-top-right"
                            @click="scope.cancel"
                          />

                          <div class="text-subtitle2 q-mb-xs">Comment<span class="text-grey-6 fs-12"> (Please leave a comment when closing a ticket directly from New or Open status.)</span></div>

                          <q-input
                            v-model="scope.value"
                            type="textarea"
                            outlined
                            autogrow
                            dense
                          />
                        </div>
                      </template>
                    </q-popup-edit>
                  </q-icon>
                </div>
              </div>
              </q-td>
              <q-td v-if="selectedColumnNames.includes('priority.dropDownValue')">{{ props.row.priority.dropDownValue }}</q-td>
              <q-td
                v-if="selectedColumnNames.includes('assignedTo.person.firstName')"
                class="common-q-td"
                :class="{ 'hoverable-cell' : props.row.id }"
                @click="activeEdit = { rowId: props.row.id, field: 'assignedTo' }"
              >
              <div
                v-if="(role === 'admin' || (roleSupportTeam && !props.row.assignedToId)) && !['Completed','Closed','Cancelled'].includes(props.row.statusText)">
                <quickEditSingleSelect
                  field="assignedTo"
                  :row-id="props.row.id"
                  :value="props.row.assignedToId"
                  :display-value="props.row.assignedTo.person.fullName"
                  :editable="props.row.id"
                  :options="supportTeamUserForDropdown.list.value"
                  :active-edit="activeEdit"
                  :show-history="!!props.row.assignedToId"
                  :loading="updatingRow.assignedTo === props.row.id"
                  @cancel="activeEdit = { rowId: null, field: null }"
                  @submit="({ rowId, value }) => onSubmitAssignedTo(rowId, value, refreshHelpDeskList, refreshAllUserListByRoleForDropdown)"
                  @history="() => onSiteModifiedLog(props.row.id, props.row.title, 'Assigned To')"
                />
              </div>
              <div v-else>
                <div class="row items-center justify-between no-wrap q-pr-xs" style="width: 100px;">
                  <span class="ellipsis q-ml-xs">
                    {{ props.row.assignedTo?.person?.fullName || '-' }}
                  </span>
                  <q-icon
                    v-if="props.row.assignedToCount > 1"
                    name="o_history"
                    class="cursor-pointer"
                    size="xs"
                    @click.stop="onSiteModifiedLog(props.row.id, props.row.title, 'Assigned To')"
                  >
                    <q-tooltip>Data Change Log</q-tooltip>
                  </q-icon>
                </div>
              </div>
              </q-td>
              <q-td v-if="selectedColumnNames.includes('createdBy.person.firstName')">{{ props.row.createdBy.person.firstName + " " + props.row.createdBy.person.lastName }}</q-td>
              <q-td v-if="selectedColumnNames.includes('createdOnUtc')">{{ props.row.createdOnUtc }}</q-td>
              <q-td v-if="selectedColumnNames.includes('updatedBy.person.firstName')">{{ props.row.updatedBy.person.firstName + " " + props.row.updatedBy.person.lastName }}</q-td>
              <q-td v-if="selectedColumnNames.includes('updatedOnUtc')">{{ props.row.updatedOnUtc }}</q-td>
              <!-- <q-td v-if="role === 'admin'" style="width: 10%;" class="hidden">{{ props.row.statusText === 'Closed' || props.row.statusText === 'Completed' ? calculateDuration(props.row.createdOnUtc, props.row.dateStr) : calculateDuration(props.row.createdOnUtc, null) }}</q-td> -->
              <q-td class="text-left actions">
                <q-icon
                  name="o_description"
                  class="cursor-pointer q-mr-sm hidden"
                  size="xs"
                  @click="onAddHelpDeskFiles(props.row.id, refreshHelpDeskList, refreshAllUserListByRoleForDropdown)"
                >
                  <q-tooltip>Files</q-tooltip>
                </q-icon>
                <a
                  v-if="!['Closed','Cancelled'].includes(props.row.statusText)"
                  style="position: relative;"
                  class="q-icon notranslate cursor-pointer q-ml-sm q-mr-md"
                  @click="onHelpDeskViewEmailReplies(props.row.id, props.row.title, props.row.employee.id, props.row.employee.person.primaryEmailAddress, props.row.twilioEmailId, 'Help Desk Notes', refreshHelpDeskList, refreshAllUserListByRoleForDropdown)"
                >
                  <q-tooltip anchor="bottom middle" self="top middle">
                    Email Replies
                  </q-tooltip>
                  <q-icon name="o_email" />
                  <q-badge
                    v-if="props.row.emailRepliesCount > 0"
                    style="position: absolute; right: -16px; top: -15px;"
                    color="green"
                    text-color="white"
                    :label="props.row.emailRepliesCount"
                  />
                </a>
                <q-icon
                  name="o_visibility"
                  class="cursor-pointer q-mr-sm"
                  size="xs"
                  @click="onHelpDeskView(props.row.id, props.row.title, props.row.employee.id, props.row.employee.person.primaryEmailAddress, props.row.twilioEmailId, 'Help Desk Notes', refreshHelpDeskList, refreshAllUserListByRoleForDropdown)"
                >
                  <q-tooltip>View</q-tooltip>
                </q-icon>
                <a
                  style="position: relative;"
                  class="q-icon notranslate cursor-pointer q-ml-sm q-mr-md"
                  @click="onAddNote(props.row.id, 'Help Desk Notes', props.row.id, props.row.title, props.row.title, !['Closed','Cancelled'].includes(props.row.statusText))"
                >
                  <q-tooltip anchor="bottom middle" self="top middle">
                    Note
                  </q-tooltip>
                  <q-icon name="o_assignment" />
                  <q-badge
                    v-if="props.row.helpDeskNotesCount > 0"
                    style="position: absolute; right: -16px; top: -15px;"
                    color="green"
                    text-color="white"
                    :label="props.row.helpDeskNotesCount"
                  />
                </a>
              </q-td>
            </q-tr>
            <q-separator />
          </template>
        </q-table>
      </div>
    </q-card>
  </q-page>
  <!-- Multi-Column Level Sorting -->
  <multiColumnSortingDialog
    v-model="showSortDialog"
    :columns="columns"
    :multi-sort="multiSort"
    :exclude-columns="['Req.', 'Tasks', 'Est. Hrs', 'Issues', 'Tags']"
    @add="addSortLevel"
    @remove="removeSortLevel"
    @apply="applyMultiSort"
  />
</template>
<script setup>
// Import libraries
import { ref, watch, onMounted, onBeforeUnmount, computed } from "vue";
import { useQuasar } from "quasar";
import { notifySuccess } from "assets/utils";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";

import helpDeskService from "modules/helpdesk/helpDesk.service";
import addNote from "modules/common/components/addNote.vue";
import helpDeskTopicsQuestionsService from "modules/helpdesk/helpDeskTopicsQuestions.service.js";

// Shared DataTable Views
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import columnVisibilityMenu from "src/components/dataTable/_columnVisibilityMenu.vue";
import multiColumnSortingDialog from "src/components/dataTable/_multiColumnSortingDialog.vue";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import helpDeskModule from "src/modules/helpdesk/utils/dropdowns.js";

// Shared Scripts DataTable Features
import useColumnResize from "composables/dataTable/useColumnResize.js";
import { useColumnManager } from "composables/dataTable/useColumnManager.js";
import useMultiSort from "composables/dataTable/useMultiSort.js";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// Shared Helpdesk Dialogs
import {
  initHelpDeskDialogs,
  onHelpDeskView,
  onHelpDeskAdd,
  onHelpDeskViewEmailReplies
} from "src/modules/helpdesk/utils/dialogs.js";

// Shared Person Dialogs
import {
  initPersonDialogs,
  onPersonView
} from "src/modules/person/utils/dialogs.js";

// Shared Site Dialogs
import {
  initSiteDialogs,
  onSiteModifiedLog
} from "src/modules/sites/utils/dialogs.js";

// Shared Notes Dialogs
// import {
//   initCommonDialogs,
//   onNoteAdd
// } from "src/modules/common/utils/dialogs.js";

// Shared HelpDesk Actions
import {
  initHelpDeskActions,
  onSubmitHelpDeskStatus,
  onSubmitAssignedTo,
  updatingRow
} from "src/modules/helpdesk/utils/actions.js";
// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const { toDate } = useFilters();
const $q = useQuasar();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const showHelpDeskTopicsOptions = ref(false);
const assignedToList = ref(["Me", "View All"]);
const authStore = useAuthStore();
const user = authStore.user;
const role = user?.roles?.includes("admin") ? "admin" : "";
const roleSupportTeam = !!user?.roles?.includes("support team");
// const logUserRole = role === "admin" ? "admin" : (roleSupportTeam ? "assignedEmployee" : "requester");
const loginUserEmployeeId = user?.employeeId;
const helpDeskTopics = ref([]);
const prefixRef = ref("");
// const activeRowId = ref(null);
const activeEdit = ref({ rowId: null, field: null });
const showSortDialog = ref(false);

const currentSiteId = computed(() => user?.siteId || null);
// const updatingRow = ref({
//   status: null,
//   assignedTo: null
// });

// ----------------------------------------------------------------------------------------------------------------
// Table variables
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);

// const empRole = user?.roles?.includes("employee") ? "employee" : "";
const columns = ref([
  { name: "ticketNo", label: "Ticket No", field: "ticketNo", align: "left", sortable: true, default: true },
  { name: "title", label: "Title", field: "title", align: "left", sortable: true, default: true },
  { name: "helpDeskTopic.title", label: "Workspace", field: "helpDeskTopic.title", align: "left", sortable: true, default: false },
  { name: "helpDeskTopicQuestions.question", label: "Menu", field: "helpDeskTopicQuestions.question", align: "left", sortable: true, default: false },
  { name: "categoryId", label: "Ticket Category", field: "categoryId", align: "left", sortable: true, default: true },
  { name: "statusId", label: "Status", field: "statusId", align: "left", sortable: true, default: true },
  { name: "priority.dropDownValue", label: "Priority", field: "priority.dropDownValue", align: "left", sortable: true, default: true },
  { name: "assignedTo.person.firstName", label: "Assigned To", field: "assignedTo.person.firstName", align: "left", isEditing: false, sortable: true, default: true },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true, default: false },
  { name: "updatedBy.person.firstName", label: "Updated By", field: "updatedBy.person.firstName", align: "left", sortable: true, default: true },
  { name: "updatedOnUtc", label: "Updated Date", field: "updatedOnUtc", align: "left", sortable: true, default: true }
]);
// if (role === "admin") {
//   columns.value.push({ name: "duration", label: "Duration", field: "duration", align: "left", sortable: false });
// }
if (role === "admin" || roleSupportTeam) {
  columns.value.unshift({ name: "requesterEmail", label: "Requester", field: "requesterEmail", align: "left", sortable: true, default: true });
}

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> get All HelpDesks list
// ----------------------------------------------------------------------------------------------------------------

const getAllHelpDesks = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;

  const sorts = {};
  const multi = multiSort.value;
  for (let i = 0; i < multi.length; i++) {
    const s = multi[i];
    if (s.column && s.direction) {
      sorts[s.column] = s.direction;
    }
  }

  search.value.ticketFromDate = search.value.ticketFromDate === "" ? null : toDate(search.value.ticketFromDate);
  search.value.ticketToDate = search.value.ticketToDate === "" ? null : toDate(search.value.ticketToDate);

  if (search.value.topicIds?.length > 0 && search.value.topicIds?.length > 0) { helpDeskMenusDropdown.load(search.value.topicIds); }
  if (search.value.ticketFromDate === "") { search.value.ticketFromDate = null; }
  if (search.value.ticketToDate === "") { search.value.ticketToDate = null; }
  search.value.ticketNo = (search.value.ticketNo === "" || search.value.ticketNo === null) ? 0 : Number(search.value.ticketNo);

  const payload = { page, pageSize: rowsPerPage, sortBy, descending, sorts, ...search.value };
  helpDeskService.getAllHelpDesks(payload).then((resp) => {
    rows.value = resp.data;
    const displayTicketNo = resp.data?.[0]?.displayTicketNo;
    if (displayTicketNo) {
      prefixRef.value = displayTicketNo.split("-")[0] + " -";
    }

    Object.assign(pagination.value, {
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    });
    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: activeRowId.value,
      sorts
    });
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  resizeWidths,
  selectedColumnNames,

  saveDataTableState,
  saveResizableWidthState,
  saveColumnsState
} = useSiteTableState({
  storageKey: "helpdesks-Index",
  siteId: currentSiteId,

  defaultSearch: {
    searchText: "",
    title: "",
    ticketNo: 0,
    categoryIds: [],
    statusIds: [],
    priorityIds: [],
    topicIds: [],
    questionIds: [],
    employeeEmails: [],
    assignedToId: null,
    ticketFromDate: "",
    ticketToDate: ""
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  },

  defaultSorts: {},

  defaultResizableWidth: {},

  defaultColumns: columns.value
    .filter(col => col.default === true)
    .map(col => col.name)
});

const lsSorts = sorts.value || null;

const highlightedId = computed(() => {
  return activeRowId.value;
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Column resize functionality (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  startResize,
  resetColumnsWidth,
  isResizing
} = useColumnResize({
  columns,
  resizeWidths,
  saveResizableWidthState
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Hide/Show Columns (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  selectAllColumns,
  defaultColumns,
  allColumnNames,
  computedColumns
} = useColumnManager({
  columns,
  selectedColumnNames,
  saveColumnsState,
  isResizing
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Sort Filter (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  multiSort,
  addSortLevel,
  removeSortLevel,
  applyMultiSort,
  selectedSortCount
} = useMultiSort({
  lsSorts,
  saveDataTableState,
  onApplySort: () => {
    refreshHelpDeskList();
  }
});
// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initHelpDeskDialogs(activeRowId);
initPersonDialogs(activeRowId);
// initCommonDialogs(activeRowId);
initSiteDialogs(activeRowId);
initHelpDeskActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshHelpDeskList = () => {
  getAllHelpDesks({ pagination: pagination.value });
};

const refreshAllUserListByRoleForDropdown = () => {
  supportTeamUserForDropdown.load("support team");
}

const handleDocumentClick = (event) => {
  const highlightElement = document.querySelector(".highlight");
  // Check if clicked inside the highlighted row or icons
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;
    saveDataTableState({
      activeRowId: null
    });
  }
};

function onSaveComment (id, comment) {
  setTimeout(function () {
    const payload = {
      closingComment: comment
    };
    helpDeskService.addorUpdateHelpDeskStatusComment(id, payload).then(resp => {
      notifySuccess({ message: "Ticket comment is saved successfully." });
      refreshHelpDeskList();
    });
  });
}

// ===========================================================
// Status Flow
// ===========================================================

const STATUS_TRANSITIONS = {
  requester: {
    New: ["Cancelled"],
    Open: ["Cancelled"],
    "Awaiting Client": ["In Progress", "Completed"],
    Completed: ["Closed", "Reopen", "Cancelled"],
    Reopen: ["Cancelled"]
  },
  admin: {
    Open: ["Closed"],
    Assigned: ["In Progress", "Closed"],
    "In Progress": ["Awaiting Client", "Completed", "Closed"],
    "Awaiting Client": ["In Progress"],
    Completed: ["Closed"],
    Reopen: ["In Progress", "Closed"]
  },
  assignedEmployee: {
    Assigned: ["In Progress"],
    "In Progress": ["Completed", "Awaiting Client"],
    "Awaiting Client": ["In Progress"],
    Reopen: ["In Progress"]
  }
};

// Get current status text
function resolveStatusText (statusId) {
  return helpDeskStatusDropdown.list.value
    .find(s => s.value === statusId)
    ?.text ?? null;
}

function getVisibleStatusOptions (row) {
  const currentStatusText =
    row.statusText || resolveStatusText(row.statusId);

  if (!currentStatusText) {
    return helpDeskStatusDropdown.list.value;
  }

  const isAdmin = role === "admin";
  const isSupport = roleSupportTeam;
  const isRequester = row.requesterId === loginUserEmployeeId;

  let allowed = [];

  // Requester Flow
  if (isRequester) {
    allowed.push(
      ...(STATUS_TRANSITIONS.requester?.[currentStatusText] || [])
    );
  }

  // Admin Flow
  if (isAdmin) {
    allowed.push(
      ...(STATUS_TRANSITIONS.admin?.[currentStatusText] || [])
    );
  }

  // Support Team Flow
  if (isSupport) {
    let supportRole = "assignedEmployee";

    if (row.assignedToId !== loginUserEmployeeId) {
      supportRole = "admin";
    }

    allowed.push(
      ...(STATUS_TRANSITIONS[supportRole]?.[currentStatusText] || [])
    );
  }

  // Fallback for normal requester-only user
  if (!isAdmin && !isSupport && !isRequester) {
    allowed =
      STATUS_TRANSITIONS.requester?.[currentStatusText] ?? [];
  }

  // Remove duplicate statuses
  allowed = [...new Set(allowed)];

  // Return visible statuses
  return helpDeskStatusDropdown.list.value.filter(option => {
    // Always display current status
    if (option.text === currentStatusText) return true;

    // Display only allowed transitions
    return allowed.includes(option.text);
  });
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------

const {
  requesterNameForDropdown,
  helpDeskStatusDropdown,
  helpDeskPriorityDropdown,
  helpDeskActiveWorkspaceDropdown,
  helpDeskMenusDropdown,
  helpDeskCategoryDropdown,
  supportTeamUserForDropdown
} = helpDeskModule();

function getAllHelpDeskTopicList () {
  helpDeskTopicsQuestionsService.getAllHelpDeskTopicList().then((resp) => {
    helpDeskTopics.value = resp;
  });
}

// ------------------------------------------------------------------------------------
// Advance Filter :- On Submit & Cancel
// ------------------------------------------------------------------------------------

// Search records as per parameters
const onAdvanceSearch = () => {
  helpDeskStatusDropdown.load("HelpDesk Status");
  refreshHelpDeskList()
};

// Clear search
const onAdvanceClear = () => {
  search.value = {
    searchText: "",
    title: "",
    ticketNo: 0,
    categoryIds: [],
    statusIds: [],
    priorityIds: [],
    topicIds: [],
    questionIds: [],
    employeeEmails: [],
    assignedToId: null,
    ticketFromDate: "",
    ticketToDate: ""
  };

  saveDataTableState({
    search: search.value
  });

  refreshHelpDeskList();
};

// ----------------------------
// popup
// ----------------------------
const onAddNote = (id, type, moduleId, module, name, isShow) => {
  activeRowId.value = id;
  $q.dialog({
    component: addNote,
    componentProps: { id, type, moduleId, module, name, isShow }
  }).onOk(() => {
    refreshHelpDeskList();
  }).onCancel(() => {
    refreshHelpDeskList();
  }).onDismiss(() => {
    activeRowId.value = id;
    refreshHelpDeskList();
  });
};

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
  ...(search.value.title ? { Title: search.value.title } : {}),
  ...(search.value.ticketNo ? { "Ticket No": prefixRef.value ? prefixRef.value + " " + search.value.ticketNo : search.value.ticketNo } : {}),
  ...mapFilterToLabel(search.value.employeeEmails, requesterNameForDropdown.list, "Requester"),
  ...(search.value.assignedToId ? { "Assigned To": search.value.assignedToId } : {}),
  ...mapFilterToLabel(search.value.statusIds, helpDeskStatusDropdown.list, "Status"),
  ...mapFilterToLabel(search.value.priorityIds, helpDeskPriorityDropdown.list, "Priority"),
  ...mapFilterToLabel(search.value.topicIds, helpDeskActiveWorkspaceDropdown.list, "Workspaces"),
  ...mapFilterToLabel(search.value.questionIds, helpDeskMenusDropdown.list, "Menus"),
  ...mapFilterToLabel(search.value.categoryIds, helpDeskCategoryDropdown.list, "Category"),
  ...(search.value.ticketFromDate ? { "From Date": search.value.ticketFromDate } : {}),
  ...(search.value.ticketToDate ? { "To Date": search.value.ticketToDate } : {})
}));

function onClearFilters (key) {
  if (key === "Requester") {
    search.value.employeeEmails = [];
  } else if (key === "Assigned To") {
    search.value.assignedToId = "";
  } else if (key === "Title") {
    search.value.title = "";
  } else if (key === "Ticket No") {
    search.value.ticketNo = 0;
  } else if (key === "From Date") {
    search.value.ticketFromDate = "";
  } else if (key === "To Date") {
    search.value.ticketToDate = "";
  } else if (key === "Category") {
    search.value.categoryIds = [];
  } else if (key === "Status") {
    search.value.statusIds = [];
  } else if (key === "Priority") {
    search.value.priorityIds = [];
  } else if (key === "Workspaces") {
    search.value.topicIds = [];
  } else if (key === "Menus") {
    search.value.questionIds = [];
  }
  delete appliedFilters.value[key];
  saveDataTableState({
    search: search.value
  });
  refreshHelpDeskList();
}

function getFilterCount (key) {
  switch (key) {
  case "Requester": return search.value.employeeEmails?.length || 0;
  case "Category": return search.value.categoryIds?.length || 0;
  case "Status": return search.value.statusIds?.length || 0;
  case "Priority": return search.value.priorityIds?.length || 0;
  case "Workspaces": return search.value.topicIds?.length || 0;
  case "Menus": return search.value.questionIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

// Quick Search
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshHelpDeskList();
});

watch(() => search.value.topicIds, (newValue, oldValue) => {
  if (newValue !== oldValue) {
    search.value.questionIds = []; // Clear question dropdown
    helpDeskMenusDropdown.load(search.value.topicIds);
  }
}, { immediate: true });

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  loading.value = true;
  tableRef.value.requestServerInteraction();
  helpDeskPriorityDropdown.load("HelpDesk Priority");
  helpDeskStatusDropdown.load("HelpDesk Status");
  requesterNameForDropdown.load();
  supportTeamUserForDropdown.load("support team");
  helpDeskCategoryDropdown.load("HelpDesk Category");
  helpDeskActiveWorkspaceDropdown.load();
  getAllHelpDeskTopicList();
  document.addEventListener("click", handleDocumentClick);
});
</script>

<style>
.q-select.help-desk-assign-list .q-field__control {
  width: 100% !important;
  min-height: 35px !important;
  min-width: 100% !important;
  word-break: break-word;
  overflow-wrap: anywhere;
}
.q-select.help-desk-input .q-field__input {
  width: 100% !important;
  min-width: 100% !important;
  word-break: break-word;
}
.Custom-DataTable {
  min-width: max-content;
}
</style>
