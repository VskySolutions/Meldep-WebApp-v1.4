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
              <q-breadcrumbs-el label="Talent Hire" />
              <q-breadcrumbs-el label="Candidates" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center q-ml-lg">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Candidate Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.fullName"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="text"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Email Address</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.emailAddress"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="text"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Mobile No.</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.mobileNumber"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="text"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.appliedWorkLocationId"
                        label="Location"
                        :options="workLocationDropdown.list.value"
                        :filter="workLocationDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="search.jobId"
                        label="Applied Job Position"
                        :options="jobPostNameDropdown.list.value"
                        :filter="jobPostNameDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Applied From Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input
                              v-model="search.fromDate"
                              fill-input
                              dense
                              mask="##/##/####"
                            >
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date
                                      v-model="search.fromDate"
                                      mask="MM/DD/YYYY"
                                      @update:model-value="() => $refs.qDateProxy.hide()"
                                    />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Applied To Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input
                              v-model="search.toDate"
                              fill-input
                              dense
                              mask="##/##/####"
                            >
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date
                                      v-model="search.toDate"
                                      mask="MM/DD/YYYY"
                                      :options="disableBeforeStartDate" @update:model-value="() => $refs.qDateProxy.hide()"
                                    />
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
              <div class="q-ml-xs">
                <q-btn
                  icon="o_add"
                  outline
                  label="Add Candidate"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onCandidateAdd(refreshCandidateList)"
                />
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
        @request="getAllCandidateList"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props"
          :class="highlightedId == props.row.id ? 'highlight' : ''"
          >
            <q-td style="width: 5%;">#{{ props.row.searchNumber }}</q-td>
            <q-td style="width: 20%; white-space: normal;" class="hoverable-cell">
              <div class="row no-wrap items-center justify-between">
                <span style="flex: 1; word-break: break-word; white-space: normal;" @click="onCandidateView(props.row.id, refreshCandidateList)">{{ props.row.person.fullName }}</span>
                <div class="row items-center q-gutter-sm q-ml-sm" style="flex-shrink: 0;">
                  <q-icon
                    name="o_radio_button_checked"
                    size="xs"
                    class="cursor-pointer"
                    @click="setActiveRowIdInLocalStorage(props.row.id); $router.push({ path: '/candidate/candidateCenter', state: { candidateId: props.row.id } })"
                  >
                    <q-tooltip>Candidate Center</q-tooltip>
                  </q-icon>
                </div>
              </div>
             </q-td>
            <q-td style="width: 20%;">
              {{ props.row.person.primaryEmailAddress }}
            </q-td>
            <q-td style="width: 10%;">
              {{ props.row.person.primaryPhoneNumber }}
            </q-td>
            <q-td
              class="common-q-td"
              :class="{ 'hoverable-cell' : activeEdit.rowId === props.row.id }"
              @click="activeEdit = { rowId: props.row.id, field: 'status' }"
              style="width: 15%;"
            >
              <quickEditSingleSelect
                field="status"
                :row-id="props.row.id"
                :value="props.row.status.id"
                :display-value="props.row.status.dropDownValue"
                :editable="activeEdit.rowId === props.row.id"
                :options="candidateStatusDropdown.list.value"
                :active-edit="activeEdit"
                :show-history="false"
                @cancel="activeEdit = { rowId: null, field: null }"
                @submit="({ rowId, value }) => onSubmitCandidateStatus(rowId, value, refreshCandidateList)"
              />
            </q-td>
            <q-td style="width: 20%;">
              {{ props.row.appliedWorkLocations.dropDownValue }}
            </q-td>
            <q-td style="width: 20%;">
              {{ props.row.jobApplyDate }}
            </q-td>
            <q-td style="width: 20%;">
              {{ props.row.job.jobTitle }}
            </q-td>
            <q-td style="width: 20%;">
              {{ props.row.source }}
            </q-td>
            <q-td style="width: 20%;">
              {{ props.row.updatedOnUtc }}
            </q-td>
            <q-td style="width: 10%;" class="text-center actions">
              <q-icon
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                @click="setActiveRowIdInLocalStorage(props.row.id); onCandidateEdit(props.row.id, refreshCandidateList)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <a style="position: relative;"
                class="q-icon notranslate cursor-pointer q-ml-sm q-mr-md"
                @click="setActiveRowIdInLocalStorage(props.row.id); onCandidateAddActivity(props.row.id, refreshCandidateList)"
              >
                <q-tooltip anchor="bottom middle" self="top middle">
                  Add Activity
                </q-tooltip>
                <q-icon name="o_notes" />
                <q-badge
                  v-if="props.row.candidateActivitiesCount > 0"
                  style="position: absolute; right: -16px; top: -15px;"
                  color="green"
                  text-color="white"
                  :label="props.row.candidateActivitiesCount"
                />
              </a>
              <a style="position: relative;"
                class="q-icon notranslate cursor-pointer q-ml-sm q-mr-md"
                @click="setActiveRowIdInLocalStorage(props.row.id); onCandidateAddFeedback(props.row.id, refreshCandidateList)"
              >
                <q-tooltip anchor="bottom middle" self="top middle">
                  Add Candidate Feedback
                </q-tooltip>
                <q-icon name="o_feedback" />
                <q-badge
                  v-if="props.row.candidateFeedbackCount > 0"
                  style="position: absolute; right: -16px; top: -15px;"
                  color="green"
                  text-color="white"
                  :label="props.row.candidateFeedbackCount"
                />
              </a>
              <a style="position: relative;" class="q-icon notranslate cursor-pointer q-ml-sm q-mr-md" @click="setActiveRowIdInLocalStorage(props.row.id); onNoteAdd(props.row.id, 'Candidate', props.row.id, props.row.person.fullName, props.row.person.fullName, refreshCandidateList)">
                <q-tooltip anchor="bottom middle" self="top middle">
                  Note
                </q-tooltip>
                <q-icon name="o_assignment" />
                <q-badge
                  v-if="props.row.candidateNotesCount > 0"
                  style="position: absolute; right: -16px; top: -15px;"
                  color="green"
                  text-color="white"
                  :label="props.row.candidateNotesCount"
                />
              </a>
              <q-icon
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                @click="onSubmitCandidateDelete(props.row.id, props.row.person.fullName, refreshCandidateList)"
              >
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr><q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import { setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";
import { parse } from "date-fns"; // Standard TimeZone Conversion

import candidateService from "modules/candidate/candidate.service";

// Shared DataTable Views
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Dropdowns
import candidateModule from "src/modules/candidate/utils/dropdowns.js";
import jobPostModule from "src/modules/job-post/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Leave Dialogs
import {
  initCandidateDialogs,
  onCandidateView,
  onCandidateAdd,
  onCandidateEdit,
  onCandidateAddActivity,
  onCandidateAddFeedback
} from "src/modules/candidate/utils/dialogs.js";

// Shared Notes Dialogs
import {
  initCommonDialogs,
  onNoteAdd
} from "src/modules/common/utils/dialogs.js";

// Shared Candidate Actions
import {
  initCandidateActions,
  onSubmitCandidateStatus,
  onSubmitCandidateDelete
} from "src/modules/candidate/utils/actions.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const activeEdit = ref({ rowId: null, field: null });

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "TrainingPortal";
const filterLocalStorage = getLocalStorage(localStorageKey);
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshCandidateList = () => {
  getAllCandidateList({ pagination: pagination.value });
};

const highlightCandidateId = filterLocalStorage?.activeRowId || null;
const activeRowId = ref(highlightCandidateId);
const highlightedId = computed(() => {
  return activeRowId.value;
});

// set active row using id
function setActiveRowIdInLocalStorage (id) {
  const storedData = getLocalStorage(localStorageKey) || {};
  setLocalStorage(localStorageKey, { ...storedData, activeRowId: id });
  activeRowId.value = id;
}

const disableBeforeStartDate = (date) => {
  if (!search.value.fromDate) {
    return true;
  }

  // Convert MM/dd/yyyy string to Date
  const start = parse(search.value.fromDate, "MM/dd/yyyy", new Date());
  const currentDate = parse(date, "yyyy/MM/dd", new Date());

  // Disable dates before the Start Date
  return currentDate >= start;
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "searchNumber", label: "Number", field: "searchNumber", align: "left", sortable: true },
  { name: "person.fullName", label: "Candidate Name", field: "person.fullName", align: "left", sortable: true },
  { name: "person.primaryEmailAddress", label: "Email Id", field: "person.primaryEmailAddress", align: "left", sortable: true },
  { name: "person.primaryPhoneNumber", label: "Mobile No.", field: "person.primaryPhoneNumber", align: "left", sortable: true },
  { name: "status.dropDownValue", label: "Status", field: "status.dropDownValue", align: "left", sortable: true },
  { name: "appliedWorkLocations.dropDownValue", label: "Location", field: "appliedWorkLocations.dropDownValue", align: "left", sortable: true },
  { name: "jobApplyDate", label: "Application Date", field: "jobApplyDate", align: "left", sortable: true },
  { name: "job.jobTitle", label: "Applied Position", field: "job.jobTitle", align: "left", sortable: true },
  { name: "source", label: "Source", field: "source", align: "left", sortable: true },
  { name: "updatedOnUtc", label: "Updated On", field: "updatedOnUtc", align: "left", sortable: true }
]);

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initCandidateDialogs(activeRowId);
initCommonDialogs(activeRowId);
initCandidateActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

// Search variables
const getFilterValue = (key, defaultValue) => {
  const val = filterLocalStorage?.[key];
  return val && val.length > 0 ? val : defaultValue;
};

const search = ref({
  searchText: getFilterValue("searchText", ""),
  fullName: getFilterValue("fullName", ""),
  emailAddress: getFilterValue("emailAddress", ""),
  mobileNumber: getFilterValue("mobileNumber", ""),
  appliedWorkLocationId: getFilterValue("appliedWorkLocationId", []),
  fromDate: getFilterValue("fromDate", ""),
  toDate: getFilterValue("toDate", ""),
  jobId: getFilterValue("jobId", [])
});

const onAdvanceSearch = () => {
  refreshCandidateList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.fullName = "";
  search.value.emailAddress = "";
  search.value.mobileNumber = "";
  search.value.appliedWorkLocationId = [];
  search.value.fromDate = null;
  search.value.toDate = null;
  search.value.jobId = [];
  clearLocalStorage(localStorageKey);
  onAdvanceSearch();
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Candidates
// ----------------------------------------------------------------------------------------------------------------

const getAllCandidateList = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  if (search.value.toDate === "") {
    search.value.toDate = null;
  }
  if (search.value.fromDate === "") {
    search.value.fromDate = null;
  }
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  candidateService.getAllCandidateList(payload).then((resp) => {
    rows.value = resp.data;
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

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------

const { workLocationDropdown, candidateStatusDropdown } = candidateModule();
const { jobPostNameDropdown } = jobPostModule();

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

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
  ...mapFilterToLabel(search.value.appliedWorkLocationId, workLocationDropdown.list, "Location"),
  ...mapFilterToLabel(search.value.jobId, jobPostNameDropdown.list, "Applied Job Position"),
  ...(search.value.fullName ? { "Candidate Name": search.value.fullName } : {}),
  ...(search.value.emailAddress ? { "Email Address": search.value.emailAddress } : {}),
  ...(search.value.mobileNumber ? { "Mobile No.": search.value.mobileNumber } : {}),
  ...(search.value.fromDate ? { "Applied From Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "Applied To Date": search.value.toDate } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Location": return search.value.appliedWorkLocationId?.length || 0;
  case "Applied Job Position": return search.value.jobId?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Location") {
    search.value.appliedWorkLocationId = [];
  } else if (key === "Applied Job Position") {
    search.value.jobId = [];
  } else if (key === "Candidate Name") {
    search.value.fullName = "";
  } else if (key === "Email Address") {
    search.value.emailAddress = "";
  } else if (key === "Mobile No.") {
    search.value.mobileNumber = "";
  } else if (key === "Applied From Date") {
    search.value.fromDate = "";
  } else if (key === "Applied To Date") {
    search.value.toDate = "";
  }
  delete appliedFilters.value[key];
  refreshCandidateList();
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshCandidateList();
});

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  tableRef.value.requestServerInteraction();
  jobPostNameDropdown.load();
  workLocationDropdown.load("Employee OrgLocation");
  candidateStatusDropdown.load("Candidate Status");
  refreshCandidateList();

  if (!activeRowId.value && highlightCandidateId) {
    activeRowId.value = highlightCandidateId;
  }
});

</script>
