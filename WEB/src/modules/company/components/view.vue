<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none dialog-scrollable-content" full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important; max-width: 50vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">View Company</div>
        <q-btn v-close-popup icon="o_close" color="white" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Company Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Company Name</div>
                <div class="text-black">{{ model.name }}</div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Website</div>
                <div class="text-black">{{ model.website }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-12">
                <div class="q-mb-xs">Profile Link</div>
                <div class="text-black">{{ model.profileLink || '-' }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Business Type</div>
                <div class="text-black">{{ model.businessTypeId }}</div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Primary Contact</div>
                <div class="text-black">{{ model.employeeName }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Email</div>
                <div class="text-black">{{ model.emailAddress || '-' }}</div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Alt. Email</div>
                <div class="text-black">{{ model.alternativeEmailAddress || '-' }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Phone Number</div>
                <div class="text-black">{{ model.phoneNumber || '-' }}</div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Alt. Phone Number</div>
                <div class="text-black">{{ model.alternativePhoneNumber || '-' }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-12 col-md-12">
                <div class="q-mb-xs">Description</div>
                <div class="text-black RichTextEditor">
                  <p v-html="model.description ? model.description : '-'" />
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <legend>Company Address</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Address 1</div>
                <div class="text-black">{{ model.addressLine1 }}</div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Address 2</div>
                <div class="text-black">{{ model.addressLine2 || '-' }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Country</div>
                <div class="text-black">{{ model.countryName }}</div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">State</div>
                <div class="text-black">{{ model.stateProvinceName }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">City</div>
                <div class="text-black">{{ model.city }}</div>
              </div>
              <div class="col-12 col-md-6">
                <div class="q-mb-xs">Zip Code</div>
                <div class="text-black">{{ model.zipCode }}</div>
              </div>
            </div>
          </fieldset>
          <fieldset class="hidden">
            <legend>Other Info</legend>
            <div class="row q-col-gutter-x-md">
              <div class="col-md-3">
                <div class="form-group">
                  <p>Start Date of Service Provided</p>
                </div>
              </div>
              <div class="col-md-3">
                <div class="form-group">
                  <p class="text-black">{{ model.serviceProviderDate }}</p>
                </div>
              </div>
              <div class="col-md-3">
                <div class="form-group">
                  <p>Created Date</p>
                </div>
              </div>
              <div class="col-md-3">
                <div class="form-group">
                  <p class="text-black">{{ model.comapnyCreatedDate }}</p>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-md-3">
                <div class="form-group">
                  <p>Service Provided</p>
                </div>
              </div>
              <div class="col-md-3">
                <div class="form-group">
                  <p class="text-black">{{ model.serviceProvidedDetails }}</p>
                </div>
              </div>
              <div class="col-md-3">
                <div class="form-group">
                  <p>Product</p>
                </div>
              </div>
              <div class="col-md-3">
                <div class="form-group">
                  <p class="text-black">{{ model.productDetails }}</p>
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset class="q-mb-lg">
            <legend>Contact Details</legend>
            <q-table
              ref="tableRef"
              v-model:pagination="pagination"
              bordered
              class="no-shadow"
              :loading="loading"
              :rows="rows"
              :columns="contactColumns"
              row-key="id"
              no-data-label="No data available"
              binary-state-sort
              :rows-per-page-options="[20, 50, 100, 200, 500] "
            >
              <template #header="contactProps">
                <q-tr :props="contactProps" class="bg-primary text-white">
                  <q-th v-for="col in contactProps.cols" :key="col.name" :props="contactProps">{{ col.label }}</q-th>
                </q-tr>
              </template>
              <template #body="contactProps">
                <q-tr :props="contactProps">
                  <q-td>{{ contactProps.row.person?.firstName || '' }}</q-td>
                  <q-td>{{ contactProps.row.person?.lastName || '' }}</q-td>
                  <q-td>{{ contactProps.row.person?.primaryEmailAddress || '' }}</q-td>
                  <q-td>{{ contactProps.row.alternateEmail || '' }}</q-td>
                  <q-td>{{ contactProps.row.person?.primaryPhoneNumber || '' }}</q-td>
                  <q-td>{{ contactProps.row.alternatePhoneNumber || '' }}</q-td>
                </q-tr>
              </template>
            </q-table>
          </fieldset>
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
import companyService from "modules/company/company.service";

const props = defineProps({ id: { type: String, default: "" } });
defineEmits([...useDialogPluginComponent.emits]);

const loading = ref(true);
// const processing = ref(false);
const states = ref([]);
const Country = ref([]);
const rows = ref([]);

const model = ref({
  description: ""
});

const contactColumns = ref([
  { name: "person.firstName", label: "First Name", field: "person.firstName", align: "left", sortable: true },
  { name: "person.lastName", label: "Last Name", field: "person.lastName", align: "left", sortable: true, style: "width: 10px" },
  { name: "person.primaryEmailAddres", label: "Email", field: "person.primaryEmailAddres", align: "left" },
  { name: "alternateEmail", label: "Alt. Email", field: "alternateEmail", align: "left" },
  { name: "person.primaryPhoneNumber", label: "Phone Number", field: "person.primaryPhoneNumber", align: "left" },
  { name: "alternatePhoneNumber", label: "Alt. Phone Number", field: "alternatePhoneNumber", align: "left" }
]);

const getCountry = () => {
  loading.value = true;
  commonService.getCountry(model.value.countryId).then((resp) => {
    Country.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

const getCompany = () => {
  loading.value = true;
  companyService.getCompanyDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    // addressRow.value = _.cloneDeep(resp.address);
    model.value.businessTypeId = resp.businessType.dropDownValue;
    model.value.addressLine1 = resp.address.addressLine1;
    model.value.addressLine2 = resp.address.addressLine2;
    model.value.countryName = resp.address.addressCountry.name;
    model.value.stateProvinceName = resp.address.addressStateProvince.name;
    model.value.city = resp.address.city;
    model.value.zipCode = resp.address.zipCode;
    model.value.employeeName = resp.employee.person.fullName;
    rows.value = resp.companyContacts.map(contact => ({
      ...contact,
      editing: false,
      personId: contact.person.id,
      firstName: contact.person.firstName,
      lastName: contact.person.lastName,
      emailAddress: contact.person.primaryEmailAddress,
      phoneNumber: contact.person.primaryPhoneNumber,
      alternateEmail: contact.alternateEmail,
      alternatePhoneNumber: contact.alternatePhoneNumber,
      companyId: model.value.companyId,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

function getStates () {
  commonService.getStates(model.value.countryId).then((resp) => {
    states.value = resp;
  });
}

// function getCompanyContacts () {
//   companyService.getCompanyContacts(props.id).then((resp) => {
//     rows.value = resp;
//   });
// }

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

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getCompany();
    // getCompanyContacts();
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
