<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white q-mr-lg">{{ id ? "Edit" : "Add" }} Tag</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Tag Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="q-mb-xs text-black">Name<span class="required">*</span></div>
                  <div>
                    <q-input
                      v-model="model.name" outlined stack-label hide-bottom-space :dense="true"
                      :error="v$.name.$error" :error-message="v$.name.$errors[0]?.$message" @click="v$.name.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <!-- Background Color -->
                <div class="col-6">
                  <div class="q-mb-xs text-black">Background Color</div>
                  <q-input v-model="model.bgColor" class="my-input">
                    <template #append>
                      <q-icon name="o_colorize" class="cursor-pointer">
                        <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                          <q-color v-model="model.bgColor" no-header no-footer default-view="palette" @update:model-value="updateBgColor" />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
                <!-- Text Color -->
                <div class="col-6">
                  <div class="q-mb-xs text-black">Text Color</div>
                  <q-input v-model="model.color" class="my-input">
                    <template #append>
                      <q-icon name="o_colorize" class="cursor-pointer">
                        <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                          <q-color v-model="model.color" no-header no-footer default-view="palette" @update:model-value="updateColor" />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
              </div>
              <div v-if="shouldShowPreview" class="q-mt-md">
                <div class="text-subtitle1 q-mb-sm">Preview:</div>
                <div
                  class="q-pa-sm q-mb-sm"
                  :style="{
                    backgroundColor: model.bgColor,
                    color: model.color,
                    borderRadius: '4px', display: 'inline-block',
                    width: 'auto', whiteSpace: 'nowrap',
                    maxWidth: '100%'
                  }"
                >
                  {{ model.name }}
                </div>
              </div>
              <div align="center" class="q-gutter-sm justify-center q-mt-lg">
                <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
              </div>
            </fieldset>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import tagMasterService from "modules/tags/tags.service";
import { required, helpers } from "@vuelidate/validators";
import { ref, onMounted, watch, computed } from "vue";
import _ from "lodash";
import { notifySuccess } from "assets/utils";
const $emit = defineEmits(["hide", "ok"]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const tagId = props.id;
const loading = ref(true);
const processing = ref(false);

// Define emits
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "",
  bgColor: "",
  color: ""
});

// Validation rules
const rules = {
  name: { required: helpers.withMessage("Name is required", required) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const getTags = (tagId) => {
  loading.value = true;
  tagMasterService.getTags(tagId).then((resp) => {
    model.value = _.cloneDeep(resp);
    // console.log(resp);
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => tagId, (newValue, oldValue) => {
  if (newValue) {
    getTags(tagId);
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
});

const updateColor = (val) => {
  if (val.startsWith("#")) {
    model.value.color = val.toUpperCase();
  }
};

const updateBgColor = (val) => {
  if (val.startsWith("#")) {
    model.value.bgColor = val.toUpperCase();
  }
};

const onSubmit = async () => {
  if (await v$.value.$validate()) {
    processing.value = true;
    tagMasterService.saveTags(props.id, model.value).then((resp) => {
      notifySuccess({ message: "Tag is saved successfully." });
      $emit("ok");
      $emit("hide");
    }).finally(() => {
      processing.value = false;
    });
  }
};

// Only show preview if all required fields are filled
const shouldShowPreview = computed(() => {
  return model.value.name &&
         model.value.bgColor &&
         model.value.color;
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
