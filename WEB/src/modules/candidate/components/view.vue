<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1000px; height: 100% !important;max-width: 100vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.person.fullName }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
            <q-tab name="1_tab" label="Basic Info." class="q-px-lg q-mr-md" />
            <q-tab name="2_tab" label="Candidate Info." class="q-px-lg" :disable="disableTab" />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated class="q-mt-xs">
            <q-tab-panel name="1_tab">
              <fieldset>
                <legend>Basic Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-4 col-sm-4 col-md-4">
                    <div class="q-mb-xs">First Name</div>
                    <div class="text-black">
                      {{ model.person.firstName }}
                    </div>
                  </div>
                  <div class="col-4 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Middle Name</div>
                    <div class="text-black">
                      {{ model.person.middleName }}
                    </div>
                  </div>
                  <div class="col-4 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Last Name</div>
                    <div class="text-black">
                      {{ model.person.lastName }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-4 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Email Address</div>
                    <div class="text-black">
                      {{ model.person.primaryEmailAddress }}
                    </div>
                  </div>
                  <div class="col-4 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Mobile Number</div>
                    <div class="text-black">
                      {{ model.person.primaryPhoneNumber }}
                    </div>
                  </div>
                </div>
              </fieldset>
              <fieldset class="q-mt-md">
                <legend>Address Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-4 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Address 1</div>
                    <div class="text-black">
                      {{ model.address.addressLine1 }}
                    </div>
                  </div>
                  <div class="col-4 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Address 2</div>
                    <div class="text-black">
                      {{ model.address.addressLine2 }}
                    </div>
                  </div>
                  <div class="col-4 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Country</div>
                    <div class="text-black">
                      {{ model.address.addressCountry.name }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-4 col-sm-4 col-md-4">
                    <div class="q-mb-xs">State</div>
                    <div class="text-black">
                      {{ model.address.addressStateProvince.name }}
                    </div>
                  </div>
                  <div class="col-4 col-sm-4 col-md-4">
                    <div class="q-mb-xs">City</div>
                    <div class="text-black">
                      {{ model.address.city }}
                    </div>
                  </div>
                  <div class="col-4 col-sm-4 col-md-4">
                    <div class="q-mb-xs">{{ baseCountryId == model.address.countryId ? 'Zip Code' : 'Pin code' }}</div>
                    <div class="text-black">
                      <span class="text-black">{{ model.address.zipCode }}</span>
                    </div>
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="2_tab">
              <fieldset>
                <legend>Candidate Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Source of Candidate</div>
                    <div class="text-black">
                      {{ model.source ? model.source : '-' }}
                    </div>
                  </div>
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Job Application Date</div>
                    <div class="text-black">
                      {{ model.jobApplyDate }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Reference Name</div>
                    <div class="text-black">
                      {{ model.referenceName ? model.referenceName : '-' }}
                    </div>
                  </div>
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">English Fluency</div>
                    <div class="text-black">
                      {{ model.englishFluencies.dropDownValue ? model.englishFluencies.dropDownValue : '-' }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Qualification</div>
                    <div class="text-black">
                      {{ model.qualification ? model.qualification : '-' }}
                    </div>
                  </div>
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Interested Department</div>
                    <div class="text-black">
                      {{ model.candidateDepartments.length ? model.candidateDepartments.map(d => d.candidateDepartments).join(', ') : '-' }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Applied For Work Location</div>
                    <div class="text-black">
                      {{ model.appliedWorkLocations.dropDownValue ? model.appliedWorkLocations.dropDownValue : '-' }}
                    </div>
                  </div>
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Applied Job Position</div>
                    <div class="text-black">
                      {{ model.job.jobTitle ? model.job.jobTitle : '-' }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Experience In Years</div>
                    <div class="text-black">
                      {{ model.experienceYears ? model.experienceYears : '-' }}
                    </div>
                  </div>
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Experience In Months</div>
                    <div class="text-black">
                      {{ model.experienceMonths ? model.experienceMonths : '-' }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Expected Salary From</div>
                    <div class="text-black">
                      {{ model.expectedSalaryFrom ? model.expectedSalaryFrom : '-' }}
                    </div>
                  </div>
                  <div class="col-lg-6 col-sm-4 col-md-43">
                    <div class="q-mb-xs">Expected Salary To</div>
                    <div class="text-black">
                      {{ model.expectedSalaryTo ? model.expectedSalaryTo : '-' }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Recruiter</div>
                    <div class="text-black">
                      {{ model.employee.person.fullName ? model.employee.person.fullName : '-' }}
                    </div>
                  </div>
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Availability for work</div>
                    <div class="text-black">
                      {{ model.availabilityWorks.dropDownValue ? model.availabilityWorks.dropDownValue : '-' }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Does the candidate have transportation?</div>
                    <div class="text-black">
                      {{ model.isTransportration ? capitalizeFirstLetter(model.isTransportration) : '-' }}
                    </div>
                  </div>
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Is the candidate ready to relocate?</div>
                    <div class="text-black">
                      {{ model.isReadyToRelocate ? capitalizeFirstLetter(model.isReadyToRelocate) : '-' }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Does The Candidate have their own system?</div>
                    <div class="text-black">
                      {{ model.isCandidateOwnSystem ? capitalizeFirstLetter(model.isCandidateOwnSystem) : '-' }}
                    </div>
                  </div>
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Is the candidate experienced?</div>
                    <div class="text-black">
                      {{ model.isCandidateOwnSystem ? capitalizeFirstLetter(model.isCandidateOwnSystem) : '-' }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-lg-6 col-sm-4 col-md-4">
                    <div class="q-mb-xs">Resume</div>
                    <div class="form-group">
                      <div v-if="model.virtualPath">
                        <a :href="model.virtualPath" download target="_blank" rel="noopener noreferrer" style="text-decoration: none; text-align: center; display: inline-block;"><i class="fas fa-download" /> Download File</a>
                      </div>
                      <div class="text-black" v-else>
                        -
                      </div>
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-lg-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Experience Details</div>
                    <div class="text-black RichTextEditor">
                      <p v-html="model.experienceDetails ? model.experienceDetails : '-'" />
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md hidden">
                  <div class="col-lg-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Notes</div>
                    <div class="text-black RichTextEditor">
                      <p v-html="model.notes ? model.notes : '-'" />
                    </div>
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
          </q-tab-panels>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";
import candidateService from "modules/candidate/candidate.service";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Common variables
const loading = ref(true);
const tab = ref("1_tab");
const baseCountryId = process.env.BASE_COUNTRY_ID;

// define model
const model = ref({
  person: {
    firstName: "",
    middleName: "",
    lastName: "",
    emailAddress: "",
    mobileNumber: ""
  },
  address: {
    addressId: "",
    addressLine1: "",
    addressLine2: "",
    countryId: "",
    stateProvinceId: "",
    city: "",
    zipCode: "",
    addressCountry: {
      name: ""
    },
    addressStateProvince: {
      name: ""
    }
  },
  employee: {
    person: {
      fullName: ""
    }
  },
  availabilityWorks: {
    dropDownValue: ""
  },
  appliedWorkLocations: {
    dropDownValue: ""
  },
  englishFluencies: {
    dropDownValue: ""
  },
  candidateDepartments: {
    departments: {
      name: ""
    }
  },
  source: "",
  jobApplyDate: "",
  referenceName: "",
  englishFluencyId: "",
  qualification: "",
  departmentId: "",
  appliedWorkLocationId: "",
  jobId: "",
  experienceYears: "",
  experienceMonths: "",
  expectedSalaryFrom: "",
  expectedSalaryTo: "",
  recruiterId: "",
  experienceDetails: "",
  notes: "",
  isTransportration: "",
  isReadyToRelocate: "",
  isCandidateOwnSystem: "",
  isExperienced: "",
  availabilityWorkId: "",
  candidateResumeFileId: "",
  virtualPath: ""
});

// get candidate details
const getCandidate = () => {
  loading.value = true;
  candidateService.getCandidate(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.virtualPath = resp.file ? resp.file.virtualPath : "";
    model.value.notes = resp.candidateNotes.map(n => n.note.note);
    model.value.candidateDepartments = resp.candidateDepartments.map(item => ({
      ...item,
      candidateDepartments: item.departments.name
    }));
  }).finally(() => {
    loading.value = false;
  });
};

const capitalizeFirstLetter = (value) => {
  return value ? value.charAt(0).toUpperCase() + value.slice(1) : value;
};

// On page rendering
onMounted(() => {
  getCandidate();
});

</script>
