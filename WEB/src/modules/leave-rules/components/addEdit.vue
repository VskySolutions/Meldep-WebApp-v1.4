<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Leave Rule</div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogClose()" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Leave Rule Info</legend>
              <div class="row q-col-gutter-md q-mb-md flex justify-between items-center">
                <div class="col-md-6 col-lg-3">
                  <label class="label q-mb-xs text-black">Year <span class="required">*</span></label>
                  <div>
                    <q-select
                      v-model="model.year" clearable use-input outlined stack-label hide-bottom-space :dense="true"
                      :options="YearOptions" option-value="value" option-label="text" emit-value map-options :error="v$.year.$error"
                      :error-message="v$.year.$errors[0]?.$message" @filter="filterFn2" @blur="v$.year.$touch"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-6 col-lg-3" style="width: auto;">
                  <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAdd" />
                </div>
              </div>
              <div class="q-pt-md cardTable">
                <div class="q-gutter-y-md" />
                <q-table
                  ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
                  no-data-label="No data available" binary-state-sort
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props" :style="col.name === 'employmentTypeId' ? 'width: 10%;' : ['casualLeaves', 'sickLeaves'].includes(col.name) ? 'width: 5%;' : ''">{{ col.label }}<span v-if="['employmentTypeId', 'casualLeaves', 'sickLeaves'].includes(col.name)" class="required">*</span></q-th>
                      <q-th style="width: 5%;" class="text-center">Actions</q-th>
                    </q-tr>
                  </template>
                  <template #top-row>
                    <q-tr v-if="mode == 'add' && editingRow" class="row-highlight">
                      <q-td width="35%">
                        <div>
                          <q-select
                            v-model="editingRow.employmentTypeId" clearable use-input outlined stack-label hide-bottom-space :dense="true"
                            :options="employeeTypeList" option-value="value" option-label="text" emit-value map-options :error="editingRowV$.employmentTypeId.$error"
                            :error-message="editingRowV$.employmentTypeId.$errors[0]?.$message" @filter="filterFn3" @blur="editingRowV$.employmentTypeId.$touch"
                          >
                            <template #option="{ itemProps, opt }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center" style="max-width: 100%;">
                                    <span>{{ opt.text }}</span>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </q-td>
                      <q-td width="30%">
                        <q-input
                          v-model="editingRow.casualLeaves" outlined stack-label hide-bottom-space dense mask="##.##" hint="00.00"
                          :error="editingRowV$.casualLeaves.$error" :error-message="editingRowV$.casualLeaves.$errors[0]?.$message" @blur="editingRowV$.casualLeaves.$touch"
                        />
                      </q-td>
                      <q-td width="30%">
                        <q-input
                          v-model="editingRow.sickLeaves" outlined stack-label hide-bottom-space dense mask="##.##" hint="00.00"
                          :error="editingRowV$.sickLeaves.$error" :error-message="editingRowV$.sickLeaves.$errors[0]?.$message" @blur="editingRowV$.sickLeaves.$touch"
                        />
                      </q-td>
                      <q-td width="5%" class="text-center">
                        <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                          <q-tooltip>Save</q-tooltip>
                        </q-icon>
                        <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                          <q-tooltip>Cancel</q-tooltip>
                        </q-icon>
                      </q-td>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                      <q-td class="text-left" width="35%">
                        <q-select
                          v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                          v-model="editingRow.employmentTypeId" clearable use-input outlined stack-label hide-bottom-space :dense="true"
                          :options="employeeTypeList" option-value="value" option-label="text" emit-value map-options :error="editingRowV$.employmentTypeId.$error"
                          :error-message="editingRowV$.employmentTypeId.$errors[0]?.$message" @filter="filterFn3" @blur="editingRowV$.employmentTypeId.$touch"
                        >
                          <template #option="{ itemProps, opt }">
                            <q-item v-bind="itemProps">
                              <q-item-section>
                                <div class="row q-col-gutter-x-md items-center" style="max-width: 100%;">
                                  <span>{{ opt.text }}</span>
                                </div>
                              </q-item-section>
                            </q-item>
                          </template>
                        </q-select>
                        <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getEmployeeType(props.row.employmentTypeId) }} </span>
                      </q-td>
                      <q-td class="text-left" width="30%">
                        <q-input
                          v-if="mode == 'edit' && editingRow && props.row.id === activeRowId" v-model="editingRow.casualLeaves" outlined stack-label hide-bottom-space dense mask="##.##" hint="00.00"
                          :error="editingRowV$.casualLeaves.$error" :error-message="editingRowV$.casualLeaves.$errors[0]?.$message" @blur="editingRowV$.casualLeaves.$touch"
                        />
                        <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.casualLeaves }} </span>
                      </q-td>
                      <q-td class="text-left" width="30%">
                        <q-input
                          v-if="mode == 'edit' && editingRow && props.row.id === activeRowId" v-model="editingRow.sickLeaves" outlined stack-label hide-bottom-space dense mask="##.##" hint="00.00"
                          :error="editingRowV$.sickLeaves.$error" :error-message="editingRowV$.sickLeaves.$errors[0]?.$message" @blur="editingRowV$.sickLeaves.$touch"
                        />
                        <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.sickLeaves }} </span>
                      </q-td>
                      <q-td width="5%" class="text-center">
                        <template v-if="mode == 'edit' && editingRow && props.row.id === activeRowId">
                          <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                            <q-tooltip>Save</q-tooltip>
                          </q-icon>
                          <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                            <q-tooltip>Cancel</q-tooltip>
                          </q-icon>
                        </template>
                        <template v-else>
                          <q-icon v-if="!props.row.deleted" name="o_edit" size="xs" class="cursor-pointer q-mr-lg" @click="onEditRow(props.row)">
                            <q-tooltip>Edit</q-tooltip>
                          </q-icon>
                          <q-icon v-if="!props.row.deleted" name="o_delete_outline" color="red" size="xs" class="cursor-pointer" @click="onDelete(props.row)">
                            <q-tooltip>Delete</q-tooltip>
                          </q-icon>
                          <q-icon v-if="props.row.deleted" name="o_redo" size="xs" class="cursor-pointer" @click="onUndo(props.row)">
                            <q-tooltip>Undo</q-tooltip>
                          </q-icon>
                        </template>
                      </q-td>
                    </q-tr>
                  </template>
                </q-table>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogClose" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, uid } from "quasar";
import useVuelidate from "@vuelidate/core";
import leaveRuleService from "../leaveRules.service";
import commonService from "services/common.service";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import _ from "lodash";
import { notifySuccess, notifyError, zwConfirmLeave } from "assets/utils";
// import useFilters from "composables/useFilters";

// Common variables
const loading = ref(true);
const processing = ref(false);
const mode = ref(null);
const editingRow = ref(null);

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "employmentTypeId", label: "Employment Type", field: "employmentTypeId", align: "left" },
  { name: "casualLeaves", label: "Casual Leaves", field: "casualLeaves", align: "left" },
  { name: "sickLeaves", label: "Sick Leaves", field: "sickLeaves", align: "left" }
]);

const options1 = ref([]);
const Year = new Date().getFullYear();
// const NextThirtyYear = parseInt(Year) + parseInt(5);
const NextThirtyYear = Year + 5;
const YearOptions = ref([]);
for (let i = Year; i <= NextThirtyYear; i++) {
  const yearObj = { value: i, text: String(i) }; // Ensure text is a string
  YearOptions.value.push(yearObj);
  options1.value.push(yearObj);
}

// Search year for dropdown
function filterFn2 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      YearOptions.value = options1.value;
    } else {
      YearOptions.value = options1.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}
// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const props = defineProps({ id: { type: String, default: "" } });

// Define model values
const model = ref({
  active: true
});

const editingRowrules = {
  employmentTypeId: { required: helpers.withMessage("Employment Type is Required", required) },
  casualLeaves: { required: helpers.withMessage("Casual Leaves is Required", required) },
  sickLeaves: { required: helpers.withMessage("Sick Leaves is Required", required) }
};

const editingRowV$ = useVuelidate(editingRowrules, editingRow, { $lazy: true, $autoDirty: true });

// Validation rules
const rules = {
  year: { required: helpers.withMessage("Year is required", required) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// On page rendering
onMounted(() => {
  getDropDown("EmploymentType");
});

const getLeaveRule = (ruleId) => {
  leaveRuleService.getLeaveRule(ruleId).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.year = resp.year;
    rows.value = resp.leaveRuleLinesList.map(item => ({
      ...item,
      employmentTypeId: item.employmentTypeId,
      casualLeaves: item.casualLeaves,
      sickLeaves: item.sickLeaves,
      // creditLeaves: item.creditLeaves,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

// Get all Employee Type list
const employeeTypeList = ref([]);
const employeeListArr = ref([]);
const options2 = ref([]);
function getDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    employeeListArr.value = resp;
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    employeeTypeList.value = responseData;
    options2.value = responseData;
  });
}

function filterFn3 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeTypeList.value = options2.value;
    } else {
      employeeTypeList.value = options2.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get row employee typee
function getEmployeeType (value) {
  if (value) {
    return employeeListArr.value.find((item) => item.id === value)?.dropdownValue;
  }
}

let isSaveDialog = false;
let isConfirmSaveDialog = false;

function onEditRow (item) {
  let isContinue = 0;
  if (isConfirmSaveDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onEditConfirm(item);
    }, () => {
      isContinue = 0;
    });
  } else {
    isContinue = 1;
  }
  if (isContinue === 1) {
    onEditConfirm(item);
  }
}

function onEditConfirm (item) {
  isSaveDialog = true;
  isConfirmSaveDialog = true;
  mode.value = "edit";
  editingRow.value = _.cloneDeep(item);
  activeRowId.value = item.id;
}

function onCancel () {
  isConfirmSaveDialog = false;
  mode.value = null;
  editingRow.value = null;
  activeRowId.value = null;
}

function onDialogClose () {
  if (isSaveDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onDialogCancel();
    }, () => {
    });
  } else {
    onDialogCancel();
  }
}

function onDelete (item) {
  isSaveDialog = true;
  item.deleted = true;
  const rowIndex = rows.value.findIndex((row) => row.id === item.id);
  if (rowIndex !== -1) {
    rows.value.splice(rowIndex, 1, {
      ...rows.value[rowIndex],
      id: item.id,
      employeeId: item.employeeId,
      employmentTypeId: item.employmentTypeId,
      // creditLeaves: item.creditLeaves,
      casualLeaves: item.casualLeaves,
      sickLeaves: item.sickLeaves,
      flag: "Delete"
    });
  }
  activeRowId.value = item.id;
}

function onUndo (item) {
  item.deleted = false;
  activeRowId.value = null;
}

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getLeaveRule(props.id);
  }
}, { immediate: true });

async function onSave () {
  if (mode.value === "edit") {
    if (!await editingRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveDialog = false;
    let isDuplicate = 0;
    rows.value.forEach((item, index) => {
      if (item.employmentTypeId.toLowerCase() === editingRow.value.employmentTypeId.toLowerCase() && item.id !== editingRow.value.id) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const rowIndex = rows.value.findIndex((row) => row.id === editingRow.value.id);
      if (rowIndex !== -1) {
        rows.value.splice(rowIndex, 1, {
          ...rows.value[rowIndex],
          employmentTypeId: editingRow.value.employmentTypeId,
          // creditLeaves: editingRow.value.creditLeaves,
          casualLeaves: editingRow.value.casualLeaves,
          sickLeaves: editingRow.value.sickLeaves,
          flag: "Edit"
        });
        editingRow.value = null;
        mode.value = null;
        activeRowId.value = null;
      }
    } else {
      notifyError({ message: "Duplicate Employee Type." });
    }
  } else if (mode.value === "add") {
    if (!await editingRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveDialog = false;
    let isDuplicate = 0;
    rows.value.forEach((item, index) => {
      if (item.employmentTypeId.toLowerCase() === editingRow.value.employmentTypeId.toLowerCase()) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const newRow = {
        id: uid(),
        employmentTypeId: editingRow.value.employmentTypeId,
        casualLeaves: editingRow.value.casualLeaves,
        sickLeaves: editingRow.value.sickLeaves,
        flag: "New"
      };
      rows.value.unshift(newRow);
      mode.value = null;
      activeRowId.value = null;
    } else {
      notifyError({ message: "Duplicate Employee Type." });
    }
  }
}

// async function onSave () {
//   if (!await editingRowV$.value.$validate()) {
//     return;
//   }
//   isConfirmSaveDialog = false;
//   if (mode.value === "edit") {
//     let isDuplicate = 0;
//     rows.value.forEach((item, index) => {
//       if (item.employmentTypeId.toLowerCase() === editingRow.value.employmentTypeId.toLowerCase() && index !== editingRow.value.index) {
//         isDuplicate = 1;
//       }
//     });
//     if (isDuplicate === 0) {
//       const rowIndex = rows.value.findIndex((row) => row.id === editingRow.value.id);
//       if (rowIndex !== -1) {
//         rows.value.splice(rowIndex, 1, {
//           ...rows.value[rowIndex],
//           employmentTypeId: editingRow.value.employmentTypeId,
//           // creditLeaves: editingRow.value.creditLeaves,
//           casualLeaves: editingRow.value.casualLeaves,
//           sickLeaves: editingRow.value.sickLeaves,
//           flag: "Edit"
//         });
//         editingRow.value = null;
//         mode.value = null;
//         activeRowId.value = null;
//       }
//     } else {
//       notifyError({ message: "Duplicate Employee Type." });
//     }
//   } else if (mode.value === "add") {
//     let isDuplicate = 0;
//     rows.value.forEach((item, index) => {
//       if (item.employmentTypeId.toLowerCase() === editingRow.value.employmentTypeId.toLowerCase()) {
//         isDuplicate = 1;
//       }
//     });
//     if (isDuplicate === 0) {
//       const newRow = {
//         id: uid(),
//         employmentTypeId: editingRow.value.employmentTypeId,
//         casualLeaves: editingRow.value.casualLeaves,
//         sickLeaves: editingRow.value.sickLeaves,
//         flag: "New"
//       };
//       rows.value.unshift(newRow);
//       mode.value = null;
//       activeRowId.value = null;
//     } else {
//       notifyError({ message: "Duplicate Employee Type." });
//     }
//   }
// }

function onAdd () {
  let isAddContinue = 0;
  if (isConfirmSaveDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onAddConfirm();
    }, () => {
      isAddContinue = 0;
    });
  } else {
    isAddContinue = 1;
  }
  if (isAddContinue === 1) {
    onAddConfirm();
  }
}
function onAddConfirm () {
  isSaveDialog = true;
  isConfirmSaveDialog = true;
  mode.value = "add";
  editingRow.value = {
    employmentTypeId: "",
    // creditLeaves: ""
    casualLeaves: "",
    sickLeaves: ""
  };
  activeRowId.value = null;
}

// Submit form
const onSubmit = async () => {
  if (!await v$.value.$validate()) {
    return;
  }
  if (rows.value.length === 0) {
    notifyError({ message: "Add at least one Leave Rule." });
    return;
  }
  // Check if there's an active row that hasn't been saved
  if (!await editingRowV$.value.$validate() && (mode.value === "add" || mode.value === "edit")) {
    notifyError({ message: "Please fill in all required fields" });
    return;
  }
  if ((mode.value === "edit" || mode.value === "add")) {
    return;
  }

  processing.value = true;
  model.value.leaveRuleLines = rows.value;
  leaveRuleService.saveLeaveRules(props.id, model.value).then((resp) => {
    notifySuccess({ message: "Leave Rule is saved successfully." });
    onDialogOK();
  }).finally(() => {
    processing.value = false;
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
.edit_projectModule .q-select__dropdown-icon{
  display: none;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
</style>
