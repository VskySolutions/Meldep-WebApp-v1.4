<template>
  <security-shell title="Site Access">
    <div class="row items-center q-pa-md q-gutter-md">
      <q-input v-model="search" outlined dense clearable debounce="300" label="Search users" aria-label="Search users" style="min-width: 280px;" @update:model-value="reload">
        <template #prepend><q-icon name="o_search" /></template>
      </q-input>
      <q-toggle v-model="blockedOnly" label="Only users who can't open this site" @update:model-value="reload" />
    </div>
    <q-table
      v-model:pagination="pagination"
      flat
      bordered
      row-key="userId"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :rows-per-page-options="[20, 50, 100]"
      no-data-label="No users match."
      @request="load"
    >
      <template #body-cell-access="props">
        <q-td :props="props">
          <q-badge :color="props.row.canAccessSite ? 'positive' : 'negative'" :label="props.row.canAccessSite ? 'Can open' : 'Blocked'" />
          <div v-if="props.row.accessIssue" class="text-caption text-grey-8">{{ props.row.accessIssue }}</div>
        </q-td>
      </template>
      <template #body-cell-roles="props">
        <q-td :props="props" style="white-space: normal;">{{ props.row.roles.map(role => role.name).join(", ") }}</q-td>
      </template>
      <template #body-cell-actions="props">
        <q-td :props="props" auto-width>
          <q-btn flat round dense icon="o_badge" aria-label="Assign roles" @click="rolesUser = props.row"><q-tooltip>Assign roles</q-tooltip></q-btn>
          <q-btn flat round dense icon="o_domain" aria-label="User's sites" @click="openSites(props.row)"><q-tooltip>User's sites</q-tooltip></q-btn>
        </q-td>
      </template>
    </q-table>

    <user-roles-dialog :user="rolesUser" @close="rolesUser = null" @saved="reload" />

    <q-dialog v-model="sites.open">
      <q-card style="min-width: 460px; max-width: 95vw;">
        <q-card-section class="text-subtitle1">Sites: {{ sites.user?.name || sites.user?.userName }}</q-card-section>
        <q-list bordered separator>
          <q-item v-for="site in sites.list" :key="site.siteId">
            <q-item-section>
              <q-item-label>{{ site.name }}</q-item-label>
              <q-item-label caption>{{ site.isSharedUser ? "Shared" : "Home" }}{{ site.lastUsed ? " · Last used" : "" }}</q-item-label>
            </q-item-section>
            <q-item-section side>
              <q-badge :color="site.canAccess ? 'positive' : 'negative'" :label="site.canAccess ? 'Can open' : 'Blocked'" />
            </q-item-section>
          </q-item>
        </q-list>
        <q-card-actions align="right"><q-btn flat no-caps label="Close" @click="sites.open = false" /></q-card-actions>
      </q-card>
    </q-dialog>
  </security-shell>
</template>

<script setup>
import { onMounted, reactive, ref } from "vue";
import securityShell from "../components/securityShell.vue";
import userRolesDialog from "../components/userRolesDialog.vue";
import securityAdminService from "../securityAdmin.service";

const search = ref("");
const blockedOnly = ref(false);
const rows = ref([]);
const loading = ref(false);
const rolesUser = ref(null);
const sites = reactive({ open: false, user: null, list: [] });
const pagination = ref({ page: 1, rowsPerPage: 20, rowsNumber: 0 });

const columns = [
  { name: "name", label: "Name", field: "name", align: "left" },
  { name: "userName", label: "User name", field: "userName", align: "left" },
  { name: "access", label: "Access", field: "canAccessSite", align: "left" },
  { name: "shared", label: "Membership", field: row => (row.isSharedUser ? "Shared" : "Home"), align: "left" },
  { name: "roles", label: "Roles here", field: "roles", align: "left" },
  { name: "actions", label: "Actions", field: "actions", align: "right" }
];

async function load ({ pagination: page }) {
  loading.value = true;
  try {
    const result = await securityAdminService.getUsers({
      search: search.value,
      blockedOnly: blockedOnly.value,
      page: page.page,
      pageSize: page.rowsPerPage
    });
    rows.value = result.items;
    pagination.value = { ...page, rowsNumber: result.total };
  } finally {
    loading.value = false;
  }
}

function reload () {
  load({ pagination: { ...pagination.value, page: 1 } });
}

async function openSites (user) {
  Object.assign(sites, { open: true, user, list: [] });
  sites.list = await securityAdminService.getUserSites(user.userId);
}

onMounted(reload);
</script>
