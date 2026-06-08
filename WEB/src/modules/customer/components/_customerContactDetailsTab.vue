<template>
  <fieldset>
    <legend>Contact Details</legend>
    <!-- <q-table :rows="rows" :columns="contactColumns" row-key="id"> -->      
      <q-table
        :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
        flat
        :loading="loading"
        :rows="rows"
        :columns="contactColumns"
        row-key="id"
        separator="cell"
        no-data-label="No data available"
        :filter="filter"
        binary-state-sort
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        @request="getCompanyContacts"
      >
      <template #loading>
        <q-inner-loading showing color="primary">
          <q-spinner-ios size="40px" class="q-mt-xl" />
        </q-inner-loading>
      </template>
      <template #header="contactProps">
        <q-tr :props="contactProps" class="bg-primary text-white">
          <q-th v-for="col in contactProps.cols" :key="col.name" :props="contactProps">{{ col.label }}</q-th>
        </q-tr>
      </template>
      <template #body="contactProps">
        <q-tr :props="contactProps">
          <q-td>{{ contactProps.row.person?.firstName || model.name || '' }}</q-td>
          <q-td>{{ contactProps.row.person?.lastName || '' }}</q-td>
          <q-td>{{ contactProps.row.person?.primaryEmailAddress || '' }}</q-td>
          <q-td>{{ contactProps.row.alternateEmail || '' }}</q-td>
          <q-td>{{ contactProps.row.person?.primaryPhoneNumber || '' }}</q-td>
          <q-td>{{ contactProps.row.alternatePhoneNumber || '' }}</q-td>
        </q-tr>
        <q-separator />
      </template>
    </q-table>
  </fieldset>
</template>
<script setup>
import { ref, watch } from "vue";

import companyService from "modules/company/company.service";
const props = defineProps({ companyId: { type: String, default: "" } });
const companyId = props.companyId;
const rows = ref([]);
const loading = ref(true);

const contactColumns = ref([
  { name: "person.firstName", label: "First Name", field: row => row.person?.firstName || "-", align: "left", sortable: true },
  { name: "person.lastName", label: "Last Name", field: row => row.person?.lastName || "-", align: "left", sortable: true, style: "width: 10px" },
  { name: "person.primaryEmailAddress", label: "Email", field: row => row.person?.primaryEmailAddress || "-", align: "left" },
  { name: "alternateEmail", label: "Alt. Email", field: "alternateEmail", align: "left" },
  { name: "person.primaryPhoneNumber", label: "Phone Number", field: row => row.person?.primaryPhoneNumber || "-", align: "left" },
  { name: "alternatePhoneNumber", label: "Alt. Phone Number", field: "alternatePhoneNumber", align: "left" }
]);

// function getCompanyContacts () {
//   loading.value = true;
//   companyService.getCompanyContacts(companyId).then((resp) => {
//     rows.value = resp;
//   });
// }

const getCompanyContacts = () => {
  loading.value = true;
  companyService.getCompanyContacts(companyId).then((resp) => {
    rows.value = resp;
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => companyId, (newValue, oldValue) => {
  if (newValue) {
    getCompanyContacts();
  }
}, { immediate: true });
</script>
