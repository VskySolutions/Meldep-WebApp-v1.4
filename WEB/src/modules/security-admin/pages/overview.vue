<template>
  <security-shell>
    <q-card-section>
      <q-banner v-if="overview && overview.mode !== 'Enforce'" dense rounded class="bg-orange-1 text-orange-10 q-mb-md">
        <template #avatar><q-icon name="o_info" /></template>
        Authorization is in {{ overview.mode }} mode. Missing menu permissions are logged but not blocked.
      </q-banner>
      <q-banner v-if="overview?.legacyShowOnlyAccess && overview.notConfiguredRowCount > 0" dense rounded class="bg-blue-1 text-blue-10 q-mb-md">
        <template #avatar><q-icon name="o_info" /></template>
        {{ overview.notConfiguredRowCount }} role permission rows have only Show set, which still means full access until View or Manage is configured.
      </q-banner>
      <q-inner-loading :showing="!overview && !failed" />
      <div v-if="overview" class="row q-col-gutter-md">
        <div v-for="tile in tiles" :key="tile.label" class="col-6 col-sm-4 col-md-2">
          <q-card flat bordered class="cursor-pointer full-height" @click="$router.push(tile.to)">
            <q-card-section>
              <div class="text-caption text-grey-8">{{ tile.label }}</div>
              <div class="text-h5 text-primary">{{ tile.value }}</div>
            </q-card-section>
          </q-card>
        </div>
      </div>
      <div v-if="overview" class="q-mt-lg">
        <div class="text-subtitle2 q-mb-sm">Current settings</div>
        <q-markup-table flat bordered dense>
          <tbody>
            <tr><td>Menu permissions</td><td>{{ overview.mode }}{{ overview.enforceAllLinks ? " (all menus)" : "" }}</td></tr>
            <tr><td>Site access check</td><td>{{ overview.siteGateMode }}</td></tr>
            <tr><td>Record-level check</td><td>{{ overview.recordScopeMode }}</td></tr>
          </tbody>
        </q-markup-table>
      </div>
    </q-card-section>
  </security-shell>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import securityShell from "../components/securityShell.vue";
import securityAdminService from "../securityAdmin.service";

const overview = ref(null);
const failed = ref(false);

const tiles = computed(() => [
  { label: "Users", value: overview.value.userCount, to: "/security-admin/users" },
  // { label: "Users with no access", value: overview.value.blockedUserCount, to: "/security-admin/site-access" },
  { label: "Roles", value: overview.value.roleCount, to: "/security-admin/roles" },
  { label: "Groups", value: overview.value.groupCount, to: "/security-admin/groups" },
  { label: "Individual permissions", value: overview.value.directGrantCount, to: "/security-admin/permissions" }
]);

onMounted(async () => {
  try {
    overview.value = await securityAdminService.getOverview();
  } catch {
    failed.value = true;
  }
});
</script>
