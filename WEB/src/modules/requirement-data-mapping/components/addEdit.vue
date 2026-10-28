<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1500px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Data Mapping</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form ref="formRef" greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable']">
          <div class="q-gutter-y-md">
            <q-card>
              <fieldset>
                <legend>Data Mapping Info</legend>
                <div>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-sm-4 col-md-4">
                      <formSingleSelectDropdown
                        v-if="!isRequirementReadonly"
                        v-model="model.projectId"
                        label="Project Name"
                        :readonly="isRequirementReadonly"
                        :required="true"
                        :options="projectNameDropdownSingleSelect.list.value"
                        :filter="projectNameDropdownSingleSelect.filter"
                        :error="modelV$.projectId.$error"
                        :error-message="modelV$.projectId.$errors?.[0]?.$message"
                        @blur="modelV$.projectId.$touch()"
                      />
                      <div v-else>
                        <div class="q-mb-xs text-black"><label>Project Name</label></div>
                          <q-input
                            v-model="model.projectName"
                            outlined
                            autogrow
                            readonly
                          />
                      </div>
                    </div>
                    <div class="col-12 col-sm-4 col-md-4">
                      <formSingleSelectDropdown
                        v-if="!isRequirementReadonly"
                        v-model="model.projectModuleId"
                        label="Project Module"
                        :disable="(model.projectId?.length || 0) === 0"
                        :readonly="isRequirementReadonly"
                        :required="true"
                        :options="projectModulesByProjectIdForDropdownSingleSelect.list.value"
                        :filter="projectModulesByProjectIdForDropdownSingleSelect.filter"
                        :error="modelV$.projectModuleId.$error"
                        :error-message="modelV$.projectModuleId.$errors?.[0]?.$message"
                        @blur="modelV$.projectModuleId.$touch()"
                      />
                      <div v-else>
                        <div class="q-mb-xs text-black"><label>Project Module</label></div>
                          <q-input
                            v-model="model.projectModuleName"
                            outlined
                            autogrow
                            readonly
                          />
                      </div>
                    </div>
                    <div class="col-12 col-sm-4 col-md-4">
                      <div class="row items-center no-wrap">
                        <div class="col">
                          <formSingleSelectDropdown
                            v-if="!isRequirementReadonly"
                            v-model="model.requirementId"
                            label="Requirement"
                            :disable="(model.projectModuleId?.length || 0) === 0"
                            :readonly="isRequirementReadonly"
                            :required="true"
                            :options="requirementsByProjectModuleIdForDropdown.list.value"
                            :filter="requirementsByProjectModuleIdForDropdown.filter"
                            :error="modelV$.requirementId.$error"
                            :error-message="modelV$.requirementId.$errors?.[0]?.$message"
                            @blur="modelV$.requirementId.$touch()"
                          />
                          <div v-else>
                            <div class="q-mb-xs text-black"><label>Requirement</label></div>
                              <q-input
                                v-model="model.requirementName"
                                outlined
                                autogrow
                                readonly
                              />
                          </div>
                        </div>
                        <div class="col-auto q-ml-sm q-mt-md">
                          <q-btn
                            v-if="model.requirementId"
                            flat
                            round
                            dense
                            color="primary"
                            icon="o_visibility"
                            size="sm"
                            @click="onRequirementView(model.requirementId)"
                          >
                            <q-tooltip>View Requirement</q-tooltip>
                          </q-btn>
                        </div>
                      </div>
                      <div class="col-auto q-ml-sm q-mt-md">
                      </div>
                    </div>
                  </div>
                  <div class="row items-end q-mb-sm q-mt-md q-col-gutter-x-md">
                    <div class="col-12 col-sm-12 col-md-12 flex justify-end">
                      <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAdd" />
                    </div>
                  </div>
                  <q-table
                    ref="tableRef"
                    class="no-shadow"
                    virtual-scroll bordered
                    :loading="loading"
                    :rows="DataMappingRows"
                    :columns="DataMappingColumns"
                    row-key="id" separator="cell"
                    binary-state-sort
                    :rows-per-page-options="[20, 50, 100, 200, 500]"
                  >
                    <template #header="props">
                      <q-tr :props="props" class="bg-primary text-white">
                        <q-th v-for="col in props.cols" :key="col.name" class="text-start">
                          {{ col.label }}
                          <span v-if="['source', 'target'].includes(col.name)" class="required">*</span>
                        </q-th>
                        <q-th class="text-center">Actions</q-th>
                      </q-tr>
                    </template>
                    <template #body="props">
                      <q-tr :class="props.row.deleted ? 'hidden' : ''">
                        <q-td style="width: 20%;">
                          <q-input
                            v-model="props.row.source"
                            outlined
                            dense
                            maxlength="500"
                            :error="getRowValidation(props.row)?.source?.$error"
                            :error-message="getRowValidation(props.row)?.source?.$errors[0]?.$message"
                            @blur="getRowValidation(props.row)?.source?.$touch()"
                          />
                        </q-td>
                        <q-td style="width: 20%;">
                          <q-input
                            v-model="props.row.target"
                            outlined
                            dense
                            maxlength="500"
                            :error="getRowValidation(props.row)?.target?.$error"
                            :error-message="getRowValidation(props.row)?.target?.$errors[0]?.$message"
                            @blur="getRowValidation(props.row)?.target?.$touch()"
                          />
                        </q-td>
                        <q-td class="hidden" style="white-space: normal; overflow-wrap: break-word; width: 50%;">
                          <q-editor
                          v-model="props.row.note"
                          :dense="$q.screen.lt.md"
                          :toolbar="rowToolbar"
                          :fonts="fonts"
                          class="relative-position"
                          />
                        </q-td>
                        <q-td class="text-center" style="width: 10%;">
                          <q-icon
                            v-if="props.row.id"
                            name="o_visibility"
                            class="cursor-pointer q-mr-sm"
                            size="xs"
                            @click="onRequirementDataMappingNoteEdit(props.row.id, props.row.source, props.row.target, false, refreshDataMappingList)"
                          >
                            <q-tooltip>
                              View Notes
                            </q-tooltip>
                            <!-- <q-badge
                              v-if="props.row.requirementDataMappingNotes > 0"
                              style="position: absolute; right: -16px; top: -15px;"
                              color="green"
                              text-color="white"
                              :label="props.row.requirementDataMappingNotes.length"
                            /> -->
                          </q-icon>
                          <q-icon
                            v-if="props.row.id"
                            name="o_assignment"
                            class="cursor-pointer q-mr-sm"
                            size="xs"
                            @click="onRequirementDataMappingNoteEdit(props.row.id, props.row.source, props.row.target, true, refreshDataMappingList)"
                          >
                            <q-tooltip>
                              Manage Notes
                            </q-tooltip>
                          </q-icon>
                          <q-icon
                            name="o_delete"
                            size="xs"
                            class="cursor-pointer text-red"
                            @click="deleteRow(props.rowIndex)"
                          >
                            <q-tooltip>Delete</q-tooltip>
                          </q-icon>
                        </q-td>
                      </q-tr>
                    </template>
                  </q-table>
                </div>
                <div align="center" class="q-gutter-sm justify-center q-mt-sm">
                  <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                  <!-- <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps /> -->
                  <q-btn
                    color="primary"
                    push
                    outline
                    label="Save and Continue"
                    type="button"
                    class="actionBtn"
                    :loading="processing && activeButton === 'continue'"
                    :disable="processing"
                    no-caps
                    style="width: 160px !important;" @click="onSubmit(false, 'continue')"
                  />
                  <!-- Save and Close -->
                  <q-btn
                    color="primary"
                    push
                    outline
                    label="Save and Close"
                    type="button"
                    class="actionBtn"
                    :loading="processing && activeButton === 'close'"
                    :disable="processing"
                    no-caps
                    @click="onSubmit(true, 'close')"
                  />
                </div>
              </fieldset>
            </q-card>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import { ref, watch, onMounted } from "vue";
import { required, helpers } from "@vuelidate/validators";
import { notifySuccess, notifyError } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import _ from "lodash";

import requirementDataMappingService from "../requirementDataMapping.service";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

// SOP Change :- Shared Dropdowns
import requirementModule from "src/modules/requirement/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

import {
  initRequirementDataMappingDialogs,
  onRequirementDataMappingNoteEdit
} from "src/modules/requirement-data-mapping/utils/dialogs.js";

import {
  initRequirementDialogs,
  onRequirementView
} from "src/modules/requirement/utils/dialogs.js";

// Define emits
// defineEmits([...useDialogPluginComponent.emits]);
const $emit = defineEmits(["hide", "ok"]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// define props
const props = defineProps({
  id: { type: String, default: "" },
  projectId: { type: String, default: "" },
  projectModuleId: { type: String, default: "" },
  isRequirementReadonly: { type: String, default: "" }
});

// Common variables
const $q = useQuasar();
const { fonts, toolbar, rowToolbar } = getEditorConfig($q);
const loading = ref(false);
const processing = ref(false);
const processingClose = ref(false);
const activeButton = ref(""); // 'continue' or 'close'=
const DataMappingRows = ref([]);
const rowValidations = ref([]);
const activeRowId = ref(null);
let requirementId = props.id;
const isRequirementReadonly = props.isRequirementReadonly;

const model = ref({
  requirementId: props.id || "",
  projectId: props.projectId || "",
  projectModuleId: props.projectModuleId || "",
  projectName: "",
  projectModuleName: "",
  requirementName: ""
});

// Data Mapping
const DataMappingColumns = ref([
  { name: "source", label: "Source", field: "source", align: "left", sortable: false },
  { name: "target", label: "Target", field: "target", align: "left" }
  // { name: "note", label: "Note", field: "note", align: "left" }
]);

// get data mapping details on edit mode
const getRequirementDataMapping = (requirementId) => {
  loading.value = true;
  requirementDataMappingService.getRequirementDataMapping(requirementId).then((resp) => {
    const mappings = resp.requirementDataMappingsList || [];

    if (mappings.length > 0) {
      const firstMapping = mappings[0];

      model.value.projectId = firstMapping.requirement?.project?.id || "";
      model.value.projectModuleId = firstMapping.requirement?.projectModule?.id || "";
      model.value.requirementId = firstMapping.requirement?.id || "";

      // Display names
      model.value.projectName =
        firstMapping.requirement?.project?.name || "";

      model.value.projectModuleName =
        firstMapping.requirement?.projectModule?.name || "";

      model.value.requirementName =
        firstMapping.requirement?.title || "";
    }
    DataMappingRows.value = mappings.map(item => ({
      ...item,
      id: item.id,
      editing: false,
      flag: "Edit",
      // Get note from RequirementDataMappingNotes
      note:
        item.requirementDataMappingNotes &&
        item.requirementDataMappingNotes.length > 0
          ? item.requirementDataMappingNotes[0].note || ""
          : "",
      notes: item.requirementDataMappingNotes || [],
      deleted: item.deleted || false
    }));
  }).finally(() => {
    loading.value = false;
  });
};

const refreshDataMappingList = () => {
  getRequirementDataMapping(model.value.requirementId);
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const onAdd = () => {
  DataMappingRows.value.unshift({
    id: "",
    source: "",
    target: "",
    note: "",
    deleted: false,
    notes: []
  });
};

const deleteRow = (index) => {
  if (DataMappingRows.value.filter(row => row.deleted === false).length > 1) {
    DataMappingRows.value[index].deleted = true;
  } else {
    notifyError({ message: "Please add at least one row." });
  }
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initRequirementDataMappingDialogs();
initRequirementDialogs(activeRowId);

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------

const { requirementsByProjectModuleIdForDropdown } = requirementModule();
const { projectNameDropdownSingleSelect } = projectModule();
const { projectModulesByProjectIdForDropdownSingleSelect } = projectModuleOfProjectModule();

// ==================================================================================
// Validation rules
// ==================================================================================

const rowRules = {
  source: { required: helpers.withMessage("Source is required", required) },
  target: { required: helpers.withMessage("Target is required", required) }
};

const getRowValidation = (row) => {
  const index = DataMappingRows.value.findIndex(
    item => item.id === row.id
  );

  return rowValidations.value[index]?.value;
};

const modelRules = {
  requirementId: { required: helpers.withMessage("Requirement is required", required) },
  projectId: { required: helpers.withMessage("Project is required", required) },
  projectModuleId: { required: helpers.withMessage("Project module is required", required) }
};

const modelV$ = useVuelidate(modelRules, model, { $lazy: true, $autoDirty: true });
// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Save & Next or Save & Close
// --------------------------------------------------------------------------------------------------------------------------------------------------

const onSubmit = async (shouldClose, buttonType) => {
  activeButton.value = buttonType;
  // processing.value = true;
  try {
    let isValid = true;

    // validate requirement only when it is not readonly
    if (!isRequirementReadonly) {
      await modelV$.value.$touch();

      const isRequirementValid = await modelV$.value.$validate();

      if(!isRequirementValid) {
        isValid = false;
      }
    }

    const nonDeletedRows = DataMappingRows.value.filter(row => !row.deleted);

    // At least one mapping is required
    if (nonDeletedRows.length === 0) {
      notifyError({ message: "Add at least one data mapping." });
      return;
    }

    rowValidations.value = nonDeletedRows.map((row) =>
      useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true })
    );

    for (const [index, row] of nonDeletedRows.entries()) {
      row.touched = true;
      const validation = useVuelidate(
        rowRules,
        row,
        {
          $lazy: true,
          $autoDirty: true
        }
      );

      rowValidations.value[index] = validation;
      await validation.value.$touch();
      const isRowValid = await validation.value.$validate();
      if (!isRowValid) {
        isValid = false;
      }
    }

    if (!isValid) {
      return;
    }

    processing.value = true;

    const payload = {
      requirementId: model.value.requirementId,
      projectId: model.value.projectId,
      projectModuleId: model.value.projectModuleId,
      // dataMappings: DataMappingRows.value.map((row) => {
      dataMappings: DataMappingRows.value.filter((row) => {
        // Don't send a new blank/deleted row to backend
        if (!row.id && row.deleted) {
          return false;
        }
        return true;
      }).map((row) => {
        const existingNote =
          row.notes && row.notes.length > 0
            ? row.notes[0]
            : null;
        return {
          id: row.id || "",
          source: row.source || "",
          target: row.target || "",
          deleted: row.deleted || false,
          notes: row.note
            ? [
                {
                  id: existingNote?.id || "",
                  requirementDataMappingId: row.id || "",
                  note: row.note,
                  deleted: false
                }
              ]
            : []
        };
      })
    };

    // Save data mappings
    const resp = await requirementDataMappingService.saveRequirementDataMapping(payload);

    notifySuccess({
      message: "Data mapping is saved successfully."
    });
    if (model.value.requirementName) {
      await getRequirementDataMapping(model.value.requirementId);
    }
    $emit("ok");
    if (shouldClose) {
      $emit("hide");
    }

  } catch (error) {
    console.error("Error in submitting the data mapping:", error);
    notifyError({ message: "An error occurred while saving the data mapping." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
      processingClose.value = false;
      activeButton.value = "";
    }, 1500);
  }
};

watch(
  () => model.value.requirementId,
  (newId) => {
    activeRowId.value = newId || null;
  },
  { immediate: true }
);

watch(
  () => model.value.projectId,
  (projectId) => {
    if ((projectId?.length || 0) > 0) {
      projectModulesByProjectIdForDropdownSingleSelect.load(
        false,
        false,
        projectId
      );
    } else {
      model.value.projectModuleId = null;
      projectModulesByProjectIdForDropdownSingleSelect.list.value = [];
      requirementsByProjectModuleIdForDropdown.list.value = [];
      modelV$.value.projectModuleId.$reset();
    }
  },
  { immediate: true }
);

watch(
  () => model.value.projectModuleId,
  (projectModuleId) => {
    if ((projectModuleId?.length || 0) > 0 && (model.value.projectId?.length || 0) > 0) {
      requirementsByProjectModuleIdForDropdown.load(
        projectModuleId,
        model.value.projectId,
        "Data Integration"
      );
    } else {
      model.value.requirementId = "";
      requirementsByProjectModuleIdForDropdown.list.value = [];
    }
  },
  { immediate: true }
);

// watches a data property with the same name i.e. immediate effect
watch(() => requirementId, (newValue, oldValue) => {
  if (newValue) {
    getRequirementDataMapping(requirementId);
  }
}, { immediate: true });

watch(
  () => model.value.requirementId,
  (newId, oldId) => {
    if (newId && newId !== oldId) {
      getRequirementDataMapping(newId);
    } else if (!newId) {
      DataMappingRows.value = [];
    }
  }
);

onMounted(() => {
  // requirementsByProjectModuleIdForDropdown.load(model.value.projectModuleId, model.value.projectId, 'Data Integration');
  projectNameDropdownSingleSelect.load();
});

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_tasks .q-select__dropdown-icon{
  display: none;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
.wrap-text {
  max-width: 250px;
}
</style>
