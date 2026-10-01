<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent position="right"  @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic column no-wrap" style="width: 50vw; max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ label || 'Manage Notes' }}</div>
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
       <!-- Timeline Section (scrollable) -->
      <div class="col scroll q-px-sm" style="overflow-y: auto; flex-grow: 1; height: 64vh; display: flex; flex-direction: column-reverse;">
        <q-timeline color="secondary">
          <q-timeline-entry
            v-for="(notes, index) in allDataMappingNotes"
            :key="index"
            :subtitle="`${notes.createdOnUtc} - ${notes.createdBy?.person?.fullName}`"
            :icon="done_all"
            :color="'primary'"
          >
            <div class="fs-14 note-row">
              <template v-if="editingNoteId === notes.id && storedUser === notes.createdBy.userName">
                <div class="relative">
                  <div class="col-11">
                    <q-editor
                      v-model="editingNotesValue"
                      class="full-width"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                      @blur="(e) => handleEditorBlur(e, notes)"
                      @keyup="onKeyUpMessage('ENV')"
                    />
                  </div>
                  <!-- Actions -->
                  <div class="flex gap-2 justify-end mt-2">
                    <q-btn
                      icon="o_check"
                      color="primary"
                      round
                      dense
                      :loading="editNoteProcessing"
                      :disable="editNoteProcessing || processing"
                      flat
                      @click="sendNote(notes)"
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
                      @click="cancelEditing(notes)"
                    >
                      <q-tooltip>Cancel</q-tooltip>
                    </q-btn>
                  </div>
                </div>
              </template>
              <template v-else>
                <div class="note-wrapper cursor-pointer RichTextEditor" @click="startEditing(notes)">
                  <span class="text-black note-text" v-html="notes.note" />
                  <q-tooltip v-if="storedUser === notes.createdBy.userName">
                    Click to edit
                  </q-tooltip>
                </div>
              </template>
              <q-btn flat dense round color="primary" icon="o_more_vert" :class="storedUser === notes.createdBy.userName ? '' : 'hidden'">
                <q-tooltip>More Options</q-tooltip>
                <q-menu auto-close>
                  <q-list style="min-width: 40px">
                    <q-item v-close-popup clickable>
                      <q-item-section>
                        <q-item v-ripple clickable @click="onDelete(notes)">
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
        <div v-if="allDataMappingNotes.length === 0">
          <h5 class="text-center text-red">No Notes Available</h5>
        </div>
      </div>
      <!-- Footer -->
      <div class="bg-white" style="position: sticky; bottom: 0; z-index: 10; border-top: 0px solid #ccc;">
        <div class="row items-center no-wrap">
          <div class="col-11">
            <q-editor
              v-model="newNote"
              class="q-ml-lg q-mb-sm"
              placeholder="Type your note..."
              :dense="$q.screen.lt.md"
              :toolbar="toolbar"
              :fonts="fonts"
              style="width: 92%;"
              @keyup="onKeyUpMessage('NN')"
            />
          </div>
          <div class="col-1">
            <q-btn
              icon="o_send"
              color="primary"
              round
              flat
              :loading="processing"
              :disable="!hasContent || processing || editNoteProcessing"
              @click="sendNote()"
            />
          </div>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted, watch, computed } from "vue";
import _ from "lodash";
import { useQuasar } from "quasar";
import { useAuthStore } from "stores/auth";
import { notifySuccess, zwConfirmDelete } from "assets/utils";
import { useDialogPluginComponent } from "quasar";

import requirementDataMappingService from "../requirementDataMapping.service";

// Shared Dropdowns
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" }
});

// common variables
const loading = ref(true);
const processing = ref(false);
const editNoteProcessing = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);

const allDataMappingNotes = ref([]);
const storedUser = user?.username;
const editingNoteId  = ref(null);
const editingNotesValue  = ref("");
const originalNotesValue  = ref("");
const newNote = ref("");
const isCancelling = ref(false);
const filteredUsers = ref([]);

const isEditorEmpty = (html = "") => {
  return html
    .replace(/<br\s*\/?>/gi, "")
    .replace(/&nbsp;/gi, "")
    .replace(/<[^>]*>/g, "")
    .trim()
    .length === 0;
};

const hasContent = computed(() => {
  return !isEditorEmpty(newNote.value);
});

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
  if (editingNotesValue.value.trim() === (originalNotesValue.value || "").trim()) {
    editingNoteId.value = null;
  }
};

const onKeyUpMessage = (flag) => {
  const selection = window.getSelection();
  const range = selection.rangeCount > 0 ? selection.getRangeAt(0) : null;
  if (!range) return;

  if (flag === "ENV") {
    textContent = editingNotesValue.value.replace(/<[^>]*>/g, ""); // Remove HTML tags
  } else {
    textContent = newNote.value.replace(/<[^>]*>/g, ""); // Remove HTML tags
  }
};

// editing notes
const startEditing = (notes) => {
  editingNoteId.value = notes.id;
  editingNotesValue.value = notes.note;
  originalNotesValue.value = notes.note;
  isCancelling.value = false;
};

// cancelEditing
const cancelEditing = (note) => {
  isCancelling.value = true; // block blur save
  editingNoteId.value = null;
  editingNotesValue.value = "";
  if (note) {
    note.note = originalNotesValue.value; // restore original text
  }
  // reset flag after tick
  setTimeout(() => (isCancelling.value = false), 0);
};

// Get all notes
const getAllRequirementDataMappingNotesById = async () => {
  if (!props.id) return;
  loading.value = true;

  try {
    const resp = await requirementDataMappingService.getAllRequirementDataMappingNotes(props.id);
    const notes = resp.requirementDataMappingNotesList || [];

    allDataMappingNotes.value = notes.map((note) => ({
      id: note.id,
      note: note.note || "",
      createdOnUtc: note.createdOnUtc,
      createdById: note.createdById,
      createdBy: note.createdBy,
      requirementDataMappingId: note.requirementDataMappingId
    }));
  } catch (error) {
    console.error(
      "Error while loading notes:",
      error
    );
  } finally {
    loading.value = false;
  }
};

const refreshDataMappingList = () => {
  getAllRequirementDataMappingNotesById();
};

// save note
const sendNote = async (note = null) => {
  // Prevent double submit
  if (processing.value || editNoteProcessing.value) return;
  try {
    // Determine if we're editing or adding
    const isEditing = !!note;

    // Get the value being saved
    const noteValue = (isEditing ? editingNotesValue.value : newNote.value) || "";

    if (isEditorEmpty(noteValue)) {
      if (isEditing) editingNoteId.value = null;
      return;
    }

    // Validate
    if (!noteValue || !noteValue.trim()) {
      if (isEditing) editingNoteId.value = null; // close edit mode
      return;
    }

    // Enable correct loader
    if (isEditing) {
      editNoteProcessing.value = true;
    } else {
      processing.value = true;
    }

    const payload = {
      id: isEditing ? note.id : null,
      requirementDataMappingId: props.id,
      note: isEditing ? editingNotesValue.value : newNote.value
    };
    await requirementDataMappingService.saveNote(payload)
      .then((resp) => {
        notifySuccess({ message: "Note is saved successfully." });
        if (isEditing) {
          editingNoteId.value = null;
        } else {
          newNote.value = ""; // clear input after add
        }
      });
      refreshDataMappingList();
  } catch (error) {
    console.error("Error in submitting the note:", error);
  } finally {
    // processing.value = true;
    // editNoteProcessing.value = true;
    setTimeout(() => {
      processing.value = false;
      editNoteProcessing.value = false;
    }, 1500);
  }
};

// onDelete
const onDelete = (item) => {
  zwConfirmDelete({ data: `${item.createdBy?.person.fullName}` }, () => {
    requirementDataMappingService.deleteNote(item.id).then(resp => {
      notifySuccess({ message: "Note is deleted successfully." });
      refreshDataMappingList();
    });
  }, () => {
  });
};

// Watch requirement ID
watch(
  () => props.id,
  (newId) => {
    if (newId) {
      getAllRequirementDataMappingNotesById(true);
    }
  },
  { immediate: true }
);

// On page rendering
onMounted(() => {
  getAllRequirementDataMappingNotesById(true);
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
