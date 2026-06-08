<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="flex-grow: 1;">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Test Plan Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Project Name</div>
                <div class="text-black q-mb-cs">
                  {{ model.project.name }}
                </div>
              </div>
              <div class="col">
                <div class="col-12 col-md-6">Test Plan Name</div>
                <div class="text-black">
                  {{ model.name }}
                </div>
              </div>
            </div>
             <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Area</div>
                <div class="text-black q-mb-cs">
                  {{ model.area.dropDownValue }}
                </div>
              </div>
              <div class="col">
                <div class="col-12 col-md-6">Workspace</div>
                <div class="text-black">
                  {{ model.workspace.dropDownValue }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Plan Maker</div>
                <div class="text-black">
                  {{ model.planMaker.person.fullName }}
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Plan Reviewer</div>
                <div class="text-black">
                  {{ model.planReviewer.person.fullName }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12">
                <div class="q-mb-xs">Description</div>
                <div class="text-black RichTextEditor">
                  <p v-html="model.description ? model.description : '-'" />
                </div>
              </div>
            </div>
          </fieldset>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";
import testplansService from "modules/test-plan/testPlan.service";
// import useFilters from "composables/useFilters";

// Common variables
// const { toDate } = useFilters();
const loading = ref(true);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "-",
  description: "",
  project: {
    name: ""
  },
  area: {
    dropDownValue: ""
  },
  workspace: {
    dropDownValue: ""
  },
  planMaker: {
    person: {
      fullName: ""
    }
  },
  planReviewer: {
    person: {
      fullName: ""
    }
  }
});

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }});

// get project details
const getTestPlanDetails = () => {
  loading.value = true;
  testplansService.getTestPlanDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// On page rendering
onMounted(() => {
  getTestPlanDetails();
});

</script>
