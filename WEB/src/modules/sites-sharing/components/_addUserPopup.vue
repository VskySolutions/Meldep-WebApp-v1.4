<template>
  <q-btn 
    icon="o_add" 
    outline 
    label="Add User" 
    no-caps 
    class="text-primary btnRounded"
    @click="email = ''"
  >
    <q-popup-edit
      v-model="email"
      anchor="center middle"
      self="center middle"
      persistent
      class="wallet-popup"
    >
      <template #default="scope">
        <div style="min-width: 260px;" class="q-pa-sm">
          <!-- Header -->
          <div class="row justify-between items-center">
            <span class="text-subtitle2 q-mb-sm">
              Add User
            </span>
            <q-btn icon="o_close" flat dense round size="sm" @click="scope.cancel" />
          </div>

          <!-- Wallet Number -->
          <div class="q-mt-sm">
            <div class="field-label">
              Email
              <span class="required">*</span>
            </div>

            <q-input
              v-model="scope.value"
              type="email"
              outlined
              dense
              :error="!!emailError"
              :error-message="emailError"
              @update:model-value="emailError = ''"
            >
            </q-input>
          </div>
          <div>            
              <div class="row q-col-gutter-x-md">
                <formMultiSelectDropdown
                  v-model="localRow.roleIds"
                  label="Roles"
                  :options="siteRolesDropdown.list.value"
                  :filter="siteRolesDropdown.filter"
                  wrapper-class="col-12 col-md-12 q-mb-md"
                  :error="!!roleError"
                  :error-message="roleError"
                  @update:model-value="val => { if (val?.trim()) roleError = '' }"
                />
              </div>
          </div>
          <!-- Buttons -->
          <div class="row justify-end q-gutter-sm q-mt-md">
            <q-btn
              flat
              dense
              label="Cancel"
              color="primary"
              @click="scope.cancel"
            />
            <q-btn
              unelevated
              dense
              label="Save"
              color="primary"
              :loading="processing"
              @click="() => onSave(scope)"
            />
          </div>
        </div>
      </template>
    </q-popup-edit>
  </q-btn>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";

// SOP Change :- Shared Dropdowns
import siteModule from "src/modules/sites/utils/dropdowns.js";

//shared Inputs
import formMultiSelectDropdown from "src/components/form-inputs/_formMultiSelectDropdown.vue";

const props = defineProps({
  onSaveApi: { type: Function, required: true }
});

const emit = defineEmits(["success"]);

const emailError = ref("");
const roleError = ref("");
const processing = ref(false);

const localRow = reactive({
  email: null,
  roleIds: []
});

// =================================================================================
// Validation
// =================================================================================

function validateEmail (email) {
  emailError.value = "";

  if (!email || !email.trim()) {
    emailError.value = "Email is required";
    return false;
  }

  const isValid = /.+@.+\..+/.test(email);
  if (!isValid) {
    emailError.value = "Enter valid email";
    return false;
  }

  return true;
}

function validateRoles() {
  roleError.value = "";

  if (!localRow.roleIds || localRow.roleIds.length === 0) {
    roleError.value = "Role is required";
    return false;
  }

  return true;
}

// =================================================================================
// dropdowns
// =================================================================================

const { siteRolesDropdown } = siteModule();

// =================================================================================
// Save
// =================================================================================

function onSave (scope) {
  const email = scope.value;    

  const isEmailValid = validateEmail(email);
  const isRoleValid = validateRoles();

  if (!isEmailValid || !isRoleValid) {
    return;
  }

  // if (!validateEmail(email)) {
  //   return; // stop save
  // }

  processing.value = true;

  const payload = {
    email: email,
    roleIds: localRow.roleIds
  };
  
  props.onSaveApi(payload)
    .then((shouldRefresh) => {
      scope.set();
      processing.value = false;

      if (shouldRefresh) {
        emit("success");
        processing.value = false;
      }
    })
    .catch(() => {
      processing.value = false;
    });
}

onMounted(() => {
  siteRolesDropdown.load();
});
</script>
