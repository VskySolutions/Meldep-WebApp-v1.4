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
              <multiSelectDropdown
                v-model="search.projectIds"
                label="Project Name"
                :options="projectNameDropdown.list.value"
                :filter="projectNameDropdown.filter"
                class="hidden"
              />
              <multiSelectDropdown
                v-model="search.requirementIds"
                label="Requirement"
                :disable="!search.projectIds"
                :options="requirementsByProjectModuleIdForDropdown.list.value"
                :filter="requirementsByProjectModuleIdForDropdown.filter"
                class="hidden"
              />
              <div class="row items-center q-mb-sm">
                <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                  <label class="Cutomlabel q-mt-sm fs-13">Question</label>
                </div>
                <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                  <q-input v-model="search.title" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
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
      :rows="filteredQA"
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
          :class="{ 'bg-blue-1': selectedQA === props.row.id }"
          @click="
            selectedQA = props.row.id;
            emit('select', props.row);
          "
        >
          <q-td class="q-pa-none">
            <div class="column q-gutter-xs" style="white-space: normal; word-break: break-word;">
              <div class="text-black text-weight-medium">
                {{ props.row.title }}
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

import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";

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
const loading = ref(false);
const searchLoader = ref(false);
const selectedQA = ref(null);
const qAndA = ref([]);
const showFilter = ref(false);
const siteId = computed(() => authStore.user?.siteId);

const columns = [
  {
    name: "qAndA",
    label: "Questions",
    field: "title",
    align: "left",
    sortable: true
  }
];

const getProjectQAByRequirementId = async ({ pagination: p }) => {
  loading.value = true;

  const { page, rowsPerPage, sortBy, descending } = p;

  const payload = {
    searchText: search.value.searchText,
    projectIds: search.value.projectIds,
    requirementId: props.requirementId,
    title: search.value.title,
    sortBy: sortBy,
    sorts: sorts.value,
    descending: descending,
    page: page,
    pageSize: rowsPerPage
  };

  try {
    loading.value = true;

    const resp = await requirementCenterService.getProjectQAByRequirementId(payload);
    qAndA.value = resp.projectQuestionsAnswerList || [];

    if (qAndA.value.length) {
      selectedQA.value = qAndA.value[0].id;
      emit("select", qAndA.value[0]);
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
    notifyError({ message: "Failed to load question answers" });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const refreshProjectQAList = () => {
  getProjectQAByRequirementId({ pagination: pagination.value });
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  saveDataTableState
} = useSiteTableState({
  storageKey: "requirement-Center-Question-Answers-List",
  siteId,
  defaultSearch: {
    searchText: "",
    title: "",
    projectIds: [],
    requirementIds: []
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

const filteredQA = computed(() => {
  if (!search.value) return qAndA.value;

  return qAndA.value.filter(task =>
    JSON.stringify(task)
      .toLowerCase()
      .includes((search.value.quickSearch ?? '').toLowerCase())
  );
});

// Clear search
const onClear = () => {
  search.value.title = "";
  search.value.projectIds = [];
  search.value.requirementIds = [];

  saveDataTableState({
    search: search.value
  });
  onSearch();
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectNameDropdown
} = projectModule();

const { requirementsByProjectModuleIdForDropdown } = requirementModule();
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
  ...(search.value.title ? { "Question": search.value.title } : {}),
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name"),
  ...mapFilterToLabel(search.value.requirementIds, requirementsByProjectModuleIdForDropdown.list, "Requirement"),
}));

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

// Search records as per parameters
const onSearch = () => { refreshProjectQAList(); };

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
  async () => {
    await getProjectQAByRequirementId({
      pagination: pagination.value
    });
  },
  { immediate: true }
);

watch(
  () => search.value.searchText,
  () => {
    searchLoader.value = true;
    refreshProjectQAList();
  }
);

watch(
  () => props.activeTab,
  () => {
    showFilter.value = false;
  }
);

watch(() => search.value.projectIds, async (newValue, oldValue) => {
  if (search.value?.projectIds?.length === 0) search.value.requirementIds = [];
  if (search.value?.projectIds?.length === 0 || newValue === oldValue) return;

  requirementsByProjectModuleIdForDropdown.load('', newValue);
}, { immediate: true });

onMounted(async () => {  
  projectNameDropdown.load();
  if (search.value.projectIds.length > 0) requirementsByProjectModuleIdForDropdown.load('', search.value.projectIds);
});
</script>
