<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1500px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Req. Data Mapping</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form ref="formRef" greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable']">
          <div class="q-gutter-y-md">
            <q-card>              
              <fieldset>
                <legend>Req. Data Mapping Info</legend>
                <div>
                  <div class="row items-end q-mb-sm q-col-gutter-x-md">                    
                    <div class="col-4 col-sm-4 col-md-4">
                      <formSingleSelectDropdown
                        v-model="model.requirementId"
                        label="Requirement"
                        :readonly="true"
                        :required="false"
                        :options="requirementsByProjectModuleIdForDropdown.list.value"
                        :filter="requirementsByProjectModuleIdForDropdown.filter"
                      />
                    </div>
                    <div class="col-8 col-sm-8 col-md-8 flex justify-end">
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
                        <q-td style="white-space: normal; overflow-wrap: break-word; width: 50%;">
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
                            name="o_description"
                            class="cursor-pointer q-mr-sm"
                            size="sm"
                            @click="onRequirementDataMappingNoteEdit(props.row.id)"
                          >
                            <q-tooltip>
                              Manage Notes
                            </q-tooltip>
                          </q-icon>
                          <q-icon
                            name="o_delete"
                            size="sm"
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
                  <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
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
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

import {
  initRequirementDataMappingDialogs,
  onRequirementDataMappingNoteEdit
} from "src/modules/requirement-data-mapping/utils/dialogs.js";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// define props
const props = defineProps({
  id: { type: String, default: "" }
});

// Common variables
const $q = useQuasar();
const { fonts, toolbar, rowToolbar } = getEditorConfig($q);
const loading = ref(true);
const processing = ref(false);
const processingClose = ref(false);
const DataMappingRows = ref([]);
const rowValidations = ref([]);
let requirementId = props.id;

// const activeRowId = null;

const model = ref({
  requirementId: props.id ? requirementId : ""
});

// Data Mapping
const DataMappingColumns = ref([
  { name: "source", label: "Source", field: "source", align: "left", sortable: false },
  { name: "target", label: "Target", field: "target", align: "left" },
  { name: "note", label: "Note", field: "note", align: "left" }
]);

// get data mapping details on edit mode
const getRequirementDataMapping = () => {
  loading.value = true;
  requirementDataMappingService.getRequirementDataMapping(requirementId).then((resp) => {
    DataMappingRows.value = resp.requirementDataMappingsList.map(item => ({
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
  getRequirementDataMapping();
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

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------

const { requirementsByProjectModuleIdForDropdown } = requirementModule();

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

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Save & Next or Save & Close
// --------------------------------------------------------------------------------------------------------------------------------------------------

const onSubmit = async () => {
  processing.value = true;
  try {
    let isValid = true;
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
    const payload = {
      requirementId: model.value.requirementId,
      dataMappings: DataMappingRows.value.map((row) => {
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
    refreshDataMappingList();

  } catch (error) {
    console.error("Error in submitting the data mapping:", error);
    notifyError({ message: "An error occurred while saving the data mapping." });
  } finally {
    setTimeout(() => {
      processing.value = false;
      processingClose.value = false;
    }, 1500);
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => requirementId, (newValue, oldValue) => {
  if (newValue) {
    getRequirementDataMapping();
  }
}, { immediate: true });

onMounted(() => {
  requirementsByProjectModuleIdForDropdown.load('', '');
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
