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
              <q-breadcrumbs-el label="Settings" clickable to="/Settings" />
              <q-breadcrumbs-el label="Dropdown Types" />
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
                          <label class="Cutomlabel q-mt-sm fs-13">Module Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                           <q-select
                              v-model="search.moduleName"
                              class="q-mx-sm w-100 h-auto"
                              use-input
                              stack-label
                              hide-bottom-space
                              :dense="true"
                              :options="moduleOptionsList"
                              option-value="value"
                              option-label="text"
                              emit-value
                              map-options
                              @filter="getAllModuleListForFilter"
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
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Group Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.groupName"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">DropDown Type</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.dropDownTypeIds"
                            push
                            class="q-mx-sm w-100 h-auto"
                            clearable
                            use-input
                            use-chips
                            transition-show="jump-up"
                            transition-hide="jump-up"
                            hide-bottom-space
                            :dense="true"
                            multiple
                            fill-input
                            input-debounce="0"
                            :options="dropDownTypeList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                            @filter="getAllDropDownTypeListForFilter"
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
                <q-btn icon="o_add" outline label="Add Dropdown Type" no-caps class="text-primary btnRounded" @click="onAdd" />
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
        @request="getDropDownTypes"
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
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''" :set="(preModuleName = null)">
            <q-td>
              <span v-if="preModuleName !== props.row.moduleName" :set="preModuleName = props.row.moduleName">
                {{ props.row.moduleName }}
              </span>
            </q-td>
            <q-td>
              <span>
                {{ props.row.groupName }}
              </span>
            </q-td>
            <q-td>{{ props.row.type }}</q-td>
            <q-td auto-width class="text-center actions">
              <q-icon name="o_edit" class="cursor-pointer q-mr-sm" @click="onEdit(props.row.id)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" @click="onDelete(props.row)">
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
import { ref, onMounted, computed, watch } from "vue";
import { useQuasar } from "quasar";
import editDropdownType from "modules/dropdown/components/addEditDropdownType.vue";
import dropdowntypeService from "modules/dropdown/dropdown.service";
import { zwConfirmDelete, notifySuccess, setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";

// Common variables
const $q = useQuasar();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
// const advanceSearchEnable = ref(false);
// const advanceSearch = () => { advanceSearchEnable.value = !advanceSearchEnable.value; };

// // local storage values
const localStorageKey = "DropDownType";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const moduleName = filterLocalStorage ? filterLocalStorage.moduleName : "";
const groupName = filterLocalStorage ? filterLocalStorage.groupName : "";
const dropDownTypeIds = filterLocalStorage ? filterLocalStorage.dropDownTypeIds : [];
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Search variables
const search = ref({
  searchText,
  moduleName,
  groupName,
  dropDownTypeIds
});

const moduleOptions = [
  { value: 'Dashboard', text: 'Dashboard' },
  { value: 'My Work', text: 'My Work' },
  { value: 'Talent Hire', text: 'Talent Hire' },
  { value: 'Project Management', text: 'Project Management' },
  { value: 'Org Management', text: 'Org Management' },
  { value: 'SDLC', text: 'SDLC' },
  { value: 'CRM', text: 'CRM' },
  { value: 'Finance', text: 'Finance' },
  { value: 'Marketing', text: 'Marketing' },
  { value: 'Infrastructure', text: 'Infrastructure' },
  { value: 'Item', text: 'Item' },
  { value: 'Help Desk', text: 'Help Desk' }
].sort((a, b) => a.text.localeCompare(b.text));

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "moduleName", label: "Module Name", field: "moduleName", align: "left", sortable: false },
  { name: "groupName", label: "Group Name", field: "groupName", align: "left", sortable: false },
  { name: "type", label: "Dropdown Type", field: "type", align: "left", sortable: true }
]);

// Get/Map project list to table
const getDropDownTypes = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  dropdowntypeService.getDropDownTypes(payload).then((resp) => {
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


const moduleOptionsListFilter = ref([...moduleOptions]);
const moduleOptionsList = ref([...moduleOptions]);
function getAllModuleListForFilter(val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";

    if (needle === "") {
      moduleOptionsList.value = moduleOptionsListFilter.value;
    } else {
      moduleOptionsList.value = moduleOptionsListFilter.value.filter(v =>
        v.text.toLowerCase().includes(needle)
      );
    }
  });
}

// Get all dropDown list for dropdown
const dropDownTypeList = ref([]);
const dropDownTypeListFilter = ref([]);
function getAllDropDownTypeListForDropdown () {
  dropdowntypeService.getAllDropDownTypeListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.type, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    dropDownTypeList.value = responseData;
    dropDownTypeListFilter.value = responseData;
  });
}
// Search project for dropdown
function getAllDropDownTypeListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      dropDownTypeList.value = dropDownTypeListFilter.value;
    } else {
      dropDownTypeList.value = dropDownTypeListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Create popup
const onAdd = () => {
  $q.dialog({
    component: editDropdownType,
    componentProps: {}
  }).onOk(() => {
    getDropDownTypes({ pagination: pagination.value });
    getAllDropDownTypeListForDropdown();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Edit popup
const onEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: editDropdownType,
    componentProps: { id }
  }).onOk(() => {
    getDropDownTypes({ pagination: pagination.value });
    getAllDropDownTypeListForDropdown();
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.type}` }, () => {
    dropdowntypeService.deleteDropDownType(item.id).then(resp => {
      notifySuccess({ message: "Dropdown Type is deleted successfully." });
      getDropDownTypes({ pagination: pagination.value });
      getAllDropDownTypeListForDropdown();
    });
  }, () => {
    activeRowId.value = null;
  });
};

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getDropDownTypes(propps);
};

// Clear search
const onClear = () => {
  search.value.moduleName = "";
  search.value.groupName = "";
  search.value.dropDownTypeIds = [];
  clearLocalStorage(localStorageKey);
  onSearch();
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
  ...mapFilterToLabel(search.value.dropDownTypeIds, dropDownTypeList, "DropDown Type")
}));

function onClearFilters (key) {
  if (key === "Module Name") {
    search.value.moduleName = "";
  } else if (key === "Group Name") {
    search.value.groupName = "";
  } else if (key === "DropDown Type") {
    search.value.dropDownTypeIds = [];
  }
  delete appliedFilters.value[key];
  getDropDownTypes({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  case "DropDown Type": return search.value.dropDownTypeIds?.length || 0;
  default: return null;
  }
}

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  getAllDropDownTypeListForDropdown();
});

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getDropDownTypes({ pagination: pagination.value });
});

</script>
