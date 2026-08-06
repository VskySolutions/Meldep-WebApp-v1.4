<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:65vw !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Question Answers</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <div class="row q-col-gutter-x-md q-mb-md">
                <formSingleSelectDropdown
                  v-model="model.projectId"
                  label="Project Name"
                  :options="projectNameDropdownSingleSelect.list.value"
                  :filter="projectNameDropdownSingleSelect.filter"
                  wrapper-class="col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12"
                  :error="v$.projectId.$error"
                  :error-message="v$.projectId.$errors[0]?.$message"
                />
                <formSingleSelectDropdown
                  v-model="model.requirementId"
                  label="Requirement"
                  :disable="!model.projectId"
                  :options="requirementByProjectModuleIdForDropdownSingleSelect.list.value"
                  :filter="requirementByProjectModuleIdForDropdownSingleSelect.filter"
                  wrapper-class="col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12"
                  :error="v$.requirementId.$error"
                  :error-message="v$.requirementId.$errors[0]?.$message"
                />
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="text-black">Title<span class="required">*</span></div>
                  <q-input
                    v-model="model.title"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :error="v$.title.$error"
                    :error-message="v$.title.$errors[0]?.$message"
                    @click="v$.title.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-12 col-md-12 col-lg-12">
                  <div class="text-black"><label>Description</label></div>
                  <div class="form-group">
                    <q-editor
                      v-model="model.description"
                      :disable="isReadOnlyMode"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
              </div>
              <q-card-actions align="center" class="q-gutter-sm justify-center">
                <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
              </q-card-actions>
            </fieldset>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import _ from "lodash";
import { notifySuccess, notifyError } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import { ref, onMounted, watch } from "vue";
import { useQuasar, useDialogPluginComponent } from "quasar";
import { required, helpers, maxLength } from "@vuelidate/validators";

import projectQuestionsAnswersService from "modules/project-questions-answers/projectQuestionsAnswers.service";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();
// defineEmits([...useDialogPluginComponent.emits]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

const $emit = defineEmits(["hide", "ok"]);
// Common variables
const loading = ref(true);
const processing = ref(false);
const isInitializing = ref(false);

// Define model values
const model = ref({
  id: "",
  title: "",
  projectId: "",
  requirementId: "",
  description: ""
});

// ==================================================================

const getQuestionAnswersInDetailsById = async (questionAnswersId) => {
  loading.value = true;
  isInitializing.value = true;

  try {
    const resp = await projectQuestionsAnswersService.getQuestionAnswersInDetailsById(questionAnswersId);

    model.value = _.cloneDeep(resp);
    model.value.projectId = resp.project?.id;

    await requirementByProjectModuleIdForDropdownSingleSelect.load("", model.value.projectId);

    model.value.requirementId = resp.requirement?.id;
    model.value.description = resp.description ?? "";
  } finally {
    isInitializing.value = false;
    loading.value = false;
  }
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectNameDropdownSingleSelect
} = projectModule();

const { requirementByProjectModuleIdForDropdownSingleSelect } = requirementModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Validation Rules
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Question Answers Info - Validation Rules
const rules = {
  projectId: { required: helpers.withMessage("Project is required", required) },
  requirementId: {
    required: helpers.withMessage("Requirement is required", required)
  },
  title: { required: helpers.withMessage("Title is required", required), maxLength: maxLength(500) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Submit form
// --------------------------------------------------------------------------------------------------------------------------------------------------

async function onSubmit () {
  processing.value = true;
  try {
    if (!await v$.value.$validate()) {
      return;
    }
    projectQuestionsAnswersService.saveQuestionAnswers(props.id, model.value).then(resp => {
      notifySuccess({ message: "Project question answer saved successfully." });
      $emit("ok");
      $emit("hide");
    });
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

watch(
  () => model.value.projectId,
  async (newValue) => {
    if (!isInitializing.value) {
      model.value.requirementId = null;
    }
    if (!newValue) return;

    await requirementByProjectModuleIdForDropdownSingleSelect.load("", newValue);
  }, { immediate: true }
);

watch(() => props.id, async (newValue, oldValue) => {
  if (newValue) {
    await getQuestionAnswersInDetailsById(newValue);
  }
}, { immediate: true });

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  await projectNameDropdownSingleSelect.load();

  if (model.value.projectId) {
    requirementByProjectModuleIdForDropdownSingleSelect.load(model.value.projectId);
  }
});

</script>
<style>
.ellipsis-cell {
  max-width: 260px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.same-size-btn {
  min-width: 150px;
  height: 50px;
}
</style>
