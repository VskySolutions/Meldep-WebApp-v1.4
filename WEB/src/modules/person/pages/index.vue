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
              <q-breadcrumbs-el label="User Management" />
              <q-breadcrumbs-el label="Person" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
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
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">First Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.firstName"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Last Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.lastName"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Phone Number</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.primaryPhoneNumber"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Identified From Date</label>
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
                          <label class="Cutomlabel q-mt-sm fs-13">Identified To Date</label>
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
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Country</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.countryId"
                            use-input
                            class="q-mx-sm w-100 h-auto"
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            :options="countryList"
                            option-value="id"
                            option-label="name"
                            emit-value
                            map-options
                            @filter="getCountryListForFilter"
                          >
                            <template #option="{ itemProps, opt }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center selection-drodown-item">
                                    <span>{{ opt.name }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">State</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.stateProvinceId"
                            push
                            use-input
                            class="q-mx-sm w-100 h-auto"
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            :options="stateList"
                            :disable="!search.countryId"
                            option-value="id"
                            option-label="name"
                            emit-value
                            map-options
                            @filter="getStateListForFilter"
                          >
                            <template #option="{ itemProps, opt }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center selection-drodown-item">
                                    <span>{{ opt.name }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">City</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.city"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                          />
                        </div>
                      </div>
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
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Add Person" no-caps class="text-primary btnRounded" @click="onAdd" />
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
        :visible-columns="visibleColumns"
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        @request="getPersons"
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
            <q-td>{{ props.row.firstName }}</q-td>
            <q-td>{{ props.row.lastName }}</q-td>
            <q-td>{{ props.row.primaryEmailAddress }}</q-td>
            <q-td style="width: 10%;">{{ props.row.primaryPhoneNumber }}</q-td>
            <q-td>{{ props.row.identifiedBy ? props.row.identifiedBy.fullName : "" }}</q-td>
            <q-td class="text-center" style="width: 5%;">{{ toDate(props.row.identifiedDate) }}</q-td>
            <q-td auto-width class="text-left actions">
              <q-icon
                name="o_visibility"
                class="cursor-pointer q-mr-sm"
                @click="onView(props.row.id)"
              >
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                :class="['q-mr-sm',storedUser === props.row.id ? 'text-grey-5 cursor-not-allowed' : 'cursor-pointer']"
                @click="storedUser !== props.row.id && onEdit(props.row.id)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon
                v-if="!props.row.isCustomer"
                name="o_swap_horiz"
                class="cursor-pointer q-mr-sm"
                @click="onConvertToCustomer(props.row.id)"
              >
                <q-tooltip>Convert To Customer</q-tooltip>
              </q-icon>
              <q-icon
                v-if="!props.row.isCustomer"
                name="o_delete_outline"
                class="cursor-pointer"
                color="negative"
                @click="onDelete(props.row)"
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
import { ref, onBeforeUnmount, onMounted, computed, watch } from "vue";
import { useQuasar } from "quasar";
import { useAuthStore } from "stores/auth";
import { zwConfirmDelete, notifySuccess, notifyWarning } from "assets/utils";
import useFilters from "composables/useFilters";

import editPerson from "modules/person/components/addEdit.vue";
import viewPerson from "modules/person/components/view.vue";

import personService from "modules/person/person.service";
import commonService from "services/common.service";

// Shared DataTable Views
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// ----------------------------
// Common variables
// ----------------------------
const { toDate } = useFilters();
const $q = useQuasar();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const authStore = useAuthStore();
const siteId = computed(() => authStore.user?.siteId);
const storedUser = computed(() => authStore.user?.personId);
// Visible columns
const visibleColumns = ref(["firstName", "lastName", "primaryEmailAddress", "primaryPhoneNumber", "identifiedById", "identifiedDate"]);

// ----------------------------
// local storage values
// ----------------------------

const {
  search,
  pagination,
  activeRowId,
  saveDataTableState
} = useSiteTableState({
  storageKey: "person-Index",
  siteId,
  defaultSearch: {
    searchText: "",
    firstName: "",
    lastName: "",
    primaryPhoneNumber: "",
    fromDate: "",
    toDate: "",
    countryId: "",
    stateProvinceId: "",
    identifiedDate: "",
    city: ""
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
  { name: "firstName", label: "First Name", field: "firstName", align: "left", sortable: true },
  { name: "lastName", label: "Last Name", field: "lastName", align: "left", sortable: true },
  { name: "primaryEmailAddress", label: "Email Address", field: "primaryEmailAddress", align: "left", sortable: true },
  { name: "primaryPhoneNumber", label: "Phone Number", field: "primaryPhoneNumber", align: "left", sortable: true },
  { name: "identifiedById", label: "Identified By", field: "identifiedById", align: "left", sortable: true },
  { name: "identifiedDate", label: "Identified Date", field: "identifiedDate", align: "center", sortable: true }
]);

// ----------------------------
// Get/Map person list to table
// ----------------------------
const getPersons = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  search.value.toDate = search.value.toDate === "" ? null : toDate(search.value.toDate);
  if (search.value.toDate === "") {
    search.value.toDate = null;
  }
  search.value.fromDate = search.value.fromDate === "" ? null : toDate(search.value.fromDate);
  if (search.value.fromDate === "") {
    search.value.fromDate = null;
  }
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  personService.getPersons(payload).then((resp) => {
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
// ----------------------------
// Search records as per parameters
// ----------------------------
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getPersons(propps);
};

// ----------------------------
// Clear search
// ----------------------------
const onClear = () => {
  search.value.firstName = "";
  search.value.lastName = "";
  search.value.primaryPhoneNumber = "";
  search.value.countryId = "";
  search.value.stateProvinceId = "";
  search.value.city = "";
  search.value.fromDate = null;
  search.value.toDate = null;
  saveDataTableState({
    search: search.value
  });
  onSearch();
};

// ----------------------------
// Get all country list for dropdown
// ----------------------------
const countryList = ref([]);
const countryListFilter = ref([]);
function getAllCountryListForDropdown () {
  commonService.getCountries().then((resp) => {
    countryList.value = resp;
    countryListFilter.value = resp;
  });
}

// ----------------------------
// Search country for dropdown
// ----------------------------
function getCountryListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      countryList.value = countryListFilter.value;
    } else {
      countryList.value = countryListFilter.value.filter(v => v.name.toLowerCase().includes(needle));
    }
  });
}

// ----------------------------
// Get all state list for dropdown
// ----------------------------
const stateList = ref([]);
const stateListFilter = ref([]);
function getAllStateListForDropdown () {
  commonService.getStates(search.value.countryId).then((resp) => {
    stateList.value = resp;
    stateListFilter.value = resp;
  });
}

// ----------------------------
// Search state for dropdown
// ----------------------------
function getStateListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      stateList.value = stateListFilter.value;
    } else {
      stateList.value = stateListFilter.value.filter(v => v.name.toLowerCase().includes(needle));
    }
  });
}

// ----------------------------
// Create popup
// ----------------------------
const onAdd = () => {
  $q.dialog({
    component: editPerson,
    componentProps: {}
  }).onOk(() => {
    getPersons({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// ----------------------------
// Edit popup
// ----------------------------
const onEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: editPerson,
    componentProps: { id }
  }).onOk(() => {
    getPersons({ pagination: pagination.value });
  }).onCancel(() => {
    activeRowId.value = id;
  }).onDismiss(() => {
    activeRowId.value = id;
  });
};

// ----------------------------
// View popup
// ----------------------------
const onView = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewPerson,
    componentProps: { id }
  }).onOk(() => {
    getPersons();
  }).onCancel(() => {
    activeRowId.value = id;
  }).onDismiss(() => {
    activeRowId.value = id;
  });
};

// ----------------------------
// Delete record
// ----------------------------
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.fullName}` }, () => {
    personService.deletePerson(item.id).then(resp => {
      notifySuccess({ message: "Person is deleted successfully." });
      getPersons({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};

// ----------------------------
// Convert person to customer
// ----------------------------
function onConvertToCustomer (id) {
  $q.dialog({
    title: "Convert Person To Customer",
    message: "Do you want this person convert into the customer?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    // Send
    personService.convertPersonToCustomer(id)
      .then(resp => {
        notifySuccess({ message: "Customer converted successfully." });
        getPersons({ pagination: pagination.value });
      });
  }).onCancel(() => {
    notifyWarning({ message: "Convert Customer Cancelled" });
  });
}

// ----------------------------
// Applied Filter Labels.
// ----------------------------
const mapSingleFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.id === id);
  const text = match ? match.name : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...(search.value.firstName ? { "First Name": search.value.firstName } : {}),
  ...(search.value.lastName ? { "Last Name": search.value.lastName } : {}),
  ...(search.value.primaryPhoneNumber ? { "Phone Number": search.value.primaryPhoneNumber } : {}),
  ...(search.value.fromDate ? { "Identified From Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "Identified To Date": search.value.toDate } : {}),
  ...mapSingleFilterToLabel(search.value.countryId, countryList, "Country"),
  ...mapSingleFilterToLabel(search.value.stateProvinceId, stateList, "State"),
  ...(search.value.city ? { City: search.value.city } : {})
}));

function onClearFilters (key) {
  if (key === "Country") {
    search.value.countryId = "";
  } else if (key === "State") {
    search.value.stateProvinceId = "";
  } else if (key === "First Name") {
    search.value.firstName = "";
  } else if (key === "Last Name") {
    search.value.lastName = "";
  } else if (key === "Phone Number") {
    search.value.primaryPhoneNumber = "";
  } else if (key === "Identified From Date") {
    search.value.fromDate = "";
  } else if (key === "Identified To Date") {
    search.value.toDate = "";
  } else if (key === "City") {
    search.value.city = "";
  }
  delete appliedFilters.value[key];
  getPersons({ pagination: pagination.value });
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => search.value.searchText, () => {
  if (search.value.searchText) searchLoader.value = true;
  getPersons({ pagination: pagination.value });
});

watch(() => search.value.countryId, (newValue, oldValue) => {
  search.value.stateProvinceId = null;
  if (newValue !== oldValue) {
    getAllStateListForDropdown(); // fetch states for the selected country
  }
}, { immediate: true });

// ----------------------------
// On page rendering
// ----------------------------

onMounted(() => {
  tableRef.value.requestServerInteraction();
  getAllCountryListForDropdown();
  document.addEventListener("click", handleDocumentClick);
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

</script>
