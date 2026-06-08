<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools flex justify-between items-center">
        <q-breadcrumbs class="text-brown text-weight-bold text-h3">
          <template #separator>
            <q-icon size="1.5em" name="o_chevron_right" color="primary" />
          </template>
          <q-breadcrumbs-el label="CRM" />
          <q-breadcrumbs-el label="Customer Files" />
        </q-breadcrumbs>
        <div class="row q-mt-sm q-gutter-sm" style="display: inline-flex;">
          <q-btn class="bg-grey-3 text-grey-10 q-mr-md btnRounded" label="Search" flat type="button" no-caps @click="advanceSearch">
            <q-icon :name="advanceSearchEnable ? 'o_filter_alt_off' : 'o_filter_alt'" />
          </q-btn>
          <q-btn icon="o_add" outline label="Add Customer File" no-caps class="text-primary btnRounded q-mr-md" @click="onAdd" />
        </div>
      </q-card-section>
      <q-separator />
      <div v-if="advanceSearchEnable">
        <div class="row q-my-md text-size search-input fieldCont q-ml-md q-mr-md">
          <div class="col-xxl-2 col-xl-2 col-lg-2 col-md-2 col-sm-4 col-xs-6 q-mb-sm">
            <label class="Cutomlabel">Created By</label>
            <q-select
              v-model="search.createdBy" class="q-mx-sm w-100 h-auto" stack-label hide-bottom-space use-input :dense="true"
              :options="createdByList" emit-value map-options :popup-content-class="customPopupContentClass"
            />
          </div>
          <div class="col-xxl-2 col-xl-2 col-lg-2 col-md-2 col-sm-4 col-xs-6 q-mb-sm">
            <label class="Cutomlabel">Note</label>
            <q-input v-model="search.note" hide-bottom-space :dense="true" />
          </div>
          <div class="col-xxl-2 col-xl-2 col-lg-2 col-md-2 col-sm-4 col-xs-6 q-mb-sm">
            <label class="Cutomlabel">Customer Name</label>
            <q-select
              v-model="search.customerId"
              class="q-ml-sm" use-input stack-label hide-bottom-space :dense="true"
              :options="customerList" option-value="value" option-label="text" emit-value map-options :popup-content-class="customPopupContentClass" clearable :disable="search.createdBy === 'Created By Me'" @filter="filterFn1"
            >
              <template #option="{ itemProps, opt }">
                <q-item v-bind="itemProps">
                  <q-item-section>
                    <div class="row q-col-gutter-x-md items-center" style="white-space: normal; overflow-wrap: break-word; max-width: 90%; width: 90%;">
                      <span>{{ opt.text }}</span>
                    </div>
                  </q-item-section>
                </q-item>
              </template>
            </q-select>
          </div>
          <div class="col-xxl-2 col-xl-2 col-lg-2 col-md-2 col-sm-4 col-xs-6 q-mb-sm">
            <label class="Cutomlabel">Year</label>
            <div class="input-group q-mx-sm w-100 h-auto">
              <q-input v-model="search.year" fill-input dense mask="####">
                <template #append>
                  <q-icon name="o_calendar_month" class="cursor-pointer">
                    <q-popup-proxy ref="qDateProxy" v-model="isPopupVisible" transition-show="scale" transition-hide="scale">
                      <q-date ref="date3ref" v-model="search.year" default-view="Years" emit-immediately minimal mask="YYYY" class="myDate" :options="onlyYears" @update:model-value="onUpdateMv2" />
                    </q-popup-proxy>
                  </q-icon>
                </template>
              </q-input>
            </div>
          </div>
          <div class="AdvanceBTN justify-center">
            <q-btn color="primary" outline label="Search" type="button" no-caps class="q-mx-md btnRounded q-px-lg q-mb-sm" @click="onSearch" />
            <q-btn color="grey-4" outline label="Clear" type="button" class="text-grey-9 text-h4 btnRounded clear q-px-lg q-mb-sm" no-caps @click="onClear" />
          </div>
        </div>
      </div>
      <q-table
        ref="tableRef"
        flat
        bordered
        :rows="rows"
        :columns="columns"
        row-key="id"
        separator="cell"
        :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
        :rows-per-page-options="[20 ,50, 100, 200, 500, 0]"
        @request="getCustomerFiles"
      >
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td colspan="2" style="background: #dbf2ff;" class="text-center">
              {{ props.row.companyClients?.company?.name ||
                (props.row.companyClients?.person?.firstName + ' ' + props.row.companyClients?.person?.lastName) }} -
              {{ props.row.year }}
              <span v-if="props.row.note">- ({{ props.row.note }})</span>
            </q-td>
            <q-td auto-width class="text-center actions" style="background: #dbf2ff;">
              <q-icon name="o_edit" class="cursor-pointer q-mr-sm" @click="onEdit(props.row.id)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" @click="onDelete(props.row)">
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
          <q-tr v-for="(line, index) in props.row.customerFilesLines" :key="line.id">
            <q-td class="text-left" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 80%;">{{ line.fileName }}</q-td>
            <q-td class="text-center" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;">{{ line.sortOrder }}</q-td>
            <q-td v-if="index === 0" :rowspan="props.row.customerFilesLines.length" class="text-left" style="width: 10%;" />
          </q-tr>
          <q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>

<script setup>
import { useQuasar } from "quasar";
import { ref, onMounted, watch } from "vue";
import { setLocalStorage, getLocalStorage, clearLocalStorage, zwConfirmDelete, notifySuccess } from "assets/utils";

import addEditFiles from "modules/customer-files/components/addEdit_files.vue";
import customerService from "src/modules/customer/customer.service";
import customerFileService from "modules/customer-files/customerFile.service";

const $q = useQuasar();
const loading = ref(true);
const localStorageKey = "CustomerFiles";
const filterLocalStorage = getLocalStorage(localStorageKey) || {};
const date3ref = ref(null);
const isPopupVisible = ref(false);
const advanceSearchEnable = ref(false);
const advanceSearch = () => { advanceSearchEnable.value = !advanceSearchEnable.value; };
const rows = ref([]);
const pagination = ref({
  sortBy: "",
  descending: true,
  rowsPerPage: 20,
  page: 1
});
const search = ref({
  note: "",
  createdBy: filterLocalStorage.createdBy || "Created By Me",
  customerId: filterLocalStorage.customerId || "",
  year: filterLocalStorage.year || new Date().getFullYear()
});
const onlyYears = (date) => { return true; };
const onUpdateMv2 = (val) => {
  search.value.year = val; // Update the reactive property with the selected year
  isPopupVisible.value = false; // Close the popup
};
const createdByList = ref(["Created By Me", "View All"]);

const getCustomerFiles = () => {
  loading.value = true;
  setLocalStorage(localStorageKey, search.value);

  const payload = {
    page: pagination.value.page,
    pageSize: pagination.value.rowsPerPage,
    sortBy: pagination.value.sortBy,
    descending: pagination.value.descending,
    ...search.value
  };

  customerFileService.getCustomerFiles(payload)
    .then((resp) => {
      rows.value = resp.data || [];
      pagination.value.rowsNumber = resp.total;
    })
    .finally(() => {
      loading.value = false;
    });
};
const tableRef = ref();
const columns = [
  // { name: "customerName", label: "Customer Name", align: "left", field: "customerName", sortable: true },
  // { name: "year", label: "Year", align: "center", field: "year", sortable: true },
  { name: "fileName", label: "File Name", align: "left", field: "fileName", sortable: true },
  { name: "sortOrder", label: "Document No", align: "center", field: "sortOrder", sortable: true },
  { name: "action", label: "Action", align: "center", field: "action", sortable: false }
];

// Sorting Data
// Row Span Logic
// const shouldShowCustomerRow = (index) =>
//   index === 0 || rows.value[index].customerName !== rows.value[index - 1].customerName;

// const getCustomerRowSpan = (index) =>
//   rows.value.filter((c) => c.customerName === rows.value[index].customerName).length;

// const shouldShowYearRow = (index) =>
//   index === 0 ||
//   rows.value[index].customerName !== rows.value[index - 1].customerName ||
//   rows.value[index].year !== rows.value[index - 1].year;

// const getYearRowSpan = (index) =>
//   rows.value.filter(
//     (c) => c.customerName === rows.value[index].customerName && c.year === rows.value[index].year
//   ).length;

// const shouldShowCustomerRows = (index) => {
//   return index === 0 ||
//          rows.value[index].customerId !== rows.value[index - 1].customerId ||
//          rows.value[index].year !== rows.value[index - 1].year;
// };

// const getCustomerRowSpans = (index) => {
//   return rows.value.filter((c) =>
//     c.customerId === rows.value[index].customerId &&
//     c.year === rows.value[index].year
//   ).length;
// };

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getCustomerFiles(propps);
};

// Clear search
const onClear = () => {
  search.value.createdBy = "Created By Me";
  search.value.customerId = "";
  search.value.year = getCurrentMonthYear();
  clearLocalStorage(localStorageKey);
  onSearch();
};

// Create popup
const onAdd = () => {
  $q.dialog({
    component: addEditFiles,
    componentProps: {}
  }).onOk(() => {
    getCustomerFiles({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

const customerList = ref([]);
const options1 = ref([]);
const getAllCustomerListForDropdown = () => {
  customerService.getAllClientListForDropdown().then((resp) => {
    customerList.value = resp.map((item) => ({
      text: item.company ? item.company.name : `${item.person.firstName} ${item.person.lastName}`,
      value: item.id
    })).sort((a, b) => a.text.localeCompare(b.text));
    options1.value = customerList.value;
  });
};

// Search Customer for dropdown
function filterFn1 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      customerList.value = options1.value;
    } else {
      customerList.value = options1.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

function getCurrentMonthYear () {
  const today = new Date();
  const year = today.getFullYear();
  return `${year}`; // Format as 'Month-YYYY'
}

watch(() => search.value.createdBy, (newValue) => {
  if (newValue === "Created By Me") {
    search.value.customerId = ""; // Clear the employee selection
  }
});

// Edit popup
// const onEdit = (customerId, year) => {
//   $q.dialog({
//     component: addEditFiles,
//     componentProps: { customerId, year }
//   }).onOk(() => {
//     getCustomerFiles({ pagination: pagination.value });
//   }).onCancel(() => {
//   }).onDismiss(() => {
//   });
// };
const onEdit = (id) => {
  $q.dialog({
    component: addEditFiles,
    componentProps: { id }
  }).onOk(() => {
    getCustomerFiles({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

const onDelete = (item) => {
  zwConfirmDelete({ data: `${item.note}` }, () => {
    customerFileService.deleteCustomerFile(item.id).then(resp => {
      notifySuccess({ message: "Customer files is deleted successfully." });
      getCustomerFiles({ pagination: pagination.value });
    });
  }, () => {
  });
};

onMounted(() => {
  tableRef.value.requestServerInteraction();
  // getCustomerFiles();
  getAllCustomerListForDropdown();
  getCurrentMonthYear();
});
</script>

<style>
.q-table thead tr {
  background-color: #1b75ab !important;
  color: white !important;
}
</style>
