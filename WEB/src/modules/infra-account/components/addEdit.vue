<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none PlannerDialog" full-height position="right">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Infra Account</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogCancel" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Account Info</legend>
              <div class="row q-col-gutter-x-md q-mb-xs">
                <formSingleSelectDropdown
                  v-model="model.providerId"
                  label="Provider"
                  :options="providerTypeDropdownSingleSelect.list.value"
                  :filter="providerTypeDropdownSingleSelect.filter"
                  :error="v$.providerId.$error"
                  :error-message="v$.providerId.$errors[0]?.$message"
                />
                <div class="col-xxl-12 col-md-4 col-lg-4">
                  <div class="text-black">Name<span class="required">*</span></div>
                  <q-input
                    v-model="model.name"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :error="v$.name.$error"
                    :error-message="v$.name.$errors[0]?.$message"
                    @click="v$.name.$touch"
                  />
                </div>
                <div class="col-12 col-md-4 col-lg-4">
                  <div class="text-black">Customer Id<span class="required">*</span></div>
                  <q-input
                    v-model="model.customerId"
                    outlined
                    :error="v$.customerId.$error"
                    :error-message="v$.customerId.$errors[0]?.$message"
                    @blur="v$.customerId.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-xs">
                <div class="col-12 col-md-4 col-lg-4">
                  <div class="text-black">URL<span class="required">*</span></div>
                  <q-input
                    v-model="model.url"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    type="url"
                    :error="v$.url.$error"
                    :error-message="v$.url.$errors[0]?.$message"
                    @blur="v$.url.$touch"
                  />
                </div>
                <div class="col-12 col-md-2 col-lg-2">
                  <div class="text-black">Credit Card Last 4 Digits<span class="required">*</span></div>
                  <q-input
                    v-model="model.ccLast4Digits"
                    outlined
                    maxlength="4"
                    inputmode="numeric"
                    :error="v$.ccLast4Digits.$error"
                    :error-message="v$.ccLast4Digits.$errors[0]?.$message"
                    @blur="v$.ccLast4Digits.$touch"
                  />
                </div>
                <formSingleSelectDropdown
                  v-model="model.walletTypeId"
                  label="Wallet Type"
                  :required="false"
                  :options="infraWalletTypeDropdownSingleSelect.list.value"
                  :filter="infraWalletTypeDropdownSingleSelect.filter"
                  :wrapperClass="'col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12'"
                />
                <div class="col-12 col-md-3 col-lg-3">
                  <div class="text-black">Wallet Number</div>
                  <q-input
                    v-model="model.walletNumber"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md">
                <div class="col-12">
                  <div class="text-black">
                    <label>Instructions</label>
                  </div>

                  <div class="form-group editor-wrapper">
                    <q-editor
                      v-model="model.instructions"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset>
              <legend>Account Services Info</legend>
              <div class="flex items-center justify-end q-my-xs">
                <q-btn color="primary" icon="o_add" label="Add Service" no-caps @click="onAdd" />
              </div>
              <div class="table-scroll-container">
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
                      <q-td style="width: 6%;">
                        <formSingleSelectDropdown
                          v-model="props.row.itemTypeId"
                          :options="itemTypeDropdownSingleSelect.list.value"
                          :filter="itemTypeDropdownSingleSelect.filter"
                          :error="rowValidations[props.rowIndex]?.value?.itemTypeId.$error"
                          :error-message="rowValidations[props.rowIndex]?.value?.itemTypeId.$errors[0]?.$message"
                        />
                      </q-td>
                      <q-td style="width: 6%;">
                        <formSingleSelectDropdown
                          v-model="props.row.ownerShipTypeId"
                          :options="ownershipTypeDropdownSingleSelect.list.value"
                          :filter="ownershipTypeDropdownSingleSelect.filter"
                          :error="rowValidations[props.rowIndex]?.value?.ownerShipTypeId.$error"
                          :error-message="rowValidations[props.rowIndex]?.value?.ownerShipTypeId.$errors[0]?.$message"
                        />
                      </q-td>
                      <q-td style="min-width: 200px;width: 18%;">
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
                      <q-td style="min-width: 250px;width: 22%;">
                        <q-input
                          v-model="props.row.url"
                          outlined
                          stack-label
                          hide-bottom-space
                        />
                      </q-td>
                      <q-td style="min-width: 150px;width: 12%;">
                        <formDate
                          v-model="props.row.startDateStr"
                          :error="rowValidations[props.rowIndex]?.value?.startDateStr.$error"
                          :error-message="rowValidations[props.rowIndex]?.value?.startDateStr.$errors[0]?.$message"
                        />
                      </q-td>
                      <q-td style="width: 6%;">
                        <formSingleSelectDropdown
                          v-model="props.row.paymentTermId"
                          :options="paymentTermDropdownSingleSelect.list.value"
                          :filter="paymentTermDropdownSingleSelect.filter"
                          :error="rowValidations[props.rowIndex]?.value?.paymentTermId.$error"
                          :error-message="rowValidations[props.rowIndex]?.value?.paymentTermId.$errors[0]?.$message"
                        />
                      </q-td>
                      <q-td style="min-width: 120px;width: 10%;">
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
                      <q-td style="width: 6%;">
                        <formSingleSelectDropdown
                          v-model="props.row.walletTypeId"
                          :options="infraWalletTypeDropdownSingleSelect.list.value"
                          :filter="infraWalletTypeDropdownSingleSelect.filter"
                        />
                      </q-td>
                      <q-td style="width: 9%;">
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
                            anchor="center middle"
                            self="center middle"
                            buttons
                            persistent
                            label-set="Save"
                            label-cancel="Cancel"
                            class="instruction-popup"
                            @save="val => handleInstructionSave(props.row, val)"
                          >
                            <template #default="scope">
                              <div class="popup-container q-pa-sm">
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
                                    class="fixed-editor"
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
                    <q-tr v-if="rows.length" class="bg-grey-2 text-black">
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
              </div>
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
import { required, helpers, url } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import { zwConfirmDelete, notifySuccess, notifyError } from "assets/utils";
import { isDate } from "validators/zw_validators.js";
import infraAccountService from "../infraAccount.service";
import infraAccountServicesService from "src/modules/infra-account-services/infraAccountServices.service";

// SOP Change :- Shared Dropdowns
import infraAccountModule from "src/modules/infra-account/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// Common variables
const tableRef = ref();
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const loading = ref(false);
const processing = ref(false);
const rowValidations = ref([]);
const rows = ref([]);
// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, selectedCustomerType: { type: String, default: "" } });

const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "itemTypeId", label: "Item Type", field: "itemTypeId", align: "left", sortable: true },
  { name: "ownerShipTypeId", label: "Ownership Type", field: "ownerShipTypeId", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "url", label: "URL", field: "url", align: "left", sortable: true },
  { name: "startDateStr", label: "Start Date", field: "startDateStr", align: "left", sortable: true },
  { name: "paymentTermId", label: "Payment Term", field: "paymentTermId", align: "left", sortable: true },
  { name: "priceInDollar", label: "Price", field: "priceInDollar", align: "right", sortable: true },
  { name: "walletTypeId", label: "Wallet Type", field: "walletTypeId", align: "left", sortable: true },
  { name: "walletNumber", label: "Wallet Number", field: "walletNumber", align: "left", sortable: true }
]);
// Define emits
const { dialogRef, onDialogCancel, onDialogOK } = useDialogPluginComponent();
const model = ref({
  providerId: "",
  instructions: ""
});

const getInfraAccount = async () => {
  try {
    loading.value = true;
    const resp = await infraAccountService.getInfraAccountDetails(props.id);
    model.value = {
      ...resp,
      providerId: resp.provider?.id ?? null,
      walletTypeId: resp.walletType?.id ?? null,
      instructions: resp.instructions ?? ""
    };

    // Ensure dropdowns are loaded first
    await Promise.all([
      infraWalletTypeDropdownSingleSelect.load("Wallet Type"),
      itemTypeDropdownSingleSelect.load("Account Item Type"),
      ownershipTypeDropdownSingleSelect.load("Ownership Type"),
      paymentTermDropdownSingleSelect.load("Payment Term")
    ]);

    rows.value = (resp.infraAccountServices ?? []).map(service => ({
      ...service,
      itemTypeId: service.itemType?.id ?? null,
      ownerShipTypeId: service.ownerShipType?.id ?? null,
      paymentTermId: service.paymentTerm?.id ?? null,
      walletTypeId: service.walletType?.id ?? null,
      startDateStr: service.startDate ?? null,
      instructions: service.instructions ?? null,
      deleted: false,
      flag: "Edit"
    }));
  } catch (error) {
    console.error("Failed to load Infra Account:", error);
  } finally {
    loading.value = false;
  }
};

const totalPrice = computed(() => {
  return rows.value.reduce((sum, row) => {
    const price = parseFloat(row.priceInDollar) || 0;
    return sum + price;
  }, 0);
});

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
  const row = rows.value[index];

  const performDelete = () => {
    if (rows.value.filter(r => r.deleted === false).length > 1) {
      row.deleted = true;
      row.flag = "Delete";
    } else {
      notifyError({ message: "Please add at least one row." });
    }
  };

  // New row → direct delete
  if (row.flag === "New") {
    performDelete();
  } else {
    zwConfirmDelete(
      { data: `${row.itemType?.dropDownValue || ""}, ${row.name}` },
      () => {
        performDelete();
      },
      () => {}
    );
  }
};

// add instruction action
function handleInstructionSave (row, instructions) {
  row.instructions = instructions;

  if (row.flag === "Edit") {
    onSaveInstructions(row.id, instructions);
  }
}

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const {
  providerTypeDropdownSingleSelect,
  infraWalletTypeDropdownSingleSelect,
  itemTypeDropdownSingleSelect,
  ownershipTypeDropdownSingleSelect,
  paymentTermDropdownSingleSelect
} = infraAccountModule();

// ====================================================================
// validation
// ====================================================================
const fourDigits = helpers.regex(/^\d{4}$/);
const numeric = helpers.regex(/^[0-9]+$/);

const rules = {
  providerId: { required: helpers.withMessage("Provider is required", required) },
  name: { required: helpers.withMessage("Name is required", required) },
  url: {
    required: helpers.withMessage("URL is required", required),
    url: helpers.withMessage("Invalid URL", url)
  },
  customerId: {
    required: helpers.withMessage("Customer id is required", required)
  },
  ccLast4Digits: {
    required: helpers.withMessage("Credit card last 4 digits are required", required),
    numeric: helpers.withMessage(
      "Only numeric values are allowed",
      numeric
    ),
    fourDigits: helpers.withMessage(
      "Must be exactly 4 digits",
      fourDigits
    )
  }
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
  priceInDollar: {
    required: helpers.withMessage("Price is required", required),
    decimalNumber: helpers.withMessage(
      "Enter valid amount (max 2 decimal places)",
      decimalNumber
    )
  }
};

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
  processing.value = true
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
      const cleanedRows = rows.value.map(row => {
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
        name: model.value.name,
        providerId: model.value.providerId,
        walletTypeId: model.value.walletTypeId,
        walletNumber: model.value.walletNumber,
        url: model.value.url,
        customerId: model.value.customerId,
        ccLast4Digits: model.value.ccLast4Digits,
        instructions: model.value.instructions,
        infraAccountServicesList: cleanedRows
      };
      await infraAccountService.saveInfraAccount(props.id, payload);
      notifySuccess({ message: "Infra Account and Services is saved successfully." });
      onDialogOK();
    }
  } catch (error) {
    console.error("Error in submitting the Infra Account and Services:", error);
  } finally {
    processing.value = false
  }
}

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getInfraAccount();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  providerTypeDropdownSingleSelect.load("Account Provider Type");
  infraWalletTypeDropdownSingleSelect.load("Wallet Type");
  itemTypeDropdownSingleSelect.load("Account Item Type");
  ownershipTypeDropdownSingleSelect.load("Ownership Type");
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
</style>
