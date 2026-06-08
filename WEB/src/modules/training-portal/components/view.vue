<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw; max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">View Training</div>
        <q-btn v-close-popup icon="o_close" color="white" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Training Info</legend>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 col-sm-4 col-md-2">
                <div class="form-group">
                  <p class="text-primary">Name</p>
                </div>
              </div>
              <div class="col-12 col-sm-8 col-md-10">
                <div class="form-group">
                  <p class="text-black">{{ model.name }}</p>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 col-sm-4 col-md-2">
                <div class="form-group">
                  <p class="text-primary">URL/Link</p>
                </div>
              </div>
              <div class="col-12 col-sm-8 col-md-10">
                <div class="form-group">
                  <a
                    :href="model.url"
                    target="_blank"
                    class="text-primary text-decoration-underline break-url"
                  >
                    {{ model.url }}
                  </a>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mt-sm">
              <div class="col-12 col-sm-4 col-md-2">
                <div class="form-group">
                  <p class="text-primary">File</p>
                </div>
              </div>
              <div class="col-12 col-sm-8 col-md-10">
                <div class="form-group">
                  <div v-if="model.virtualPath">
                    <a
                      :href="model.virtualPath"
                      download
                      target="_blank"
                      rel="noopener noreferrer"
                      style="text-decoration: none; text-align: center; display: inline-block;"
                    >
                      <i class="fas fa-download" />
                      Download File
                    </a>
                    <!-- <a :href="baseURL + model.virtualPath" download style="width: 50%" :target='_blank'>Download File</a> -->
                  </div>
                  <div v-else class="text-black">
                    -
                  </div>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 col-sm-4 col-md-2">
                <div class="form-group">
                  <p class="text-primary">Assign To</p>
                </div>
              </div>
              <div class="col-12 col-sm-8 col-md-10">
                <div class="form-group text-black">
                  <span v-if="model.trainingPortalMappings && model.trainingPortalMappings.length">
                    {{ model.trainingPortalMappings
                      .map(mapping => mapping.employeeDesignationType.dropDownValue)
                      .join(', ') }}
                  </span>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 col-sm-12 col-md-12">
                <div class="form-group">
                  <p class="text-primary">Description</p>
                </div>
              </div>
              <div class="col-12 col-md-12">
                <div class="form-group">
                  <p class="text-black RichTextEditor" v-html="model.description ? model.description : '-'" />
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
import { useDialogPluginComponent } from "quasar";
import { ref, watch } from "vue";
import _ from "lodash";
import trainingService from "modules/training-portal/trainingPortal.service";
defineEmits([...useDialogPluginComponent.emits]);

const loading = ref(true);
const model = ref({
});

const props = defineProps({ id: { type: String, default: "" } });

const getTraining = () => {
  loading.value = true;
  trainingService.getTraining(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.name = resp.name;
    model.value.virtualPath = resp.file ? resp.file.virtualPath : "";
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getTraining();
  }
}, { immediate: true });

// function isImage (path) {
//   const imageExtensions = ["jpg", "jpeg", "png"];
//   const extension = path.split(".").pop().toLowerCase();
//   return imageExtensions.includes(extension);
// }

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.break-url {
  overflow-wrap: anywhere;
  word-break: break-all;
  white-space: normal;
}
</style>
