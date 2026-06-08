<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Task Assignment</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <!-- <q-card-section class="card-header with-tools"> -->
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>
                <q-breadcrumbs class="text-brown text-weight-bold text-h3">
                  <template #separator>
                    <q-icon size="1.5em" name="o_chevron_right" color="primary" />
                  </template>
                  <q-breadcrumbs-el :label="'Project - ' + model.projectName" />
                  <q-breadcrumbs-el :label="'Project Module - ' + model.projectModuleName" />
                  <q-breadcrumbs-el class="text-primary" :label="'Task - ' + model.taskName" />
                </q-breadcrumbs>
              </legend>
              <div class="row q-col-gutter-x-md q-mb-sm">
                <div class="col-12 col-sm-6 col-md-4 col-lg-4 col-xxl-4">
                  <formSingleSelectDropdown
                    v-model="model.assignedToId"
                    label="Activity Owner"
                    :readonly="isMyTaskActivity"
                    :options="projectCharterEmployeesWithWeeklyPlanHoursForDropdown.list.value"
                    :filter="projectCharterEmployeesWithWeeklyPlanHoursForDropdown.filter"
                    :error="v$.assignedToId.$error"
                    :error-message="v$.assignedToId.$errors[0]?.$message"
                  />
                </div>
                <div class="col-12 col-sm-6 col-md-4 col-lg-4 col-xxl-4">
                  <formSingleSelectDropdown
                    :readonly="isMyTaskActivity"
                    label="Activity Name"
                    v-model="model.name"
                    :options="projectTaskActivityNameForDropdownSingleSelect.list.value"
                    :filter="projectTaskActivityNameForDropdownSingleSelect.filter"
                    :error="v$.name.$error"
                    :error-message="v$.name.$errors[0]?.$message"
                  >
                    <template #option="{ itemProps, opt }">
                      <q-item v-bind="itemProps">
                        <q-item-section>
                          <div class="row q-col-gutter-x-md items-center">
                            <span>{{ opt.text }}</span>
                            <q-icon
                              v-if="opt.data"
                              name="o_info"
                              size="17px"
                              class="q-ml-xs"
                            >
                             <q-tooltip class="text-wrap break-words" max-width="300px">
                                <div v-html="opt.data" />
                              </q-tooltip>
                            </q-icon>
                          </div>
                        </q-item-section>
                      </q-item>
                    </template>
                  </formSingleSelectDropdown>
                </div>
                <div class="col-12 col-sm-6 col-md-2 col-lg-2 col-xxl-3">
                  <label class="label q-mb-xs text-black">Activity Status<span class="required">*</span></label>
                  <div>
                    <q-select
                      v-model="model.activityStatusId"
                      clearable
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="statusOptions"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      :error="v$.activityStatusId.$error"
                      :error-message="v$.activityStatusId.$errors[0]?.$message"
                      @blur="v$.activityStatusId.$touch"
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
                <div class="col-12 col-sm-6 col-md-2 col-lg-2 col-xxl-3">
                  <label class="label q-mb-xs text-black">Estimate Hours<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.estimateHours"
                      outlined
                      dense
                      :rules="[validateHours]"
                      hint="hh.mm"
                      maxlength="5"
                      :error="v$.estimateHours.$error"
                      :error-message="v$.estimateHours.$errors[0]?.$message"
                      @blur="v$.estimateHours.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-sm">
                <div class="col-xxl-12 col-lg-12 col-md-12 col-sm-12 col-xs-12">
                  <div class="form-group">
                    <label class="label q-mb-xs text-black">Activity Details<span v-if="isMyTaskActivity" class="required">*</span></label>
                    <q-editor
                      v-model="model.description"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                    <div
                      v-if="isMyTaskActivity && v$.description.$error"
                      class="text-negative text-caption q-mt-xs"
                    >
                      {{ v$.description.$errors[0]?.$message }}
                    </div>
                  </div>
                </div>
                <div class="col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12 hidden">
                  <div class="form-group Activity">
                    <label class="label q-mb-xs text-black">Project Task Activity Files</label>
                    <multiFileUploader
                      :initial-files="model.projectTaskActivityFiles"
                      :allowed-extensions="[
                        '.pdf','.xls','.xlsx','.doc','.docx','.jpeg','.jpg','.png','.ppt','.pptx'
                      ]"
                      :max-size-in-mb="25"
                      label="Drag files here or (+) to upload."
                      @files-selected="handleFiles"
                    />
                  </div>
                </div>
              </div>
              <div v-if="showRow" class="row q-col-gutter-x-md q-mb-lg hidden">
                <!-- Display Files in Square Boxes with File Name Below -->
                <div v-if="model.projectTaskActivityFiles && model.projectTaskActivityFiles.length > 0" class="row q-gutter-md">
                  <div
                    v-for="(file, index) in model.projectTaskActivityFiles"
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
                            {{ file.file?.name || file.name || extractFileName(file.file?.seoFilename) }}
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
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" :disable="processing" no-caps />
        </q-card-actions>
      </q-form>
      <!-- </q-card-section> -->
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted, toRaw, computed } from "vue";
import { notifySuccess, notifyError, notifyWarning } from "assets/utils";
import _ from "lodash";
import { format } from "date-fns"; // Standard TimeZone Conversion

import useFilters from "composables/useFilters";
import activityService from "modules/project-tasks-activities/projectTasksActivities.service";
import commonService from "services/common.service";

// SOP Change :- Shared Inputs
import multiFileUploader from "src/components/form-inputs/_multiFileUpload.vue";
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectTasksActivities from "src/modules/project-tasks-activities/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// ------------------------------------------------------------------------------------
// Common variables
// ------------------------------------------------------------------------------------
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const { toPrice } = useFilters();
const loading = ref(true);
const processing = ref(false);

// ------------------------------------------------------------------------------------
// Define emits
// ------------------------------------------------------------------------------------
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// ------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ------------------------------------------------------------------------------------
const props = defineProps({ id: { type: String, default: "" }, projectName: { type: String, default: "" }, moduleName: { type: String, default: "" }, projectIdAttr: { type: String, default: "" }, moduleIdAttr: { type: String, default: "" }, projectId: { type: String, default: "" }, moduleIdValue: { type: String, default: "" }, taskIdAttr: { type: String, default: "" }, taskName: { type: String, default: "" }, isMyTaskActicity: { type: Boolean, default: false } });
const isMyTaskActivity = props.isMyTaskActicity;
const showRow = props.id ? ref(true) : ref(false);

// ------------------------------------------------------------------------------------
// Define model values
// ------------------------------------------------------------------------------------
const model = ref({
  projectId: props.projectIdAttr !== "" ? props.projectIdAttr : "",
  projectModuleId: props.moduleIdAttr !== "" ? props.moduleIdAttr : "",
  taskId: props.taskIdAttr,
  projectName: props.projectName !== "" ? props.projectName : "",
  projectModuleName: props.moduleName !== "" ? props.moduleName : "",
  taskName: props.taskName,
  name: "",
  estimateHours: "",
  assignedToId: "",
  startDateStr: format(new Date(), "MM/dd/yyyy"),
  endDateStr: format(new Date(), "MM/dd/yyyy"),
  description: ""
});

// ------------------------------------------------------------------------------------
// Validation rules
// ------------------------------------------------------------------------------------
const descriptionRequired = helpers.withMessage(
  "Description is required",
  value => {
    if (!value) return false;
    const descriptionText = stripHtml(value);
    const containsImage = hasImage(value);
    return descriptionText || containsImage;
  }
);

const stripHtml = (html) => {
  if (!html) return "";
  return html.replace(/<[^>]*>/g, "").replace(/&nbsp;/g, " ").trim();
};

const hasImage = (html) => {
  if (!html) return false;
  return /<img\s+[^>]*src=/i.test(html);
};

const rules = computed(() => ({
  assignedToId: {
    required: helpers.withMessage("Activity owner is required", required)
  },

  activityStatusId: {
    required: helpers.withMessage("Activity status is required", required)
  },

  name: {
    required: helpers.withMessage("Activity name is required", required)
  },

  estimateHours: {
    required: helpers.withMessage("Estimate Hours is required", required),
    minValue: helpers.withMessage(
      "Invalid Estimate Hours",
      (value) => value >= 0
    )
  },

  description: isMyTaskActivity
    ? {
      descriptionRequired: helpers.withMessage(
        "Description is required",
        descriptionRequired
      )
    }
    : {}
}));

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

function validateHours (value) {
  // Ensure value is treated as a string
  const strValue = (value ?? "").toString().trim();
  const regex = /^(?:\d{1,2}(?:\.\d{1,2})?)$/;
  if (!strValue || (regex.test(strValue) && strValue.length <= 5)) {
    return true; // Valid input
  }
  return "Invalid hours format.";
}

// ------------------------------------------------------------------------------------
// Get Project Activity details
// ------------------------------------------------------------------------------------
const getProjectActivity = () => {
  loading.value = true;
  activityService.getProjectActivity(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.estimateHours = toPrice(resp.estimateHours);
    model.value.description = resp.description ? resp.description : "";
    model.value.startDateStr = resp.startDate ? format(resp.startDate, "MM/dd/yyyy") : "";
    model.value.endDateStr = resp.endDate ? format(resp.endDate, "MM/dd/yyyy") : "";
    model.value.projectTaskActivityFiles = resp.projectTaskActivityFilesList || [];
  }).finally(() => {
    loading.value = false;
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get All Dropdowns
// --------------------------------------------------------------------------------------------------------------------------------------------------
const { projectCharterEmployeesWithWeeklyPlanHoursForDropdown } = projectModule();

const {
  projectTaskActivityNameForDropdownSingleSelect
} = projectTasksActivities();

const activityStatusList = ref([]);
function getAllActivityStatusListForDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp
      .map((item) => ({ text: item.dropdownValue, value: item.id }));
    activityStatusList.value = responseData;
  });
}

// activity status options, disable "Open" if the activity description is empty
const statusOptions = computed(() => {
  return activityStatusList.value.map(option => {
    const plainText = model.value.description
      ? model.value.description.replace(/<[^>]*>/g, "").replace(/&nbsp;/gi, " ").trim()
      : "";

    if (option.text === "Open" && plainText === "") {
      return { ...option, disable: true };
    }
    // Disable "New" if current status is "Open"
    if (option.text === "New" && model.value.activityStatus?.dropDownValue === "Open") {
      return { ...option, disable: true };
    }
    if (option.text === "Close") {
      return { ...option, disable: true };
    }
    return { ...option, disable: false };
  });
});

function handleFiles (files) {
  model.value.projectTaskActivityFiles = files;
  model.value.projectFileFlag = "edit";
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
function removeFile (index) {
  const file = model.value.projectTaskActivityFiles[index];

  if (file && file.name && file.type) {
    URL.revokeObjectURL(file); // Clean up object URL
  }

  if (file && file.file && file.file.virtualPath) {
    // If it's an existing file, mark it as "remove" instead of deleting from array
    file.flag = "remove";
    model.value.projectTaskActivityFiles.splice(index, 1);
  } else {
    // For new files, just remove them from the list
    model.value.projectTaskActivityFiles.splice(index, 1);
  }

  if (model.value.projectTaskActivityFiles.length === 0) {
    // model.value.projectFileFlag = "remove";
  }
}

// -------------------------------------------------------------------------------------------------------
// Submit form
// -------------------------------------------------------------------------------------------------------
const onSubmit = async () => {
  try {
    if (processing.value) {
      notifyWarning({ message: "Double click not allowed. Please wait..." });
      return; // stop further submit
    }
    const formData = new FormData();
    const selectedOption = projectTaskActivityNameForDropdownSingleSelect.getLabelByValue(model.value.name);
    model.value.name = selectedOption ?? model.value.name;
    const hoursValidation = validateHours(model.value.hours);
    if (hoursValidation !== true) {
      notifyError({ message: hoursValidation }); // Display error message
      return; // Prevent save operation
    }
    processing.value = true;
    if (await v$.value.$validate()) {
      // Append other fields
      formData.append("name", selectedOption ? selectedOption : model.value.name);
      formData.append("projectId", model.value.projectId);
      formData.append("projectModuleId", model.value.projectModuleId);
      formData.append("taskId", model.value.taskId);
      formData.append("startDateStr", model.value.startDateStr);
      formData.append("endDateStr", model.value.endDateStr);
      formData.append("assignedToId", model.value.assignedToId);
      formData.append("activityStatusId", model.value.activityStatusId);
      formData.append("estimateHours", model.value.estimateHours);
      formData.append("description", model.value.description);

      toRaw(model.value.projectTaskActivityFiles || []).forEach((file) => {
        if (file.file && file.file.virtualPath) {
        // For existing files, append metadata instead of the file itself
          formData.append("ExistingFiles", JSON.stringify({
            id: file.id,
            virtualPath: file.file.virtualPath
          // flag: file.flag || "new"
          }));
        } else {
        // For new files, append as raw file objects (IFormFile)
          formData.append("projectTaskActivityFiles", file);
        }
      });
      activityService.saveProjectActivity(props.id, formData).then((resp) => {
        notifySuccess({ message: "Project activity is saved successfully." });
        onDialogOK();
      });
    }
  } catch (error) {
    console.error("Error in submitting the project activity:", error);
    notifyError({ message: "An error occurred while saving the project activity" });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------
onMounted(() => {
  projectTaskActivityNameForDropdownSingleSelect.load("Project Activities");
  getAllActivityStatusListForDropDown("Activity Status");
});

watch(
  () => [props.projectIdAttr, props.projectId],
  async ([projectIdAttr, projectId]) => {
    const selectedProjectId =
      projectIdAttr && projectIdAttr !== ""
        ? projectIdAttr
        : projectId;

    if (selectedProjectId) {
      projectCharterEmployeesWithWeeklyPlanHoursForDropdown.load(selectedProjectId);
    }
  },
  { immediate: true }
);

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getProjectActivity();
  }
}, { immediate: true });

watch(
  () => model.value.description,
  (val) => {
    const text = val?.replace(/<[^>]*>/g, "").replace(/&nbsp;/gi, " ").trim() || "";
    // Find selected status object
    const selectedStatus = activityStatusList.value.find(
      s => s.value === model.value.activityStatusId
    );
    if (!text && selectedStatus?.text === "Open") {
      model.value.activityStatusId = null;
    }
  }
);
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_tasks_activity .q-select__dropdown-icon{
  display: none;
}
.basic_info .q-select__dropdown-icon{
  display: none;
}
.Activity .q-uploader {
  width: auto;
}
</style>
