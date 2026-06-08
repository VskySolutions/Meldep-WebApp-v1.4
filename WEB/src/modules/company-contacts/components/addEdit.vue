<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 65vw !important; max-width: 65vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Contact</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <!-- <q-card class="card-header with-tools headerBasic"> -->
            <fieldset>
              <legend>Contact Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                <div class="col-xs-12 col-sm-6 col-md-4">
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
                        v-if="!readonlyEmployee"
                        name="fa-solid fa-user-plus"
                        color="primary"
                        class="cursor-pointer q-ml-sm"
                        @click="onPersonAdd(null, refreshPersonNameDropdown)"
                      >
                        <q-tooltip>Add new person</q-tooltip>
                      </q-icon>
                    </template>
                  </formSingleSelectDropdown>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <formSingleSelectDropdown
                    v-model="model.companyId"
                    label="Company"
                    :options="companyNameDropdownSingleSelect.list.value"
                    :filter="companyNameDropdownSingleSelect.filter"
                    :error="v$.companyId.$error"
                    :error-message="v$.companyId.$errors[0]?.$message"
                  >
                    <template #after>
                      <q-icon
                        v-if="!readonlyEmployee"
                        name="o_add"
                        color="primary"
                        class="cursor-pointer q-ml-sm add-icon"
                        @click="onCompanyAdd(refreshCompanyNameDropdown)"
                      >
                        <q-tooltip>Add new company</q-tooltip>
                      </q-icon>
                    </template>
                  </formSingleSelectDropdown>
                </div>
              </div>
              <div v-if="model.personId" class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                <div class="col-xs-12 col-sm-6 col-md-3">
                  <div class="q-mb-xs text-black">First Name: {{ model.firstName }}</div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-3">
                  <div class="q-mb-xs text-black">Middle Name: {{ model.middleName }}</div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-3">
                  <div class="q-mb-xs text-black">Last Name: {{ model.lastName }}</div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-3">
                  <div class="q-mb-xs text-black">Gender: {{ model.gender }}</div>
                </div>
              </div>
              <div v-if="model.personId" class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="q-mb-xs text-black">Email: {{ model.emailAddress }}</div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="q-mb-xs text-black">Phone Number: {{ model.phoneNumber }}</div>
                </div>
              </div>
              <div v-if="model.personId" class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="q-mb-xs text-black">Alt. Email</div>
                  <div>
                    <q-input
                      v-model="model.alternateEmail"
                      outlined
                      hide-bottom-space
                      :dense="true"
                      maxlength="128"
                      :error="v$.alternateEmail.$error"
                      :error-message="v$.alternateEmail.$errors[0]?.$message"
                      @blur="v$.alternateEmail.$touch"
                    />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="q-mb-xs text-black">Alt. Phone Number</div>
                  <div>
                    <q-input
                      v-model="model.alternatePhoneNumber"
                      outlined
                      hide-bottom-space
                      :dense="true"
                      maxlength="14"
                      mask="(###)-###-####"
                      :error="v$.alternatePhoneNumber.$error"
                      :error-message="v$.alternatePhoneNumber.$errors[0]?.$message"
                      @blur="v$.alternatePhoneNumber.$touch"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn v-if="tab !== '4_tab'" color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers, email } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import useFilters from "composables/useFilters";
import companyContactService from "modules/company-contacts/companyContacts.service";
import personService from "modules/person/person.service";
// import editPerson from "modules/person/components/addEdit.vue";
// import editCompany from "modules/company/components/addEdit.vue";
import _ from "lodash";

// Shared Dropdowns
import personModule from "src/modules/person/utils/dropdowns.js";
import companyModule from "src/modules/company/utils/dropdowns.js";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

// Shared Person Dialogs
import { 
  initPersonDialogs,
  onPersonAdd
} from "src/modules/person/utils/dialogs.js";

// Shared Company Dialogs
import { 
  initCompanyDialogs,
  onCompanyAdd
} from "src/modules/company/utils/dialogs.js";

// ----------------------------------------------------------------------------------------------------------------
// Define emits
// ----------------------------------------------------------------------------------------------------------------

const $emit = defineEmits(["hide", "ok"]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------

const props = defineProps({ id: { type: String, default: "" } });
// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

// const $q = useQuasar();
const { toDate } = useFilters();
const loading = ref(true);
const processing = ref(false);

// ----------------------------------------------------------------------------------------------------------------
// Define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  personId: "",
  companyId: "",
  firstName: "",
  lastName: "",
  emailAddress: "",
  gender: ""
});

// ----------------------------------------------------------------------------------------------------------------
// Validation rules
// ----------------------------------------------------------------------------------------------------------------

const rules = {
  personId: { required: helpers.withMessage("Person Name is required", required) },
  companyId: { required: helpers.withMessage("Company is required", required) },
  alternateEmail: { email: helpers.withMessage("Invalid email", email) },
  alternatePhoneNumber: {
    validLength: helpers.withMessage(
      "Invalid phone number",
      value => !value || value.length === 14
    )
  }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// ----------------------------------------------------------------------------------------------------------------
// Get Company contact details
// ----------------------------------------------------------------------------------------------------------------

const getCompany = () => {
  loading.value = true;
  companyContactService.getCompanyContactDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.personId = resp.person.id;
    model.value.createdById = resp.createdById;
    model.value.createdOnUtc = toDate(resp.createdOnUtc);
  }).finally(() => {
    loading.value = false;
  });
};

// ----------------------------------------------------------------------------------------------------------------
// Get person details
// ----------------------------------------------------------------------------------------------------------------

function getPersonbyId (PersonId) {
  personService.getPerson(PersonId).then((resp) => {
    model.value.personId = PersonId;
    model.value.firstName = resp.firstName;
    model.value.middleName = resp.middleName;
    model.value.lastName = resp.lastName;
    model.value.emailAddress = resp.primaryEmailAddress;
    model.value.phoneNumber = resp.primaryPhoneNumber;
    model.value.gender = resp.gender.dropDownValue;
  });
}

// ----------------------------------------------------------------------------------------------------------------
// Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshPersonNameDropdown = () => {
  personNameDropdownSingleSelect.load();
}

const refreshCompanyNameDropdown = () => {
  companyNameDropdownSingleSelect.load();
}

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initPersonDialogs(props.id);
initCompanyDialogs(props.id);

// ------------------------------------------------------------------------------------
// All Dropdowns
// ------------------------------------------------------------------------------------

const { personNameDropdownSingleSelect } = personModule();
const { companyNameDropdownSingleSelect } = companyModule();

async function onSubmit () {
  try {
    if (!await v$.value.$validate()) {
      return;
    }
    processing.value = true;

    const payload = {
      personId: model.value.personId,
      companyId: model.value.companyId,
      alternateEmail: model.value.alternateEmail,
      alternatePhoneNumber: model.value.alternatePhoneNumber,
      alternativePhoneNumber: model.value.alternativePhoneNumber,
      createdById: model.value.createdById,
      createdOnUtc: model.value.createdOnUtc
    };
    companyContactService.saveCompanyContacts(props.id, payload).then(resp => {
      notifySuccess({ message: "Company contact saved successfully." });
      $emit("ok");
      $emit("hide");
    // router.push({ name: "Company", params: {} });
    });
  } catch (error) {
    console.error("Error in submitting the company contact:", error);
    notifyError({ message: "An error occurred while saving the company contact." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

// Create popup
// const onAddPerson = () => {
//   $q.dialog({
//     component: editPerson,
//     componentProps: {}
//   }).onOk(() => {
//     personNameDropdownSingleSelect.load();
//   }).onCancel(() => {
//   }).onDismiss(() => {
//   });
// };

// const onAddCompany = () => {
//   $q.dialog({
//     component: editCompany,
//     componentProps: {}
//   }).onOk(() => {
//     companyNameDropdownSingleSelect.load();
//   }).onCancel(() => {
//   }).onDismiss(() => {
//   });
// };

if (model.value.personId !== null) {
  watch(() => model.value.personId, (newValue, oldValue) => {
    if (newValue) {
      getPersonbyId(newValue);
    }
  }, { immediate: false });
}

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getCompany();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  personNameDropdownSingleSelect.load();
  companyNameDropdownSingleSelect.load();
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
