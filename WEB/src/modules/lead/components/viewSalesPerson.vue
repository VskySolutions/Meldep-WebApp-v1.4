<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1000px; max-height: 100% !important;max-width: 150vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">View Employee</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <!-- <q-card-section class="card-header with-tools"> -->
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <!-- <q-card class="card-header with-tools headerBasic"> -->
            <fieldset>
              <legend>Basic Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-3">
                  <div class="q-mb-xs text-black text-black">First Name : {{ model.person.firstName }}</div>
                </div>
                <div class="col-3">
                  <div class="q-mb-xs text-black">Middle Name : {{ model.person.middleName }} </div>
                </div>
                <div class="col-3">
                  <div class="q-mb-xs text-black">Last Name : {{ model.person.lastName }}</div>
                </div>
                <div class="col-3">
                  <div class="q-mb-xs text-black">Gender : {{ model.person.gender.dropDownValue }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-3">
                  <div class="q-mb-xs text-black">Email Address : {{ model.person.primaryEmailAddress }}</div>
                </div>
                <div class="col-3">
                  <div class="q-mb-xs text-black">Country : {{ model.person.address.addressCountry.name }}</div>
                </div>
                <div class="col">
                  <div class="q-mb-xs text-black">Phone Number : {{ model.person.primaryPhoneNumber }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-1">
                  <div class="q-mb-xs text-black">Profile Picture : </div>
                </div>
                <div class="col-3">
                  <div>
                    <img :src="model.virtualPath" alt="" style="width: 20%">
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset class="q-mt-lg">
              <legend>Primary Address Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-3">
                  <div class="q-mb-xs text-black">Address Type :  {{ model.person.addressType.dropDownValue }}</div>
                </div>
                <div class="col-3">
                  <div class="q-mb-xs text-black">Address 1 :  {{ model.person.address.addressLine1 }}</div>
                </div>
                <div class="col-3">
                  <div class="q-mb-xs text-black">Address 2 :  {{ model.person.address.addressLine2 }}</div>
                </div>
                <div class="col-3">
                  <div class="q-mb-xs text-black">State :  {{ model.person.address.addressStateProvince.name }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-3">
                  <div class="q-mb-xs text-black">City :  {{ model.person.address.city }}</div>
                </div>
                <div class="col-3">
                  <div class="q-mb-xs text-black">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }} : {{ model.person.address.zipCode }}</div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
      </q-form>
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

// Common variables
const baseCountryId = process.env.BASE_COUNTRY_ID;
// const baseURL = process.env.API_BASE_URL;
const tab = ref("1_tab");
const loading = ref(true);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
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
