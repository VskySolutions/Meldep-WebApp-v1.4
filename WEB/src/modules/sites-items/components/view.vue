<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important; max-width: 60vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.itemName }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Sites Item Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Item Subcategory</div>
                <div class="text-black">
                  {{ model.itemSubcategory.name ? model.itemSubcategory.name : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Item Name</div>
                <div class="text-black">
                  {{ model.itemName ? model.itemName : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12">
                <div class="q-mb-xs">Description</div>
                <div class="text-black RichTextEditor">
                  <p v-html="model.description ? model.description : '-'" />
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
            <legend>Sites Item Attribute Info</legend>
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
            >
              <!-- Header -->
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white text-left">
                  <q-th v-for="col in props.cols" :key="col.name">
                    {{ col.name }}
                  </q-th>
                </q-tr>
              </template>

              <!-- Body -->
              <template #body="props">
                <q-tr :props="props" :class="props.row.deleted ? 'hidden' : ''">
                  <q-td v-for="(col, colIndex) in props.cols" :key="col.name">
                    {{ props.row.sitesItemsAttributesList[colIndex]?.value || '-' }}
                  </q-td>
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

import sitesItemsService from "../sitesItems.service";
import itemSubcategoryAttributeService from "src/modules/items/itemSubCategoryAttributes.service";

// Common variables
const loading = ref(true);
const rows = ref([]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "itemSubCategoryAttributes.name ", label: "Item Subcategory Attributes Name", field: "itemSubCategoryAttributes.name ", align: "left", sortable: true },
  { name: "value", label: "Value", field: "value", align: "left", sortable: true }
]);

// Define model values
const model = ref({
  itemSubcategory: {
    id: "",
    name: ""
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
// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

const getSitesItemDetailsById = async (id) => {
  loading.value = true;
  try {
    const resp = await sitesItemsService.getSitesItemDetailsById(props.id);
    model.value = _.cloneDeep(resp);

    // Load attribute columns
    await getAllSitesItemSubCategoryAttributesListByItemSubCategoryId(model.value.itemSubcategory.id);

    const attributes = resp.sitesItemsAttributeList || [];

    // Group attributes by attributeId
    const attributeMap = attributes.reduce((map, item) => {
      if (!map[item.itemSubCategoryAttributeId]) map[item.itemSubCategoryAttributeId] = [];
      map[item.itemSubCategoryAttributeId].push(item);
      return map;
    }, {});

    // Find max rows
    const maxRows = Math.max(...Object.values(attributeMap).map(a => a.length), 0);

    // Build rows for the table
    rows.value = Array.from({ length: maxRows }, (_, rowIndex) => ({
      rowCounter: rowIndex,
      deleted: false,
      sitesItemsAttributesList: columns.value.map(col => {
        const attr = attributeMap[col.itemSubCategoryAttributeId]?.[rowIndex];
        return {
          id: attr?.id ?? null,
          itemSubCategoryAttributeId: col.itemSubCategoryAttributeId,
          value: attr?.value ?? null
        };
      })
    }));
  } finally {
    loading.value = false;
  }
};

const getAllSitesItemSubCategoryAttributesListByItemSubCategoryId = (subCategoryId) => {
  return itemSubcategoryAttributeService
    .getAllSitesItemSubCategoryAttributesListByItemSubCategoryId(subCategoryId)
    .then((resp) => {
      columns.value = resp.map(item => ({
        id: item.id,
        itemSubCategoryAttributeId: item.itemSubCategoryAttributeId,
        name: item.itemSubCategoryAttributes.name,
        align: "left"
      }));
    });
};

// On page rendering
onMounted(() => {
  getSitesItemDetailsById();
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
