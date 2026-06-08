<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 40vw !important;max-width: 40vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Module</div>
        <q-btn v-close-popup icon="o_close" class="close text-white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <q-card-section class="card-body scroll">
          <div class="row q-col-gutter-x-md">
            <div class="col-xs-12 col-sm-12 col-md-6">
              <div class="form-group">
                <div class="q-mb-xs text-black">Module Name<span class="required">*</span></div>
                <q-input
                  v-model="model.name" outlined stack-label hide-bottom-space :dense="false" maxlength="256" autofocus
                  :error="v$.name.$error" :error-message="v$.name.$errors[0]?.$message" @blur="v$.name.$touch"
                />
              </div>
            </div>
            <div class="col-xs-12 col-sm-12 col-md-6">
              <div class="form-group">
                <div class="q-mb-xs text-black">Sort Order<span class="required">*</span></div>
                <q-input
                  v-model="model.sortorder" outlined stack-label hide-bottom-space :dense="false" maxlength="256" autofocus
                  :error="v$.sortorder.$error" :error-message="v$.sortorder.$errors[0]?.$message" @blur="v$.sortorder.$touch"
                />
              </div>
            </div>
          </div>
        </q-card-section>
        <q-separator />
        <q-card-actions class="q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch } from "vue";
import _ from "lodash";
import moduleService from "modules/module/module.service";
import { notifySuccess } from "assets/utils";

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const loading = ref(true);
const processing = ref(false);
const model = ref({
  name: "",
  sortorder: ""
});

const props = defineProps({ id: { type: String, default: "" } });

const rules = {
  name: { required: helpers.withMessage("Module Name is required", required) },
  sortorder: { required: helpers.withMessage("Sortorder is required", required) }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const getModule = () => {
  loading.value = true;
  moduleService.getModule(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getModule();
  }
}, { immediate: true });

const onSubmit = async () => {
  if (await v$.value.$validate()) {
    processing.value = true;
    moduleService.saveModule(props.id, model.value).then((resp) => {
      notifySuccess({ message: "Module is saved successfully." });
      onDialogOK();
    }).finally(() => {
      processing.value = false;
    });
  }
};
</script>
