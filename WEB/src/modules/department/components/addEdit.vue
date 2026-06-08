<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 55vw;max-width: 55vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Department</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Department Information</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs text-black text-black">Department Name<span class="required">*</span></div>
                  <div>
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
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-lg">
                <div class="col-12">
                  <div class="form-group">
                    <div class="q-mb-xs text-black"><label>Description</label></div>
                    <q-editor
                      v-model="model.description"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
              </div>
              <!-- Hint added manually -->
              <!-- <div class="text-caption text-grey-7 q-mt-xs">The maximum length allowed is 500.</div> -->
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn v-if="tab !== '4_tab'" color="primary" outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
          <!-- <q-btn color="primary" outline label="Save & Close" type="button" class="rounded-corners" :loading="processingClose" no-caps @click="onSubmitClose()" /> -->
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
import _ from "lodash";
import { notifySuccess, notifyError } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import departmentsService from "modules/department/department.service";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Common variables
const tab = ref("1_tab");
const $q = useQuasar();
const loading = ref(true);
const processing = ref(false);
const processingClose = ref(false);

let departmentId = props.id;

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "",
  description: "",
  tab
});

// Validation rules
const rules = {
  name: { required: helpers.withMessage("Department name is required", required), minLength: minLength(1), maxLength: maxLength(500) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// get department details on edit mode
const getDepartment = () => {
  loading.value = true;
  departmentsService.getDepartment(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

const departmentList = ref([]);
const departmentListFilter = ref([]);
function getAllDepartmentListForDropdown () {
  departmentsService.getAllDepartmentListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.name, value: item.id }));
    departmentList.value = responseData;
    departmentListFilter.value = responseData;
  });
}

// save department
const onSubmit = async (isClose = 0) => {
  try {
    if (await v$.value.$validate()) {
      if (isClose === 1) {
        processingClose.value = true;
      } else {
        processing.value = true;
      }
      model.value.tab = tab;
      departmentsService.saveDepartment(departmentId, model.value).then((resp) => {
        notifySuccess({ message: "Department is saved successfully." });
        departmentId = resp;
        onDialogOK();
      });
    }
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    // processingClose.value = true;
    setTimeout(() => {
      processing.value = false;
      processingClose.value = false;
    }, 1500);
  }
};

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

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getDepartment();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  getAllDepartmentListForDropdown();
});

</script>

<style>
.q-dialog__inner--minimized>div {
  max-height: calc(100vh) !important;
}

.q-dialog__inner--minimized {
  padding: 0;
}
</style>
