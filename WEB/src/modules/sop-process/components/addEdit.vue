<!-- eslint-disable vue/no-v-html -->
<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="min-width:80vw; max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ props.id ? "Edit" : "Add" }} SOP Process</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogCancel()" />
      </q-card-section>
      <q-separator />
      <q-form greedy class="q-pa-md">
        <fieldset>
          <legend>Process Info</legend>
          <div class="row q-col-gutter-x-md q-mb-md">
            <div class="col-xxl-12 col-lg-12 col-md-12">
              <div class="q-mb-xs text-black">Title<span class="required">*</span></div>
              <q-input
                v-model="model.title"
                outlined
                stack-label
                hide-bottom-space
                :dense="true"
                maxlength="300"
                :readonly="isReadOnlyMode"
                hint="The maximum length allowed is 300."
                :error="v$.title.$error"
                :error-message="v$.title.$errors[0]?.$message"
                @click="v$.title.$touch"
              />
            </div>
          </div>
          <div class="row q-col-gutter-x-md q-mb-md">
            <div class="col-xxl-12 col-lg-12 col-md-12">
              <div class="q-mb-xs text-black">Purpose<span class="required">*</span></div>
              <q-input
                v-model="model.purpose"
                outlined
                stack-label
                hide-bottom-space
                :dense="true"
                maxlength="500"
                :readonly="isReadOnlyMode"
                hint="Maximum 500 characters allowed. Use comma-separated values."
                :error="v$.purpose.$error"
                :error-message="v$.purpose.$errors[0]?.$message"
                @click="v$.purpose.$touch"
              />
            </div>
          </div>
          <div class="row q-col-gutter-x-md q-mb-md">
            <formSingleSelectDropdown
              v-model="model.categoryId"
              label="Category"
              :options="sopProcessCategoryDropdownSingleSelect.list.value"
              :filter="sopProcessCategoryDropdownSingleSelect.filter"
              :readonly="isReadOnlyMode"
              :error="v$.categoryId.$error"
              :error-message="v$.categoryId.$errors[0]?.$message"
              @update:model-value="getSubCategoriesByCategoryId(model.categoryId)"
            />
            <formSingleSelectDropdown
              v-model="model.subCategoryId"
              label="Subcategory"
              :options="sopProcessSubCategoryDropdownSingleSelect.list.value"
              :filter="sopProcessSubCategoryDropdownSingleSelect.filter"
              :readonly="isReadOnlyMode"
              :error="v$.subCategoryId.$error"
              :error-message="v$.subCategoryId.$errors[0]?.$message"
            />
            <!-- <div class="col-12 col-sm-6 col-md-4">
              <div class="row items-end no-wrap full-width">
                <div class="col">
                <formSingleSelectDropdown
                  v-if="props.id"
                  v-model="model.statusId"
                  label="Status"
                  :options="sopProcessStatusDropdownSingleSelect.list.value"
                  :filter="sopProcessStatusDropdownSingleSelect.filter"
                  readonly
                  :error="v$.statusId.$error"
                  :error-message="v$.statusId.$errors[0]?.$message"
                />
                </div>
                <div v-if="model.statusId && props.id" class="q-pl-sm q-pb-sm flex flex-center">
                  <q-icon
                    name="o_history"
                    class="cursor-pointer q-ml-sm"
                    size="xs"
                    @click.stop="onSOPProcessStatusLog(model.id)"
                  >
                    <q-tooltip>Data Change Log</q-tooltip>
                  </q-icon>
                </div>
              </div>
            </div> -->
            <div class="col-xxl-2 col-lg-2 col-md-2 col-sm-2 col-xs-12">
              <div class="label text-black">Version<span class="required">*</span></div>
              <q-input
                v-model="model.version"
                outlined
                stack-label
                hide-bottom-space
                :dense="true"
                :readonly="isReadOnlyMode"
                :error="v$.version.$error"
                :error-message="v$.version.$errors[0]?.$message"
                @click="v$.version.$touch"
              />
            </div>
            <div class="col-xxl-2 col-lg-2 col-md-2 col-sm-2 col-xs-12">
              <div>
                <div class="text-black"><label>Set As Active?</label></div>
                <div class="form-group">
                  <q-checkbox
                    v-model="model.isActive"
                    :disable="isReadOnlyMode"
                    :dense="true"
                  />
                </div>
              </div>
            </div>
          </div>
          <div class="row q-col-gutter-x-md q-mb-md">
            <div class="col-xxl-6 col-lg-6 col-md-6 col-sm-12 col-xs-12 hidden">
              <div>
                <div class="q-mb-xs text-black">Short Description</div>
                <div class="form-group">
                  <q-input
                    v-model="model.shortDescription"
                    outlined
                    maxlength="500"
                    autogrow
                    hint="The maximum length allowed is 500."
                    :readonly="isReadOnlyMode"
                  />
                </div>
              </div>
            </div>
            <div class="col-xxl-12 col-lg-12 col-md-12 col-sm-12 col-xs-12">
              <div class="q-mb-xs text-black">Process Description<span class="required">*</span></div>
              <div class="form-group">
                <q-editor
                  v-model="model.description"
                  :dense="$q.screen.lt.md"
                  :toolbar="toolbar"
                  :fonts="fonts"
                  :readonly="isReadOnlyMode"
                  :disable="isReadOnlyMode"
                  :error="v$.description.$error"
                  :error-message="v$.description.$errors[0]?.$message"
                  @blur="v$.description.$touch"
                />
              </div>
            </div>
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
          <!-- APPROVER BUTTON -->
          <template v-if="canApprove">
            <q-btn
              color="positive"
              label="Approve"
              class="actionBtn"
              :loading="processingSubmit"
              no-caps
              @click="onSubmit('approve')"
            />
          </template>

          <!-- EDITOR / OTHER USERS -->
          <template v-else-if="canAdd && (!props.id || canEditDraft)">
            <q-btn
              label="Save & Close"
              color="primary"
              class="actionBtn"
              :loading="processingSave"
              no-caps
              :disable="isReadOnlyMode"
              @click="onSubmit('save')"
            />
            <q-btn
              color="primary"
              label="Save & Submit"
              class="actionBtn"
              :loading="processingSubmit"
              no-caps
              :disable="isReadOnlyMode"
              @click="onSubmit('submit')"
            />
          </template>
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
import { useQuasar, useDialogPluginComponent } from "quasar";
import { required, helpers, numeric } from "@vuelidate/validators";

import sopProcessService from "../sopProcess.service";

// SOP Change :- Shared Dropdowns
import sOPProcessModule from "src/modules/sop-process/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

// SOP Change :- Shared Project Dialogs
// import {
//   onSOPProcessStatusLog
// } from "src/modules/sop-process/utils/dialogs.js";

const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();
defineEmits([...useDialogPluginComponent.emits]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const processId = props.id;

// Common variables
const loading = ref(true);
const processingSave = ref(false);
const processingSubmit = ref(false);

// Define model values
const model = ref({
  description: "",
  isActive: true
});

// check login user role
const authStore = useAuthStore();
const user = authStore.user;
const editorRoles = ["admin", "site-super-admin", "system-super-admin", "sop editor"];
const approverRoles = ["sop approver"];

const hasEditorRole = user?.roles?.some(r => editorRoles.includes(r));
const hasApproverRole = user?.roles?.some(r => approverRoles.includes(r));

const role =
  hasEditorRole && hasApproverRole
    ? "both"
    : hasApproverRole
      ? "approver"
      : hasEditorRole
        ? "editor"
        : "";

// current status
const currentStatus = computed(() =>
  model.value.statusText?.toLowerCase() || ""
);

// Add Mode Permission
const canAdd = computed(() =>
  role === "editor" || role === "both"
);

// Approver can only open submitted items for approval
const canApprove = computed(() =>
  !!props.id &&
  (role === "approver" || role === "both") &&
  currentStatus.value === "submitted"
);

// Editor can edit only draft
const canEditDraft = computed(() =>
  !!props.id &&
  (role === "editor" || role === "both") &&
  currentStatus.value === "draft"
);

// field ReadOnly Logic
const isReadOnlyMode = computed(() => {
  // Add mode
  if (!props.id) {
    return !canAdd.value;
  }

  // Draft editable by editor/both
  if (canEditDraft.value) {
    return false;
  }

  // Submitted editable only by approver/both
  if (canApprove.value) {
    return false;
  }

  // Approved / Published / Archived = locked
  return true;
});

const getSOPProcessInDetailsById = (ProcessId) => {
  loading.value = true;
  sopProcessService.getSOPProcessByIdInDetail(ProcessId).then((resp) => {
    model.value = _.cloneDeep(resp);
    sopProcessSubCategoryDropdownSingleSelect.load(resp.category.id);
    model.value.categoryId = resp.category.id;
    model.value.subCategoryId = resp.subCategory.id;
  }).finally(() => {
    loading.value = false;
  });
};

function getSubCategoriesByCategoryId (categoryId) {
  model.value.subCategoryId = "";
  sopProcessSubCategoryDropdownSingleSelect.load(categoryId);
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  sopProcessCategoryDropdownSingleSelect,
  sopProcessSubCategoryDropdownSingleSelect,
  sopProcessStatusDropdownSingleSelect
} = sOPProcessModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Save & Next or Save & Close
// --------------------------------------------------------------------------------------------------------------------------------------------------

const requiredEditor = helpers.withMessage(
  "Process description is required",

  (value) => {
    if (!value) return false;

    const text = stripHtml(value);
    const imageExists = hasImage(value);
    return text.length > 0 || imageExists;
  }
);

const rules = computed(() => ({
  title: {
    required: helpers.withMessage("Title is required", required)
  },
  categoryId: {
    required: helpers.withMessage("Category is required", required)
  },
  subCategoryId: {
    required: helpers.withMessage("Sub Category is required", required)
  },
  statusId: {
    required: helpers.withMessage("Status is required", required)
  },
  description: { requiredEditor },
  purpose: {
    required: helpers.withMessage("Purpose is required", required)
  },
  version: {
    required: helpers.withMessage("Version is required", required), numeric
  }
}));

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const stripHtml = (html) => {
  if (!html) return "";
  return html.replace(/<[^>]*>/g, "").replace(/&nbsp;/g, " ").trim();
};

const hasImage = (html) => {
  if (!html) return false;
  return /<img\s+[^>]*src=/i.test(html);
};

const sanitizeEditorHtml = (html) => {
  if (!html) return "";

  return html
    // remove multiple spaces
    .replace(/\s+/g, " ")
    // remove nbsp
    .replace(/&nbsp;/g, " ")
    // remove empty paragraphs
    .replace(/<p>\s*<\/p>/gi, "")
    .replace(/<p><br><\/p>/gi, "")
    // trim start/end
    .trim();
};

const onSubmit = async (type) => {
  if (type === "save") processingSave.value = true;
  else processingSubmit.value = true;

  try {
    v$.value.$touch(); // ensure all fields are validated

    const descriptionHtmlRaw = model.value.description;
    const descriptionHtml = sanitizeEditorHtml(descriptionHtmlRaw);
    const descriptionText = stripHtml(descriptionHtml);
    const containsImage = hasImage(descriptionHtml);
    if (!descriptionText && !containsImage) {
      v$.value.description.$touch();
      notifyError({ message: "Process description is required" });
      return;
    }

    const isValid = await v$.value.$validate();
    if (!isValid) { v$.value.$touch(); return; }

    // Set status based on button type
    if (type === "save") {
      model.value.statusId = await sopProcessStatusDropdownSingleSelect.getValueByLabel("Draft"); // Draft
    } else if(type === "approve") {
      model.value.statusId = await sopProcessStatusDropdownSingleSelect.getValueByLabel("Approved"); // Approved
    } else {
      model.value.statusId = await sopProcessStatusDropdownSingleSelect.getValueByLabel("Submitted"); // Submitted
    }

    await sopProcessService.saveSOPProcess(props.id, model.value);
    onDialogOK();
    notifySuccess({
      message:
        type === "save"
          ? `Process successfully ${props.id ? "updated" : "created"} and saved as draft`
          : type === "approve"
            ? "Process successfully approved"
            : `Process successfully ${props.id ? "updated" : "created"} and sent for approver`
    });
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    if (type === "save") processingSave.value = false;
    else processingSubmit.value = false;
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(() => processId, (newValue) => {
  if (newValue) {
    getSOPProcessInDetailsById(processId);
  }
}, { immediate: true });

onMounted(async () => {
  sopProcessCategoryDropdownSingleSelect.load("SOP Process Category");
  await sopProcessStatusDropdownSingleSelect.load("SOP Process Status");

  const setSOPProcessStatus = sopProcessStatusDropdownSingleSelect.list.value.find(status => status.text.toLowerCase() === "draft");

  // Set Default values for advance filter
  if (model.value.statusId === null || model.value.statusId === undefined) model.value.statusId = setSOPProcessStatus.value;
});
</script>
<style scoped>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
