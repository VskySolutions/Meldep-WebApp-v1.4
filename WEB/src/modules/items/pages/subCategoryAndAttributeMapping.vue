<template>
  <q-page padding class="PersonMain">
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center justify-between">
          <div class="col">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Item" />
              <q-breadcrumbs-el label="Item Subcategory Attributes & Values" class="cursor-pointer" @click="$router.push('/item/add-attributes-values')" />
              <q-breadcrumbs-el label="Subcategory  Attributes Mapping" />
            </q-breadcrumbs>
          </div>
          <div class="col-auto q-gutter-sm">
            <q-btn
              icon="o_chevron_left"
              class="text-primary"
              outline
              label="Previous"
              @click="$router.push('/item/add-attributes-values')"
            />
            <q-btn
              class="text-primary"
              outline
              @click="$router.push('/sites-items')"
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
          <div class="col-12 col-lg-9">
            <fieldset class="q-mt-md">
              <div class="row q-col-gutter-md">
                <q-inner-loading :showing="isLoading" color="primary">
                  <div class="row items-center justify-center">
                    <q-spinner-ios size="40px" />
                  </div>
                </q-inner-loading>
                <div class="col-12 col-sm-6 col-lg-4 q-mt-md">
                  <div class="row items-center justify-between q-mb-md">
                    <div class="text-h3 col-12 col-sm-auto">
                      <span>Item Category</span>
                    </div>
                  </div>
                  <div
                    v-if="!itemCategories || itemCategories.length === 0"
                    class="text-center text-grey-6 q-pa-lg"
                  >
                    No Data Available
                  </div>
                  <div class="scroll scroll-container">
                    <div
                      v-for="(itemCategory) in itemCategories"
                      :key="itemCategory.id"
                      :class="['row items-center q-mb-md q-pa-sm', selectedItemCategory.id === itemCategory.id ? 'highlight' : '']"
                      style="border: 1px solid #ccc;"
                    >
                      <div class="row items-center no-wrap full-width q-col-gutter-x-sm">
                        <div class="col-6 cursor-pointer">
                          <div
                            class="row items-center justify-between q-pa-xs cursor-pointer"
                            @click="getAllItemSubcategoryList(itemCategory.name, itemCategory.id)"
                          >
                            <span>{{ itemCategory.name }}</span>
                          </div>
                        </div>
                        <div class="col-5 cursor-pointer">
                          <div
                            class="q-pa-xs cursor-pointer"
                            @dblclick="itemCategory._oldPrefix = itemCategory.prefix; activeEdit = { rowId: itemCategory.id, field: 'prefix' }"
                          >
                            <span>{{ itemCategory.prefix }}</span>
                          </div>
                        </div>
                        <div class="col-1 cursor-pointer">
                          <q-badge
                            v-if="itemCategory.totalSitesItemSubCategoryAttributesMappingCount > 0"
                            color="primary"
                            text-color="white"
                            square
                            class="flex justify-center"
                            style="width: 25px; height: 25px;"
                          >
                            {{ itemCategory.totalSitesItemSubCategoryAttributesMappingCount }}
                            <q-tooltip>Number of Assigned Attributes</q-tooltip>
                          </q-badge>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="col-12 col-sm-6 col-lg-4 q-mt-md">
                  <div class="row items-center justify-between q-mb-md">
                    <div class="text-h3">
                      <div v-if="!itemSubcategories || itemSubcategories.length === 0 && selectedItemCategory.name">
                        Item Subcategories
                      </div>
                      <div v-else>
                        <span class="text-primary" style="flex: 1; word-break: break-word; white-space: normal;">
                          {{ selectedItemCategory.name }}
                        </span>
                      </div>
                    </div>
                  </div>
                  <div
                    v-if="!itemSubcategories || itemSubcategories.length === 0 && selectedItemCategory.name"
                    class="text-center text-grey-6 q-pa-lg"
                  >
                    No Data Available
                  </div>
                  <div class="scroll scroll-container">
                    <div
                      v-for="(itemSubcategory) in itemSubcategories"
                      :key="itemSubcategory.id"
                      :class="[
                        'row items-center q-mb-md q-pa-sm q-pl-none',
                        selectedItemSubcategory.id === itemSubcategory.id ? 'highlight' : ''
                      ]"
                      style="border: 1px solid #ccc;"
                    >
                      <div class="row items-center no-wrap full-width q-col-gutter-x-md">
                        <div class="col-1 row items-center q-gutter-sm">
                          <span class="text-grey-8">
                            {{ itemSubcategory.sortOrder }}
                          </span>
                        </div>
                        <div class="col-5">
                          <div
                            class="row items-center justify-between q-pa-xs cursor-pointer"
                            @click="getAllItemSubCategoryAttributeList(itemSubcategory.id); getAllSitesItemSubCategoryAttributesListByItemSubCategoryId(itemSubcategory.name, itemSubcategory.id)"
                          >
                            <span class="q-ml-xs">{{ itemSubcategory.name }}</span>
                          </div>
                        </div>
                        <div class="col-5 cursor-pointer">
                          <div
                            class="q-pa-xs cursor-pointer"
                          >
                            <span>{{ itemSubcategory.prefix }}</span>
                          </div>
                        </div>
                        <div class="col-1 cursor-pointer">
                          <q-badge
                            v-if="itemSubcategory.totalSitesItemSubCategoryAttributesMappingCount > 0"
                            color="primary"
                            text-color="white"
                            square
                            class="justify-center q-mr-sm"
                            style="width: 25px; height: 25px;"
                          >
                            {{ itemSubcategory.totalSitesItemSubCategoryAttributesMappingCount }}
                            <q-tooltip>Number of Assigned Attributes</q-tooltip>
                          </q-badge>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div
                  v-if="
                    selectedItemSubcategory.id &&
                      itemSubcategories.some(s => s.id === selectedItemSubcategory.id)
                  " class="col-12 col-sm-6 col-lg-4 q-mt-sm"
                >
                  <div class="row items-center justify-between q-mb-sm">
                    <div class="text-h3">
                      <span>
                        Assigned Attributes
                      </span>
                      <span
                        v-if="sitesItemSubCategoryAttributes.length > 0"
                        class="text-primary"
                      >
                        : {{ selectedItemSubcategory.name }}
                      </span>
                    </div>
                    <q-btn
                      v-if="sitesItemSubCategoryAttributes.length > 0"
                      square
                      outline
                      color="negative"
                      unelevated
                      :disable="selectedMappedAttributeIds.length <= 1"
                      class="bg-transparent shadow-2 q-pa-sm"
                      @click="deleteSelectedAttributes()"
                    >
                      <q-icon
                        name="o_delete"
                        class="material-icons-outlined text-negative"
                      />
                      <q-badge
                        v-if="selectedMappedAttributeIds.length > 1"
                        floating
                        color="grey"
                        text-color="white"
                        square
                      >
                        {{ selectedMappedAttributeIds.length }}
                      </q-badge>

                      <q-tooltip anchor="bottom middle" self="top middle">
                        Delete Attributes
                      </q-tooltip>
                    </q-btn>
                  </div>
                  <div id="sortable-item-subcategory-attributes-values">
                    <div
                      v-if="!sitesItemSubCategoryAttributes || sitesItemSubCategoryAttributes.length === 0 && selectedItemSubcategory.name"
                      class="text-center text-grey-6 q-pa-lg"
                    >
                      No Attribute Selected
                    </div>
                    <div class="scroll scroll-container">
                      <div
                        v-for="(sitesItemSubCategoryAttribute) in sitesItemSubCategoryAttributes"
                        :key="sitesItemSubCategoryAttribute.id"
                        class="row items-center q-mb-md q-pa-xs"
                        style="border: 1px solid #ccc;"
                      >
                        <div class="row items-center full-width q-col-gutter-x-sm">
                          <div class="col-12 col-md-5">
                            <div
                              class="q-pa-xs"
                            >
                              <q-checkbox
                                v-model="sitesItemSubCategoryAttribute.isSelected"
                                size="xs"
                                class="q-pa-none cursor-pointer"
                              />
                              <span>{{ sitesItemSubCategoryAttribute.itemSubCategoryAttributes.name }}</span>
                            </div>
                          </div>
                          <div class="col-12 col-md-5">
                            <span>
                              {{ sitesItemSubCategoryAttribute.itemSubCategoryAttributes.fieldType }}
                            </span>
                          </div>
                          <q-icon
                            v-ripple
                            name="o_delete"
                            color="negative"
                            size="sm"
                            class="cursor-pointer"
                            @click="deleteSelectedAttributes(sitesItemSubCategoryAttribute.id)"
                          >
                            <q-tooltip>
                              Delete
                            </q-tooltip>
                          </q-icon>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
          <div
            v-if="
              selectedItemSubcategory.id &&
                itemSubcategories.some(s => s.id === selectedItemSubcategory.id)
            " class="col-12 col-md-3"
          >
            <fieldset class="q-mt-md bg-grey-2">
              <div class="col-12 col-sm-6 col-lg-3">
                <div class="row items-center justify-between q-mb-sm">
                  <div class="text-h3">
                    <span>Available Item Attributes List</span>
                  </div>
                  <q-btn
                    :disabled="selectedAvailableAttributeIds.length === 0"
                    square
                    outline
                    color="primary"
                    unelevated
                    class="bg-transparent shadow-2 q-pa-sm"
                    @click="saveSelectedAttributes()"
                  >
                    <q-icon
                      name="o_add"
                      class="material-icons-outlined text-primary"
                    />
                    <q-badge
                      v-if="selectedAvailableAttributeIds.length > 0"
                      floating
                      color="grey"
                      text-color="white"
                      square
                    >
                      {{ selectedAvailableAttributeIds.length }}
                    </q-badge>

                    <q-tooltip anchor="bottom middle" self="top middle">
                      Add Attributes
                    </q-tooltip>
                  </q-btn>
                </div>
                <div
                  v-if="!itemSubcategoryAttributes || itemSubcategoryAttributes.length === 0"
                  class="text-center text-grey-6 q-pa-lg"
                >
                  No Attributes Available
                </div>
                <div class="scroll scroll-container">
                  <div
                    v-for="(itemSubcategoryAttribute) in itemSubcategoryAttributes"
                    :key="itemSubcategoryAttribute.id"
                    :class="['row items-center q-mb-md q-pa-xs', selectedItemSubcategoryAttribute.id === itemSubcategoryAttribute.id ? 'highlight' : '']"
                    style="border: 1px solid #ccc;"
                  >
                    <div class="row items-center full-width q-col-gutter-x-sm">
                      <div class="col-5">
                        <div
                          class="q-pa-xs"
                        >
                          <q-checkbox
                            v-model="itemSubcategoryAttribute.isSelected"
                            size="xs"
                            class="q-pa-none"
                          />
                          <span>{{ itemSubcategoryAttribute.name }}</span>
                        </div>
                      </div>
                      <div class="col-5 cursor-pointer">
                        <span>
                          {{ itemSubcategoryAttribute.fieldType }}
                        </span>
                        <q-badge
                          v-if="itemSubcategoryAttribute.totalItemSubCategoryAttributesValuesCount > 0 && itemSubcategoryAttribute.fieldType ==='Dropdown'"
                          color="primary"
                          text-color="white"
                          square
                          class="justify-center q-ml-sm"
                          style="width: 25px; height: 25px;"
                        >
                          {{ itemSubcategoryAttribute.totalItemSubCategoryAttributesValuesCount }}
                          <q-tooltip>Number of Attribute Values</q-tooltip>
                        </q-badge>
                      </div>
                      <div class="col-2 cursor-pointer actions">
                        <q-icon
                          name="o_visibility"
                          class="cursor-pointer q-mr-sm"
                          size="xs"
                          @click="onViewItemAttribute(itemSubcategoryAttribute.id)"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
      </div>
    </q-card>
  </q-page>
</template>
<script setup>
import { ref, onMounted, computed } from "vue";
import { notifyError, notifySuccess } from "assets/utils";
import { useQuasar } from "quasar";

import viewItemAttribute from "modules/items/components/viewAttribute.vue";

import itemCategoryService from "../itemCategory.service";
import itemSubcategoryAttributeService from "../itemSubCategoryAttributes.service";

const itemCategories = ref([]);
const itemSubcategories = ref([]);
const itemSubcategoryAttributes = ref([]);
const sitesItemSubCategoryAttributes = ref([]);
const activeRowId = ref(null);
const selectedItemCategory = ref({ name: "", id: null });
const selectedItemSubcategoryAttribute = ref({ name: "", fieldType: "", id: null });
const selectedItemSubcategory = ref({ name: "", id: null });
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
    })
    .catch((err) => {
      console.error(err);
    })
    .finally(() => {
      isLoading.value = false;
    });
}
// --------------------------------------------------------------------------------------------------------------------------------------------------
// get all item subCategory attributes
// --------------------------------------------------------------------------------------------------------------------------------------------------
function getAllItemSubCategoryAttributeList (id) {
  isLoading.value = true;
  selectedItemSubcategory.value.id = id;
  selectedItemSubcategoryAttribute.value = { name: "", fieldType: "", id: null };
  itemSubcategoryAttributeService.getItemAttributeListNotInMappingAsync(id)
    .then((resp = {}) => {
      itemSubcategoryAttributes.value =
    (resp.itemSubcategoryAttributes ?? []).map(attr => ({
      ...attr,
      isSelected: false
    }));
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
  // selectedItemSubcategory.value.id = null;
  selectedItemSubcategoryAttribute.value = { name: "", fieldType: "", id: null };
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
// Get question list by help desk topic
// --------------------------------------------------------------------------------------------------------------------------------------------------
const getAllSitesItemSubCategoryAttributesListByItemSubCategoryId = async (name, id) => {
  selectedItemSubcategory.value = { name, id };
  sitesItemSubCategoryAttributes.value = [];
  isLoading.value = true;
  itemSubcategoryAttributeService.getAllSitesItemSubCategoryAttributesListByItemSubCategoryId(selectedItemSubcategory.value.id)
    .then((resp) => {
      sitesItemSubCategoryAttributes.value = (resp || []).map(attr => ({
        ...attr,
        isSelected: false
      }));
    })
    .finally(() => {
      isLoading.value = false;
    });
};
// For Available Attributes (Save button)
const selectedAvailableAttributeIds = computed(() =>
  itemSubcategoryAttributes.value
    ?.filter(x => x.isSelected)
    .map(x => x.id) || []
);

// For Mapped/Site Attributes (Delete button)
const selectedMappedAttributeIds = computed(() =>
  sitesItemSubCategoryAttributes.value
    ?.filter(x => x.isSelected)
    .map(x => x.id) || []
);

const saveSelectedAttributes = async () => {
  if (!selectedAvailableAttributeIds.value.length) {
    notifyError({
      type: "warning",
      message: "Please select at least one attribute"
    });
    return;
  }
  const payload = {
    itemSubCategoryId: selectedItemSubcategory.value.id,
    itemSubCategoryAttributeIds: selectedAvailableAttributeIds.value
  };

  try {
    await itemSubcategoryAttributeService.saveSitesItemSubCategoryAttributesMapping(payload)
      .then((resp) => {
        notifySuccess({
          message: "Attributes assigned successfully."
        });
      });
    getAllItemCategoryList();
    getAllItemSubcategoryList(selectedItemCategory.value.name, selectedItemCategory.value.id);
    await getAllSitesItemSubCategoryAttributesListByItemSubCategoryId(
      selectedItemSubcategory.value.name,
      selectedItemSubcategory.value.id
    );
    getAllItemSubCategoryAttributeList(selectedItemSubcategory.value.id);

    itemSubcategoryAttributes.value.forEach(x => {
      x.isSelected = false;
    });
  } catch (error) {
    console.error(error);
  }
};

const deleteSelectedAttributes = async (id) => {
  const ids = id || selectedMappedAttributeIds.value.join(",");
  if (!id && !selectedMappedAttributeIds.value.length) {
    notifyError({
      type: "warning",
      message: "Please select at least one attribute"
    });
    return;
  }

  try {
    await itemSubcategoryAttributeService.deleteSitesItemSubCategoryAttributesMapping(ids)
      .then(() => {
        notifySuccess({
          message: "Attributes deleted successfully"
        });
      });
    getAllItemCategoryList();
    getAllItemSubcategoryList(selectedItemCategory.value.name, selectedItemCategory.value.id);
    await getAllSitesItemSubCategoryAttributesListByItemSubCategoryId(
      selectedItemSubcategory.value.name,
      selectedItemSubcategory.value.id
    );
    getAllItemSubCategoryAttributeList(selectedItemSubcategory.value.id);
    sitesItemSubCategoryAttributes.value.forEach(x => {
      x.isSelected = false;
    });
  } catch (error) {
    console.error(error);
  }
};

// View item subcategory attribute  popup
const onViewItemAttribute = (id) => {
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
