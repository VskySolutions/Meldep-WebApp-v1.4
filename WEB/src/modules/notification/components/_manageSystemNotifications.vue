<template>
  <q-card class="project6">
    <q-card-section class="card-header with-tools">
      <div class="row items-center">
        <div class="col-12 col-xs-2 col-sm-2 col-md-2 col-lg-2 col-xl-2">
          <q-breadcrumbs class="text-brown text-weight-bold text-h3">
            <template #separator>
              <q-icon size="1.5em" name="o_chevron_right" color="primary" />
            </template>
            <q-breadcrumbs-el label="System Notifications" />
          </q-breadcrumbs>
        </div>
        <div class="col-12 col-xs-4 col-sm-3 col-md-3 col-lg-3 col-xl-3">
          <div class="row items-center">
            <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
            <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
              {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
            </q-chip>
          </div>
        </div>
        <div class="col-12 col-xs-6 col-sm-7 col-md-7 col-lg-7 col-xl-7">
          <div class="row items-center justify-end no-wrap">
            <q-select
              v-if="role === 'admin'"
              v-model="selectedUserId"
              clearable
              use-input
              fill-input
              hide-selected
              outlined
              dense
              :options="userNameList"
              option-value="value"
              option-label="text"
              placeholder="Select User"
              emit-value
              map-options
              style="width:250px;"
              @update:model-value="onChangeEmployeeSystemNotificationList"
              @filter="getAllUserNameListForFilter"
            >
              <q-tooltip>Select User</q-tooltip>
            </q-select>
            <div class="row items-center no-wrap q-ml-sm">
              <span class="Cutomlabel fs-13 text-black">
                Turn On/Off
              </span>
              <!-- Info Icon -->
              <q-icon
                name="o_info"
                size="xs"
                class="q-ml-xs"
              >
                <q-tooltip>Turn all notifications on or off</q-tooltip>
              </q-icon>
              <q-toggle
                v-model="model.setActive"
                color="primary"
                @click="onAllPermissions(model.setActive)"
              />
            </div>
            <div class="row items-center q-mr-xs" style="flex-wrap: nowrap;">
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
                >
                  <template #prepend>
                    <q-icon name="o_search" />
                  </template>
                </q-input>
                <q-btn
                  unelevated
                  :color="Object.keys(appliedFilters).length > 0 ? 'primary' : 'grey-7'"
                  text-color="white"
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
                  <q-tooltip anchor="bottom middle" self="top middle">Advanced Filter</q-tooltip>
                </q-btn>
                <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                  <q-card class="q-pa-sm">
                    <div class="row items-center q-mb-sm">
                      <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                        <label class="Cutomlabel q-mt-sm fs-13">Type</label>
                      </div>
                      <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                        <div>
                          <q-input
                            v-model="search.type"
                            fill-input
                            :dense="true"
                            class="q-mx-sm"
                          />
                        </div>
                      </div>
                    </div>
                    <div class="row items-center q-mb-sm">
                      <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                        <label class="Cutomlabel q-mt-sm fs-13">Title</label>
                      </div>
                      <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                        <div>
                          <q-input
                            v-model="search.title"
                            fill-input
                            :dense="true"
                            class="q-mx-sm"
                          />
                        </div>
                      </div>
                    </div>
                    <div class="row items-center q-mb-sm">
                      <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                        <label class="Cutomlabel q-mt-sm fs-13">Message</label>
                      </div>
                      <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                        <div>
                          <q-input
                            v-model="search.message"
                            fill-input
                            :dense="true"
                            class="q-mx-sm"
                          />
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
            <q-btn
              icon="o_chevron_left"
              outline
              label="Back"
              no-caps
              class="text-primary btnRounded q-mr-md no-space-between"
              @click="$router.push(storedUser.siteLandingPageLink)"
            />
          </div>
        </div>
      </div>
    </q-card-section>
    <q-separator />
    <q-separator />
    <q-table
      ref="tableRef"
      v-model:pagination="pagination"
      :class="systemNotificationRows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
      :loading="loading"
      :rows="systemNotificationRows"
      :columns="systemNotificationColumns"
      row-key="id"
      separator="cell"
      no-data-label="No data available"
      binary-state-sort
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      @request="getAllSystemNotifications"
    >
      <template #loading>
        <q-inner-loading showing color="primary">
          <q-spinner-ios size="40px" class="q-mt-xl" />
        </q-inner-loading>
      </template>
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
          <q-th auto-width class="text-center">On/Off</q-th>
        </q-tr>
      </template>
      <template #body="props">
        <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
          <q-td width="10%">
            {{ props.row.notification.type }}
          </q-td>
          <q-td width="40%">
            {{ props.row.notification.title }}
          </q-td>
          <q-td width="40%">
            {{ props.row.notification.message }}
          </q-td>
          <q-td width="40%" class="text-center actions">
            <q-toggle
              v-model="props.row.active"
              color="primary"
              @click="onPermission(props.row.id, props.row.active)"
            />
          </q-td>
        </q-tr><q-separator />
      </template>
    </q-table>
  </q-card>
</template>

<script setup>
// Import libraries
import { ref, onMounted, computed, watch } from "vue";
import { notifySuccess, setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";
import { useAuthStore } from "stores/auth";
import notificationsService from "modules/notification/notifications.service";
import usersService from "modules/user-management/userManagement.service";

// Common variables
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const selectedUserId = ref(null);
const authStore = useAuthStore();
const user = authStore.user;
const siteId = user?.siteId;
const role = user?.roles?.includes("admin") ? "admin" : "";

const model = ref({ setActive: true });

// local storage values
const localStorageKey = "manageSystemNotifications";
const filterLocalStorage = getLocalStorage(localStorageKey);
const storedUser = getLocalStorage("user");
const searchText = ref(filterLocalStorage?.searchText || "");
const title = filterLocalStorage ? filterLocalStorage.title : "";
const type = filterLocalStorage ? filterLocalStorage.type : "";
const message = filterLocalStorage ? filterLocalStorage.message : "";
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Search variables
const search = ref({
  searchText,
  title,
  type,
  message
});

// Table variables
const tableRef = ref();
const systemNotificationRows = ref([]);
const activeRowId = ref(null);
const systemNotificationColumns = ref([
  { name: "notification.type", label: "Type", field: "notification.type", align: "left", sortable: true },
  { name: "notification.title", label: "Title", field: "notification.title", align: "left", sortable: true },
  { name: "notification.message", label: "Message", field: "notification.message", align: "left", sortable: true }
]);

// Get/Map system notification list
const getAllSystemNotifications = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, userId: selectedUserId.value, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  notificationsService.getAllNotificationPermissions(payload).then((resp) => {
    systemNotificationRows.value = resp.data;
    // Check if all rows have active === false
    const allInactive = systemNotificationRows.value.every(row => !row.active);
    model.value.setActive = !allInactive;
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

// on or off notification permission
function onPermission (id, activeStatus) {
  setTimeout(function () {
    const status = activeStatus ? "on" : "off";
    notificationsService.updateNotificationPerssion(id, activeStatus, selectedUserId.value).then(resp => {
      notifySuccess({ message: `System Notification ${status} successfully.` });
      getAllSystemNotifications({ pagination: pagination.value });
    });
  });
}

// on or off all notification permissions
function onAllPermissions (setActive) {
  setTimeout(function () {
    const status = setActive ? "on" : "off";
    notificationsService.updateAllPermissions(setActive, selectedUserId.value).then(resp => {
      notifySuccess({ message: `All System Notifications ${status} successfully.` });
      getAllSystemNotifications({ pagination: pagination.value });
    });
  });
}

function onChangeEmployeeSystemNotificationList () {
  getAllSystemNotifications({ pagination: pagination.value });
}

// ---------------------------------------------------------------------
// DROPDOWN LISTS
// ---------------------------------------------------------------------

// Get all user username for dropdown
const userNameList = ref([]);
const userNameFilter = ref([]);
function getAllUserListForDropdown (siteId) {
  usersService.getAllUserListForDropdown(siteId, "US").then((resp) => {
    const responseData = resp
      .map((item) => ({ text: `${item.person.firstName} ${item.person.lastName}`, value: item.id }))
      .sort((a, b) => a.text.localeCompare(b.text));
    userNameList.value = responseData;
    userNameFilter.value = responseData;
  });
}

function getAllUserNameListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      userNameList.value = userNameFilter.value;
    } else {
      userNameList.value = userNameFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getAllSystemNotifications(propps);
};

// Clear search
const onClear = () => {
  search.value.title = null;
  search.value.type = null;
  search.value.message = null;
  clearLocalStorage(localStorageKey);
  onSearch();
};

// appliedFilters
const appliedFilters = computed(() => ({
  ...(search.value.type ? { Type: search.value.type } : {}),
  ...(search.value.title ? { Title: search.value.title } : {}),
  ...(search.value.message ? { Message: search.value.message } : {})
}));

// clear filters
function onClearFilters (key) {
  if (key === "Type") {
    search.value.type = "";
  } else if (key === "Title") {
    search.value.title = null;
  } else if (key === "Message") {
    search.value.message = "";
  }
  delete appliedFilters.value[key];
  getAllSystemNotifications({ pagination: pagination.value });
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getAllSystemNotifications({ pagination: pagination.value });
});

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
  getAllUserListForDropdown(siteId);
});
</script>
