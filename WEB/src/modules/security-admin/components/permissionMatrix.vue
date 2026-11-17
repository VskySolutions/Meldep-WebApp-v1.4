<template>
  <div>
    <div class="row items-center q-pa-md q-col-gutter-md">
      <div class="col-12 col-sm-6 col-md-4">
        <q-input v-model="filter" outlined dense clearable label="Search menus" aria-label="Search menus">
          <template #prepend><q-icon name="o_search" /></template>
        </q-input>
      </div>
      <div class="col-12 col-sm-6 col-md-8 text-caption text-grey-8">
        Inherit adds nothing and leaves the decision to other sources. Deny overrides role and group grants.
      </div>
    </div>
    <q-table
      flat
      bordered
      dense
      row-key="siteModuleMenuId"
      :rows="rows"
      :columns="columns"
      :filter="filter"
      :loading="loading"
      :pagination="{ rowsPerPage: 50, sortBy: 'module' }"
      :rows-per-page-options="[20, 50, 100, 0]"
      no-data-label="No menus are available in this site."
    >
      <template #body-cell-showInMenu="props">
        <q-td :props="props">
          <q-checkbox
            dense
            :model-value="props.row.showInMenu"
            :aria-label="`Show ${props.row.menu} in the menu`"
            @update:model-value="value => change(props.row, { showInMenu: value })"
          />
        </q-td>
      </template>
      <template #body-cell-viewAccess="props">
        <q-td :props="props">
          <q-btn-toggle
            dense
            unelevated
            no-caps
            size="sm"
            toggle-color="primary"
            :model-value="props.row.viewAccess"
            :options="accessOptions"
            :aria-label="`View access for ${props.row.menu}`"
            @update:model-value="value => change(props.row, { viewAccess: value })"
          />
        </q-td>
      </template>
      <template #body-cell-manageAccess="props">
        <q-td :props="props">
          <q-btn-toggle
            dense
            unelevated
            no-caps
            size="sm"
            toggle-color="primary"
            :model-value="props.row.manageAccess"
            :options="accessOptions"
            :aria-label="`Manage access for ${props.row.menu}`"
            @update:model-value="value => change(props.row, { manageAccess: value })"
          />
        </q-td>
      </template>
      <template v-if="requireReason" #body-cell-reason="props">
        <q-td :props="props" class="text-caption" style="white-space: normal;">
          {{ props.row.reason }}
          <div v-if="props.row.expiresOnUtc" class="text-grey-7">Ends {{ formatUtc(props.row.expiresOnUtc) }}</div>
        </q-td>
      </template>
    </q-table>

    <q-dialog v-model="reasonDialog.open" persistent>
      <q-card style="min-width: 420px; max-width: 95vw;">
        <q-card-section class="text-subtitle1">Individual permission: {{ reasonDialog.row?.menu }}</q-card-section>
        <q-card-section class="q-pt-none q-gutter-md">
          <q-input v-model="reasonDialog.reason" outlined dense autofocus label="Reason *" aria-label="Reason" maxlength="500" />
          <q-input v-model="reasonDialog.expires" outlined dense type="date" stack-label label="Ends on (optional)" aria-label="Ends on" :min="today" />
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat no-caps label="Cancel" @click="reasonDialog.open = false" />
          <q-btn unelevated no-caps color="primary" label="Save" :disable="!reasonDialog.reason?.trim()" @click="confirmReason" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script setup>
import { computed, reactive, ref } from "vue";
import { formatUtc } from "../securityAdmin.service";

const props = defineProps({
  rows: { type: Array, required: true },
  loading: { type: Boolean, default: false },
  requireReason: { type: Boolean, default: false },
  save: { type: Function, required: true }
});

const filter = ref("");
const today = new Date().toISOString().slice(0, 10);

const accessOptions = [
  { label: "Inherit", value: 0 },
  { label: "Allow", value: 1 },
  { label: "Deny", value: 2 }
];

const columns = computed(() => [
  { name: "module", label: "Module", field: "module", align: "left", sortable: true },
  { name: "menu", label: "Menu", field: "menu", align: "left", sortable: true },
  { name: "link", label: "Link", field: "link", align: "left", sortable: true },
  { name: "showInMenu", label: "Show", field: "showInMenu", align: "center" },
  { name: "viewAccess", label: "View", field: "viewAccess", align: "center" },
  { name: "manageAccess", label: "Manage", field: "manageAccess", align: "center" },
  ...(props.requireReason ? [{ name: "reason", label: "Reason", field: "reason", align: "left" }] : [])
]);

const reasonDialog = reactive({ open: false, row: null, change: null, reason: "", expires: "" });

function change (row, values) {
  const next = { ...row, ...values };
  const isEmpty = !next.showInMenu && next.viewAccess === 0 && next.manageAccess === 0;

  // Individual grants need a reason, but clearing one back to Inherit doesn't.
  if (props.requireReason && !isEmpty) {
    Object.assign(reasonDialog, {
      open: true,
      row,
      change: values,
      reason: row.reason || "",
      expires: row.expiresOnUtc ? String(row.expiresOnUtc).slice(0, 10) : ""
    });
    return;
  }

  apply(row, values, { reason: null, expiresOnUtc: null });
}

function confirmReason () {
  const expiresOnUtc = reasonDialog.expires ? new Date(`${reasonDialog.expires}T23:59:59`).toISOString() : null;
  reasonDialog.open = false;
  apply(reasonDialog.row, reasonDialog.change, { reason: reasonDialog.reason.trim(), expiresOnUtc });
}

async function apply (row, values, extra) {
  const previous = { ...row };
  Object.assign(row, values, extra);

  try {
    await props.save({
      siteModuleMenuId: row.siteModuleMenuId,
      showInMenu: row.showInMenu,
      viewAccess: row.viewAccess,
      manageAccess: row.manageAccess,
      reason: row.reason,
      expiresOnUtc: row.expiresOnUtc
    });
  } catch {
    // The request's error is already shown, so put the control back to the saved value.
    Object.assign(row, previous);
  }
}
</script>
