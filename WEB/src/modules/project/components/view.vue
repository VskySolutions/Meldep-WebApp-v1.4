<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1000px; height: 100% !important;max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
            <q-tab name="1_tab" label="Project Info." class="q-px-lg q-mr-md" />
            <q-tab name="2_tab" label="Project Charter" class="q-px-lg" />
            <q-tab v-if="role === 'admin'" name="3_tab" label="Infra Services" class="q-px-lg" :disable="disableTab" />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated class="q-mt-xs">
            <q-tab-panel name="1_tab">
              <!-- <q-card class="card-header with-tools headerBasic"> -->
              <fieldset>
                <legend>Project Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Category</div>
                    <div class="text-black">
                      {{ model.projectCategories.type ? model.projectCategories.type : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Subcategory</div>
                    <div class="text-black">
                      {{ model.projectCategoriesSubCategories.dropDownValue ? model.projectCategoriesSubCategories.dropDownValue : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-12 col-md-12">
                    <div class="q-mb-xs">Subcategory Description</div>
                    <div class="text-black">
                      {{ model.projectCategoriesSubCategories.description ? model.projectCategoriesSubCategories.description : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Customer</div>
                    <div class="text-black">
                      {{ model.customer.company.name ? model.customer.company.name : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Company Contact</div>
                    <div class="text-black">
                      {{ model.companyContact.person.fullName ? model.companyContact.person.fullName : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Start Date</div>
                    <div class="text-black">
                      {{ model.startDateStr ? model.startDateStr : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Project Completion Date</div>
                    <div class="text-black">
                      {{ model.goLiveDateStr ? model.goLiveDateStr : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Status</div>
                    <div class="text-black">
                      {{ model.projectStatus.dropDownValue ? model.projectStatus.dropDownValue : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Project Priority</div>
                    <div class="text-black">
                      {{ model.projectPriority.dropDownValue ? model.projectPriority.dropDownValue : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Project Type</div>
                    <div class="text-black">
                      {{ model.projectType.dropDownValue ? model.projectType.dropDownValue : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-12 col-md-12">
                    <div class="q-mb-xs">Description</div>
                    <div class="text-black RichTextEditor">
                      <p v-html="model.description ? model.description : '-'" />
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Created By</div>
                    <div class="text-black">
                      {{ model.createdBy.person.fullName ? model.createdBy.person.fullName : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Created Date</div>
                    <div class="text-black">
                      {{ model.createdOnUtc ? model.createdOnUtc : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Updated By</div>
                    <div class="text-black">
                      {{ model.updatedBy.person.fullName ? model.updatedBy.person.fullName : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Updated Date</div>
                    <div class="text-black">
                      {{ model.updatedOnUtc ? model.updatedOnUtc : "-" }}
                    </div>
                  </div>
                </div>
              </fieldset>
              <fieldset class="q-mb-lg">
                <legend>Project Files</legend>
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
                      <!-- <q-td>
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
                        </q-icon> -->
                        <!-- <a :href="baseURL + props.row.file.virtualPath" download target="_blank" class="q-mr-sm" rel="noopener noreferrer"><q-icon name="o_visibility" color="black" size="xs" /><q-tooltip>View</q-tooltip></a> -->
                      </q-td>
                    </q-tr>
                  </template>
                </q-table>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="2_tab">
              <fieldset class="q-mb-lg">
                <legend>Project Charter</legend>
                <q-table
                  ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
                  binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]"
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                      <q-td style="width: 45%">{{ props.row.employeeName }}</q-td>
                      <q-td style="width: 45%">{{ props.row.employeeRole }}</q-td>
                      <q-td style="width: 10%" align="right">{{ props.row.productivityFactor }}</q-td>
                    </q-tr>
                  </template>
                </q-table>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel v-if="role === 'admin'" name="3_tab">
              <AccountServicesTab
                :rows="servicesRows"
                :loading="loading"
              />
            </q-tab-panel>
          </q-tab-panels>
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
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";
import projectService from "modules/project/projects.service";
import AccountServicesTab from "modules/infra-account/components/_accountServicesTab.vue";

// login user role
const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "infrastructureadmin", "finance"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

// Common variables
const { toDate } = useFilters();
const rows = ref([]);
// const baseURL = process.env.API_BASE_URL;
const filesrows = ref([]);
const servicesRows = ref([]);
const loading = ref(true);
const tab = ref("1_tab");
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "employeeName", label: "Employee Name", field: "employeeName", align: "left" },
  { name: "employeeRole", label: "Role", field: "employeeRole", align: "left" },
  { name: "productivityFactor", label: "Productivity Factor", field: "productivityFactor", align: "right" }
]);

const filepagination = ref({ sortBy: "", descending: true, rowsPerPage: 20, page: 1 });
const fileColumns = ref([
  { name: "virtualPath", label: "File Name", field: "file.virtualPath", align: "left" },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left" },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left" }
]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "-",
  projectStatusId: "",
  startDateStr: "-",
  goLiveDateStr: "-",
  description: "",
  projectStatus: "-",
  projectPriority: "-",
  projectType: "-",
  projectEmployeeMappings: [],
  customer: {
    company: {
      name: ""
    }
  },
  companyContact: {
    person: {
      fullName: ""
    }
  },
  projectCategories: {
    type: ""
  },
  projectCategoriesSubCategories: {
    dropDownValue: "",
    description: ""
  },
  createdBy: {
    person: {
      fullName: ""
    }
  },
  updatedBy: {
    person: {
      fullName: ""
    }
  }
});
// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// On page rendering
onMounted(() => {
  getProject();
});

// get project details
const getProject = () => {
  loading.value = true;
  projectService.getProject(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.startDateStr = resp.startDate ? toDate(resp.startDate) : "";
    model.value.goLiveDateStr = resp.goLiveDate ? toDate(resp.goLiveDate) : "";
    rows.value = resp.projectEmployeeMappings.map(item => ({
      ...item,
      employeeName: item.employee.person.fullName,
      employeeRole: item.employeeRoleDropdown.dropDownValue,
      startDate: item.productivityFactor
    }));
    filesrows.value = resp.projectFileList.map(item => ({
      ...item
    }));
    servicesRows.value = (resp.infraProjectServices ?? []).map(row => {
      const service = row.infraAccountServices || {};
      return {
        ...service,
        itemTypeId: service.itemType?.id ?? null,
        ownerShipTypeId: service.ownerShipType?.id ?? null,
        paymentTermId: service.paymentTerm?.id ?? null,
        walletTypeId: service.walletType?.id ?? null,
        priceInDollar: service.priceInDollar ?? null,
        actualPriceInDollar: service.actualPriceInDollar ?? null,
        instructions: service.instructions ?? null,
        deleted: false
      };
    });
  }).finally(() => {
    loading.value = false;
  });
};
function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
}
// // Download file method
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

</script>
