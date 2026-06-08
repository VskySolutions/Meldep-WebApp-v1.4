<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 68vw !important;max-width: 68vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Inventory</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense @click="onDialogClose()" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Inventory Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md q-justify-center">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Office Location</label>
                  <q-select
                    v-model="model.officeLocationId"
                    clearable
                    use-input
                    stack-label
                    outlined
                    hide-bottom-space
                    :dense="true"
                    :options="officeLocationList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    @filter="getAllOfficeLocationDropdownForFilter"
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
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Device Type<span class="required">*</span></label>
                  <q-select
                    v-model="model.itemTypeId" clearable use-input outlined stack-label hide-bottom-space :dense="true" :readonly="readonlyDeviceType!= '' ? '' : 'readonlyDeviceType'"
                    :options="itemTypeList" option-value="value" option-label="text" emit-value map-options :error="v$.itemTypeId.$error" :error-message="v$.itemTypeId.$errors[0]?.$message"
                    @filter="filterFn1" @update:model-value="fetchInventoryCode" @blur="v$.itemTypeId.$touch"
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
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Device No<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.inventorycode" outlined stack-label hide-bottom-space :dense="true"
                      :error="v$.inventorycode.$error" :error-message="v$.inventorycode.$errors[0]?.$message" @blur="v$.inventorycode.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Service code</label>
                  <div>
                    <q-input v-model="model.serviceCode" outlined stack-label hide-bottom-space :dense="true" />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Warranty</label>
                  <div>
                    <q-input v-model="model.warranty" outlined stack-label hide-bottom-space :dense="true" maxlength="50" />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Warranty Expiry Date</label>
                  <div>
                    <q-input v-model="model.warrantyExpiryDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense :error="v$.warrantyExpiryDateStr.$error" :error-message="v$.warrantyExpiryDateStr.$errors[0]?.$message" @blur="v$.warrantyExpiryDateStr.$touch">
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date v-model="model.warrantyExpiryDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Guaranty</label>
                  <div>
                    <q-input v-model="model.guaranty" outlined stack-label hide-bottom-space :dense="true" maxlength="50" />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Model Name / Number</label>
                  <div>
                    <q-input v-model="model.modelNameORNumber" outlined stack-label hide-bottom-space :dense="true" />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Inventory Status<span class="required">*</span></label>
                  <q-select
                    v-model="model.inventoryStatusId" clearable use-input outlined stack-label hide-bottom-space :dense="true" :options="inventoryStatusList" option-value="value" option-label="text" emit-value map-options :error="v$.inventoryStatusId.$error"
                    :error-message="v$.inventoryStatusId.$errors[0]?.$message" @filter="filterFn2" @blur="v$.inventoryStatusId.$touch"
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
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Purchase Date</label>
                  <div>
                    <q-input v-model="model.dateofPurchaseStr" outlined stack-label hide-bottom-space mask="##/##/####" dense :error="v$.dateofPurchaseStr.$error" :error-message="v$.dateofPurchaseStr.$errors[0]?.$message" @blur="v$.dateofPurchaseStr.$touch">
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date v-model="model.dateofPurchaseStr" mask="MM/DD/YYYY" :options="optionsFn" @update:model-value="() => $refs.qDateProxy.hide()" />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
              </div>
              <div v-if="itemTypeText === 'Laptop' || itemTypeText === 'Desktop'" class="row q-col-gutter-x-md q-mb-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Operating System</label>
                  <div>
                    <q-input v-model="model.operatingSystem" outlined stack-label hide-bottom-space :dense="true" />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Processor Type</label>
                  <div>
                    <q-input v-model="model.processorType" outlined stack-label hide-bottom-space :dense="true" />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Memory / RAM</label>
                  <div>
                    <q-input v-model="model.memoryORRAM" outlined stack-label hide-bottom-space :dense="true" />
                  </div>
                </div>
              </div>
              <div v-if="itemTypeText === 'Laptop' || itemTypeText === 'Desktop' || itemTypeText === 'HardDisk' || itemTypeText === 'Pendrive'" class="row q-col-gutter-x-md q-mb-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Hard Drive / Storage Capacity</label>
                  <div>
                    <q-input v-model="model.hardDriveORStorageCapacity" outlined stack-label hide-bottom-space :dense="true" />
                  </div>
                </div>
                <div v-if="itemTypeText === 'Laptop' || itemTypeText === 'Desktop'" class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Graphics Card</label>
                  <div>
                    <q-input v-model="model.graphicsCard" outlined stack-label hide-bottom-space :dense="true" />
                  </div>
                </div>
                <div v-if="itemTypeText === 'Laptop' || itemTypeText === 'Desktop'" class="col-xs-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Virus Protection</label>
                  <div>
                    <q-input v-model="model.virusProtection" outlined stack-label hide-bottom-space :dense="true" />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-xs-12 col-sm-12 col-md-12 col-lg-6">
                  <div class="form-group">
                    <label class="label q-mb-xs text-black">Description</label>
                    <q-editor
                      v-model="model.description" :dense="$q.screen.lt.md"
                      :toolbar="[
                        [
                          {
                            label: $q.lang.editor.align,
                            icon: $q.iconSet.editor.align,
                            fixedLabel: true,
                            list: 'only-icons',
                            options: ['left', 'center', 'right', 'justify']
                          },
                        ],
                        ['bold', 'italic', 'strike', 'underline'],
                        // ['token', 'hr', 'link', 'custom_btn'],
                        [
                          {
                            label: $q.lang.editor.formatting,
                            icon: $q.iconSet.editor.formatting,
                            list: 'no-icons',
                            options: [
                              'p',
                              'h1',
                              'h2',
                              'h3',
                              'h4',
                              'h5',
                              'h6',
                              'code'
                            ]
                          },
                          'removeFormat'
                        ],
                        ['quote', 'unordered', 'ordered', 'outdent', 'indent'],

                        ['undo', 'redo'],
                        // ['viewsource']
                      ]"

                      :fonts="{
                        arial: 'Arial',
                        arial_black: 'Arial Black',
                        comic_sans: 'Comic Sans MS',
                        courier_new: 'Courier New',
                        impact: 'Impact',
                        lucida_grande: 'Lucida Grande',
                        times_new_roman: 'Times New Roman',
                        verdana: 'Verdana'
                      }"
                    />
                  </div>
                </div>
                <div class="col-xs-12 col-sm-12 col-md-12 col-lg-6">
                  <div class="form-group">
                    <label class="label q-mb-xs text-black">Note</label>
                    <q-input
                      v-model="model.notes" outlined stack-label type="textarea" hide-bottom-space :dense="true"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset>
              <legend>Assign Item to Employees</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-xs-12 col-sm-6 col-md-4">
                  <div>
                    <q-radio v-for="inventoryAssign in assignmentTypeList" :key="inventoryAssign.id" v-model="model.inventoryAssignId" class="q-mb-xs text-black text-black" checked-icon="o_task_alt" unchecked-icon="o_panorama_fish_eye" :val="inventoryAssign.id" :label="inventoryAssign.dropdownValue" :disable="isDisabled" />
                  </div>
                </div>
              </div>
              <div class="q-pa-md cardTable">
                <div class="q-gutter-y-md" />
                <div class="flex items-center justify-end q-mb-md">
                  <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAdd" />
                </div>
                <q-table
                  ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
                  no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]"
                >
                  <template #header="props">
                    <q-tr :props="props">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}
                        <span v-if="['employeeId', 'assignDateStr'].includes(col.name)" class="required">*</span>
                      </q-th>
                      <q-th auto-width class="text-center">Actions</q-th>
                    </q-tr>
                  </template>
                  <template #top-row>
                    <q-tr v-if="mode == 'add' && editingRow" class="row-highlight">
                      <q-td style="width: 300px; max-width: 300px; white-space: normal; overflow-wrap: break-word;">
                        <div>
                          <q-select
                            v-model="editingRow.employeeId" clearable use-input outlined stack-label hide-bottom-space :dense="true"
                            :options="employeeList" option-value="value" option-label="text" emit-value map-options :error="editingRowV$.employeeId.$error"
                            :error-message="editingRowV$.employeeId.$errors[0]?.$message" @filter="filterFn3" @blur="editingRowV$.employeeId.$touch"
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
                      </q-td>
                      <q-td>
                        <q-input
                          v-model="editingRow.assignDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense
                          :error="editingRowV$.assignDateStr.$error" :error-message="editingRowV$.assignDateStr.$errors[0]?.$message" @blur="editingRowV$.assignDateStr.$touch"
                        >
                          <template #append>
                            <q-icon name="o_calendar_month" class="cursor-pointer">
                              <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                <q-date v-model="editingRow.assignDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                              </q-popup-proxy>
                            </q-icon>
                          </template>
                        </q-input>
                      </q-td>
                      <q-td>
                        <q-input
                          v-model="editingRow.returnDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense
                          :error="editingRowV$.returnDateStr.$error" :error-message="editingRowV$.returnDateStr.$errors[0]?.$message" @blur="editingRowV$.returnDateStr.$touch"
                        >
                          <template #append>
                            <q-icon name="o_calendar_month" class="cursor-pointer">
                              <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                <q-date v-model="editingRow.returnDateStr" :options="disableBeforeAssignDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                              </q-popup-proxy>
                            </q-icon>
                          </template>
                        </q-input>
                      </q-td>
                      <q-td>
                        <div>
                          <q-input
                            v-model="editingRow.returnReson" outlined stack-label type="textarea" hide-bottom-space :dense="true" maxlength="128"
                          />
                        </div>
                      </q-td>
                      <q-td auto-width class="text-center">
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
                      <q-td class="text-left" style="width: 300px; max-width: 300px; white-space: normal; overflow-wrap: break-word;">
                        <q-select
                          v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                          v-model="editingRow.employeeId" clearable use-input outlined stack-label hide-bottom-space :dense="true"
                          :options="employeeList" option-value="value" option-label="text" emit-value map-options :error="editingRowV$.employeeId.$error"
                          :error-message="editingRowV$.employeeId.$errors[0]?.$message" @filter="filterFn3" @blur="editingRowV$.employeeId.$touch"
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
                        <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getEmployee(props.row.employeeId) }} </span>
                      </q-td>
                      <q-td class="text-left">
                        <q-input
                          v-if="mode == 'edit' && editingRow && props.row.id === activeRowId" v-model="editingRow.assignDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense
                          :error="editingRowV$.assignDateStr.$error" :error-message="editingRowV$.assignDateStr.$errors[0]?.$message" @blur="editingRowV$.assignDateStr.$touch"
                        >
                          <template #append>
                            <q-icon name="o_calendar_month" class="cursor-pointer">
                              <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                <q-date v-model="editingRow.assignDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                              </q-popup-proxy>
                            </q-icon>
                          </template>
                        </q-input>
                        <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.assignDateStr }} </span>
                      </q-td>
                      <q-td class="text-left">
                        <q-input
                          v-if="mode == 'edit' && editingRow && props.row.id === activeRowId" v-model="editingRow.returnDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense
                          :error="editingRowV$.returnDateStr.$error" :error-message="editingRowV$.returnDateStr.$errors[0]?.$message" @blur="editingRowV$.returnDateStr.$touch"
                        >
                          <template #append>
                            <q-icon name="o_calendar_month" class="cursor-pointer">
                              <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                <q-date v-model="editingRow.returnDateStr" :options="disableBeforeAssignDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                              </q-popup-proxy>
                            </q-icon>
                          </template>
                        </q-input>
                        <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.returnDateStr }} </span>
                      </q-td>
                      <q-td class="text-left">
                        <q-input
                          v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                          v-model="editingRow.returnReson" outlined stack-label type="textarea" hide-bottom-space :dense="true" maxlength="128"
                        />
                        <span v-else :class="props.row.deleted ? 'text-delete' : ''" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 40%;">{{ props.row.returnReson }} </span>
                      </q-td>
                      <q-td auto-width class="text-center">
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
import inventoryService from "modules/inventory/inventory.service";
import commonService from "services/common.service";
import employeesService from "src/modules/employee/employee.service";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import _ from "lodash";
import { notifySuccess, notifyError, zwConfirmLeave } from "assets/utils";
import useFilters from "composables/useFilters";
import { isDate } from "validators/zw_validators.js";

// Common variables
const { toDate } = useFilters();
const loading = ref(true);
const processing = ref(false);
const currentDate = new Date();
const mode = ref(null);
const editingRow = ref(null);
const selectedSiteId = ref(history.state?.siteId);

// Table variables
const tableRef = ref();
const rows = ref([]);
const activeRowId = ref(null);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "employeeId", label: "Assign To", field: "employeeId", align: "left", sortable: true },
  { name: "assignDateStr", label: "Assigned Date", field: "assignDateStr", align: "left", sortable: true },
  { name: "returnDateStr", label: "Return Date", field: "returnDateStr", align: "left", sortable: true },
  { name: "returnReson", label: "Reason", field: "returnReson", align: "left", sortable: true }
]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const props = defineProps({ id: { type: String, default: "" } });
const isDisabled = props.id !== "";
const readonlyDeviceType = props.id ? "readonly" : "";

// Define model values
const model = ref({
  name: "",
  inventorycode: "",
  assignDateStr: toDate(currentDate),
  returnDateStr: "",
  description: "",
  officeLocationId: "",
  active: true
});

const editingRowrules = {
  employeeId: { required: helpers.withMessage("Employee is Required", required) },
  assignDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    required: helpers.withMessage("Assigned Date is Required", required)
  },
  returnDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    returnDateStr: helpers.withMessage("End date must occur after the assign date", (value, { assignDateStr }) => {
      if (!value) return true;
      return new Date(value) >= new Date(assignDateStr);
    })
  }
};

const editingRowV$ = useVuelidate(editingRowrules, editingRow, { $lazy: true, $autoDirty: true });

// Validation rules
const rules = {
  itemTypeId: { required: helpers.withMessage("Device Type is required", required) },
  warrantyExpiryDateStr: { isDate: helpers.withMessage("Date is invalid", isDate) },
  dateofPurchaseStr: { isDate: helpers.withMessage("Date is invalid", isDate) },
  inventoryStatusId: { required: helpers.withMessage("Inventory Status is required", required) },
  inventorycode: { required: helpers.withMessage("Device No is required", required) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Computed property to get the identifiedUserType's text
const itemTypeText = computed(() => {
  const selectedOption = itemTypeList.value.find(
    item => item.value === model.value.itemTypeId
  );
  return selectedOption ? selectedOption.text : null;
});

// get get Requirement on edit mode
const getInventory = () => {
  loading.value = true;
  inventoryService.getInventory(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.warrantyExpiryDateStr = resp.warrantyExpiryDate ? toDate(resp.warrantyExpiryDate) : "";
    model.value.dateofPurchaseStr = resp.dateofPurchase ? toDate(resp.dateofPurchase) : "";
    // getAllRequirementGroupListForDropdown(resp.projectId);
    rows.value = resp.inventoryAssignmentList.map(item => ({
      ...item,
      employeeId: item.employeeId,
      assignDateStr: item.assignDate,
      returnDateStr: item.returnDate,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

// Get dropdown list for Address Type
const assignmentTypeList = ref([]);
function getAssignmentTypes () {
  commonService.getDropDown("Assignment Type").then((resp) => {
    assignmentTypeList.value = resp;
  });
}

// Get all type List
const itemTypeList = ref([]);
const options1 = ref([]);
function getAllItemType (name) {
  inventoryService.getAllItemType(name).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    itemTypeList.value = responseData;
    options1.value = responseData;
  });
}

// Search Type List for dropdown
function filterFn1 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      itemTypeList.value = options1.value;
    } else {
      itemTypeList.value = options1.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Function to fetch the inventory code based on selected item type
function fetchInventoryCode (itemTypeId) {
  if (!itemTypeId) {
    model.value.inventorycode = ""; // Clear field if no selection
    return;
  }
  inventoryService.getNextInventoryCode(itemTypeId).then((resp) => {
    model.value.inventorycode = resp.itemCode; // Set inventory code
  });
}

// Get all status List
const inventoryStatusList = ref([]);
const options2 = ref([]);
function getDropDownStatus (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    inventoryStatusList.value = responseData;
    options2.value = responseData;
  });
}

// Search status List for dropdown
function filterFn2 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      inventoryStatusList.value = options2.value;
    } else {
      inventoryStatusList.value = options2.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all employee list
const employeeList = ref([]);
const employeeListArr = ref([]);
const options3 = ref([]);
function getAllEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    employeeListArr.value = resp;
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    employeeList.value = responseData;
    options3.value = responseData;
  });
}

// Search  employee for dropdown
function filterFn3 (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = options3.value;
    } else {
      employeeList.value = options3.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all office Location list
const officeLocationList = ref([]);
const officeLocationFilter = ref([]);
function getAllOfficeLocationForDropDown (selectedSiteId, typeName) {
  commonService.getDropDownForSite(selectedSiteId, typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    officeLocationList.value = responseData;
    officeLocationFilter.value = responseData;
  });
}

function getAllOfficeLocationDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      officeLocationList.value = officeLocationFilter.value;
    } else {
      officeLocationList.value = officeLocationFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get row employee typee
function getEmployee (value) {
  if (value) {
    return employeeListArr.value.find((item) => item.id === value)?.person.fullName;
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
      assignDate: item.assignDate,
      returnDate: item.returnDate,
      returnReson: item.returnReson,
      flag: "Delete"
    });
  }
  activeRowId.value = item.id;
}

function onUndo (item) {
  item.deleted = false;
  activeRowId.value = null;
}

async function onSave () {
  if (mode.value === "edit") {
    if (!await editingRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveDialog = false;
    const rowIndex = rows.value.findIndex((row) => row.id === editingRow.value.id);
    if (rowIndex !== -1) {
      rows.value.splice(rowIndex, 1, {
        ...rows.value[rowIndex],
        employeeId: editingRow.value.employeeId,
        assignDateStr: editingRow.value.assignDateStr,
        returnDateStr: editingRow.value.returnDateStr,
        returnReson: editingRow.value.returnReson,
        flag: "Edit"
      });
      editingRow.value = null;
      mode.value = null;
      activeRowId.value = null;
    }
  } else if (mode.value === "add") {
    if (!await editingRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveDialog = false;
    let isDuplicate = 0;
    rows.value.forEach((item, index) => {
      if (item.employeeId.toLowerCase() === editingRow.value.employeeId.toLowerCase()) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const newRow = {
        id: uid(),
        employeeId: editingRow.value.employeeId,
        assignDateStr: editingRow.value.assignDateStr,
        returnDateStr: editingRow.value.returnDateStr,
        returnReson: editingRow.value.returnReson,
        flag: "New"
      };
      rows.value.unshift(newRow);
      mode.value = null;
      activeRowId.value = null;
    } else {
      notifyError({ message: "Duplicate Employee." });
    }
  }
}

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
    employeeId: "",
    assignDateStr: toDate(currentDate),
    returnDateStr: "",
    notes: ""
  };
  activeRowId.value = null;
}

function disableBeforeAssignDate (date) {
  // If no Start Date is set, allow all dates
  if (!editingRow.value.assignDateStr) {
    return true;
  }
  const start = new Date(editingRow.value.assignDateStr);
  const current = new Date(date);

  // Disable dates before the Start Date
  return current >= start;
}

// Submit form
const onSubmit = async () => {
  try {
    if ((mode.value === "add" || mode.value === "edit") && !await editingRowV$.value.$validate()) {
      notifyError({ message: "Please fill in all required fields" });
      return;
    }
    if ((mode.value === "edit" || mode.value === "add")) {
      return;
    }
    if (await v$.value.$validate()) {
      processing.value = true;
      model.value.inventoryAssignments = rows.value;
      inventoryService.saveInventorys(props.id, model.value).then((resp) => {
        notifySuccess({ message: "Inventory is saved successfully." });
        onDialogOK();
      }).finally(() => {
        processing.value = false;
      });
    }
  } catch (error) {
    console.error("Error in submitting the inventory:", error);
    notifyError({ message: "An error occurred while saving the inventory." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getInventory();
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  // getDropDown("Inventory Item Type");
  getAllOfficeLocationForDropDown(selectedSiteId.value, "Employee OrgLocation");
  getAllItemType();
  getDropDownStatus("Inventory Status");
  getAllEmployeesListForDropdown();
  getAssignmentTypes();
});

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
