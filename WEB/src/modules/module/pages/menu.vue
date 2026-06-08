<template>
  <q-page padding>
    <q-card>
      <q-card-section class="card-header with-tools flex justify-between items-center">
        <q-breadcrumbs class="text-brown text-weight-bold text-h3">
          <template #separator>
            <q-icon size="1.5em" name="o_chevron_right" color="primary" />
          </template>
          <!-- <q-breadcrumbs-el label="Dashboard" icon="o_home" clickable to="/dashboard" /> -->
          <q-breadcrumbs-el label="Modules" clickable to="/modules" />
          <q-breadcrumbs-el>{{ model.name }} Menus</q-breadcrumbs-el>
        </q-breadcrumbs>
        <div class="items-center flex">
          <q-select v-model="moduleId" class="q-mr-sm size-1z q-my-sm" use-input outlined stack-label hide-bottom-space :dense="true" :options="modules" option-value="value" option-label="text" emit-value map-options style="width:300px;margin-top: 10px;" @update:model-value="onModuleChange(moduleId)" />
          <q-btn icon="o_add" outline label="Add Menu" no-caps class="text-primary btnRounded" @click="onAdd" />
          <div class="q-ml-sm">
            <q-btn color="black" class="btnRounded" icon="o_arrow_back" label="Back" no-caps style="display: none;" @click="$router.push('/modules')" />
            <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded no-space-between" @click="$router.push('/modules')" />
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-table
        v-model:pagination="pagination" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell" class="Custom-DataTable"
        no-data-label="No data available" :filter="filter" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]"
      >
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td>{{ props.row.displayName }}</q-td>
            <q-td>{{ props.row.menuName }}</q-td>
            <q-td class="text-right">{{ props.row.sortorder }}</q-td>
            <q-td class="text-left">{{ props.row.active==true ? 'Active' : 'InActive' }}</q-td>
            <q-td class="text-center actions">
              <q-icon name="o_edit" class="cursor-pointer q-mr-sm" size="xs" @click="onEdit(props.row.id)">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onDelete(props.row)">
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
        </template>
      </q-table>
      <q-inner-loading :showing="pageLoading" label="Please wait..." label-class="text-teal" />
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onMounted, watch } from "vue";
import { useQuasar } from "quasar";
import { useRoute, useRouter } from "vue-router";
import EditMenu from "modules/module/components/addEditMenu.vue";
import moduleService from "modules/module/module.service";
import { zwConfirmDelete, notifySuccess } from "assets/utils";
import _ from "lodash";

const route = useRoute();
const router = useRouter();
const moduleId = route.params.id;
const modules = ref([]);
const $q = useQuasar();
const loading = ref(true);
const pageLoading = ref(false);
const rows = ref([]);
const filter = ref("");
const activeRowId = ref(null);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "displayName", label: "Menu Name", field: "name", align: "left", sortable: true },
  { name: "menuName", label: "Menu Identifire", field: "menuPrefix", align: "left", sortable: true },
  { name: "sortorder", label: "Sort Order", field: "center", align: "right", sortable: true },
  { name: "active", label: "Status", field: "status", align: "left", sortable: true }
]);

const model = ref({
  name: ""
});

onMounted(() => {
  getModule();
  getModules();
  getModuleMenus(moduleId);
});

const getModule = () => {
  loading.value = true;
  moduleService.getModule(route.params.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

const getModules = () => {
  loading.value = true;
  moduleService.getModulesList().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    modules.value = responseData;
  }).finally(() => {
    loading.value = false;
  });
};

const getModuleMenus = (moduleId) => {
  loading.value = true;
  moduleService.getModuleMenus(moduleId).then((resp) => {
    rows.value = resp;
  }).finally(() => {
    loading.value = false;
  });
};

const onModuleChange = (id) => {
  if (id) {
    router.push({ path: "/modules/" + id + "/menus", reload: true });
  }
};

watch(() => route.params.id, (newValue, oldValue) => {
  if (newValue) {
    const moduleId = route.params.id;
    getModule();
    getModuleMenus(moduleId);
  }
}, { immediate: true });

const onAdd = () => {
  $q.dialog({
    component: EditMenu,
    componentProps: { moduleId: route.params.id }
  }).onOk(() => {
    getModuleMenus(route.params.id);
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

const onEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: EditMenu,
    componentProps: { id }
  }).onOk(() => {
    getModuleMenus(route.params.id);
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.displayName}` }, () => {
    moduleService.deleteMenu(item.id).then(resp => {
      notifySuccess({ message: "Menu is deleted successfully." });
      getModuleMenus(route.params.id);
    });
  }, () => {
    activeRowId.value = null;
  });
};
</script>
