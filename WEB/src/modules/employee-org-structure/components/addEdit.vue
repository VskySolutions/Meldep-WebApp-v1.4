<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Org Structure</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Org Structure Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-lg-6 col-md-6 col-sm-12 col-xs-12 q-mb-md">
                  <label class="q-mb-xs text-black">Year<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.year"
                      outlined
                      stack-label
                      hide-bottom-space
                      dense
                      mask="####"
                      :error="v$.year.$error"
                      :error-message="v$.year.$errors[0]?.$message"
                      @click="v$.year.$touch"
                    >
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" v-model="isPopupVisible" transition-show="scale" transition-hide="scale">
                            <q-date v-model="model.year" default-view="Years" emit-immediately minimal mask="YYYY" class="myDate" @update:model-value="onUpdateMv2" />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
                <div class="col-lg-6 col-md-6 col-sm-12 col-xs-12">
                  <div class="q-mb-xs text-black">Level<span class="required">*</span></div>
                  <q-select
                    v-model="model.level"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="employeeLevelList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.level.$error"
                    :error-message="v$.level.$errors[0]?.$message"
                    @blur="v$.level.$touch"
                    @filter="getAllLevelListDropdownForFilter"
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
                <div class="col-lg-6 col-md-6 col-sm-12 col-xs-12 q-mb-md">
                  <div class="q-mb-xs text-black">Manager<span class="required">*</span></div>
                  <q-select
                    v-model="model.managerId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="managerList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.managerId.$error"
                    :error-message="v$.managerId.$errors[0]?.$message"
                    @filter="getAllManagerListDropdownForFilter"
                    @blur="v$.managerId.$touch"
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
                <div class="col-lg-6 col-md-6 col-sm-12 col-xs-12">
                  <div class="q-mb-xs text-black">Employee<span class="required">*</span></div>
                  <q-select
                    v-model="model.employeeId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="filteredEmployeeList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.employeeId.$error"
                    :error-message="v$.employeeId.$errors[0]?.$message"
                    @filter="getAllEmployeesListDropdownForFilter"
                    @blur="v$.employeeId.$touch"
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
                <div class="col-lg-6 col-md-6 col-sm-12 col-xs-12 q-mb-md">
                  <div class="q-mb-xs text-black">Department<span class="required">*</span></div>
                  <q-select
                    v-model="model.departmentId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="employeeDepartmentList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.departmentId.$error"
                    :error-message="v$.departmentId.$errors[0]?.$message"
                    @filter="getAllDepartmentListDropdownForFilter"
                    @blur="v$.departmentId.$touch"
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
                <div class="col-lg-6 col-md-6 col-sm-12 col-xs-12">
                  <div class="q-mb-xs text-black">Role<span class="required">*</span></div>
                  <q-select
                    v-model="model.employeeDesignationIdsArray"
                    push
                    class="w-100 h-auto"
                    outlined
                    use-input
                    use-chips
                    clearable
                    transition-show="jump-up"
                    transition-hide="jump-up"
                    hide-bottom-space
                    :dense="true"
                    multiple
                    fill-input
                    input-debounce="0"
                    :options="employeeDesignationList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.employeeDesignationIdsArray.$error"
                    :error-message="v$.employeeDesignationIdsArray.$errors[0]?.$message"
                    @filter="getAllDesignationListDropDownForFilter"
                    @blur="v$.employeeDesignationIdsArray.$touch"
                  >
                    <template #option="{ itemProps, opt, selected, toggleOption }">
                      <q-item v-bind="itemProps">
                        <q-item-section>
                          <div class="row q-col-gutter-x-md items-center">
                            <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                            <span>{{ opt.text }}</span>
                          </div>
                        </q-item-section>
                      </q-item>
                    </template>
                  </q-select>
                </div>
                <div class="col-lg-6 col-md-6 col-sm-12 col-xs-12 q-mb-md">
                  <div class="q-mb-xs text-black">Sort Order</div>
                  <q-input v-model="model.sortOrder" outlined stack-label hide-bottom-space mask="####" :dense="true" maxlength="128" />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="form-group">
                    <label class="label q-mb-xs text-black">Responsibilities</label>
                    <q-editor
                      v-model="model.responsibilities"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
              </div>
              <!-- Hint added manually -->
              <div class="text-caption text-grey-7 q-mt-xs">The maximum length allowed is 500.</div>
              <div class="row q-col-gutter-x-md q-mb-md q-mt-md">
                <div class="col-md-3">
                  <div class="q-mb-xs text-black">Color</div>
                  <q-input v-model="model.color" filled class="my-input">
                    <template #append>
                      <q-icon name="o_colorize" class="cursor-pointer">
                        <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                          <q-color v-model="model.color" @update:model-value="updateColor" />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import commonService from "services/common.service";
import _ from "lodash";
import employeesService from "src/modules/employee/employee.service";
import departmentService from "modules/department/department.service";
import orgStructureService from "modules/employee-org-structure/employeeOrgStructure.service";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Define emits
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Common variables
const loading = ref(true);
const processing = ref(false);
const processingClose = ref(false);
const isPopupVisible = ref(false);
const $q = useQuasar();

const onUpdateMv2 = (val) => {
  // Always stay in the "Years" view
  model.value.selectedYear = val; // Store the selected year
  isPopupVisible.value = false; // Close the popup after selecting a year
};

// Define model values
const model = ref({
  employeeId: "",
  managerId: "",
  departmentId: "",
  employeeDesignationIdsArray: [],
  year: "",
  level: null,
  sortOrder: null,
  responsibilities: "",
  color: null
});

const updateColor = (val) => {
  if (val.startsWith("#")) {
    model.value.color = val.toUpperCase();
  }
};

// Validation rules
const rules = {
  managerId: { required: helpers.withMessage("Manager is required", required) },
  employeeId: { required: helpers.withMessage("Employee is required", required) },
  departmentId: { required: helpers.withMessage("Department is required", required) },
  employeeDesignationIdsArray: { required: helpers.withMessage("Role is required", required) },
  year: { required: helpers.withMessage("Year is required", required) },
  level: { required: helpers.withMessage("Level is required", required) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Get all employee level list for dropdown
const employeeLevelList = ref([]);
const employeeLevelListFilter = ref([]);
function getAllEmployeeLevelListForDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropDownText, value: item.dropdownValue }));
    employeeLevelList.value = responseData;
    employeeLevelListFilter.value = responseData;
  });
}

function getAllLevelListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeLevelList.value = employeeLevelListFilter.value;
    } else {
      employeeLevelList.value = employeeLevelListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all Employee and Manager List for dropdown
const employeeList = ref([]);
const managerList = ref([]);
const employeeListFilter = ref([]);
const managerListFilter = ref([]);
function getAllActiveEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id }));
    employeeList.value = responseData;
    managerList.value = responseData;
    employeeListFilter.value = responseData;
    managerListFilter.value = responseData;
  });
}

const filteredEmployeeList = computed(() => {
  return employeeList.value.filter((employee) => employee.value !== model.value.managerId);
});

// Search manager for dropdown
function getAllManagerListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      managerList.value = managerListFilter.value;
    } else {
      managerList.value = managerListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Search employee for dropdown
function getAllEmployeesListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = employeeListFilter.value;
    } else {
      employeeList.value = employeeListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all department list for dropdown
const employeeDepartmentList = ref([]);
const employeeDepartmentListFilter = ref([]);
function getAllDepartmentListForDropdown () {
  departmentService.getAllDepartmentListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.name, value: item.id }));
    employeeDepartmentList.value = responseData;
    employeeDepartmentListFilter.value = responseData;
  });
}
// Search department for dropdown
function getAllDepartmentListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeDepartmentList.value = employeeDepartmentListFilter.value;
    } else {
      employeeDepartmentList.value = employeeDepartmentListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all Employee Designation list
const employeeDesignationList = ref([]);
const employeeDesignationListFilter = ref([]);
function getAllDesignationListForDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    employeeDesignationList.value = responseData;
    employeeDesignationListFilter.value = responseData;
  });
}

// Search employee designation for dropdown
function getAllDesignationListDropDownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeDesignationList.value = employeeDesignationListFilter.value;
    } else {
      employeeDesignationList.value = employeeDesignationListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// get org structure details on edit mode
const getEmployeeOrgStructure = () => {
  loading.value = true;
  orgStructureService.getEmployeeOrgStructure(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.employeeDesignationIdsArray = resp.employeeOrgStructureDesignationMapping.map(mapping => mapping.employeeDesignationId);
  }).finally(() => {
    loading.value = false;
  });
};

async function onSubmit () {
  processing.value = true;
  try {
    if (!await v$.value.$validate()) {
      return;
    }
    model.value.level = model.value.level ? Number(model.value.level) : 0;
    model.value.sortOrder = model.value.sortOrder === "" ? null : model.value.sortOrder;
    orgStructureService.saveEmployeeOrgStructure(props.id, model.value).then(resp => {
      notifySuccess({ message: "Org stucture saved successfully." });
      onDialogOK();
    });
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
      processingClose.value = false;
    }, 1500);
  }
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Rich Editor Tools: Fonts and Toolbar
// --------------------------------------------------------------------------------------------------------------------------------------------------

const fonts = {
  arial: "Arial",
  arial_black: "Arial Black",
  comic_sans: "Comic Sans MS",
  courier_new: "Courier New",
  impact: "Impact",
  lucida_grande: "Lucida Grande",
  times_new_roman: "Times New Roman",
  verdana: "Verdana"
};

const toolbar = [
  [
    {
      label: $q.lang.editor.align,
      icon: $q.iconSet.editor.align,
      fixedLabel: true,
      list: "only-icons",
      options: ["left", "center", "right", "justify"]
    }
  ],
  ["bold", "italic", "strike", "underline"],
  ["token", "hr", "link", "custom_btn"],
  [
    {
      label: $q.lang.editor.formatting,
      icon: $q.iconSet.editor.formatting,
      list: "no-icons",
      options: ["p", "h1", "h2", "h3", "h4", "h5", "h6", "code"]
    },
    "removeFormat"
  ],
  ["quote", "unordered", "ordered", "outdent", "indent"],
  ["undo", "redo"],
  ["viewsource"]
];

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getEmployeeOrgStructure();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  getAllDesignationListForDropDown("Employee Designation");
  getAllEmployeeLevelListForDropDown("Level Type");
  getAllActiveEmployeesListForDropdown();
  getAllDepartmentListForDropdown();
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
