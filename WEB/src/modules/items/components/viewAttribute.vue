<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important; max-width: 60vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Item Attribute Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Attribute Name</div>
                <div class="text-black">
                  {{ model.name ? model.name : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Field Type</div>
                <div class="text-black">
                  {{ model.fieldType ? model.fieldType : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created By</div>
                <div class="text-black">{{ model.createdBy.person.fullName }}</div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created Date</div>
                <div class="text-black">{{ model.createdOnUtc }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Updated By</div>
                <div class="text-black">{{ model.updatedBy.person.fullName }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Updated Date</div>
                <div class="text-black">{{ model.updatedOnUtc }}</div>
              </div>
            </div>
          </fieldset>
          <fieldset v-if="model.fieldType === 'Dropdown'" class="q-mb-lg">
            <legend>Item Attribute Value Info</legend>
            <q-table
              ref="tableRef"
              v-model:pagination="pagination"
              bordered
              class="no-shadow"
              :loading="loading"
              :rows="rows"
              :columns="columns"
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
                  <q-td style="width: 25%;">{{ props.row.text }}</q-td>
                  <q-td style="width: 15%;">{{ props.row.value }}</q-td>
                  <q-td style="width: 10%;" class="text-right">{{ props.row.sortOrder }}</q-td>
                  <q-td style="width: 15%;">{{ props.row.createdBy.person.fullName }}</q-td>
                  <q-td style="width: 10%;" class="text-center">{{ props.row.createdOnUtc }}</q-td>
                  <q-td style="width: 15%;">{{ props.row.updatedBy.person.fullName }}</q-td>
                  <q-td style="width: 10%;" class="text-center">{{ props.row.updatedOnUtc }}</q-td>
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

import itemSubcategoryAttributeService from "../itemSubCategoryAttributes.service";

// Common variables
const loading = ref(true);
const rows = ref([]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "text", label: "Text", field: "text", align: "left", sortable: true },
  { name: "value", label: "Value", field: "value", align: "left", sortable: true },
  { name: "sortOrder", label: "Sort Order", field: "sortOrder", align: "right", sortable: true },
  { name: "createdBy.person.fullName ", label: "Created By", field: "createdBy.person.fullName ", align: "left", sortable: true },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "center", sortable: true },
  { name: "updatedBy.person.fullName", label: "Updated By", field: "updatedBy.person.fullName", align: "left", sortable: true },
  { name: "updatedOnUtc", label: "Updated Date", field: "updatedOnUtc", align: "left", sortable: true }
]);

// Define model values
const model = ref({
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
// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// get item attribute details
const getItemCategory = () => {
  loading.value = true;
  itemSubcategoryAttributeService.getItemSubcategoryAttributeDetailsById(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rows.value = resp.itemSubCategoryAttributesValues.map(item => ({
      ...item,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

// On page rendering
onMounted(() => {
  getItemCategory();
});
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
