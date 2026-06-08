<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:25vw !important; max-width: 25vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="width: 98%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">Task Description</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset class="q-mb-lg">
            <legend>Description</legend>
            <div class="q-pa-sm text-black">
              <div class="row no-wrap items-start justify-between">
                <div class="col-11 RichTextEditor" style="white-space: pre-wrap; word-break: break-word;">
                  <p v-if="model.description" v-html="model.description" />
                  <p v-else class="text-grey-7">No Description available</p>
                </div>
                <q-icon
                  v-if="model.description"
                  name="o_content_copy"
                  size="sm"
                  class="q-ml-sm cursor-pointer"
                  @click="copyToClipboard(model.description)"
                >
                  <q-tooltip>Copy Description</q-tooltip>
                </q-icon>
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
import { notifySuccess, notifyError } from "assets/utils";
import _ from "lodash";
import taskService from "modules/project-tasks/projectTasks.service";

// Common variables
const loading = ref(true);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Define model values
const model = ref({
  description: ""
});
// On page rendering
onMounted(() => {
  getProjectTask();
});

// get project details on edit mode
const getProjectTask = () => {
  loading.value = true;
  taskService.getProjectTaskDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.description = resp.description ? resp.description : "";
  }).finally(() => {
    loading.value = false;
  });
};
// function copyToClipboard (text) {
//   navigator.clipboard.writeText(text);
//   notifySuccess({ message: "Copied" });
// }
function copyToClipboard (htmlText) {
  const tempDiv = document.createElement("div");
  tempDiv.innerHTML = htmlText || "";
  const plainText = tempDiv.textContent || "";
  navigator.clipboard.writeText(plainText).then(() => {
    notifySuccess({ message: "Copied" });
  }).catch(() => {
    notifyError({ message: "Failed to copy" });
  });
}

</script>
