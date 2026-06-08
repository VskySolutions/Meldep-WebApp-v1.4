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
          <!-- <q-card class="card-header with-tools headerBasic"> -->
          <fieldset>
            <legend>Item Category Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Category Name</div>
                <div class="text-black">
                  {{ model.name ? model.name : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Prefix</div>
                <div class="text-black">
                  {{ model.prefix ? model.prefix : "-" }}
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
          <fieldset class="q-mb-lg">
            <legend>Item Subcategory Info</legend>
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
                  <q-td>{{ props.row.name }}</q-td>
                  <q-td>{{ props.row.prefix }}</q-td>
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
import itemCategoryService from "../itemCategory.service";

// Common variables
const loading = ref(true);
const rows = ref([]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "name", label: "Item Subcategory", field: "name", align: "left", sortable: true },
  { name: "prefix", label: "Item Prefix", field: "prefix", align: "left", sortable: true }
]);

// Define model values
const model = ref({
  name: "",
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

// get item category details
const getItemCategory = () => {
  loading.value = true;
  itemCategoryService.getItemCategoryDetailsById(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rows.value = resp.itemSubcategory.map(item => ({
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
