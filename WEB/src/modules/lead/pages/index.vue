<template>
  <q-page padding>
    <q-card class="q-pa-sm">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="CRM" />
              <q-breadcrumbs-el label="Lead" />
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
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <!-- Dropdown Content -->
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <singleSelectDropdown
                        v-model="search.personId"
                        label="Lead"
                        :options="leadNameDropdown.list.value"
                        :filter="leadNameDropdown.filter"
                      />
                      <singleSelectDropdown
                        v-model="search.companyId"
                        label="Company Name"
                        :options="companyNameDropdownSingleSelect.list.value"
                        :filter="companyNameDropdownSingleSelect.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.leadGroupIds"
                        label="Group Name"
                        :options="leadGroupsDropdown.list.value"
                        :filter="leadGroupsDropdown.filter"
                      />
                      <singleSelectDropdown
                        v-model="search.leadSourceId"
                        label="Lead Source"
                        :options="leadSourceDropdown.list.value"
                        :filter="leadSourceDropdown.filter"
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
              <div class="q-ml-sm">
                <q-btn icon="o_add" outline label="Add Lead" no-caps class="text-primary btnRounded" @click="onLeadAdd(refreshLeadNameDropdown, refreshLeadList)" />
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_playlist_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
                  @click="showManageDropdownOptions = !showManageDropdownOptions"
                >
                  <q-tooltip>Manage Dropdowns</q-tooltip>
                </q-btn>
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
        @request="getLeads"
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
          <q-tr :props="props" :class="highlightedId == props.row.id ? 'highlight' : ''">
            <q-td>
              {{ props.row.person.fullName }}
            </q-td>
            <q-td>
              {{ props.row.company.name }}
            </q-td>
            <q-td>
              {{ props.row.leadGroup.dropDownValue }}
            </q-td>
            <q-td>
              {{ props.row.leadSources.dropDownValue }}
            </q-td>
            <q-td>
              {{ props.row.person.primaryPhoneNumber }}
            </q-td>
            <q-td>
              {{ props.row.person.primaryEmailAddress }}</q-td>
            <q-td class="text-center" style="width: 5%;">
              {{ props.row.leadArrivalDate }}
            </q-td>
            <q-td auto-width class="text-center actions">
              <a
                style="position: relative;"
                color="negative"
                class="q-icon notranslate cursor-pointer q-mr-md"
                @click="onLeadAddActivity(props.row.id, refreshLeadList)"
              >
                <q-tooltip>Add Activity</q-tooltip>
                <q-badge style="position: absolute;right: -16px;top: -15px;" color="green" text-color="white" :class="props.row.leadActivityLogs.length == 0 ? 'hidden': ''" :label="props.row.leadActivityLogs.length" />
                <i class="fas fa-plus" />
              </a>
              <q-icon
                name="o_visibility"
                class="cursor-pointer q-mr-sm"
                @click="onLeadView(props.row.id)"
              >
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                @click="onLeadEdit(props.row.id, refreshLeadList)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <a
                style="position: relative;"
                class="q-icon notranslate cursor-pointer q-ml-sm q-mr-md"
                @click="onNoteAdd(props.row.id, 'Lead', props.row.id, props.row.name, props.row.name, refreshLeadList)"
              >
                <q-tooltip anchor="bottom middle" self="top middle">
                  Note
                </q-tooltip>
                <q-icon name="o_assignment" />
                <q-badge
                  v-if="props.row.leadNotesCount > 0"
                  style="position: absolute; right: -16px; top: -15px;"
                  color="green"
                  text-color="white"
                  :label="props.row.leadNotesCount"
                />
              </a>
              <q-icon
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                @click="onSubmitLeadDelete(props.row.id, props.row.person.fullName, refreshLeadList)"
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
import { ref, onBeforeUnmount, onMounted, computed, watch } from "vue";
import { useAuthStore } from "stores/auth";

import leadsService from "modules/lead/lead.service";
import manageDropdownsService from "modules/dropdown/dropdown.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// Shared Dropdowns
import leadModule from "src/modules/lead/utils/dropdowns.js";
import companyModule from "src/modules/company/utils/dropdowns.js";

// Shared Inputs
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Notes Dialogs
import {
  initLeadDialogs,
  onLeadView,
  onLeadAdd,
  onLeadEdit,
  onLeadAddActivity
} from "src/modules/lead/utils/dialogs.js";

// Shared Notes Dialogs
import {
  initCommonDialogs,
  onNoteAdd
} from "src/modules/common/utils/dialogs.js";

// Shared Lead Actions
import {
  initLeadActions,
  onSubmitLeadDelete
} from "src/modules/lead/utils/actions.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const loading = ref(true);
const rows = ref([]);
const searchLoader = ref(false);
const showFilter = ref(false);
const dropdownTypes = ref([]);
const showManageDropdownOptions = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";
const siteId = computed(() => authStore.user?.siteId);

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const {
  search,
  pagination,
  activeRowId,
  saveDataTableState
} = useSiteTableState({
  storageKey: "lead-Index",
  siteId,
  defaultSearch: {
    searchText: "",
    personId: null,
    companyId: null,
    leadSourceId: null,
    leadGroupIds: []
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

const highlightedId = computed(() => {
  return activeRowId.value;
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const columns = ref([
  { name: "personId", label: "Lead", field: "personId", align: "left", sortable: true },
  { name: "companyId", label: "Company", field: "companyId", align: "left", sortable: true },
  { name: "leadGroup.dropDownValue", label: "Group Name", field: "leadGroup.dropDownValue", align: "left", sortable: true },
  { name: "leadSourceId", label: "Lead Source", field: "leadSourceId", align: "left", sortable: true },
  { name: "person.primaryPhoneNumber", label: "Phone Number", field: "person.primaryPhoneNumber", align: "left", sortable: true },
  { name: "person.primaryEmailAddress", label: "Email", field: "person.primaryEmailAddress", align: "left", sortable: true },
  { name: "leadArrivalDate", label: "Lead Arrival Date", field: "leadArrivalDate", align: "center", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Leads
// ----------------------------------------------------------------------------------------------------------------

const getLeads = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  leadsService.getLeads(payload).then((resp) => {
    rows.value = resp.data;
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
    saveDataTableState({
      search: search.value,
      pagination: props.pagination,
      activeRowId: activeRowId.value
    });
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

function getDropdownTypeByModuleName (moduleName) {
  manageDropdownsService.getDropdownTypeByModuleName(moduleName).then((resp) => {
    dropdownTypes.value = resp;
  });
}

// ----------------------------------------------------------------------------------------------------------------
// Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshLeadList = () => {
  getLeads({ pagination: pagination.value });
};

const refreshLeadNameDropdown = () => {
  leadNameDropdown.load();
};

const handleDocumentClick = (event) => {
  if (event.target.closest(".q-dialog")) {
    return;
  }

  const highlightElement = document.querySelector(".highlight");

  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;

    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: null
    });
  }
};
// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

const onAdvanceSearch = () => {
  refreshLeadList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.personId = null;
  search.value.companyId = null;
  search.value.leadSourceId = null;
  search.value.leadGroupIds = [];
  saveDataTableState({
    search: search.value,
    pagination: pagination.value,
    activeRowId: null
  });
  onAdvanceSearch();
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initLeadDialogs(activeRowId);
initCommonDialogs(activeRowId);
initLeadActions(activeRowId);

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

const mapSingleFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapSingleFilterToLabel(search.value.personId, leadNameDropdown.list, "Lead"),
  ...mapSingleFilterToLabel(search.value.companyId, companyNameDropdownSingleSelect.list, "Company Name"),
  ...mapFilterToLabel(search.value.leadGroupIds, leadGroupsDropdown.list, "Group Name"),
  ...mapSingleFilterToLabel(search.value.leadSourceId, leadSourceDropdown.list, "Lead Source")
}));

function onClearFilters (key) {
  if (key === "Lead") {
    search.value.personId = null;
  } else if (key === "Company Name") {
    search.value.companyId = null;
  } else if (key === "Group Name") {
    search.value.leadGroupIds = [];
  } else if (key === "Lead Source") {
    search.value.leadSourceId = null;
  }
  delete appliedFilters.value[key];
  refreshLeadList();
}

function getFilterCount (key) {
  switch (key) {
  case "Group Name": return search.value.leadGroupIds?.length || 0;
  default: return null;
  }
}
// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const {
  leadNameDropdown,
  leadSourceDropdown,
  leadGroupsDropdown
} = leadModule();

const {
  companyNameDropdownSingleSelect
} = companyModule();

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshLeadList();
});

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

onMounted(() => {
  tableRef.value.requestServerInteraction();
  leadNameDropdown.load();
  companyNameDropdownSingleSelect.load();
  leadSourceDropdown.load("Lead Sources");
  getDropdownTypeByModuleName("CRM");
  leadGroupsDropdown.load("Lead Group");
  document.addEventListener("click", handleDocumentClick);
});
</script>
