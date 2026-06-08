<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="max-width: 95%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ model.title }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset class="q-mb-lg">
            <legend>SOP Process status change log</legend>
            <q-table
              ref="tableRef"
              bordered
              class="no-shadow"
              :loading="loading"
              :rows="rows"
              :columns="columns"
              row-key="id"
              separator="cell"
              binary-state-sort
              :rows-per-page-options="[20, 50, 100, 200, 500]"
            >
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props">
                  <q-td style="width: 35%;">{{ props.row.status }}</q-td>
                  <q-td style="width: 35%;">{{ props.row.changeBy }}</q-td>
                  <q-td style="width: 30%;">{{ props.row.changeDate }}</q-td>
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
import sopProcessService from "../sopProcess.service";

// Common variables
const loading = ref(true);
const rows = ref([]);
const model = ref({});

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

const columns = ref([
  { name: "status.dropDownValue", label: "Status", field: "status.dropDownValue", align: "left", sortable: false },
  { name: "changeBy", label: "Changed By", field: "changeBy", align: "left", sortable: false },
  { name: "changeDate", label: "Changed Date", field: "changeDate", align: "left", sortable: true }
]);

// get SOP Process details
const getSOPProcessInDetailsById = () => {
  loading.value = true;
  sopProcessService.getSOPProcessByIdInDetail(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rows.value = resp.sopProcessStatusLog.map(item => ({
      ...item,
      sOPProcessName: model.name,
      status: item.status.dropDownValue,
      changeBy: item.createdBy.person.fullName,
      changeDate: item.createdOnUtc
    }));
  }).finally(() => {
    loading.value = false;
  });
};

// On page rendering
onMounted(() => {
  getSOPProcessInDetailsById();
});
</script>
<style scoped>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
