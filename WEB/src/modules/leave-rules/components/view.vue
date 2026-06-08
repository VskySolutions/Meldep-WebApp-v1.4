<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.year }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Leave Rule Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md q-justify-center">
              <div class="col-3 offset-md-5">
                <div class="q-mb-xs text-black">Year: <span class="text-primary">{{ model.year }}</span></div>
              </div>
            </div>
            <q-table
              ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
              no-data-label="No data available" binary-state-sort
            >
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                  <q-td>{{ props.row.employmentTypeId }}</q-td>
                  <q-td>{{ props.row.casualLeaves }}</q-td>
                  <q-td>{{ props.row.sickLeaves }}</q-td>
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
import leaveRuleService from "../leaveRules.service";

const rows = ref([]);
const loading = ref(true);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "employmentTypeId", label: "Employment Type", field: "employmentTypeId", align: "left" },
  { name: "casualLeaves", label: "Casual Leave", field: "casualLeaves", align: "left" },
  { name: "sickLeaves", label: "Sick Leave", field: "sickLeaves", align: "left" }
]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  year: ""
});

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// On page rendering
onMounted(() => {
  getLeaveRule();
});

// get project details
const getLeaveRule = () => {
  loading.value = true;
  leaveRuleService.getLeaveRule(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rows.value = resp.leaveRuleLinesList.map(item => ({
      ...item,
      employmentTypeId: item.employmentType.dropDownValue,
      casualLeaves: item.casualLeaves,
      sickLeaves: item.sickLeaves
    }));
  }).finally(() => {
    loading.value = false;
  });
};

</script>
