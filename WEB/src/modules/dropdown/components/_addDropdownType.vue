<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 40vw !important;max-width: 40vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Dropdown Type</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Dropdown Type Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                    <div class="q-mb-xs text-black">Type<span class="required">*</span></div>
                    <div>
                      <q-radio
                        v-model="model.selectionType"
                        :val="true"
                        label="Group"
                      />

                      <q-radio
                        v-model="model.selectionType"
                        :val="false"
                        label="Single"
                      />
                    </div>
                  </div>
                  <div v-if="model.selectionType === true" class="col-12 col-sm-6 col-md-6 col-lg-6">
                    <div class="q-mb-xs text-black">Group Name</div>
                    <div>
                      <q-input
                        v-model="model.groupName"
                        outlined
                        dense
                        :error="v$.groupName.$error"
                        :error-message="v$.groupName.$errors[0]?.$message"
                        @click="v$.groupName.$touch"
                      />
                    </div>
                  </div>
                </div>
                <div class="col-12">
                  <div class="q-mb-xs text-black">Dropdown Type<span class="required">*</span></div>
                  <div>
                    <q-input
                      v-model="model.type"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :error="v$.type.$error"
                      :error-message="v$.type.$errors[0]?.$message"
                      @click="v$.type.$touch"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import dropdowntypeService from "modules/dropdown/dropdown.service";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref } from "vue";
import { notifySuccess } from "assets/utils";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const processing = ref(false);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Define model values
const model = ref({
  type: "",
  groupName: ""
});

// Validation rules
const rules = {
  type: { required: helpers.withMessage("Dropdown Type is required", required), minLength: minLength(1), maxLength: maxLength(100) },
  groupName: { required: helpers.withMessage("GroupName is required", required) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Submit form
const onSubmit = async () => {
  if (await v$.value.$validate()) {
    processing.value = true;
    const payload = {
      type: model.value.type,
      displayName: model.value.type
    };
    dropdowntypeService.saveDropDownType(props.id, payload).then((resp) => {
      notifySuccess({ message: "DropDown Type is saved successfully." });
      onDialogOK();
    }).finally(() => {
      processing.value = false;
    });
  }
};
</script>

<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
