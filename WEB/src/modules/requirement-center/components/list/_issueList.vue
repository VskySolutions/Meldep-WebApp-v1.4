<template>
  <q-card flat bordered>
    <q-separator />
    <q-card-section>
      <!-- <q-input
        v-model="search"
        dense
        outlined
        placeholder="Search issue..."
        clearable
      >
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input> -->
      <div class="row justify-end">
        <div class="search-container position-relative">
          <searchFilterBar
            v-model="search.searchText"
            :loading="searchLoader"
            :applied-filters="appliedFilters"
            class="search-bar"
            @toggle-filter="showFilter = !showFilter"
          />
          <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
            <q-card class="q-pa-sm">
              <div class="row items-center q-mb-sm">
                <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                  <label class="Cutomlabel q-mt-sm fs-13">Issue Id</label>
                </div>
                <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                  <q-input v-model="search.issueNumber" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                </div>
              </div>
              <div class="row items-center q-mb-sm">
                <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                  <label class="Cutomlabel q-mt-sm fs-13">Issue Name</label>
                </div>
                <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                  <q-input
                    v-model="search.name"
                    class="q-mx-sm w-100 h-auto"
                    fill-input
                    :dense="true"
                  />
                </div>
              </div>
              <multiSelectDropdown
                v-model="search.priorityIds"
                label="Issue Priority"
                :options="issuePriorityForDropdown.list.value"
                :filter="issuePriorityForDropdown.filter"
                :isShowAll="true"
              />
              <multiSelectDropdown
                v-model="search.statusIds"
                label="Status"
                :options="issueStatusForDropdown.list.value"
                :filter="issueStatusForDropdown.filter"
                :isShowAll="true"
              />
              <multiSelectDropdown
                v-model="search.issueTypeIds"
                label="Issue Type"
                :options="issueTypeForDropdown.list.value"
                :filter="issueTypeForDropdown.filter"
                :isShowAll="true"
              />
              <multiSelectDropdown
                v-model="search.employeeIds"
                label="Assign To"
                :options="activeEmployeesDropdown.list.value"
                :filter="activeEmployeesDropdown.filter"
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
    </q-card-section>
    <q-separator />
    <q-table
      ref="tableRef"
      v-model:pagination="pagination"
      :class="(filteredIssues.length > 0 ? 'my-sticky-header-table' : '') + 'Custom-DataTable TicketTable'"
      :loading="loading"
      :rows="filteredIssues"
      :columns="columns"
      row-key="id"
      separator="cell"
      no-data-label="No data available"
      binary-state-sort
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      :filter="searchText"
      style="height: 100vh;"
    >
      <template #loading>
        <q-inner-loading showing color="primary">
          <q-spinner size="40px" />
        </q-inner-loading>
      </template>
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th v-for="col in props.cols" :key="col.name" :props="props">
            {{ col.label }}
          </q-th>
        </q-tr>
      </template>
      <template #body="props">
        <q-tr
          :props="props"
          class="cursor-pointer"
          :class="{ 'bg-blue-1': selectedIssue === props.row.id }"
          @click="
            selectedIssue = props.row.id;
            emit('select', props.row);
          "
        >
          <q-td class="q-pa-none">
            <div class="column q-gutter-xs" style="white-space: normal; word-break: break-word;">
                <div class="row items-center justify-between">
                  <div class="text-caption text-weight-bold text-black">
                    #{{ props.row.issueNumber }}
                  </div>
                  <div>
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
                    <q-badge
                      rounded
                      class="q-mr-xs"
                      :style="{
                        backgroundColor: props.row.statusBgColor,
                        color: props.row.statusTextColor
                      }"
                    >
                      {{ props.row.statusName }}
                      <q-tooltip>Status</q-tooltip>
                    </q-badge>
                  </div>
                </div>
              <div class="text-black fs-14">
                {{ props.row.name }}
              </div>

              <div class="text-caption text-grey-7">
                {{ props.row.projectName }}
              </div>

              <div class="text-caption text-grey-7 q-mt-xs">
                <strong>Assign To:</strong>
                {{ props.row.assignTo !== ' ' ? props.row.assignTo : '-' }}
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
import { computed, ref, watch, onMounted } from 'vue';
import { useAuthStore } from "stores/auth";
import { notifyError } from "assets/utils";
import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Dropdowns
import issueModule from "src/modules/issue/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";

const emit = defineEmits(['select']);
const props = defineProps({
  requirementId: {
    type: String,
    required: true
  },
  activeTab: String
});

const selectedIssue = ref(null);
const Issues = ref([]);
const searchLoader = ref(false);
const showFilter = ref(false);
const loading = ref(true);
const authStore = useAuthStore();
const siteId = computed(() => authStore.user?.siteId);

const columns = [{ name: "Issue", label: "Issues", align: "left", field: "issueNumber", sortable: true }];

const getIssuesByRequirementId = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;

  const number = search.value.issueNumber ? search.value.issueNumber.replace(/[^0-9]/g, "").replace(/^0+(?!$)/, "") : "";
  search.value.issueNumber = number || "0";

  const payload = {
    requirementId: props.requirementId,
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    ...search.value
  };

  saveDataTableState({
    search: search.value,
    pagination: p,
    activeRowId: activeRowId.value,
    sorts
  });

  try {
    loading.value = true;

    const resp = await requirementCenterService.getIssuesByRequirementId(payload);

    Issues.value = resp.map(item => ({
      ...item,
      projectName: item.project?.name ?? '-',
      assignTo: item.employee?.person?.fullName ?? '-',
      statusName: item.status?.dropDownValue ?? '-',
      statusTextColor: item.status?.color ?? '#000',
      statusBgColor: item.status?.bgColor ?? '#e0e0e0',
      priorityName: item.priority?.dropDownValue ?? '-',
      priorityTextColor: item.priority?.color ?? '#000',
      priorityBgColor: item.priority?.bgColor ?? '#e0e0e0'
    }));

    if (Issues.value.length) {
      selectedIssue.value = Issues.value[0].id;
      emit('select', Issues.value[0]);
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
      activeRowId: activeRowId.value
    });

  } catch (err) {
    console.error(err);
    notifyError({ message: 'Failed to load issues' });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

const filteredIssues = computed(() => Issues.value);

const {
  search,
  pagination,
  activeRowId,
  sorts,
  saveDataTableState
} = useSiteTableState({
  storageKey: "requirement-Center-Issue-List",
  siteId: siteId,

  defaultSearch: {
    searchText: "",
    issueNumber: "",
    issueTypeIds: [],
    priorityIds: [],
    statusIds: [],
    employeeIds: [],
    name: null
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const refreshIssueList = () => {
  getIssuesByRequirementId({ pagination: pagination.value });
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const { activeEmployeesDropdown } = employeeModule();
const {
  issueStatusForDropdown,
  issuePriorityForDropdown,
  issueTypeForDropdown,
  issueStatusDropdownSingleSelect
} = issueModule();


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
  ...mapFilterToLabel(search.value.priorityIds, issuePriorityForDropdown.list, "Issue Priority"),
  ...mapFilterToLabel(search.value.statusIds, issueStatusForDropdown.list, "Status"),
  ...mapFilterToLabel(search.value.issueTypeIds, issueTypeForDropdown.list, "Issue Type"),
  ...mapFilterToLabel(search.value.employeeIds, activeEmployeesDropdown.list, "Assign To"),
  ...(search.value.issueNumber > 0 ? { "Issue Id": search.value.issueNumber } : {}),
  ...(search.value.name ? { "Issue Name": search.value.name } : {})
}));
// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

// Search records as per parameters
const onAdvanceSearch = () => { refreshIssueList(); };

// Clear search
const onAdvanceClear = () => {
  search.value.issueNumber = undefined;
  search.value.name = "";
  search.value.priorityIds = [];
  search.value.statusIds = [];
  search.value.issueTypeIds = [];
  search.value.employeeIds = [];
  saveDataTableState({
    search: search.value
  });
  onAdvanceSearch();
};
// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
    async () => {
      await getIssuesByRequirementId({
        pagination: pagination.value
      });
    },
  {
    immediate: true
  }
);

watch(
  () => search.value.searchText,
  () => {
    searchLoader.value = true;
    refreshIssueList();
  }
);

watch(
  () => props.activeTab,
  () => {
    showFilter.value = false;
  }
);
// ----------------------------------------------------------------------------------------------------------------
// On page rendering
// ----------------------------------------------------------------------------------------------------------------

onMounted(() => {
  issueStatusForDropdown.load("Issue Status");
  issuePriorityForDropdown.load("Issue Priority");
  issueStatusDropdownSingleSelect.load("Issue Status");
  issueTypeForDropdown.load("Issue Type");
  activeEmployeesDropdown.load(siteId.value);
});
</script>

<style scoped>
.q-item--active {
  border-left: 4px solid #1976d2;
}
</style>
