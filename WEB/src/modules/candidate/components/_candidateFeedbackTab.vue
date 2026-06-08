<template>
  <fieldset>
    <legend>Feedback Details</legend>
    <div class="q-mb-sm q-gutter-sm flex justify-end">
      <q-input
        v-model="filterFeedback"
        outlined
        class="bg-white q-mr-sm search-box"
        debounce="300"
        placeholder="Search"
        dense
        clearable
      >
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </div>
    <q-table
      ref="tableRef"
      v-model:pagination="pagination"
      virtual-scroll
      class="border Custom-DataTable"
      :loading="loading"
      :rows="filteredFeedback"
      :columns="columns"
      row-key="id"
      separator="cell"
      binary-state-sort
      :filter="filterFeedback"
      no-data-label="No data available"
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      @request="getCandidateFeedbackLog"
    >
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
        </q-tr>
      </template>
      <template #body="props">
        <q-tr :props="props">
          <q-td style="width: 5%;">
            {{ props.row.dueDate }}
          </q-td>
          <q-td style="width: 10%;">
            {{ props.row.employee.person.fullName }}
          </q-td>
          <q-td style="width: 20%; white-space: break-spaces;">
            {{ props.row.candidateQuestions.dropDownValue }}
          </q-td>
          <q-td style="width: 30%; white-space: break-spaces;" @click="showComment(props.row.answer, true)">
            <div class="clamped-text" v-html="removeLeadingSpaces(props.row.answer)" />
          </q-td>
        </q-tr>
      </template>
    </q-table>
  </fieldset>
  <q-dialog v-model="isDialogOpen">
    <q-card style="width: 700px; max-width: 80vw;">
      <q-card-section style="background-color: #1b75ab">
        <div class="text-h2 text-weight-medium text-white">
          {{ currentCommentFlag ? "Answer" : "Description" }}
        </div>
      </q-card-section>
      <q-card-section class="q-pt-sm RichTextEditor">
        <div v-html="currentComment" />
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
import { ref, watch, computed } from "vue";
import candidateService from "modules/candidate/candidate.service";

// define props
const props = defineProps({ candidateId: { type: String, default: "" } });
const candidateId = props.candidateId;

// common variables
const loading = ref(true);
const currentComment = ref("");
const currentCommentFlag = ref(false);
const isDialogOpen = ref(false);
const filterFeedback = ref("");

// define table variables
const rows = ref([]);
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "dueDate", label: "Due date", field: "dueDate", align: "left", sortable: true },
  { name: "employee.person.fullName", label: "Owner", field: row => row.employee?.person?.fullName, align: "left", sortable: true },
  { name: "candidateQuestions.dropDownValue", label: "Question", field: row => row.candidateQuestions?.dropDownValue, align: "left", sortable: true },
  { name: "answer", label: "Answer", field: "answer", align: "left", sortable: true }
]);

// getCandidateFeedbackLog
const getCandidateFeedbackLog = () => {
  loading.value = true;
  candidateService.getCandidateFeedbackDetailsById(candidateId).then((resp) => {
    rows.value = resp;
  }).finally(() => {
    loading.value = false;
  });
};

const showComment = (comment, isAnswer) => {
  currentComment.value = comment;
  isDialogOpen.value = true;
  currentCommentFlag.value = isAnswer;
};

function removeLeadingSpaces (html) {
  if (!html) return "";
  // Remove leading spaces, non-breaking spaces, and empty tags
  return html.replace(/^(?:\s|&nbsp;|<[^>]+>)*\s*/, "");
}

// for static filter
const filterRows = (data, searchTerm, columns) => {
  if (!searchTerm) return data;
  const lowerCaseTerm = searchTerm.toLowerCase();

  return data.filter(row =>
    columns.some(column => {
      const value = typeof column.field === "function" // This checks whether the field is a function or not. -field: row => row.candidateQuestions?.dropDownValue
        ? column.field(row)
        : column.field.split(".").reduce((obj, key) => obj?.[key], row);
      return String(value || "").toLowerCase().includes(lowerCaseTerm);
    })
  );
};

const feedbackColumns = columns.value;
const filteredFeedback = computed(() => filterRows(rows.value, filterFeedback.value, feedbackColumns));

watch(() => candidateId, (newValue, oldValue) => {
  if (newValue) {
    getCandidateFeedbackLog();
  }
}, { immediate: true });

</script>
