<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Settings" clickable to="/Settings" />
              <q-breadcrumbs-el label="Sites" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center">
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
                          <label class="Cutomlabel q-mt-sm fs-13">Site Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.name" push class="q-mx-sm w-100 h-auto" hide-bottom-space :dense="true" type="text" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Person Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.fullName" push class="q-mx-sm w-100 h-auto" hide-bottom-space :dense="true" type="text" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Email</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.emailAddress" push class="q-mx-sm w-100 h-auto" hide-bottom-space :dense="true" type="email" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel">Site Status</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.siteStatus" clearable class="q-mx-sm w-100 h-auto" stack-label hide-bottom-space use-input :dense="true"
                            :options="siteStatusList" emit-value map-options :popup-content-class="customPopupContentClass"
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
              <div class="q-ml-sm">
                <q-btn icon="o_add" outline label="Add Site" no-caps class="text-primary btnRounded" @click="onAdd" />
                <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-md" @click="$router.push('/Settings')" />
              </div>
            </div>
          </div>
        </div></q-card-section>
      <q-separator />
      <q-table
        ref="tableRef" v-model:pagination="pagination" :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
        no-data-label="No data available" binary-state-sort :rows-per-page-options="[15, 30, 50 ,100]" @request="getSites"
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
            <q-td>{{ props.row.name }}</q-td>
            <q-td>{{ props.row.person.fullName }}</q-td>
            <q-td>{{ props.row.person.primaryPhoneNumber }}</q-td>
            <q-td>{{ props.row.person.primaryEmailAddress }}</q-td>
            <q-td class="text-center">
              <q-icon v-if="props.row.active" name="o_check_circle_outline" size="sm" color="positive" />
              <q-icon v-else name="o_cancel" size="sm" color="negative" />
            </q-td>
            <q-td auto-width class="text-left actions">
              <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" @click="onView(props.row.id)">
                <q-tooltip>View</q-tooltip>
              </q-icon>
              <q-icon name="o_edit" class="cursor-pointer q-mr-sm" size="xs" @click="onEdit(props.row.id)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_playlist_add_check" class="cursor-pointer q-mr-sm" size="xs" @click="generateDropdown(props.row.id)">
                <q-tooltip>Generate Dropdown</q-tooltip>
              </q-icon>
              <q-icon name="o_notification_add" class="cursor-pointer q-mr-sm" size="xs" @click="generateMasterNotifications(props.row.id)">
                <q-tooltip>Generate Master Notifications</q-tooltip>
              </q-icon>
              <q-icon v-if="props.row.isDropdownGenerated" name="o_manage_accounts" class="cursor-pointer q-mr-sm" size="xs" @click="$router.push({path: 'sites/site-menu-role-permissions', state: { siteId: props.row.id } } )">
                <q-tooltip>Menu Permissions</q-tooltip>
                <q-tooltip>{{ props.row.isDropdownGenerated ? '' : 'Please Generate Dropdown First' }}</q-tooltip>
              </q-icon>
              <q-icon v-else name="o_manage_accounts" class="text-grey-5 cursor-not-allowed q-mr-sm" size="xs">
                <q-tooltip>Please Generate Dropdown First</q-tooltip>
              </q-icon>
              <q-icon v-if="props.row.isDropdownGenerated" name="o_person" class="cursor-pointer q-mr-sm" size="xs" @click="$router.push({ path: 'sites/site-users', state: { siteId: props.row.id } })">
                <q-tooltip>Users</q-tooltip>
              </q-icon>
              <q-icon v-else name="o_person" class="text-grey-5 cursor-not-allowed q-mr-sm" size="xs">
                <q-tooltip>Please Generate Dropdown First</q-tooltip>
              </q-icon>
              <q-icon v-if="props.row.isDropdownGenerated" name="o_supervisor_account" class="cursor-pointer q-mr-sm" size="xs" @click="$router.push({ path: '/sites-sharing', state: { siteId: props.row.id } })">
                <q-tooltip>Share My Tenant</q-tooltip>
              </q-icon>
              <q-icon v-else name="o_supervisor_account" class="text-grey-5 cursor-not-allowed q-mr-sm" size="xs">
                <q-tooltip>Please Generate Dropdown First</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onDelete(props.row)">
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr><q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onMounted, watch, computed } from "vue";
import { useQuasar } from "quasar";
import EditOrganization from "modules/sites/components/addEdit.vue";
import viewOrganization from "modules/sites/components/view.vue";
import siteService from "modules/sites/site.service";
import newPassword from "modules/sites/components/newPassword.vue";
import { zwConfirmDelete, notifySuccess, getLocalStorage, setLocalStorage, clearLocalStorage } from "assets/utils";

// Common variables
const $q = useQuasar();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
// const advanceSearchEnable = ref(true);
// const advanceSearch = () => { advanceSearchEnable.value = !advanceSearchEnable.value; };
const siteStatusList = ref(["Active", "Inactive"]);

// local storage values
const localStorageKey = "Sites";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const siteStatus = filterLocalStorage ? filterLocalStorage.siteStatus : "Active";
const siteName = filterLocalStorage ? filterLocalStorage.name : "";
const fullName = filterLocalStorage ? filterLocalStorage.fullName : "";
const emailAddress = filterLocalStorage ? filterLocalStorage.emailAddress : "";
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
// Search variables
const search = ref({
  searchText,
  name: siteName,
  fullName,
  emailAddress,
  siteStatus
});

// Table variables
const tableRef = ref();
const activeRowId = ref(null);
const rows = ref([]);
const columns = ref([
  { name: "name", label: "Site Name", field: "name", align: "left", sortable: true },
  { name: "person.fullName", label: "Person Name", field: "person.fullName", align: "left", sortable: true },
  { name: "person.primaryPhoneNumber", label: "Phone Number", field: "person.primaryPhoneNumber", align: "left", sortable: true },
  { name: "person.primaryEmailAddress", label: "Email", field: "person.primaryEmailAddress", align: "left", sortable: true },
  { name: "active", label: "Active", align: "center", field: "active", sortable: true }
]);

// On page rendering
onMounted(() => {
  tableRef.value.requestServerInteraction();
});

const getSites = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
  siteService.getSites(payload).then((resp) => {
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

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getSites(propps);
};

// Clear search
const onClear = () => {
  search.value.siteStatus = "Active";
  search.value.name = "";
  search.value.fullName = "";
  search.value.emailAddress = "";
  clearLocalStorage(localStorageKey);
  onSearch();
};

// Create popup
const onAdd = () => {
  $q.dialog({
    component: EditOrganization,
    componentProps: {}
  }).onOk((resp) => {
    getSites({ pagination: pagination.value });
    onNewPassword(resp);
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Edit popup
const onEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: EditOrganization,
    componentProps: { id }
  }).onOk(() => {
    getSites({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// Delete record
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.name}` }, () => {
    siteService.deleteOrganization(item.id).then(resp => {
      notifySuccess({ message: "Site is deleted successfully." });
      getSites({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};

// View popup
const onView = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewOrganization,
    componentProps: { id }
  }).onOk(() => {
    getSites({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

const generateDropdown = (id) => {
  $q.dialog({
    title: "Confirm",
    message: "Do you want to generate dropdown?",
    ok: { label: "Yes", color: "primary" },
    cancel: { label: "No", color: "negative" },
    persistent: true
  }).onOk(() => {
    siteService.generateDropdown(id).then(() => {
      notifySuccess({ message: "Dropdowns generated successfully." });
      getSites({ pagination: pagination.value });
    });
  });
};

const generateMasterNotifications = (id) => {
  $q.dialog({
    title: "Confirm",
    message: "Do you want to generate master notifications?",
    ok: { label: "Yes", color: "primary" },
    cancel: { label: "No", color: "negative" },
    persistent: true
  }).onOk(() => {
    siteService.generateMasterNotifications(id).then(() => {
      notifySuccess({ message: "Master Notifications generated successfully." });
      getSites({ pagination: pagination.value });
    });
  });
};

function onNewPassword (item) {
  $q.dialog({
    component: newPassword,
    componentProps: { item }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getSites({ pagination: pagination.value });
});

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
  ...mapSingleFilterToLabel(search.value.siteStatus, siteStatusList, "Site Status"),
  ...(search.value.name ? { "Site Name": search.value.name } : {}),
  ...(search.value.fullName ? { "Person Name": search.value.fullName } : {}),
  ...(search.value.emailAddress ? { Email: search.value.emailAddress } : {})
}));

function onClearFilters (key) {
  if (key === "Site Status") {
    search.value.siteStatus = null;
  } else if (key === "Site Name") {
    search.value.name = "";
  } else if (key === "Person Name") {
    search.value.fullName = "";
  } else if (key === "Email") {
    search.value.emailAddress = "";
  }
  delete appliedFilters.value[key];
  getSites({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  default: return null;
  }
}
</script>
