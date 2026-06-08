<!-- eslint-disable vue/no-v-html -->
<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="min-width:80vw; max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div v-if="!isReleaseTrackingItems" class="text-h2 text-white">{{ props.id ? "Edit" : "Add" }} SOP Template</div>
        <div v-else class="text-h2 text-white">{{ model.name }}</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogCancel()" />
      </q-card-section>
      <q-separator />
      <q-form greedy class="q-pa-md">
        <fieldset>
          <legend>Template Info</legend>
          <div class="row q-col-gutter-x-md q-mb-sm">
            <div class="col-xxl-1 col-lg-1 col-md-1 col-sm-2 col-xs-12">
              <div class="q-mb-xs text-black">Version<span class="required">*</span></div>
              <q-input
                v-model="model.version"
                type="number"
                outlined
                stack-label
                hide-bottom-space
                :dense="true"
                :error="v$.version.$error"
                :error-message="v$.version.$errors[0]?.$message"
                @click="v$.version.$touch"
              />
            </div>
            <div class="col-xxl-4 col-lg-4 col-md-3 col-sm-9 col-xs-12">
              <div class="q-mb-xs text-black">Template Name<span class="required">*</span></div>
              <q-input
                v-model="model.name"
                outlined
                stack-label
                hide-bottom-space
                :dense="true"
                :error="v$.name.$error"
                :error-message="v$.name.$errors[0]?.$message"
                @click="v$.name.$touch"
              />
            </div><div class="col-xxl-1 col-lg-1 col-md-1 col-sm-2 col-xs-12">
              <div class="q-mb-xs text-black">Sort Order<span class="required">*</span>
              </div>
              <q-input
                v-model="model.sortOrder"
                type="number"
                outlined
                stack-label
                hide-bottom-space
                :dense="true"
                hint="Numbers only"
                :error="v$.sortOrder.$error"
                :error-message="v$.sortOrder.$errors[0]?.$message"
                @click="v$.sortOrder.$touch"
              />
            </div>
            <div class="col-xxl-6 col-lg-5 col-md-5 col-sm-12 col-xs-12">
              <div class="q-mb-xs text-black"><label>Description</label></div>
              <div class="form-group">
                <q-editor
                  v-model="model.description"
                  :dense="$q.screen.lt.md"
                  :toolbar="toolbar"
                  :fonts="fonts"
                />
              </div>
            </div>
            <div class="col-xxl-1 col-lg-1 col-md-2 col-sm-6 col-xs-12 flex column justify-between items-end">
              <div>
                <div class="q-mb-xs text-black"><label>Set As Active?</label></div>
                <div class="form-group">
                  <q-checkbox v-model="model.isActive" :dense="true" />
                </div>
              </div>
            </div>
          </div>
        </fieldset>
        <fieldset style="height: 58vh; overflow: auto;">
          <legend>Section & Items</legend>
          <div v-if="model.sopTemplateSections?.length === 0" class="text-center text-red">No data available</div>
          <div v-else class="row">
            <div class="col-12">
              <div v-for="(section, sectionIndex) in model.sopTemplateSections" :key="section.id" class="q-mb-lg" style="box-shadow: 0px 0px 10px 2px #e9e9e9; border-bottom: 1px solid;">
                <div class="row q-pa-sm q-mb-xs">
                  <div class="col-xxl-4 col-lg-4 col-md-4 col-sm-6 col-xs-12 q-pa-xs">
                    <div class="q-mb-xs text-black">Section Name<span class="required">*</span></div>
                    <span v-if="section.deleted" class="text-strike text-red">{{ section.name }}</span>
                    <q-input
                      v-else
                      v-model="section.name"
                      outlined
                      stack-label
                      hide-bottom-space
                      :error="!!v$.sopTemplateSections?.$each?.$response?.$errors?.[sectionIndex]?.name?.length"
                      :error-message="v$.sopTemplateSections?.$each?.$response?.$errors?.[sectionIndex]?.name?.[0]?.$message"
                    />
                  </div>
                  <div class="col-xxl-7 col-lg-7 col-md-7 col-sm-6 col-xs-10 q-pa-xs">
                    <div class="q-mb-xs text-black"><label>Description</label></div>
                    <div class="form-group">
                      <span v-if="section.deleted" class="text-strike text-red" v-html="section.description ?? '--'" />
                      <q-editor
                        v-else
                        v-model="section.description"
                        :dense="$q.screen.lt.md"
                        :toolbar="toolbar"
                        :fonts="fonts"
                      />
                    </div>
                  </div>
                  <div class="col-xxl-1 col-lg-1 col-md-1 col-sm-12 col-xs-2 q-pa-xs flex column justify-between items-end">
                    <q-icon
                      :name="section.deleted || section.deleted ? 'o_redo' : 'o_delete_outline'"
                      class="cursor-pointer"
                      color="red"
                      size="sm"
                      @click="onDeleteSection(section.isNew, model.sopTemplateSections, sectionIndex);"
                    >
                      <q-tooltip>{{ section.deleted || section.deleted ? 'Undo Section?' : 'Delete Section?' }}</q-tooltip>
                    </q-icon>
                    <q-btn icon="o_add" outline label="Add Item" no-caps class="text-primary btnRounded" size="sm" @click="onAddSOPTemplateSectionItem(section, 'start')" />
                  </div>
                </div>
                <div class="row q-pa-sm q-mb-xs">
                  <table class="SOPTemplate-Section full-width">
                    <thead>
                      <tr class="text-left fs-15">
                        <th style="width: 15%">Name<span class="required">*</span></th>
                        <th style="width: 30%">Description</th>
                        <th style="width: 15%">Input Type<span class="required">*</span></th>
                        <th class="text-center" style="width:5%">Required</th>
                        <th class="text-center" style="width:5%">Evidence</th>
                        <th style="width: 25%">Validation</th>
                        <th class="text-center">Action</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-if="section?.sopTemplateSectionItems?.length === 0">
                        <td colspan="7" class="text-center text-red">No data available</td>
                      </tr>
                      <tr v-for="(sectionItem, sectionItemIndex) in section.sopTemplateSectionItems" :key="sectionItem.id">
                        <td>
                          <span v-if="sectionItem.deleted || section.deleted" class="text-strike text-red">{{ sectionItem.name }}</span>
                          <q-input
                            v-else
                            v-model="sectionItem.name"
                            outlined
                            stack-label
                            hide-bottom-space
                            :error="!!v$.sopTemplateSections?.$each?.$response?.$errors?.[sectionIndex]?.sopTemplateSectionItems?.[0]?.$response?.$errors?.[sectionItemIndex]?.name?.length"
                            :error-message="v$.sopTemplateSections?.$each?.$response?.$errors?.[sectionIndex]?.sopTemplateSectionItems?.[0]?.$response?.$errors?.[sectionItemIndex]?.name?.[0]?.$message"
                          />
                        </td>
                        <td>
                          <span v-if="sectionItem.deleted || section.deleted" class="text-strike text-red">{{ sectionItem.description }}</span>
                          <q-input
                            v-else
                            v-model="sectionItem.description"
                            outlined
                            stack-label
                            hide-bottom-space
                          />
                        </td>
                        <td>
                          <span v-if="sectionItem.deleted || section.deleted" class="text-strike text-red">{{ sectionItem.inputType.dropDownText }}</span>
                          <formSingleSelectDropdown
                            v-else
                            v-model="sectionItem.inputTypeId"
                            :options="inputTypeDropdown.list.value"
                            :filter="inputTypeDropdown.filter"
                            :error="!!v$.sopTemplateSections?.$each?.$response?.$errors?.[sectionIndex]?.sopTemplateSectionItems?.[0]?.$response?.$errors?.[sectionItemIndex]?.inputTypeId?.length"
                            :error-message="v$.sopTemplateSections?.$each?.$response?.$errors?.[sectionIndex]?.sopTemplateSectionItems?.[0]?.$response?.$errors?.[sectionItemIndex]?.inputTypeId?.[0]?.$message"
                          />
                        </td>
                        <td class="text-center">
                          <q-checkbox v-model="sectionItem.isMandatory" :dense="true" :disable="sectionItem.deleted || section.deleted" />
                        </td>
                        <td class="text-center">
                          <q-checkbox v-model="sectionItem.isRequiredEvidence" :dense="true" :disable="sectionItem.deleted || section.deleted" />
                        </td>
                        <td>
                          <span v-if="sectionItem.deleted || section.deleted" class="text-strike text-red">{{ sectionItem.validationJson }}</span>
                          <q-input
                            v-else
                            v-model="sectionItem.validationJson"
                            outlined
                            stack-label
                            hide-bottom-space
                          />
                        </td>
                        <td class="text-center">
                          <q-icon
                            v-if="!section.deleted" :name="sectionItem.deleted || section.deleted ? 'o_redo' : 'o_delete_outline'"
                            class="cursor-pointer"
                            color="secondary"
                            size="sm"
                            @click="onDeleteSectionItem(sectionItem.isNew, section.sopTemplateSectionItems, sectionItemIndex);"
                          >
                            <q-tooltip>{{ sectionItem.deleted || section.deleted ? 'Undo Item?' : 'Delete Item?' }}</q-tooltip>
                          </q-icon>
                          <q-icon v-else name="o_delete_outline" class="cursor-pointer" color="secondary" size="sm" disabled />
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>
          <div class="row flex justify-end">
            <q-btn icon="o_add" outline label="Add Section" no-caps class="text-green btnRounded" size="sm" @click="onAddSOPTemplateSection(model.sopTemplateSections, 'end')" />
          </div>
        </fieldset>
        <div align="center" class="q-gutter-sm q-mt-md justify-center">
          <!-- CLOSE -->
          <q-btn
            color="grey-4"
            outline
            label="Close"
            class="text-grey-9 actionBtn"
            no-caps
            @click="onDialogCancel()"
          />
          <q-btn
            label="Save & Close"
            color="primary"
            class="actionBtn"
            :loading="processing"
            no-caps
            @click="onSubmit"
          />
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import _ from "lodash";
import { notifyError, notifySuccess } from "assets/utils";
import { useAuthStore } from "stores/auth";
import useVuelidate from "@vuelidate/core";
import { ref, onMounted, watch, computed } from "vue";
import { useQuasar, uid, useDialogPluginComponent } from "quasar";
import { required, helpers, numeric } from "@vuelidate/validators";

import sopTemplateService from "../sopTemplate.service";
import commonService from "src/services/common.service";

// SOP Change :- Shared Dropdowns
import sOPTemplateModule from "src/modules/sop-template/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();
defineEmits([...useDialogPluginComponent.emits]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const templateId = props.id;

// Common variables
const loading = ref(true);
const processing = ref(false);

// check login user role
const authStore = useAuthStore();
const user = authStore.user;

const inputTypeId = ref("");

// Define model values
const model = ref({
  id: uid(),
  siteId: user.siteId,
  name: "",
  sortOrder: 0,
  description: "",
  version: 1.0,
  isActive: true,
  sopTemplateSections: []
});

const getSOPTemplateInDetailsById = (templateId) => {
  loading.value = true;
  sopTemplateService.getSOPTemplateByIdInDetail(templateId).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.sopTemplateSections = model.value.sopTemplateSections.map(section => ({
      ...section,
      description: section.description ?? "",
      sopTemplateSectionItems: section.sopTemplateSectionItems?.map(item => ({
        ...item,
        description: item.description ?? ""
      }))
    }));
  }).finally(() => {
    loading.value = false;
  });
};

const createSectionItem = () => ({
  id: uid(),
  templateId: model.value.id,
  sectionId: "",
  inputTypeId: inputTypeId.value,
  name: "",
  description: "",
  isMandatory: false,
  isRequiredEvidence: false,
  sortOrder: 0,
  deleted: false,
  isNew: true
});

const createSection = () => ({
  id: uid(),
  templateId: model.value.id,
  name: "",
  description: "",
  sortOrder: 0,
  sopTemplateSectionItems: [createSectionItem()],
  isNew: true
});

const onAddSOPTemplateSection = (sectionList, position = "end") => {
  const section = createSection();

  if (position === "start") {
    sectionList.unshift(section);
  } else {
    sectionList.push(section);
  }
};

const onAddSOPTemplateSectionItem = (section, position = "end") => {
  if (!section) return;

  const item = createSectionItem();

  const list = (section.sopTemplateSectionItems ||= []);

  position === "start"
    ? list.unshift(item)
    : list.push(item);
};

const onDeleteSection = (isNew, sectionList, index) => {
  if (isNew) sectionList?.splice(index, 1);
  else sectionList[index].deleted = !sectionList[index].deleted;
};

const onDeleteSectionItem = (isNew, sectionItemList, index) => {
  if (isNew) sectionItemList?.splice(index, 1);
  else sectionItemList[index].deleted = !sectionItemList[index].deleted;
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  inputTypeDropdown
} = sOPTemplateModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Save & Next or Save & Close
// --------------------------------------------------------------------------------------------------------------------------------------------------
const rules = computed(() => ({
  name: {
    required: helpers.withMessage("Template Name is required", required)
  },
  sortOrder: {
    required: helpers.withMessage("Sort order is required", required, numeric)
  },
  version: {
    required: helpers.withMessage("Version is required", required)
  },
  sopTemplateSections: {
    // Wrap the section rules in helpers.forEach
    $each: helpers.forEach({
      name: {
        required: helpers.withMessage("Section Name is required", required)
      },
      sopTemplateSectionItems: {
        // Wrap the nested item rules in helpers.forEach again
        $each: helpers.forEach({
          name: {
            required: helpers.withMessage("Item Name is required", required)
          },
          inputTypeId: {
            required: helpers.withMessage("Input Type is required", required)
          }
        })
      }
    })
  }
}));

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const onSubmit = async () => {
  processing.value = true;

  try {
    v$.value.$touch(); // ensure all fields are validated
    const isValid = await v$.value.$validate();
    if (!isValid) { v$.value.$touch(); return; }

    await sopTemplateService.createUpdateSOPTemplate(model.value);
    onDialogOK();
    notifySuccess({ message: "Template Successfully " + (props.id ? "Updated" : "Created") });
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = false;
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------
onMounted(() => {
  inputTypeDropdown.load("InputType");
  commonService.getDropDownByTypeNameAndName("InputType", "CheckBox").then((resp) => {
    inputTypeId.value = resp;
  }).finally(() => {
    return inputTypeId.value;
  });
});

watch(() => templateId, (newValue, oldValue) => {
  if (newValue) {
    getSOPTemplateInDetailsById(templateId);
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
.SOPTemplate-Section table {
  font-family: Arial, Helvetica, sans-serif;
  border-collapse: collapse;
  width: 100%;
}

.SOPTemplate-Section  td, .SOPTemplate-Section th {
  border: 1px solid #ddd;
  padding: 8px;
}

.SOPTemplate-Section tr:nth-child(even){background-color: #f2f2f2;}

.SOPTemplate-Section tr:hover {background-color: #ddd;}

.SOPTemplate-Section th {
  padding-top: 7px;
  padding-bottom: 7px;
  text-align: left;
  background-color: #1b75ab;
  color: white;
  font-weight: 500;
}
</style>
