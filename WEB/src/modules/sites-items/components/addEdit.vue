<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1500px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Site Items</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset class="q-mb-lg">
              <legend>Site Item Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4">
                  <label class="label q-mb-xs text-black">Item Subcategory</label>
                  <span class="required">*</span>
                  <div>
                    <q-select
                      v-model="model.itemSubCategoryId"
                      clearable
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="itemSubcategoryList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      :disable="!!props.id"
                      :error="v$.itemSubCategoryId.$error"
                      :error-message="v$.itemSubCategoryId.$errors[0]?.$message"
                      @blur="v$.itemSubCategoryId.$touch"
                      @filter="getAllItemSubcategoryListForFilter"
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
                <div class="col-12 col-md-4">
                  <label class="label q-mb-sm text-black">
                    Item Name<span class="required">*</span>
                  </label>
                  <q-input
                    v-model="model.itemName"
                    outlined
                    dense
                    hide-bottom-space
                    :error="v$.itemName.$error"
                    :error-message="v$.itemName.$errors[0]?.$message"
                    @blur="v$.itemName.$touch"
                  />
                </div>
              </div>
              <div class="row items-center q-col-gutter-x-md">
                <div class="col-12">
                  <label class="label q-mb-sm text-black">
                    Description<span class="required">*</span>
                  </label>
                  <q-editor
                    v-model="model.description"
                    :dense="$q.screen.lt.md"
                    :toolbar="toolbar"
                    :fonts="fonts"
                    @blur="v$.description.$touch"
                  />
                  <div
                    v-if="v$.description.$error"
                    class="text-negative text-caption q-mt-xs"
                  >
                    {{ v$.description.$errors[0]?.$message }}
                  </div>
                </div>
                <q-space />
              </div>
              <div class="row q-my-md justify-end">
                <div class="col-auto">
                  <q-btn
                    color="primary"
                    icon="o_add"
                    label="Add Attributes"
                    no-caps
                    :disable="!model.itemSubCategoryId || !columns.length"
                    @click="onAttributeAdd"
                  />
                </div>
              </div>
              <div
                v-if="!model.itemSubCategoryId"
                class="text-grey-6 q-mt-md"
              >
                Please select Item Subcategory to add attributes
              </div>
              <div
                v-else-if="!columns.length"
                class="text-grey-6 q-mt-md"
              >
                Selected Subcategory has no attributes
              </div>
              <q-table
                v-else
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
                      {{ col.name }}
                    </q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr
                    :props="props"
                    :class="props.row.deleted ? 'hidden' : ''"
                  >
                    <q-td
                      v-for="(col, colIndex) in props.cols"
                      :key="col.id"
                    >
                      <q-input
                        v-if="col.fieldType === 'Input'"
                        v-model="props.row.sitesItemsAttributesList[colIndex].value"
                        dense
                        outlined
                        hide-bottom-space
                        :error="!props.row.sitesItemsAttributesList[colIndex].value && props.row.touched"
                        :error-message="!props.row.sitesItemsAttributesList[colIndex].value && props.row.touched ? 'Value is Required' : ''"
                        @blur="props.row.touched = true"
                      />
                      <q-select
                        v-else-if="col.fieldType === 'Dropdown'"
                        v-model="props.row.sitesItemsAttributesList[colIndex].value"
                        :options="col.attributeValues"
                        clearable
                        use-input
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        option-value="text"
                        option-label="text"
                        emit-value
                        map-options
                        :error="!props.row.sitesItemsAttributesList[colIndex].value && props.row.touched"
                        :error-message="!props.row.sitesItemsAttributesList[colIndex].value && props.row.touched ? 'Value is Required' : ''"
                        @blur="props.row.touched = true"
                      />

                    </q-td>
                    <q-td auto-width class="text-center">
                      <q-icon
                        name="o_delete"
                        size="sm"
                        class="cursor-pointer text-negative"
                        @click="onAttributeDelete(props.row)"
                      >
                        <q-tooltip>Delete</q-tooltip>
                      </q-icon>
                    </q-td>
                  </q-tr>
                </template>
              </q-table>
            </fieldset>
            <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
              <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogClose" />
              <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" :disable="processing" no-caps />
            </q-card-actions>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>
<script setup>
import { ref, onMounted, watch } from "vue";
import { useDialogPluginComponent, useQuasar, uid } from "quasar";
import { notifySuccess, notifyError } from "assets/utils";
import { required, helpers } from "@vuelidate/validators";
import useVuelidate from "@vuelidate/core";
import _ from "lodash";

import itemCategoryService from "modules/items/itemCategory.service";
import itemSubcategoryAttributeService from "modules/items/itemSubCategoryAttributes.service";
import sitesItemsService from "../sitesItems.service";

const loading = ref(true);
const processing = ref(false);
const $q = useQuasar();
const columns = ref([]);
const rowValidations = ref([]);

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogOK, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  itemSubCategoryId: "",
  itemName: "",
  description: ""
});

const rows = ref([]);
const pagination = ref({ sortBy: "", descending: false, rowsPerPage: 20, page: 1 });

const descriptionRequired = helpers.withMessage(
  "Description is required",
  value => {
    if (!value) return false;
    const descriptionText = stripHtml(value);
    const containsImage = hasImage(value);
    return descriptionText || containsImage;
  }
);

const stripHtml = (html) => {
  if (!html) return "";
  return html.replace(/<[^>]*>/g, "").replace(/&nbsp;/g, " ").trim();
};

const hasImage = (html) => {
  if (!html) return false;
  return /<img\s+[^>]*src=/i.test(html);
};

// Validation rules
const rules = {
  itemSubCategoryId: { required: helpers.withMessage("Item Subcategory is required", required) },
  itemName: { required: helpers.withMessage("Item Name is required", required) },
  description: {
    descriptionRequired
  }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Define rules for the row
const rowRules = {
  value: { required: helpers.withMessage("Value is Required", required) }
};

function syncRowValidations () {
  rowValidations.value = rows.value.map(row =>
    !row.deleted ? useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true }) : null
  );
}

const getSitesItemDetailsById = async () => {
  loading.value = true;
  try {
    const resp = await sitesItemsService.getSitesItemDetailsById(props.id);
    model.value = _.cloneDeep(resp);
    // Load attribute columns
    await getAllSitesItemSubCategoryAttributesListByItemSubCategoryId(
      model.value.itemSubCategoryId
    );
    const attributes = resp.sitesItemsAttributeList || {};
    // Group attributes by attributeId
    const attributeMap = attributes.reduce((map, item) => {
      if (!map[item.itemSubCategoryAttributeId]) {
        map[item.itemSubCategoryAttributeId] = [];
      }
      map[item.itemSubCategoryAttributeId].push(item);
      return map;
    }, {});
    // Find max rows
    const maxRows = Math.max(
      ...Object.values(attributeMap).map(a => a.length),
      0
    );
    // Create rows same as view page structure
    rows.value = Array.from({ length: maxRows }, (_, rowIndex) => ({
      rowCounter: rowIndex,
      deleted: false,
      sitesItemsAttributesList: columns.value.map(col => {
        const attr =
          attributeMap[col.itemSubCategoryAttributeId]?.[rowIndex];

        return {
          id: attr?.id ?? null,
          itemSubCategoryAttributeId: col.itemSubCategoryAttributeId,
          value: attr?.value ?? null
        };
      })
    }));
  } finally {
    loading.value = false;
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get item subcategories
// --------------------------------------------------------------------------------------------------------------------------------------------------
const itemSubcategoryList = ref([]);
const itemSubcategoryFilter = ref([]);
const getAllItemSubcategoryList = () => {
  itemCategoryService.getAllItemSubcategoryList()
    .then((resp) => {
      const responseData = resp.map((item) => ({ text: item.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
      itemSubcategoryList.value = responseData;
      itemSubcategoryFilter.value = responseData;
    })
    .finally(() => {
    });
};

// Search item subcategory for dropdown
function getAllItemSubcategoryListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      itemSubcategoryList.value = itemSubcategoryFilter.value;
    } else {
      itemSubcategoryList.value = itemSubcategoryFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const getAllSitesItemSubCategoryAttributesListByItemSubCategoryId = (subCategoryId) => {
  return itemSubcategoryAttributeService
    .getAllSitesItemSubCategoryAttributesListByItemSubCategoryId(subCategoryId)
    .then((resp) => {
      columns.value = resp.map(item => ({
        id: item.id,
        itemSubCategoryAttributeId: item.itemSubCategoryAttributeId,
        name: item.itemSubCategoryAttributes.name,
        fieldType: item.itemSubCategoryAttributes.fieldType,
        attributeValues: (item.itemSubCategoryAttributes.itemSubCategoryAttributesValues || [])
          .map(attr => ({
            text: attr.text,
            value: attr.id
          })),
        align: "left"
      }));

      rows.value.forEach(row => {
        row.sitesItemsAttributesList = columns.value.map(col => ({
          itemSubCategoryAttributeId: col.itemSubCategoryAttributeId,
          value: null
        }));
        row.touched = false;
      });
    });
};

// Actions
const onAttributeAdd = () => {
  const newRow = {
    sitesItemsAttributesList: []
  };

  columns.value.forEach(col => {
    newRow.sitesItemsAttributesList.push({
      id: uid(),
      itemSubCategoryAttributeId: col.itemSubCategoryAttributeId,
      value: null,
      deleted: false
    });
  });
  rows.value.push(newRow);
  syncRowValidations();
};

const onAttributeDelete = (row) => {
  const index = rows.value.indexOf(row);
  if (index === -1) return;

  const activeRows = rows.value.filter(r => !r.deleted);
  if (activeRows.length > 1) {
    rows.value[index].deleted = true;
  } else {
    notifyError({ message: "Please add at least one attribute." });
  }
};

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    if (!await v$.value.$validate()) {
      return;
    }
    // Stop if no rows exist
    if (rows.value.length === 0) {
      notifyError({ message: "Add at least one attribute." });
      return;
    }
    const nonDeletedRows = rows.value.filter(row => !row.deleted);
    // Check empty fields
    let isValid = true;
    for (const row of nonDeletedRows) {
      row.touched = true; // mark all rows as touched to show required
      for (const attr of row.sitesItemsAttributesList) {
        if (!attr.value) isValid = false;
      }
    }
    // Stop submission if any row is invalid
    if (!isValid) return;
    if ((await v$.value.$validate() && isValid)) {
    // Prepare payload including deleted flags
      const payload = {
        itemSubCategoryId: model.value.itemSubCategoryId,
        itemName: model.value.itemName,
        description: model.value.description,
        sitesItemsAttributesList: rows.value.flatMap(row =>
          row.sitesItemsAttributesList.map(attr => ({
            ...attr,
            deleted: row.deleted || false
          }))
        )
      };
      // Save API call
      sitesItemsService.saveSitesItem(props.id, payload).then((resp) => {
        notifySuccess({ message: "Site Item saved successfully." });

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

watch(
  () => model.value.itemSubCategoryId,
  (val) => {
    if (!val) {
      columns.value = [];
      rows.value = [];
      return;
    }

    // Only run for Add page
    if (!props.id) {
      getAllSitesItemSubCategoryAttributesListByItemSubCategoryId(val);
    }
  }
);

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getSitesItemDetailsById();
  }
}, { immediate: true });

onMounted(() => {
  getAllItemSubcategoryList();
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
