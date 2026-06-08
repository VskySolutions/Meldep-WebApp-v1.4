<template>
  <q-page padding>
    <q-card>
      <q-card-section class="card-header with-tools flex justify-between items-center">
        <q-breadcrumbs class="text-brown text-weight-bold text-h3">
          <template #separator>
            <q-icon size="1.5em" name="o_chevron_right" color="primary" />
          </template>
          <q-breadcrumbs-el label="Settings" clickable to="/Settings" />
          <q-breadcrumbs-el label="Master Roles" />
        </q-breadcrumbs>
        <div class="items-center">
          <q-btn icon="o_add" outline label="Add Role" no-caps class="text-primary btnRounded" @click="onAdd" />
          <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-sm" @click="$router.back()" />
        </div>
      </q-card-section>
      <q-separator />
      <div v-if="loading">
        <div class="flex justify-center q-py-md">
          <q-spinner-ios size="40px" color="grey" />
        </div>
      </div>
      <div class="row q-col-gutter-md Custom-DataTable q-pa-md">
        <div v-for="role in rows" :key="role.id" class="col-12 col-sm-6 col-md-3 col-lg-3">
          <q-card :class="activeRowId == role.id ? 'highlight' : ''" style="border:1px solid #1b75ab">
            <div class="q-pa-md">
              <div class="flex justify-between items-center">
                <span class="text-h2 break-word" style="white-space: normal; word-break: break-word;">{{ role.name }}</span>
                <div class="actions flex items-center">
                  <q-icon
                    name="o_edit"
                    class="cursor-pointer q-mr-sm"
                    @click="onEdit(role.id)"
                  >
                    <q-tooltip>Edit</q-tooltip>
                  </q-icon>
                  <q-icon
                    v-if="role.name !== 'Site Super Admin'"
                    name="o_delete_outline"
                    class="cursor-pointer"
                    color="negative"
                    @click="onDelete(role)"
                  >
                    <q-tooltip>Delete</q-tooltip>
                  </q-icon>
                </div>
              </div>
            </div>
          </q-card>
        </div>
      </div>
    </q-card>
    <q-card />
  </q-page>
</template>
<script setup>
// Import libraries
import { ref, onMounted } from "vue";
import { useQuasar } from "quasar";
import { zwConfirmDelete, notifySuccess } from "assets/utils";

import roleService from "modules/roles/role.service";

import EditRole from "modules/roles/components/addEdit.vue";

// ----------------------------
// Common variables
// ----------------------------
const $q = useQuasar();
const loading = ref(true);
const rows = ref([]);
const activeRowId = ref(null);

// ----------------------------
// Get the list of master roles
// ----------------------------
const getMasterRoles = () => {
  try {
    loading.value = true;
    roleService.getRoles().then((resp) => {
      rows.value = resp.data;
    });
  } catch (error) {
    console.error("Error loading master roles:", error);
  } finally {
    setTimeout(() => {
      loading.value = false;
    }, 1500);
  }
};

// ----------------------------
// Create popup
// ----------------------------
const onAdd = () => {
  $q.dialog({
    component: EditRole,
    componentProps: {}
  }).onOk(() => {
    getMasterRoles();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// ----------------------------
// Edit popup
// ----------------------------
const onEdit = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: EditRole,
    componentProps: { id }
  }).onOk(() => {
    getMasterRoles();
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// ----------------------------
// Delete record
// ----------------------------
const onDelete = (item) => {
  activeRowId.value = item.id;
  zwConfirmDelete({ data: `${item.name}` }, () => {
    roleService.deleteMasterRole(item.id).then(resp => {
      notifySuccess({ message: "Role is deleted successfully." });
      getMasterRoles();
    });
  }, () => {
    activeRowId.value = null;
  });
};

// ----------------------------
// On page rendering
// ----------------------------
onMounted(() => {
  getMasterRoles();
});
</script>
