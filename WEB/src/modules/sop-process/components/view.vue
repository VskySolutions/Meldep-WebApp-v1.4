<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="min-width:80vw; max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">View SOP Process</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogCancel()" />
      </q-card-section>
      <q-separator />
      <q-form greedy class="q-pa-md">
        <fieldset>
          <legend>Process Info</legend>
          <div class="row q-col-gutter-x-md q-mb-md">
            <div class="col-12">
              <div class="q-mb-xs">Title</div>
              <div class="text-black">
                {{ model.title }}
              </div>
            </div>
          </div>
          <div class="row q-col-gutter-x-md q-mb-md">
            <div class="col-12">
              <div class="q-mb-xs">Purpose</div>
              <div class="text-black">
                {{ model.purpose }}
              </div>
            </div>
          </div>
          <div class="row q-col-gutter-x-md q-mb-md">
            <div class="col-12 col-md-6">
              <div class="q-mb-xs">Category</div>
              <div class="text-black">
                {{ model.category.type }}
              </div>
            </div>
            <div class="col-12 col-md-6">
              <div class="q-mb-xs">Sub Category</div>
              <div class="text-black">
                {{ model.subCategory.dropDownValue }}
              </div>
            </div>
          </div>
          <div class="row q-col-gutter-x-md q-mb-md">
            <div class="col-12 col-md-6">
              <div class="q-mb-xs">Version</div>
              <div class="text-black">
                {{ model.version ? model.version : "-" }}
              </div>
            </div>
            <div class="col-12 col-sm-6 col-md-6">
              <div class="q-mb-xs">Status</div>
              <div class="text-black">
                {{ model.statusText ? model.statusText : "-" }}
              </div>
            </div>
          </div>
          <div class="row q-col-gutter-x-md q-mb-md">
            <div class="col-12 col-sm-6 col-md-6">
              <div>
                <div class="q-mb-xs">Is Active?</div>
                <div class="text-black">
                  {{ model.isActive ? 'Active' : "InActive" }}
                </div>
              </div>
            </div>
          </div>
          <div class="row q-col-gutter-x-md q-mb-md hidden">
            <div class="col-12">
              <div class="q-mb-xs">Short Description</div>
              <div class="text-black">
                {{ model.shortDescription ? model.shortDescription : "-" }}
              </div>
            </div>
          </div>
          <div class="row q-col-gutter-x-md q-mb-md">
            <div class="col-xxl-12 col-lg-12 col-md-12 col-sm-12 col-xs-12">
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
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import _ from "lodash";
import { ref, watch } from "vue";
import { useDialogPluginComponent } from "quasar";

import sopProcessService from "../sopProcess.service";

const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();
defineEmits([...useDialogPluginComponent.emits]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const processId = props.id;

// Common variables
const loading = ref(true);

// Define model values
const model = ref({
  name: "",
  description: "",
  // shortDescription: "",
  category: {
    type: ""
  },
  subCategory: {
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

const getSOPProcessInDetailsById = (processId) => {
  loading.value = true;
  sopProcessService.getSOPProcessByIdInDetail(processId).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => processId, (newValue, oldValue) => {
  if (newValue) {
    getSOPProcessInDetailsById(processId);
  }
}, { immediate: true });
</script>
<style scoped>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
