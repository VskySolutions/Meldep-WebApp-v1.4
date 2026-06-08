<template>
  <q-dialog ref="dialogRef" class="customDialog DialogContainer" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 100vw !important;">
      <!-- Header Section -->
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Purchase Expense</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense :disable="processing" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="submitForm">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset class="q-mt-xs">
              <legend class="text-primary text-h6">{{ id ? "Edit" : "Add" }} Purchase Expense</legend>
              <div class="row q-col-gutter-md">
                <div class="col-12 col-md-4 col-lg-4">
                  <label class=" q-mb-xs text-black">Request Date<span class="required">*</span></label>
                  <q-input
                    v-model="model.requestDate"
                    :readonly="readonlyDate!= '' ? '' : 'readonlyDate'"
                    class=""
                    outlined
                    stack-label
                    hide-bottom-space
                    mask="##/##/####"
                    dense
                    :error="v$.requestDate.$error"
                    :error-message="v$.requestDate.$errors[0]?.$message"
                    @blur="v$.requestDate.$touch"
                    @focus="markAsEdited"
                  >
                    <template #append>
                      <q-icon name="o_calendar_month" class="cursor-pointer" :class="{ 'disabled-icon': readonlyDate !== '' }">
                        <q-popup-proxy
                          v-if="readonlyDate === ''"
                          ref="qDateProxy"
                          transition-show="scale"
                          transition-hide="scale"
                        >
                          <q-date
                            v-model="model.requestDate"
                            mask="MM/DD/YYYY"
                            @update:model-value="() => $refs.qDateProxy.hide()"
                          />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
                <div class="col-12 col-sm-4 col-md-4 col-lg-4">
                  <label class=" q-mb-xs text-black">Vendor<span class="required">*</span></label>
                  <q-select
                    v-model="model.vendorId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="vendorList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.vendorId.$error"
                    :error-message="v$.vendorId.$errors[0]?.$message"
                    @filter="getAllExpenseVendorListForDropdownFilter"
                    @blur="v$.vendorId.$touch"
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
                    <template #after><q-icon v-if="props.id == ''" name="o_add" color="primary" class="cursor-pointer q-ml-xs add-icon" @click="onAddVendor()">
                      <q-tooltip>Add new Vendor</q-tooltip>
                    </q-icon>
                    </template>
                  </q-select>
                </div>
                <div class="col-12 col-sm-4 col-md-4 col-lg-4">
                  <label class="q-mb-xs text-black">Item Name<span class="required">*</span></label>
                  <q-input
                    v-model="model.itemName"
                    type="text"
                    outlined
                    clearable
                    dense
                    :error="v$.itemName.$error"
                    :error-message="v$.itemName.$errors[0]?.$message"
                    @blur="v$.itemName.$touch"
                    @focus="markAsEdited"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-md q-mb-md">
                <div class="col-12 col-sm-4 col-md-4 col-lg-4">
                  <label class="q-mb-xs text-black">Item Category<span class="required">*</span></label>
                  <q-select
                    v-model="model.itemCategoryId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="purchaseExpenseItemCategoryList"
                    option-value="value"
                    option-label="text"
                    emit-value map-options
                    :error="v$.itemCategoryId.$error"
                    :error-message="v$.itemCategoryId.$errors[0]?.$message"
                    @filter="getAllPurchaseExpenseCategoriesListForDropdownFilter"
                    @blur="v$.itemCategoryId.$touch"
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
                <div class="col-12 col-sm-4 col-md-4 col-lg-4">
                  <label class="q-mb-xs text-black">Item Sub Category</label>
                  <q-select
                    v-model="model.itemSubCategoryId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="subCategoriesList"
                    option-value="value"
                    option-label="text"
                    emit-value map-options
                    @filter="getAllExpenseSubcategoryDropDownListFilter"
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
                <div class="col-12 col-sm-4 col-md-4 col-lg-4">
                  <label class="q-mb-xs text-black">Requested By<span class="required">*</span></label>
                  <q-select
                    v-model="model.requestedById"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="employeeList"
                    option-value="value"
                    option-label="text"
                    emit-value map-options
                    :error="v$.requestedById.$error"
                    :error-message="v$.requestedById.$errors[0]?.$message"
                    @filter="getAllEmployeesListForDropdownFilter"
                    @blur="v$.requestedById.$touch"
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
              <div class="row q-col-gutter-md">
                <div class="col-12 col-sm-4 col-md-4 col-lg-4">
                  <label class="q-mb-xs text-black">Purchase By<span class="required">*</span></label>
                  <q-select
                    v-model="model.purchaserId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="employeeList"
                    option-value="value"
                    option-label="text"
                    emit-value map-options
                    :error="v$.purchaserId.$error"
                    :error-message="v$.purchaserId.$errors[0]?.$message"
                    @filter="getAllEmployeesListForDropdownFilter"
                    @blur="v$.purchaserId.$touch"
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
                <div class="col-12 col-sm-4 col-md-4 col-lg-4">
                  <label class="q-mb-xs text-black">Quantity<span class="required">*</span></label>
                  <q-input
                    v-model="model.quantity"
                    type="text"
                    maxlength="5"
                    outlined
                    clearable
                    dense
                    :error="v$.quantity.$error"
                    :error-message="v$.quantity.$errors[0]?.$message"
                    @blur="v$.quantity.$touch"
                    @focus="markAsEdited"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4 col-lg-4">
                  <label class="q-mb-xs text-black">Estimated Rate<span class="required">*</span></label>
                  <q-input
                    v-model="model.estimatedRate"
                    type="text"
                    maxlength="8"
                    outlined
                    clearable
                    dense
                    :error="v$.estimatedRate.$error"
                    :error-message="v$.estimatedRate.$errors[0]?.$message"
                    @blur="v$.estimatedRate.$touch"
                    @focus="markAsEdited"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-md q-mb-md">
                <div class="col-12 col-sm-4 col-md-4 col-lg-4">
                  <label class="q-mb-xs text-black">Discount<span class="required">*</span></label>
                  <q-input
                    v-model="model.discount"
                    type="text"
                    maxlength="4"
                    outlined
                    clearable
                    dense
                    :error="v$.discount.$error"
                    :error-message="v$.discount.$errors[0]?.$message"
                    @blur="v$.discount.$touch"
                    @focus="markAsEdited"
                  />
                </div>
                <div class="col-12 col-sm-4 col-md-4 col-lg-4">
                  <label class="q-mb-xs text-black">Estimated Amount<span class="required">*</span></label>
                  <q-input
                    v-model="model.estimatedAmount"
                    type="text"
                    maxlength="8"
                    outlined
                    clearable
                    dense
                    :error="v$.estimatedAmount.$error"
                    :error-message="v$.estimatedAmount.$errors[0]?.$message"
                    @blur="v$.estimatedAmount.$touch"
                    @focus="markAsEdited"
                  />
                </div>
                <div class="col-12 col-sm-12 col-md-12 col-lg-12">
                  <div class="q-mb-xs text-black"><label>Description</label></div>
                  <div class="form-group">
                    <q-editor
                      v-model="model.description"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                      @focus="markAsEdited"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-lg" @paste="handlePaste">
                <div class="col-12 q-mb-xs text-black">Purchase Expense Files</div>

                <!-- File Uploader -->
                <div class="col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12">
                  <div class="form-group">
                    <q-uploader
                      ref="documentUploaderRef"
                      v-model="model.expensePurchaseRequestFiles"
                      class="prodUploader"
                      color="white"
                      text-color="dark"
                      with-credentials
                      hide-upload-btn
                      multiple
                      field-name="ProjectFiles"
                      flat
                      bordered
                      label="Drag files here or (+) to upload."
                      @added="onFileAdded"
                      @removed="onFileRemoved"
                      @focus="markAsEdited"
                    />
                    <div class="text-grey-7 text-caption q-mt-xs">
                      <i>Allowed Files: jpg, png, jpeg, pdf, excel, doc, ppt</i>
                    </div>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-lg">
                <!-- Display Files in Square Boxes with File Name Below -->
                <div v-if="model.expensePurchaseRequestFiles && model.expensePurchaseRequestFiles.length > 0" class="row q-gutter-md">
                  <div
                    v-for="(file, index) in model.expensePurchaseRequestFiles"
                    :key="index"
                    class="col-3 position-relative file-card text-center"
                    style="max-width: 140px; min-width: 140px;"
                  >
                    <div class="file-preview square-box">
                      <template v-if="isImageFile(file)">
                        <img :src="file.file.virtualPath ? file.file.virtualPath : getFilePreview(file.file)" alt="File Preview" class="square-content centered-image">
                      </template>
                      <template v-else>
                        <q-icon :name="getFileIcon(file.file?.mimeType)" class="file-icon square-content" size="70px" />
                      </template>
                      <div class="file-name q-mt-sm">
                        <q-btn v-if="file.file?.virtualPath || file?.name" class="bg-primary text-white q-pa-xs" no-caps @click="viewFile(file)">
                          <span class="truncate-text">
                            {{ file.file?.name || file.name || extractFileName(file.file?.seoFilename) }}
                          </span>
                        </q-btn>
                      </div>
                    </div>
                    <!-- Remove Button -->
                    <q-btn color="negative" flat round dense icon="o_close" class="remove-file-icon" @click="removeFile(index)" />
                  </div>
                </div>
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
            v-if="id && model.purchaseRequestStatus?.dropDownValue !== 'Submitted'"
            label="Update and Save Draft"
            color="primary"
            class="actionBtn"
            no-caps
            style="width: auto;"
            :loading="processing && activeButton === 'Draft'"
            :disabled="processing"
            @click="submitForm('Draft')"
          />
          <q-btn
            v-else-if="!id"
            label="Save Draft"
            color="primary"
            class="actionBtn"
            no-caps
            :loading="processing && activeButton === 'Draft'"
            :disabled="processing"
            @click="submitForm('Draft')"
          />
          <q-btn
            v-if="id || model.purchaseRequestStatus?.dropDownValue === 'Submitted'"
            label="Update And Close"
            color="primary"
            class="actionBtn"
            no-caps
            :loading="processing && activeButton === 'Submitted'"
            :disabled="processing"
            @click="submitForm('Submitted')"
          />
          <q-btn
            v-else-if="!id"
            label="Submit And Close"
            color="primary"
            class="actionBtn"
            no-caps
            :loading="processing && activeButton === 'Submitted'"
            :disabled="processing"
            @click="submitForm('Submitted')"
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted, computed, watch, toRaw } from "vue";
import { useDialogPluginComponent, useQuasar } from "quasar";
import { notifySuccess, notifyError } from "assets/utils";
import { required, helpers, decimal, numeric } from "@vuelidate/validators";
import useVuelidate from "@vuelidate/core";
import { isDate } from "validators/zw_validators.js";
import { format } from "date-fns";

import commonService from "services/common.service";
import purchaseExpensesService from "../financeExpensePurchaseRequest.service";
import employeesService from "modules/employee/employee.service";
import expenseVendorService from "modules/finance-expense-vendors/financeExpenseVendors.service";
import addVendor from "modules/finance-expense-vendors/components/addEdit.vue";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

const props = defineProps({ id: { type: String, default: "" } });
const readonlyDate = props.id ? "readonly" : "";
const { dialogRef, onDialogOK, onDialogHide } = useDialogPluginComponent();
const $q = useQuasar();
defineEmits([...useDialogPluginComponent.emits]);
const { fonts, toolbar } = getEditorConfig($q);
const processing = ref(false);
const activeButton = ref("");

const model = ref({
  requestDate: format(new Date(), "MM/dd/yyyy"),
  vendorId: "",
  itemName: "",
  itemCategoryId: "",
  itemSubCategoryId: null,
  requestedById: "",
  purchaserId: "",
  quantity: null,
  estimatedRate: null,
  discount: 0,
  estimatedAmount: null,
  description: "",
  expensePurchaseRequestFiles: [],
  expensePurchaseRequestFileFlag: "edit"
});

const rules = computed(() => ({
  vendorId: {
    required: helpers.withMessage("Vendor is required", required)
  },
  requestDate: {
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  purchaserId: {
    required: helpers.withMessage("Purchase By is required", required)
  },
  itemName: {
    required: helpers.withMessage("Item Name is required", required)
  },
  itemCategoryId: {
    required: helpers.withMessage("Item Category is required", required)
  },
  requestedById: {
    required: helpers.withMessage("Requested By is required", required)
  },
  quantity: {
    required: helpers.withMessage("Quantity is required", required),
    numeric: helpers.withMessage("Quantity is invalid", numeric)
  },
  estimatedRate: {
    required: helpers.withMessage("Estimated Rate is required", required),
    decimal: helpers.withMessage("Estimated Rate is invalid", decimal),
    numeric: helpers.withMessage("Estimated Rate is invalid", numeric)
  },
  discount: {
    required: helpers.withMessage("Discount is required", required),
    decimal: helpers.withMessage("Discount is invalid", decimal),
    numeric: helpers.withMessage("Discount is invalid", numeric)
  },
  estimatedAmount: {
    required: helpers.withMessage("Estimated Amount is required", required),
    decimal: helpers.withMessage("Estimated Amount is invalid", decimal),
    numeric: helpers.withMessage("Estimated Amount is invalid", numeric)
  }
}));

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const getPurchaseExpenseDetailsById = async (id) => {
  try {
    purchaseExpensesService.getPurchaseExpenseDetailsById(props.id).then((resp) => {
      model.value = {
        ...model.value,
        ...resp,
        expensePurchaseRequestFiles: resp.expensePurchaseRequestFileList || []
      };
      isEdited.value = resp.isEdited;
    });
  } catch (error) {
  }
};

const submitForm = async (statusType) => {
  activeButton.value = statusType;
  model.value.statusType = statusType;
  processing.value = true;
  try {
    if (await v$.value.$validate()) {
      processing.value = true;

      const formData = new FormData();
      formData.append("requestDate", model.value.requestDate);
      formData.append("vendorId", model.value.vendorId);
      formData.append("itemName", model.value.itemName);
      formData.append("itemCategoryId", model.value.itemCategoryId);
      formData.append("itemSubCategoryId", model.value.itemSubCategoryId ? model.value.itemSubCategoryId : "");
      formData.append("requestedById", model.value.requestedById);
      formData.append("purchaserId", model.value.purchaserId);
      formData.append("quantity", model.value.quantity);
      formData.append("estimatedRate", model.value.estimatedRate);
      formData.append("discount", model.value.discount);
      formData.append("estimatedAmount", model.value.estimatedAmount);
      formData.append("description", model.value.description ? model.value.description : "");
      formData.append("statusType", model.value.statusType);
      formData.append("isEdited", isEdited.value);

      if (model.value.purchaseRequestStatus?.dropDownValue === "Submitted") {
        formData.append("isSubmitted", "true");
      }

      // Append files
      toRaw(model.value.expensePurchaseRequestFiles || []).forEach((file) => {
        if (file.file?.virtualPath) {
          formData.append("ExistingFiles", JSON.stringify({
            id: file.id,
            virtualPath: file.file.virtualPath
          }));
        } else {
          formData.append("expensePurchaseRequestFiles", file);
        }
      });

      formData.append("expensePurchaseRequestFileFlag", model.value.expensePurchaseRequestFileFlag || "no_change");
      await purchaseExpensesService.savePurchaseExpenseRequest(props.id, formData);
      notifySuccess({
        message: props.id
          ? "Purchase expense updated successfully!"
          : "Purchase expense saved successfully!"
      });
      onDialogOK();
      dialogRef.value.hide();
    }
  } catch (error) {
    console.error(error);
    $q.notify({
      type: "negative",
      message: "Failed to save purchase expense."
    });
  } finally {
    setTimeout(() => {
      processing.value = false;
      activeButton.value = "";
    }, 1500);
  }
};

// Show confirmation dialog on cancel
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

// Cancel and close the dialog
const cancelAction = () => {
  dialogRef.value.hide();
};

// Called on click/focus
const isEdited = ref(false);
const markAsEdited = () => {
  if (model.value.purchaseRequestStatus?.dropDownValue !== "Submitted") return;
  isEdited.value = true;
};

const vendorList = ref([]);
const vendorListFilter = ref([]);
function getAllExpenseVendorListForDropdown () {
  expenseVendorService.getAllExpenseVendorListForDropdown().then((resp) => {
    const responseData = resp
      .map((item) => ({ text: item.vendorName, value: item.id }))
      .sort((a, b) => a.text.localeCompare(b.text));
    vendorList.value = responseData;
    vendorListFilter.value = responseData;
  });
}
function getAllExpenseVendorListForDropdownFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      vendorList.value = vendorListFilter.value;
    } else {
      vendorList.value = vendorListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const employeeList = ref([]);
const employeeListFilter = ref([]);
function getAllEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    employeeList.value = responseData;
    employeeListFilter.value = responseData;
  });
}
function getAllEmployeesListForDropdownFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = employeeListFilter.value;
    } else {
      employeeList.value = employeeListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const purchaseExpenseItemCategoryList = ref([]);
const purchaseExpenseItemCategoryFilter = ref([]);
function getAllPurchaseExpenseCategoriesListForDropdown (groupName) {
  commonService.getDropdownTypeByGroupName(groupName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.type, value: item.id }));
    purchaseExpenseItemCategoryList.value = responseData;
    purchaseExpenseItemCategoryFilter.value = responseData;
  });
  // purchaseExpensesService.getPurchaseExpenseCategoriesListForDropdown().then((resp) => {
  //   const responseData = resp.map((item) => ({ text: item.text, value: item.value }));
  //   purchaseExpenseItemCategoryList.value = responseData;
  //   purchaseExpenseItemCategoryFilter.value = responseData;
  // });
}
function getAllPurchaseExpenseCategoriesListForDropdownFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      purchaseExpenseItemCategoryList.value = purchaseExpenseItemCategoryFilter.value;
    } else {
      purchaseExpenseItemCategoryList.value = purchaseExpenseItemCategoryFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const subCategoriesList = ref([]);
const subCategoriesListFilter = ref([]);
// function getSubcategoryByCatgoryId (CategoryId) {
//   model.value.itemSubCategoryId = null;
//   subCategoriesList.value = [];
//   if (CategoryId) {
//     getAllExpenseSubcategoryDropDownList(CategoryId);
//   }
// }

function getAllExpenseSubcategoryDropDownList () {
  commonService.getDropdownByTypeId(model.value.itemCategoryId).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id, data: item.description }));
    subCategoriesList.value = responseData;
    subCategoriesListFilter.value = responseData;
  });
}
function getAllExpenseSubcategoryDropDownListFilter (val, update, abort, counter) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      subCategoriesList.value = subCategoriesListFilter.value;
    } else {
      subCategoriesList.value = subCategoriesListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
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

// Handles file selection and validation when files are added
const onFileAdded = (files) => {
  if (!files || files.length === 0) return;

  if (!model.value.expensePurchaseRequestFiles) {
    model.value.expensePurchaseRequestFiles = [];
  }

  const invalidFiles = files.filter(file => !isValidFile(file));
  const validFiles = files.filter(isValidFile);
  // Show an alert if there are invalid files
  if (invalidFiles.length > 0) {
    const invalidFileNames = invalidFiles.map(file => file.name).join(", ");
    notifyError({ message: `The following file type is not allowed: ${invalidFileNames}` });
  }

  // Add a "new" flag to the newly added files
  validFiles.forEach(file => {
    file.flag = "new"; // Mark file as "new"
  });
  invalidFiles.forEach((file) => {
    documentUploaderRef.value?.removeFile(file);
  });

  model.value.expensePurchaseRequestFiles.push(...validFiles);
  model.value.expensePurchaseRequestFileFlag = "edit"; // Set the overall flag for tracking
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

// Upload Image
const documentUploaderRef = ref(null);
const allowedExtensions = [".pdf", ".xls", ".xlsx", ".doc", ".docx", ".jpeg", ".jpg", ".png", ".gif", ".ppt", ".pptx"];
const allowedFileTypes = [
  "application/pdf", // PDF
  "application/vnd.ms-excel", // Excel (old format)
  "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // Excel (new format)
  "application/msword", // Word (old format)
  "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // Word (new format)
  "image/jpeg", "image/png", "image/gif", // Images
  "application/vnd.ms-powerpoint", // PowerPoint (old format)
  "application/vnd.openxmlformats-officedocument.presentationml.presentation" // PowerPoint (new format)
];

const isValidFile = (file) => {
  // Normalize type by trimming
  const mimeType = file.type ? file.type.trim() : "";
  const fileName = file.name ? file.name.toLowerCase() : "";

  // Check MIME type
  const fileTypeValid = mimeType && allowedFileTypes.includes(mimeType);

  // Check file extension as a fallback (for edge cases)
  const fileExtensionValid = fileName && allowedExtensions.some(ext => fileName.endsWith(ext));

  return fileTypeValid || fileExtensionValid; // Pass if either check succeeds
};

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
        ${imageFormats.includes(fileExtension) ? `<img src="${fileUrl}" alt="Image Preview">` : `<iframe src="${viewerUrl}"></iframe>`}
      </body>
    </html>
  `);
  }, 100);
}

function getFileIcon (mimeType) {
  const mimeToIconMap = {
    "application/pdf": "o_picture_as_pdf",
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": "o_insert_chart",
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document": "o_description",
    "application/vnd.openxmlformats-officedocument.presentationml.presentation": "o_slideshow",
    "application/vnd.ms-powerpoint": "o_slideshow",
    "application/zip": "o_folder_zip",
    "text/plain": "o_article",
    "image/png": "o_image",
    "image/jpeg": "o_image",
    "image/gif": "o_image",
    default: "o_insert_drive_file"
  };

  return mimeToIconMap[mimeType] || mimeToIconMap.default;
}

function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
}

function removeFile (index) {
  const file = model.value.expensePurchaseRequestFiles[index];
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
  model.value.expensePurchaseRequestFiles.splice(index, 1);
  if (model.value.expensePurchaseRequestFiles.length === 0) {
    model.value.expensePurchaseRequestFileFlag = "remove";
  }
}

const onFileRemoved = (removedFiles) => {
  removedFiles.forEach((removedFile) => {
    model.value.expensePurchaseRequestFiles = model.value.expensePurchaseRequestFiles.filter(
      file =>
        file.name !== removedFile.name &&
        file.file?.name !== removedFile.name
    );
  });
};

onMounted(() => {
  getAllPurchaseExpenseCategoriesListForDropdown("Expense Category");
  getAllExpenseVendorListForDropdown();
  getAllEmployeesListForDropdown();
  if (props.id) getPurchaseExpenseDetailsById(props.id);
});

watch(() => model.value.itemCategoryId,
  (newVal) => {
    if (newVal) {
      getAllExpenseSubcategoryDropDownList();
    } else {
      // if category is cleared, also clear subcategory list
      subCategoriesList.value = [];
      model.value.itemSubCategoryId = null;
    }
  }, { immediate: true }
);
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
