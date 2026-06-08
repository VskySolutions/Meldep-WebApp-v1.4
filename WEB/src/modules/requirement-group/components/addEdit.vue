<template>
  <q-dialog class="customDialog dialog-scrollable-content" ref="dialogRef" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw !important;max-width: 60vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Requirement Group</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
        <q-form greedy @submit.prevent.stop="onSubmit">
          <div class="q-pa-md cardTable">
            <div class="q-gutter-y-md">
                <fieldset>
                  <legend>Requirement Group Info</legend>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-sm-5 col-md-5">
                      <label class="label q-mb-xs text-black">Project Name<span class="required">*</span></label>
                      <div>
                        <q-select
                          v-model="model.projectId"
                          clearable
                          use-input
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          :options="projectList"
                          option-value="value"
                          option-label="text"
                          emit-value
                          map-options
                          @filter="getAllProjectListDropdownForFilter"
                          :error="v$.projectId.$error"
                          :error-message="v$.projectId.$errors[0]?.$message"
                          @blur="v$.projectId.$touch"
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
                    <div class="col-12 col-sm-7 col-md-7">
                      <label class="label q-mb-xs text-black">Name<span class="required">*</span></label>
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
                    <div class="col-12">
                      <div class="form-group">
                        <label class="label q-mb-xs text-black">Description</label>
                        <q-editor
                          v-model="model.description"
                          :dense="$q.screen.lt.md"
                          :toolbar="[
                            [
                              {
                                label: $q.lang.editor.align,
                                icon: $q.iconSet.editor.align,
                                fixedLabel: true,
                                list: 'only-icons',
                                options: ['left', 'center', 'right', 'justify']
                              },
                            ],
                            ['bold', 'italic', 'strike', 'underline'],
                            ['token', 'hr', 'link', 'custom_btn'],
                            [
                              {
                                label: $q.lang.editor.formatting,
                                icon: $q.iconSet.editor.formatting,
                                list: 'no-icons',
                                options: [
                                  'p',
                                  'h1',
                                  'h2',
                                  'h3',
                                  'h4',
                                  'h5',
                                  'h6',
                                  'code'
                                ]
                              },
                              'removeFormat'
                            ],
                            ['quote', 'unordered', 'ordered', 'outdent', 'indent'],

                            ['undo', 'redo'],
                            ['viewsource']
                          ]"

                          :fonts="{
                            arial: 'Arial',
                            arial_black: 'Arial Black',
                            comic_sans: 'Comic Sans MS',
                            courier_new: 'Courier New',
                            impact: 'Impact',
                            lucida_grande: 'Lucida Grande',
                            times_new_roman: 'Times New Roman',
                            verdana: 'Verdana'
                          }"
                        />
                      </div>
                    </div>
                  </div>
                </fieldset>
            </div>
          </div>
          <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
              <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
              <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
            </q-card-actions>
        </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import { notifySuccess, notifyError, getLocalStorage } from "assets/utils";
import _ from "lodash";
import useVuelidate from "@vuelidate/core";
import projectService from "modules/project/projects.service";
import requirementGroupsService from "modules/requirement-group/requirementGroup.service";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, projectIdAttr: { type: String, default: "" }, projectIdValue: { type: String, default: "" } });

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Common variables
const loading = ref(true);
const processing = ref(false);
const $q = useQuasar();

// local storage values
const localStorageKey = "editRequirementGroup";
const filterLocalStorage = getLocalStorage(localStorageKey);
const projectIds = filterLocalStorage ? filterLocalStorage.projectIds[0] : [];

// Define model values
const model = ref({
  projectId: props.projectIdAttr !== "" ? props.projectIdAttr : (props.projectIdValue !== "" ? props.projectIdValue : (projectIds !== "" ? projectIds : null)),
  name: "",
  description: ""
});

// Validation rules
const rules = {
  projectId: { required: helpers.withMessage("Project name is required", required) },
  name: { required: helpers.withMessage("Name is required", required), minLength: minLength(1), maxLength: maxLength(200) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// get requirement group details on edit mode
const getRequirementGroup = () => {
  loading.value = true;
  requirementGroupsService.getRequirementGroup(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.description = model.value.description ? model.value.description : "";
  }).finally(() => {
    loading.value = false;
  });
};

// Get all project list for dropdown
const projectList = ref([]);
const projectListFilter = ref([]);
function getAllProjectListForDropdown () {
  projectService.getAllProjectListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.name, value: item.id }));
    projectList.value = responseData;
    projectListFilter.value = responseData;
  });
}

// Search project for dropdown
function getAllProjectListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectList.value = projectListFilter.value;
    } else {
      projectList.value = projectListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    if (await v$.value.$validate()) {
      processing.value = true;
      requirementGroupsService.saveRequirementGroup(props.id, model.value).then((resp) => {
        notifySuccess({ message: "Requirement Group is saved successfully." });
        onDialogOK();
      });
    }
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getRequirementGroup();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  getAllProjectListForDropdown();
});

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
