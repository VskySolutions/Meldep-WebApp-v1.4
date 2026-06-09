<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 70vw !important;max-width: 70vw;">
      <!-- Header Section -->
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Expenses</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense :disable="processing" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset class="q-mt-xs">
              <legend class="text-primary text-h6">{{ id ? "Edit" : "Add" }} Expenses</legend>
              <div class="row q-col-gutter-md">
                <div class="col-12 col-md-4 col-lg-4">
                  <label class="q-mb-xs text-black">Payment Date<span class="required">*</span></label>
                  <q-input
                    v-model="model.paymentDate"
                    :readonly="readonlyDate!= '' ? '' : 'readonlyDate'"
                    class=""
                    outlined
                    stack-label
                    hide-bottom-space
                    mask="##/##/####"
                    dense
                    :error="v$.paymentDate.$error"
                    :error-message="v$.paymentDate.$errors[0]?.$message"
                    @blur="v$.paymentDate.$touch"
                    @focus="markAsEdited"
                  >
                    <template #append>
                      <q-icon name="o_calendar_month" class="cursor-pointer" :class="{ 'disabled-icon': readonlyDate !== '' }">
                        <q-popup-proxy v-if="readonlyDate === ''" ref="qDateProxy" transition-show="scale" transition-hide="scale">
                          <q-date v-model="model.paymentDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
                <div class="col-12 col-md-4 col-lg-4 row items-center no-wrap">
                  <formSingleSelectDropdown
                    v-model="model.payeeId"
                    label="Vendor"
                    :wrapperClass="'col-12'"
                    :options="vendorNameDropdownSingleSelect.list.value"
                    :filter="vendorNameDropdownSingleSelect.filter"
                    :required="false"
                     @update:model-value="getAccountsByVendorId(model.payeeId)"
                     @focus="markAsEdited"
                  >
                    <template #after>
                      <q-icon
                        v-if="props.id == ''"
                        name="o_add"
                        color="primary"
                        class="cursor-pointer q-ml-xs add-icon"
                        @click="onAddVendor()"
                      >
                        <q-tooltip>Add new Vendor</q-tooltip>
                      </q-icon>
                   </template>
                  </formSingleSelectDropdown>
                </div>
                <formSingleSelectDropdown
                  v-model="model.expenseVendorBankAccountId"
                  label="Vendor Payment Method"
                  :required="false"
                  :options="expenseVendorBankAccountDropdownSingleSelect.list.value"
                  :filter="expenseVendorBankAccountDropdownSingleSelect.filter"
                  @focus="markAsEdited"
                />
                <formSingleSelectDropdown
                  v-model="model.locationId"
                  label="Location"
                  :options="locationDropdownSingleSelect.list.value"
                  :filter="locationDropdownSingleSelect.filter"
                  :error="v$.locationId.$error"
                  :error-message="v$.locationId.$errors[0]?.$message"
                  @click="v$.locationId.$touch"
                  @focus="markAsEdited"
                />
                <formSingleSelectDropdown
                  v-model="model.customerId"
                  label="Customer"
                  :required="false"
                  :options="customerDropdownSingleSelect.list.value"
                  :filter="customerDropdownSingleSelect.filter"
                  @focus="markAsEdited"
                />
                <div class="col-12 col-sm-4 col-md-4 col-lg-4">
                  <label class="q-mb-xs text-black">Is Reimbursement</label>
                  <div class="row">
                    <div class="col-2 text-center">
                      <q-checkbox
                        v-model="model.isReImbursement"
                        :true-value="true"
                        :false-value="false"
                        class="text-h4"
                        @focus="markAsEdited"
                      >
                        <q-tooltip>Is Reimbursement</q-tooltip>
                      </q-checkbox>
                    </div>
                    <div class="col-10 hidden">
                      <q-select
                        v-if="!model.isReImbursement"
                        v-model="model.ref_no"
                        clearable
                        use-input
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        :options="advanceExpenseRequestList"
                        option-value="value"
                        option-label="text"
                        emit-value
                        map-options
                        @filter="getAllAdvanceExpenseRequestDropDownListFilter"
                        @focus="markAsEdited"
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
                </div>
              </div>
              <!-- Memo and Attachment -->
              <div class="row q-col-gutter-md q-mt-xs">
                <div class="col-12 col-sm-12 col-md-12 col-lg-12">
                  <div class="q-mb-xs text-black"><label>Memo</label></div>
                  <div class="form-group">
                    <q-editor
                      v-model="model.memo" :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                      @focus="markAsEdited"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-md q-mt-xs" @paste="handlePaste">
                <!-- File Uploader -->
                <div class="col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12">
                  <div class="q-mb-xs text-black"><label>Files</label></div>
                  <div class="form-group">
                    <multiFileUploader
                      :initialFiles="model.expenseFiles"
                      :allowedExtensions="[
                        '.jpg','.xls','.xlsx','.doc','.docx','.jpeg','.jpg','.png','.ppt','.pptx','pdf'
                      ]"
                      :maxSizeInMb="25"
                      label="Drag files here or (+) to upload."
                      @files-selected="handleFiles"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-lg">
                <!-- Display Files in Square Boxes with File Name Below -->
                <div v-if="model.expenseFiles && model.expenseFiles.length > 0" class="row q-gutter-md">
                  <div
                    v-for="(file, index) in model.expenseFiles"
                    :key="index"
                    class="col-3 position-relative file-card text-center"
                    style="max-width: 140px; min-width: 140px;"
                  >
                    <div class="file-preview square-box">
                      <template v-if="isImageFile(file)">
                        <img
                          :src="file.file.virtualPath ? file.file.virtualPath : getFilePreview(file.file)"
                          alt="File Preview"
                          class="square-content centered-image"
                        >
                      </template>
                      <template v-else>
                        <q-icon
                          :name="getFileIcon(file.file?.mimeType)"
                          class="file-icon square-content"
                          size="70px"
                        />
                      </template>
                      <div class="file-name q-mt-sm">
                        <q-btn
                          v-if="file.file?.virtualPath || file?.name"
                          class="bg-primary text-white q-pa-xs"
                          no-caps
                          @click="viewFile(file)"
                        >
                          <span class="truncate-text">
                            {{ file.file?.name || file.SeoFilename || extractFileName(file.file?.seoFilename) }}
                          </span>
                        </q-btn>
                      </div>
                    </div>
                    <!-- Remove Button -->
                    <q-btn
                      color="negative"
                      flat
                      round
                      dense
                      icon="o_close"
                      class="remove-file-icon"
                      @click="removeFile(index)"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset class="q-mt-xs">
              <legend>Expenses Lines List</legend>
              <!-- <span class="text-primary text-h2 q-mt-xxl">Add Expenses</span> -->
              <div class="row justify-end q-pa-none">
                <q-btn label="Add Expense" color="primary" icon="o_add" class="q-mb-sm" @click="onAddExpense" />
              </div>
              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                flat
                bordered separator="cell"
                :rows="expenses"
                :columns="columns"
                row-key="expenseCategory"
                class="no-shadow" :loading="loading"
              >
                <!-- <q-table :rows="expenses" :columns="columns" row-key="id" flat bordered class="q-pa-2 center" separator="cell"> -->
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}<span v-if="['expenseCategoryId', 'quantity', 'unitPrice'].includes(col.name)" class="required">*</span></q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <!-- Custom Row Slot -->
                <template #body="props">
                  <q-tr v-show="!props.row.hidden" class="row-highlight">
                    <q-td class="text-left">
                      <formSingleSelectDropdown
                        v-model="props.row.expenseCategoryId"
                        :options="expenseCategoryForDropdownSingleSelectWithRowIndex.getByIndex(props.rowIndex)"
                        :filter="expenseCategoryForDropdownSingleSelectWithRowIndex.filter(props.rowIndex)"
                        :error="rowValidations[props.rowIndex]?.value?.expenseCategoryId.$error"
                        :errorMessage="rowValidations[props.rowIndex]?.value?.expenseCategoryId.$errors[0]?.$message"
                        @update:model-value="getSubcategoryByCategoryId(props.row.expenseCategoryId, props.rowIndex)"
                        @focus="markAsEdited"
                      />
                    </q-td>
                    <q-td class="text-left">
                      <formSingleSelectDropdown
                        v-model="props.row.expenseSubcategoryId"
                        :options="expenseSubCategoryForDropdownSingleSelectWithRowIndex.getByIndex(props.rowIndex, props.row.expenseCategoryId)"
                        :filter="expenseSubCategoryForDropdownSingleSelectWithRowIndex.filter(props.rowIndex)"
                        @focus="markAsEdited"
                      />
                    </q-td>
                    <q-td style="width: 400px; max-width: 400px;">
                      <q-input
                        v-model="props.row.description"
                        autogrow
                        outlined
                        hide-bottom-space
                        dense
                        maxlength="600"
                        style="max-height: 100px;overflow-y: auto;"
                        @focus="markAsEdited"
                      />
                    </q-td>
                    <q-td class="text-left">
                      <q-input
                        v-model="props.row.quantity"
                        maxlength="5"
                        dense
                        outlined
                        hide-bottom-space
                        style="max-width: 200px;"
                        :error="rowValidations[props.rowIndex]?.value?.quantity.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.quantity.$errors[0]?.$message"
                        @update:model-value="updateAmount(props.row)"
                        @focus="markAsEdited"
                      />
                    </q-td>
                    <q-td class="text-left">
                      <q-input
                        v-model="props.row.unitPrice"
                        dense
                        maxlength="7"
                        autogrow
                        outlined
                        hide-bottom-space
                        :error="rowValidations[props.rowIndex]?.value?.unitPrice.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.unitPrice.$errors[0]?.$message"
                        style="max-width: 250px;"
                        @update:model-value="updateAmount(props.row)"
                        @focus="markAsEdited"
                      />
                    </q-td>
                    <q-td class="text-left">
                      <q-input
                        v-model="props.row.amount"
                        dense
                        outlined
                        hide-bottom-space
                        disable
                        @focus="markAsEdited"
                      />
                    </q-td>

                    <!-- Actions -->
                    <q-td class="text-center actions" width="5%">
                      <div class="q-pa-xs">
                        <q-icon name="o_delete_outline" size="xs" class="cursor-pointer Abtn" color="negative" @click="onDeleteExpense(props.rowIndex)">
                          <q-tooltip>Delete</q-tooltip>
                        </q-icon>
                      </div>
                    </q-td>
                  </q-tr>
                  <q-tr v-if="props.pageIndex === expenses.length - 1">
                    <q-td colspan="5" class="text-right font-bold"><b>Total Hours:</b></q-td>
                    <q-td class="text-right"><b>{{ totalAmt }}</b></q-td>
                    <q-td />
                  </q-tr><q-separator />
                </template>
              </q-table>
            </fieldset>
            <fieldset class="q-mt-xs">
              <legend>Recurring</legend>
              <div class="row q-col-gutter-md justify-start q-mb-xs">
                <div class="col-12 col-sm-6 col-md-2 col-lg-2 q-mr-sm">
                  <q-checkbox v-model="isRecurring" :true-value="true" :false-value="false" class="text-h4" label="Set Recurring" @update:model-value="toggleRecurring" />
                </div>
                <template v-if="isRecurring">
                  <formSingleSelectDropdown
                    v-model="model.interval"
                    label="Interval"
                    :required="false"
                    :options="intervalTypeDropdownSingleSelect.list.value"
                    :filter="intervalTypeDropdownSingleSelect.filter"
                    :wrapperClass="'col-12 col-sm-6 col-md-3 col-lg-3'"
                    @focus="markAsEdited"
                  />
                  <div class="col-12 col-sm-6 col-md-3 col-lg-3 q-mr-sm q-ml-sm">
                    <label class="q-mb-xs text-black">Start Date </label>
                    <q-input
                      v-model="startDate"
                      outlined
                      stack-label
                      hide-bottom-space
                      mask="##/##/####"
                      dense
                      :rules="[
                        (val) => isValidDate(val) || 'Enter a valid date in MM/DD/YYYY format'
                      ]"
                      @focus="markAsEdited"
                    >
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date v-model="startDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                  <div class="col-12 col-sm-6 col-md-3 col-lg-3 q-mr-sm q-ml-sm">
                    <label class="q-mb-xs text-black">End Date </label>
                    <q-input
                      v-model="endDate"
                      outlined
                      stack-label
                      hide-bottom-space
                      mask="##/##/####"
                      dense
                      :rules="[
                        (val) => isValidDate(val) || 'Enter a valid date in MM/DD/YYYY format',
                        (val) =>
                          !startDate || new Date(val) > new Date(startDate) ||
                          'End date must be after the start date',
                      ]"
                      @focus="markAsEdited"
                    >
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date v-model="endDate" :options="disableBeforeStartDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </template>
              </div>
            </fieldset>
          </div>
        </div>
        <!-- Submit and Cancel Buttons -->
        <q-card-actions align="center" class="q-gutter-sm justify-center">
          <q-btn
            label="Close"
            color="grey-4"
            class="text-grey-9 actionBtn"
            no-caps
            :disable="processing"
            @click="showCancelConfirmation"
          />
          <q-btn
            v-if="id && model.expenseStatus?.dropDownValue !== 'Submitted'"
            label="Update and Save Draft"
            color="primary"
            class="actionBtn" no-caps style="width: auto;"
            :disabled="isDisabled"
            :loading="processing && activeButton === 'Draft'"
            @click="onSubmit('Draft')"
          />
          <q-btn
            v-else-if="!id"
            label="Save Draft"
            color="primary"
            class="actionBtn" no-caps
            :disabled="isDisabled"
            :loading="processing && activeButton === 'Draft'"
            @click="onSubmit('Draft')"
          />
          <q-btn
            v-if="id || model.expenseStatus?.dropDownValue === 'Submitted'"
            label="Update And Close"
            color="primary"
            class="actionBtn"
            no-caps
            :disabled="isDisabled"
            :loading="processing && activeButton === 'Submitted'"
            @click="onSubmit('Submitted')"
          />
          <q-btn
            v-else-if="!id"
            label="Submit And Close"
            color="primary"
            class="actionBtn"
            no-caps
            :disabled="isDisabled"
            :loading="processing && activeButton === 'Submitted'"
            @click="onSubmit('Submitted')"
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed, toRaw } from "vue";
import { useDialogPluginComponent, useQuasar } from "quasar";
import { format } from "date-fns";
import { isDate } from "validators/zw_validators.js";
import { notifySuccess, notifyError } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import useFilters from "composables/useFilters";
import _ from "lodash";

import ExpenseService from "modules/finance-expense/financeExpense.service";
import addVendor from "modules/finance-expense-vendors/components/addEdit.vue";

// Shared Dropdowns
import financeExpenseVendorsModule from "src/modules/finance-expense-vendors/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";
import financeExpenseModule from "src/modules/finance-expense/utils/dropdowns.js";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import multiFileUploader from "src/components/form-inputs/_multiFileUpload.vue";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// Common variables
const props = defineProps({ id: { type: String, default: "" } });
const readonlyDate = props.id ? "readonly" : "";
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogOK, onDialogHide } = useDialogPluginComponent();

const ExpId = ref("");
const documentUploaderRef = ref(null);
const processing = ref(false);
const activeButton = ref("");

const status = ref("");
const isRecurring = ref(false);
const interval = ref("");
const startDate = ref(format(new Date(), "MM/dd/yyyy"));
const endDate = ref(format(new Date(), "MM/dd/yyyy")); // Formats date as DD-MM-YYYY
const expenses = ref([]);

const toggleRecurring = () => {
  if (!isRecurring.value) {
    interval.value = null;
    startDate.value = format(new Date(), "MM/dd/yyyy");
    endDate.value = format(new Date(), "MM/dd/yyyy");
  }
};

const model = ref({
  payeeId: null,
  paymentDate: format(new Date(), "MM/dd/yyyy"),
  expenseVendorBankAccountId: "",
  locationId: ref(),
  virtualpath: "",
  memo: "",
  virtualpathLines: "",
  expenseFiles: [],
  expenseFileFlag: "edit",
  FileKeys: [],
  expensesListModelProps: [],
  ref_no: "",
  isReImbursement: false,
  customerId: null
});

const imageAttachemnt = ref("");

// Table variables
const loading = ref(true);
const pagination = ref({ sortBy: "", descending: false, rowsPerPage: 20, page: 1 });
const columns = [
  { name: "expenseCategoryId", label: "Expense Category", align: "center", field: "expenseCategoryId" },
  { name: "expenseSubcategoryId", label: "Sub Category", align: "center", field: "expenseSubcategoryId" },
  { name: "description", label: "Description", align: "center", field: "description" },
  { name: "quantity", label: "Quantity", align: "center", field: "quantity" },
  { name: "unitPrice", label: "Unit Price", align: "center", field: "unitPrice" },
  { name: "amount", label: "Amount", align: "center", field: "amount" }
];

// Validation rules
const rules = computed(() => ({
  locationId: {
    required: helpers.withMessage("Location is required", required)
  },
  paymentDate: {
    required: helpers.withMessage("Payment Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  }
}));

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const rowRules = {
  expenseCategoryId: {
    required: helpers.withMessage("Expense Category is required", required)
  },
  quantity: {
    required: helpers.withMessage("Quantity is required", required),
    isNumber: helpers.withMessage("Quantity must be a valid number", (val) => /^[0-9]+(\.[0-9]+)?$/.test(val)),
    isGreaterThanZero: helpers.withMessage("Quantity must be greater than zero", (val) => parseFloat(val) > 0)
  },
  unitPrice: {
    required: helpers.withMessage("Unit Price is required", required),
    isNumber: helpers.withMessage("Unit Price must be a valid number", (val) => /^[0-9]+(\.[0-9]+)?$/.test(val)),
    isGreaterThanZero: helpers.withMessage("Unit Price must be greater than zero", (val) => parseFloat(val) > 0)
  }
};

const rowValidations = ref([]);
function syncRowValidations () {
  rowValidations.value = expenses.value.map(row =>
    !row.deleted ? useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true }) : null
  );
}

const isValidDate = (val) => {
  const datePattern = /^\d{2}\/\d{2}\/\d{4}$/; // Check MM/DD/YYYY format
  return datePattern.test(val) && !isNaN(new Date(val).getTime());
};

const getExpenseDetails = async (id) => {
  try {
    ExpenseService.getExpenseById(props.id).then((resp) => {
      model.value = _.cloneDeep(resp);
      ExpId.value = id;
      model.value.paymentDate = resp.expenseDate ? resp.expenseDate : "";
      model.value.payeeId = resp.payeeId; // Employee name for payee
      expenseVendorBankAccountDropdownSingleSelect.load(model.value.payeeId);
      model.value.expenseVendorBankAccountId = resp.expenseVendorBankAccountId;
      model.value.ref_no = resp.ref_no; // Adjusted key
      model.value.memo = resp.memo ? resp.memo : ""; // Correct as-is
      model.value.virtualpath = resp.picture.virtualPath || "";
      imageAttachemnt.value = model.value.virtualpath;
      status.value = resp.status; // Adjusted key
      // model.value.Location = resp.location; // Adjusted key
      status.value = resp.statusId; // Adjusted key
      model.value.locationId = resp.locationId; // Adjusted key
      isRecurring.value = !!resp.setRecurring;
      interval.value = resp.recurringInterval;
      interval.value = resp.setRecurring ? resp.recurringIntervalId : null;
      model.value.expenseFiles = resp.expenseFilesList || [];
      model.value.isReImbursement = resp.isReImbursement;
      isEdited.value = resp.isEdited;
      if (!resp.setRecurring) {
        startDate.value = "";
        endDate.value = "";
      } else {
        if (resp.recurringStartDate) {
          startDate.value = resp.recurringStartDate;
        }
        if (resp.recurringEndDate) {
          startDate.value = resp.recurringEndDate;

        }
      }
     expenses.value = resp.expenseLines?.map((expenseItem, index) => {
     const rowIndex = index;
     expenseCategoryForDropdownSingleSelectWithRowIndex.load(rowIndex, "Expense Category");
     expenseSubCategoryForDropdownSingleSelectWithRowIndex.load(
      rowIndex,
      expenseItem.expenseCategoryId
     );
     console.log(resp);;
      return {
        ...expenseItem,
        id: expenseItem.id || uid(),
        expenseCategoryId: expenseItem.expenseCategoryId,
        expenseSubcategoryId: expenseItem.expenseSubcategoryId,
        description: expenseItem.description || "",
        amount: expenseItem.amount,
        quantity: expenseItem.quantity,
        unitPrice: expenseItem.unitPrice,
        attachment: expenseItem.Filepic,
        virtualpathLines: expenseItem.Filepic,
        rowIndex: rowIndex,
        flag: "Edit"
      };
    }) || [];
      // Calculate total amount
      totalAmt.value = expenses.value.reduce((acc, row) => {
        return acc + (parseFloat(expenses.value.amount) || 0);
      }, 0);
    });
  } catch (error) {
  }
};

function disableBeforeStartDate (date) {
  if (!startDate.value) {
    return true;
  }
  const start = new Date(startDate.value);
  const current = new Date(date);
  return current >= start;
}

const totalAmt = computed(() => {
  return expenses.value.reduce((total, row) => {
    if (!row.deleted) {
      total += parseFloat(row.amount) || 0;
    }
    return total;
  }, 0);
});

const calculateTotalAmount = () => {
  totalAmt.value = expenses.value.reduce((acc, row) => {
    return acc + (parseFloat(row.amount) || 0); // Use parseFloat to ensure correct addition
  }, 0);
};

function updateAmount(row) {
  if (row.isDeleted) {
    row.amount = 0;
    return;
  }

  if (row.quantity && row.unitPrice) {
    row.amount = (row.quantity * row.unitPrice).toFixed(2);
  } else {
    row.amount = 0;
  }
}

const onAddExpense = () => {
  const rowIndex = expenses.value.length;
  expenses.value.push({
    id: null,
    hidden: false,
    expenseCategoryId: null,
    expenseSubcategoryId: null,
    description: "",
    amount: null,
    flag: "New",
    touched: false,
    attachmentLinesRef: "",
    LineFilePics: [],
    FileKeys: []
  });
  expenseCategoryForDropdownSingleSelectWithRowIndex.load(
    rowIndex,
    "Expense Category"
  );

  syncRowValidations();
};

async function onDeleteExpense (index) {
  const activeRows = expenses.value.filter(row => !row.hidden);
  if (activeRows.length <= 1) {
    notifyError({ message: "Add at least one category." });
    return;
  }
  const expense = expenses.value[index];
  if (expense.id) {
    expense.flag = "Delete";
    expense.hidden = true;
  } else {
    expenses.value.splice(index, 1); // Remove immediate new row
  }
}

const attachmentFiles = ref([]);
const isDisabled = ref(false);

const onSubmit = async (statusValue) => {
  activeButton.value = statusValue;
  processing.value = true;

  try {
    const activeExpenses = expenses.value.filter(row => !row.deleted);

    const isExpenseValid = await v$.value.$validate();
    if (!isExpenseValid) return;

    if (activeExpenses.length === 0) {
      notifyError({ message: "Add at least one category." });
      processing.value = false; // Remember to reset loading state
      return;
    }
    // Row-level validation
    rowValidations.value = activeExpenses.map(row =>
      useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true })
    );

    let isExpenseLinesValid = true;

    for (let i = 0; i < rowValidations.value.length; i++) {
      const validation = rowValidations.value[i];

      if (validation?.value) {
        await validation.value.$touch();
        const isRowValid = await validation.value.$validate();
        if (!isRowValid) isExpenseLinesValid = false;
      }
    }
    if (!isExpenseLinesValid) return;
    isDisabled.value = true;
    const formData = new FormData();

    const Payload = {
      id: ExpId.value,
      expenseDate: model.value.paymentDate,
      PayeeId: model.value.payeeId,
      expenseVendorBankAccountId: model.value.expenseVendorBankAccountId,
      Ref_no: model.value.ref_no,
      Memo: model.value.memo,
      StatusId: statusValue,
      LocationId: model.value.locationId,
      ChangeFlag: model.value.changeFlag,
      SetRecurring: isRecurring.value,
      RecurringIntervalId: isRecurring.value ? interval.value : null,
      RecurringStartDate: isRecurring.value && startDate.value ? startDate.value : null,
      RecurringEndDate: isRecurring.value && endDate.value ? endDate.value : null,
      isReImbursement: model.value.isReImbursement,
      customerId: model.value.customerId || null,
      isEdited: isEdited.value,
      isSubmitted: model.value.expenseStatus?.dropDownValue === "Submitted",
      expensesListModelProps: activeExpenses.map((expense, index) => {
        const files = attachmentFiles.value[index] || [];

        return {
          files,
          hidden: false,
          expenseId: expense.ExpenseId,
          expenseCategoryId: expense.expenseCategoryId,
          expenseSubcategoryId: expense.expenseSubcategoryId,
          FileKeys: expense.FileKeys || [],
          description: expense.description,
          amount: expense.amount,
          quantity: expense.quantity,
          unitPrice: expense.unitPrice,
          flag: expense.flag,
          id: expense.id,
          FileName: files.length ? files.map(file => file.name) : null
        };
      })
    };
    toRaw(model.value.expenseFiles || []).forEach((file) => {
      if (file.file?.virtualPath) {
        formData.append(
          "ExistingFiles",
          JSON.stringify({ id: file.id, virtualPath: file.file.virtualPath })
        );
      } else {
        formData.append("ExpenseFiles", file);
      }
    });
    Object.entries(Payload).forEach(([key, value]) => {
      if (key !== "expensesListModelProps" && value != null) {
        formData.append(key, value);
      }
    });

    Payload.expensesListModelProps.forEach((expense, index) => {
      Object.entries(expense).forEach(([key, value]) => {
        if (value != null) {
          formData.append(`expensesListModelProps[${index}].${key}`, value);
        }
      });
    });
    await ExpenseService.saveExpense(props.id, formData);
    notifySuccess({ message: "Expense is saved successfully." });
    onDialogOK();

  } catch (error) {
    console.error("API ERROR:", error);

    notifyError({
      message: error?.response?.data?.message || "Failed to save expense."
    });

    isDisabled.value = false;
  } finally {
    setTimeout(() => {
      processing.value = false;
      activeButton.value = "";
    }, 1000);
  }
};

const showCancelConfirmation = () => {
  $q.dialog({
    title: "Cancel Confirmation",
    message: "Are you sure you want to cancel? Unsaved changes will be lost.",
    ok: { label: "Yes", color: "negative" },
    cancel: { label: "No", color: "primary" },
    persistent: true
  }).onOk(() => {
    cancelAction();
  });
};

const cancelAction = () => {
  dialogRef.value.hide();
};

function handlePaste (event) {
  const clipboardItems = event.clipboardData.items;
  const files = [];
  for (const item of clipboardItems) {
    if (item.kind === "file") {
      const file = item.getAsFile();
      if (file) {
        files.push(file);
      }
    }
  }

  if (files.length > 0 && documentUploaderRef.value) {
    // Add files to uploader
    documentUploaderRef.value.addFiles(files);
  }
}

function handleFiles (files) {
  model.value.expenseFiles = files;
  model.value.expenseFileFlag = "edit";
}

function getFilePreview (file) {
  return file && file instanceof File ? URL.createObjectURL(file) : "";
}

function isImageFile (file) {
  if (file.file instanceof File) {
    return file.file.type.startsWith("image/");
  } else if (file.file && file.file.mimeType) {
    return file.file.mimeType.startsWith("image/");
  }
  return false;
}

function getFileIcon (mimeType) {
  const mimeToIconMap = {
    "application/pdf": "o_picture_as_pdf",
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": "o_insert_chart",
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document": "o_description",
    "application/vnd.openxmlformats-officedocument.presentationml.presentation": "o_slideshow", // PPTX MIME type
    "application/vnd.ms-powerpoint": "o_slideshow", // PPT MIME type
    "application/zip": "o_folder_zip",
    "text/plain": "o_article",
    "image/png": "o_image",
    "image/jpeg": "o_image",
    "image/gif": "o_image",
    // Default icon for unknown MIME types
    default: "o_insert_drive_file"
  };

  return mimeToIconMap[mimeType] || mimeToIconMap.default;
}

function viewFile (file) {
  let fileUrl; // Declare fileUrl before using it

  if (file?.file?.virtualPath) {
    fileUrl = new URL(file.file.virtualPath).href; // For uploaded files
  } else if (file?.file instanceof File) {
    fileUrl = URL.createObjectURL(file.file); // For newly added files
  } else if (file instanceof File) {
    fileUrl = URL.createObjectURL(file); // Direct File object case
  }

  // const fileUrl = new URL(file, baseURL).href;
  const fileExtension = fileUrl.split(".").pop().toLowerCase();
  const supportedFormats = ["pdf", "docx", "xlsx", "pptx"];
  const imageFormats = ["jpg", "jpeg", "png", "gif", "svg"];

  let viewerUrl = fileUrl;

  // Use Google Docs Viewer for Documents
  if (supportedFormats.includes(fileExtension)) {
    viewerUrl = `https://docs.google.com/gview?url=${encodeURIComponent(fileUrl)}&embedded=true`;
  }

  // Open new window
  const newWindow = window.open("", "_blank");

  // Check if the popup is blocked
  if (!newWindow) {
    alert("Popup blocked! Please allow popups for this site.");
    return;
  }

  // Delay to avoid null reference issues
  setTimeout(() => {
    newWindow.document.write(`
<html>
<head>
<title>${fileUrl.split("/").pop()}</title>
<style>
              * { margin: 0; padding: 0; box-sizing: border-box; }
              body, html { width: 100vw; height: 100vh; display: flex; align-items: center; justify-content: center; background-color: #f4f4f4; overflow: hidden; }
              .top-right {
                position: fixed;
                top: 10px;
                right: 10px;
                background: #007bff;
                color: white;
                padding: 10px 15px;
                border-radius: 5px;
                font-size: 16px;
                text-decoration: none;
                z-index: 10;
              }
              .top-right:hover {
                background: #0056b3;
              }

              iframe, img {
                width: 100%;
                height: 100%;
                border: none;
                display: block;
                object-fit: contain; /* Ensures images fit properly */
              }

              /* Responsive Fixes */
              @media (max-width: 768px) {
                .top-right {
                  top: 5px;
                  right: 5px;
                  padding: 8px 12px;
                  font-size: 14px;
                }
              }
</style>
</head>
<body>
<a class="top-right" href="${fileUrl}" download>Download</a>
            ${
  imageFormats.includes(fileExtension)
    ? `<img src="${fileUrl}" alt="Image Preview">` // Show image directly
    : `<iframe src="${viewerUrl}"></iframe>` // Show document using iframe
}
</body>
</html>
        `);
  }, 100);
}

function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
}

function removeFile (index) {
  const file = model.value.expenseFiles[index];
  if (!file) return;

  // REMOVE FROM UPLOADER ALSO
  const uploader = documentUploaderRef.value;
  if (uploader) {
    const uploaderFile = uploader.files.find(
      f =>
        f.name === file.name ||
        f.name === file.file?.name
    );
    if (uploaderFile) {
      uploader.removeFile(uploaderFile);
    }
  }

  // Existing file (from DB)
  if (file.file?.virtualPath) {
    file.flag = "remove";
  }
  model.value.expenseFiles.splice(index, 1);
  if (model.value.expenseFiles.length === 0) {
    model.value.expenseFileFlag = "remove";
  }
}

const onAddVendor = () => {
  $q.dialog({
    component: addVendor,
    componentProps: { }
  }).onOk(() => {
    getAllExpenseVendorListForDropdown();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Called on click/focus
const isEdited = ref(false);
const markAsEdited = () => {
  if (model.value.expenseStatus?.dropDownValue !== "Submitted") return;
  isEdited.value = true;
};

function getAccountsByVendorId (payeeId) {
  model.value.expenseVendorBankAccountId = "";
  expenseVendorBankAccountDropdownSingleSelect.load(payeeId);
}

const getSubcategoryByCategoryId = (categoryId, rowIndex) => {
  if (categoryId) {
    expenseSubCategoryForDropdownSingleSelectWithRowIndex.load(
      rowIndex,
      categoryId
    );
  } else {
    expenses.value[rowIndex].expenseSubcategoryId = null;
    expenseSubCategoryForDropdownSingleSelectWithRowIndex.clearByIndex(rowIndex);
  }
};

// ------------------------------------------------------------------------------------
// Get All Dropdowns
// ------------------------------------------------------------------------------------

const { locationDropdownSingleSelect, intervalTypeDropdownSingleSelect, expenseCategoryForDropdownSingleSelectWithRowIndex, expenseSubCategoryForDropdownSingleSelectWithRowIndex } = financeExpenseModule();
const { vendorNameDropdownSingleSelect, expenseVendorBankAccountDropdownSingleSelect } = financeExpenseVendorsModule();
const { customerDropdownSingleSelect } = customerModule();

watch(() => expenses.value?.amount, (newValue) => {
  if (newValue !== undefined) {
    calculateTotalAmount(); // Recalculate total hours whenever editingRow.hours changes
  }
});

watch(startDate, (newStartDate) => {
  if (new Date(endDate.value) <= new Date(newStartDate)) {
    endDate.value = ""; // Reset endDate if it becomes invalid
  }
});

watch(() => props.id, (newValue) => {
  if (newValue) {
    getExpenseDetails();
  } else {
    loading.value = false;
  }
}, { immediate: true });

onMounted(() => {
  vendorNameDropdownSingleSelect.load();
  expenseVendorBankAccountDropdownSingleSelect.load();
  locationDropdownSingleSelect.load("Location");
  customerDropdownSingleSelect.load();
  intervalTypeDropdownSingleSelect.load("Interval Type");
  // if (props.id) getExpenseDetails(props.id);
});

</script>
<style>
  .q-dialog__inner--minimized > div{
    max-height: calc(100vh) !important;
  }
  .q-dialog__inner--minimized{
    padding: 0;
  }
  .edit_projectModule .q-select__dropdown-icon{
    display: none;
  }
  .add-icon {
    border: 2px solid;
    padding: 4px;
    display: flex;
  }
  .error-input .q-field__messages {
    max-width: 250px;
    white-space: normal;
    overflow-wrap: break-word;
  }
</style>
