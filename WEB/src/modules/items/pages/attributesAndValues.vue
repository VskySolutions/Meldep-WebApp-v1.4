<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center justify-between">
          <div class="col">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Item" />
              <q-breadcrumbs-el label="Item Categories & Subcategories" class="cursor-pointer" @click="$router.push('/item/add-categories-subcategories')" />
              <q-breadcrumbs-el label="Item Attributes & Values" />
            </q-breadcrumbs>
          </div>
          <div class="col-auto row q-gutter-sm justify-end">
            <q-btn
              icon="o_chevron_left"
              class="text-primary"
              outline
              label="Previous"
              @click="$router.push('/item/add-categories-subcategories')"
            />
            <q-btn
              class="text-primary"
              outline
              @click="$router.push('/item-subcategory/subcategory-and-attribute-mapping')"
            >
              <span>Next</span>
              <q-icon name="o_chevron_right" class="q-ml-xs" />
            </q-btn>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="q-px-md">
        <div class="row q-col-gutter-sm">
          <q-inner-loading :showing="isLoading" color="primary">
            <div class="row items-center justify-center">
              <q-spinner-ios size="40px" />
            </div>
          </q-inner-loading>
          <div class="col-12 col-md-6 q-mt-md">
            <div class="row items-center justify-between q-mb-md">
              <div class="text-h3 text-weight-medium">
                Attribute
              </div>
              <div class="row q-gutter-sm">
                <q-btn
                  color="primary"
                  icon="o_add"
                  label="Add"
                  no-caps
                  @click="onAddItemSubcategoryAttribute"
                />
              </div>
            </div>
            <div class="row items-center q-pa-sm bg-primary text-white text-h6">
              <div class="col-6">
                Attribute Name
              </div>
              <div class="col-4">
                Field Type
              </div>
              <div class="col-2 text-center">
                Actions
              </div>
            </div>
            <div
              v-if="!itemSubcategoryAttributes || itemSubcategoryAttributes.length === 0"
              class="text-center text-grey-6 q-pa-lg"
            >
              No Data Available
            </div>
            <div class="scroll scroll-container">
              <div
                v-for="(itemSubcategoryAttribute, itemSubcategoryAttributeIndex) in itemSubcategoryAttributes"
                :key="itemSubcategoryAttribute.id || itemSubcategoryAttribute.tempId"
                :class="['row items-center q-mb-md q-pa-sm', activeRowId && activeRowId === itemSubcategoryAttribute.id ? 'bg-grey-2' : '']"
                style="border: 1px solid #ccc;"
              >
                <div class="row items-center full-width">
                  <div class="col-6 cursor-pointer">
                    <template v-if="itemSubcategoryAttribute.showInputForNewItemSubcategoryAttribute || activeEdit.field === 'name' && activeEdit.rowId === itemSubcategoryAttribute.id">
                      <div class="row items-center no-wrap q-mr-sm">
                        <q-input
                          v-model="itemSubcategoryAttribute.name"
                          outlined
                          stack-label
                          hide-bottom-space
                          dense
                          class="full-width"
                          placeholder="Enter Item Attribute"
                          @keyup.enter="saveOrUpdateItemSubcategoryAttribute(itemSubcategoryAttribute);"
                        >
                          <q-tooltip>
                            Press enter key to save Item Attribute
                          </q-tooltip>
                        </q-input>
                        <q-btn
                          v-if="itemSubcategoryAttribute.name && !itemSubcategoryAttribute.showInputForNewItemSubcategoryAttribute"
                          icon="o_close"
                          size="xs"
                          color="black"
                          flat
                          round
                          dense
                          class="q-ml-sm"
                          @click="
                            itemSubcategoryAttribute.name = itemSubcategoryAttribute._oldName;
                            activeEdit = { rowId: null, field: '' }
                          "
                        />
                      </div>
                    </template>

                    <template v-else>
                      <div
                        class="q-pa-xs cursor-pointer"
                        @click="getAllItemSubcategoryAttributeValuesByAttributeId(itemSubcategoryAttribute.name, itemSubcategoryAttribute.fieldType, itemSubcategoryAttribute.id)"
                        @dblclick=" itemSubcategoryAttribute._oldName = itemSubcategoryAttribute.name; activeEdit = { rowId: itemSubcategoryAttribute.id, field: 'name' }"
                      >
                        <span style="flex: 1; word-break: break-word; white-space: normal;">{{ itemSubcategoryAttribute.name }}</span>
                      </div>
                    </template>

                    <q-tooltip v-if="itemSubcategoryAttribute.name && !itemSubcategoryAttribute.showInputForNewItemSubcategoryAttribute && activeEdit.rowId !== itemSubcategoryAttribute.id">Double click to edit</q-tooltip>
                  </div>
                  <div class="col-4 cursor-pointer">
                    <div class="row items-center no-wrap">
                      <q-radio
                        v-model="itemSubcategoryAttribute.fieldType"
                        label="Input"
                        dense
                        class="text-black"
                        val="Input"
                        @update:model-value="saveOrUpdateItemSubcategoryAttribute(itemSubcategoryAttribute)"
                        @click="getAllItemSubcategoryAttributeValuesByAttributeId(itemSubcategoryAttribute.name, itemSubcategoryAttribute.fieldType, itemSubcategoryAttribute.id)"
                      />
                      <q-radio
                        v-model="itemSubcategoryAttribute.fieldType"
                        label="Dropdown"
                        dense
                        class="text-black q-ml-md"
                        val="Dropdown"
                        @update:model-value="saveOrUpdateItemSubcategoryAttribute(itemSubcategoryAttribute)"
                        @click="getAllItemSubcategoryAttributeValuesByAttributeId(itemSubcategoryAttribute.name, itemSubcategoryAttribute.fieldType, itemSubcategoryAttribute.id)"
                      />
                    </div>
                  </div>
                  <div class="col-2 flex flex-center actions q-gutter-sm">
                    <q-icon
                      v-if="itemSubcategoryAttribute.id"
                      name="o_visibility"
                      class="cursor-pointer"
                      size="xs"
                      @click="onViewItemSubcategoryAttribute(itemSubcategoryAttribute.id)"
                    />
                    <q-badge
                      v-if="itemSubcategoryAttribute.totalItemSubCategoryAttributesValuesCount > 0"
                      color="primary"
                      text-color="white"
                      square
                      class="justify-center"
                      style="width: 25px; height: 25px;"
                    >
                      {{ itemSubcategoryAttribute.totalItemSubCategoryAttributesValuesCount }}
                      <q-tooltip>Number of Attribute Values</q-tooltip>
                    </q-badge>
                    <q-icon
                      name="o_delete"
                      size="xs"
                      :color="itemSubcategoryAttribute.totalItemSubCategoryAttributesValuesCount > 0 ? 'red-3' : 'negative'"
                      class="cursor-pointer"
                      @click="onDeleteItemSubCategoryAttribute(itemSubcategoryAttribute, itemSubcategoryAttributeIndex)"
                    >
                      <q-tooltip>Delete</q-tooltip>
                    </q-icon>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-12 col-md-6 q-mt-md">
            <div class="row items-center justify-between q-mb-md">
              <div
                v-if="selectedItemSubcategoryAttribute?.name"
                class="row items-center justify-between"
              >
                <div class="text-h3">
                  <div v-if="!itemSubcategoryAttributesValues || itemSubcategoryAttributesValues.length === 0">
                    Values
                  </div>
                  <div v-else>
                    <span class="text-black">
                      Values :
                    </span>
                    <span class="text-primary text-weight-medium">
                      {{ selectedItemSubcategoryAttribute.name }}
                    </span>
                  </div>
                </div>
              </div>
              <q-btn
                v-if="selectedItemSubcategoryAttribute.name"
                color="primary"
                icon="o_add"
                label="Add"
                no-caps
                :disable="selectedItemSubcategoryAttribute.fieldType === 'Input'"
                @click="onAddItemSubcategoryAttributeValue"
              />
            </div>
            <div v-if="selectedItemSubcategoryAttribute.name" class="row items-center q-pa-sm bg-primary text-white text-h6">
              <div class="col-2">
                Sort Order
              </div>
              <div class="col-3">
                Item Subcategory
              </div>
              <div class="col-3">
                Text
              </div>
              <div class="col-3">
                Value
              </div>
              <div class="col-1 text-center">
                Actions
              </div>
            </div>
            <div class="scroll scroll-container">
              <div id="sortable-item-subcategory-attributes-values">
                <div
                  v-if="selectedItemSubcategoryAttribute.name && selectedItemSubcategoryAttribute.fieldType === 'Input'"
                  class="text-center text-grey-6 q-pa-lg"
                >
                  Field type 'Input' cannot have attribute values
                </div>

                <div
                  v-else-if="(!itemSubcategoryAttributesValues || itemSubcategoryAttributesValues.length === 0)&& selectedItemSubcategoryAttribute.name"
                  class="text-center text-grey-6 q-pa-lg"
                >
                  No Data Available
                </div>
                <div
                  v-for="(itemSubcategoryAttributeValue, itemSubcategoryAttributeValueIndex) in itemSubcategoryAttributesValues"
                  v-else
                  :key="itemSubcategoryAttributeValue.id || itemSubcategoryAttributeValue.tempId"
                  :class="[
                    'row items-center q-mb-md q-pa-sm bg-grey-2',
                    itemSubcategoryAttributeValue.showInputForNewItemSubcategoryAttributeValue ? 'not-draggable' : 'cursor-grab'
                  ]"
                  style="border: 1px solid #ccc;"
                >
                  <div class="row items-center full-width">
                    <div class="col-2 row items-center">
                      <q-icon
                        name="o_reorder"
                        size="xs"
                      />
                      <span class="text-grey-8 q-ml-sm">
                        {{ itemSubcategoryAttributeValue.sortOrder }}
                      </span>
                    </div>
                    <div class="col-3 cursor-pointer">
                      <template
                        v-if="itemSubcategoryAttributeValue.showInputForNewItemSubcategoryAttributeValue
                          || (activeEdit.field === 'itemSubCategory' && activeEdit.rowId === itemSubcategoryAttributeValue.id)"
                      >
                        <div class="row items-center q-mr-sm">
                          <q-select
                            v-model="itemSubcategoryAttributeValue.itemSubCategoryId"
                            use-input
                            clearable
                            outlined
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            :options="itemSubcategoryList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            class="col"
                            @filter="getAllItemSubcategoryListForFilter"
                            @update:model-value="val => {
                              saveOrUpdateItemSubcategoryAttributeValue(itemSubcategoryAttributeValue)
                              activeEdit = { rowId: null, field: '' }
                            }"
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
                      </template>

                      <template v-else>
                        <div
                          class="row items-center justify-between q-pa-xs cursor-pointer"
                          @dblclick="
                            activeEdit = {
                              rowId: itemSubcategoryAttributeValue.id,
                              field: 'itemSubCategory'
                            }
                          "
                        >
                          <span style="flex: 1; word-break: break-word; white-space: normal;">
                            {{ itemSubcategoryList.find(o => o.value === itemSubcategoryAttributeValue.itemSubCategoryId)?.text }}
                          </span>
                        </div>
                      </template>

                      <q-tooltip v-if="itemSubcategoryAttributeValue.itemSubCategoryId && !itemSubcategoryAttributeValue.showInputForNewItemSubcategoryAttributeValue && activeEdit.rowId !== itemSubcategoryAttributeValue.id">Double-click to edit</q-tooltip>
                    </div>
                    <div class="col-3 cursor-pointer">
                      <template v-if="itemSubcategoryAttributeValue.showInputForNewItemSubcategoryAttributeValue || activeEdit.field === 'text' && activeEdit.rowId === itemSubcategoryAttributeValue.id">
                        <div class="row items-center q-mr-sm">
                          <q-input
                            v-model="itemSubcategoryAttributeValue.text"
                            outlined
                            stack-label
                            hide-bottom-space
                            dense
                            class="col"
                            placeholder="Enter Item Attribute Text"
                            @keyup.enter="setTextToValueAndSave(itemSubcategoryAttributeValue)"
                            @blur="setTextToValueAndSave(itemSubcategoryAttributeValue)"
                          >
                            <q-tooltip>
                              Press enter key to save Item Attribute Text
                            </q-tooltip>
                          </q-input>
                          <q-btn
                            v-if="itemSubcategoryAttributeValue.text && !itemSubcategoryAttributeValue.showInputForNewItemSubcategoryAttributeValue"
                            icon="o_close"
                            size="xs"
                            color="black"
                            flat
                            round
                            dense
                            class="q-mx-sm"
                            @click="
                              itemSubcategoryAttributeValue.text = itemSubcategoryAttributeValue._oldText;
                              activeEdit = { rowId: null, field: '' }
                            "
                          />
                        </div>
                      </template>

                      <template v-else>
                        <div
                          class="row items-center justify-between q-pa-xs cursor-pointer"
                          @dblclick="
                            itemSubcategoryAttributeValue._oldText = itemSubcategoryAttributeValue.text;
                            activeEdit = { rowId: itemSubcategoryAttributeValue.id, field: 'text' }
                          "
                        >
                          <span style="flex: 1; word-break: break-word; white-space: normal;">{{ itemSubcategoryAttributeValue.text }}</span>
                        </div>
                      </template>

                      <q-tooltip v-if="itemSubcategoryAttributeValue.text && !itemSubcategoryAttributeValue.showInputForNewItemSubcategoryAttributeValue && activeEdit.rowId !== itemSubcategoryAttributeValue.id">Double-click to edit</q-tooltip>
                    </div>
                    <div class="col-3 cursor-pointer">
                      <template
                        v-if="itemSubcategoryAttributeValue.showInputForNewItemSubcategoryAttributeValue ||
                          activeEdit.field === 'value' &&
                          activeEdit.rowId === itemSubcategoryAttributeValue.id"
                      >
                        <div class="row items-center no-wrap">
                          <q-input
                            v-model="itemSubcategoryAttributeValue.value"
                            outlined
                            stack-label
                            hide-bottom-space
                            dense
                            class="full-width"
                            placeholder="Enter Item Attribute Value"
                            @keyup.enter="saveOrUpdateItemSubcategoryAttributeValue(itemSubcategoryAttributeValue);"
                          >
                            <q-tooltip>
                              Press enter key to save Item Attribute Value
                            </q-tooltip>
                          </q-input>
                          <q-btn
                            v-if="itemSubcategoryAttributeValue.value && !itemSubcategoryAttributeValue.showInputForNewItemSubcategoryAttributeValue"
                            icon="o_close"
                            size="xs"
                            color="black"
                            flat
                            round
                            dense
                            class="q-mx-sm"
                            @click="itemSubcategoryAttributeValue.value = itemSubcategoryAttributeValue._oldValue;
                                    activeEdit = { rowId: null, field: '' }"
                          />
                        </div>
                      </template>

                      <template v-else>
                        <div
                          class="cursor-pointer"
                          @dblclick="itemSubcategoryAttributeValue._oldValue =itemSubcategoryAttributeValue.value;
                                     activeEdit = { rowId: itemSubcategoryAttributeValue.id, field: 'value' }"
                        >
                          <span style="flex: 1; word-break: break-word; white-space: normal;">{{ itemSubcategoryAttributeValue.value }}</span>
                        </div>
                      </template>

                      <q-tooltip
                        v-if="
                          itemSubcategoryAttributeValue.value &&
                            !itemSubcategoryAttributeValue.showInputForNewItemSubcategoryAttributeValue &&
                            activeEdit.rowId !== itemSubcategoryAttributeValue.id
                        "
                      >
                        Double-click to edit
                      </q-tooltip>
                    </div>
                    <div class="col-1 flex flex-center actions q-gutter-sm">
                      <q-icon
                        v-if="itemSubcategoryAttributeValue.id"
                        name="o_visibility"
                        class="cursor-pointer"
                        size="xs"
                        @click="onViewItemSubcategoryAttributeValue(itemSubcategoryAttributeValue.id)"
                      />
                      <q-icon
                        name="o_delete"
                        color="negative"
                        size="xs"
                        class="cursor-pointer"
                        @click="onDeleteItemSubCategoryAttributeValue(itemSubcategoryAttributeValue, itemSubcategoryAttributeValueIndex)"
                      >
                        <q-tooltip>Delete</q-tooltip>
                      </q-icon>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onMounted, nextTick } from "vue";
import { notifyError, notifySuccess, zwConfirmDelete, zwConfirm } from "assets/utils";
import { Sortable } from "sortablejs";
import { useQuasar, uid } from "quasar";

import itemSubcategoryAttributeService from "../itemSubCategoryAttributes.service";
import itemCategoryService from "../itemCategory.service";

import viewItemAttribute from "modules/items/components/viewAttribute.vue";
import viewItemAttributeValue from "modules/items/components/viewAttributeValues.vue";

const itemSubcategoryAttributes = ref([]);
const itemSubcategoryAttributesValues = ref([]);
const activeRowId = ref(null);
const selectedItemSubcategoryAttribute = ref({ name: "", fieldType: "", id: null });
const activeEdit = ref({ rowId: null, field: null });
const isLoading = ref(false);
const $q = useQuasar();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// get all item subCategory attributes
// --------------------------------------------------------------------------------------------------------------------------------------------------
function getAllItemSubCategoryAttributeList () {
  isLoading.value = true;
  itemSubcategoryAttributeService.getAllItemSubCategoryAttributeList()
    .then((resp) => {
      itemSubcategoryAttributes.value = resp || [];
      nextTick(makeSortable);
      // showSubcategoryAttributes.value = true;
    })
    .catch((err) => {
      console.error(err);
    })
    .finally(() => {
      isLoading.value = false;
    });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get question list by help desk topic
// --------------------------------------------------------------------------------------------------------------------------------------------------
const getAllItemSubcategoryAttributeValuesByAttributeId = (name, fieldType, id) => {
  selectedItemSubcategoryAttribute.value = { name, fieldType, id };
  isLoading.value = true;
  itemSubcategoryAttributeService.getAllItemSubcategoryAttributeValuesByAttributeId(id)
    .then((resp) => {
      itemSubcategoryAttributesValues.value = resp;
      activeRowId.value = id;
      // console.log(resp);
    })
    .finally(() => {
      isLoading.value = false;
    });
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
      // console.log(responseData);
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

// --------------------------------------------------------------------------------------------------------------------------------------------------
//  Add new item subcategory attribute
// --------------------------------------------------------------------------------------------------------------------------------------------------
function onAddItemSubcategoryAttribute () {
  itemSubcategoryAttributes.value.unshift({
    tempId: uid(),
    id: null,
    name: "",
    fieldType: "Input",
    showInputForNewItemSubcategoryAttribute: true,
    deleted: false
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
//  Add new item subcategory attribute value
// --------------------------------------------------------------------------------------------------------------------------------------------------
function onAddItemSubcategoryAttributeValue () {
  const lastSortOrder = itemSubcategoryAttributesValues.value.length
    ? Math.max(...itemSubcategoryAttributesValues.value.map(x => Number(x.sortOrder) || 0))
    : 0;
  if (!Array.isArray(itemSubcategoryAttributesValues.value)) {
    itemSubcategoryAttributesValues.value = [];
  }
  itemSubcategoryAttributesValues.value.unshift({
    tempId: uid(),
    id: null,
    itemSubCategoryId: itemSubcategoryAttributesValues.value.length
      ? itemSubcategoryAttributesValues.value[0].itemSubCategoryId
      : null,
    text: "",
    value: "",
    sortOrder: lastSortOrder + 1,
    showInputForNewItemSubcategoryAttributeValue: true
  });
}

const setTextToValueAndSave = (attributeValueRow) => {
  if (!attributeValueRow.isConfirmed) {
    attributeValueRow.value = attributeValueRow.text;
    attributeValueRow.isConfirmed = true;
    return;
  }
  saveOrUpdateItemSubcategoryAttributeValue(attributeValueRow);
  attributeValueRow.isConfirmed = false;
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Delete item SubCategory attribute
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onDeleteItemSubCategoryAttribute = async (itemSubcategoryAttribute, itemSubcategoryAttributeIndex) => {
  activeRowId.value = itemSubcategoryAttribute.id;
  try {
    if (!itemSubcategoryAttribute.id) {
      itemSubcategoryAttributes.value.splice(itemSubcategoryAttributeIndex, 1);
      return;
    }
    const resp = await itemSubcategoryAttributeService.checkItemSubcategoryAttributeCanBeDeleted(itemSubcategoryAttribute.id);
    const canDelete = resp?.canDelete;
    if (canDelete) {
      zwConfirmDelete({ data: `${itemSubcategoryAttribute.name}` }, () => {
        itemSubcategoryAttributeService.deleteItemSubCategoryAttribute(itemSubcategoryAttribute.id).then(() => {
          notifySuccess({ message: "Item Attribute deleted successfully." });
          itemSubcategoryAttributes.value.splice(itemSubcategoryAttributeIndex, 1);
          if (selectedItemSubcategoryAttribute.value.id === itemSubcategoryAttribute.id) {
            selectedItemSubcategoryAttribute.value = { name: "", id: null };
          }
        });
      });
    } else {
      zwConfirm({
        title: "Active Item Attribute Value Found",
        message: "This Item  Attribute has active Item Attribute Value. You cannot delete it.",
        data: `${itemSubcategoryAttribute.name}`,
        okLabel: "OK",
        cancel: false
      }, () => {
      });
    }
  } catch (error) {
    console.error("Error checking item attribute:", error);
  } finally {
    activeRowId.value = null;
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Delete item subcategory attribute value
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onDeleteItemSubCategoryAttributeValue = (itemSubcategoryAttributeValue, itemSubcategoryAttributeValueIndex) => {
  if (!itemSubcategoryAttributeValue.id) {
    itemSubcategoryAttributesValues.value.splice(itemSubcategoryAttributeValueIndex, 1);
    return;
  }
  zwConfirmDelete({ data: `${itemSubcategoryAttributeValue.text}` }, () => {
    itemSubcategoryAttributeService.deleteItemSubCategoryAttributeValue(itemSubcategoryAttributeValue.id).then(() => {
      notifySuccess({ message: "Item attribute value is deleted successfully." });
      itemSubcategoryAttributesValues.value.splice(itemSubcategoryAttributeValueIndex, 1);
      getAllItemSubCategoryAttributeList();
    });
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Save or update item subcategory attribute
// --------------------------------------------------------------------------------------------------------------------------------------------------
const saveOrUpdateItemSubcategoryAttribute = (itemSubcategoryAttribute) => {
  if (!itemSubcategoryAttribute.name || !itemSubcategoryAttribute.name.trim()) {
    notifyError({ message: "Please enter a value for the item attribute." });
    return;
  }
  const isUpdate = !!itemSubcategoryAttribute.id; // true if updating

  const payload = {
    id: itemSubcategoryAttribute.id || null,
    name: itemSubcategoryAttribute.name,
    fieldType: itemSubcategoryAttribute.fieldType
  };
  itemSubcategoryAttributeService.saveItemSubCategoryAttribute(payload.id, payload).then((resp) => {
    if (!isUpdate) {
      itemSubcategoryAttribute.id = resp.id;
    }
    itemSubcategoryAttribute.showInputForNewItemSubcategoryAttribute = false; // hide input
    activeEdit.value = { rowId: null }; // stop editing

    notifySuccess({
      message: isUpdate
        ? "Item Attribute updated successfully."
        : "Item Attribute saved successfully."
    });
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Save or update item subcategory attribute value
// --------------------------------------------------------------------------------------------------------------------------------------------------
const saveOrUpdateItemSubcategoryAttributeValue = (itemSubcategoryAttributeValue) => {
  const isUpdate = !!itemSubcategoryAttributeValue.id;
  if (
    !itemSubcategoryAttributeValue.text?.trim() ||
    !itemSubcategoryAttributeValue.value?.trim()
  ) {
    notifyError({
      message: !itemSubcategoryAttributeValue.text?.trim()
        ? "Please enter a value for the item attribute text."
        : "Please enter a value for the item attribute value."
    });
    return false;
  }

  const payload = {
    id: itemSubcategoryAttributeValue.id,
    itemSubCategoryId: itemSubcategoryAttributeValue.itemSubCategoryId,
    text: itemSubcategoryAttributeValue.text,
    value: itemSubcategoryAttributeValue.value,
    sortOrder: itemSubcategoryAttributeValue.sortOrder,
    ItemSubCategoryAttributeId: selectedItemSubcategoryAttribute.value.id
  };
  itemSubcategoryAttributeService.saveItemSubCategoryAttributeValue(payload.id, payload).then((resp) => {
    if (!isUpdate) {
      itemSubcategoryAttributeValue.id = resp.id;
    }
    itemSubcategoryAttributeValue.showInputForNewItemSubcategoryAttributeValue = false; // hide input
    activeEdit.value = { rowId: null }; // stop editing

    notifySuccess({
      message: isUpdate
        ? "Item Attribute Value updated successfully."
        : "Item Attribute value saved successfully."
    });
    getAllItemSubCategoryAttributeList();
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Initialize Sortable.js for dropdown types and dropdown values
// --------------------------------------------------------------------------------------------------------------------------------------------------
const sortableInstances = ref([]);
const makeSortable = () => {
  nextTick(() => {
    // Sortable for item subcategory attribute values
    const itemSubcategoryAttributeValueContainer = document.getElementById("sortable-item-subcategory-attributes-values");
    if (itemSubcategoryAttributeValueContainer) {
      sortableInstances.value.push(
        new Sortable(itemSubcategoryAttributeValueContainer, {
          animation: 150,
          ghostClass: "sortable-ghost",
          draggable: ".cursor-grab",
          onEnd (evt) {
            if (evt.oldIndex === evt.newIndex) return;
            const movedItem = itemSubcategoryAttributesValues.value.splice(evt.oldIndex, 1)[0];
            itemSubcategoryAttributesValues.value.splice(evt.newIndex, 0, movedItem);
            // Update sort order
            updateItemSubcategoryAttributeValueSortOrder();
          }
        })
      );
    }
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update sortOrder for all item subcategory attribute values
// --------------------------------------------------------------------------------------------------------------------------------------------------
const updateItemSubcategoryAttributeValueSortOrder = async () => {
  if (!itemSubcategoryAttributesValues.value?.length) return;
  const updatedItems = [];
  // Update sortOrder & collect only changed rows
  itemSubcategoryAttributesValues.value.forEach((item, index) => {
    const newSortOrder = index + 1;

    if (item.sortOrder !== newSortOrder) {
      item.sortOrder = newSortOrder;
      updatedItems.push(item);
    }
  });
  // Save only modified rows
  for (const item of updatedItems) {
    saveOrUpdateItemSubcategoryAttributeValue(item);
  }
};

// View item subcategory attribute  popup
const onViewItemSubcategoryAttribute = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewItemAttribute,
    componentProps: { id }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// View item subcategory attribute value  popup
const onViewItemSubcategoryAttributeValue = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewItemAttributeValue,
    componentProps: { id }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};
// --------------------------------------------------------------------------------------------------------------------------------------------------
// On page rendering
// --------------------------------------------------------------------------------------------------------------------------------------------------
onMounted(() => {
  getAllItemSubCategoryAttributeList();
  getAllItemSubcategoryList();
});
</script>
<style>
.cursor-grab{
  cursor: grab;
}
.sortable-ghost {
  opacity: 0.5;
  background-color: rgb(216, 216, 216);
}
.actions i{
    font-size: 18px;
    border: 1px solid var(--q-secondary);
    padding: 3px;
    border-radius: 5px;
  }
  .actions i:hover{
    border-color: var(--q-primary);
    color: white;
    background-color: var(--q-primary);
  }
  .actions i.text-negative:hover {
    border-color: var(--q-negative);
    color: white !important;
    background-color: var(--q-negative);
  }
  .scroll-container {
  max-height: 75vh;
}
</style>
