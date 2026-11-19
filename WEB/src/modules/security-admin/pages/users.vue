<template>
  <security-shell title="Users">
    <div class="row items-center justify-between q-pa-md q-gutter-sm">
      <q-input v-model="search" outlined dense clearable debounce="300" label="Search users" aria-label="Search users" style="min-width: 280px;" @update:model-value="reload">
        <template #prepend><q-icon name="o_search" /></template>
      </q-input>
      <q-btn outline no-caps color="primary" icon="o_manage_accounts" label="Create or edit users" to="/user-management" />
    </div>
    <q-table
      v-model:pagination="pagination"
      flat
      bordered
      row-key="userId"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      no-data-label="No users found in this site."
      @request="load"
    >
      <template #body-cell-roles="props">
        <q-td :props="props" style="white-space: normal;">
          <q-badge v-for="role in props.row.roles" :key="role.siteRoleId" outline color="primary" class="q-mr-xs" :label="role.name" />
          <q-badge v-if="props.row.roles.length === 0" color="negative" label="No role" />
        </q-td>
      </template>
      <!-- Same Active/Inactive display as User Management > User List -->
      <template #body-cell-active="props">
        <q-td :props="props">
          <q-icon :name="props.row.active ? 'o_check_circle' : 'o_cancel'" :color="props.row.active ? 'positive' : 'negative'" class="cursor-pointer" size="sm">
            <q-tooltip>{{ props.value }}</q-tooltip>
          </q-icon>
        </q-td>
      </template>
      <template #body-cell-actions="props">
        <q-td :props="props" auto-width>
          <q-btn flat round dense icon="o_badge" aria-label="Assign Roles" @click="rolesUser = props.row"><q-tooltip>Assign Roles</q-tooltip></q-btn>
          <q-btn flat round dense icon="o_tune" aria-label="Individual permissions" :to="{ path: '/security-admin/permissions', query: { type: 'user', id: props.row.userId } }"><q-tooltip>Individual permissions</q-tooltip></q-btn>
          <q-btn flat round dense icon="o_verified_user" aria-label="Effective permissions" :to="{ path: '/security-admin/effective', query: { userId: props.row.userId } }"><q-tooltip>Effective permissions</q-tooltip></q-btn>
        </q-td>
      </template>
    </q-table>
    <user-roles-dialog :user="rolesUser" @close="rolesUser = null" @saved="reload" />
  </security-shell>
</template>

<script setup>
import { onMounted, ref } from "vue";
import securityShell from "../components/securityShell.vue";
import userRolesDialog from "../components/userRolesDialog.vue";
import securityAdminService from "../securityAdmin.service";

const search = ref("");
const rows = ref([]);
const loading = ref(false);
const rolesUser = ref(null);
const pagination = ref({ page: 1, rowsPerPage: 20, rowsNumber: 0 });

const columns = [
  { name: "name", label: "Name", field: "name", align: "left" },
  { name: "userName", label: "User name", field: "userName", align: "left" },
  { name: "active", label: "Active/Inactive", field: row => (row.active ? "Active" : "Inactive"), align: "center" },
  { name: "roles", label: "Roles", field: "roles", align: "left" },
  { name: "groupCount", label: "Groups", field: "groupCount", align: "center" },
  { name: "directGrantCount", label: "Individual permissions", field: "directGrantCount", align: "center" },
  { name: "actions", label: "Actions", field: "actions", align: "right" }
];

async function load ({ pagination: page }) {
  loading.value = true;
  try {
    const result = await securityAdminService.getUsers({ search: search.value, page: page.page, pageSize: page.rowsPerPage });
    rows.value = result.items;
    pagination.value = { ...page, rowsNumber: result.total };
  } finally {
    loading.value = false;
  }
}

function reload () {
  load({ pagination: { ...pagination.value, page: 1 } });
}

onMounted(reload);
</script>
