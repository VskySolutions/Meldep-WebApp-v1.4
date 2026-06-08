<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white q-mr-lg">{{ id ? "Edit" : "Add" }} Dropdown Type</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Dropdown Type Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                  <div class="row items-center q-gutter-md">
                    <div class="text-black">
                      Type
                    </div>
                    <div class="row items-center q-gutter-sm">
                      <q-radio
                        v-model="model.selectionType"
                        :val="true"
                        label="Group"
                        dense
                        :disable="!!props.id"
                      />
                      <q-radio
                        v-model="model.selectionType"
                        :val="false"
                        label="Single"
                        dense
                        :disable="!!props.id"
                      />
                    </div>
                  </div>
                </div>
                <div v-if="model.selectionType === false" class="col-12 col-sm-6 col-md-6 col-lg-6">
                  <div class="row items-center q-gutter-md">
                    <div class="text-black">
                      Values Sorting
                    </div>
                    <div class="row items-center q-gutter-sm">
                      <q-radio
                        v-model="model.isAlphabeticalOrNumerical"
                        label="Alphabetical"
                        dense
                        class="text-black q-mr-sm"
                        :val="false"
                      />
                      <q-radio
                        v-model="model.isAlphabeticalOrNumerical"
                        label="Numerical"
                        dense
                        class="text-black"
                        :val="true"
                      />
                    </div>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md">
                <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                  <div class="q-mb-xs text-black">Module Name<span class="required">*</span></div>
                  <div>
                    <q-select
                      v-model="model.moduleName"
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="moduleOptionsList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      :error="v$.moduleName.$error"
                      :error-message="v$.moduleName.$errors[0]?.$message"
                      @blur="v$.moduleName.$touch"
                      @filter="getAllModuleListForFilter"
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
                <div v-if="model.selectionType === true" class="col-12 col-sm-6 col-md-6 col-lg-6">
                  <div class="q-mb-xs text-black">Group Name<span class="required">*</span></div>
                  <div>
                    <q-input
                      v-model="model.groupName"
                      outlined
                      dense
                      :disable="!!props.id"
                      :error="v$.groupName.$error"
                      :error-message="v$.groupName.$errors[0]?.$message"
                      @blur="v$.groupName.$touch"
                    />
                  </div>
                </div>
                <div v-if="model.selectionType === false" class="col-12 col-sm-6 col-md-6 col-lg-6">
                  <div class="q-mb-xs text-black">Dropdown Type<span class="required">*</span></div>
                  <div>
                    <q-input
                      v-model="model.type"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :disable="!!props.id"
                      :error="v$.type.$error"
                      :error-message="v$.type.$errors[0]?.$message"
                      @blur="v$.type.$touch"
                    />
                  </div>
                </div>
              </div>
              <div v-if="model.selectionType === true">
                <div class="row justify-end q-mb-sm">
                  <q-btn
                    color="primary"
                    icon="o_add"
                    label="Add"
                    no-caps
                    @click="onAddDropDownType"
                  />
                </div>
                <q-table
                  ref="tableRef"
                  v-model:pagination="pagination"
                  bordered
                  class="no-shadow"
                  virtual-scroll
                  :loading="loading"
                  :rows="rows"
                  :columns="columns"
                  row-key="id"
                  separator="cell"
                  binary-state-sort
                  :rows-per-page-options="[20, 50, 100, 200, 500]"
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th
                        v-for="col in props.cols"
                        :key="col.name"
                        :props="props"
                      >
                        {{ col.label }}
                        <span v-if="['type'].includes(col.name)" class="required">*</span>
                      </q-th>
                      <q-th auto-width class="text-center">Actions</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :class="props.row.deleted ? 'hidden' : ''">
                      <q-td style="width: 1dvb;">
                        <div>
                          <q-input
                            v-model="props.row.sortOrder"
                            outlined
                            stack-label
                            hide-bottom-space
                            dense
                            @blur="fixSortOrder(props.row)"
                          />
                        </div>
                      </q-td>
                      <q-td style="width: 80%;">
                        <div>
                          <q-input
                            v-model="props.row.type"
                            outlined
                            stack-label
                            hide-bottom-space
                            dense
                            :error="rowValidations[props.rowIndex]?.value?.type.$error"
                            :error-message="rowValidations[props.rowIndex]?.value?.type.$errors[0]?.$message"
                            @blur="rowValidations[props.rowIndex]?.value?.type.$touch"
                          />
                        </div>
                      </q-td>
                      <q-td class="text-center" style="width: 10%;">
                        <q-icon name="o_delete" size="xs" class="cursor-pointer text-red" @click="onDeleteDropDownType(props.rowIndex)">
                          <q-tooltip>Delete</q-tooltip>
                        </q-icon>
                      </q-td>
                    </q-tr>
                  </template>
                </q-table>
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
import { useDialogPluginComponent, uid } from "quasar";
import useVuelidate from "@vuelidate/core";
import dropdowntypeService from "modules/dropdown/dropdown.service";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, watch } from "vue";
import _ from "lodash";
import { notifySuccess, notifyError } from "assets/utils";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const loading = ref(true);
const processing = ref(false);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Define model values
const model = ref({
  groupName: "",
  moduleName: "",
  dropDownValue: "",
  selectionType: false,
  isAlphabeticalOrNumerical: false
});

const moduleOptions = [
  { value: 'Dashboard', text: 'Dashboard' },
  { value: 'My Work', text: 'My Work' },
  { value: 'Talent Hire', text: 'Talent Hire' },
  { value: 'Project Management', text: 'Project Management' },
  { value: 'Org Management', text: 'Org Management' },
  { value: 'User Management', text: 'User Management' },
  { value: 'SDLC', text: 'SDLC' },
  { value: 'CRM', text: 'CRM' },
  { value: 'Finance', text: 'Finance' },
  { value: 'Marketing', text: 'Marketing' },
  { value: 'Infrastructure', text: 'Infrastructure' },
  { value: 'Item', text: 'Item' },
  { value: 'Reports', text: 'Reports' },
  { value: 'Help Desk', text: 'Help Desk' }
].sort((a, b) => a.text.localeCompare(b.text));

// Validation rules
const rules = {
  groupName: {
    required: helpers.withMessage(
      "Group Name is required",
      (value) => {
        if (model.value.selectionType === true) {
          return value && value.toString().trim().length > 0;
        }
        return true;
      }
    )
  },
  moduleName: { required: helpers.withMessage("Module Name is required", required) },
  type: {
    required: helpers.withMessage(
      "Dropdown Type is required",
      (value) => {
        if (model.value.selectionType === false) {
          return value && value.toString().trim().length > 0;
        }
        return true;
      }
    ),
    minLength: minLength(1),
    maxLength: maxLength(50)
  }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const rowRules = {
  type: { required: helpers.withMessage("Dropdown Type is required", required) }
};

const rowValidations = ref([]);
const rows = ref([]);
const rowCounter = ref(0);
const pagination = ref({ sortBy: "", descending: false, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "sortOrder", label: "Sort Order", field: "sortOrder", align: "left", sortable: true },
  { name: "type", label: "Dropdown Type", field: "type", align: "left", sortable: true }
]);

// get person details on edit mode
const getDropDownType = () => {
  loading.value = true;
  dropdowntypeService.getDropDownType(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.selectionType = !!resp.groupName;
    rows.value = resp.dropDownTypeList.map(item => ({
      ...item
    }));
  }).finally(() => {
    loading.value = false;
  });
};

const moduleOptionsListFilter = ref([...moduleOptions]);
const moduleOptionsList = ref([...moduleOptions]);
function getAllModuleListForFilter(val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";

    if (needle === "") {
      moduleOptionsList.value = moduleOptionsListFilter.value;
    } else {
      moduleOptionsList.value = moduleOptionsListFilter.value.filter(v =>
        v.text.toLowerCase().includes(needle)
      );
    }
  });
}

function onAddDropDownType () {
  const currentCounter = ++rowCounter.value;
  const maxSortOrder = rows.value
    .filter(row => !row.deleted && row.sortOrder !== "")
    .reduce((max, row) => {
      return Math.max(max, Number(row.sortOrder));
    }, 0);

  rows.value.unshift({
    id: uid(),
    sortOrder: maxSortOrder + 1 || 0,
    type: "",
    deleted: false,
    rowCounter: currentCounter
  });
}

const onDeleteDropDownType = (index) => {
  if (rows.value.filter(row => row.deleted === false).length > 1) {
    rows.value[index].deleted = true;
  } else {
    notifyError({ message: "Please add at least one dropdown type." });
  }
};

// Submit form
const onSubmit = async () => {
  processing.value = true;

  try {
    let isValid = true;
    if (rows.value.length === 0 && model.value.selectionType === true && await v$.value.$validate()) {
      notifyError({ message: "Add at least one dropdown type." });
      return;
    }
    // Initialize validations for all rows
    const nonDeletedRows = rows.value.filter(row => !row.deleted);
    rowValidations.value = nonDeletedRows.map((row) =>
      useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true })
    );

    // Validate each row
    for (const [index, validation] of rowValidations.value.entries()) {
      if (validation?.value) {
        await validation.value.$touch(); // Mark the row as touched
        const isRowValid = await validation.value.$validate(); // Validate the row
        if (!isRowValid) {
          isValid = false; // If any row is invalid, set isValid to false
        }
      } else {
        console.error(`Validation object for row ${index} is undefined`);
        isValid = false;
      }
    }

    if (!isValid || !(await v$.value.$validate())) return;
    if (await v$.value.$validate() && isValid) {
      processing.value = true;
      if (model.value.selectionType === true) {
        const payload = {
          moduleName: model.value.moduleName,
          groupName: model.value.groupName,
          dropDownTypeList: rows.value
        };
        // Call bulk save method
        await dropdowntypeService.saveBulkDropDownTypes(props.id, payload);
      } else {
      // Call regular save method
        await dropdowntypeService.saveDropDownType(props.id, model.value);
      }
      // Notify success
      notifySuccess({ message: "DropDown Type is saved successfully." });
      onDialogOK();
    }
  } catch (error) {
    console.error("Error in submitting:", error);
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

function fixSortOrder (row) {
  // If empty, null, or not a number → default to 0
  if (!row.sortOrder || row.sortOrder === "" || isNaN(row.sortOrder)) {
    row.sortOrder = 0;
  } else {
    row.sortOrder = Number(row.sortOrder); // convert string to number
  }
}

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getDropDownType();
  }
}, { immediate: true });

watch(() => model.value.selectionType, () => {
  v$.value.$reset();
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
