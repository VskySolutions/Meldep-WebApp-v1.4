<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none dialog-scrollable-content" full-height position="right">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 90vw; max-width: 90vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Infra Account</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogCancel" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Account Services Info</legend>
              <div class="flex items-center justify-between q-my-md">
                <formSingleSelectDropdown
                  v-model="model.infraAccountId"
                  label="Account"
                  :options="infraAccountDropdownSingleSelect.list.value"
                  :filter="infraAccountDropdownSingleSelect.filter"
                  :error="v$.infraAccountId.$error"
                  :error-message="v$.infraAccountId.$errors[0]?.$message"
                />
                <div>
                  <q-btn color="primary" icon="o_add" label="Add Service" no-caps @click="onAdd" />
                </div>
              </div>
              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                virtual-scroll
                bordered
                class="no-shadow"
                :loading="loading"
                :rows="rows"
                :columns="columns"
                row-key="id"
                separator="cell"
                :rows-per-page-options="[20, 50, 100, 200, 500]"
                no-data-label="No data available"
                binary-state-sort
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}<span v-if="['itemTypeId', 'ownerShipTypeId', 'paymentTermId','name', 'startDateStr', 'priceInDollar'].includes(col.name)" class="required">*</span></q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :class="props.row.deleted ? 'hidden' : ''">
                    <q-td style="width: 5%;">
                      <formSingleSelectDropdown
                        v-model="props.row.itemTypeId"
                        :options="itemTypeDropdownSingleSelect.list.value"
                        :filter="itemTypeDropdownSingleSelect.filter"
                        :error="rowValidations[props.rowIndex]?.value?.itemTypeId.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.itemTypeId.$errors[0]?.$message"
                      />
                    </q-td>
                    <q-td style="width: 5%;">
                      <formSingleSelectDropdown
                        v-model="props.row.ownerShipTypeId"
                        :options="ownershipTypeDropdownSingleSelect.list.value"
                        :filter="ownershipTypeDropdownSingleSelect.filter"
                        :error="rowValidations[props.rowIndex]?.value?.ownerShipTypeId.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.ownerShipTypeId.$errors[0]?.$message"
                      />
                    </q-td>
                    <q-td style="width: 20%;">
                      <q-input
                        v-model="props.row.name"
                        outlined
                        stack-label
                        hide-bottom-space
                        :error="rowValidations[props.rowIndex]?.value?.name.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.name.$errors[0]?.$message"
                        @blur="rowValidations[props.rowIndex]?.value?.name.$touch"
                      />
                    </q-td>
                    <q-td style="width: 20%;">
                      <q-input
                        v-model="props.row.url"
                        outlined
                        stack-label
                        hide-bottom-space
                      />
                    </q-td>
                    <q-td style="width: 10%;">
                      <formDate
                        v-model="props.row.startDateStr"
                        :error="rowValidations[props.rowIndex]?.value?.startDateStr.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.startDateStr.$errors[0]?.$message"
                      />
                    </q-td>
                    <q-td style="width: 5%;">
                      <formSingleSelectDropdown
                        v-model="props.row.paymentTermId"
                        :options="paymentTermDropdownSingleSelect.list.value"
                        :filter="paymentTermDropdownSingleSelect.filter"
                        :error="rowValidations[props.rowIndex]?.value?.paymentTermId.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.paymentTermId.$errors[0]?.$message"
                      />
                    </q-td>
                    <q-td style="width: 14%;">
                      <q-input
                        v-model="props.row.priceInDollar"
                        outlined
                        stack-label
                        hide-bottom-space
                        prefix="$"
                        input-class="text-right"
                        inputmode="decimal"
                        :error="rowValidations[props.rowIndex]?.value?.priceInDollar.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.priceInDollar.$errors[0]?.$message"
                        class="break-error"
                        @blur="rowValidations[props.rowIndex]?.value?.priceInDollar.$touch"
                      />
                    </q-td>
                    <q-td style="width: 8%;">
                      <formSingleSelectDropdown
                        v-model="props.row.walletTypeId"
                        :options="infraWalletTypeDropdownSingleSelect.list.value"
                        :filter="infraWalletTypeDropdownSingleSelect.filter"
                      />
                    </q-td>
                    <q-td style="width: 8%;">
                      <q-input
                        v-model="props.row.walletNumber"
                        outlined
                        stack-label
                        hide-bottom-space
                      />
                    </q-td>
                    <q-td class="text-center" style="width: 5%;">
                      <q-icon
                        name="o_note_alt"
                        size="xs"
                        class="cursor-pointer q-mr-xs"
                      >
                        <q-tooltip>Add Instructions</q-tooltip>
                        <q-popup-edit
                          v-model="props.row.instructions"
                          anchor="top middle"
                          self="bottom middle"
                          buttons
                          persistent
                          label-set="Save"
                          label-cancel="Cancel"
                          class="instruction-popup"
                          @save="val => handleInstructionSave(props.row, val)"
                        >
                          <template #default="scope">
                            <div class="popup-container q-pa-sm" style="min-width: 260px;">
                              <q-btn
                                icon="o_close"
                                flat
                                round
                                dense
                                size="sm"
                                class="absolute-top-right"
                                @click="scope.cancel"
                              />
                              <div class="text-subtitle2 q-mb-xs">Instructions</div>
                              <div class="editor-wrapper">
                                <q-editor
                                  v-model="scope.value"
                                  :dense="$q.screen.lt.md"
                                  :toolbar="toolbar"
                                  :fonts="fonts"
                                  autogrow
                                />
                              </div>
                            </div>
                          </template>
                        </q-popup-edit>
                      </q-icon>
                      <q-icon name="o_delete_outline" size="xs" class="cursor-pointer" color="negative" @click="deleteRow(props.rowIndex)">
                        <q-tooltip>Delete</q-tooltip>
                      </q-icon>
                    </q-td>
                  </q-tr>
                  <q-separator />
                </template>
                <template #bottom-row>
                  <q-tr v-if="rows.filter(r => !r.deleted).length > 0" class="bg-grey-2 text-black">
                    <q-td colspan="6" class="text-right text-weight-bold">
                      Total Price:
                    </q-td>
                    <q-td class="text-right text-weight-bold">
                      ${{ totalPrice.toFixed(2) }}
                    </q-td>
                    <q-td />
                    <q-td />
                    <q-td />
                  </q-tr>
                </template>
              </q-table>
            </fieldset>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, uid, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, onMounted, computed } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import { isDate } from "validators/zw_validators.js";
import infraAccountServicesService from "../infraAccountServices.service";
// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// SOP Change :- Shared Dropdowns
import infraAccountModule from "src/modules/infra-account/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// Props values i.e. come from query string
// const props = defineProps({ id: { type: String, default: "" } });

// Define emits
const { dialogRef, onDialogCancel, onDialogOK } = useDialogPluginComponent();

// Common variables
const tableRef = ref();
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const loading = ref(false);
const processing = ref(false);
const rowValidations = ref([]);
const rows = ref([]);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "itemTypeId", label: "Item Type", field: "itemTypeId", align: "left", sortable: true },
  { name: "ownerShipTypeId", label: "Ownership Type", field: "ownerShipTypeId", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "url", label: "URL", field: "url", align: "left", sortable: true },
  { name: "startDateStr", label: "Start Date", field: "startDateStr", align: "left", sortable: true },
  { name: "paymentTermId", label: "Payment Term", field: "paymentTermId", align: "left", sortable: true },
  { name: "priceInDollar", label: "Price", field: "priceInDollar", align: "left", sortable: true },
  { name: "walletTypeId", label: "Wallet Type", field: "walletTypeId", align: "left", sortable: true },
  { name: "walletNumber", label: "Wallet Number", field: "walletNumber", align: "left", sortable: true }
]);

const model = ref({
  providerId: "",
  instructions: ""
});

// const getInfraAccount = async () => {
//   try {
//     loading.value = true;
//     const resp = await infraAccountServicesService.getInfraAccountDetails(props.id);
//     model.value = {
//       ...resp,
//       providerId: resp.provider?.id ?? null,
//       walletTypeId: resp.walletType?.id ?? null,
//       instructions: resp.instructions ?? ""
//     };

//     // Ensure dropdowns are loaded first
//     await Promise.all([
//       infraWalletTypeDropdownSingleSelect.load("Wallet Type"),
//       itemTypeDropdownSingleSelect.load("Account Item Type"),
//       ownershipTypeDropdownSingleSelect.load("Ownership Type"),
//       paymentTermDropdownSingleSelect.load("Payment Term")
//     ]);

//     rows.value = (resp.infraAccountServices ?? []).map(service => ({
//       ...service,
//       itemTypeId: service.itemType?.id ?? null,
//       ownerShipTypeId: service.ownerShipType?.id ?? null,
//       paymentTermId: service.paymentTerm?.id ?? null,
//       walletTypeId: service.walletType?.id ?? null,
//       startDateStr: service.startDate ?? null,
//       deleted: false,
//       flag: "Edit"
//     }));
//   } catch (error) {
//     console.error("Failed to load Infra Account:", error);
//   } finally {
//     loading.value = false;
//   }
// };

const totalPrice = computed(() =>
  rows.value.reduce((sum, row) =>
    sum + (!row.deleted ? (parseFloat(row.priceInDollar) || 0) : 0)
  , 0)
);

const rules = {
  infraAccountId: { required: helpers.withMessage("Account is required", required) }
};
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });
const decimalNumber = helpers.regex(/^\d+(\.\d{1,2})?$/);
const rowRules = {
  itemTypeId: { required: helpers.withMessage("Item type is required", required) },
  ownerShipTypeId: { required: helpers.withMessage("OwnerShip type is required", required) },
  paymentTermId: { required: helpers.withMessage("Payment term is required", required) },
  startDateStr: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  name: { required: helpers.withMessage("Name is required", required) },
  // url: {
  //   required: helpers.withMessage("URL is required", required),
  //   url: helpers.withMessage("Invalid URL", url)
  // },
  priceInDollar: {
    required: helpers.withMessage("Price is required", required),
    decimalNumber: helpers.withMessage(
      "Enter valid amount (max 2 decimal places)",
      decimalNumber
    )
  }
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const {
  infraAccountDropdownSingleSelect,
  infraWalletTypeDropdownSingleSelect,
  itemTypeDropdownSingleSelect,
  ownershipTypeDropdownSingleSelect,
  paymentTermDropdownSingleSelect
} = infraAccountModule();

function onAdd () {
  rows.value.unshift({
    id: uid(),
    itemTypeId: null,
    walletTypeId: null,
    walletNumber: "",
    ownerShipTypeId: null,
    paymentTermId: null,
    name: "",
    url: "",
    startDateStr: "",
    endDateStr: "",
    priceInDollar: "",
    instructions: "",
    flag: "New",
    deleted: false
  });
}

const deleteRow = (index) => {
  if (rows.value.filter(row => row.deleted === false).length > 1) {
    rows.value[index].deleted = true;
  } else {
    notifyError({ message: "Please add at least one row." });
  }
};

// add instruction action
function handleInstructionSave (row, instructions) {
  row.instructions = instructions;

  if (row.flag === "Edit") {
    onSaveInstructions(row.id, instructions);
  }
}

function onSaveInstructions (id, instructions) {
  setTimeout(function () {
    const payload = {
      instructions
    };

    infraAccountServicesService
      .addOrUpdateInstructions(id, payload)
      .then(() => {
        notifySuccess({ message: "Instruction is saved successfully." });
      });
  });
}

async function onSubmit () {
  try {
    let isValid = true;

    if (!await v$.value.$validate()) {
      return;
    }
    if (rows.value.length === 0) {
      notifyError({ message: "Add at least one service." });
      return;
    }
    const nonDeletedRows = rows.value.filter(row => !row.deleted);
    rowValidations.value = nonDeletedRows.map((row) =>
      useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true })
    );
    // Validate each row
    for (const [index, validation] of rowValidations.value.entries()) {
      if (validation?.value) {
        await validation.value.$touch();
        const isRowValid = await validation.value.$validate();
        if (!isRowValid) {
          isValid = false;
        }
      } else {
        console.error(`Validation object for row ${index} is undefined`);
        isValid = false;
      }
    }
    if (isValid) {
      const cleanedRows = nonDeletedRows.map(row => {
        const trimmedPrice = (row.priceInDollar ?? "").toString().trim();
        let priceInDollar;
        if (row.deleted === true) {
          priceInDollar = 0.0;
        } else {
          priceInDollar = trimmedPrice === "" ? null : parseFloat(trimmedPrice);
        }
        return {
          ...row,
          priceInDollar,
          deleted: row.deleted ?? ""
        };
      });
      const payload = {
        infraAccountId: model.value.infraAccountId,
        infraAccountServicesLines: cleanedRows
      };
      await infraAccountServicesService.saveInfraAccountServices(null, payload);
      notifySuccess({ message: "Infra Account and Services is saved successfully." });
      onDialogOK();
    }
  } catch (error) {
    console.error("Error in submitting the Infra Account:", error);
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

// watch(() => props.id, (newValue, oldValue) => {
//   if (newValue) {
//     getInfraAccount();
//   }
// }, { immediate: true });

// On page rendering
onMounted(() => {
  infraAccountDropdownSingleSelect.load();
  infraWalletTypeDropdownSingleSelect.load("Wallet Type"),
  itemTypeDropdownSingleSelect.load("Account Item Type"),
  ownershipTypeDropdownSingleSelect.load("Ownership Type"),
  paymentTermDropdownSingleSelect.load("Payment Term")
});
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.instruction-popup {
  width: 35vw;
  max-width: 500px;
  min-width: 320px;
}
</style>
