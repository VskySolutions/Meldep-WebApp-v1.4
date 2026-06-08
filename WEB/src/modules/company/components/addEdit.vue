<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 90vw; max-width: 90vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }}
          {{ selectedCustomerType === 'business' ? 'Company' : (selectedCustomerType === 'joint family' ? 'Family' : 'Company') }}
        </div>
        <q-btn icon="o_close" class="close" color="white" flat round dense @click="onDialogClose()" />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>{{ selectedCustomerType === 'business' ? 'Company Info' : (selectedCustomerType === 'joint family' ? 'Family Info' : 'Company Info') }}</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs text-black">{{ selectedCustomerType === 'business' ? 'Company' : (selectedCustomerType === 'joint family' ? 'Family' : 'Company') }} Name<span class="required">*</span></div>
                  <q-input
                    v-model="model.name"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    maxlength="128"
                    :error="v$.name.$error"
                    :error-message="v$.name.$errors[0]?.$message"
                    @blur="v$.name.$touch"
                  />
                </div>
                <div class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs text-black">Website</div>
                  <q-input
                    v-model="model.website"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    maxlength="64"
                    type="url"
                    :error="v$.website.$error"
                    :error-message="v$.website.$errors[0]?.$message"
                    @blur="v$.website.$touch"
                  />
                </div>
                <div class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs text-black">Profile Link</div>
                  <q-input
                    v-model="model.profileLink"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    type="url"
                    :error="v$.profileLink.$error"
                    :error-message="v$.profileLink.$errors[0]?.$message"
                    @blur="v$.profileLink.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4 col-lg-4">
                  <formSingleSelectDropdown
                    v-model="model.businessTypeId"
                    label="Business Type"
                    :options="businessTypeForSiteIdDropdownSingleSelect.list.value"
                    :filter="businessTypeForSiteIdDropdownSingleSelect.filter"
                    :error="v$.businessTypeId.$error"
                    :error-message="v$.businessTypeId.$errors[0]?.$message"
                  />
                </div>
                <div class="col-12 col-md-4 col-lg-4">
                  <formSingleSelectDropdown
                    v-model="model.employeeId"
                    label="Primary Employee Person"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                    :error="v$.employeeId.$error"
                    :error-message="v$.employeeId.$errors[0]?.$message"
                  />
                </div>
                <div class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs text-black">Email</div>
                  <q-input
                    v-model="model.emailAddress"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    maxlength="128"
                    :error="v$.emailAddress.$error"
                    :error-message="v$.emailAddress.$errors[0]?.$message" @blur="v$.emailAddress.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs text-black">Alternate Email</div>
                  <div class="form-group">
                    <q-input
                      v-model="model.alternativeEmailAddress"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      maxlength="128"
                      :error="v$.alternativeEmailAddress.$error"
                      :error-message="v$.alternativeEmailAddress.$errors[0]?.$message"
                      @blur="v$.alternativeEmailAddress.$touch"
                    />
                  </div>
                </div>
                <div v-if="model.countryId === baseCountryId" class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs text-black">Phone Number<span class="required">*</span></div>
                  <q-input
                    v-model="model.phoneNumber"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    mask="(###)-###-####"
                    :error="v$.phoneNumber.$error"
                    :error-message="v$.phoneNumber.$errors[0]?.$message"
                    @blur="v$.phoneNumber.$touch"
                  />
                </div>
                <div v-else class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs text-black">Phone Number<span class="required">*</span></div>
                  <q-input
                    v-model="model.phoneNumber"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    mask="##########"
                    :error="v$.phoneNumber.$error"
                    :error-message="v$.phoneNumber.$errors[0]?.$message"
                    @blur="v$.phoneNumber.$touch"
                  />
                </div>
                <div v-if="model.countryId === baseCountryId" class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs text-black">Alternate Phone Number</div>
                  <div class="form-group">
                    <q-input
                      v-model="model.alternativePhoneNumber"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      mask="(###)-###-####"
                      :error="v$.alternativePhoneNumber.$error"
                      :error-message="v$.alternativePhoneNumber.$errors[0]?.$message"
                      @blur="v$.alternativePhoneNumber.$touch"
                    />
                  </div>
                </div>
                <div v-else class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs text-black">Alternate Phone Number</div>
                  <div class="form-group">
                    <q-input
                      v-model="model.alternativePhoneNumber"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      mask="##########"
                      :error="v$.alternativePhoneNumber.$error"
                      :error-message="v$.alternativePhoneNumber.$errors[0]?.$message"
                      @blur="v$.alternativePhoneNumber.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md">
                <div class="col-12 col-sm-12 col-md-12 col-lg-12">
                  <div class="q-mb-xs text-black"><label>Description</label></div>
                  <div class="form-group">
                    <q-editor
                      v-model="model.description"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset>
              <legend>Company Address</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs q-mt-sm text-black">Address 1<span class="required">*</span></div>
                  <q-input
                    v-model="model.addressLine1"
                    autogrow
                    hint="Street name/Building number."
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    maxlength="128"
                    rows="3"
                    :error="v$.addressLine1.$error"
                    :error-message="v$.addressLine1.$errors[0]?.$message"
                    @blur="v$.addressLine1.$touch"
                  />
                </div>
                <div class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs q-mt-sm text-black">Address 2</div>
                  <div class="form-group">
                    <q-input
                      v-model="model.addressLine2"
                      outlined
                      hint="Apartment/Unit/Suite"
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      maxlength="128"
                      rows="3"
                    />
                  </div>
                </div>
                <div class="col-12 col-md-4 col-lg-4 q-mt-sm">
                  <formSingleSelectDropdown
                    v-model="model.countryId"
                    label="Country"
                    :options="countryNameDropdownSingleSelect.list.value"
                    :filter="countryNameDropdownSingleSelect.filter"
                    :error="v$.countryId.$error"
                    :error-message="v$.countryId.$errors[0]?.$message"
                    @click="v$.countryId.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4 col-lg-4 q-mt-sm">
                  <formSingleSelectDropdown
                    v-model="model.stateProvinceId"
                    label="State"
                    :options="stateNameDropdownSingleSelect.list.value"
                    :filter="stateNameDropdownSingleSelect.filter"
                    :disable="!model.countryId"
                    :error="v$.stateProvinceId.$error"
                    :error-message="v$.stateProvinceId.$errors[0]?.$message"
                    @click="v$.stateProvinceId.$touch"
                  />
                </div>
                <div class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs text-black">City<span class="required">*</span></div>
                  <q-input
                    v-model="model.city"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    maxlength="128"
                    :error="v$.city.$error"
                    :error-message="v$.city.$errors[0]?.$message"
                    @blur="v$.city.$touch"
                  />
                </div>
                <div class="col-12 col-md-4 col-lg-4">
                  <div class="q-mb-xs text-black">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }}<span class="required">*</span></div>
                  <div v-if="model.countryId === baseCountryId" class="form-group">
                    <q-input v-model="model.zipCode" outlined stack-label hide-bottom-space :dense="true" mask="#####" :error="v$.zipCode.$error" :error-message="v$.zipCode.$errors[0]?.$message" @blur="v$.zipCode.$touch" />
                  </div>
                  <div v-else class="form-group">
                    <q-input v-model="model.zipCode" outlined stack-label hide-bottom-space :dense="true" mask="######" :error="v$.zipCode.$error" :error-message="v$.zipCode.$errors[0]?.$message" @blur="v$.zipCode.$touch" />
                  </div>
                </div>
              </div>
            </fieldset>
            <!-- <fieldset class="hidden">
              <legend>Other Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col">
                  <div class="q-mb-xs text-black">Start Date of Service Provided<span class="required">*</span></div>
                  <q-input v-model="model.startDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense :error="v$.startDateStr.$error" :error-message="v$.startDateStr.$errors[0]?.$message" @click="v$.startDateStr.$touch">
                    <template #append>
                      <q-icon name="o_calendar_month" class="cursor-pointer">
                        <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                          <q-date v-model="model.startDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
                <div class="col">
                  <div class="q-mb-xs text-black">Created Date<span class="required">*</span></div>
                  <q-input v-model="model.createdDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense :error="v$.createdDateStr.$error" :error-message="v$.createdDateStr.$errors[0]?.$message" @click="v$.createdDateStr.$touch">
                    <template #append>
                      <q-icon name="o_calendar_month" class="cursor-pointer">
                        <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                          <q-date v-model="model.createdDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col">
                  <div class="q-mb-xs text-black">Service Provided</div>
                  <q-input v-model="model.serviceProvidedDetails" outlined stack-label hide-bottom-space :dense="true" maxlength="64" type="text"
                  :error="v$.serviceProvidedDetails.$error" :error-message="v$.serviceProvidedDetails.$errors[0]?.$message" @blur="v$.serviceProvidedDetails.$touch" />
                </div>
                <div class="col">
                  <div class="q-mb-xs text-black">Product<span class="required">*</span></div>
                  <q-input v-model="model.productDetails" outlined stack-label hide-bottom-space :dense="true" maxlength="64" type="text" :error="v$.productDetails.$error" :error-message="v$.productDetails.$errors[0]?.$message" @blur="v$.productDetails.$touch"/>
                </div>
              </div>
            </fieldset> -->
            <fieldset>
              <!-- <legend>Add Contact</legend> -->
              <legend>{{ selectedCustomerType === 'business' ? 'Add Contact' : (selectedCustomerType === 'joint family' ? 'Add Family Contact' : 'Add Contact') }}</legend>
              <div class="flex items-center justify-end q-my-xs">
                <q-btn color="primary" icon="o_add" label="Add Contact" no-caps @click="onAdd" />
              </div>
              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                virtual-scroll
                bordered
                class="no-shadow"
                :loading="loading"
                :rows="rows"
                :columns="columns"
                row-key="id"
                separator="cell"
                no-data-label="No data available"
                binary-state-sort
                :rows-per-page-options="[20, 50, 100, 200, 500]"
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}<span v-if="['personId'].includes(col.name)" class="required">*</span></q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #top-row>
                  <q-tr v-if="mode == 'add' && editingRow" class="row-highlight">
                    <q-td>
                      <formSingleSelectDropdown
                        v-model="editingRow.personId"
                        :options="personNameDropdownSingleSelect.list.value"
                        :filter="personNameDropdownSingleSelect.filter"
                        :error="editingRowV$.personId.$error"
                        :error-message="editingRowV$.personId.$errors[0]?.$message"
                      >
                        <template #after>
                          <q-icon
                            name="fa-solid fa-user-plus"
                            color="primary"
                            class="cursor-pointer q-ml-sm"
                            @click="onAddPerson()"
                          >
                            <q-tooltip>Add new person</q-tooltip>
                          </q-icon>
                        </template>
                      </formSingleSelectDropdown>
                    </q-td>
                    <q-td>
                      {{ editingRow.firstName }}
                    </q-td>
                    <q-td>
                      {{ editingRow.lastName }}
                    </q-td>
                    <q-td>
                      {{ editingRow.emailAddress }}
                    </q-td>
                    <q-td>
                      {{ editingRow.phoneNumber }}
                    </q-td>
                    <q-td>
                      <q-input
                        v-model="editingRow.alternateEmail" outlined
                        hide-bottom-space :dense="true" maxlength="128" :error="editingRowV$.alternateEmail.$error" :error-message="editingRowV$.alternateEmail.$errors[0]?.$message" @blur="editingRowV$.alternateEmail.$touch"
                      />
                    </q-td>
                    <q-td>
                      <div v-if="model.countryId === baseCountryId" class="col">
                        <div class="form-group">
                          <q-input v-model="editingRow.alternatePhoneNumber" outlined stack-label hide-bottom-space :dense="true" maxlength="16" mask="(###)-###-####" />
                        </div>
                      </div>
                      <div v-else class="col">
                        <div class="form-group">
                          <q-input v-model="editingRow.alternatePhoneNumber" outlined stack-label hide-bottom-space :dense="true" maxlength="16" mask="##########" />
                        </div>
                      </div>
                      <!-- <q-input
                        v-model="editingRow.alternatePhoneNumber" outlined
                        hide-bottom-space :dense="true" maxlength="16" mask="(###)-###-####"
                      /> -->
                    </q-td>
                    <q-td auto-width class="text-center">
                      <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                        <q-tooltip>Save</q-tooltip>
                      </q-icon>
                      <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancelRow">
                        <q-tooltip>Cancel</q-tooltip>
                      </q-icon>
                    </q-td>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                    <q-td>
                      <formSingleSelectDropdown
                        v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                        v-model="editingRow.personId"
                        :options="personNameDropdownSingleSelect.list.value"
                        :filter="personNameDropdownSingleSelect.filter"
                        :error="editingRowV$.personId.$error"
                        :error-message="editingRowV$.personId.$errors[0]?.$message"
                        @update:model-value="onPersonSelected"
                      />
                      <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getPersonName(props.row.personId) }} </span>
                    </q-td>
                    <q-td class="text-left">
                      <!-- <span :class="props.row.deleted ? 'text-delete' : ''">{{ editingRow.firstName }} </span> -->
                      <!-- <span :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.firstName }} </span> -->
                      {{ (mode === 'edit' && editingRow && props.row.id === activeRowId) ? editingRow.firstName : props.row.firstName }}
                    </q-td>
                    <q-td class="text-left">
                      <!-- <span :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.lastName }} </span> -->
                      {{ (mode === 'edit' && editingRow && props.row.id === activeRowId) ? editingRow.lastName : props.row.lastName }}
                    </q-td>
                    <q-td class="text-left">
                      <!-- <span :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.emailAddress }} </span> -->
                      {{ (mode === 'edit' && editingRow && props.row.id === activeRowId) ? editingRow.emailAddress : props.row.emailAddress }}
                    </q-td>
                    <q-td class="text-left">
                      <!-- <span :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.phoneNumber }} </span> -->
                      {{ (mode === 'edit' && editingRow && props.row.id === activeRowId) ? editingRow.phoneNumber : props.row.phoneNumber }}
                    </q-td>
                    <q-td class="text-left">
                      <q-input
                        v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                        v-model="editingRow.alternateEmail" outlined
                        hide-bottom-space :dense="true" maxlength="128" :error="editingRowV$.alternateEmail.$error" :error-message="editingRowV$.alternateEmail.$errors[0]?.$message" @blur="editingRowV$.alternateEmail.$touch"
                      />
                      <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.alternateEmail }} </span>
                    </q-td>
                    <q-td class="text-left">
                      <div class="col">
                        <div class="form-group">
                          <!-- <div v-if="mode == 'edit' && editingRow && props.row.id === activeRowId && props.row.address.countryId === baseCountryId">
                            <q-input v-model="editingRow.alternatePhoneNumber" outlined hide-bottom-space :dense="true" maxlength="16" mask="(###)-###-####"/>
                          </div>
                          <div v-if="props.row.address.countryId !== baseCountryId">
                            <q-input v-model="editingRow.alternatePhoneNumber" outlined hide-bottom-space :dense="true" maxlength="16" mask="##########"/>
                          </div> -->
                          <!-- <q-input v-model="editingRow.alternatePhoneNumber" outlined stack-label hide-bottom-space :dense="true" maxlength="16" mask="(###)-###-####" /> -->
                        </div>
                      </div>
                      <q-input
                        v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                        v-model="editingRow.alternatePhoneNumber" outlined
                        hide-bottom-space :dense="true" maxlength="16" mask="(###) ### - ####"
                      />
                      <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.alternatePhoneNumber }} </span>
                    </q-td>
                    <q-td auto-width class="text-center">
                      <template v-if="mode == 'edit' && editingRow && props.row.id === activeRowId">
                        <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                          <q-tooltip>Save</q-tooltip>
                        </q-icon>
                        <!-- <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel"> -->
                        <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancelRow">
                          <q-tooltip>Cancel</q-tooltip>
                        </q-icon>
                      </template>
                      <template v-else>
                        <q-icon name="o_edit" size="xs" class="cursor-pointer q-mr-lg" @click="onEdit(props.row)">
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
            </fieldset>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <!-- <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" /> -->
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogClose" />
          <q-btn v-if="tab !== '4_tab'" color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, uid, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers, minLength, maxLength, email, url } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import { notifySuccess, notifyError, zwConfirmLeave } from "assets/utils";
import companyService from "modules/company/company.service";
import personService from "modules/person/person.service";
import _ from "lodash";
import useFilters from "composables/useFilters";
import editPerson from "modules/person/components/addEdit.vue";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

// Shared Dropdowns
import companyModule from "src/modules/company/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import commonModule from "src/modules/common/utils/dropdowns.js";
import personModule from "src/modules/person/utils/dropdowns.js";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// Shared Person Dialogs
// import {
//   initPersonDialogs,
//   onPersonAdd
// } from "src/modules/person/utils/dialogs.js";

// ----------------------------------------------------------------------------------------------------------------
// define emits
// ----------------------------------------------------------------------------------------------------------------
const $emit = defineEmits(["hide", "ok"]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------

const props = defineProps({ id: { type: String, default: "" }, selectedCustomerType: { type: String, default: "" } });

// select customer type from add customer form
const selectedCustomerType = props.selectedCustomerType;

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const $q = useQuasar();
const loading = ref(false);
const processing = ref(false);
const baseCountryId = process.env.BASE_COUNTRY_ID;
const mode = ref(null);
// Common variables
const { toDate } = useFilters();
const selectedSiteId = ref(history.state?.siteId);
const { fonts, toolbar } = getEditorConfig($q);
const rowCounter = ref(0); // Initialize the counter

const editingRow = ref({
  personId: "",
  alternateEmail: ""
});
const activeRowId = ref(null);
const rows = ref([]);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "personId", label: "Person Name", field: "personId", align: "left", sortable: true },
  { name: "firstName", label: "First Name", field: "firstName", align: "left", sortable: true },
  { name: "lastName", label: "Last Name", field: "lastName", align: "left", sortable: true },
  { name: "emailAddress", label: "Email Address", field: "emailAddress", align: "left", sortable: true },
  { name: "phoneNumber", label: "Phone Number", field: "Phonenumber", align: "left", sortable: true },
  { name: "alternateEmail", label: "Alt. Email", field: "alternateEmail", align: "left", sortable: true },
  { name: "alternatePhoneNumber", label: "Alt. Phone Number", field: "alternatePhoneNumber", align: "left", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  name: "",
  employeeId: "",
  phoneNumber: "",
  emailAddress: "",
  website: "",
  alternativeEmailAddress: "",
  alternativePhoneNumber: "",
  businessTypeId: "",
  city: "",
  countryId: baseCountryId,
  stateProvinceId: null,
  zipCode: "",
  startDateStr: "",
  serviceProvidedDetails: "",
  productDetails: "",
  description: "",
  profileLink: "",
  active: true
});

// ----------------------------------------------------------------------------------------------------------------
// get Company details
// ----------------------------------------------------------------------------------------------------------------

const getCompany = () => {
  loading.value = true;
  companyService.getCompanyDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    rowCounter.value += 1;
    // getAllPersonListForDropdown(rowCounter.value);
    personNameDropdownSingleSelect.load(selectedSiteId.value);
    // addressRow.value = _.cloneDeep(resp.address);
    model.value.businessTypeId = resp.businessType.id;
    model.value.addressLine1 = resp.address.addressLine1;
    model.value.addressLine2 = resp.address.addressLine2;
    model.value.countryId = resp.address.addressCountry.id;
    model.value.stateProvinceId = resp.address.addressStateProvince.id;
    model.value.city = resp.address.city;
    model.value.zipCode = resp.address.zipCode;
    model.value.startDateStr = resp.serviceProviderDate ? toDate(resp.serviceProviderDate) : "";
    // model.value.createdDateStr = resp.comapnyCreatedDate ? toDate(resp.comapnyCreatedDate) : "";
    model.value.employeeId = resp.employee.id;
    model.value.description = resp.description ? resp.description : "";
    rows.value = resp.companyContacts.map(contact => ({
      ...contact,
      editing: false,
      rowCounter: rowCounter.value,
      personId: contact.person.id,
      firstName: contact.person.firstName,
      lastName: contact.person.lastName,
      emailAddress: contact.person.primaryEmailAddress,
      phoneNumber: contact.person.primaryPhoneNumber,
      alternateEmail: contact.alternateEmail,
      alternatePhoneNumber: contact.alternatePhoneNumber,
      companyId: model.value.companyId,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

// ----------------------------------------------------------------------------------------------------------------
// get Person details
// ----------------------------------------------------------------------------------------------------------------

function getPersonbyId (PersonId) {
  personService.getPerson(PersonId).then((resp) => {
    editingRow.value.personId = PersonId;
    editingRow.value.firstName = resp.firstName;
    editingRow.value.middleName = resp.middleName;
    editingRow.value.lastName = resp.lastName;
    editingRow.value.emailAddress = resp.primaryEmailAddress;
    editingRow.value.phoneNumber = resp.primaryPhoneNumber;
    // editingRow.value.countryId = resp.address.countryId;
  });
}

// ----------------------------------------------------------------------------------------------------------------
// Validation rules
// ----------------------------------------------------------------------------------------------------------------

// const zipCodeRule = helpers.withMessage(baseCountryId === model.value.countryId ? "Zip code is required" : "Pin code is required", required);
let maxLengthCountry = baseCountryId === model.value.countryId ? "14" : "10";
let maxLengthZip = baseCountryId === model.value.countryId ? "5" : "6";
let rules = {
  name: { required: helpers.withMessage("Company name is required", required), minLength: minLength(1), maxLength: maxLength(100) },
  website: {
    validUrl: helpers.withMessage(
      "Invalid URL",
      value => !value || /^https:\/\/[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})+$/.test(value)
    )
  },
  profileLink: {
    validUrl: helpers.withMessage(
      "Invalid URL",
      value => !value || /^https:\/\/[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})+$/.test(value)
    )
  },
  businessTypeId: { required: helpers.withMessage("Business type is required", required) },
  phoneNumber: { required: helpers.withMessage("Phone number is required", required), minLength: minLength(maxLengthCountry), maxLength: maxLength(maxLengthCountry) },
  addressLine1: { required: helpers.withMessage("Address is required", required) },
  city: { required: helpers.withMessage("City is required", required) },
  countryId: { required: helpers.withMessage("Country is required", required) },
  stateProvinceId: { required: helpers.withMessage("State is required", required) },
  zipCode: { required: helpers.withMessage(maxLengthZip === "5" ? "Zip code is required" : "Pin code is required", required), minLength: minLength(maxLengthZip), maxLength: maxLength(maxLengthZip) },
  emailAddress: { email: helpers.withMessage("Invalid email", email) },
  alternativeEmailAddress: { email: helpers.withMessage("Invalid email", email) },
  alternativePhoneNumber: { minLength: minLength(maxLengthCountry), maxLength: maxLength(maxLengthCountry) },
  employeeId: { required: helpers.withMessage("Primary employee is required", required) }
};

let v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const editingRowrules = {
  personId: { required: helpers.withMessage("Person Name is required", required) },
  alternateEmail: { email: helpers.withMessage("Invalid email", email) }
};
const editingRowV$ = useVuelidate(editingRowrules, editingRow, { $lazy: true, $autoDirty: true });

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

// initPersonDialogs(activeRowId);

// ------------------------------------------------------------------------------------
// All Dropdowns
// ------------------------------------------------------------------------------------

const {
  businessTypeForSiteIdDropdownSingleSelect
} = companyModule();

const { activeEmployeesDropdownSingleSelect } = employeeModule();
const { personNameDropdownSingleSelect } = personModule();

const {
  countryNameDropdownSingleSelect,
  stateNameDropdownSingleSelect
 } = commonModule();

function onPersonSelected (personId) {
  if (personId) {
    getPersonbyId(personId);
  }
}

// watch(() => editingRow.value?.personId, (newValue, oldValue) => {
//   if (newValue && newValue !== oldValue) {
//     getPersonbyId(newValue);
//   }
// });

// let isSaveDialog = false;
// let isConfirmSaveDialog = false;
// function onAdd () {
//   mode.value = "add";
//   rowCounter.value += 1; // Increment the counter
//   editingRow.value = {
//     personId: "",
//     firstName: "",
//     lastName: "",
//     phoneNumber: "",
//     emailAddress: "",
//     alternateEmail: "",
//     address: "",
//     companyId: model.value.id
//   };
//   activeRowId.value = null;
//   getAllPersonListForDropdown(rowCounter.value);
// }

let isSaveDialog = false;
let isConfirmSaveDialog = false;
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
  rowCounter.value += 1; // Increment the counter
  editingRow.value = {
    personId: "",
    firstName: "",
    lastName: "",
    phoneNumber: "",
    emailAddress: "",
    alternateEmail: "",
    address: "",
    companyId: model.value.id
  };
  activeRowId.value = null;
  // getAllPersonListForDropdown(rowCounter.value);
  personNameDropdownSingleSelect.load(selectedSiteId.value);
}

// function onEdit (item) {
//   mode.value = "edit";
//   editingRow.value = _.cloneDeep(item);
//   activeRowId.value = item.id;
//   getAllPersonListForDropdown(rowCounter.value);
// }

function onEdit (item) {
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
  // getAllPersonListForDropdown(rowCounter.value);
  personNameDropdownSingleSelect.load(selectedSiteId.value);
}

// function onCancel () {
//   mode.value = null;
//   // editingRow.value = null;
//   activeRowId.value = null;
// }

function onCancelRow () {
  isConfirmSaveDialog = false;
  mode.value = null;
  // editingRow.value = null;
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

// Create person
const onAddPerson = () => {
  $q.dialog({
    component: editPerson,
    componentProps: {
      siteId: selectedSiteId.value
    }
  }).onOk((newPersonId) => {
    // Refresh person list
    personNameDropdownSingleSelect.load(selectedSiteId.value);
    setTimeout(() => {
      editingRow.value.personId = newPersonId;
    }, 100);
  });
};

async function onSave () {
  if (!await editingRowV$.value.$validate()) {
    return;
  }
  isConfirmSaveDialog = false;
  if (mode.value === "edit") {
    if (!await editingRowV$.value.$validate()) {
      return;
    }
    let isDuplicatePerson = 0;
    rows.value.forEach((item, index) => {
      if (item.personId === editingRow.value.personId && item.id !== editingRow.value.id) {
        isDuplicatePerson = 1;
      }
    });
    const rowIndex = rows.value.findIndex((row) => row.id === editingRow.value.id);
    if (isDuplicatePerson === 0) {
      if (rowIndex !== -1) {
        rows.value.splice(rowIndex, 1, {
          ...rows.value[rowIndex],
          personId: editingRow.value.personId,
          firstName: editingRow.value.firstName,
          lastName: editingRow.value.lastName,
          emailAddress: editingRow.value.emailAddress,
          alternateEmail: editingRow.value.alternateEmail,
          alternatePhoneNumber: editingRow.value.alternatePhoneNumber,
          phoneNumber: editingRow.value.phoneNumber,
          address: editingRow.value.address,
          // countryId: editingRow.value.address.countryId,
          rowCounter: rowCounter.value,
          personName: getPersonName(editingRow.value.rowCounter),
          flag: "Edit"
        });
        // editingRow.value = null;
        mode.value = null;
        activeRowId.value = null;
      }
    } else {
      notifyError({ message: "Duplicate Person." });
    }
  } else if (mode.value === "add") {
    // check duplicate row
    let isDuplicate = 0;
    rows.value.forEach((item, index) => {
      if (item.personId === editingRow.value.personId) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const newRow = {
        id: uid(),
        personId: editingRow.value.personId,
        firstName: editingRow.value.firstName,
        lastName: editingRow.value.lastName,
        emailAddress: editingRow.value.emailAddress,
        alternateEmail: editingRow.value.alternateEmail,
        alternatePhoneNumber: editingRow.value.alternatePhoneNumber,
        phoneNumber: editingRow.value.phoneNumber,
        address: editingRow.value.address,
        // countryId: editingRow.value.address.countryId,
        rowCounter: rowCounter.value,
        personName: getPersonName(editingRow.value.personId, rowCounter.value),
        flag: "New"
      };

      rows.value.unshift(newRow);
      mode.value = null;
      activeRowId.value = null;
    } else {
      notifyError({ message: "Duplicate Person." });
    }
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
      firstName: item.firstName,
      lastName: item.lastName,
      emailAddress: item.emailAddress,
      alternateEmail: item.alternateEmail,
      alternatePhoneNumber: item.alternatePhoneNumber,
      phoneNumber: item.phoneNumber,
      address: item.address,
      flag: "Delete"
    });
  }
  activeRowId.value = item.id;
}

function onUndo (item) {
  item.deleted = false;
  activeRowId.value = null;
}

async function onSubmit () {
  try {
  // if (!await v$.value.$validate() && editingRowV$.value.$validate()) {
  //   return;
  // }
    if (!await v$.value.$validate()) {
      return;
    }
    processing.value = true;
    if (rows.value.length === 0) {
      notifyError({ message: "Add at least one contact." });
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

    const payload = {
      id: model.value.id,
      employeeId: model.value.employeeId,
      name: model.value.name,
      emailAddress: model.value.emailAddress,
      phoneNumber: model.value.phoneNumber,
      alternativePhoneNumber: model.value.alternativePhoneNumber,
      website: model.value.website,
      alternativeEmailAddress: model.value.alternativeEmailAddress,
      businessTypeId: model.value.businessTypeId,
      active: model.value.active,
      countryId: model.value.countryId,
      addressLine1: model.value.addressLine1,
      addressLine2: model.value.addressLine2,
      city: model.value.city,
      stateProvinceId: model.value.stateProvinceId,
      zipCode: model.value.zipCode,
      addressId: model.value.addressId,
      clientAdvisorIds: model.value.clientAdvisorIds,
      serviceProvidedDetails: model.value.serviceProvidedDetails,
      statusId: model.value.statusId,
      profileLink: model.value.profileLink,
      description: model.value.description,
      companyContactModel: rows.value,
      siteId: selectedSiteId.value
    };
    companyService.saveCompanyAndContacts(props.id, payload).then(resp => {
      notifySuccess({ message: "Company saved successfully." });
      $emit("ok");
      $emit("hide");
    // router.push({ name: "Company", params: {} });
    });
  } catch (error) {
    console.error("Error in submitting the company:", error);
    notifyError({ message: "An error occurred while saving the company." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

function getPersonName (value, counter) {
  if (value) {
    // return personNameDropdownSingleSelect.list.value.find((item) => item.value === value)?.text;
    const found = personNameDropdownSingleSelect.list.value.find(item => item.value === value);
    return found?.text || "";
  }
}

if (editingRow.value.personId !== null) {
  watch(() => editingRow.value.personId, (newValue, oldValue) => {
    if (newValue) {
      getPersonbyId(newValue);
    }
  }, { immediate: false });
}

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getCompany();
  }
}, { immediate: true });

watch(() => model.value.countryId, (newValue, oldValue) => {
  model.value.stateProvinceId = newValue !== oldValue && oldValue !== null ? "" : model.value.stateProvinceId;
  if (newValue) {
    maxLengthCountry = baseCountryId === model.value.countryId ? "14" : "10";
    maxLengthZip = baseCountryId === model.value.countryId ? "5" : "6";
    stateNameDropdownSingleSelect.load(newValue);
    rules = {
      name: { required: helpers.withMessage("Company name is required", required), minLength: minLength(1), maxLength: maxLength(100) },
      website: {
        // required: helpers.withMessage("Website is required", required),
        url: helpers.withMessage("Invalid URL", url)
      },
      profileLink: {
        url: helpers.withMessage("Invalid URL", url)
      },
      businessTypeId: { required: helpers.withMessage("Business type is required", required) },
      phoneNumber: { required: helpers.withMessage("Phone number is required", required), minLength: minLength(maxLengthCountry), maxLength: maxLength(maxLengthCountry) },
      addressLine1: { required: helpers.withMessage("Address is required", required) },
      city: { required: helpers.withMessage("City is required", required) },
      countryId: { required: helpers.withMessage("Country is required", required) },
      stateProvinceId: { required: helpers.withMessage("State is required", required) },
      zipCode: { required: helpers.withMessage(maxLengthZip === "5" ? "Zip code is required" : "Pin code is required", required), minLength: minLength(maxLengthZip), maxLength: maxLength(maxLengthZip) },
      emailAddress: { email: helpers.withMessage("Invalid email", email) },
      alternativeEmailAddress: { email: helpers.withMessage("Invalid email", email) },
      alternativePhoneNumber: { minLength: minLength(maxLengthCountry), maxLength: maxLength(maxLengthCountry) },
      employeeId: { required: helpers.withMessage("Primary employee is required", required) }
    };
    // Validate rules
    v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });
  }
}, { immediate: false });

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  businessTypeForSiteIdDropdownSingleSelect.load(selectedSiteId.value, "Business Type");
  countryNameDropdownSingleSelect.load();
  personNameDropdownSingleSelect.load(selectedSiteId.value);
  activeEmployeesDropdownSingleSelect.load(selectedSiteId.value);
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
