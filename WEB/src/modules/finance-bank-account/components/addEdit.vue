<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:800px !important; max-width: 100vw !important;">
      <!-- Header Section -->
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Bank Account Details</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <!-- Account Details Form -->
          <fieldset>
            <legend>Account Details</legend>
            <div class="row q-col-gutter-x-md q-mb-sm">
              <div class="col-12 col-md-6">
                <div class="label q-mb-xs text-black">Bank Name<span class="required">*</span></div>
                <q-input
                  v-model="model.bankName"
                  outlined
                  dense
                  maxlength="100"
                  :error="v$.bankName.$error"
                  :error-message="v$.bankName.$errors[0]?.$message"
                  @blur="v$.bankName.$touch"
                />
              </div>
              <div class="col-12 col-md-6">
                <div class="label q-mb-xs text-black">Bank Account Number<span class="required">*</span></div>
                <q-input
                  v-model="model.accountNumber"
                  outlined
                  dense
                  mask="##################"
                  hint="##################"
                  :error="v$.accountNumber.$error"
                  :error-message="v$.accountNumber.$errors[0]?.$message"
                  @blur="v$.accountNumber.$touch"
                />
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-sm">
              <div class="col-12 col-md-6">
                <div class="label q-mb-xs text-black">IFSC Code<span class="required">*</span></div>
                <q-input
                  v-model="model.ifscCode"
                  outlined
                  dense
                  maxlength="11"
                  :error="v$.ifscCode.$error"
                  :error-message="v$.ifscCode.$errors[0]?.$message"
                  :rules="[val => /^[A-Z]{4}0[A-Z0-9]{6}$/.test(val) || 'Invalid IFSC Code']"
                  @blur="v$.ifscCode.$touch"
                />
              </div>
              <formSingleSelectDropdown
                v-model="model.accountTypeId"
                label="Account Type"
                :options="accountTypeDropdownSingleSelect.list.value"
                :filter="accountTypeDropdownSingleSelect.filter"
                :wrapper-class="'col-12 col-md-6'"
                :error="v$.accountTypeId.$error"
                :error-message="v$.accountTypeId.$errors[0]?.$message"
              />
              </div>
            <div class="row q-col-gutter-x-md q-mb-sm">
              <div class="col-12 col-md-6">
                <label class="label q-mb-xs text-black">Branch Location<span class="required">*</span></label>
                <q-input
                  v-model="model.branchName"
                  maxlength="150"
                  outlined
                  dense
                  :error="v$.branchName.$error"
                  :error-message="v$.branchName.$errors[0]?.$message"
                  @blur="v$.branchName.$touch"
                />
              </div>
              <div class="col-12 col-md-6">
                <div class="text-h6"><label class="label q-mb-xs text-black">Active Status</label></div>
                <q-checkbox
                  v-model="model.isActive"
                  :true-value="true"
                  :false-value="false"
                  label="Set as Active"
                  checked
                  dense
                />
              </div>
            </div>
          </fieldset>
        </div>
      </div>
      <!-- Submit and Cancel Buttons -->
      <q-card-actions align="center" class="q-gutter-sm justify-center stickyFooter">
        <q-btn label="Close" push outline color="grey-4" type="button" no-caps class="text-grey-9 actionBtn" @click="onDialogCancel" />
        <q-btn label="Save" color="primary" class="actionBtn" :loading="processing" no-caps @click="submitForm" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, watch, onMounted } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import { useDialogPluginComponent } from "quasar";
import { required, helpers } from "@vuelidate/validators";
import useVuelidate from "@vuelidate/core";
import _ from "lodash";

import bankAccountService from "modules/finance-bank-account/financeBankAccount.service";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

// Shared Dropdowns
import financeExpenseVendorsModule from "src/modules/finance-expense-vendors/utils/dropdowns.js";

// Props value for 'id' (to determine if editing or adding)
const props = defineProps({ id: { type: String, default: "" } });

// define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogOK, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// define variables
const processing = ref(false);

// define model
const model = ref({
  bankName: "",
  accountNumber: "",
  branchName: "",
  ifscCode: "",
  accountTypeId: "",
  isActive: true
});

// validate rules
const rules = {
  bankName: {
    required: helpers.withMessage("Bank name is required", required),
    alpha: helpers.withMessage("Bank name should contain only alphabets", value => /^[A-Za-z\s]+$/.test(value)) // Only alphabets
  },
  accountNumber: {
    required: helpers.withMessage("Account number is required", required),
    lengthCheck: helpers.withMessage("Account number must be between 8 and 16 digits", (value) => value && value.length >= 8 && value.length <= 16)
  },
  branchName: { required: helpers.withMessage("Branch location is required", required) },
  ifscCode: {
    required: helpers.withMessage("IFSC code is required", required),
    pattern: helpers.withMessage("IFSC code is invalid", (value) => /^[A-Z]{4}0[A-Z0-9]{6}$/.test(value))
  },
  accountTypeId: { required: helpers.withMessage("Account type is required", required) }
};

// validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Reset form fields
const resetForm = () => {
  model.value.bankName = "";
  // accountNumber.value = "";
  model.value.ifscCode = "";
  model.value.accountTypeId = "";
  model.value.branchName = "";
  model.value.isActive = true;
};

// Fetch existing account details for edit mode
const getBankAccountDetails = (id) => {
  bankAccountService.getBankAccountById(id)
    .then((resp) => {
      model.value = _.cloneDeep(resp);
    })
    .catch((error) => {
      console.error("Error fetching account details:", error);
    });
};

// Submit form
const submitForm = async () => {
  processing.value = true;
  try {
    if (await v$.value.$validate()) {
      // processing.value = true;
      bankAccountService.saveBankAccount(props.id, model.value).then((resp) => {
        notifySuccess({ message: "Bank details is saved successfully." });
        onDialogOK();
      });
    }
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

// ------------------------------------------------------------------------------------
// Get All Dropdowns
// ------------------------------------------------------------------------------------
const { accountTypeDropdownSingleSelect } = financeExpenseVendorsModule();

// Fetch dropdown options on component mount
onMounted(() => {
  accountTypeDropdownSingleSelect.load("Account Type");
});

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getBankAccountDetails(newValue);
  } else {
    resetForm(); // Reset form for 'Add' mode
  }
}, { immediate: true });


</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
