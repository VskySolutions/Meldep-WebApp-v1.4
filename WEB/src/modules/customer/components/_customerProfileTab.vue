<template>
  <fieldset v-if="model.company">
    <legend>Company Info</legend>
    <div class="row q-col-gutter-x-md">
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Company Name</div>
          <div class="text-black q-mb-sm">{{ model.name }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Website</div>
          <div class="text-black q-mb-sm">{{ model.website }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Business Type</div>
          <div class="text-black q-mb-sm">{{ model.businessTypeId ? model.businessTypeId : '-' }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Primary Employee Person</div>
          <div class="text-black q-mb-sm">{{ model.employeeName ? model.employeeName : '-' }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Email</div>
          <div class="text-black q-mb-sm">{{ model.emailAddress }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Alt. Email</div>
          <div class="text-black q-mb-sm">{{ model.alternativeEmailAddress ? model.alternativeEmailAddress : '-' }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Phone Number</div>
          <div class="text-black q-mb-sm">{{ model.phoneNumber }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Alt. Phone Number</div>
          <div class="text-black q-mb-sm">{{ model.alternativePhoneNumber ? model.alternativePhoneNumber : '-' }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Country</div>
          <div class="text-black q-mb-sm">{{ model.countryName ? model.countryName : '-' }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">State</div>
          <div class="text-black q-mb-sm">{{ model.stateProvinceName ? model.stateProvinceName : '-' }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Zip Code</div>
          <div class="text-black q-mb-sm">{{ model.zipCode ? model.zipCode : '-' }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">City</div>
          <div class="text-black q-mb-sm">{{ model.city ? model.city : '-' }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Address 1</div>
          <div class="text-black q-mb-sm">{{ model.addressLine1 ? model.addressLine1 : '-' }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Address 2</div>
          <div class="text-black q-mb-sm">{{ model.addressLine2 ? model.addressLine2 : '-' }}</div>
        </div>
      </div>
    </div>

    <div class="row q-col-gutter-x-md">
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Created By</div>
          <div class="text-black q-mb-sm">{{ model.createdBy ? model.createdBy : '-' }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Created Date</div>
          <div class="text-black q-mb-sm">{{ model.createdOnUtc ? model.createdOnUtc : '-' }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Updated By</div>
          <div class="text-black q-mb-sm">{{ model.updatedBy ? model.updatedBy : '-' }}</div>
        </div>
      </div>
      <div class="col-12 col-md-3">
        <div class="form-group">
          <div class="q-mb-xs">Updated Date</div>
          <div class="text-black q-mb-sm">{{ model.updatedOnUtc ? model.updatedOnUtc : '-' }}</div>
        </div>
      </div>
    </div>

  </fieldset>
  <fieldset v-if="model.person">
    <legend>Basic Info</legend>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">First Name</div>
        <div class="text-black">
          {{ model.firstName }}
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">Middle Name</div>
        <div class="text-black">
          {{ model.middleName ? model.middleName : '-' }}
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">Last Name</div>
        <div class="text-black">
          {{ model.lastName }}
        </div>
      </div>
    </div>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">Gender</div>
        <div class="text-black">
          {{ model.gender ? model.gender : '-' }}
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">Date of birth</div>
        <div class="text-black">
          {{ model.dob ? model.dob : '-' }}
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">Email Address</div>
        <div class="text-black">{{ model.primaryEmailAddress }}
        </div>
      </div>
    </div>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">Phone Number</div>
        <div class="text-black">
          {{ model.primaryPhoneNumber ? model.primaryPhoneNumber : '-' }}
        </div>
      </div>
    </div>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-12 col-md-6">
        <div class="q-mb-xs">Profile Picture</div>
        <div>
          <div v-if="model.pictureId" class="row justify-center">
            <img :src="model.virtualPath" alt="" style="width: 50%">
          </div>
        </div>
      </div>
    </div>
  </fieldset>
  <fieldset v-if="model.person" class="q-mt-lg">
    <legend>Primary Address Info</legend>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">Address Type</div>
        <div class="text-black">
          {{ model.addressType ? model.addressType : '-' }}
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">Address 1</div>
        <div class="text-black">
          {{ model.addressLine1 }}
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">Address 2</div>
        <div class="text-black">
          {{ model.addressLine2 ? model.addressLine2 : '-' }}
        </div>
      </div>
    </div>
    <div class="row q-col-gutter-x-md q-mb-md">
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">State</div>
        <div class="text-black">
          {{ model.state ? model.state : '-' }}
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">City</div>
        <div class="text-black">
          {{ model.city ? model.city : '-' }}
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }}</div>
        <div class="text-black">
          {{ model.zipCode ? model.zipCode : '-' }}
        </div>
      </div>
    </div>
  </fieldset>
  <fieldset v-if="model.person" class="q-mt-lg">
    <legend>Other Info</legend>
    <div class="row q-col-gutter-x-md q-mb-lg">
      <div class="col-12 col-md-4">
        <div class="q-mb-xs"><label>Identified Date</label></div>
        <div class="form-group text-black">
          {{ model.identifiedDateStr ? model.identifiedDateStr : '-' }}
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">Identifier</div>
        <div class="text-black">
          {{ model.identifiedByName ? model.identifiedByName : '-' }}
        </div>
      </div>
    </div>
    <div class="row q-col-gutter-x-md q-mb-lg">
      <div class="col-12">
        <div class="q-mb-xs"><label>Identification Note</label></div>
        <div class="form-group text-black RichTextEditor">
          <p v-html="model.identificationNote ? model.identificationNote : '-'" />
        </div>
      </div>
    </div>
  </fieldset>
  <fieldset v-if="model.person" class="q-mt-lg">
    <legend>Emergency Contact Info</legend>
    <div class="row q-col-gutter-x-md q-mb-sm">
      <div class="col-12 col-md-4">
        <div class="q-mb-xs"><label>Relation</label></div>
        <div class="form-group text-black">
          {{ model.relation ? model.relation : '-' }}
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="q-mb-xs">Full Name</div>
        <div class="text-black">
          {{ model.relationFullName ? model.relationFullName : '-' }}
        </div>
      </div>
      <div class="col-12 col-md-4">
        <div class="q-mb-xs"><label>Phone Number</label></div>
        <div class="form-group text-black">
          <p v-html="model.phoneNumber ? model.phoneNumber : '-'" />
        </div>
      </div>
    </div>
  </fieldset>
</template>
<script setup>
import { ref, watch, onMounted } from "vue";
import _ from "lodash";
import customerService from "modules/customer/customer.service";
// const baseURL = process.env.API_BASE_URL;
const props = defineProps({ customerId: { type: String, default: "" }, isPerson: { type: Boolean, default: false } });
const model = ref({
  person: props.isPerson
});
const loading = ref(true);
const customerId = props.customerId;
const rows = ref([]);

const search = ref({
  customerId
});

const getCompanyDetails = () => {
  loading.value = true;
  customerService.getCustomerDetails(customerId).then((resp) => {
    model.value = _.cloneDeep(resp);
    search.value.companyId = resp.companyId;
    model.value.customerType = resp.customerType?.dropDownValue || null;
    // Set company-related details
    if (resp.company) {
      model.value.name = resp.company.name;
      model.value.website = resp.company.website;
      model.value.businessTypeId = resp.company.businessType?.dropDownValue || "";
      model.value.phoneNumber = resp.company.phoneNumber;
      model.value.alternativeEmailAddress = resp.company.alternativeEmailAddress;
      model.value.emailAddress = resp.company.emailAddress;
      model.value.alternativePhoneNumber = resp.company.alternativePhoneNumber;
      model.value.phoneNumber = resp.company.phoneNumber;
      model.value.createdOnUtc = resp.company.createdOnUtc;
      model.value.updatedOnUtc = resp.company.createdOnUtc;
      if (resp.company.createdBy) {
        model.value.createdBy = resp.company.createdBy.person.firstName + " " + resp.company.createdBy.person.lastName;
      }
      if (resp.company.updatedBy) {
        model.value.updatedBy = resp.company.updatedBy.person.firstName + " " + resp.company.updatedBy.person.lastName;
      }
      // Set company address
      if (resp.company.address) {
        model.value.addressLine1 = resp.company.address.addressLine1 || "";
        model.value.addressLine2 = resp.company.address.addressLine2 || "";
        model.value.countryName = resp.company.address.addressCountry?.name || "";
        model.value.stateProvinceName = resp.company.address.addressStateProvince?.name || "";
        model.value.city = resp.company.address.city || "";
        model.value.zipCode = resp.company.address.zipCode || "";
      }
      // Set employee details
      if (resp.company.employee?.person) {
        model.value.employeeName = resp.company.employee.person.firstName + " " + resp.company.employee.person.lastName || "";
      }
      // Set company contacts
      rows.value = resp.company.companyContacts?.map(contact => ({
        ...contact,
        editing: false,
        companyId: model.value.companyId,
        flag: "Edit"
      })) || [];
    }

    // Set person details
    if (resp.person) {
      model.value.name = resp.person.firstName + " " + resp.person.lastName || "";
      model.value.firstName = resp.person.firstName;
      model.value.middleName = resp.person.middleName;
      model.value.lastName = resp.person.lastName;
      model.value.primaryPhoneNumber = resp.person.primaryPhoneNumber || "";
      model.value.primaryEmailAddress = resp.person.primaryEmailAddress || "";
      model.value.identifiedDateStr = resp.person.identifiedDate;
      model.value.identifiedByName = resp.person.identifiedByName || "";
      model.value.identificationNote = resp.person.identificationNote || "";
      model.value.relation = resp.person.relation || "";
      model.value.relationFullName = resp.person.relationFullName || "";
      model.value.phoneNumber = resp.person.phoneNumber;
      model.value.dob = resp.person.dob;
      model.value.gender = resp.person.gender ? resp.person.gender.dropDownValue : "";
      model.value.addressType = resp.person.addressType ? resp.person.addressType.dropDownValue : "";

      // Set personal address
      if (resp.person.address) {
        model.value.addressLine1 = resp.person.address.addressLine1 || "";
        model.value.addressLine2 = resp.person.address.addressLine2 || "";
        model.value.countryName = resp.person.address.addressCountry?.name || "";
        model.value.stateProvinceName = resp.person.address.addressStateProvince?.name || "";
        model.value.city = resp.person.address.city || "";
        model.value.zipCode = resp.person.address.zipCode || "";
      }
    }
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => customerId, (newValue, oldValue) => {
  if (newValue) {
    getCompanyDetails();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  getCompanyDetails();
});
</script>
