<template>
  <security-shell title="Roles">
    <div class="row items-center justify-end q-pa-md">
      <q-btn outline no-caps color="primary" icon="o_badge" label="Edit Roles" to="/site-roles" />
    </div>
    <q-table
      flat
      bordered
      row-key="siteRoleId"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :pagination="{ rowsPerPage: 50 }"
      :rows-per-page-options="[20, 50, 100]"
      no-data-label="No roles are set up in this site."
    >
      <template #body-cell-notConfiguredCount="props">
        <q-td :props="props">
          <q-badge v-if="props.value > 0" outline color="orange-9" :label="props.value">
            <q-tooltip>These menus have only Show set, which means full access until View or Manage is set.</q-tooltip>
          </q-badge>
          <span v-else>0</span>
        </q-td>
      </template>
      <template #body-cell-actions="props">
        <q-td :props="props" auto-width>
          <q-btn flat round dense icon="o_tune" aria-label="Role permissions" @click="openPermissions(props.row)"><q-tooltip>Role permissions</q-tooltip></q-btn>
        </q-td>
      </template>
    </q-table>
  </security-shell>
</template>

<script setup>
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import securityShell from "../components/securityShell.vue";
import securityAdminService from "../securityAdmin.service";

const router = useRouter();
const rows = ref([]);
const loading = ref(false);

const columns = [
  { name: "name", label: "Role", field: "name", align: "left", sortable: true },
  { name: "userCount", label: "Users", field: "userCount", align: "center", sortable: true },
  { name: "menuCount", label: "Menus granted", field: "menuCount", align: "center", sortable: true },
  { name: "notConfiguredCount", label: "Not configured", field: "notConfiguredCount", align: "center", sortable: true },
  { name: "actions", label: "Actions", field: "actions", align: "right" }
];

// The existing role matrix already works, so this opens it filtered to the role.
function openPermissions (role) {
  router.push({ name: "site-menu-role-permissions", state: { siteRoleId: role.siteRoleId } });
}

onMounted(async () => {
  loading.value = true;
  try {
    rows.value = await securityAdminService.getRoles();
  } finally {
    loading.value = false;
  }
});
</script>
