<template>
  <div class="col scroll q-px-sm" style="overflow-y: auto; flex-grow: 1; height: 64vh; display: flex; flex-direction: column-reverse;">
    <q-timeline color="secondary">
      <q-timeline-entry
        v-for="(responseLogDescription, index) in allResponseLogDescriptions"
        :key="index"
        :subtitle="`${responseLogDescription.createdOnUtc} - ${responseLogDescription.createdBy?.person?.fullName}`"
        :icon="done_all"
        :color="'primary'"
      >
        <div class="fs-14 note-row">
          <template  v-if="editingResponseLogDescriptionId === responseLogDescription.id && (
            (
              responseLogDescription.isRequirementDescription &&
              responseLogDescription.editingStatus === 1
            ) ||
            (
              !responseLogDescription.isRequirementDescription &&
              storedUser === responseLogDescription.createdBy?.userName
            )
          )">
            <div class="relative">
              <div class="col-11">
                <q-editor
                  v-model="editingResponseLogDescriptionValue"
                  class="full-width"
                  :dense="$q.screen.lt.md"
                  :toolbar="toolbar"
                  :fonts="fonts"
                  @blur="handleEditorBlur"
                />
              </div>
              <!-- Actions -->
              <div class="flex gap-2 justify-end mt-2">
                <q-btn
                  icon="o_check"
                  color="primary"
                  round
                  dense
                  :loading="responseLogDescriptionEditProcessing"
                  :disable="responseLogDescriptionEditProcessing || processing || !hasResponseLogDescriptionContent(true)"
                  flat
                  @click="submitResponseLogDescription(responseLogDescription)"
                >
                  <q-tooltip>Save</q-tooltip>
                </q-btn>
                <q-btn
                  icon="o_close"
                  color="negative"
                  round
                  dense
                  flat
                  @mousedown.prevent
                  @click="cancelEditingResponseLogDescription(responseLogDescription)"
                >
                  <q-tooltip>Cancel</q-tooltip>
                </q-btn>
              </div>
            </div>
          </template>
          <template v-else>
            <div
              class="note-wrapper cursor-pointer RichTextEditor full-width q-pa-sm"
              @click="
                responseLogDescription.isRequirementDescription
                  ? editRequirementDescription(responseLogDescription)
                  : startEditingResponseLogDescription(responseLogDescription)
              "
            >
              <span
                class="text-black note-text"
                v-html="responseLogDescription.description || ''"
              ></span>

              <q-tooltip
                 v-if="
                  (
                    responseLogDescription.isRequirementDescription &&
                    responseLogDescription.editingStatus === 1
                  ) ||
                  (
                    !responseLogDescription.isRequirementDescription &&
                    storedUser === responseLogDescription.createdBy?.userName
                  )
                "
              >
                Click to edit
              </q-tooltip>
            </div>
          </template>
          <q-btn v-if="!responseLogDescription.isRequirementDescription" flat dense round color="primary" icon="o_more_vert" :class="storedUser === responseLogDescription.createdBy?.userName ? '' : 'hidden'">
            <q-tooltip>More Options</q-tooltip>
            <q-menu auto-close>
              <q-list style="min-width: 40px">
                <q-item v-close-popup clickable>
                  <q-item-section>
                    <q-item v-ripple clickable @click="onResponseLogDescriptionDelete(responseLogDescription)">
                      <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                      <q-item-section class="text-negative">Delete</q-item-section>
                    </q-item>
                  </q-item-section>
                </q-item>
              </q-list>
            </q-menu>
          </q-btn>
        </div>
      </q-timeline-entry>
    </q-timeline>
    <div v-if="allResponseLogDescriptions.length === 0">
      <h5 class="text-center text-grey">No Descriptions Available</h5>
    </div>
  </div>
  <div class="bg-white" style="position: sticky; bottom: 0; z-index: 10; border-top: 0px solid #ccc;">
    <div class="row items-center no-wrap">
      <div class="col-11">
        <q-editor
          v-model="responseLogDescription"
          class="q-ml-lg"
          placeholder="Type your description..."
          :dense="$q.screen.lt.md"
          :toolbar="toolbar"
          :fonts="fonts"
          style="width: 92%;"
          :disable="isDraftRequirement"
        />
        <q-tooltip v-if="isDraftRequirement">
          Description cannot be added while the Requirement is in Draft status.
        </q-tooltip>
      </div>
      <div class="col-1">
        <q-btn
          icon="o_send"
          color="primary"
          round
          flat
          :loading="processing"
          :disable=" isDraftRequirement || !hasResponseLogDescriptionContent(false)  || processing || responseLogDescriptionEditProcessing"
          @click="submitResponseLogDescription()"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from "vue";
import _ from "lodash";
import { useQuasar } from "quasar";
import { useAuthStore } from "stores/auth";
import { notifySuccess, zwConfirmDelete } from "assets/utils";

import requirementService from "../requirement.service";

// Shared Dropdowns
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" }
});

// common variables
const loading = ref(true);
const processing = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);

const editDescriptionProcessing = ref(false);
const allResponseLogDescriptions = ref([]);
const responseLogDescription = ref("");
const storedUser = user?.username;
const editingResponseLogDescriptionId  = ref(null);
const editingResponseLogDescriptionValue  = ref("");
const originalResponseLogDescriptionValue  = ref("");

const hasResponseLogDescriptionContent = (isEdit = false) => {
  const description = isEdit
    ? editingResponseLogDescriptionValue.value
    : responseLogDescription.value;

  return !isEditorEmpty(description);
};

const isEditorEmpty = (html = "") => {
  return html
    .replace(/<br\s*\/?>/gi, "")
    .replace(/&nbsp;/gi, "")
    .replace(/<[^>]*>/g, "")
    .trim()
    .length === 0;
};

const isCancelling = ref(false);
// editing notes
const startEditingResponseLogDescription  = (responseLogDescriptionItem) => {
  editingResponseLogDescriptionId.value = responseLogDescriptionItem.id;
  editingResponseLogDescriptionValue.value = responseLogDescriptionItem.description;
  originalResponseLogDescriptionValue.value = responseLogDescriptionItem.description;
  isCancelling.value = false;
};

const editRequirementDescription = (requirementDescription) => {
  editingResponseLogDescriptionId.value = requirementDescription.id;

  editingResponseLogDescriptionValue.value =
    requirementDescription.description || "";

  originalResponseLogDescriptionValue.value =
    requirementDescription.description || "";

  isCancelling.value = false;
};

const cancelEditingResponseLogDescription  = (responseLogDescriptionItem) => {
  isCancelling.value = true; // block blur save
  editingResponseLogDescriptionId.value = null;
  editingResponseLogDescriptionValue.value = "";
  if (responseLogDescriptionItem) {
    responseLogDescriptionItem.description = originalResponseLogDescriptionValue.value; // restore original text
  }
  // reset flag after tick
  setTimeout(() => (isCancelling.value = false), 0);
};

// handleEditorBlur
const handleEditorBlur = (event) => {
// ignore if cancel in progress
  if (isCancelling.value) {
    return;
  }
  // If blur is because of toolbar click, ignore
  if (event.relatedTarget && event.relatedTarget.closest(".q-editor__toolbar")) {
    return;
  }
  // If no changes → exit without saving
  if (editingResponseLogDescriptionValue.value.trim() === (originalResponseLogDescriptionValue.value || "").trim()) {
    editingResponseLogDescriptionId.value = null;
  }
};

// Get all descriptions and change logs
const getAllRequirementDescriptionsById = async (openDraft = false) => {
  if (!props.id) return;

  loading.value = true;

  try {
    const resp = await requirementService.getAllRequirementDescriptionsById(
      props.id, false
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
        new Date(a.createdOnUtc).getTime() -
        new Date(b.createdOnUtc).getTime()
    );

    allResponseLogDescriptions.value = responseLogDescriptions;
    if (openDraft) {
      const draftRequirement = responseLogDescriptions.find(
        item =>
          item.isRequirementDescription &&
          item.editingStatus === 1
      );

      if (draftRequirement) {
        editRequirementDescription(draftRequirement);
      }
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

const isDraftRequirement = computed(() => {
  const requirementDescription = allResponseLogDescriptions.value.find(
    item => item.isRequirementDescription
  );

  return requirementDescription?.editingStatus === 1;
});

// Group descriptions by date
// const groupedDescriptions = computed(() => {
//   return allDescriptions.value.reduce((groups, note) => {
//     const date = new Date(note.createdOnUtc).toDateString();

//     if (!groups[date]) {
//       groups[date] = [];
//     }

//     groups[date].push(note);

//     return groups;
//   }, {});
// });

// onDelete
const onResponseLogDescriptionDelete = (item) => {
  zwConfirmDelete(
    {
      data: `${item.createdBy.person.fullName}`
    },
    () => {
      requirementService.deleteRequirementChangeLog(item.id).then(() => {
        notifySuccess({ message: "Description deleted successfully." });
        getAllRequirementDescriptionsById(false);
      });
    },
    () => {}
  );
};

// Submit description
const submitResponseLogDescription = async (responseLogDescriptionItem = null) => {
  // Prevent double submit
  if (processing.value || editDescriptionProcessing.value) return;

  try {
    // Determine if we're editing or adding
      const isEditing = !!responseLogDescriptionItem;

    // Get the value being saved
    const responseLogDescriptionValue = (
      isEditing
        ? editingResponseLogDescriptionValue.value
        : responseLogDescription.value
    ) || "";

    // Check if editor is empty
    if (!responseLogDescriptionItem?.isRequirementDescription && isEditorEmpty(responseLogDescriptionValue)) {
      if (isEditing) {
        editingResponseLogDescriptionId.value = null;
      }
      return;
    }

    // Validate
    if (!responseLogDescriptionValue || !responseLogDescriptionValue.trim()) {
      if (isEditing) {
        editingResponseLogDescriptionId.value = null;
      }
      return;
    }

    // Enable the correct loader
    if (isEditing) {
      editDescriptionProcessing.value = true;
    } else {
      processing.value = true;
    }

    // const payload = {
    //   id: isEditing ? responseLogDescriptionItem.id : null,
    //   requirementId: props.id,
    //   employeeId: user.employeeId,
    //   description: isEditing ? editingResponseLogDescriptionValue.value : responseLogDescription.value,
    // };

    // await requirementService.saveDescription(payload);

    if (responseLogDescriptionItem?.isRequirementDescription) {
      // Original Requirement Description
      const payload = {
        id: responseLogDescriptionItem.id,
        description: editingResponseLogDescriptionValue.value || ""
      };
      await requirementService.updateRequirementDescription(payload);
    } else {
      // Requirement Change Log Description
      await requirementService.saveResponseLogDescription({
        id: isEditing ? responseLogDescriptionItem.id : null,
        requirementId: props.id,
        employeeId: user.employeeId,
        description: isEditing ? editingResponseLogDescriptionValue.value : responseLogDescription.value,
      });
    }

    notifySuccess({
      message: "Description is saved successfully."
    });

    // Clear/close editor after save
    if (isEditing) {
      editingResponseLogDescriptionId.value = null;
      editingResponseLogDescriptionValue.value = "";
    } else {
      responseLogDescription.value = "";
    }

    // Reload descriptions
    await getAllRequirementDescriptionsById(false);

  } catch (error) {
    console.error("Error in submitting the description:", error);
  } finally {
    // Reset loaders
    setTimeout(() => {
      processing.value = false;
      editDescriptionProcessing.value = false;
    }, 1500);
  }
};


// Watch requirement ID
watch(
  () => props.id,
  (newId) => {
    if (newId) {
      getAllRequirementDescriptionsById(true);
    }
  },
  { immediate: true }
);

// On page rendering
onMounted(() => {
  getAllRequirementDescriptionsById(true);
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
