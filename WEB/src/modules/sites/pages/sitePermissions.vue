<template>
<!-- OLD PAGE - Retained for reference only.
 Deleted -->
  <q-page padding class="permissions">
    <q-card>
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-6">
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
          <div class="col-12 col-md-6">
            <div class="row justify-end no-wrap">
              <label class="Cutomlabel q-mt-sm">Role Name : </label>
              <q-select
                v-model="model.siteRoleId"
                class="q-mx-sm w-100 h-auto col-12 col-sm-4 col-md-4"
                use-input
                hide-bottom-space
                :dense="true"
                :options="siteRolesList"
                option-value="value"
                option-label="text"
                emit-value
                map-options
                @update:model-value="onSiteRoleSelected"
                @filter="filterFn3"
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
              <div class="q-ml-sm">
                <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded" @click="$router.push(siteId !== null && siteId !== undefined ? '/sites' : '/site-roles')" />
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
    </q-card>
    <q-separator />
    <q-card class="no-border flex flex-center">
      <q-table
        v-model:pagination="pagination" :loading="loading" :rows="rows" :columns="columns" row-key="id" style="border:1px solid rgba(180, 180, 180, 0.4);max-width:1000px; width: 100%;" flat bordered
        no-data-label="Please Select Role Name" :filter="filter" binary-state-sort class="q-mt-md" :rows-per-page-options="[20, 50, 100, 200, 500]"
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
            <q-th colspan="3" class="text-center">Permissions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''" :set="(preModuleName = null)">
            <q-td class="text-left" style="width: 25%; padding: 8px;"><span v-if="preModuleName !== props.row.sitesModulesMenus.sitesModules.modules.name" :set="preModuleName = props.row.sitesModulesMenus.sitesModules.modules.name">{{ props.row.sitesModulesMenus.sitesModules.modules.name }}</span></q-td>
            <q-td class="text-left" style="width: 25%; padding: 8px;">{{ props.row.sitesModulesMenus.modulesMenus.displayName }}</q-td>
            <q-td class="text-right" style="width: 25%; vertical-align: middle;">
              <q-toggle v-model="props.row.isShowMenu" color="green" label="Show Menu" @click="onPermission(props.row.isShowMenu, props.row.siteModuleMenuId, props.row.id)" />
            </q-td>
            <q-td class="text-right" style="width: 25%; vertical-align: middle;">
              <q-toggle v-model="props.row.isView" color="green" label="View Permission" @click="onPermission(props.row.isView, props.row.siteModuleMenuId, props.row.id)" />
            </q-td>
            <q-td class="text-right" style="width: 25%; vertical-align: middle;">
              <q-toggle v-model="props.row.isManage" color="green" label="Manage Permission" @click="onPermission(props.row.isManage, props.row.siteModuleMenuId, props.row.id)" />
            </q-td>
          </q-tr>
        </template>
      </q-table>
      <q-inner-loading :showing="pageLoading" label="Please wait..." label-class="text-teal" />
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onMounted } from "vue";
import moduleService from "modules/module/module.service";
import { notifySuccess } from "assets/utils";
import siteService from "../site.service";

const loading = ref(true);
const pageLoading = ref(false);
const rows = ref([]);
const filter = ref("");
const activeRowId = ref(null);
const pagination = ref({ sortBy: "sitesModulesMenus.sitesModules.modules.name", descending: false, rowsPerPage: 100, page: 1 });
// const columns = ref([
//   { name: "sitesModulesMenus.sitesModules.modules.modulesMenus.name", label: "Menu Name", field: "sitesModulesMenus.sitesModules.modules.modulesMenus.name", align: "left", sortable: true }
// ]);
const siteId = ref(history.state?.siteId);
const siteRoleId = ref(history.state?.siteRoleId);
const selectedSiteRoleId = ref(siteRoleId.value);
const siteName = ref("");

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

const onSiteRoleSelected = (selectedRoleId) => {
  selectedSiteRoleId.value = selectedRoleId;
  getSiteModuleMenuPermission(siteId.value, selectedRoleId);
};

const getSiteModuleMenuPermission = (siteId, siteRoleId) => {
  loading.value = true;
  moduleService.getSiteModuleMenuPermission(siteId, siteRoleId).then((resp) => {
    if (resp && Array.isArray(resp) && resp.length > 0) {
      rows.value = resp;
    }
  }).finally(() => {
    loading.value = false;
  });
};

const getOrganization = () => {
  if (siteId.value !== null && siteId.value !== undefined) {
    siteService.getOrganization(siteId.value).then((resp) => {
      siteName.value = resp.name;
    });
  }
};

const siteRolesList = ref([]);
const options1 = ref([]);
function getAllSitesRoleListForDropdown (siteId) {
  siteService.getAllSitesRoleListForDropdown(siteId).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.applicationRole.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    siteRolesList.value = responseData;
    options1.value = responseData;
    model.value.siteRoleId = responseData.some(opt => opt.value === selectedSiteRoleId.value)
      ? selectedSiteRoleId.value
      : null;
  });
}

// Search role for dropdown
function filterFn3 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      siteRolesList.value = options1.value;
    } else {
      siteRolesList.value = options1.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const onPermission = (isShowMenu, siteModuleMenuId, rowId) => {
  loading.value = true;
  pageLoading.value = true;
  const model = ref({
    siteModuleMenuId,
    siteId,
    siteRoleId: selectedSiteRoleId.value,
    id: rowId,
    isShowMenu
  });
  moduleService.addSitesModuleMenusPermission(model.value).then((resp) => {
    notifySuccess({ message: "Permission updated successfully." });
  }).finally(() => {
    pageLoading.value = false;
    loading.value = false;
  });
};

onMounted(() => {
  getAllSitesRoleListForDropdown(siteId.value);
  getSiteModuleMenuPermission(siteId.value, selectedSiteRoleId.value);
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
