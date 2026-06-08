<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card
      class="q-dialog-plugin PersonMain card-header with-tools headerBasic"
      style="width: 60vw !important; max-width: 60vw !important;"
    >
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div v-if="isShow" class="text-h2 text-white">Add/View Notes</div>
        <div v-else class="text-h2 text-white">View Notes</div>
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
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset v-if="isShow">
              <legend>Add Note</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
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
                  @click="onDialogCancel"
                />
                <q-btn
                  v-if="tab !== '4_tab'"
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
                    <q-td style="width: 20%;">{{ props.row.user.person.firstName }} {{ props.row.user.person.lastName }}</q-td>
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
    </q-card>
  </q-dialog>
  <q-dialog v-model="isDialogOpen">
    <q-card style="width: 700px; max-width: 80vw;">
      <q-card-section style="background-color: #1b75ab">
        <div class="text-h2 text-weight-medium text-white">Note Summary</div>
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
import { useDialogPluginComponent, useQuasar } from "quasar";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import { zwConfirmDelete, notifySuccess, getLocalStorage, notifyError } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import _ from "lodash";
import commonService from "services/common.service";
import employeesService from "src/modules/employee/employee.service";

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
// const mentionStart = ref(-1);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

const props = defineProps({
  id: { type: String, default: "" },
  type: { type: String, default: "" },
  moduleId: { type: String, default: "" },
  module: { type: String, default: "" },
  name: { type: String, default: "" },
  isShow: { type: Boolean, default: true }
});

const isShow = props.isShow;
// Define model values
const model = ref({
  notes: "",
  type: ""
});

const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([{ name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true },
  { name: "Contributor", label: "Created By", field: "Contributor", align: "left", sortable: true },
  { name: "note", label: "Note", field: "note", align: "left", sortable: true }]
);

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

// ===========================================================
// Mention functionality
// ===========================================================
// const onKeyUpMessage = (event) => {
//   const selection = window.getSelection();
//   const range = selection.rangeCount > 0 ? selection.getRangeAt(0) : null;
//   if (!range) return;

//   const cursorPos = range.startOffset; // Get cursor position inside `q-editor`
//   const textContent = model.value.notes.replace(/<[^>]*>/g, ""); // Remove HTML tags
//   const lastAtPos = textContent.lastIndexOf("@", cursorPos - 1);

//   // Show mention list if "@" is followed by any character
//   if (lastAtPos !== -1 && cursorPos >= lastAtPos + 1) {
//     mentionStart.value = lastAtPos;
//     const searchTerm = textContent.slice(lastAtPos + 1, cursorPos).toLowerCase();

//     // Filter user list based on search term
//     filteredUsers.value = employeeList.value.filter((user) =>
//       searchTerm ? user.text.toLowerCase().includes(searchTerm) : true
//     );

//     showSuggestions.value = filteredUsers.value.length > 0;
//   } else {
//     showSuggestions.value = false;
//   }
// };
// const addMention = (user) => {
//   if (mentionStart.value === -1) return; // No active mention

//   const editor = document.querySelector(".q-editor__content");
//   if (!editor) return;

//   const selection = window.getSelection();
//   const range = selection.rangeCount > 0 ? selection.getRangeAt(0) : null;
//   if (!range) return;

//   // Get the current content
//   const content = model.value.notes;

//   // Preserve text before and after the mention being typed
//   const beforeMention = content.slice(0, mentionStart.value); // Text before "@"
//   const afterMention = content.slice(mentionStart.value); // Text after "@"

//   // Find the end of the mention (first space, punctuation, or end of string)
//   const mentionEndMatch = afterMention.match(/(\s|<\/?[a-z][\s\S]*>|&nbsp;|$)/);
//   const mentionEndIndex = mentionEndMatch ? mentionEndMatch.index : afterMention.length;

//   // Extract remaining text after the mention
//   const remainingText = afterMention.slice(mentionEndIndex);

//   // Format mention
//   const nameParts = user.text.split(" ");
//   const formattedName = `${nameParts[0]} ${nameParts[nameParts.length - 1]}`.trim();
//   const mentionTag = `<span class="tagged-user">@${formattedName}</span>&nbsp;`;

//   // Insert mention at the correct position
//   model.value.notes = beforeMention + mentionTag + remainingText;

//   showSuggestions.value = false;
//   mentionStart.value = -1;

//   // Restore Cursor Position after mention insertion
//   setTimeout(() => {
//     if (editor) {
//       const mentionSpan = editor.querySelector(".tagged-user:last-child");
//       if (mentionSpan && mentionSpan.nextSibling) {
//         range.setStartAfter(mentionSpan.nextSibling);
//         range.setEndAfter(mentionSpan.nextSibling);
//       } else {
//         range.setStartAfter(mentionSpan);
//         range.setEndAfter(mentionSpan);
//       }
//       selection.removeAllRanges();
//       selection.addRange(range);
//     }
//   }, 0);
// };
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

// const addMention = (user) => {
//   if (!mentionRange.value) return;

//   const sel = window.getSelection();
//   sel.removeAllRanges();
//   sel.addRange(mentionRange.value);

//   // delete "@something"
//   mentionRange.value.deleteContents();

//   const parts = user.text.split(" ");
//   const displayName = `@${parts[0]} ${parts[parts.length - 1]}`;

//   const span = document.createElement("span");
//   span.className = "tagged-user";
//   span.textContent = displayName;

//   const space = document.createTextNode("\u00A0");

//   mentionRange.value.insertNode(space);
//   mentionRange.value.insertNode(span);

//   // move cursor after mention
//   mentionRange.value.setStartAfter(space);
//   mentionRange.value.collapse(true);

//   sel.removeAllRanges();
//   sel.addRange(mentionRange.value);

//   showSuggestions.value = false;
//   mentionRange.value = null;
// };
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
  // span.contentEditable = "false"; // prevents typing inside

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

// const extractMentionedUsers = (text) => {
//   const mentionedNames = [...text.matchAll(/@(\w+)/g)].map(match => match[1]);
//   console.log(employeeList);
//   return employeeList.value
//     .filter(emp => mentionedNames.includes(emp.text.split(" ")[0])) // Match only the first name
//     .map(emp => emp.value); // Get user IDs
// };

// const extractMentionedUsers = (text) => {
//   const mentionedNames = [...text.matchAll(/@([\w\s]+)/g)].map(match => match[1].trim().toLowerCase());
//   return employeeList.value.filter(emp => {
//     const empFullName = emp.text.trim().toLowerCase();
//     const empParts = empFullName.split(/\s+/);
//     const empFirstName = empParts[0];
//     const empLastName = empParts[empParts.length - 1];

//     return mentionedNames.some(name => {
//       const mention = name.toLowerCase();
//       return (
//         mention === empFullName || // full match
//         mention === `${empFirstName} ${empLastName}` // first + last match
//       );
//     });
//   }).map(emp => emp.value);
// };
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

const onSubmit = async () => {
  // processing.value = true;
  try {
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
      sub_Module: props.name
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
      options: [
        "p",
        "h1",
        "h2",
        "h3",
        "h4",
        "h5",
        "h6",
        "code"
      ]
    },
    "removeFormat"
  ],
  ["quote", "unordered", "ordered", "outdent", "indent"],

  ["undo", "redo"],
  ["viewsource"]
];

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getAllNoteByTypeAndRecord(props.id, props.type, true);
  }
}, { immediate: true });

onMounted(() => {
  getAllActiveEmployeesListForDropdown();
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
