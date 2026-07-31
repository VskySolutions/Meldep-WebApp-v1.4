<template>
  <q-card flat bordered>
   <q-card-section class="row bg-primary text-white q-pa-sm items-center justify-between">
      <div class="row items-center">
        <q-icon
          name="o_fact_check"
          size="sm"
          class="q-mr-md"
        />
        <div class="column">
          <div class="text-caption text-blue-2">
            #{{ model.testCaseNumber }} • TEST CASE
          </div>

          <div class="text-h6 text-weight-bold">
            {{ model.name }}
          </div>
        </div>
      </div>
    </q-card-section>
    <q-separator />

    <!-- Tabs -->
    <q-tabs
      v-model="tab"
      dense
      active-color="primary"
      indicator-color="primary"
      align="left"
    >
      <q-tab name="details" label="Details" />
      <q-tab name="description" label="Description" />
      <q-tab name="steps" label="Steps" />
      <q-tab name="expected" label="Expected Result" />
      <q-tab name="actual" label="Actual Result" />
      <q-tab name="testPlan" label="Test Plan" />
    </q-tabs>

    <q-separator />
    <q-tab-panels
      v-model="tab"
      animated
    >
      <!-- ================================================= -->
      <!-- Details -->
      <!-- ================================================= -->
      <q-tab-panel name="details">
        <div class="row q-col-gutter-lg">
          <div class="col-12 col-md-6">
            <q-list dense>
              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Test Case Name</div>
                  <div class="q-mb-sm">{{ model.name || '-' }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Project Name</div>
                  <div class="q-mb-sm">{{ model.project?.name || '-' }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Requirement</div>
                  <div class="q-mb-sm">{{ model.requirement?.title || '-' }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Area</div>
                  <div class="q-mb-sm">{{ model.area?.dropDownValue || '-' }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Tested By</div>
                  <div class="q-mb-sm">{{ model.testedByEmployee?.person?.fullName || '-' }}</div>
                </q-item-section>
              </q-item>
              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Created By</div>
                  <div>{{ model.createdByUser?.person?.fullName || '-' }}</div>
                </q-item-section>
              </q-item>
            </q-list>
          </div>
          <div class="col-12 col-md-6">
            <q-list dense>
              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Test Plan</div>
                  <div class="q-mb-sm">{{ model.testPlan?.name || '-' }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Project Module</div>
                  <div class="q-mb-sm">{{ model.projectModule?.name || '-' }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Status</div>
                  <div class="q-mb-sm">{{ model.status?.dropDownValue }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Workspace</div>
                  <div class="q-mb-sm">{{ model.workspace?.dropDownValue || '-' }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Tested Date</div>
                  <div class="q-mb-sm">{{ model.testedDate || '-' }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Created Date</div>
                  <div class="q-mb-sm">{{ model.createdOnUtc || '-' }}</div>
                </q-item-section>
              </q-item>
            </q-list>
          </div>
        </div>
      </q-tab-panel>

      <!-- ================================================= -->
      <!-- Description -->
      <!-- ================================================= -->
      <q-tab-panel name="description">
        <q-card
          flat
          bordered
          class="q-pa-md"
        >
          <div
            class="RichTextEditor"
            v-html="model.description || '-'"
          />
        </q-card>
      </q-tab-panel>

      <!-- ================================================= -->
      <!-- Steps -->
      <!-- ================================================= -->
      <q-tab-panel name="steps">
        <q-card
          flat
          bordered
          class="q-pa-md"
        >
          <div
            class="RichTextEditor"
            v-html="model.steps || '-'"
          />
        </q-card>
      </q-tab-panel>

      <!-- ================================================= -->
      <!-- Expected Result -->
      <!-- ================================================= -->
      <q-tab-panel name="expected">
        <q-card
          flat
          bordered
          class="q-pa-md"
        >
          <div
            class="RichTextEditor"
            v-html="model.expectedResult || '-'"
          />
        </q-card>
      </q-tab-panel>

      <!-- ================================================= -->
      <!-- Actual Result -->
      <!-- ================================================= -->
      <q-tab-panel name="actual">
        <q-card
          flat
          bordered
          class="q-pa-md"
        >
          <div
            class="RichTextEditor"
            v-html="model.actualResult || '-'"
          />
        </q-card>
      </q-tab-panel>

      <!-- ================================================= -->
      <!-- Test Plan -->
      <!-- ================================================= -->
      <q-tab-panel name="testPlan">
        <div class="row q-col-gutter-lg">
          <div class="col-12 col-md-6">
            <q-list dense>
              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Project Name</div>
                  <div class="q-mb-sm">{{ testPlanModel.project?.name || '-' }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Plan Maker</div>
                  <div class="q-mb-sm">{{ testPlanModel.planMaker?.person?.fullName || '-' }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Area</div>
                  <div class="q-mb-sm">{{ testPlanModel.area?.dropDownValue || '-' }}</div>
                </q-item-section>
              </q-item>

            </q-list>
          </div>
          <div class="col-12 col-md-6">
            <q-list dense>
              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Test Plan</div>
                  <div class="q-mb-sm">{{ testPlanModel.name || '-' }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Plan Reviewer</div>
                  <div class="q-mb-sm">{{ testPlanModel.planReviewer?.person?.fullName || '-' }}</div>
                </q-item-section>
              </q-item>

              <q-item>
                <q-item-section>
                  <div class="text-caption text-grey">Workspace</div>
                  <div class="q-mb-sm">{{ testPlanModel.workspace?.dropDownValue || '-' }}</div>
                </q-item-section>
              </q-item>
            </q-list>
          </div>
          <div class="col-12">
            <div class="text-subtitle2 q-mb-sm">
              Description
            </div>
            <q-card
              flat
              bordered
              class="q-pa-md"
            >
              <div
                class="RichTextEditor"
                v-html="testPlanModel.description || '-'"
              />
            </q-card>
          </div>
        </div>
      </q-tab-panel>
    </q-tab-panels>
  </q-card>
</template>

<script setup>
import { ref, watch, onMounted } from "vue";
import _ from "lodash";

import testcaseService from "modules/test-case/testCase.service";
import testPlansService from "modules/test-plan/testPlan.service";

const props = defineProps({
  id: {
    type: String,
    required: true
  },
  testPlanId: {
    type: String,
    default: ""
  }
});

const loading = ref(false);
const tab = ref("details");

const model = ref({
  testCaseNumber: "",
  name: "",
  description: "",
  steps: "",
  expectedResult: "",
  actualResult: "",
  testedDate: "",
  createdOnUtc: "",

  project: {
    name: ""
  },

  projectModule: {
    name: ""
  },

  requirement: {
    title: ""
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
    dropDownValue: "",
    bgColor: "",
    color: ""
  }
});

const testPlanModel = ref({
  name: "",
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

const getTestCaseDetails = async () => {
  if (!props.id) return;

  try {
    loading.value = true;

    const resp = await testcaseService.getTestCaseDetails(props.id);
    model.value = _.cloneDeep(resp);

  } finally {
    loading.value = false;
  }
};

const getTestPlanDetails = async () => {
  if (!props.testPlanId) return;

  try {
    loading.value = true;

    const resp = await testPlansService.getTestPlanDetails(props.testPlanId);
    testPlanModel.value = _.cloneDeep(resp);

  } finally {
    loading.value = false;
  }
};

watch(tab, async (newTab) => {
  if (newTab === "details") {
    await getTestCaseDetails();
  }

  if (newTab === "testPlan") {
    await getTestPlanDetails();
  }
});

watch(
  () => props.id,
  async () => {
    await getTestCaseDetails();
  },
  {
    immediate: true
  }
);

onMounted(async () => {
  await getTestCaseDetails();
});
</script>
