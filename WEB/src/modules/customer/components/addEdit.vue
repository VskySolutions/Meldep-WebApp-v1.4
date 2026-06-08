<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 90vw; max-width: 90vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Customer</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogClose()" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend v-if="selectedCustomerType === 'business' || selectedCustomerType === 'joint family'">Company Info</legend>
              <legend v-if="selectedCustomerType === 'individual'">Person Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-6 col-md-3 col-lg-3">
                  <formSingleSelectDropdown
                    v-model="model.customerTypeId"
                    label="Customer Type"
                    :readonly="readonlyCustomer!= '' ? '' : 'readonlyCustomer'"
                    :options="customerTypesDropdownSingleSelect.list.value"
                    :filter="customerTypesDropdownSingleSelect.filter"
                    :error="v$.customerTypeId.$error"
                    :error-message="v$.customerTypeId.$errors[0]?.$message"
                  />
                </div>
                <div v-if="model.customerTypeId" class="col-6 col-md-3 col-lg-3">
                  <formDate
                    v-model="model.assignedDate"
                    label="Advocate Assigned Date"
                    :error="v$.assignedDate.$error"
                    :error-message="v$.assignedDate.$errors[0]?.$message"
                  />
                </div>
                <div v-if="model.customerTypeId" class="col-6 col-md-3 col-lg-3">
                  <formSingleSelectDropdown
                    v-model="model.assignedToId"
                    label="Customer Advocate"
                    :required="false"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                  />
                </div>
                <div class="col-6 col-md-3 col-lg-3">
                  <formSingleSelectDropdown
                    v-model="model.parentCustomerId"
                    label="Parent Customer"
                    :required="false"
                    :options="parentCustomerDropdownSingleSelect.list.value"
                    :filter="parentCustomerDropdownSingleSelect.filter"
                  />
                </div>
              </div>
              <div v-if="selectedCustomerType === 'business' || selectedCustomerType === 'joint family'">
                <fieldset>
                  <legend>Basic Info</legend>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-md-4">
                      <label v-if="selectedCustomerType === 'business'" class="q-mb-xs text-black">Company Name <span class="required">*</span></label>
                      <label v-else class="q-mb-xs text-black">Family Name <span class="required">*</span></label>
                      <formSingleSelectDropdown
                        v-model="model.companyId"
                        :options="companyNameDropdownSingleSelect.list.value"
                        :filter="companyNameDropdownSingleSelect.filter"
                        :readonly="readonlyCustomer!= '' ? '' : 'readonlyCustomer'"
                        :error="v$.companyId.$error"
                        :error-message="v$.companyId.$errors[0]?.$message"
                      >
                        <template #after>
                          <q-icon
                            v-if="!readonlyCustomer"
                            name="fa-solid fa-user-plus"
                            color="primary"
                            class="cursor-pointer q-ml-sm"
                            @click="onAddCompany(selectedCustomerType)"
                          >
                            <q-tooltip>Add new company</q-tooltip>
                          </q-icon>
                        </template>
                      </formSingleSelectDropdown>
                    </div>
                  </div>
                  <div v-if="model.companyId">
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Name : {{ model.name }}</div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Website : {{ model.website }}</div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Business Type : {{ model.businessTypeId }}</div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Primary Employee Person : {{ model.employeeId }}</div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Email : {{ model.emailAddress }}<q-badge v-if="!model.emailAddress" color="red-4" square outline>No Data</q-badge></div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Alt. Email : {{ model.alternativeEmailAddress }}<q-badge v-if="!model.alternativeEmailAddress" color="red-4" square outline>No Data</q-badge></div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Phone No : {{ model.phoneNumber }}<q-badge v-if="!model.phoneNumber" color="red-4" square outline>No Data</q-badge></div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Alt. Phone No : {{ model.alternativePhoneNumber }}<q-badge v-if="!model.alternativePhoneNumber" color="red-4" square outline>No Data</q-badge>
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Address 1 : {{ model.addressLine1 }}</div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Address 2 : {{ model.addressLine2 }}<q-badge v-if="!model.addressLine2" color="red-4" square outline>No Data</q-badge></div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Country : {{ model.country }}</div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">State : {{ model.stateProvinceId }}</div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">City : {{ model.city }}</div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Zip Code : {{ model.zipCode }}</div>
                      </div>
                    </div>
                  </div>
                </fieldset>
              </div>
            </fieldset>
            <fieldset v-if="model.companyId && (selectedCustomerType === 'business' || selectedCustomerType === 'joint family')">
              <legend v-if="model.companyId && selectedCustomerType === 'business'">Company Contacts</legend>
              <legend v-if="model.companyId && selectedCustomerType === 'joint family'">Family Contacts</legend>
              <div class="flex items-center justify-end q-my-xs">
                <q-btn
                  color="primary"
                  icon="o_add"
                  label="Add Contact"
                  no-caps
                  @click="onAdd"
                />
              </div>
              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                virtual-scroll
                bordered
                class="no-shadow"
                :loading="loading"
                :rows="rows"
                :columns="columns"
                row-key="id"
                separator="cell"
                no-data-label="No data available"
                binary-state-sort
                :rows-per-page-options="[20, 50, 100, 200, 500]"
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :class="props.row.deleted ? 'hidden row-highlight' : 'row-highlight'">
                    <q-td style="width: 30%;">
                      <formSingleSelectDropdown
                        v-model="props.row.personId"
                        :options="personNameDropdownSingleSelect.list.value"
                        :filter="personNameDropdownSingleSelect.filter"
                        :error="rowValidations[props.rowIndex]?.value?.personId.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.personId.$errors[0]?.$message"
                        @update:model-value="val => onPersonSelected(val, props.row)"
                      >
                        <template #after>
                          <q-icon
                            name="fa-solid fa-user-plus"
                            color="primary"
                            class="cursor-pointer q-ml-xs"
                            @click="handleAddPerson(props.row, refreshPersonNameDropdown)"
                          >
                            <q-tooltip>Add new person</q-tooltip>
                          </q-icon>
                        </template>
                      </formSingleSelectDropdown>
                    </q-td>
                    <q-td>
                      {{ props.row.firstName }}
                    </q-td>
                    <q-td>
                      {{ props.row.lastName }}
                    </q-td>
                    <q-td>
                      {{ props.row.emailAddress }}
                    </q-td>
                    <q-td>
                      {{ props.row.phoneNumber }}
                    </q-td>
                    <q-td>
                      <q-input
                        v-model="props.row.alternateEmail"
                        outlined
                        hide-bottom-space
                        :dense="true"
                        maxlength="128"
                        :error="rowValidations[props.rowIndex]?.value?.alternateEmail.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.alternateEmail.$errors[0]?.$message"
                        @blur="rowValidations[props.rowIndex]?.value?.alternateEmail.$touch"
                      />
                    </q-td>
                    <q-td>
                      <div v-if="model.countryId === baseCountryId" class="col">
                        <div class="form-group">
                          <q-input
                            v-model="props.row.alternatePhoneNumber"
                            outlined
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            mask="(###)-###-####"
                            :error="rowValidations[props.rowIndex]?.value?.alternatePhoneNumber.$error"
                            :error-message="rowValidations[props.rowIndex]?.value?.alternatePhoneNumber.$errors[0]?.$message"
                            @blur="rowValidations[props.rowIndex]?.value?.alternatePhoneNumber.$touch"
                          />
                        </div>
                      </div>
                      <div v-else class="col">
                        <div class="form-group">
                          <q-input
                            v-model="props.row.alternatePhoneNumber"
                            outlined
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            mask="##########"
                          />
                        </div>
                      </div>
                    </q-td>
                    <q-td class="text-center" style="width: 5%;">
                      <q-icon
                        name="o_delete_outline"
                        size="xs"
                        class="cursor-pointer"
                        color="negative"
                        @click="deleteContactRow(props.rowIndex)"
                      >
                        <q-tooltip>Delete</q-tooltip>
                      </q-icon>
                    </q-td>
                  </q-tr>
                </template>
              </q-table>
            </fieldset>
            <div v-if="selectedCustomerType === 'individual'">
              <fieldset>
                <legend>Basic Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-md-4">
                    <formSingleSelectDropdown
                      v-model="model.personId"
                      label="Person Name"
                      :readonly="readonlyCustomer!= '' ? '' : 'readonlyCustomer'"
                      :options="personNameDropdownSingleSelect.list.value"
                      :filter="personNameDropdownSingleSelect.filter"
                      :error="v$.personId.$error"
                      :error-message="v$.personId.$errors[0]?.$message"
                    >
                      <template #after>
                        <q-icon
                          v-if="!readonlyCustomer"
                          name="fa-solid fa-user-plus"
                          color="primary"
                          class="cursor-pointer q-ml-sm"
                          @click="handleAddPerson(null, refreshPersonNameDropdown)"
                        >
                          <q-tooltip>Add new person</q-tooltip>
                        </q-icon>
                      </template>
                    </formSingleSelectDropdown>
                  </div>
                </div>
                <div v-if="model.personId">
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-sm-6 col-md-3">
                      <div class="q-mb-xs text-black">First Name : {{ model.firstName }}</div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-3">
                      <div class="q-mb-xs text-black">Middle Name : {{ model.middleName }}</div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-3">
                      <div class="q-mb-xs text-black">Last Name : {{ model.lastName }}</div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-3">
                      <div class="q-mb-xs text-black">Gender : {{ model.genderId }}</div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-sm-6 col-md-3">
                      <div class="q-mb-xs text-black">Date Of Birth : {{ model.dob }}</div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-3">
                      <div class="q-mb-xs text-black">Email Address : {{ model.primaryEmailAddress }}</div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-3">
                      <div class="q-mb-xs text-black">Country : {{ model.country }}</div>
                    </div>
                    <div class="col-12 col-sm-6 col-md-3">
                      <div class="q-mb-xs text-black">Phone Number : {{ model.primaryPhoneNumber }}</div>
                    </div>
                  </div>
                  <div class="row q-col-gutter-x-md q-mb-md">
                    <div class="col-12 col-md-6">
                      <div class="q-mb-xs text-black">Profile Picture : </div>
                      <div class="row justify-center">
                        <img :src="model.virtualPath" alt="" style="width: 40%;">
                      </div>
                    </div>
                  </div>
                </div>
              </fieldset>
              <fieldset v-if="model.personId" class="q-mt-lg">
                <legend>Primary Address Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-3">
                    <div class="q-mb-xs text-black">Address Type : {{ model.addressTypeId }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-3">
                    <div class="q-mb-xs text-black">Address 1 :  {{ model.addressLine1 }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-3">
                    <div class="q-mb-xs text-black">Address 2 : {{ model.addressLine2 }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-3">
                    <div class="q-mb-xs text-black">State :  {{ model.stateProvinceId }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-3">
                    <div class="q-mb-xs text-black">City :  {{ model.city }} </div>
                  </div>
                  <div class="q-mb-xs text-black">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }} : {{ model.zipCode }}</div>
                </div>
              </fieldset>
            </div>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogClose" />
          <q-btn v-if="tab !== '4_tab'" color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useQuasar, useDialogPluginComponent, uid } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers, email } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import { notifySuccess, notifyError, zwConfirmLeave } from "assets/utils";
import { isDate } from "validators/zw_validators.js";
import _ from "lodash";
import { format } from "date-fns"; // Standard TimeZone Conversion

import companyService from "modules/company/company.service";
import personService from "modules/person/person.service";
import customerService from "modules/customer/customer.service";
import addEditCompany from "modules/company/components/addEdit.vue";

// Shared Dropdowns
import customerModule from "src/modules/customer/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import companyModule from "src/modules/company/utils/dropdowns.js";
import personModule from "src/modules/person/utils/dropdowns.js";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// Shared Person Dialogs
import {
  initPersonDialogs,
  onPersonAddAndReturnPersonId
} from "src/modules/person/utils/dialogs.js";

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------
const props = defineProps({ id: { type: String, default: "" }, customerTypeId: { type: String, default: "" }, personId: { type: String, default: "" }, companyId: { type: String, default: "" } });

// ----------------------------------------------------------------------------------------------------------------
// define emits
// ----------------------------------------------------------------------------------------------------------------
const $emit = defineEmits(["hide", "ok"]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const customerId = props.id;
const readonlyCustomer = props.id ? "readonly" : "";
const baseCountryId = process.env.BASE_COUNTRY_ID;
const $q = useQuasar();
const loading = ref(true);
const processing = ref(false);
// const states = ref([]);
// const countries = ref([]);
const rowValidations = ref([]);
const rowCounter = ref(0); // Initialize the counter

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);
const pagination = ref({ sortBy: "createdOnUtc", descending: false, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "personName", label: "Person Name", field: "personName", align: "left", sortable: true },
  { name: "firstName", label: "First Name", field: "firstName", align: "left", sortable: true },
  { name: "lastName", label: "Last Name", field: "lastName", align: "left", sortable: true },
  { name: "emailAddress", label: "Email Address", field: "emailAddress", align: "left", sortable: true },
  { name: "phoneNumber", label: "Phone Number", field: "Phonenumber", align: "left", sortable: true },
  { name: "alternateEmail", label: "Alt. Email", field: "alternateEmail", align: "left", sortable: true },
  { name: "alternatePhoneNumber", label: "Alt. Phone Number", field: "alternatePhoneNumber", align: "left", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  isCustomer: true,
  customerId: null,
  name: "",
  parentCustomerId: null,
  employeeId: null,
  phoneNumber: "",
  emailAddress: "",
  website: "",
  alternativeEmailAddress: "",
  alternativePhoneNumber: "",
  businessTypeId: "",
  city: "",
  countryId: baseCountryId,
  country: "",
  stateProvinceId: null,
  zipCode: "",
  startDateStr: "",
  serviceProvidedDetails: "",
  productDetails: "",
  active: true,
  customerTypeId: "",
  assignedDate: "",
  // for person
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
  identifiedById: null,
  identifiedDateStr: format(new Date(), "MM/dd/yyyy"),
  identificationNote: "",
  relation: "",
  relationFullName: "",
  pictureId: null,
  virtualPath: "",
  personId: null
});

// ----------------------------------------------------------------------------------------------------------------
// get customer, company, person details
// ----------------------------------------------------------------------------------------------------------------
const getCustomerDetails = () => {
  loading.value = true;
  customerService.getCustomerDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rowCounter.value += 1;
    personNameDropdownSingleSelect.load(rowCounter.value);
    // Set Customer Type
    model.value.customerTypeId = props.customerTypeId;
    // Set Company Details
    if (resp.company) {
      model.value.name = resp.company.name;
      model.value.website = resp.company.website;
      model.value.businessTypeId = resp.company.businessType?.id || "";
      model.value.phoneNumber = resp.company.phoneNumber;
      model.value.alternativeEmailAddress = resp.company.alternativeEmailAddress;
      model.value.emailAddress = resp.company.emailAddress;
      model.value.alternativePhoneNumber = resp.company.alternativePhoneNumber;
      // Set Address Details
      if (resp.company.address) {
        model.value.addressLine1 = resp.company.address.addressLine1 || "";
        model.value.addressLine2 = resp.company.address.addressLine2 || "";
        model.value.countryId = resp.company.address.addressCountry?.id || "";
        model.value.country = resp.company.address.addressCountry.name || "";
        model.value.stateProvinceId = resp.company.address.addressStateProvince?.id || "";
        model.value.city = resp.company.address.city || "";
        model.value.zipCode = resp.company.address.zipCode || "";
      }
      // Set Employee Details
      if (resp.company.employee?.person) {
        model.value.employeeId = resp.company.employee.id;
        model.value.employeeName = resp.company.employee.person.fullName || "";
      }
      // Set Company Contacts
      rows.value = resp.company.companyContacts?.map(contact => ({
        ...contact,
        editing: false,
        rowCounter: rowCounter.value,
        personId: contact.person.id,
        firstName: contact.person.firstName,
        lastName: contact.person.lastName,
        emailAddress: contact.person.primaryEmailAddress,
        phoneNumber: contact.person.primaryPhoneNumber,
        alternateEmail: contact.alternateEmail,
        alternatePhoneNumber: contact.alternatePhoneNumber,
        companyId: model.value.companyId,
        flag: "Edit"
      })) || [];
    }
    // Set Person Details (if applicable)
    if (resp.person) {
      model.value.name = `${resp.person.firstName} ${resp.person.lastName}`.trim();
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
      model.value.dob = resp.person.dob;
      model.value.gender = resp.person.gender ? resp.person.gender.dropDownValue : "";
      model.value.addressType = resp.person.addressType ? resp.person.addressType.dropDownValue : "";
      // Set Person Address
      if (resp.person.address) {
        model.value.addressLine1 = resp.person.address.addressLine1 || "";
        model.value.addressLine2 = resp.person.address.addressLine2 || "";
        model.value.countryId = resp.person.address.addressCountry?.id || "";
        model.value.stateProvinceId = resp.person.address.addressStateProvince?.id || "";
        model.value.city = resp.person.address.city || "";
        model.value.zipCode = resp.person.address.zipCode || "";
      }
    }
    // Set Dates
    model.value.startDateStr = resp.serviceProviderDate ? format(resp.serviceProviderDate, "MM/dd/yyyy") : "";
    model.value.createdOnUtc = resp.companyCreatedDate ? format(resp.companyCreatedDate, "MM/dd/yyyy") : "";
  }).finally(() => {
    loading.value = false;
  });
};

// getPersonBy personId
function onPersonSelected (personId, row) {
  if (personId) {
    getPersonById(personId, row);
  }
}

// getPersonBy personId
function getPersonById (personId, row) {
  personService.getPerson(personId).then((resp) => {
    row.personId = personId;
    row.firstName = resp.firstName;
    row.middleName = resp.middleName;
    row.lastName = resp.lastName;
    row.emailAddress = resp.primaryEmailAddress;
    row.phoneNumber = resp.primaryPhoneNumber;
    row.countryId = resp.address.countryId;
  });
}

// getpersonbyID for person details
function getPersonByIdForPersonDetails () {
  personService.getPerson(model.value.personId).then((resp) => {
    model.value.firstName = resp.firstName;
    model.value.middleName = resp.middleName;
    model.value.lastName = resp.lastName;
    model.value.genderId = resp.gender.dropDownValue;
    model.value.primaryEmailAddress = resp.primaryEmailAddress;
    model.value.email = resp.email;
    model.value.countryId = resp.address.countryId;
    model.value.country = resp.address.addressCountry.name;
    model.value.primaryPhoneNumber = resp.primaryPhoneNumber;
    model.value.virtualPath = resp.picture.virtualPath ? resp.picture.virtualPath : "";
    model.value.addressTypeId = resp.addressType.dropDownValue;
    model.value.addressLine1 = resp.address.addressLine1;
    model.value.addressLine2 = resp.address.addressLine2;
    model.value.city = resp.address.city;
    model.value.zipCode = resp.address.zipCode;
    model.value.stateProvinceId = resp.address.addressStateProvince.name;
    model.value.dob = resp.dob;
  });
}

// get company details
function getCompanyDetails () {
  companyService.getCompanyDetails(model.value.companyId).then((resp) => {
    model.value.name = resp.name;
    model.value.website = resp.website;
    model.value.businessTypeId = resp.businessType.dropDownValue;
    model.value.employeeId = resp.employee.person.fullName;
    model.value.emailAddress = resp.emailAddress;
    model.value.alternativeEmailAddress = resp.alternativeEmailAddress;
    model.value.phoneNumber = resp.phoneNumber;
    model.value.alternativePhoneNumber = resp.alternativePhoneNumber;
    model.value.addressLine1 = resp.address.addressLine1;
    model.value.addressLine2 = resp.address.addressLine2;
    model.value.city = resp.address.city;
    model.value.zipCode = resp.address.zipCode;
    model.value.stateProvinceId = resp.address.addressStateProvince.name;
    model.value.country = resp.address.addressCountry.name;
    // Store CompanyContacts Data in rows for the Table
    rows.value = resp.companyContacts.map(contact => ({
      id: contact.id,
      personId: contact.person.id,
      personName: contact.person.firstName + " " + contact.person.lastName,
      firstName: contact.person.firstName,
      lastName: contact.person.lastName,
      emailAddress: contact.person.primaryEmailAddress,
      phoneNumber: contact.person.primaryPhoneNumber,
      alternateEmail: contact.alternateEmail,
      alternatePhoneNumber: contact.alternatePhoneNumber
    }));
  });
}

// ----------------------------------------------------------------------------------------------------------------
// Validate rules
// ----------------------------------------------------------------------------------------------------------------

const rules = {
  personId: { required: helpers.withMessage("Person is required", required) },
  companyId: { required: helpers.withMessage("Name is required", required) },
  assignedDate: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    required: helpers.withMessage("Assigned date is required", required)
  },
  customerTypeId: { required: helpers.withMessage("Customer Type is required", required) }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// editingRowRules for contact
// const maxLengthCountry = baseCountryId === rows.value.countryId ? "14" : "10";
const editingRowRules = {
  personId: { required: helpers.withMessage("Person Name is required", required) },
  alternateEmail: { email: helpers.withMessage("Invalid email", email) },
  alternatePhoneNumber: {
    validLength: helpers.withMessage(
      "Invalid phone number",
      value => !value || value.length === 14
    )
  }
};

// ------------------------------------------------------------------------------------
// All Dropdowns
// ------------------------------------------------------------------------------------

const { activeEmployeesDropdownSingleSelect } = employeeModule();
const { companyNameDropdownSingleSelect } = companyModule();
const { personNameDropdownSingleSelect } = personModule();

const {
  customerTypesDropdownSingleSelect,
  parentCustomerDropdownSingleSelect
} = customerModule();

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initPersonDialogs(props.id);

// ----------------------------------------------------------------------------------------------------------------
// Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshPersonNameDropdown = () => {
  personNameDropdownSingleSelect.load();
}

const handleAddPerson = (row = null, refreshPersonNameDropdown) => {
  onPersonAddAndReturnPersonId(
    refreshPersonNameDropdown,
    (newPersonId) => {
      setTimeout(() => {
        if (row) {
          row.personId = newPersonId;
          getPersonById(newPersonId, row);
          // console.log("row.personId", row.personId);
        } else {
          model.value.personId = newPersonId;
          // console.log("model.personId ", model.value.personId);
        }
      }, 100);
    }
  );
};

const selectedCustomerType = computed(() => {
  const selectedType = customerTypesDropdownSingleSelect.list.value.find(
    item => item.value == model.value.customerTypeId
  );
  return selectedType ? selectedType.text.toLowerCase() : "";
});

// Create company
const onAddCompany = (selectedCustomerType) => {
  $q.dialog({
    component: addEditCompany,
    componentProps: { selectedCustomerType }
  }).onOk(() => {
    companyNameDropdownSingleSelect.load();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Add new contact row
async function onAdd () {
  // Add new row if validation passes or it's the first row
  rows.value.unshift({
    id: uid(),
    personId: "",
    alternateEmail: "",
    alternatePhoneNumber: "",
    deleted: false
  });
}

// ------------------------------------------------------------------------------------
// delete contact row
// ------------------------------------------------------------------------------------
const deleteContactRow = (index) => {
  // Check if more than one row is not deleted
  if (rows.value.filter(row => !row.deleted).length > 1) {
    // Mark the selected row as deleted
    rows.value[index].deleted = true;
  } else {
    // Show error if trying to delete the last row
    notifyError({ message: "Please add at least one row." });
  }
};

// save customer data
async function onSubmit () {
  try {
    let filteredV$ = v$;
    let isValid = true;
    const nonDeletedRows = rows.value.filter(row => !row.deleted);
    rowValidations.value = nonDeletedRows.map((row) =>
      useVuelidate(editingRowRules, row, { $lazy: true, $autoDirty: true })
    );
    // Validate each row
    for (const [index, validation] of rowValidations.value.entries()) {
      if (validation?.value) {
        await validation.value.$touch();
        const isRowValid = await validation.value.$validate();
        if (!isRowValid) {
          isValid = false;
        }
      } else {
        console.error(`Validation object for row ${index} is undefined`);
        isValid = false;
      }
    }

    if (selectedCustomerType.value === "business" || selectedCustomerType.value === "joint family") {
      filteredV$ = {
        assignedDate: v$.value.assignedDate,
        companyId: v$.value.companyId,
        customerTypeId: v$.value.customerTypeId
      };
    } else {
      filteredV$ = {
        personId: v$.value.personId,
        customerTypeId: v$.value.customerTypeId,
        assignedDate: v$.value.assignedDate
      };
    }
    // Validate only the selectedCustomerType fields
    for (const key in filteredV$) {
      if (!(await filteredV$[key].$validate())) {
        isValid = false;
      }
    }

    if (!isValid) {
      return;
    }
    if (selectedCustomerType.value === "business" || selectedCustomerType.value === "joint family") {
      if (!model.value.company) {
        model.value.company = {};
      }
      model.value.company.companyContacts = rows.value;
      customerService.saveCustomer(customerId, model.value).then(resp => {
        notifySuccess({ message: "Customer saved successfully." });
        $emit("ok");
        $emit("hide");
      });
      // Person
    } else if (selectedCustomerType.value === "individual") {
      processing.value = true;
      model.value.isCustomer = true;
      customerService.saveCustomer(customerId, model.value).then(resp => {
        notifySuccess({ message: "Customer saved successfully." });
        $emit("ok");
        $emit("hide");
      });
    }
  } catch (error) {
    console.error("Error in submitting the customer:", error);
    notifyError({ message: "An error occurred while saving the customer." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

function onDialogClose () {
  zwConfirmLeave({ data: "" }, () => {
    onDialogCancel();
  }, () => {
  });
}

// for person details
watch(() => model.value.personId != null, (newValue, oldValue) => {
  if (newValue) {
    getPersonByIdForPersonDetails();
  }
}, { immediate: false });

// for company details
watch(() => model.value.companyId, (newValue, oldValue) => {
  if (newValue) {
    getCompanyDetails();
  }
}, { immediate: false });

// getCustomerDetails
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getCustomerDetails();
  }
}, { immediate: true });

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(async() => {
  await customerTypesDropdownSingleSelect.load("Customer Type");
  // console.log("customerTypesDropdownSingleSelect", customerTypesDropdownSingleSelect);
  personNameDropdownSingleSelect.load();
  activeEmployeesDropdownSingleSelect.load();
  companyNameDropdownSingleSelect.load();
  parentCustomerDropdownSingleSelect.load();
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
