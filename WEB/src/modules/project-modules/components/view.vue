<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1000px !important; height: 100%; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Project Module Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Project Name</div>
                <div class="text-black">
                  {{ model.project.name }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Start Date</div>
                <div class="text-black">
                  {{ model.startDate ? model.startDate : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6 hidden">
                <div class="q-mb-xs">Project Module Number</div>
                <div class="text-black">
                  {{ model.projectModuleNumber }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">End Date</div>
                <div class="text-black">
                  {{ model.endDate ? model.endDate : "-" }}
                </div>
              </div>
              <div class="col-6 col-sm-6 col-md-6">
                <div class="q-mb-xs">Project Module Status</div>
                <div class="text-black">
                  {{ model.projectModuleStatus.dropDownValue }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <!-- <div class="col">
                  <div class="q-mb-xs text-black">Project Module Type</div>
                  <div>
                    {{ model.projectModuleType.dropDownValue }}
                  </div>
                </div> -->
              <div class="col-6 col-sm-6 col-md-6">
                <div class="q-mb-xs">Sort Order</div>
                <div class="text-black">
                  {{ model.sortOrder }}
                </div>
              </div>
              <div class="col-6 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created Date</div>
                <div class="text-black">
                  {{ model.createdOnUtc }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created By</div>
                <div class="text-black">
                  {{ model.createdBy.person.firstName + " "+ model.createdBy.person.lastName }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Updated By</div>
                <div class="text-black">
                  {{ model.updatedBy?.person?.firstName && model.updatedBy?.person?.lastName
                    ? model.updatedBy.person.firstName + " " + model.updatedBy.person.lastName
                    : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12">
                <div class="q-mb-xs">Description:</div>
                <div class="text-black RichTextEditor">
                  <p v-html="model.description ? model.description : '-'" />
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12">
                <div class="q-mb-xs">Notes:</div>
                <div class="text-black RichTextEditor">
                  <p v-html="model.notes ? model.notes : '-'" />
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset class="q-mb-lg">
            <legend>Project Module Files</legend>
            <q-table
              ref="tableRef" v-model:pagination="filepagination" bordered class="no-shadow" :loading="loading" :rows="filesrows" :columns="fileColumns" row-key="id" separator="cell"
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
                  <!-- <q-td style="width: 10%">
                    <a :href="baseURL + props.row.file.virtualPath" target="_blank">
                      {{ extractFileName(props.row.file.virtualPath) }}
                    </a>
                  </q-td> -->
                  <q-td>{{ extractFileName(props.row.file.seoFilename) }}</q-td>
                  <q-td>{{ props.row.createdBy.person.firstName + " " + props.row.createdBy.person.lastName }}</q-td>
                  <q-td>{{ props.row.createdOnUtc.replaceAll("-", "/") }}</q-td>
                  <q-td style="width: 5%;" class="text-center actions">
                    <q-btn icon="o_visibility" size="sm" class="q-pr-xs" flat @click="viewFile(props.row.file.virtualPath)" />
                    <q-btn icon="o_download" size="sm" class="q-pl-xs" flat @click="downloadFile(props.row.file.virtualPath)" />
                    <!-- <q-icon name="o_download" class="cursor-pointer q-mr-sm" size="xs" @click="onDownload(props.row.file.virtualPath)">
                      <q-tooltip>Download</q-tooltip>
                    </q-icon>
                    <a :href="baseURL + props.row.file.virtualPath" download target="_blank" class="q-mr-sm" rel="noopener noreferrer"><q-icon name="o_visibility" color="black" size="xs" /><q-tooltip>View</q-tooltip></a> -->
                  </q-td>
                </q-tr>
              </template>
            </q-table>
          </fieldset>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";
import projectModulesService from "modules/project-modules/projectModules.service";
import useFilters from "composables/useFilters";

// Common variables
const { toDate } = useFilters();
const loading = ref(true);
const filesrows = ref([]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "-",
  description: "",
  project: {
    name: ""
  },
  projectModuleStatus: {
    dropDownValue: ""
  },
  projectModuleType: {
    dropDownValue: ""
  },
  createdBy: {
    person: {
      firstName: "",
      lastName: ""
    }
  },
  updatedBy: {
    person: {
      firstName: "",
      lastName: ""
    }
  }
});

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const filepagination = ref({ sortBy: "", descending: true, rowsPerPage: 20, page: 1 });
const fileColumns = ref([
  { name: "virtualPath", label: "File Name", field: "file.virtualPath", align: "left" },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left" },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left" }
]);
// On page rendering
onMounted(() => {
  getProjectModuleDetails();
});

// get project details
const getProjectModuleDetails = () => {
  loading.value = true;
  projectModulesService.getProjectModuleDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    filesrows.value = resp.projectModuleFilesList.map(item => ({
      ...item
    }));
  }).finally(() => {
    loading.value = false;
  });
};
function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
}
// Download file method
// function onDownload (filePath) {
//   // Construct full file URL dynamically using this.baseURL if part of Vue instance
//   const fullPath = `${baseURL}${filePath}`;

//   // Create a temporary <a> element for triggering the download
//   const link = document.createElement("a");
//   link.href = fullPath;
//   link.download = filePath.split("/").pop(); // Extract the filename from the file path
//   link.click(); // Trigger download
// }
function downloadFile (file) {
  const link = document.createElement("a");
  link.href = file;
  link.download = file.split("/").pop();
  link.click();
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
    // console.log("googleDocsViewer", viewerUrl);
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

</script>
