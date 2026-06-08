<template>
  <!-- Timeline Section (scrollable) -->
  <div class="col scroll q-px-sm" style="overflow-y:auto; flex-grow:1; height:64vh; display:flex; flex-direction:column-reverse">
    <q-timeline color="secondary">
      <template v-for="(group, date) in groupedNotes" :key="date">
        <!-- Date badge -->
        <!-- <div class="text-center q-my-md">
            <q-badge color="grey-4" text-color="black" rounded>
              {{ formatDate(date) }}
            </q-badge>
          </div> -->
        <q-timeline-entry
          v-for="note in group"
          :key="note.id"
          :subtitle="`${formatTime(note.createdOnUtc)} • ${note.user?.person?.fullName || ''}`"
          :side="user.userId === note.createdById ? 'right' : 'left'"
          color="primary"
          :icon="done_all"
        >
          <!-- NOTE BODY -->
          <div class="fs-14 note-row" :class="{ editing: editingNoteId === note.id }">
            <!-- EDIT MODE -->
            <template v-if="editingNoteId === note.id && user.username === note.user?.userName">
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
                </div>
                <!-- Actions -->
                <div class="flex gap-2 justify-end q-mt-sm">
                  <q-btn
                    icon="o_check"
                    color="primary"
                    round
                    dense
                    flat
                    :loading="editNoteProcessing"
                    :disable="editNoteProcessing || processing"
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
            <!-- VIEW MODE -->
            <template v-else>
              <div
                class="note-wrapper RichTextEditor"
                :class="{
                  'cursor-pointer': user.username === note.user?.userName && isShow
                }"
                @click="user.username === note.user?.userName && isShow && startEditing(note)"
              >
                <span class="text-black note-text" v-html="note.note" />
                <q-tooltip v-if="user.username === note.user?.userName && isShow">
                  Click to edit
                </q-tooltip>
              </div>
            </template>
            <!-- MORE OPTIONS -->
            <q-btn
              v-if="isShow"
              :class="user.username === note.user?.userName ? '' : 'hidden'"
              flat
              dense
              round
              color="primary"
              icon="o_more_vert"
            >
              <q-tooltip>More Options</q-tooltip>
              <q-menu auto-close>
                <q-list style="min-width: 40px">
                  <q-item v-close-popup clickable>
                    <q-item-section>
                      <q-item v-ripple clickable @click="onDeleteNotes(note)">
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
      </template>
    </q-timeline>
    <div v-if="AllNotes.length === 0">
      <h5 class="text-center text-grey">No Notes Available</h5>
    </div>
  </div>
  <!-- Footer -->
  <div class="bg-white" style="position: sticky; bottom: 0; z-index: 10; border-top: 0px solid #ccc;">
    <div class="row items-center no-wrap q-mt-lg">
      <div class="col-11">
        <q-editor
          v-model="newNote"
          class="q-ml-lg q-mb-sm"
          :class="{ 'editor-locked': !isShow }"
          placeholder="Type your note..."
          :dense="$q.screen.lt.md"
          :toolbar="toolbar"
          :fonts="fonts"
          :disable="!isShow"
          :readonly="!isShow"
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
          :disable="!isShow || !hasContent || processing || editNoteProcessing"
          @click="sendNote()"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useAuthStore } from "stores/auth";
import { notifySuccess, zwConfirmDelete } from "assets/utils";
import { useQuasar } from "quasar";
import commonService from "services/common.service";
import _ from "lodash";

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" },
  title: { type: String, default: "" },
  notesType: { type: String, default: "" },
  isShow: { type: Boolean, default: true }
});

// common variables
const editNoteProcessing = ref(false);
const processing = ref(false);
const loading = ref(true);
const authStore = useAuthStore();
const user = authStore.user;
const $q = useQuasar();
const isShow = props.isShow;

// notes
const AllNotes = ref([]);
const newNote = ref("");
const editingNoteId = ref(null);
const editingNoteValue = ref("");
const originalNoteValue = ref("");
const isCancelling = ref(false);
const moduleId = ref(props.id);
const subModuleId = ref(props.id);
const module = ref(props.title);
const subModule = ref(props.title);
const notesTypes = "Help Desk Notes";

// get all notes and map list
const getAllNoteByTypeAndRecord = () => {
  loading.value = true;
  commonService.getAllNoteByTypeAndRecord(props.id, notesTypes, false).then((resp) => {
    AllNotes.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// group the notes
const groupedNotes = computed(() => {
  return AllNotes.value.reduce((groups, note) => {
    const date = new Date(note.createdOnUtc).toDateString();
    if (!groups[date]) {
      groups[date] = [];
    }
    groups[date].push(note);
    return groups;
  }, {});
});

// const hasContent = computed(() => {
//   // Remove HTML tags and whitespace
//   const text = newNote.value.replace(/<[^>]*>/g, "").trim();
//   return text.length > 0;
// });
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
};

// cancel editing
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

// handle on key up
const onKeyUpMessage = (flag) => {
  const selection = window.getSelection();
  if (!selection || selection.rangeCount === 0) return;

  const range = selection.getRangeAt(0);
  if (!range) return;

  const html =
    flag === "ENV"
      ? editingNoteValue.value
      : newNote.value;

  const textContent = html.replace(/<[^>]*>/g, "").trim();
  return textContent;
};

// editing notes
const startEditing = (notes) => {
  editingNoteId.value = notes.id;
  editingNoteValue.value = notes.note;
  originalNoteValue.value = notes.note;
  isCancelling.value = false;
};

// time format
function formatTime (dateStr) {
  const options = { weekday: "long", hour: "2-digit", minute: "2-digit", hour12: true };
  return new Date(dateStr).toLocaleTimeString(undefined, options);
}

// save notes
const sendNote = async (note = null) => {
  // Prevent double submit
  if (processing.value || editNoteProcessing.value) return;
  try {
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

    const payload = {
      id: isEditing ? note.id : null,
      moduleId: isEditing ? note.moduleId : moduleId.value,
      subModuleId: isEditing ? note.subModuleId : subModuleId.value,
      module: isEditing ? note.module : module.value,
      sub_Module: isEditing ? note.sub_Module : subModule.value,
      note: noteValue,
      type: isEditing ? note.type : notesTypes
    };
    await commonService.saveNote(payload);
    notifySuccess({ message: "Note is saved successfully." });
    // if (isEditing) {
    //   editingNoteId.value = null;
    // } else {
    //   newNote.value = "";
    // }
    isEditing ? editingNoteId.value = null : (newNote.value = "");
    // Refresh timeline
    getAllNoteByTypeAndRecord();
  } catch (error) {
    console.error("Error saving note:", error);
  } finally {
    setTimeout(() => {
      processing.value = false;
      editNoteProcessing.value = false;
    }, 1500);
  }
};

// onDelete
const onDeleteNotes = (item) => {
  zwConfirmDelete({ data: `${item.user.person.firstName + " " + item.user.person.lastName}` }, () => {
    commonService.deleteNote(item.id).then(resp => {
      notifySuccess({ message: "Note is deleted successfully." });
      // Refresh timeline
      getAllNoteByTypeAndRecord();
    });
  }, () => {
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Rich Editor Tools: Fonts and Toolbar
// --------------------------------------------------------------------------------------------------------------------------------------------------

const fonts = {
  arial: "Arial",
  arial_black: "Arial Black",
  comic_sans: "Comic Sans MS",
  courier_new: "Courier New",
  impact: "Impact",
  lucida_grande: "Lucida Grande",
  times_new_roman: "Times New Roman",
  verdana: "Verdana"
};

const toolbar = [
  [
    {
      label: $q.lang.editor.align,
      icon: $q.iconSet.editor.align,
      fixedLabel: true,
      list: "only-icons",
      options: ["left", "center", "right", "justify"]
    }
  ],
  ["bold", "italic", "strike", "underline"],
  ["token", "hr", "link", "custom_btn"],
  [
    {
      label: $q.lang.editor.formatting,
      icon: $q.iconSet.editor.formatting,
      list: "no-icons",
      options: ["p", "h1", "h2", "h3", "h4", "h5", "h6", "code"]
    },
    "removeFormat"
  ],
  ["quote", "unordered", "ordered", "outdent", "indent"],
  ["undo", "redo"],
  ["viewsource"]
];

// ======================================================================
// On page rendering
onMounted(() => {
  getAllNoteByTypeAndRecord();
});

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
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
  visibility: hidden;
}
.note-row:hover .q-btn, .note-row.editing .q-btn {
  visibility: visible; /* show when row hovered */
}
.editor-locked .q-editor__toolbar {
  pointer-events: none;
  opacity: 0.6; /* optional - gives disabled look */
}

</style>
