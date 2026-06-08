<template>
  <q-dialog ref="dialogRef" v-model="small" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 45vw !important;max-width: 45vw !important;">
      <q-card-section class="card-header bg-primary with-tool stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Menu</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-xs-12 col-sm-12 col-md-6">
                  <div class="form-group">
                    <div class="q-mb-xs text-black">Menu Name<span class="required">*</span></div>
                    <q-input
                      v-model="model.displayName" outlined stack-label hide-bottom-space :dense="true" maxlength="128" autofocus
                      :error="v$.displayName.$error" :error-message="v$.displayName.$errors[0]?.$message" @blur="v$.displayName.$touch" @change="onDisplayNameChange()"
                    />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-12 col-md-6">
                  <div class="form-group">
                    <div class="q-mb-xs text-black">Menu Prefix<span class="required">*</span></div>
                    <q-input
                      v-model="model.menuName" outlined stack-label hide-bottom-space :dense="true" maxlength="128" autofocus readonly
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md">
                <div class="col-xs-12 col-sm-12 col-md-6">
                  <div class="form-group">
                    <div class="q-mb-xs text-black">Sort Order<span class="required">*</span></div>
                    <q-input
                      v-model="model.sortorder" outlined stack-label hide-bottom-space :dense="true" maxlength="128" autofocus
                      :error="v$.sortorder.$error" :error-message="v$.sortorder.$errors[0]?.$message" @blur="v$.sortorder.$touch"
                    />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-12 col-md-6">
                  <div class="form-group">
                    <div class="q-mb-xs text-black">Module<span class="required">*</span></div>
                    <q-select
                      v-model="model.moduleId" clearable use-input outlined stack-label hide-bottom-space :options="modules" option-value="value" option-label="text" :dense="true" emit-value map-options
                      :error="v$.moduleId.$error" :error-message="v$.moduleId.$errors[0]?.$message" @blur="v$.moduleId.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md">
                <div class="col-xs-12 col-sm-12 col-md-6">
                  <div class="q-mb-xs text-black q-mt-md">Icon</div>
                  <q-input
                    v-model="model.icon" outlined stack-label hide-bottom-space :dense="true" maxlength="128" autofocus
                  />
                </div>
                <div class="col-xs-12 col-sm-12 col-md-6">
                  <div class="q-mb-xs text-black q-mt-md">Menu URL<span class="required">*</span></div>
                  <q-input
                    v-model="model.link" outlined stack-label hide-bottom-space :dense="true" maxlength="900" autofocus
                    :error="v$.link.$error" :error-message="v$.link.$errors[0]?.$message" @blur="v$.link.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md" style="margin-top: 20px;">
                <div class="col-xs-6 col-sm-6 col-md-2">
                  <div class="form-group">
                    <q-checkbox
                      v-model="model.active"
                      label="Active"
                      dense
                    />
                  </div>
                </div>
                <div class="col-xs-6 col-sm-6 col-md-10">
                  <div class="form-group">
                    <q-checkbox
                      v-model="model.isQuickLink"
                      label="Set as quick link"
                      dense
                    />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
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

const props = defineProps({ id: { type: String, default: "" }, moduleId: { type: String, default: "" } });

// common variables
const loading = ref(true);
const processing = ref(false);
const modules = ref([]);

// define model
const model = ref({
  menuName: "",
  displayName: "",
  sortorder: "",
  parentMenuId: "",
  link: "",
  moduleId: props.moduleId,
  icon: "",
  active: false,
  isQuickLink: false
});

const rules = {
  displayName: { required: helpers.withMessage("Menu name is required", required) },
  sortorder: { required: helpers.withMessage("Sortorder is required", required) },
  moduleId: { required: helpers.withMessage("Module is required", required) },
  link: { required: helpers.withMessage("Menu URL is required", required) }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const onDisplayNameChange = () => {
  if (!props.id) {
    model.value.menuName = "M-" + model.value.displayName.toLowerCase().trim().replace(/\s+/g, "-");
  }
};

const getMenu = () => {
  loading.value = true;
  moduleService.getMenu(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    if (resp.sitesModulesMenus && resp.sitesModulesMenus.length > 0) {
      model.value.isQuickLink = resp.sitesModulesMenus[0].isQuickLink;
    } else {
      model.value.isQuickLink = false;
    }
  }).finally(() => {
    loading.value = false;
  });
};

const getModules = () => {
  loading.value = true;
  moduleService.getModules().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    modules.value = responseData;
  }).finally(() => {
    loading.value = false;
  });
};

const onSubmit = async () => {
  if (await v$.value.$validate()) {
    processing.value = true;
    moduleService.saveMenu(props.id, model.value).then((resp) => {
      notifySuccess({ message: "Menu is saved successfully." });
      onDialogOK();
    }).finally(() => {
      processing.value = false;
    });
  }
};

watch(() => props.id, (newValue, oldValue) => {
  getModules();
  if (newValue) {
    getMenu();
  }
}, { immediate: true });

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
