import { useQuasar } from "quasar";
import viewJobPost from "modules/job-post/components/view.vue";
import editJobPost from "modules/job-post/components/addEdit.vue";

let $q;
let activeRowId;

export function initJobPostDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onJobPostView (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: viewJobPost,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  }).onCancel(() => {
    activeRowId.value = id;
  }).onDismiss(() => {
    activeRowId.value = id;
  });
}

export function onJobPostAdd (refresh) {
  $q.dialog({
    component: editJobPost,
    componentProps: {}
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}

export function onJobPostEdit (id, refresh) {
  activeRowId.value = id;
  $q.dialog({
    component: editJobPost,
    componentProps: { id }
  }).onOk(() => {
    refresh();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
