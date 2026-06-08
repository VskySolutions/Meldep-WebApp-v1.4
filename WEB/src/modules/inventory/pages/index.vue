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
              <q-breadcrumbs-el label="Infrastructure" />
              <q-breadcrumbs-el label="Inventories" />
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
                          <label class="Cutomlabel q-mt-sm fs-13">Device Type</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.ItemTypeIds" push class="q-mx-sm w-100 h-auto" use-input use-chips transition-show="jump-up"
                            transition-hide="jump-up" hide-bottom-space :dense="true" multiple fill-input input-debounce="0"
                            :options="itemTypeList" option-value="value" option-label="text" emit-value map-options @filter="filterFn1"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center">
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
                          <label class="Cutomlabel q-mt-sm fs-13">Device No</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.code" class="q-mx-sm w-100 h-auto" fill-input :dense="true" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Office Location</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.officeLocationIds"
                            push
                            class="q-mx-sm w-100 h-auto"
                            use-input
                            use-chips
                            transition-show="jump-up"
                            transition-hide="jump-up"
                            hide-bottom-space
                            :dense="true"
                            multiple
                            fill-input
                            input-debounce="0"
                            :options="officeLocationList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            @filter="getAllOfficeLocationDropdownForFilter"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center">
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
                          <label class="Cutomlabel q-mt-sm fs-13">Status</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.inventoryStatusIds" push class="q-mx-sm w-100 h-auto" use-input use-chips transition-show="jump-up"
                            transition-hide="jump-up" hide-bottom-space :dense="true" multiple fill-input input-debounce="0"
                            :options="inventoryStatusList" option-value="value" option-label="text" emit-value map-options :popup-content-class="customPopupContentClass" @filter="filterFn2"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center">
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
                          <label class="Cutomlabel q-mt-sm fs-13">Employee Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.employeeIds" push class="q-mx-sm w-100 h-auto" clearable use-input use-chips transition-show="jump-up"
                            transition-hide="jump-up" hide-bottom-space :dense="true" multiple fill-input input-debounce="0"
                            :options="employeeList" option-value="value" option-label="text" emit-value map-options :popup-content-class="customPopupContentClass" @filter="filterFn3"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center">
                                    <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
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
                <q-btn icon="o_add" outline label="Add Inventory" no-caps class="text-primary btnRounded" @click="onAdd" />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />

      <q-table
        ref="tableRef" v-model:pagination="pagination" :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
        no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getInventorys"
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
            <q-td>{{ props.row.inventorycode }}</q-td>
            <q-td>{{ props.row.officeLocation.dropDownValue }}</q-td>
            <q-td>{{ props.row.itemType.name }}</q-td>
            <q-td>{{ props.row.modelNameORNumber }}</q-td>
            <q-td>{{ props.row.processorType }}</q-td>
            <q-td>{{ props.row.memoryORRAM }}</q-td>
            <q-td>{{ props.row.serviceCode }}</q-td>
            <q-td> {{ props.row.inventoryAssignmentList.map(a => a.employee.person.fullName).join(', ') }}</q-td>
            <q-td style="width: 10%;">
              <q-chip :color="getStatusColor(props.row.inventoryStatus.dropDownValue)" name="o_done" class="rounded q-px-lg" text-color="black">
                {{ props.row.inventoryStatus.dropDownValue }}
              </q-chip>
            </q-td>
            <q-td auto-width class="text-center actions">
              <a style="position: relative;" class="q-icon notranslate cursor-pointer q-ml-sm q-mr-md" @click="onAddNote(props.row.id, 'Inventory', props.row.id, props.row.name, props.row.name)">
                <q-tooltip anchor="bottom middle" self="top middle">Note</q-tooltip>
                <q-icon name="o_assignment" />
                <q-badge v-if="props.row.inventoryNotesCount > 0" style="position: absolute; right: -16px; top: -15px;" color="green" text-color="white" :label="props.row.inventoryNotesCount" />
              </a>
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
import addeditLeaveRule from "modules/inventory/components/addedit_inventory.vue";
import viewInventory from "modules/inventory/components/view_inventory.vue";
import inventoryService from "modules/inventory/inventory.service";
import commonService from "services/common.service";
import addNote from "modules/common/components/addNote.vue";
import employeesService from "src/modules/employee/employee.service";
import { zwConfirmDelete, notifySuccess, setLocalStorage, getLocalStorage, clearLocalStorage } from "assets/utils";

// Common variables
const $q = useQuasar();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const selectedSiteId = ref(history.state?.siteId);
// const advanceSearchEnable = ref(false);
// const advanceSearch = () => { advanceSearchEnable.value = !advanceSearchEnable.value; };

const localStorageKey = "Inventory";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const ItemTypeIds = filterLocalStorage ? filterLocalStorage.ItemTypeIds : [];
const code = filterLocalStorage ? filterLocalStorage.code : "";
const employeeIds = filterLocalStorage ? filterLocalStorage.employeeIds : [];
const inventoryStatusIds = filterLocalStorage ? filterLocalStorage.inventoryStatusIds : [];
const officeLocationIds = filterLocalStorage ? filterLocalStorage.officeLocationIds : [];
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Search variables
const search = ref({
  searchText,
  ItemTypeIds,
  code,
  employeeIds,
  inventoryStatusIds,
  officeLocationIds
});

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "inventorycode", label: "Device No", field: "inventorycode", align: "left", sortable: true },
  { name: "officeLocation.dropDownValue", label: "Office Location", field: "officeLocation.dropDownValue", align: "left", sortable: true },
  { name: "itemType.name", label: "Device Type", field: "itemType.name", align: "left", sortable: true },
  { name: "modelNameORNumber", label: "Model No", field: "modelNameORNumber", align: "left", sortable: true },
  { name: "processorType", label: "Proccessor", field: "processorType", align: "left", sortable: true },
  { name: "memoryORRAM", label: "RAM(GB)", field: "memoryORRAM", align: "left", sortable: true },
  { name: "serviceCode", label: "Service Tag", field: "serviceCode", align: "left", sortable: true },
  { name: "inventoryAssignmentList.employee.person.fullName", label: "Allocate To", field: "inventoryAssignmentList.employee.person.fullName", align: "left", sortable: false },
  { name: "inventoryStatus.dropDownValue", label: "Status", field: "inventoryStatus.dropDownValue", align: "left", sortable: true }
]);

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  getAllEmployeesListForDropdown();
  getAllItemType();
  getDropDownStatus("Inventory Status");
  getAllOfficeLocationForDropDown(selectedSiteId.value, "Employee OrgLocation");
});

// open advance search
// const hasActiveFilters = (search) => {
//   return (
//     (search.ItemTypeIds && search.ItemTypeIds.length > 0) ||
//     (search.code) ||
//     (search.employeeIds && search.employeeIds.length > 0) ||
//     (search.inventoryStatusIds && search.inventoryStatusIds.length > 0)
//   );
// };

// Get/Map project list to table
const getInventorys = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  // advanceSearchEnable.value = hasActiveFilters(search.value);
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  inventoryService.getInventorys(payload).then((resp) => {
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
  getInventorys(propps);
};

// Clear search
const onClear = () => {
  search.value.ItemTypeIds = [];
  search.value.code = "";
  search.value.inventoryStatusIds = [];
  search.value.employeeIds = [];
  search.value.officeLocationIds = [];
  clearLocalStorage(localStorageKey);
  onSearch();
};

// Get all type List const responseData = resp
const itemTypeList = ref([]);
const options1 = ref([]);
function getAllItemType (name) {
  inventoryService.getAllItemType(name).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    itemTypeList.value = responseData;
    options1.value = responseData;
  });
}
// Search Type List for dropdown
function filterFn1 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      itemTypeList.value = options1.value;
    } else {
      itemTypeList.value = options1.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all status List
const inventoryStatusList = ref([]);
const options2 = ref([]);
function getDropDownStatus (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    inventoryStatusList.value = responseData;
    options2.value = responseData;
  });
}
// Search status List for dropdown
function filterFn2 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      inventoryStatusList.value = options2.value;
    } else {
      inventoryStatusList.value = options2.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all employee list
const employeeList = ref([]);
const options3 = ref([]);
function getAllEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    employeeList.value = responseData;
    options3.value = responseData;
  });
}
// Search  employee for dropdown
function filterFn3 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = options3.value;
    } else {
      employeeList.value = options3.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all office Location list
const officeLocationList = ref([]);
const officeLocationFilter = ref([]);
function getAllOfficeLocationForDropDown (selectedSiteId, typeName) {
  commonService.getDropDownForSite(selectedSiteId, typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    officeLocationList.value = responseData;
    officeLocationFilter.value = responseData;
  });
}

function getAllOfficeLocationDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      officeLocationList.value = officeLocationFilter.value;
    } else {
      officeLocationList.value = officeLocationFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Create popup
const onAdd = () => {
  $q.dialog({
    component: addeditLeaveRule,
    componentProps: {}
  }).onOk(() => {
    getInventorys({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

const onAddNote = (id, type, moduleId, module, name) => {
  activeRowId.value = id;
  $q.dialog({
    component: addNote,
    componentProps: { id, type, moduleId, module, name }
  }).onOk(() => {
    getInventorys({ pagination: pagination.value });
  }).onCancel(() => {
    getInventorys({ pagination: pagination.value });
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// Edit popup
const onEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: addeditLeaveRule,
    componentProps: { id }
  }).onOk(() => {
    getInventorys({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// View popup
const onView = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewInventory,
    componentProps: { id }
  }).onOk(() => {
    getInventorys();
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.itemType.name} ${item.inventorycode}` }, () => {
    inventoryService.deleteInventory(item.id).then(resp => {
      notifySuccess({ message: "Inventory is deleted successfully." });
      getInventorys({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};

// Added colors for task status dropdown list
function getStatusColor (statusText) {
  if (statusText) {
    switch (statusText) {
    case "Assigned":
      return "yellow-5";
    case "Available":
      return "green-5";
    case "Damaged":
      return "red-5";
    case "Donate":
      return "light-blue-5";
    default:
      return "#ffffff";
    }
  }
}
// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getInventorys({ pagination: pagination.value });
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
  ...mapFilterToLabel(search.value.ItemTypeIds, itemTypeList, "Device Type"),
  ...mapFilterToLabel(search.value.inventoryStatusIds, inventoryStatusList, "Status"),
  ...mapFilterToLabel(search.value.employeeIds, employeeList, "Employee Name"),
  ...mapFilterToLabel(search.value.officeLocationIds, officeLocationList, "Office Location"),
  ...(search.value.code ? { "Device No": search.value.code } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Device Type": return search.value.ItemTypeIds?.length || 0;
  case "Status": return search.value.inventoryStatusIds?.length || 0;
  case "Employee Name": return search.value.employeeIds?.length || 0;
  case "Office Location": return search.value.officeLocationIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Device Type") {
    search.value.ItemTypeIds = [];
  } else if (key === "Device No") {
    search.value.code = "";
  } else if (key === "Status") {
    search.value.inventoryStatusIds = [];
  } else if (key === "Employee Name") {
    search.value.employeeIds = [];
  } else if (key === "Office Location") {
    search.value.officeLocationIds = [];
  }
  delete appliedFilters.value[key];
  getInventorys({ pagination: pagination.value });
}
</script>
