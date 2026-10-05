<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card
      class="q-dialog-plugin PersonMain card-header with-tools headerBasic"
      style="width: 60vw !important; max-width: 60vw !important;"
    >
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ props.label || (isShow ? 'Add/View Notes' : 'View Notes') }}</div>
        <q-btn
          v-close-popup
          icon="o_close"
          class="close"
          color="white"
          flat
          round
          dense
        />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <template v-if="props.showNoteType">
          <q-tabs
            v-model="tab"
            dense
            class="text-primary"
            active-color="primary"
            indicator-color="primary"
            active-class="bg-blue-1 borderRadiusTabs"
            align="left"
            narrow-indicator
            inline-label
            mobile-arrows
          >
            <q-tab
              name="1_tab"
              label="Team Task Updates"
              class="q-px-lg q-mr-md"
            />

            <q-tab
              name="2_tab"
              label="Add Notes"
              class="q-px-lg q-mr-md"
            />
          </q-tabs>

          <q-separator />

          <q-tab-panels
            v-model="tab"
            animated
            keep-alive
          >
            <q-tab-panel name="1_tab">
              <viewTeamTaskUpdates :id="props.id" />
            </q-tab-panel>

            <q-tab-panel name="2_tab">
              <addViewNote
                :id="props.id"
                :type="props.type"
                :module-id="props.moduleId"
                :module="props.module"
                :name="props.name"
                :is-show="props.isShow"
                :label="props.label"
                :show-note-type="true"
                @close="onDialogHide"
              />
            </q-tab-panel>
          </q-tab-panels>
        </template>
        <template v-else>
          <addViewNote
            :id="props.id"
            :type="props.type"
            :module-id="props.moduleId"
            :module="props.module"
            :name="props.name"
            :is-show="props.isShow"
            :label="props.label"
            :show-note-type="false"
            @close="onDialogHide"
          />
        </template>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref } from "vue";

import viewTeamTaskUpdates from "src/modules/requirement/components/_teamTaskUpdatesView.vue";
import addViewNote from "src/modules/common/components/_addViewNote.vue";

const tab = ref("1_tab");

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

const props = defineProps({
  id: { type: String, default: "" },
  type: { type: String, default: "" },
  moduleId: { type: String, default: "" },
  module: { type: String, default: "" },
  name: { type: String, default: "" },
  isShow: { type: Boolean, default: true },
  label: { type: String, default: ""},
  showNoteType: { type: Boolean, default: false }
});

const isShow = props.isShow;

</script>

<style>
.q-dialog__inner--minimized>div {
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized {
  padding: 0;
}
.clamped-text {
  display: -webkit-box;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 3; /* Number of lines to show */
  overflow: hidden;
  text-overflow: ellipsis;
}
.tagged-user {
  color: var(--q-primary); /* Apply primary color */
  font-weight: bold;
  background-color: rgba(33, 150, 243, 0.1); /* Light blue background */
  padding: 2px 4px;
  border-radius: 4px;
  display: inline-block;
}
.mention-dropdown {
  width: 300px;
  max-width: 100%;
  max-height: 200px;
  overflow-y: auto;
}
</style>
