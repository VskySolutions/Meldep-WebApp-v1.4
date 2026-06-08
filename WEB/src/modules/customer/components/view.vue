<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw !important; max-width: 60vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">View Customer</div>
        <q-btn v-close-popup icon="o_close" color="white" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
            <q-tab name="1_tab" :label="model.person ? 'Person Info' : 'Company Info'" class="q-px-lg q-mr-md" />
            <q-tab v-if="!model.person" name="2_tab" label="Company Contacts" class="q-px-lg" />
          </q-tabs>
          <q-tab-panels v-model="tab" animated>
            <q-tab-panel name="1_tab">
              <fieldset v-if="model.company">
                <!-- <legend>Company Info</legend> -->
                 <div class="row q-col-gutter-x-md">
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Customer Type</div>
                      <div class="text-black q-mb-sm">{{ model.customerType ? model.customerType : "-" }}</div>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Parent Customer</div>
                      <div class="text-black q-mb-sm">{{ model.parentCustomerName ? model.parentCustomerName : "-" }}</div>
                    </div>
                  </div>
                 </div>
                 <div class="row q-col-gutter-x-md">
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Advocate Assigned Date</div>
                      <div class="text-black q-mb-sm">{{ model.assignedDate ? model.assignedDate : "-" }}</div>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Customer Advocate</div>
                      <div class="text-black q-mb-sm">{{ model.assignedTo?.person.firstName ? model.assignedTo?.person.firstName + " " + model.assignedTo?.person.lastName : "-" }}</div>
                    </div>
                  </div>
                 </div>
                <div class="row q-col-gutter-x-md">
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Name</div>
                      <div class="text-black q-mb-sm">{{ model.name ? model.name : "-" }}</div>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Website</div>
                      <a :href="model.website" target="_blank" class="text-bluee">
                        {{ model.website ? model.website : "-" }}
                      </a>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Business Type</div>
                      <div class="text-black q-mb-sm">{{ model.businessTypeId ? model.businessTypeId : "-" }}</div>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Primary Employee Person</div>
                      <div class="text-black q-mb-sm">{{ model.employeeName ? model.employeeName : "-" }}</div>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Email</div>
                      <a v-if="model.emailAddress" :href="'mailto:' + model.emailAddress">
                        {{ model.emailAddress }}
                      </a>
                      <a v-else>-</a>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Alt. Email</div>
                      <a v-if="model.alternativeEmailAddress" :href="'mailto:' + model.alternativeEmailAddress">
                        {{ model.alternativeEmailAddress }}
                      </a>
                      <a v-else>-</a>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Phone Number</div>
                      <a v-if="model.phoneNumber" :href="'tel:' + model.phoneNumber">
                        {{ model.phoneNumber }}
                      </a>
                      <a v-else>-</a>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Alt. Phone Number</div>
                      <a v-if="model.alternativePhoneNumber" :href="'tel:' + model.alternativePhoneNumber">
                        {{ model.alternativePhoneNumber }}
                      </a>
                      <a v-else>-</a>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Address 1</div>
                      <div class="text-black q-mb-sm">{{ model.addressLine1 ? model.addressLine1 : '-'}}</div>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Address 2</div>
                      <div class="text-black q-mb-sm">{{ model.addressLine2 ? model.addressLine2 : "-" }}</div>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">Country</div>
                      <div class="text-black q-mb-sm">{{ model.countryName ? model.countryName : '-' }}</div>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">State</div>
                      <div class="text-black q-mb-sm">{{ model.stateProvinceName ? model.stateProvinceName : '-' }}</div>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">City</div>
                      <div class="text-black q-mb-sm">{{ model.city ? model.city : '-' }}</div>
                    </div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <div class="q-mb-xs">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }}</div>
                      <div class="text-black q-mb-sm">{{ model.zipCode ? model.zipCode : '-' }}</div>
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md">
                  <div class="col-12 col-md-6">
                    <div>Created By</div>
                    <div class="text-black q-mb-sm">{{ model.createdBy }}</div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div>Created Date</div>
                    <div class="text-black q-mb-sm">{{ model.createdOnUtc }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md">
                  <div class="col-12 col-md-6">
                    <div>Updated By</div>
                    <div class="text-black q-mb-sm">{{ model.updatedBy }}</div>
                  </div>
                  <div class="col-12 col-md-6">
                    <div>Updated Date</div>
                    <div class="text-black q-mb-sm">{{ model.updatedOnUtc }}</div>
                  </div>
                </div>
              </fieldset>
              <fieldset v-if="model.person">
                <legend>Basic Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-4">
                    <div class="q-mb-xs">First Name</div>
                    <div class="text-black">
                      {{ model.firstName ? model.firstName : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-md-4">
                    <div class="q-mb-xs">Middle Name</div>
                    <div class="text-black">
                      {{ model.middleName ? model.middleName : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-md-4">
                    <div class="q-mb-xs">Last Name</div>
                    <div class="text-black">
                      {{ model.lastName ? model.lastName : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-4" >
                    <div class="q-mb-xs">Gender</div>
                    <div class="text-black">
                      {{ model.gender ? model.gender : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-md-4" >
                    <div class="q-mb-xs">Date of birth</div>
                    <div class="text-black">
                      {{ model.dob ? model.dob : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-md-4" >
                    <div class="q-mb-xs">Email Address</div>
                      <a v-if="model.primaryEmailAddress" :href="'mailto:' + model.primaryEmailAddress">
                        {{ model.primaryEmailAddress }}
                      </a>
                      <a v-else>-</a>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-4">
                    <div class="q-mb-xs">Phone Number</div>
                    <div class="text-black">
                      <a v-if="model.primaryPhoneNumber" :href="'tel:' + model.primaryPhoneNumber">
                        {{ model.primaryPhoneNumber }}
                      </a>
                      <a v-else>-</a>
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
                      <div v-else>-</div>
                    </div>
                  </div>
                </div>
              </fieldset>
              <fieldset class="q-mt-lg" v-if="model.person">
                <legend>Primary Address Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-4" >
                    <div class="q-mb-xs">Address Type</div>
                    <div class="text-black">
                      {{ model.addressType ? model.addressType : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-md-4" >
                    <div class="q-mb-xs">Address 1</div>
                    <div class="text-black">
                      {{ model.addressLine1 ? model.addressLine1 : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-md-4">
                    <div class="q-mb-xs">Address 2</div>
                    <div class="text-black">
                      {{ model.addressLine2 ? model.addressLine2 : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-4" >
                    <div class="q-mb-xs">Country</div>
                    <div class="text-black">
                      {{ model.country ? model.country : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-md-4" >
                    <div class="q-mb-xs">State</div>
                    <div class="text-black">
                      {{ model.state ? model.state : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-md-4">
                    <div class="q-mb-xs">City</div>
                    <div class="text-black">
                      {{ model.city ? model.city : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-md-4">
                    <div class="q-mb-xs">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }}</div>
                    <div class="text-black">
                      {{ model.zipCode ? model.zipCode : "-" }}
                    </div>
                  </div>
                </div>
              </fieldset>
              <fieldset class="q-mt-lg" v-if="model.person">
              <legend>Other Info</legend>
                <div class="row q-col-gutter-x-md q-mb-lg">
                  <div class="col-12 col-md-4">
                    <div class="q-mb-xs"><label>Identified Date</label></div>
                    <div class="form-group text-black">
                      {{ model.identifiedDateStr ? model.identifiedDateStr : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-md-4">
                    <div class="q-mb-xs">Identifier</div>
                    <div class="text-black">
                      {{ model.identifiedByName ? model.identifiedByName : "-" }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-lg">
                  <div class="col-12">
                    <div class="q-mb-xs"><label>Identification Note</label></div>
                    <div class="form-group text-black RichTextEditor">
                      <p v-html="model.identificationNote ? model.identificationNote : '-' "></p>
                    </div>
                  </div>
                </div>
              </fieldset>
              <fieldset class="q-mt-lg" v-if="model.person">
              <legend>Emergency Contact Info</legend>
                <div class="row q-col-gutter-x-md q-mb-sm">
                  <div class="col-12 col-md-4">
                    <div class="q-mb-xs"><label>Relation</label></div>
                    <div class="form-group text-black">
                      {{ model.relation ? model.relation : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-md-4">
                    <div class="q-mb-xs">Full Name</div>
                    <div class="text-black">
                      {{ model.relationFullName ? model.relationFullName : "-" }}
                    </div>
                  </div>
                  <div class="col-12 col-md-4">
                    <div class="q-mb-xs"><label>Phone Number</label></div>
                    <div class="form-group text-black">
                      {{ model.phoneNumber ? model.phoneNumber : "-" }}
                    </div>
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="2_tab">
              <div>
                <!-- <legend>Contact Details</legend> -->
                <q-table
                  :rows="rows"
                  :columns="contactColumns"
                  row-key="id"
                  :rows-per-page-options="[20, 50, 100, 200, 500]"
                >
                <template #header="contactProps">
                  <q-tr :props="contactProps" class="bg-primary text-white">
                    <q-th v-for="col in contactProps.cols" :key="col.name" :props="contactProps">{{ col.label }}</q-th>
                  </q-tr>
                </template>
                <template #body="contactProps">
                  <q-tr :props="contactProps">
                    <q-td>{{ contactProps.row.firstName || '-' }}</q-td>
                    <q-td>{{ contactProps.row.lastName || '-' }}</q-td>
                    <q-td>{{ contactProps.row.email || '-' }}</q-td>
                    <q-td>{{ contactProps.row.alternateEmail || '-' }}</q-td>
                    <q-td>{{ contactProps.row.phoneNumber || '-' }}</q-td>
                    <q-td>{{ contactProps.row.alternatePhoneNumber || '-' }}</q-td>
                  </q-tr>
                </template>
                </q-table>
              </div>
            </q-tab-panel>
          </q-tab-panels>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent } from "quasar";
import { ref, watch } from "vue";
import _ from "lodash";
import commonService from "services/common.service";
import customerService from "modules/customer/customer.service";

// define props
const props = defineProps({ id: { type: String, default: "" } });

// define emits
defineEmits([...useDialogPluginComponent.emits]);

// common variables
const tab = ref("1_tab");
const loading = ref(true);
const states = ref([]);
const Country = ref([]);
const rows = ref([]);
// const baseURL = process.env.API_BASE_URL;
const baseCountryId = process.env.BASE_COUNTRY_ID;

// define model
const model = ref({
  description: ""
});

// contact columns
const contactColumns = ref([
  { name: "firstName", label: "First Name", field: "firstName", align: "left", sortable: true },
  { name: "lastName", label: "Last Name", field: "lastName", align: "left", sortable: true },
  { name: "primaryEmailAddress", label: "Email", field: "primaryEmailAddress", align: "left" },
  { name: "alternateEmail", label: "Alt. Email", field: "alternateEmail", align: "left" },
  { name: "primaryPhoneNumber", label: "Phone Number", field: "primaryPhoneNumber", align: "left" },
  { name: "alternatePhoneNumber", label: "Alt. Phone Number", field: "alternatePhoneNumber", align: "left" }
]);

// getCompanyDetails
const getCompanyDetails = () => {
  loading.value = true;
  customerService.getCustomerDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.customerType = resp.customerType?.dropDownValue || null;
    model.value.parentCustomerName = resp.parentCustomerName || null;
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
      model.value.createdBy = resp.company.createdBy?.person?.firstName + " " + resp.company.createdBy?.person?.lastName;
      model.value.updatedBy = resp.company.updatedBy?.person?.firstName + " " + resp.company.updatedBy?.person?.lastName;
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
        model.value.employeeId = resp.company.employee.id;
        model.value.employeeName = resp.company.employee.person.firstName + " " + resp.company.employee.person.lastName || "";
      }
      rows.value = resp.company.companyContacts?.map(contact => ({
        id: contact.id,
        firstName: contact.person?.firstName || "",
        lastName: contact.person?.lastName || "",
        email: contact.person?.primaryEmailAddress || "",
        phoneNumber: contact.person?.primaryPhoneNumber || "",
        alternateEmail: contact.alternateEmail,
        alternatePhoneNumber: contact.alternatePhoneNumber,
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
      model.value.identifiedByName = resp.person.identifiedBy ? resp.person.identifiedBy?.firstName + " " + resp.person.identifiedBy?.lastName : "";
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

// get country
const getCountry = () => {
  loading.value = true;
  commonService.getCountry(model.value.countryId).then((resp) => {
    Country.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => model.value.stateProvinceId, (newValue) => {
  if (newValue) {
    getStates();
  }
}, { immediate: true });

watch(() => model.value.countryId, (newValue) => {
  if (newValue) {
    getCountry();
  }
}, { immediate: true });

function getStates () {
  commonService.getStates(model.value.countryId).then((resp) => {
    states.value = resp;
  });
}

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getCompanyDetails();
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
