<template>
  <fieldset class="q-mb-lg">
    <q-table
      v-model:pagination="pagination"
      bordered
      class="no-shadow"
      :loading="loading"
      :rows="rows"
      :columns="ftpColumns"
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
          <q-td style="width: 10%">{{ props.row.protocolType.dropDownValue }}</q-td>
          <q-td style="width: 10%">{{ props.row.encryptionType.dropDownValue }}</q-td>
          <q-td style="width: 10%">{{ props.row.walletType.dropDownValue }}</q-td>
          <q-td style="width: 10%">{{ props.row.walletNumber }}</q-td>
          <q-td style="width: 10%">{{ props.row.name }}</q-td>
          <q-td style="width: 10%">{{ props.row.host }}</q-td>
          <q-td style="width: 10%">{{ props.row.port }}</q-td>
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

const ftpColumns = ref([
  { name: "infraService.name", label: "Infra Account Service", field: "infraService.name", align: "left", sortable: true },
  { name: "protocolType.dropDownValue", label: "Protocol Type", field: "protocolType.dropDownValue", align: "left", sortable: true },
  { name: "encryptionType.dropDownValue", label: "Encryption Type", field: "encryptionType.dropDownValue", align: "left", sortable: true },
  { name: "walletTypeId", label: "Wallet Type", field: "walletTypeId", align: "left", sortable: true },
  { name: "walletNumber", label: "Wallet Number", field: "walletNumber", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "host", label: "Host", field: "host", align: "left", sortable: true },
  { name: "port", label: "Port", field: "port", align: "left", sortable: true },
  { name: "instructions", label: "Instructions", field: "instructions", align: "left", sortable: true }
]);
</script>
