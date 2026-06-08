<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Expenses" />
              <q-breadcrumbs-el label="Approve Purchase Expenses" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-4">
            <div class="row items-center q-ml-lg">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container" style="position: relative; display: flex; align-items: center; width: 320px;">
                  <q-input v-model="searchText" :loading="searchLoader" outlined dense clearable debounce="300" placeholder="Search" class="bg-white search-box" style="flex: 1; border-top-right-radius: 0; border-bottom-right-radius: 0; border-top-right-radius: 0; max-width: 250px;">
                    <template #prepend>
                      <q-icon name="o_search" />
                    </template>
                  </q-input>
                  <q-btn
                    unelevated
                    :color="Object.keys(appliedFilters).length > 0 ? 'primary' : 'grey-7'"
                    text-color="white"
                    size=""
                    class="q-pa-xs q-mr-xs filter-btn"
                    style="height: 40px; border-top-left-radius: 0; border-bottom-left-radius: 0;"
                    @click.stop="showFilter = !showFilter"
                  >
                    <q-badge v-if="Object.keys(appliedFilters).length > 0" color="green" floating>{{ Object.keys(appliedFilters).length }}</q-badge>
                    <q-icon name="o_filter_alt" size="sm" color="white" class="q-mr-xs" />
                    <q-item-label class="text-xs fs-12">
                      <span class="block">Set/Clear</span>
                      <span class="block">FILTER</span>
                    </q-item-label>
                    <q-tooltip anchor="bottom middle" self="top middle">Purchased Filter</q-tooltip>
                  </q-btn>
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Requested By</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.employeeId"
                            class="q-mx-sm w-100 h-auto"
                            use-input
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            :options="employeeList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            @filter="getAllEmployeesListForFilter"
                          >
                            <template #option="{ itemProps, opt }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center">
                                    <span>{{ opt.text }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">ReferenceId</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.referenceId"
                            push
                            class="q-mx-sm w-100 h-auto"
                            clearable
                            use-input
                            use-chips
                            transition-show="jump-up"
                            transition-hide="jump-up"
                            hide-bottom-space
                            :dense="true"
                            multiple
                            fill-input
                            input-debounce="0"
                            :options="purchaseExpenseRequestList"
                            option-value="value"
                            option-label="text"
                            emit-value map-options
                            :popup-content-class="customPopupContentClass"
                            @filter="getAllPurchaseExpenseRequestDropDownListFilter"
                          >
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
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Status</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.statusId"
                            push
                            class="q-mx-sm w-100 h-auto"
                            clearable
                            use-input
                            use-chips
                            transition-show="jump-up"
                            transition-hide="jump-up"
                            hide-bottom-space
                            :dense="true"
                            multiple
                            fill-input
                            input-debounce="0"
                            :options="purchaseExpenseStatusList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                            @filter="getAllPurchaseExpenseStatusDropDownFilter"
                          >
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
                      </div>
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="q-table-container q-pa-none">
        <q-table
          ref="tableRef"
          v-model:pagination="pagination"
          :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
          bordered
          :loading="loading"
          :rows="rows"
          :columns="columns"
          row-key="id"
          separator="cell"
          no-data-label="No data available"
          binary-state-sort
          :rows-per-page-options="[20, 50, 100, 200, 500]"
          @request="getAllApprovePurchaseExpenseRequests"
        >
          <template #loading>
            <q-inner-loading showing color="primary">
              <q-spinner-ios size="40px" class="q-mt-xl" />
            </q-inner-loading>
          </template>
          <template #header="props">
            <q-tr :props="props" class="bg-primary text-white">
              <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
              <q-th auto-width class="text-center">Actions</q-th>
            </q-tr>
          </template>
          <template #body="props">
            <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
              <q-td style="width: 20%;">{{ props.row.referenceId }}
                <q-icon
                  v-if="props.row.isEdited"
                  name="o_edit"
                  size="xs"
                  color="primary"
                  class="q-ml-xs"
                >
                  <q-tooltip>Purchase Expense Updated</q-tooltip>
                </q-icon>
              </q-td>
              <q-td class="text-center" style="width: 10%;">{{ props.row.requestDate.replace(/-/g, '/') }}</q-td>
              <q-td style="width: 16%;">{{ props.row.itemCategory }}</q-td>
              <q-td style="width: 16%;">{{ props.row.itemName }}</q-td>
              <q-td style="width: 10%;">{{ props.row.requestedEmployee }}</q-td>
              <q-td style="width: 10%;">
                <q-badge :style="{ backgroundColor: props.row.bgColor, color:props.row.color }">
                  <span v-if="props.row.status === 'Submitted'">Waiting for Pre-Approver</span>
                  <span v-else-if="props.row.purchaseExpenseStatus === 'Pre-Approved'">Waiting for Approver</span>
                  <span v-else>{{ props.row.purchaseExpenseStatus }}</span>
                </q-badge>
                <!-- <q-badge :style="{ backgroundColor: props.row.bgColor, color:props.row.color }">{{ props.row.purchaseExpenseStatus }}</q-badge> -->
              </q-td>
              <q-td style="text-align: right;width: 0%;">{{ props.row.quantity }}</q-td>
              <q-td style="text-align: right;width: 0%;">{{ props.row.estimatedRate }}</q-td>
              <q-td style="text-align: right;width: 0%;">{{ props.row.discount }}</q-td>
              <q-td style="text-align: right;width: 0%;">{{ props.row.estimatedAmount }}</q-td>
              <q-td style="width: 0%;" class="actions text-center">
                <div>
                  <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" @click="onView(props.row)">
                    <q-tooltip>View</q-tooltip>
                  </q-icon>
                  <q-icon v-if="(roles.includes('finance-approver') || roles.includes('finance-preapprove')|| roles.includes('finance-paidby') || role) && props.row.purchaseExpenseStatus != 'Paid' && props.row.purchaseExpenseStatus != 'Declined' && props.row.purchaseExpenseStatus != 'Cancelled'" name="o_double_arrow" class="cursor-pointer q-mr-sm" @click="approvePopup(props.row)">
                    <q-tooltip>
                      {{ props.row.purchaseExpenseStatus === 'Approved' ? 'View Details' : 'Approve/Decline Expense' }}
                    </q-tooltip>
                  </q-icon>
                  <q-icon v-if="(roles.includes('finance-approver') || roles.includes('finance-preapprove') || role) && props.row.purchaseExpenseStatus == 'Request For Cancellation'" name="o_cancel" class="cursor-pointer q-mr-sm" @click="onForwardToApprover(props.row, 'Cancelled')">
                    <q-tooltip>Cancel Expense Request</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_history"
                    class="cursor-pointer"
                    size="xs"
                    @click.stop="onStatusLog(props.row.id, props.row.referenceId, 'Purchase Expense Status')"
                  >
                    <q-tooltip>Data Change Log</q-tooltip>
                  </q-icon>
                </div>
              </q-td>
            </q-tr>
            <q-tr v-if="props.pageIndex === paginatedRows.length - 1 || props.pageIndex === rows.length - 1">
              <q-td colspan="9" class="text-right font-bold"><b>Total Amount:</b></q-td>
              <q-td class="text-right"><b>{{ totalAmountForPage }}</b></q-td>
              <q-td colspan="3" />
            </q-tr><q-separator />
          </template>
        </q-table>
      </div>
    </q-card>
  </q-page>
</template>

<script setup>
import { ref, onMounted, computed, watch } from "vue";
import { useQuasar } from "quasar";
import { notifySuccess, setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";
import { useAuthStore } from "stores/auth";

import viewPurchaseExpenses from "modules/finance-expense-purchase-request/components/view.vue";
import purchaseExpensesService from "../financeExpensePurchaseRequest.service";
import commonService from "services/common.service";
import siteStatusLog from "modules/sites/components/_siteModifiedLogs.vue";
import employeesService from "modules/employee/employee.service";
import ApproverNoteDialog from "modules/finance-expense/components/approveNote.vue";

const authStore = useAuthStore();
const user = authStore.user;
const roles = user?.roles || [];
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";
const $q = useQuasar();
const loading = ref(true);
const rows = ref([]);
const showFilter = ref(false);
const searchLoader = ref(false);

const localStorageKey = "Approve Purchase Expense";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const referenceId = filterLocalStorage ? filterLocalStorage.referenceId : [];
const statusId = filterLocalStorage ? filterLocalStorage.statusId : [];
const employeeId = filterLocalStorage ? filterLocalStorage.employeeId : "";

const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const search = ref({
  searchText,
  referenceId,
  statusId,
  statusName: "Draft",
  employeeId
});

const activeRowId = ref(null);
const columns = ref([
  { name: "referenceId", label: "ReferenceId", field: "referenceId", align: "left", sortable: true },
  { name: "requestDate", label: "Request Date", field: "requestDate", align: "center", sortable: true },
  { name: "itemCategoryId", label: "Item Category", field: "itemCategoryId", align: "left", sortable: true },
  { name: "itemName", label: "Item Name", field: "itemName", align: "left", sortable: true },
  { name: "requestedById", label: "Requested By", field: "requestedById", align: "left", sortable: true },
  { name: "statusId", label: "Status", field: "statusId", align: "left", sortable: true },
  { name: "quantity", label: "QTY.", field: "quantity", align: "right", sortable: false },
  { name: "estimatedRate", label: "Est. Rate", field: "estimatedRate", align: "right", sortable: false },
  { name: "discount", label: "Discount", field: "discount", align: "right", sortable: false },
  { name: "estimatedAmount", label: "Est. Amt.", field: "estimatedAmount", align: "right", sortable: false }
]);

const getAllApprovePurchaseExpenseRequests = async (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;

  try {
    const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
    setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });

    // Await the service call so loader works correctly
    const resp = await purchaseExpensesService.getAllApprovePurchaseExpenseRequests(payload);

    rows.value = resp.data.map((purchaseExpense, index) => ({
      index: index + 1,
      id: purchaseExpense.id,
      referenceId: purchaseExpense.referenceId,
      requestDate: purchaseExpense.requestDate,
      itemName: purchaseExpense.itemName,
      itemCategoryId: purchaseExpense.itemCategoryId,
      quantity: purchaseExpense.quantity,
      requestedBy: purchaseExpense.requestedBy,
      estimatedRate: purchaseExpense.estimatedRate,
      discount: purchaseExpense.discount,
      estimatedAmount: purchaseExpense.estimatedAmount,
      statusId: purchaseExpense.statusId,
      requestedEmployee: purchaseExpense.requestedEmployee.person.fullName,
      purchaseExpenseStatus: purchaseExpense.purchaseRequestStatus.dropDownValue,
      itemCategory: purchaseExpense.itemCategory.type,
      bgColor: purchaseExpense.purchaseRequestStatus.bgColor,
      color: purchaseExpense.purchaseRequestStatus.color,
      isEdited: purchaseExpense.isEdited
    }));

    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  } catch (error) {
    $q.notify({ color: "negative", message: "Failed to fetch data." });
    console.error(error);
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getAllApprovePurchaseExpenseRequests(propps);
};

// Clear search
const onClear = () => {
  search.value.referenceId = [];
  search.value.statusId = [];
  search.value.employeeId = "";
  clearLocalStorage(localStorageKey);
  onSearch();
};

// Toatal Amount  calculation
const paginatedRows = computed(() => {
  if (pagination.value.rowsPerPage === 0) {
    // If rowsPerPage is 0, show all rows
    return rows.value;
  }
  const start = (pagination.value.page - 1) * pagination.value.rowsPerPage;
  const end = pagination.value.page * pagination.value.rowsPerPage;
  return rows.value.slice(start, end);
});
const totalAmountForPage = computed(() => {
  return paginatedRows.value
    .reduce((sum, row) => sum + (parseFloat(row.estimatedAmount) || 0), 0);
});

const onView = (row) => {
  activeRowId.value = row.id;
  $q.dialog({
    component: viewPurchaseExpenses,
    componentProps: { id: row.id, isShowActions: false }
  }).onOk(() => {
    getAllApprovePurchaseExpenseRequests({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

const onStatusLog = (id, name, columnName) => {
  activeRowId.value = id;
  $q.dialog({
    component: siteStatusLog,
    componentProps: { id, name, columnName }
  }).onOk(() => {
    getAllApprovePurchaseExpenseRequests({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

const approvePopup = (row) => {
  activeRowId.value = row.id;
  $q.dialog({
    component: viewPurchaseExpenses,
    componentProps: { id: row.id, isShowActions: true }
  }).onOk(() => {
    getAllApprovePurchaseExpenseRequests({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

const onForwardToApprover = (row, approver) => {
  activeRowId.value = row.id;
  row.approver = approver;
  const payload = {
    id: row.id,
    approver
  };
  if (approver === "Cancelled") {
    $q.dialog({
      component: ApproverNoteDialog,
      componentProps: {
        title: "<span class=\"text-primary\">Confirm</span>",
        message: "Are you sure you want to cancel ?",
        approver
      }
    })
      .onOk((note) => {
        payload.preApproverNote = note;
        loading.value = true;
        purchaseExpensesService.forwardPurchaseExpenseToApprovers(payload)
          .then(() => {
            notifySuccess({
              message: "Request Cancelled Successfully!"
            });
            loading.value = false;
            $q.loading.hide();
            activeRowId.value = null;
            getAllApprovePurchaseExpenseRequests({ pagination: pagination.value });
          })
          .catch(() => {
            loading.value = false;
            $q.loading.hide();
            activeRowId.value = null;
            $q.notify({
              type: "negative",
              message: "Failed to update expense."
            });
          });
      })
      .onCancel(() => {
        activeRowId.value = null;
      });
  }
};

// ----------------------------
// Applied Filter Labels.
// ----------------------------
const mapFilterToLabel = (ids, list, label) => {
  if (!Array.isArray(ids) || !ids.length) return {};

  const text = ids
    .map(id => {
      const match = list.value.find(item => item.value === id);
      return match ? match.text : id;
    })
    .join(", ");

  return { [label]: text };
};

const mapFilterToSingleLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.referenceId, purchaseExpenseRequestList, "Purchase Expense Request"),
  ...mapFilterToLabel(search.value.statusId, purchaseExpenseStatusList, "Status"),
  ...mapFilterToSingleLabel(search.value.employeeId, employeeList, "Requested By")
}));

function getFilterCount (key) {
  switch (key) {
  case "Purchase Expense Request": return search.value.referenceId?.length || 0;
  case "Status": return search.value.statusId?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Requested By") {
    search.value.employeeId = "";
  } else if (key === "Purchase Expense Request") {
    search.value.referenceId = [];
  } else if (key === "Status") {
    search.value.statusId = [];
  }
  delete appliedFilters.value[key];
  getAllApprovePurchaseExpenseRequests({ pagination: pagination.value });
}

const purchaseExpenseRequestList = ref([]);
const purchaseExpenseRequestListFilter = ref([]);
function getAllPurchaseExpenseRequestDropDownList () {
  purchaseExpensesService.getPurchaseExpenseRequestList().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.text, value: item.value })).sort((a, b) => a.text.localeCompare(b.text));
    purchaseExpenseRequestList.value = responseData;
    purchaseExpenseRequestListFilter.value = responseData;
  });
}
function getAllPurchaseExpenseRequestDropDownListFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      purchaseExpenseRequestList.value = purchaseExpenseRequestListFilter.value;
    } else {
      purchaseExpenseRequestList.value = purchaseExpenseRequestListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const purchaseExpenseStatusList = ref([]);
const purchaseExpenseStatusListFilter = ref([]);
function getAllPurchaseExpenseStatusDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    purchaseExpenseStatusList.value = responseData;
    purchaseExpenseStatusListFilter.value = responseData;
  });
}
function getAllPurchaseExpenseStatusDropDownFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      purchaseExpenseStatusList.value = purchaseExpenseStatusListFilter.value;
    } else {
      purchaseExpenseStatusList.value = purchaseExpenseStatusListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all employee list for dropdown
const employeeList = ref([]);
const employeeFilter = ref([]);
function getAllEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    employeeList.value = responseData;
    employeeFilter.value = responseData;
  });
}

// Search employee for dropdown
function getAllEmployeesListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = employeeFilter.value;
    } else {
      employeeList.value = employeeFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getAllApprovePurchaseExpenseRequests({ pagination: pagination.value });
});

onMounted(() => {
  getAllApprovePurchaseExpenseRequests({ pagination: pagination.value });
  getAllPurchaseExpenseRequestDropDownList();
  getAllPurchaseExpenseStatusDropDown("Purchase Expense Status");
  getAllEmployeesListForDropdown();
});
</script>
