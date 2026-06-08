<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important; max-width: 50vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Ad Channel</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Ad Channel Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="label q-mb-xs text-black">Ad Channel Number</label>
                  <div>
                    <q-input v-model="model.channelNumber" outlined stack-label hide-bottom-space :dense="true" readonly="readonly" />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="label q-mb-xs text-black">Project Name<span class="required">*</span></label>
                  <div>
                    <q-select
                      v-model="model.projectId" use-input outlined stack-label hide-bottom-space :dense="true"
                      :options="projectList" option-value="value" option-label="text" emit-value map-options :error="v$.projectId.$error"
                      :error-message="v$.projectId.$errors[0]?.$message" @filter="projectListForFilter" @blur="v$.projectId.$touch" @update:model-value="getProject(model.projectId)"
                    >
                      <template #option="{ itemProps, opt }">
                        <q-item v-bind="itemProps">
                          <q-item-section>
                            <div class="row q-col-gutter-x-md items-center">
                              <span>{{ opt.text }}</span>
                            </div>
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
                  </div>
                </div>
                <div class="col-12 col-sm-6 col-md-6 edit_adModule">
                  <label class="label q-mb-xs text-black">Client Name<span class="required">*</span></label>
                  <div>
                    <q-input v-model="model.customerName" outlined hide-bottom-space readonly="readonly" />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="label q-mb-xs text-black">Channel Name<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.name" outlined stack-label hide-bottom-space :dense="true"
                      :error="v$.name.$error" :error-message="v$.name.$errors[0]?.$message" @blur="v$.name.$touch"
                    />
                  </div>
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="label q-mb-xs text-black">Group member count<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.groupMemberCount"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      maxlength="9"
                      :error="v$.groupMemberCount.$error"
                      :error-message="v$.groupMemberCount.$errors[0]?.$message"
                      @blur="v$.groupMemberCount.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="form-group">
                    <label class="label q-mb-xs text-black">Description</label>
                    <q-editor
                      v-model="model.description"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" :disable="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import projectService from "modules/project/projects.service";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import _ from "lodash";
import { notifySuccess } from "assets/utils";
import adPostChannelService from "modules/marketing-ad-post-channel/marketingAdPostChannel.service";

// SOP Change :- Shared Dropdowns
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// Common variables
const loading = ref(true);
const processing = ref(false);
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Define model values
const model = ref({
  channelNumber: "",
  name: "",
  description: "",
  projectId: "",
  customerId: ""
});

// get Ad Post Channel details on edit mode
const getAdPostChannelDetails = () => {
  loading.value = true;
  adPostChannelService.getAdPostChannelDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.groupMemberCount = Number(model.value.groupMemberCount);
  }).finally(() => {
    loading.value = false;
  });
};

// get Channel Number on edit mode
const getChannelNumber = () => {
  loading.value = true;
  adPostChannelService.getChannelNumber().then((resp) => {
    model.value.channelNumber = resp;
  }).finally(() => {
    loading.value = false;
  });
};

// get project details on edit mode
const getProject = (projectId) => {
  const project = projectList.value.find(x => x.value === projectId);

  if (project) {
    model.value.customerId = project.customerId;
    model.value.customerName = project.customerName;
  }
};

// -------------------------------------------------------------------------
// DropDown
// -------------------------------------------------------------------------
// Get all project list for dropdown
const projectList = ref([]);
const projectListOptions = ref([]);
function getAllProjectListForDropdown () {
  projectService.getAllProjectListForDropdown().then((resp) => {
    const responseData = resp
      .map((item) => ({ text: item.name, value: item.id, customerId: item.customer.id, customerName: item.customer.name }));
    projectList.value = responseData;
    projectListOptions.value = responseData;
    if (model.value.projectId) {
      getProject(model.value.projectId);
    }
  });
}

// Search project for dropdown
function projectListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectList.value = projectListOptions.value;
    } else {
      projectList.value = projectListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}
// -------------------------------------------------------------------------------------------------------

// Validation rules
const rules = {
  projectId: { required: helpers.withMessage("Project is required", required) },
  name: { required: helpers.withMessage("Channel Name is required", required), minLength: minLength(1), maxLength: maxLength(200) },
  groupMemberCount: {
    required: helpers.withMessage("Group Member Count is required", required),
    numeric: helpers.withMessage("Only numbers allowed", val => /^[0-9]+$/.test(val))
  }
};
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Submit form
const onSubmit = async () => {
  try {
    const isValid = await v$.value.$validate();
    if (!isValid) return;

    processing.value = true;
    await adPostChannelService.saveAdPostChannel(props.id, model.value);
    notifySuccess({ message: "Ad Channel is saved successfully." });
    onDialogOK();
  } catch (error) {
    console.error("Error in submitting:", error);
  } finally {
    processing.value = false;
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getAdPostChannelDetails();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  getAllProjectListForDropdown();
  getChannelNumber();
});
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_adModule .q-select__dropdown-icon{
  display: none;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
</style>
