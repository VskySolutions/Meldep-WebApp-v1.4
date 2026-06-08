<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center justify-between">
          <div class="col-12 col-md-4">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Item" />
              <q-breadcrumbs-el label="Item Categories & Subcategories" />
            </q-breadcrumbs>
          </div>
          <q-btn
            class="text-primary"
            outline
            @click="$router.push('/item/add-attributes-values')"
          >
            <span>Next</span>
            <q-icon name="o_chevron_right" class="q-ml-xs" />
          </q-btn>
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
              <div class="text-h3">
                Category
              </div>
              <div class="row q-gutter-sm">
                <q-btn
                  color="primary"
                  icon="o_add"
                  label="Add"
                  no-caps
                  @click="onAddItemCategory"
                />
                <q-btn
                  color="primary"
                  icon="o_add"
                  label="Add Bulk"
                  no-caps
                  @click="onAddBulkItemSubcategories"
                />
              </div>
            </div>
            <div class="row items-center q-pa-sm bg-primary text-white text-h6">
              <div class="col-6">Category Name</div>
              <div class="col-4">Prefix</div>
              <div class="col-2 text-center">Actions</div>
            </div>
            <div
              v-if="!itemCategories || itemCategories.length === 0"
              class="text-center text-grey-6 q-pa-lg"
            >
              No Data Available
            </div>
            <div class="scroll scroll-container">
              <div
                v-for="(itemCategory, itemCategoryIndex) in itemCategories"
                :key="itemCategory.id || itemCategory.tempId"
                :class="['row items-center q-mb-md q-pa-sm', activeRowId && activeRowId === itemCategory.id ? 'bg-grey-2' : '']"
                style="border: 1px solid #ccc;"
              >
                <div class="row items-center full-width">
                  <div class="col-6 cursor-pointer">
                    <template v-if="itemCategory.showInputForNewItemCategory || activeEdit.field === 'name' && activeEdit.rowId === itemCategory.id">
                      <div class="row items-center no-wrap q-mr-sm">
                        <q-input
                          v-model="itemCategory.name"
                          outlined
                          stack-label
                          hide-bottom-space
                          dense
                          class="full-width"
                          placeholder="Enter Item Category"
                          @keyup.enter="saveOrUpdateItemCategory(itemCategory);"
                        >
                          <q-tooltip>
                            Press enter key to save Item Category
                          </q-tooltip>
                        </q-input>
                        <q-btn
                          v-if="itemCategory.name && !itemCategory.showInputForNewItemCategory"
                          icon="o_close"
                          size="xs"
                          color="black"
                          flat
                          round
                          dense
                          class="q-ml-sm"
                          @click="itemCategory.name = itemCategory._oldName;
                                  activeEdit = { rowId: null, field: '' }"
                        />
                      </div>
                    </template>

                    <template v-else>
                      <div
                        class="cursor-pointer"
                        @click="getAllItemSubcategoryList(itemCategory.name, itemCategory.id)"
                        @dblclick="itemCategory._oldName = itemCategory.name;
                                   activeEdit = { rowId: itemCategory.id, field: 'name' }"
                      >
                        <span style="flex: 1; word-break: break-word; white-space: normal;">{{ itemCategory.name }}</span>
                      </div>
                    </template>

                    <q-tooltip v-if="itemCategory.name && !itemCategory.showInputForNewItemCategory && activeEdit.rowId !== itemCategory.id">Double-click to edit</q-tooltip>
                  </div>
                  <div class="col-4 cursor-pointer">
                    <template v-if="itemCategory.showInputForNewItemCategory || activeEdit.field === 'prefix' && activeEdit.rowId === itemCategory.id">
                      <div class="row items-center no-wrap">
                        <q-input
                          v-model="itemCategory.prefix"
                          outlined
                          stack-label
                          hide-bottom-space
                          dense
                          class="full-width"
                          placeholder="Enter Item Prefix"
                          @keyup.enter="saveOrUpdateItemCategory(itemCategory);"
                        >
                          <q-tooltip>
                            Press enter key to save Item Category Prefix
                          </q-tooltip>
                        </q-input>
                        <q-btn
                          v-if="itemCategory.prefix && !itemCategory.showInputForNewItemCategory"
                          icon="o_close"
                          size="xs"
                          color="black"
                          flat
                          round
                          dense
                          class="q-ml-sm"
                          @click="itemCategory.prefix = itemCategory._oldPrefix; activeEdit = { rowId: null, field: '' }"
                        />
                      </div>
                    </template>

                    <template v-else>
                      <div
                        class="cursor-pointer"
                        @dblclick="itemCategory._oldPrefix = itemCategory.prefix; activeEdit = { rowId: itemCategory.id, field: 'prefix' }"
                      >
                        <span>{{ itemCategory.prefix }}</span>
                      </div>
                    </template>

                    <q-tooltip v-if="itemCategory.prefix && !itemCategory.showInputForNewItemCategory && activeEdit.rowId !== itemCategory.id">Double-click to edit</q-tooltip>
                  </div>
                  <div class="col-2 flex flex-center actions">
                    <q-icon
                      v-if="itemCategory.id"
                      name="o_visibility"
                      class="cursor-pointer q-mr-sm"
                      size="xs"
                      @click="onViewItemCategory(itemCategory.id)"
                    >
                      <q-tooltip>View</q-tooltip>
                    </q-icon>
                    <q-badge
                      v-if="itemCategory.totalItemSubcategoryCount > 0"
                      color="primary"
                      text-color="white"
                      square
                      class="rounded-full flex justify-center q-mr-sm"
                      style="width: 25px; height: 25px;"
                    >
                      {{ itemCategory.totalItemSubcategoryCount }}
                      <q-tooltip>Number of Subcategories</q-tooltip>
                    </q-badge>
                    <q-icon
                      name="o_delete"
                      size="xs"
                      :color="itemCategory.totalItemSubcategoryCount > 0 ? 'red-3' : 'negative'"
                      class="cursor-pointer"
                      @click="onDeleteItemCategory(itemCategory, itemCategoryIndex)"
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
                v-if="selectedItemCategory?.name"
                class="row items-center justify-between"
              >
                <div class="text-h3">
                  <div v-if="!itemSubcategories || itemSubcategories.length === 0 && selectedItemCategory.name">
                    Subcategory
                  </div>
                  <div v-else>
                    <span class="text-black">
                      Subcategory :
                    </span>
                    <span class="text-primary text-weight-medium">
                      {{ selectedItemCategory.name }}
                    </span>
                  </div>
                </div>
              </div>
              <q-btn
                v-if="selectedItemCategory.name"
                color="primary"
                icon="o_add"
                label="Add"
                no-caps
                @click="onAddItemSubcategory"
              />
            </div>
            <div v-if="selectedItemCategory.name" class="row items-center q-pa-sm bg-primary text-white text-h6">
              <div class="col-2">
                Sort Order
              </div>
              <div class="col-4">
                Subcategory Name
              </div>
              <div class="col-4">
                Prefix
              </div>
              <div class="col-2 text-center">
                Actions
              </div>
            </div>
            <div class="scroll scroll-container">
              <div id="sortable-item-subcategory">
                <div
                  v-if="!itemSubcategories || itemSubcategories.length === 0 && selectedItemCategory.name"
                  class="text-center text-grey-6 q-pa-lg"
                >
                  No Data Available
                </div>
                <div
                  v-for="(itemSubcategory, itemSubcategoryIndex) in itemSubcategories"
                  :key="itemSubcategory.id || itemSubcategory.tempId"
                  :class="[
                    'row items-center q-mb-md q-pa-sm bg-grey-2',
                    itemSubcategory.showInputForNewItemSubcategory ? 'not-draggable' : 'cursor-grab'
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
                        {{ itemSubcategory.sortOrder }}
                      </span>
                    </div>
                    <div class="col-4">
                      <template
                        v-if="itemSubcategory.showInputForNewItemSubcategory || activeEdit.field === 'name' && activeEdit.rowId === itemSubcategory.id"
                      >
                        <div class="row items-center q-mr-sm">
                          <q-input
                            v-model="itemSubcategory.name"
                            outlined
                            stack-label
                            hide-bottom-space
                            dense
                            class="col"
                            placeholder="Enter Item Subcategory"
                            @keyup.enter="generatePrefixForSubcategoryName(itemSubcategory);"
                          >
                            <q-tooltip>
                              Press enter key to generate prefix and save.
                            </q-tooltip>
                          </q-input>
                          <q-btn
                            v-if="itemSubcategory.name && !itemSubcategory.showInputForNewItemSubcategory"
                            icon="o_close"
                            size="xs"
                            color="black"
                            flat
                            round
                            dense
                            class="q-mx-sm"
                            @click="itemSubcategory.name = itemSubcategory._oldName; activeEdit = { rowId: null, field: '' }"
                          />
                        </div>
                      </template>

                      <template v-else>
                        <div
                          class="row items-center justify-between q-pa-xs cursor-pointer"
                          @dblclick="itemSubcategory._oldName = itemSubcategory.name;
                                     activeEdit = { rowId: itemSubcategory.id, field: 'name' }"
                        >
                          <span style="flex: 1; word-break: break-word; white-space: normal;">{{ itemSubcategory.name }}</span>
                        </div>
                      </template>
                      <q-tooltip v-if="itemSubcategory.name && !itemSubcategory.showInputForNewItemSubcategory && activeEdit.rowId !== itemSubcategory.id">Double-click to edit</q-tooltip>
                    </div>
                    <div class="col-4 cursor-pointer">
                      <template v-if="itemSubcategory.showInputForNewItemSubcategory || activeEdit.field === 'prefix' && activeEdit.rowId === itemSubcategory.id">
                        <div class="row items-center no-wrap">
                          <q-input
                            v-model="itemSubcategory.prefix"
                            outlined
                            stack-label
                            hide-bottom-space
                            dense
                            class="full-width"
                            placeholder="Enter Item Subcategory Prefix"
                            @keyup.enter="saveOrUpdateItemSubcategory(itemSubcategory);"
                          >
                            <q-tooltip>
                              Press enter key to save Item Subcategory Prefix
                            </q-tooltip>
                          </q-input>
                          <q-btn
                            v-if="itemSubcategory.prefix && !itemSubcategory.showInputForNewItemSubcategory"
                            icon="o_close"
                            size="xs"
                            color="black"
                            flat
                            round
                            dense
                            class="q-mx-sm"
                            @click="itemSubcategory.prefix = itemSubcategory._oldPrefix; activeEdit = { rowId: null, field: '' }"
                          />
                        </div>
                      </template>

                      <template v-else>
                        <div
                          class="cursor-pointer"
                          @dblclick="itemSubcategory._oldPrefix = itemSubcategory.prefix;
                                     activeEdit = { rowId: itemSubcategory.id, field: 'prefix' }"
                        >
                          <span>{{ itemSubcategory.prefix }}</span>
                        </div>
                      </template>

                      <q-tooltip v-if="itemSubcategory.prefix && !itemSubcategory.showInputForNewItemCategory && activeEdit.rowId !== itemSubcategory.id">Double-click to edit</q-tooltip>
                    </div>
                    <div class="col-2 flex flex-center actions q-gutter-sm">
                      <q-icon
                        v-if="itemSubcategory.id"
                        name="o_visibility"
                        class="cursor-pointer"
                        size="xs"
                        @click="onViewItemSubcategory(itemSubcategory.id)"
                      >
                        <q-tooltip>View</q-tooltip>
                      </q-icon>
                      <q-icon
                        name="o_delete"
                        color="negative"
                        size="xs"
                        class="cursor-pointer"
                        @click="onDeleteItemSubcategory(itemSubcategory, itemSubcategoryIndex)"
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
      </div></q-card>
  </q-page>
</template>
<script setup>
import { ref, onMounted, nextTick } from "vue";
import { notifyError, notifySuccess, zwConfirmDelete, zwConfirm } from "assets/utils";
import { Sortable } from "sortablejs";
import { useQuasar, uid } from "quasar";

import viewItemCategory from "modules/items/components/viewCategory.vue";
import viewItemSubcategory from "modules/items/components/viewSubCategory.vue";
import itemCategoryService from "../itemCategory.service";
import addBulkItemSubcategories from "modules/items/components/addEditCategory.vue";

const itemCategories = ref([]);
const itemSubcategories = ref([]);
const activeRowId = ref(null);
const selectedItemCategory = ref({ name: "", id: null });
const activeEdit = ref({ rowId: null, field: null });
const isLoading = ref(false);
const $q = useQuasar();
// --------------------------------------------------------------------------------------------------------------------------------------------------
// get all item categories
// --------------------------------------------------------------------------------------------------------------------------------------------------
function getAllItemCategoryList () {
  isLoading.value = true;
  itemCategoryService.getAllItemCategoryList()
    .then((resp) => {
      itemCategories.value = resp || [];
      nextTick(makeSortable);
    })
    .catch((err) => {
      console.error(err);
    })
    .finally(() => {
      isLoading.value = false;
    });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get item subcategories
// --------------------------------------------------------------------------------------------------------------------------------------------------
const getAllItemSubcategoryList = (name, id) => {
  selectedItemCategory.value = { name, id };
  isLoading.value = true;
  itemCategoryService.getAllItemSubcategoryList(id)
    .then((resp) => {
      itemSubcategories.value = resp;
      activeRowId.value = id;
    })
    .finally(() => {
      isLoading.value = false;
    });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
//  Add new item category
// --------------------------------------------------------------------------------------------------------------------------------------------------
function onAddItemCategory () {
  itemCategories.value.unshift({
    tempId: uid(),
    id: null,
    name: "",
    prefix: "",
    showInputForNewItemCategory: true,
    deleted: false
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
//  Add new item subcategory
// --------------------------------------------------------------------------------------------------------------------------------------------------
function onAddItemSubcategory () {
  const lastSortOrder = itemSubcategories.value.length
    ? Math.max(...itemSubcategories.value.map(x => Number(x.sortOrder) || 0))
    : 0;
  if (!Array.isArray(itemSubcategories.value)) {
    itemSubcategories.value = [];
  }
  itemSubcategories.value.unshift({
    tempId: uid(),
    id: null,
    name: "",
    prefix: "",
    sortOrder: lastSortOrder + 1,
    showInputForNewItemSubcategory: true
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Delete item category
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onDeleteItemCategory = async (itemCategory, itemCategoryIndex) => {
  activeRowId.value = itemCategory.id;
  try {
    if (!itemCategory.id) {
      itemCategories.value.splice(itemCategoryIndex, 1);
      return;
    }
    const resp = await itemCategoryService.checkItemCategoryCanBeDeleted(itemCategory.id);
    const canDelete = resp?.canDelete;
    if (canDelete) {
      zwConfirmDelete({ data: `${itemCategory.name}` }, () => {
        itemCategoryService.deleteItemCategory(itemCategory.id).then(() => {
          notifySuccess({ message: "Item Category deleted successfully." });
          itemCategories.value.splice(itemCategoryIndex, 1);
          if (selectedItemCategory.value.id === itemCategory.id) {
            selectedItemCategory.value = { name: "", id: null };
          }
        });
      });
    } else {
      zwConfirm({
        title: "Active Item Subcategory Found",
        message: "This Item Category has active Item Subcategory. You cannot delete it.",
        data: `${itemCategory.name}`,
        okLabel: "OK",
        cancel: false
      }, () => {
      });
    }
  } catch (error) {
    console.error("Error checking item subcategory:", error);
  } finally {
    activeRowId.value = null;
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Delete item subcategory
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onDeleteItemSubcategory = (itemSubcategory, itemSubcategoryIndex) => {
  if (!itemSubcategory.id) {
    itemSubcategories.value.splice(itemSubcategoryIndex, 1);
    return;
  }
  zwConfirmDelete({ data: `${itemSubcategory.name}` }, () => {
    itemCategoryService.deleteItemSubcategory(itemSubcategory.id).then(() => {
      notifySuccess({ message: "Item Subcategory is deleted successfully." });
      itemSubcategories.value.splice(itemSubcategoryIndex, 1);
      getAllItemCategoryList();
    });
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Save or update item sub category
// --------------------------------------------------------------------------------------------------------------------------------------------------
const saveOrUpdateItemCategory = (itemCategory) => {
  const isUpdate = !!itemCategory.id;

  if (!itemCategory.name?.trim() || !itemCategory.prefix?.trim()) {
    notifyError({
      message: !itemCategory.name?.trim()
        ? "Please enter a value for the Item Category."
        : "Please enter a value for the Prefix."
    });
    return false;
  }
  const payload = {
    id: itemCategory.id || null,
    name: itemCategory.name,
    prefix: itemCategory.prefix,
    groupName: "Inventory"
  };
  itemCategoryService.saveItemCategory(payload.id, payload).then((resp) => {
    if (!isUpdate) {
      itemCategory.id = resp.id;
    }
    itemCategory.showInputForNewItemCategory = false; // hide input
    activeEdit.value = { rowId: null }; // stop editing

    notifySuccess({
      message: isUpdate
        ? "Item Category updated successfully."
        : "Item Category saved successfully."
    });
  });
};

const generatePrefixForSubcategoryName = async (itemSubcategory) => {
  if (!itemSubcategory.name?.trim()) {
    notifyError({ message: "Please enter a value for the Item Subcategory." });
    return;
  }

  try {
    const generatedPrefix = await itemCategoryService.generatePrefixForSubcategoryName(
      itemSubcategory.name,
      itemSubcategory.id // no existing prefix
    );
    itemSubcategory.prefix = generatedPrefix;
    saveOrUpdateItemSubcategory(itemSubcategory);
  } catch (error) {
    console.error("Prefix generation failed:", error);
    notifyError({ message: "Failed to generate prefix." });
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Save or update item subcategory
// --------------------------------------------------------------------------------------------------------------------------------------------------
const saveOrUpdateItemSubcategory = async (itemSubcategory) => {
  const isUpdate = !!itemSubcategory.id;
  if (!itemSubcategory.name?.trim() || !itemSubcategory.prefix?.trim()) {
    notifyError({
      message: !itemSubcategory.name?.trim()
        ? "Please enter a value for the Item Subcategory Name."
        : "Please enter a value for the Prefix."
    });
    return false;
  }
  const payload = {
    id: itemSubcategory.id || null,
    prefix: itemSubcategory.prefix,
    sortOrder: itemSubcategory.sortOrder,
    itemCategoryId: selectedItemCategory.value.id,
    name: itemSubcategory.name
  };
  itemCategoryService.saveItemSubcategory(payload.id, payload).then((resp) => {
    if (!isUpdate) {
      itemSubcategory.id = resp.id;
    }
    itemSubcategory.showInputForNewItemSubcategory = false; // hide input
    activeEdit.value = { rowId: null }; // stop editing

    notifySuccess({
      message: isUpdate
        ? "Item Subcategory updated successfully."
        : "Item Subcategory saved successfully."
    });
    getAllItemCategoryList();
  });
};

// ----------------------------
// Add Bulk Item Categories
// ----------------------------
const onAddBulkItemSubcategories = () => {
  $q.dialog({
    component: addBulkItemSubcategories,
    componentProps: {}
  }).onOk(() => {
    getAllItemCategoryList();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Initialize Sortable.js for dropdown types and dropdown values
// --------------------------------------------------------------------------------------------------------------------------------------------------
const sortableInstances = ref([]);
const makeSortable = () => {
  nextTick(() => {
    sortableInstances.value.forEach(instance => instance.destroy());
    sortableInstances.value = [];
    // Sortable for item subcategory
    const itemSubcategoryContainer = document.getElementById("sortable-item-subcategory");
    if (itemSubcategoryContainer) {
      sortableInstances.value.push(
        new Sortable(itemSubcategoryContainer, {
          animation: 150,
          ghostClass: "sortable-ghost",
          draggable: ".cursor-grab",
          onEnd (evt) {
            if (evt.oldIndex === evt.newIndex) return;
            const moved = itemSubcategories.value.splice(evt.oldIndex, 1)[0];
            itemSubcategories.value.splice(evt.newIndex, 0, moved);
            updateItemSubcategorySortOrder();
          }
        })
      );
    }
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update sortOrder for all item subcategory
// --------------------------------------------------------------------------------------------------------------------------------------------------
const updateItemSubcategorySortOrder = async () => {
  if (!itemSubcategories.value?.length) return;
  const updatedItems = [];
  // Update sortOrder & collect only changed rows
  itemSubcategories.value.forEach((itemSubcategory, index) => {
    const newSortOrder = index + 1;
    if (itemSubcategory.sortOrder !== newSortOrder) {
      itemSubcategory.sortOrder = newSortOrder;
      updatedItems.push(itemSubcategory);
    }
  });
  // Save only modified rows
  for (const item of updatedItems) {
    await saveOrUpdateItemSubcategory(item);
  }
};

// View item category popup
const onViewItemCategory = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewItemCategory,
    componentProps: { id }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

// View item subcategory  popup
const onViewItemSubcategory = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewItemSubcategory,
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
  getAllItemCategoryList();
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
