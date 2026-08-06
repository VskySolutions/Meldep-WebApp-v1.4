<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="flex-grow: 1;">{{ model.title }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Project Action Items Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Project Name</div>
                <div class="text-black q-mb-cs">
                  {{ model.project.name }}
                </div>
              </div>
              <div class="col">
                <div class="col-12 col-md-6">Requirement</div>
                <div class="text-black">
                  {{ model.requirement.title }}
                </div>
              </div>
            </div>
             <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Title</div>
                <div class="text-black q-mb-cs">
                  {{ model.title }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">AssignedTo</div>
                <div class="text-black">
                  {{ model.assignedTo }}
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Priority</div>
                <div class="text-black">
                  {{ model.priority.dropDownValue }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Due Date</div>
                <div class="text-black">
                  {{ model.dueDate }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12">
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

import projectActionItemsService from "modules/project-action-items/projectActionItems.service";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }});

// Common variables
const loading = ref(true);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  title: "",
  description: "",
  assignedTo: "",
  dueDate: "",
  createdOnUtc: "",
  updatedOnUtc: "",
  project: {
    name: ""
  },
  requirement: {
    title: ""
  },
  priority: {
    dropDownValue: ""
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

// get Project Action Items details
const getProjectActionItemDetailsById = () => {
  loading.value = true;
  projectActionItemsService.getProjectActionItemDetailsById(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// On page rendering
onMounted(() => {
  getProjectActionItemDetailsById();
});

</script>
