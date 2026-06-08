<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height="" position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">View Inventory</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <!-- <q-form greedy @submit.prevent.stop="onSubmit"> -->
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Inventory Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Device Type</div>
                <div class="text-black">{{ model.itemType.name }}</div>
              </div>
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs ">Device No</div>
                <div class="text-black">{{ model.inventorycode }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Service Code</div>
                <div class=" text-black">{{ model.serviceCode }}</div>
              </div>
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Model Name/Number</div>
                <div class="text-black">{{ model.modelNameORNumber }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Purchase Date</div>
                <div class=" text-black">{{ model.dateofPurchase }}</div>
              </div>
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Inventory Status :</div>
                <div class="text-black">{{ model.inventoryStatus.dropDownValue ? model.inventoryStatus.dropDownValue:'-' }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Office Location</div>
                <div class=" text-black">{{ model.officeLocation.dropDownValue ? model.officeLocation.dropDownValue : '-' }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-xs-12 col-sm-12 col-md-12">
                <div class="q-mb-xs">Description</div>
                <div class="text-black RichTextEditor">
                  <p v-html="model.description" />
                </div>
              </div>
              <div class="col-xs-12 col-sm-12 col-md-12">
                <div class="q-mb-xs">Note</div>
                <div class="text-black RichTextEditor">
                  <p v-html="model.notes" />
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-xs-12 col-sm-6 col-md-6 q-mb-md">
                <div class="q-mb-xs">Created By</div>
                <div class=" text-black">{{ model.createdBy.person.fullName }}</div>
              </div>
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created Date</div>
                <div class=" text-black">{{ model.createdOnUtc }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-xs-12 col-sm-6 col-md-6 q-mb-md">
                <div class="q-mb-xs">Updated By</div>
                <div class=" text-black">{{ model.updatedBy.person.fullName }}</div>
              </div>
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Updated Date</div>
                <div class=" text-black">{{ model.updatedOnUtc }}</div>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <legend>Warranty/Guaranty</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Warranty</div>
                <div class="text-black">{{ model.warranty }}</div>
              </div>
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Warranty Date</div>
                <div class="text-black">{{ model.warrantyExpiryDate }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Guaranty</div>
                <div class="text-black">{{ model.guaranty }}</div>
              </div>
            </div>
          </fieldset>
          <fieldset v-if="model.itemType.name === 'Laptop' || model.itemType.name === 'Desktop'">
            <legend>Specification</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Operating System</div>
                <div class="text-primary text-black">{{ model.operatingSystem }}</div>
              </div>
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Processor Type</div>
                <div class="text-black">{{ model.processorType }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Memory OR RAM</div>
                <div class="text-black">{{ model.memoryORRAM }}</div>
              </div>
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">HardDrive OR Storage Capacity</div>
                <div class="text-black">{{ model.hardDriveORStorageCapacity }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Graphics Card</div>
                <div class=" text-black">{{ model.graphicsCard }}</div>
              </div>
              <div class="col-xs-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Virus Protection</div>
                <div class="text-black">{{ model.virusProtection }}</div>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <legend>Assign Item to Employees Info</legend>
            <q-table
              ref="tableRef"
              v-model:pagination="pagination"
              bordered class="no-shadow"
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
                <q-tr :props="props">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                  <q-td>{{ props.row.employeeName }}</q-td>
                  <q-td>{{ props.row.assignDate }}</q-td>
                  <q-td>{{ props.row.returnDate }}</q-td>
                  <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 40%;">{{ props.row.returnReson }}</q-td>
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
import inventoryService from "modules/inventory/inventory.service";

const rows = ref([]);
const loading = ref(true);
const columns = ref([
  { name: "employeeName", label: "Assign To", field: "employeeName", align: "left", sortable: true },
  { name: "assignDate", label: "Assigned Date", field: "assignDate", align: "left", sortable: true },
  { name: "returnDate", label: "Return Date", field: "returnDate", align: "left", sortable: true },
  { name: "returnReson", label: "Reason", field: "returnReson", align: "left", sortable: true }
]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  inventorycode: "",
  serviceCode: "",
  warranty: "",
  guaranty: "",
  operatingSystem: "",
  processorType: "",
  modelNameORNumber: "",
  dateofPurchase: "",
  memoryORRAM: "",
  graphicsCard: "",
  hardDriveORStorageCapacity: "",
  virusProtection: "",
  itemType: {
    name: ""
  },
  inventoryStatus: {
    dropDownValue: ""
  },
  officeLocation: {
    dropDownValue: ""
  },
  createdOnUtc: "",
  updatedOnUtc: "",
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

// get project details
const getInventory = () => {
  loading.value = true;
  inventoryService.getInventory(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rows.value = resp.inventoryAssignmentList.map(item => ({
      ...item,
      employeeName: item.employee.person.fullName,
      assignDateStr: item.assignDate,
      returnDateStr: item.returnDate
    }));
  }).finally(() => {
    loading.value = false;
  });
};

// On page rendering
onMounted(() => {
  getInventory();
});
</script>
