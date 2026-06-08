<template>
  <q-page padding>
    <q-card class="q-pa-sm">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template v-slot:separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <!-- <q-breadcrumbs-el label="Dashboard" icon="o_home" clickable to="/dashboard" /> -->
              <q-breadcrumbs-el label="CRM" />
              <q-breadcrumbs-el label="Company Contact" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs">
                <!-- <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge> -->
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
                        v-model="search.companyId"
                        label="Company Name"
                        :options="companyNameDropdownSingleSelect.list.value"
                        :filter="companyNameDropdownSingleSelect.filter"
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
              <div class="q-ml-sm">
                <q-btn
                  icon="o_add"
                  outline
                  label="Add Contact"
                  no-caps
                  @click="onCompanyContactAdd(refreshCompanyContactList)"
                  class="text-primary btnRounded"
                />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-separator />
      <q-table
        ref="tableRef"
        :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
        flat
        v-model:pagination="pagination"
        :loading="loading"
        :rows="rows"
        :columns="columns"
        row-key="id"
        separator="cell"
        no-data-label="No data available"
        :filter="filter"
        binary-state-sort
        @request="getCompanyContact"
        :rows-per-page-options="[20, 50, 100, 200, 500]"
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
              {{ props.row.company.name }}
            </q-td>
            <q-td>
              {{ props.row.person.firstName }}
            </q-td>
            <q-td>
              {{ props.row.person.lastName }}
            </q-td>
            <q-td>
              {{ props.row.person.primaryEmailAddress }}
            </q-td>
            <q-td>
              {{ props.row.person.primaryPhoneNumber }}
            </q-td>
            <q-td auto-width class="text-center actions">
              <q-icon
                name="o_visibility"
                class="cursor-pointer q-mr-sm"
                @click="onCompanyContactView(props.row.id)"
              >
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                @click="onCompanyContactEdit(props.row.id, refreshCompanyContactList)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <a
                style="position: relative;"
                class="q-icon notranslate cursor-pointer q-ml-sm q-mr-md"
                @click="onNoteAdd(props.row.id, 'Company Contact', props.row.companyId, props.row.company.name, props.row.name, refreshCompanyContactList)"
              >
                <q-tooltip anchor="bottom middle" self="top middle">
                  Note
                </q-tooltip>
                <q-icon name="o_assignment" />
                <q-badge
                  v-if="props.row.companyContactNotesCount > 0"
                  style="position: absolute; right: -16px; top: -15px;"
                  color="green"
                  text-color="white"
                  :label="props.row.companyContactNotesCount"
                />
              </a>
              <q-icon
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                @click="onSubmitCompanyContactDelete(props.row.id, props.row.company.name, refreshCompanyContactList)"
              >
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr><q-separator/>
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onBeforeUnmount, onMounted, computed, watch } from "vue";
import { useAuthStore } from "stores/auth";

import companyContactService from "modules/company-contacts/companyContacts.service";

// Shared Dropdowns
import companyModule from "src/modules/company/utils/dropdowns.js";

// Shared Inputs
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// Shared Company Contact Dialogs
import {
  initCompanyContactDialogs,
  onCompanyContactView,
  onCompanyContactAdd,
  onCompanyContactEdit
} from "src/modules/company-contacts/utils/dialogs.js";

// Shared Notes Dialogs
import {
  initCommonDialogs,
  onNoteAdd
} from "src/modules/common/utils/dialogs.js";

// Shared company contacts Actions
import {
  initCompanyContactActions,
  onSubmitCompanyContactDelete
} from "src/modules/company-contacts/utils/actions.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const loading = ref(true);
const rows = ref([]);
const filter = ref("");
const searchLoader = ref(false);
const showFilter = ref(false);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const columns = ref([
  { name: "company.name", label: "Company", field: "company.name", align: "left", sortable: true },
  { name: "person.firstName", label: "First Name", field: "person.firstName", align: "left", sortable: true },
  { name: "person.lastName", label: "Last Name", field: "person.lastName", align: "left", sortable: true },
  { name: "person.primaryEmailAddress", label: "Email", field: "person.primaryEmailAddress", align: "left", sortable: true },
  { name: "phonenumber", label: "Phone Number", field: "phonenumber", align: "left", sortable: false }
]);

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const authStore = useAuthStore();

const siteId = computed(() => authStore.user?.siteId);

const {
  search,
  pagination,
  activeRowId,
  saveDataTableState
} = useSiteTableState({
  storageKey: "company-Contacts-Index",
  siteId,

  defaultSearch: {
    searchText: "",
    companyId: null
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

const highlightedId = computed(() => activeRowId.value);
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Company contacts
// ----------------------------------------------------------------------------------------------------------------

const getCompanyContact = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  companyContactService.getCompanyContactList(payload).then((resp) => {
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
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

const refreshCompanyContactList = () => {
  getCompanyContact({ pagination: pagination.value });
};

// On Search
const onAdvanceSearch = () => {
  refreshCompanyContactList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.companyId = null;

  saveDataTableState({
    search: search.value
  });
  onAdvanceSearch();
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
// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initCompanyContactDialogs(activeRowId);
initCommonDialogs(activeRowId);
initCompanyContactActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const {
  companyNameDropdownSingleSelect
} = companyModule();

// ----------------------------
// Applied Filter Labels.
// ----------------------------

const mapFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};

  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;

  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.companyId, companyNameDropdownSingleSelect.list, "Company Name")
}));

function onClearFilters (key) {
  if (key === "Company Name") {
    search.value.companyId = null;
  }
  delete appliedFilters.value[key];
  refreshCompanyContactList();
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshCompanyContactList();
});

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

onMounted(() => {
  tableRef.value.requestServerInteraction();
  companyNameDropdownSingleSelect.load();
  document.addEventListener("click", handleDocumentClick);
});

</script>
