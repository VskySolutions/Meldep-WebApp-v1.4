<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools flex justify-between items-center">
        <h1 class="text-dark">Sales Person</h1>
        <div class="items-center">
          <q-btn class="bg-grey-3 text-grey-10 q-mr-md btnRounded" label="Search" flat type="button" no-caps @click="advanceSearch">
            <q-icon :name="advanceSearchEnable ? 'o_filter_alt_off' : 'o_filter_alt'" />
          </q-btn>
          <q-btn icon="o_add" outline label="Add Employee" no-caps class="text-primary btnRounded" @click="onAdd" />
        </div>
      </q-card-section>
      <q-separator />
      <div v-if="advanceSearchEnable">
        <div class="row q-my-md text-size search-input justify-center fieldCont">
          <div class="col-xxl-3 col-xl-3 col-lg-3 col-md-2 col-sm-6 col-xs-12 q-mb-sm">
            <label class="Cutomlabel">Employee Name</label>
            <q-select v-model="search.personIds" push class="q-mx-sm w-100 h-auto" clearable use-input use-chips transition-show="jump-up" transition-hide="jump-up" hide-bottom-space :dense="true" multiple fill-input input-debounce="0" :options="personList" option-value="value" option-label="text" emit-value map-options :popup-content-class="customPopupContentClass" @filter="filterFn1">
              <template #option="{ itemProps, opt, selected, toggleOption }">
                <q-item v-bind="itemProps">
                  <q-item-section>
                    <div class="row q-col-gutter-x-md items-center">
                      <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                      <span>{{ opt.text }}</span>
                    </div>
                  </q-item-section>
                </q-item>
              </template>
            </q-select>
          </div>
          <div class="col-xxl-3 col-xl-3 col-lg-3 col-md-3 col-md-4 col-sm-6 col-xs-12 q-mb-sm">
            <label class="Cutomlabel">Employee Email</label>
            <q-input v-model="search.primaryEmailAddress" push class="q-mx-sm w-100 h-auto" hide-bottom-space :dense="true" type="email"/>
          </div>
        </div>
        <div class="q-mb-lg AdvanceBTN justify-center">
          <q-btn color="primary" outline label="Search" type="button" no-caps class="q-mx-md btnRounded q-px-lg" @click="onSearch" />
          <q-btn color="grey-4" outline label="Clear" type="button" class="btnRounded text-grey-9 text-h4 clear q-px-lg" no-caps @click="onClear" />
        </div>
      </div>
      <q-separator />

      <q-table
        ref="tableRef" v-model:pagination="pagination" class="Custom-DataTable my-sticky-header-table" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
        no-data-label="I didn't find anything for you" binary-state-sort @request="getEmployees"
      >
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td>{{ props.row.person.firstName }}</q-td>
            <q-td>{{ props.row.person.lastName }}</q-td>
            <q-td>{{ props.row.person.primaryEmailAddress }}</q-td>
            <q-td auto-width class="text-center actions">
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" @click="onView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon name="o_edit" class="cursor-pointer q-mr-sm" @click="onEdit(props.row.id)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" @click="onDelete(props.row)">
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted } from "vue";
import { useQuasar } from "quasar";
import editSalesPerson from "src/modules/lead/components/addEditSalesPerson.vue";
import employeesService from "modules/lead/sales_person.service";
import { zwConfirmDelete, notifySuccess, setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";

// Common variables
const $q = useQuasar();
const loading = ref(true);
const advanceSearchEnable = ref(false);
const advanceSearch = () => { advanceSearchEnable.value = !advanceSearchEnable.value; };

// local storage values
const localStorageKey = "Employee";
const filterLocalStorage = getLocalStorage(localStorageKey);
const personIds = filterLocalStorage ? filterLocalStorage.personIds : [];
const primaryEmailAddress = filterLocalStorage ? filterLocalStorage.primaryEmailAddress : "";

// Search variables
const search = ref({
  personIds: personIds,
  primaryEmailAddress: primaryEmailAddress
});

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "person.firstName", label: "First Name", field: "person.firstName", align: "left", sortable: true },
  { name: "person.lastName", label: "Last Name", field: "person.lastName", align: "left", sortable: true },
  { name: "person.primaryEmailAddress", label: "Email Address", field: "person.primaryEmailAddress", align: "left", sortable: true }
]);

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  getAllEmployeesListForDropdown();
});

// Get/Map project list to table
const getEmployees = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, search.value);
  employeesService.getEmployees(payload).then((resp) => {
    rows.value = resp.data;
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getEmployees(propps);
};

// Clear search
const onClear = () => {
  search.value.personIds = [];
  search.value.primaryEmailAddress = "";
  clearLocalStorage(localStorageKey);
  onSearch();
};

// Get all employee list for dropdown
const personList = ref([]);
const options1 = ref([]);
function getAllEmployeesListForDropdown () {
  employeesService.getAllEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.person.id }));
    personList.value = responseData;
    options1.value = responseData;
  });
}
// Search person for dropdown
function filterFn1 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      personList.value = options1.value;
    } else {
      personList.value = options1.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Create popup
const onAdd = () => {
  $q.dialog({
    component: editSalesPerson,
    componentProps: {}
  }).onOk(() => {
    getEmployees({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Edit popup
const onEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({
    // component: editSalesPerson,
    componentProps: { id }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// View popup
// const onView = (id) => {
//   activeRowId.value = id;
//   $q.dialog({
//     component: viewEmployee,
//     componentProps: { id }
//   }).onOk(() => {
//     getEmployees();
//   }).onCancel(() => {
//   }).onDismiss(() => {
//     activeRowId.value = null;
//   });
// };

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.fullName}` }, () => {
    employeesService.deleteEmployee(item.id).then(resp => {
      notifySuccess({ message: "Employee is deleted successfully." });
      getEmployees({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};
</script>
