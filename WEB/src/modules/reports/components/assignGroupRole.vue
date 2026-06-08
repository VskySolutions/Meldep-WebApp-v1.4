<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white q-mr-lg">{{ id ? "Edit" : "Add" }} Role To Report Group</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Assign Role To Report Group Info</legend>
              <div class="row q-col-gutter-x-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="q-mb-xs text-black">Role Name<span class="required">*</span></label>
                  <div class="col-12 col-sm-6 col-md-6">
                    <q-select
                      v-model="model.siteRoleId"
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="siteRolesList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      :error="v$.siteRoleId.$error"
                      :error-message="v$.siteRoleId.$errors[0]?.$message"
                      @filter="getAllSitesRoleListForFilter"
                      @blur="v$.siteRoleId.$touch"
                    >
                      <template #option="{ itemProps, opt, }">
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
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="q-mb-xs text-black">Report Group Name<span class="required">*</span></label>
                  <div class="col-12 col-sm-6 col-md-6">
                    <q-select
                      v-model="model.reportGroupIds"
                      outlined
                      push
                      multiple
                      clearable
                      use-input
                      use-chips
                      transition-show="jump-up"
                      transition-hide="jump-up"
                      hide-bottom-space
                      input-debounce="0"
                      :dense="true"
                      :options="reportGroupList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      :popup-content-class="customPopupContentClass"
                      :error="v$.reportGroupIds.$error"
                      :error-message="v$.reportGroupIds.$errors[0]?.$message"
                      @filter="getAllReportGroupListForFilter"
                      @blur="v$.reportGroupIds.$touch"
                    >
                      <template #option="{ itemProps, opt, selected, toggleOption }">
                        <q-item v-bind="itemProps">
                          <q-item-section>
                            <div class="row q-col-gutter-x-md items-center">
                              <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                              <span>{{ opt.text }}</span>
                            </div>
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
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
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import useVuelidate from "@vuelidate/core";
import { notifySuccess } from "assets/utils";
import { required, helpers } from "@vuelidate/validators";

import commonService from "services/common.service";
import sitesService from "modules/sites/site.service";
import reportService from "modules/reports/reports.service";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const processing = ref(false);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel, onDialogOK } = useDialogPluginComponent();

// Define model values
const model = ref({
  siteRoleId: "",
  reportGroupIds: []
});

// Validation rules
const rules = {
  siteRoleId: { required: helpers.withMessage("Role is required", required) },
  reportGroupIds: { required: helpers.withMessage("Report Group is required", required) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Get all site role list for dropdown
const siteRolesList = ref([]);
const siteRolesListFilter = ref([]);
function getAllSitesRoleListForDropdown () {
  sitesService.getAllSitesRoleListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.applicationRole.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    siteRolesList.value = responseData;
    siteRolesListFilter.value = responseData;
  });
}

// Search site role for dropdown
function getAllSitesRoleListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      siteRolesList.value = siteRolesListFilter.value;
    } else {
      siteRolesList.value = siteRolesListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all report group list for dropdown
const reportGroupList = ref([]);
const reportGroupListFilter = ref([]);
function getAllReportGroupListForDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    reportGroupList.value = responseData;
    reportGroupListFilter.value = responseData;
  });
}

// Search  report group for dropdown
function getAllReportGroupListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      reportGroupList.value = reportGroupListFilter.value;
    } else {
      reportGroupList.value = reportGroupListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Submit form
const onSubmit = async () => {
  if (await v$.value.$validate()) {
    processing.value = true;
    reportService.saveGroupRole(props.id, model.value).then((resp) => {
      notifySuccess({ message: "Role assigned successfully." });
      onDialogOK();
    }).finally(() => {
      processing.value = false;
    });
  }
};

// On page rendering
onMounted(() => {
  getAllSitesRoleListForDropdown();
  getAllReportGroupListForDropDown("Report Group");
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
