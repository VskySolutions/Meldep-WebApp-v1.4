<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none  dialog-scrollable-content" full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 55vw !important;max-width: 55vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Lead</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Lead Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.personId"
                    label="Person"
                    :options="personNameDropdownSingleSelect.list.value"
                    :filter="personNameDropdownSingleSelect.filter"
                    :error="v$.personId.$error"
                    :error-message="v$.personId.$errors[0]?.$message"
                  >
                    <template #after>
                      <q-icon
                        name="fa-solid fa-user-plus"
                        color="primary"
                        class="cursor-pointer q-ml-sm"
                        @click="handleAddPerson(null, refreshPersonNameDropdown)"
                      >
                        <q-tooltip>Add new person</q-tooltip>
                      </q-icon>
                    </template>
                  </formSingleSelectDropdown>
                </div>
                <div class="col-12 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.companyId"
                    label="Company"
                    :required="false"
                    :options="companyNameDropdownSingleSelect.list.value"
                    :filter="companyNameDropdownSingleSelect.filter"
                  >
                    <template #after>
                      <q-icon
                        name="fa-solid fa-user-plus"
                        color="primary"
                        class="cursor-pointer q-ml-sm"
                       @click="onCompanyAdd(refreshCompanyNameDropdown)"
                      >
                        <q-tooltip>Add new company</q-tooltip>
                      </q-icon>
                    </template>
                  </formSingleSelectDropdown>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                  <formSingleSelectDropdown
                    v-model="model.leadGroupId"
                    label="Lead Group Name"
                    :options="leadGroupsDropdown.list.value"
                    :filter="leadGroupsDropdown.filter"
                    :error="v$.leadGroupId.$error"
                    :error-message="v$.leadGroupId.$errors[0]?.$message"
                  />
                  <formSingleSelectDropdown
                    v-model="model.leadSourceId"
                    label="Lead Source"
                    :options="leadSourceDropdown.list.value"
                    :filter="leadSourceDropdown.filter"
                    :error="v$.leadSourceId.$error"
                    :error-message="v$.leadSourceId.$errors[0]?.$message"
                  />
                <div class="col-12 col-md-4">
                  <div class="text-black">Lead Reference</div>
                  <q-input v-model="model.leadReference" outlined autogrow="" stack-label hide-bottom-space :dense="true" maxlength="128" rows="3" />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4">
                  <formDate
                    v-model="model.arrivalDateStr"
                    label="Lead Arrival Date"
                    :error="v$.arrivalDateStr.$error"
                    :error-message="v$.arrivalDateStr.$errors[0]?.$message"
                    :onBlur="() => v$.arrivalDateStr.$touch()"
                  />
                </div>
              </div>
              <div>
                <div class="col-12">
                  <div class="text-black">Lead Notes</div>
                  <q-editor
                    v-model="model.leadNote"
                    :dense="$q.screen.lt.md"
                    :toolbar="toolbar"
                    :fonts="fonts"
                  />
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-separator />
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
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import _ from "lodash";
import leadService from "modules/lead/lead.service";
import useFilters from "composables/useFilters";

import personModule from "src/modules/person/utils/dropdowns.js";
import companyModule from "src/modules/company/utils/dropdowns.js";
import leadModule from "src/modules/lead/utils/dropdowns.js";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// Shared Person Dialogs
import {
  initPersonDialogs,
  onPersonAddAndReturnPersonId
} from "src/modules/person/utils/dialogs.js";

// Shared Company Dialogs
import {
  initCompanyDialogs,
  onCompanyAdd
} from "src/modules/company/utils/dialogs.js";

// ----------------------------------------------------------------------------------------------------------------
// define emits
// ----------------------------------------------------------------------------------------------------------------

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------

const props = defineProps({ id: { type: String, default: "" } });
let leadId = props.id;

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const { toDate } = useFilters();
const loading = ref(true);
const processing = ref(false);
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);

// ----------------------------------------------------------------------------------------------------------------
// define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  leadNote: "",
  arrivalDateStr: ""
});

// ----------------------------------------------------------------------------------------------------------------
// Validation rules
// ----------------------------------------------------------------------------------------------------------------

const rules = {
  personId: { required: helpers.withMessage("Person is required", required) },
  leadSourceId: { required: helpers.withMessage("Lead Sources is required", required) },
  arrivalDateStr: { required: helpers.withMessage("Arrival Date is required", required) },
  leadGroupId: { required: helpers.withMessage("Lead Group is required", required) }
  // clientId: { required: helpers.withMessage("Client is required", required) },
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const getLead = () => {
  loading.value = true;
  leadService.getLead(leadId).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.arrivalDateStr = resp.leadArrivalDate ? toDate(resp.leadArrivalDate) : "";
  }).finally(() => {
    loading.value = false;
  });
};
// ----------------------------------------------------------------------------------------------------------------
// Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshPersonNameDropdown = () => {
  personNameDropdownSingleSelect.load();
}

const refreshCompanyNameDropdown = () => {
  companyNameDropdownSingleSelect.load();
}

const handleAddPerson = (row = null, refreshPersonNameDropdown) => {
  onPersonAddAndReturnPersonId(
    refreshPersonNameDropdown,
    (newPersonId) => {
      setTimeout(() => {
        if (row) {
          row.personId = newPersonId;
          getPersonById(newPersonId, row);
        } else {
          model.value.personId = newPersonId;
        }
      }, 100);
    }
  );
};

// ------------------------------------------------------------------------------------
// All Dropdowns
// ------------------------------------------------------------------------------------

const { personNameDropdownSingleSelect } = personModule();
const { companyNameDropdownSingleSelect } = companyModule();
const { leadSourceDropdown, leadGroupsDropdown } = leadModule();
// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initPersonDialogs(props.id);
initCompanyDialogs(props.id);

const onSubmit = async () => {
  try {
    if (await v$.value.$validate()) {
      processing.value = true;
      if (model.value.arrivalDateStr != null) {
        model.value.leadArrivalDate = model.value.arrivalDateStr;
      }
      leadService.saveLead(leadId, model.value).then((resp) => {
        notifySuccess({ message: "Lead is saved successfully." });
        leadId = resp;
        onDialogOK(leadId);
      }).finally(() => {
        processing.value = false;
      });
    }
  } catch (error) {
    console.error("Error in submitting the lead:", error);
    notifyError({ message: "An error occurred while saving the lead." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getLead();
  }
}, { immediate: true });

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  leadSourceDropdown.load("Lead Sources");
  personNameDropdownSingleSelect.load();
  companyNameDropdownSingleSelect.load();
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
