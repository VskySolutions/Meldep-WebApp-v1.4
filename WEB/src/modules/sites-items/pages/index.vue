<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center justify-between">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Item" />
              <q-breadcrumbs-el label="Site Items" />
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
                          <label class="Cutomlabel q-mt-sm fs-13">Item Subcategory</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.itemSubcategoryIds"
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
                            :options="itemSubcategoryList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                            @filter="getAllItemSubcategoryListForFilter"
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
                          <label class="Cutomlabel q-mt-sm fs-13">Item Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.itemName"
                            push
                            class="q-mx-sm w-100 h-auto"
                            hide-bottom-space
                            :dense="true"
                            type="text"
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
              <div class="row q-gutter-sm justify-end">
                <q-btn
                  icon="o_add"
                  outline
                  label="Add Site Item"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onSiteItemAdd"
                />
                <q-btn
                  icon="o_chevron_left"
                  class="text-primary"
                  outline
                  label="Previous"
                  @click="$router.push('/item-subcategory/subcategory-and-attribute-mapping')"
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
        @request="getAllSitesItemList"
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
            <q-td style="width: 20%;">{{ props.row.itemSubcategory?.name }}</q-td>
            <q-td style="width: 20%;">{{ props.row.itemName }}</q-td>
            <q-td style="width: 25%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
              {{ props.row.createdBy?.person?.firstName + " " + props.row.createdBy?.person?.lastName }}
            </q-td>
            <q-td style="width: 25%;">
              {{ props.row.createdOnUtc }}
            </q-td>
            <q-td style="width: 10%;" class="text-center actions">
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="onSiteItemView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon name="o_edit" class="cursor-pointer q-mr-sm" size="xs" @click="onSiteItemEdit(props.row.id)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onSiteItemDelete(props.row)">
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
// Import libraries
import { ref, onMounted, computed, watch } from "vue";
import { useQuasar } from "quasar";
import { zwConfirmDelete, notifySuccess, getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";

import sitesItemsService from "../sitesItems.service";
import itemCategoryService from "src/modules/items/itemCategory.service";

import editSiteItem from "modules/sites-items/components/addEdit.vue";
import viewSiteItem from "modules/sites-items/components/view.vue";

// Common variables
const $q = useQuasar();
const loading = ref(true);
const searchLoader = ref(false);
const showFilter = ref(false);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// LocalStorage:- Get values from Cookies
// --------------------------------------------------------------------------------------------------------------------------------------------------
const localStorageKey = "Sites Items";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const createdBy = filterLocalStorage ? filterLocalStorage.createdBy : "";
const employeeId = filterLocalStorage ? filterLocalStorage.employeeId : "";
const itemSubcategoryIds = filterLocalStorage ? filterLocalStorage.itemSubcategoryIds : [];
const itemName = filterLocalStorage ? filterLocalStorage.itemName : "";
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// --------------------------------------------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// --------------------------------------------------------------------------------------------------------------------------------------------------

// Search variables
const search = ref({
  searchText,
  createdBy,
  employeeId,
  itemSubcategoryIds,
  itemName
});

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "itemSubcategory.name", label: "Item Subcategory", field: "itemSubcategory.name", align: "left", sortable: true },
  { name: "itemName", label: "Item Name", field: "itemName", align: "left", sortable: true },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left", sortable: true },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "center", sortable: true }
]);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// DataTable:- Get Sites Item List
// --------------------------------------------------------------------------------------------------------------------------------------------------
const getAllSitesItemList = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  sitesItemsService.getAllSitesItemList(payload).then((resp) => {
    rows.value = resp.sitesItems ?? [];
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

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get item subcategories
// --------------------------------------------------------------------------------------------------------------------------------------------------
const itemSubcategoryList = ref([]);
const itemSubcategoryFilter = ref([]);
const getAllItemSubcategoryList = () => {
  itemCategoryService.getAllItemSubcategoryList()
    .then((resp) => {
      const responseData = resp.map((item) => ({ text: item.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
      itemSubcategoryList.value = responseData;
      itemSubcategoryFilter.value = responseData;
    });
};

// Search item subcategory for dropdown
function getAllItemSubcategoryListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      itemSubcategoryList.value = itemSubcategoryFilter.value;
    } else {
      itemSubcategoryList.value = itemSubcategoryFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// ------------------------------------------------------------------------------------
// DataTable:- Actions
// ------------------------------------------------------------------------------------

// Create popup
const onSiteItemAdd = () => {
  $q.dialog({
    component: editSiteItem,
    componentProps: {}
  }).onOk(() => {
    getAllSitesItemList({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Edit popup
const onSiteItemEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: editSiteItem,
    componentProps: { id }
  }).onOk(() => {
    getAllSitesItemList({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// View popup
const onSiteItemView = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewSiteItem,
    componentProps: { id }
  }).onOk(() => {
    getAllSitesItemList({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = id;
  });
};

// Delete record
const onSiteItemDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.itemName}` }, () => {
    sitesItemsService.deleteSitesItem(item.id).then(resp => {
      notifySuccess({ message: "Sites item is deleted successfully." });
      getAllSitesItemList({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
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
  ...mapFilterToLabel(search.value.itemSubcategoryIds, itemSubcategoryList, "Item Subcategory"),
  ...(search.value.itemName ? { "Item Name": search.value.itemName } : {})
}));

function onClearFilters (key) {
  if (key === "Item Subcategory") {
    search.value.itemSubcategoryIds = [];
  } else if (key === "Item Name") {
    search.value.itemName = "";
  }
  delete appliedFilters.value[key];
  getAllSitesItemList({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  case "Item Subcategory": return search.value.itemSubcategoryIds?.length || 0;
  default: return null;
  }
}

const onSearch = () => {
  const propps = { pagination: pagination.value };
  getAllSitesItemList(propps);
};

// Clear search
const onClear = () => {
  search.value.itemSubcategoryIds = [];
  search.value.itemName = "";
  clearLocalStorage(localStorageKey);
  onSearch();
};

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getAllSitesItemList({ pagination: pagination.value });
});

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  getAllItemSubcategoryList();
});

</script>
