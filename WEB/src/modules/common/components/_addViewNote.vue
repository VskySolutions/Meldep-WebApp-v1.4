<template>
  <q-form greedy @submit.prevent.stop="onSubmit">
    <div class="q-pa-md cardTable">
      <div class="q-gutter-y-md">
        <fieldset v-if="isShow">
          <legend>Note Info</legend>
          <div v-if="showNoteType" class="row q-col-gutter-x-md q-mb-md">
            <div class="col-12">
              <div class="q-mb-xs text-black">
                <label>Note Type</label>
              </div>
              <q-radio
                v-for="option in noteTypeListSingleSelect.list.value"
                :key="option.value"
                v-model="model.noteTypeId"
                :val="option.value"
                :label="option.text"
              />
            </div>
          </div>
          <div class="row q-col-gutter-x-md q-mb-md">
            <div class="col-12">
              <div class="q-mb-xs text-black"><label>Add Note</label><span class="required">*</span></div>
              <q-editor
                v-model="model.notes"
                :dense="$q.screen.lt.md"
                :toolbar="toolbar"
                :fonts="fonts"
                :error="v$.notes.$error"
                :error-message="v$.notes.$errors[0]?.$message"
                @click="v$.notes.$touch"
                @keyup="onKeyUpMessage"
                @keydown="onKeyDownEditor"
              />
              <q-list v-if="showSuggestions" class="suggestions mention-dropdown bg-white shadow-3 rounded-borders bordered q-pa-sm scroll">
                <q-item
                  v-for="(user, index) in filteredUsers"
                  :key="index"
                  clickable
                  @mousedown.prevent
                  @click="addMention(user)"
                >
                  <q-item-section class="text-black">{{ user.text }}</q-item-section>
                </q-item>
              </q-list>
            </div>
          </div>
          <q-card-actions align="center">
            <q-btn
              color="grey-4"
              style="width:150px"
              push
              outline
              label="Close"
              type="button"
              class="text-grey-9 actionBtn"
              no-caps
              @click="emit('close')"
            />
            <q-btn
              color="primary"
              style="width:150px"
              push
              outline
              label="Save"
              class="actionBtn"
              :loading="processing"
              no-caps
              @click="onSubmit()"
            />
          </q-card-actions>
        </fieldset>
        <fieldset class="q-mt-lg">
          <legend v-if="isShow">View Notes</legend>
          <q-table
            ref="tableRef"
            v-model:pagination="pagination"
            class="note_table q-table__container"
            :loading="loading"
            :rows="rows"
            :columns="columns"
            row-key="id"
            separator="cell"
            no-data-label="No data available"
            binary-state-sort
            :rows-per-page-options="[20, 50, 100, 200, 500]"
            @request="getAllNoteByTypeAndRecord"
          >
            <template #header="props">
              <q-tr :props="props" class="bg-primary text-white">
                <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                <q-th v-if="isShow" auto-width class="text-center">Actions</q-th>
              </q-tr>
            </template>
            <template #body="props">
              <q-tr class="" :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                <q-td style="width: 10%;">{{ props.row.createdOnUtc }}</q-td>
                <q-td style="width: 20%;">{{ props.row.user?.person?.firstName }} {{ props.row.user?.person?.lastName }}</q-td>
                <q-td v-if="showNoteType" style="width: 10%;">{{ props.row.noteType?.dropDownValue }}</q-td>
                <q-td style="white-space: break-spaces;" @click="showComment(props.row.note)">
                  <div class="clamped-text RichTextEditor" v-html="removeLeadingSpaces(props.row.note)" />
                </q-td>
                <q-td v-if="isShow" auto-width class="text-center actions">
                  <q-icon
                    name="o_edit"
                    class="cursor-pointer q-mr-sm"
                    size="xs"
                    :class="storedUser.username === props.row.user.userName ? '' : 'hidden'"
                    @click="onEdit(props.row)"
                  >
                    <q-tooltip>Edit</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_delete_outline"
                    class="cursor-pointer"
                    size="xs"
                    :class="storedUser.username === props.row.user.userName ? '' : 'hidden'"
                    color="negative"
                    @click="onDelete(props.row)"
                  >
                    <q-tooltip>Delete</q-tooltip>
                  </q-icon>
                </q-td>
              </q-tr>
              <q-separator />
            </template>
          </q-table>
        </fieldset>
      </div>
    </div>
    <!-- <q-separator /> -->
  </q-form>
  <q-dialog v-model="isDialogOpen">
    <q-card style="width: 700px; max-width: 80vw;">
      <q-card-section style="background-color: #1b75ab">
        <div class="text-h2 text-weight-medium text-white">{{ label || 'Note Summary' }}</div>
      </q-card-section>
      <q-card-section class="q-pt-sm">
        <div class="RichTextEditor" v-html="currentComment" />
      </q-card-section>
      <q-card-actions align="right" class="bg-white text-teal">
        <q-btn
          v-close-popup
          color="grey-4"
          style="width:100px"
          push
          outline
          label="Close"
          type="button"
          class="text-grey-9 actionBtn"
          no-caps
        />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useQuasar } from "quasar";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, computed, onMounted } from "vue";
import { zwConfirmDelete, notifySuccess, getLocalStorage, notifyError } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import _ from "lodash";
import commonService from "services/common.service";
import employeesService from "src/modules/employee/employee.service";

// SOP Change :- Shared Dropdowns
import requirementModule from "src/modules/requirement/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";
const emit = defineEmits(["close"]);

const storedUser = getLocalStorage("user");
const $q = useQuasar();
const loading = ref(true);
const processing = ref(false);
const rows = ref([]);
const activeRowId = ref(null);
const isDialogOpen = ref(false);
const currentComment = ref("");
const filteredUsers = ref([]);
const showSuggestions = ref(false);
const { fonts, toolbar } = getEditorConfig($q);

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
const showNoteType = computed(() => props.showNoteType);

// Define model values
const model = ref({
  notes: "",
  noteTypeId: "",
  type: ""
});

const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = computed(() => {
  const cols = [
    {
      name: "createdOnUtc",
      label: "Created Date",
      field: "createdOnUtc",
      align: "left",
      sortable: true
    },
    {
      name: "Contributor",
      label: "Created By",
      field: "Contributor",
      align: "left",
      sortable: true
    }
  ];

  if (showNoteType.value) {
    cols.push({
      name: "noteType.dropDownValue",
      label: "Note Type",
      field: "noteType.dropDownValue",
      align: "left",
      sortable: true
    });
  }

  cols.push({
    name: "note",
    label: "Note",
    field: "note",
    align: "left",
    sortable: true
  });

  return cols;
});

const rules = {
  notes: { required: helpers.withMessage("Comment is required", required) }
};
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const getAllNoteByTypeAndRecord = () => {
  loading.value = true;
  commonService.getAllNoteByTypeAndRecord(props.id, props.type, true).then((resp) => {
    rows.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

function removeLeadingSpaces (html) {
  if (!html) return "";

  // Remove only plain leading spaces and &nbsp;, but keep tags
  return html.replace(/^(?:\s|&nbsp;)+/, "");
  // return html.replace(/^(?:\s|&nbsp;|<[^>]+>)*\s*/, "");
}

// ===========================================================
// DropDowns
// ===========================================================
// let mentionStart = -1;
// const personList = ref([]);
// function getAllPersonListForDropdown () {
//   personService.getAllPersonListForDropdown().then((resp) => {
//     const responseData = resp
//       .map((item) => ({ text: [item.firstName, item.middleName, item.lastName].filter(Boolean).join(" "), value: item.id }))
//       .sort((a, b) => a.text.localeCompare(b.text));
//     personList.value = responseData;
//   });
// }
const employeeList = ref([]);
function getAllActiveEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: [item.person.firstName, item.person.middleName, item.person.lastName].filter(Boolean).join(" "), value: item.id }));
    employeeList.value = responseData;
  });
}

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const {
  noteTypeListSingleSelect
} = requirementModule();
// ===========================================================
// Mention functionality
// ===========================================================
const mentionRange = ref(null);

const onKeyUpMessage = () => {
  const sel = window.getSelection();
  if (!sel || sel.rangeCount === 0) return;

  const range = sel.getRangeAt(0);
  const node = range.startContainer;
  const offset = range.startOffset;

  if (!node || node.nodeType !== Node.TEXT_NODE) {
    showSuggestions.value = false;
    return;
  }

  const textBeforeCursor = node.textContent.slice(0, offset);
  const match = textBeforeCursor.match(/@([\w\s]*)$/);

  if (!match) {
    showSuggestions.value = false;
    mentionRange.value = null;
    return;
  }

  mentionRange.value = range.cloneRange();
  mentionRange.value.setStart(node, textBeforeCursor.lastIndexOf("@"));

  const query = match[1].toLowerCase();

  filteredUsers.value = employeeList.value.filter(u =>
    u.text.toLowerCase().includes(query)
  );

  showSuggestions.value = filteredUsers.value.length > 0;
};

const addMention = (user) => {
  if (!mentionRange.value) return;

  const sel = window.getSelection();
  sel.removeAllRanges();
  sel.addRange(mentionRange.value);

  mentionRange.value.deleteContents();

  const parts = user.text.split(" ");
  const displayName = `@${parts[0]} ${parts[parts.length - 1]}`;

  const span = document.createElement("span");
  span.className = "tagged-user";
  span.textContent = displayName;
  span.setAttribute("data-id", user.value);

  const space = document.createTextNode("\u00A0");

  mentionRange.value.insertNode(space);
  mentionRange.value.insertNode(span);

  mentionRange.value.setStartAfter(space);
  mentionRange.value.collapse(true);

  sel.removeAllRanges();
  sel.addRange(mentionRange.value);

  showSuggestions.value = false;
  mentionRange.value = null;
};

const onKeyDownEditor = (e) => {
  if (e.key !== "Backspace") return;

  const sel = window.getSelection();
  if (!sel || !sel.rangeCount) return;

  const range = sel.getRangeAt(0);
  const node = range.startContainer;
  const offset = range.startOffset;

  let prevNode = null;

  // cursor in TEXT NODE
  if (node.nodeType === Node.TEXT_NODE) {
    if (offset === 0) {
      prevNode = node.previousSibling;
    } else {
      return; // normal typing
    }
  }

  // cursor in ELEMENT
  if (node.nodeType === Node.ELEMENT_NODE) {
    prevNode = node.childNodes[offset - 1];
  }

  // SKIP SPACE / NBSP / EMPTY TEXT
  while (
    prevNode &&
    (
      (prevNode.nodeType === Node.TEXT_NODE && prevNode.textContent.trim() === "") ||
      (prevNode.nodeType === Node.TEXT_NODE && prevNode.textContent === "\u00A0") ||
      prevNode.nodeName === "BR"
    )
  ) {
    prevNode = prevNode.previousSibling;
  }
  // HANDLE MENTION DELETE LOGIC
  if (prevNode && prevNode.classList?.contains("tagged-user")) {
    e.preventDefault();

    const text = prevNode.textContent.replace("@", "").trim();
    const parts = text.split(" ");

    /* ---------- FIRST BACKSPACE ---------- */
    if (!prevNode.dataset.short && parts.length > 1) {
      prevNode.textContent = `@${parts[0]}`;
      prevNode.dataset.short = "true";
      return;
    }

    /* ---------- SECOND BACKSPACE ---------- */
    const parent = prevNode.parentNode;

    if (
      prevNode.nextSibling &&
      prevNode.nextSibling.nodeType === Node.TEXT_NODE &&
      prevNode.nextSibling.textContent === "\u00A0"
    ) {
      parent.removeChild(prevNode.nextSibling);
    }

    const newRange = document.createRange();
    newRange.setStartBefore(prevNode);

    parent.removeChild(prevNode);

    newRange.collapse(true);
    sel.removeAllRanges();
    sel.addRange(newRange);
  }
};

const extractMentionedUsers = () => {
  const editor = document.querySelector(".q-editor__content");
  if (!editor) return [];

  return Array.from(editor.querySelectorAll(".tagged-user"))
    .map(el => el.getAttribute("data-id"))
    .filter(Boolean);
};

// Edit popup
const onEdit = (item) => {
  activeRowId.value = item.id;
  model.value.notes = item.note;
  model.value.noteTypeId = item.noteTypeId;
  const generalNoteValue =
    noteTypeListSingleSelect.getValueByLabel("General Note");

  if (
    model.value.noteTypeId === null ||
    model.value.noteTypeId === undefined ||
    model.value.noteTypeId === ""
  ) {
    model.value.noteTypeId = generalNoteValue;
  }
};

const showComment = (comment) => {
  currentComment.value = comment;
  isDialogOpen.value = true;
};

const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.user.person.firstName + " " + item.user.person.lastName}` }, () => {
    commonService.deleteNote(item.id).then(resp => {
      notifySuccess({ message: "Note is deleted successfully." });
      getAllNoteByTypeAndRecord();
      activeRowId.value = "";
    });
  }, () => {
    activeRowId.value = null;
  });
};

const isNoteEmpty = (html) => {
  if (!html) return true;

  const div = document.createElement("div");
  div.innerHTML = html;

  // Convert HTML to plain text and replace non-breaking spaces
  const text = div.textContent
    .replace(/\u00A0/g, " ")
    .trim();

  return text.length === 0;
};


const onSubmit = async () => {
  // processing.value = true;
  try {
    if (isNoteEmpty(model.value.notes)) {
      notifyError({ message: "Please enter a note." });
      return;
    }

    const isValid = await v$.value.$validate();
    if (!isValid) return;
    processing.value = true;

    // Extract mentioned users from the note
    const mentionedUsers = extractMentionedUsers(model.value.notes);

    const payload = {
      id: activeRowId.value ? activeRowId.value : null,
      taggedPersonId: mentionedUsers.join(","),
      subModuleId: props.id,
      note: model.value.notes,
      type: props.type,
      moduleId: props.moduleId,
      module: props.module,
      sub_Module: props.name,
      noteTypeId: model.value.noteTypeId
    };

    await commonService.saveNote(payload);
    notifySuccess({ message: "Note is saved successfully." });

    // Reset form
    activeRowId.value = null;
    model.value.notes = "";

    getAllNoteByTypeAndRecord();
  } catch (error) {
    console.error("Error saving note:", error);
    notifyError({ message: "Failed to save the note." });
  } finally {
    // processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

watch(
  () => props.id,
  (newValue) => {
    if (newValue) {
      getAllNoteByTypeAndRecord();
    }
  },
  { immediate: true }
);

onMounted(async () => {
  getAllActiveEmployeesListForDropdown();

  const noteTypes = await noteTypeListSingleSelect.load("Note Type");

  noteTypes.sort(
    (a, b) => Number(a.data) - Number(b.data)
  );

  const generalNoteValue =
    await noteTypeListSingleSelect.getValueByLabel("General Note");

  if (
    model.value.noteTypeId === null ||
    model.value.noteTypeId === undefined ||
    model.value.noteTypeId === ""
  ) {
    model.value.noteTypeId = generalNoteValue;
  }
});
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
