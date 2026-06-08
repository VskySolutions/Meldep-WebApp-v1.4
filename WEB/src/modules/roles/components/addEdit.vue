<template>
  <q-dialog ref="dialogRef" v-model="small" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 40vw !important;max-width: 40vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Role</div>
        <q-btn v-close-popup icon="o_close" color="white" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable']">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Role Info</legend>
              <div class="row q-col-gutter-x-md">
                <div class="col-12 col-sm-12 col-md-12 col-lg-6">
                  <div class="form-group">
                    <div class="q-mb-xs text-black">Role Name<span class="required">*</span></div>
                    <q-input
                      v-model="model.name"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      autogrow
                      :error="v$.name.$error"
                      :error-message="v$.name.$errors[0]?.$message"
                      @blur="v$.name.$touch"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-separator />
        <q-card-actions class="q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" no-caps class="text-grey-9 actionBtn" @click="onDialogCancel" />
          <q-btn color="primary" label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers, maxLength } from "@vuelidate/validators";
import { ref, watch } from "vue";
import _ from "lodash";
import roleService from "modules/roles/role.service";
import { notifySuccess } from "assets/utils";

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const loading = ref(true);
const processing = ref(false);

const model = ref({
  name: ""
});

const props = defineProps({ id: { type: String, default: "" } });

const rules = {
  name: {
    required: helpers.withMessage("Role name is required", required),
    alphabeticChar: helpers.withMessage("Role name should contain only alphabets", value => /^[A-Za-z\s-]+$/.test(value)),
    maxLength: helpers.withMessage("Role name must not exceed 100 characters", maxLength(100))
  }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const getMasterRole = () => {
  loading.value = true;
  roleService.getMasterRole(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

const onSubmit = async () => {
  if (await v$.value.$validate()) {
    processing.value = true;
    roleService.saveMasterRole(props.id, model.value).then((resp) => {
      notifySuccess({ message: "Role is saved successfully." });
      onDialogOK();
    }).finally(() => {
      processing.value = false;
    });
  }
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getMasterRole();
  }
}, { immediate: true });
</script>
