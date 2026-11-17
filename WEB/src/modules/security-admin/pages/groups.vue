<template>
  <security-shell title="Groups">
    <div class="row items-center justify-end q-pa-md">
      <q-btn unelevated no-caps color="primary" icon="o_add" label="Add group" @click="openEdit()" />
    </div>
    <q-table
      flat
      bordered
      row-key="id"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :pagination="{ rowsPerPage: 20 }"
      :rows-per-page-options="[20, 50, 100]"
    >
      <template #no-data>
        <div class="full-width text-center q-pa-lg text-grey-8">
          <q-icon name="o_groups" size="40px" /><div class="q-mt-sm">No security groups in this site yet.</div>
        </div>
      </template>
      <template #body-cell-active="props">
        <q-td :props="props">
          <q-badge :color="props.value ? 'positive' : 'grey-7'" :label="props.value ? 'Active' : 'Inactive'" />
        </q-td>
      </template>
      <template #body-cell-actions="props">
        <q-td :props="props" auto-width>
          <q-btn flat round dense icon="o_group_add" aria-label="Members" @click="openMembers(props.row)"><q-tooltip>Members</q-tooltip></q-btn>
          <q-btn flat round dense icon="o_tune" aria-label="Group permissions" :to="{ path: '/security-admin/permissions', query: { type: 'group', id: props.row.id } }"><q-tooltip>Group permissions</q-tooltip></q-btn>
          <q-btn flat round dense icon="o_edit" aria-label="Edit group" @click="openEdit(props.row)"><q-tooltip>Edit</q-tooltip></q-btn>
          <q-btn flat round dense icon="o_delete" color="negative" aria-label="Delete group" @click="confirmDelete(props.row)"><q-tooltip>Delete</q-tooltip></q-btn>
        </q-td>
      </template>
    </q-table>

    <q-dialog v-model="edit.open">
      <q-card style="min-width: 420px; max-width: 95vw;">
        <q-card-section class="text-subtitle1">{{ edit.id ? "Edit group" : "Add group" }}</q-card-section>
        <q-card-section class="q-pt-none q-gutter-md">
          <q-input v-model="edit.name" outlined dense autofocus label="Name *" aria-label="Group name" maxlength="200" />
          <q-input v-model="edit.description" outlined dense type="textarea" rows="2" label="Description" aria-label="Description" maxlength="1000" />
          <q-toggle v-if="edit.id" v-model="edit.active" label="Active" />
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat no-caps label="Cancel" @click="edit.open = false" />
          <q-btn unelevated no-caps color="primary" label="Save" :disable="!edit.name?.trim()" @click="saveEdit" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <q-dialog v-model="members.open">
      <q-card style="min-width: 560px; max-width: 95vw;">
        <q-card-section class="text-subtitle1">Members: {{ members.group?.name }}</q-card-section>
        <q-card-section class="q-pt-none">
          <div class="row q-col-gutter-sm items-center">
            <div class="col"><user-select v-model="members.toAdd" multiple label="Add users" /></div>
            <div class="col-auto"><q-btn unelevated no-caps color="primary" label="Add" :disable="!members.toAdd?.length" @click="addMembers" /></div>
          </div>
          <q-list bordered separator class="q-mt-md" style="max-height: 320px; overflow: auto;">
            <q-item v-for="member in members.list" :key="member.userId">
              <q-item-section>
                <q-item-label>{{ member.name?.trim() || member.userName }}</q-item-label>
                <q-item-label caption>{{ member.userName }}</q-item-label>
              </q-item-section>
              <q-item-section side>
                <q-badge v-if="!member.canAccessSite" outline color="orange-9" label="No site access">
                  <q-tooltip>This user has no valid role here, so the group gives them nothing yet.</q-tooltip>
                </q-badge>
              </q-item-section>
              <q-item-section side>
                <q-btn flat round dense icon="o_close" :aria-label="`Remove ${member.userName}`" @click="removeMember(member)"><q-tooltip>Remove from group</q-tooltip></q-btn>
              </q-item-section>
            </q-item>
            <q-item v-if="members.list.length === 0"><q-item-section class="text-grey-8">No members yet.</q-item-section></q-item>
          </q-list>
        </q-card-section>
        <q-card-actions align="right"><q-btn flat no-caps label="Close" @click="members.open = false" /></q-card-actions>
      </q-card>
    </q-dialog>
  </security-shell>
</template>

<script setup>
import { onMounted, reactive, ref } from "vue";
import { useQuasar } from "quasar";
import { notifySuccess } from "assets/utils";
import securityShell from "../components/securityShell.vue";
import userSelect from "../components/userSelect.vue";
import securityAdminService from "../securityAdmin.service";

const $q = useQuasar();
const rows = ref([]);
const loading = ref(false);

const columns = [
  { name: "name", label: "Group", field: "name", align: "left", sortable: true },
  { name: "description", label: "Description", field: "description", align: "left" },
  { name: "memberCount", label: "Members", field: "memberCount", align: "center", sortable: true },
  { name: "permissionCount", label: "Menu rules", field: "permissionCount", align: "center", sortable: true },
  { name: "active", label: "Status", field: "active", align: "left" },
  { name: "actions", label: "Actions", field: "actions", align: "right" }
];

const edit = reactive({ open: false, id: null, name: "", description: "", active: true });
const members = reactive({ open: false, group: null, list: [], toAdd: [] });

async function load () {
  loading.value = true;
  try {
    rows.value = await securityAdminService.getGroups();
  } finally {
    loading.value = false;
  }
}

function openEdit (group) {
  Object.assign(edit, {
    open: true,
    id: group?.id || null,
    name: group?.name || "",
    description: group?.description || "",
    active: group?.active ?? true
  });
}

async function saveEdit () {
  const model = { name: edit.name, description: edit.description, active: edit.active };
  try {
    if (edit.id) await securityAdminService.updateGroup(edit.id, model);
    else await securityAdminService.createGroup(model);
    notifySuccess({ message: edit.id ? "Group updated." : "Group created." });
    edit.open = false;
    load();
  } catch {
    // The request's error is already shown, and the dialog stays open to correct it.
  }
}

function confirmDelete (group) {
  $q.dialog({
    title: "Delete group",
    message: `Delete "${group.name}"? Its ${group.memberCount} members lose everything this group grants, and any deny it applies ends.`,
    cancel: { flat: true, noCaps: true },
    ok: { label: "Delete group", color: "negative", unelevated: true, noCaps: true }
  }).onOk(async () => {
    await securityAdminService.deleteGroup(group.id);
    notifySuccess({ message: "Group deleted." });
    load();
  });
}

async function loadMembers () {
  members.list = await securityAdminService.getGroupMembers(members.group.id);
}

async function openMembers (group) {
  Object.assign(members, { open: true, group, list: [], toAdd: [] });
  await loadMembers();
}

async function addMembers () {
  await securityAdminService.addGroupMembers(members.group.id, members.toAdd);
  members.toAdd = [];
  notifySuccess({ message: "Members added." });
  await loadMembers();
  load();
}

async function removeMember (member) {
  await securityAdminService.removeGroupMember(members.group.id, member.userId);
  notifySuccess({ message: "Member removed." });
  await loadMembers();
  load();
}

onMounted(load);
</script>
