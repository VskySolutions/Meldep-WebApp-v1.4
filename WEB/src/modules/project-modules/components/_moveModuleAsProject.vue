<template>
  <q-dialog ref="dialogRef" class="customDialog DialogContainer" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:55vw !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Move module as project</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable']">
          <div class="q-gutter-y-md">
            <fieldset>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-12">
                  <label class="label q-mb-xs text-black">Project Name<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.name"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :error="v$.name.$error"
                      :error-message="v$.name.$errors[0]?.$message"
                      @blur="v$.name.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-12">
                  <q-banner class="bg-orange-1 text-black" rounded>
                    <template v-slot:avatar>
                      <q-icon name="o_warning" color="orange" />
                    </template>
                    <div class="text-weight-medium">
                      This action will move the <b>Project Module</b> as a <b>new Project</b>.
                    </div>
                    <div class="q-mt-xs">
                      The following modules will also be updated automatically:
                    </div>
                    <ul class="q-mt-sm">
                      <li>Project Module</li>
                      <li>Project Task</li>
                      <li>Project Task Activities</li>
                      <li>Daily Planner</li>
                      <li>Timesheets</li>
                    </ul>
                  </q-banner>
                </div>
              </div>
              <q-banner class="bg-red-1 text-black q-mb-md q-mt-sm" rounded>
                <q-icon name="o_priority_high" color="red" class="q-mr-sm" />
                Type <b>"CONFIRM"</b> below to proceed with moving this module as a project.
              </q-banner>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4">
                  <div>
                    <q-input
                      v-model="model.confirmMessage"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      placeholder="Enter CONFIRM text"
                      :error="v$.confirmMessage.$error"
                      :error-message="v$.confirmMessage.$errors[0]?.$message"
                      @blur="v$.confirmMessage.$touch"
                      @paste.prevent
                      @contextmenu.prevent
                      @copy.prevent
                      @cut.prevent
                      @drop.prevent
                    />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" :disable="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref } from "vue";
import { notifySuccess, notifyWarning } from "assets/utils";
import { required, helpers } from "@vuelidate/validators";
import useVuelidate from "@vuelidate/core";
import projectModuleService from "modules/project-modules/projectModules.service";

// define props
const props = defineProps({ id: { type: String, default: "" }, name: { type: String, default: "" }, projectId: { type: String, default: "" } });

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// Common variables
const processing = ref(false);
const projectId = props.projectId;
const projectModuleId = props.id;
const projectModuleName = props.name;

// Define model values
const model = ref({
  id: projectModuleId,
  name: projectModuleName,
  projectId: projectId
});

// validation rule
const rule = {
  name: { required: helpers.withMessage("Project name is required", required) },
  confirmMessage: { required: helpers.withMessage("Confirm is required", required) }
};

// Validate rules
const v$ = useVuelidate(rule, model, { $lazy: true, $autoDirty: true });

const closeDialog = () => {
  if (dialogRef.value) {
    dialogRef.value.hide();
  }
};

// Submit form
const onSubmit = async () => {
  try {
    if (model.value.confirmMessage !== "CONFIRM") {
      notifyWarning({ message: "Please type 'CONFIRM' to proceed." });
      return;
    }
    if (processing.value) {
      notifyWarning({ message: "Double click not allowed. Please wait..." });
      return; // stop further submit
    }
    if (await v$.value.$validate()) {
      processing.value = true;
      projectModuleService.moveProjectModuleAsProject(model.value).then((resp) => {
        notifySuccess({ message: "Project module moved as project successfully." });
        closeDialog();
      });
    }
  } catch (error) {
    console.error("Error in submitting the project:", error);
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 3000);
  }
};
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_projectModule .q-select__dropdown-icon{
  display: none;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
</style>
