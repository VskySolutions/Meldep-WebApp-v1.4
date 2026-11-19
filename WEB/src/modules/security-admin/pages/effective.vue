<template>
  <security-shell title="Effective Permissions">
    <div class="row q-col-gutter-md q-pa-md items-center">
      <div class="col-12 col-sm-6 col-md-4"><user-select v-model="userId" label="User" @update:model-value="load" /></div>
      <div class="col-12 col-sm-6 col-md-4">
        <q-input v-model="filter" outlined dense clearable label="Search menus" aria-label="Search menus">
          <template #prepend><q-icon name="o_search" /></template>
        </q-input>
      </div>
      <div class="col-12 col-md-auto"><q-toggle v-model="grantedOnly" label="Only menus with access" /></div>
    </div>
    <q-banner v-if="result?.siteIssue" dense class="bg-red-1 text-negative q-mx-md q-mb-md" rounded>
      <template #avatar><q-icon name="o_block" /></template>
      {{ result.siteIssue }}
    </q-banner>
    <q-table
      v-if="userId"
      flat
      bordered
      dense
      row-key="link"
      :rows="visibleRows"
      :columns="columns"
      :filter="filter"
      :loading="loading"
      v-model:pagination="pagination"
      :rows-per-page-options="[20, 50, 100, 0]"
      no-data-label="No menus match."
    >
      <template #body="props">
        <q-tr :props="props">
          <q-td auto-width>
            <q-btn flat round dense size="sm" :icon="props.expand ? 'o_expand_less' : 'o_expand_more'" aria-label="Show sources" @click="props.expand = !props.expand" />
          </q-td>
          <q-td key="module" :props="props">{{ props.row.module }}</q-td>
          <q-td key="menu" :props="props">{{ props.row.menu }}</q-td>
          <q-td key="link" :props="props">{{ props.row.link }}</q-td>
          <q-td key="show" :props="props"><q-icon :name="props.row.show ? 'o_check' : 'o_remove'" :color="props.row.show ? 'positive' : 'grey-6'" size="20px" /></q-td>
          <q-td key="view" :props="props">
            <q-icon :name="props.row.view ? 'o_check' : 'o_close'" :color="props.row.view ? 'positive' : 'negative'" size="20px" />
            <span class="text-caption q-ml-xs">{{ props.row.viewReason }}</span>
          </q-td>
          <q-td key="manage" :props="props">
            <q-icon :name="props.row.manage ? 'o_check' : 'o_close'" :color="props.row.manage ? 'positive' : 'negative'" size="20px" />
            <span class="text-caption q-ml-xs">{{ props.row.manageReason }}</span>
          </q-td>
        </q-tr>
        <q-tr v-show="props.expand" :props="props">
          <q-td colspan="100%" class="bg-grey-1">
            <div v-if="props.row.sources.length === 0" class="text-caption text-grey-8">No role, group or individual rule applies to this menu.</div>
            <div v-for="(source, index) in props.row.sources" :key="index" class="text-caption">
              <strong>{{ source.kind }}: {{ source.name }}</strong> — Show {{ source.show ? "Yes" : "No" }}, View {{ source.view }}, Manage {{ source.manage }}
            </div>
          </q-td>
        </q-tr>
      </template>
    </q-table>
    <div v-else class="text-center text-grey-8 q-pa-xl">
      <q-icon name="o_verified_user" size="40px" /><div class="q-mt-sm">Choose a user to see what they can do in this site, and why.</div>
    </div>
  </security-shell>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import securityShell from "../components/securityShell.vue";
import userSelect from "../components/userSelect.vue";
import securityAdminService from "../securityAdmin.service";

import { useAuthStore } from "stores/auth";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

const authStore = useAuthStore();
const user = authStore.user;
const route = useRoute();

const result = ref(null);
const loading = ref(false);
const filter = computed({
  get: () => search.value.filter,
  set: value => {
    search.value.filter = value;
    saveDataTableState({ search: search.value });
  }
});

const grantedOnly = computed({
  get: () => search.value.grantedOnly,
  set: value => {
    search.value.grantedOnly = value;
    saveDataTableState({ search: search.value });
  }
});

const columns = [
  { name: "expand", label: "", field: "expand" },
  { name: "module", label: "Module", field: "module", align: "left", sortable: true },
  { name: "menu", label: "Menu", field: "menu", align: "left", sortable: true },
  { name: "link", label: "Link", field: "link", align: "left", sortable: true },
  { name: "show", label: "In menu", field: "show", align: "center" },
  { name: "view", label: "View", field: row => row.viewReason, align: "left" },
  { name: "manage", label: "Manage", field: row => row.manageReason, align: "left" }
];

const {
  search,
  pagination,
  saveDataTableState
} = useSiteTableState({
  storageKey: "effectivePermissions-Index",
  siteId: user?.siteId,

  defaultSearch: {
    filter: "",
    grantedOnly: false
  },

  defaultPagination: {
    page: 1,
    rowsPerPage: 50
  },

  defaultSorts: {},
  defaultResizableWidth: {},
  defaultColumns: []
});

const userId = ref(
  route.query.userId || search.value.userId || null
);

const visibleRows = computed(() => {
  const menus = result.value?.menus || [];
  return grantedOnly.value ? menus.filter(menu => menu.view || menu.manage) : menus;
});

async function load () {
  result.value = null;
  if (!userId.value) return;

  saveDataTableState({
    search: {
      ...search.value,
      userId: userId.value
    },
    pagination: pagination.value
  });

  loading.value = true;

  try {
    result.value = await securityAdminService.getEffective(userId.value);
  } finally {
    loading.value = false;
  }
}

onMounted(load);
</script>
