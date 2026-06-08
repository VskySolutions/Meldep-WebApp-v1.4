<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw !important; max-width: 60vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">View Contact</div>
        <q-btn v-close-popup icon="o_close" color="white" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Contact Info</legend>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 col-md-6">
                <div class="form-group">
                  <div>Person Name</div>
                  <div class="text-black q-mb-sm">{{ model.personName }}</div>
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="form-group">
                  <div>Company Name</div>
                  <div class="text-black q-mb-sm">{{ model.companyName }}</div>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 col-md-6">
                <div class="form-group">
                  <div>First Name</div>
                  <div class="text-black q-mb-sm">{{ model.firstName }}</div>
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="form-group">
                  <div>Middle Name</div>
                  <div class="text-black q-mb-sm">{{ model.middleName ? model.middleName : '-' }}</div>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 col-md-6">
                <div class="form-group">
                  <div>Last Name</div>
                  <div class="text-black q-mb-sm">{{ model.lastName }}</div>
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="form-group">
                  <div>Email</div>
                  <div class="text-black q-mb-sm">{{ model.Email }}</div>
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="form-group">
                  <div>Alt Email</div>
                  <div class="text-black q-mb-sm">{{ model.alternateEmail ? model.alternateEmail : '-' }}</div>
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="form-group">
                  <div>Phone Number</div>
                  <div class="text-black q-mb-sm">{{ model.primaryPhoneNumber ? model.primaryPhoneNumber : '-' }}</div>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 col-md-6">
                <div class="form-group">
                  <div>Alt. PhoneNumber</div>
                  <div class="text-black q-mb-sm">{{ model.alternatePhoneNumber ? model.alternatePhoneNumber : '-' }}</div>
                </div>
              </div>
              <div class="col-12 col-md-6">
                <div class="form-group">
                  <div>Gender</div>
                  <div class="text-black q-mb-sm">{{ model.gender ? model.gender : '-' }}</div>
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
import { useDialogPluginComponent } from "quasar";
import { ref, watch } from "vue";
import _ from "lodash";
import companyContactService from "modules/company-contacts/companyContacts.service";
defineEmits([...useDialogPluginComponent.emits]);

const loading = ref(true);
const model = ref({
  person: {
    primaryEmailAddress: ""
  }
});

const props = defineProps({ id: { type: String, default: "" } });

const getCompany = () => {
  loading.value = true;
  companyContactService.getCompanyContactDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.personName = resp.person.firstName + " " + resp.person.lastName;
    model.value.companyName = resp.company.name;
    model.value.firstName = resp.person.firstName;
    model.value.lastName = resp.person.lastName;
    model.value.middleName = resp.person.middleName;
    model.value.Email = resp.person.primaryEmailAddress;
    model.value.alternateEmail = resp.alternateEmail;
    model.value.primaryPhoneNumber = resp.person.primaryPhoneNumber;
    model.value.alternatePhoneNumber = resp.alternatePhoneNumber;
    model.value.gender = resp.person.gender.dropDownValue;
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getCompany();
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
