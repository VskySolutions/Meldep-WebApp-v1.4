<template>
  <q-card flat bordered>
    <q-separator />

    <q-card-section>
      <div class="row justify-end">
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
              <div class="row items-center q-mb-sm">
                <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                  <label class="Cutomlabel q-mt-sm fs-13">Title</label>
                </div>
                <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                  <q-input
                    v-model="search.title"
                    fill-input
                    class="q-mx-sm w-100 h-auto"
                    :dense="true"
                  />
                </div>
              </div>
              <multiSelectDropdown
                v-model="search.customerIds"
                label="Customer"
                :options="customerNameDropdown.list.value"
                :filter="customerNameDropdown.filter"
              />
              <multiSelectDropdown
                v-model="search.employeeIds"
                label="Employee"
                :options="activeEmployeesDropdown.list.value"
                :filter="activeEmployeesDropdown.filter"
              />
              <multiSelectDropdown
                v-model="search.priorityIds"
                label="Priority"
                :options="projectActionItemPriorityForDropdown.list.value"
                :filter="projectActionItemPriorityForDropdown.filter"
                :isShowAll="true"
              />
              <div class="row items-center q-mb-sm">
                <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                  <label class="Cutomlabel q-mt-sm fs-13">Due Date</label>
                </div>
                <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                  <div class="input-group q-ml-sm w-100 h-auto">
                    <q-input
                      v-model="search.dueDate"
                      fill-input
                      dense
                      mask="##/##/####"
                    >
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date
                              v-model="search.dueDate"
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
    </q-card-section>
    <q-separator />

    <q-table
      v-model:pagination="pagination"
      flat
      :loading="loading"
      :rows="filteredItems"
      :columns="columns"
      row-key="id"
      separator="cell"
      binary-state-sort
      no-data-label="No data available"
      :rows-per-page-options="[20, 50, 100, 200]"
      style="height: calc(100vh - 220px)"
    >
      <template #loading>
        <q-inner-loading showing color="primary">
          <q-spinner size="40px" />
        </q-inner-loading>
      </template>

      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th
            v-for="col in props.cols"
            :key="col.name"
            :props="props"
          >
            {{ col.label }}
          </q-th>
        </q-tr>
      </template>

      <template #body="props">
        <q-tr
          :props="props"
          class="cursor-pointer"
          :class="{ 'bg-blue-1': selectedActionItems === props.row.id }"
          @click="
            selectedActionItems = props.row.id;
            emit('select', props.row);
          "
        >
          <q-td class="q-pa-none">
            <div class="column q-gutter-xs" style="white-space: normal; word-break: break-word;">
              <div class="row items-center justify-between">
              </div>
              <div class="text-black text-weight-medium">
                {{ props.row.title }}
              </div>
              <!-- <div class="text-caption text-grey-7">
                {{ props.row.project.name }}
              </div> -->
              <div class="text-black text-weight-medium">
                <q-badge
                  rounded
                  class="q-mr-xs"
                  :style="{
                    backgroundColor: props.row.priorityBgColor,
                    color: props.row.priorityTextColor
                  }"
                >
                  {{ props.row.priorityName }}
                  <q-tooltip>Priority</q-tooltip>
                </q-badge>
              </div>
              <div class="text-caption text-grey-7">
                <strong>Due Date:</strong>
                {{ props.row.dueDate }}
              </div>
            </div>
          </q-td>
        </q-tr>
        <q-separator />
      </template>
    </q-table>
  </q-card>
</template>

<script setup>
import { computed, ref, watch, onMounted } from "vue";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";

import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

// SOP Change :- Shared Dropdowns
import projectActionItemModule from "src/modules/project-action-items/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

const emit = defineEmits(["select"]);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  },
  projectId: {
    type: String,
    required: true
  },
  activeTab: String
});

const authStore = useAuthStore();
const user = authStore.user;
const loading = ref(false);
const searchLoader = ref(false);
const selectedActionItems = ref(null);
const rows = ref([]);
const showFilter = ref(false);
const siteId = computed(() => authStore.user?.siteId);
const { toDate } = useFilters();

const columns = [
  {
    name: "Action Items",
    label: "Action Items",
    field: "title",
    align: "left",
    sortable: false
  }
];

const getProjectActionItemsByRequirementId = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;  
  search.value.dueDate = search.value.dueDate
    ? toDate(search.value.dueDate)
    : null;
  const payload = {
    searchText: search.value.searchText,
    requirementId: props.requirementId,
    title: search.value.title,
    priorityIds: search.value.priorityIds,
    customerIds: search.value.customerIds,
    employeeIds: search.value.employeeIds,
    dueDate: search.value.dueDate,
    sortBy: sortBy,
    descending: descending,
    page: page,
    pageSize: rowsPerPage
  };

  try {
    loading.value = true;
    const resp = await requirementCenterService.getProjectActionItemsByRequirementId(payload);

    rows.value = resp.projectActionItemList.map(item => ({
      ...item,
      priorityName: item.priority?.dropDownValue ?? '-',
      priorityTextColor: item.priority?.color ?? '#000',
      priorityBgColor: item.priority?.bgColor ?? '#e0e0e0'
    }));

    if (rows.value.length > 0) {
      selectedActionItems.value = rows.value[0].id;
      emit("select", rows.value[0]);
    }    
    Object.assign(pagination.value, {
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    });
    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: activeRowId.value,
      sorts
    });
  } catch (err) {
    console.error(err);
    notifyError({ message: "Failed to load Project action items" });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const refreshProjectActionItemsList = () => {
  getProjectActionItemsByRequirementId({ pagination: pagination.value });
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  saveDataTableState
} = useSiteTableState({
  storageKey: "requirement-Center-Project-Action-Items-List",
  siteId,
  defaultSearch: {
    searchText: "",
    title: "",
    employeeIds: [],
    customerIds: [],
    priorityIds: [],
    dueDate: ""
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

const filteredItems = computed(() => {
  if (!search.value) return rows.value;

  return rows.value.filter(item =>
    JSON.stringify(item)
      .toLowerCase()
      .includes((search.value.quickSearch ?? '').toLowerCase())
  );
});

// Clear search
const onClear = () => {
  search.value.priorityIds = [];
  search.value.customerIds = [];
  search.value.employeeIds = [];
  search.value.title = "";
  search.value.dueDate = "";

  saveDataTableState({
    search: search.value
  });
  onSearch();
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Applied Filter Labels.
// ----------------------------------------------------------------------------------------------------------------
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
  ...mapFilterToLabel(search.value.priorityIds, projectActionItemPriorityForDropdown.list, "Priority"),
  ...mapFilterToLabel(search.value.customerIds, customerNameDropdown.list, "Customer"),
  ...mapFilterToLabel(search.value.employeeIds, activeEmployeesDropdown.list, "Employee"),
  ...(search.value.title ? { "Title": search.value.title } : {}),
  ...(search.value.dueDate ? { "Due Date": search.value.dueDate } : {})
}));
// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

// Search records as per parameters
const onSearch = () => { refreshProjectActionItemsList(); };

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const {
  projectActionItemPriorityForDropdown
} = projectActionItemModule();

const { activeEmployeesDropdown } = employeeModule();
const { customerNameDropdown } = customerModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
  async () => {
    await getProjectActionItemsByRequirementId({
      pagination: pagination.value
    });
  },
  { immediate: true }
);

watch(
  () => search.value.searchText,
  () => {
    searchLoader.value = true;
    refreshProjectActionItemsList();
  }
);

watch(
  () => props.activeTab,
  () => {
    showFilter.value = false;
  }
);

onMounted(async () => {
  activeEmployeesDropdown.load(user.siteId);
  projectActionItemPriorityForDropdown.load("Project Action Item Priority");
  customerNameDropdown.load();
});
</script>
