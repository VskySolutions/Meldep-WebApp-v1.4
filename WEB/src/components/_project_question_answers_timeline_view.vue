<template>
  <!-- Timeline Section (scrollable) -->
  <div class="col scroll q-px-sm" style="overflow-y:auto; flex-grow:1; height:64vh; display:flex; flex-direction:column-reverse">
    <q-timeline color="secondary">
      <template v-for="(group, date) in groupedAnswers" :key="date">
        <!-- :side="user.userId === note.createdById ? 'right' : 'left'" -->
        <q-timeline-entry
          v-for="note in group"
          :key="note.id"
          color="primary"
          :icon="done_all"
        >
          <template v-slot:subtitle>
            <div class="text-weight-bolder text-primary">
              {{ note.createdOnUtc }} • {{ note.createdBy?.person?.fullName || '' }}
            </div>
          </template>
          <!-- NOTE BODY -->
          <div class="fs-14 note-row">
            <div
              class="note-wrapper RichTextEditor"
            >
              <span class="text-black note-text" v-html="note.description" />
            </div>
          </div>
        </q-timeline-entry>
      </template>
    </q-timeline>
    <div v-if="allAnswers.length === 0">
      <h5 class="text-center text-grey">No Notes Available</h5>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from "vue";
import _ from "lodash";

import projectQuestionsAnswersService from "modules/project-questions-answers/projectQuestionsAnswers.service";

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" }
});

// common variables
const loading = ref(true);

// notes
const allAnswers = ref([]);

// get all Answers and map list
const getAllQuestionAnswersByQuestionId = () => {
  loading.value = true;
  projectQuestionsAnswersService.getAllQuestionAnswersByQuestionId(props.id, false).then((resp) => {
    // allAnswers.value = resp.projectQuestionsAnswerList || [];
    const questionList = resp.projectQuestionsAnswerList || [];
    const answers = [];

    questionList.forEach((question) => {

      // Add original Question
      answers.push({
        id: question.id,
        description: question.description,
        createdOnUtc: question.createdOnUtc,
        createdById: question.createdById,
        createdBy: question.createdBy,
        type: "Question"
      });

      // Add Response Logs
      (question.projectQuestionsAnswersResponseLog || []).forEach(
        (response) => {
          answers.push({
            id: response.id,
            description: response.description,
            createdOnUtc: response.createdOnUtc,
            createdById: response.createdById,
            createdBy: response.createdBy,
            type: "Response"
          });
        }
      );

      // Sort oldest → latest
      answers.sort(
        (a, b) =>
          new Date(a.createdOnUtc) -
          new Date(b.createdOnUtc)
      );
      allAnswers.value = answers;
      console.log("allAnswers.value", allAnswers.value);
    });

  }).finally(() => {
    loading.value = false;
  });
};

// group the Answers
const groupedAnswers = computed(() => {
  return allAnswers.value.reduce((groups, note) => {
    const date = new Date(note.createdOnUtc).toDateString();
    if (!groups[date]) {
      groups[date] = [];
    }
    groups[date].push(note);
    return groups;
  }, {});
});

watch(
  () => props.id,
  getAllQuestionAnswersByQuestionId,
  { immediate: true }
)
// ======================================================================
// On page rendering
onMounted(() => {
  getAllQuestionAnswersByQuestionId();
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
/* .note-input {
  flex: 1;
  min-width: 100px;
} */
.note-text {
  display: inline-block; /* shrink-wraps to text width */
}

</style>
