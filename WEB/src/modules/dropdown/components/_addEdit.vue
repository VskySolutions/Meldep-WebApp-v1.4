<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 40vw !important;max-width: 40vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white q-mr-lg">{{ id ? "Edit" : "Add" }} Dropdown</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Dropdown Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md items-center q-py-sm">
                <div class="col-12">
                  <div class="q-mb-xs text-black">Dropdown Type<span class="required">*</span>
                  </div>
                  <div>
                    <q-select
                      v-model="model.dropDownTypeId"
                      clearable
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="dropDownTypeList"
                      option-value="value"
                      option-label="text"
                      emit-value map-options
                      :error="v$.dropDownTypeId.$error"
                      :error-message="v$.dropDownTypeId.$errors[0]?.$message"
                      @filter="getAllDropDownTypeListFilter"
                      @blur="v$.dropDownTypeId.$touch"
                    >
                      <template #option="{ itemProps, opt }">
                        <q-item v-bind="itemProps">
                          <q-item-section>
                            <div class="row q-col-gutter-x-md items-center">
                              <span>{{ opt.text }}</span>
                            </div>
                          </q-item-section>
                        </q-item>
                      </template>
                      <template #after><q-icon v-if="!readonlyProject && props.id == ''" name="o_add" color="primary" class="cursor-pointer q-ml-xs add-icon" @click="onAddDropDownType()">
                        <q-tooltip>Add new Dropdown Type</q-tooltip>
                      </q-icon>
                      </template>
                    </q-select>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                  <div class="q-mb-xs text-black">Dropdown Value<span class="required">*</span></div>
                  <div>
                    <q-input
                      v-model="model.dropDownValue"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :error="v$.dropDownValue.$error"
                      :error-message="v$.dropDownValue.$errors[0]?.$message"
                      @click="v$.dropDownValue.$touch"
                    />
                  </div>
                </div>
                <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                  <div class="q-mb-xs text-black">Dropdown Text</div>
                  <div>
                    <q-input
                      v-model="model.dropDownText"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                  <div class="q-mb-xs text-black">Sort Order</div>
                  <div>
                    <q-input
                      v-model="model.sortOrder" outlined stack-label hide-bottom-space :dense="true"
                    />
                  </div>
                </div>
                <div class="col-lg-6 col-md-6 col-sm-6 col-xs-6">
                  <div class="q-mb-xs q-mt-md text-black">Set As Active?</div>
                  <q-checkbox v-model="model.active" label="Active" :dense="true" />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="ccol-12 col-sm-12 col-md-12 col-lg-12">
                  <div class="q-mb-xs text-black">Description</div>
                  <div class="form-group">
                    <q-editor
                      v-model="model.description"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-6">
                  <div class="q-mb-xs text-black">Background Color</div>
                  <q-input v-model="model.bgColor" filled class="my-input">
                    <template #append>
                      <q-icon name="o_colorize" class="cursor-pointer">
                        <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                          <q-color v-model="model.bgColor" no-header no-footer default-view="palette" />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
                <div class="col-6">
                  <div class="q-mb-xs text-black">Text Color</div>
                  <q-input v-model="model.color" filled class="my-input">
                    <template #append>
                      <q-icon name="o_colorize" class="cursor-pointer">
                        <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                          <q-color v-model="model.color" no-header no-footer default-view="palette" />
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
                  {{ model.dropDownValue }}
                </div>
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
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import _ from "lodash";
import { notifySuccess, getLocalStorage } from "assets/utils";

import addDropdownType from "modules/dropdown/components/addEditDropdownType.vue";
import dropdownService from "modules/dropdown/dropdown.service";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const $q = useQuasar();

const loading = ref(true);
const processing = ref(false);
// Only show preview if all required fields are filled
const shouldShowPreview = computed(() => {
  return model.value.dropDownTypeId &&
         model.value.dropDownValue &&
         model.value.bgColor &&
         model.value.color;
});
// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel, onDialogOK } = useDialogPluginComponent();

// local storage values
const localStorageKey = "DropDown";
const filterLocalStorage = getLocalStorage(localStorageKey);
const dropDownTypeIds = filterLocalStorage ? filterLocalStorage.dropDownTypeIds[0] : [];

// Define model values
const model = ref({
  dropDownTypeId: dropDownTypeIds && dropDownTypeIds.length > 0 ? dropDownTypeIds : null,
  dropDownValue: "",
  dropDownText: "",
  description: "",
  bgColor: "",
  color: "",
  sortOrder: 0,
  active: false
});

// Validation rules
const rules = {
  dropDownValue: { required: helpers.withMessage("Dropdown Value is required", required), minLength: minLength(1), maxLength: maxLength(50) },
  dropDownTypeId: { required: helpers.withMessage("Dropdown Type is required", required) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// get person details on edit mode
const getDropDownValue = () => {
  loading.value = true;
  dropdownService.getDropDownValue(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    if (resp) {
      model.value.dropDownTypeId = resp.dropDownTypeId;
      model.value.dropDownValue = resp.dropDownValue;
      model.value.description = resp.description ? resp.description : "";
    }
  }).finally(() => {
    loading.value = false;
  });
};

// Get all dropdown type list for dropdown
const dropDownTypeList = ref([]);
const dropDownTypeFilter = ref([]);
function getAllDropDownTypeListForDropdown () {
  dropdownService.getAllDropDownTypeListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.type, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    dropDownTypeList.value = responseData;
    dropDownTypeFilter.value = responseData;
  });
}
// Search dropdown type for dropdown
function getAllDropDownTypeListFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      dropDownTypeList.value = dropDownTypeFilter.value;
    } else {
      dropDownTypeList.value = dropDownTypeFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Add new Drop Down Type popup
const onAddDropDownType = () => {
  $q.dialog({
    component: addDropdownType,
    componentProps: { }
  }).onOk(() => {
    getAllDropDownTypeListForDropdown();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Submit form
const onSubmit = async () => {
  if (await v$.value.$validate()) {
    processing.value = true;
    model.value.sortOrder = model.value.sortOrder ? model.value.sortOrder : 0;
    dropdownService.saveDropDown(props.id, model.value).then((resp) => {
      notifySuccess({ message: "DropDown is saved successfully." });
      // showPreview.value = true;
      onDialogOK();
    }).finally(() => {
      processing.value = false;
    });
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Rich Editor Tools: Fonts and Toolbar
// --------------------------------------------------------------------------------------------------------------------------------------------------

const fonts = {
  arial: "Arial",
  arial_black: "Arial Black",
  comic_sans: "Comic Sans MS",
  courier_new: "Courier New",
  impact: "Impact",
  lucida_grande: "Lucida Grande",
  times_new_roman: "Times New Roman",
  verdana: "Verdana"
};

const toolbar = [
  [
    {
      label: $q.lang.editor.align,
      icon: $q.iconSet.editor.align,
      fixedLabel: true,
      list: "only-icons",
      options: ["left", "center", "right", "justify"]
    }
  ],
  ["bold", "italic", "strike", "underline"],
  ["token", "hr", "link", "custom_btn"],
  [
    {
      label: $q.lang.editor.formatting,
      icon: $q.iconSet.editor.formatting,
      list: "no-icons",
      options: ["p", "h1", "h2", "h3", "h4", "h5", "h6", "code"]
    },
    "removeFormat"
  ],
  ["quote", "unordered", "ordered", "outdent", "indent"],
  ["undo", "redo"],
  ["viewsource"]
];

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getDropDownValue();
    // getAllDropDownTypeListForDropdown();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  getAllDropDownTypeListForDropdown();
});
</script>

<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
</style>
