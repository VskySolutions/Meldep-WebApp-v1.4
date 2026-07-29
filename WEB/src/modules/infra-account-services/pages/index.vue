<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Infrastructure" />
              <q-breadcrumbs-el label="Account" clickable to="/infra-account" />
              <q-breadcrumbs-el label="Services" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-md-4">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-12 col-md-5">
            <div class="row items-center justify-end no-wrap">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <multiSelectDropdown
                        v-model="search.itemTypeIds"
                        label="Item Type"
                        :options="itemTypesForDropdown.list.value"
                        :filter="itemTypesForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.infraAccountIds"
                        label="Account"
                        :options="infraAccountsForDropdown.list.value"
                        :filter="infraAccountsForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.ownerShipTypeIds"
                        label="Ownership Type"
                        :options="ownershipTypesForDropdown.list.value"
                        :filter="ownershipTypesForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.paymentTermIds"
                        label="Payment Term"
                        :options="paymentTermsForDropdown.list.value"
                        :filter="paymentTermsForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.projectIds"
                        label="Project Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
                      />
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              <!-- </div> -->
              <div class="q-ml-xs">
                <q-btn icon="o_add" outline label="Add Bulk" no-caps class="text-primary btnRounded" @click="onInfraAccountServicesAddBulk(refreshInfraAccountServicesList)">
                  <q-tooltip>Add Infra Account Services</q-tooltip>
                </q-btn>
                 <!-- Reset Column Width -->
                <q-btn
                  icon="o_refresh"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="resetColumnsWidth()"
                >
                  <q-tooltip>Reset Columns Width</q-tooltip>
                </q-btn>
                <!-- Column Hide/Show -->
                <columnVisibilityMenu
                  :all-column-names="allColumnNames"
                  :selected-column-names="selectedColumnNames"
                  @update:selected-column-names="selectedColumnNames = $event"
                  @select-all-columns="selectAllColumns"
                  @default-columns="defaultColumns"
                />
                <!-- Button to Open Sorting Dialog -->
                <q-btn
                  color="primary"
                  icon="o_sort"
                  class="btnRounded q-ml-xs"
                  @click="showSortDialog = true"
                >
                  <q-badge v-if="selectedSortCount > 0" color="green" floating class="q-ml-xs">
                    {{ selectedSortCount }}
                  </q-badge>
                  <q-tooltip>Sort</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <div class="table-infra-account-services">
        <div class="table-scroll-container">
          <q-table
            ref="tableRef"
            v-model:pagination="pagination"
            :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
            :loading="loading"
            :rows="rows"
            :columns="computedColumns"
            row-key="id"
            separator="cell"
            no-data-label="No data available"
            binary-state-sort
            :rows-per-page-options="[20, 50, 100, 200, 500]"
            @request="getAllInfraAccountServicesForList"
          >
            <template #loading>
              <q-inner-loading showing color="primary">
                <q-spinner-ios size="40px" class="q-mt-xl" />
              </q-inner-loading>
            </template>
            <template #header="props">
              <q-tr :props="props" class="bg-primary text-white">
                <!-- <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th> -->
                <q-th
                  v-for="col in props.cols"
                  :key="col.name"
                  :props="props"
                  :style="{
                    width: (resizeWidths?.[col.name] || 120) + 'px',
                    minWidth: '80px',
                    position: 'relative'
                  }"
                  @click="!isResizing && col.sortable"
                >
                  {{ col.label }}
                  <div class="resize-handle" @mousedown="(e) => startResize(e, col.name)" />
                </q-th>
                <q-th auto-width class="text-center">Actions</q-th>
              </q-tr>
            </template>
            <template #body="props">
              <q-tr
                :props="props"
                :class="highlightedId == props.row.id ? 'highlight' : ''"
              >
                <q-td v-if="selectedColumnNames.includes('infraAccount.name')">
                  <div v-if="editingRowId !== props.row.id">
                    <span
                      class="hoverable-cell"
                      @click="onInfraAccountView(props.row.infraAccount.id)"
                    >
                      {{ props.row.infraAccount.customerId + "(" + props.row.infraAccount.provider.dropDownValue + ")" }}
                    </span>
                  </div>
                  <formSingleSelectDropdown
                    v-else
                    v-model="props.row.infraAccountId"
                    :options="infraAccountDropdownSingleSelect.list.value"
                    :filter="infraAccountDropdownSingleSelect.filter"
                    :error="v$.infraAccountId.$error"
                    :error-message="v$.infraAccountId.$errors[0]?.$message"
                    @update:model-value="getInfraAccountServicesByInfraAccountId"
                  />
                </q-td>
                <q-td v-if="selectedColumnNames.includes('itemType.dropDownValue')">
                  <div v-if="editingRowId !== props.row.id">
                    {{ props.row.itemType.dropDownValue }}
                  </div>
                  <formSingleSelectDropdown
                    v-else
                    v-model="props.row.itemTypeId"
                    :options="itemTypeDropdownSingleSelect.list.value"
                    :filter="itemTypeDropdownSingleSelect.filter"
                    :error="v$.itemTypeId.$error"
                    :error-message="v$.itemTypeId.$errors[0]?.$message"
                  />
                </q-td>
                <q-td v-if="selectedColumnNames.includes('ownerShipType.dropDownValue')">
                  <div v-if="editingRowId !== props.row.id">
                    {{ props.row.ownerShipType.dropDownValue }}
                  </div>
                  <formSingleSelectDropdown
                    v-else
                    v-model="props.row.ownerShipTypeId"
                    :options="ownershipTypeDropdownSingleSelect.list.value"
                    :filter="ownershipTypeDropdownSingleSelect.filter"
                    :error="v$.ownerShipTypeId.$error"
                    :error-message="v$.ownerShipTypeId.$errors[0]?.$message"
                  />
                </q-td>
                <q-td v-if="selectedColumnNames.includes('name')">
                  <div v-if="editingRowId !== props.row.id">
                    {{ props.row.name }}
                  </div>
                  <q-input
                    v-else
                    v-model="props.row.name"
                    outlined
                    stack-label
                    hide-bottom-space
                    :error="v$.name.$error"
                    :error-message="v$.name.$errors[0]?.$message"
                    @blur="v$.name.$touch"
                    style="min-width:250px"
                  />
                </q-td>
                <q-td v-if="selectedColumnNames.includes('url')">
                  <div v-if="editingRowId !== props.row.id" class="ellipsis-cell">
                    {{ props.row.url }}
                  </div>
                  <q-input
                    v-else
                    v-model="props.row.url"
                    outlined
                    stack-label
                    hide-bottom-space
                    style="min-width:250px"
                  />
                </q-td>
                <q-td v-if="selectedColumnNames.includes('startDate')">
                  <div v-if="editingRowId !== props.row.id">
                    {{ props.row.startDate }}
                  </div>
                  <formDate
                    v-else
                    v-model="props.row.startDateStr"
                    :error="v$.startDateStr.$error"
                    :error-message="v$.startDateStr.$errors[0]?.$message"
                    :onBlur="() => v$.startDateStr.$touch()"
                  />
                </q-td>
                <q-td v-if="selectedColumnNames.includes('paymentTerm.dropDownValue')">
                  <div v-if="editingRowId !== props.row.id">
                    {{ props.row.paymentTerm.dropDownValue }}
                  </div>
                  <formSingleSelectDropdown
                    v-else
                    v-model="props.row.paymentTermId"
                    :options="paymentTermDropdownSingleSelect.list.value"
                    :filter="paymentTermDropdownSingleSelect.filter"
                    :error="v$.paymentTermId.$error"
                    :error-message="v$.paymentTermId.$errors[0]?.$message"
                  />
                </q-td>
                <q-td v-if="selectedColumnNames.includes('price')" align="right">
                  <div>
                    ${{ props.row.price }}
                  </div>
                  <q-popup-edit
                    v-if="!props.row.priceEndDate"
                    v-model="props.row.price"
                    v-slot="scope"
                    class="small-popup-title common-q-td"
                    style="width: 300px;"
                    @show="
                      props.row.oldPrice = props.row.price;
                      props.row.oldPriceStartDate = props.row.priceStartDate;
                    "
                  >
                    <div class="row items-center justify-between no-wrap q-mb-sm">
                      <div class="text-subtitle2">
                        Update Price :
                        <span class="text-primary">{{ props.row.name }}</span>
                      </div>
                      <q-btn
                        icon="o_close"
                        size="sm"
                        color="black"
                        flat
                        round
                        dense
                        @click="onPricePopupHide(props.row); scope.cancel()"
                      />
                    </div>
                    <div class="q-mb-xs">
                      <label class="label q-mb-xs text-black">Price<span class="required">*</span></label>
                    </div>
                    <q-input
                      v-model="props.row.price"
                      outlined
                      hide-bottom-space
                      prefix="$"
                      inputmode="decimal"
                      :error="!!props.row.priceError"
                      :error-message="props.row.priceError"
                      @update:model-value="props.row.priceError = ''"
                    />
                    <div class="q-mt-md">
                      <formDate
                        v-model="props.row.priceStartDate"
                        label="Price Start Date"
                        :wrapperClass="'col-12'"
                        :dateOptions="date => disableFutureDates(date, props.row.oldPriceStartDate, props.row.priceEndDate)"
                        :error="!!props.row.priceStartDateError"
                        :error-message="props.row.priceStartDateError"
                        :disable="!isPriceChanged(props.row)"
                        @update:model-value="props.row.priceStartDateError = ''"
                      />
                    </div>
                    <div class="row justify-end q-gutter-sm q-mt-md">
                      <q-btn
                        flat
                        dense
                        label="Cancel"
                        color="primary"
                        @click="onPricePopupHide(props.row), scope.cancel()"
                      />
                      <q-btn
                        unelevated
                        dense
                        label="Save"
                        color="primary"
                        :disable="!isPriceChanged(props.row)"
                        @click="onSubmitInfraAccountServicePrice(props.row, scope, 'price')"
                      />
                    </div>
                  </q-popup-edit>
                  <!-- <q-input
                    v-else
                    v-model="props.row.price"
                    outlined
                    stack-label
                    hide-bottom-space
                    prefix="$"
                    input-class="text-right"
                    inputmode="decimal"
                    class="break-error"
                    :error="v$.price.$error"
                    :error-message="v$.price.$errors[0]?.$message"
                    @blur="v$.price.$touch()"
                    @focus="props.row._originalPrice = props.row.price"
                    @change="onPriceChange(props.row)"
                  />
                  -->
                </q-td>
                <q-td v-if="selectedColumnNames.includes('ytd')" align="right">
                  <div>
                    ${{ props.row.ytd }}
                  </div>
                </q-td>
                <q-td v-if="selectedColumnNames.includes('infraProjectServices')">
                  <div class="row items-center q-gutter-xs">
                    <q-chip
                      v-for="(item, index) in (props.row.infraProjectServices || []).slice(0, 2)"
                      :key="index"
                      dense
                      class="bg-primary text-white"
                    >
                      {{ item.project.name }}
                    </q-chip>
                    <q-chip
                      v-if="props.row.infraProjectServices?.length > 2"
                      dense
                      clickable
                      class="bg-grey-4 text-black"
                      @click="onInfraAccountServicesView(props.row.id, true, refreshInfraAccountServicesList)"
                    >
                      +{{ props.row.infraProjectServices.length - 2 }} more...
                    </q-chip>
                  </div>
                </q-td>
                <q-td v-if="selectedColumnNames.includes('infraAccountServiceId')">
                  <div v-if="editingRowId !== props.row.id">
                    {{ props.row.infraAccountService.name }}
                  </div>
                  <formSingleSelectDropdown
                    v-else
                    v-model="props.row.infraAccountServiceId"
                    :required="false"
                    :disable="!props.row.infraAccountId"
                    :options="infraAccountServiceForDropdownSingleSelect.list.value"
                    :filter="infraAccountServiceForDropdownSingleSelect.filter"
                  />
                </q-td>
              <q-td
                v-if="selectedColumnNames.includes('createdBy.person.firstName')"
                class="common-q-td"
              >
                {{ props.row.createdBy.person.fullName }}
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('createdOnUtc')"
                class="common-q-td"
              >
                {{ props.row.createdOnUtc }}
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('updatedBy.person.firstName')"
                class="common-q-td"
              >
                {{ props.row.updatedBy.person.fullName }}
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('updatedOnUtc')"
                class="common-q-td"
              >
                {{ props.row.updatedOnUtc }}
              </q-td>
                <q-td class="text-center actions">
                  <template v-if="editingRowId === props.row.id">
                    <q-icon
                      name="o_cancel"
                      class="cursor-pointer q-mr-sm"
                      size="xs"
                      color="negative"
                      @click="onCancel(props.row)"
                    >
                      <q-tooltip>Cancel</q-tooltip>
                    </q-icon>
                    <q-icon
                      :loading="processing"
                      name="o_save"
                      class="cursor-pointer q-mr-sm hover-white"
                      size="xs"
                      color="primary"
                      @click="onSave(props.row)"
                    >
                      <q-tooltip>Save</q-tooltip>
                    </q-icon>
                  </template>
                  <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" size="xs" @click="onInfraAccountServicesView(props.row.id)">
                    <q-tooltip>View</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_add_task"
                    class="cursor-pointer q-mr-sm"
                    :class="props.row.priceEndDate ? 'text-grey cursor-not-allowed q-mr-sm' : 'cursor-pointer q-mr-sm'"
                    size="xs"
                    @click="!props.row.priceEndDate && onInfraAccountServicesView(props.row.id, true, refreshInfraAccountServicesList)"
                  >
                    <q-tooltip>Assign Project</q-tooltip>
                  </q-icon>
                  <q-icon
                    v-if="editingRowId !== props.row.id"
                    name="o_edit"
                    class="cursor-pointer q-mr-sm"
                    :class="props.row.priceEndDate ? 'text-grey cursor-not-allowed q-mr-sm' : 'cursor-pointer q-mr-sm'"
                    size="xs"
                    @click="!props.row.priceEndDate && onEdit(props.row)"
                  >
                    <q-tooltip>Edit</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_note_alt"
                    size="xs"
                    class="cursor-pointer q-mr-sm"
                    @click="() => {
                      activeRowId = props.row.id;
                      activeActionType = 'instruction';
                    }"
                  >
                    <q-tooltip>Add Instructions</q-tooltip>
                    <q-popup-edit
                      v-model="props.row.instructions"
                      anchor="top middle"
                      self="bottom middle"
                      buttons
                      persistent
                      label-set="Save"
                      label-cancel="Cancel"
                      class="instruction-popup"
                      @save="val => onSaveInstructions(props.row.id, val)"
                    >
                      <template #default="scope">
                        <div class="popup-container q-pa-sm">
                          <q-btn
                            icon="o_close"
                            flat
                            round
                            dense
                            size="sm"
                            class="absolute-top-right"
                            @click="scope.cancel"
                          />
                          <div class="text-subtitle2 q-mb-xs">Instructions</div>
                          <div class="editor-wrapper">
                            <q-editor
                              v-model="scope.value"
                              :dense="$q.screen.lt.md"
                              :toolbar="toolbar"
                              :fonts="fonts"
                              class="fixed-editor"
                            />
                          </div>
                        </div>
                      </template>
                    </q-popup-edit>
                  </q-icon>
                  <WalletPopup
                    :row="props.row"
                    :wallet-options="infraWalletTypeDropdownSingleSelect.list.value"
                    :on-save-api="saveWalletDetails"
                  />
                  <q-icon
                    name="o_delete_outline"
                    class="cursor-pointer"
                    color="negative"
                    size="xs"
                    @click="onSubmitInfraAccountServiceDelete(props.row, refreshInfraAccountServicesList)"
                  >
                    <q-tooltip>Delete</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_stop_circle"
                    class="cursor-pointer q-ml-sm"
                    size="xs"
                    color="red"
                  >
                    <q-tooltip>Stop</q-tooltip>
                    <q-popup-edit
                      v-model="props.row.priceEndDate"
                      v-slot="scope"
                      class="small-popup-title"
                      style="width: 300px;"
                      @show="props.row.priceEndDateError = ''"
                    >
                      <div class="row items-center justify-between no-wrap q-mb-sm">
                        <div class="text-subtitle2">
                          Stop Account Service :
                        <span class="text-primary">{{ props.row.name }}</span>
                      </div>
                        <q-btn v-close-popup icon="o_close" size="sm" color="black" flat round dense />
                      </div>
                      <formDate
                        v-model="scope.value"
                        label="Price End Date"
                        :wrapperClass="'col-12'"
                        :dateOptions="date => disableBeforePriceStartDate(date, props.row.priceStartDate)"
                        :error="!!props.row?.priceEndDateError"
                        :error-message="props.row?.priceEndDateError || ''"
                        @update:model-value="props.row.priceEndDateError = ''"
                      />
                      <div class="row justify-end q-gutter-sm q-mt-sm">
                        <q-btn v-close-popup label="Cancel" color="grey" flat dense />
                        <q-btn label="Save" color="primary" dense @click="onSubmitInfraAccountServicePrice(props.row, scope, 'endDate')" />
                      </div>
                    </q-popup-edit>
                  </q-icon>
                </q-td>
              </q-tr>
              <q-separator />
            </template>
            <template #bottom-row>
              <q-tr
                v-if="rows.length && totalPriceColumnIndex !== -1"
                class="bg-grey-2 text-black"
              >
                <!-- Columns before Total Services Cost -->
                <q-td
                  :colspan="totalPriceColumnIndex"
                  class="text-right text-weight-bold"
                >
                  Total Price:
                </q-td>

                <!-- Total Services Cost -->
                <q-td class="text-right text-weight-bold">
                  ${{ totalPrice.toFixed(2) }}
                </q-td>

                <!-- Year To Date -->
                <q-td
                  v-if="totalYtdColumnIndex !== -1"
                  class="text-right text-weight-bold"
                >
                  ${{ totalYtd.toFixed(2) }}
                </q-td>

                <!-- Remaining visible columns -->
                <q-td
                  v-for="n in trailingColumns"
                  :key="n"
                />

                <!-- Action column -->
                <q-td />
              </q-tr>
            </template>
          </q-table>
        </div>
      </div>
    </q-card>
  </q-page>
  <!-- Multi-Column Level Sorting -->
  <multiColumnSortingDialog
    v-model="showSortDialog"
    :columns="sortableColumns"
    :exclude-columns="['Price (Dollar)','Year To Date']"
    :multi-sort="multiSort"
    @add="addSortLevel"
    @remove="removeSortLevel"
    @apply="applyMultiSort"
  />
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import { useQuasar, Dialog } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { isDate } from "validators/zw_validators.js";
import { zwConfirm, notifySuccess } from "assets/utils";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
// import Confirmation from "src/dialogs/confirmation.vue";

import infraAccountsServicesService from "modules/infra-account-services/infraAccountServices.service";
import WalletPopup from "modules/infra-account/components/_walletPopup.vue";

// Shared Dropdowns
import infraAccountModule from "src/modules/infra-account/utils/dropdowns.js";
import infraAccountServiceModule from "src/modules/infra-account-services/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// SOP Change :- Shared DataTable Views
import multiColumnSortingDialog from "src/components/dataTable/_multiColumnSortingDialog.vue";
import columnVisibilityMenu from "src/components/dataTable/_columnVisibilityMenu.vue";

// SOP Change :- Shared Scripts DataTable Features
import { useColumnManager } from "composables/dataTable/useColumnManager.js";
import useColumnResize from "composables/dataTable/useColumnResize.js";
import useMultiSort from "composables/dataTable/useMultiSort.js";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

import {
  initInfraAccountServicesDialogs,
  onInfraAccountServicesView,
  onInfraAccountServicesAddBulk
} from "src/modules/infra-account-services/utils/dialogs.js";

import {
  initInfraAccountDialogs,
  onInfraAccountView
} from "src/modules/infra-account/utils/dialogs.js";

// SOP Change :- Shared Project Actions
import {
  initInfraAccountServiceActions,
  onSubmitInfraAccountServiceDelete
} from "src/modules/infra-account-services/utils/actions.js";

// Common variables
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);

const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const editingRowId = ref(null);
const editingRow = ref(null);
const processing = ref(false);
const activeActionType = ref(null);
const showSortDialog = ref(false);

const highlightedId = computed(() => { return activeRowId.value; });

// Table variables
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "infraAccount.name", label: "Account", field: "infraAccount.name", align: "left", sortable: true, default: true },
  { name: "itemType.dropDownValue", label: "Item Type", field: "itemType.dropDownValue", align: "left", sortable: true, default: true },
  { name: "ownerShipType.dropDownValue", label: "Ownership Type", field: "ownerShipType.dropDownValue", align: "left", sortable: true, default: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true, default: true },
  { name: "url", label: "URL", field: "url", align: "left", sortable: true, default: true },
  { name: "startDate", label: "Service Start Date", field: "startDate", align: "left", sortable: true, default: true },
  { name: "paymentTerm.dropDownValue", label: "Payment Term", field: "paymentTerm.dropDownValue", align: "left", sortable: true, default: true },
  { name: "price", label: "Price (Dollar)", field: "price", align: "right", sortable: true, default: true },
  { name: "ytd", label: "Year To Date", field: "ytd", align: "right", sortable: true, default: true },
  { name: "infraProjectServices", label: "Projects", field: "infraProjectServices", align: "left", sortable: false, default: true },
  { name: "infraAccountServiceId", label: "Infra Account Service", field: "infraAccountServiceId", align: "left", sortable: true, default: true },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "createdOnUtc", label: "Created On", field: "createdOnUtc", align: "left", sortable: true, default: false },
  { name: "updatedBy.person.firstName", label: "Updated By", field: "updatedBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "updatedOnUtc", label: "Updated On", field: "updatedOnUtc", align: "left", sortable: true, default: false }
]);

// Get/Map Infra Account list to table
const getAllInfraAccountServicesForList = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;
  loading.value = true;
  const sorts = {};
  const multi = multiSort.value;
  for (let i = 0; i < multi.length; i++) {
    const s = multi[i];
    if (s.column && s.direction) {
      sorts[s.column] = s.direction;
    }
  }
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, sorts, ...search.value };
  saveDataTableState({
    search: search.value,
    pagination: p,
    activeRowId: activeRowId.value,
    sorts
  });
  infraAccountsServicesService.getAllInfraAccountServicesForList(payload).then((resp) => {
    rows.value = resp?.infraAccountServicesList?.map(service => ({
      ...service,
      id: service.id,
      instructions: service.instructions || "",
      infraAccountServiceId: service.infraAccountService.id || "",
      startDateStr: service.startDate,
      infraAccountId: service.infraAccount.id,
      itemTypeId: service.itemType.id,
      ownerShipTypeId: service.ownerShipType.id,
      paymentTermId: service.paymentTerm.id,
      walletTypeId: service.walletType.id,
      isEditing: false
    })) ?? [];

    pagination.value = {
      ...pagination.value,
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    };
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

function refreshInfraAccountServicesList () {
  getAllInfraAccountServicesForList({ pagination: pagination.value });
}

const {
  search,
  pagination,
  activeRowId,
  sorts,
  resizeWidths,
  selectedColumnNames,

  saveDataTableState,
  saveResizableWidthState,
  saveColumnsState
} = useSiteTableState({
  storageKey: "infra-Accounts-Service-Index",

  defaultSearch: {
    searchText: "",
    itemTypeIds: [],
    projectIds: [],
    infraAccountIds: [],
    ownerShipTypeIds: [],
    paymentTermIds: []
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  },

  defaultSorts: {},

  defaultResizableWidth: {},

  defaultColumns: columns.value
    .filter(col => col.default === true)
    .map(col => col.name)
});

const lsSorts = sorts.value || null;
const sortableColumns = computed(() =>
  columns.value.filter(col => col.sortable)
);
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Column resize functionality (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  startResize,
  resetColumnsWidth,
  isResizing
} = useColumnResize({
  columns,
  resizeWidths,
  saveResizableWidthState
});
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Hide/Show Columns (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  selectAllColumns,
  defaultColumns,
  allColumnNames,
  computedColumns
} = useColumnManager({
  columns,
  selectedColumnNames,
  saveColumnsState,
  isResizing
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Sort Filter (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const {
  multiSort,
  addSortLevel,
  removeSortLevel,
  applyMultiSort,
  selectedSortCount
} = useMultiSort({
  lsSorts,
  saveDataTableState,
  onApplySort: () => {
    refreshInfraAccountServicesList();
  }
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const visibleColumns = computed(() =>
  columns.value.filter(col =>
    selectedColumnNames.value.includes(col.name)
  )
);

const totalPriceColumnIndex = computed(() =>
  visibleColumns.value.findIndex(c => c.name === "price")
);

const totalYtdColumnIndex = computed(() =>
  visibleColumns.value.findIndex(c => c.name === "ytd")
);

const trailingColumns = computed(() => {
  if (totalYtdColumnIndex.value === -1) return 0;

  return visibleColumns.value.length - totalYtdColumnIndex.value - 1;
});

const totalPrice = computed(() => {
  return rows.value.reduce((sum, row) => {
    const price = parseFloat(row.price) || 0;
    return sum + price;
  }, 0);
});

const totalYtd = computed(() => {
  return rows.value.reduce((sum, row) => {
    return sum + (Number(row.ytd) || 0);
  }, 0);
});

// Search records as per parameters
const onSearch = () => {
  const propps = { pagination: pagination.value };
  getAllInfraAccountServicesForList(propps);
};

// Clear search
const onClear = () => {
  search.value.projectIds = [];
  search.value.itemTypeIds = [];
  search.value.infraAccountIds = [];
  search.value.ownerShipTypeIds = [];
  search.value.paymentTermIds = [];
  saveDataTableState({
    search: search.value
  });
  onSearch();
};

function onEdit(row) {
  // prevent switching edit rows
  if (editingRowId.value && editingRowId.value !== row.id) {
    zwConfirm({
      title: "Edit in Progress",
      message: "Please finish editing the current row before continuing.",
      okLabel: "OK",
      cancel: false
    }, () => {});
    return;
  }

  editingRowId.value = row.id;
  editingRow.value = { ...row };

  infraAccountServiceForDropdownSingleSelect.load(row.infraAccountId);
  itemTypeDropdownSingleSelect.load("Account Item Type");
  infraAccountDropdownSingleSelect.load();
  ownershipTypeDropdownSingleSelect.load("Ownership Type");
  paymentTermDropdownSingleSelect.load("Payment Term");
}

function onCancel(row) {
  const index = rows.value.findIndex(r => r.id === row.id);

  if (index !== -1 && editingRow.value) {
    // revert changes
    rows.value[index] = { ...editingRow.value };
  }

  editingRowId.value = null;
  editingRow.value = null;
}

// const onPriceChange = (row) => {
//   const oldValue = Number(row._originalPrice);
//   const newValue = Number(row.price);

//   if (row.price == null || row.price === "" || newValue === oldValue) {
//     return;
//   }
//   Dialog.create({
//     component: Confirmation,
//     componentProps: {
//       title: "Confirmation",
//       message: "Are you sure you want to change the price?",
//       cancel: true
//     }
//   })
//     .onOk(() => {
//       row._originalPrice = newValue;
//     })
//     .onCancel(() => {
//       row.price = oldValue;
//     });
// }

function disableBeforePriceStartDate(date, startDate) {
  return new Date(date) >= new Date(startDate);
}

const disableFutureDates = (date, startDate, endDate) => {
  const selectedDate = new Date(date);
  selectedDate.setHours(0, 0, 0, 0);

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  // End Date exists → allow only dates after End Date
  if (endDate) {
    const end = new Date(endDate);
    end.setHours(0, 0, 0, 0);

    return selectedDate > end;
  }

  // Previous Start Date exists → allow dates from Start Date to Today
  if (startDate) {
    const start = new Date(startDate);
    start.setHours(0, 0, 0, 0);

    return selectedDate >= start && selectedDate <= today;
  }

  // Default → disable future dates
  return selectedDate <= today;
};

function getInfraAccountServicesByInfraAccountId(infraAccountId) {
  const row = rows.value.find(r => r.id === editingRowId.value);

  if (row) {
    row.infraAccountServiceId = "";
  }

  infraAccountServiceForDropdownSingleSelect.load(infraAccountId);
}

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initInfraAccountServicesDialogs(activeRowId);
initInfraAccountDialogs(activeRowId);
initInfraAccountServiceActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { projectNameDropdown } = projectModule();
const {
  infraWalletTypeDropdownSingleSelect,
  itemTypesForDropdown,
  infraAccountsForDropdown,
  ownershipTypesForDropdown,
  paymentTermsForDropdown,
  itemTypeDropdownSingleSelect,
  infraAccountDropdownSingleSelect,
  ownershipTypeDropdownSingleSelect,
  paymentTermDropdownSingleSelect
} = infraAccountModule();
const { infraAccountServiceForDropdownSingleSelect } = infraAccountServiceModule();

// ----------------------------
// Applied Filter Labels.
// ----------------------------
const mapFilterToLabel = (ids, list, label) => {
  if (!Array.isArray(ids) || !ids.length) return {};

  const text = ids
    .map(id => {
      const match = list.value.find(item => item.value === id);
      return match ? match.text : id;
    })
    .join(", ");

  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.itemTypeIds, itemTypesForDropdown.list, "Item Type"),
  ...mapFilterToLabel(search.value.infraAccountIds, infraAccountsForDropdown.list, "Account"),
  ...mapFilterToLabel(search.value.ownerShipTypeIds, ownershipTypesForDropdown.list, "OwnerShip Type"),
  ...mapFilterToLabel(search.value.paymentTermIds, paymentTermsForDropdown.list, "Payment Term"),
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name")
}));

function getFilterCount (key) {
  switch (key) {
  case "Item Type": return search.value.itemTypeIds?.length || 0;
  case "Account": return search.value.infraAccountIds?.length || 0;
  case "OwnerShip Type": return search.value.ownerShipTypeIds?.length || 0;
  case "Payment Term": return search.value.paymentTermIds?.length || 0;
  case "Project Name": return search.value.projectIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Item Type") {
    search.value.itemTypeIds = [];
  } else if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Account") {
    search.value.infraAccountIds = [];
  } else if (key === "OwnerShip Type") {
    search.value.ownerShipTypeIds = [];
  } else if (key === "Payment Term") {
    search.value.paymentTermIds = [];
  }
  delete appliedFilters.value[key];
  getAllInfraAccountServicesForList({ pagination: pagination.value });
}
// Validate rules
const decimalNumber = helpers.regex(/^\d+(\.\d{1,2})?$/);
const rules = {
  infraAccountId: { required: helpers.withMessage("Account is required", required) },
  itemTypeId: { required: helpers.withMessage("Item type is required", required) },
  ownerShipTypeId: { required: helpers.withMessage("OwnerShip type is required", required) },
  paymentTermId: { required: helpers.withMessage("Payment term is required", required) },
  startDateStr: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  name: { required: helpers.withMessage("Name is required", required) },
  price: {
    required: helpers.withMessage("Price is required", required),
    decimalNumber: helpers.withMessage(
      "Enter valid amount (max 2 decimal places)",
      decimalNumber
    )
  }
};
const currentRow = computed(() =>
  rows.value.find(r => r.id === editingRowId.value) || {}
);
const v$ = useVuelidate(rules, currentRow, { $lazy: true, $autoDirty: true });

const isValidDate = (value) => {
  if (!value) return false;
  const date = new Date(value);
  return !isNaN(date.getTime());
};

const isPriceChanged = (row) => {
  return (
    row.price !== '' &&
    Number(row.price) !== Number(row.oldPrice)
  );
};

// =================================================================================
// wallet details
// =================================================================================
function saveWalletDetails (row, payload) {
  return infraAccountsServicesService
    .addOrUpdateInstructions(row.id, payload)
    .then(() => {
      notifySuccess({ message: "Wallet details is saved successfully." });
    });
}

function onSaveInstructions (id, instructions) {
  setTimeout(function () {
    const payload = {
      instructions,
      isInstruction: true
    };

    infraAccountsServicesService
      .addOrUpdateInstructions(id, payload)
      .then(() => {
        notifySuccess({ message: "Instruction is saved successfully." });
      });
  });
}
// =================================================================================

async function onSave (row) {
  v$.value.$touch();

  if (v$.value.$invalid) {
    return;
  }

  try {
    const payload = {
      infraAccountId: row.infraAccountId,
      infraAccountServiceId: row.infraAccountServiceId,
      itemTypeId: row.itemTypeId,
      ownerShipTypeId: row.ownerShipTypeId,
      name: row.name,
      url: row.url,
      startDateStr: row.startDateStr,
      paymentTermId: row.paymentTermId,
      price: row.price,
      walletTypeId: row.walletTypeId,
      walletNumber: row.walletNumber
    };
    await infraAccountsServicesService.saveInfraAccountServices(row.id, payload);

    notifySuccess({
      message: "Infra Account Service updated successfully."
    });

    editingRowId.value = null;

    getAllInfraAccountServicesForList({ pagination: pagination.value });
  } catch (error) {
    console.error(error);
  }
}

const onPricePopupHide = (row) => {
  if (!row.isPriceSaved) {
    row.price = row.oldPrice;
    row.priceStartDate = row.oldPriceStartDate;
  }

  row.priceError = "";
  row.priceStartDateError = "";
};

const onSubmitInfraAccountServicePrice = async (row, scope, type) => {
  row.priceError = "";
  row.priceStartDateError = "";
  row.priceEndDateError = "";

  let isValid = true;

  if (type === "price") {
    if (row.price === null || row.price === undefined || row.price === "") {
      row.priceError = "Price is required";
      isValid = false;
    } else if (Number(row.price) <= 0) {
      row.priceError = "Price must be greater than 0";
      isValid = false;
    }

    if (!row.priceStartDate) {
      row.priceStartDateError = "Start Date is required";
      isValid = false;
    } else if (!isValidDate(row.priceStartDate)) {
      row.priceStartDateError = "Please enter a valid Start Date";
      isValid = false;
    }
  }

  if (type === "endDate") {
    if (!scope.value) {
      row.priceEndDateError = "End Date is required";
      isValid = false;
    } else if (!isValidDate(scope.value)) {
      row.priceEndDateError = "Please enter a valid End Date";
      isValid = false;
    } else {
      row.priceEndDate = scope.value;
    }
  }
  if (!isValid) return;
  try {
    if (type === "price" && row.oldPrice === row.price) {
      scope.cancel();
      return;
    }
    processing.value = true;
    const payload = {
      price: row.price,
      startDate: row.priceStartDate,
      endDate: row.priceEndDate ?? null
    };
    await infraAccountsServicesService.updateInfraAccountServicePrice(row.id, payload);
    row.isPriceSaved = true;
    getAllInfraAccountServicesForList({
      pagination: pagination.value
    });
    scope.cancel();
    notifySuccess({
      message: "Updated successfully."
    });
  } catch (error) {
    sendError("Error updating data", error);
  } finally {
    processing.value = false;
  }
};
// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshInfraAccountServicesList();
});

watch(activeRowId, (val) => {
  const formattedSorts = {};

  for (const s of multiSort.value) {
    if (s.column && s.direction) {
      formattedSorts[s.column] = s.direction;
    }
  }

  saveDataTableState({
    search: search.value,
    pagination: pagination.value,
    activeRowId: val,
    sorts: formattedSorts
  });
});

onMounted(() => {
  infraAccountsForDropdown.load();
  itemTypesForDropdown.load("Account Item Type");
  ownershipTypesForDropdown.load("Ownership Type");
  paymentTermsForDropdown.load("Payment Term");

  projectNameDropdown.load(false, true, true);
  infraWalletTypeDropdownSingleSelect.load("Wallet Type");
  infraAccountServiceForDropdownSingleSelect.load();
  if (tableRef.value) {
    tableRef.value.requestServerInteraction();
  }

  if (!activeRowId.value) {
    activeRowId.value = null;
  }
});

</script>
<style>
.ellipsis-cell {
  max-width: 260px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.hover-white:hover {
  color: #fff !important;
}
.table-infra-account-services .Custom-DataTable {
  min-width: max-content;
}
</style>
