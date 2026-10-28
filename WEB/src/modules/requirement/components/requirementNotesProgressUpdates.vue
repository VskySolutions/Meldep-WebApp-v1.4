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
            <q-tab name="1_tab" label="View Task Notes" class="q-px-lg q-mr-md" />
            <q-tab name="2_tab" label="Client Meeting Updates" class="q-px-lg q-mr-md" />
            <q-tab name="3_tab" label="Weekly Status" class="q-px-lg q-mr-md" />
            <q-tab name="4_tab" label="Weekly Next Steps" class="q-px-lg q-mr-md" />
            <q-tab name="5_tab" label="Roadblocks" class="q-px-lg q-mr-md" />
            <q-tab name="6_tab" label="Questions for Client" class="q-px-lg q-mr-md hidden" />
            <q-tab name="7_tab" label="Updates to Share with Client" class="q-px-lg q-mr-md hidden" />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated>

            <q-tab-panel name="1_tab">
              <viewTeamTaskUpdates
                :id="selectedRequirementId"
              />
            </q-tab-panel>

            <q-tab-panel name="2_tab">
              <viewClientMeetingUpdatesView
                :id="selectedRequirementId"
                :noteType="clientMeetingNoteType"
                :noteTypeId="clientMeetingNoteTypeId"
                :projectId="projectId"
                :type="props.type"
                :projectName="props.projectName"
                :RequirementTitle="props.RequirementTitle"
              />
            </q-tab-panel>

            <q-tab-panel name="3_tab">
              <viewClientMeetingUpdatesView
                :id="selectedRequirementId"
                :noteType="weeklyStatusNoteType"
                :noteTypeId="weeklyStatusNoteTypeId"
                :projectId="projectId"
                :type="props.type"
                :projectName="props.projectName"
                :RequirementTitle="props.RequirementTitle"
              />
            </q-tab-panel>

            <q-tab-panel name="4_tab">
              <viewClientMeetingUpdatesView
                :id="selectedRequirementId"
                :noteType="weeklyNextStepNoteType"
                :noteTypeId="weeklyNextStepNoteTypeId"
                :projectId="projectId"
                :type="props.type"
                :projectName="props.projectName"
                :RequirementTitle="props.RequirementTitle"
              />
            </q-tab-panel>

            <q-tab-panel name="5_tab">
              <viewClientMeetingUpdatesView
                :id="selectedRequirementId"
                :noteType="roadBlockNoteType"
                :noteTypeId="roadBlockNoteTypeId"
                :projectId="projectId"
                :type="props.type"
                :projectName="props.projectName"
                :RequirementTitle="props.RequirementTitle"
              />
            </q-tab-panel>

            <q-tab-panel name="6_tab" class="hidden">
              <viewQuestionsForClient
                :id="selectedRequirementId"
              />
            </q-tab-panel>

            <q-tab-panel name="7_tab">
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
import viewClientMeetingUpdatesView from "./_clientMeetingUpdatesView.vue";

// SOP Change :- Shared Dropdowns
import requirementModule from "src/modules/requirement/utils/dropdowns.js";

// Props values i.e. come from query string
const props = defineProps({ 
  id: { type: String, default: "" },
  projectId: { type: String, default: "" },
  type: { type: String, default: "" },
  noteType: { type: String, default: "" },
  projectId: { type: String, default: "" },
  projectName: { type: String, default: "" },
  RequirementTitle: { type: String, default: "" }
});
const rows = ref([]);
const changeLogRows = ref([]);
const projectId = props.projectId;

// Common variables
const selectedRequirementId = ref(props.id);
const loading = ref(true);
const tab = ref("1_tab");

// noteType and NoteTypeId
const clientMeetingNoteType="Client Meeting Updates";
const weeklyStatusNoteType="Weekly Status";
const weeklyNextStepNoteType="Weekly Next Steps";
const roadBlockNoteType="Roadblocks";
const clientMeetingNoteTypeId = ref(null);
const weeklyStatusNoteTypeId = ref(null);
const weeklyNextStepNoteTypeId = ref(null);
const roadBlockNoteTypeId = ref(null);

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

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const {
  noteTypeListSingleSelect
} = requirementModule();

watch(tab, (newTab) => {
  if (newTab === "1_tab") {
    getRequirement();
  } else if (newTab === "2_tab") {
    getRequirement();
  }
});

onMounted(async () => {
  getRequirement();

  await noteTypeListSingleSelect.load("Note Type");
  clientMeetingNoteTypeId.value = await noteTypeListSingleSelect.getValueByLabel("Client Meeting Updates");
  weeklyStatusNoteTypeId.value = await noteTypeListSingleSelect.getValueByLabel("Weekly Status");
  weeklyNextStepNoteTypeId.value = await noteTypeListSingleSelect.getValueByLabel("Weekly Next Steps");
  roadBlockNoteTypeId.value = await noteTypeListSingleSelect.getValueByLabel("Roadblocks");
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
