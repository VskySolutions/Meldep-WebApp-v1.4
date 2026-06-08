<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1000px; height: 100% !important;max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ helpDeskTitle }} Files</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="">
            <fieldset>
              <legend>Add Files</legend>
              <div class="row q-col-gutter-x-md" @paste="handlePaste">
                <div class="col-12 q-mb-xs text-black">Ticket Files</div>

                <!-- File Uploader -->
                <div class="col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12">
                  <div class="form-group">
                    <q-uploader
                      ref="documentUploaderRef"
                      v-model="model.helpDeskFiles"
                      class="prodUploader"
                      color="white"
                      text-color="dark"
                      with-credentials
                      hide-upload-btn
                      multiple
                      field-name="HelpDeskFiles"
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
                <div v-if="model.helpDeskFiles && model.helpDeskFiles.length > 0" class="row q-gutter-md">
                  <div
                    v-for="(file, index) in model.helpDeskFiles"
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
                            {{ file.file?.name || file.name || extractFileName(file.file?.virtualPath) }}
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
            <q-card-actions align="center" class="q-gutter-sm justify-center">
              <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
              <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
            </q-card-actions>
            <fieldset class="q-mb-lg">
              <legend>Ticket Files</legend>
              <div class="q-mb-sm q-gutter-sm flex justify-end">
                <q-input v-model="filter" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search" dense clearable>
                  <template #prepend>
                    <q-icon name="o_search" />
                  </template>
                </q-input>
              </div>
              <q-table
                ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="rows" :columns="columns" row-key="id" :filter="filter" separator="cell"
                binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]"
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''" :set="(preSourceName = null,preSubModuleName = null)">
                    <q-td>{{ extractFileName(props.row.virtualPath) }}</q-td>
                    <q-td>{{ props.row.createdBy.person.fullName }}</q-td>
                    <q-td>{{ props.row.createdOnUtc }}</q-td>
                    <q-td style="width: 5%;" class="text-center actions">
                      <q-btn icon="o_visibility" size="sm" class="q-pr-xs" flat @click="viewFile(props.row.virtualPath)" />
                      <!-- <q-btn icon="o_download" size="sm" class="q-pl-sm" flat :href="baseURL + props.row.virtualPath" :download="props.row.virtualPath" /> -->
                      <q-btn icon="o_download" size="sm" class="q-pl-xs q-pr-xs" flat @click="downloadFile(props.row.virtualPath)" />
                      <q-btn icon="o_delete_outline" color="negative" size="sm" class="q-pl-xs text-negative" flat @click="onDelete(props.row)" />
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
import helpDeskService from "modules/helpdesk/helpDesk.service";

// Common variables
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const rows = ref([]);
const filter = ref("");
const processing = ref(false);
const loading = ref(true);
const activeRowId = ref(null);
const columns = ref([
  { name: "virtualPath", label: "File Name", field: "virtualPath", align: "left" },
  { name: "createdBy.person.fullName", label: "Uploaded By", field: "createdByPersonFullName", align: "left", sortable: false },
  { name: "createdOnUtc", label: "Uploaded Date", field: "createdOnUtc", align: "left" }
]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, name: { type: String, default: "" } });
const helpDeskTitle = props.name;
// Define model values
const model = ref({
  id: "",
  helpDeskFiles: []
});

// get project details
const getAllFilesByHelpDeskId = (propss) => {
  const projectId = props.id;
  loading.value = true;
  const { page, rowsPerPage, sortBy, descending } = propss.pagination;
  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    projectId
  };
  projectService.getAllFilesByProjectId(payload).then((resp) => {
    rows.value = resp.data.map(item => ({
      ...item,
      createdByPersonFullName: item.createdBy?.person?.fullName
    }));
  }).finally(() => {
    loading.value = false;
  });
};
function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
}

function viewFile (file) {
  let fileUrl = "";
  let fileName = "";
  // let mimeType = "";

  if (typeof file === "string") {
    fileUrl = file;
    fileName = file.split("/").pop();
    // isServerFile = true;
  } else if (file?.file?.virtualPath) {
    fileUrl = file.file.virtualPath;
    fileName = file.file.name || file.file.virtualPath.split("/").pop();
    // isServerFile = true;
  } else if (file instanceof File || file?.name) {
    const rawFile = file.file ?? file;
    fileUrl = URL.createObjectURL(rawFile);
    fileName = rawFile.name;
  } else {
    return;
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
<title>${fileName}</title>
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

function downloadFile (file) {
  const link = document.createElement("a");
  link.href = file;
  link.download = file.split("/").pop();
  link.click();
}
const onFileRemoved = (removedFiles) => {
  removedFiles.forEach((removedFile) => {
    model.value.helpDeskFiles = model.value.helpDeskFiles.filter(
      file =>
        file.name !== removedFile.name &&
        file.file?.name !== removedFile.name
    );
  });
};

// ----------------------------------------------
const documentUploaderRef = ref(null);
const allowedExtensions = [".pdf", ".xls", ".xlsx", ".doc", ".docx", ".jpeg", ".jpg", ".png", ".gif", ".ppt", ".pptx", ".txt"];
const allowedFileTypes = [
  "application/pdf", // PDF
  "application/vnd.ms-excel", // Excel (old format)
  "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // Excel (new format)
  "application/msword", // Word (old format)
  "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // Word (new format)
  "image/jpeg", "image/png", "image/gif", // Images
  "application/vnd.ms-powerpoint", // PowerPoint (old format)
  "application/vnd.openxmlformats-officedocument.presentationml.presentation", // PowerPoint (new format)
  "text/plain" // TXT
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

const onFileAdded = (files) => {
  if (!files || files.length === 0) return;

  if (!model.value.helpDeskFiles) {
    model.value.helpDeskFiles = [];
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

  model.value.helpDeskFiles.push(...validFiles);
  model.value.helpDeskFileFlag = "edit"; // Set the overall flag for tracking
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
  const file = model.value.helpDeskFiles[index];
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

  model.value.helpDeskFiles.splice(index, 1);

  if (model.value.helpDeskFiles.length === 0) {
    model.value.helpDeskFlag = "remove";
  }
}

// -------------------------------------------------------------------------------------------------------

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.virtualPath.split("/").pop()}` }, () => {
    projectService.deleteFile(item.id, item.type).then(resp => {
      notifySuccess({ message: "File is deleted successfully." });
      getAllFilesByHelpDeskId({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    const formData = new FormData();
    const files = toRaw(model.value.helpDeskFiles || []);

    if (!files.length) {
      notifyError({ message: "Please select at least one file." });
      return;
    }

    // Append other fields
    toRaw(model.value.helpDeskFiles || []).forEach((file) => {
      if (file.file && file.file.virtualPath) {
      // For existing files, append metadata instead of the file itself
        formData.append("ExistingFiles", JSON.stringify({
          id: file.id,
          virtualPath: file.file.virtualPath
        // flag: file.flag || "new"
        }));
      } else {
      // For new files, append as raw file objects (IFormFile)
        formData.append("HelpDeskFiles", file);
      }
    });

    formData.append("id", props.id);
    helpDeskService.saveHelpDeskFiles(formData).then((resp) => {
      notifySuccess({ message: "Files are saved successfully." });
      getAllFilesByHelpDeskId({ pagination: pagination.value });
      // Reset the uploader input
      if (documentUploaderRef.value) {
        documentUploaderRef.value.reset();
      }
      // Also clear the model if needed
      model.value.helpDeskFiles = [];
    });
  } catch (error) {
    console.error("Error while saving ticket files:", error);
    notifyError({ message: "An error occurred while saving ticket files." });
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
  getAllFilesByHelpDeskId(propps);
});
</script>
