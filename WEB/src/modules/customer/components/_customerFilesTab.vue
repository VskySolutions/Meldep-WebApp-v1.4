<template>
  <fieldset>
    <legend>{{ model.person ? 'Customer Files' : 'Company Files' }}</legend>

    <q-table
      ref="tableRef"
      flat
      bordered
      :rows="FilesRows"
      :columns="Filescolumns"
      :pagination="FilesPagination"
      row-key="id"
      separator="cell"
      :class="FilesRows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
    >
      <template #body="props">
        <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
          <q-td colspan="2" style="background: #dbf2ff;" class="text-center">{{ props.row.companyClients?.company?.name || props.row.companyClients?.person?.firstName + ' ' + props.row.companyClients?.person?.lastName }} - {{ props.row.year }} - ({{ props.row.note }})</q-td>
        </q-tr>
        <q-tr v-for="(line) in props.row.customerFilesLines" :key="line.id">
          <q-td class="text-left" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 80%;">{{ line.fileName }}</q-td>
          <q-td class="text-center" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;">{{ line.sortOrder }}</q-td>
        </q-tr>
        <q-separator/>
      </template>
    </q-table>
  </fieldset>
</template>
<script setup>
import { ref, onMounted } from "vue";

import customerFileService from "modules/customer-files/customerFile.service";

const props = defineProps({ customerId: { type: String, default: "" }, isPerson: { type: Boolean, default: false } });
const customerId = props.customerId;

const model = ref({
  description: "",
  person: props.isPerson
});
// Search variables
const search = ref({
  customerId: customerId
});
const loading = ref(true);
const activeRowId = ref(null);
const FilesRows = ref([]);
const FilesPagination = ref({
  sortBy: "",
  descending: true,
  rowsPerPage: 20,
  page: 1
});
const Filescolumns = [
  { name: "fileName", label: "File Name", align: "left", field: "fileName", sortable: true },
  { name: "sortOrder", label: "Document No", align: "center", field: "sortOrder", sortable: true }
];
// getCustomerFiles
const getCustomerFiles = () => {
  loading.value = true;
  const payload = {
    page: FilesPagination.value.page,
    pageSize: FilesPagination.value.rowsPerPage,
    sortBy: FilesPagination.value.sortBy,
    descending: FilesPagination.value.descending,
    ...search.value
  };

  customerFileService.getCustomerFiles(payload)
    .then((resp) => {
      FilesRows.value = resp.data || [];
      FilesPagination.value.rowsNumber = resp.total;
    })
    .finally(() => {
      loading.value = false;
    });
};

onMounted(() => {
  getCustomerFiles();
});

</script>
