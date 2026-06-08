<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw !important;max-width: 60vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Sites</div>
        <q-btn v-close-popup icon="o_close" class="close text-white" flat round dense @click="onDialogClose()" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Site Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Site Name<span class="required">*</span></label>
                  <div class="form-group q-mt-sm">
                    <q-input
                      v-model="model.name" outlined stack-label hide-bottom-space :dense="false" maxlength="128"
                      :error="v$.name.$error" :error-message="v$.name.$errors[0]?.$message" @blur="v$.name.$touch"
                    />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="label q-mb-xs text-black q-mt-sm">Logo file</div>
                  <div v-if="!model.siteLogoId" class="col">
                    <div class="form-group">
                      <q-uploader
                        ref="documentUploaderRef" color="white" text-color="dark" with-credentials hide-upload-btn field-name="file"
                        flat bordered label="Drag file here or (+) to upload. (image)" @uploaded="onUploaded" @added="onFileAdded"
                      />
                    </div>
                  </div>
                  <div v-if="model.siteLogoId" class="row justify-center">
                    <img v-if="model.siteLogoId" :src="model.siteLogoPath" alt="" style="width: 30%">
                  </div>
                  <div v-if="model.siteLogoId" class="row justify-center q-mt-sm">
                    <q-btn color="primary" label="Remove" outline no-caps @click="clearImage" />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="label q-mb-xs text-black q-mt-sm">Favicon Icon</div>
                  <div>
                    <div v-if="!model.siteFaviconId" class="col">
                      <div class="form-group">
                        <q-uploader
                          ref="documentUploaderRef" color="white" text-color="dark" with-credentials hide-upload-btn field-name="file"
                          flat bordered label="Drag file here or (+) to upload. (image)" @uploaded="onIconUploaded" @added="onFileIconAdded"
                        />
                      </div>
                    </div>
                    <div v-if="model.siteFaviconId" class="row justify-center">
                      <img :src="model.siteFaviconPath" alt="" style="width: 30%">
                    </div>
                    <div v-if="model.siteFaviconId" class="row justify-center q-mt-sm">
                      <q-btn color="primary" label="Remove" outline no-caps @click="clearIcon" />
                    </div>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="label q-mb-xs text-black q-mt-sm">Site Activation</div>
                  <q-checkbox v-model="model.active" label="Active" :dense="true" />
                </div>
                <formSingleSelectDropdown
                  v-model="model.timeZone"
                  label="Time Zone"
                  required
                  :options="timeZoneDropdownSingleSelect.list.value"
                  :filter="timeZoneDropdownSingleSelect.filter"
                  :error="v$.timeZone.$error"
                  :error-message="v$.timeZone.$errors[0]?.$message"
                />
              </div>
            </fieldset>
            <fieldset>
              <legend>Site Address</legend>
              <div class="row q-col-gutter-x-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="form-group q-mt-sm">
                    <label class="label q-mb-xs text-black">Country</label>
                    <q-select
                      v-model="model.countryId" clearable use-input outlined hide-bottom-space :dense="true"
                      :options="countryList" option-value="id" option-label="name" emit-value map-options @filter="filterFn2"
                    >
                      <template #option="{ itemProps, opt }">
                        <q-item v-bind="itemProps">
                          <q-item-section>
                            <div class="row q-col-gutter-x-md items-center">
                              <span>{{ opt.name }}</span>
                            </div>
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="form-group q-mt-sm">
                    <label class="label q-mb-xs text-black">State</label>
                    <q-select
                      v-model="model.stateProvinceId" clearable use-input outlined hide-bottom-space :dense="true"
                      :options="stateList" option-value="id" option-label="name" emit-value map-options @filter="filterFn3"
                    >
                      <template #option="{ itemProps, opt }">
                        <q-item v-bind="itemProps">
                          <q-item-section>
                            <div class="row q-col-gutter-x-md items-center">
                              <span>{{ opt.name }}</span>
                            </div>
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="form-group q-mt-sm">
                    <label class="label q-mb-xs text-black">City</label>
                    <q-input v-model="model.city" outlined stack-label hide-bottom-space :dense="false" maxlength="128" />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="form-group q-mt-sm">
                    <label class="label q-mb-xs text-black">Address Line 1</label>
                    <q-input v-model="model.addressLine1" outlined stack-label hide-bottom-space :dense="false" maxlength="64" />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="form-group q-mt-sm">
                    <label class="label q-mb-xs text-black">Address Line 2</label>
                    <q-input v-model="model.addressLine2" outlined stack-label hide-bottom-space :dense="false" maxlength="64" />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="text-black q-mt-sm">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }}<span class="required">*</span></div>
                  <div v-if="model.countryId === baseCountryId" class="form-group">
                    <q-input
                      v-model="model.zipCode" outlined stack-label hide-bottom-space :dense="true" mask="#####"
                      :error="v$.zipCode.$error" :error-message="v$.zipCode.$errors[0]?.$message" @click="v$.zipCode.$touch"
                    />
                  </div>
                  <div v-else class="form-group">
                    <q-input
                      v-model="model.zipCode" outlined stack-label hide-bottom-space :dense="true" mask="######"
                      :error="v$.zipCode.$error" :error-message="v$.zipCode.$errors[0]?.$message" @click="v$.zipCode.$touch"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset>
              <legend>Site Super Admin</legend>
              <div class="row q-col-gutter-x-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="form-group q-mt-sm">
                    <label class="label q-mb-xs text-black">Person Name<span class="required">*</span></label>
                    <q-select
                      v-model="model.personId" clearable use-input outlined stack-label hide-bottom-space :dense="true" :disable="!!props.id"
                      :options="personList" option-value="value" option-label="text" emit-value map-options :error="v$.personId.$error" :error-message="v$.personId.$errors[0]?.$message" @blur="v$.personId.$touch" @filter="filterFn1"
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
                      <template v-if="!props.id" #after><q-icon name="fa-solid fa-user-plus" color="primary" class="cursor-pointer q-ml-sm q-mt-sm" @click="onAddPerson()">
                        <q-tooltip>Add New Person</q-tooltip>
                      </q-icon>
                      </template>
                    </q-select>
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="form-group q-mt-sm">
                    <label class="label q-mb-xs text-black">Email Address</label>
                    <q-input
                      v-model="model.primaryEmailAddress" outlined stack-label hide-bottom-space :dense="false" maxlength="128" disable
                    />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="form-group q-mt-sm">
                    <label class="label q-mb-xs text-black">Phone Number</label>
                    <q-input v-model="model.primaryPhoneNumber" outlined stack-label hide-bottom-space :dense="false" mask="(###)-###-####" disable />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset>
              <legend>Site Roles</legend>
              <div class="row q-col-gutter-x-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="form-group q-mt-sm">
                    <label class="label q-mb-xs text-black">Roles<span class="required">*</span></label>
                    <q-select
                      v-model="model.roleIds" push class="w-100 h-auto" clearable use-input use-chips transition-show="jump-up"
                      transition-hide="jump-up" outlined hide-bottom-space :dense="true" multiple fill-input input-debounce="0"
                      :options="roles" option-value="id" option-label="name" emit-value map-options :error="v$.roleIds.$error" :error-message="v$.roleIds.$errors[0]?.$message" @blur="v$.roleIds.$touch" @filter="filterFn4"
                    >
                      <template #option="{ itemProps, opt, selected, toggleOption }">
                        <q-item v-bind="itemProps">
                          <q-item-section>
                            <div class="row q-col-gutter-x-md items-center">
                              <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                              <span>{{ opt.name }}</span>
                            </div>
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset v-if="!props.id">
              <legend>User Name</legend>
              <div class="row q-col-gutter-x-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="form-group q-mt-sm">
                    <label class="label q-mb-xs text-black">User Name<span class="required">*</span></label>
                    <q-input
                      v-model="model.userName" outlined stack-label hide-bottom-space :dense="false" maxlength="16" autocomplete="off"
                      :error="v$.userName.$error" :error-message="v$.userName.$errors[0]?.$message" @blur="v$.userName.$touch"
                    />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="form-group q-mt-sm">
                    <div class="text-black">Password</div>
                    <div>
                      <q-input v-model="model.password" outlined hide-bottom-space :dense="true" maxlength="20" :error="v$.password.$error" :error-message="v$.password.$errors[0]?.$message" hint="If no password is provided, the system will automatically generate a password." :type="isPassword ? 'password' : 'text'" @blur="v$.password.$touch">
                        <template #append>
                          <q-icon :name="isPassword ? 'o_visibility_off' : 'o_visibility'" class="cursor-pointer" @click="isPassword = !isPassword" />
                        </template>
                      </q-input>
                    </div>
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div class="q-mb-xs q-mt-lg text-black">Send Email ?<q-checkbox v-model="model.sendEmail" :dense="true" class="q-ml-sm" /></div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogClose" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, requiredIf, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import useFilters from "composables/useFilters";
import { notifySuccess, notifyError, zwConfirm } from "assets/utils";
import _ from "lodash";

import editPerson from "modules/person/components/addEdit.vue";

import sitesService from "modules/sites/site.service";
import commonService from "services/common.service";
import personService from "modules/person/person.service";
import roleService from "modules/roles/role.service";

// SOP Change :- Shared Dropdowns
import siteModule from "src/modules/sites/utils/dropdowns.js";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

const { toDate } = useFilters();
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogOK, onDialogHide } = useDialogPluginComponent();
const loading = ref(true);
const processing = ref(false);
const $q = useQuasar();
const baseCountryId = process.env.BASE_COUNTRY_ID;

const documentUploaderRef = ref(null);
const isPassword = ref(true);
const model = ref({
  name: "",
  personId: null,
  primaryPhoneNumber: "",
  primaryEmailAddress: "",
  addressLine1: "",
  addressLine2: "",
  countryId: baseCountryId,
  stateProvinceId: null,
  active: true,
  siteLogoId: null,
  siteFaviconId: null,
  userName: "",
  city: "",
  zipCode: "",
  siteLogoPath: "",
  siteFaviconPath: "",
  roleIds: [],
  password: "",
  sendEmail: false
});

const passwordValidation = computed(() => {
  if (model.value.password) {
    return {
      minLength: minLength(8),
      containsLowerCase: helpers.withMessage(
        () => "The password must contain a lowercase character",
        (value) => /[a-z]/.test(value)
      ),
      containsUppercase: helpers.withMessage(
        () => "The password must contain an uppercase character",
        (value) => /[A-Z]/.test(value)
      ),
      containsNumber: helpers.withMessage(
        () => "The password must contain a number",
        (value) => /[0-9]/.test(value)
      ),
      containsSpecialCharacter: helpers.withMessage(
        () => "The password must contain special character",
        (value) => /[#?!@$%^&*-]/.test(value)
      )
    };
  } else {
    return {};
  }
});

const props = defineProps({ id: { type: String, default: "" } });
let maxLengthZip = baseCountryId === model.value.countryId ? "5" : "6";
let rules = {
  name: { required: helpers.withMessage("Site Name is required", required) },
  timeZone: { required: helpers.withMessage("Time Zone is required", required) },
  personId: { required: helpers.withMessage("Person Name is required", required) },
  zipCode: { minLength: minLength(maxLengthZip), maxLength: maxLength(maxLengthZip), required: helpers.withMessage(maxLengthZip === "5" ? "Zip code is required" : "Pin code is required", required) },
  roleIds: { required: helpers.withMessage("Role is required", required) },
  userName: {
    required: helpers.withMessage("User Name is required", requiredIf(() => !props.id))
  },
  password: passwordValidation
};

let v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const getOrganization = () => {
  loading.value = true;
  sitesService.getOrganization(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    if (resp.address) {
      model.value.addressLine1 = resp.address.addressLine1;
      model.value.addressLine2 = resp.address.addressLine2;
      model.value.countryId = resp.address.countryId ? resp.address.countryId : "";
      model.value.stateProvinceId = resp.address.stateProvinceId;
      model.value.city = resp.address.city;
      model.value.zipCode = resp.address.zipCode;
    }
    model.value.roleIds = resp.sitesRoles.map(mapping => mapping.roleId);
    if (resp.subscriptionStartDate) {
      resp.subscriptionStartDate = toDate(resp.subscriptionStartDate);
    }

    if (resp.subscriptionEndDate) {
      resp.subscriptionEndDate = toDate(resp.subscriptionEndDate);
    }
  }).finally(() => {
    loading.value = false;
  });
};

function addNewPersonToDropdown (personId) {
  if (personId) {
    personService.getPerson(personId).then((resp) => {
      const fullText = `${resp.fullName}${resp.primaryEmailAddress ? ` (${resp.primaryEmailAddress})` : ""}`;
      const newOption = {
        text: fullText,
        value: resp.id
      };
      options1.value.push(newOption);
      model.value.personId = resp.id;
    });
  }
}

// Get dropdown list for countries
const countryList = ref([]);
const options2 = ref([]);
function getCountryList () {
  commonService.getCountries().then((resp) => {
    countryList.value = resp;
    options2.value = resp;
    // model.value.countryId = resp[1].id;
  });
}
function filterFn2 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      countryList.value = options2.value;
    } else {
      countryList.value = options2.value.filter(v => v.name.toLowerCase().includes(needle));
    }
  });
}
// Get dropdown list for states
const stateList = ref([]);
const options3 = ref([]);
function getStates () {
  commonService.getStates(model.value.countryId).then((resp) => {
    stateList.value = resp;
    options3.value = resp;
  });
}
function filterFn3 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      stateList.value = options3.value;
    } else {
      stateList.value = options3.value.filter(v => v.name.toLowerCase().includes(needle));
    }
  });
}

function clearImage () {
  zwConfirm({ message: "Do you want to clear this Logo ?" }, () => {
    model.value.siteLogoId = null;
    model.value.changeFlag = "remove";
    model.value.fileChangeFlag = "remove";
  }, () => {
  });
}

function clearIcon () {
  zwConfirm({ message: "Do you want to clear this Icon ?" }, () => {
    model.value.siteFaviconId = null;
    model.value.changeFlag = "remove";
    model.value.fileIconChangeFlag = "remove";
  }, () => {
  });
}

function onUploaded (info) {
  notifySuccess({ message: "Logo Uploaded successfully." });
  documentUploaderRef.value.reset();
}

function onIconUploaded (info) {
  notifySuccess({ message: "Logo Uploaded successfully." });
  documentUploaderRef.value.reset();
}

function onFileAdded (files) {
  const file = files[0];
  const validImageTypes = ["image/jpeg", "image/png"];
  if (validImageTypes.includes(file.type)) {
    model.value.file = file;
    model.value.fileChangeFlag = "edit"; // Set the change flag
  } else {
  // Handle invalid file type
    notifyError({ message: "Invalid file type. Please upload an image file in JPEG or PNG format." });
    model.value.file = null;
    model.value.fileChangeFlag = null;
  }
}

function onFileIconAdded (files) {
  const file = files[0];
  const validImageTypes = ["image/jpeg", "image/png"];
  if (validImageTypes.includes(file.type)) {
    model.value.fileIcon = file;
    model.value.fileIconChangeFlag = "edit"; // Set the change flag
  } else {
  // Handle invalid file type
    notifyError({ message: "Invalid file type. Please upload an image file in JPEG or PNG format." });
    model.value.fileIcon = null;
    model.value.fileIconChangeFlag = null;
  }
}

// Create person
const onAddPerson = () => {
  $q.dialog({
    component: editPerson,
    componentProps: {
      personSiteFlag: true
    }
  }).onOk((newPersonId) => {
    if (newPersonId) {
      addNewPersonToDropdown(newPersonId); // this will add the person to dropdown
    }
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

const personList = ref([]);
const options1 = ref([]);
function getAllPersonListForDropdown (id) {
  personService.getAllPersonListForDropdown(id).then((resp) => {
    const responseData = resp
      .map((item) => ({ text: item.fullName + (item.primaryEmailAddress ? " (" + item.primaryEmailAddress + ")" : ""), value: item.id }))
      .sort((a, b) => a.text.localeCompare(b.text));
    personList.value = responseData;
    options1.value = responseData;
  });
}

function filterFn1 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      personList.value = options1.value;
    } else {
      personList.value = options1.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

function getPersonbyId () {
  personService.getPerson(model.value.personId).then((resp) => {
    model.value.fullName = resp.fullName;
    model.value.primaryEmailAddress = resp.primaryEmailAddress;
    model.value.primaryPhoneNumber = resp.primaryPhoneNumber;
  });
}
const roles = ref([]);
const options4 = ref([]);
function getRoles () {
  roleService.getRoles().then((resp) => {
    const responseData = resp.data;
    roles.value = responseData;
    options4.value = responseData;
  });
}

function filterFn4 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      roles.value = options4.value;
    } else {
      roles.value = options4.value.filter(v => v.name.toLowerCase().includes(needle));
    }
  });
}

function onDialogClose () {
  if (!props.id && model.value.personId) {
    personService.deletePerson(model.value.personId);
  }
}

const onSubmit = async () => {
  if (await v$.value.$validate()) {
    processing.value = true;
    if (!props.id) {
      const superAdminRole = roles.value.find(
        r => r.name?.toLowerCase().trim() === "site super admin"
      );

      if (superAdminRole && !model.value.roleIds.includes(superAdminRole.id)) {
        model.value.roleIds.push(superAdminRole.id);
      }
    }
    sitesService.saveSite(props.id, model.value).then((resp) => {
      notifySuccess({ message: "Site is saved successfully." });
      model.value.password = resp;
      onDialogOK({ password: model.value.password, userName: model.value.userName });
    }).finally(() => {
      processing.value = false;
    });
  }
};

const { timeZoneDropdownSingleSelect } = siteModule();

onMounted(() => {
  getCountryList();
  getAllPersonListForDropdown(props.id);
  getRoles();

  timeZoneDropdownSingleSelect.load();
});

watch(() => model.value.countryId, (newValue, oldValue) => {
  if (newValue) {
    getStates();
  }
}, { immediate: true });

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getOrganization();
  }
}, { immediate: true });

watch(() => model.value.personId, (newValue, oldValue) => {
  getPersonbyId();
}, { immediate: false });

watch(() => model.value.countryId, (newValue, oldValue) => {
  model.value.stateProvinceId = newValue !== oldValue && oldValue !== null ? "" : model.value.stateProvinceId;
  model.value.zipCode = newValue !== oldValue && oldValue !== null ? "" : model.value.zipCode;
  if (newValue) {
    maxLengthZip = baseCountryId === model.value.countryId ? "5" : "6";
    getStates(newValue);
    rules = {
      name: { required: helpers.withMessage("Site Name is required", required) },
      personId: { required: helpers.withMessage("Person Name is required", required) },
      zipCode: { minLength: minLength(maxLengthZip), maxLength: maxLength(maxLengthZip), required: helpers.withMessage(maxLengthZip === "5" ? "Zip code is required" : "Pin code is required", required) },
      roleIds: { required: helpers.withMessage("Role is required", required) },
      userName: {
        required: helpers.withMessage("User Name is required", requiredIf(() => !props.id))
      },
      password: passwordValidation
    };
    // Validate rules
    v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });
  }
}, { immediate: false });
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
