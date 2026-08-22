<template>
  <q-card flat bordered>

    <!-- Header -->
    <q-card-section class="row bg-primary text-white q-pa-sm items-center justify-between">
      <div class="row items-center">
        <q-icon
          name="o_task_alt"
          size="sm"
          class="q-mr-md"
        />
        <div class="column">
          <div class="text-caption text-blue-2">
            #{{ model.projectTaskNumber }} • TASK
          </div>

          <div class="text-h6 text-weight-bold">
            {{ model.name }}
          </div>
        </div>
      </div>
    </q-card-section>

    <q-separator />

    <!-- Tabs -->
    <q-tabs
      v-model="tab"
      dense
      active-color="primary"
      indicator-color="primary"
      align="left"
      class="text-grey-8"
    >
      <q-tab
        name="details"
        label="Details"
      />

      <q-tab
        name="activities"
        label="Activities"
      />

      <q-tab
        name="files"
        label="Files"
      />
    </q-tabs>

    <q-separator />

    <q-tab-panels
      v-model="tab"
      animated
    >
      <!-- DETAILS -->
      <q-tab-panel name="details">
        <div class="row q-col-gutter-lg">
          <div class="col-12 col-md-6">
            <q-list dense>
              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Task No
                  </div>
                  <div class="q-mb-sm">
                    {{ model.projectTaskNumber || "-" }}
                  </div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Project Name
                  </div>
                  <div class="q-mb-sm">
                    {{ model.project?.name || "-" }}
                  </div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Area
                  </div>
                  <div class="q-mb-sm">{{ model.area?.dropDownValue || "-" }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Action
                  </div>
                  <div class="q-mb-sm">{{ model.action?.dropDownValue || "-" }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Task Owner
                  </div>
                  <div class="q-mb-sm">
                    {{
                      model.assignedTo?.person?.firstName
                        ? `${model.assignedTo.person.firstName} ${model.assignedTo.person.lastName ?? ''}`
                        : '-'
                    }}
                  </div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Status
                  </div>
                  <div class="q-mb-sm">{{ model.status?.dropDownValue || "-" }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Start Date
                  </div>
                  <div class="q-mb-sm">{{ model.startDate || "-" }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Sort Order
                  </div>
                  <div class="q-mb-sm">{{ Number(model.sortOrder || 0).toFixed(3) }}</div>
                </q-item-section>
              </q-item>
              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Issue
                  </div>
                  <div class="q-mb-sm">{{ model.issueText }}</div>
                </q-item-section>
              </q-item>
              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Created By
                  </div>
                  <div class="q-mb-sm">{{ model.createdBy.person.firstName ? model.createdBy.person.firstName + " "+ model.createdBy.person.lastName : "-" }}</div>
                </q-item-section>
              </q-item>
              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Updated By
                  </div>
                  <div class="q-mb-sm">{{ model.updatedBy.person.firstName ? model.updatedBy.person.firstName + " "+ model.updatedBy.person.lastName : "-" }}</div>
                </q-item-section>
              </q-item>
            </q-list>
          </div>
          <div class="col-12 col-md-6">
            <q-list dense>
              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Project Task Name
                  </div>
                  <div class="q-mb-sm">
                    {{ model.name || "-" }}
                  </div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Project Module
                  </div>
                  <div class="q-mb-sm">
                   {{ model.projectModule?.name || "-" }}
                  </div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Workspace
                  </div>
                  <div class="q-mb-sm">{{ model.workspace?.dropDownValue || "-" }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Type
                  </div>
                  <div class="q-mb-sm">{{ model.type?.dropDownValue || "-" }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Estimated Hours
                  </div>
                  <div class="q-mb-sm">{{ model.estimateTime || "-" }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Priority
                  </div>
                  <div class="q-mb-sm">{{ model.priority?.dropDownValue || "-" }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Due Date
                  </div>
                  <div class="q-mb-sm">{{ model.endDate || "-" }}</div>
                </q-item-section>
              </q-item>

              <!-- <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Activity Hrs
                  </div>
                  <div class="q-mb-sm">{{ totalEstimateHours() }}</div>
                </q-item-section>
              </q-item> -->

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Requirement
                  </div>
                  <div class="q-mb-sm">{{ model.requirementText }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Created Date
                  </div>
                  <div class="q-mb-sm">{{ model.createdOnUtc ? model.createdOnUtc : "-" }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">
                    Updated Date
                  </div>
                  <div class="q-mb-sm">{{ model.updatedOnUtc ? model.updatedOnUtc : "-" }}</div>
                </q-item-section>
              </q-item>
            </q-list>
          </div>
        </div>
        <div class="col-12 q-ml-md">
          <div class="text-caption text-grey q-mb-sm">Description</div>

          <div
            class="RichTextEditor"
            v-html="model.description || '-'"
          />
        </div>
      </q-tab-panel>

      <!-- ACTIVITIES -->
      <q-tab-panel name="activities">
        <q-table
          v-model:pagination="taskActivityPagination"
          :loading="loading"
          :rows="projectActivities"
          :columns="taskActivityColumns"
          row-key="id"
          separator="cell"
          :rows-per-page-options="[20, 50, 100, 200, 500]"
          binary-state-sort
          class="Custom-DataTable"
          no-data-label="No task activity available"
          :filter="filter"
        >
          <template #header="props">
            <q-tr :props="props" class="bg-primary text-white">
              <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            </q-tr>
          </template>
          <template #body="props">
            <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
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
              <!-- <q-td style="width: 10%;" class="text-right">{{ props.row.estimateHours }}</q-td> -->
              <q-td class="RichTextEditor"><div style="display: block; max-width: 500px; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;" v-html="props.row.description" /></q-td>
            </q-tr>
            <q-tr class="hidden" v-if="props.pageIndex === projectActivities.length - 1">
              <q-td colspan="2" class="text-right font-bold"><b>Total Hours:</b></q-td>
              <q-td class="text-right"><b>{{ totalEstimateHours() }}</b></q-td>
              <q-td />
            </q-tr>
          </template>
        </q-table>
      </q-tab-panel>

      <!-- FILES -->
      <q-tab-panel name="files">
        <q-table
          ref="tableRef"
          v-model:pagination="filePagination"
          bordered class="no-shadow"
          :loading="loading"
          :rows="filesRows"
          :columns="fileColumns"
          row-key="id"
          separator="cell"
          binary-state-sort
          :rows-per-page-options="[20, 50, 100, 200, 500]"
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
              </q-td>
            </q-tr>
          </template>
        </q-table>
      </q-tab-panel>

    </q-tab-panels>

  </q-card>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import _ from "lodash";
import projectTaskService from "modules/project-tasks/projectTasks.service";

const props = defineProps({
  id: {
    type: String,
    required: true
  }
});

const tab = ref("details");
const loading = ref(false);

const model = ref({
  project: {},
  projectModule: {},
  area: {},
  workspace: {},
  action: {},
  assignedTo: { person: {} },
  status: {},
  priority: {},
  type: {},
  createdBy: { person: {} },
  updatedBy: { person: {} }
});

const projectActivities = ref([]);
const filesRows = ref([]);

const taskActivityPagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const taskActivityColumns = ref([
  { name: "ActivityOwner", label: "Activity Owner", field: "ActivityOwner", align: "left", sortable: true },
  { name: "name", label: "Activity Type", field: "name", align: "left", sortable: true },
  // { name: "EstimatedHrs", label: "Estimated Hrs.", field: "EstimatedHrs", align: "right", sortable: true },
  { name: "description", label: "Description", field: "description", align: "left", sortable: true }
]);
const filePagination = ref({ sortBy: "", descending: true, rowsPerPage: 20, page: 1 });
const fileColumns = ref([
  { name: "virtualPath", label: "File Name", field: "file.virtualPath", align: "left" },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left" },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left" }
]);

const totalEstimateHours = () => {
  const totalMinutes = projectActivities.value.reduce((total, item) => {
    const value = String(item.estimateHours ?? "").trim();

    if (!value) return total;

    const [hours = 0, minutes = 0] = value.split(":").map(Number);

    return total + (hours * 60) + minutes;
  }, 0);

  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;

  return `${String(hours).padStart(2, "0")}:${String(minutes).padStart(2, "0")}`;
};

function extractFileName(path) {
  return path ? path.split("/").pop() : "-";
}

async function getProjectTaskDetails() {
  if (!props.id) return;
  loading.value = true;

  try {
    const resp = await projectTaskService.getProjectTaskDetails(props.id);

    model.value = _.cloneDeep(resp);

    projectActivities.value = resp.projectActivities || [];

    filesRows.value = resp.projectTaskFilesList.map(item => ({
      ...item
    }));
    const issue = resp.projectTaskRelatedMappings?.find(x => x.issueId);

    model.value.issueText = issue
      ? `#${issue.issue.issueNumber} (${issue.issue.status.dropDownValue})`
      : "-";

    const requirement = resp.projectTaskRelatedMappings?.find(
      x => x.requirementId
    );

    model.value.requirementText = requirement
      ? `#${requirement.requirement.requirementNumber} (${requirement.requirement.status.dropDownValue})`
      : "-";
  } finally {
    loading.value = false;
  }
}

watch(
  () => props.id,
  async () => {
    await getProjectTaskDetails();
  },
  {
    immediate: true
  }
);

onMounted(async () => {
  await getProjectTaskDetails();
});
</script>
