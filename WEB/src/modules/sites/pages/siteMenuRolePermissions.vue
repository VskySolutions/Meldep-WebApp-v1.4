<template>
  <q-page padding class="permissions">
    <q-card>
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-xs-3 col-sm-2 col-md-3 col-lg-4 col-xl-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el v-if="siteId" label="Settings" clickable to="/Settings" />
              <q-breadcrumbs-el v-else label="Site Roles" clickable to="/site-roles" />
              <q-breadcrumbs-el v-if="siteId" :label="`Sites : ${siteName}`" clickable to="/sites" />
              <q-breadcrumbs-el label="Permissions" />
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
          <div class="col-12 col-xs-6 col-sm-8 col-md-7 col-lg-6 col-xl-6">
            <div class="row items-center justify-end no-wrap">
              <div class="search-container position-relative q-mr-sm">
                <searchFilterBar
                  v-model="search.searchText"
                  :loading="searchLoader"
                  :applied-filters="appliedFilters"
                  @toggle-filter="showFilter = !showFilter"
                />
                <!-- Dropdown Content -->
                <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                  <q-card class="q-pa-sm">
                    <multiSelectDropdown
                      v-model="search.moduleIds"
                      label="Module Name"
                      :options="siteModulesDropdown.list.value"
                      :filter="siteModulesDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.menuIds"
                      label="Menu Name"
                      :options="siteModuleMenusDropdown.list.value"
                      :filter="siteModuleMenusDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.roleIds"
                      label="Role Name"
                      :options="siteRolesDropdown.list.value"
                      :filter="siteRolesDropdown.filter"
                    />
                    <!-- Search and Clear Buttons -->
                    <div class="row justify-end q-gutter-sm q-mb-sm">
                      <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onSearch(); }" />
                      <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClear" />
                      <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                    </div>
                  </q-card>
                </q-menu>
              </div>
              <div class="flex items-center">
                <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded" @click="$router.back()" />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
    </q-card>
    <q-separator />
    <q-card class="no-border flex flex-center">
      <q-table
        v-model:pagination="pagination"
        :loading="loading"
        :rows="rows"
        :columns="columns"
        row-key="id"
        style="border:1px solid rgba(180, 180, 180, 0.4);max-width:1000px; width: 100%;"
        flat bordered
        no-data-label="Please Select Role Name"
        :filter="filter"
        binary-state-sort
        class="q-mt-md"
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        @request="getSiteModuleMenuPermission"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white" style="letter-spacing:0.6px;">
            <q-th class="text-center">Modules</q-th>
            <q-th class="text-center">Menus</q-th>
            <q-th class="text-center">Roles</q-th>
            <q-th class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.siteModuleMenuId ? 'highlight' : ''" :set="(preModuleName = null)">
            <q-td class="text-left" style="width: 25%; padding: 8px;"><span v-if="preModuleName !== props.row.moduleName" :set="preModuleName = props.row.moduleName">{{ props.row.moduleName }}</span></q-td>
            <q-td class="text-left" style="width: 25%; padding: 8px;">{{ props.row.menuName }}</q-td>
            <q-td style="width: 50%;">
              <div class="row items-center q-gutter-xs">
                <q-chip
                  v-for="(item, index) in (props.row.roles || [])"
                  :key="index"
                  dense
                  removable
                  class="bg-primary text-white"
                  @remove="onRemoveModuleMenuRoleAccess(
                    siteId,
                    props.row.siteModuleMenuId,
                    item.id,
                    props.row.menuName,
                    item.name,
                    refreshSiteModuleMenuPermissionList
                  )"
                >
                  {{ item.name }}
                </q-chip>
              </div>
            </q-td>
            <q-td auto-width>
              <q-btn
                icon="o_add"
                class="cursor-pointer q-mr-sm"
                size="xs"
                @click="openAddRolePopup(props.row)"
              >
                <q-tooltip>Add Role</q-tooltip>
              </q-btn>
            </q-td>
          </q-tr>
        </template>
      </q-table>
      <q-inner-loading :showing="pageLoading" label="Please wait..." label-class="text-teal" />
    </q-card>
  </q-page>
  <q-dialog v-model="showAddRoleDialog" @hide="activeRowId = null">
    <q-card style="min-width: 400px">
      <q-card-section>
        <div class="text-subtitle2">Add Role</div>
      </q-card-section>

      <q-card-section>
        <formMultiSelectDropdown
          v-model="model.siteRoleId"
          :options="siteRolesDropdown.list.value"
          :filter="siteRolesDropdown.list.filter"
          :option-disable="isRoleDisabled"
          popup-content-class="customPopupContentClass"
        />
      </q-card-section>

      <q-card-actions align="right" class="q-mb-sm">
        <q-btn v-close-popup label="Cancel" color="grey" flat dense />
        <q-btn label="Save" color="primary" class="q-mr-sm" dense @click="addRole" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>
<script setup>
import { ref, onMounted, computed, watch } from "vue";
import moduleService from "modules/module/module.service";
import { notifySuccess, notifyError } from "assets/utils";
import siteService from "../site.service";
import { useAuthStore } from "stores/auth";

// SOP Change :- Shared Inputs
import formMultiSelectDropdown from "src/components/form-inputs/_formMultiSelectDropdown.vue";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Project Actions
import {
  initSiteActions,
  onRemoveModuleMenuRoleAccess
} from "src/modules/sites/utils/actions.js";

// SOP Change :- Shared Dropdowns
import siteModule from "src/modules/sites/utils/dropdowns.js";

const loading = ref(true);
const pageLoading = ref(false);
const rows = ref([]);
const filter = ref("");
const showFilter = ref(false);
const searchLoader = ref(false);
const authStore = useAuthStore();
const loginUserSiteId = computed(() => authStore.user?.siteId);

const siteId = ref(history.state?.siteId || loginUserSiteId);
const siteRoleId = ref(history.state?.siteRoleId);
const selectedSiteRoleId = ref(siteRoleId.value);

const siteName = ref("");
const showAddRoleDialog = ref(false);
const selectedMenu = ref(null);

const model = ref({
  sitesModulesMenus: {
    sitesModules: {
      modules: {
        modulesMenus: {
          name: ""
        }
      }
    }
  },
  siteRoleId: null,
  siteId: null
});

// Search records as per parameters
const onSearch = () => {
  getSiteModuleMenuPermission({
    pagination: pagination.value
  });
};

// Clear search
const onClear = () => {
  search.value.searchText = "";
  search.value.moduleIds = [];
  search.value.menuIds = [];
  search.value.roleIds = [];
  saveDataTableState({
    search: search.value
  });
  onSearch();
};

function isRoleDisabled(opt) {
  if (!selectedMenu.value || !selectedMenu.value.roles) {
    return false;
  }
  return selectedMenu.value.roles.some(role => role.id === opt.value);
}

function refreshSiteModuleMenuPermissionList () {
  getSiteModuleMenuPermission({
    pagination: pagination.value
  });
}

const getSiteModuleMenuPermission = async ({ pagination: pageData }) => {
  try {
    loading.value = true;

    const { page, rowsPerPage, sortBy, descending } = pageData;

    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: activeRowId.value
    });

    const payload = {
      page,
      pageSize: rowsPerPage,
      sortBy,
      descending,
      siteId: siteId.value,
      ...search.value
    };

    const resp = await moduleService.getSiteMenuRolePermissions(payload);

    rows.value = resp?.siteMenuRolePermissionsList || [];

    Object.assign(pagination.value, {
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp?.total || 0
    });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

const getOrganization = () => {
  if (siteId.value !== null && siteId.value !== undefined) {
    siteService.getOrganization(siteId.value).then((resp) => {
      siteName.value = resp.name;
    });
  }
};

const {
  search,
  pagination,
  activeRowId,
  saveDataTableState
} = useSiteTableState({
  storageKey: "site-Menu-Role-Permission-Index",
  siteId,

  defaultSearch: {
    searchText: "",
    moduleIds: [],
    menuIds: [],
    roleIds: selectedSiteRoleId.value
    ? [selectedSiteRoleId.value]
    : []
  },

  defaultPagination: {
    sortBy: "sitesModulesMenus.sitesModules.modules.name",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initSiteActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { siteRolesDropdown, siteModulesDropdown, siteModuleMenusDropdown } = siteModule();

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Applied Filter Labels.
// ----------------------------------------------------------------------------------------------------------------
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

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.moduleIds, siteModulesDropdown.list, "Module Name"),
  ...mapFilterToLabel(search.value.menuIds, siteModuleMenusDropdown.list, "Menu Name"),
  ...mapFilterToLabel(search.value.roleIds, siteRolesDropdown.list, "Role Name")
}));

const onClearFilters = (key) => {
  if (key === "Module Name") {
    search.value.moduleIds = [];
  } else if (key === "Menu Name") {
    search.value.menuIds = [];
  } else if (key === "Role Name") {
    search.value.roleIds = [];
  }
  delete appliedFilters.value[key];
  refreshSiteModuleMenuPermissionList();
};

function getFilterCount (key) {
  switch (key) {
  case "Module Name": return search.value.moduleIds?.length || 0;
  case "Menu Name": return search.value.menuIds?.length || 0;
  case "Role Name": return search.value.roleIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

const openAddRolePopup = (row) => {
  activeRowId.value = row.siteModuleMenuId;
  selectedMenu.value = row;
  model.value.siteRoleId = null;
  showAddRoleDialog.value = true;
};

const addRole = async () => {
  if (!model.value.siteRoleId) {
    notifyError({
      message: "Please select a role."
    });
    return;
  }

  try {
    activeRowId.value = selectedMenu.value.siteModuleMenuId;

    pageLoading.value = true;

    const requestModel = {
      siteId: siteId.value,
      siteModuleMenuId: selectedMenu.value.siteModuleMenuId,
      siteRoleIds: model.value.siteRoleId.join(",")
    };

    await moduleService.assignRolesToMenu(requestModel);

    notifySuccess({
      message: "Access has been assigned to the selected role successfully."
    });

    showAddRoleDialog.value = false;
    model.value.siteRoleId = null;

    refreshSiteModuleMenuPermissionList();
  } finally {
    pageLoading.value = false;
    activeRowId.value = null;
  }
};

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(
  () => search.value.searchText,
  () => {
    searchLoader.value = true;
    refreshSiteModuleMenuPermissionList();
  }
);

watch(() => search.value.moduleIds, async (newValue, oldValue) => {
  if (newValue === oldValue) return;
  if (newValue?.length === 0) siteModuleMenusDropdown.load(siteId.value);

  siteModuleMenusDropdown.load(siteId.value, newValue);
});

onMounted(() => {
  const roleId = history.state?.siteRoleId;

  if (roleId) {
    search.value = {
      ...search.value,
      roleIds: [roleId]
    };

    saveDataTableState({
      search: search.value,
      pagination: pagination.value
    });
  }

  siteRolesDropdown.load(siteId.value);
  siteModulesDropdown.load(siteId.value);
  siteModuleMenusDropdown.load(siteId.value, search.value.moduleIds);
  getSiteModuleMenuPermission({
    pagination: pagination.value
  });
  getOrganization();
});
</script>

<style scoped>
  .permissions .rounded{
    border-radius: 8px;
  }
  @media(max-width:768px){
    .permissions .q-table{
      overflow-x: auto;
    }
  }
</style>
