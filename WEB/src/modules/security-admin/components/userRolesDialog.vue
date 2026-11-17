<template>
  <q-dialog :model-value="!!user" @update:model-value="value => { if (!value) $emit('close'); }" @show="load">
    <q-card style="min-width: 420px; max-width: 95vw;">
      <q-card-section class="text-subtitle1">Roles in this site: {{ user?.name || user?.userName }}</q-card-section>
      <q-card-section class="q-pt-none">
        <q-select
          v-model="selected"
          :options="roles"
          :loading="loading"
          multiple
          use-chips
          outlined
          dense
          emit-value
          map-options
          option-value="siteRoleId"
          option-label="name"
          label="Roles *"
          aria-label="Roles"
        />
        <div class="text-caption text-grey-8 q-mt-sm">A user needs at least one valid role to open this site.</div>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat no-caps label="Cancel" @click="$emit('close')" />
        <q-btn unelevated no-caps color="primary" label="Save" :disable="selected.length === 0" :loading="saving" @click="save" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref } from "vue";
import { notifySuccess } from "assets/utils";
import securityAdminService from "../securityAdmin.service";

const props = defineProps({
  user: { type: Object, default: null }
});
const emit = defineEmits(["close", "saved"]);

const roles = ref([]);
const selected = ref([]);
const loading = ref(false);
const saving = ref(false);

async function load () {
  loading.value = true;
  selected.value = [];
  try {
    const [allRoles, userRoles] = await Promise.all([
      securityAdminService.getRoles(),
      securityAdminService.getUserRoles(props.user.userId)
    ]);
    roles.value = allRoles;
    selected.value = userRoles.map(role => role.siteRoleId);
  } finally {
    loading.value = false;
  }
}

async function save () {
  saving.value = true;
  try {
    await securityAdminService.setUserRoles(props.user.userId, selected.value);
    notifySuccess({ message: "Roles updated." });
    emit("saved");
    emit("close");
  } catch {
    // The request's error is already shown, and the dialog stays open to try again.
  } finally {
    saving.value = false;
  }
}
</script>
