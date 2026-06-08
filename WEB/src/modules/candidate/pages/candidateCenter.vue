<template>
  <q-page padding>
    <q-card class="breadcrumSection project6 flex justify-between items-center">
      <!-- Breadcrumb Section -->
      <q-card-section class="card-header with-tools flex justify-between items-center">
        <q-breadcrumbs class="text-brown text-weight-bold text-h3">
          <template v-slot:separator>
            <q-icon size="1.5em" name="o_chevron_right" color="primary" />
          </template>
          <q-breadcrumbs-el label="Candidate" clickable to="/candidate"/>
          <q-breadcrumbs-el :label="'Candidate Center - ' + (model.person.fullName)"/>
        </q-breadcrumbs>
      </q-card-section>
      <!-- Chat Popup Button -->
      <div>
        <q-btn icon="o_chevron_left" outline label="Back To List" no-caps class="text-primary btnRounded q-mr-lg no-space-between" @click="$router.push('/candidate')" />
      </div>
    </q-card>
    <div class="items-center">
      <div class="q-pt-md cardTable">
        <div class="q-gutter-y-md">
          <q-card class="PersonMain card-header with-tools headerBasic">
            <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
              <q-tab name="1_tab" label="Profile" class="q-px-lg q-mr-md" />
              <q-tab name="2_tab" label="Activity" class="q-px-lg q-mr-md" />
              <q-tab name="3_tab" label="Feedback" class="q-px-lg q-mr-md" />
              <q-tab name="4_tab" label="Notes" class="q-px-lg q-mr-md" />
            </q-tabs>
            <q-tab-panels v-model="tab" animated>
              <q-tab-panel name="1_tab">
                <candidateProfileTab :candidateId="candidateId"/>
              </q-tab-panel>

              <q-tab-panel name="2_tab">
                <candidateActivityTab :candidateId="candidateId"/>
              </q-tab-panel>

              <q-tab-panel name="3_tab">
                <candidateFeedbackTab :candidateId="candidateId"/>
              </q-tab-panel>

              <q-tab-panel name="4_tab">
                <candidateNotesTab :candidateId="candidateId"/>
              </q-tab-panel>
            </q-tab-panels>
          </q-card>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { useDialogPluginComponent } from "quasar";
import { onMounted, ref } from "vue";
import _ from "lodash";
import candidateService from "modules/candidate/candidate.service";

// Tabs
import candidateProfileTab from "../components/_candidateProfileTab.vue";
import candidateActivityTab from "../components/_candidateActivityTab.vue";
import candidateFeedbackTab from "../components/_candidateFeedbackTab.vue";
import candidateNotesTab from "../components/_candidateNotesTab.vue";

// define emits
defineEmits([...useDialogPluginComponent.emits]);

// Id from routes
const candidateId = history.state?.candidateId;

// common variables
const tab = ref("1_tab");
const loading = ref(true);

const model = ref({
  person: {
    fullName: ""
  }
});

// get candidate details
const getCandidate = () => {
  loading.value = true;
  candidateService.getCandidate(candidateId).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

onMounted(() => {
  getCandidate();
});
</script>
