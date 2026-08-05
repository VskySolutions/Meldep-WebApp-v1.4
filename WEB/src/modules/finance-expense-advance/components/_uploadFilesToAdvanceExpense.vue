<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic"
      style="width: 1000px; height: 100% !important;max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ itemCategoryType }} Files</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="">
            <fieldset>
              <legend>Add Files</legend>
              <div class="row q-col-gutter-x-md">
                <div class="col-12 q-mb-xs text-black">Advance Expense Files</div>

                <!-- File Uploader -->
                <div class="col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12">
                  <div class="form-group">
                    <q-uploader
                      ref="documentUploaderRef"
                      v-model="model.expenseAdvanceRequestFiles"
                      class="prodUploader"
                      color="white"
                      text-color="dark"
                      with-credentials
                      hide-upload-btn
                      multiple
                      field-name="ExpenseAdvanceRequestFiles"
                      flat
                      bordered
                      label="Drag files here or (+) to upload."
                      @added="onFileAdded"
                      @removed="onFileRemoved"
                    />
                    <div class="text-grey-7 text-caption q-mt-xs">
                      <i>Allowed Files: jpg, png, jpeg, pdf, excel, doc, ppt</i>
                    </div>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-lg">
                <!-- Display Files in Square Boxes with File Name Below -->
                <div v-if="model.expenseAdvanceRequestFiles && model.expenseAdvanceRequestFiles.length > 0" class="row q-gutter-md">
                  <div v-for="(file, index) in model.expenseAdvanceRequestFiles" :key="index"
                    class="col-3 position-relative file-card text-center" style="max-width: 140px; min-width: 140px;">
                    <div class="file-preview square-box">
                      <template v-if="isImageFile(file)">
                        <img :src="file.file.virtualPath ? file.file.virtualPath : getFilePreview(file.file)"
                          alt="File Preview" class="square-content centered-image">
                      </template>
                      <template v-else>
                        <q-icon :name="getFileIcon(file.file?.mimeType)" class="file-icon square-content" size="70px" />
                      </template>
                      <div class="file-name q-mt-sm">
                        <q-btn v-if="file.file?.virtualPath || file?.name" class="bg-primary text-white q-pa-xs" no-caps
                          @click="viewFile(file)">
                          <span class="truncate-text">
                            {{ file.file?.name || file.name || extractFileName(file.file?.virtualPath) }}
                          </span>
                        </q-btn>
                      </div>
                    </div>
                    <!-- Remove Button -->
                    <q-btn color="negative" flat round dense icon="o_close" class="remove-file-icon"
                      @click="removeFile(index)" />
                  </div>
                </div>
              </div>
            </fieldset>
            <q-card-actions align="center" class="q-gutter-sm justify-center">
              <q-btn
                color="grey-4"
                push
                outline
                label="Close"
                type="button"
                class="text-grey-9 actionBtn"
                no-caps
                @click="onDialogCancel"
              />
              <q-btn
                color="primary"
                push
                outline
                label="Save"
                type="submit"
                class="actionBtn"
                :loading="processing"
                no-caps
              />
            </q-card-actions>
            <fieldset class="q-mb-lg">
              <legend>Advance Expense Files</legend>
              <div class="q-mb-sm q-gutter-sm flex justify-end">
                <q-input
                  v-model="filter"
                  outlined
                  class="bg-white q-mr-sm search-box"
                  debounce="300"
                  placeholder="Search"
                  dense
                  clearable
                >
                  <template #prepend>
                    <q-icon name="o_search" />
                  </template>
                </q-input>
              </div>
              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                bordered
                class="no-shadow"
                :loading="loading"
                :rows="rows"
                :columns="columns"
                row-key="id"
                :filter="filter"
                separator="cell"
                binary-state-sort
                :rows-per-page-options="[20, 50, 100, 200, 500]"
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th v-for="col in props.cols" :key="col.name" :props="props">
                      {{ col.label }}
                    </q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''" :set="(preSourceName = null, preSubModuleName = null)">
                    <q-td>
                      <span v-if="preSourceName !== props.row.type" :set="preSourceName = props.row.type" class="common-q-td">
                        {{ props.row.type }}
                      </span>
                    </q-td>
                    <q-td>
                      <span v-if="preSubModuleName !== props.row.sub_Module" :set="preSubModuleName = props.row.sub_Module" class="common-q-td">
                        {{ props.row.sub_Module }}
                      </span>
                    </q-td>
                    <q-td>
                      {{ extractFileName(props.row.seoFilename) }}
                    </q-td>
                    <q-td>
                      {{ props.row.createdBy.person.fullName }}
                    </q-td>
                    <q-td>
                      {{ props.row.createdOnUtc }}
                    </q-td>
                    <q-td class="text-center actions">
                      <q-btn
                        icon="o_visibility"
                        size="sm"
                        class="q-pr-xs"
                        flat
                        @click="viewFile(props.row.virtualPath)"
                      />
                      <q-btn
                        icon="o_download"
                        size="sm"
                        class="q-pl-xs q-pr-xs"
                        flat
                        @click="downloadFile(props.row.virtualPath)"
                      />
                      <q-btn
                        icon="o_delete_outline"
                        color="negative"
                        size="sm"
                        class="q-pl-xs text-negative"
                        flat
                        @click="onDelete(props.row)"
                      />
                    </q-td>
                  </q-tr>
                </template>
              </q-table>
            </fieldset>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted, toRaw } from "vue";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";

import projectService from "modules/project/projects.service";
import advanceExpensesService from "../financeExpenseAdvance.service";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, name: { type: String, default: "" } });
const itemCategoryType = props.name;

// Common variables
const filter = ref("");
const processing = ref(false);
const loading = ref(true);
const activeRowId = ref(null);
const rows = ref([]);

const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "type", label: "Source", field: "type", align: "left" },
  { name: "sub_Module", label: "Source Name", field: "sub_Module", align: "left" },
  { name: "virtualPath", label: "File Name", field: row => extractFileName(row.seoFilename), align: "left" },
  { name: "createdBy.person.fullName", label: "Created By", field: "createdByPersonFullName", align: "left", sortable: false },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left" }
]);

// Define model values
const model = ref({
  id: "",
  expenseAdvanceRequestFiles: [],
  expenseAdvanceRequestFileFlag: "edit"
});

// get expense details
const getAllFilesByExpenseId = (propss) => {
  const expenseId = props.id;
  loading.value = true;
  const { page, rowsPerPage, sortBy, descending } = propss.pagination;
  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    expenseId
  };
  advanceExpensesService.getAllFilesByExpenseId(payload).then((resp) => {
    rows.value = resp.data.map(item => ({
      ...item,
      createdByPersonFullName: item.createdBy?.person?.fullName
    }));
  }).finally(() => {
    loading.value = false;
  });
};

function extractFileName(path) {
  return path ? path.split("/").pop() : "Unknown File";
}

function viewFile(file) {
  const fileUrl = new URL(file).href;
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
<title>${file.split("/").pop()}</title>
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
            ${imageFormats.includes(fileExtension)
        ? `<img src="${fileUrl}" alt="Image Preview">` // Show image directly
        : `<iframe src="${viewerUrl}"></iframe>` // Show document using iframe
      }
</body>
</html>
        `);
  }, 100);
}

function downloadFile(file) {
  const link = document.createElement("a");
  link.href = file;
  link.download = file.split("/").pop();
  link.click();
}

// ----------------------------------------------
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

const onFileAdded = (files) => {
  if (!files || files.length === 0) return;

  if (!model.value.expenseAdvanceRequestFiles) {
    model.value.expenseAdvanceRequestFiles = [];
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

  model.value.expenseAdvanceRequestFiles.push(...validFiles);
  model.value.expenseAdvanceRequestFileFlag = "edit"; // Set the overall flag for tracking
};

function onFileRemoved(files) {
  files.forEach(file => {
    const index = model.value.expenseAdvanceRequestFiles.findIndex(f =>
      f.name === file.name &&
      f.size === file.size &&
      f.lastModified === file.lastModified
    );

    if (index !== -1) {
      model.value.expenseAdvanceRequestFiles.splice(index, 1);
    }
  });

  if (model.value.expenseAdvanceRequestFiles.length === 0) {
    model.value.expenseAdvanceRequestFileFlag = "remove";
  }
}

function getFilePreview(file) {
  return file && file instanceof File ? URL.createObjectURL(file) : "";
}

function isImageFile(file) {
  if (file.file instanceof File) {
    return file.file.type.startsWith("image/");
  } else if (file.file && file.file.mimeType) {
    return file.file.mimeType.startsWith("image/");
  }
  return false;
}

function getFileIcon(mimeType) {
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

function removeFile(index) {
  const file = model.value.expenseAdvanceRequestFiles[index];

  if (file && file.name && file.type) {
    URL.revokeObjectURL(file); // Clean up object URL
  }

  if (file && file.file && file.file.virtualPath) {
    // If it's an existing file, mark it as "remove" instead of deleting from array
    file.flag = "remove";
    model.value.expenseAdvanceRequestFiles.splice(index, 1);
  } else {
    // Remove from q-uploader
    documentUploaderRef.value?.removeFile(file);
    // For new files, just remove them from the list
    model.value.expenseAdvanceRequestFiles.splice(index, 1);
  }

  if (model.value.expenseAdvanceRequestFiles.length === 0) {
    model.value.expenseAdvanceRequestFileFlag = "remove";
  }
}

// -------------------------------------------------------------------------------------------------------

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.virtualPath.split("/").pop()}` }, () => {
    projectService.deleteFile(item.id, item.type).then(resp => {
      notifySuccess({ message: "File is deleted successfully." });
      getAllFilesByExpenseId({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};

// Submit form
const onSubmit = async () => {
  processing.value = true;

  // Validate file selection
  if (!model.value.expenseAdvanceRequestFiles || model.value.expenseAdvanceRequestFiles.length === 0) {
    notifyError({ message: "Please select at least one file." });
    processing.value = false;
    return;
  }

  try {
    const formData = new FormData();
    // Append other fields
    toRaw(model.value.expenseAdvanceRequestFiles || []).forEach((file) => {
      if (file.file && file.file.virtualPath) {
        // For existing files, append metadata instead of the file itself
        formData.append("ExistingFiles", JSON.stringify({
          id: file.id,
          virtualPath: file.file.virtualPath
        }));
      } else {
        // For new files, append as raw file objects (IFormFile)
        formData.append("ExpenseAdvanceRequestFiles", file);
      }
    });

    // Also pass the expenseAdvanceRequestFileFlag for general status tracking
    formData.append("expenseAdvanceRequestFileFlag", model.value.expenseAdvanceRequestFileFlag || "no_change");
    formData.append("id", props.id);
    advanceExpensesService.saveAdvanceExpenseRequestFiles(formData).then((resp) => {
      notifySuccess({ message: "Files are saved successfully." });
      getAllFilesByExpenseId({ pagination: pagination.value });
      // Reset the uploader input
      if (documentUploaderRef.value) {
        documentUploaderRef.value.reset();
      }
      // Also clear the model if needed
      model.value.expenseAdvanceRequestFiles = [];
    });
  } catch (error) {
    console.error("Error while saving project files:", error);
    notifyError({ message: "An error occurred while saving project files." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

// On page rendering
onMounted(() => {
  const propps = { pagination: pagination.value };
  getAllFilesByExpenseId(propps);
});
</script>
