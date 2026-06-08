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
              <q-breadcrumbs-el label="Leave Rules" />
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
                          <label class="Cutomlabel q-mt-sm fs-13">Year</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select v-model="search.years" push class="q-mx-sm w-100 h-auto" clearable use-input use-chips transition-show="jump-up" transition-hide="jump-up" hide-bottom-space :dense="true" multiple fill-input input-debounce="0" :options="YearOptions" option-value="value" option-label="text" emit-value map-options :popup-content-class="customPopupContentClass" @filter="filterFn1">
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
                <q-btn icon="o_add" outline label="Add Leave Rules" no-caps class="text-primary btnRounded" @click="onAdd" />
                <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-sm" @click="$router.back()" />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-table
        ref="tableRef" v-model:pagination="pagination" :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
        no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getLeaveRules"
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
            <q-td class="text-left" style="width: 50%;">{{ props.row.year }}</q-td>
            <q-td style="width: 50%;" auto-width class="text-center actions">
              <!-- <q-icon name="o_autorenew" class="cursor-pointer q-mr-sm" @click="onGenerate(props.row.id)"> -->
              <q-icon name="o_autorenew" :class="{ 'cursor-pointer q-mr-sm': isEditFromCurrentOrFutureYear(props.row.year), 'text-grey-5 pointer-events-none q-mr-sm': !isEditFromCurrentOrFutureYear(props.row.year) }" @click="isEditFromCurrentOrFutureYear(props.row.year) && onGenerate(props.row.id)">
                <q-tooltip v-if="isEditFromCurrentOrFutureYear(props.row.year)">Generate leave rule</q-tooltip>
              </q-icon>
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" @click="onView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon name="o_event_available" class="cursor-pointer q-mr-sm" flat size="xs" @click="$router.push({ path: '/leave-yearly-schedule', query: { id: props.row.id, year: props.row.year} })">
                <q-tooltip>Leave Schedule</q-tooltip>
              </q-icon>
              <!-- <q-btn flat @click="$router.push('/dashboard')" class="no-padding q-ml-md"> -->
              <!-- <q-icon name="o_event_available" class="cursor-pointer q-mr-sm" @click="onLeaveSchedule(props.row.id)" size="xs">
                <q-tooltip>Leave Schedule</q-tooltip>
              </q-icon> -->
              <!-- <q-icon name="o_edit" class="q-mr-sm" :class="{ 'cursor-pointer': !props.row.isGenerated, 'text-grey-5 pointer-events-none': props.row.isGenerated }" @click="!props.row.isGenerated && onEdit(props.row.id)" size="xs"> -->
              <q-icon
                name="o_edit" class="q-mr-sm" :class="{ 'pointer-events-none text-grey-5': !isEditFromCurrentOrFutureYear(props.row.year), 'cursor-pointer': isEditFromCurrentOrFutureYear(props.row.year) }"
                size="xs" @click="isEditFromCurrentOrFutureYear(props.row.year) && onEdit(props.row.id)"
              >
                <q-tooltip v-if="isEditFromCurrentOrFutureYear(props.row.year)">Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" :class="{ 'cursor-pointer': !props.row.isGenerated, 'text-grey-5 pointer-events-none': props.row.isGenerated }" color="negative" size="xs" @click="!props.row.isGenerated && onDelete(props.row)">
                <q-tooltip v-if="!props.row.isGenerated">Delete</q-tooltip>
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
import editLeaveRule from "modules/leave-rules/components/addEdit.vue";
import viewLeaveRule from "modules/leave-rules/components/view.vue";
import leaveRuleService from "../leaveRules.service";
import { setLocalStorage, zwConfirm, zwConfirmDelete, getLocalStorage, notifySuccess, clearLocalStorage } from "assets/utils";
// import { useRoute } from "vue-router";

// Common variables
const $q = useQuasar();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
// const advanceSearchEnable = ref(false);
// const advanceSearch = () => { advanceSearchEnable.value = !advanceSearchEnable.value; };
// const route = useRoute();

const options1 = ref([]);
const Year = new Date().getFullYear();
// const NextThirtyYear = parseInt(Year) + parseInt(5);
const NextThirtyYear = Year + 5;
const YearOptions = ref([]);
for (let i = Year; i <= NextThirtyYear; i++) {
  const yearObj = { value: i, text: String(i) }; // Ensure text is a string
  YearOptions.value.push(yearObj);
  options1.value.push(yearObj);
}
// Search year for dropdown
function filterFn1 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      YearOptions.value = options1.value;
    } else {
      YearOptions.value = options1.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}
// local storage values
const localStorageKey = "Leave Rule";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const years = filterLocalStorage ? filterLocalStorage.years : [];
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Search variables
const search = ref({
  searchText,
  years
});

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "year", label: "Year", field: "year", align: "left", sortable: true }
]);

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
});

// open advance search
// const hasActiveFilters = (search) => {
//   return (
//     (search.years && search.years.length > 0)
//   );
// };

// Get/Map project list to table
const getLeaveRules = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  // advanceSearchEnable.value = hasActiveFilters(search.value);
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  leaveRuleService.getLeaveRules(payload).then((resp) => {
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
  getLeaveRules(propps);
};

// Clear search
const onClear = () => {
  search.value.years = [];
  clearLocalStorage(localStorageKey);
  onSearch();
};

// Create popup
const onAdd = () => {
  $q.dialog({
    component: editLeaveRule,
    componentProps: {}
  }).onOk(() => {
    getLeaveRules({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Edit popup
const onEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: editLeaveRule,
    componentProps: { id }
  }).onOk(() => {
    getLeaveRules({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// leave schedule popup
// const onLeaveSchedule = (id) => {
//   activeRowId.value = id;
//   route.push({ path: "/yearly-leave-schedule", query: { id } });
// };

// const onLeaveSchedule = (id) => {
//   activeRowId.value = id;
//   $q.dialog({
//     component: leaveSchedule,
//     componentProps: { id }
//   }).onOk(() => {
//     getLeaveRules({ pagination: pagination.value });
//   }).onCancel(() => {
//   }).onDismiss(() => {
//     activeRowId.value = null;
//   });
// };

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.year}` }, () => {
    leaveRuleService.deleteLeaveRule(item.id).then(resp => {
      notifySuccess({ message: "Leave Rule is deleted successfully." });
      getLeaveRules({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};

// View popup
const onView = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewLeaveRule,
    componentProps: { id }
  }).onOk(() => {
    getLeaveRules();
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// Delete record
const onGenerate = (item) => {
  activeRowId.value = item;
  zwConfirm({ data: `${item.fullName}` }, () => {
    leaveRuleService.generateLeaveRule(item).then(resp => {
      notifySuccess({ message: "Leave Rule is generated successfully." });
      getLeaveRules({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};

function isEditFromCurrentOrFutureYear (ruleYear) {
  // const eventYear = new Date(ruleYear).getFullYear();
  const currentYear = new Date().getFullYear();
  return ruleYear >= currentYear; // Enable if event is from current or future year
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getLeaveRules({ pagination: pagination.value });
});

// ----------------------------
// Applied Filter Labels.
// ----------------------------
const mapFilterToLabel = (ids, list, label) => {
  if (!Array.isArray(ids) || !ids.length || !list?.value) return {};

  const text = ids
    .map(id => {
      const match = list.value.find(item => item.value === id);
      return match ? match.text : id;
    })
    .join(", ");

  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.years, YearOptions, "Year")
}));

function onClearFilters (key) {
  if (key === "Year") {
    search.value.years = [];
  }
  delete appliedFilters.value[key];
  getLeaveRules({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  case "Year": return search.value.years?.length || 0;
  default: return null;
  }
}
</script>
