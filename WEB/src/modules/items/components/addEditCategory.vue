<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1000px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Add Bulk Subcategories</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset class="q-mb-lg">
              <div class="row items-center q-my-sm  q-col-gutter-x-md">
                <div class="col-12 col-md-4">
                  <label class="label q-mb-sm text-black">
                    Category Name<span class="required">*</span>
                  </label>
                  <q-input
                    v-model="model.name"
                    outlined
                    dense
                    hide-bottom-space
                    :error="v$.name.$error"
                    :error-message="v$.name.$errors[0]?.$message"
                    @blur="v$.name.$touch"
                  />
                </div>
                <div class="col-12 col-md-4">
                  <label class="label q-mb-sm text-black">
                    Prefix<span class="required">*</span>
                  </label>
                  <q-input
                    v-model="model.prefix"
                    outlined
                    dense
                    hide-bottom-space
                    :error="v$.prefix.$error"
                    :error-message="v$.prefix.$errors[0]?.$message"
                    @blur="v$.prefix.$touch"
                  />
                </div>
                <q-space />
                <div class="col-auto">
                  <q-btn
                    color="primary"
                    icon="o_add"
                    label="Add"
                    no-caps
                    @click="onAddItemSubcategory"
                  />
                </div>
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
                      <span v-if="['name', 'prefix'].includes(col.name)" class="required">*</span>
                    </q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :class="props.row.deleted ? 'hidden' : ''">
                    <q-td width="40%">
                      <div>
                        <q-input
                          v-model="props.row.name"
                          outlined
                          hide-bottom-space
                          dense
                          :error="rowValidations[props.rowIndex]?.value?.name.$error"
                          :error-message="rowValidations[props.rowIndex]?.value?.name.$errors[0]?.$message"
                          @blur="rowValidations[props.rowIndex]?.value?.name.$touch"
                          @keydown.enter.prevent="generatePrefixForSubcategoryName($event, props.rowIndex)"
                        >
                          <q-tooltip>
                            Press enter key after typing the Subcategory name to auto-generate the prefix
                          </q-tooltip>
                        </q-input>
                      </div>
                    </q-td>
                    <q-td width="40%">
                      <div>
                        <q-input
                          v-model="props.row.prefix"
                          outlined
                          stack-label
                          hide-bottom-space
                          dense
                          :error="rowValidations[props.rowIndex]?.value?.prefix.$error"
                          :error-message="rowValidations[props.rowIndex]?.value?.prefix.$errors[0]?.$message"
                          @blur="rowValidations[props.rowIndex]?.value?.name.$touch"
                        />
                      </div>
                    </q-td>
                    <q-td class="text-center" style="width: 10%;">
                      <q-icon name="o_delete" size="xs" class="cursor-pointer text-red" @click="onDeleteItemSubcategory(props.rowIndex)">
                        <q-tooltip>Delete</q-tooltip>
                      </q-icon>
                    </q-td>
                  </q-tr>
                </template>
              </q-table>
            </fieldset>
            <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
              <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogClose" />
              <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
            </q-card-actions>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>
<script setup>
import { ref, watch } from "vue";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { useDialogPluginComponent, uid } from "quasar";
import { notifySuccess, notifyError } from "assets/utils";
import _ from "lodash";

import itemCategoryService from "../itemCategory.service";

const loading = ref(true);
const processing = ref(false);
const props = defineProps({ id: { type: String, default: "" } });

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogOK, onDialogHide } = useDialogPluginComponent();

const model = ref({
  name: ""
});

// Validation rules
const rules = {
  name: { required: helpers.withMessage("Item Category is required", required) },
  prefix: { required: helpers.withMessage("Item Category Prefix is required", required) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const rowRules = {
  name: { required: helpers.withMessage("Item Subcategory Name is required", required) },
  prefix: { required: helpers.withMessage("Item Prefix is required", required) }
};

const rowValidations = ref([]);
function syncRowValidations () {
  rowValidations.value = rows.value.map(row =>
    !row.deleted ? useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true }) : null
  );
}

const rows = ref([]);
const rowCounter = ref(0);
const pagination = ref({ sortBy: "", descending: false, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "name", label: "Subcategory Name", field: "name", align: "left", sortable: true },
  { name: "prefix", label: "Prefix", field: "prefix", align: "left", sortable: true }
]);

const getItemCategory = () => {
  loading.value = true;
  itemCategoryService.getItemCategoryDetailsById(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rows.value = resp.itemSubcategory.map(item => ({
      ...item,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

function onAddItemSubcategory () {
  // const currentCounter = rowCounter.value += 1;
  const currentCounter = ++rowCounter.value;
  rows.value.unshift({
    id: uid(),
    name: "",
    prefix: "",
    deleted: false,
    rowCounter: currentCounter
  });
  syncRowValidations();
}

const onDeleteItemSubcategory = (index) => {
  if (rows.value.filter(row => row.deleted === false).length > 1) {
    rows.value[index].deleted = true;
  } else {
    notifyError({ message: "Please add at least one subcategory." });
  }
};

const generatePrefixForSubcategoryName = async (event, rowIndex) => {
  const row = rows.value[rowIndex];
  row.name = event.target.value;
  // const prefixes = rows.value.map(r => r.prefix).filter(p => p);
  const prefix = await itemCategoryService.generatePrefixForSubcategoryName(row.name, null);
  row.prefix = prefix;
};

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    let isValid = true;
    if (!await v$.value.$validate()) { return; }
    if (rows.value.length === 0) {
      notifyError({ message: "Add at least one sub category." });
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

    if (!isValid) {
      return; // Prevent submission
    }
    if ((await v$.value.$validate() && isValid)) {
      //  Check for duplicate names or prefixes
      const uniqueSubcategoryNames = {};
      const uniquePrefixes = {};
      const duplicateSubcategoryNames = new Set();
      const duplicatePrefixes = new Set();

      for (const { name, prefix } of nonDeletedRows) {
        const normalizedSubcategoryName = name?.trim().toLowerCase();
        const normalizedPrefix = prefix?.trim().toLowerCase();

        if (normalizedSubcategoryName && uniqueSubcategoryNames[normalizedSubcategoryName]) {
          duplicateSubcategoryNames.add(name.trim());
        }

        if (normalizedPrefix && uniquePrefixes[normalizedPrefix]) {
          duplicatePrefixes.add(prefix.trim());
        }

        if (normalizedSubcategoryName) uniqueSubcategoryNames[normalizedSubcategoryName] = true;
        if (normalizedPrefix) uniquePrefixes[normalizedPrefix] = true;
      }
      const duplicateMessageSections = [];
      if (duplicateSubcategoryNames.size) {
        const quoted = [...duplicateSubcategoryNames].map(n => `'${n}'`);
        duplicateMessageSections.push(`Subcategory name: ${quoted.join(", ")}`);
      }
      if (duplicatePrefixes.size) {
        const quoted = [...duplicatePrefixes].map(p => `'${p}'`);
        duplicateMessageSections.push(`Prefix: ${quoted.join(", ")}`);
      }
      if (duplicateMessageSections.length) {
        notifyError({
          message: duplicateMessageSections.length === 1
            ? `${duplicateMessageSections[0]} already exists. Please try another.`
            : `The following already exist: ${duplicateMessageSections.join("; ")}. Please try different ones.`
        });
        processing.value = false;
        return;
      }

      processing.value = true;
      const payload = {
        name: model.value.name,
        itemSubcategoryList: rows.value,
        prefix: model.value.prefix,
        groupName: "Inventory"
      };
      itemCategoryService.saveBulkItemSubcategories(payload).then((resp) => {
        notifySuccess({ message: "Item Category is saved successfully." });
        onDialogOK();
      });
    }
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getItemCategory();
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
