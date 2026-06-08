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
              <q-breadcrumbs-el label="Settings" clickable to="/Settings" />
              <q-breadcrumbs-el label="Site Roles" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-4">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <q-separator />
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
                          <label class="Cutomlabel q-mt-sm fs-13">Role Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.siteRoleIds"
                            push
                            class="q-mx-sm w-100 h-auto"
                            clearable
                            use-input
                            use-chips
                            hide-bottom-space
                            :dense="true"
                            multiple
                            input-debounce="0"
                            :options="siteRolesList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            @filter="getAllSitesRoleListForFilter"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center selection-drodown-item">
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
              <div class="q-ml-xs">
                <q-btn
                  icon="o_chevron_left"
                  outline label="Back"
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
                  to="/Settings"
                />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div v-if="loading">
        <div class="flex justify-center q-py-md">
          <q-spinner-ios size="40px" color="grey" />
        </div>
      </div>
      <div class="row q-col-gutter-md Custom-DataTable q-pa-md">
        <div v-for="siteRole in rows" :key="siteRole.id" class="col-12 col-sm-6 col-md-3 col-lg-3">
          <q-card :class="activeRowId == siteRole.id ? 'highlight' : ''" style="border:1px solid #1b75ab">
            <div class="q-pa-md">
              <div class="flex justify-between items-center">
                <span class="text-h2">{{ siteRole.applicationRole.name }}</span>
                <div class="actions flex items-center">
                  <q-icon
                    name="o_manage_accounts"
                    class="cursor-pointer q-mr-sm"
                    @click="$router.push({ path: 'sites/site-menu-role-permissions', state: { siteRoleId: siteRole.id } })"
                  >
                    <q-tooltip>Manage Permissions</q-tooltip>
                  </q-icon>
                  <div class="relative-position q-mr-sm">
                    <q-icon name="o_group" class="cursor-pointer" @click="onRoleUserListView(siteRole.id)">
                      <q-tooltip>Users</q-tooltip>
                    </q-icon>
                    <q-badge
                      style="position: absolute; right: -6px; top: -10px;"
                      color="green"
                      text-color="white"
                      :label="userCounts[siteRole.id]"
                    />
                  </div>
                </div>
              </div>
            </div>
          </q-card>
        </div>
      </div>
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onMounted, computed, watch } from "vue";
import { useQuasar } from "quasar";
import { setLocalStorage, getLocalStorage, clearLocalStorage } from "assets/utils";

import RoleUserListView from "modules/roles/components/roleUserList.vue";

import roleService from "modules/roles/role.service";
import sitesService from "modules/sites/site.service";

// ----------------------------
// Common variables
// ----------------------------
const $q = useQuasar();
const loading = ref(true);
const rows = ref([]);
const activeRowId = ref(null);
const showFilter = ref(false);
const searchLoader = ref(false);
const userCounts = ref({});

// ----------------------------
// local storage values
// ----------------------------
const localStorageKey = "SiteRoles";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const siteRoleIds = filterLocalStorage ? filterLocalStorage.siteRoleIds : [];

// ----------------------------
// Search variables
// ----------------------------
const search = ref({
  searchText,
  siteRoleIds
});

// ----------------------------
// Table variable
// ----------------------------
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "", descending: true, rowsPerPage: 100, page: 1 });

const getTotalUser = (siteRoleId) => {
  if (!userCounts.value[siteRoleId]) {
    roleService.getUserCountBySiteRole(siteRoleId).then((resp) => {
      userCounts.value[siteRoleId] = resp;
    });
  }
};

const getSiteRoles = (props) => {
  try {
    const { page, rowsPerPage, sortBy, descending } = props.pagination;
    loading.value = true;
    const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
    setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination });
    roleService.getSiteRoles(payload).then((resp) => {
      rows.value = resp.data;
      rows.value.forEach((siteRole) => getTotalUser(siteRole.id));
      pagination.value.page = page;
      pagination.value.rowsPerPage = rowsPerPage;
      pagination.value.sortBy = sortBy;
      pagination.value.descending = descending;
      pagination.value.rowsNumber = resp.total;
    });
  } catch (error) {
    console.error("Error loading site roles:", error);
  } finally {
    setTimeout(() => {
      loading.value = false;
      searchLoader.value = false;
    }, 1500);
  }
};

// ----------------------------
// Get All Dropdowns
// ----------------------------

// Get all site role list for dropdown
const siteRolesList = ref([]);
const siteRolesListFilter = ref([]);
function getAllSitesRoleListForDropdown () {
  sitesService.getAllSitesRoleListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.applicationRole.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    siteRolesList.value = responseData;
    siteRolesListFilter.value = responseData;
  });
}

// Search site role for dropdown
function getAllSitesRoleListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      siteRolesList.value = siteRolesListFilter.value;
    } else {
      siteRolesList.value = siteRolesListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// ----------------------------
// Role User List View popup
// ----------------------------
const onRoleUserListView = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: RoleUserListView,
    componentProps: { id }
  }).onOk(() => {
    getSiteRoles({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
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

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.siteRoleIds, siteRolesList, "Role Name")
}));

// ----------------------------
// Search records as per parameters
// ----------------------------

const onSearch = () => {
  const props = { pagination: pagination.value };
  getSiteRoles(props);
};

// ----------------------------
// Clear search
// ----------------------------
const onClear = () => {
  search.value.siteRoleIds = [];
  clearLocalStorage(localStorageKey);
  onSearch();
};

function onClearFilters (key) {
  if (key === "Role Name") {
    search.value.siteRoleIds = [];
  }
  delete appliedFilters.value[key];
  getSiteRoles({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  case "Role Name": return search.value.siteRoleIds?.length || 0;
  default: return null;
  }
}

// ----------------------------
// On page rendering
// ----------------------------
onMounted(() => {
  getSiteRoles({ pagination: pagination.value });
  getAllSitesRoleListForDropdown();
});

// ----------------------------
// Save static search into localstorage.
// ----------------------------
watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getSiteRoles({ pagination: pagination.value });
});
</script>
