<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none PlannerDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.title }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
            <q-tab name="1_tab" label="Ticket Details" class="q-px-lg q-mr-md" />
            <q-tab v-if="!props.hideSecondTab" name="2_tab" label="Email Replies" class="q-px-lg" />
            <q-tab name="3_tab" label="Notes" class="q-px-lg" />
            <q-tab name="4_tab" label="Files" class="q-px-lg" :disable="disableTab" />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated class="q-mt-xs">
            <q-tab-panel name="1_tab">
              <fieldset>
                <legend>Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Ticket No
                    </div>
                    <div v-if="model.ticketNo" class="text-black">
                      #{{ model.sitePrefix }} - {{ model.ticketNo }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Title
                    </div>
                    <div v-if="model.title" class="text-black">
                      {{ model.title }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-4 col-md-4">
                    <div>
                      <div class="q-mb-xs">Status</div>
                      <div class="row items-center no-wrap">
                        <q-select
                          v-model="model.statusId"
                          outlined
                          use-input
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          :options="getVisibleStatusOptions(model)"
                          class="help-desk-status-list q-mr-sm"
                          :disable="isDisabled(model.statusId) === 'Closed' || isDisabled(model.statusId) === 'Cancelled'"
                          option-value="value"
                          option-label="text"
                          emit-value
                          map-options
                          style="width: 49%"
                          :bg-color="getStatusColor(model.statusText)"
                          :loading="formLoading.status"
                          @filter="getAllStatusListForFilter"
                          @update:model-value="onChangeStatus(model.id, model.statusId)"
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
                        <q-icon
                          v-if="model.statusText == 'Closed' && model.previousStatusText == 'Open'"
                          name="o_comment"
                          size="xs"
                          class="cursor-pointer"
                        >
                          <q-tooltip>Add Comments</q-tooltip>
                          <q-popup-edit
                            v-model="model.closingComment"
                            anchor="top middle"
                            self="bottom middle"
                            buttons
                            persistent
                            label-set="Save"
                            label-cancel="Cancel"
                            @save="val => onSaveComment(props.id, val)"
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
                    <!-- <div v-else>
                      <div class="q-mb-xs">Status
                      </div>
                      <div v-if="model.statusText" class="text-black">
                        {{ model.statusText }}
                      </div>
                    </div> -->
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Workspace
                    </div>
                    <div class="text-black">
                      {{ model.helpDeskTopic?.title }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Menu
                    </div>
                    <div class="text-black">
                      {{ model.helpDeskTopicQuestions?.question }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-2">
                    <div class="q-mb-xs">Priority</div>
                    <div v-if="roleSupportTeamOrAdmin" class="col-12 col-sm-6 col-md-4">
                      <div
                        v-if="isDisabled(model.statusId) === 'Completed' || isDisabled(model.statusId) === 'Closed' || isDisabled(model.statusId) === 'Cancelled'"
                        class="text-black"
                      >
                        {{ model.priority?.dropDownValue }}
                      </div>
                      <div v-else class="text-black">
                        <q-select
                          v-model="model.priorityId"
                          outlined
                          stack-label
                          use-input
                          hide-bottom-space
                          :dense="true"
                          :options="priorityList"
                          class="help-desk-status-list"
                          option-value="value"
                          option-label="text"
                          emit-value
                          map-options
                          :loading="formLoading.priority"
                          @filter="getAllPriorityListForFilter"
                          @update:model-value="onChangePriority(model.id, model.priorityId)"
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
                    <div v-else class="col-12 col-sm-6 col-md-4">
                      <!-- <div class="q-mb-xs">Priority -->
                      <!-- <q-badge v-if="!model.statusText" color="red" square class="q-ml-sm">No Data</q-badge> -->
                      <!-- </div> -->
                      <div v-if="model.priority" class="text-black">
                        {{ model.priority.dropDownValue }}
                      </div>
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Ticket Category
                    </div>
                    <div v-if="model.category" class="text-black">
                      {{ model.category.dropDownValue }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Requester
                    </div>
                    <div class="row items-center no-wrap">
                      <div v-if="model.requesterEmail" class="text-black">
                        {{ model.requesterEmail }}
                      </div>
                      <div v-else-if="model.requesterId" class="text-black">
                        {{ model.employee?.person?.primaryEmailAddress }}
                      </div>
                      <q-icon
                        v-if="model.employee.person.id"
                        name="o_person"
                        class="cursor-pointer q-pl-sm"
                        size="xs"
                        @click="onPersonView(model.employee.person.id)"
                      >
                        <q-tooltip>View Requester</q-tooltip>
                      </q-icon>
                    </div>
                  </div>
                  <div class="col-12 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Assigned To
                    </div>
                    <div v-if="role === 'admin'">
                      <div
                        v-if="isDisabled(model.statusId) === 'Completed' || isDisabled(model.statusId) === 'Closed' || isDisabled(model.statusId) === 'Cancelled'"
                        class="text-black"
                      >
                        {{ model.assignedTo?.person?.fullName }}
                        <q-icon
                          v-if="model.assignedToId"
                          name="o_history"
                          class="cursor-pointer q-ml-xs"
                          size="xs"
                          @click.stop="onStatusLog(model.id, model.title, 'Assigned To')"
                        >
                          <q-tooltip>Data Change Log</q-tooltip>
                        </q-icon>
                      </div>
                      <div v-else class="row items-center no-wrap text-black">
                        <q-select
                          v-model="model.assignedToId"
                          outlined
                          use-input
                          hide-bottom-space
                          dense
                          :options="userList"
                          class="help-desk-status-list q-mr-sm"
                          option-value="value"
                          option-label="text"
                          emit-value
                          map-options
                          style="width: 49%;"
                          :loading="formLoading.assignedTo"
                          @filter="getAllUserListForFilter"
                          @update:model-value="onChangeAssignedTo(model.id, model.assignedToId)"
                        />
                        <q-icon
                          v-if="model.assignedToCount > 1"
                          name="o_history"
                          class="cursor-pointer q-ml-xs"
                          size="xs"
                          @click.stop="onStatusLog(model.id, model.title, 'Assigned To')"
                        >
                          <q-tooltip>Data Change Log</q-tooltip>
                        </q-icon>
                      </div>
                    </div>
                    <div v-else-if="model.assignedToId" class="text-black">
                      {{ model.assignedTo.person.fullName }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md hidden">
                  <div class="col-12 col-sm-6 col-md-2">
                    <div class="q-mb-xs">Company</div>
                    <div v-if="roleSupportTeamOrAdmin" class="col-12 col-sm-6 col-md-4 hidden">
                      <div
                        v-if="isDisabled(model.statusId) === 'Completed' || isDisabled(model.statusId) === 'Closed' || isDisabled(model.statusId) === 'Cancelled'"
                        class="text-black"
                      >
                        {{ model.company?.name }}
                      </div>
                      <div v-else>
                        <q-select
                          v-model="model.companyId"
                          outlined
                          stack-label
                          use-input
                          hide-bottom-space
                          :dense="true"
                          :options="customerList"
                          class="help-desk-status-list"
                          option-value="value"
                          option-label="text"
                          emit-value
                          map-options
                          :loading="formLoading.company"
                          @update:model-value="onChangeCompanyClient(model.id, model.companyId)"
                          @filter="getAllCustomerListForFilter"
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
                    <div class="col-12 col-sm-6 col-md-2">
                      <div v-if="model.company?.name" class="text-black">
                        {{ model.company?.name }}
                      </div>
                    </div>
                    <!-- </div> -->
                  </div>
                </div>
                <div v-if="model.description" class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-12 col-md-12">
                    <div class="q-mb-xs flex items-center">
                      Description
                    </div>
                    <div class="text-black RichTextEditor">
                      <p v-if="model.description" v-html="model.description" />
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div v-if="roleSupportTeamOrAdmin" class="col-12 col-sm-6 col-md-4">
                    <div class="q-mb-xs">Average Duration
                      <div class="text-black">
                        {{ model.averageDurationText ? model.averageDurationText : 0 }}
                      </div>
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-4">
                    <div class="q-mb-xs">Created Date
                      <div class="text-black">
                        {{ model.createdOnUtc }}
                      </div>
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-4">
                    <div class="q-mb-xs">Created By
                      <div class="text-black">
                        {{ model.createdBy?.person?.fullName }}
                      </div>
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-4">
                    <div class="q-mb-xs">Updated Date
                      <div class="text-black">
                        {{ model.updatedOnUtc }}
                      </div>
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-4">
                    <div class="q-mb-xs">Updated By
                      <div class="text-black">
                        {{ model.updatedBy?.person?.fullName }}
                      </div>
                    </div>
                  </div>
                </div>
              </fieldset>
              <fieldset class="q-mt-lg">
                <legend>History of Status</legend>
                <q-table
                  ref="tableRef"
                  v-model:pagination="statusPagination"
                  bordered
                  class="no-shadow"
                  :loading="loading"
                  :rows="statusRows"
                  :columns="statusColumns"
                  row-key="id"
                  separator="cell"
                  no-data-label="No data available"
                  :rows-per-page-options="[20, 50, 100, 200, 500]"
                  binary-state-sort
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">
                        {{ col.label }}
                      </q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                      <q-td>
                        {{ props.row.status.dropDownValue }}
                      </q-td>
                      <q-td>
                        {{ props.row.statusDurationText }}
                      </q-td>
                      <q-td>
                        {{ props.row.createdOnUtc }}
                      </q-td>
                      <q-td>
                        {{ props.row.createdBy?.person?.fullName }}
                      </q-td>
                    </q-tr>
                  </template>
                </q-table>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel v-if="!props.hideSecondTab" name="2_tab">
              <!-- <emailReplies :help-desk-id="helpDeskId" :logged-in-email="loggedInEmail" :employee-id="employeeId" :twilio-email-id="twilioEmailId" :title="title" /> -->
              <fieldset>
                <!-- <div class="box-shadow"> -->
                <div class="q-mb-sm q-pa-xs">
                  <div class="row items-center justify-end">
                    <div class="row items-center q-gutter-sm">
                      <q-btn
                        dense
                        unelevated
                        outline
                        icon="o_refresh"
                        color="primary"
                        :disable="!helpDeskId"
                        @click="refreshEmails"
                      >
                        <q-tooltip>Refresh conversation</q-tooltip>
                      </q-btn>
                      <q-btn
                        v-if="!['Closed','Cancelled'].includes(model.statusText)"
                        dense
                        unelevated
                        outline
                        color="white"
                        text-color="primary"
                        :icon="showReplyEditor ? 'o_close' : 'o_reply'"
                        :label="showReplyEditor ? 'Close' : 'Reply'"
                        :disable="!helpDeskId"
                        @click="onReplyToggle"
                      />
                      <!-- Show system emails checkbox -->
                      <q-checkbox
                        v-model="showSystemEmails"
                        label="Show system emails?"
                        dense
                        color="primary"
                      />
                    </div>
                  </div>
                </div>
                <!-- EMAIL COMPONENT -->
                <emailReplies
                  v-if="helpDeskId"
                  :key="emailKey"
                  v-model="replyText"
                  :help-desk-id="helpDeskId"
                  :logged-in-email="loggedInEmail"
                  :employee-id="employeeId"
                  :twilio-email-id="twilioEmailId"
                  :show-reply="showReplyEditor"
                  :title="title"
                  :show-system-emails="showSystemEmails"
                  :notes-type="notesTypes"
                />
                <div v-else class="q-pa-sm text-center text-red">
                  No data available
                </div>
                <!-- </div> -->
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="3_tab">
              <addNotes :id="helpDeskId" :notes-type="notesTypes" :title="title" :is-show="!['Closed','Cancelled'].includes(model.statusText)" />
            </q-tab-panel>
            <q-tab-panel name="4_tab">
              <fieldset class="q-mb-lg">
                <legend>Files</legend>
                <div class="q-mb-sm q-gutter-sm flex justify-end">
                  <q-input
                    v-model="filter"
                    outlined
                    class="bg-white q-mr-sm search-box"
                    debounce="300"
                    placeholder="Search"
                    dense
                    clearable
                  >
                    <template #prepend>
                      <q-icon
                        name="o_search"
                      />
                    </template>
                  </q-input>
                </div>
                <q-table
                  ref="tableRef"
                  v-model:pagination="filesPagination"
                  bordered class="no-shadow"
                  :loading="loading"
                  :rows="filesRows"
                  :columns="filesColumns"
                  row-key="id"
                  :filter="filter"
                  separator="cell"
                  binary-state-sort
                  :rows-per-page-options="[20, 50, 100, 200, 500]"
                  @request="getAllFilesByHelpDeskId"
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">
                        {{ col.label }}
                      </q-th>
                      <q-th auto-width class="text-center">
                        Actions
                      </q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''" :set="(preSourceName = null,preSubModuleName = null)">
                      <q-td>
                        {{ extractFileName(props.row.seoFilename) }}
                      </q-td>
                      <q-td>
                        {{ props.row.createdBy.person.fullName }}
                      </q-td>
                      <q-td>
                        {{ props.row.createdOnUtc }}
                      </q-td>
                      <q-td style="width: 5%;" class="text-center actions">
                        <q-btn
                          icon="o_visibility"
                          size="sm"
                          class="q-pr-xs"
                          flat
                          @click="viewFile(props.row.virtualPath)"
                        />
                        <!-- <q-btn icon="o_download" size="sm" class="q-pl-sm" flat :href="baseURL + props.row.virtualPath" :download="props.row.virtualPath" /> -->
                        <q-btn
                          icon="o_download"
                          size="sm"
                          class="q-pl-xs q-pr-xs"
                          flat
                          @click="downloadFile(props.row.virtualPath)"
                        />
                        <q-btn
                          icon="o_delete_outline"
                          color="negative"
                          size="sm"
                          class="q-pl-xs text-negative"
                          flat
                          @click="onDelete(props.row)"
                        />
                      </q-td>
                    </q-tr>
                  </template>
                </q-table>
              </fieldset>
            </q-tab-panel>
          </q-tab-panels>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import { ref, onMounted, watch, nextTick } from "vue";
import _ from "lodash";
import helpDeskService from "modules/helpdesk/helpDesk.service";
import commonService from "services/common.service";
import customerService from "src/modules/customer/customer.service";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import { useAuthStore } from "stores/auth";
import projectService from "modules/project/projects.service";
import usersService from "modules/user-management/userManagement.service";
import emailReplies from "modules/helpdesk/components/_emailReplies.vue";
import siteStatusLog from "modules/sites/components/_siteModifiedLogs.vue";
import viewPerson from "modules/person/components/view.vue";
import addNotes from "modules/common/components/_addNoteTimelineView.vue";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" },
  title: { type: String, default: "" },
  employeeId: { type: String, default: "" },
  primaryEmailAddress: { type: String, default: "" },
  twilioEmailId: { type: String, default: "" },
  hideSecondTab: { type: Boolean, default: false },
  defaultTab: {
    type: String,
    default: "1_tab"
  },
  notesType: { type: String, default: "" }
});

// Common variables
const loading = ref(true);
const filter = ref("");
const authStore = useAuthStore();
const user = authStore.user;
const role = user?.roles?.includes("admin") ? "admin" : "";
const roleSupportTeamOrAdmin = user?.roles?.includes("admin") || user?.roles?.includes("support team");
const roleSupportTeam = !!user?.roles?.includes("support team");
// const logUserRole = roleAdmin ? "admin" : (roleSupportTeam ? "assignedEmployee" : "requester");
const loginUserEmployeeId = user?.employeeId;
// const baseURL = process.env.API_BASE_URL;
const helpDeskId = props.id;
const loggedInEmail = props.primaryEmailAddress;
const employeeId = props.employeeId;
const twilioEmailId = props.twilioEmailId;
const title = props.title;
const notesTypes = "Help Desk Notes";
const $q = useQuasar();
const emailKey = ref(0);
const showReplyEditor = ref(false);
const replyText = ref("");
const showSystemEmails = ref(false);
// const isCompanyReadonly = ref(false);

// Define model values
const model = ref({
  title: "",
  companyId: "",
  description: "",
  createdOnUtc: "",
  updatedOnUtc: "",
  priorityId: "",
  sitePrefix: "",
  assignedToCount: "",
  company: {
    name: ""
  },
  employee: {
    person: {
      fullName: "",
      primaryEmailAddress: ""
    }
  },
  priority: {
    dropDownValue: ""
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
  },
  helpDeskTopic: {
    title: ""
  },
  helpDeskTopicQuestions: {
    question: ""
  }
});

const formLoading = ref({
  status: false,
  assignedTo: false,
  priority: false,
  category: false
});
// status rows
const statusRows = ref([]);
const filesRows = ref([]);
const tab = ref("1_tab");
const statusPagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const statusColumns = ref([
  { name: "status.dropDownValue", label: "Status", field: "status.dropDownValue", align: "left", sortable: false },
  { name: "statusDurationText", label: "Duration", field: "statusDurationText", align: "left", sortable: false },
  { name: "createdOnUtc", label: "Changed Date", field: "createdOnUtc", align: "left", sortable: false },
  { name: "createdBy.person.fullName", label: "Changed By", field: "createdBy.person.fullName", align: "left", sortable: false }
]);

// files rows
const filesPagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const filesColumns = ref([
  { name: "virtualPath", label: "Name", field: "virtualPath", align: "left" },
  { name: "createdBy.person.fullName", label: "Uploaded By", field: "createdByPersonFullName", align: "left", sortable: false },
  { name: "createdOnUtc", label: "Uploaded Date", field: "createdOnUtc", align: "left" }
]);

// refresh emails
const refreshEmails = () => {
  emailKey.value++;
};

// View popup
const onStatusLog = (id, name, columnName) => {
  $q.dialog({
    component: siteStatusLog,
    componentProps: { id, name, columnName }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

function onSaveComment (id, comment) {
  setTimeout(function () {
    const payload = {
      closingComment: comment
    };
    helpDeskService.addorUpdateHelpDeskStatusComment(id, payload).then(resp => {
      notifySuccess({ message: "Ticket comment is saved successfully." });
    });
  });
}

const onReplyToggle = () => {
  if (showReplyEditor.value && replyText.value.trim()) {
    $q.dialog({
      title: "Discard reply?",
      message: "You have unsent text. Are you sure you want to close?",
      cancel: true,
      persistent: true
    }).onOk(() => {
      replyText.value = "";
      showReplyEditor.value = false;
    });
  } else {
    showReplyEditor.value = !showReplyEditor.value;
  }
};

const priorityList = ref([]);
const priorityListOptions = ref([]);
function getHelpDeskPriority (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    priorityList.value = responseData;
    priorityListOptions.value = responseData;
  });
}

function getAllPriorityListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      priorityList.value = priorityListOptions.value;
    } else {
      priorityList.value = priorityListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const customerList = ref([]);
const customerListOptions = ref([]);
function getAllCustomerListForDropdown () {
  customerService.getAllCustomerListForDropdown().then((resp) => {
    const responseData = resp
      .map((item) => ({ text: item.company ? item.company.name : `${item.person.firstName} ${item.person.lastName}`, value: item.id }))
      .sort((a, b) => a.text.localeCompare(b.text));
    customerList.value = responseData;
    customerListOptions.value = responseData;
    // If only one company -> auto select & readonly
    // if (responseData.length === 1) {
    //   isCompanyReadonly.value = true;
    // } else {
    //   isCompanyReadonly.value = false;
    // }
  });
}

function getAllCustomerListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      customerList.value = customerListOptions.value;
    } else {
      customerList.value = customerListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all user list for dropdown
const userList = ref([]);
const userListOptions = ref([]);
function getAllUserListByRoleForDropdown () {
  usersService.getSupportTeamUsersDataForDropdown("support team").then((resp) => {
    const responseData = resp.filter(item => item.employeeId)
      .map((item) => ({
        text: `${item.person.firstName} ${item.person.lastName} ${
          item.ticketCounts?.total > 0 ? ` (${item.ticketCounts.total})` : ""
        }`,
        value: item.employeeId
      }))
      .sort((a, b) => a.text.localeCompare(b.text));
    userList.value = responseData;
    userListOptions.value = responseData;
  });
}

function getAllUserListForFilter (val, update, abort, counter) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      userList.value = userListOptions.value;
    } else {
      userList.value = userListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

function isDisabled (statusId) {
  const status = allHelpDeskStatuses.value?.find(item => item.value === statusId)?.text;
  return status;
}

// =========================================================================
// Update methods
// =========================================================================
const withFormLoader = async (
  field,
  apiCall,
  successMessage = "Updated successfully.",
  errorMessage = "Update failed.",
  afterSuccess = null
) => {
  formLoading.value[field] = true;
  await nextTick();
  document.activeElement?.blur();

  try {
    await apiCall();
    notifySuccess({ message: successMessage });

    if (afterSuccess) {
      await afterSuccess();
    }
  } catch (error) {
    notifyError({ message: errorMessage });
  } finally {
    formLoading.value[field] = false;
  }
};

// change status
const onChangeStatus = (id, statusId) => {
  return withFormLoader(
    "status",
    () => helpDeskService.updateHelpDeskStatus(id, statusId),
    "Ticket status updated successfully.",
    "Failed to update status.",
    () => getHelpDesk()
  );
};

// change priority
const onChangePriority = (id, priorityId) => {
  return withFormLoader(
    "priority",
    () => helpDeskService.updateHelpDeskPriority(id, priorityId),
    "Priority updated successfully."
  );
};

// change assignedTo
const onChangeAssignedTo = (id, assignedToId) => {
  return withFormLoader(
    "assignedTo",
    () => helpDeskService.updateAssignedTo(id, assignedToId),
    "Assignment updated successfully.",
    "Failed to update assignment.",
    () => getAllUserListByRoleForDropdown()
  );
};

// change company client
const onChangeCompanyClient = (id, companyId) => {
  return withFormLoader(
    "company",
    () => helpDeskService.updateCompanyClient(id, companyId),
    "Company is saved successfully."
  );
};

// ===========================================================
// Status Flow
// ===========================================================
const allHelpDeskStatuses = ref([]);
const statusListOptions = ref([]);
async function getHelpDeskStatus (typeName) {
  const resp = await commonService.getDropDown(typeName);
  const responseData = resp.map(item => ({
    text: item.dropdownValue,
    value: item.id
  }));
  allHelpDeskStatuses.value = responseData;
  statusListOptions.value = responseData;
}

function getAllStatusListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      allHelpDeskStatuses.value = statusListOptions.value;
    } else {
      allHelpDeskStatuses.value = statusListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

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
  return allHelpDeskStatuses.value
    .find(s => s.value === statusId)
    ?.text ?? null;
}

// options disable/enable
// function getVisibleStatusOptions (row) {
//   const currentStatusText =
//     row.statusText || resolveStatusText(row.statusId);

//   if (!currentStatusText) return true;
//   // const effectiveRole = resolveRoleForRow(row);
//   let effectiveRole = logUserRole;
//   if (
//     logUserRole === "assignedEmployee" &&
//   row.assignedToId !== loginUserEmployeeId
//   ) {
//     effectiveRole = "admin";
//   }

//   const allowed =
//     STATUS_TRANSITIONS?.[effectiveRole]?.[currentStatusText] ?? [];

//   // Always allow current status
//   return allHelpDeskStatuses.value.filter(option => {
//     // Always show current status
//     if (option.text === currentStatusText) return true;

//     // Show only allowed transitions
//     return allowed.includes(option.text);
//   });
// }
function getVisibleStatusOptions (row) {
  const currentStatusText =
    row.statusText || resolveStatusText(row.statusId);

  if (!currentStatusText) {
    return allHelpDeskStatuses.value;
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
  return allHelpDeskStatuses.value.filter(option => {
    // Always display current status
    if (option.text === currentStatusText) return true;

    // Display only allowed transitions
    return allowed.includes(option.text);
  });
}

// Added colors for task status dropdown list
function getStatusColor (statusText) {
  if (statusText) {
    switch (statusText) {
    case "New":
      return "blue-grey-4";
    case "Awaiting Client":
      return "brown-4";
    case "In Progress":
      return "blue-4";
    case "Completed":
      return "green-4";
    case "Open":
      return "purple-4";
    case "Assigned":
      return "deep-purple-2";
    case "Reopen":
      return "orange-2";
    case "Cancelled":
      return "deep-orange-2";
    case "Closed":
      return "grey-4";
    default:
      return "#ffffff";
    }
  }
}
// ===========================================================

// get help desk details
const getHelpDesk = () => {
  loading.value = true;
  helpDeskService.getHelpDesk(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    statusRows.value = resp.helpDeskStatusLog.map(item => ({
      ...item,
      editing: false
    }));
  }).finally(() => {
    loading.value = false;
  });
};

const onPersonView = (id) => {
  $q.dialog({
    component: viewPerson,
    componentProps: { id }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// ================================================================
// Files
// ================================================================
// get files
const getAllFilesByHelpDeskId = (propss) => {
  const projectId = props.id;
  loading.value = true;
  const { page, rowsPerPage, sortBy, descending } = propss.pagination;
  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    projectId
  };
  projectService.getAllFilesByProjectId(payload).then((resp) => {
    filesRows.value = resp.data.map(item => ({
      ...item,
      createdByPersonFullName: item.createdBy?.person?.fullName
    }));
  }).finally(() => {
    loading.value = false;
  });
};

function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
}

function viewFile (file) {
  let fileUrl = "";
  let fileName = "";
  // let mimeType = "";

  if (typeof file === "string") {
    fileUrl = file;
    fileName = file.split("/").pop();
    // isServerFile = true;
  } else if (file?.file?.virtualPath) {
    fileUrl = file.file.virtualPath;
    fileName = file.file.name || file.file.virtualPath.split("/").pop();
    // isServerFile = true;
  } else if (file instanceof File || file?.name) {
    const rawFile = file.file ?? file;
    fileUrl = URL.createObjectURL(rawFile);
    fileName = rawFile.name;
  } else {
    return;
  }

  // const fileUrl = new URL(file, baseURL).href;
  const fileExtension = fileUrl.split(".").pop().toLowerCase();
  const supportedFormats = ["pdf", "docx", "xlsx", "pptx"];
  const imageFormats = ["jpg", "jpeg", "png", "gif", "svg"];

  let viewerUrl = fileUrl;

  // Use Google Docs Viewer for Documents
  if (supportedFormats.includes(fileExtension)) {
    viewerUrl = `https://docs.google.com/gview?url=${encodeURIComponent(fileUrl)}&embedded=true`;
  }

  // Open new window
  const newWindow = window.open("", "_blank");

  // Check if the popup is blocked
  if (!newWindow) {
    alert("Popup blocked! Please allow popups for this site.");
    return;
  }

  // Delay to avoid null reference issues
  setTimeout(() => {
    newWindow.document.write(`
<html>
<head>
<title>${fileName}</title>
<style>
              * { margin: 0; padding: 0; box-sizing: border-box; }
              body, html { width: 100vw; height: 100vh; display: flex; align-items: center; justify-content: center; background-color: #f4f4f4; overflow: hidden; }
              .top-right {
                position: fixed;
                top: 10px;
                right: 10px;
                background: #007bff;
                color: white;
                padding: 10px 15px;
                border-radius: 5px;
                font-size: 16px;
                text-decoration: none;
                z-index: 10;
              }
              .top-right:hover {
                background: #0056b3;
              }

              iframe, img {
                width: 100%;
                height: 100%;
                border: none;
                display: block;
                object-fit: contain; /* Ensures images fit properly */
              }

              /* Responsive Fixes */
              @media (max-width: 768px) {
                .top-right {
                  top: 5px;
                  right: 5px;
                  padding: 8px 12px;
                  font-size: 14px;
                }
              }
</style>
</head>
<body>
<a class="top-right" href="${fileUrl}" download>Download</a>
            ${
  imageFormats.includes(fileExtension)
    ? `<img src="${fileUrl}" alt="Image Preview">` // Show image directly
    : `<iframe src="${viewerUrl}"></iframe>` // Show document using iframe
}
</body>
</html>
        `);
  }, 100);
}

function downloadFile (file) {
  const link = document.createElement("a");
  link.href = file;
  link.download = file.split("/").pop();
  link.click();
}

// Delete record
const onDelete = (item) => {
  zwConfirmDelete({ data: `${item.virtualPath.split("/").pop()}` }, () => {
    projectService.deleteFile(item.id, item.type).then(resp => {
      notifySuccess({ message: "File is deleted successfully." });
      getAllFilesByHelpDeskId({ pagination: statusPagination.value });
    });
  }, () => {
  });
};

watch(helpDeskId, (val) => {
  if (val) {
    getHelpDesk(val);
    showReplyEditor.value = false;
  }
});

// ======================================================================
// On page rendering
onMounted(() => {
  getHelpDesk();
  getHelpDeskStatus("HelpDesk Status");
  // getAllNoteByTypeAndRecord();
  getHelpDeskPriority("HelpDesk Priority");
  getAllCustomerListForDropdown();
  tab.value = props.defaultTab; // set active tab
  const propps = { pagination: filesPagination.value };
  getAllFilesByHelpDeskId(propps);
  getAllUserListByRoleForDropdown();
});

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
