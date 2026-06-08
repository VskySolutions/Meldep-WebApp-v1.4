<template>
  <fieldset class="q-mb-lg">
    <legend>Project Files</legend>
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
          <q-td style="width: 10%"><span v-if="preSourceName !== props.row.type" :set="preSourceName = props.row.type">{{ props.row.type }}</span></q-td>
          <q-td style="width: 10%"><span v-if="preSubModuleName !== props.row.sub_Module" :set="preSubModuleName = props.row.sub_Module">{{ props.row.sub_Module }}</span></q-td>
          <q-td>{{ extractFileName(props.row.seoFilename) }}</q-td>
          <q-td>{{ props.row.createdBy.person.fullName }}</q-td>
          <q-td>{{ props.row.createdOnUtc }}</q-td>
          <q-td style="width: 5%;" class="text-center actions">
            <q-btn icon="o_visibility" size="sm" class="q-pr-xs" flat @click="viewFile(props.row.virtualPath)" />
            <q-btn icon="o_download" size="sm" class="q-pl-xs" flat @click="downloadFile(props.row.virtualPath)" />
          </q-td>
        </q-tr>
      </template>
    </q-table>
  </fieldset>
</template>

<script setup>
// Import libraries
import { ref, onMounted } from "vue";
import projectService from "modules/project/projects.service";

// Common variables
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const rows = ref([]);
const filter = ref("");
// const baseURL = process.env.API_BASE_URL;
const loading = ref(true);
const columns = ref([
  { name: "type", label: "Source", field: "type", align: "left" },
  { name: "sub_Module", label: "Source Name", field: "sub_Module", align: "left" },
  { name: "virtualPath", label: "File Name", field: "virtualPath", align: "left" },
  { name: "createdBy.person.fullName", label: "Created By", field: "createdByPersonFullName", align: "left", sortable: false },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left" }
]);

// Props values i.e. come from query string
const props = defineProps({ projectId: { type: String, default: "" } });
const projectId = props.projectId;
// On page rendering
onMounted(() => {
  const propps = { pagination: pagination.value };
  getAllFilesByProjectId(propps);
});

// get project details
const getAllFilesByProjectId = (propss) => {
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
  const fileUrl = new URL(file).href;
  const fileExtension = fileUrl.split(".").pop().toLowerCase();
  const supportedFormats = ["pdf", "docx", "xlsx", "pptx"];
  const imageFormats = ["jpg", "jpeg", "png", "gif", "svg"];

  let viewerUrl = fileUrl;
  // console.log("fileUrl", fileUrl);

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
function downloadFile (file) {
  const link = document.createElement("a");
  link.href = file;
  link.download = file.split("/").pop();
  link.click();
}
</script>
