<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1000px !important;height: 100%; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
            <q-tab name="1_tab" label="Task Info." class="q-px-lg q-mr-md" />
            <q-tab name="2_tab" label="Task Activities" class="q-px-lg" :disable="disableTab" />
            <q-tab name="3_tab" label="Task Files" class="q-px-lg" :disable="disableTab" />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated class="q-mt-xs">
            <q-tab-panel name="1_tab">
              <fieldset>
                <legend>Project Task Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-lg-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Project Task Name</div>
                    <div class="text-black">
                      {{ model.name ? model.name : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Project Name</div>
                    <div class="text-black">
                      {{ model.project.name ? model.project.name: "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Project Module Name</div>
                    <div class="text-black">
                      {{ model.projectModule.name ? model.projectModule.name : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Area</div>
                    <div class="text-black">
                      {{ model.area.dropDownValue ? model.area.dropDownValue : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Workspace</div>
                    <div class="text-black">
                      {{ model.workspace.dropDownValue ? model.workspace.dropDownValue : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Action</div>
                    <div class="text-black">
                      {{ model.action.dropDownValue ? model.action.dropDownValue : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Task Owner</div>
                      <div class="text-black">
                        {{ model.assignedTo.person.firstName ? model.assignedTo.person.firstName + " " + model.assignedTo.person.lastName : "-" }}
                      </div>
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Estimated Hours</div>
                    <div class="text-black">
                      {{ model.estimateTime ? model.estimateTime : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Status</div>
                    <div class="text-black">
                      {{ model.status.dropDownValue ? model.status.dropDownValue : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">

                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Priority</div>
                    <div class="text-black">
                      {{ model.priority.dropDownValue ? model.priority.dropDownValue : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Start Date</div>
                    <div class="text-black">
                      {{ model.startDate ? model.startDate : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Due Date</div>
                    <div class="text-black">
                      {{ model.endDate ? model.endDate : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Type</div>
                    <div class="text-black">
                      {{ model.type ? model.type.dropDownValue : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Sort Order</div>
                    <div class="text-black">
                      {{ Number(model.sortOrder).toFixed(3) }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Activity Hrs</div>
                    <div class="text-black">
                      {{ totalEstimateHours() }}
                    </div>
                  </div>
                </div>
                <!-- <div class="row q-col-gutter-x-md q-mb-md">
                </div> -->
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12">
                    <div class="q-mb-xs">Issue</div>
                    <div class="text-black">
                      <span v-if="model.issueNumbersWithStatuses">
                        <span
                          class="hoverable-cell"
                          style="cursor: pointer;"
                          @click="onViewIssue(model.issueNumbersWithStatuses.issueId)"
                        >
                          {{ model.issueNumbersWithStatuses.text }}
                        </span>
                      </span>
                      <span v-else>-</span>
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12">
                    <div class="q-mb-xs">Requirement</div>
                    <div class="text-black">
                      <span v-if="model.requirementNumbersWithStatuses">
                        <span
                          class="hoverable-cell"
                          style="cursor: pointer;"
                          @click="onViewRequirement(model.requirementNumbersWithStatuses.requirementId)"
                        >
                          {{ model.requirementNumbersWithStatuses.text }}
                        </span>
                      </span>
                      <span v-else>-</span>
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
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Created By</div>
                    <div class="text-black">
                      {{ model.createdBy.person.firstName ? model.createdBy.person.firstName + " "+ model.createdBy.person.lastName : "-" }}
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
                      {{ model.updatedBy.person.firstName ? model.updatedBy.person.firstName + " "+ model.updatedBy.person.lastName : "-" }}
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
            </q-tab-panel>

            <q-tab-panel name="2_tab">
              <fieldset class="q-mb-lg">
                <legend>Task Activities</legend>
                <q-table
                  v-model:pagination="taskActivityPagination" :loading="loading" :rows="projectActivities" :columns="taskActivityColumns" row-key="id" separator="cell" :rows-per-page-options="[20, 50, 100, 200, 500]"
                  binary-state-sort class="Custom-DataTable" no-data-label="No task activity available" :filter="filter"
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                      <!-- <q-td style="width: 20%;">{{ toMonthYear(props.row.targetMonth) }}</q-td> -->
                      <q-td style="width: 20%;">{{ props.row.assignedTo.person.firstName + " " + props.row.assignedTo.person.lastName }}</q-td>
                      <q-td style="width: 20%;">{{ props.row.name }}
                        <q-icon
                          v-if="props.row.activityNameDescription"
                          name="o_info"
                          size="16px"
                          class="q-ml-xs"
                        >
                          <q-tooltip v-if="props.row.activityNameDescription" class="text-wrap break-words q-pa-sm" max-width="300px">
                            <div v-html="props.row.activityNameDescription" />
                          </q-tooltip>
                        </q-icon>
                      </q-td>
                      <!-- <q-td style="width: 20%;">{{ props.row.activityStatus.dropDownValue }}</q-td> -->
                      <q-td style="width: 10%;" class="text-right">{{ props.row.estimateHours }}</q-td>
                      <q-td class="RichTextEditor"><div style="display: block; max-width: 500px; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;" v-html="props.row.description" /></q-td>
                    </q-tr>
                    <q-tr v-if="props.pageIndex === rows.length - 1">
                      <q-td colspan="2" class="text-right font-bold"><b>Total Hours:</b></q-td>
                      <q-td class="text-right"><b>{{ totalEstimateHours() }}</b></q-td>
                      <q-td />
                    </q-tr>
                  </template>
                </q-table>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="3_tab">
              <fieldset class="q-mb-lg">
                <legend>Project Task Files</legend>
                <q-table
                  ref="tableRef" v-model:pagination="filePagination" bordered class="no-shadow" :loading="loading" :rows="filesrows" :columns="fileColumns" row-key="id" separator="cell"
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
            </q-tab-panel>
          </q-tab-panels>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";
import projectTaskService from "modules/project-tasks/projectTasks.service";
import useFilters from "composables/useFilters";

import viewIssue from "modules/issue/components/view.vue";
import viewRequirement from "modules/requirement/components/view.vue";

// Common variables
const { toDate } = useFilters();
const loading = ref(true);
const $q = useQuasar();
const rows = ref([]);
const filesrows = ref([]);
const tab = ref("1_tab");
const projectActivities = ref([]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "-",
  estimateTime: "",
  description: "",
  project: {
    name: ""
  },
  projectModule: {
    name: ""
  },
  area: {
    dropDownValue: ""
  },
  workspace: {
    dropDownValue: ""
  },
  action: {
    dropDownValue: ""
  },
  status: {
    dropDownValue: ""
  },
  priority: {
    dropDownValue: ""
  },
  type: {
    dropDownValue: ""
  },
  assignedTo: {
    person: {
      firstName: "",
      lastName: ""
    }
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
  },
  issueNumbersWithStatuses: "-",
  requirementNumbersWithStatuses: "-"
});

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const filePagination = ref({ sortBy: "", descending: true, rowsPerPage: 20, page: 1 });
const fileColumns = ref([
  { name: "virtualPath", label: "File Name", field: "file.virtualPath", align: "left" },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left" },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left" }
]);

const taskActivityPagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const taskActivityColumns = ref([
  // { name: "targetMonth", label: "Target Month", field: "targetMonth", align: "left", sortable: false },
  { name: "ActivityOwner", label: "Activity Owner", field: "ActivityOwner", align: "left", sortable: true },
  { name: "name", label: "Activity Name", field: "name", align: "left", sortable: true },
  // { name: "name", label: "Activity Status", field: "name", align: "left", sortable: true },
  { name: "EstimatedHrs", label: "Estimated Hrs.", field: "EstimatedHrs", align: "right", sortable: true },
  { name: "description", label: "Description", field: "description", align: "left", sortable: true }
]);

function totalEstimateHours () {
  // Calculate the total estimate hours
  const total = rows.value.reduce((sum, activity) => {
    return sum + (activity.estimateHours || 0); // Add activity's estimateHours if present
  }, 0);

  return parseFloat(total.toFixed(2)); // Round to 2 decimal places
}

// get project details
const getProjectTaskDetails = () => {
  loading.value = true;
  projectTaskService.getProjectTaskDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rows.value = resp.projectActivities.map(activity => ({
      ...activity
    }));
    filesrows.value = resp.projectTaskFilesList.map(item => ({
      ...item
    }));
    projectActivities.value = resp.projectActivities.map(item => ({
      ...item
    }));
    model.value.issueNumbersWithStatuses = (() => {
      const mapping = resp.projectTaskRelatedMappings?.find(m => m?.issueId != null);
      if (!mapping) return null;

      return {
        value: String(mapping.id),
        text: `#${mapping.issue?.issueNumber ?? ""} (${mapping.issue?.status?.dropDownValue ?? ""})`,
        issueId: mapping.issueId
      };
    })();
    model.value.requirementNumbersWithStatuses = (() => {
      const mapping = resp.projectTaskRelatedMappings?.find(m => m?.requirementId != null);
      if (!mapping) return null;

      return {
        value: String(mapping.id),
        text: `#${mapping.requirement?.requirementNumber ?? ""} (${mapping.requirement?.status?.dropDownValue ?? ""})`,
        requirementId: mapping.requirementId
      };
    })();
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

// View popup
const onViewIssue = (id) => {
  $q.dialog({
    component: viewIssue,
    componentProps: { id }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};
// View popup
const onViewRequirement = (id) => {
  $q.dialog({
    component: viewRequirement,
    componentProps: { id }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// On page rendering
onMounted(() => {
  getProjectTaskDetails();
});
</script>
