<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw; max-width: 60vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Training</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Training Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-6">
                  <label class="label q-mb-xs text-black">Training Name<span class="required">*</span></label>
                  <q-input
                    v-model="model.name"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    maxlength="128"
                    :error="v$.name.$error"
                    :error-message="v$.name.$errors[0]?.$message"
                    @blur="v$.name.$touch"
                  />
                </div>
                <div class="col-12 col-md-6">
                  <label class="label q-mb-xs text-black">URL/Link</label>
                  <q-input
                    v-model="model.url"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    maxlength="2048"
                    autogrow
                    type="url"
                    :error="v$.url.$error"
                    :error-message="v$.url.$errors[0]?.$message"
                    @blur="v$.url.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-6">
                  <label class="label q-mb-xs text-black">Assign To</label>
                  <q-select
                    v-model="model.employeeDesignationIdsArray"
                    push
                    class="w-100 h-auto"
                    outlined
                    use-input
                    use-chips
                    clearable
                    transition-show="jump-up"
                    transition-hide="jump-up"
                    hide-bottom-space
                    :dense="true"
                    multiple
                    fill-input
                    input-debounce="0"
                    :options="employeeDesignationList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    @filter="getAllDesignationListDropDownForFilter"
                  >
                    <template #option="{ itemProps, opt, selected, toggleOption }">
                      <q-item v-bind="itemProps">
                        <q-item-section>
                          <div class="row q-col-gutter-x-md items-center">
                            <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                            <span>{{ opt.text }}</span>
                          </div>
                        </q-item-section>
                      </q-item>
                    </template>
                  </q-select>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-12">
                  <div class="form-group">
                    <label class="label q-mb-xs text-black">Description</label>
                    <q-editor
                      v-model="model.description"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">                
                <div class="col-12 col-md-5">
                  <label class="label q-mb-xs text-black">Training File</label>
                  <!-- <div v-if="!model.trainingFileId" class="form-group">
                    <q-uploader
                      ref="documentUploaderRef" color="white" text-color="dark" with-credentials hide-upload-btn style="min-height: 128px; width: 100%" field-name="trainingfile"
                      flat bordered label="Drag file here or (+) to upload. (File)" @uploaded="onUploaded" @added="onFileAdded" @removed="onFileRemoved"
                    />
                  </div> -->
                  <!-- <div v-if="model.trainingFileId" class="row justify-center">
                    <img :src="model.virtualPath" alt="" style="width: 30%;">
                  </div> -->
                  <singleFileUploader
                    :allowedExtensions="[
                      '.pdf','.xls','.xlsx','.doc','.docx','.jpeg','.jpg','.png','.ppt','.pptx'
                    ]"
                    :maxSizeInMb="25"
                    label="Drag file here or (+) to upload. (File)"
                    @file-selected="handleFile"
                    @file-valid="isFileValid = $event"
                  />
                  <div v-if="model.trainingFileId" class="row justify-center q-mt-sm">
                    <a :href="model.file.virtualPath" target="_blank" class="q-mr-md">
                      <i class="fa fa-file q-ml-md" style="font-size: 25px; color: gray; transition: transform 0.2s, color 0.2s;" />
                      <span style="display: block; font-size: 14px; color: #555; margin-top: 8px;">
                        View File
                      </span>
                    </a>
                    <q-btn
                      color="negative"
                      label="Remove"
                      style="font-size: 12px;"
                      outline
                      no-caps
                      @click="clearFile"
                    />
                  </div>
                  <!-- <div v-if="errorMessage" class="text-negative q-mt-sm">
                    {{ errorMessage }}
                  </div> -->
                </div>
              </div>
            </fieldset>
          </div>
        </div>
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
import { useDialogPluginComponent, useQuasar } from "quasar";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import { notifySuccess, notifyError, zwConfirm, notifyWarning } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import _ from "lodash";
import trainingService from "modules/training-portal/trainingPortal.service";
import commonService from "services/common.service";

// Shared Inputs
import singleFileUploader from "src/components/form-inputs/_singleFileUpload.vue";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const trainingId = props.id;

// Define emits
const $emit = defineEmits(["hide", "ok"]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// Common variables
const $q = useQuasar();
const loading = ref(true);
const processing = ref(false);
const isFileValid = ref(true);

// Define model values
const model = ref({
  employeeDesignationId: "",
  EmployeeDesignationIdsArray: [],
  name: "",
  url: "",
  description: "",
  trainingFileId: "",
  virtualPath: ""
});

// Validation rules
const rules = {
  name: { required: helpers.withMessage("Training name is required", required) },
  // url: {
  //   validUrl: helpers.withMessage("Invalid URL", value => {
  //     if (!value) return true; // optional field
  //     // Simple URL regex
  //     const pattern = /^(https?:\/\/)?([\w-]+(\.[\w-]+)+)(\/[\w-./?%&=]*)?$/;
  //     return pattern.test(value);
  //   })
  // }
  url: {
    validUrl: helpers.withMessage("Invalid URL", value => {
      if (!value) return true;
      return URL.canParse(value);
    })
  }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Get all Employee Designation list
const employeeDesignationList = ref([]);
const employeeDesignationListFilter = ref([]);
function getAllDesignationListForDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    employeeDesignationList.value = responseData;
    employeeDesignationListFilter.value = responseData;
  });
}

// Search employee designation for dropdown
function getAllDesignationListDropDownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeDesignationList.value = employeeDesignationListFilter.value;
    } else {
      employeeDesignationList.value = employeeDesignationListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// -------------------------------------------------------------------------------------------------------
// Upload Image
// -------------------------------------------------------------------------------------------------------
// const documentUploaderRef = ref(null);
// // let errorMessage = "";
// const errorMessage = ref("");
// function onFileAdded (files) {
//   if (!files || !files.length) return;
//   const allowedTypes = ["image/jpeg", // JPEG images
//     "image/png", // JPEG images
//     "application/pdf", // PDF files
//     "application/vnd.ms-powerpoint", // PowerPoint files (older .ppt format)
//     "application/vnd.openxmlformats-officedocument.presentationml.presentation", // PowerPoint files (.pptx format)
//     "application/vnd.ms-excel", // Excel files (older .xls format)
//     "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // Excel files (.xlsx format)
//     "application/msword", // Word files (.doc format)
//     "application/vnd.openxmlformats-officedocument.wordprocessingml.document" // Word files (.docx format)
//   ];
//   const file = files[0];
//   // Check if the file type is valid
//   if (allowedTypes.includes(file.type)) {
//     // Valid file type, update the model
//     model.value.filePic = files[0];
//     model.value.fileChangeFlag = "edit";
//     // Reset any error message (if needed)
//     errorMessage.value = "";
//   } else {
//     // Invalid file type, show an error message
//     errorMessage.value = "Please select a valid image (jpg, png), PDF, PPT, Excel, or Word file.";
//     // Optionally clear the file input or reset the model to prevent invalid file submission
//     model.value.filePic = null;
//     model.value.fileChangeFlag = null;
//   }
// }

// function onUploaded (info) {
//   notifySuccess({ message: "File Uploaded successfully." });
//   documentUploaderRef.value.reset();
// }

function clearFile () {
  zwConfirm({ message: "Do you want to clear this file ?" }, () => {
    model.value.trainingFileId = null;
    model.value.fileChangeFlag = "remove";
  }, () => {
  });
}

// function onFileRemoved () {
//   errorMessage.value = "";
// }

function handleFile (file) {
  if (file) {
    model.value.filePic = file;
    model.value.fileChangeFlag = "edit";
  } else {
    model.value.filePic = null;
    model.value.trainingFileId = null;
    model.value.fileChangeFlag = "remove";
    isFileValid.value = true;
  }
}
// -------------------------------------------------------------------------------------------------------

// get training details on edit mode
const getTraining = () => {
  loading.value = true;
  trainingService.getTraining(trainingId).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.virtualPath = resp.file ? resp.file.virtualPath : "";
    model.value.name = resp.name;
    model.value.description = resp.description ? resp.description : "";
    model.value.employeeDesignationIdsArray = resp.trainingPortalMappings.map(mapping => mapping.employeeDesignationId);
  }).finally(() => {
    loading.value = false;
  });
};

async function onSubmit () {
  processing.value = true;
  try {
    if (!await v$.value.$validate()) {
      return;
    }
    if (!isFileValid.value && model.value.filePic) {
      notifyWarning({ message: "Please upload a valid file" });
      isFileValid.value = true;
      return;
    }
    // if (errorMessage.value !== "") return;
    trainingService.saveTraining(props.id, model.value).then(resp => {
      notifySuccess({ message: "Training saved successfully." });
      $emit("ok");
      $emit("hide");
    });
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getTraining();
  }
}, { immediate: true });

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Rich Editor Tools: Fonts and Toolbar
// --------------------------------------------------------------------------------------------------------------------------------------------------

const fonts = {
  arial: "Arial",
  arial_black: "Arial Black",
  comic_sans: "Comic Sans MS",
  courier_new: "Courier New",
  impact: "Impact",
  lucida_grande: "Lucida Grande",
  times_new_roman: "Times New Roman",
  verdana: "Verdana"
};

const toolbar = [
  [
    {
      label: $q.lang.editor.align,
      icon: $q.iconSet.editor.align,
      fixedLabel: true,
      list: "only-icons",
      options: ["left", "center", "right", "justify"]
    }
  ],
  ["bold", "italic", "strike", "underline"],
  ["token", "hr", "link", "custom_btn"],
  [
    {
      label: $q.lang.editor.formatting,
      icon: $q.iconSet.editor.formatting,
      list: "no-icons",
      options: ["p", "h1", "h2", "h3", "h4", "h5", "h6", "code"]
    },
    "removeFormat"
  ],
  ["quote", "unordered", "ordered", "outdent", "indent"],
  ["undo", "redo"],
  ["viewsource"]
];

// On page rendering
onMounted(() => {
  getAllDesignationListForDropDown("Employee Designation");
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
