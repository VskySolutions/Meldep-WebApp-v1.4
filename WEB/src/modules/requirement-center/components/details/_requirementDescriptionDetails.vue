<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <div class="col scroll q-px-sm" style="overflow-y: auto; flex-grow: 1; height: 64vh; display: flex; flex-direction: column-reverse;">
      <q-timeline color="secondary">
        <q-timeline-entry
          v-for="(responseLogDescription, index) in allResponseLogDescriptions"
          :key="index"
          :subtitle="`${responseLogDescription.createdOnUtc} - ${responseLogDescription.createdBy?.person?.fullName}`"
          :icon="done_all"
          :color="'primary'"
        >          
        <div v-if="allResponseLogDescriptions.length" class="fs-14 note-row">
          <div class="note-wrapper">
            <div
              class="text-black note-text"
              v-html="responseLogDescription.description || ''"
            />
          </div>
        </div>
        </q-timeline-entry>
      </q-timeline>
      <div v-if="allResponseLogDescriptions.length === 0">
        <h5 class="text-center text-grey">No Descriptions Available</h5>
      </div>
    </div>
  </q-card>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import _ from "lodash";

import requirementService from "modules/requirement/requirement.service";

// Props values i.e. come from query string
const props = defineProps({
  requirementId: { type: String, default: "" }
});

// common variables
const loading = ref(true);
const allResponseLogDescriptions = ref([]);

// Get all descriptions and change logs
const getAllRequirementDescriptionsById = async () => {
  if (!props.requirementId) return;

  loading.value = true;

  try {
    const resp = await requirementService.getAllRequirementDescriptionsById(
      props.requirementId, false
    );

    const requirementsList = resp.requirementList || [];
    const responseLogDescriptions = [];

    requirementsList.forEach((requirement) => {
      const requirementDescription = requirement.description
        ?.replace(/<[^>]*>/g, "")
        .trim();

      // Add requirement only when description exists
      if (requirementDescription) {
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
        new Date(a.createdOnUtc).getTime() -
        new Date(b.createdOnUtc).getTime()
    );

    allResponseLogDescriptions.value = responseLogDescriptions;

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
  () => props.requirementId,
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
.note-row {
  display: flex;
  align-items: center;
  gap: 6px;
}

.note-row .label {
  font-weight: bold;
  white-space: nowrap;
}

.note-text {
  display: inline-block; /* shrink-wraps to text width */
}
.note-row .q-btn {
  visibility: hidden; /* hide by default */
}

.note-row:hover .q-btn {
  visibility: visible; /* show when row hovered */
}
.notes-box-shadow {
  box-shadow: 0 1px 5px rgba(0, 0, 0, 0.2), 0 2px 2px rgba(0, 0, 0, 0.14), 0 3px 1px -2px rgba(0, 0, 0, 0.12) !important;
  background-color: #fff;
  border-radius: 4px 4px 4px 4px !important;
}
</style>
