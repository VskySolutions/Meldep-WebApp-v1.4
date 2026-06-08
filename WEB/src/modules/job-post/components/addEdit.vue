<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw !important; max-width: 60vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Job</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <!-- <q-card class="card-header with-tools headerBasic"> -->
            <fieldset>
              <legend>Job Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-lg-6 col-md-6 col-sm-6 col-xs-6">
                  <div class="q-mb-xs text-black">Job Title<span class="required">*</span></div>
                  <q-input
                    v-model="model.jobTitle"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    maxlength="128"
                    :error="v$.jobTitle.$error"
                    :error-message="v$.jobTitle.$errors[0]?.$message"
                    @click="v$.jobTitle.$touch"
                  />
                </div>
                <div class="col-lg-6 col-md-6 col-sm-6 col-xs-6">
                  <div class="q-mb-xs text-black">Criteria</div>
                  <q-input
                    v-model="model.criteria"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    maxlength="100"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-lg-6 col-md-6 col-sm-6 col-xs-6">
                  <div class="q-mb-xs text-black">Job Published Date<span class="required">*</span></div>
                  <q-input
                    v-model="model.publishedJobDateStr"
                    outlined
                    stack-label
                    hide-bottom-space
                    mask="##/##/####"
                    dense
                    :error="v$.publishedJobDateStr.$error"
                    :error-message="v$.publishedJobDateStr.$errors[0]?.$message"
                    @click="v$.publishedJobDateStr.$touch"
                  >
                    <template #append>
                      <q-icon name="o_calendar_month" class="cursor-pointer">
                        <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                          <q-date
                            v-model="model.publishedJobDateStr"
                            mask="MM/DD/YYYY"
                            @update:model-value="() => $refs.qDateProxy.hide()"
                          />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
                <div class="col-lg-6 col-md-6 col-sm-6 col-xs-6">
                  <div class="q-mb-xs text-black">Job Reference</div>
                  <q-input
                    v-model="model.jobReference"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    maxlength="128"
                  />
                </div>
                <div class="col-lg-6 col-md-6 col-sm-6 col-xs-6">
                  <div class="q-mb-xs q-mt-md text-black">Status</div>
                  <q-checkbox
                    v-model="model.isActive"
                    label="Active"
                    :dense="true"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-lg">
                <div class="col-12">
                  <div class="q-mb-xs text-black"><label>Description</label></div>
                  <div class="form-group RichTextEditor">
                    <q-editor
                      v-model="model.jobDescription"
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
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn
            color="grey-4"
            push
            outline
            label="Close"
            type="button"
            class="text-grey-9 actionBtn"
            no-caps
            @click="onDialogCancel"
          />
          <q-btn
            color="primary"
            push
            outline
            label="Save"
            type="submit"
            class="actionBtn"
            :loading="processing"
            :disable="processing"
            no-caps
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import jobPostService from "modules/job-post/jobPost.service";
import useFilters from "composables/useFilters";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, watch } from "vue";
import _ from "lodash";
import { isDate } from "validators/zw_validators.js";
import { notifySuccess, notifyError, notifyWarning } from "assets/utils";

// Define emits
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();
const $emit = defineEmits(["hide", "ok"]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Common variables
const { toDate } = useFilters();
const $q = useQuasar();
const loading = ref(true);
const processing = ref(false);

// Define model values
const model = ref({
  jobTitle: "",
  jobDescription: "",
  isActive: true
});

// Validation rules
const rules = {
  jobTitle: { required: helpers.withMessage("Job Title is required", required), minLength: minLength(1), maxLength: maxLength(100) },
  publishedJobDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    required: helpers.withMessage("Job published date is required", required)
  }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// get project details on edit mode
const getJobPost = () => {
  loading.value = true;
  jobPostService.getJobPost(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.publishedJobDateStr = resp.publishedJobDate ? toDate(resp.publishedJobDate) : "";
    model.value.jobDescription = resp.jobDescription ? resp.jobDescription : "";
  }).finally(() => {
    loading.value = false;
  });
};

// Submit form
const onSubmit = async () => {
  try {
    if (processing.value) {
      notifyWarning({ message: "Double click not allowed. Please wait..." });
      return; // stop further submit
    }
    if (!await v$.value.$validate()) {
      return;
    }
    processing.value = true;
    jobPostService.saveJobPost(props.id, model.value).then((resp) => {
      notifySuccess({ message: "Job Post is saved successfully." });
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
};

const toolbar = [
  [
    {
      label: $q.lang.editor.align,
      icon: $q.iconSet.editor.align,
      fixedLabel: true,
      list: "only-icons",
      options: ["left", "center", "right", "justify"]
    }
  ],
  ["bold", "italic", "strike", "underline"],
  ["token", "hr", "link", "custom_btn"],
  [
    {
      label: $q.lang.editor.formatting,
      icon: $q.iconSet.editor.formatting,
      list: "no-icons",
      options: ["p", "h1", "h2", "h3", "h4", "h5", "h6", "code"]
    },
    "removeFormat"
  ],
  ["quote", "unordered", "ordered", "outdent", "indent"],
  ["undo", "redo"],
  ["viewsource"]
];

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getJobPost();
  }
}, { immediate: true });

</script>
