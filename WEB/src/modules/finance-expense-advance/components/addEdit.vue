<template>
  <q-dialog ref="dialogRef" class="customDialog DialogContainer" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 100vw !important;">
      <!-- Header Section -->
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Advance Expense</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense :disable="processing" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="submitForm">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset class="q-mt-xs">
              <legend class="text-primary text-h6">{{ id ? "Edit" : "Add" }} Advance Expense</legend>
              <div class="row q-col-gutter-md q-mb-md">
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
                  <label class="q-mb-xs text-black">Item Category<span class="required">*</span></label>
                  <q-select
                    v-model="model.itemCategoryId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="advanceExpenseItemCategoryList"
                    option-value="value"
                    option-label="text"
                    emit-value map-options
                    :error="v$.itemCategoryId.$error"
                    :error-message="v$.itemCategoryId.$errors[0]?.$message"
                    @filter="getAllAdvanceExpenseCategoriesListForDropdownFilter"
                    @blur="v$.itemCategoryId.$touch"
                    @update:model-value="(value) => { getSubcategoryByCatgoryId(value, props.rowIndex); }"
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
              </div>
              <div class="row q-col-gutter-md q-mb-md">
                <div class="col-12 col-sm-4 col-md-4 col-lg-4">
                  <label class=" q-mb-xs text-black">Location<span class="required">*</span></label>
                  <q-select
                    v-model="model.locationId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="locationList"
                    option-value="value"
                    option-label="text"
                    emit-value map-options
                    :error="v$.locationId.$error"
                    :error-message="v$.locationId.$errors[0]?.$message"
                    @filter="getAllLocationsDropDownListFilter"
                    @blur="v$.locationId.$touch"
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
                  <label class=" q-mb-xs text-black">Payment Type<span class="required">*</span></label>
                  <q-select
                    v-model="model.paymentTypeId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="paymentTypeList"
                    option-value="value"
                    option-label="text"
                    emit-value map-options
                    :error="v$.paymentTypeId.$error"
                    :error-message="v$.paymentTypeId.$errors[0]?.$message"
                    @filter="getAllPaymentTypeDropDownListFilter"
                    @blur="v$.paymentTypeId.$touch"
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
                    v-model="model.requestedBy"
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
                    :error="v$.requestedBy.$error"
                    :error-message="v$.requestedBy.$errors[0]?.$message"
                    @filter="getAllEmployeesListForDropdownFilter"
                    @blur="v$.requestedBy.$touch"
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
                  <label class="q-mb-xs text-black">Amount<span class="required">*</span></label>
                  <q-input
                    v-model="model.amount"
                    min="1"
                    maxlength="8"
                    type="text"
                    outlined
                    clearable
                    dense
                    :error="v$.amount.$error"
                    :error-message="v$.amount.$errors[0]?.$message"
                    @blur="v$.amount.$touch"
                    @focus="markAsEdited"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-md">
                <div class="col-12 col-sm-12 col-md-12 col-lg-12 q-mb-sm">
                  <div class="q-mb-xs text-black">
                    <label class="q-mb-xs text-black">Notes</label>
                  </div>
                  <div class="form-group">
                    <q-input
                      v-model="model.notes"
                      outlined
                      dense
                      autogrow
                      @focus="markAsEdited"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-md">
                <div class="col-12 col-sm-12 col-md-12 col-lg-12">
                  <div class="form-group">
                    <q-checkbox
                      v-model="model.applyToTrip"
                      :true-value="true"
                      :false-value="false"
                      class="text-h4"
                      label="Apply To Trip"
                      @focus="markAsEdited"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-md q-mb-lg">
                <div class="col-12 col-sm-12 col-md-12 col-lg-12">
                  <div class="q-mb-xs text-black">
                    <label class="q-mb-xs text-black">Advance Details<span class="required">*</span>
                    </label>
                  </div>
                  <div class="form-group">
                    <q-editor
                      v-model="model.advanceDetails"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                      :error="v$.advanceDetails.$error"
                      :error-message="v$.advanceDetails.$errors[0]?.$message"
                      @blur="v$.advanceDetails.$touch"
                      @focus="markAsEdited"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-lg" @paste="handlePaste">
                <div class="col-12 q-mb-xs text-black">Advance Expense Files</div>
                <!-- File Uploader -->
                <div class="col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12">
                  <div class="form-group">
                    <!-- <q-uploader
                      ref="documentUploaderRef"
                      v-model="model.expenseAdvanceRequestFiles"
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
                    </div> -->
                    <multiFileUploader
                      :initialFiles="model.expenseAdvanceRequestFiles"
                      :allowedExtensions="[
                        '.jpg','.xls','.xlsx','.doc','.docx','.jpeg','.jpg','.png','.ppt','.pptx', 'pdf'
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
                <div v-if="model.expenseAdvanceRequestFiles && model.expenseAdvanceRequestFiles.length > 0" class="row q-gutter-md">
                  <div
                    v-for="(file, index) in model.expenseAdvanceRequestFiles"
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
            v-if="id && model.advanceExpenseStatus?.dropDownValue !== 'Submitted'"
            label="Update and Save Draft"
            color="primary"
            class="actionBtn"
            no-caps
            style="width: auto;"
            :disabled="processing"
            :loading="processing && activeButton === 'Draft'"
            @click="submitForm('Draft')"
          />
          <q-btn
            v-else-if="!id"
            label="Save Draft"
            color="primary"
            class="actionBtn"
            no-caps
            :disabled="processing"
            :loading="processing && activeButton === 'Draft'"
            @click="submitForm('Draft')"
          />
          <q-btn
            v-if="id || model.advanceExpenseStatus?.dropDownValue === 'Submitted'"
            label="Update And Close"
            color="primary"
            class="actionBtn"
            no-caps
            :disabled="processing"
            :loading="processing && activeButton === 'Submitted'"
            @click="submitForm('Submitted')"
          />
          <q-btn
            v-else-if="!id"
            label="Submit And Close"
            color="primary"
            class="actionBtn"
            no-caps
            :disabled="processing"
            :loading="processing && activeButton === 'Submitted'"
            @click="submitForm('Submitted')"
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted, computed, toRaw } from "vue";
import { useDialogPluginComponent, useQuasar } from "quasar";
import { isDate } from "validators/zw_validators.js";
import { notifySuccess } from "assets/utils";
import { required, helpers, decimal, numeric } from "@vuelidate/validators";
import useVuelidate from "@vuelidate/core";
import { format } from "date-fns";
import _ from "lodash";

import commonService from "services/common.service";
import advanceExpensesService from "../financeExpenseAdvance.service";
import employeesService from "modules/employee/employee.service";

// Shared Inputs
import multiFileUploader from "src/components/form-inputs/_multiFileUpload.vue";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";


const $q = useQuasar();
const props = defineProps({ id: { type: String, default: "" } });
const readonlyDate = props.id ? "readonly" : "";
const { fonts, toolbar } = getEditorConfig($q);
const processing = ref(false);
const activeButton = ref("");

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogOK, onDialogHide } = useDialogPluginComponent();

const model = ref({
  requestedBy: "",
  paymentTypeId: "",
  locationId: "",
  referenceId: "",
  requestDate: format(new Date(), "MM/dd/yyyy"),
  amount: "",
  applyToTrip: false,
  advanceDetails: "",
  notes: "",
  itemCategoryId: "",
  itemSubCategoryId: null,
  expenseAdvanceRequestFiles: [],
  expenseAdvanceRequestFileFlag: "edit"
});

const rules = computed(() => ({
  locationId: {
    required: helpers.withMessage("Location is required", required)
  },
  itemCategoryId: {
    required: helpers.withMessage("Item Category is required", required)
  },
  requestedBy: {
    required: helpers.withMessage("Requested By is required", required)
  },
  paymentTypeId: {
    required: helpers.withMessage("Payment Type is required", required)
  },
  advanceDetails: {
    required: helpers.withMessage("Advance details Type is required", required)
  },
  requestDate: {
    required: helpers.withMessage("Request Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  amount: {
    required: helpers.withMessage("Amount is required", required),
    decimal: helpers.withMessage("Amount is invalid", decimal),
    numeric: helpers.withMessage("Amount is invalid", numeric)
  }
}));
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const getAdvanceExpenseDetailsById = async (id) => {
  try {
    advanceExpensesService.getAdvanceExpenseDetailsById(props.id).then((resp) => {
      model.value = _.cloneDeep(resp);
      getAllExpenseSubcategoryDropDownList(resp.itemCategoryId);
      isEdited.value = resp.isEdited;
      model.value.itemSubCategoryId = resp.itemSubCategory ? resp.itemSubCategory.id : null;
      model.value.expenseAdvanceRequestFiles = resp.expenseAdvanceRequestFileList || [];
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
      formData.append("requestedBy", model.value.requestedBy);
      formData.append("paymentTypeId", model.value.paymentTypeId);
      formData.append("locationId", model.value.locationId);
      formData.append("referenceId", model.value.referenceId);
      formData.append("requestDate", model.value.requestDate);
      formData.append("amount", model.value.amount);
      formData.append("applyToTrip", model.value.applyToTrip);
      formData.append("advanceDetails", model.value.advanceDetails || "");
      formData.append("notes", model.value.notes || "");
      formData.append("itemCategoryId", model.value.itemCategoryId);
      formData.append("itemSubCategoryId", model.value.itemSubCategoryId);
      formData.append("statusType", model.value.statusType);
      formData.append("isEdited", isEdited.value);
      if (model.value.advanceExpenseStatus?.dropDownValue === "Submitted") {
        formData.append("isSubmitted", "true");
      }

      toRaw(model.value.expenseAdvanceRequestFiles || []).forEach((file) => {
        if (file.file && file.file.virtualPath) {
          // For existing files, append metadata instead of the file itself
          formData.append("ExistingFiles", JSON.stringify({
            id: file.id,
            virtualPath: file.file.virtualPath
            // flag: file.flag || "new"
          }));
        } else {
          // For new files, append as raw file objects (IFormFile)
          formData.append("expenseAdvanceRequestFiles", file);
        }
      });
      await advanceExpensesService.saveAdvanceExpenseRequest(props.id, formData).then(() => {
        notifySuccess({ message: props.id ? "Advance expense updated successfully!" : "Advance expense saved successfully!" });
        onDialogOK();
        dialogRef.value.hide();
        $q.loading.hide();
      });
    }
  } catch (error) {
    $q.loading.hide();
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
  if (model.value.advanceExpenseStatus?.dropDownValue !== "Submitted") return;
  isEdited.value = true;
};

const locationList = ref([]);
const locationListFilter = ref([]);
function getAllLocationsDropDownList (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    locationList.value = responseData;
    locationListFilter.value = responseData;
  });
}
function getAllLocationsDropDownListFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      locationList.value = locationListFilter.value;
    } else {
      locationList.value = locationListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const paymentTypeList = ref([]);
const paymentTypeFilter = ref([]);
function getAllPaymentTypeDropDownList (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    paymentTypeList.value = responseData;
    paymentTypeFilter.value = responseData;
  });
}
function getAllPaymentTypeDropDownListFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      paymentTypeList.value = paymentTypeFilter.value;
    } else {
      paymentTypeList.value = paymentTypeFilter.value.filter(v => v.text.toLowerCase().includes(needle));
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

const advanceExpenseItemCategoryList = ref([]);
const advanceExpenseItemCategoryFilter = ref([]);
function getAllAdvanceExpenseCategoriesListForDropdown (groupName) {
  commonService.getDropdownTypeByGroupName(groupName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.type, value: item.id }));
    advanceExpenseItemCategoryList.value = responseData;
    advanceExpenseItemCategoryFilter.value = responseData;
  });
}
function getAllAdvanceExpenseCategoriesListForDropdownFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      advanceExpenseItemCategoryList.value = advanceExpenseItemCategoryFilter.value;
    } else {
      advanceExpenseItemCategoryList.value = advanceExpenseItemCategoryFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const subCategoriesList = ref([]);
const subCategoriesListFilter = ref([]);
function getSubcategoryByCatgoryId (CategoryId) {
  model.value.itemSubCategoryId = null;
  subCategoriesList.value = [];
  if (CategoryId) {
    getAllExpenseSubcategoryDropDownList(CategoryId);
  }
}

function getAllExpenseSubcategoryDropDownList (CategoryId) {
  commonService.getDropdownByTypeId(CategoryId).then((resp) => {
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

// Handles file selection and validation when files are added
// const onFileAdded = (files) => {
//   if (!files || files.length === 0) return;

//   if (!model.value.expenseAdvanceRequestFiles) {
//     model.value.expenseAdvanceRequestFiles = [];
//   }

//   const invalidFiles = files.filter(file => !isValidFile(file));
//   const validFiles = files.filter(isValidFile);
//   // Show an alert if there are invalid files
//   if (invalidFiles.length > 0) {
//     const invalidFileNames = invalidFiles.map(file => file.name).join(", ");
//     notifyError({ message: `The following file type is not allowed: ${invalidFileNames}` });
//   }

//   // Add a "new" flag to the newly added files
//   validFiles.forEach(file => {
//     file.flag = "new"; // Mark file as "new"
//   });
//   invalidFiles.forEach((file) => {
//     documentUploaderRef.value?.removeFile(file);
//   });

//   model.value.expenseAdvanceRequestFiles.push(...validFiles);
//   model.value.expenseAdvanceRequestFileFlag = "edit"; // Set the overall flag for tracking
// };

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

  // if (files.length > 0 && documentUploaderRef.value) {
  //   // Add files to uploader
  //   documentUploaderRef.value.addFiles(files);
  // }
}

// Upload Image
// const documentUploaderRef = ref(null);
// const allowedExtensions = [".pdf", ".xls", ".xlsx", ".doc", ".docx", ".jpeg", ".jpg", ".png", ".gif", ".ppt", ".pptx"];
// const allowedFileTypes = [
//   "application/pdf", // PDF
//   "application/vnd.ms-excel", // Excel (old format)
//   "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // Excel (new format)
//   "application/msword", // Word (old format)
//   "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // Word (new format)
//   "image/jpeg", "image/png", "image/gif", // Images
//   "application/vnd.ms-powerpoint", // PowerPoint (old format)
//   "application/vnd.openxmlformats-officedocument.presentationml.presentation" // PowerPoint (new format)
// ];

// const isValidFile = (file) => {
//   // Normalize type by trimming
//   const mimeType = file.type ? file.type.trim() : "";
//   const fileName = file.name ? file.name.toLowerCase() : "";

//   // Check MIME type
//   const fileTypeValid = mimeType && allowedFileTypes.includes(mimeType);

//   // Check file extension as a fallback (for edge cases)
//   const fileExtensionValid = fileName && allowedExtensions.some(ext => fileName.endsWith(ext));

//   return fileTypeValid || fileExtensionValid; // Pass if either check succeeds
// };

// function handleFiles (files) {
//   const existingFiles = model.value.expenseAdvanceRequestFiles || [];

//   // Merge without losing old files
//   model.value.expenseAdvanceRequestFiles = [...existingFiles, ...files];

//   model.value.expenseAdvanceRequestFileFlag = "edit";
// }

function handleFiles (files) {
  model.value.expenseAdvanceRequestFiles = files;
  model.value.expenseAdvanceRequestFileFlag = "edit";
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
  const file = model.value.expenseAdvanceRequestFiles[index];
  if (!file) return;

  // REMOVE FROM UPLOADER ALSO
  // const uploader = documentUploaderRef.value;
  // if (uploader) {
  //   const uploaderFile = uploader.files.find(
  //     f =>
  //       f.name === file.name ||
  //       f.name === file.file?.name
  //   );
  //   if (uploaderFile) {
  //     uploader.removeFile(uploaderFile);
  //   }
  // }

  // Existing file (from DB)
  if (file.file?.virtualPath) {
    file.flag = "remove";
  }
  model.value.expenseAdvanceRequestFiles.splice(index, 1);
  if (model.value.expenseAdvanceRequestFiles.length === 0) {
    model.value.expenseAdvanceRequestFileFlag = "remove";
  }
}

// const onFileRemoved = (removedFiles) => {
//   removedFiles.forEach((removedFile) => {
//     model.value.expenseAdvanceRequestFiles = model.value.expenseAdvanceRequestFiles.filter(
//       file =>
//         file.name !== removedFile.name &&
//         file.file?.name !== removedFile.name
//     );
//   });
// };

onMounted(() => {
  getAllLocationsDropDownList("Location");
  getAllPaymentTypeDropDownList("Payment Type");
  getAllAdvanceExpenseCategoriesListForDropdown("Expense Category");
  getAllEmployeesListForDropdown();
  if (props.id) getAdvanceExpenseDetailsById(props.id);
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
