<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Leads" clickable to="/Lead" />
              <q-breadcrumbs-el label="Lead Group User Assignment" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-4">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <q-separator />
          <div class="col-12 col-md-5">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container" style="position: relative; display: flex; align-items: center; width: 320px;">
                  <q-input v-model="search.searchText" :loading="searchLoader" outlined dense clearable debounce="300" placeholder="Search" class="bg-white search-box" style="flex: 1; border-top-right-radius: 0; border-bottom-right-radius: 0; border-top-right-radius: 0; max-width: 250px;">
                    <template #prepend>
                      <q-icon name="o_search" />
                    </template>
                  </q-input>
                  <q-btn
                    unelevated :color="Object.keys(appliedFilters).length > 0 ? 'primary' : 'grey-7'" text-color="white" size="" class="q-pa-xs q-mr-xs filter-btn" style="height: 40px; border-top-left-radius: 0; border-bottom-left-radius: 0;"
                    @click.stop="showFilter = !showFilter"
                  >
                    <q-badge v-if="Object.keys(appliedFilters).length > 0" color="green" floating>{{ Object.keys(appliedFilters).length }}</q-badge>
                    <q-icon name="o_filter_alt" size="sm" color="white" class="q-mr-xs" />
                    <q-item-label class="text-xs fs-12">
                      <span class="block">Set/Clear</span>
                      <span class="block">FILTER</span>
                    </q-item-label>
                    <q-tooltip anchor="bottom middle" self="top middle">Advanced Filter</q-tooltip>
                  </q-btn>
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <multiSelectDropdown
                        v-model="search.userIds"
                        label="User Name"
                        :options="allUsersForDropdown.list.value"
                        :filter="allUsersForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.leadGroupIds"
                        label="Lead Group Name"
                        :options="leadGroupsDropdown.list.value"
                        :filter="leadGroupsDropdown.filter"
                      />
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
              <!-- SOP Change -->
              <manageDropdownOptions
                v-model="showManageDropdownOptions"
                :manage-drop-down-types="manageDropDownTypes"
                :selected-field="selectedField"
              />
              <div class="q-ml-xs">
                <q-btn
                  icon="o_add"
                  outline
                  label="Assign Users to Lead Groups"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onLeadGroupAdd(refreshLeadGroupAssignedUsersList)"
                />
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
                <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-sm" @click="$router.back()" />
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
        @request="getUsersWithAssignedLeadGroups"
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
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td>
              <span v-if="props.row.showUserName">
                {{ props.row.user.person.fullName }}
              </span>
            </q-td>
            <q-td>{{ props.row.leadGroup.dropDownValue }}</q-td>
            <q-td auto-width class="text-center actions">
              <q-icon
                :name="props.row.active ? 'o_check_circle' : 'o_block'"
                :color="props.row.active ? 'positive' : 'negative'"
                class="q-mr-xs hoverable-cell"
                @click="() => { onSubmitLeadGroupStatus(props.row.id, props.row.active, refreshLeadGroupAssignedUsersList) }"
              >
                <q-tooltip>{{ props.row.active ? 'Set Inactive?' : 'Set Active?' }}</q-tooltip>
              </q-icon>
              <q-icon
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                @click="() => { onSubmitLeadGroupUserDelete(props.row.id, props.row.user.person.fullName, props.row.leadGroup.dropDownValue, refreshLeadGroupAssignedUsersList) }"
              >
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
          <q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
import { useAuthStore } from "stores/auth";
import { ref, onMounted, watch, computed } from "vue";

import leadService from "modules/lead/lead.service";

// Shared Dropdowns
import userModule from "src/modules/user-management/utils/dropdowns.js";
import leadModule from "src/modules/lead/utils/dropdowns.js";
import manageDropdownModule from "src/modules/dropdown/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import manageDropdownOptions from "src/components/dataTable/_manageDropdownOptions.vue";

// Shared DataTable Features
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Project Dialogs
import {
  initLeadDialogs,
  onLeadGroupAdd
} from "src/modules/lead/utils/dialogs.js";

// SOP Change :- Shared Project Actions
import {
  initLeadActions,
  onSubmitLeadGroupStatus,
  onSubmitLeadGroupUserDelete
} from "src/modules/lead/utils/actions.js";

// ----------------------------
// Common variables
// ----------------------------
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const showManageDropdownOptions = ref(false);
const manageDropDownTypes = ref([]);

const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";
// ----------------------------
// local storage values
// ----------------------------
const siteId = computed(() => authStore.user?.siteId);

const {
  search,
  pagination,
  activeRowId,
  saveDataTableState
} = useSiteTableState({
  storageKey: "lead-User-Group-Index",
  siteId,
  defaultSearch: {
    searchText: "",
    userIds: [],
    leadGroupIds: []
  },
  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

// ----------------------------
// Table variables
// ----------------------------
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "user.person.firstName", label: "User Name", field: "user.person.firstName", align: "left", sortable: true },
  { name: "leadGroup.dropDownValue", label: "Lead Group Name", field: "leadGroup.dropDownValue", align: "left", sortable: true }
]);

const getUsersWithAssignedLeadGroups = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  saveDataTableState({
    search: search.value,
    pagination: props.pagination
  });
  leadService.getUsersWithAssignedLeadGroups(payload).then((resp) => {
    let lastUserName = null;
    const Rows = resp.data
      // .sort((a, b) => a.user?.person?.fullName?.localeCompare(b.user?.person?.fullName || ""))
      .map(row => {
        const showUserName = row.user?.person?.fullName !== "" &&
        row.user?.person?.fullName !== lastUserName;
        lastUserName = row.user?.person?.fullName;

        return {
          ...row,
          showUserName
        };
      });
    rows.value = Rows;
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

const refreshLeadGroupAssignedUsersList = () => {
  getUsersWithAssignedLeadGroups({ pagination: pagination.value });
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const { allUsersForDropdown } = userModule();
const { leadGroupsDropdown } = leadModule();
const { getDropdownTypesByModuleNameForDropdown } = manageDropdownModule();

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------

initLeadDialogs(activeRowId);
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

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.userIds, allUsersForDropdown.list, "User Name"),
  ...mapFilterToLabel(search.value.leadGroupIds, leadGroupsDropdown.list, "Lead Group Name")
}));

// ----------------------------
// Search records as per parameters
// ----------------------------

const onSearch = () => {
  refreshLeadGroupAssignedUsersList();
};

// ----------------------------
// Clear search
// ----------------------------
const onClear = () => {
  search.value.userIds = [];
  search.value.leadGroupIds = [];
  saveDataTableState({
    search: search.value
  });
  onSearch();
};

function onClearFilters (key) {
  if (key === "User Name") {
    search.value.userIds = [];
  } else if (key === "Lead Group Name") {
    search.value.leadGroupIds = [];
  }

  refreshLeadGroupAssignedUsersList();
  saveDataTableState({
    search: search.value,
    pagination: pagination.value
  });
}

function getFilterCount (key) {
  switch (key) {
  case "User Name": return search.value.userIds?.length || 0;
  case "Lead Group Name": return search.value.leadGroupIds?.length || 0;
  default: return null;
  }
}
// ----------------------------
// On page rendering
// ----------------------------
onMounted(async () => {
  tableRef.value.requestServerInteraction();
  allUsersForDropdown.load(user.siteId);
  leadGroupsDropdown.load("Lead Group");

  // Admin:- Manage all Lead Dropdowns and Types
  manageDropDownTypes.value = await getDropdownTypesByModuleNameForDropdown("Lead Management");
});

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => search.value.searchText, () => {
  if (search.value.searchText) {
    searchLoader.value = true;
  }

  refreshLeadGroupAssignedUsersList();
});
</script>
