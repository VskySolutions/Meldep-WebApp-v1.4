<template>
  <q-dialog ref="chatContainer" v-model="templateDialog" class="project-template-dialog customDialog dialog-scrollable-content" persistent position="right">
    <q-card style="width: 65vw;">
      <q-card-section class="flex items-center q-pa-sm bg-primary text-white">
        <h3 style="width:90%;">Convert Project To Template?</h3>
        <q-btn dense flat icon="o_close" class="float-end" @click="toggleDilog" />
      </q-card-section>
      <q-card-section class="q-pa-md">
        <div class="row q-py-sm q-mb-xs">
          <div class="col-8">Project: <span class="text-primary" style="font-weight:600;">{{ props.projectName }}</span></div>
          <div class="col-4">Started On: <span class="text-primary" style="font-weight:600;">{{ props.startDate }}</span></div>
        </div>
        <div class="row full-width">
          <div class="col-12">
            <label class="q-mb-xs">Template Name<span class="required">*</span></label>
            <q-input
              v-model="model.name"
              outlined dense
              class="q-mr-sm"
              :rules="[ val => !!val || 'Template name is required']"
            />
          </div>
        </div>
      </q-card-section>
      <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
        <q-btn color="grey-8" push outline label="Cancel" type="button" no-caps @click="toggleDilog" />
        <q-btn color="primary" push outline label="Create" type="submit" no-caps :disable="IsDisabled" :loading="processing" @click="saveProjectTemplate()" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, watch } from "vue";
import { notifyError, notifySuccess } from "src/assets/utils";

import projectService from "../projects.service";

const processing = ref(false);
const templateDialog = ref(false);
const IsDisabled = ref(true);
const toggleDilog = () => { templateDialog.value = !templateDialog.value; };

const props = defineProps({ projectId: { type: String, default: "" }, projectName: { type: String, default: "" }, startDate: { type: String, default: "" } });

const model = ref({
  id: props.projectId,
  name: props.projectName,
  isTemplate: true
});

const saveProjectTemplate = async () => {
  processing.value = true;

  try {
    if (!model.value.id || !model.value.name) {
      notifyError({ message: "Required Field Missing" });
      return;
    }

    await projectService.copyProjectAsTemplate(model.value);

    notifySuccess({ message: "Project Template Generated" });
    toggleDilog();

  } catch (error) {
    console.error("Error in generating project template:", error);
    notifyError({ message: "An error occurred while generating the project template." });

  } finally {
    processing.value = false;
  }
};

watch(() => model.value.name, (newValue, oldValue) => {
  if (newValue.length > 0) {
    IsDisabled.value = false;
  } else {
    IsDisabled.value = true;
  }
}, { immediate: true });

</script>
