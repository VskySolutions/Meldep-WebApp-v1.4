<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h3 text-white q-mr-lg" style="flex-grow: 1;">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Issue Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Issue Number</div>
                <div class="text-black">
                  {{ model.issueNumber }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Project Name</div>
                <div class="text-black">
                  {{ model.project.name }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Project Module</div>
                <div class="text-black">
                  {{ model.projectModule.name ? model.projectModule.name : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12">
                <div class="q-mb-xs">Issue Name</div>
                <div class="text-black">
                  {{ model.name }}
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
              <div class="col-12">
                <div class="q-mb-xs">Task</div>
                <div class="text-black">
                  <span v-if="model.projectTaskRelatedMappings?.length">
                    <template v-for="(item, index) in model.projectTaskRelatedMappings" :key="index">
                      <span class="hoverable-cell" style="cursor: pointer;" @click="onViewTask(item.taskId)">
                        #{{ item.projectTask?.projectTaskNumber }}
                        <span v-if="item.projectTask?.status">
                          ({{ item.projectTask.status.dropDownValue }})
                        </span>
                      </span>
                      <span v-if="index < model.projectTaskRelatedMappings.length - 1">, </span>
                    </template>
                  </span>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-3">
                <div class="q-mb-xs">Assign To</div>
                <div class="text-black">
                  {{ model.employee.person.fullName }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-3">
                <div class="q-mb-xs">Issue Priority</div>
                <div class="text-black">
                  {{ model.priority.dropDownValue }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-3">
                <div class="q-mb-xs">Reported By</div>
                <div class="text-black">
                  {{ model.reportedBy.person.fullName }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-3">
                <div class="q-mb-xs">Created Date</div>
                <div class="text-black">
                  {{ model.createdOnUtc }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12">
                <div class="q-mb-xs">Description:</div>
                <div class="text-black Customeditor RichTextEditor">
                  <p style="max-height: 300px; overflow-y: auto;" v-html="model.description ? model.description : '-'" />
                </div>
              </div>
            </div>
          </fieldset>
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
import issueService from "../issue.service";
import viewProjectTask from "modules/project-tasks/components/view.vue";
// import useFilters from "composables/useFilters";

// Common variables
// const { toDate } = useFilters();
const loading = ref(true);
const $q = useQuasar();

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Define model values
const model = ref({
  name: "-",
  issuIssueNumber: "",
  description: "",
  createdOnUtc: "",
  area: {
    dropDownValue: ""
  },
  workspace: {
    dropDownValue: ""
  },
  priority: {
    dropDownValue: ""
  },
  project: {
    name: ""
  },
  projectModule: {
    name: ""
  },
  employee: {
    person: {
      fullName: ""
    }
  },
  reportedBy: {
    person: {
      fullName: ""
    }
  },
  projectTaskRelatedMappings: "-"
});

// get project details
const getIssueDetails = () => {
  loading.value = true;
  issueService.getIssueDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
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

// On page rendering
onMounted(() => {
  getIssueDetails();
});
</script>

<style>
.Customeditor img {
  max-width: 100%; /* Ensures the image width does not exceed the editor's width */
  height: auto; /* Maintains the aspect ratio of the image */
  display: block; /* Prevents inline spacing issues */
}
</style>
