<template>
  <q-page padding>
    <q-card>
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-2">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el v-if="hasSiteId" label="User Management" />
              <q-breadcrumbs-el v-if="!hasSiteId" label="Settings" clickable to="/Settings" />
              <q-breadcrumbs-el v-if="!hasSiteId" :label="`Sites : ${siteName}`" clickable to="/Sites" />
              <q-breadcrumbs-el label="Users" />
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
                  <q-input v-model="search.searchText" :loading="searchLoader" outlined dense clearable debounce="300" placeholder="Search" class="bg-white search-box" style="flex: 1; border-top-right-radius: 0; border-bottom-right-radius: 0; border-top-right-radius: 0; max-width: 250px;">
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
                            v-model="search.userStatus" clearable class="q-mx-sm w-100 h-auto" stack-label hide-bottom-space use-input :dense="true"
                            :options="userStatusList" emit-value map-options :popup-content-class="customPopupContentClass"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel">User Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.userName"
                            clearable class="q-mx-sm w-100 h-auto" use-input stack-label hide-bottom-space :dense="true" :options="userNameList"
                            option-value="value" option-label="text" emit-value map-options @filter="userNameListForFilter"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel">Full Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.fullName" push class="q-mx-sm w-100 h-auto" hide-bottom-space :dense="true" />

                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Email</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.email" push class="q-mx-sm w-100 h-auto" hide-bottom-space :dense="true" type="email" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel">Roles</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.siteRoleIds" push class="q-mx-sm w-100 h-auto" clearable use-input use-chips transition-show="jump-up"
                            transition-hide="jump-up" hide-bottom-space :dense="true" multiple fill-input input-debounce="0"
                            :options="siteRolesList" option-value="value" option-label="text" emit-value map-options :popup-content-class="customPopupContentClass" @filter="siteRolesListForFilter"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center selection-drodown-item">
                                    <q-checkbox
                                      :model-value="selected"
                                      @update:model-value="toggleOption(opt)"
                                    />
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
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Add User" no-caps class="text-primary btnRounded" @click="onAdd" />
                <q-btn v-if="!hasSiteId" icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-md no-space-between" @click="$router.push('/Sites')" />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-table ref="tableRef" v-model:pagination="pagination" :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" flat :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell" no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getUsers">
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
            <q-td style="width: 5%;">{{ props.row.username }}</q-td>
            <q-td style="width: 15%;">{{ props.row.person.fullName }}</q-td>
            <q-td style="width: 15%;">{{ props.row.email }}</q-td>
            <q-td style="width: 10%;">{{ props.row.phoneNumber }}</q-td>
            <q-td style="width: 30%;">
              <span v-if="props.row.userRoles.length > 0">
                <span v-for="(userMapping, index) in props.row.userRoles" :key="userMapping.id">
                  <q-chip outline square color="deep-orange" text-color="white">{{ userMapping.role.name }}</q-chip>
                  <span v-if="index !== props.row.userRoles.length - 1" />
                </span>
              </span>
            </q-td>
            <q-td style="width: 5%;">{{ props.row.type }}</q-td>
            <q-td style="width: 5%;">
              <q-icon :name="props.row.person.isSharedUser ? 'o_check_circle' : 'o_cancel'" :color="props.row.person.isSharedUser ? 'positive' : 'negative'" class="cursor-pointer" size="sm" style="margin-left: 45%;">
                <q-tooltip>{{ props.row.person.isSharedUser ? 'Shared User' : 'Not Shared User' }}</q-tooltip>
              </q-icon>
            </q-td>
            <q-td style="width: 5%;">
              <q-icon :name="props.row.active === 'Active' ? 'o_check_circle' : 'o_cancel'" :color="props.row.active === 'Active' ? 'positive' : 'negative'" class="cursor-pointer" size="sm" style="margin-left: 45%;">
                <q-tooltip>{{ props.row.active === 'Active' ? 'Active' : 'Inactive' }}</q-tooltip>
              </q-icon>
            </q-td>
            <q-td auto-width class="text-left actions">
              <div v-if="!props.row.person.isSharedUser">
                <q-icon name="o_mail" class="cursor-pointer q-mr-sm" @click="onSendUserLoginDetails(props.row)">
                  <q-tooltip>Send User Login Details</q-tooltip>
                </q-icon>
                <q-icon name="o_edit" class="cursor-pointer q-mr-sm" @click="onEdit(props.row.id)">
                  <q-tooltip>Edit</q-tooltip>
                </q-icon>
                <q-icon v-if="!props.row.userRoles.some(role => role.role.name === 'Site Super Admin' || role.role.name === 'System Super Admin')" name="o_delete_outline" class="cursor-pointer" color="negative" @click="onDelete(props.row)">
                  <q-tooltip>Delete</q-tooltip>
                </q-icon>
                <q-icon v-if="!props.row.userRoles.some(role => role.role.name === 'Site Super Admin' || role.role.name === 'System Super Admin')" :name="props.row.active === 'Active' ? 'o_block' : 'o_check_circle_outline'" :color="props.row.active === 'Active' ? 'negative' : 'positive'" class="cursor-pointer q-ml-sm" @click="toggleActiveStatus(props.row)">
                  <q-tooltip>{{ props.row.active === 'Active' ? 'Make Inactive' : 'Set Active' }}</q-tooltip>
                </q-icon>
              </div>
            </q-td>
          </q-tr><q-separator />
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onBeforeUnmount, onMounted, computed, watch } from "vue";
import { useQuasar } from "quasar";
import { useAuthStore } from "stores/auth";
import { zwConfirm, zwConfirmDelete, notifySuccess } from "assets/utils";
import EditUser from "modules/user-management/components/addEdit.vue";
import usersService from "modules/user-management/userManagement.service";
import sitesService from "src/modules/sites/site.service";

// Shared DataTable Views
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

const $q = useQuasar();
const loading = ref(true);
const rows = ref([]);
const authStore = useAuthStore();
const userStatusList = ref(["Active", "Inactive"]);
const showFilter = ref(false);
const searchLoader = ref(false);
const siteId = ref(history.state?.siteId);
const siteName = ref("");
const hasSiteId = computed(() => !siteId.value);

const tableRef = ref();
const currentSiteId = computed(() => authStore.user?.siteId);

const columns = ref([
  { name: "username", label: "User Name", field: "username", align: "left", sortable: true },
  { name: "person.fullName", label: "Full Name", field: "person.fullName", align: "left", sortable: true },
  { name: "email", label: "Email", field: "email", align: "left", sortable: true },
  { name: "phoneNumber", label: "Phone Number", field: "phoneNumber", align: "left", sortable: false },
  { name: "userRoles", label: "Assigned Roles", field: "userRoles", align: "left", sortable: false },
  { name: "type", label: "Type", field: "type", sortable: true, align: "left" },
  { name: "isSharedUser", label: "Shared User", field: "isSharedUser", sortable: false, align: "center" },
  { name: "active", label: "Active/Inactive", field: "active", align: "center" }
]);

const {
  search,
  pagination,
  activeRowId,
  saveDataTableState
} = useSiteTableState({
  storageKey: "users-Index",
  siteId: currentSiteId,
  defaultSearch: {
    searchText: "",
    userStatus: "Active",
    userName: "",
    fullName: "",
    email: "",
    siteRoleIds: []
  },
  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

const getUsers = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    searchText: search.value.searchText || null,
    siteId: siteId.value ? siteId.value : null,
    userStatus: search.value.userStatus,
    userName: search.value.userName,
    fullName: search.value.fullName,
    email: search.value.email,
    siteRoleIds: search.value.siteRoleIds,
    firstName: search.value.firstName,
    lastName: search.value.lastName
  };
  usersService.getUsers(payload).then((resp) => {
    rows.value = resp.data.map(item => ({
      ...item,
      active: item.active ? "Active" : "Inactive"// Set active status for each row
    }));
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
    saveDataTableState({
      search: search.value,
      pagination: props.pagination,
      activeRowId: activeRowId.value
    });
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

const getOrganization = () => {
  if (siteId.value) {
    sitesService.getOrganization(siteId.value).then((resp) => {
      siteName.value = resp.name;
    });
  }
};

const handleDocumentClick = (event) => {
  if (event.target.closest(".q-dialog")) {
    return;
  }

  const highlightElement = document.querySelector(".highlight");
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;

    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: null
    });
  }
};

const onSearch = () => {
  const propps = { pagination: pagination.value };
  getUsers(propps);
};
// Clear search
const onClear = () => {
  search.value.userStatus = "Active";
  search.value.userName = "";
  search.value.siteRoleIds = [];
  search.value.fullName = "";
  search.value.email = "";
  saveDataTableState({
    search: search.value
  });
  onSearch();
};

const onAdd = () => {
  $q.dialog({ component: EditUser, componentProps: {} }).onOk(() => {
    getUsers({ pagination: pagination.value });
    getAllUserListForDropdown(siteId.value);
  }).onCancel(() => { }).onDismiss(() => { });
};

const onEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({ component: EditUser, componentProps: { id } }).onOk(() => {
    getUsers({ pagination: pagination.value });
  }).onCancel(() => { }).onDismiss(() => { activeRowId.value = id; });
};

const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.username}, ${item.person.firstName}` }, () => {
    usersService.deleteUser(item.id).then(resp => {
      notifySuccess({ message: "User status is saved successfully." });
      getUsers({ pagination: pagination.value });
    });
  }, () => {
    activeRowId.value = null;
  });
};

// Toggle Active/Inactive Status
const toggleActiveStatus = (row) => {
  activeRowId.value = row.id;
  const isCurrentlyActive = row.active === "Active";
  const newStatus = !isCurrentlyActive; // Toggle the status

  $q.dialog({
    title: "Confirmation",
    message: `Are you sure you want to ${isCurrentlyActive ? "deactivate" : "activate"} this user?`,
    ok: { label: "Yes", color: "primary" },
    cancel: { label: "No", color: "negative" }
  }).onOk(() => {
    usersService.updateUserStatus(row.id, newStatus)
      .then(() => {
        notifySuccess({ message: `User has been ${newStatus ? "activated" : "deactivated"} successfully.` });
        row.active = newStatus ? "Active" : "Inactive";
        getUsers({ pagination: pagination.value });
        activeRowId.value = null;
      })
      .catch(() => {
        $q.notify({
          type: "negative",
          message: `Failed to ${newStatus ? "activate" : "deactivate"} the user.`
        });
        activeRowId.value = null;
      })
      .finally(() => {
        activeRowId.value = null;
      });
  })
  .onCancel(() => {
    activeRowId.value = null;
  });
};

// ----------------------------
// DropDowns
// ----------------------------
// Get all user username for dropdown
const userNameList = ref([]);
const userNameListOptions = ref([]);
function getAllUserListForDropdown (siteId) {
  usersService.getAllUserListForDropdown(siteId).then((resp) => {
    const responseData = resp
      .map((item) => ({ text: item.userName, value: item.userName }))
      .sort((a, b) => a.text.localeCompare(b.text));
    userNameList.value = responseData;
    userNameListOptions.value = responseData;
  });
}

function userNameListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      userNameList.value = userNameListOptions.value;
    } else {
      userNameList.value = userNameListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const siteRolesList = ref([]);
const siteRolesListOptions = ref([]);
function getAllSitesRoleListForDropdown (siteId) {
  sitesService.getAllSitesRoleListForDropdown(siteId).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.applicationRole.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    siteRolesList.value = responseData;
    siteRolesListOptions.value = responseData;
  });
}

function siteRolesListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      siteRolesList.value = siteRolesListOptions.value;
    } else {
      siteRolesList.value = siteRolesListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

function onSendUserLoginDetails (item) {
  activeRowId.value = item.id;
  zwConfirm({ message: "User login details will send to " + item.email + "?" }, () => {
    loading.value = true;
    usersService.sendUserLogin(item.id).then(resp => {
      notifySuccess({ message: "Sent successfully." });
      loading.value = false;
    });
  }, () => { });
}

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

const mapSingleFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapSingleFilterToLabel(search.value.userStatus, userStatusList, "User Status"),
  ...(search.value.userName ? { "User Name": search.value.userName } : {}),
  ...(search.value.fullName ? { "Full Name": search.value.fullName } : {}),
  ...(search.value.email ? { Email: search.value.email } : {}),
  ...mapFilterToLabel(search.value.siteRoleIds, siteRolesList, "Roles")
}));

function onClearFilters (key) {
  if (key === "User Status") {
    search.value.userStatus = null;
  } else if (key === "User Name") {
    search.value.userName = [];
  } else if (key === "Full Name") {
    search.value.fullName = "";
  } else if (key === "Email") {
    search.value.email = "";
  }
  delete appliedFilters.value[key];
  getUsers({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  case "Roles": return search.value.siteRoleIds?.length || 0;
  default: return null;
  }
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => search.value.searchText, () => {
  if (search.value.searchText) {
    searchLoader.value = true;
  }
  getUsers({ pagination: pagination.value });
});

onMounted(() => {
  tableRef.value.requestServerInteraction();
  getAllUserListForDropdown(siteId.value);
  getAllSitesRoleListForDropdown(siteId.value);
  getOrganization();
  document.addEventListener("click", handleDocumentClick);
});

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});
</script>
