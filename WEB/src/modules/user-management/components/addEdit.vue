<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 65vw; max-width: 65vw;">
      <q-card-section class="card-header with-tools bg-primary">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} User</div>
        <q-btn v-close-popup icon="o_close" color="white" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>User Info</legend>
              <div v-if="!id" class="row q-mb-md">
                <div class="col-12 col-md-4">
                  <div class="q-gutter-sm">
                    <q-radio v-model="model.isEmployeeOrClient" dense val="employee" label="Employee" @click="clearField()" />
                    <q-radio v-model="model.isEmployeeOrClient" dense val="customer-contact" label="Customer Contacts" :class="{ hidden: model.showCheckbox }" @click="clearField()" />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md">
                <div v-if="model.isEmployeeOrClient === 'employee' && !id" class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Employee <span class="required">*</span></div>
                  <div class="row no-wrap items-center">
                    <q-select
                      v-model="model.employeeId" clearable use-input outlined stack-label hide-bottom-space :dense="true" class="col-grow" :options="employeeList" option-value="value" option-label="text" emit-value map-options
                      :error="v$.employeeId.$error" :error-message="v$.employeeId.$errors[0]?.$message" @blur="v$.employeeId.$touch" @filter="filterFn1"
                    />
                    <q-icon name="fa-solid fa-user-plus" color="primary" class="cursor-pointer q-ml-md q-mb-md" size="sm" @click="onAddEmployee"><q-tooltip>Add New Employee</q-tooltip></q-icon>
                  </div>
                </div>
                <div v-if="model.isEmployeeOrClient === &quot;customer-contact&quot; && !id" class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Customer Contacts <span class="required">*</span></div>
                  <div class="row no-wrap items-center">
                    <q-select
                      v-model="model.personId" clearable use-input outlined stack-label hide-bottom-space :dense="true" class="col-grow" :options="contactList" option-value="value" option-label="text" emit-value map-options
                      :error="v$.personId.$error" :error-message="v$.personId.$errors[0]?.$message" @blur="v$.personId.$touch" @filter="filterFn2"
                    />
                    <q-icon name="fa-solid fa-user-plus" color="primary" class="cursor-pointer q-ml-md q-mb-md" size="sm" @click="onAddCompanyContact"><q-tooltip>Add New Company Contact</q-tooltip></q-icon>
                  </div>
                </div>
              </div>
              <div v-if="model.employeeId || model.personId" class="row q-col-gutter-x-md q-mb-md q-pb-sm q-pt-md">
                <div class="col-12 col-md-3">
                  <div class="q-mb-xs text-black">First Name: {{ model.firstName }}</div>
                </div>
                <div class="col-12 col-md-3">
                  <div class="q-mb-xs text-black">Last Name: {{ model.lastName }}</div>
                </div>
                <div v-if="model.employeeId && !id" class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Office Email: {{ model.officialEmail }}</div>
                </div>
                <div v-if="model.personId && !id" class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Email: {{ model.email }}</div>
                </div>
                <div v-if="id" class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Email: {{ model.userEmail }}</div>
                </div>
              </div>
              <div v-if="model.employeeId || model.personId" class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Username <span class="required">*</span></div>
                  <div>
                    <q-input v-model="model.username" :readonly="readonlyUser!= '' ? '' : 'readonlyUser'" outlined hide-bottom-space :dense="true" maxlength="128" :error="v$.username.$error" :error-message="v$.username.$errors[0]?.$message" @blur="v$.username.$touch" />
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Password <span v-if="!id" class="required">*</span></div>
                  <div>
                    <q-input v-model="model.password" outlined hide-bottom-space :dense="true" maxlength="20" :error="v$.password.$error" :error-message="v$.password.$errors[0]?.$message" :type="isPassword ? 'password' : 'text'" @blur="v$.password.$touch">
                      <template #append>
                        <q-icon :name="isPassword ? 'o_visibility_off' : 'o_visibility'" class="cursor-pointer" @click="isPassword = !isPassword" />
                      </template>
                    </q-input>
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Roles <span class="required">*</span></div>
                  <div class="form-group">
                    <q-select
                      v-model="model.siteRoleIds" push class="w-100 h-auto" clearable use-input use-chips transition-show="jump-up"
                      transition-hide="jump-up" outlined hide-bottom-space :dense="true" multiple fill-input input-debounce="0"
                      :options="siteRolesList" option-value="value" option-label="text" emit-value map-options :error="v$.siteRoleIds.$error"
                      :error-message="v$.siteRoleIds.$errors[0]?.$message" @filter="getAllSitesRoleListForFilter" @blur="v$.siteRoleIds.$touch"
                    >
                      <template #option="{ itemProps, opt, selected, toggleOption }">
                        <q-item v-bind="itemProps">
                          <q-item-section>
                            <div class="row q-col-gutter-x-md items-center">
                              <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                              <span>{{ opt.text }}</span>
                            </div>
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
                  </div>
                </div>
              </div>
              <div v-if="model.employeeId || model.personId" class="row q-col-gutter-x-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="q-mb-xs text-black">Send Email ?<q-checkbox v-model="model.sendEmail" :dense="true" class="q-ml-sm" /></div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-separator />
        <q-card-actions align="center" class="q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn label="Save" type="submit" color="primary" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import _ from "lodash";
import usersService from "modules/user-management/userManagement.service";
import sitesService from "modules/sites/site.service";
import { notifySuccess, notifyError } from "assets/utils";
import employeesService from "modules/employee/employee.service";
import customerService from "src/modules/customer/customer.service";
import personService from "modules/person/person.service";
import editEmployee from "modules/employee/components/addEdit.vue";
import editCompany from "modules/company/components/addEdit.vue";

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();
const loading = ref(true);
const processing = ref(false);
const isPassword = ref(true);
const $q = useQuasar();
const selectedSiteId = ref(history.state?.siteId);

const model = ref({
  username: "",
  firstName: "",
  lastName: "",
  email: "",
  phoneNumber: "",
  siteRoleIds: [],
  active: true,
  isEmployeeOrClient: "employee",
  password: "",
  sendEmail: false
});

const props = defineProps({ id: { type: String, default: "" } });
const readonlyUser = props.id ? "readonly" : "";
const passwordValidation = computed(() => {
  const isEdit = !!props.id;

  return {
    // Required only for create page
    ...(isEdit ? {} : { required: helpers.withMessage("Password is required", required) }),

    // Other validations applied only if user types something
    minLength: helpers.withMessage(
      "Password must be at least 8 characters",
      (value) => !value || value.length >= 8
    ),
    containsLowerCase: helpers.withMessage(
      "Password must contain a lowercase character",
      (value) => !value || /[a-z]/.test(value)
    ),
    containsUpperCase: helpers.withMessage(
      "Password must contain an uppercase character",
      (value) => !value || /[A-Z]/.test(value)
    ),
    containsNumber: helpers.withMessage(
      "Password must contain a number",
      (value) => !value || /[0-9]/.test(value)
    ),
    containsSpecialChar: helpers.withMessage(
      "Password must contain a special character",
      (value) => !value || /[#?!@$%^&*-]/.test(value)
    )
  };
});

// Conditionally set validation rules
const rules = computed(() => {
  const isEmployee = model.value.isEmployeeOrClient === "employee";
  const isCustomerContact = model.value.isEmployeeOrClient === "customer-contact";
  return {
    // password: { required: helpers.withMessage("Password is required", required), minLength: minLength(8), containsLowerCase: helpers.withMessage(() => "The password must contain a lowercase character", (value) => /[a-z]/.test(value)), containsUppercase: helpers.withMessage(() => "The password must contain an uppercase character", (value) => /[A-Z]/.test(value)), containsNumber: helpers.withMessage(() => "The password must contain a number", (value) => /[0-9]/.test(value)), containsSpecialCharacter: helpers.withMessage(() => "The password must contain special character", (value) => /[#?!@$%^&*-]/.test(value)) },
    username: { required: helpers.withMessage("Username is required", required) },
    password: passwordValidation,
    siteRoleIds: { required: helpers.withMessage("Role is required", required) },
    employeeId: isEmployee ? { required: helpers.withMessage("Employee is required", required) } : {},
    personId: isCustomerContact ? { required: helpers.withMessage("Contact is required", required) } : {}
  };
});

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const getUser = () => {
  loading.value = true;
  usersService.getUser(props.id, selectedSiteId.value).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.email = resp.email;
    model.value.userEmail = resp.email;
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getUser();
  }
}, { immediate: true });

onMounted(() => {
  getAllSitesRoleListForDropdown(selectedSiteId.value);
  // getRoles();
  getAllEmployeesListForDropdown(selectedSiteId.value);
  getAllCustomerContactListForDropdown(selectedSiteId.value);
});

// function getRoles () {
//   roleService.getRoles().then((resp) => {
//     roles.value = resp.data;
//   });
// }

const siteRolesList = ref([]);
const siteRolesListFilter = ref([]);
function getAllSitesRoleListForDropdown (siteId) {
  sitesService.getAllSitesRoleListForDropdown(siteId).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.applicationRole.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    siteRolesList.value = responseData;
    siteRolesListFilter.value = responseData;
  });
}

// Search employee for dropdown
function getAllSitesRoleListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      siteRolesList.value = siteRolesListFilter.value;
    } else {
      siteRolesList.value = siteRolesListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const employeeList = ref([]);
const options1 = ref([]);
function getAllEmployeesListForDropdown (siteId) {
  employeesService.getAllActiveEmployeesListForDropdown(siteId).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    employeeList.value = responseData;
    options1.value = responseData;
  });
}

// Search employee for dropdown
function filterFn1 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = options1.value;
    } else {
      employeeList.value = options1.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const contactList = ref([]);
const options2 = ref([]);
function getAllCustomerContactListForDropdown (siteId) {
  customerService.getAllCustomerContactListForDropdown(siteId).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.person.id })).sort((a, b) => a.text.localeCompare(b.text));
    contactList.value = responseData;
    options2.value = responseData;
  });
}

// Search employee for dropdown
function filterFn2 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      contactList.value = options2.value;
    } else {
      contactList.value = options2.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

function getEmployeebyId (employeeId) {
  employeesService.getEmployee(employeeId).then((resp) => {
    model.value.employeeId = employeeId;
    model.value.firstName = resp.person.firstName;
    model.value.middleName = resp.person.middleName;
    model.value.lastName = resp.person.lastName;
    model.value.emailAddress = resp.person.primaryEmailAddress;
    model.value.phoneNumber = resp.person.primaryPhoneNumber;
    model.value.officialEmail = resp.officialEmail;
  });
}

if (model.value.employeeId !== null) {
  watch(() => model.value.employeeId, (newValue, oldValue) => {
    if (newValue) {
      getEmployeebyId(newValue);
    }
  }, { immediate: false });
}
function getPersonbyId () {
  personService.getPerson(model.value.personId).then((resp) => {
    model.value.firstName = resp.firstName;
    model.value.middleName = resp.middleName;
    model.value.lastName = resp.lastName;
    model.value.genderId = resp.gender.dropDownValue;
    model.value.primaryEmailAddress = resp.primaryEmailAddress;
    model.value.email = resp.email;
    model.value.countryId = resp.address.addressCountry.name;
    model.value.primaryPhoneNumber = resp.primaryPhoneNumber;
    model.value.virtualPath = resp.picture.virtualPath ? resp.picture.virtualPath : "";
    model.value.addressTypeId = resp.addressType.dropDownValue;
    model.value.addressLine1 = resp.address.addressLine1;
    model.value.addressLine2 = resp.address.addressLine2;
    model.value.city = resp.address.city;
    model.value.zipCode = resp.address.zipCode;
    model.value.stateProvinceId = resp.address.addressStateProvince.name;
  });
}

if (model.value.personId !== null) {
  watch(() => model.value.personId, (newValue, oldValue) => {
    if (newValue) {
      getPersonbyId(newValue);
    }
  }, { immediate: false });
}

const onSubmit = async () => {
  processing.value = true;
  try {
    if (await v$.value.$validate()) {
      model.value.employeeEmail = model.value.officialEmail;
      model.value.email = model.value.primaryEmailAddress;
      model.value.siteId = selectedSiteId.value;
      usersService.saveUser(props.id, model.value).then((resp) => {
        notifySuccess({ message: "User is saved successfully." });
        onDialogOK();
      });
    }
  } catch (error) {
    console.error("Error in submitting the user:", error);
    notifyError({ message: "An error occurred while saving the user." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

function clearField () {
  model.value.firstName = "";
  model.value.lastName = "";
  model.value.email = "";
  model.value.officialEmail = "";
  model.value.username = "";
  model.value.password = "";
  model.value.siteRoleIds = [];
  model.value.employeeId = "";
  model.value.personId = "";
}

// Create popup
const onAddEmployee = () => {
  $q.dialog({
    component: editEmployee,
    componentProps: {}
  }).onOk(() => {
    getAllEmployeesListForDropdown(selectedSiteId.value);
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Create Company popup
const onAddCompanyContact = () => {
  $q.dialog({
    component: editCompany,
    componentProps: {
      siteId: selectedSiteId.value
    }
  }).onOk(() => {
    getAllCustomerContactListForDropdown(selectedSiteId.value);
  }).onCancel(() => {
  }).onDismiss(() => {
  });
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
