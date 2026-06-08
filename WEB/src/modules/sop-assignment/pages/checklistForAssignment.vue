<template>
  <q-page padding>
    <q-card class="q-pa-md">
      <div class="row items-center justify-between q-mb-md">
        <div class="text-h2 text-weight-bold text-primary">
          {{ model.name }}
        </div>
        <q-btn
          icon="o_chevron_left"
          outline
          label="Back"
          no-caps
          class="text-primary btnRounded"
          @click="$router.back()"
        />
      </div>
      <div class="row q-col-gutter-md q-mb-md">
        <div v-for="(field, i) in infoFields" :key="i" class="col-12 col-sm-6 col-md-3">
          <div class="label text-primary">{{ field.label }}</div>
          <div class="value">{{ field.value || '-' }}</div>
        </div>
      </div>
      <div class="q-mb-lg">
        <div class="label text-primary">Description</div>
        <div class="description-box" v-html="model.description || '-'" />
      </div>
      <div class="row items-center q-col-gutter-md text-bold text-grey-7 q-pb-sm">
        <div :class="`col-${colSizes.checklist}`">Check list</div>
        <div :class="`col-${colSizes.response}`">Response</div>
        <div :class="`col-${colSizes.evidence}`">Evidence</div>
        <div v-if="isRejected" :class="`col-${colSizes.comment}`">Approver Comment</div>
        <div :class="`col-${colSizes.updated}`">Updated</div>
      </div>
      <q-separator class="q-mb-sm" />
      <div
        v-for="(section) in model.sopTemplateSections"
        :key="section.id"
      >
        <div class="section-header bg-primary text-white">
          <div class="fs-12 section-title">{{ section.name }}</div>
          <!-- <div class="fs-10">
            {{ section.description || '' }}
          </div> -->
          <div
            class="fs-10"
            v-html="truncateText(section.description, 500)"
          />
        </div>
        <div
          v-for="item in section.sopTemplateSectionItems"
          :key="item.id"
          :class="[
            'row items-center q-py-xs checklist-row',
            isRejected && item.isApproved === false ? 'bg-red-1' : ''
          ]"
        >
          <div :class="`col-${colSizes.checklist}`" class="row items-start q-gutter-sm">
            <!-- Checkbox -->
            <div class="checkbox-slot">
              <q-checkbox
                :model-value="getCheckboxValue(item)"
                :disable="item.type !== 'checkbox' || !isEditable(item)"
                @update:model-value="val => onCheckboxChange(val, item)"
              />
            </div>
            <!-- Text -->
            <div>
              <div class="fs-14 text-weight-bold">
                {{ item.name }}
              </div>
              <div
                class="fs-12 text-grey-7"
                v-html="truncateText(item.description, 500)"
              />
            </div>
          </div>
          <div :class="`col-${colSizes.response}`">
            <template v-if="item.type === 'text'">
              <q-input
                v-if="isEditable(item)"
                v-model="item.response"
                dense outlined
                :error="!!item.responseError"
                :error-message="item.responseError"
              />
              <div v-else class="readonly-text">
                {{ item.response || '-' }}
              </div>
            </template>
            <template v-else-if="item.type === 'number'">
              <q-input
                v-if="isEditable(item)"
                v-model="item.response"
                type="number"
                dense
                outlined
                hint="Only numbers allowed"
                :error="!!item.numberError"
                :error-message="item.numberError"
              />
              <div v-else class="readonly-text">
                {{ item.response || '-' }}
              </div>
            </template>
            <template v-else-if="item.type === 'dropdown'">
              <formSingleSelectDropdown
                v-if="isEditable(item)"
                v-model="item.response"
                :options="item.options"
                :filter="item.options.filter"
                :error="!!item.responseError"
                :error-message="item.responseError"
              />
              <div v-else class="readonly-text">
                -
              </div>
            </template>
          </div>
          <div v-if="item.isRequiredEvidence" :class="`col-${colSizes.evidence}`" class="q-ml-sm">
            <q-icon
              v-if="item.isRequiredEvidence && isEditable(item)"
              name="o_attach_file"
              size="xs"
              class="cursor-pointer"
              @click="openUpload(item)"
            >
              <q-tooltip>
                Upload Evidences
              </q-tooltip>
            </q-icon>
            <q-dialog v-model="item.showUploader" persistent position="right">
              <q-card class="instruction-popup" style="min-width:500px; max-width:600px">
                <q-card-section class="row items-center justify-between q-pb-none">
                  <div class="text-subtitle2">
                    Upload Evidences
                  </div>
                  <q-btn
                    v-close-popup
                    icon="o_close"
                    flat
                    round
                    dense
                    size="sm"
                  />
                </q-card-section>
                <q-card-section>
                  <div class="editor-wrapper relative-position">
                    <multiFileUploader
                      :initial-files="item.tempFiles"
                      :allowed-extensions="[
                        '.jpg','.xls','.xlsx','.doc','.docx','.jpeg','.png','.ppt','.pptx'
                      ]"
                      :max-size-in-mb="25"
                      label="Drag files here or (+) to upload."
                      @files-selected="files => handleTempFiles(files, item)"
                    />
                    <q-inner-loading
                      v-if="item.isUploading"
                      showing
                      class="absolute-full"
                      color="primary"
                      size="30px"
                    />
                  </div>
                </q-card-section>
                <q-card-actions class="q-mt-sm flex justify-center">
                  <q-btn
                    v-close-popup
                    label="Cancel"
                    flat @click="item.showUploader = false"
                  />
                  <q-btn
                    label="Save"
                    text-color="blue"
                    unelevated
                    :loading="item.isUploading"
                    :disable="item.isUploading"
                    @click="handleEvidenceSave(item)"
                  />
                </q-card-actions>
              </q-card>
            </q-dialog>
            <div v-if="item.files?.length" class="row items-center q-gutter-sm q-mt-xs">
              <div
                v-for="(f, i) in item.files"
                :key="i"
                class="file-wrapper"
              >
                <a :href="f.url" target="_blank">
                  <q-tooltip>
                    {{ f.fileName }}
                  </q-tooltip>

                  <q-icon
                    :name="getFileIcon(f.fileName)"
                    size="30px"
                    :class="f.isNew ? 'text-primary' : 'text-grey-7'"
                    class="hover-icon"
                  />
                </a>
                <q-icon
                  v-if="isEditable(item)"
                  name="o_cancel"
                  size="16px"
                  color="red"
                  class="remove-icon"
                  @click.stop="removeFile(item, i)"
                />
              </div>
            </div>
            <div v-if="item.evidenceError" class="text-negative text-caption q-mb-xs">
              {{ item.evidenceError }}
            </div>
          </div>
          <div v-else :class="`col-${colSizes.evidence}`" class="q-ml-sm"> - </div>
          <div v-if="isRejected && item.isApproved === false" :class="`col-${colSizes.comment} text-negative`">
            {{ item.approvedComment || '-' }}
          </div>
          <div v-else-if="isRejected" :class="`col-${colSizes.comment}`">-</div>
          <div :class="`col-${colSizes.updated}`">
            <div class="row items-center no-wrap q-gutter-sm">
              <q-avatar size="32px" color="primary" text-color="white">
                {{ getInitials(item.updatedBy?.person?.fullName) }}
              </q-avatar>
              <div>
                <div class="text-grey-8 text-weight-medium">
                  {{ item.updatedBy?.person?.fullName || '-' }}
                </div>
                <div class="text-grey-6 text-caption">
                  {{ item.updatedOnUtc || '-' }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </q-card>
    <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
      <q-btn color="grey-4" label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="$router.back();" />
      <q-btn
        color="primary"
        label="Save & Close"
        class="actionBtn"
        :loading="processingSave"
        no-caps
        @click="onSubmit('save')"
      />

      <q-btn
        color="primary"
        label="Save & Submit"
        class="actionBtn"
        :loading="processingSubmit"
        no-caps
        @click="onSubmit('submit')"
      />
    </q-card-actions>
  </q-page>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useRouter } from "vue-router";
import { notifySuccess, notifyError, zwConfirm } from "assets/utils";
import sopAssignmentService from "modules/sop-assignment/sopAssignment.service";

// Shared Inputs
import multiFileUploader from "src/components/form-inputs/_multiFileUpload.vue";
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

const selectedAssignmentId = history.state?.id;
const router = useRouter();

const loading = ref(false);
const processingSave = ref(false);
const processingSubmit = ref(false);
const model = ref({
  name: "",
  sopTemplateSections: []
});

const infoFields = computed(() => [
  { label: "Template Name", value: model.value.template?.name },
  { label: "Assigned To", value: model.value.assignedToEmployee?.person.fullName },
  { label: "Approver", value: model.value.approverEmployee?.person.fullName },
  { label: "Priority", value: model.value.priority?.dropDownValue },
  { label: "Status", value: model.value.status?.dropDownValue },
  { label: "Assigned Date", value: model.value.assignedDate },
  { label: "Due Date", value: model.value.dueDate }
]);

const isRejected = computed(() =>
  model.value.status?.dropDownValue === "Rejected"
);
const isInProgress = computed(() =>
  model.value.status?.dropDownValue === "In Progress"
);

const isEditable = (item) => {
  // In Progress then everything editable
  if (isInProgress.value) return true;

  // Rejected then only rejected items editable
  if (isRejected.value && item.isApproved === false) return true;

  return false;
};

const truncateText = (text, limit = 500) => {
  if (!text) return "";

  return text.length > limit
    ? text.substring(0, limit) + "..."
    : text;
};

const colSizes = computed(() => {
  return isRejected.value
    ? {
      checklist: 3,
      response: 2,
      evidence: 3,
      comment: 2,
      updated: 2
    }
    : {
      checklist: 4,
      response: 3,
      evidence: 3,
      updated: 2
    };
});

const mapType = (item) => {
  switch (item.inputType.dropDownValue) {
  case "CheckBox": return "checkbox";
  case "Number Field": return "number";
  case "Dropdown": return "dropdown";
  default: return "text";
  }
};

const getDefaultResponse = (type) => {
  if (type === "checkbox") return false;
  if (type === "number") return null;
  if (type === "dropdown") return null;
  return "";
};

const getInitials = (name) => {
  if (!name) return "-";

  return name
    .split(" ")
    .map(n => n[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);
};

const getFileIcon = (fileName) => {
  if (!fileName) return "o_insert_drive_file";

  const ext = fileName.split(".").pop().toLowerCase();

  switch (ext) {
  case "pdf": return "o_picture_as_pdf";
  case "doc":
  case "docx": return "o_description";
  case "xls":
  case "xlsx": return "o_table_chart";
  case "png":
  case "jpg":
  case "jpeg":
  case "gif": return "o_image";
  case "zip":
  case "rar": return "o_folder_zip";
  case "txt": return "o_article";
  default: return "o_insert_drive_file";
  }
};

function openUpload (item) {
  item.tempFiles = [...(item.files || [])]; // 👈 temp copy
  item.showUploader = true;
}

function getCheckboxValue (item) {
  // If response checkbox then use response
  if (item.type === "checkbox") {
    return !!item.response;
  }

  // auto check based on evidence
  if (item.isRequiredEvidence) {
    return (item.files?.length || 0) > 0;
  }

  return false;
}

function onCheckboxChange (val, item) {
  // Allow only manual checkbox items
  if (item.type === "checkbox") {
    item.response = val;
  }
}

// ===================================================
// file add, remove
// ===================================================
// Add file
function handleTempFiles (files, item) {
  if (!item.tempFiles) item.tempFiles = [];

  const normalized = files.map(f => {
    if (f instanceof File) {
      return {
        file: f,
        fileName: f.name,
        url: URL.createObjectURL(f),
        isNew: true
      };
    }

    return {
      id: f.id,
      fileId: f.fileId,
      fileName: f.fileName,
      url: f.url,
      file: null,
      isNew: false
    };
  });

  // 🔥 prevent duplicates by fileName
  const existingNames = new Set(item.tempFiles.map(f => f.fileName));

  const newUniqueFiles = normalized.filter(
    f => !existingNames.has(f.fileName)
  );

  item.tempFiles = [...item.tempFiles, ...newUniqueFiles];
}
const handleEvidenceSave = (item) => {
  item.files = [...(item.tempFiles || [])]; // 👈 save here
  item.showUploader = false;
};
// Remove file
function removeFile (item, index) {
  zwConfirm({ message: "Do you want to remove this file?" }, () => {
    const removed = item.files[index];

    if (removed?.id) {
      item.deletedFiles.push(removed);
    }

    item.files.splice(index, 1);
  });
}
// ===================================================
const parseValidationJson = (val) => {
  if (!val) return null;

  try {
    // only parse if it looks like JSON
    if (typeof val === "string" && val.trim().startsWith("{")) {
      return JSON.parse(val);
    }
    return null;
  } catch (e) {
    console.warn("Invalid validationJson:", val);
    return null;
  }
};

const getSOPTemplateSectionItemsByAssignmentId = async () => {
  loading.value = true;

  try {
    const assignmentResp = await sopAssignmentService.getSOPAssignmentByIdInDetail(selectedAssignmentId);

    const responses = assignmentResp?.sopAssignmentResponses || [];
    const sections = assignmentResp?.template?.sopTemplateSections || [];

    model.value = {
      ...(assignmentResp || {}),

      templateId: assignmentResp?.templateId,
      name: assignmentResp?.name,

      sopTemplateSections: sections.map(section => ({
        ...section,

        sopTemplateSectionItems: section.sopTemplateSectionItems?.map(item => {
          const type = mapType(item);

          const existing = responses.find(r => r.sectionItemId === item.id);

          return {
            ...item,
            type,

            isMandatory: item.isMandatory,
            isRequiredEvidence: item.isRequiredEvidence, // already using
            validationJson: parseValidationJson(item.validationJson),

            response: existing
              ? (type === "checkbox" ? existing.isChecked : existing.response)
              : getDefaultResponse(type),

            responseId: existing?.id || null,
            isApproved: existing?.isApproved ?? false,
            approvedComment: existing?.approvedComment || "",

            responseError: "",
            numberError: "",
            evidenceError: "",
            updatedBy: existing?.updatedBy ?? item.updatedBy ?? null,
            updatedOnUtc: existing?.updatedOnUtc ?? item.updatedOnUtc ?? "",
            options: type === "dropdown"
              ? (item.options?.length
                ? item.options.map(opt => ({
                  label: opt.label ?? opt,
                  value: opt.value ?? opt
                }))
                : [])
              : [],
            files: existing?.sopAssignmentResponseEvidences?.map(e => ({
              id: e.id,
              fileId: e.fileId,
              fileName: e.file?.seoFilename,
              url: e.file?.virtualPath
            })) || [],

            originalFiles: existing?.sopAssignmentResponseEvidences?.map(e => ({
              id: e.id,
              fileId: e.fileId,
              fileName: e.file?.seoFilename,
              url: e.file?.virtualPath
            })) || [],

            deletedFiles: []
          };
        })
      }))
    };
  } catch (err) {
    console.error(err);
  } finally {
    loading.value = false;
  }
};

const validateForm = () => {
  let isValid = true;

  model.value.sopTemplateSections.forEach(section => {
    section.sopTemplateSectionItems.forEach(item => {
      item.responseError = "";
      item.numberError = "";
      item.evidenceError = "";

      const val = item.response;
      const rules = item.validationJson;

      // Required Check
      let isEmpty = false;
      if (item.type === "checkbox") {
        isEmpty = false;
      } else if (item.type === "dropdown") {
        isEmpty = val === null || val === undefined;
      } else {
        isEmpty = val === null || val === undefined || String(val).trim() === "";
      }

      if (item.isMandatory && isEmpty) {
        item.responseError = "This field is required";
        if (item.type === "number") {
          item.numberError = "This field is required and Only valid numbers are allowed";
        }
        isValid = false;
      }

      // Number Validation
      if (item.type === "number" && val !== null && String(val).trim() !== "") {
        const num = Number(val);

        if (isNaN(num)) {
          item.numberError = "Only valid numbers are allowed";
          isValid = false;
        } else if (rules) {
          if (rules.min !== undefined && num < rules.min) {
            item.numberError = `Minimum value is ${rules.min}`;
            isValid = false;
          }
          if (rules.max !== undefined && num > rules.max) {
            item.numberError = `Maximum value is ${rules.max}`;
            isValid = false;
          }
        }
      }

      // Evidence Validation
      if (item.isRequiredEvidence) {
        const hasValidFile = item.files?.some(f => f.file || f.url); // check either uploaded file or existing file
        if (!hasValidFile) {
          item.evidenceError = "Evidence is required";
          isValid = false;
        }
      }
    });
  });

  return isValid;
};

const onSubmit = async (type) => {
  if (!validateForm()) {
    notifyError({ message: "Please fill all required fields." });
    return;
  }

  if (type === "save") processingSave.value = true;
  else processingSubmit.value = true;

  try {
    const formData = new FormData();
    formData.append("isSubmitted", type === "submit");
    formData.append("Id", selectedAssignmentId || "");
    formData.append("Name", model.value.name || "");
    formData.append("ActionType", type);

    let index = 0;

    model.value.sopTemplateSections.forEach(section => {
      section.sopTemplateSectionItems.forEach(item => {
        formData.append(`SOPAssignmentResponses[${index}].Id`, item.responseId || "");
        formData.append(`SOPAssignmentResponses[${index}].SectionItemId`, item.id);

        if (item.type === "checkbox") {
          formData.append(
            `SOPAssignmentResponses[${index}].Response`,
            item.response ? "true" : "false"
          );
          formData.append(
            `SOPAssignmentResponses[${index}].IsChecked`,
            item.response ? "true" : "false"
          );
        } else {
          formData.append(
            `SOPAssignmentResponses[${index}].Response`,
            item.response ?? ""
          );
          formData.append(
            `SOPAssignmentResponses[${index}].IsChecked`,
            "false"
          );
        }

        // file handling
        const newFiles = item.files?.filter(f => f.file instanceof File);
        const existingFiles = item.files?.filter(f => f.id && !f.file);
        const deletedFiles = item.deletedFiles || [];
        const originalFiles = item.originalFiles || [];

        // existing files
        existingFiles.forEach(f => {
          formData.append(
            `SOPAssignmentResponses[${index}].ExistingFiles`,
            JSON.stringify({
              id: f.id,
              fileId: f.fileId,
              url: f.url
            })
          );
        });

        // new files
        newFiles.forEach(f => {
          formData.append(
            `SOPAssignmentResponses[${index}].EvidenceFiles`,
            f.file
          );
        });

        // delete files
        deletedFiles.forEach(f => {
          formData.append(
            `SOPAssignmentResponses[${index}].DeletedFiles`,
            JSON.stringify({
              id: f.id,
              fileId: f.fileId
            })
          );
        });

        // flag
        let flag = "no_change";

        if (newFiles.length > 0 || deletedFiles.length > 0) {
          flag = "edit";
        }

        // all removed
        if (originalFiles.length > 0 && item.files.length === 0) {
          flag = "remove";
        }

        formData.append(
          `SOPAssignmentResponses[${index}].FileChangeFlag`,
          flag
        );

        index++;
      });
    });

    await sopAssignmentService.saveSOPAssignmentResponses(formData);

    notifySuccess({
      message:
        type === "save"
          ? "Checklist Saved successfully."
          : "Checklist Submitted successfully."
    });

    router.push("/sop-assignment");
  } catch (err) {
    console.error(err);
    notifyError({ message: "Failed to save checklist." });
  } finally {
    if (type === "save") processingSave.value = false;
    else processingSubmit.value = false;
  }
};

onMounted(() => {
  getSOPTemplateSectionItemsByAssignmentId();
});
</script>

<style scoped>
.border-bottom {
  border-bottom: 1px solid #eee;
}
.floating-actions {
  position: fixed;
  bottom: 20px;
  right: 20px;
  width: 220px;
}
.section-header {
  padding: 10px 12px;
}
.section-title {
  font-weight: 600;
  font-size: 14px;
}
.checklist-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 10px;
  border-bottom: 1px solid #e0e0e0;
  background: #fafafa;
}
.checklist-text {
  flex: 1;
  font-size: 13px;
  margin-left: 10px;
}
.checkbox-slot {
  width: 26px;
  min-width: 26px;
  height: 24px;
  display: flex;
  align-items: flex-start;
  justify-content: center;
}
</style>
