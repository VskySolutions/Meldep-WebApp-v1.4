<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1300px !important;max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div
          class="text-h2
          text-white"
          style="width: 98%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;"
        >
          {{ props.releaseVersion ? props.releaseVersion : props.name }}
        </div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset class="q-mb-lg">
              <legend>Change log</legend>
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
                    <q-td>{{ props.row.status }}</q-td>
                    <q-td>{{ props.row.changedBy }}</q-td>
                    <q-td>{{ props.row.changedDate }}</q-td>
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
import testcaseService from "modules/test-case/testCase.service";

// Common variables
const loading = ref(true);
const rows = ref([]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, releaseVersion: { type: String, default: "" }, name: { type: String, default: "" }});

const columns = [
  {
      name: "status",
      label: "Log",
      field: "status",
      align: "left"
  },
  {
      name: "changedBy",
      label: "Modified By",
      field: "changedBy",
      align: "left"
  },
  {
      name: "changedDate",
      label: "Modified On",
      field: "changedDate",
      align: "left"
  }
];

// get status log
const getStatusChangeLog = () => {
    loading.value = true;

    testcaseService.getStatusChangeLog(props.id)
        .then(resp => {
            rows.value = resp;
        })
        .finally(() => {
            loading.value = false;
        });
};

// On page rendering
onMounted(() => {
    getStatusChangeLog();
});
</script>
