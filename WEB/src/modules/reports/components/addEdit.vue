<template>
  <q-dialog ref="dialogRef" v-model="small" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw !important;max-width: 60vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Report</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Report Info</legend>
              <div class="row q-col-gutter-x-md">
                <div class="col-md-6 col-sm-12">
                  <div class="form-group">
                    <div class="text-black q-mb-sm">
                      <div class="q-mb-xs text-black">Name<span class="required">*</span></div>
                      <div>
                        <q-input
                          v-model="model.reportName"
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          :error="v$.reportName.$error"
                          :error-message="v$.reportName.$errors[0]?.$message"
                        />
                      </div>
                    </div>
                  </div>
                </div>
                <div class="col-md-6 col-sm-12">
                  <div class="form-group">
                    <div class="q-mb-xs text-black">Group By<span class="required">*</span></div>
                    <q-select
                      v-model="model.reportGroupId"
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      dense
                      :options="reportGroupList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      :error="v$.reportGroupId.$error"
                      :error-message="v$.reportGroupId.$errors[0]?.$message"
                      @blur="v$.reportGroupId.$touch"
                      @filter="getAllReportGroupForFilter"
                    >
                      <template #option="{ itemProps, opt }">
                        <q-item v-bind="itemProps">
                          <q-item-section>
                            <div class="row items-center">
                              <span>{{ opt.text }}</span>
                            </div>
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mt-md">
                <div class="col-md-12 col-sm-12">
                  <div class="form-group">
                    <div class="text-black q-mb-sm">
                      <div class="q-mb-xs text-black">Link<span class="required">*</span></div>
                        <q-input
                          v-model="model.url"
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          type="url"
                          :error="v$.url.$error"
                          :error-message="v$.url.$errors[0]?.$message"
                          @blur="v$.url.$touch"
                        />
                    </div>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mt-md">
                <div class="col-12">
                  <div class="form-group">
                    <label class="q-mb-xs text-black">Description</label>
                    <q-editor
                      v-model="model.reportDescription"
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
        <q-separator />
        <q-card-actions class="q-gutter-sm justify-center">
          <q-btn
            color="grey-4"
            push
            outline
            label="Close"
            type="button"
            no-caps
            class="text-grey-9 actionBtn"
            @click="onDialogCancel"
          />
          <q-btn
            color="primary"
            label="Save"
            type="submit"
            class="actionBtn"
            :loading="processing"
            no-caps
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers, url } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import { notifySuccess } from "assets/utils";
import _ from "lodash";
import commonService from "services/common.service";
import reportService from "modules/reports/reports.service";

// define emits
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();
const $emit = defineEmits(["hide", "ok"]);

// define props
const props = defineProps({ id: { type: String, default: "" } });

// common variables
const loading = ref(true);
const processing = ref(false);
const $q = useQuasar();

// define model
const model = ref({
  reportName: "",
  reportGroupId: "",
  reportDescription: "",
  url: ""
});

// Report Details Info - Validation Rules
const rules = {
  reportGroupId: { required: helpers.withMessage("Group by is required", required) },
  reportName: { required: helpers.withMessage("Name is required", required) },
  url: {
    required: helpers.withMessage("Link is required", required),
    url: helpers.withMessage("Invalid URL", url)
  }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// ger report and map
const getReportById = () => {
  loading.value = true;
  reportService.getReportById(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.reportDescription = resp.reportDescription ? resp.reportDescription : "";
  }).finally(() => {
    loading.value = false;
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get All Dropdowns
// --------------------------------------------------------------------------------------------------------------------------------------------------

// Get all report group list for dropdown
const reportGroupList = ref([]);
const reportGroupFilter = ref([]);
function getAllReportGroupForDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    reportGroupList.value = responseData;
    reportGroupFilter.value = responseData;
  });
}

// Search  report group for dropdown
function getAllReportGroupForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      reportGroupList.value = reportGroupFilter.value;
    } else {
      reportGroupList.value = reportGroupFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Rich Editor Tools: Fonts and Toolbar
// --------------------------------------------------------------------------------------------------------------------------------------------------

const fonts = {
  arial: "Arial",
  arial_black: "Arial Black",
  comic_sans: "Comic Sans MS",
  courier_new: "Courier New",
  impact: "Impact",
  lucida_grande: "Lucida Grande",
  times_new_roman: "Times New Roman",
  verdana: "Verdana"
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

// save report details
const onSubmit = async () => {
  if (await v$.value.$validate()) {
    processing.value = true;
    reportService.saveReport(props.id, model.value).then((resp) => {
      notifySuccess({ message: "Report is saved successfully." });
      $emit("ok");
      $emit("hide");
    }).finally(() => {
      processing.value = false;
    });
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getReportById();
  }
}, { immediate: true });

onMounted(() => {
  getAllReportGroupForDropDown("Report Group");
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
