<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1300px !important;max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="width: 98%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset class="q-mb-lg">
              <legend>Change log</legend>
              <q-table
                ref="tableRef" bordered class="no-shadow" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
                binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]">
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                    <!-- <q-td style="width: 15%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.module }}</q-td> -->
                    <!-- <q-td style="width: 15%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.sub_Module }}</q-td> -->
                    <q-td style="width: 5%;">{{ props.row.columnName  }}</q-td>
                    <q-td style="width: 15%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.columnValue }}</q-td>
                    <q-td style="width: 5%;">{{ props.row.lastModifiedBy  }}</q-td>
                    <q-td style="width: 5%;">{{ props.row.lastModifiedOnUtc  }}</q-td>
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
import siteService from "modules/sites/site.service";

// Common variables
const loading = ref(true);
const rows = ref([]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

const columns = ref([
  { name: "columnName", label: "Field Name", field: "columnName", align: "left", sortable: false },
  { name: "columnValue", label: "Log", field: "columnValue", align: "left", sortable: false },
  { name: "lastModifiedBy", label: "Modified By", field: "lastModifiedBy", align: "left", sortable: false },
  { name: "lastModifiedOnUtc", label: "Modified On", field: "lastModifiedOnUtc", align: "left", sortable: false }
]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, name: { type: String, default: "" }, columnName: { type: String, default: "" } });

// On page rendering
onMounted(() => {
  getSiteModifiedLogs();
});

// get project details
const getSiteModifiedLogs = () => {
  loading.value = true;
  siteService.getSiteModifiedLogs(props.id, props.columnName).then((resp) => {
    rows.value = resp.map(item => ({
      ...item,
      id: item.id,
      columnValue: item.columnValue,
      columnName: item.columnName,
      lastModifiedBy: item.user?.person?.fullName,
      lastModifiedOnUtc: item.lastModifiedOnUtc,
      lastModifiedOnUtc: item.lastModifiedOnUtc
    })).sort((a, b) => new Date(b.lastModifiedOnUtc) - new Date(a.lastModifiedOnUtc));
  }).finally(() => {
    loading.value = false;
  });
};

</script>
