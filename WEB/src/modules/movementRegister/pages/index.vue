<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-xs-2 col-sm-1 col-md-2 col-lg-3 col-xl-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Movement Register List" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-xs-3 col-sm-2 col-md-2 col-lg-2 col-xl-3">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-xs-7 col-sm-9 col-md-8 col-lg-7 col-xl-7">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center" style="flex-wrap: nowrap;">
                <div class="search-container" style="position: relative; display: flex; align-items: center; width: 320px;">
                  <q-input
                    v-model="searchText"
                    :loading="searchLoader"
                    outlined
                    dense
                    clearable
                    debounce="300"
                    placeholder="Search"
                    class="bg-white search-box"
                    style="flex: 1; border-top-right-radius: 0; border-bottom-right-radius: 0; border-top-right-radius: 0; max-width: 250px;"
                  >
                    <template #prepend>
                      <q-icon name="o_search" />
                    </template>
                  </q-input>
                  <q-btn
                    unelevated :color="Object.keys(appliedFilters).length > 0 ? 'primary' : 'grey-7'" text-color="white" size="" class="q-pa-xs q-mr-xs filter-btn" style="height: 40px; border-top-left-radius: 0; border-bottom-left-radius: 0;"
                    @click.stop="showFilter = !showFilter"
                  >
                    <q-badge v-if="Object.keys(appliedFilters).length > 0" color="green" floating>{{ Object.keys(appliedFilters).length }}</q-badge>
                    <q-icon name="o_filter_alt" size="sm" color="white" class="q-mr-xs" />
                    <q-item-label class="text-xs fs-12">
                      <span class="block">Set/Clear</span>
                      <span class="block">FILTER</span>
                    </q-item-label>
                    <q-tooltip anchor="bottom middle" self="top middle">Advanced Filter</q-tooltip>
                  </q-btn>
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Created By</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.createdBy"
                            class="q-mx-sm w-100 h-auto"
                            stack-label
                            hide-bottom-space use-input
                            :dense="true"
                            :options="createdByList"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Employee Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.employeeId"
                            class="q-ml-sm"
                            use-input
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            :options="employeeList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                            :disable="search.createdBy === 'Created By Me'"
                            @filter="getAllEmployeesListForDropdownFilter"
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
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Type</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.typeId"
                            class="q-ml-sm"
                            use-input
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            :options="movementRegisterTypeList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                            @filter="getAllMovementRegisterTypeListForFilter"
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
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">From Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.fromDate" fill-input dense>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.fromDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">To Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.toDate" fill-input dense>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.toDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn
                          style="width: 20%;"
                          outline color="primary"
                          label="Search"
                          class="btnRounded"
                          no-caps
                          @click="() => { showFilter = false; onSearch(); }"
                        />
                        <q-btn
                          style="width: 20%;"
                          outline
                          color="grey-4"
                          label="Clear"
                          class="text-grey-9 btnRounded"
                          no-caps
                          @click="onClear"
                        />
                        <q-btn
                          style="width: 20%;"
                          outline color="negative"
                          label="Close"
                          class="btnRounded"
                          no-caps
                          @click="() => { showFilter = false; }"
                        />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
              <div>
                <q-btn
                  icon="o_chevron_left"
                  outline
                  label="Back"
                  no-caps
                  class="text-primary btnRounded no-space-between q-ml-sm"
                  @click="$router.back()"
                />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-table
        ref="tableRef"
        v-model:pagination="pagination"
        :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
        :loading="loading"
        :rows="rows"
        :columns="columns"
        row-key="id"
        separator="cell"
        no-data-label="No data available"
        binary-state-sort
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        @request="getAllMovementRegisters"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center hidden">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td colspan="5" style="background: #dbf2ff;" class="text-center">{{ props.row.date }}</q-td>
          </q-tr>
          <q-tr
            v-for="(line) in props.row.movementRegisterDetails"
            :key="line.id"
          >
            <q-td class="text-left">
              {{ line.employees?.person?.fullName }}
            </q-td>
            <q-td class="text-left">
              <q-chip v-if="line.type.dropDownValue == 'Break'" label="Break" name="o_done" class="rounded q-px-lg" color="indigo-11" text-color="black" />
              <q-chip v-if="line.type.dropDownValue == 'Time Adjustment'" label="Time Adjustment" name="o_done" class="rounded q-px-lg" color="purple-3" text-color="black" />
              <q-chip v-if="line.type.dropDownValue == 'Work From Home'" label="Work From Home" name="o_done" class="rounded q-px-lg" color="cyan-3" text-color="black" />
            </q-td>
            <q-td class="text-left text-wrap" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 30%;">
              {{ line.message }}<span v-if="line.wfhDuration?.dropDownText">
                ({{ line.wfhDuration?.dropDownText }})
              </span>
            </q-td>
            <q-td class="text-left">
              {{ line.approvers?.person?.fullName }}
            </q-td>
            <q-td class="text-right">
              {{ line.timeInMinutes }}
            </q-td>
          </q-tr>
          <q-tr v-if="props.pageIndex === rows.length - 1">
            <q-td colspan="4" class="text-right"><strong>Total Time:</strong></q-td>
            <q-td class="text-right"><strong>{{ calculateTotalTime(rows) }}</strong></q-td>
          </q-tr><q-separator />
          <q-separator />
        </template>
      </q-table></q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import { getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";

import commonService from "src/services/common.service";
import employeesService from "src/modules/employee/employee.service";
import movementRegisterService from "../movementRegister.service";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const createdByList = ref(["Created By Me", "View All"]);

// local storage values
const localStorageKey = "Movement Register";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const createdBy = filterLocalStorage ? filterLocalStorage.createdBy : "Created By Me";
const employeeId = filterLocalStorage ? filterLocalStorage.employeeId : "";
const typeId = filterLocalStorage ? filterLocalStorage.typeId : "";
const fromDate = filterLocalStorage ? filterLocalStorage.fromDate : "";
const toDates = filterLocalStorage ? filterLocalStorage.toDate : "";

// Search variables
const search = ref({
  searchText,
  createdBy,
  employeeId,
  typeId,
  fromDate,
  toDate: toDates
});

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "employeeName", label: "Employee Name", field: row => row.employees?.person?.fullName, align: "center", sortable: true },
  { name: "type", label: "Type", field: row => row.type.dropDownValue, align: "center", sortable: true },
  { name: "message", label: "Message", field: row => row.message, align: "center", sortable: true },
  { name: "approverName", label: "Approver Name", field: row => row.approvers?.person?.fullName, align: "center", sortable: true },
  { name: "timeInMinutes", label: "Time In Minutes", field: row => row.timeInMinutes, align: "center", sortable: true }
]);

// Get/Map MovementRegister list to table
const getAllMovementRegisters = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  search.value.toDate = isValidDate(search.value.toDate) ? search.value.toDate : null;
  search.value.fromDate = isValidDate(search.value.fromDate) ? search.value.fromDate : null;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, search.value);
  movementRegisterService.getAllMovementRegisters(payload).then((resp) => {
    rows.value = resp.moveRegisterList.map(r => ({
      ...r, // keep moveRegisterList fields
      ...(r.movementRegisterDetails) // merge detail fields
    }));
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getAllMovementRegisters(propps);
};

// Clear search
const onClear = () => {
  search.value.createdBy = "Created By Me";
  search.value.employeeId = "";
  search.value.typeId = "";
  search.value.fromDate = null;
  search.value.toDate = null;
  clearLocalStorage(localStorageKey);
  onSearch();
};

// check valid date
const isValidDate = (dateStr) => {
  const regex = /^(0[1-9]|1[0-2])\/(0[1-9]|[12]\d|3[01])\/\d{4}$/;
  if (!regex.test(dateStr)) return false;

  const [mm, dd, yyyy] = dateStr.split("/").map(Number);
  const date = new Date(yyyy, mm - 1, dd);
  return date.getFullYear() === yyyy && date.getMonth() === mm - 1 && date.getDate() === dd;
};

// get all employee list
const employeeList = ref([]);
const employeeListFilter = ref([]);
function getAllEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id }));
    employeeList.value = responseData;
    employeeListFilter.value = responseData;
  });
}

// Search employee for dropdown
function getAllEmployeesListForDropdownFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = employeeListFilter.value;
    } else {
      employeeList.value = employeeListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all issue priority List
const movementRegisterTypeList = ref([]);
const movementRegisterTypeListFilter = ref([]);
function getAllMovementRegisterTypeListForDropdown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    movementRegisterTypeList.value = responseData;
    movementRegisterTypeListFilter.value = responseData;
  });
}
// Search project Module Type List for dropdown
function getAllMovementRegisterTypeListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      movementRegisterTypeList.value = movementRegisterTypeListFilter.value;
    } else {
      movementRegisterTypeList.value = movementRegisterTypeListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

function calculateTotalTime (rows) {
  const total = rows.flatMap(r => r.movementRegisterDetails).reduce((sum, l) => sum + Number(l.timeInMinutes || 0), 0);
  const h = Math.floor(total / 60), m = total % 60;
  return [h ? `${h} hr${h > 1 ? "s" : ""}` : "", m ? `${m} mins` : ""].filter(Boolean).join(" ");
}

// ----------------------------
// Applied Filter Labels.
// ----------------------------

const mapFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.createdBy, createdByList, "Created By"),
  ...mapFilterToLabel(search.value.employeeId, employeeList, "Employee Name"),
  ...mapFilterToLabel(search.value.typeId, movementRegisterTypeList, "Type"),
  ...(search.value.fromDate ? { "From Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "To Date": search.value.toDate } : {})
}));

function onClearFilters (key) {
  if (key === "Created By") {
    search.value.createdBy = "";
  } else if (key === "Employee Name") {
    search.value.employeeId = "";
  } else if (key === "Type") {
    search.value.typeId = "";
  } else if (key === "From Date") {
    search.value.fromDate = "";
  } else if (key === "To Date") {
    search.value.toDate = "";
  }
  delete appliedFilters.value[key];
  getAllMovementRegisters({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  default: return null; // For single-value filters like Year, Status
  }
}

// search for static search
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getAllMovementRegisters({ pagination: pagination.value });
});

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  getAllEmployeesListForDropdown();
  getAllMovementRegisterTypeListForDropdown("Type");
});

</script>
