<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1300px !important;max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="width: 98%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset class="q-mb-lg">
              <legend>Task Status change log</legend>
              <q-table
                ref="tableRef" bordered class="no-shadow" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
                binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]"
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                    <q-td style="width: 5%;">{{ props.rowIndex + 1 }}</q-td>
                    <q-td style="width: 15%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.taskName }}</q-td>
                    <q-td style="width: 5%;">{{ props.row.status }}</q-td>
                    <q-td style="width: 5%;">{{ props.row.changeBy }}</q-td>
                    <q-td style="width: 5%;">{{ props.row.changeDate }}</q-td>
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
import projectTaskService from "modules/project-tasks/projectTasks.service";
// import useFilters from "composables/useFilters";

// Common variables
// const { toDate } = useFilters();
const loading = ref(true);
const rows = ref([]);
const model = ref({});

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

const columns = ref([
  { name: "projectTaskNumber", label: "Log Id", field: "projectTaskNumber", align: "left", sortable: true },
  { name: "project.name", label: "Task Name", field: "project.name", align: "left", sortable: false },
  { name: "projectModule.name", label: "Status", field: "projectModule.name", align: "left", sortable: false },
  { name: "name", label: "Changed By", field: "name", align: "left", sortable: false },
  { name: "status.dropDownValue", label: "Changed Date", field: "status.dropDownValue", align: "left", sortable: true }
]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// get project details
const getProjectTaskDetails = () => {
  loading.value = true;
  projectTaskService.getProjectTaskDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rows.value = resp.projectTaskStatusLog.map(item => ({
      ...item,
      taskName: item.task.name,
      status: item.status.dropDownValue,
      changeBy: item.statusChangedByEmployee.person.fullName,
      changeDate: item.statusChangedDate
    })).sort((a, b) => new Date(b.changeDate) - new Date(a.changeDate)); // Sort by date in ascending order
  }).finally(() => {
    loading.value = false;
  });
};

// On page rendering
onMounted(() => {
  getProjectTaskDetails();
});
</script>
