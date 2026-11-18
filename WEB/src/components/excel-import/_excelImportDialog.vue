<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 95vw !important; max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ config.title }}</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense :disable="isConfirming" @click="onClose" />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <fieldset>
          <legend>Upload File</legend>
          <div class="row q-col-gutter-md items-center">
            <div class="col-12 col-md-4">
              <q-file
                v-model="fileModel"
                outlined
                dense
                clearable
                :accept="config.policy.accept"
                label="Select completed template (.xlsx)"
                :disable="isConfirming || flow.state.phase === 'imported'"
                :loading="isUploading"
                @update:model-value="onFileSelected"
              >
                <template #prepend>
                  <q-icon name="o_upload_file" />
                </template>
              </q-file>
            </div>
            <div class="col-12 col-md-8 text-caption text-grey-8">
              Only .xlsx files from the template, up to {{ maxSizeMb }} MB and {{ config.policy.maxRows }} rows.
              Please select all required fields.
              <!-- {{ config.guidance }} -->
              If any row is invalid, nothing is imported.
              <q-btn flat dense no-caps color="primary" icon="o_download" label="Download Excel Template" class="q-ml-xs" :loading="downloading" @click="onDownloadTemplate" />
            </div>
          </div>
        </fieldset>

        <!-- File / structure errors -->
        <q-banner v-if="flow.state.fileErrors.length" class="bg-red-1 text-negative q-mt-md" rounded>
          <template #avatar><q-icon name="o_error" color="negative" /></template>
          <div v-for="(err, i) in flow.state.fileErrors" :key="i">{{ err.message }}</div>
        </q-banner>

        <!-- Result banner (text only, never HTML) -->
        <q-banner v-if="flow.state.resultMessage" :class="flow.state.resultType === 'positive' ? 'bg-green-1 text-positive q-mt-md' : 'bg-red-1 text-negative q-mt-md'" rounded>
          {{ flow.state.resultMessage }}
        </q-banner>

        <!-- Preview -->
        <fieldset v-if="flow.state.preview" class="q-mt-md">
          <legend>Preview</legend>
          <div class="row q-gutter-sm q-mb-sm items-center">
            <q-chip dense class="bg-grey-3 text-grey-10">Total rows: {{ flow.state.preview.totalRows }}</q-chip>
            <q-chip dense class="bg-green-1 text-positive">Valid: {{ flow.state.preview.validRows }}</q-chip>
            <q-chip dense class="bg-red-1 text-negative">Invalid: {{ flow.state.preview.invalidRows }}</q-chip>
            <q-chip v-for="(chip, i) in summaryChips" :key="'s' + i" dense class="bg-orange-1 text-orange-10">{{ chip }}</q-chip>
            <span v-if="!canConfirmImport && flow.state.phase === 'previewed'" class="text-caption text-negative q-ml-sm">
              Fix every invalid row in the file and upload it again to enable import.
            </span>
          </div>
          <q-table
            class="Custom-DataTable my-sticky-header-table import-preview-table"
            :rows="flow.state.preview.rows"
            :columns="previewColumns"
            row-key="rowNumber"
            separator="cell"
            dense
            flat
            bordered
            no-data-label="No rows found in the file"
            :rows-per-page-options="[20, 50, 100, 200, 500, 0]"
          >
            <template #header="props">
              <q-tr :props="props" class="bg-primary text-white">
                <q-th v-for="col in props.cols" :key="col.name">
                  {{ col.label }}<span v-if="col.required" class="required">*</span>
                </q-th>
              </q-tr>
            </template>
            <template #body="props">
              <q-tr :props="props" :class="rowHasErrors(props.row) ? 'bg-red-1' : ''">
                <q-td class="text-right">
                  {{ props.row.rowNumber - 1 }}
                </q-td>
                <q-td
                  v-for="field in config.fields"
                  :key="field.key"
                  :class="fieldErrors(props.row, field.key).length ? 'import-cell-error' : ''"
                  style="white-space: normal; word-break: break-word; vertical-align: top;"
                >
                  <div v-if="field.multiline" class="import-multiline">{{ props.row[field.key] }}</div>
                  <template v-else-if="field.labelKey">
                    <!-- Resolved: readable label + record ID. Unresolved: the value as entered in the file. -->
                    <template v-if="props.row[field.labelKey]">
                      <div>{{ props.row[field.labelKey] }}</div>
                      <!-- <div class="text-caption text-grey-7">ID: {{ props.row[field.key] }}</div> -->
                    </template>
                    <div v-else>{{ props.row[field.key] }}</div>
                  </template>
                  <div v-else>{{ props.row[field.key] }}</div>
                  <div v-for="(err, i) in fieldErrors(props.row, field.key)" :key="i" class="text-caption text-negative">
                    {{ err.message }}
                  </div>
                </q-td>
                <q-td style="white-space: normal; vertical-align: top;">
                  <q-icon v-if="!rowHasErrors(props.row)" name="o_check_circle" color="positive" size="xs" />
                  <template v-else>
                    <q-icon name="o_error" color="negative" size="xs" />
                    <div v-for="(err, i) in rowLevelErrors(props.row, config.fields)" :key="i" class="text-caption text-negative">{{ err.message }}</div>
                  </template>
                  <div v-if="config.rowNote && config.rowNote(props.row)" class="text-caption text-orange-10">{{ config.rowNote(props.row) }}</div>
                </q-td>
              </q-tr>
              <q-separator />
            </template>
          </q-table>
        </fieldset>
      </div>
      <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
        <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps :disable="isConfirming" @click="onClose" />
        <q-btn
          v-if="flow.state.phase !== 'imported'"
          color="primary"
          push
          outline
          label="Confirm Import"
          type="button"
          class="actionBtn"
          no-caps
          :disable="!canConfirmImport"
          :loading="isConfirming"
          @click="onConfirm"
        />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Shared Excel import dialog (Quasar dialog plugin component).
// Props: `config` (title, fields, policy, messages, guidance, optional rowNote(row) / summary(previewRaw)),
// `service` ({ previewImport(file), confirmImport(rows) }) and `downloadTemplate` (async fn).
// Imported values are rendered with text interpolation only (never v-html).
import { useDialogPluginComponent } from "quasar";
import { ref, reactive, computed } from "vue";
import { notifySuccess, notifyError, zwConfirmLeave } from "assets/utils";
import {
  createImportState,
  createExcelImportFlow,
  rowHasErrors,
  fieldErrors,
  rowLevelErrors
} from "composables/excelImport/excelImportFlow.js";

const props = defineProps({
  config: { type: Object, required: true },
  service: { type: Object, required: true },
  downloadTemplate: { type: Function, required: true }
});

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const fileModel = ref(null);
const downloading = ref(false);
const flow = createExcelImportFlow(props.service, props.config, reactive(createImportState()));

const isUploading = computed(() => flow.state.phase === "uploading");
const isConfirming = computed(() => flow.state.phase === "confirming");
const canConfirmImport = computed(() => flow.canConfirm());
const maxSizeMb = computed(() => Math.round(props.config.policy.maxFileSizeBytes / (1024 * 1024)));
const summaryChips = computed(() => (flow.state.preview && props.config.summary ? props.config.summary(flow.state.preview.raw) : []));

const previewColumns = [
  { name: "rowNumber", label: "Row", field: "rowNumber", align: "center" },
  ...props.config.fields.map(f => ({ name: f.key, label: f.label, field: f.key, align: "left", required: f.required })),
  { name: "status", label: "Status", field: "status", align: "left" }
];

function onFileSelected (file) {
  flow.upload(file || null);
}

async function onConfirm () {
  const result = await flow.confirm();
  if (result.skipped) return;
  if (result.success) {
    // Fixed text + server counts only (notify renders HTML).
    notifySuccess({ message: flow.state.resultMessage });
    onDialogOK({ importedCount: result.importedCount });
  } else {
    notifyError({ message: props.config.messages.importFailed });
  }
}

function onClose () {
  // if (isConfirming.value) return;
  // // Closing/cancelling never sends a confirmation request.
  // flow.reset();
  // onDialogCancel();  
  zwConfirmLeave({ data: "" }, () => {
    onDialogCancel();
  }, () => {
  });
}

async function onDownloadTemplate () {
  downloading.value = true;
  try {
    await props.downloadTemplate();
  } finally {
    downloading.value = false;
  }
}
</script>

<style scoped>
.import-cell-error {
  background: #ffebee;
  box-shadow: inset 0 0 0 1px #c10015;
}
.import-multiline {
  white-space: pre-wrap;
  max-height: 6em;
  overflow: auto;
}
.import-preview-table {
  max-height: 60vh;
}
</style>
