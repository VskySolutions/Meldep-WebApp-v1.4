<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:65vw !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Question Answers</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset v-if="!showResponseLog">
              <div class="row q-col-gutter-x-md q-mb-md">
                <formSingleSelectDropdown
                  v-model="model.projectId"
                  label="Project Name"
                  :options="projectNameDropdownSingleSelect.list.value"
                  :filter="projectNameDropdownSingleSelect.filter"
                  wrapper-class="col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12"
                  :error="v$.projectId.$error"
                  :error-message="v$.projectId.$errors[0]?.$message"
                />
                <formSingleSelectDropdown
                  v-model="model.requirementId"
                  label="Requirement"
                  :disable="!model.projectId"
                  :options="requirementByProjectModuleIdForDropdownSingleSelect.list.value"
                  :filter="requirementByProjectModuleIdForDropdownSingleSelect.filter"
                  wrapper-class="col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12"
                  :error="v$.requirementId.$error"
                  :error-message="v$.requirementId.$errors[0]?.$message"
                />
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="text-black">Question<span class="required">*</span></div>
                  <q-input
                    v-model="model.title"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :error="v$.title.$error"
                    :error-message="v$.title.$errors[0]?.$message"
                    @click="v$.title.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-12 col-md-12 col-lg-12">
                  <div class="text-black"><label>Answer</label></div>
                  <div class="form-group">
                    <q-editor
                      v-model="model.description"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset v-if="props.id" class="q-mb-lg">
              <legend>Response Log</legend>
              <div class="flex items-center justify-end q-mb-md">
                <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAddResponseLog" />
              </div>
              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                bordered class="no-shadow"
                :loading="loading"
                :rows="logRows"
                :columns="logColumns"
                row-key="id"
                separator="cell"
                :hide-no-data="mode === 'addChangeLog'"
                no-data-label="No data available"
                :rows-per-page-options="[20, 50, 100, 200, 500]"
                binary-state-sort
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th
                      v-for="col in props.cols"
                      :key="col.name" :props="props"
                    >{{ col.label }}
                      <span v-if="['requirementLogDate','employeeId','requirementName'].includes(col.name)" class="required">*</span>
                    </q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #top-row>
                  <q-tr v-if="mode == 'addChangeLog' && editingLogRow" class="row-highlight">
                    <q-td>
                      <q-editor
                        v-model="editingLogRow.description"
                        :dense="$q.screen.lt.md"
                        :toolbar="toolbar"
                        :fonts="fonts"
                        @blur="editingLogRowV$.description.$touch()"
                      />

                      <div
                        v-if="
                          mode === 'addChangeLog' &&
                          editingLogRowV$.description.$dirty &&
                          editingLogRowV$.description.$error
                        "
                        class="text-negative text-caption q-mt-xs"
                      >
                        {{ editingLogRowV$.description.$errors[0].$message }}
                      </div>
                    </q-td>
                    <q-td>{{ getCreatedBy(props.row) }}</q-td>
                    <q-td>
                      {{ formatDateTime(getCreatedOn(props.row)) }}
                    </q-td>
                    <q-td auto-width class="text-center">
                      <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                        <q-tooltip>Save</q-tooltip>
                      </q-icon>
                      <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                        <q-tooltip>Cancel</q-tooltip>
                      </q-icon>
                    </q-td>
                  </q-tr>
                  <q-separator></q-separator>
                </template>
                <template #body="props">
                  <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                    <q-td class="text-left">
                      <q-editor
                        v-if="mode == 'editLog' && editingLogRow && props.row.id === activeRowId"
                        v-model="editingLogRow.description"
                        :dense="$q.screen.lt.md"
                        :toolbar="toolbar"
                        :fonts="fonts"
                        :error="editingLogRowV$.description.$error" :error-message="editingLogRowV$.description.$errors[0]?.$message" @blur="editingLogRowV$.description.$touch"
                      />
                      <span
                        v-else :class="props.row.deleted ? 'text-delete RichTextEditor' : 'RichTextEditor'"
                        style="display: block; overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 350px;"
                        v-html="props.row.description"
                      />
                      <div
                        v-if="
                          mode === 'editLog' &&
                          props.row.id === activeRowId &&
                          editingLogRowV$.description.$dirty &&
                          editingLogRowV$.description.$error
                        "
                        class="text-negative text-caption q-mt-xs"
                      >
                        {{ editingLogRowV$.description.$errors[0].$message }}
                      </div>
                    </q-td>
                    <q-td>{{ getCreatedBy(props.row) }}</q-td>
                    <q-td>
                      {{ formatDateTime(getCreatedOn(props.row)) }}
                    </q-td>
                    <q-td auto-width class="text-center">
                      <template v-if="mode == 'editLog' && editingLogRow && props.row.id === activeRowId">
                        <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                          <q-tooltip>Save</q-tooltip>
                        </q-icon>
                        <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                          <q-tooltip>Cancel</q-tooltip>
                        </q-icon>
                      </template>
                      <template v-else>
                      <q-icon
                        v-if="!props.row.deleted"
                        name="o_edit"
                        size="xs"
                        color="primary"
                        class="cursor-pointer q-mr-md"
                        @click="onEditLog(props.row)"
                      >
                        <q-tooltip>Edit</q-tooltip>
                      </q-icon>
                        <q-icon v-if="!props.row.deleted" name="o_delete_outline" color="red" size="xs" class="cursor-pointer" @click="onDeleteLog(props.row)">
                          <q-tooltip>Delete</q-tooltip>
                        </q-icon>
                        <q-icon v-if="props.row.deleted" name="o_redo" size="xs" class="cursor-pointer" @click="onUndo(props.row)">
                          <q-tooltip>Undo</q-tooltip>
                        </q-icon>
                      </template>
                    </q-td>
                  </q-tr>
                </template>
              </q-table>
            </fieldset>
          </div>
        </div>
        <q-separator />
        <q-card-actions align="center" class="q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import _ from "lodash";
import { notifySuccess, notifyError } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import { ref, onMounted, watch } from "vue";
import { uid, date } from "quasar";
import { useAuthStore } from "stores/auth";
import { useQuasar, useDialogPluginComponent } from "quasar";
import { required, helpers, maxLength } from "@vuelidate/validators";

import projectQuestionsAnswersService from "modules/project-questions-answers/projectQuestionsAnswers.service";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// Props values i.e. come from query string
// const props = defineProps({ id: { type: String, default: "" } });
const props = defineProps({
  id: {
    type: String,
    default: ""
  },
  projectIdAttr: { type: String, default: "" },
  requirementIdAttr: { type: String, default: "" },
  showResponseLog: {
    type: Boolean,
    default: false
  }
});


const $emit = defineEmits(["hide", "ok"]);
const authStore = useAuthStore();
const user = authStore.user;
const currentDate = ref(new Date());
const showResponseLog = ref(props.showResponseLog);

const formatDateTime = (value) =>
  value ? date.formatDate(value, "MM/DD/YYYY hh:mm A") : "";

  const getCreatedBy = (row) => {
  if (mode.value === "editLog" && row.id === activeRowId.value) {
    return editingLogRow.value.flag === "New"
      ? `${user.firstName} ${user.lastName}`
      : editingLogRow.value.createdBy?.person?.fullName;
  }

  // return row.value.createdBy?.person?.fullName;
  return row?.createdBy?.person?.fullName || '';
};

const getCreatedOn = (row) => {
  if (mode.value === "editLog" && row.id === activeRowId.value) {
    return editingLogRow.value.flag === "New"
      ? currentDate.value
      : editingLogRow.value.createdOnUtc;
  }

  return row?.createdOnUtc;
};
// Common variables
const loading = ref(true);
const processing = ref(false);
const isInitializing = ref(false);
const mode = ref(null);
const activeRowId = ref(null);
const editingLogRow = ref(null);
const logRows = ref([]);
const pagination = ref({
  sortBy: "createdOnUtc",
  descending: true,
  rowsPerPage: 20,
  page: 1
});

// Define model values
const model = ref({
  id: "",
  title: "",
  projectId: props.projectIdAttr || props.projectIdValue || null,
  requirementId: props.requirementIdAttr || "",
  description: ""
});

// ----------------------------------------------------------------------------------------------------------------
// Define columns for Response Change log
// ----------------------------------------------------------------------------------------------------------------

const logColumns = ref([
  { name: "description", label: "Answer", field: "description", align: "left", sortable: true },
  { name: "createdBy.person.fullname", label: "Created By", field: "createdBy.person.fullname", align: "left", sortable: true },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true }
]);
// ==================================================================

const getQuestionAnswersInDetailsById = async (questionAnswersId) => {
  loading.value = true;
  isInitializing.value = true;

  try {
    const resp = await projectQuestionsAnswersService.getQuestionAnswersInDetailsById(questionAnswersId);

    model.value = _.cloneDeep(resp);
    model.value.projectId = resp.project?.id;

    await requirementByProjectModuleIdForDropdownSingleSelect.load("", model.value.projectId);

    model.value.requirementId = resp.requirement?.id;
    model.value.description = resp.description ?? "";

    logRows.value = (resp.projectQuestionsAnswersResponseLog ?? []).map(item => ({
      ...item,
      editing: false,
      flag: "Edit"
    }));

  } finally {
    isInitializing.value = false;
    loading.value = false;
  }
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectNameDropdownSingleSelect
} = projectModule();

const { requirementByProjectModuleIdForDropdownSingleSelect } = requirementModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Validation Rules
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Question Answers Info - Validation Rules
const rules = {
  projectId: { required: helpers.withMessage("Project is required", required) },
  requirementId: {
    required: helpers.withMessage("Requirement is required", required)
  },
  title: { required: helpers.withMessage("Question is required", required), maxLength: maxLength(500) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Response Log
// --------------------------------------------------------------------------------------------------------------------------------------------------

function onAddResponseLog() {
  if (editingLogRow.value) {
    editingLogRowV$.value.$touch();

    notifyError({
      message: "Please save the current response log first."
    });

    return;
  }
  editingLogRowV$.value.$reset();
  mode.value = "addChangeLog";
  activeRowId.value = null;

  editingLogRow.value = {
    id: uid(),
    description: "",
    createdOnUtc: new Date(),
    createdBy: {
      person: {
        fullname: `${user.firstName} ${user.lastName}`
      }
    },
    flag: "New"
  };

  editingLogRowV$.value.$reset();
}

function onCancel() {
  mode.value = null;
  activeRowId.value = null;
  editingLogRow.value = null;
  editingLogRowV$.value.$reset();
}

function onUndo(item) {
  const rowIndex = logRows.value.findIndex(row => row.id === item.id);
  if (rowIndex === -1) return;

  logRows.value[rowIndex] = {
    ...logRows.value[rowIndex],
    deleted: false,
    flag: "Edit"
  };

  activeRowId.value = null;
}

function onEditLog(row) {
  if (editingLogRow.value) {
    editingLogRowV$.value.$touch();

    notifyError({
      message: "Please save the current response log first."
    });

    return;
  }
  editingLogRowV$.value.$reset();
  mode.value = "editLog";
  activeRowId.value = row.id;

  editingLogRow.value = _.cloneDeep(row);

  editingLogRowV$.value.$reset();
}

function onDeleteLog(item) {
  const rowIndex = logRows.value.findIndex(row => row.id === item.id);
  if (rowIndex === -1) return;

  // If it's a newly added row, remove it completely
  if (logRows.value[rowIndex].flag === "New") {
    logRows.value.splice(rowIndex, 1);
    return;
  }

  logRows.value[rowIndex] = {
    ...logRows.value[rowIndex],
    deleted: true,
    flag: "Delete"
  };

  activeRowId.value = item.id;
}

// ----------------------------------------------------------------------------------------------------------------
// Response Change Log - Validation Rules
// ----------------------------------------------------------------------------------------------------------------
function stripHtml(html = "") {
  return html
    .replace(/<[^>]*>/g, "")
    .replace(/&nbsp;/g, " ")
    .trim();
}

function hasImage(html = "") {
  return /<img\b[^>]*>/i.test(html);
}

const requiredEditor = helpers.withMessage(
  "Answer is required",
  (value) => {
    if (!value) return false;

    const text = stripHtml(value);
    const imageExists = hasImage(value);

    return text.length > 0 || imageExists;
  }
);

const editingLogRowRules = {
  description: {
    requiredEditor
  }
};

const editingLogRowV$ = useVuelidate(editingLogRowRules, editingLogRow, { $lazy: true, $autoDirty: true });

async function onSave() {
  editingLogRowV$.value.$touch();

  if (!await editingLogRowV$.value.$validate()) {
    return;
  }

  const duplicate = logRows.value.some(item =>
    item.id !== editingLogRow.value.id &&
    item.flag !== "Delete" &&
    item.description.trim().toLowerCase() ===
      editingLogRow.value.description.trim().toLowerCase()
  );

  if (duplicate) {
    notifyError({ message: "Duplicate Answer." });
    return;
  }

  if (mode.value === "addChangeLog") {
    logRows.value.unshift({
      ...editingLogRow.value
    });
  }

  if (mode.value === "editLog") {
    const index = logRows.value.findIndex(
      x => x.id === editingLogRow.value.id
    );

    if (index > -1) {
      logRows.value.splice(index, 1, {
        ...editingLogRow.value,
        flag:
          logRows.value[index].flag === "New"
            ? "New"
            : "Edit"
      });
    }
  }

  mode.value = null;
  activeRowId.value = null;
  editingLogRow.value = null;
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Submit form
// --------------------------------------------------------------------------------------------------------------------------------------------------
const hasPendingChanges = () => {
  if (!editingLogRow.value) {
    return false;
  }

  if (mode.value === "addChangeLog") {
    return true;
  }

  if (mode.value === "editLog") {
    return true;
  }

  return false;
};

async function onSubmit() {
  processing.value = true;

  try {
    if (!await v$.value.$validate()) {
      return;
    }

    if (hasPendingChanges()) {
      editingLogRowV$.value.$touch();
      await editingLogRowV$.value.$validate();

      notifyError({
        message: "Please save the current response log first."
      });

      return;
    }

    model.value.projectQuestionsAnswersResponseLogs = logRows.value;

    await projectQuestionsAnswersService.saveQuestionAnswers(
      props.id,
      model.value
    );

    notifySuccess({
      message: "Project question answer saved successfully."
    });

    mode.value = null;
    activeRowId.value = null;
    editingLogRow.value = null;
    editingLogRowV$.value.$reset();

    $emit("ok");
    $emit("hide");
  } catch (error) {
    console.error(error);
    notifyError({
      message: "An error occurred while saving."
    });
  } finally {
    processing.value = false;
  }
}

watch(
  () => model.value.projectId,
  async (newValue) => {
    if (!isInitializing.value) {
      model.value.requirementId = "";
    }
    if (!newValue) return;

    await requirementByProjectModuleIdForDropdownSingleSelect.load("", newValue);
  }, { immediate: true }
);

watch(
  () => props.id,
  async (newValue) => {
    if (!newValue) return;

    await getQuestionAnswersInDetailsById(newValue);
  },
  {
    immediate: true
  }
);
// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  await projectNameDropdownSingleSelect.load();

  if (model.value.projectId) {
    await requirementByProjectModuleIdForDropdownSingleSelect.load("", model.value.projectId);
  }
  if (props.requirementIdAttr) {
    model.value.requirementId = props.requirementIdAttr;
  }
});

</script>
<style>
.ellipsis-cell {
  max-width: 260px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.same-size-btn {
  min-width: 150px;
  height: 50px;
}
</style>
