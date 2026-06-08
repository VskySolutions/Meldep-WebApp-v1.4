<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white q-mr-lg">Assign Users to Lead Groups</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Assignment Details</legend>
              <div class="row q-col-gutter-x-md">
                <formMultiSelectDropdown
                  v-model="model.userIds"
                  label="User Name"
                  required
                  :options="allUsersForDropdown.list.value"
                  :filter="allUsersForDropdown.filter"
                  wrapper-class="col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12"
                  :error="v$.userIds.$error"
                  :error-message="v$.userIds.$errors[0]?.$message"
                  :onBlur="() => v$.userIds.$touch()"
                  popup-content-class="customPopupContentClass"
                />
                <formMultiSelectDropdown
                  v-model="model.leadGroupIds"
                  label="Lead Group Name"
                  :options="leadGroupsDropdown.list.value"
                  :filter="leadGroupsDropdown.filter"
                  wrapper-class="col-xxl-6 col-lg-6 col-md-6 col-sm-6 col-xs-12"
                  :error="v$.leadGroupIds.$error"
                  :error-message="v$.leadGroupIds.$errors[0]?.$message"
                  :onBlur="() => v$.leadGroupIds.$touch()"
                  popup-content-class="customPopupContentClass"
                />
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import { useAuthStore } from "stores/auth";
import useVuelidate from "@vuelidate/core";
import { notifySuccess } from "assets/utils";
import { required, helpers } from "@vuelidate/validators";

import leadService from "modules/lead/lead.service";

// Shared Inputs
import formMultiSelectDropdown from "src/components/form-inputs/_formMultiSelectDropdown.vue";

// Shared Dropdowns
import userModule from "src/modules/user-management/utils/dropdowns.js";
import leadModule from "src/modules/lead/utils/dropdowns.js";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const processing = ref(false);
const authStore = useAuthStore();
const user = authStore.user;

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel, onDialogOK } = useDialogPluginComponent();

// Define model values
const model = ref({
  userIds: [],
  leadGroupIds: []
});

// Validation rules
const rules = {
  userIds: { required: helpers.withMessage("User is required", required) },
  leadGroupIds: { required: helpers.withMessage("Lead Group is required", required) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const { allUsersForDropdown } = userModule();
const { leadGroupsDropdown } = leadModule();

// Submit form
const onSubmit = async () => {
  if (await v$.value.$validate()) {
    processing.value = true;
    leadService.assignUserToLeadGroup(props.id, model.value).then((resp) => {
      notifySuccess({ message: "User assigned successfully." });
      onDialogOK();
    }).finally(() => {
      processing.value = false;
    });
  }
};

// On page rendering
onMounted(() => {
  allUsersForDropdown.load(user.siteId);
  leadGroupsDropdown.load("Lead Group");
});
</script>

<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
