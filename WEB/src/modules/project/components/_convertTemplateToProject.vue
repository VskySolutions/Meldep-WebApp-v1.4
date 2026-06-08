<template>
  <q-dialog ref="chatContainer" v-model="templateDialog" class="project-template-dialog customDialog dialog-scrollable-content" persistent position="right">
    <q-card style="width: 65vw;">
      <q-card-section class="flex items-center q-pa-sm bg-primary text-white">
        <h3 style="width:90%;">Convert Template To Project?</h3>
        <q-btn dense flat icon="o_close" class="float-end" @click="toggledialog" />
      </q-card-section>
      <q-card-section class="q-pa-md">
        <div class="row q-py-sm q-mb-xs">
          <div class="col-12">Template: <span class="text-primary" style="font-weight:600;">{{ props.templateName }}</span></div>
        </div>
        <div class="row full-width">
          <div class="col-8">
            <label class="q-mb-xs">Project Name<span class="required">*</span></label>
            <q-input
              v-model="model.name"
              outlined dense
              class="q-mr-sm"
              :error="v$.name.$error"
              :error-message="v$.name.$errors[0]?.$message"
            />
          </div>
          <formDate
            v-model="model.startDate"
            label="Start Date"
            :error="v$.startDate.$error"
            :error-message="v$.startDate.$errors[0]?.$message"
          />
        </div>
      </q-card-section>
      <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
        <q-btn color="grey-8" push outline label="Cancel" type="button" no-caps @click="toggledialog" />
        <q-btn color="primary" push outline label="Create" type="submit" no-caps :disable="IsDisabled" :loading="processing" @click="saveTemplateAsProject()" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, watch } from "vue";
import { notifyError, notifySuccess } from "src/assets/utils";
import useVuelidate from "@vuelidate/core";
import { isDate } from "validators/zw_validators.js";
import { required, helpers } from "@vuelidate/validators";

import projectService from "../projects.service";
import formDate from "src/components/form-inputs/_formDate.vue";

const processing = ref(false);
const templateDialog = ref(false);
const IsDisabled = ref(false);

const props = defineProps({ projectId: { type: String, default: "" }, templateName: { type: String, default: "" } });

const toggledialog = () => { templateDialog.value = !templateDialog.value; };

const today = new Date();
const mm = String(today.getMonth() + 1).padStart(2, "0");
const dd = String(today.getDate()).padStart(2, "0");
const yyyy = today.getFullYear();

const todaysDate = `${mm}/${dd}/${yyyy}`;

const model = ref({
  id: props.projectId,
  name: "",
  startDate: todaysDate,
  isTemplate: false
});

const rules = {
  name: { required: helpers.withMessage("Project name is required", required) },
  startDate: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const saveTemplateAsProject = async () => {
  processing.value = true;

  try {
    if (!model.value.id || !model.value.name || !model.value.startDate) {
      notifyError({ message: "Required Field Missing" });
      return;
    }

    if (!await v$.value.$validate()) {
      return;
    }

    await projectService.copyProjectAsTemplate(model.value);

    notifySuccess({ message: "Project Generated" });
    toggledialog();

  } catch (error) {
    console.error("Error in generating project template:", error);
    notifyError({ message: "An error occurred while generating the project template." });

  } finally {
    processing.value = false;
  }
};

watch(() => model.value.startDate, (newValue, oldValue) => {
  if (newValue.length > 0) {
    IsDisabled.value = false;
  } else {
    IsDisabled.value = true;
  }
}, { immediate: true });

watch(() => model.value.name, (newValue, oldValue) => {
  if (newValue.length > 0) {
    IsDisabled.value = false;
  } else {
    IsDisabled.value = true;
  }
}, { immediate: true });

</script>
