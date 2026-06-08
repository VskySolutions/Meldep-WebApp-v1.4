<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none candidateDailog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 95vw !important; max-width: 95vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Candidate</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <q-card>
              <fieldset>
                <legend>Basic Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                  <div class="col-lg-3 col-md-3 col-sm-4 col-xs-6">
                    <div class="q-mb-xs text-black">First Name<span class="required">*</span></div>
                    <q-input
                      v-model="model.firstName"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      maxlength="128"
                      :error="v$.firstName.$error"
                      :error-message="v$.firstName.$errors[0]?.$message"
                      @blur="v$.firstName.$touch"
                    />
                  </div>
                  <div class="col-lg-3 col-md-3 col-sm-4 col-xs-6">
                    <div class="q-mb-xs text-black">Middle Name</div>
                    <q-input
                      v-model="model.middleName"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      maxlength="128"
                    />
                  </div>
                  <div class="col-lg-3 col-md-3 col-sm-4 col-xs-6">
                    <div class="q-mb-xs text-black">Last Name<span class="required">*</span></div>
                    <q-input
                      v-model="model.lastName"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      maxlength="128"
                      :error="v$.lastName.$error"
                      :error-message="v$.lastName.$errors[0]?.$message"
                      @blur="v$.lastName.$touch"
                    />
                  </div>
                  <div class="col-lg-3 col-md-3 col-sm-4 col-xs-6">
                    <div class="q-mb-xs text-black">Email Address<span class="required">*</span></div>
                    <q-input
                      v-model="model.emailAddress"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      type="email"
                      :error="v$.emailAddress.$error"
                      :error-message="v$.emailAddress.$errors[0]?.$message"
                      @click="v$.emailAddress.$touch"
                    />
                  </div>
                </div>
              </fieldset>
              <fieldset class="q-mt-lg">
                <legend>Address Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                  <div class="col-lg-3 col-md-4 col-sm-6 col-xs-6">
                    <div class="q-mb-xs text-black">Address 1<span class="required">*</span></div>
                    <q-input
                      v-model="model.addressLine1"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      hint="Street name/Building number."
                      :error="v$.addressLine1.$error"
                      :error-message="v$.addressLine1.$errors[0]?.$message"
                      @blur="v$.addressLine1.$touch"
                    />
                  </div>
                  <div class="col-lg-3 col-md-4 col-sm-6 col-xs-6">
                    <div class="q-mb-xs text-black">Address 2</div>
                    <q-input
                      v-model="model.addressLine2"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      hint="Apartment/Unit/Suite"
                    />
                  </div>
                  <div class="col-lg-3 col-md-4 col-sm-6 col-xs-6">
                    <formSingleSelectDropdown
                      v-model="model.countryId"
                      label="Country"
                      :options="countryNameDropdownSingleSelect.list.value"
                      :filter="countryNameDropdownSingleSelect.filter"
                      :error="v$.countryId.$error"
                      :error-message="v$.countryId.$errors[0]?.$message"
                      @click="v$.countryId.$touch"
                    />
                  </div>
                  <div class="col-lg-3 col-md-4 col-sm-6 col-xs-6">
                    <formSingleSelectDropdown
                      v-model="model.stateProvinceId"
                      label="State"
                      :options="stateNameDropdownSingleSelect.list.value"
                      :filter="stateNameDropdownSingleSelect.filter"
                      :disable="!model.countryId"
                      :error="v$.stateProvinceId.$error"
                      :error-message="v$.stateProvinceId.$errors[0]?.$message"
                      @click="v$.stateProvinceId.$touch"
                    />
                  </div>
                  <div class="col-lg-3 col-md-4 col-sm-6 col-xs-6">
                    <div class="q-mb-xs text-black">City<span class="required">*</span></div>
                    <div>
                      <q-input
                        v-model="model.city"
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        :error="v$.city.$error"
                        :error-message="v$.city.$errors[0]?.$message"
                        @blur="v$.city.$touch"
                      />
                    </div>
                  </div>
                  <div class="col-lg-3 col-md-3 col-sm-3 col-xs-6">
                    <div class="q-mb-xs text-black">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }}<span class="required">*</span></div>
                    <div v-if="model.countryId === baseCountryId">
                      <q-input
                        v-model="model.zipCode"
                        outlined
                        hide-bottom-space
                        :dense="true"
                        mask="#####"
                        :error="v$.zipCode.$error"
                        :error-message="v$.zipCode.$errors[0]?.$message"
                        @blur="v$.zipCode.$touch"
                      />
                    </div>
                    <div v-else>
                      <q-input
                        v-model="model.zipCode"
                        outlined
                        hide-bottom-space
                        :dense="true"
                        mask="######"
                        :error="v$.zipCode.$error"
                        :error-message="v$.zipCode.$errors[0]?.$message"
                        @blur="v$.zipCode.$touch"
                      />
                    </div>
                  </div>
                  <div class="col-lg-2 col-md-3 col-sm-4 col-xs-6">
                    <div class="q-mb-xs text-black">Mobile Number<span class="required">*</span></div>
                    <div v-if="model.countryId === baseCountryId">
                      <q-input
                        v-model="model.mobileNumber"
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        mask="(###)-###-####"
                        :error="v$.mobileNumber.$error"
                        :error-message="v$.mobileNumber.$errors[0]?.$message"
                        @click="v$.mobileNumber.$touch"
                      />
                    </div>
                    <div v-else>
                      <q-input
                        v-model="model.mobileNumber"
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        mask="##########"
                        :error="v$.mobileNumber.$error"
                        :error-message="v$.mobileNumber.$errors[0]?.$message"
                        @click="v$.mobileNumber.$touch"
                      />
                    </div>
                  </div>
                </div>
              </fieldset>
              <fieldset class="q-mt-lg">
                <legend>Candidate Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <div class="q-mb-xs text-black">Source of candidate</div>
                    <q-input
                      v-model="model.source"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      maxlength="128"
                    />
                  </div>
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <formDate
                      v-model="model.jobApplyDate"
                      label="Job Application Date"
                      :error="v$.jobApplyDate.$error"
                      :error-message="v$.jobApplyDate.$errors[0]?.$message"
                      :onBlur="() => v$.jobApplyDate.$touch()"
                    />
                  </div>
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <div class="q-mb-xs text-black">Reference Name</div>
                    <q-input
                      v-model="model.referenceName"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      maxlength="128"
                    />
                  </div>
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <formSingleSelectDropdown
                      v-model="model.englishFluencyId"
                      label="English Fluency"
                      :required="false"
                      :options="englishFluencyDropdown.list.value"
                      :filter="englishFluencyDropdown.filter"
                    />
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <div class="q-mb-xs text-black">Qualification</div>
                    <q-input
                      v-model="model.qualification"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      maxlength="128"
                    />
                  </div>
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <formMultiSelectDropdown
                      v-model="model.departmentIdsArray"
                      label="Interested Department"
                      :options="departmentNameDropdown.list.value"
                      :filter="departmentNameDropdown.filter"
                      popup-content-class="customPopupContentClass"
                    />
                    <!-- <div class="q-mb-xs text-black">Interested Department</div> -->
                    <!-- <q-select
                      v-model="model.departmentIdsArray"
                      push class="w-100 h-auto"
                      outlined
                      use-input
                      transition-show="jump-up"
                      transition-hide="jump-up"
                      hide-bottom-space
                      :dense="true"
                      multiple
                      fill-input
                      input-debounce="0"
                      :options="departmentList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      @filter="getAllDepartmentListFilter"
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
                    </q-select> -->
                  </div>
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <formSingleSelectDropdown
                      v-model="model.appliedWorkLocationId"
                      label="Applied For Work Location"
                      :options="workLocationDropdownSingleSelect.list.value"
                      :filter="workLocationDropdownSingleSelect.filter"
                      :error="v$.appliedWorkLocationId.$error"
                      :error-message="v$.appliedWorkLocationId.$errors[0]?.$message"
                    />
                  </div>
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <formSingleSelectDropdown
                      v-model="model.jobId"
                      label="Applied Job Position"
                      :options="jobPostNameDropdownSingleSelect.list.value"
                      :filter="jobPostNameDropdownSingleSelect.filter"
                      :error="v$.jobId.$error"
                      :error-message="v$.jobId.$errors[0]?.$message"
                    />
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <div class="q-mb-xs text-black">Experience In Years</div>
                    <q-input
                      v-model="model.experienceYears"
                      outlined
                      hide-bottom-space
                      :dense="true"
                      mask="##"
                    />
                  </div>
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <div class="q-mb-xs text-black">Experience In Months</div>
                    <q-input
                      v-model="model.experienceMonths"
                      outlined
                      hide-bottom-space
                      :dense="true"
                      mask="#####"
                    />
                  </div>
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <div class="q-mb-xs text-black">Expected Salary From (Per Annum L)</div>
                    <q-input
                      v-model="model.expectedSalaryFrom"
                      outlined
                      hide-bottom-space
                      :dense="true"
                      mask="#################"
                    />
                  </div>
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <div class="q-mb-xs text-black">Expected Salary To (Per Annum L)</div>
                    <q-input
                      v-model="model.expectedSalaryTo"
                      outlined
                      hide-bottom-space
                      :dense="true"
                      mask="#################"
                    />
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                  <div class="col-lg-3 col-xs-12 col-sm-6 col-md-4">
                    <formSingleSelectDropdown
                      v-model="model.recruiterId"
                      label="Recruiter"
                      :required="false"
                      :options="activeEmployeesDropdownSingleSelect.list.value"
                      :filter="activeEmployeesDropdownSingleSelect.filter"
                    />
                  </div>
                  <div class="col-lg-3 col-md-3 col-sm-6 col-xs-12">
                    <formSingleSelectDropdown
                      v-model="model.availabilityWorkId"
                      label="Availability for work"
                      :required="false"
                      :options="candidateShiftDropdownSingleSelect.list.value"
                      :filter="candidateShiftDropdownSingleSelect.filter"
                    />
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                  <div class="col-lg-6 col-md-4 col-sm-6 col-xs-12">
                    <div class="q-mb-xs text-black">Experience Details</div>
                    <div style="white-space: normal; overflow-wrap: break-word;">
                      <q-editor
                        v-model="model.experienceDetails"
                        class="full-width "
                        y:dense="$q.screen.lt.md"
                        :toolbar="toolbar"
                        :fonts="fonts"
                      />
                    </div>
                  </div>
                  <!-- <div class="col-lg-6 col-md-4 col-sm-6 col-xs-12 hidden">
                    <div class="q-mb-xs text-black">Notes</div>
                    <div style="white-space: normal; overflow-wrap: break-word;">
                      <q-editor
                        v-model="model.notes"
                        class="full-width "
                        y:dense="$q.screen.lt.md"
                        :toolbar="toolbar"
                        :fonts="fonts"
                      />
                    </div>
                  </div> -->
                  <div class="col-lg-3 col-md-4 col-sm-6 col-xs-12">
                    <div class="q-mb-xs text-black">Resume</div>
                    <div class="col">
                      <div v-if="!model.candidateResumeFileId" class="form-group">
                      <singleFileUploader
                        :allowedExtensions="[
                          '.pdf','.doc','.docx'
                        ]"
                        :maxSizeInMb="25"
                        label="Drag file here or (+) to upload. (File)"
                        @file-selected="handleFile"
                        @file-valid="isFileValid = $event"
                      />
                      </div>
                      <div v-if="model.candidateResumeFileId" class="row justify-center">
                        <img :src="model.virtualPath" alt="" style="width: 30%;">
                      </div>
                      <div v-if="model.candidateResumeFileId" class="row justify-center q-mt-sm">
                        <a :href="model.file.virtualPath" target="_blank" class="q-mr-md">
                          <i class="fa fa-file q-ml-md" style="font-size: 25px; color: gray; transition: transform 0.2s, color 0.2s;" />
                          <span style="display: block; font-size: 14px; color: #555; margin-top: 8px;">
                            View File
                          </span>
                        </a>
                        <q-btn color="negative" label="Remove" style="font-size: 12px;" outline no-caps @click="clearImage" />
                      </div>
                      <!-- <div v-if="errorMessage" class="text-negative q-mt-sm">
                        {{ errorMessage }}
                      </div> -->
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md q-pb-sm">
                  <div class="col-lg-3 col-md-3 col-sm-6 col-xs-12">
                    <div class="q-mb-xs text-black">Does the candidate have transportation?</div>
                    <q-radio v-model="model.isTransportration" val="yes" label="Yes" />
                    <q-radio v-model="model.isTransportration" val="no" label="No" />
                    <q-radio v-model="model.isTransportration" val="unknown" label="Unknown" />
                  </div>
                  <div class="col-lg-3 col-md-3 col-sm-6 col-xs-12">
                    <div class="q-mb-xs text-black">Is the candidate ready to relocate?</div>
                    <q-radio v-model="model.isReadyToRelocate" val="yes" label="Yes" />
                    <q-radio v-model="model.isReadyToRelocate" val="no" label="No" />
                    <q-radio v-model="model.isReadyToRelocate" val="unknown" label="Unknown" />
                  </div>
                  <div class="col-lg-3 col-md-3 col-sm-6 col-xs-12">
                    <div class="q-mb-xs text-black">Does The Candidate have their own system?</div>
                    <q-radio v-model="model.isCandidateOwnSystem" val="yes" label="Yes" />
                    <q-radio v-model="model.isCandidateOwnSystem" val="no" label="No" />
                    <q-radio v-model="model.isCandidateOwnSystem" val="unknown" label="Unknown" />
                  </div>
                  <div class="col-lg-3 col-md-3 col-sm-6 col-xs-12">
                    <div class="q-mb-xs text-black">Is the candidate experienced?</div>
                    <q-radio v-model="model.isExperienced" val="yes" label="Yes" />
                    <q-radio v-model="model.isExperienced" val="no" label="No" />
                    <q-radio v-model="model.isExperienced" val="unknown" label="Unknown" />
                  </div>
                </div>
              </fieldset>
            </q-card>
          </div>
        </div>
        <q-card-actions class="stickyFooter q-gutter-sm justify-center">
          <q-btn
            color="grey-4"
            push
            outline
            label="Close"
            type="button"
            class="text-grey-9 actionBtn"
            no-caps
            @click="onDialogCancel"
          />
          <q-btn
            v-if="tab !== '4_tab'"
            color="primary"
            push
            outline
            label="Save"
            type="submit"
            class="actionBtn"
            :loading="processing"
            :disable="processing"
            no-caps
          />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers, minLength, maxLength, email } from "@vuelidate/validators";
import { isDate } from "validators/zw_validators.js";
import { ref, watch, onMounted } from "vue";
import { notifySuccess, notifyError, zwConfirm, notifyWarning, getLocalStorage } from "assets/utils";
import _ from "lodash";
import candidateService from "modules/candidate/candidate.service";
import { format } from "date-fns"; // Standard TimeZone Conversion

// Shared Inputs
import singleFileUploader from "src/components/form-inputs/_singleFileUpload.vue";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formMultiSelectDropdown from "src/components/form-inputs/_formMultiSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// Shared Dropdowns
import commonModule from "src/modules/common/utils/dropdowns.js";
import candidateModule from "src/modules/candidate/utils/dropdowns.js";
import jobPostModule from "src/modules/job-post/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import departmentModule from "src/modules/department/utils/dropdowns.js";

// ----------------------------------------------------------------------------------------------------------------
// Define emits
// ----------------------------------------------------------------------------------------------------------------

const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();
const $emit = defineEmits(["hide", "ok"]);

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------

const props = defineProps({ id: { type: String, default: "" } });
const candidateId = props.id;

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const $q = useQuasar();
const loading = ref(true);
const processing = ref(false);
const isFileValid = ref(true);
const baseCountryId = process.env.BASE_COUNTRY_ID;
const storedUser = getLocalStorage("user");
const { fonts, toolbar } = getEditorConfig($q);

// ----------------------------------------------------------------------------------------------------------------
// Define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  firstName: "",
  middleName: "",
  lastName: "",
  emailAddress: "",
  mobileNumber: "",
  phoneNumber: "",
  addressLine1: "",
  addressLine2: "",
  countryId: baseCountryId,
  stateProvinceId: null,
  city: "",
  zipCode: "",
  source: "",
  jobApplyDate: format(new Date(), "MM/dd/yyyy"),
  referenceName: "",
  englishFluencyId: "",
  qualification: "",
  departmentId: "",
  appliedWorkLocationId: "",
  jobId: "",
  experienceYears: 0,
  experienceMonths: 0,
  expectedSalaryFrom: 0.00,
  expectedSalaryTo: 0.00,
  recruiterId: "",
  experienceDetails: "",
  notes: "",
  isTransportration: "",
  isReadyToRelocate: "",
  isCandidateOwnSystem: "",
  isExperienced: "",
  availabilityWorkId: "",
  candidateResumeFileId: "",
  virtualPath: "",
  filePic: ""
});

// ----------------------------------------------------------------------------------------------------------------
// Get Candidate details
// ----------------------------------------------------------------------------------------------------------------

const getCandidate = () => {
  loading.value = true;
  candidateService.getCandidate(candidateId).then((resp) => {
    model.value = _.cloneDeep(resp);
    // model.value.jobApplyDate = resp.jobApplyDate ? resp.jobApplyDate : format(resp.jobApplyDate, "MM/dd/yyyy");
    model.value.jobApplyDate =
      resp.jobApplyDate && !isNaN(new Date(resp.jobApplyDate))
        ? format(new Date(resp.jobApplyDate), "MM/dd/yyyy")
        : "";
    if (resp.address) {
      model.value.addressLine1 = resp.address.addressLine1;
      model.value.addressLine2 = resp.address.addressLine2;
      model.value.countryId = resp.address.countryId;
      // model.value.countryId = resp.address.countryId ? resp.address.countryId : "";
      model.value.stateProvinceId = resp.address.stateProvinceId;
      model.value.city = resp.address.city;
      model.value.zipCode = resp.address.zipCode;
    }
    if (resp.person) {
      model.value.firstName = resp.person.firstName;
      model.value.middleName = resp.person.middleName;
      model.value.lastName = resp.person.lastName;
      model.value.emailAddress = resp.person.primaryEmailAddress;
      model.value.mobileNumber = resp.person.primaryPhoneNumber;
    }
    model.value.virtualPath = resp.picture ? resp.picture.virtualPath : "";
    model.value.experienceDetails = resp.experienceDetails ? resp.experienceDetails : "";
    model.value.notes = resp.candidateNotes.map(n => n.note.note);
    model.value.departmentIdsArray = resp.candidateDepartments.map(mapping => mapping.departments.id);
  }).finally(() => {
    loading.value = false;
  });
};

// ----------------------------------------------------------------------------------------------------------------
// Validation Rules
// ----------------------------------------------------------------------------------------------------------------

let maxLengthCountry = baseCountryId === model.value.countryId ? "14" : "10";
let maxLengthZip = baseCountryId === model.value.countryId ? "5" : "6";
let rules = {
  firstName: { required: helpers.withMessage("First name is required", required) },
  lastName: { required: helpers.withMessage("Last name is required", required) },
  emailAddress: {
    required: helpers.withMessage("Email is required", required),
    email: helpers.withMessage("Invalid email", email)
  },
  addressLine1: { required: helpers.withMessage("Address 1 is required", required) },
  countryId: { required: helpers.withMessage("Country is required", required) },
  stateProvinceId: { required: helpers.withMessage("State is required", required) },
  city: { required: helpers.withMessage("City is required", required) },
  // zipCode: { required: helpers.withMessage("Code is required", required) },
  zipCode: { required: helpers.withMessage(maxLengthZip === "5" ? "Zip code is required" : "Pin code is required", required), minLength: minLength(maxLengthZip), maxLength: maxLength(maxLengthZip) },
  jobApplyDate: {
    required: helpers.withMessage("Job application date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  appliedWorkLocationId: { required: helpers.withMessage("Work location is required", required) },
  jobId: { required: helpers.withMessage("Applied job position is required", required) },
  mobileNumber: { required: helpers.withMessage("Mobile number is required", required), minLength: minLength(maxLengthCountry), maxLength: maxLength(maxLengthCountry) }
};

// Validate rules
let v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// ------------------------------------------------------------------------------------
// All Dropdowns
// ------------------------------------------------------------------------------------

const { jobPostNameDropdownSingleSelect } = jobPostModule();
const { activeEmployeesDropdownSingleSelect } = employeeModule();
const { departmentNameDropdown } = departmentModule();

const {
  countryNameDropdownSingleSelect,
  stateNameDropdownSingleSelect
 } = commonModule();

const {
  englishFluencyDropdown,
  workLocationDropdownSingleSelect,
  candidateShiftDropdownSingleSelect
} = candidateModule();

// ------------------------------------------------------------------------------------
// File Upload
// ------------------------------------------------------------------------------------

function handleFile (file) {
  model.value.filePic = file;

  if (file) {
    model.value.fileChangeFlag = "edit";
  } else {
    model.value.filePic = null;
    model.value.candidateResumeFileId = null;
    model.value.fileChangeFlag = "remove";
  }
}

function clearImage () {
  zwConfirm({ message: "Do you want to clear this file ?" }, () => {
    model.value.candidateResumeFileId = null;
    model.value.fileChangeFlag = "remove";
  }, () => {
  });
}

// Submit form
async function onSubmit () {
  try {
    if (processing.value) {
      notifyWarning({ message: "Double click not allowed. Please wait..." });
      return; // stop further submit
    }
    if (!await v$.value.$validate()) {
      return;
    }
    if (!isFileValid.value) {
      notifyWarning({ message: "Please upload a valid file" });
      return;
    }
    // Check if resume file is provided
    // if (invalidFile.value) {
    //   errorMessage.value = "Please select a valid image (jpg, png), PDF or Word file."; // Show the required message
    //   return;
    // }
    processing.value = true;
    candidateService.saveCandidate(props.id, model.value).then(resp => {
      notifySuccess({ message: "Candidate saved successfully." });
      $emit("ok");
      $emit("hide");
    });
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getCandidate();
  }
}, { immediate: true });

watch(() => model.value.countryId, (newValue, oldValue) => {
  if (newValue) {
    maxLengthCountry = baseCountryId === model.value.countryId ? "14" : "10";
    maxLengthZip = baseCountryId === model.value.countryId ? "5" : "6";
    stateNameDropdownSingleSelect.load(newValue);
    rules = {
      firstName: { required: helpers.withMessage("First name is required", required) },
      lastName: { required: helpers.withMessage("Last name is required", required) },
      emailAddress: {
        required: helpers.withMessage("Email is required", required),
        email: helpers.withMessage("Invalid email", email)
      },
      jobApplyDate: {
        required: helpers.withMessage("Job application date is required", required),
        isDate: helpers.withMessage("Date is invalid", isDate)
      },
      appliedWorkLocationId: { required: helpers.withMessage("Work location is required", required) },
      jobId: { required: helpers.withMessage("Applied job position is required", required) },
      addressLine1: { required: helpers.withMessage("Address 1 is required", required), maxLength: maxLength(500) },
      addressLine2: { maxLength: maxLength(500) },
      countryId: { required: helpers.withMessage("Country is required", required) },
      stateProvinceId: { required: helpers.withMessage("State is required", required) },
      city: { required: helpers.withMessage("City is required", required), maxLength: maxLength(500) },
      zipCode: { minLength: minLength(maxLengthZip), maxLength: maxLength(maxLengthZip), required: helpers.withMessage(maxLengthZip === "5" ? "Zip code is required" : "Pin code is required", required) },
      mobileNumber: { required: helpers.withMessage("Mobile number is required", required), minLength: minLength(maxLengthCountry), maxLength: maxLength(maxLengthCountry) },
      identificationNote: { maxLength: maxLength("500") }
    };
    // Validate rules
    v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });
  }
}, { immediate: true });

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  countryNameDropdownSingleSelect.load();
  englishFluencyDropdown.load("Candidate English Fluency");
  departmentNameDropdown.load();
  workLocationDropdownSingleSelect.load("Employee OrgLocation");
  jobPostNameDropdownSingleSelect.load(storedUser?.siteId);
  activeEmployeesDropdownSingleSelect.load();
  candidateShiftDropdownSingleSelect.load("Employee Shift");
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
