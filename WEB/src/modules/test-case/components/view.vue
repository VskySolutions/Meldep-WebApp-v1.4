<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader justify-between">
        <div class="text-h2 text-white q-mr-lg">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator inline-label mobile-arrows>
            <q-tab name="1_tab" label="Test Case Info." class="q-px-lg q-mr-md" />
            <q-tab name="2_tab" label="Description" class="q-px-lg" :disable="disableTab" />
            <q-tab name="3_tab" label="Steps" class="q-px-lg" :disable="disableTab" />
            <q-tab name="4_tab" label="Expected Result" class="q-px-lg" :disable="disableTab" />
            <q-tab name="5_tab" label="Actual Result" class="q-px-lg" :disable="disableTab" />
            <q-tab name="6_tab" label="Test Plan Info." class="q-px-lg" />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated>
            <q-tab-panel name="1_tab">
              <!-- <q-card class="card-header with-tools headerBasic"> -->
              <fieldset>
                <legend>Test Case Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12">
                    <div class="q-mb-xs">Test Case Name</div>
                    <div class="text-black q-mb-sm">{{ model.name }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Project Name</div>
                    <div class="text-black q-mb-sm">{{ model.project?.name }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Test Plan Name</div>
                    <div class="text-black q-mb-sm">{{ model.testPlan?.name }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Area</div>
                    <div class="text-black q-mb-sm">{{ model.area?.dropDownValue }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Workspace</div>
                    <div class="text-black q-mb-sm">{{ model.workspace?.dropDownValue }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Tested By</div>
                    <div class="text-black q-mb-sm">{{ model.testedByEmployee?.person?.fullName }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Tested Date</div>
                    <div class="text-black q-mb-sm">{{ model.testedDate }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Created By</div>
                    <div class="text-black q-mb-sm"> {{ model.createdByUser?.person?.fullName }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Created Date</div>
                    <div class="text-black q-mb-sm">{{ model.createdOnUtc }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Status</div>
                    <div class="text-black q-mb-sm">{{ model.status?.dropDownValue }}</div>
                  </div>
                  <div v-if="model.status?.dropDownValue === 'Fail'" class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Assign To</div>
                    <div v-if="model.status?.dropDownValue === 'Fail'" class="col-12 col-sm-6 col-md-6 text-black">{{ model.employee?.person?.fullName }}</div>
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="2_tab">
              <fieldset>
                <legend>Description</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12">
                    <div>
                      <p class="q-pt-md text-black RichTextEditor" v-html="model.description ? model.description : '-'" />
                    </div>
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="3_tab">
              <fieldset>
                <legend>Steps</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12">
                    <div>
                      <p class="q-pt-md text-black RichTextEditor" v-html="model.steps" />
                    </div>
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="4_tab">
              <fieldset>
                <legend>Expected Result</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12">
                    <div>
                      <p class="q-pt-md text-black RichTextEditor" v-html="model.expectedResult" />
                    </div>
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="5_tab">
              <fieldset>
                <legend>Actual Result</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12">
                    <div>
                      <p class="q-pt-md text-black RichTextEditor" v-html="model.actualResult" />
                    </div>
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="6_tab" class="items-center q-pa-md q-mx-auto">              
              <fieldset>
                <legend>Test Plan Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-6">
                    <div class="q-mb-xs">Project Name</div>
                    <div class="text-black q-mb-cs">
                      {{ model.project?.name }}
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
                      {{ model.area?.dropDownValue }}
                    </div>
                  </div>
                  <div class="col">
                    <div class="col-12 col-md-6">Workspace</div>
                    <div class="text-black">
                      {{ model.workspace?.dropDownValue }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-6">
                    <div class="q-mb-xs">Plan Maker</div>
                    <div class="text-black">
                      {{ model.planMaker?.person?.fullName }}
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="q-mb-xs">Plan Reviewer</div>
                    <div class="text-black">
                      {{ model.planReviewer?.person?.fullName }}
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
            </q-tab-panel>
          </q-tab-panels>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted, watch } from "vue";
import _ from "lodash";

import testcaseService from "../testCase.service";
import testplansService from "modules/test-plan/testPlan.service";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, testPlanId : { type: String, default: "" } });

// Common variables
// const { toDate } = useFilters();
const loading = ref(true);
const tab = ref("1_tab");

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "-",
  description: "",
  steps: "",
  expectedResult: "",
  actualResult: "",
  testedDate: "",
  createdOnUtc: "",
  project: {
    name: ""
  },
  area: {
    dropDownValue: ""
  },
  workspace: {
    dropDownValue: ""
  },
  testPlan: {
    name: ""
  },
  testedByEmployee: {
    person: {
      fullName: ""
    }
  },
  createdByUser: {
    person: {
      fullName: ""
    }
  },
  employee: {
    person: {
      fullName: ""
    }
  },
  status: {
    dropDownValue: ""
  },
  planReviewer: {
    person: {
      fullName: ""
    }
  },
  planMaker: {
    person: {
      fullName: ""
    }
  }
});

// get Test case details
const getTestCaseDetails = () => {
  loading.value = true;
  testcaseService.getTestCaseDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// get Test plan details
const getTestPlanDetails = () => {
  loading.value = true;
  testplansService.getTestPlanDetails(props.testPlanId).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

watch(tab, (newTab) => {
  if (newTab === "1_tab") {
    getTestCaseDetails();
  } else if (newTab === "6_tab") {
    getTestPlanDetails();
  }
});

// On page rendering
onMounted(() => {
  getTestCaseDetails();
});

</script>
