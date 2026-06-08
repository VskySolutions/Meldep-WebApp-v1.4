<template>
  <q-dialog ref="dialogRef" class="customDialog DialogContainer" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 90vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Workspace And Menus</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-card-section>
        <div class="row items-center justify-end">
          <div class="col-auto">
            <div class="search-container justify-end" style="position: relative; display: flex; align-items: center; width: 320px;">
              <q-input
                v-model="search.searchText"
                :loading="searchLoader"
                outlined
                dense
                clearable
                debounce="300"
                placeholder="Search"
                class="bg-white search-box"
              >
                <template #prepend>
                  <q-icon name="o_search" />
                </template>
              </q-input>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-form greedy @submit.prevent.stop="onSelectWorkspaceAndMenu">
        <div class="q-pa-sm">
          <div class="">
            <q-table
              ref="tableRef"
              v-model:pagination="pagination"
              :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
              flat
              bordered
              :loading="loading"
              :rows="rows"
              :columns="columns"
              row-key="id"
              separator="cell"
              no-data-label="No data available"
              binary-state-sort
              :rows-per-page-options="[20, 50, 100, 200, 500]"
              @request="getAllHelpDeskTopicAndQuestionList"
            >
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                  <q-td style="width: 5%;">
                    <q-radio
                      v-model="selectedRow"
                      :val="props.row"
                    />
                  </q-td>
                  <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 50%;">{{ props.row.helpDeskTopic.title }}</q-td>
                  <q-td style="width: 45%;">{{ props.row.question }}</q-td>
                </q-tr>
                <q-separator />
              </template>
            </q-table>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
      <!-- </q-card-section> -->
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import { ref, onMounted, watch, computed } from "vue";
import { useAuthStore } from "stores/auth";
import helpDeskTopicsQuestionsService from "modules/helpdesk/helpDeskTopicsQuestions.service";

// Shared DataTable Features
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel, onDialogOK } = useDialogPluginComponent();

// Common variables
const processing = ref(false);
const loading = ref(true);
const searchLoader = ref(false);
const selectedRow = ref(null);
const $q = useQuasar();
const authStore = useAuthStore();
const siteId = computed(() => authStore.user?.siteId);

// table variables
const rows = ref([]);
const columns = ref([
  { name: "select", label: "", field: "select", align: "center" },
  { name: "topicId", label: "Workspace", field: "topicId", align: "left" },
  { name: "question", label: "Menu", field: "question", align: "left" }
]);

// local storage values
const {
  search,
  pagination,
  saveDataTableState
} = useSiteTableState({
  storageKey: "workspace-Menus-QuickSelect",
  siteId,
  defaultSearch: {
    searchText: ""
  },
  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});
// get all workspace and menus list and map
const getAllHelpDeskTopicAndQuestionList = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  helpDeskTopicsQuestionsService.getAllHelpDeskTopicAndQuestionList(payload).then((resp) => {
    rows.value = resp.helpDeskTopicQuestionList;
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
    saveDataTableState({
      search: search.value,
      pagination: props.pagination
    });
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

// select workspace and menus
const onSelectWorkspaceAndMenu = () => {
  if (!selectedRow.value) {
    $q.notify({
      type: "negative",
      message: "Please select Workspace and Menu"
    });
    return;
  }
  onDialogOK({
    topicId: selectedRow.value.topicId,
    questionId: selectedRow.value.id
  });
};

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => search.value.searchText, () => {
  if (search.value.searchText) {
    searchLoader.value = true;
  }

  getAllHelpDeskTopicAndQuestionList({
    pagination: pagination.value
  });
});

// On page rendering
onMounted(() => {
  const propps = { pagination: pagination.value };
  getAllHelpDeskTopicAndQuestionList(propps);
});

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_projectModule .q-select__dropdown-icon{
  display: none;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
</style>
