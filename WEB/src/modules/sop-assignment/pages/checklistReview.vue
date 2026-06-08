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
      <div class="row text-bold text-grey-7 q-pb-sm">
        <div class="col-3">Checklist</div>
        <div class="col-2">Response</div>
        <div class="col-2">Evidence</div>
        <div class="col-2">Is Approved</div>
        <div class="col-3">Approver Comment</div>
      </div>
      <q-separator class="q-mb-sm" />
      <div v-for="section in model.sopTemplateSections" :key="section.id">
        <div class="section-header bg-primary text-white">
          <div class="fs-12 section-title">{{ section.name }}</div>
          <div
            class="fs-10"
            v-html="truncateText(section.description, 500)"
          />
        </div>
        <div
          v-for="item in section.sopTemplateSectionItems"
          :key="item.id"
          :class="['row items-center q-py-xs checklist-row', item.isApproved ? 'bg-green-1' : '']"
        >
          <div class="col-3">
            <div class="fs-13 text-weight-bold">
              {{ item.name }}
            </div>
            <div
              class="fs-10 text-grey-7"
              v-html="truncateText(item.description, 500)"
            />
          </div>
          <div class="col-2">
            <span v-if="item.type === 'checkbox'">
              {{ item.response ? "Yes" : "No" }}
            </span>
            <span v-else>
              {{ item.response || '-' }}
            </span>
          </div>
          <!-- Evidence -->
          <div class="col-2">
            <div v-if="item.files?.length" class="row q-gutter-xs">
              <div v-for="f in item.files" :key="f.id" class="text-center">
                <a :href="f.url" target="_blank">
                  <q-icon
                    :name="getFileIcon(f.fileName)"
                    size="30px"
                    :class="f.isNew ? 'text-primary' : 'text-grey-7'"
                    class="hover-icon"
                  >
                    <q-tooltip>{{ f.fileName }}</q-tooltip>
                  </q-icon>
                </a>
              </div>
            </div>
            <div v-else>-</div>
          </div>
          <div class="col-2">
            <template v-if="modeOfPage === 'edit'">
              <q-checkbox v-model="item.isApproved" />
            </template>
            <template v-else>
              <q-icon
                v-if="
                  (model.assignmentStatus === 'Approved' || model.assignmentStatus === 'Rejected' || model.assignmentStatus === 'Submitted') &&
                    item.isApproved === true
                "
                name="o_done"
                color="green"
                size="20px"
              />
              <q-icon
                v-else-if="
                  (model.assignmentStatus === 'Approved' || model.assignmentStatus === 'Rejected' || model.assignmentStatus === 'Submitted') &&
                    item.isApproved === false
                "
                name="o_restart_alt"
                color="red"
                size="20px"
              />
              <span v-else />
            </template>
          </div>
          <!-- Comment -->
          <div class="col-3">
            <template v-if="modeOfPage === 'edit'">
              <q-input
                v-model="item.comment"
                dense
                outlined
                placeholder="Enter comment"
              />
            </template>
            <template v-else>
              {{ item.comment || '-' }}
            </template>
          </div>
        </div>
      </div>
    </q-card>
    <q-card-actions v-if="modeOfPage === 'edit'" align="center" class="stickyFooter q-gutter-sm justify-center">
      <q-btn color="grey-4" label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="$router.back();" />
      <q-btn
        color="negative"
        label="Rework"
        class="actionBtn"
        :loading="processingRework"
        no-caps
        @click="submitReview('rework')"
      />
      <q-btn
        color="positive"
        label="Approve"
        class="actionBtn"
        :loading="processingApprove"
        no-caps
        @click="submitReview('approve')"
      />
    </q-card-actions>
  </q-page>
</template>
<script setup>
import { ref, onMounted, computed } from "vue";
import { useRouter } from "vue-router";
import sopAssignmentService from "modules/sop-assignment/sopAssignment.service";
import { notifySuccess, notifyError } from "assets/utils";

const router = useRouter();
const processingRework = ref(false);
const processingApprove = ref(false);
const selectedAssignmentId = history.state?.id;
const modeOfPage = history.state?.mode;

const infoFields = computed(() => [
  { label: "Template Name", value: model.value.template?.name },
  { label: "Assigned To", value: model.value.assignedToEmployee?.person.fullName },
  { label: "Approver", value: model.value.approverEmployee?.person.fullName },
  { label: "Priority", value: model.value.priority?.dropDownValue },
  { label: "Status", value: model.value.status?.dropDownValue },
  { label: "Assigned Date", value: model.value.assignedDate },
  { label: "Due Date", value: model.value.dueDate }
]);

const model = ref({
  name: "",
  sopTemplateSections: []
});

// map type
const mapType = (item) => {
  switch (item.inputType.dropDownValue) {
  case "CheckBox": return "checkbox";
  case "Number Field": return "number";
  case "Dropdown": return "dropdown";
  default: return "text";
  }
};

// load data
const getSOPTemplateSectionItemsResponsesByAssignmentId = async () => {
  try {
    const resp = await sopAssignmentService.getSOPAssignmentByIdInDetail(selectedAssignmentId);

    const responses = resp?.sopAssignmentResponses || [];

    model.value = {
      ...(resp || {}),
      name: resp.name,
      sopTemplateSections: resp.template.sopTemplateSections.map(section => ({
        ...section,
        sopTemplateSectionItems: section.sopTemplateSectionItems.map(item => {
          const existing = responses.find(r => r.sectionItemId === item.id);
          return {
            ...item,
            type: mapType(item),

            response: existing
              ? (item.inputType.dropDownValue === "CheckBox"
                ? existing.isChecked
                : existing.response)
              : null,

            responseId: existing?.id || null,

            files: existing?.sopAssignmentResponseEvidences?.map(e => ({
              id: e.id,
              fileName: e.file?.seoFilename,
              url: e.file?.virtualPath
            })) || [],

            isApproved: existing
              ? (existing.isApproved ?? "-")
              : "-",
            comment: existing?.approvedComment || ""
          };
        })
      }))
    };
    model.value.assignmentStatus = resp.status.dropDownValue;
  } catch (err) {
    notifyError({ message: "Failed to load data" });
  }
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

const truncateText = (text, limit = 500) => {
  if (!text) return "";

  return text.length > limit
    ? text.substring(0, limit) + "..."
    : text;
};

// submit review
const submitReview = async (type) => {
  if (type === "rework") processingRework.value = true;
  else processingApprove.value = true;

  try {
    // Validation for approve
    if (type === "approve") {
      const allItems = model.value.sopTemplateSections.flatMap(section =>
        section.sopTemplateSectionItems
      );

      const notApprovedItems = allItems.filter(item => !item.isApproved);

      if (notApprovedItems.length > 0) {
        notifyError({
          message: "All checklist items must be approved before submitting."
        });

        return;
      }
    }

    const responses = model.value.sopTemplateSections.flatMap(section =>
      section.sopTemplateSectionItems
        .filter(item => item.responseId)
        .map(item => ({
          id: item.responseId,
          approvedComment: item.comment || "",
          isApproved: item.isApproved
        }))
    );

    const payload = {
      id: selectedAssignmentId,
      isApproved: type === "approve",
      sopAssignmentResponses: responses
    };

    await sopAssignmentService.submitReview(payload);

    notifySuccess({
      message: type === "approve"
        ? "Approved successfully"
        : "Sent back for correction"
    });

    router.push("/sop-assignment");
  } catch (err) {
    notifyError({ message: "Action failed" });
  } finally {
    if (type === "rework") processingRework.value = false;
    else processingApprove.value = false;
  }
};

onMounted(() => {
  getSOPTemplateSectionItemsResponsesByAssignmentId();
});
</script>
<style>

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
