<template>
  <q-page padding>
    <q-card flat bordered>
      <q-card-section class="card-header with-tools">
        <q-breadcrumbs class="text-brown text-weight-bold text-h3">
          <template #separator>
            <q-icon size="1.5em" name="o_chevron_right" color="primary" />
          </template>
          <q-breadcrumbs-el label="Settings" to="/settings" />
          <q-breadcrumbs-el label="Security Administration" :to="title ? '/security-admin' : undefined" />
          <q-breadcrumbs-el v-if="title" :label="title" />
        </q-breadcrumbs>
      </q-card-section>
      <q-separator />
      <q-tabs dense no-caps align="left" active-color="primary" indicator-color="primary" class="text-grey-8" outside-arrows mobile-arrows>
        <q-route-tab v-for="tab in tabs" :key="tab.to" :to="tab.to" :label="tab.label" :icon="tab.icon" exact />
      </q-tabs>
      <q-separator />
      <slot />
    </q-card>
  </q-page>
</template>

<script setup>
defineProps({
  title: { type: String, default: "" }
});

const tabs = [
  { to: "/security-admin", label: "Overview", icon: "o_shield" },
  { to: "/security-admin/users", label: "Users", icon: "o_person" },
  { to: "/security-admin/roles", label: "Roles", icon: "o_badge" },
  { to: "/security-admin/groups", label: "Groups", icon: "o_groups" },
  { to: "/security-admin/permissions", label: "Permissions", icon: "o_tune" },
  // { to: "/security-admin/site-access", label: "Site Access", icon: "o_domain" },
  { to: "/security-admin/effective", label: "Effective Permissions", icon: "o_verified_user" },
  { to: "/security-admin/audit", label: "Audit Log", icon: "o_history" }
];
</script>
