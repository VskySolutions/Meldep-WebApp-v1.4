import { useQuasar } from "quasar";
import addEditHelpDesk from "modules/helpdesk/components/addEdit.vue";
import viewHelpDesk from "modules/helpdesk/components/view.vue";
import addHelpDeskFiles from "modules/helpdesk/components/_files.vue";

let $q;
let activeRowId;

export function initHelpDeskDialogs (rowRef) {
  $q = useQuasar();
  activeRowId = rowRef;
}

export function onHelpDeskView (id, title, employeeId, primaryEmailAddress, twilioEmailId, notesType, refreshHelpDeskList, refreshAllUserListByRoleForDropdown) {
  activeRowId.value = id;
  $q.dialog({
    component: viewHelpDesk,
    componentProps: { id, title, employeeId, primaryEmailAddress, twilioEmailId, defaultTab: "1_tab", notesType: notesType }
  }).onOk(() => { 
    activeRowId.value = id;
    refreshHelpDeskList();
    refreshAllUserListByRoleForDropdown();
  })
  .onCancel(() => { 
    activeRowId.value = id;      
    refreshHelpDeskList();
    refreshAllUserListByRoleForDropdown();
  })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onHelpDeskViewEmailReplies (id, title, employeeId, primaryEmailAddress, twilioEmailId, notesType, refreshHelpDeskList, refreshAllUserListByRoleForDropdown) {
  activeRowId.value = id;
  $q.dialog({
    component: viewHelpDesk,
    componentProps: { id, title, employeeId, primaryEmailAddress, twilioEmailId, defaultTab: "2_tab", notesType: notesType }
  }).onOk(() => { 
    activeRowId.value = id;
    refreshHelpDeskList();
    refreshAllUserListByRoleForDropdown();
  })
  .onCancel(() => { 
    activeRowId.value = id;      
    refreshHelpDeskList();
    refreshAllUserListByRoleForDropdown();
  })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onAddHelpDeskFiles (id, refreshHelpDeskList, refreshAllUserListByRoleForDropdown) {
  activeRowId.value = id;
  $q.dialog({
    component: addHelpDeskFiles,
    componentProps: { id }
  }).onOk(() => { 
    activeRowId.value = id;
    refreshHelpDeskList();
    refreshAllUserListByRoleForDropdown();
  })
    .onCancel(() => { activeRowId.value = id; })
    .onDismiss(() => { activeRowId.value = id; });
}

export function onHelpDeskAdd (refreshHelpDeskList) {
  $q.dialog({
    component: addEditHelpDesk,
    componentProps: {}
  }).onOk(() => {
    refreshHelpDeskList();
  })
    .onCancel(() => { })
    .onDismiss(() => { });
}
