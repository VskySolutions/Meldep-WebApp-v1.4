<template>
  <div
    class="col scroll"
    :class="{ 'q-pa-sm': props.isRequirementCenter }"
    :style="{
      overflowY: 'auto',
      flexGrow: 1,
      height: '65vh',
      display: 'flex',
      flexDirection: 'column',
      ...(props.isRequirementCenter
        ? { border: '0.5px solid #1b75ab' }
        : {})
    }"
  >
    <q-timeline color="secondary">
      <q-timeline-entry
        v-for="(note, index) in notes"
        :key="index"
        :subtitle="`${note.createdOnUtc} - ${note.user?.person?.fullName}`"
        :icon="done_all"
        :color="'primary'"
      >
        <div class="fs-14 note-row">
          <template v-if="editingNoteId === note.id && storedUser === note.user?.userName">
            <div class="relative">
              <div class="col-11">
                <q-editor
                  v-model="editingNoteValue"
                  class="full-width"
                  :dense="$q.screen.lt.md"
                  :toolbar="toolbar"
                  :fonts="fonts"
                  @blur="(e) => handleEditorBlur(e, note)"
                  @keyup="onKeyUpMessage('ENV')"
                />
                <q-list v-if="showSuggestionsInEdit" class="suggestions mention-dropdown bg-white shadow-3 rounded-borders bordered q-pa-sm scroll">
                  <q-item v-for="(user, index) in filteredUsers" :key="index" clickable @click="addMention(user, 'ENV')">
                    <q-item-section class="text-black">{{ user.text }}</q-item-section>
                  </q-item>
                </q-list>
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
                  @click="sendNote(note)"
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
                  @click="cancelEditing(note)"
                >
                  <q-tooltip>Cancel</q-tooltip>
                </q-btn>
              </div>
            </div>
          </template>
          <template v-else>
            <div class="note-wrapper cursor-pointer RichTextEditor" @click="startEditing(note)">
              <span class="text-black note-text" v-html="note.note" />
              <q-tooltip v-if="storedUser === note.user?.userName">
                Click to edit
              </q-tooltip>
            </div>
          </template>
          <q-btn flat dense round color="primary" icon="o_more_vert" :class="storedUser === note.user?.userName ? '' : 'hidden'">
            <q-tooltip>More Options</q-tooltip>
            <q-menu auto-close>
              <q-list style="min-width: 40px">
                <q-item v-close-popup clickable>
                  <q-item-section>
                    <q-item v-ripple clickable @click="onDelete(note)">
                      <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                      <q-item-section class="text-negative">Delete</q-item-section>
                    </q-item>
                  </q-item-section>
                </q-item>
              </q-list>
            </q-menu>
          </q-btn>
        </div>
        <!-- <div v-if="note.length">
          <div
            class="note-wrapper"
          >
              <div
                class="text-black note-text"
                v-html="responseLogDescription.note || ''"/>
              <q-separator v-if="showDescriptionInfo" class="q-my-sm" />
          </div>
        </div> -->
      </q-timeline-entry>
    </q-timeline>
    <div v-if="notes.length === 0">
      <h5 class="text-center text-grey">No Notes Available</h5>
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
          <q-list v-if="showSuggestions" class="suggestions mention-dropdown bg-white shadow-3 rounded-borders bordered q-pa-sm scroll">
            <q-item v-for="(user, index) in filteredUsers" :key="index" clickable @click="addMention(user,'NN')">
              <q-item-section class="text-black">{{ user.text }}</q-item-section>
            </q-item>
          </q-list>
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
</template>
<script setup>
import { ref, watch, computed, onMounted } from "vue";
import { useAuthStore } from "stores/auth";
import { notifySuccess, zwConfirmDelete } from "assets/utils";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";
import { useQuasar } from "quasar";
import _ from "lodash";

import commonService from "services/common.service";
import projectService from "modules/project/projects.service";

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" },
  showDescriptionInfo: { type: Boolean, default: false },
  type: { type: String, default: "" },
  noteTypeId: { type: String, default: "" },
  noteType: { type: String, default: "" },
  isRequirementCenter: { type: Boolean, default: false },
  projectId: { type: String, default: "" },
  projectName: { type: String, default: "" },
  RequirementTitle: { type: String, default: "" }
});

// common variables
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const loading = ref(true);
const processing = ref(false);
const editNoteProcessing = ref(false);
const notes = ref([]);
const projectId = props.projectId;
const authStore = useAuthStore();
const user = authStore.user;
const storedUser = user?.username;

// new notes
const newNote = ref("");
const activeNoteContext = ref({
  moduleId: props.projectId,
  type: props.type,
  noteTypeId: props.noteTypeId,
  module: props.projectName,
  sub_Module: props.RequirementTitle,
  subModuleId: props.id
});

// editing notes
const editingNoteId = ref(null);
const editingNoteValue = ref("");
const originalNoteValue = ref("");
const isCancelling = ref(false);

// Get all descriptions and change logs
const getAllRequirementNoteByTypeAndRecord = () => {
  loading.value = true;
  commonService.getAllNoteByTypeAndRecord(props.id, 'Requirement', false).then((resp) => {
    // const allowedNoteTypes = ["Client Meeting Updates"];
    const allowedNoteTypes = props.noteType;

    notes.value = _.cloneDeep(resp).filter((x) =>
      allowedNoteTypes.includes(x.noteType?.dropDownValue)
    );

  }).finally(() => {
    loading.value = false;
  });
};

//------------------------------------------------------------------------------------------------------------
// Custom functions
//------------------------------------------------------------------------------------------------------------

// editing notes
const startEditing = (notes) => {
  editingNoteId.value = notes.id;
  editingNoteValue.value = notes.note;
  originalNoteValue.value = notes.note;
  isCancelling.value = false;
};

// handleEditorBlur
const handleEditorBlur = (event, note) => {
// ignore if cancel in progress
  if (isCancelling.value) {
    return;
  }
  // If blur is because of toolbar click, ignore
  if (event.relatedTarget && event.relatedTarget.closest(".q-editor__toolbar")) {
    return;
  }
  // If no changes → exit without saving
  if (editingNoteValue.value.trim() === (originalNoteValue.value || "").trim()) {
    editingNoteId.value = null; // close edit mode
  }
  // sendNote(note);
};

const cancelEditing = (note) => {
  isCancelling.value = true; // block blur save
  editingNoteId.value = null;
  editingNoteValue.value = "";
  if (note) {
    note.note = originalNoteValue.value; // restore original text
  }
  // reset flag after tick
  setTimeout(() => (isCancelling.value = false), 0);
};

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

const showSuggestions = ref(false);
const showSuggestionsInEdit = ref(false);
const mentionStart = ref(-1);
const filteredUsers = ref([]);

const onKeyUpMessage = (flag) => {
  const selection = window.getSelection();
  const range = selection.rangeCount > 0 ? selection.getRangeAt(0) : null;
  if (!range) return;

  const cursorPos = range.startOffset; // Get cursor position inside `q-editor`
  let textContent;
  if (flag === "ENV") {
    textContent = editingNoteValue.value.replace(/<[^>]*>/g, ""); // Remove HTML tags
  } else {
    textContent = newNote.value.replace(/<[^>]*>/g, ""); // Remove HTML tags
  }
  const lastAtPos = textContent.lastIndexOf("@", cursorPos - 1);

  // Show mention list if "@" is followed by any character
  if (lastAtPos !== -1 && cursorPos >= lastAtPos + 1) {
    mentionStart.value = lastAtPos;
    const searchTerm = textContent.slice(lastAtPos + 1, cursorPos).toLowerCase();

    // Filter user list based on search term
    filteredUsers.value = projectEmployeeList.value.filter((user) =>
      searchTerm ? user.text.toLowerCase().includes(searchTerm) : true
    );

    if (flag === "ENV") {
      showSuggestionsInEdit.value = filteredUsers.value.length > 0;
    } else {
      showSuggestions.value = filteredUsers.value.length > 0;
    }
  } else {
    if (flag === "ENV") { showSuggestionsInEdit.value = false; } else { showSuggestions.value = false; }
  }
};

const addMention = (user, flag) => {
  if (mentionStart.value === -1) return; // No active mention

  const editor = document.querySelector(".q-editor__content");
  if (!editor) return;

  const selection = window.getSelection();
  const range = selection.rangeCount > 0 ? selection.getRangeAt(0) : null;
  if (!range) return;

  // Get the current content
  const content = flag === "ENV" ? editingNoteValue.value : newNote.value;

  // Preserve text before and after the mention being typed
  const beforeMention = content.slice(0, mentionStart.value); // Text before "@"
  const afterMention = content.slice(mentionStart.value); // Text after "@"

  // Find the end of the mention (first space, punctuation, or end of string)
  const mentionEndMatch = afterMention.match(/(\s|<\/?[a-z][\s\S]*>|&nbsp;|$)/);
  const mentionEndIndex = mentionEndMatch ? mentionEndMatch.index : afterMention.length;

  // Extract remaining text after the mention
  const remainingText = afterMention.slice(mentionEndIndex);

  // Format mention
  const nameParts = user.text.split(" ");
  const formattedName = `${nameParts[0]} ${nameParts[nameParts.length - 1]}`.trim();
  const mentionTag = `<span class="tagged-user">@${formattedName}</span>&nbsp;`;

  // Insert mention at the correct position
  if (flag === "ENV") {
    editingNoteValue.value = beforeMention + mentionTag + remainingText;
    showSuggestionsInEdit.value = false;
  } else {
    newNote.value = beforeMention + mentionTag + remainingText;
    showSuggestions.value = false;
  }

  mentionStart.value = -1;

  // Restore Cursor Position after mention insertion
  setTimeout(() => {
    if (editor) {
      const mentionSpan = editor.querySelector(".tagged-user:last-child");
      if (mentionSpan && mentionSpan.nextSibling) {
        range.setStartAfter(mentionSpan.nextSibling);
        range.setEndAfter(mentionSpan.nextSibling);
      } else {
        range.setStartAfter(mentionSpan);
        range.setEndAfter(mentionSpan);
      }
      selection.removeAllRanges();
      selection.addRange(range);
    }
  }, 0);
};

const extractMentionedUsers = (text) => {
  const mentionedNames = [...text.matchAll(/@([\w\s]+)/g)].map(match => match[1].trim().toLowerCase());
  return projectEmployeeList.value.filter(emp => {
    const empFullName = emp.text.trim().toLowerCase();
    const empParts = empFullName.split(/\s+/);
    const empFirstName = empParts[0];
    const empLastName = empParts[empParts.length - 1];

    return mentionedNames.some(name => {
      const mention = name.toLowerCase();
      return (
        mention === empFullName || // full match
        mention === `${empFirstName} ${empLastName}` // first + last match
      );
    });
  }).map(emp => emp.value);
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Advance Filter:- Dropdown Functions
// --------------------------------------------------------------------------------------------------------------------------------------------------

const projectEmployeeList = ref([]);

function getProjectEmployees (projectId) {
  projectService.getProjectEmployees(projectId).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.text, value: item.value }));
    projectEmployeeList.value = responseData;
  });
}

// save note
const sendNote = async (note = null) => {
  // Prevent double submit
  if (processing.value || editNoteProcessing.value) return;
  try {
    // Determine if we're editing or adding
    const isEditing = !!note;

    // Get the value being saved
    const noteValue = (isEditing ? editingNoteValue.value : newNote.value) || "";

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
    // Extract mentioned users from the note
    const mentionedUsers = extractMentionedUsers(isEditing ? editingNoteValue.value : newNote.value);
    const payload = {
      id: isEditing ? note.id : null,
      subModuleId: isEditing ? note.subModuleId : activeNoteContext.value.subModuleId,
      moduleId: isEditing ? note.moduleId : activeNoteContext.value.moduleId,
      note: isEditing ? editingNoteValue.value : newNote.value,
      noteTypeId: isEditing ? note.noteTypeId : activeNoteContext.value.noteTypeId,
      type: isEditing ? note.type : activeNoteContext.value.type,
      module: isEditing ? note.module : activeNoteContext.value.module,
      sub_Module: isEditing ? note.sub_Module : activeNoteContext.value.sub_Module,
      taggedPersonId: mentionedUsers.join(",")
    };
    await commonService.saveNote(payload)
      .then((resp) => {
        notifySuccess({ message: "Note is saved successfully." });
        getAllRequirementNoteByTypeAndRecord();
        if (isEditing) {
          editingNoteId.value = null;
        } else {
          newNote.value = ""; // clear input after add
        }
      });
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
  // activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.user.person.firstName + " " + item.user.person.lastName}` }, () => {
    commonService.deleteNote(item.id).then(resp => {
      notifySuccess({ message: "Note is deleted successfully." });
      getAllRequirementNoteByTypeAndRecord();
    });
  }, () => {
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

onMounted(async () => {
  getProjectEmployees(projectId);
});

</script>
<style scoped>
:deep(.q-timeline__content) {
  padding-bottom: 0 !important;
}
:deep(.q-timeline__subtitle) {
  color: #1b75ab !important;
}
.note-row {
  display: flex;
  align-items: center;
  gap: 6px;
}

.note-row .label {
  font-weight: bold;
  white-space: nowrap;
}
.note-input {
  flex: 1;
  min-width: 100px;
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
