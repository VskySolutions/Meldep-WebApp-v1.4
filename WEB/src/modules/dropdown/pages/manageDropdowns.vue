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
              <q-breadcrumbs-el :label="moduleName" class="cursor-pointer" @click="$router.back()" />
              <q-breadcrumbs-el label="Manage Dropdowns" />
            </q-breadcrumbs>
          </div>
          <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary btnRounded q-ml-sm" @click="$router.back()" />
        </div>
      </q-card-section>
      <q-separator />
      <div class="q-px-md">
        <div class="row q-col-gutter-sm">
          <div class="col-12 col-md-6">
            <div   :class="['row items-center justify-between',dropdownTypes.length && dropdownTypes[0].groupName ? 'q-my-sm' : 'q-my-md']">
              <div class="text-h3">
                <span v-if="dropdownTypes.length && dropdownTypes[0].groupName">
                  Group Name : <span class="text-primary">{{ dropdownTypes[0].groupName }}</span>
                </span>
                <span v-else>Dropdown Type</span>
              </div>
              <q-btn
                v-if="dropdownTypes.length && dropdownTypes[0].groupName"
                color="primary"
                icon="o_add"
                label="Add"
                no-caps
                @click="onAddDropdownType"
              />
            </div>
            <div class="row items-center q-pa-sm bg-primary text-white text-h6">
              <div class="col-3">
                Dropdown Type
              </div>
              <div class="col-4">
                Module Name
              </div>
              <div class="col-4">
                Values Sorting
              </div>
              <div class="col-1 text-center">
                Actions
              </div>
            </div>
            <div
              v-if="!dropdownTypes || dropdownTypes.length === 0"
              class="text-center text-grey-6 q-pa-lg"
            >
              No Data Available
            </div>
            <div class="scroll scroll-container">
              <div id="sortable-dropdown-types">
                <div
                  v-for="(dropdownType, dropdownTypeIndex) in dropdownTypes"
                  :id="`dropdownType-${dropdownType.sortOrder}`"
                  :key="dropdownType.id"
                  :class="activeRowId == dropdownType.id ? 'bg-grey-2' : ''"
                  class="row items-center q-mb-md q-pa-sm drag-cursor"
                  style="border: 1px solid #ccc;"
                >
                  <div class="col-3 cursor-pointer">
                    <template v-if="dropdownType.showInputForNew || (activeEdit.field === 'type' && activeEdit.rowId === dropdownType.id) && dropdownType.groupName">
                      <div class="row items-center no-wrap">
                        <q-input
                          v-model="dropdownType.type"
                          outlined
                          stack-label
                          hide-bottom-space
                          dense
                          class="full-width q-pr-sm"
                          placeholder="Enter Dropdown Type"
                          hint="Press Enter to save"
                          @keyup.enter="saveOrUpdateDropdownType(dropdownType)"
                        >
                          <q-tooltip>
                            Press enter key to save Dropdown Type
                          </q-tooltip>
                        </q-input>
                        <q-btn
                          v-if="dropdownType.type && !dropdownType.showInputForNew"
                          icon="o_close"
                          size="xs"
                          color="black"
                          flat
                          round
                          dense
                          class="q-mb-md"
                          @click="
                            dropdownType.type = dropdownType._oldText;
                            activeEdit = { rowId: null, field: '' }
                          "
                        />
                      </div>
                    </template>
                    <template v-else>
                      <div
                        class="row items-center justify-between q-pa-xs cursor-pointer"
                        @click="getAllDropdownValuesList(dropdownType.type, dropdownType.id)"
                        @dblclick="
                          dropdownType._oldText = dropdownType.type;
                          activeEdit = { rowId: dropdownType.id, field: 'type' }
                        "
                      >
                        <span>{{ dropdownType.type }}</span>
                      </div>
                    </template>
                    <q-tooltip v-if="dropdownType.type && !dropdownType.showInputForNew && activeEdit.rowId !== dropdownType.id && dropdownType.groupName">Double-click to edit</q-tooltip>
                  </div>
                  <div class="col-4 cursor-pointer">
                    <template v-if="dropdownType.showInputForNew || activeEdit.field === 'moduleName' && activeEdit.rowId === dropdownType.id">
                      <div class="row items-center no-wrap">
                        <q-select
                          v-model="dropdownType.moduleName"
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
                          class="full-width q-pr-sm"
                          hint="Press Enter to save"
                          @filter="getAllModuleListForFilter"
                          @keyup.enter="saveOrUpdateDropdownType(dropdownType)"
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

                          <q-tooltip>
                            Press enter key to save Module Name
                          </q-tooltip>
                        </q-select>
                        <q-btn
                          v-if="dropdownType.moduleName && !dropdownType.showInputForNew"
                          icon="o_close"
                          size="xs"
                          color="black"
                          flat
                          round
                          dense
                          class="q-mb-md"
                          @click="
                            dropdownType.moduleName = dropdownType._oldText;
                            activeEdit = { rowId: null, field: '' }
                          "
                        />
                      </div>
                    </template>

                    <template v-else>
                      <div
                        class="row items-center justify-between q-pa-xs cursor-pointer"
                        @click="getAllDropdownValuesList(dropdownType.type, dropdownType.id)"
                        @dblclick=" dropdownType._oldText = dropdownType.moduleName;
                                    activeEdit = { rowId: dropdownType.id, field: 'moduleName' }"
                      >
                        <span>{{ dropdownType.moduleName }}</span>
                      </div>
                    </template>

                    <q-tooltip v-if="dropdownType.moduleName && !dropdownType.showInputForNew && activeEdit.rowId !== dropdownType.id">
                      Double-click to edit
                    </q-tooltip>
                  </div>
                  <div class="col-4">
                    <q-radio
                      v-model="dropdownType.isAlphabeticalOrNumerical"
                      label="Alphabetical"
                      dense
                      class="text-black q-mr-sm"
                      :val="false"
                      @update:model-value="saveOrUpdateDropdownType(dropdownType)"
                    />
                    <q-radio
                      v-model="dropdownType.isAlphabeticalOrNumerical"
                      label="Custom Order"
                      dense
                      class="text-black"
                      :val="true"
                      @update:model-value="saveOrUpdateDropdownType(dropdownType)"
                    />
                  </div>
                  <div class="col-1 text-center">
                    <q-icon
                      name="o_delete"
                      color="negative"
                      size="sm"
                      class="cursor-pointer q-ml-sm"
                      @click="onDeleteDropdownType(dropdownType, dropdownTypeIndex)"
                    >
                      <q-tooltip>Delete</q-tooltip>
                    </q-icon>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <!-- Right Column -->
          <div class="col-12 col-md-6">
            <div class="row items-center justify-between q-my-sm">
              <div class="text-h3">
                <span v-if="selectedType.type">
                  Dropdown Values : <span class="text-primary"> {{ selectedType.type }}</span>
                </span>
              </div>
              <q-btn
                v-if="selectedType.type"
                color="primary"
                icon="o_add"
                label="Add"
                no-caps
                @click="onAddDropdownValue"
              />
            </div>
            <div v-if="selectedType?.type" class="row items-center q-pa-sm bg-primary text-white text-h6">
              <div class="col-1">
                Sort No.
              </div>
              <div class="col-4">
                Dropdown Value
              </div>
              <div class="col-1">
                Des.
                <q-tooltip>Description</q-tooltip>
              </div>
              <div class="col-2">
                Bg Color
              </div>
              <div class="col-2">
                Txt Color
              </div>
              <div class="col-1 text-center">
                Active
              </div>
              <div class="col-1 text-center">
                Actions
              </div>
            </div>
            <div
              v-if="selectedType?.type && (!dropdownValues || dropdownValues.length === 0)"
              class="text-center text-grey-6 q-pa-lg"
            >
              No Data Available
            </div>
            <div class="scroll scroll-container">
              <div id="sortable-dropdown">
                <div
                  v-for="(dropdown, dropdownIndex) in dropdownValues"
                  :id="`dropdown-${dropdown.sortOrder}`"
                  :key="dropdown.id"
                  class="row items-center q-mb-md q-pa-xs bg-grey-2 drag-cursor"
                  style="border: 1px solid #ccc;"
                >
                  <div class="col-1">
                    <span class="q-mx-md">{{ dropdown.sortOrder }}</span>
                  </div>
                  <div class="col-4">
                    <!-- Dropdown Value Column -->
                    <template v-if="dropdown.showInputForNewDropdownValue || activeEdit.rowId === dropdown.id && activeEdit.field === 'dropdownValue'">
                      <div class="row items-center">
                        <q-input
                          v-model="dropdown.dropdownValue"
                          outlined
                          stack-label
                          hide-bottom-space
                          dense
                          class="q-ml-sm"
                          placeholder="Enter Dropdown Value"
                          hint="Press Enter to save"
                          @keyup.enter="saveOrUpdateDropdownvalue(dropdown, dropdownIndex)"
                        >
                          <q-tooltip>
                            Press enter key to save Dropdown Value
                          </q-tooltip>
                        </q-input>
                        <q-btn
                          v-if="dropdown.dropdownValue && !dropdown.showInputForNewDropdownValue"
                          icon="o_close"
                          size="xs"
                          color="black"
                          flat
                          round
                          dense
                          class="q-ml-sm q-mb-md"
                          @click="activeEdit = { rowId: null, field: null }"
                        />
                      </div>
                    </template>
                    <template v-else>
                      <div
                        class="row items-center justify-between cursor-pointer q-ml-sm"
                        @dblclick="
                          activeEdit = {
                            rowId: dropdown.id,
                            field: 'dropdownValue'
                          }
                        "
                      >
                        <span>{{ dropdown.dropdownValue }}</span>
                      </div>
                    </template>
                    <q-tooltip v-if="dropdown.dropdownValue && !dropdown.showInputForNew && activeEdit.rowId !== dropdown.id">Double click to edit</q-tooltip>
                  </div>
                  <div class="col-1">
                    <q-icon
                      name="o_notes"
                      color="primary"
                      size="sm"
                      class="cursor-pointer"
                      @click="openDescriptionDialog(dropdown)"
                    >
                      <q-tooltip>Description</q-tooltip>
                    </q-icon>
                    <q-icon
                      v-if="dropdown.description"
                      name="o_info"
                      size="16px"
                      class="q-mr-xs"
                    >
                      <q-tooltip v-if="dropdown.description" class="text-wrap break-words q-pa-sm" max-width="300px">
                        <div v-html="dropdown.description" />
                      </q-tooltip>
                    </q-icon>
                  </div>
                  <div class="col-2">
                    <q-input
                      v-model="dropdown.bgColor"
                      filled
                      placeholder="Bg Color"
                      class="my-input q-mr-sm"
                      @update:model-value="() => saveOrUpdateDropdownvalue(dropdown, dropdownIndex)"
                    >
                      <template #append>
                        <!-- Color preview square -->
                        <div :style="{ backgroundColor: dropdown.bgColor }" class="color-square q-mr-sm" />
                        <q-icon name="o_colorize" class="cursor-pointer">
                          <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                            <q-color
                              v-model="dropdown.bgColor"
                              no-header
                              no-footer
                              default-view="palette"
                              @change="() => saveOrUpdateDropdownvalue(dropdown, dropdownIndex)"
                            />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                  <div class="col-2">
                    <q-input
                      v-model="dropdown.color"
                      placeholder="Txt Color"
                      filled
                      class="my-input q-my-xs"
                      @update:model-value="() => saveOrUpdateDropdownvalue(dropdown, dropdownIndex)"
                    >
                      <template #append>
                        <!-- Color preview square -->
                        <div :style="{ backgroundColor: dropdown.color }" class="color-square q-mr-sm" />
                        <q-icon name="o_colorize" class="cursor-pointer">
                          <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                            <q-color
                              v-model="dropdown.color"
                              no-header
                              no-footer
                              default-view="palette"
                              @change="() => saveOrUpdateDropdownvalue(dropdown, dropdownIndex)"
                            />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                  <div class="col-1 text-center">
                    <q-toggle
                      v-model="dropdown.active"
                      dense class="q-mx-md"
                      @update:model-value="() => saveOrUpdateDropdownvalue(dropdown, dropdownIndex)"
                    />
                  </div>
                  <div class="col-1 text-center">
                    <q-icon
                      name="o_delete"
                      color="negative"
                      size="sm"
                      class="cursor-pointer"
                      @click="onDeleteDropdownValue(dropdown, dropdownIndex)"
                    >
                      <q-tooltip>Delete</q-tooltip>
                    </q-icon>
                  </div>
                  <q-dialog v-model="isDescriptionDialogOpen[dropdown.id]" persistent transition-show="scale" transition-hide="scale" @hide="onDialogHide">
                    <q-card>
                      <q-card-section class="card-header with-tools position-relative">
                        <div class="text-h2">Description For {{ dropdown.dropdownValue }}</div>
                        <q-btn v-close-popup icon="o_close" class="close" flat round dense style="position: absolute; right: 5px; top: 10px;" />
                      </q-card-section>

                      <q-card-section>
                        <q-editor
                          v-model="dropdown.description"
                          :model-value="dropdown.description ?? ''"
                          :dense="$q.screen.lt.md"
                          :toolbar="toolbar"
                          :fonts="fonts"
                          placeholder="Type description here..."
                        />
                      </q-card-section>

                      <q-card-actions align="right">
                        <q-btn v-close-popup color="grey-8" flat dense bordered label="Close" />
                        <q-btn color="primary" flat bordered label="Save" @click="saveOrUpdateDropdownvalue(dropdown, dropdownIndex)" />
                      </q-card-actions>
                    </q-card>
                  </q-dialog>
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
import { Sortable } from "sortablejs";
import { notifyError, notifySuccess, zwConfirmDelete } from "assets/utils";
import { useQuasar } from "quasar";

import commonService from "services/common.service";
import manageDropdownsService from "modules/dropdown/dropdown.service";

const $q = useQuasar();
const dropdownTypeId = ref(history.state?.id);
const groupName = ref(history.state?.groupName);
const moduleName = ref(history.state?.moduleName);
const activeEdit = ref({ rowId: null, field: null });
const dropdownTypes = ref([]);
const dropdownValues = ref([]);
const selectedType = ref({ name: "", id: null });
const isDescriptionDialogOpen = ref({});
const activeRowId = ref(null);

const moduleOptions = [
  { value: 'Dashboard', text: 'Dashboard' },
  { value: 'My Work', text: 'My Work' },
  { value: 'Talent Hire', text: 'Talent Hire' },
  { value: 'Project Management', text: 'Project Management' },
  { value: 'Org Management', text: 'Org Management' },
  { value: 'SDLC', text: 'SDLC' },
  { value: 'CRM', text: 'CRM' },
  { value: 'Finance', text: 'Finance' },
  { value: 'Marketing', text: 'Marketing' },
  { value: 'Infrastructure', text: 'Infrastructure' },
  { value: 'Item', text: 'Item' },
  { value: 'Help Desk', text: 'Help Desk' }
].sort((a, b) => a.text.localeCompare(b.text));

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get dropdown types by ID and GroupName
// --------------------------------------------------------------------------------------------------------------------------------------------------
function getDropdownTypesByIdAndGroupName () {
  manageDropdownsService.getDropdownTypesByIdAndGroupName(dropdownTypeId.value, groupName.value).then((resp) => {
    dropdownTypes.value = resp;
    nextTick(makeSortable);
    if (dropdownTypes.value.length === 1) {
      const onlyItem = dropdownTypes.value[0];
      getAllDropdownValuesList(onlyItem.type, onlyItem.id);
    }
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get dropdown values by DropdownType
// --------------------------------------------------------------------------------------------------------------------------------------------------
const getAllDropdownValuesList = (type, id) => {
  commonService.getDropDown(type).then((resp) => {
    selectedType.value = { type, id };
    dropdownValues.value = resp;
    activeRowId.value = id;
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

// --------------------------------------------------------------------------------------------------------------------------------------------------
//  Add new dropdown type
// --------------------------------------------------------------------------------------------------------------------------------------------------
function onAddDropdownType () {
  const lastSortOrder = dropdownTypes.value.length
    ? dropdownTypes.value[dropdownTypes.value.length - 1].sortOrder || 0
    : 0;
  dropdownTypes.value.push({
    moduleName: moduleName.value || "",
    groupName: dropdownTypes.value[0]?.groupName,
    type: "",
    sortOrder: lastSortOrder + 1,
    showInputForNew: true,
    isAlphabeticalOrNumerical: false,
    deleted: false
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
//  Add new dropdown value
// --------------------------------------------------------------------------------------------------------------------------------------------------
function onAddDropdownValue () {
  if (!Array.isArray(dropdownValues.value)) {
    dropdownValues.value = [];
  }
  const lastSortOrder = dropdownValues.value.length
    ? dropdownValues.value[dropdownValues.value.length - 1].sortOrder || 0
    : 0;
  dropdownValues.value.push({
    dropdownValue: "",
    sortOrder: lastSortOrder + 1,
    description: "",
    bgColor: "",
    color: "",
    active: false,
    showInputForNewDropdownValue: true
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Delete dropdown Value
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onDeleteDropdownValue = (dropdown, dropdownIndex) => {
  if (!dropdown.id) {
    dropdownValues.value.splice(dropdownIndex, 1);
    return;
  }
  zwConfirmDelete({ data: `${dropdown.dropdownValue}` }, () => {
    manageDropdownsService.deleteDropDown(dropdown.id).then(() => {
      notifySuccess({ message: "Dropdown Value is deleted successfully." });
      dropdownValues.value.splice(dropdownIndex, 1);
    });
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Delete dropdown type
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onDeleteDropdownType = (dropdownType, dropdownTypeIndex) => {
  if (!dropdownType.id) {
    dropdownTypes.value.splice(dropdownTypeIndex, 1);
    return;
  }
  zwConfirmDelete({ data: `${dropdownType.type}` }, () => {
    manageDropdownsService.deleteDropDownType(dropdownType.id).then(() => {
      notifySuccess({ message: "Dropdown Type is deleted successfully." });
      dropdownTypes.value.splice(dropdownTypeIndex, 1);
    });
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
    // Sortable for Dropdown Types
    const dropdownTypeContainer = document.getElementById("sortable-dropdown-types");
    if (dropdownTypeContainer) {
      sortableInstances.value.push(
        new Sortable(dropdownTypeContainer, {
          animation: 150,
          ghostClass: "sortable-ghost",
          onMove (evt) {
            // Prevent moving to dropdownValues
            return evt.to.id !== "sortable-dropdown";
          },
          onEnd (evt) {
            if (evt.oldIndex === evt.newIndex) return;
            const moved = dropdownTypes.value.splice(evt.oldIndex, 1)[0];
            dropdownTypes.value.splice(evt.newIndex, 0, moved);
            updateDropdownTypeSortOrder();
          }
        })
      );
    }
    // Sortable for Dropdown Values
    const dropdownContainer = document.getElementById("sortable-dropdown");
    if (dropdownContainer) {
      sortableInstances.value.push(
        new Sortable(dropdownContainer, {
          animation: 150,
          ghostClass: "sortable-ghost",
          onMove (evt) {
            // Prevent moving to dropdownTypes
            return evt.to.id !== "sortable-dropdown-types";
          },
          onEnd (evt) {
            if (evt.oldIndex === evt.newIndex) return;
            const movedItem = dropdownValues.value.splice(evt.oldIndex, 1)[0];
            dropdownValues.value.splice(evt.newIndex, 0, movedItem);

            // Update sort order
            updateDropdownSortOrder();
          }
        })
      );
    }
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update sortOrder for all dropdown types
// --------------------------------------------------------------------------------------------------------------------------------------------------
const updateDropdownTypeSortOrder = async () => {
  if (dropdownTypes.value && dropdownTypes.value.length) {
    dropdownTypes.value.forEach((dropdownType, dropdownTypeIndex) => {
      dropdownType.sortOrder = dropdownTypeIndex + 1;
    });
    for (const dropdownType of dropdownTypes.value) {
      saveOrUpdateDropdownType(dropdownType);
    }
    getDropdownTypesByIdAndGroupName();
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update sortOrder for all dropdown values
// --------------------------------------------------------------------------------------------------------------------------------------------------
const updateDropdownSortOrder = async () => {
  // Update sortOrder based on index
  dropdownValues.value.forEach((dropdown, dropdownIndex) => {
    dropdown.sortOrder = dropdownIndex + 1;
  });
  // Save all dropdown values one by one
  for (const dropdown of dropdownValues.value) {
    await saveOrUpdateDropdownvalue(dropdown);
  }
  getDropdownTypesByIdAndGroupName();
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Save or update dropdown type
// --------------------------------------------------------------------------------------------------------------------------------------------------
const saveOrUpdateDropdownType = (dropdownType) => {
  if (!dropdownType.type || !dropdownType.type.trim()) {
    notifyError({ message: "Please enter a value for the dropdown type." });
    return;
  }

  if (!dropdownType.moduleName || !dropdownType.moduleName.trim()) {
    notifyError({ message: "Please enter a module name." });
    return;
  }
  const isUpdate = !!dropdownType.id; // true if updating

  const payload = {
    id: dropdownType.id || null,
    type: dropdownType.type,
    moduleName: dropdownType.moduleName,
    isAlphabeticalOrNumerical: dropdownType.isAlphabeticalOrNumerical,
    groupName: dropdownTypes.value[0]?.groupName,
    sortOrder: dropdownType.sortOrder
  };
  manageDropdownsService.saveDropDownType(payload.id, payload).then((resp) => {
    if (!isUpdate) {
      dropdownType.id = resp.id;
      dropdownType.type = resp.type;
      dropdownType.sortOrder = resp.sortOrder;
    }
    getDropdownTypesByIdAndGroupName();
    dropdownType.showInputForNew = false; // hide input
    activeEdit.value = { rowId: null }; // stop editing

    notifySuccess({
      message: isUpdate
        ? "Dropdown type updated successfully."
        : "Dropdown type saved successfully."
    });
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Save or update dropdown value
// --------------------------------------------------------------------------------------------------------------------------------------------------
async function saveOrUpdateDropdownvalue (dropdown, dropdownIndex) {
  if (!dropdown.dropdownValue || !dropdown.dropdownValue.trim()) {
    notifyError({ message: "Please enter a value for the dropdown value." });
    return;
  }
  const isUpdate = !!dropdown.id; // true if updating
  const payload = {
    id: dropdown.id || null,
    dropdownTypeId: selectedType.value.id,
    dropdownValue: dropdown.dropdownValue,
    sortOrder: dropdown.sortOrder || 0,
    description: dropdown.description,
    bgColor: dropdown.bgColor,
    color: dropdown.color,
    active: dropdown.active
  };
  manageDropdownsService.saveDropDown(payload.id, payload).then((resp) => {
    if (!isUpdate) {
      dropdown.id = resp.id;
    }
    dropdown.sortOrder = payload.sortOrder;
    dropdown.showInputForNewDropdownValue = false; // hide input
    activeEdit.value = { rowId: null }; // stop editing

    notifySuccess({
      message: isUpdate
        ? "Dropdown Value updated successfully."
        : "Dropdown Value saved successfully."
    });
    isDescriptionDialogOpen.value = {
      ...isDescriptionDialogOpen.value,
      [payload.id]: false
    };
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Open description dialog
// --------------------------------------------------------------------------------------------------------------------------------------------------
function openDescriptionDialog (dropdown) {
  isDescriptionDialogOpen.value[dropdown.id] = true;
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

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On page rendering
// --------------------------------------------------------------------------------------------------------------------------------------------------
onMounted(() => {
  getDropdownTypesByIdAndGroupName();
});
</script>
<style>
.color-square {
  width: 24px;
  height: 24px;
  border: 1px solid #ccc;
  border-radius: 4px;
}
.drag-cursor{
 cursor: grab;
}
.sortable-ghost {
  opacity: 0.5;
  background-color: rgb(216, 216, 216);
}
.scroll-container {
  max-height: 75vh;
}
</style>
