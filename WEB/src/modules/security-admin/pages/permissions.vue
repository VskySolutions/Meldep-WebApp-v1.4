<template>
  <security-shell title="Permissions">
    <div class="row q-col-gutter-md q-pa-md items-center">
      <div class="col-12 col-sm-auto">
        <q-btn-toggle
          v-model="type"
          no-caps
          unelevated
          toggle-color="primary"
          aria-label="Permissions for"
          :options="[{ label: 'Role', value: 'role' }, { label: 'Group', value: 'group' }, { label: 'Individual user', value: 'user' }]"
          @update:model-value="onTypeChange"
        />
      </div>
      <div v-if="type === 'user'" class="col-12 col-sm-6 col-md-4">
        <user-select v-model="subjectId" label="User" @update:model-value="load" />
      </div>
      <div v-if="type === 'group'" class="col-12 col-sm-6 col-md-4">
        <q-select v-model="subjectId" :options="groups" outlined dense emit-value map-options use-input option-value="id" option-label="name" label="Group" aria-label="Group" @update:model-value="load" />
      </div>
      <div v-if="type === 'role'" class="col-12 col-sm-6 col-md-4">
        <q-select v-model="subjectId" :options="roles" outlined dense emit-value map-options option-value="siteRoleId" option-label="name" label="Role" aria-label="Role" />
      </div>
      <div v-if="type === 'role'" class="col-12 col-sm-auto">
        <q-btn unelevated no-caps color="primary" label="Open role permissions" :disable="!subjectId" @click="openRole" />
      </div>
    </div>
    <q-separator />
    <permission-matrix v-if="type !== 'role' && subjectId" :rows="rows" :loading="loading" :require-reason="type === 'user'" :save="save" />
    <div v-else-if="type !== 'role'" class="text-center text-grey-8 q-pa-xl">
      <q-icon name="o_tune" size="40px" /><div class="q-mt-sm">Choose a {{ type === "user" ? "user" : "group" }} to see and change their permissions.</div>
    </div>
  </security-shell>
</template>

<script setup>
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { notifySuccess } from "assets/utils";
import securityShell from "../components/securityShell.vue";
import userSelect from "../components/userSelect.vue";
import permissionMatrix from "../components/permissionMatrix.vue";
import securityAdminService from "../securityAdmin.service";

const route = useRoute();
const router = useRouter();

const type = ref(["user", "group", "role"].includes(route.query.type) ? route.query.type : "group");
const subjectId = ref(route.query.id || null);
const groups = ref([]);
const roles = ref([]);
const rows = ref([]);
const loading = ref(false);

function onTypeChange () {
  subjectId.value = null;
  rows.value = [];
}

async function load () {
  if (!subjectId.value || type.value === "role") return;

  loading.value = true;
  try {
    rows.value = type.value === "user"
      ? await securityAdminService.getUserPermissions(subjectId.value)
      : await securityAdminService.getGroupPermissions(subjectId.value);
  } finally {
    loading.value = false;
  }
}

async function save (model) {
  if (type.value === "user") await securityAdminService.setUserPermission(subjectId.value, model);
  else await securityAdminService.setGroupPermission(subjectId.value, model);
  notifySuccess({ message: "Permission saved." });
}

function openRole () {
  router.push({ name: "site-menu-role-permissions", state: { siteRoleId: subjectId.value } });
}

onMounted(async () => {
  [groups.value, roles.value] = await Promise.all([securityAdminService.getGroups(), securityAdminService.getRoles()]);
  await load();
});
</script>
