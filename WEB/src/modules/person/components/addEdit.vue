<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 65vw; max-width: 65vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Person</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Basic Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black text-black">First Name<span class="required">*</span></div>
                  <div>
                    <q-input
                      v-model="model.firstName"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :error="v$.firstName.$error"
                      :error-message="v$.firstName.$errors[0]?.$message"
                      @click="v$.firstName.$touch"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Middle Name</div>
                  <div>
                    <q-input
                      v-model="model.middleName"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :error="v$.middleName.$error"
                      :error-message="v$.middleName.$errors[0]?.$message"
                      @click="v$.middleName.$touch"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Last Name<span class="required">*</span></div>
                  <div>
                    <q-input
                      v-model="model.lastName"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :error="v$.lastName.$error"
                      :error-message="v$.lastName.$errors[0]?.$message"
                      @click="v$.lastName.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Gender</div>
                  <div>
                    <q-radio
                      v-for="gender in genderList"
                      :key="gender.id"
                      v-model="model.genderId"
                      class="q-mb-xs text-black text-black"
                      checked-icon="o_task_alt"
                      unchecked-icon="o_panorama_fish_eye"
                      :val="gender.id"
                      :label="gender.dropdownValue"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Title
                    <q-icon
                      name="o_info"
                      size="16px"
                      color="grey-7"
                      class="cursor-pointer"
                    >
                      <q-tooltip>
                        <strong>Example:</strong><br>
                        Co-Founder & CFO,<br>
                        Director of Finance,<br>
                        President & CFO,<br>
                        CFO Finance,<br>
                        Founder and CFO
                      </q-tooltip>
                    </q-icon>
                  </div>
                  <div>
                    <q-input
                      v-model="model.title"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black"><label class="">Date of birth (Optional)</label></div>
                  <div class="form-group">
                    <q-input
                      v-model="model.dob"
                      outlined
                      stack-label
                      hide-bottom-space
                      mask="##/##/####"
                      dense
                      :error="v$.dob.$error"
                      :error-message="v$.dob.$errors[0]?.$message"
                      @blur="v$.dob.$touch"
                    >
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date
                              v-model="model.dob"
                              mask="MM/DD/YYYY"
                              @update:model-value="() => $refs.qDateProxy.hide()"
                            />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-6">
                  <div class="q-mb-xs text-black">Email Address<span class="required">*</span></div>
                  <div><q-input
                    v-model="model.primaryEmailAddress"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    type="email"
                    :error="v$.primaryEmailAddress.$error"
                    :error-message="v$.primaryEmailAddress.$errors[0]?.$message"
                    @click="v$.primaryEmailAddress.$touch"
                  />
                  </div>
                </div>
                <div class="col-12 col-md-6 col-lg-6">
                  <div class="q-mb-xs text-black">Profile Link</div>
                  <q-input
                    v-model="model.profileLink"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    type="url"
                    :error="v$.profileLink.$error"
                    :error-message="v$.profileLink.$errors[0]?.$message"
                    @blur="v$.profileLink.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-6">
                  <div class="q-mb-xs text-black">Phone Number<span class="required">*</span></div>
                  <div>
                    <q-input
                      v-model="model.primaryPhoneNumber"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :maxlength="countryValidation.phoneNumberMaxLength"
                      :mask="countryValidation.phoneNumberPlaceHolder"
                      :placeholder="countryValidation.phoneNumberPlaceHolder"
                      :error="v$.primaryPhoneNumber.$error"
                      :error-message="v$.primaryPhoneNumber.$errors[0]?.$message"
                      @blur="v$.primaryPhoneNumber.$touch"
                    >
                      <template #prepend>
                        <span class="text-black text-caption">
                          {{ countryValidation.countryCode }}
                        </span>
                      </template>
                    </q-input>
                  </div>
                </div>
                <div class="col-12 col-lg-6 col-md-6">
                  <div class="q-mb-xs text-black">Profile Picture</div>
                  <!-- <div>
                    <div v-if="!model.pictureId" class="col">
                      <div class="form-group">
                        <q-uploader
                          ref="documentUploaderRef"
                          color="white"
                          text-color="dark"
                          class="prodUploader"
                          with-credentials
                          hide-upload-btn
                          accept=".jpg,.jpeg,.png"
                          field-name="personfile"
                          flat bordered label="Drag file here or (+) to upload"
                          @uploaded="onUploaded"
                          @added="onFileAdded"
                        />
                        <div class="text-grey-7 text-caption q-mt-xs">
                          <i>Allowed Files: jpg, png, jpeg</i>
                        </div>
                      </div>
                    </div>
                    <div v-if="model.pictureId" class="row justify-center">
                      <img :src="model.virtualPath" alt="" style="width: 30%">
                    </div>
                    <div v-if="model.pictureId" class="row justify-center q-mt-sm">
                      <q-btn color="negative" label="Remove" outline no-caps @click="clearImage" />
                    </div>
                  </div> -->
                  <singleFileUploader
                    :allowedTypes="['image/jpeg','image/png','image/jpg']"
                    :maxSizeInMb="25"
                    :imageSize="500"
                    :imageHeight="500"
                    :isImage="true"
                    label="Upload Profile Image"
                    @file-selected="handleFile"
                    @file-valid="isFileValid = $event"
                    :initialUrl="model.virtualPath"
                  />
                </div>
              </div>
            </fieldset>
            <fieldset class="q-mt-lg">
              <legend>Primary Address Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-sm-12 col-md-6 col-lg-4">
                  <div class="q-mb-xs text-black">Country<span class="required">*</span></div>
                  <div>
                    <q-select
                      v-model="model.countryId"
                      clearable
                      use-input
                      outlined
                      hide-bottom-space
                      :dense="true"
                      :options="countryList"
                      option-value="id"
                      option-label="name"
                      emit-value
                      map-options
                      :error="v$.countryId.$error"
                      :error-message="v$.countryId.$errors[0]?.$message"
                      @filter="countryListForFilter"
                      @blur="v$.countryId.$touch"
                      @update:model-value="onCountryChange(model.countryId)"
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
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">State</div>
                  <div>
                    <q-select
                      v-model="model.stateProvinceId"
                      clearable
                      use-input
                      outlined
                      hide-bottom-space
                      :dense="true"
                      :options="stateList"
                      option-value="id"
                      option-label="name"
                      emit-value
                      map-options
                      @filter="stateListForFilter"
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
                <div class="col-sm-12 col-md-6 col-lg-4">
                  <div class="q-mb-xs text-black">Address Type</div>
                  <div>
                    <q-radio
                      v-for="addressType in addressTypeList"
                      :key="addressType.id"
                      v-model="model.addressTypeId"
                      class="q-mb-xs text-black text-black"
                      checked-icon="o_task_alt"
                      unchecked-icon="o_panorama_fish_eye"
                      :val="addressType.id"
                      :label="addressType.dropdownValue"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Address 1</div>
                  <div>
                    <q-input
                      v-model="model.addressLine1"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      hint="Street name/Building number."
                    />
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">Address 2</div>
                  <div>
                    <q-input
                      v-model="model.addressLine2"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      hint="Apartment/Unit/Suite"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">City</div>
                  <div>
                    <q-input
                      v-model="model.city"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4">
                  <div class="q-mb-xs text-black">
                     {{ countryValidation.zipCodeLabel }}
                  </div>
                  <div>
                    <q-input
                      v-model="model.zipCode"
                      outlined hide-bottom-space
                      :dense="true"
                      :maxlength="countryValidation.zipCodeMaxLength"
                      :placeholder="countryValidation.zipCodePlaceHolder"
                      :error="v$.zipCode.$error"
                      :error-message="v$.zipCode.$errors[0]?.$message"
                      @blur="v$.zipCode.$touch"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset class="q-mt-lg">
              <legend>Other Info</legend>
              <div class="row q-col-gutter-x-md q-mb-lg">
                <div class="col-12 col-md-6 col-lg-4">
                  <div class="q-mb-xs text-black"><label class="">Identified Date</label></div>
                  <div class="form-group">
                    <q-input
                      v-model="model.identifiedDateStr"
                      outlined
                      stack-label
                      hide-bottom-space
                      mask="##/##/####"
                      dense
                      :error="v$.identifiedDateStr.$error"
                      :error-message="v$.identifiedDateStr.$errors[0]?.$message"
                      @click="v$.identifiedDateStr.$touch"
                    >
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date v-model="model.identifiedDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
                <div class="col-12 col-md-6 col-lg-4">
                  <div class="q-mb-xs text-black">Identifier</div>
                  <div>
                    <q-select
                      v-model="model.identifiedById"
                      clearable
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="personList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      @filter="personListForFilter"
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
                    </q-select>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-lg">
                <div class="col-12">
                  <div class="q-mb-xs text-black"><label>Identification Note</label></div>
                  <div class="form-group">
                    <q-input
                      v-model="model.identificationNote"
                      outlined
                      autogrow
                      hint="The maximum length allowed is 500."
                      :error="v$.identificationNote.$error"
                      :error-message="v$.identificationNote.$errors[0]?.$message"
                      @click="v$.identificationNote.$touch"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset class="q-mt-lg">
              <legend>Emergency Contact Info</legend>
              <div class="row q-col-gutter-x-md q-mb-lg">
                <div class="col-12 col-md-6 col-lg-4">
                  <div class="q-mb-xs text-black"><label>Relation</label></div>
                  <div>
                    <q-input v-model="model.relation" outlined stack-label hide-bottom-space :dense="true" />
                  </div>
                </div>
                <div class="col-12 col-md-6 col-lg-4">
                  <div class="q-mb-xs text-black"><label>Full Name</label></div>
                  <div>
                    <q-input
                      v-model="model.relationFullName"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :error="v$.relationFullName.$error"
                      :error-message="v$.relationFullName.$errors[0]?.$message"
                      @click="v$.relationFullName.$touch"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-6 col-lg-4">
                  <div class="q-mb-xs text-black"><label>Phone Number</label></div>
                  <div>
                    <q-input
                      v-model="model.phoneNumber"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :error="v$.phoneNumber.$error"
                      :error-message="v$.phoneNumber.$errors[0]?.$message"
                      @blur="v$.phoneNumber.$touch"
                    />
                  </div>
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
// Import libraries
import { useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers, maxLength, email, url } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import _ from "lodash";
import { isDate } from "validators/zw_validators.js";
import { notifySuccess, notifyWarning } from "assets/utils";
import { format } from "date-fns"; // Standard TimeZone Conversion

import personService from "modules/person/person.service";
import commonService from "services/common.service";

// Shared Inputs
import singleFileUploader from "src/components/form-inputs/_singleFileUpload.vue";

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" },
  siteId: { type: String, default: "" },
  personSiteFlag: { type: Boolean, default: false }
});

// Common variables
const baseCountryId = process.env.BASE_COUNTRY_ID;
const loading = ref(true);
const processing = ref(false);
const isFileValid = ref(true);

let personId = props.id;
const siteId = props.siteId;
const flag = props.personSiteFlag;
const countryValidation = ref({});

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Define model values
const model = ref({
  firstName: "",
  middleName: "",
  lastName: "",
  genderId: null,
  dob: "",
  primaryEmailAddress: "",
  primaryPhoneNumber: "",
  addressTypeId: null,
  addressLine1: "",
  addressLine2: "",
  countryId: baseCountryId,
  stateProvinceId: null,
  city: "",
  zipCode: "",
  identifiedById: null,
  identifiedDateStr: format(new Date(), "MM/dd/yyyy"),
  identificationNote: "",
  relation: "",
  relationFullName: "",
  phoneNumber: "",
  pictureId: null,
  virtualPath: "",
  profileLink: ""
});

// get person details on edit mode
const getPerson = () => {
  loading.value = true;
  personService.getPerson(personId).then((resp) => {
    Object.assign(model.value, _.cloneDeep(resp));

    model.value.dob = resp.dob ? format(resp.dob, "MM/dd/yyyy") : "";
    if (resp.address) {
      model.value.addressLine1 = resp.address.addressLine1;
      model.value.addressLine2 = resp.address.addressLine2;
      model.value.countryId = resp.address.countryId ? resp.address.countryId : "";
      model.value.stateProvinceId = resp.address.stateProvinceId;
      model.value.city = resp.address.city;
      model.value.zipCode = resp.address.zipCode;
    }
    model.value.virtualPath = resp.picture ? resp.picture.virtualPath : "";
    model.value.identifiedDateStr = resp.identifiedDate ? format(resp.identifiedDate, "MM/dd/yyyy") : "";
    model.value.primaryPhoneNumber = resp.primaryPhoneNumber;
  }).finally(() => {
    loading.value = false;
  });
};

const validPhoneLength = helpers.withMessage(
  "Invalid phone number",
  value => {
    if (!value) return true;
    return value.replace(/\D/g, "").length === 10;
  }
);

// Validation rules
const createNameRules = (fieldLabel, isRequired = true) => {
  const rules = {
    alphaOnly: helpers.withMessage(
      `${fieldLabel} can contain only alphabetic characters (no spaces, numbers, or special characters)`,
      value => {
        if (!value) return true; // allow empty for optional fields
        return /^[A-Za-z]+$/.test(value);
      }
    ),
    maxLength: helpers.withMessage(
      `${fieldLabel} must not exceed 50 characters`,
      maxLength(50)
    )
  };

  if (isRequired) {
    rules.required = helpers.withMessage(
      `${fieldLabel} is required`,
      required
    );
  }
  return rules;
};

const rules = computed(() => {
  return {
    firstName: createNameRules("First name"),
    lastName: createNameRules("Last name"),
    middleName: createNameRules("Middle name", false),
    primaryEmailAddress: {
      required: helpers.withMessage("Email is required", required),
      email: helpers.withMessage("Invalid email", email)
    },
    profileLink: {
      url: helpers.withMessage("Invalid URL", url)
    },
    countryId: { required: helpers.withMessage("Country is required", required) },
    primaryPhoneNumber: {
      required: helpers.withMessage("Phone Number is required", required),
      validPhone: helpers.withMessage(
        'Invalid phone number format',
        value => {
          if (!value) {
            return true;
          }
          const phoneNumberPattern = countryValidation.value?.phoneNumberPattern;
          return phoneNumberPattern ? new RegExp(phoneNumberPattern).test(value) : true;
        }
      )
    },
    zipCode: {
      validZipCode: helpers.withMessage(
        () => `${countryValidation.value?.zipCodeLabel} is invalid`,
        (value) => {
          if (!value) return true;
          const zipCodePattern = countryValidation.value?.zipCodePattern;
          return zipCodePattern ? new RegExp(zipCodePattern).test(value) : true;
        }
      )
    },
    identifiedDateStr: { isDate: helpers.withMessage("Date is invalid", isDate) },
    dob: { isDate: helpers.withMessage("Date of birth is invalid", isDate) },
    addressLine1: { maxLength: maxLength(500) },
    addressLine2: { maxLength: maxLength(500) },
    city: { maxLength: maxLength(500) },
    identificationNote: { maxLength: maxLength("500") },
    relationFullName: { pattern: helpers.withMessage("Name must contain only alphabetic characters.", value => !value || /^[A-Za-z][A-Za-z'-]*$/.test(value)) },
    phoneNumber: { validPhoneLength }
  };
});

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// =====================================================================
// Dropdown Lists
// =====================================================================
const personList = ref([]);
const personListOptions = ref([]);
function getAllPersonListForDropdown () {
  personService.getAllPersonListForDropdown().then((resp) => {
    const responseData = resp
      .map((item) => ({ text: item.fullName + (item.primaryEmailAddress ? " (" + item.primaryEmailAddress + ")" : ""), value: item.id }))
      .sort((a, b) => a.text.localeCompare(b.text));
    personList.value = responseData;
    personListOptions.value = responseData;
  });
}

function personListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      personList.value = personListOptions.value;
    } else {
      personList.value = personListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get dropdown list for gender
const genderList = ref([]);
function getGenders () {
  commonService.getDropDownForSite(siteId, "Gender").then((resp) => {
    genderList.value = resp;
    if (model.value.genderId === null) {
      model.value.genderId = resp[0].id; // Set to the first item by default
    }
  });
}

// Get dropdown list for countries
const countryList = ref([]);
const countryListOptions = ref([]);
function getCountryList () {
  commonService.getCountries().then((resp) => {
    countryList.value = resp;
    countryListOptions.value = resp;
  });
}
function countryListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      countryList.value = countryListOptions.value;
    } else {
      countryList.value = countryListOptions.value.filter(v => v.name.toLowerCase().includes(needle));
    }
  });
}

// Get dropdown list for countries
const stateList = ref([]);
const stateListOptions = ref([]);
function getStates () {
  model.value.stateProvinceId = null;
  commonService.getStates(model.value.countryId).then((resp) => {
    stateList.value = resp;
    stateListOptions.value = resp;
  });
}
function stateListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      stateList.value = stateListOptions.value;
    } else {
      stateList.value = stateListOptions.value.filter(v => v.name.toLowerCase().includes(needle));
    }
  });
}

// Get dropdown list for Address Type
const addressTypeList = ref([]);
function getAddressTypes () {
  commonService.getDropDownForSite(siteId, "Address Type").then((resp) => {
    addressTypeList.value = resp;
    if (model.value.addressTypeId === null) {
      model.value.addressTypeId = resp[0].id; // Set to the first item by default
    }
  });
}

// Upload Image
// -------------------------------------------------------------------------------------------------------
// const documentUploaderRef = ref(null);

// function onFileAdded (files) {
//   const file = files[0];
//   const validImageTypes = ["image/jpeg", "image/png"];
//   if (!file) return;

//   if (!validImageTypes.includes(file.type)) {
//     notifyError({
//       message: "Only JPEG and PNG images are allowed."
//     });
//     documentUploaderRef.value.reset();
//     model.value.personPic = null;
//     return;
//   }

//   model.value.personPic = file;
//   model.value.personChangeFlag = "edit";
// }

// function onUploaded (info) {
//   notifySuccess({ message: "File Uploaded successfully." });
//   documentUploaderRef.value.reset();
// }

// function clearImage () {
//   zwConfirm({ message: "Do you want to clear this Picture ?" }, () => {
//     model.value.pictureId = null;
//     model.value.personChangeFlag = "remove";
//   }, () => {
//   });
// }

function handleFile (file) {
  model.value.personPic = file;

  if (file) {
    model.value.personChangeFlag = "edit";
  } else {
    model.value.personPic = null;
    model.value.pictureId = null;
    model.value.personChangeFlag = "remove";
  }
}

// change country
const onCountryChange = async (countryId) => {
  if (!countryId) {
    countryCode.value = '';
    phoneNumberPattern.value = '';
    zipCodePattern.value = '';
    return;
  }
  try {
    countryValidation.value = await commonService.getValidationDetailsByCountryId(countryId);
    // countryCode.value = resp.countryCode || '';
    // phoneNumberPattern.value = resp.phoneNumberPattern || '';
    // zipCodePattern.value = resp.zipCodePattern || '';
    // zipCodeLabel.value = resp.zipCodeLabel || 'Postal Code';
    console.log(resp);
  } catch (error) {
    console.error('Error fetching country details:', error);
  }
};

// Submit form
const onSubmit = async () => {
  try {
    if (!isFileValid.value) {
      notifyWarning({ message: "Please upload a valid file" });
      return;
    }
    if (await v$.value.$validate()) {
      processing.value = true;
      model.value.siteId = siteId;
      model.value.personSiteFlag = flag;
      personService.savePerson(personId, model.value).then((resp) => {
        notifySuccess({ message: "Person is saved successfully." });
        personId = resp;
        onDialogOK(personId);
      });
    }
  } catch (error) {
    console.error("Error in submitting the person:", error);
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => model.value.countryId, (newValue, oldValue) => {
  if (newValue) {
    getStates();
  }
}, { immediate: true });

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getPerson();
  }
}, { immediate: true });

watch(
  () => model.value.countryId,
  (newValue) => {
    if (newValue) {
      onCountryChange(newValue);
    }
  },
  { immediate: true }
);

// On page rendering
onMounted(() => {
  getAllPersonListForDropdown();
  getGenders();
  getAddressTypes();
  getCountryList();
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
