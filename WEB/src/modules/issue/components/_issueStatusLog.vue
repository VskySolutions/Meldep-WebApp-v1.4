<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="max-width: 95%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <!-- <q-form greedy @submit.prevent.stop="onSubmit"> -->
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset class="q-mb-lg">
            <legend>Issue status change log</legend>
            <q-table
              ref="tableRef" bordered class="no-shadow" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
              binary-state-sort
            >
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                  <q-td>{{ props.rowIndex + 1 }}</q-td>
                  <q-td style="max-width: 350px; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.issue.name }}</q-td>
                  <q-td>{{ props.row.status }}</q-td>
                  <q-td>{{ props.row.statusChangedByEmployee.person.fullName }}</q-td>
                  <q-td>{{ props.row.statusChangedDate }}</q-td>
                </q-tr>
              </template>
            </q-table>
          </fieldset>
        </div>
      </div>
      <!-- </q-form> -->
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";
import issueService from "modules/issue/issue.service";
import { format } from "date-fns"; // Standard TimeZone Conversion

// Common variables
const loading = ref(true);
const rows = ref([]);
const model = ref({});

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

const columns = ref([
  { name: "issueNumber", label: "Log Id", field: "issueNumber", align: "left", sortable: false },
  { name: "issue.name", label: "Issue Name", field: "issue.name", align: "left", sortable: false },
  { name: "status.dropDownValue", label: "Status", field: "status.dropDownValue", align: "left", sortable: false },
  { name: "statusChangedByEmployee.person.fullName", label: "Changed By", field: "statusChangedByEmployee.person.fullName", align: "left", sortable: false },
  { name: "statusChangedDate", label: "Changed Date", field: "statusChangedDate", align: "left", sortable: true }
]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// On page rendering
onMounted(() => {
  getIssueDetails();
});

// get Issue details
const getIssueDetails = () => {
  loading.value = true;
  issueService.getIssueDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rows.value = resp.issueStatusChangedLog.map(item => ({
      ...item,
      issueName: item.issue.name,
      status: item.status.dropDownValue,
      changeBy: item.statusChangedByEmployee.person.fullName,
      changeDate: item.statusChangedDate
    })).sort((a, b) => format(b.changeDate, "MM/dd/yyyy") - format(a.changeDate, "MM/dd/yyyy")); // Sort by date in ascending order
  }).finally(() => {
    loading.value = false;
  });
};

</script>
