<template>
  <q-page padding>
    <q-card class="no-border">
      <q-card-section class="card-header with-tools flex justify-between items-center">
        <q-breadcrumbs class="text-brown text-weight-bold text-h3">
          <template v-slot:separator>
            <q-icon size="1.5em" name="o_chevron_right" color="primary" />
          </template>
          <q-breadcrumbs-el label="Settings" clickable to="/Settings" />
          <q-breadcrumbs-el label="Modules" />
        </q-breadcrumbs>
        <div class="items-center">
          <q-btn icon="o_add" outline label="Add Module" no-caps @click="onAdd" class="text-primary btnRounded" />
          <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-sm" @click="$router.push('/Settings')" />
        </div>
      </q-card-section>
      <q-separator />
      <q-table
        ref="tableRef" v-model:pagination="pagination" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell" class="Custom-DataTable"
        no-data-label="No data available" :filter="filter" binary-state-sort :rows-per-page-options="[15, 30, 50 ,100]"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
            <q-th auto-width class="text-center hidden">Menu</q-th>
            <q-th auto-width class="text-center">Actions</q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
            <q-td style="width: 75%">{{ props.row.name }}</q-td>
            <q-td style="width: 10%" class="text-right">{{ props.row.sortorder }}</q-td>
            <q-td class="hidden">
              <q-btn class="text-primary btnRounded" outline no-caps @click="$router.push('/modules/'+props.row.id+'/menus')">Add Menu</q-btn>
            </q-td>
            <q-td auto-width class="text-center actions" style="width: 25%">
              <q-icon name="o_add" class="cursor-pointer q-mr-sm" @click="$router.push('/modules/'+props.row.id+'/menus')" size="xs">
                <q-tooltip>Add Menu</q-tooltip>
              </q-icon>
              <q-icon name="o_edit" class="cursor-pointer q-mr-sm" @click="onEdit(props.row.id)" size="xs">
                <q-tooltip>Edit</q-tooltip>
              </q-icon>
              <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" @click="onDelete(props.row)" size="xs">
                <q-tooltip>Delete</q-tooltip>
              </q-icon>
            </q-td>
          </q-tr>
        </template>
      </q-table>
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onMounted } from "vue";
import { useQuasar } from "quasar";
import EditModule from "modules/module/components/addEdit.vue";
import moduleService from "modules/module/module.service";
import { zwConfirmDelete, notifySuccess } from "assets/utils";

const $q = useQuasar();

const loading = ref(true);
const rows = ref([]);
const filter = ref("");
const activeRowId = ref(null);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "name", label: "Module Name", field: "name", align: "left", sortable: true },
  { name: "sortorder", label: "Sort Order", field: "sortorder", align: "right", sortable: true }
]);

onMounted(() => {
  getModules();
});

const getModules = () => {
  loading.value = true;
  moduleService.getModulesList().then((resp) => {
    rows.value = resp;
  }).finally(() => {
    loading.value = false;
  });
};

const onAdd = () => {
  $q.dialog({
    component: EditModule,
    componentProps: {}
  }).onOk(() => {
    getModules();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

const onEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: EditModule,
    componentProps: { id }
  }).onOk(() => {
    getModules();
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.name}` }, () => {
    moduleService.deleteModule(item.id).then(resp => {
      notifySuccess({ message: "Module is deleted successfully." });
      getModules();
    });
  }, () => {
    activeRowId.value = null;
  });
};
</script>
