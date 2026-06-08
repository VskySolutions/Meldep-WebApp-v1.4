<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1400px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Vendor Account</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Vendor Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black text-black">Vendor Name<span class="required">*</span></div>
                  <div>
                    <q-input
                      v-model="model.vendorName"
                      outlined
                      stack-label
                      hide-bottom-space
                      maxlength="100"
                      :dense="true"
                      :error="v$.vendorName.$error"
                      :error-message="v$.vendorName.$errors[0]?.$message"
                      @click="v$.vendorName.$touch"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Phone Number</div>
                  <div v-if="model.countryId === baseCountryId">
                    <q-input
                      v-model="model.vendor_Phone"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      mask="(###)-###-####"
                    />
                  </div>
                  <div v-else>
                    <q-input
                      v-model="model.vendor_Phone"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      mask="##########"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Email Address</div>
                  <div><q-input
                    v-model="model.vendor_Email"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    type="email"
                  />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Address 1</div>
                  <div>
                    <q-input
                      v-model="model.addressLine1"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      hint="Street name/Building number."
                    />
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Address 2</div>
                  <div>
                    <q-input
                      v-model="model.addressLine2"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      hint="Apartment/Unit/Suite"
                    />
                  </div>
                </div>
                <formSingleSelectDropdown
                  v-model="model.countryId"
                  label="Country"
                  :required="false"
                  :options="countryNameDropdownSingleSelect.list.value"
                  :filter="countryNameDropdownSingleSelect.filter"
                />
                <formSingleSelectDropdown
                  v-model="model.stateProvinceId"
                  label="State"
                  :required="false"
                  :options="stateNameDropdownSingleSelect.list.value"
                  :filter="stateNameDropdownSingleSelect.filter"
                />
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">City</div>
                  <div>
                    <q-input
                      v-model="model.city" outlined stack-label hide-bottom-space :dense="true"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }}</div>
                  <div v-if="model.countryId === baseCountryId">
                    <q-input
                      v-model="model.zipCode" outlined hide-bottom-space :dense="true" mask="#####"
                    />
                  </div>
                  <div v-else>
                    <q-input
                      v-model="model.zipCode" outlined hide-bottom-space :dense="true" mask="######"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset>
              <legend>Vendor's Owner Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <formSingleSelectDropdown
                  v-model="model.personId"
                  label="Owner Name"
                  :required="false"
                  :options="personNameDropdownSingleSelect.list.value"
                  :filter="personNameDropdownSingleSelect.filter"
                >
                  <template #after>
                    <q-icon
                      name="fa-solid fa-user-plus"
                      color="primary"
                      class="cursor-pointer q-ml-sm"
                      @click="handleAddPerson(null, refreshPersonNameDropdown)"
                    >
                      <q-tooltip>Add new Owner</q-tooltip>
                    </q-icon>
                  </template>
                </formSingleSelectDropdown>
              </div>
            </fieldset>
            <fieldset class="q-mt-md">
              <legend>Payment Profile</legend>
              <div>
                <div class="q-pa-none" style="display: flex; justify-content: flex-end;">
                  <q-btn class="q-mb-md" color="primary" icon="o_add" label="Add Profile" no-caps @click="onAddPaymentProfile" />
                </div>
                <q-table
                  ref="tableRef"
                  virtual-scroll
                  bordered
                  class="no-shadow"
                  :loading="loading"
                  :rows="bankrows"
                  :columns="bankcolumns"
                  row-key="id"
                  separator="cell"
                  binary-state-sort
                  :rows-per-page-options="[20, 50, 100, 200, 500]"
                >
                  <template #header="">
                    <q-tr class="bg-primary text-white">
                      <q-th v-for="col in filteredColumns" :key="col.name">{{ col.label }}<span v-if="['bankAccount'].includes(col.name)" class="required">*</span></q-th>
                      <q-th v-if="hasColumn === 'upI_ID'" :colspan="5">
                        UPI Number
                      </q-th>
                      <q-th v-else-if="hasColumn === 'by_cash'" :colspan="5" />
                      <q-th auto-width class="text-center">Actions</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr v-show="!props.row.hidden">
                      <q-td class="text-left" style="width: 5%; white-space: normal; overflow-wrap: break-word;">
                      <formSingleSelectDropdown
                        v-model="props.row.paymentTypeId"
                        :options="vendorAccountTypeDropdownSingleSelect.list.value"
                        :filter="vendorAccountTypeDropdownSingleSelect.filter"
                        :error="rowValidations[props.rowIndex]?.value?.paymentTypeId.$error"
                        :errorMessage="rowValidations[props.rowIndex]?.value?.paymentTypeId.$errors[0]?.$message"
                        @update:model-value="(val) => handlePaymentTypeChange(val, props.row)"
                       />
                      </q-td>
                      <template v-if="getPaymentText(props.row.bankAccount) === 'Bank Account'">
                        <q-td style="width: 30%;">
                          <q-input
                            v-model="props.row.bankName"
                            outlined hide-bottom-space
                            :dense="true" autogrow :rules="[(val) => (val) => /^[A-Za-z\s]+$/.test(val) || 'Bank name must contain only letters']"
                          />
                        </q-td>

                        <q-td style="width: 10%;">
                          <q-input
                            v-model="props.row.accountNumber"
                            outlined hide-bottom-space
                            :dense="true"
                            autogrow
                            mask="##################"
                            hint="##################"
                            :rules="[(val) => (val) => val && val.length >= 8 && val.length <= 16 || 'Account number must be 8-16 digits']"
                          />
                        </q-td>

                        <q-td style="width: 25%;">
                          <q-input
                            v-model="props.row.ifscCode"
                            outlined
                            hide-bottom-space
                            :dense="true"
                            autogrow
                            mask="AAAA0######"
                            hint="AAAA0######" :rules="[(val) => (val) => /^[A-Z]{4}0[A-Z0-9]{6}$/.test(val) || 'IFSC Code is Invalid']"
                          />
                        </q-td>

                        <q-td style="width: 5%;">
                        <formSingleSelectDropdown
                          v-model="props.row.accountTypeId"
                          :options="accountTypeDropdownSingleSelect.list.value"
                          :filter="accountTypeDropdownSingleSelect.filter"
                        />
                        </q-td>
                        <q-td style="width: 25%;">
                          <q-input
                            v-model="props.row.branchName"
                            outlined hide-bottom-space
                            :dense="true"
                            autogrow
                          />
                        </q-td>
                      </template>
                      <q-td v-else-if="getPaymentText(props.row.bankAccount) === 'UPI'" :colspan="5" style="width: 25%;">
                        <q-input
                          v-model="props.row.upI_ID"
                          outlined hide-bottom-space
                          :dense="true"
                          autogrow :rules="[(val) => (val) => /^[a-zA-Z0-9._]+@[a-zA-Z]+$/.test(val) || 'UPI Number is Invalid']"
                        />
                      </q-td>
                      <q-td v-else :colspan="5" style="width: 25%;" />
                      <!-- Actions -->
                      <q-td auto-width class="text-center actions" style="width: 5%;">
                        <q-icon name="o_delete_outline" class="cursor-pointer" size="xs" color="negative" @click="onDeletePaymentProfile(props.rowIndex)">
                          <q-tooltip>Delete</q-tooltip>
                        </q-icon>
                      </q-td>
                    </q-tr>
                  </template>
                </q-table>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import _ from "lodash";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";

import expenseVendorBankAccountService from "modules/finance-expense-vendors/financeExpenseVendors.service";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

// Shared Dropdowns
import commonModule from "src/modules/common/utils/dropdowns.js";
import personModule from "src/modules/person/utils/dropdowns.js";

// Shared Dropdowns
import financeExpenseVendorsModule from "src/modules/finance-expense-vendors/utils/dropdowns.js";

// Shared Person Dialogs
import {
  initPersonDialogs,
  onPersonAddAndReturnPersonId
} from "src/modules/person/utils/dialogs.js";

const loading = ref(true);
const processing = ref(false);
const baseCountryId = process.env.BASE_COUNTRY_ID;
const $q = useQuasar();
const bankrows = ref([]);
const rowValidations = ref([]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

const bankcolumns = ref([
  { name: "bankAccount", label: "Payment Type", field: "bankAccount", align: "left", sortable: true },
  { name: "bankName", label: "Bank Name", field: "bankName", align: "left", sortable: true },
  { name: "accountNumber", label: "Bank Account Number", field: "accountNumber", align: "left", sortable: true },
  { name: "ifscCode", label: "IFSC Code", field: "ifscCode", align: "left", sortable: true },
  { name: "accountTypeId", label: "Account Type", field: "accountTypeId", align: "left", sortable: true },
  { name: "branchName", label: "Branch Location", field: "branchName", align: "left", sortable: true }
]);

function updateBankColumns (paymentText) {
  bankcolumns.value = [ // Always reset before updating
    { name: "bankAccount", label: "Payment Type", field: "bankAccount", align: "left", sortable: true }
  ];

  if (paymentText === "Bank Account") {
    bankcolumns.value.push(
      { name: "bankName", label: "Bank Name", field: "bankName", align: "left", sortable: true },
      { name: "accountNumber", label: "Bank Account Number", field: "accountNumber", align: "left", sortable: true },
      { name: "ifscCode", label: "IFSC Code", field: "ifscCode", align: "left", sortable: true },
      { name: "accountTypeId", label: "Account Type", field: "accountTypeId", align: "left", sortable: true },
      { name: "branchName", label: "Branch Location", field: "branchName", align: "left", sortable: true }
    );
  } else if (paymentText === "UPI") {
    bankcolumns.value.push(
      { name: "upI_ID", label: "UPI Number", field: "upI_ID", align: "left", sortable: true, colspan: 5 }
    );
  } else {
    bankcolumns.value.push(
      { name: "by_cash", label: "", field: "", align: "left", sortable: false }
    );
  }
}

// Define model values
const model = ref({
  vendorName: "",
  countryId: null,
  vendor_Phone: "",
  zipCode: ""
});

// Vendor Info - Validation Rules
let rules = {
  vendorName: { required: helpers.withMessage("Vendor name is required", required) }
};

let v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Payment Profile - Validation Rules
const rowRules = {
  paymentTypeId: { required: helpers.withMessage("Payment Type is required", required) }
};

function syncRowValidations () {
  rowValidations.value = bankrows.value.map(row =>
    !row.deleted ? useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true }) : null
  );
}

const filteredColumns = computed(() => {
  return bankcolumns.value.filter(col => col.name !== "upI_ID" && col.name !== "by_cash");
});

const hasColumn = computed(() => {
  if (bankcolumns.value.some(col => col.name === "upI_ID")) {
    return "upI_ID";
  } else if (bankcolumns.value.some(col => col.name === "by_cash")) {
    return "by_cash";
  }
  return null;
});

// get Vendor details on edit mode
const getVendor = () => {
  loading.value = true;
  expenseVendorBankAccountService.getVendor(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    if (resp.address) {
      model.value.addressLine1 = resp.address.addressLine1;
      model.value.addressLine2 = resp.address.addressLine2;
      model.value.countryId = resp.address.countryId ? resp.address.countryId : "";
      model.value.stateProvinceId = resp.address.stateProvinceId;
      model.value.city = resp.address.city;
      model.value.zipCode = resp.address.zipCode;
    }
    bankrows.value = resp.expenseVendorBankAccounts.map(item => ({
      ...item,
      editing: false,
      isBankAccount: !!item.isBankAccount, // Convert to boolean
      bankAccount: item.paymentType?.dropDownValue === "By Cash"
        ? resp.byCashType
        : (item.isBankAccount === true ? resp.bankAccountType : resp.upiAccountType),
      flag: "Edit"
    }));
    model.value.isBankAccount = bankrows.value.length > 0;
  }).finally(() => {
    loading.value = false;
  });
};

function getPaymentText (paymentId) {
  const selectedOption = vendorAccountTypeDropdownSingleSelect.getLabelByValue(paymentId);
  return selectedOption ? selectedOption : "";
}

const editingBankRow = ref({
  bankName: "",
  accountNumber: "",
  ifscCode: "",
  accountTypeId: "",
  branchName: ""
});


// Add new row
function onAddPaymentProfile () {
  const defaultOption = vendorAccountTypeDropdownSingleSelect.getValueByLabel("Bank Account");

  bankrows.value.push({
    ...editingBankRow.value,
    bankAccount: defaultOption ? defaultOption : "", // Default to "Bank Account"
    isBankAccount: true // Ensure isBankAccount is set correctly
  });
  // Reset the editing row
  editingBankRow.value = {
    bankAccount: defaultOption ? defaultOption : "", // Reset with default
    bankName: "",
    accountNumber: "",
    ifscCode: "",
    accountTypeId: "",
    branchName: ""
  };
  syncRowValidations();
}

async function onDeletePaymentProfile (index) {
  const account = bankrows.value[index];
  zwConfirmDelete({ data: `Bank Account : ${index + 1}` }, async () => {
    try {
      if (account.id) {
        account.flag = "Delete";
        account.hidden = true;
        notifySuccess({
          message: "Account deleted successfully."
        });
      } else {
        bankrows.value.splice(index, 1);
        notifySuccess({
          message: "Account removed successfully."
        });
      }
    } catch (error) {
      $q.notify({ type: "negative", message: "Error deleting account." });
    }
  });
}

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    if (!await v$.value.$validate()) {
      return;
    }
    if (bankrows.value.length === 0) {
      notifyError({ message: "Add at least one account." });
      processing.value = false;
      return;
    }

    let isValid = true;
    rowValidations.value = bankrows.value.map(row =>
      !row.deleted ? useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true }) : null
    );

    for (let i = 0; i < rowValidations.value.length; i++) {
      const validation = rowValidations.value[i];
      const row = bankrows.value[i];

      if (!row.deleted && validation?.value) {
        await validation.value.$touch();
        const isRowValid = await validation.value.$validate();
        if (!isRowValid) isValid = false;
      }
    }
    model.value.bankAccountList = bankrows.value;
    if (await v$.value.$validate()  && isValid ) {
      processing.value = true;
      expenseVendorBankAccountService.saveVendor(props.id, model.value).then((resp) => {
        notifySuccess({ message: "Vendor Account is saved successfully." });
        onDialogOK();
      }).finally(() => {
        processing.value = false;
      });
    }
  } catch (error) {
    console.error("Error in submitting the vendor account:", error);
    notifyError({ message: "An error occurred while saving the vendor account." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

function handlePaymentTypeChange (val, row) {
  row.bankAccount = val;
  const paymentText = vendorAccountTypeDropdownSingleSelect.getLabelByValue(val);

  if (paymentText === "Bank Account") {
    // Initialize bank-related fields
    row.bankName = row.bankName || "";
    row.accountNumber = row.accountNumber || "";
    row.ifscCode = row.ifscCode || "";
    row.accountTypeId = row.accountTypeId || null;
    row.branchName = row.branchName || "";
    row.upI_ID = null; // Reset UPI when Bank Account is selected
  } else if (paymentText === "UPI") {
    // Reset bank-related fields and set UPI
    row.bankName = null;
    row.accountNumber = null;
    row.ifscCode = null;
    row.accountTypeId = null;
    row.branchName = null;
    row.upI_ID = row.upI_ID || "";
  } else {
  // For By Cash or others → clear all
    row.bankName = null;
    row.accountNumber = null;
    row.ifscCode = null;
    row.accountTypeId = null;
    row.branchName = null;
    row.upI_ID = null;
  }
  updateBankColumns(paymentText);
}

const refreshPersonNameDropdown = () => {
  personNameDropdownSingleSelect.load();
};

const handleAddPerson = (row = null, refreshPersonNameDropdown) => {
  onPersonAddAndReturnPersonId(
    refreshPersonNameDropdown,
    (newPersonId) => {
      setTimeout(() => {
        if (row) {
          row.personId = newPersonId;
          getPersonById(newPersonId, row);
        }
      }, 100);
    }
  );
};

// ------------------------------------------------------------------------------------
// Get All Dropdowns
// ------------------------------------------------------------------------------------

const {
  countryNameDropdownSingleSelect,
  stateNameDropdownSingleSelect
 } = commonModule();

const { personNameDropdownSingleSelect } = personModule();
const { accountTypeDropdownSingleSelect, vendorAccountTypeDropdownSingleSelect } = financeExpenseVendorsModule();

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initPersonDialogs(props.id);

onMounted(() => {
  personNameDropdownSingleSelect.load();
  countryNameDropdownSingleSelect.load();
  accountTypeDropdownSingleSelect.load("Account Type");
  vendorAccountTypeDropdownSingleSelect.load("Vendor Account Type");
});

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue) => {
  if (newValue) {
    getVendor();
  } else {
    loading.value = false;
  }
}, { immediate: true });

// watches a data property with the same name i.e. immediate effect
watch(() => model.value.countryId, (newValue, oldValue) => {
  if (props.id && newValue !== oldValue && oldValue !== null) {
    model.value.stateProvinceId = null;
    model.value.zipCode = "";
    model.value.vendor_Phone = "";
  }
  if (newValue) {
    stateNameDropdownSingleSelect.load(newValue);
    rules = {
      vendorName: { required: helpers.withMessage("Vendor name is required", required) }
    };
    // Validate rules
    v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });
  }
}, { immediate: true });
</script>
