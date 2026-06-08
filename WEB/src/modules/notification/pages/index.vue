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
              <q-breadcrumbs-el label="Notifications" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-4">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <!-- <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge> -->
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-md-5">
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
                  <!-- Dropdown Content -->
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Start Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.startDate" fill-input dense mask="##/##/####">
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.startDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">End Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.endDate" fill-input dense mask="##/##/####">
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="search.endDate" mask="MM/DD/YYYY" :options="disableBeforeStartDate" @update:model-value="() => $refs.qDateProxy.hide()" />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
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
              <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-xs" @click="$router.push(storedUser.siteLandingPageLink)" />
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-separator />
      <q-table
        ref="tableRef" v-model:pagination="pagination" :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
        no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getNotifications"
      >
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <!-- Ref to first column - Added Date Column and set to auto-width -->
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <!-- Ref to first column and it's value - Added Date Manually -->
            <q-td width="10%">{{ props.row.createdOnUtc }}</q-td>
            <q-td width="30%" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.title }}</q-td>
            <q-td width="40%" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.message }}</q-td>
            <q-td width="5%" class="text-center actions">
              <q-icon name="o_double_arrow" class="cursor-pointer q-mr-sm" @click="handleClick(props.row.redirectURL, props.row.id, props.row.recordId)">
                <q-tooltip>Redirect</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
          <q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted, computed, watch } from "vue";
// import { useQuasar } from "quasar";
import notificationsService from "modules/notification/notifications.service";
import { setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";
import { parse } from "date-fns"; // Standard TimeZone Conversion

// Common variables
// const $q = useQuasar();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const storedUser = getLocalStorage("user");

// local storage values
const localStorageKey = "Notifications";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const startDate = filterLocalStorage ? filterLocalStorage.startDate : "";
const endDate = filterLocalStorage ? filterLocalStorage.endDate : "";
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Search variables
const search = ref({
  searchText,
  startDate,
  endDate
});

// open advance search
// const hasActiveFilters = (search) => {
//   return (
//     (search.startDate) ||
//     (search.endDate)
//   );
// };
// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const columns = ref([
  { name: "createdOnUtc", label: "Date", field: "createdOnUtc", align: "left", sortable: true },
  { name: "title", label: "Title", field: "title", align: "left", sortable: true },
  { name: "message", label: "Message", field: "message", align: "left", sortable: true }
]);



function isValidDate (value) {
  const date = new Date(value);
  return !isNaN(date.getTime());
}

// Get/Map department list to table
const getNotifications = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  search.value.startDate = isValidDate(search.value.startDate) ? search.value.startDate : null;
  search.value.endDate = isValidDate(search.value.endDate) ? search.value.endDate : null;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  notificationsService.getNotifications(payload).then((resp) => {
    rows.value = resp.data;
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

const disableBeforeStartDate = (startDate) => {
  if (!search.value.startDate) {
    return true;
  }

  // Convert MM/dd/yyyy string to Date
  const start = parse(search.value.startDate, "MM/dd/yyyy", new Date());
  const currentDate = parse(startDate, "yyyy/MM/dd", new Date());

  return currentDate >= start;
};

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getNotifications(propps);
};

// Clear search
const onClear = () => {
  search.value.startDate = null;
  search.value.endDate = null;
  clearLocalStorage(localStorageKey);
  onSearch();
};

const appliedFilters = computed(() => ({
  ...(search.value.startDate ? { "Start Date": search.value.startDate } : {}),
  ...(search.value.endDate ? { "End Date": search.value.endDate } : {})
}));

function onClearFilters (key) {
  if (key === "Start Date") {
    search.value.startDate = "";
  } else if (key === "End Date") {
    search.value.endDate = "";
  }
  delete appliedFilters.value[key];
  getNotifications({ pagination: pagination.value });
}
// function redirectToURL (url) {
//   if (url) {
//     window.location.href = url; // Redirect to the provided URL
//   } else {
//     console.warn("No URL provided for redirection");
//   }
// }
async function handleClick (url, notificationId, recordId) {
  if (notificationId) {
    // Then, fetch the notification list
    await notificationsService.getNotificationList(notificationId, "RN");
  }
  // First, handle the redirection
  if (url) {
    window.location.href = url; // Redirect to the provided URL
  } else {
    console.warn("No URL provided for redirection");
  }
}
// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getNotifications({ pagination: pagination.value });
});

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
});
</script>
