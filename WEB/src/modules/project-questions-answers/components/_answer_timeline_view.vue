<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent position="right"  @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic column no-wrap" style="width: 50vw; max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ label }}</div>
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
      <div v-if="allAnswers.length === 0">
          <h5 class="text-center text-grey">No Answers Available</h5>
      </div>
       <!-- Timeline Section (scrollable) -->
      <div class="col scroll q-px-xl" style="overflow-y:auto; flex-grow:1; display:flex; flex-direction:column">
        <q-timeline color="secondary">
          <template v-for="(group, date) in groupedAnswers" :key="date">
            <q-timeline-entry
              v-for="answer in group"
              :key="answer.id"
              :side="user.userId === answer.createdById ? 'right' : 'left'"
              color="primary"
              :icon="done_all"
            >
              <template v-slot:subtitle>
                <div class="text-weight-bolder text-primary">
                  {{ answer.createdOnUtc }} • {{ answer.createdBy?.person?.fullName || '' }}
                </div>
              </template>
              <div class="fs-14 note-row">
                  <div
                    class="note-wrapper RichTextEditor"
                  >
                    <span class="text-black note-text" v-html="answer.description" />
                  </div>
              </div>
            </q-timeline-entry>
          </template>
        </q-timeline>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useAuthStore } from "stores/auth";
import { useDialogPluginComponent } from "quasar";
import projectQuestionsAnswersService from "modules/project-questions-answers/projectQuestionsAnswers.service";
import _ from "lodash";

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" },
  label: { type: String, default: "" }
});

// common variables
const loading = ref(true);
const authStore = useAuthStore();
const user = authStore.user;

// answers
const allAnswers = ref([]);

// get all answers and map list
const getAllQuestionAnswersByQuestionId = () => {
  loading.value = true;
  projectQuestionsAnswersService.getAllQuestionAnswersByQuestionId(props.id, true).then((resp) => {
    const questionList = resp.projectQuestionsAnswerList || [];
    const answers = [];

    questionList.forEach((question) => {
      const description = question.description
            ?.replace(/<[^>]*>/g, '')
            .trim();
      // Add original Question
     if (description) {
      answers.push({
        id: question.id,
        description: question.description,
        createdOnUtc: question.createdOnUtc,
        createdBy: question.createdBy,
      });
    }

      // Add Response Logs
      (question.projectQuestionsAnswersResponseLog || []).forEach(
        (response) => {
          answers.push({
            id: response.id,
            description: response.description,
            createdOnUtc: response.createdOnUtc,
            createdBy: response.createdBy,
          });
        }
      );
      answers.sort(
        (a, b) =>
          new Date(b.createdOnUtc).getTime() -
          new Date(a.createdOnUtc).getTime()
      );
      allAnswers.value = answers;
    });
  }).finally(() => {
    loading.value = false;
  });
};

// group the answers
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
.note-input {
  flex: 1;
  min-width: 100px;
}
.note-text {
  display: inline-block; /* shrink-wraps to text width */
}
.note-row .q-btn {
  visibility: hidden;
}
.note-row:hover .q-btn, .note-row.editing .q-btn {
  visibility: visible; /* show when row hovered */
}
.editor-locked .q-editor__toolbar {
  pointer-events: none;
  opacity: 0.6; /* optional - gives disabled look */
}

</style>

