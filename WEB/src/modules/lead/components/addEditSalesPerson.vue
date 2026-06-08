<template>
  <q-dialog class="customDialog" ref="dialogRef" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1400px; height: 100% !important;max-width: 150vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Salesperson</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <!-- <q-card-section class="card-header with-tools"> -->
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md', readonlyEmployee != '' ? 'edit_employee' : '']">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Basic Info</legend>
              <div class="absolute-top-right q-mr-xl q-mt-md" style="line-height: 1.2em;"><q-icon v-if="readonlyEmployee" name="fa-regular fa-pen-to-square" size="md" class="cursor-pointer" @click="onEdit(model.employeeId)"></q-icon></div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-md-4">
                  <label class="label q-mb-xs text-black">Employee Name</label>
                  <div>
                    <q-select
                      v-model="model.employeeId" clearable use-input outlined stack-label hide-bottom-space :dense="true" :readonly="readonlyEmployee!= '' ? '' : 'readonlyEmployee'"
                      :options="empoyeeList" option-value="value" option-label="text" emit-value map-options :error="v$.employeeId.$error" :error-message="v$.employeeId.$errors[0]?.$message" @blur="v$.employeeId.$touch" @filter="filterFn1"
                    >
                      <template #option="{ itemProps, opt }">
                        <q-item v-bind="itemProps">
                          <q-item-section>
                            <div class="row q-col-gutter-x-md items-center">
                              <span>{{ opt.text }}</span>
                            </div>
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset class="q-mt-lg">
              <legend>Primary Address Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-3">
                  <div class="q-mb-xs text-black">Address Type : {{ model.addressTypeId }}</div>
                </div>
                <div class="col-3">
                  <div class="q-mb-xs text-black">Address 1 :  {{ model.addressLine1 }}</div>
                </div>
                <div class="col-3">
                  <div class="q-mb-xs text-black">Address 2 : {{ model.addressLine2 }}</div>
                </div>
                <div class="col-3">
                  <div class="q-mb-xs text-black">State :  {{ model.stateProvinceId }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-3">
                  <div class="q-mb-xs text-black">State :  {{ model.stateProvinceId }}</div>
                </div>
                <div class="col-3">
                  <div class="q-mb-xs text-black">City :  {{ model.city }} </div>
                </div>
                <div class="q-mb-xs text-black">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }} : {{ model.zipCode }}</div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
// import { useQuasar, useDialogPluginComponent, uid } from "quasar";
import { useQuasar, useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import _ from "lodash";
import editPerson from "modules/person/components/addEdit.vue";
import salesPersonService from "modules/lead/sales_person.service";
import { required, helpers } from "@vuelidate/validators";
import useFilters from "composables/useFilters";
import { ref, watch, onMounted } from "vue";
import { notifySuccess } from "assets/utils";
import employeesService from "modules/employee/employee.service";

// Common variables
const baseCountryId = process.env.BASE_COUNTRY_ID;
const $q = useQuasar();
const rows = ref([]);
const { toDate } = useFilters();
const tab = ref("1_tab");
const loading = ref(true);
const processing = ref(false);
const processingClose = ref(false);
const props = defineProps({ id: { type: String, default: "" }, employeeId: { type: String, default: "" } });
const readonlyEmployee = props.id ? "readonly" : "";
const employeeId = props.id;
// Validation rules
const rules = {
  employeeId: { required: helpers.withMessage("Person name is required", required) }
};

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Define model values
const model = ref({
  firstName: "",
  middleName: "",
  lastName: "",
  genderId: null,
  primaryEmailAddress: "",
  primaryPhoneNumber: "",
  addressTypeId: null,
  addressLine1: "",
  addressLine2: "",
  countryId: null,
  stateProvinceId: null,
  city: "",
  zipCode: "",
  identifiedById: null,
  employeeId: null,
  identifiedDateStr: toDate(new Date()),
  identificationNote: "",
  pictureId: null,
  virtualPath: ""
});
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// On page rendering
onMounted(() => {
  getAllEmployeesListForDropdown();
});

const getEmployee = () => {
  employeesService.getEmployee(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getEmployee();
  }
}, { immediate: true });

const empoyeeList = ref([]);
const options1 = ref([]);
function getAllEmployeesListForDropdown () {
  salesPersonService.getAllEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id }));
    empoyeeList.value = responseData;
    options1.value = responseData;
  });
}

function filterFn1 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      empoyeeList.value = options1.value;
    } else {
      empoyeeList.value = options1.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// function getEmployeeStatus (value) {
//   if (value) {
//     return employeeTypeArr.value.find((item) => item.id === value)?.dropdownValue;
//   }
// }

// Get all Employee Type list

// Edit popup
const onEdit = (id) => {
  $q.dialog({
    component: editPerson,
    componentProps: { id }
  }).onOk(() => {
    getAllEmployeesListForDropdown();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Submit form
const onSubmit = async (isClose = 0) => {
  if (await v$.value.$validate()) {
    if (isClose === 1) {
      processingClose.value = true;
    } else {
      processing.value = true;
    }
    model.value.tab = tab;
    model.value.employeeTypeModel = rows.value;
    employeesService.saveEmployee(employeeId, model.value).then((resp) => {
      notifySuccess({ message: "Employee is saved successfully." });
      employeeId.value = resp;
      onDialogOK();
    }).finally(() => {
      processing.value = false;
      processingClose.value = false;
    });
  }
};

</script>

<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_employee .q-select__dropdown-icon{
  display: none;
}
</style>
