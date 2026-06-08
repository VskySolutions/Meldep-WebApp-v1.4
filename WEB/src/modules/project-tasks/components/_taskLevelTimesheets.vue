<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1300px !important;max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="max-width: 800px; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">Task Timesheet</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div>
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset class="q-mb-lg">
              <legend>Task timesheet info</legend>
              <q-table
                ref="tableRef" bordered class="no-shadow" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
                no-data-label="No data available" :filter="filter" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]"
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :props="props" :class="activeRowId == props.row.id">
                    <q-td style="width: 18%; max-width: 350px; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.task.name }}</q-td>
                    <q-td style="width: 15%; max-width: 350px; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.project.name }}</q-td>
                    <q-td v-if="flag === 'DT'" style="width: 12%; max-width: 350px; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.projectActivity.name }}</q-td>
                    <q-td v-if="flag === 'DT'" style="width: 35%; max-width: 350px; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;"><div v-html="props.row.description" /></q-td>
                    <q-td style="width: 15%;">{{ props.row.timesheet.employee.person.fullName ? props.row.timesheet.employee.person.fullName : "" }}</q-td>
                    <q-td class="text-right" style="width: 3%;">{{ props.row.hours }}</q-td>
                  </q-tr>
                  <q-tr v-if="props.pageIndex === rows.length - 1">
                    <q-td v-if="flag === 'DT'" colspan="5" class="text-right font-bold"><b>Total Hours:</b></q-td>
                    <q-td v-else colspan="3" class="text-right font-bold"><b>Total Hours:</b></q-td>
                    <q-td class="text-right">{{ totalActualHours }}</q-td>
                  </q-tr>
                </template>
              </q-table>
            </fieldset>
          </div>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted, computed } from "vue";
import _ from "lodash";
import projectTaskService from "modules/project-tasks/projectTasks.service";

// Common variables
const loading = ref(true);
const rows = ref([]);
// const data = ref([]);
const model = ref({});

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, flag: { type: String, default: "" } });
const flag = props.flag;

const columns = ref([
  { name: "task.name", label: "Task Name", field: "task.name", align: "left", sortable: true },
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: false },
  { name: "timesheet.employee.person.fullName", label: "Employee Name", field: "timesheet.employee.person.fullName", align: "left", sortable: false },
  { name: "hours", label: "Actual Hrs", field: "hours", align: "center", sortable: false }
]);

if (flag === "DT") {
  columns.value.splice(2, 0, // insert after Project Activity
    { name: "projectActivity.name", label: "Project Activity", field: "projectActivity.name", align: "left", sortable: false },
    { name: "description", label: "Activity Details", field: "description", align: "left", sortable: false }
  );
}

const getTimesheetByTaskId = () => {
  loading.value = true;
  projectTaskService.getTimesheetByTaskId(props.id).then((resp) => {
    rows.value = resp;
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

const totalActualHours = computed(() => {
  return rows.value.reduce((sum, row) => {
    const hrs = Number(row.hours) || 0;
    return sum + hrs;
  }, 0);
});

// On page rendering
onMounted(() => {
  getTimesheetByTaskId();
});
</script>
