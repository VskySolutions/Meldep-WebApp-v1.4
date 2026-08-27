<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <!-- Header -->
    <q-card-section class="row items-center justify-end q-pb-sm">
      <div class="row items-center">
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

      <div class="row items-center q-gutter-sm">
        <q-btn
          v-if="projectId"
          icon="o_open_in_new"
          size="sm"
          outline
          class="text-primary q-ml-md"
          style="padding: 3px 7px; min-height: 30px;"
          @click="$router.push({ path: '/project-action-items',
          state: {
            projectId: projectId,
            requirementId: props.requirementId
          }})"
        >
          <q-tooltip>Open Project Action Items</q-tooltip>
        </q-btn>
      </div>
    </q-card-section>
    <q-separator />

    <!-- Table -->
    <q-table
      flat
      :rows="filteredItems"
      :columns="columns"
      :loading="loading"
      v-model:pagination="pagination"
      row-key="id"
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      class="req-dashboard-table"
      separator="cell"
      no-data-label="No data available"
    >
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
        <q-tr :props="props">

          <q-td style="width:40%;" class="hoverable-cell" @click="onProjectActionItemsView(props.row.id)">
            {{ props.row.title }}
          </q-td>
          
          <q-td style="width:40%;">
            {{ props.row.customer.name }}
          </q-td>
          
          <q-td style="width:40%;">
            {{ props.row.employee.person.fullName }}
          </q-td>

          <q-td style="width:40%;">
            {{ props.row.dueDate }}
          </q-td>
          
          <q-td style="width:40%;">
            {{ props.row.priority.dropDownValue }}
          </q-td>

        </q-tr>
      </template>
    </q-table>
  </q-card>
</template>
<script setup>
import { computed, ref, watch, onMounted } from "vue";
import { notifyError } from "assets/utils";
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

// SOP Change :- Shared Project Dialogs
import {
  initProjectActionItemsDialogs,
  onProjectActionItemsView
} from "src/modules/project-action-items/utils/dialogs.js";

const emit = defineEmits(['summary']);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

// common variables
const authStore = useAuthStore();
const user = authStore.user;
const loading = ref(false);
const searchLoader = ref(false);
const rows = ref([]);
const showFilter = ref(false);
const siteId = computed(() => authStore.user?.siteId);
const { toDate } = useFilters();

const columns = [
  { name: "title", label: "TITLE", field: "title", align: "left", sortable: true },
  { name: "customerId", label: "CUSTOMER", field: "customer.name", align: "left", sortable: true },
  { name: "employeeId", label: "EMPLOYEE", field: "employee.person.fullName", align: "left", sortable: true },
  { name: "dueDate", label: "DUE DATE", field: "dueDate", align: "left", sortable: true },
  { name: "priority.dropDownValue", label: "PRIORITY", field: "priority.dropDownValue", align: "left", sortable: true }
]

const projectId = ref('');
const getProjectActionItemsByRequirementId = async ({ pagination: p }) => {
  loading.value = true;
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
    rows.value = resp.projectActionItemList || [];

    if (rows.value.length > 0) {
      projectId.value = rows.value[0].project?.id || '';
    } else {
      projectId.value = '';
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

    emit('summary', {
      total: rows.value.length
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
  storageKey: "requirement-Center-Project-Action-Items-Tabular-List",
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

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------
initProjectActionItemsDialogs(activeRowId);

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

onMounted(async () => {
  activeEmployeesDropdown.load(user.siteId);
  projectActionItemPriorityForDropdown.load("Project Action Item Priority");
  customerNameDropdown.load();
});
</script>
