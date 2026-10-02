<template>
  <div
    class="col scroll"
    style="overflow-y: auto; flex-grow: 1; display: flex; flex-direction: column;"
  >
    <q-timeline color="secondary">
      <q-timeline-entry
        v-for="(responseLogDescription, index) in notes"
        :key="index"
        :subtitle="
        `${responseLogDescription.createdOnUtc} - ${responseLogDescription.user?.person?.firstName} ${responseLogDescription.user?.person?.lastName}`
        "
        :icon="done_all"
        :color="'primary'"
      >
        <div v-if="notes.length">
          <div
            class="note-wrapper"
          >
              <div
                class="text-black note-text"
                v-html="responseLogDescription.note || ''"/>
              <q-separator v-if="showDescriptionInfo" class="q-my-sm" />
          </div>
        </div>
      </q-timeline-entry>
    </q-timeline>
    <div v-if="notes.length === 0">
      <h5 class="text-center text-grey">No Notes Available</h5>
    </div>
  </div>
</template>
<script setup>
import { ref, watch } from "vue";
import _ from "lodash";

import commonService from "services/common.service";

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" },
  showDescriptionInfo: { type: Boolean, default: false }
});

// common variables
const loading = ref(true);
const notes = ref([]);

// Get all descriptions and change logs
const getAllRequirementNoteByTypeAndRecord = () => {
  loading.value = true;
  commonService.getAllNoteByTypeAndRecord(props.id, 'Requirement', true).then((resp) => {
    const allowedNoteTypes = ["Client Status", "Client Follow-up"];

    notes.value = _.cloneDeep(resp).filter((x) =>
      allowedNoteTypes.includes(x.noteType?.dropDownValue)
    );
  }).finally(() => {
    loading.value = false;
  });
};

// Watch requirement ID
watch(
  () => props.id,
  (newId) => {
    if (newId) {
      getAllRequirementNoteByTypeAndRecord();
    }
  },
  { immediate: true }
);

</script>
<style scoped>
:deep(.q-timeline__content) {
  padding-bottom: 0 !important;
}
</style>
