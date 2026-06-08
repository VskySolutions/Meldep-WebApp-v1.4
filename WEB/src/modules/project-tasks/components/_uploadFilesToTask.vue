<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1000px; height: 100% !important;max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ taskName }} Files</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="">
            <fieldset>
              <legend>Add Files</legend>
              <div class="row q-col-gutter-x-md">
                <div class="col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-6">
                  <div class="q-mb-xs">Project Name:</div>
                  <div class="text-black">
                    {{ projectName }}
                  </div>
                </div>
                <div class="col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-6">
                  <div class="q-mb-xs">Project Module Name:</div>
                  <div class="text-black">
                    {{ moduleName }}
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mt-sm">
                <div class="col-12 q-mb-xs">Upload Files</div>
                <!-- File Uploader -->
                <div class="col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12">
                  <div class="form-group">
                    <q-uploader
                      ref="documentUploaderRef" class="prodUploader" color="white" text-color="dark" with-credentials hide-upload-btn multiple
                      field-name="ProjectTaskFiles" flat bordered label="Drag files here or (+) to upload." @added="onFileAdded"
                    />
                    <div class="text-grey-7 text-caption q-mt-xs">
                      <i>Allowed Files: jpg, png, jpeg, pdf, excel, doc, ppt</i>
                    </div>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-lg">
                <!-- Display Files in Square Boxes with File Name Below -->
                <div v-if="model.projectTaskFiles && model.projectTaskFiles.length > 0" class="row q-gutter-md">
                  <div v-for="(file, index) in model.projectTaskFiles" :key="index" class="col-3 position-relative file-card text-center" style="max-width: 140px; min-width: 140px;">
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
                            {{ file.file?.name || file.name || extractFileName(file.file?.virtualPath) }}
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
            <q-card-actions align="center" class="q-gutter-sm justify-center">
              <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
              <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
            </q-card-actions>
            <fieldset class="q-mb-lg">
              <legend>Files List</legend>
              <div class="q-mb-sm q-gutter-sm flex justify-end">
                <q-input v-model="filter" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search" dense clearable>
                  <template #prepend>
                    <q-icon name="o_search" />
                  </template>
                </q-input>
              </div>
              <q-table
                ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="filteredTask" :columns="columns" row-key="id" separator="cell"
                binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]"
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                    <q-td>{{ extractFileName(props.row.file.seoFilename) }}</q-td>
                    <q-td>{{ props.row.createdBy.person.firstName + " " + props.row.createdBy.person.lastName }}</q-td>
                    <q-td>{{ props.row.createdOnUtc.replaceAll("-", "/") }}</q-td>
                    <q-td style="width: 5%;" class="text-center actions">
                      <q-btn icon="o_visibility" size="sm" class="q-pr-xs" flat @click="viewFile(props.row.file.virtualPath)" />
                      <q-btn icon="o_download" size="sm" class="q-pl-xs" flat @click="downloadFile(props.row.file.virtualPath)" />
                      <q-btn
                        icon="o_delete_outline" color="negative" size="sm" class="q-pl-xs text-negative" flat :class="user.userId === props.row.createdBy.id ? '' : 'hidden'"
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
import { ref, onMounted, toRaw, computed } from "vue";
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import projectTaskService from "modules/project-tasks/projectTasks.service";
import { useAuthStore } from "stores/auth";

// Common variables
const authStore = useAuthStore();
const user = authStore.user;
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const filter = ref("");
const processing = ref(false);
const loading = ref(true);
const activeRowId = ref(null);
const rows = ref([]);
const columns = ref([
  { name: "virtualPath", label: "File Name", field: "file.virtualPath", align: "left" },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left" },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left" }
]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, name: { type: String, default: "" }, projectName: { type: String, default: "" }, moduleName: { type: String, default: "" } });
const projectName = props.projectName;
const moduleName = props.moduleName;
const taskName = props.name;
// const readonlyProject = props.id ? "readonly" : "";
// Define model values
const model = ref({
  id: "",
  projectTaskFiles: []
});

// get project details
const getProjectTaskDetails = () => {
  loading.value = true;
  projectTaskService.getProjectTaskDetails(props.id).then((resp) => {
    rows.value = resp.projectTaskFilesList.map(item => ({
      ...item
    }));
  }).finally(() => {
    loading.value = false;
  });
};

function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
}

function viewFile (file) {
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

  if (!model.value.projectTaskFiles) {
    model.value.projectTaskFiles = [];
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

  model.value.projectTaskFiles.push(...validFiles);
  model.value.projectFileFlag = "edit"; // Set the overall flag for tracking
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
  const file = model.value.projectTaskFiles[index];

  if (file && file.name && file.type) {
    URL.revokeObjectURL(file); // Clean up object URL
  }

  if (file && file.file && file.file.virtualPath) {
    // If it's an existing file, mark it as "remove" instead of deleting from array
    file.flag = "remove";
    model.value.projectTaskFiles.splice(index, 1);
  } else {
    // For new files, just remove them from the list
    model.value.projectTaskFiles.splice(index, 1);
  }

  if (model.value.projectTaskFiles.length === 0) {
    model.value.projectFileFlag = "remove";
  }
}

// -------------------------------------------------------------------------------------------------------

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.file.virtualPath.split("/").pop()}` }, () => {
    projectTaskService.deleteFile(item.id, item.type).then(resp => {
      notifySuccess({ message: "File is deleted successfully." });
      getProjectTaskDetails({ pagination: pagination.value });
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
    // Append other fields
    toRaw(model.value.projectTaskFiles || []).forEach((file) => {
      if (file.file && file.file.virtualPath) {
      // For existing files, append metadata instead of the file itself
        formData.append("ExistingFiles", JSON.stringify({
          id: file.id,
          virtualPath: file.file.virtualPath
        }));
      } else {
      // For new files, append as raw file objects (IFormFile)
        formData.append("ProjectTaskFiles", file);
      }
    });

    // Also pass the projectFileFlag for general status tracking
    formData.append("projectFileFlag", model.value.projectFileFlag || "no_change");
    // Check the FormData content (for debugging)
    // for (const [key, value] of formData.entries()) {
    //   // console.log(key, value);
    // }
    formData.append("id", props.id);
    projectTaskService.saveProjectTaskFiles(formData).then((resp) => {
      notifySuccess({ message: "Files are saved successfully." });
      getProjectTaskDetails({ pagination: pagination.value });
      // Reset the uploader input
      if (documentUploaderRef.value) {
        documentUploaderRef.value.reset();
      }
      // Also clear the model if needed
      model.value.projectTaskFiles = [];
    });
  } catch (error) {
    console.error("Error in submitting the files:", error);
    notifyError({ message: "An error occurred while submitting the files." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

// for static filter
// const getNestedValue = (obj, path) => {
//   return path.split(".").reduce((acc, key) => acc && acc[key], obj);
// };

// const taskColumns = columns.value;
// const filterRows = (data, searchTerm, columns) => {
//   if (!searchTerm) return data; // If no filter, return all data
//   const lowerCaseTerm = searchTerm.toLowerCase();
//   return data.filter(row =>
//     columns.some(column => {
//       // const value = column.field.split(".").reduce((obj, key) => obj?.[key], row); // Handle nested fields
//       const value = getNestedValue(row, column.field);
//       return String(value || "").toLowerCase().includes(lowerCaseTerm);
//     })
//   );
// };
const filterRows = (data, searchTerm) => {
  if (!searchTerm) return data;

  const lowerCaseTerm = searchTerm.toLowerCase();

  return data.filter(row => {
    const fileName = extractFileName(row.file?.virtualPath);
    const createdBy = `${row.createdBy?.person?.firstName || ""} ${row.createdBy?.person?.lastName || ""}`;
    const createdDate = row.createdOnUtc?.replaceAll("-", "/") || "";

    return (
      fileName.toLowerCase().includes(lowerCaseTerm) ||
      createdBy.toLowerCase().includes(lowerCaseTerm) ||
      createdDate.toLowerCase().includes(lowerCaseTerm)
    );
  });
};

// Computed properties for each table’s filtered data
// const filteredTask = computed(() => filterRows(rows.value, filter.value, taskColumns));
const filteredTask = computed(() => filterRows(rows.value, filter.value));

// On page rendering
onMounted(() => {
  const propps = { pagination: pagination.value };
  getProjectTaskDetails(propps);
});
</script>
