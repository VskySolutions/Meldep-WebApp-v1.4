<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 70vw; max-width:70vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.person.firstName + '  ' + model.person.lastName }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <!-- <q-card-section class="card-header with-tools"> -->
      <!-- <q-form greedy @submit.prevent.stop="onSubmit"> -->
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
            <q-tab name="1_tab" label="Employee Info." class="q-px-lg q-mr-md" />
            <q-tab name="2_tab" label="Employment Details" class="q-px-lg" :disable="disableTab" />
            <q-tab name="3_tab" label="Other Information" class="q-px-lg" :disable="disableTab" />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated>
            <q-tab-panel name="1_tab">
              <!-- <q-card class="card-header with-tools headerBasic"> -->
              <fieldset>
                <legend>Basic Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-3">
                    <div class="q-mb-xs">First Name : <span class="text-black">{{ model.person.firstName }}</span></div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-3">
                    <div class="q-mb-xs">Middle Name : <span class="text-black">{{ model.person.middleName ? model.person.middleName : '-' }}</span></div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-3">
                    <div class="q-mb-xs">Last Name : <span class="text-black">{{ model.person.lastName }}</span></div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-3">
                    <div class="q-mb-xs">Gender : <span class="text-black">{{ model.person.gender.dropDownValue }}</span></div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-xs-12 col-sm-6">
                    <div class="q-mb-xs">Email Address : <span class="text-black">{{ model.person.primaryEmailAddress }}</span></div>
                  </div>
                  <div class="col-xs-12 col-sm-6">
                    <div class="q-mb-xs">Employee Code : <span class="text-black">{{ model.employeeCode }}</span></div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-xs-12 col-sm-6">
                    <div class="q-mb-xs">Country : <span class="text-black">{{ model.person.address.addressCountry.name }}</span></div>
                  </div>
                  <div class="col-xs-12 col-sm-6">
                    <div class="q-mb-xs">Phone Number : <span class="text-black">{{ model.person.primaryPhoneNumber }}</span></div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-3">
                    <div class="q-mb-xs">Profile Picture : </div>
                    <div class="row justify-center">
                      <img :src="model.virtualPath" alt="" style="width: 40%;">
                    </div>
                  </div>
                </div>
              </fieldset>
              <fieldset class="q-mt-lg">
                <legend>Primary Address Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-xs-12 col-sm-6">
                    <div class="q-mb-xs">Address Type :  <span class="text-black">{{ model.person.addressType.dropDownValue }}</span></div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-xs-12 col-sm-6">
                    <div class="q-mb-xs">Address 1 :  <span class="text-black">{{ model.person.address.addressLine1 }}</span></div>
                  </div>
                  <div class="col-xs-12 col-sm-6">
                    <div class="q-mb-xs">Address 2 :  <span class="text-black">{{ model.person.address.addressLine2 ? model.person.address.addressLine2 : '-' }}</span></div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-xs-12 col-sm-3">
                    <div class="q-mb-xs">State :  <span class="text-black">{{ model.person.address.addressStateProvince.name }}</span></div>
                  </div>
                  <div class="col-xs-12 col-sm-3">
                    <div class="q-mb-xs">City :  <span class="text-black">{{ model.person.address.city }}</span></div>
                  </div>
                  <div class="col-xs-12 col-sm-3">
                    <div class="q-mb-xs">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }} : <span class="text-black">{{ model.person.address.zipCode }}</span></div>
                  </div>
                </div>
              </fieldset>
              <fieldset class="q-mt-lg">
                <legend>Current Address</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-xs-12 col-sm-6">
                    <div class="q-mb-xs">Address 1 :  <span class="text-black">{{ model.address.addressLine1 }}</span></div>
                  </div>
                  <div class="col-xs-12 col-sm-6">
                    <div class="q-mb-xs">Address 2 :  <span class="text-black">{{ model.address.addressLine2 ? model.address.addressLine2 : '-' }}</span></div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-xs-12 col-sm-3">
                    <div class="q-mb-xs">Country :  <span class="text-black">{{ model.address.addressCountry.name }}</span></div>
                  </div>
                  <div class="col-xs-12 col-sm-3">
                    <div class="q-mb-xs">State :  <span class="text-black">{{ model.address.addressStateProvince.name }}</span></div>
                  </div>
                  <div class="col-xs-12 col-sm-3">
                    <div class="q-mb-xs">City :  <span class="text-black">{{ model.address.city }}</span></div>
                  </div>
                  <div class="col-xs-12 col-sm-3">
                    <div class="q-mb-xs">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }} : <span class="text-black">{{ model.address.zipCode }}</span></div>
                  </div>
                </div>
              </fieldset>
              <fieldset v-if="model.personId" class="q-mt-lg">
                <legend>Employee Activation</legend>
                <div class="col-lg-6 col-md-6 col-sm-6 col-xs-6">
                  <div class="q-mb-xs q-mt-md text-black">Set As Active?</div>
                  {{ model.active == true ? "Active" : "Ex-Employee" }}
                </div>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="2_tab">
              <fieldset class="q-mb-lg">
                <legend>Employment Type</legend>
                <q-table
                  ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
                  no-data-label="No data available" binary-state-sort
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                      <q-td>{{ props.row.employeeType }}</q-td>
                      <q-td>{{ props.row.startDateStr }}</q-td>
                      <q-td>{{ props.row.endDateStr }}</q-td>
                      <q-td>{{ props.row.duration }}</q-td>
                      <q-td>{{ props.row.note }}</q-td>
                    </q-tr>
                  </template>
                </q-table>
              </fieldset>
              <fieldset class="q-mb-lg">
                <legend>Employment Status</legend>
                <q-table
                  ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="statusRows" :columns="columnsStatus" row-key="id" separator="cell"
                  no-data-label="No data available" binary-state-sort
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                      <q-td>{{ props.row.employeeStatuses }}</q-td>
                      <q-td>{{ props.row.statusStartDateStr }}</q-td>
                      <q-td>{{ props.row.statusEndDateStr }}</q-td>
                      <q-td>{{ props.row.duration }}</q-td>
                      <q-td>{{ props.row.note }}</q-td>
                    </q-tr>
                  </template>
                </q-table>
              </fieldset>
              <fieldset class="q-mb-lg">
                <legend>Department</legend>
                <q-table
                  ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="deptRows" :columns="columnsDepartment" row-key="id" separator="cell"
                  no-data-label="No data available" binary-state-sort
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                      <q-td>{{ props.row.employeeDepartment }}</q-td>
                      <q-td>{{ props.row.departmentStartDateStr }}</q-td>
                      <q-td>{{ props.row.departmentEndDateStr }}</q-td>
                      <q-td>{{ props.row.duration }}</q-td>
                      <q-td>{{ props.row.note }}</q-td>
                    </q-tr>
                  </template>
                </q-table>
              </fieldset>
              <fieldset class="q-mb-lg">
                <legend>Designation</legend>
                <q-table
                  ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="desgRows" :columns="columnsDesignation" row-key="id" separator="cell"
                  no-data-label="No data available" binary-state-sort
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                      <q-td>{{ props.row.employeeDesignation }}</q-td>
                      <q-td>{{ props.row.employeeShift }}</q-td>
                      <q-td>{{ props.row.employeeLeaveApprover }}</q-td>
                      <q-td>{{ props.row.designationStartDateStr }}</q-td>
                      <q-td>{{ props.row.designationEndDateStr }}</q-td>
                      <q-td>{{ props.row.duration }}</q-td>
                      <q-td>{{ props.row.note }}</q-td>
                    </q-tr>
                  </template>
                </q-table>
              </fieldset>
              <fieldset class="q-mb-lg">
                <legend>Org Location</legend>
                <q-table
                  ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="orglocRows" :columns="columnsOrgLocation" row-key="id" separator="cell"
                  no-data-label="No data available" binary-state-sort
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                      <q-td>{{ props.row.employeeOrgLocation }}</q-td>
                      <q-td>{{ props.row.orgLocationStartDateStr }}</q-td>
                      <q-td>{{ props.row.orgLocationEndDateStr }}</q-td>
                      <q-td>{{ props.row.duration }}</q-td>
                      <q-td>{{ props.row.note }}</q-td>
                    </q-tr>
                  </template>
                </q-table>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="3_tab">
              <fieldset v-if="model.address.addressCountry.name === 'India'" class="q-mt-lg">
                <legend>KYC Information</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-xs-12 col-sm-6">
                    <div class="q-mb-xs">Aadhar Card No : <span class="text-black">{{ model.aadhaarCardNo }}</span></div>
                  </div>
                  <div class="col-xs-12 col-sm-6">
                    <div>Pan Card No : <span class="text-black">{{ model.panCardNo }}</span> </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-xs-12 col-sm-6">
                    <div>EPF UAN No : <span class="text-black">{{ model.epfuanNo }}</span></div>
                  </div>
                </div>
              </fieldset>
              <fieldset class="q-mt-lg">
                <legend>Employment Period Details</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-xs-12 col-sm-6">
                    <div class="q-mb-xs">Joining Date :  <span class="text-black">{{ model.joiningDateStr }}</span></div>
                  </div>
                  <div class="col-xs-12 col-sm-6">
                    <div class="q-mb-xs">Release Date/Last Date :  <span class="text-black">{{ model.releaseDateStr }}</span></div>
                  </div>
                </div>
              </fieldset>
              <fieldset class="q-mt-lg">
                <legend>Education Information</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-xs-12">
                    <!-- <div class="q-mb-xs">Education Details :  <span class="text-black">{{ model.educationDetail }}</span></div> -->
                    <div class="q-mb-xs">Education Details :  <span class="text-black"><p v-html="model.educationDetail ? model.educationDetail : '-'" /></span></div>
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
          </q-tab-panels>
        </div>
      </div>
      <!-- </q-form> -->
      <!-- </q-card-section> -->
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";
import employeesService from "src/modules/employee/employee.service";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const rows = ref([]);
const statusRows = ref([]);
const desgRows = ref([]);
const deptRows = ref([]);
const orglocRows = ref([]);

// Common variables
const baseCountryId = process.env.BASE_COUNTRY_ID;
// const baseURL = process.env.API_BASE_URL;
const tab = ref("1_tab");
const loading = ref(true);
const columns = ref([
  { name: "employeeTypeId", label: "Employment Type", field: "employeeTypeId", align: "left", sortable: true },
  { name: "startDate", label: "Employment Type Start Date", field: "startDate", align: "left", sortable: true },
  { name: "endDate", label: "Employment Type End Date", field: "endDate", align: "left", sortable: true },
  { name: "duration", label: "Duration", field: "duration", align: "left", sortable: false },
  { name: "note", label: "Note", field: "note", align: "left", sortable: false }
]);
const columnsStatus = ref([
  { name: "employeeStatusId", label: "Employment Status", field: "employeeStatusId", align: "left", sortable: true },
  { name: "startDate", label: "Employment Status Start Date", field: "startDate", align: "left", sortable: true },
  { name: "endDate", label: "Employment Status End Date", field: "endDate", align: "left", sortable: true },
  { name: "duration", label: "Duration", field: "duration", align: "left", sortable: false },
  { name: "note", label: "Note", field: "note", align: "left", sortable: false }
]);
const columnsDepartment = ref([
  { name: "employeeDepartmentId", label: "Department", field: "employeeDepartmentId", align: "left", sortable: true },
  { name: "startDate", label: "Department Start Date", field: "startDate", align: "left", sortable: true },
  { name: "endDate", label: "Department End Date", field: "endDate", align: "left", sortable: true },
  { name: "duration", label: "Duration", field: "duration", align: "left", sortable: false },
  { name: "note", label: "Note", field: "note", align: "left", sortable: false }
]);
const columnsDesignation = ref([
  { name: "employeeDesignation", label: "Designation", field: "employeeDesignation", align: "left", sortable: true },
  { name: "employeeShift", label: "Shift", field: "employeeShift", align: "left", sortable: true },
  { name: "employeeLeaveApprover", label: "Leave Approver", field: "employeeLeaveApprover", align: "left", sortable: true },
  { name: "startDate", label: "Designation Start Date", field: "startDate", align: "left", sortable: true },
  { name: "endDate", label: "Designation End Date", field: "endDate", align: "left", sortable: true },
  { name: "duration", label: "Duration", field: "duration", align: "left", sortable: false },
  { name: "note", label: "Note", field: "note", align: "left", sortable: false }
]);
const columnsOrgLocation = ref([
  { name: "orgLocationId", label: "Org Location", field: "orgLocationId", align: "left", sortable: true },
  { name: "startDate", label: "Org Location Start Date", field: "startDate", align: "left", sortable: true },
  { name: "endDate", label: "Org Location End Date", field: "endDate", align: "left", sortable: true },
  { name: "duration", label: "Duration", field: "duration", align: "left", sortable: false },
  { name: "note", label: "Note", field: "note", align: "left", sortable: false }
]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  address: {
    addressId: "",
    addressLine1: "",
    addressLine2: "",
    countryId: null,
    addressCountry: {
      name: ""
    },
    addressStateProvince: {
      name: ""
    },
    city: "",
    zipCode: ""
  },
  person: {
    firstName: "",
    middleName: "",
    lastName: "",
    primaryEmailAddress: "",
    primaryPhoneNumber: "",
    pictureId: null,
    virtualPath: "",
    tab,
    gender: {
      dropDownValue: ""
    },
    addressType: {
      dropDownValue: ""
    },
    address: {
      addressId: "",
      addressLine1: "",
      addressLine2: "",
      addressCountry: {
        name: ""
      },
      addressStateProvince: {
        name: ""
      },
      city: "",
      zipCode: ""
    }
  }
});

onMounted(() => {
  getEmployee();
});
// get person details on edit mode
const getEmployee = () => {
  employeesService.getEmployee(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.virtualPath = resp.person.picture.virtualPath ? resp.person.picture.virtualPath : "";
    model.value.joiningDateStr = model.value.joiningDate;
    model.value.countryId = resp.address.countryId;
    model.value.releaseDateStr = model.value.releaseDate;
    rows.value = resp.employeeType.map(item => ({
      ...item,
      employeeType: item.employeeTypeDropdown.dropDownValue,
      startDateStr: item.startDate,
      endDateStr: item.endDate,
      duration: item.duration,
      note: item.note
    }));
    statusRows.value = resp.employeeStatuses.map(item => ({
      ...item,
      employeeStatuses: item.status.dropDownValue,
      statusStartDateStr: item.startDate,
      statusEndDateStr: item.endDate,
      duration: item.duration,
      note: item.note
    }));
    deptRows.value = resp.employeeDepartment.map(item => ({
      ...item,
      employeeDepartment: item.department.name,
      departmentStartDateStr: item.startDate,
      departmentEndDateStr: item.endDate,
      duration: item.duration,
      note: item.note,
      editing: false,
      flag: "Edit"
    }));
    desgRows.value = resp.employeeDesignation.map(item => ({
      ...item,
      employeeDesignation: item.designation.dropDownValue,
      employeeShift: item.shift.dropDownValue,
      employeeLeaveApprover: item.leaveApprover.person.fullName,
      designationStartDateStr: item.startDate,
      designationEndDateStr: item.endDate,
      duration: item.duration,
      note: item.note,
      editing: false,
      flag: "Edit"
    }));
    orglocRows.value = resp.employeeOrgLocation.map(item => ({
      ...item,
      employeeOrgLocation: item.orgLocation.dropDownValue,
      orgLocationStartDateStr: item.startDate,
      orgLocationEndDateStr: item.endDate,
      duration: item.duration,
      note: item.note,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
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
