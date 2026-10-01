<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1200px; height: 100%; max-height: 100% !important;max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white q-mr-lg" style="flex-grow: 1;">{{ model.title }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <!-- <q-card-section class="card-header with-tools"> -->
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator inline-label mobile-arrows>
            <q-tab name="1_tab" label="Team Task Updates" class="q-px-lg q-mr-md" />
            <q-tab name="2_tab" label="Questions for Client" class="q-px-lg q-mr-md" />
            <q-tab name="3_tab" label="Updates to Share with Client" class="q-px-lg q-mr-md" />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated>
            <q-tab-panel name="1_tab">
              <viewTeamTaskUpdates
                :id="selectedRequirementId"
              />
            </q-tab-panel>
            <q-tab-panel name="2_tab">
              <viewQuestionsForClient
                :id="selectedRequirementId"
              />
            </q-tab-panel>
            <q-tab-panel name="3_tab">
              <viewUpdatesToShareWithClient
                :id="selectedRequirementId"
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
import { useDialogPluginComponent, useQuasar, QBtn } from "quasar";
import { ref, onMounted, watch } from "vue";
import _ from "lodash";

import requirementService from "../requirement.service";

import viewTeamTaskUpdates from "src/modules/requirement/components/_teamTaskUpdatesView.vue";
import viewQuestionsForClient from "src/modules/requirement/components/_questionsForClientView.vue";
import viewUpdatesToShareWithClient from "src/modules/requirement/components/_updatesToShareWithClientView.vue";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const rows = ref([]);
const changeLogRows = ref([]);

// Common variables
const selectedRequirementId = ref(props.id);
const loading = ref(true);
const tab = ref("1_tab");

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  title: "",
  notes: "",
  employeeId: "",
  IdentifiedDate: "",
  approvalStatus: "",
  description: "",
  createdOnUtc: "",
  lastNote: "",
  shortDescription: "",
  scope: ""
});

// get get Requirement on edit mode
const getRequirement = () => {
  loading.value = true;
  requirementService.getRequirementDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rows.value = resp.filePathDetails.map(item => ({
      ...item,
      editing: false,
      flag: "Edit"
    }));

    changeLogRows.value = resp.requirementChangeLog.map(item => ({
      ...item,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

watch(tab, (newTab) => {
  if (newTab === "1_tab") {
    getRequirement();
  } else if (newTab === "2_tab") {
    getRequirement();
  }
});

onMounted(() => {
  getRequirement();
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
