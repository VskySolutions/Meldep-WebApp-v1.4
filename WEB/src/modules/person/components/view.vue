<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw !important; max-width: 60vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.fullName }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
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
                  {{ model.gender }}
                </div>
              </div>
              <div class="col-12 col-md-4">
                <div class="q-mb-xs">Title</div>
                <div class="text-black">
                  {{ model.title }}
                </div>
              </div>
              <div class="col-12 col-md-4">
                <div class="q-mb-xs">Date of birth</div>
                <div class="text-black">
                  {{ model.dob }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-4">
                <div class="q-mb-xs">Email Address</div>
                <div class="text-black">{{ model.primaryEmailAddress }}
                </div>
              </div>
              <div class="col-12 col-md-8">
                <div class="q-mb-xs">Profile Link</div>
                <div class="text-black">
                  {{ model.profileLink }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-4">
                <div class="q-mb-xs">Phone Number</div>
                <div class="text-black">
                  {{ model.primaryPhoneNumber }}
                </div>
              </div>
              <div class="col-12 col-md-8">
                <div class="q-mb-xs">Profile Picture</div>
                <div>
                  <div v-if="model.pictureId" class="row justify-center">
                    <img :src="model.virtualPath" alt="" style="width: 50%">
                  </div>
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset class="q-mt-lg">
            <legend>Primary Address Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-4">
                <div class="q-mb-xs">Country</div>
                <div class="text-black">
                  {{ model.country }}
                </div>
              </div>
              <div class="col-12 col-md-4">
                <div class="q-mb-xs">State</div>
                <div class="text-black">
                  {{ model.state ? model.state : '-' }}
                </div>
              </div>
              <div class="col-12 col-md-4">
                <div class="q-mb-xs">Address Type</div>
                <div class="text-black">
                  {{ model.addressType }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
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
              <div class="col-12 col-md-4">
                <div class="q-mb-xs">City</div>
                <div class="text-black">
                  {{ model.city }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-4">
                <div class="q-mb-xs">{{ zipCodeLabel }}</div>
                <div class="text-black">
                  {{ model.zipCode }}
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset class="q-mt-lg">
            <legend>Other Info</legend>
            <div class="row q-col-gutter-x-md q-mb-lg">
              <div class="col-12 col-md-4">
                <div class="q-mb-xs"><label>Identified Date</label></div>
                <div class="form-group text-black">
                  {{ model.identifiedDateStr }}
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
          <fieldset class="q-mt-lg">
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
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import personService from "modules/person/person.service";
import { ref, onMounted } from "vue";
import _ from "lodash";
import { format } from "date-fns"; // Standard TimeZone Conversion

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Common variables
const tab = ref("1_tab");
const loading = ref(true);
const zipCodeLabel= ref('');
console.log(zipCodeLabel);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  firstName: "",
  middleName: "",
  lastName: "",
  gender: null,
  primaryEmailAddress: "",
  dob: "",
  primaryPhoneNumber: "",
  addressType: null,
  addressLine1: "",
  addressLine2: "",
  country: null,
  countryId: null,
  state: null,
  city: "",
  zipCode: "",
  identifiedById: null,
  identifiedDateStr: format(new Date(), "MM/dd/yyyy"),
  identificationNote: "",
  relation: "",
  relationFullName: "",
  phoneNumber: "",
  pictureId: null,
  virtualPath: "",
  tab
});

// get person details on edit mode
const getPerson = () => {
  loading.value = true;
  personService.getPerson(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    if (resp.address) {
      model.value.addressLine1 = resp.address.addressLine1;
      model.value.addressLine2 = resp.address.addressLine2;
      model.value.countryId = resp.address.countryId;
      model.value.country = resp.address.addressCountry ? resp.address.addressCountry.name : "";
      model.value.state = resp.address.addressStateProvince ? resp.address.addressStateProvince.name : "";
      model.value.city = resp.address.city;
      model.value.zipCode = resp.address.zipCode;
      zipCodeLabel.value = resp.address.addressCountry.zipCodeLabel;
    }
    model.value.gender = resp.gender ? resp.gender.dropDownValue : "";
    model.value.addressType = resp.addressType ? resp.addressType.dropDownValue : "";
    model.value.virtualPath = resp.picture ? resp.picture.virtualPath : "";
    model.value.identifiedDateStr = resp.identifiedDate ? format(resp.identifiedDate, "MM/dd/yyyy") : "";
    model.value.identifiedByName = resp.identifiedBy ? resp.identifiedBy.fullName : "";
  }).finally(() => {
    loading.value = false;
  });
};

// On page rendering
onMounted(() => {
  getPerson();
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
