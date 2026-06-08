<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="min-width:80vw; max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">View SOP Template</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogCancel()" />
      </q-card-section>
      <q-separator />
      <q-form greedy class="q-pa-md">
        <fieldset>
          <legend>Template Info</legend>
          <div class="row q-col-gutter-x-md q-mb-sm">
            <div class="col-xxl-1 col-lg-1 col-md-1 col-sm-2 col-xs-12">
              <div class="q-mb-xs">Version</div>
              <div class="text-black">
                {{ model.version ? model.version : "-" }}
              </div>
            </div>
            <div class="col-xxl-4 col-lg-4 col-md-3 col-sm-9 col-xs-12">
              <div class="q-mb-xs">Template Name</div>
              <div class="text-black">
                {{ model.name ? model.name : "-" }}
              </div>
            </div>
            <div class="col-xxl-1 col-lg-1 col-md-1 col-sm-2 col-xs-12">
              <div class="q-mb-xs">SortOrder</div>
              <div class="text-black">
                {{ model.sortOrder ? model.sortOrder : "-" }}
              </div>
            </div>
            <div class="col-xxl-6 col-lg-5 col-md-5 col-sm-12 col-xs-12">
              <div class="q-mb-xs">Description</div>
              <div class="text-black RichTextEditor">
                <p v-html="model.description ? model.description : '-'" />
              </div>
            </div>
            <div class="col-xxl-1 col-lg-1 col-md-2 col-sm-6 col-xs-12 flex column justify-between items-end">
              <div>
                <div class="q-mb-xs">Is Active?</div>
                <div class="text-black">
                  {{ model.isActive ? 'Active' : "InActive" }}
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
              <div v-for="(section) in model.sopTemplateSections" :key="section.id" class="q-mb-lg" style="box-shadow: 0px 0px 10px 2px #e9e9e9; border-bottom: 1px solid;">
                <div class="row q-pa-sm q-mb-xs">
                  <div class="col-xxl-4 col-lg-4 col-md-4 col-sm-6 col-xs-12 q-pa-xs">
                    <div class="q-mb-xs">Section Name</div>
                    <div class="text-black">
                      {{ section.name ? section.name : "-" }}
                    </div>
                  </div>
                  <div class="col-xxl-7 col-lg-7 col-md-7 col-sm-6 col-xs-10 q-pa-xs">
                    <div class="q-mb-xs">Description</div>
                    <div class="text-black RichTextEditor">
                      <p v-html="section.description ? section.description : '-'" />
                    </div>
                  </div>
                </div>
                <div class="row q-pa-sm q-mb-xs">
                  <table class="SOPTemplate-Section full-width">
                    <thead>
                      <tr class="text-left fs-15">
                        <th style="width: 15%">Name</th>
                        <th style="width: 30%">Description</th>
                        <th style="width: 15%">Input Type</th>
                        <th class="text-center" style="width:5%">Required</th>
                        <th class="text-center" style="width:5%">Evidence</th>
                        <th style="width: 25%">Validation</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-if="section?.sopTemplateSectionItems?.length === 0">
                        <td colspan="7" class="text-center text-red">No data available</td>
                      </tr>
                      <tr v-for="(sectionItem) in section.sopTemplateSectionItems" :key="sectionItem.id">
                        <td>
                          <span class="fs-13 text-black">{{ sectionItem.name }}</span>
                        </td>
                        <td>
                          <span class="fs-13 text-black RichTextEditor">
                            <p v-html="sectionItem.description ? sectionItem.description : '-'" />
                          </span>
                        </td>
                        <td>
                          <span class="fs-13 text-black">{{ sectionItem.inputType.dropDownValue }}</span>
                        </td>
                        <td class="text-center">
                          <q-icon
                            :name="sectionItem.isMandatory ? 'o_check_circle' : 'o_cancel'"
                            :color="sectionItem.isMandatory ? 'green' : 'grey'"
                            size="20px"
                          />
                        </td>

                        <td class="text-center">
                          <q-icon
                            :name="sectionItem.isRequiredEvidence ? 'o_check_circle' : 'o_cancel'"
                            :color="sectionItem.isRequiredEvidence ? 'green' : 'grey'"
                            size="20px"
                          />
                        </td>
                        <td>
                          <span class="fs-13 text-black">{{ sectionItem.validationJson }}</span>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
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
import { useAuthStore } from "stores/auth";
import { ref, watch } from "vue";
import { uid, useDialogPluginComponent } from "quasar";

import sopTemplateService from "../sopTemplate.service";

const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();
defineEmits([...useDialogPluginComponent.emits]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const templateId = props.id;

// Common variables
const loading = ref(true);

// check login user role
const authStore = useAuthStore();
const user = authStore.user;

// Define model values
const model = ref({
  id: uid(),
  siteId: user.siteId,
  name: "",
  description: "",
  inputType: {
    dropDownText: ""
  },
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
