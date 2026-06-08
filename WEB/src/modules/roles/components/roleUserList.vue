<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1300px !important;max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white" style="max-width: 800px; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">Users Assigned to Role</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <div class="row items-center q-ml-lg">
        <div class="col-12 col-md-6">
          <div class="row items-center">
            <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
            <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
              {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
            </q-chip>
          </div>
        </div>
        <div class="col-12 col-md-6">
          <div class="row items-center justify-end no-wrap q-ma-md">
            <div class="row items-center" style="flex-wrap: nowrap;">
              <div class="search-container" style="position: relative; display: flex; align-items: center; width: 320px;">
                <q-input v-model="searchText" :loading="searchLoader" outlined dense clearable debounce="300" placeholder="Search" class="bg-white search-box" style="flex: 1; border-top-right-radius: 0; border-bottom-right-radius: 0; border-top-right-radius: 0; max-width: 250px;">
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
                <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                  <q-card class="q-pa-sm">
                    <div class="row items-center q-mb-sm">
                      <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                        <label class="Cutomlabel">User Status</label>
                      </div>
                      <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                        <q-select
                          v-model="search.userStatus"
                          class="q-mx-sm w-100 h-auto"
                          stack-label
                          hide-bottom-space
                          use-input
                          :dense="true"
                          :options="userStatusList"
                          emit-value
                          map-options
                          :popup-content-class="customPopupContentClass"
                        />
                      </div>
                    </div>
                    <div class="row items-center q-mb-sm">
                      <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                        <label class="Cutomlabel">Full Name</label>
                      </div>
                      <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                        <q-input
                          v-model="search.fullName"
                          push class="q-mx-sm w-100 h-auto"
                          hide-bottom-space
                          :dense="true"
                        />

                      </div>
                    </div>
                    <div class="row items-center q-mb-sm">
                      <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                        <label class="Cutomlabel q-mt-sm fs-13">Email</label>
                      </div>
                      <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                        <q-input
                          v-model="search.email"
                          push
                          class="q-mx-sm w-100 h-auto"
                          hide-bottom-space
                          :dense="true"
                          type="email"
                        />
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
      <q-separator />
      <div>
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <q-table
              ref="tableRef" v-model:pagination="pagination" bordered class="Custom-DataTable" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
              no-data-label="No data available" :filter="filter" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getUsers"
            >
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                  <q-th auto-width class="text-center">Actions</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="activeRowId == props.row.id">
                  <q-td>
                    {{ props.row.username }}
                  </q-td>
                  <q-td style="max-width: 350px; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                    {{ props.row.person.fullName }}
                  </q-td>
                  <q-td>{{ props.row.email }}</q-td>
                  <q-td>{{ props.row.phoneNumber }}</q-td>
                  <q-td class="text-center actions">
                    <q-icon
                      :name="props.row.active ? 'o_check_circle' : 'o_block'"
                      :color="props.row.active ? 'positive' : 'negative'"
                      class="q-mr-xs hoverable-cell"
                      @click="toggleActiveStatus(props.row)"
                    >
                      <q-tooltip>{{ props.row.active ? 'Set Inactive?' : 'Set Active?' }}</q-tooltip>
                    </q-icon>
                  </q-td>
                </q-tr>
              </template>
            </q-table>
          </div>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted, watch, computed } from "vue";
import { notifySuccess, setLocalStorage, getLocalStorage, clearLocalStorage } from "assets/utils";

import usersService from "modules/user-management/userManagement.service";

// ----------------------------
// Common variables
// ----------------------------
const loading = ref(true);
const rows = ref([]);
const showFilter = ref(false);
const searchLoader = ref(false);
const userStatusList = ref(["Active", "Inactive"]);

// ----------------------------
// Props values i.e. come from query string
// ----------------------------
const props = defineProps({ id: { type: String, default: "" } });

const localStorageKey = `Users-${props.id}`;
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const userStatus = filterLocalStorage ? filterLocalStorage.userStatus : "Active";
const fullName = filterLocalStorage ? filterLocalStorage.fullName : "";
const email = filterLocalStorage ? filterLocalStorage.email : "";
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// ----------------------------
// Search variables
// ----------------------------
const search = ref({
  searchText,
  userStatus,
  fullName,
  email
});

// ----------------------------
// Define emits
// ----------------------------
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

const columns = ref([
  { name: "username", label: "User Name", field: "username", align: "left", sortable: true },
  { name: "person.fullName", label: "Full Name", field: "person.fullName", align: "left", sortable: true },
  { name: "email", label: "Email", field: "email", align: "left", sortable: true },
  { name: "phoneNumber", label: "Phone Number", field: "phoneNumber", align: "left", sortable: false }
]);

const getUsers = (params) => {
  const { page, rowsPerPage, sortBy, descending } = params.pagination;
  loading.value = true;
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    searchText: searchText.value || null,
    userStatus: search.value.userStatus,
    fullName: search.value.fullName,
    email: search.value.email,
    siteRoleIds: [props.id]
  };
  usersService.getUsers(payload).then((resp) => {
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

// ----------------------------
// Toggle Active/Inactive Status
// ----------------------------
const toggleActiveStatus = (row) => {
  const isCurrentlyActive = row.active === true;
  const newStatus = !isCurrentlyActive; // Toggle the status
  usersService.updateUserStatus(row.id, newStatus).then(() => {
    notifySuccess({ message: "User status saved successfully." });
    row.active = newStatus;
    getUsers({ pagination: pagination.value });
  });
};

// ----------------------------
// Applied Filter Labels.
// ----------------------------

const mapSingleFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapSingleFilterToLabel(search.value.userStatus, userStatusList, "User Status"),
  ...(search.value.fullName ? { "Full Name": search.value.fullName } : {}),
  ...(search.value.email ? { Email: search.value.email } : {})
}));

const onSearch = () => {
  const props = { pagination: pagination.value };
  getUsers(props);
};

// ----------------------------
// Clear search
// ----------------------------
const onClear = () => {
  search.value.userStatus = "Active";
  search.value.fullName = "";
  search.value.email = "";
  clearLocalStorage(localStorageKey);
  onSearch();
};

function onClearFilters (key) {
  if (key === "User Status") {
    search.value.userStatus = null;
  } else if (key === "Full Name") {
    search.value.fullName = "";
  } else if (key === "Email") {
    search.value.email = "";
  }
  delete appliedFilters.value[key];
  getUsers({ pagination: pagination.value });
}

// ----------------------------
// On page rendering
// ----------------------------
onMounted(() => {
  getUsers({ pagination: pagination.value });
});

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getUsers({ pagination: pagination.value });
});

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
