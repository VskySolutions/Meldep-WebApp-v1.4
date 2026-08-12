import { useQuasar } from "quasar";
import addEditQuestionAnswers from "modules/project-questions-answers/components/addEdit.vue";
import viewQuestionAnswers from "modules/project-questions-answers/components/view.vue";

let $q;
let activeRowId;

export function initQuestionsAnswersDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onQuestionAnswersView (id) {
  activeRowId.value = id;
  $q.dialog({
    component: viewQuestionAnswers,
    componentProps: { id }
  }).onOk(() => { activeRowId.value = id; })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onQuestionAnswersAdd (refresh) {
  $q.dialog({
    component: addEditQuestionAnswers,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onQuestionAnswersEdit(id, refresh, showResponseLog = false) {
  activeRowId.value = id;

  $q.dialog({
    component: addEditQuestionAnswers,
    componentProps: {
      id,
      showResponseLog
    }
  })
    .onOk(() => {
      refresh();
    })
    .onCancel(() => {})
    .onDismiss(() => {});
}
