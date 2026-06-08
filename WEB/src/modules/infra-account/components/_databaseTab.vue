<template>
  <fieldset class="q-mb-lg">
    <q-table
      v-model:pagination="pagination"
      bordered
      class="no-shadow"
      :loading="loading"
      :rows="rows"
      :columns="databaseColumns"
      row-key="id"
      separator="cell"
      binary-state-sort
      :rows-per-page-options="[20, 50, 100, 200, 500]"
    >
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
        </q-tr>
      </template>

      <template #body="props">
        <q-tr :props="props">
          <q-td style="width: 10%">{{ props.row.infraService.name }}</q-td>
          <q-td style="width: 10%">{{ props.row.walletType.dropDownValue }}</q-td>
          <q-td style="width: 10%">{{ props.row.walletNumber }}</q-td>
          <q-td style="width: 20%">{{ props.row.name }}</q-td>
          <q-td style="width: 20%">{{ props.row.serverName }}</q-td>
          <q-td style="width: 10%">{{ props.row.isReadOrWrite ? 'Read' : 'Write' }}</q-td>
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;">
            <p v-html="props.row.instructions || '-'" />
          </q-td>
        </q-tr>
      </template>
    </q-table>
  </fieldset>
</template>

<script setup>
import { ref } from "vue";

defineProps({
  rows: Array,
  loading: Boolean
});

const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const databaseColumns = ref([
  { name: "infraService.name", label: "Infra Account Service", field: "infraService.name", align: "left", sortable: true },
  { name: "walletTypeId", label: "Wallet Type", field: "walletTypeId", align: "left", sortable: true },
  { name: "walletNumber", label: "Wallet Number", field: "walletNumber", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "serverName", label: "Server Name", field: "serverName", align: "left", sortable: true },
  { name: "isReadOrWrite", label: "Access Type", field: "isReadOrWrite", align: "left", sortable: true },
  { name: "instructions", label: "Instructions", field: "instructions", align: "left", sortable: true }
]);

</script>
