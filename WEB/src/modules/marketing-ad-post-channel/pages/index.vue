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
              <q-breadcrumbs-el label="Marketing" />
              <q-breadcrumbs-el label="Ad Post Channel List" />
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
                <div class="search-container" style="position: relative; display: flex; align-items: center; width: 320px;">
                  <q-input v-model="searchText" :loading="searchLoader" outlined dense clearable debounce="300" placeholder="Search" class="bg-white search-box" style="flex: 1; border-top-right-radius: 0; border-bottom-right-radius: 0; border-top-right-radius: 0; max-width: 250px;">
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
                          <label class="Cutomlabel q-mt-sm fs-13">Ad Channel Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.name" push class="q-mx-sm w-100 h-auto" fill-input :dense="true" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Project Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.projectIds" push class="q-mx-sm w-100 h-auto" clearable use-input use-chips transition-show="jump-up"
                            transition-hide="jump-up" hide-bottom-space :dense="true" multiple fill-input input-debounce="0"
                            :options="projectList" option-value="value" option-label="text" emit-value map-options :popup-content-class="customPopupContentClass" @filter="filterFn1"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center selection-drodown-item">
                                    <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                                    <span>{{ opt.text }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Customer Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.customerIds" push class="q-mx-sm w-100 h-auto" clearable use-input use-chips transition-show="jump-up"
                            transition-hide="jump-up" hide-bottom-space :dense="true" multiple fill-input input-debounce="0"
                            :options="customerList" option-value="value" option-label="text" emit-value map-options @filter="filterFn2"
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
                <q-btn icon="o_add" outline label="Create Ad Post Channel" no-caps class="text-primary btnRounded" @click="onAdd" />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />

      <q-table
        ref="tableRef" v-model:pagination="pagination" :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
        no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500] " @request="getAllAdPostChannel"
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
            <q-td style="width: 3%;" class="text-right">#{{ props.row.channelNumber }}</q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 43%;">{{ props.row.customer.name }}</q-td>
            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 44%;">{{ props.row.name }}</q-td>
            <q-td style="width: 5%;" class="text-right">{{ props.row.groupMemberCount }}</q-td>
            <q-td auto-width class="text-center actions">
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="onView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon name="o_edit" class="cursor-pointer q-mr-sm" size="xs" @click="onEdit(props.row.id)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onDelete(props.row)">
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
import { useQuasar } from "quasar";
import addeditAdPostChannel from "modules/marketing-ad-post-channel/components/addEdit.vue";
import projectService from "modules/project/projects.service";
// import employeesService from "src/modules/employee/employee.service";
import viewAdPostChannel from "modules/marketing-ad-post-channel/components/view.vue";
import adPostChannelService from "modules/marketing-ad-post-channel/marketingAdPostChannel.service";
import customerService from "src/modules/customer/customer.service";
// import TestCaseModule from "modules/test-case/pages/index.vue";
import { zwConfirmDelete, notifySuccess, getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";

// Common variables
// const { toDate } = useFilters();
const $q = useQuasar();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
// const advanceSearchEnable = ref(false);
// const advanceSearch = () => { advanceSearchEnable.value = !advanceSearchEnable.value; };

// local storage values
const localStorageKey = "Ad Post Channel";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const projectIds = filterLocalStorage ? filterLocalStorage.projectIds : [];
const name = filterLocalStorage ? filterLocalStorage.name : "";
const customerIds = filterLocalStorage ? filterLocalStorage.customerIds : [];
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Search variables
const search = ref({
  searchText,
  projectIds,
  name,
  customerIds
});

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "channelNumber", label: "Channel No.", field: "channelNumber", align: "left", sortable: true },
  { name: "customerId", label: "Client Name", field: "customerId", align: "left", sortable: true },
  { name: "name", label: "Channel Name", field: "name", align: "left", sortable: true },
  { name: "groupMemberCount", label: "Group members count", field: "groupMemberCount", align: "right", sortable: true }
]);

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  getAllProjectListForDropdown();
  getAllCustomerListForDropdown();
});
// open advance search
// const hasActiveFilters = (search) => {
//   return (
//     (search.projectIds && search.projectIds.length > 0) ||
//     (search.name) ||
//     (search.customerIds)
//   );
// };

// Get/Map project list to table
const getAllAdPostChannel = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  // advanceSearchEnable.value = hasActiveFilters(search.value);
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  adPostChannelService.getAllAdPostChannel(payload).then((resp) => {
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

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getAllAdPostChannel(propps);
};

// Clear search
const onClear = () => {
  search.value.projectIds = [];
  search.value.name = "";
  search.value.customerIds = [];
  clearLocalStorage(localStorageKey);
  onSearch();
};

// Get all project list for dropdown
const projectList = ref([]);
const options1 = ref([]);
function getAllProjectListForDropdown () {
  projectService.getAllProjectListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.name, value: item.id }));
    projectList.value = responseData;
    options1.value = responseData;
  });
}
// Search project for dropdown
function filterFn1 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectList.value = options1.value;
    } else {
      projectList.value = options1.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all Customer list
const customerList = ref([]);
const options2 = ref([]);
function getAllCustomerListForDropdown () {
  customerService.getAllCustomerListForDropdown().then((resp) => {
    const responseData = resp
      .map((item) => ({
        text: item.company
          ? item.company.name // Use Company name if available
          : `${item.person.firstName} ${item.person.lastName}`, // Otherwise, use Person name
        value: item.id
      }))
      .sort((a, b) => a.text.localeCompare(b.text));
    customerList.value = responseData;
    options2.value = responseData;
  });
}
// Search Customer for dropdown
function filterFn2 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      customerList.value = options2.value;
    } else {
      customerList.value = options2.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Create popup
const onAdd = () => {
  $q.dialog({
    component: addeditAdPostChannel,
    componentProps: {}
  }).onOk(() => {
    getAllAdPostChannel({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Edit popup
const onEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: addeditAdPostChannel,
    componentProps: { id }
  }).onOk(() => {
    getAllAdPostChannel({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// View popup
const onView = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewAdPostChannel,
    componentProps: { id }
  }).onOk(() => {
    getAllAdPostChannel();
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.name}, ${item.project.name}` }, () => {
    adPostChannelService.deleteAdPostChannel(item.id).then(resp => {
      notifySuccess({ message: "Ad Channel is deleted successfully." });
      getAllAdPostChannel({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getAllAdPostChannel({ pagination: pagination.value });
});

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
  ...mapFilterToLabel(search.value.projectIds, projectList, "Project Name"),
  ...mapFilterToLabel(search.value.customerIds, customerList, "Customer Name"),
  ...(search.value.name ? { "Ad Channel Name": search.value.name } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Customer Name") {
    search.value.customerIds = [];
  } else if (key === "Ad Channel Name") {
    search.value.name = "";
  }
  delete appliedFilters.value[key];
  getAllAdPostChannel({ pagination: pagination.value });
}

</script>
