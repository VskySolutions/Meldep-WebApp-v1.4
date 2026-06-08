<template>
  <q-dialog ref="dialogRef" class="customDialog PlannerDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Customer Files</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogClose()" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="submitForm">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>File Info</legend>
              <div class="row justify-between items-center q-mb-md">
                <div class="row q-gutter-x-md items-center col">
                  <!-- Year Picker -->
                  <div class="col-2">
                    <label class="label q-mb-xs text-black">Year<span class="required">*</span></label>
                    <q-input v-model="model.year" outlined fill-input dense mask="####">
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" v-model="isPopupVisible" transition-show="scale" transition-hide="scale">
                            <q-date
                              ref="date3ref" v-model="model.year" default-view="Years" emit-immediately
                              minimal mask="YYYY" class="myDate" :options="onlyYears" :error="v$.year.$error"
                              :error-message="v$.year.$errors[0]?.$message" @update:model-value="onUpdateMv2" @blur="v$.year.$touch"
                            />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                  <!-- Customer Dropdown -->
                  <div class="col-2">
                    <label class="label q-mb-xs text-black">Customer<span class="required">*</span></label>
                    <q-select
                      v-model="model.customerId" clearable use-input outlined stack-label hide-bottom-space dense
                      :options="customerList" option-value="value" option-label="text" emit-value map-options :error="v$.customerId.$error" :error-message="v$.customerId.$errors[0]?.$message" @filter="filterFn2" @blur="v$.customerId.$touch"
                    >
                      <template #option="{ itemProps, opt }">
                        <q-item v-bind="itemProps">
                          <q-item-section>
                            <div class="row q-col-gutter-x-md items-center">
                              <span>{{ opt.text }}</span>
                            </div>
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
                  </div>
                </div>

                <!-- Add Button (Right Aligned) -->
                <div class="col-auto">
                  <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAdd" />
                </div>
              </div>

              <!-- File Table -->
              <q-table
                ref="tableRef" v-model:pagination="pagination" virtual-scroll bordered :loading="loading"
                :rows="rows" :columns="columns" row-key="id" separator="cell"
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th v-for="col in props.cols" :key="col.name" class="text-start">{{ col.label }}
                      <span v-if="['fileName', 'sortOrder'].includes(col.name)" class="required">*</span>
                    </q-th>
                    <q-th class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :class="props.row.deleted ? 'hidden' : ''">
                    <q-td style="width: 90%;">
                      <q-input v-model="props.row.fileName" outlined dense autogrow />
                    </q-td>
                    <q-td style="width: 5%;">
                      <q-input v-model="props.row.sortOrder" outlined dense type="number" />
                    </q-td>
                    <q-td class="text-center" style="width: 5%;">
                      <q-icon name="o_delete" size="xs" class="cursor-pointer text-red" @click="deleteRow(props.rowIndex)">
                        <q-tooltip>Delete</q-tooltip>
                      </q-icon>
                    </q-td>
                  </q-tr>
                </template>
              </q-table>
              <div class="row q-mt-md">
                <div class="col-4">
                  <div class="q-mb-xs text-black">Note</div>
                  <q-input v-model="model.note" autogrow rows="2" outlined stack-label hide-bottom-space :dense="true" />
                </div>
              </div>
            </fieldset>
          </div>
        </div>

        <q-card-actions align="center" class="q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogClose()" />
          <q-btn class="hidden" label="Save And Continue" color="primary" :disabled="isSubmitting" no-caps @click="submitForm('Draft')" />
          <q-btn label="Save And Close" color="primary" :disabled="isSubmitting" no-caps @click="submitForm('Submitted')" />
        </q-card-actions>
      </q-form>

    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, watch, onMounted } from "vue";
import { notifySuccess, zwConfirmLeave } from "assets/utils";
import _ from "lodash";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";

import customerFileService from "modules/customer-files/customerFile.service";
import customerService from "src/modules/customer/customer.service";

const { dialogRef, onDialogHide, onDialogCancel, onDialogOK } = useDialogPluginComponent();

// Common variables
const loading = ref(true);
const date3ref = ref(null);
const isPopupVisible = ref(false);
const isSubmitting = ref(false);
const model = ref({
  note: "",
  year: new Date().getFullYear().toString(),
  customerId: "",
  customerFilesLines: []
});

// Model validation rules
const rules = {
  year: { required: helpers.withMessage("Year is required", required) },
  customerId: { required: helpers.withMessage("Customer is required", required) }
};
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const onlyYears = (date) => { return true; };
const onUpdateMv2 = (val) => {
  model.value.year = val; // Update the reactive property with the selected year
  isPopupVisible.value = false; // Close the popup
};

function getCurrentMonthYear () {
  const today = new Date();
  const year = today.getFullYear();
  return `${year}`; // Format as 'Month-YYYY'
}

// Table variables
const tableRef = ref();
const rows = ref([]);
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "fileName", label: "File Name", field: "fileName", align: "left", sortable: false },
  { name: "sortOrder", label: "Document No", field: "sortOrder", align: "left", sortable: false }
]);

const isSaveDialog = ref(false);
const props = defineProps({ id: { type: String, default: "" } });
const customerList = ref([]);
const options2 = ref([]);
function getAllCustomerListForDropdown () {
  customerService.getAllClientListForDropdown().then((resp) => {
    const responseData = resp
      .map((item) => ({
        text: item.company
          ? item.company.name // Use Company name if available
          : `${item.person.firstName} ${item.person.lastName}`, // Otherwise, use Person name
        value: item.id
      }))
      .sort((a, b) => a.text.localeCompare(b.text));

    customerList.value = responseData;
    options2.value = responseData;
  });
}

// Search Customer for dropdown
function filterFn2 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      customerList.value = options2.value;
    } else {
      customerList.value = options2.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
  isSaveDialog.value = true; // Somefields Changed (Are you sure Pop-up)
}

const getCustomerFileDetailsById = () => {
  loading.value = true;
  customerFileService.getCustomerFileDetailsById(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rows.value = resp.customerFilesLines.map((item) => ({
      id: item.id,
      sortOrder: item.sortOrder,
      fileName: item.fileName,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

const onAdd = () => {
  rows.value.push({ fileName: "", sortOrder: rows.value.length + 1, customerId: model.value.customerId, year: model.value.year });
  isSaveDialog.value = true; // Somefields Changed (Are you sure Pop-up)
};

const deleteRow = (index) => {
  rows.value[index].deleted = true;
  isSaveDialog.value = true; // Somefields Changed (Are you sure Pop-up)
};

const submitForm = async () => {
  if (await v$.value.$validate()) {
    model.value.customerFilesLines = rows.value;
    customerFileService.saveCustomerFiles(props.id, model.value).then((resp) => {
      notifySuccess({ message: "Files saved successfully!" });
      // onDialogHide(); // Close the popup
      onDialogOK();
    });
  }
};

function onDialogClose () {
  if (isSaveDialog.value === true) {
    zwConfirmLeave({ data: "" }, () => {
      onDialogCancel();
    }, () => {
    });
  } else {
    onDialogCancel();
  }
}

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getCustomerFileDetailsById();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  getCurrentMonthYear();
  getAllCustomerListForDropdown();
  if (!props.id) {
    for (let i = 1; i <= 3; i++) {
      rows.value.push({
        fileName: "",
        sortOrder: i, // Incrementing sortOrder from 1 to 5
        customerId: model.value.customerId,
        year: model.value.year
      });
    }
  }
});

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
</style>
