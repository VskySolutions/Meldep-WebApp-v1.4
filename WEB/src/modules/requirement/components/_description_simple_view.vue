<template>
  <div
    class="col scroll"
    style="overflow-y: auto; flex-grow: 1; display: flex; flex-direction: column;"
  >
    <q-timeline color="secondary">
      <q-timeline-entry
        v-for="(responseLogDescription, index) in allResponseLogDescriptions"
        :key="index"
        :icon="done_all"
        :color="'primary'"
      >
        <div v-if="allResponseLogDescriptions.length">
          <div
            class="note-wrapper"
          >
              <div
                class="text-black note-text"
                v-html="responseLogDescription.description || ''"/>
              <q-separator class="q-my-sm" />
          </div>
        </div>
      </q-timeline-entry>
    </q-timeline>
    <div v-if="allResponseLogDescriptions.length === 0">
      <h5 class="text-center text-grey">No Descriptions Available</h5>
    </div>
  </div>
</template>
<script setup>
import { ref, onMounted, watch } from "vue";
import _ from "lodash";

import requirementService from "../requirement.service";

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" }
});

// common variables
const loading = ref(true);
const allResponseLogDescriptions = ref([]);

// Get all descriptions and change logs
const getAllRequirementDescriptionsById = async () => {
  if (!props.id) return;
  loading.value = true;
  try {
    const resp = await requirementService.getAllRequirementDescriptionsById(
      props.id, true
    );
    const requirementsList = resp.requirementList || [];
    const responseLogDescriptions = [];

    requirementsList.forEach((requirement) => {
      const requirementDescription = requirement.description
        ?.replace(/<[^>]*>/g, "")
        .trim();

      // Add requirement only when description exists
      if (requirement.editingStatus === 1 || requirementDescription) {
        responseLogDescriptions.push({
          id: requirement.id,
          description: requirement.description,
          createdOnUtc: requirement.createdOnUtc,
          createdById: requirement.createdById,
          createdBy: requirement.createdBy,
          editingStatus: requirement.editingStatus,
          isRequirementDescription: true
        });
      }

      // Add change logs only when description exists
      (requirement.requirementChangeLog || []).forEach((responseLogDescriptionItem) => {
        const responseLogDescriptionText = responseLogDescriptionItem.description
          ?.replace(/<[^>]*>/g, "")
          .trim();

        if (responseLogDescriptionText) {
          responseLogDescriptions.push({
            id: responseLogDescriptionItem.id,
            description: responseLogDescriptionItem.description,
            createdOnUtc: responseLogDescriptionItem.createdOnUtc,
            createdById: responseLogDescriptionItem.createdById,
            createdBy: responseLogDescriptionItem.createdBy,
            isRequirementDescription: false
          });
        }
      });
    });

    responseLogDescriptions.sort(
      (a, b) =>
        new Date(b.createdOnUtc).getTime() -
        new Date(a.createdOnUtc).getTime()
    );

    allResponseLogDescriptions.value = responseLogDescriptions;
    const draftRequirement = responseLogDescriptions.find(
    item =>
      item.isRequirementDescription &&
      item.editingStatus === 1
    );

    if (draftRequirement) {
      editRequirementDescription(draftRequirement);
    }
  } catch (error) {
    console.error(
      "Error while loading requirement descriptions:",
      error
    );
  } finally {
    loading.value = false;
  }
};

// Watch requirement ID
watch(
  () => props.id,
  (newId) => {
    if (newId) {
      getAllRequirementDescriptionsById();
    }
  },
  { immediate: true }
);

// On page rendering
onMounted(() => {
  getAllRequirementDescriptionsById();
});

</script>
<style scoped>
:deep(.q-timeline__content) {
  padding-bottom: 0 !important;
}
</style>
