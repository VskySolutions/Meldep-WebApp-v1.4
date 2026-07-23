<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 65vw !important;max-width: 65vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader justify-between">
        <div class="text-h2 text-white q-mr-lg">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator inline-label mobile-arrows>
            <q-tab name="1_tab" label="Release Info." class="q-px-lg q-mr-md" />
            <q-tab name="2_tab" label="Release Tracking Items" class="q-px-lg" />
            <q-tab name="3_tab" label="Retest Test Cases" class="q-px-lg" />
            <q-tab
              name="4_tab"
              label="Test Case Execution"
              :disable="disableTab"
            />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated>
            <q-tab-panel name="1_tab">
              <!-- <q-card class="card-header with-tools headerBasic"> -->
              <fieldset>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Project Name</div>
                    <div class="text-black q-mb-sm">{{ model.project.name }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Infra Instance</div>
                    <div class="text-black q-mb-sm">{{ model.infraInstance.url }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Deployment Owner</div>
                    <div class="text-black q-mb-sm">{{ model.deploymentOwner.person.fullName }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Approver</div>
                    <div class="text-black q-mb-sm">{{ model.approver.person.fullName }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Tester</div>
                    <div class="text-black q-mb-sm">{{ model.tester.person.fullName }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Release Type</div>
                    <div class="text-black q-mb-sm">{{ model.releaseType.dropDownValue }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Version Number</div>
                    <div class="text-black q-mb-sm">{{ model.versionNumber }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Planned Release Date</div>
                    <div class="text-black q-mb-sm">{{ model.plannedReleaseDate }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-12 col-md-12">
                    <div class="q-mb-xs">Name</div>
                    <div class="text-black q-mb-sm">{{ model.name }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Created By</div>
                    <div class="text-black q-mb-sm"> {{ model.createdBy.person.fullName }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Created Date</div>
                    <div class="text-black q-mb-sm">{{ model.createdOnUtc }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Updated By</div>
                    <div class="text-black q-mb-sm"> {{ model.updatedBy.person.fullName }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Updated Date</div>
                    <div class="text-black q-mb-sm">{{ model.updatedOnUtc }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12">
                    <div class="q-mb-xs">Description</div>
                    <p class="q-pt-md text-black RichTextEditor" v-html="model.description ? model.description : '-'" />
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="2_tab">
              <selectionViewTab
                title="Deployment Items"
                :rows="rows.filter(x => x.type?.toLowerCase() !== 'testcase')"
                :loading="loading"
                :search="filterDeploymentItems"
                @update:search="filterDeploymentItems = $event"
                :show-type="true"
              />
            </q-tab-panel>

            <q-tab-panel name="3_tab">
              <selectionViewTab
                title="Test Cases for Retesting"
                :rows="rows.filter(x => x.type?.toLowerCase() === 'testcase')"
                :loading="loading"
                :search="filterTestCases"
                @update:search="filterTestCases = $event"
                :show-type="false"
              />
            </q-tab-panel>
            <q-tab-panel name="4_tab">
              <testCaseReleaseHistoryTable
                :rows="historyRows"
                :loading="loading"
                :show-release-version="false"
                :search="search"
                :statusEditable="false"
                @update:search="search = $event"
                @refresh="loadHistory"
              />
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
import { ref, onMounted, watch, computed } from "vue";
import _ from "lodash";
import useFilters from "composables/useFilters";
import releaseTrackingService from "modules/project-release-tracking/projectReleaseTracking.service";
import testCaseService from "src/modules/test-case/testCase.service";

import selectionViewTab from "modules/project-release-tracking/components/_selectionViewTab.vue";
import testCaseReleaseHistoryTable from "modules/test-case/components/_testCaseReleaseHistoryTable.vue";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Common variables
const loading = ref(true);
const { toDate } = useFilters();
// const filterItems = ref("");
const filterDeploymentItems = ref("");
const filterTestCases = ref("");
const search = ref("");

// Define model values
const model = ref({
  name: "-",
  description: "",
  createdOnUtc: "",
  project: {
    name: ""
  },
  infraInstance: {
    url: ""
  },
  deploymentOwner: {
    person: {
      fullName: ""
    }
  },
  tester: {
    person: {
      fullName: ""
    }
  },
  approver: {
    person: {
      fullName: ""
    }
  },
  releaseType: {
    dropDownValue: ""
  },
  createdBy: {
    person: {
      fullName: ""
    }
  },
  updatedBy: {
    person: {
      fullName: ""
    }
  }
});

const releaseTrackingId = props.id;
const tab = ref("1_tab");
const rows = ref([]);

// get release tracking details
const getReleaseTrackingInDetailsById = () => {
  loading.value = true;
  releaseTrackingService.getReleaseTrackingInDetailsById(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.plannedReleaseDateStr = resp.plannedReleaseDate ? toDate(resp.plannedReleaseDate) : "";
    model.value.description = resp.description ? resp.description : "";
    rows.value = resp.mappingItems || [];
  }).finally(() => {
    loading.value = false;
  });
};

const loadMappedItems = async () => {
  try {
    const mappedList = await releaseTrackingService.getMappingByReleaseTrackingId(releaseTrackingId);

    rows.value = (mappedList || []).map(x => ({
      id: x.refId,
      type: x.type,
      name: x.name,
      number: x.number,
      date: x.date
    }));
  } catch (err) {
    console.error(err);
  }
};

const testCaseIds = computed(() =>
  rows.value
    .filter(x => x.type?.toLowerCase() === "testcase")
    .map(x => x.id)
);

const historyRows = ref([]);

const loadHistory = async () => {
  if (!testCaseIds.value.length) {
    historyRows.value = [];
    return;
  }

  loading.value = true;

  try {
    historyRows.value =
      await testCaseService.getReleaseWiseTestCaseHistoryByTestCaseIds(
        testCaseIds.value,
        model.value.versionNumber
      );
  } finally {
    loading.value = false;
  }
};

watch(() => tab.value, async (newTab) => {
  if (newTab === "4_tab") {
    if (releaseTrackingId) {
      await loadMappedItems();
    }
    await loadHistory();
  }

  if (newTab === "1_tab") return;

  if (releaseTrackingId) {
    await loadMappedItems();
  }
});

// On page rendering
onMounted(() => {
  getReleaseTrackingInDetailsById();
});

</script>
