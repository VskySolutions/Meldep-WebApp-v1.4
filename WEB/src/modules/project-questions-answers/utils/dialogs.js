import { useQuasar } from "quasar";
import addEditQuestionAnswers from "modules/project-questions-answers/components/addEdit.vue";
import viewQuestionAnswers from "modules/project-questions-answers/components/view.vue";
import viewAnswerTimeLineView from "modules/project-questions-answers/components/_answer_timeline_view.vue";


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

export function onQuestionAnswersAdd (
  projectId,
  requirementId,
  refresh
) {
  const componentProps = {};
  if (projectId) componentProps.projectIdAttr = projectId;
  if (requirementId) componentProps.requirementIdAttr = requirementId;
  $q.dialog({
    component: addEditQuestionAnswers,
    componentProps
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

export function onAnswerTimelineView(id, label) {
  activeRowId.value = id;
  $q.dialog({
    component: viewAnswerTimeLineView,
    componentProps: { id, label}
  })
    .onOk(() => {
    })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}
