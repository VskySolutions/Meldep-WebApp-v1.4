<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 q-mr-lg text-white">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Site Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">Site Name</div>
                <div class="text-black">
                  {{ model.name }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">Person Name</div>
                <div class="text-black">
                  {{ model.person.fullName }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">Email Address</div>
                <div class="text-black">
                  {{ model.person.primaryEmailAddress }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">Phone Number</div>
                <div class="text-black">
                  {{ model.person.primaryPhoneNumber }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">Time Zone</div>
                <div class="text-black">
                  {{ model.timeZone }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">Logo file</div>
                <div>
                  <div v-if="model.siteLogoId" class="row justify-start">
                    <img :src="model.siteLogoPath" alt="" style="width: 20%">
                  </div>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">Favicon Icon</div>
                <div>
                  <div v-if="model.siteFaviconId" class="row justify-start">
                    <img :src="model.siteFaviconPath" alt="" style="width: 20%">
                  </div>
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <legend>Site Address</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">Address Line 1</div>
                <div class="text-black">
                  {{ model.address.addressLine1 }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">Address Line 2</div>
                <div class="text-black">
                  {{ model.address.addressLine2 }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">Country</div>
                <div class="text-black">
                  {{ model.country }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">State</div>
                <div class="text-black">
                  {{ model.state }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">City</div>
                <div class="text-black">
                  {{ model.address.city }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">Zip Code</div>
                <div class="text-black">
                  {{ model.address.zipCode }}
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <legend>Site Roles</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12">
                <div class="q-mb-xs">Roles</div>
                <span v-if="model.sitesRoles && model.sitesRoles.length">
                  <span v-for="(sitesRole) in model.sitesRoles" :key="sitesRole.id" class="q-mr-sm">
                    <q-badge color="primary" class="q-pa-xs text-black fs-13 q-mt-sm" square outline>{{ sitesRole.applicationRole.name }}</q-badge>
                  </span>
                </span>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <legend>User Name</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                <div class="q-mb-xs">User Name</div>
                <div class="text-black">
                  {{ model.userName }}
                </div>
              </div>
            </div>
          </fieldset>
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
import sitesService from "modules/sites/site.service";

const loading = ref(true);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "-",
  person: {
    fullName: "",
    primaryEmailAddress: "",
    primaryPhoneNumber: ""
  },
  address: {
    addressLine1: "",
    addressLine2: "",
    country: null,
    countryId: null,
    state: null,
    city: "",
    zipCode: ""
  },
  siteLogoId: null,
  siteLogoPath: "",
  siteFaviconId: null,
  siteFaviconPath: "",
  userName: ""
});

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// On page rendering
onMounted(() => {
  getOrganization();
});

// get project details
const getOrganization = () => {
  loading.value = true;
  sitesService.getOrganization(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    if (resp.address) {
      model.value.addressLine1 = resp.address.addressLine1;
      model.value.addressLine2 = resp.address.addressLine2;
      model.value.countryId = resp.address.countryId;
      model.value.country = resp.address.addressCountry ? resp.address.addressCountry.name : "";
      model.value.state = resp.address.addressStateProvince ? resp.address.addressStateProvince.name : "";
      model.value.city = resp.address.city;
      model.value.zipCode = resp.address.zipCode;
    }
  }).finally(() => {
    loading.value = false;
  });
};

</script>
