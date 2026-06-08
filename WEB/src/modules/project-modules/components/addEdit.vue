<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Project Module</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable', readonlyProject != '' ? 'edit_projectModule' : '']">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Module Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <formSingleSelectDropdown
                  v-model="model.projectId"
                  label="Project Name"
                  :readonly="!!readonlyProject"
                  :class="readonlyProject !== '' ? 'edit_tasks' : ''"
                  :options="projectNameDropdownSingleSelect.list.value"
                  :filter="projectNameDropdownSingleSelect.filter"
                  @update:model-value="getSortOrderByProjectId(model.projectId)"
                  :wrapperClass="'col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12'"
                  :error="v$.projectId.$error"
                  :error-message="v$.projectId.$errors[0]?.$message"
                >
                  <template #after>
                    <q-icon
                      v-if="!readonlyProject && props.id == ''"
                      name="o_add"
                      color="primary"
                      class="cursor-pointer q-ml-xs add-icon"
                      @click="onProjectAdd(null, refreshProjectDropdownList)"
                    >
                      <q-tooltip>Add new Project</q-tooltip>
                    </q-icon>
                  </template>
                </formSingleSelectDropdown>
                <div class="col-12 col-md-6">
                  <label class="label q-mb-xs text-black">Project Module<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.name" outlined stack-label hide-bottom-space :dense="true"
                      :error="v$.name.$error" :error-message="v$.name.$errors[0]?.$message" @blur="v$.name.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <formDate
                  v-model="model.startDateStr"
                  label="Start Date"
                  :error="v$.startDateStr.$error"
                  :error-message="v$.startDateStr.$errors[0]?.$message"
                  :onBlur="() => v$.startDateStr.$touch()"
                  :dateOptions="optionsFn"
                />
                <formDate
                  v-model="model.endDateStr"
                  label="Due Date"
                  :error="v$.endDateStr.$error"
                  :error-message="v$.endDateStr.$errors[0]?.$message"
                  :onBlur="() => v$.endDateStr.$touch()"
                  :dateOptions="disableBeforeStartDate"
                />
                <formSingleSelectDropdown
                  v-model="model.projectModuleStatusId"
                  label="Project Module Status"
                  :wrapperClass="'col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12'"
                  :options="projectModuleStatusForDropdownSingleSelect.list.value"
                  :filter="projectModuleStatusForDropdownSingleSelect.filter"
                  :error="v$.projectModuleStatusId.$error"
                  :error-message="v$.projectModuleStatusId.$errors[0]?.$message"
                />
                <div class="col-12 col-sm-3 col-md-3 col-lg-3">
                  <div class="q-mb-xs text-black">Sort Order<span class="required">*</span></div>
                  <div>
                    <q-input
                      v-model="model.sortOrder"
                      :placeholder="nestSortOrderByLastSortOrder"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      maxlength="5"
                      :rules="[val => validateSortOrder(val) || 'Enter a valid sort order']"
                      :error="v$.sortOrder.$error"
                      :error-message="v$.sortOrder.$errors[0]?.$message"
                      @blur="v$.sortOrder.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="form-group">
                    <label class="label q-mb-xs text-black">Module Description</label>
                    <q-editor
                      v-model="model.description"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                      class="relative-position"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-12 col-md-12 col-lg-12">
                  <div>
                    <div class="q-mb-xs text-black">Notes</div>
                    <div class="form-group">
                      <q-input v-model="model.notes" outlined autogrow hint="The maximum length allowed is 500." maxlength="500" />
                    </div>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-lg">
                <div class="col-12 q-mb-xs text-black">Project Module Files</div>
                <div class="col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12">
                  <div class="form-group">
                    <multiFileUploader
                      :initialFiles="model.projectModuleFiles"
                      :allowedExtensions="[
                        '.pdf','.xls','.xlsx','.doc','.docx','.jpeg','.jpg','.png','.ppt','.pptx'
                      ]"
                      :maxSizeInMb="25"
                      label="Drag files here or (+) to upload."
                      @files-selected="handleFiles"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-lg">
                <div v-if="model.projectModuleFiles && model.projectModuleFiles.length > 0" class="row q-gutter-md">
                  <div
                    v-for="(file, index) in model.projectModuleFiles"
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
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
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
import _ from "lodash";
import { isDate } from "validators/zw_validators.js";
import useFilters from "composables/useFilters";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, watch, onMounted, toRaw } from "vue";
import { notifySuccess, getLocalStorage, notifyError, zwConfirm, notifyWarning } from "assets/utils";
import { parse } from "date-fns"; // Standard TimeZone Conversion

import projectModuleService from "modules/project-modules/projectModules.service";
import projectService from "modules/project/projects.service";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectAdd
} from "src/modules/project/utils/dialogs.js";

// Shared Inputs
import multiFileUploader from "src/components/form-inputs/_multiFileUpload.vue";

// Common variables
const { toDate } = useFilters();
const loading = ref(true);
const processing = ref(false);
let oldStatus = null;
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);

const today = new Date();
today.setHours(0, 0, 0, 0);
const optionsFn = (date) => {
  const dateObj = new Date(date);
  return dateObj >= today;
};

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" },
  projectName: { type: String, default: "" },
  projectIdAttr: { type: String, default: "" },
  projectIdValue: { type: String, default: "" },
  startDate: { type: String, default: "" },
  endDate: { type: String, default: "" }
});

const readonlyProject = props.projectIdAttr ? "readonly" : "";

// local storage values
const localStorageKey = "Project Modules";
const filterLocalStorage = getLocalStorage(localStorageKey);
const projectIds = filterLocalStorage ? filterLocalStorage.projectIds[0] : [];

// Define model values
const model = ref({
  projectId: props.projectIdAttr !== "" ? props.projectIdAttr : (props.projectIdValue !== "" ? props.projectIdValue : (projectIds !== "" ? projectIds : null)),
  name: "",
  startDateStr: props.startDate ? props.startDate : "",
  endDateStr: props.endDate ? props.endDate : "",
  description: "",
  active: true,
  notes: ""
});

let nestSortOrderByLastSortOrder = 1;
// get project details on edit mode
const getNextSortOrderOfProjectModuleAndTask = (projectId) => {
  loading.value = true;
  projectModuleService.getNextSortOrderOfProjectModuleAndTask(projectId).then((resp) => {
    nestSortOrderByLastSortOrder = resp.nextSortOrderOfProjectModule;
    model.value.sortOrder = resp.nextSortOrderOfProjectModule;
  }).finally(() => {
    loading.value = false;
  });
};

// get project details on edit mode
const getProjectModule = () => {
  loading.value = true;
  projectModuleService.getProjectModuleDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    oldStatus = model.value.projectModuleStatus.dropDownValue;
    model.value.description = resp.description ? resp.description : "";
    model.value.notes = resp.notes ? resp.notes : "";
    model.value.startDateStr = resp.startDate ? toDate(resp.startDate) : "";
    model.value.endDateStr = resp.endDate ? toDate(resp.endDate) : "";
    model.value.projectModuleFiles = resp.projectModuleFilesList || [];
  }).finally(() => {
    loading.value = false;
  });
};

// let projectTemplateDisable = false;
const getProject = (projectId) => {
  loading.value = true;
  projectService.getProject(projectId).then((resp) => {
    const projectStart = resp.startDate ? toDate(resp.startDate) : null;
    const projectEnd = resp.goLiveDate ? toDate(resp.goLiveDate) : null;

    const propStart = props.startDate ? toDate(props.startDate) : null;
    const propEnd = props.endDate ? toDate(props.endDate) : null;

    const isBetween = (date, start, end) => {
      return date && start && end && date >= start && date <= end;
    };

    if (!projectEnd && propStart && propEnd && propStart >= projectStart && propEnd >= projectStart) {
      model.value.startDateStr = propStart;
      model.value.endDateStr = propEnd;
    } else {
      model.value.startDateStr = isBetween(propStart, projectStart, projectEnd) ? propStart : projectStart || "";
      model.value.endDateStr = isBetween(propEnd, projectStart, projectEnd) ? propEnd : projectEnd || "";
    }
  }).finally(() => {
    loading.value = false;
  });
};

// Validation rules
const rules = {
  projectId: { required: helpers.withMessage("Project is required", required) },
  projectModuleStatusId: { required: helpers.withMessage("Project module status is required", required) },
  name: { required: helpers.withMessage("Module name is required", required), minLength: minLength(1), maxLength: maxLength(100) },
  sortOrder: { required: helpers.withMessage("Sort Order is required", required) },
  startDateStr: {
    required: helpers.withMessage("Start date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  endDateStr: {
    required: helpers.withMessage("End date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate),
    afterStartDate: helpers.withMessage("End date must occur after the start date", (value, startDate) => {
      return new Date(value) > new Date(startDate.startDateStr);
    })
  }
};
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

function validateSortOrder (value) {
  if (typeof value !== "string" && typeof value !== "number") return false;
  const str = String(value).trim();

  // Must be digits only
  if (!/^\d+$/.test(str)) {
    return false;
  }

  // Convert to number and check > 0
  return Number(str) > 0;
}

const disableBeforeStartDate = (startDate) => {
  if (!model.value.startDateStr) {
    return true;
  }

  // Convert MM/dd/yyyy string to Date
  const start = parse(model.value.startDateStr, "MM/dd/yyyy", new Date());
  const currentDate = parse(startDate, "yyyy/MM/dd", new Date());

  return currentDate >= start;
};

function getSortOrderByProjectId (projectId) {
  model.value.sortOrder = "";
  getNextSortOrderOfProjectModuleAndTask(projectId);
}

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initProjectDialogs(props.id);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const refreshProjectDropdownList = () => {
  projectNameDropdownSingleSelect.load();
};

const { projectModuleStatusForDropdownSingleSelect } = projectModuleOfProjectModule();

// -------------------------------------------------------------------------------------------------------
// Upload Image
// -------------------------------------------------------------------------------------------------------
function handleFiles (files) {
  model.value.projectModuleFiles = files;
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
  const file = model.value.projectModuleFiles[index];

  if (file && file.name && file.type) {
    URL.revokeObjectURL(file); // Clean up object URL
  }

  if (file && file.file && file.file.virtualPath) {
    // If it's an existing file, mark it as "remove" instead of deleting from array
    file.flag = "remove";
    model.value.projectModuleFiles.splice(index, 1);
  } else {
    // For new files, just remove them from the list
    model.value.projectModuleFiles.splice(index, 1);
  }

  if (model.value.projectModuleFiles.length === 0) {
    // model.value.projectFileFlag = "remove";
  }
}
// -------------------------------------------------------------------------------------------------------

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { projectNameDropdownSingleSelect
} = projectModule();


// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    processing.value = true;
    const formData = new FormData();
    if (await v$.value.$validate()) {
      processing.value = true;

      // Status confirmation
      if (props.id) {
        const selected = projectModuleStatusForDropdownSingleSelect.list.value.find(item => item.value === model.value.projectModuleStatusId);
        if (oldStatus.toLowerCase() !== "close" && selected?.text.toLowerCase() === "close") {
          const resp = await projectModuleService.checkModuleCanBeDeleted(props.id);
          const canClose = resp?.canDelete;
          if (!canClose) {
            // Warning confirmation
            zwConfirm({
              title: "Active Tasks or Activities Found",
              message: "This module has active tasks or activities. You cannot close it.",
              okLabel: "OK",
              cancel: false
            }, () => {
            });
            return;
          }
        }
      }

      // Append other fields
      formData.append("projectId", model.value.projectId);
      formData.append("name", model.value.name);
      formData.append("startDateStr", model.value.startDateStr);
      formData.append("endDateStr", model.value.endDateStr);
      formData.append("sortOrder", model.value.sortOrder);
      formData.append("projectModuleStatusId", model.value.projectModuleStatusId);
      formData.append("description", model.value.description);
      formData.append("notes", model.value.notes);

      toRaw(model.value.projectModuleFiles || []).forEach((file) => {
        if (file.file && file.file.virtualPath) {
        // For existing files, append metadata instead of the file itself
          formData.append("ExistingFiles", JSON.stringify({
            id: file.id,
            virtualPath: file.file.virtualPath
          // flag: file.flag || "new"
          }));
        } else {
        // For new files, append as raw file objects (IFormFile)
          formData.append("projectModuleFiles", file);
        }
      });

      // Also pass the projectFileFlag for general status tracking
      // formData.append("projectFileFlag", model.value.projectFileFlag || "no_change");
      // Check the FormData content (for debugging)
      // for (const [key, value] of formData.entries()) {
      //   console.log(key, value);
      // }
      // projectModuleService.displayWarningForSortOrder(model.value.projectId, model.value.sortOrder, props.id).then((resp) => {
      //   if (resp.warning) { notifyWarning({ message: resp.warning }); }
      // }).catch(() => {
      // });
      projectModuleService.saveProjectModule(props.id, formData).then((resp) => {
        if (resp.warning) { notifyWarning({ message: resp.warning }); }
        notifySuccess({ message: resp.message });
        onDialogOK();
      });
    }
  } catch (error) {
    console.error("Error while saving project module:", error);
    notifyError({ message: "An error occurred while saving the project module." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getProjectModule();
  }
}, { immediate: true });

watch(() => model.value.projectId, (newValue, oldValue) => {
  if (newValue) {
    if (!props.id && model.value.projectId) {
      getNextSortOrderOfProjectModuleAndTask(newValue);
      getProject(model.value.projectId);
    }
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  projectNameDropdownSingleSelect.load();
  projectModuleStatusForDropdownSingleSelect.load("WO Status");
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
</style>
