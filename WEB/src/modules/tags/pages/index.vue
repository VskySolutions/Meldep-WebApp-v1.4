<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-xs-1 col-sm-1 col-md-1 col-lg-1 col-xl-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Settings" clickable to="/Settings" />
              <q-breadcrumbs-el label="Tags" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-xs-3 col-sm-3 col-md-3 col-lg-5 col-xl-5">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <!-- <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge> -->
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /><q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /><q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-xs-8 col-sm-8 col-md-8 col-lg-6 col-xl-5">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container" style="position: relative; display: flex; align-items: center; width: 320px;">
                  <q-input
                    v-model="tagfilter" outlined dense clearable debounce="300" placeholder="Search"
                    class="bg-white search-box" style="flex: 1; border-top-right-radius: 0; border-bottom-right-radius: 0; border-top-right-radius: 0;"
                  >
                    <template #prepend>
                      <q-icon name="o_search" />
                    </template>
                  </q-input>
                  <q-btn
                    unelevated :color="Object.keys(appliedFilters).length > 0 ? 'primary' : 'grey-7'" text-color="white" class="q-pa-xs" style="height: 40px; border-top-left-radius: 0; border-bottom-left-radius: 0;"
                    @click.stop="showFilter = !showFilter"
                  >
                    <q-icon name="o_filter_alt" size="sm" color="white" class="q-mr-xs" />
                    <q-item-label class="text-xs fs-12">
                      <span class="block">Set/Clear</span>
                      <span class="block">FILTER</span>
                    </q-item-label>
                    <q-tooltip anchor="bottom middle" self="top middle">Advanced Filter</q-tooltip>
                  </q-btn>
                  <!-- Dropdown Content -->
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 400px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-md">
                        <div class="col-lg-3 col-md-3 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Name</label>
                        </div>
                        <div class="col-lg-9 col-md-9 col-sm-12 col-xs-12">
                          <q-input
                            v-model="search.name"
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
              <div class="q-ml-sm">
                <q-btn icon="o_add" outline label="Add Tag" no-caps class="text-primary btnRounded" @click="onAdd" />
                <q-btn icon="o_chevron_left" outline label="Back To List" no-caps class="text-primary btnRounded q-ml-sm" @click="$router.push('/Settings')" />
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
        :rows="filteredTag"
        :columns="columns"
        row-key="id"
        separator="cell"
        no-data-label="No data available"
        loading-label=" "
        binary-state-sort
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        :filter="tagfilter"
        @request="getAllTags"
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
            <q-td style="width: 20%;">
              <div class="chip-container" style="display: flex; flex-wrap: wrap; gap: 4px;">
                <q-chip :style="{ backgroundColor: props.row.bgColor, color: props.row.color, padding: '4px 8px', maxWidth: '100%', wordBreak: 'break-word' }">
                  {{ props.row.name }}
                </q-chip>
              </div>
            </q-td>
            <q-td style="width: 5%;">
              <q-input v-model="props.row.bgColor" filled class="my-input">
                <template #append>
                  <q-icon
                    name="o_colorize"
                    class="cursor-pointer"
                    @click="storePreviousColor(props.row)"
                  >
                    <q-popup-proxy
                      cover
                      transition-show="scale"
                      transition-hide="scale"
                    >
                      <q-color
                        v-model="props.row.bgColor"
                        no-header
                        no-footer
                        default-view="palette"
                        @update:model-value="startColorSelection"
                        @change="finalizeColorSelection(props.row, true)"
                      />
                    </q-popup-proxy>
                  </q-icon>
                </template>
                <div class="q-mt-sm" :style="{ backgroundColor: props.row.bgColor, width: '24px', height: '24px', borderRadius: '50%', border: '1px solid #ccc' }" />
              </q-input>
            </q-td>
            <q-td style="width: 5%;">
              <q-input v-model="props.row.color" filled class="my-input">
                <template #append>
                  <q-icon
                    name="o_colorize"
                    class="cursor-pointer"
                    @click="storePreviousColor(props.row)"
                  >
                    <q-popup-proxy
                      cover
                      transition-show="scale"
                      transition-hide="scale"
                    >
                      <q-color
                        v-model="props.row.color"
                        no-header
                        no-footer
                        default-view="palette"
                        @update:model-value="startColorSelection"
                        @change="finalizeColorSelection(props.row, false)"
                      />
                    </q-popup-proxy>
                  </q-icon>
                </template>
                <div class="q-mt-sm" :style="{ backgroundColor: props.row.color, width: '24px', height: '24px', borderRadius: '50%', border: '1px solid #ccc' }" />
              </q-input>
            </q-td>
            <q-td style="width: 2%;" class="text-center actions">
              <q-icon
                name="o_edit"
                class="cursor-pointer q-mr-sm"
                @click="onEdit(props.row.id)"
              >
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon
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
import { ref, onMounted, computed } from "vue";
import tagMasterService from "modules/tags/tags.service";
import { useQuasar } from "quasar";
import { setLocalStorage, clearLocalStorage, getLocalStorage, notifySuccess, zwConfirmDelete } from "assets/utils";
import addEditTags from "modules/tags/components/addEdit.vue";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const tagfilter = ref("");
const $q = useQuasar();
// const advanceSearchEnable = ref(false);
// const advanceSearch = () => { advanceSearchEnable.value = !advanceSearchEnable.value; };

// local storage values
const localStorageKey = "TagsMaster";
const filterLocalStorage = getLocalStorage(localStorageKey);
const name = filterLocalStorage ? filterLocalStorage.name : "";
// const pagination = ref(filterLocalStorage?.pagination || { sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Search variables
const search = ref({
  name
});

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "name", label: "Name", field: "name", align: "left", sortable: true, style: "width: 10%" },
  { name: "bgColor", label: "Background Color", field: "bgColor", align: "left", sortable: true },
  { name: "color", label: "Color", field: "color", align: "left", sortable: true }
]);

// static search
const filterRows = (data, searchTerm, columns) => {
  if (!searchTerm) return data;
  const lowerCaseTerm = searchTerm.toLowerCase();

  return data.filter(row =>
    columns.some(col => {
      const value = col.field.split(".").reduce((obj, key) => obj?.[key], row);
      return String(value || "").toLowerCase().includes(lowerCaseTerm);
    })
  );
};

const tagColumns = columns.value;
const filteredTag = computed(() => filterRows(rows.value, tagfilter.value, tagColumns));

// open advance search
// const hasActiveFilters = (search) => {
//   return (
//     (search.name && search.name.length > 0)
//   );
// };

// Get/Map tags list to table
const getAllTags = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  // advanceSearchEnable.value = hasActiveFilters(search.value);
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  // setLocalStorage(localStorageKey, search.value);
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  tagMasterService.getAllTags(payload).then((resp) => {
    rows.value = resp.data;
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getAllTags(propps);
};

// Clear search
const onClear = () => {
  search.value.name = "";
  clearLocalStorage(localStorageKey);
  onSearch();
};
let isSliderActive = false;

const startColorSelection = () => {
  isSliderActive = true; // The user is select with the slider
};

// const finalizeColorSelection = (row) => {
//   if (!isSliderActive) return false;
//   isSliderActive = true; // Reset the flag

//   // Check if the color has changed before submitting
//   if (isSliderActive) {
//     if (row.color !== previousColor.value) {
//       onSubmitColor(row.id, row.color);
//       return true;
//     }
//   }
//   return false;
// };
const finalizeColorSelection = (row, isBgColor = false) => {
  if (!isSliderActive) return false;
  isSliderActive = true;

  const newColor = isBgColor ? row.bgColor : row.color;
  const prevColor = previousColor.value;

  if (newColor !== prevColor) {
    onSubmitColor(row.id, newColor, isBgColor);
    return true;
  }
  return false;
};

const onSubmitColor = (id, color, isBgColor) => {
  // const payload = { color: color };
  const payload = {
    color: isBgColor ? null : color,
    bgColor: isBgColor ? color : null,
    isBgColor
  };
  setTimeout(() => {
    tagMasterService.updateTags(id, payload).then(resp => {
      notifySuccess({ message: "Color updated successfully." });
      getAllTags({ pagination: pagination.value });
    });
  });
};

const previousColor = ref(""); // Store previous color
const storePreviousColor = (row) => {
  previousColor.value = row.color ? row.color : "#e0e0e0"; // Store previous color before opening picker
};

// Create popup
const onAdd = () => {
  $q.dialog({
    component: addEditTags,
    componentProps: {}
  }).onOk(() => {
    getAllTags({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Edit popup
const onEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: addEditTags,
    componentProps: { id }
  }).onOk(() => {
    getAllTags({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.name}` }, () => {
    tagMasterService.deleteTags(item.id).then(resp => {
      notifySuccess({ message: "Tag is deleted successfully." });
      getAllTags({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};

// appled filters
const appliedFilters = computed(() => ({
  ...(search.value.name ? { Name: search.value.name } : {})
}));

// Clear filters based on key
function onClearFilters (key) {
  if (key === "Name") {
    search.value.name = "";
  }
  delete appliedFilters.value[key];
  getAllTags({ pagination: pagination.value });
}

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
});

// function getFilterCount (key) {
//   switch (key) {
//   case "Name": return search.value.name;
//   default: return null; // For single-value filters like Year, Status
//   }
// }
</script>
