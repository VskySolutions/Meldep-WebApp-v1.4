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
              <q-breadcrumbs-el label="Settings" clickable to="/Settings" />
              <q-breadcrumbs-el label="Share My Tenant" />
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
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <multiSelectDropdown
                        v-model="search.personIds"
                        label="Person Name"
                        :options="isSharedPersonNameForDropdown.list.value"
                        :filter="isSharedPersonNameForDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel fs-13">Email</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.primaryEmailAddress"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="email"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm hidden">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel fs-13">User Status</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.userStatus"
                            clearable
                            class="q-mx-sm w-100 h-auto"
                            stack-label
                            hide-bottom-space
                            use-input
                            :dense="true"
                            :options="userStatusList"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel fs-13">Is Shared</label>
                        </div>

                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-checkbox
                            v-model="search.isSharedUser"
                            class="q-mx-sm"
                            :dense="true"
                          />
                        </div>
                      </div>
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn
                          style="width: 20%;"
                          outline
                          color="primary"
                          label="Search"
                          class="btnRounded"
                          no-caps
                          @click="() => { showFilter = false; onAdvanceSearch(); }"
                        />
                        <q-btn
                          style="width: 20%;"
                          outline
                          color="grey-4"
                          label="Clear"
                          class="text-grey-9 btnRounded"
                          no-caps
                          @click="onAdvanceClear"
                        />
                        <q-btn
                          style="width: 20%;"
                          outline
                          color="negative"
                          label="Close"
                          class="btnRounded"
                          no-caps
                          @click="() => { showFilter = false; }"
                        />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
              <div class="q-ml-sm">

                <addUserPopup
                  :on-save-api="saveSiteShareDetails"
                  :siteId= "siteId"
                />

                <q-btn
                  icon="o_chevron_left"
                  outline
                  label="Back"
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
                  @click="$router.back()"
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
        :rows-per-page-options="[15, 30, 50 ,100]"
        @request="refreshSiteShareList"
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
              {{ props.row.person.fullName }}
            </q-td>
            <q-td>
              {{ props.row.person.primaryEmailAddress }}
            </q-td>
            <q-td>
              {{ props.row.createdBy.person.fullName }}
            </q-td>
            <q-td>
              {{ props.row.createdOnUtc }}
            </q-td>
            <q-td auto-width class="text-left actions">
              <q-icon
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                size="xs"
                @click="onSubmitSiteShareDelete(props.row.id, props.row.person.fullName, refreshSiteShareList)"
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
import { ref, onMounted, watch, computed } from "vue";
import { notifySuccess} from "assets/utils";
import { useAuthStore } from "stores/auth";

import siteShareService from "modules/sites-sharing/sitesSharing.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// Shared Dropdowns
import personModule from "src/modules/person/utils/dropdowns.js";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import addUserPopup from "modules/sites-sharing/components/_addUserPopup.vue";

// Shared DataTable Views
import useSiteTableState from "composables/dataTable/useSiteTableState.js";
// Shared Site Share Actions
import {
  initSiteShareActions,
  onSubmitSiteShareDelete
} from "src/modules/sites-sharing/utils/actions.js";

// ---------------------------------------------------------------------------------------------------
// Common variables
// ---------------------------------------------------------------------------------------------------

const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const userStatusList = ref(["Active", "Inactive"]);
const siteId = ref(history.state?.siteId);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "person.fullName", label: "Person Name", field: "person.fullName", align: "left", sortable: true },
  { name: "person.primaryEmailAddress", label: "Email", field: "person.primaryEmailAddress", align: "left", sortable: true },
  { name: "createdBy.person.fullName", label: "Created By", field: "createdById", align: "left", sortable: true },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// get Person details and map
// ----------------------------------------------------------------------------------------------------------------

const getAllSiteShares = (props) => {
  loading.value = true;

  const { page, rowsPerPage, sortBy, descending } = props.pagination;

  const payload = {    
    siteId: siteId.value ? siteId.value : null,
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    ...search.value
  };

  siteShareService.getAllSiteShares(payload)
    .then((resp) => {
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
    })
    .finally(() => {
      loading.value = false;
      searchLoader.value = false;
    });
};
// ---------------------------------------------------------
// Site-wise DataTable and Search State
// ---------------------------------------------------------

const authStore = useAuthStore();
const currentSiteId = computed(() => authStore.user.siteId);

const {
  search,
  pagination,
  activeRowId,
  saveDataTableState
} = useSiteTableState({
  storageKey: "shareMyTenant-Index",
  siteId: currentSiteId,
  defaultSearch: {
    searchText: "",
    personIds: [],
    primaryEmailAddress: "",
    userStatus: "Active",
    isSharedUser: true
  },
  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initSiteShareActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

const refreshSiteShareList = () => {
  getAllSiteShares({ pagination: pagination.value });
}

// Search records as per parameters
const onAdvanceSearch = () => {
  saveDataTableState();
  refreshSiteShareList();
};

// Clear advance search filters
const onAdvanceClear = () => {
  search.value.personIds = [];
  search.value.primaryEmailAddress = "";
  search.value.userStatus = "Active";
  search.value.isSharedUser = true;

  pagination.value.page = 1;

  saveDataTableState({
    search: search.value
  });
  onAdvanceSearch();
};
// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------

const { isSharedPersonNameForDropdown } = personModule();

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
  ...mapFilterToLabel(search.value.personIds, isSharedPersonNameForDropdown.list, "Person Name"),
  ...(search.value.primaryEmailAddress ? { Email: search.value.primaryEmailAddress } : {}),
  ...mapSingleFilterToLabel(search.value.userStatus, userStatusList, "User Status"),
   ...(search.value.isSharedUser !== null
    ? { "Is Shared": search.value.isSharedUser ? "Yes" : "No" }
    : {})
}));

function onClearFilters(key) {
  if (key === "Person Name") {
    search.value.personIds = [];
  } else if (key === "Email") {
    search.value.primaryEmailAddress = "";
  } else if (key === "User Status") {
    search.value.userStatus = null;
  } else if (key === "Is Shared") {
    search.value.isSharedUser = null;
  }

  pagination.value.page = 1;

  saveDataTableState();
  refreshSiteShareList();
}

function getFilterCount (key) {
  switch (key) {
  default: return null;
  }
}

// --------------------------------------------------------------------------------------------------------------------
// save Site Share User
// --------------------------------------------------------------------------------------------------------------------

function saveSiteShareDetails (payload) {
  return siteShareService
    .saveSiteShare(payload)
    .then(() => {
      notifySuccess({ message: "User added successfully." });
      refreshSiteShareList();
    });
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  saveDataTableState();
  refreshSiteShareList();
});
// ----------------------------
// On page rendering
// ----------------------------

onMounted(() => {
  isSharedPersonNameForDropdown.load();
  tableRef.value.requestServerInteraction();
});

</script>
