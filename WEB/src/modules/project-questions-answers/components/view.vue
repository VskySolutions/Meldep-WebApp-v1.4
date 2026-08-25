<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 65vw !important;max-width: 65vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader justify-between">
        <div class="text-h2 text-white q-mr-lg">{{ model.title }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Project Question Answers Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Project Name</div>
                <div class="text-black q-mb-sm">{{ model.project.name }}</div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Requirement</div>
                <div class="text-black q-mb-sm">{{ model.requirement.title }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-12 col-md-12">
                <div class="q-mb-xs">Question</div>
                <div class="text-black q-mb-sm">{{ model.title }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12">
                <div class="q-mb-xs">Answer</div>
                <p class="q-pt-md text-black RichTextEditor" v-html="model.description ? model.description : '-'" />
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created By</div>
                <div class="text-black q-mb-sm"> {{ model.createdBy.person.fullName }}</div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created Date</div>
                <div class="text-black q-mb-sm">{{ model.createdOnUtc }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Updated By</div>
                <div class="text-black q-mb-sm"> {{ model.updatedBy.person.fullName }}</div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Updated Date</div>
                <div class="text-black q-mb-sm">{{ model.updatedOnUtc }}</div>
              </div>
            </div>
          </fieldset>
          <fieldset v-if="changeLogRows && changeLogRows.length > 0" class="q-mb-lg">
            <legend>Response Log</legend>
            <q-table
              ref="tableRef"
              v-model:pagination="changeLogPagination"
              bordered
              class="no-shadow"
              :loading="loading"
              :rows="changeLogRows"
              :columns="changeLogColumns"
              row-key="id"
              separator="cell"
              no-data-label="No data available"
              binary-state-sort
              :rows-per-page-options="[20, 50, 100, 200, 500]"
            >
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                </q-tr>
              </template>

              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                  <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 40%;"><div class="RichTextEditor" v-html="props.row.description" /></q-td>
                  <q-td>{{ props.row.createdBy?.person?.fullName }}</q-td>
                  <q-td>{{ props.row.createdOnUtc }}</q-td>
                  <q-td>{{ props.row.updatedBy?.person?.fullName }}</q-td>
                  <q-td>{{ props.row.updatedOnUtc }}</q-td>
                </q-tr>
              </template>
            </q-table>
          </fieldset>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";

import projectQuestionsAnswersService from "modules/project-questions-answers/projectQuestionsAnswers.service";
// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Common variables
const loading = ref(true);
const changeLogRows = ref([]);

// Define model values
const model = ref({
  name: "-",
  description: "",
  createdOnUtc: "",
  project: {
    name: ""
  },
  requirement: {
    title: ""
  },
  createdBy: {
    person: {
      fullName: ""
    }
  },
  updatedBy: {
    person: {
      fullName: ""
    }
  }
});

const changeLogPagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const changeLogColumns = ref([
  { name: "description", label: "Answer", field: "description", align: "left", sortable: true },
  { name: "createdBy.person.fullName", label: "Created By", field: "createdBy.person.fullName", align: "left", sortable: true },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true },
  { name: "updatedBy.person.fullName", label: "Updated By", field: "updatedBy.person.fullName", align: "left", sortable: true },
  { name: "updatedOnUtc", label: "Updated Date", field: "updatedOnUtc", align: "left", sortable: true }
]);

// get Question Answers details
const getQuestionAnswersInDetailsById = async () => {
  loading.value = true;

  try {
    const resp = await projectQuestionsAnswersService.getQuestionAnswersInDetailsById(props.id);

    model.value = _.cloneDeep(resp);

    changeLogRows.value = (resp.projectQuestionsAnswersResponseLog ?? []).map(item => ({
      ...item,
      editing: false,
      flag: "Edit"
    }));
  } finally {
    loading.value = false;
  }
};

// On page rendering
onMounted(() => {
  getQuestionAnswersInDetailsById();
});
</script>
