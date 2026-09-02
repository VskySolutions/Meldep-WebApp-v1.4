<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1200px; height: 100%; max-height: 100% !important;max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white q-mr-lg" style="flex-grow: 1;">{{ model.title }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <!-- <q-card-section class="card-header with-tools"> -->
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator inline-label mobile-arrows>
            <q-tab name="1_tab" label="Manage Description" class="q-px-lg q-mr-md" />
            <q-tab name="2_tab" label="View Description" class="q-px-lg" :disable="disableTab" />
            <q-tab name="3_tab" label="Short Description" class="q-px-lg" :disable="disableTab" />
            <q-tab name="4_tab" label="Requirement Info." class="q-px-lg" :disable="disableTab" />
            <q-tab name="5_tab" label="Document Reference List" class="q-px-lg" :disable="disableTab" />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated>
            <q-tab-panel name="1_tab">
              <viewDescriptionTimeLineView
                :id="selectedRequirementId"
              />
            </q-tab-panel>
            <q-tab-panel name="2_tab">
              <viewDescriptionSimpleView
                :id="selectedRequirementId"
              />
            </q-tab-panel>
            <q-tab-panel name="3_tab">
              <fieldset>
                <legend>Short Description</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="text-black RichTextEditor">
                    <span v-html="model.shortDescription || '-'"></span>
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="4_tab">
              <fieldset>
                <legend>Requirement Info.</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12">
                    <div class="q-mb-xs">Requirement Title :</div>
                    <div class="text-black">{{ model.title }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Requirement No. : </div>
                    <div class="text-black">{{ model.requirementNumber }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Project Name : </div>
                    <div class="text-black">{{ model.project.name }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Project Module Name :</div>
                    <div class="text-black">{{ model.projectModule.name }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Type :</div>
                    <div class="text-black">{{ model.requirementType.dropDownValue }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Area : </div>
                    <div class="text-black">{{ model.area.dropDownValue }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Workspace :</div>
                    <div class="text-black">{{ model.workspace.dropDownValue }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12">
                    <div class="q-mb-xs">Task :</div>
                      <div class="text-black">
                        <span v-if="model.projectTaskRelatedMappings?.length">
                          <template v-for="(item, index) in model.projectTaskRelatedMappings" :key="index">
                            <span class="hoverable-cell" style="cursor: pointer;" @click="onViewTask(item.id)">
                              #{{ item.projectTaskNumber }}
                              <span v-if="item.status">
                                ({{ item.status.dropDownValue }})
                              </span>
                            </span>
                            <span v-if="index < model.projectTaskRelatedMappings.length - 1">, </span>
                          </template>
                        </span>
                      </div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Draft/Confirmed :</div>
                      <div class="text-black">{{ model.editingStatus === 1  ? 'Draft' : 'Confirmed' }} </div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Requirement Identified User Type :</div>
                      <div class="text-black">{{ model.userType.dropDownValue }}</div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Requirement Identified By :</div>
                      <div class="text-black">{{ model.userType.dropDownValue === 'Customer' ? (model.customer && model.customer.fullName ? model.customer.fullName : 'N/A') : (model.employee && model.employee.person && model.employee.person.fullName ? model.employee.person.fullName : 'N/A') }}</div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Requirement Entered By :</div>
                      <div class="text-black">{{ model.requirementEntered.person.fullName }}</div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Requirement Status :</div>
                      <div class="text-black">{{ model.status.dropDownValue }}</div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Approval Status :</div>
                      <div class="text-black">{{ model.approvalStatusDropDown.dropDownValue }}</div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Planned Start Date :</div>
                      <div class="text-black">{{ model.plannedStartDate }}</div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Planned End Date :</div>
                      <div class="text-black">{{ model.plannedEndDate }}</div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Actual Start Date :</div>
                      <div class="text-black">{{ model.actualStartDate }}</div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Actual End Date :</div>
                      <div class="text-black">{{ model.actualEndDate }}</div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md hidden">
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Confirmed By :</div>
                      <div class="text-black">{{ model.confirmedBy.person.fullName }}</div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Approved By :</div>
                      <div class="text-black">{{ model.approvedBy.person.fullName }}</div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div v-if="model.status.dropDownValue == 'Close'" class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Requirement Close Date :</div>
                      <div class="text-black">{{ model.closeDate }}</div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md hidden">
                    <div class="col-12">
                      <div class="q-mb-xs">Notes :</div>
                      <div class="text-black"><p v-html="model.notes ? model.notes : '-'" /></div>
                    </div>
                  </div>
                  <div v-if="model.lastNote" class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-sm-12 col-md-12">
                      <div class="q-mb-xs">Last Note :</div>
                      <div class="text-black">
                        <span v-html="model.lastNote" />
                      </div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Created By :</div>
                      <div class="text-black">{{ model.createdBy.person.firstName + " "+ model.createdBy.person.lastName }}</div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Created Date :</div>
                      <div class="text-black">{{ model.createdOnUtc }}</div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Updated By :</div>
                      <div class="text-black">{{ model.updatedBy?.person?.firstName && model.updatedBy?.person?.lastName
                        ? model.updatedBy.person.firstName + " " + model.updatedBy.person.lastName
                        : "-" }}
                      </div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-6">
                      <div class="q-mb-xs">Updated Date :</div>
                      <div class="text-black">{{ model.updatedOnUtc }}</div>
                    </div>
                  </div>
                  <fieldset v-if="changeLogRows && changeLogRows.length === 0" class="q-mb-lg hidden">
                    <legend>Requirement Change Log</legend>
                    <q-table
                      ref="tableRef"
                      v-model:pagination="changeLogPagination"
                      bordered
                      class="no-shadow"
                      :loading="loading"
                      :rows="changeLogRows"
                      :columns="chnageLogColumns"
                      row-key="id"
                      separator="cell"
                      no-data-label="No data available"
                      binary-state-sort
                      :rows-per-page-options="[20, 50, 100, 200, 500]"
                    >
                      <template #header="props">
                        <q-tr :props="props" class="bg-primary text-white">
                          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                        </q-tr>
                      </template>

                      <template #body="props">
                        <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                          <q-td>{{ props.row.requirementLogDate }}</q-td>
                          <q-td>{{ props.row.employee.person.fullName }}</q-td>
                          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;">{{ props.row.requirementName }}</q-td>
                          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 40%;"><div class="RichTextEditor" v-html="props.row.description" /></q-td>
                        </q-tr>
                      </template>
                    </q-table>
                  </fieldset>
                </fieldset>
              </q-tab-panel>
              <q-tab-panel name="5_tab">
                 <fieldset class="q-mb-lg">
                    <legend>Document Reference List</legend>
                    <q-table
                      ref="tableRef"
                      v-model:pagination="pagination"
                      bordered class="no-shadow"
                      :loading="loading"
                      :rows="rows"
                      :columns="columns"
                      row-key="id"
                      separator="cell"
                      no-data-label="No data available"
                      binary-state-sort
                      :rows-per-page-options="[20, 50, 100, 200, 500]"
                    >
                      <template #header="props">
                        <q-tr :props="props" class="bg-primary text-white">
                          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                        </q-tr>
                      </template>
                      <template #body="props">
                        <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">
                            <a :href="props.row.filePath" target="_blank" class="text-bluee">
                              {{ props.row.filePath }}
                            </a>
                          </q-td>
                          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;">{{ props.row.fileName }}</q-td>
                          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 30%;">{{ props.row.note }}</q-td>
                        </q-tr>
                      </template>
                    </q-table>
                  </fieldset>
              </q-tab-panel>
            </q-tab-panels>
          </div>
        </div>
      <!-- </q-card-section> -->
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar, QBtn } from "quasar";
import { ref, onMounted, watch } from "vue";
import _ from "lodash";

import requirementService from "../requirement.service";
import viewProjectTask from "modules/project-tasks/components/view.vue";

import viewDescriptionTimeLineView from "src/modules/requirement/components/_description_timeline_view.vue";
import viewDescriptionSimpleView from "src/modules/requirement/components/_description_simple_view.vue";


// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const rows = ref([]);
const changeLogRows = ref([]);
const $q = useQuasar();

// Common variables
const selectedRequirementId = ref(props.id);
const loading = ref(true);
const tab = ref("1_tab");
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "filePath", label: "File Path", field: "filePath", align: "left", sortable: true },
  { name: "fileName", label: "File Name", field: "fileName", align: "left", sortable: true },
  { name: "note", label: "Notes", field: "note", align: "left", sortable: true }
]);

const changeLogPagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const chnageLogColumns = [
  { name: "requirementLogDate", label: "Change Date", field: "requirementLogDate", align: "left", sortable: true },
  { name: "employee.person.fullName", label: "Changed By", field: "employee.person.fullName", align: "left", sortable: true },
  { name: "requirementName", label: "Requirement", field: "requirementName", align: "left", sortable: true },
  { name: "description", label: "Description", field: "description", align: "left", sortable: false }
];

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  title: "",
  notes: "",
  employeeId: "",
  IdentifiedDate: "",
  approvalStatus: "",
  description: "",
  createdOnUtc: "",
  lastNote: "",
  shortDescription: "",
  project: {
    name: ""
  },
  projectModule: {
    name: ""
  },
  requirementType: {
    dropDownValue: ""
  },
  area: {
    dropDownValue: ""
  },
  workspace: {
    dropDownValue: ""
  },
  customer: {
    fullName: ""
  },
  status: {
    dropDownValue: ""
  },
  userType: {
    dropDownValue: ""
  },
  requirementEntered: {
    person: {
      fullName: ""
    }
  },
  confirmedBy: {
    person: {
      fullName: ""
    }
  },
  approvedBy: {
    person: {
      fullName: ""
    }
  },
  employee: {
    person: {
      fullName: ""
    }
  },
  approvalStatusDropDown: {
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

// get get Requirement on edit mode
const getRequirement = () => {
  loading.value = true;
  requirementService.getRequirementDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rows.value = resp.filePathDetails.map(item => ({
      ...item,
      editing: false,
      flag: "Edit"
    }));

    changeLogRows.value = resp.requirementChangeLog.map(item => ({
      ...item,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};
// View popup
const onViewTask = (id) => {
  $q.dialog({
    component: viewProjectTask,
    componentProps: { id }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

watch(tab, (newTab) => {
  if (newTab === "1_tab") {
    getRequirement();
  } else if (newTab === "2_tab") {
    getRequirement();
  }
});

onMounted(() => {
  getRequirement();
});
</script>

<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
