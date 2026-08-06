import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import { ref } from "vue";
import projectQuestionsAnswersService from "modules/project-questions-answers/projectQuestionsAnswers.service";

let activeRowId;

export function initQuestionsAnswersActions (rowRef) {
  activeRowId = rowRef;
}

export const updatingRow = ref({
  status: null
});

// Delete Questions Answers
export const onSubmitQuestionsAnswersDelete = async (
  id,
  QuestionsAnswersName,
  refreshQuestionsAnswersList
) => {
  activeRowId.value = id;

  zwConfirmDelete(
    { data: `${QuestionsAnswersName}` },
    async () => {
      try {
        await projectQuestionsAnswersService.deleteQuestionAnswers(id);
        notifySuccess({ message: "Question Answers is deleted successfully." });
        refreshQuestionsAnswersList();
      } catch (error) {
        sendError("Error deleting questions answers", error);
      } finally {
        activeRowId.value = null;
      }
    },
    () => {
      activeRowId.value = null;
    }
  );
};

function sendError (message, error) {
  notifyError({ message });
  console.error(message, error);
}
