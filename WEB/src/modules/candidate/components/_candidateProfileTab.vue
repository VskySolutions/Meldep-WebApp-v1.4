<template>
  <fieldset>
    <legend>Candidate Info</legend>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">First Name</div>
        <div class="text-black">
          {{ model.person.firstName }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Middle Name</div>
        <div class="text-black">
          {{ model.person.middleName }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Last Name</div>
        <div class="text-black">
          {{ model.person.lastName }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Email Address</div>
        <div class="text-black">
          {{ model.person.primaryEmailAddress }}
        </div>
      </div>
    </div>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Mobile Number</div>
        <div class="text-black">
          {{ model.person.primaryPhoneNumber }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">City</div>
        <div class="text-black">
          {{ model.address.city }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">State</div>
        <div class="text-black">
          {{ model.address.addressStateProvince.name }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Country</div>
        <div class="text-black">
          {{ model.address.addressCountry.name }}
        </div>
      </div>
    </div>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">{{ baseCountryId == model.address.countryId ? 'Zip Code' : 'Pin code' }}</div>
        <div class="text-black">
          <span class="text-black">{{ model.address.zipCode }}</span>
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Address 1</div>
        <div class="text-black">
          {{ model.address.addressLine1 }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Address 2</div>
        <div class="text-black">
          {{ model.address.addressLine2 }}
        </div>
      </div>
    </div>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Source of Candidate</div>
        <div class="text-black">
          {{ model.source ? model.source : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Job Application Date</div>
        <div class="text-black">
          {{ model.jobApplyDate }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Reference Name</div>
        <div class="text-black">
          {{ model.referenceName ? model.referenceName : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">English Fluency</div>
        <div class="text-black">
          {{ model.englishFluencies.dropDownValue ? model.englishFluencies.dropDownValue : '-' }}
        </div>
      </div>
    </div>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Qualification</div>
        <div class="text-black">
          {{ model.qualification ? model.qualification : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Interested Department</div>
        <div class="text-black">
          {{ model.candidateDepartments.length ? model.candidateDepartments.map(d => d.candidateDepartments).join(', ') : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Applied For Work Location</div>
        <div class="text-black">
          {{ model.appliedWorkLocations.dropDownValue ? model.appliedWorkLocations.dropDownValue : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Applied Job Position</div>
        <div class="text-black">
          {{ model.job.jobTitle ? model.job.jobTitle : '-' }}
        </div>
      </div>
    </div>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Experience In Years</div>
        <div class="text-black">
          {{ model.experienceYears ? model.experienceYears : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Experience In Months</div>
        <div class="text-black">
          {{ model.experienceMonths ? model.experienceMonths : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Expected Salary From</div>
        <div class="text-black">
          {{ model.expectedSalaryFrom ? model.expectedSalaryFrom : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Expected Salary To</div>
        <div class="text-black">
          {{ model.expectedSalaryTo ? model.expectedSalaryTo : '-' }}
        </div>
      </div>
    </div>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Recruiter</div>
        <div class="text-black">
          {{ model.employee.person.fullName ? model.employee.person.fullName : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Availability for work</div>
        <div class="text-black">
          {{ model.availabilityWorks.dropDownValue ? model.availabilityWorks.dropDownValue : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Does the candidate have transportation?</div>
        <div class="text-black">
          {{ model.isTransportration ? capitalizeFirstLetter(model.isTransportration) : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Is the candidate ready to relocate?</div>
        <div class="text-black">
          {{ model.isReadyToRelocate ? capitalizeFirstLetter(model.isReadyToRelocate) : '-' }}
        </div>
      </div>
    </div>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Does The Candidate have their own system?</div>
        <div class="text-black">
          {{ model.isCandidateOwnSystem ? capitalizeFirstLetter(model.isCandidateOwnSystem) : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Is the candidate experienced?</div>
        <div class="text-black">
          {{ model.isCandidateOwnSystem ? capitalizeFirstLetter(model.isCandidateOwnSystem) : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
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
    <div class="row q-col-gutter-x-md q-mb-md q-mt-md">
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Created By</div>
        <div class="text-black">
          {{ model.createdBy.person.fullName ? model.createdBy.person.fullName : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Created Date</div>
        <div class="text-black">
          {{ model.createdOnUtc ? model.createdOnUtc : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Updated By</div>
        <div class="text-black">
          {{ model.updatedBy.person.fullName ? model.updatedBy.person.fullName : '-' }}
        </div>
      </div>
      <div class="col-3 col-sm-3 col-md-3">
        <div class="q-mb-xs">Updated Date</div>
        <div class="text-black">
          {{ model.updatedOnUtc ? model.updatedOnUtc : '-' }}
        </div>
      </div>
    </div>
  </fieldset>
</template>

<script setup>
import { ref, watch, onMounted } from "vue";
import _ from "lodash";
import candidateService from "modules/candidate/candidate.service";

// define Props
const props = defineProps({ candidateId: { type: String, default: "" } });

// common variables
const loading = ref(true);
const candidateId = props.candidateId;
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
  job: {
    jobTitle: ""
  },
  createdBy: {
    person: {
      fullName: ""
    }
  },
  updatedBy: {
    person: {
      fullName: ""
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

const capitalizeFirstLetter = (value) => {
  return value ? value.charAt(0).toUpperCase() + value.slice(1) : value;
};

// get candidate details
const getCandidate = () => {
  loading.value = true;
  candidateService.getCandidate(candidateId).then((resp) => {
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

watch(() => candidateId, (newValue, oldValue) => {
  if (newValue) {
    getCandidate();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  getCandidate();
});
</script>
