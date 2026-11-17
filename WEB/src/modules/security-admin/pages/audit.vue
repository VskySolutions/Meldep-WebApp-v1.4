<template>
  <security-shell title="Audit Log">
    <div class="row q-col-gutter-md q-pa-md items-center">
      <div class="col-12 col-sm-6 col-md-3">
        <q-select
          v-model="filters.category"
          :options="eventTypeForDropdown.list.value"
          :filter="eventTypeForDropdown.filter"
          outlined
          use-input
          dense
          clearable
          emit-value
          map-options
          option-label="text"
          option-value="value"
          label="Type"
          aria-label="Type"
          @update:model-value="reload"
        />
        </div>
      <div class="col-12 col-sm-6 col-md-3">
        <q-input v-model="filters.search" outlined dense clearable debounce="300" label="Search" aria-label="Search" @update:model-value="reload">
          <template #prepend><q-icon name="o_search" /></template>
        </q-input>
      </div>
      <div class="col-6 col-md-2"><q-input v-model="filters.from" outlined dense stack-label type="date" label="From" aria-label="From" @update:model-value="reload" /></div>
      <div class="col-6 col-md-2"><q-input v-model="filters.to" outlined dense stack-label type="date" label="To" aria-label="To" @update:model-value="reload" /></div>
      <div v-if="canSeeAllSites" class="col-12 col-md-2"><q-toggle v-model="filters.allSites" label="All sites" @update:model-value="reload" /></div>
    </div>
    <q-table
      v-model:pagination="pagination"
      flat
      bordered
      dense
      row-key="id"
      :rows="rows"
      :columns="columns"
      :loading="loading"
      :rows-per-page-options="[20, 50, 100, 200]"
      no-data-label="No audit entries match."
      @request="load"
    >
      <template #body="props">
        <q-tr :props="props">
          <q-td auto-width>
            <q-btn flat round dense size="sm" :icon="props.expand ? 'o_expand_less' : 'o_expand_more'" aria-label="Show details" @click="props.expand = !props.expand" />
          </q-td>
          <q-td key="createdOnUtc" :props="props">{{ formatUtc(props.row.createdOnUtc) }}</q-td>
          <q-td key="message" :props="props">{{ props.row.message }}</q-td>
          <q-td key="userName" :props="props">{{ props.row.userName || "—" }}</q-td>
          <q-td key="pageUrl" :props="props">{{ props.row.pageUrl }}</q-td>
          <q-td key="ipAddress" :props="props">{{ props.row.ipAddress }}</q-td>
        </q-tr>
        <q-tr v-show="props.expand" :props="props">
          <q-td colspan="100%" class="bg-grey-1"><pre class="q-ma-none text-caption" style="white-space: pre-wrap; word-break: break-word;">{{ pretty(props.row.details) }}</pre></q-td>
        </q-tr>
      </template>
    </q-table>
  </security-shell>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from "vue";
import securityShell from "../components/securityShell.vue";
import securityAdminService, { formatUtc } from "../securityAdmin.service";
import securityAdminModule from "src/modules/security-admin/utils/dropdowns.js";

const rows = ref([]);
const loading = ref(false);
const canSeeAllSites = ref(false);
const pagination = ref({ page: 1, rowsPerPage: 20, rowsNumber: 0 });
const filters = reactive({ category: null, search: "", from: "", to: "", allSites: false });

const baseCategories = [
  { label: "Denied", value: "denied" },
  { label: "Would deny (audit mode)", value: "would-deny" },
  { label: "Permission changes", value: "permission" },
  { label: "Sign-in and site switching", value: "session" },
  { label: "User activity", value: "activity" }
];

// The API allows error rows for System Super Admins only.
const categories = computed(() => (canSeeAllSites.value
  ? [...baseCategories, { label: "Errors", value: "errors" }]
  : baseCategories));

const columns = [
  { name: "expand", label: "", field: "expand" },
  { name: "createdOnUtc", label: "When", field: "createdOnUtc", align: "left" },
  { name: "message", label: "Event", field: "message", align: "left" },
  { name: "userName", label: "User", field: "userName", align: "left" },
  { name: "pageUrl", label: "Path", field: "pageUrl", align: "left" },
  { name: "ipAddress", label: "IP address", field: "ipAddress", align: "left" }
];

function pretty (details) {
  try {
    return JSON.stringify(JSON.parse(details), null, 2);
  } catch {
    return details || "No further details.";
  }
}

// Date inputs are local days, so these are converted to the matching UTC moments.
function boundary (date, time) {
  return date ? new Date(`${date}T${time}`).toISOString() : undefined;
}

async function load ({ pagination: page }) {
  loading.value = true;
  try {
    const result = await securityAdminService.getAudit({
      category: filters.category || undefined,
      search: filters.search || undefined,
      fromUtc: boundary(filters.from, "00:00:00"),
      toUtc: boundary(filters.to, "23:59:59"),
      allSites: filters.allSites,
      page: page.page,
      pageSize: page.rowsPerPage
    });
    rows.value = result.items;
    canSeeAllSites.value = result.canSeeAllSites;
    pagination.value = { ...page, rowsNumber: result.total };
  } finally {
    loading.value = false;
  }
}

function reload () {
  load({ pagination: { ...pagination.value, page: 1 } });
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { eventTypeForDropdown } = securityAdminModule();

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  await eventTypeForDropdown.load("Audit Log Event Type");
  console.log(
    "Dropdown options:",
    eventTypeForDropdown.list.value
  );
  reload();
});
</script>
