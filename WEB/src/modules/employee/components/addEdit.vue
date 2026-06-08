<template>
  <q-dialog ref="dialogRef" class="customDialog" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 80vw; max-width: 80vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Employee</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense @click="onDialogClose()" />
      </q-card-section>
      <q-separator />
      <!-- <q-card-section class="card-header with-tools"> -->
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md']">
          <!-- <div :class="['q-pa-md', readonlyEmployee != '' ? 'edit_employee' : '']"> -->
          <div class="q-gutter-y-md">
            <!-- <q-card class="card-header with-tools headerBasic"> -->
            <q-card>
              <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator inline-label mobile-arrows>
                <q-tab name="1_tab" label="Employee Info." class="q-px-lg q-mr-md" />
                <q-tab name="2_tab" label="Employment Details" class="q-px-lg" :disable="disableTab" />
                <q-tab name="3_tab" label="Other Information" class="q-px-lg" :disable="disableTab" />
              </q-tabs>
              <q-separator />
              <q-tab-panels v-model="tab" animated>
                <q-tab-panel name="1_tab">
                  <fieldset>
                    <legend>Basic Info</legend>
                    <div class="absolute-top-right q-mr-xl q-mt-md" style="line-height: 1.2em;"><q-icon v-if="readonlyEmployee" name="fa-regular fa-pen-to-square" size="md" class="cursor-pointer" @click="onEdit(model.personId)" /></div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-md-4">
                        <formSingleSelectDropdown
                          v-model="model.personId"
                          label="Person Name"
                          :options="personNameDropdownSingleSelect.list.value"
                          :filter="personNameDropdownSingleSelect.filter"
                          :readonly="readonlyEmployee!= '' ? '' : 'readonlyEmployee'"
                          :error="v$.personId.$error"
                          :error-message="v$.personId.$errors[0]?.$message"
                        >
                         <template #after>
                            <q-icon
                              v-if="!readonlyEmployee"
                              name="fa-solid fa-user-plus"
                              color="primary"
                              class="cursor-pointer q-ml-sm"
                              @click="onAddPerson()"
                            >
                              <q-tooltip>Add new person</q-tooltip>
                            </q-icon>
                          </template>
                        </formSingleSelectDropdown>
                      </div>
                      <div class="col-12 col-md-3">
                        <div v-if="model.personId">
                          <div class="q-mb-xs text-black">Office Email <span class="required">*</span></div>
                          <div>
                            <q-input
                              v-model="model.officialEmail" outlined stack-label hide-bottom-space :dense="true"
                              :error="v$.officialEmail.$error" :error-message="v$.officialEmail.$errors[0]?.$message" @click="v$.officialEmail.$touch"
                            />
                        </div>
                        </div>
                      </div>
                      <div class="col-12 col-md-3">
                        <div v-if="model.personId">
                          <div class="q-mb-xs text-black">Employee Code <span class="required">*</span></div>
                          <div>
                            <q-input
                              v-model="model.employeeCode" outlined stack-label hide-bottom-space :dense="true" :readonly="!['hr', 'admin', 'site-super-admin'].some(r => roles.includes(r))"
                              :error="v$.employeeCode.$error" :error-message="v$.employeeCode.$errors[0]?.$message" @click="v$.employeeCode.$touch"
                            />
                        </div>
                        </div>
                      </div>
                    </div>
                    <div v-if="model.personId">
                      <div class="row q-col-gutter-x-md q-mb-md">
                        <div class="col-12 col-sm-6 col-md-3">
                          <div class="q-mb-xs text-black">First Name : {{ model.firstName }}</div>
                        </div>
                        <div class="col-12 col-sm-6 col-md-3">
                          <div class="q-mb-xs text-black">Middle Name : {{ model.middleName }}</div>
                        </div>
                        <div class="col-12 col-sm-6 col-md-3">
                          <div class="q-mb-xs text-black">Last Name : {{ model.lastName }}</div>
                        </div>
                      </div>
                      <div class="row q-col-gutter-x-md q-mb-md">
                        <div class="col-12 col-sm-6 col-md-3">
                          <div class="q-mb-xs text-black">Email Address : {{ model.primaryEmailAddress }}</div>
                        </div>
                        <div class="col-12 col-sm-6 col-md-3">
                          <div class="q-mb-xs text-black">Phone Number : {{ model.primaryPhoneNumber }}</div>
                        </div>
                      </div>
                      <div class="row q-col-gutter-x-md q-mb-md">
                        <div class="col-12 col-sm-6 col-md-3">
                          <div class="q-mb-xs text-black">Gender : {{ model.genderId }}</div>
                        </div>
                        <div class="col-12 col-sm-6 col-md-3">
                          <div class="q-mb-xs text-black">Profile Picture : </div>
                          <div class="row justify-center">
                            <img :src="model.virtualPath" alt="" style="width: 30%;">
                          </div>
                        </div>
                      </div>
                    </div>
                    <!-- <div class="col-3">
                          <div class="q-mb-xs text-black ">Profile Picture : <img :src="baseURL + model.virtualPath" alt="" style="width: 20%"/></div>
                        </div>
                      </div> -->
                  </fieldset>
                  <fieldset v-if="model.personId" class="q-mt-lg">
                    <legend>Primary Address Info</legend>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Country : {{ model.country }}</div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">State :  {{ model.stateProvinceId }}</div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Address Type : {{ model.addressTypeId }}</div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Address 1 :  {{ model.addressLine1 }}</div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Address 2 : {{ model.addressLine2 }}</div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">City :  {{ model.city }} </div>
                      </div>
                      <div class="q-mb-xs text-black">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }} : {{ model.zipCode }}</div>
                    </div>
                  </fieldset>
                  <fieldset v-if="model.personId" class="q-mt-lg">
                    <legend>Current Address</legend>
                    <div v-if="model.addressLine1" class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-3">
                        <q-checkbox v-model="model.sameASPermanentAddress" label="Same as the permanent address" :dense="true" />
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-3">
                        <div v-if="model.sameASPermanentAddress" class="q-mb-xs text-black">Address 1 :  {{ model.addressLine1 }}</div>
                        <div v-else>
                          <div class="q-mb-xs text-black">Address 1 <span class="required">*</span></div>
                          <div>
                            <q-input
                              v-model="currentaddrmodel.addressLine1" outlined stack-label hide-bottom-space :dense="true" hint="Street name/Building number."
                              :error="crtaddrv$.addressLine1.$error" :error-message="crtaddrv$.addressLine1.$errors[0]?.$message" @click="crtaddrv$.addressLine1.$touch"
                            />
                        </div>
                        </div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div v-if="model.sameASPermanentAddress" class="q-mb-xs text-black">Address 2 : {{ model.addressLine2 }}</div>
                        <div v-else>
                          <div class="q-mb-xs text-black">Address 2</div>
                          <div>
                            <q-input v-model="currentaddrmodel.addressLine2" outlined stack-label hide-bottom-space :dense="true" hint="Apartment/Unit/Suite" />
                          </div>
                        </div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div v-if="model.sameASPermanentAddress" class="q-mb-xs text-black">Country :  {{ model.country }}</div>
                        <div v-else>
                          <formSingleSelectDropdown
                            v-model="currentaddrmodel.countryId"
                            label="Country"
                            :options="countryNameDropdownSingleSelect.list.value"
                            :filter="countryNameDropdownSingleSelect.filter"
                            :error="crtaddrv$.countryId.$error"
                            :error-message="crtaddrv$.countryId.$errors[0]?.$message"
                            @click="crtaddrv$.countryId.$touch"
                          />
                        </div>
                      </div>
                    </div>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-3">
                        <div v-if="model.sameASPermanentAddress" class="q-mb-xs text-black">State :  {{ model.stateProvinceId }}</div>
                        <div v-else>
                          <formSingleSelectDropdown
                            v-model="currentaddrmodel.stateProvinceId"
                            label="State"
                            :options="stateNameDropdownSingleSelect.list.value"
                            :filter="stateNameDropdownSingleSelect.filter"
                            :disable="!currentaddrmodel.countryId"
                            :error="crtaddrv$.stateProvinceId.$error"
                            :error-message="crtaddrv$.stateProvinceId.$errors[0]?.$message"
                            @click="crtaddrv$.stateProvinceId.$touch"
                          />
                        </div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div v-if="model.sameASPermanentAddress" class="q-mb-xs text-black">City :  {{ model.city }} </div>
                        <div v-else>
                          <div class="q-mb-xs text-black">City <span class="required">*</span></div>
                          <div>
                            <q-input
                              v-model="currentaddrmodel.city" outlined stack-label hide-bottom-space :dense="true"
                              :error="crtaddrv$.city.$error" :error-message="crtaddrv$.city.$errors[0]?.$message" @click="crtaddrv$.city.$touch"
                            />
                        </div>
                        </div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div v-if="model.sameASPermanentAddress" class="q-mb-xs text-black">{{ baseCountryId == model.countryId ? 'Zip Code' : 'Pin code' }} : {{ model.zipCode }}</div>
                        <div v-else>
                          <div class="q-mb-xs text-black">{{ baseCountryId == currentaddrmodel.countryId ? 'Zip Code' : 'Pin code' }} <span class="required">*</span></div>
                          <div>
                            <q-input
                              v-model="currentaddrmodel.zipCode" outlined hide-bottom-space :dense="true" :mask="baseCountryId == currentaddrmodel.countryId ? '#####' : '######'"
                              :error="crtaddrv$.zipCode.$error" :error-message="crtaddrv$.zipCode.$errors[0]?.$message" @click="crtaddrv$.zipCode.$touch"
                            />
                          </div>
                        </div>
                      </div>
                    </div>
                  </fieldset>
                  <fieldset v-if="model.personId" class="q-mt-lg">
                    <legend>Employee Activation</legend>
                    <div class="col-lg-6 col-md-6 col-sm-6 col-xs-6">
                      <div class="q-mb-xs q-mt-md text-black">Set As Active?</div>
                      <q-checkbox v-model="model.active" label="Active" :dense="true" />
                    </div>
                  </fieldset>
                </q-tab-panel>
                <q-tab-panel name="2_tab">
                  <fieldset class="q-mb-lg">
                    <legend>Employment Type</legend>
                    <div class="flex items-center justify-end q-mb-md">
                      <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAdd" />
                    </div>
                    <q-table
                      ref="tableRef"
                      v-model:pagination="pagination"
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
                        <q-tr :props="props">
                          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}
                            <span v-if="['employeeTypeId', 'startDate'].includes(col.name)" class="required">*</span>
                          </q-th>
                          <q-th auto-width class="text-center">Actions</q-th>
                        </q-tr>
                      </template>
                      <template #top-row>
                        <q-tr v-if="mode == 'add' && editingRow" class="row-highlight">
                          <q-td>
                            <div>
                              <formSingleSelectDropdown
                                v-model="editingRow.employeeTypeId"
                                :options="employeeTypeDropdownSingleSelect.list.value"
                                :filter="employeeTypeDropdownSingleSelect.filter"
                                :error="editingRowV$.employeeTypeId.$error"
                                :error-message="editingRowV$.employeeTypeId.$errors[0]?.$message"
                              />
                            </div>
                          </q-td>
                          <q-td>
                            <formDate
                              v-model="editingRow.startDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingRowV$.startDateStr.$error"
                              :error-message="editingRowV$.startDateStr.$errors[0]?.$message"
                              :onBlur="() => editingRowV$.startDateStr.$touch()"
                              @update:model-value="calculateTypeDuration"
                            />
                          </q-td>
                          <q-td>
                            <formDate
                              v-model="editingRow.endDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingRowV$.endDateStr.$error"
                              :error-message="editingRowV$.endDateStr.$errors[0]?.$message"
                              :onBlur="() => editingRowV$.endDateStr.$touch()"
                              @update:model-value="calculateTypeDuration"
                            />
                          </q-td>
                          <q-td>
                            <q-input
                              v-model="editingRow.duration" outlined
                              hide-bottom-space :dense="true" readonly
                            />
                          </q-td>
                          <q-td>
                            <q-input
                              v-model="editingRow.note" outlined
                              hide-bottom-space :dense="true" maxlength="300"
                            />
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
                          <q-td class="text-left" width="15%">
                            <formSingleSelectDropdown
                              v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                              v-model="editingRow.employeeTypeId"
                              :options="employeeTypeDropdownSingleSelect.list.value"
                              :filter="employeeTypeDropdownSingleSelect.filter"
                              :error="editingRowV$.employeeTypeId.$error"
                              :error-message="editingRowV$.employeeTypeId.$errors[0]?.$message"
                            />
                            <!-- <q-select
                              v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                              v-model="editingRow.employeeTypeId" clearable use-input outlined stack-label hide-bottom-space :dense="true"
                              :options="employeeTypeList" option-value="value" option-label="text" emit-value map-options :error="editingRowV$.employeeTypeId.$error"
                              :error-message="editingRowV$.employeeTypeId.$errors[0]?.$message" @filter="getAllEmploymentTypeListForFilter" @blur="editingRowV$.employeeTypeId.$touch"
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
                            </q-select> -->
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getEmployeeType(props.row.employeeTypeId) }} </span>
                          </q-td>
                          <q-td class="text-left" width="15%">
                            <formDate
                              v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                              v-model="editingRow.startDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingRowV$.startDateStr.$error"
                              :error-message="editingRowV$.startDateStr.$errors[0]?.$message"
                              :onBlur="() => editingRowV$.startDateStr.$touch()"
                              @update:model-value="calculateTypeDuration"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.startDateStr }} </span>
                          </q-td>
                          <q-td class="text-left" width="15%">
                            <formDate
                              v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                              v-model="editingRow.endDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingRowV$.endDateStr.$error ||
                                (showAddButtonEndDateError && props.row.id === activeRowId)"
                              :error-message="(showAddButtonEndDateError && props.row.id === activeRowId)
                                ? 'Employment Type End Date is required.'
                                : editingRowV$.endDateStr.$errors[0]?.$message"
                              :onBlur="() => editingRowV$.endDateStr.$touch()"
                              @update:model-value="
                                showAddButtonEndDateError = false;
                                calculateTypeDuration();
                              "
                            />
                            <!-- <q-input
                              v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                              v-model="editingRow.endDateStr"
                              outlined
                              stack-label
                              hide-bottom-space
                              mask="##/##/####"
                              dense
                              :error="editingRowV$.endDateStr.$error ||
                                (showAddButtonEndDateError && props.row.id === activeRowId)"
                              :error-message="(showAddButtonEndDateError && props.row.id === activeRowId)
                                ? 'Employment Type End Date is required.'
                                : editingRowV$.endDateStr.$errors[0]?.$message"
                              @blur="editingRowV$.endDateStr.$touch"
                              @update:model-value="
                                showAddButtonEndDateError = false;
                                calculateTypeDuration();
                              "
                            >
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy transition-show="scale" transition-hide="scale">
                                    <q-date
                                      v-model="editingRow.endDateStr"
                                      :options="disableBeforeStartDate"
                                      mask="MM/DD/YYYY"
                                      @update:model-value="
                                        showAddButtonEndDateError = false;
                                        calculateTypeDuration();
                                      "
                                    />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input> -->
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.endDateStr }} </span>
                          </q-td>
                          <q-td class="text-left" width="11%">
                            <q-input
                              v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                              v-model="editingRow.duration" outlined
                              hide-bottom-space :dense="true" readonly
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.duration }} </span>
                          </q-td>
                          <q-td class="text-left">
                            <q-input
                              v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                              v-model="editingRow.note" outlined
                              hide-bottom-space :dense="true" maxlength="300"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.note }} </span>
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
                              <q-icon name="o_edit" size="xs" class="cursor-pointer q-mr-lg" @click="onEditRow(props.row)">
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
                  <fieldset class="q-mb-lg">
                    <legend>Employee Status</legend>
                    <div class="flex items-center justify-end q-mb-md">
                      <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAddStatus" />
                    </div>
                    <q-table
                      ref="tableRef"
                      v-model:pagination="pagination"
                      bordered
                      class="no-shadow"
                      :loading="loading"
                      :rows="statusRows"
                      :columns="columnsStatus"
                      row-key="id"
                      separator="cell"
                      no-data-label="No data available"
                      binary-state-sort
                      :rows-per-page-options="[20, 50, 100, 200, 500]"
                    >
                      <template #header="props">
                        <q-tr :props="props">
                          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}
                            <span v-if="['employeeStatusId', 'startDate'].includes(col.name)" class="required">*</span>
                          </q-th>
                          <q-th auto-width class="text-center">Actions</q-th>
                        </q-tr>
                      </template>
                      <template #top-row>
                        <q-tr v-if="mode == 'addStatus' && editingStatusRow" class="row-highlight">
                          <q-td>
                            <div>
                              <formSingleSelectDropdown
                                v-model="editingStatusRow.employeeStatusId"
                                :options="employeeStatusDropdownSingleSelect.list.value"
                                :filter="employeeStatusDropdownSingleSelect.filter"
                                :error="editingStatusRowV$.employeeStatusId.$error"
                                :error-message="editingStatusRowV$.employeeStatusId.$errors[0]?.$message"
                              />
                            </div>
                          </q-td>
                          <q-td>
                            <formDate
                              v-model="editingStatusRow.statusStartDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingStatusRowV$.statusStartDateStr.$error"
                              :error-message="editingStatusRowV$.statusStartDateStr.$errors[0]?.$message"
                              :onBlur="() => editingStatusRowV$.statusStartDateStr.$touch()"
                              @update:model-value="calculateStatusDuration"
                            />
                          </q-td>
                          <q-td>
                            <formDate
                              v-model="editingStatusRow.statusEndDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingStatusRowV$.statusEndDateStr.$error"
                              :error-message="editingStatusRowV$.statusEndDateStr.$errors[0]?.$message"
                              :onBlur="() => editingStatusRowV$.statusEndDateStr.$touch()"
                              @update:model-value="calculateStatusDuration"
                              :options="disableBeforeStatusStartDate"
                            />
                          </q-td>
                          <q-td>
                            <q-input
                              v-model="editingStatusRow.duration" outlined
                              hide-bottom-space :dense="true" readonly
                            />
                          </q-td>
                          <q-td>
                            <q-input
                              v-model="editingStatusRow.note" outlined
                              hide-bottom-space :dense="true" maxlength="300"
                            />
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
                          <q-td class="text-left" width="15%">
                            <formSingleSelectDropdown
                              v-if="mode == 'editStatus' && editingStatusRow && props.row.id === activeRowId"
                              v-model="editingStatusRow.employeeStatusId"
                              :options="employeeStatusDropdownSingleSelect.list.value"
                              :filter="employeeStatusDropdownSingleSelect.filter"
                              :error="editingStatusRowV$.employeeStatusId.$error"
                              :error-message="editingStatusRowV$.employeeStatusId.$errors[0]?.$message"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getEmployeeStatus(props.row.employeeStatusId) }} </span>
                          </q-td>
                          <q-td class="text-left">
                            <formDate
                              v-if="mode == 'editStatus' && editingStatusRow && props.row.id === activeRowId"
                              v-model="editingStatusRow.statusStartDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingStatusRowV$.statusStartDateStr.$error"
                              :error-message="editingStatusRowV$.statusStartDateStr.$errors[0]?.$message"
                              :onBlur="() => editingStatusRowV$.statusStartDateStr.$touch()"
                              @update:model-value="calculateStatusDuration"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.statusStartDateStr }} </span>
                          </q-td>
                          <q-td class="text-left">
                            <formDate
                              v-if="mode == 'editStatus' && editingStatusRow && props.row.id === activeRowId"
                              v-model="editingStatusRow.statusEndDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingStatusRowV$.statusEndDateStr.$error"
                              :error-message="editingStatusRowV$.statusEndDateStr.$errors[0]?.$message"
                              :onBlur="() => editingStatusRowV$.statusEndDateStr.$touch()"
                              @update:model-value="calculateStatusDuration"
                              :options="disableBeforeStatusStartDate"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.statusEndDateStr }} </span>
                          </q-td>
                          <q-td class="text-left" width="10%">
                            <q-input
                              v-if="mode == 'editStatus' && editingStatusRow && props.row.id === activeRowId"
                              v-model="editingStatusRow.duration" outlined
                              hide-bottom-space :dense="true" readonly
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.duration }} </span>
                          </q-td>
                          <q-td class="text-left">
                            <q-input
                              v-if="mode == 'editStatus' && editingStatusRow && props.row.id === activeRowId"
                              v-model="editingStatusRow.note" outlined
                              hide-bottom-space :dense="true" maxlength="300"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.note }} </span>
                          </q-td>
                          <q-td auto-width class="text-center">
                            <template v-if="mode == 'editStatus' && editingStatusRow && props.row.id === activeRowId">
                              <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                                <q-tooltip>Save</q-tooltip>
                              </q-icon>
                              <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                                <q-tooltip>Cancel</q-tooltip>
                              </q-icon>
                            </template>
                            <template v-else>
                              <q-icon name="o_edit" size="xs" class="cursor-pointer q-mr-lg" @click="onEditStatusRow(props.row)">
                                <q-tooltip>Edit</q-tooltip>
                              </q-icon>
                              <q-icon v-if="!props.row.deleted" name="o_delete_outline" color="red" size="xs" class="cursor-pointer" @click="onDeleteStatus(props.row)">
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
                  <fieldset class="q-mb-lg">
                    <legend>Department</legend>
                    <div class="flex items-center justify-end q-mb-md">
                      <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAddDepartment" />
                    </div>
                    <q-table
                      ref="tableRef"
                      v-model:pagination="pagination"
                      bordered
                      class="no-shadow"
                      :loading="loading"
                      :rows="deptRows"
                      :columns="columnsDepartment"
                      row-key="id"
                      separator="cell"
                      no-data-label="No data available"
                      binary-state-sort
                      :rows-per-page-options="[20, 50, 100, 200, 500]"
                    >
                      <template #header="props">
                        <q-tr :props="props">
                          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}
                            <span v-if="['employeeDepartmentId', 'startDate'].includes(col.name)" class="required">*</span>
                          </q-th>
                          <q-th auto-width class="text-center">Actions</q-th>
                        </q-tr>
                      </template>
                      <template #top-row>
                        <q-tr v-if="mode == 'addDepartment' && editingDeptRow" class="row-highlight">
                          <q-td>
                            <div>
                              <formSingleSelectDropdown
                                v-model="editingDeptRow.employeeDepartmentId"
                                :options="departmentNameDropdownSingleSelect.list.value"
                                :filter="departmentNameDropdownSingleSelect.filter"
                                :error="editingDeptRowV$.employeeDepartmentId.$error"
                                :error-message="editingDeptRowV$.employeeDepartmentId.$errors[0]?.$message"
                              />
                            </div>
                          </q-td>
                          <q-td>
                            <formDate
                              v-model="editingDeptRow.departmentStartDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingDeptRowV$.departmentStartDateStr.$error"
                              :error-message="editingDeptRowV$.departmentStartDateStr.$errors[0]?.$message"
                              :onBlur="() => editingDeptRowV$.departmentStartDateStr.$touch()"
                              @update:model-value="calculateDepartmentDuration"
                            />
                          </q-td>
                          <q-td>
                            <formDate
                              v-model="editingDeptRow.departmentEndDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingDeptRowV$.departmentEndDateStr.$error"
                              :error-message="editingDeptRowV$.departmentEndDateStr.$errors[0]?.$message"
                              :onBlur="() => editingDeptRowV$.departmentEndDateStr.$touch()"
                              @update:model-value="calculateDepartmentDuration"
                            />
                          </q-td>
                          <q-td>
                            <q-input
                              v-model="editingDeptRow.duration" outlined
                              hide-bottom-space :dense="true" readonly
                            />
                          </q-td>
                          <q-td>
                            <q-input
                              v-model="editingDeptRow.note" outlined
                              hide-bottom-space :dense="true" maxlength="300"
                            />
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
                          <q-td class="text-left" width="15%">
                            <formSingleSelectDropdown
                              v-if="mode == 'editDepartment' && editingDeptRow && props.row.id === activeRowId"
                              v-model="editingDeptRow.employeeDepartmentId"
                              :options="departmentNameDropdownSingleSelect.list.value"
                              :filter="departmentNameDropdownSingleSelect.filter"
                              :error="editingDeptRowV$.employeeDepartmentId.$error"
                              :error-message="editingDeptRowV$.employeeDepartmentId.$errors[0]?.$message"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getEmployeeDepartment(props.row.employeeDepartmentId) }} </span>
                          </q-td>
                          <q-td class="text-left" width="15%">
                            <formDate
                              v-if="mode == 'editDepartment' && editingDeptRow && props.row.id === activeRowId"
                              v-model="editingDeptRow.departmentStartDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingDeptRowV$.departmentStartDateStr.$error"
                              :error-message="editingDeptRowV$.departmentStartDateStr.$errors[0]?.$message"
                              :onBlur="() => editingDeptRowV$.departmentStartDateStr.$touch()"
                              @update:model-value="calculateDepartmentDuration"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.departmentStartDateStr }} </span>
                          </q-td>
                          <q-td class="text-left" width="15%">
                            <formDate
                              v-if="mode == 'editDepartment' && editingDeptRow && props.row.id === activeRowId"
                              v-model="editingDeptRow.departmentEndDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingDeptRowV$.departmentEndDateStr.$error"
                              :error-message="editingDeptRowV$.departmentEndDateStr.$errors[0]?.$message"
                              :onBlur="() => editingDeptRowV$.departmentEndDateStr.$touch()"
                              @update:model-value="calculateDepartmentDuration"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.departmentEndDateStr }} </span>
                          </q-td>
                          <q-td class="text-left" width="11%">
                            <q-input
                              v-if="mode == 'editDepartment' && editingDeptRow && props.row.id === activeRowId"
                              v-model="editingDeptRow.duration" outlined
                              hide-bottom-space :dense="true" readonly
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.duration }} </span>
                          </q-td>
                          <q-td class="text-left">
                            <q-input
                              v-if="mode == 'editDepartment' && editingDeptRow && props.row.id === activeRowId"
                              v-model="editingDeptRow.note" outlined
                              hide-bottom-space :dense="true" maxlength="300"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.note }} </span>
                          </q-td>
                          <q-td auto-width class="text-center">
                            <template v-if="mode == 'editDepartment' && editingDeptRow && props.row.id === activeRowId">
                              <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                                <q-tooltip>Save</q-tooltip>
                              </q-icon>
                              <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                                <q-tooltip>Cancel</q-tooltip>
                              </q-icon>
                            </template>
                            <template v-else>
                              <q-icon name="o_edit" size="xs" class="cursor-pointer q-mr-lg" @click="onEditDepartmentRow(props.row)">
                                <q-tooltip>Edit</q-tooltip>
                              </q-icon>
                              <q-icon v-if="!props.row.deleted" name="o_delete_outline" color="red" size="xs" class="cursor-pointer" @click="onDeleteDepartment(props.row)">
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
                  <fieldset class="q-mb-lg">
                    <legend>Designation </legend>
                    <div class="flex items-center justify-end q-mb-md">
                      <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAddDesignation" />
                    </div>
                    <q-table
                      ref="tableRef"
                      v-model:pagination="pagination"
                      bordered
                      class="no-shadow"
                      :loading="loading"
                      :rows="desgRows"
                      :columns="columnsDesignation"
                      row-key="id"
                      separator="cell"
                      no-data-label="No data available"
                      binary-state-sort
                      :rows-per-page-options="[20, 50, 100, 200, 500]"
                    >
                      <template #header="props">
                        <q-tr :props="props">
                          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}
                            <span v-if="['employeeDesignationId', 'leaveApproverId', 'startDate'].includes(col.name)" class="required">*</span>
                          </q-th>
                          <q-th auto-width class="text-center">Actions</q-th>
                        </q-tr>
                      </template>
                      <template #top-row>
                        <q-tr v-if="mode == 'addDesignation' && editingDesgRow" class="row-highlight">
                          <q-td width="15%">
                            <div>
                              <formSingleSelectDropdown
                                v-model="editingDesgRow.employeeDesignationId"
                                :options="employeeDesignationBySiteIdDropdownSingleSelect.list.value"
                                :filter="employeeDesignationBySiteIdDropdownSingleSelect.filter"
                                :error="editingDesgRowV$.employeeDesignationId.$error"
                                :error-message="editingDesgRowV$.employeeDesignationId.$errors[0]?.$message"
                              />
                            </div>
                          </q-td>
                          <q-td width="5%">
                            <div>
                              <formSingleSelectDropdown
                                v-model="editingDesgRow.shiftId"
                                :options="employeeShiftDropdownSingleSelect.list.value"
                                :filter="employeeShiftDropdownSingleSelect.filter"
                                :required="false"
                              />
                            </div>
                          </q-td>
                          <q-td width="20%">
                            <div>
                              <formSingleSelectDropdown
                                v-model="editingDesgRow.leaveApproverId"
                                :options="activeEmployeesDropdownSingleSelect.list.value"
                                :filter="activeEmployeesDropdownSingleSelect.filter"
                                :error="editingDesgRowV$.leaveApproverId.$error"
                                :error-message="editingDesgRowV$.leaveApproverId.$errors[0]?.$message"
                              />
                            </div>
                          </q-td>
                          <q-td width="10%">
                            <formDate
                              v-model="editingDesgRow.designationStartDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingDesgRowV$.designationStartDateStr.$error"
                              :error-message="editingDesgRowV$.designationStartDateStr.$errors[0]?.$message"
                              :onBlur="() => editingDesgRowV$.designationStartDateStr.$touch()"
                              @update:model-value="calculateDesignationDuration"
                            />
                          </q-td>
                          <q-td width="10%">
                            <formDate
                              v-model="editingDesgRow.designationEndDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingDesgRowV$.designationEndDateStr.$error"
                              :error-message="editingDesgRowV$.designationEndDateStr.$errors[0]?.$message"
                              :onBlur="() => editingDesgRowV$.designationEndDateStr.$touch()"
                              @update:model-value="calculateDesignationDuration"
                              :options="disableBeforDesignationStartDate"
                            />
                          </q-td>
                          <q-td width="10%">
                            <q-input
                              v-model="editingDesgRow.duration" outlined
                              hide-bottom-space :dense="true" readonly
                            />
                          </q-td>
                          <q-td width="30%">
                            <q-input
                              v-model="editingDesgRow.note" outlined
                              hide-bottom-space :dense="true" maxlength="300"
                            />
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
                          <q-td class="text-left" width="10%">
                            <formSingleSelectDropdown
                              v-if="mode == 'editDesignation' && editingDesgRow && props.row.id === activeRowId"
                              v-model="editingDesgRow.employeeDesignationId"
                              :options="employeeDesignationBySiteIdDropdownSingleSelect.list.value"
                              :filter="employeeDesignationBySiteIdDropdownSingleSelect.filter"
                              :error="editingDesgRowV$.employeeDesignationId.$error"
                              :error-message="editingDesgRowV$.employeeDesignationId.$errors[0]?.$message"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getEmployeeDesignation(props.row.employeeDesignationId) }} </span>
                          </q-td>
                          <q-td class="text-left" width="10%">
                            <formSingleSelectDropdown
                              v-if="mode == 'editDesignation' && editingDesgRow && props.row.id === activeRowId"
                              v-model="editingDesgRow.shiftId"
                              :options="employeeShiftDropdownSingleSelect.list.value"
                              :filter="employeeShiftDropdownSingleSelect.filter"
                              :required="false"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getEmployeeShift(props.row.shiftId) }} </span>
                          </q-td>
                          <q-td class="text-left" width="10%">
                            <formSingleSelectDropdown
                              v-if="mode == 'editDesignation' && editingDesgRow && props.row.id === activeRowId"
                              v-model="editingDesgRow.leaveApproverId"
                              :options="activeEmployeesDropdownSingleSelect.list.value"
                              :filter="activeEmployeesDropdownSingleSelect.filter"
                              :error="editingDesgRowV$.leaveApproverId.$error"
                              :error-message="editingDesgRowV$.leaveApproverId.$errors[0]?.$message"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getEmployeeLeaveApprover(props.row.leaveApproverId) }} </span>
                          </q-td>
                          <q-td class="text-left" width="10%">
                            <formDate
                              v-if="mode == 'editDesignation' && editingDesgRow && props.row.id === activeRowId"
                              v-model="editingDesgRow.designationStartDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingDesgRowV$.designationStartDateStr.$error"
                              :error-message="editingDesgRowV$.designationStartDateStr.$errors[0]?.$message"
                              :onBlur="() => editingDesgRowV$.designationStartDateStr.$touch()"
                              @update:model-value="calculateDesignationDuration"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.designationStartDateStr }} </span>
                          </q-td>
                          <q-td class="text-left" width="10%">
                            <formDate
                              v-if="mode == 'editDesignation' && editingDesgRow && props.row.id === activeRowId"
                              v-model="editingDesgRow.designationEndDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingDesgRowV$.designationEndDateStr.$error ||
                                (showAddButtonEndDateError && props.row.id === activeRowId)"
                              :error-message="(showAddButtonEndDateError && props.row.id === activeRowId)
                                ? 'Designation End Date is required.'
                                : editingDesgRowV$.designationEndDateStr.$errors[0]?.$message"
                              :onBlur="() => editingDesgRowV$.designationEndDateStr.$touch()"
                              @update:model-value=" showAddButtonEndDateError = false; calculateDesignationDuration"
                              :options="disableBeforDesignationStartDate"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.designationEndDateStr }} </span>
                          </q-td>
                          <q-td class="text-left" width="8%">
                            <q-input
                              v-if="mode == 'editDesignation' && editingDesgRow && props.row.id === activeRowId"
                              v-model="editingDesgRow.duration" outlined
                              hide-bottom-space :dense="true" readonly
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.duration }} </span>
                          </q-td>
                          <q-td class="text-left" width="30%">
                            <q-input
                              v-if="mode == 'editDesignation' && editingDesgRow && props.row.id === activeRowId"
                              v-model="editingDesgRow.note" outlined
                              hide-bottom-space :dense="true" maxlength="300"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.note }} </span>
                          </q-td>
                          <q-td auto-width class="text-center">
                            <template v-if="mode == 'editDesignation' && editingDesgRow && props.row.id === activeRowId">
                              <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                                <q-tooltip>Save</q-tooltip>
                              </q-icon>
                              <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                                <q-tooltip>Cancel</q-tooltip>
                              </q-icon>
                            </template>
                            <template v-else>
                              <q-icon name="o_edit" size="xs" class="cursor-pointer q-mr-lg" @click="onEditDesignationRow(props.row)">
                                <q-tooltip>Edit</q-tooltip>
                              </q-icon>
                              <q-icon v-if="!props.row.deleted" name="o_delete_outline" color="red" size="xs" class="cursor-pointer" @click="onDeleteDesignation(props.row)">
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
                  <fieldset class="q-mb-lg">
                    <legend>Org Location</legend>
                    <div class="flex items-center justify-end q-mb-md">
                      <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAddOrgLocation" />
                    </div>
                    <q-table
                      ref="tableRef"
                      v-model:pagination="pagination"
                      bordered
                      class="no-shadow"
                      :loading="loading"
                      :rows="orglocRows"
                      :columns="columnsOrgLocation"
                      row-key="id"
                      separator="cell"
                      no-data-label="No data available"
                      binary-state-sort
                      :rows-per-page-options="[20, 50, 100, 200, 500]"
                    >
                      <template #header="props">
                        <q-tr :props="props">
                          <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}
                            <span v-if="['orgLocationId', 'leaveApproverId', 'startDate'].includes(col.name)" class="required">*</span>
                          </q-th>
                          <q-th auto-width class="text-center">Actions</q-th>
                        </q-tr>
                      </template>
                      <template #top-row>
                        <q-tr v-if="mode == 'addOrgLocation' && editingOrgLocRow" class="row-highlight">
                          <q-td width="10%">
                            <div>
                            <formSingleSelectDropdown
                              v-model="editingOrgLocRow.orgLocationId"
                              :options="employeeOrgLocationDropdownSingleSelect.list.value"
                              :filter="employeeOrgLocationDropdownSingleSelect.filter"
                              :error="editingOrgLocRowV$.orgLocationId.$error"
                              :error-message="editingOrgLocRowV$.orgLocationId.$errors[0]?.$message"
                            />
                            </div>
                          </q-td>
                          <q-td width="20%">
                            <formDate
                              v-model="editingOrgLocRow.orgLocationStartDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingOrgLocRowV$.orgLocationStartDateStr.$error"
                              :error-message="editingOrgLocRowV$.orgLocationStartDateStr.$errors[0]?.$message"
                              :onBlur="() => editingOrgLocRowV$.orgLocationStartDateStr.$touch()"
                              @update:model-value="calculateOrgLocationDuration"
                            />
                          </q-td>
                          <q-td width="20%">
                            <formDate
                              v-model="editingOrgLocRow.orgLocationEndDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingOrgLocRowV$.orgLocationEndDateStr.$error"
                              :error-message="editingOrgLocRowV$.orgLocationEndDateStr.$errors[0]?.$message"
                              :onBlur="() => editingOrgLocRowV$.orgLocationEndDateStr.$touch()"
                              @update:model-value="calculateOrgLocationDuration"
                              :options="disableBeforOrgStartDate"
                            />
                          </q-td>
                          <q-td width="10%">
                            <q-input
                              v-model="editingOrgLocRow.duration" outlined
                              hide-bottom-space :dense="true" readonly
                            />
                          </q-td>
                          <q-td width="40%">
                            <q-input
                              v-model="editingOrgLocRow.note" outlined
                              hide-bottom-space :dense="true" maxlength="300"
                            />
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
                          <q-td class="text-left" width="15%">
                            <formSingleSelectDropdown
                              v-if="mode == 'editOrgLocation' && editingOrgLocRow && props.row.id === activeRowId"
                              v-model="editingOrgLocRow.orgLocationId"
                              :options="employeeOrgLocationDropdownSingleSelect.list.value"
                              :filter="employeeOrgLocationDropdownSingleSelect.filter"
                              :error="editingOrgLocRowV$.orgLocationId.$error"
                              :error-message="editingOrgLocRowV$.orgLocationId.$errors[0]?.$message"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getEmployeeOrgLocation(props.row.orgLocationId) }} </span>
                          </q-td>
                          <q-td class="text-left" width="15%">
                            <formDate
                              v-if="mode == 'editOrgLocation' && editingOrgLocRow && props.row.id === activeRowId"
                              v-model="editingOrgLocRow.orgLocationStartDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingOrgLocRowV$.orgLocationStartDateStr.$error"
                              :error-message="editingOrgLocRowV$.orgLocationStartDateStr.$errors[0]?.$message"
                              :onBlur="() => editingOrgLocRowV$.orgLocationStartDateStr.$touch()"
                              @update:model-value="calculateOrgLocationDuration"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.orgLocationStartDateStr }} </span>
                          </q-td>
                          <q-td class="text-left" width="15%">
                            <formDate
                              v-if="mode == 'editOrgLocation' && editingOrgLocRow && props.row.id === activeRowId"
                              v-model="editingOrgLocRow.orgLocationEndDateStr"
                              :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                              :error="editingOrgLocRowV$.orgLocationEndDateStr.$error"
                              :error-message="editingOrgLocRowV$.orgLocationEndDateStr.$errors[0]?.$message"
                              :onBlur="() => editingOrgLocRowV$.orgLocationEndDateStr.$touch()"
                              @update:model-value="calculateOrgLocationDuration"
                              :options="disableBeforOrgStartDate"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.orgLocationEndDateStr }} </span>
                          </q-td>
                          <q-td class="text-left" width="11%">
                            <q-input
                              v-if="mode == 'editOrgLocation' && editingOrgLocRow && props.row.id === activeRowId"
                              v-model="editingOrgLocRow.duration" outlined
                              hide-bottom-space :dense="true" readonly
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.duration }} </span>
                          </q-td>
                          <q-td class="text-left" width="40%">
                            <q-input
                              v-if="mode == 'editOrgLocation' && editingOrgLocRow && props.row.id === activeRowId"
                              v-model="editingOrgLocRow.note" outlined
                              hide-bottom-space :dense="true" maxlength="300"
                            />
                            <span v-else :class="props.row.deleted ? 'text-delete' : ''" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.note }} </span>
                          </q-td>
                          <q-td auto-width class="text-center">
                            <template v-if="mode == 'editOrgLocation' && editingOrgLocRow && props.row.id === activeRowId">
                              <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                                <q-tooltip>Save</q-tooltip>
                              </q-icon>
                              <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                                <q-tooltip>Cancel</q-tooltip>
                              </q-icon>
                            </template>
                            <template v-else>
                              <q-icon name="o_edit" size="xs" class="cursor-pointer q-mr-lg" @click="onEditOrgLocationRow(props.row)">
                                <q-tooltip>Edit</q-tooltip>
                              </q-icon>
                              <q-icon v-if="!props.row.deleted" name="o_delete_outline" color="red" size="xs" class="cursor-pointer" @click="onDeleteOrgLocation(props.row)">
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
                  <!-- <fieldset class="q-mb-lg">
                      <legend>Client Location</legend>
                      <div class="flex items-center justify-end q-mb-md">
                        <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAddClientLocation" />
                      </div>
                      <q-table
                        ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="clientlocRows" :columns="columnsClientLocation" row-key="id" separator="cell"
                        no-data-label="No data available" binary-state-sort
                      >
                        <template #header="props">
                          <q-tr :props="props">
                            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}
                              <span v-if="['clientLocationId', 'startDate'].includes(col.name)"  class="required">*</span>
                            </q-th>
                            <q-th auto-width class="text-center">Actions</q-th>
                          </q-tr>
                        </template>
                        <template #top-row>
                          <q-tr v-if="mode == 'addClientLocation' && editingClientLocRow" class="row-highlight">
                            <q-td>
                              <div>
                                <q-select
                                  v-model="editingClientLocRow.clientLocationId" clearable use-input stack-label outlined hide-bottom-space :dense="true"
                                  :options="employeeClientLocationList" option-value="value" option-label="text" emit-value map-options @filter="filterFn12"
                                  :error="editingClientLocRowV$.clientLocationId.$error" :error-message="editingClientLocRowV$.clientLocationId.$errors[0]?.$message" @blur="editingClientLocRowV$.clientLocationId.$touch"
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
                              <q-input v-model="editingClientLocRow.clientLocationStartDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense :error="editingClientLocRowV$.clientLocationStartDateStr.$error" :error-message="editingClientLocRowV$.clientLocationStartDateStr.$errors[0]?.$message" @blur="editingClientLocRowV$.clientLocationStartDateStr.$touch">
                                <template #append>
                                  <q-icon name="o_calendar_month" class="cursor-pointer">
                                    <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="editingClientLocRow.clientLocationStartDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()"  />
                                      </q-popup-proxy>
                                    </q-icon>
                                </template>
                              </q-input>
                            </q-td>
                            <q-td>
                              <q-input v-model="editingClientLocRow.clientLocationEndDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense :error="editingClientLocRowV$.clientLocationEndDateStr.$error" :error-message="editingClientLocRowV$.clientLocationEndDateStr.$errors[0]?.$message" @blur="editingClientLocRowV$.clientLocationEndDateStr.$touch">
                                <template #append>
                                  <q-icon name="o_calendar_month" class="cursor-pointer">
                                    <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date v-model="editingClientLocRow.clientLocationEndDateStr" :options="disableBeforClientLocationStartDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                                      </q-popup-proxy>
                                    </q-icon>
                                </template>
                              </q-input>
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
                            <q-td class="text-left">
                              <q-select
                                v-if="mode == 'editClientLocation' && editingClientLocRow && props.row.id === activeRowId"
                                v-model="editingClientLocRow.clientLocationId" outlined hide-bottom-space :dense="true"
                                :options="employeeClientLocationList" option-value="value" option-label="text" emit-value map-options @filter="filterFn12"
                                :error="editingClientLocRowV$.clientLocationId.$error" :error-message="editingClientLocRowV$.clientLocationId.$errors[0]?.$message" @blur="editingClientLocRowV$.clientLocationId.$touch"
                              />
                              <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ getEmployeeClientLocation(props.row.clientLocationId) }} </span>
                            </q-td>
                            <q-td class="text-left">
                              <q-input v-if="mode == 'editClientLocation' && editingClientLocRow && props.row.id === activeRowId" v-model="editingClientLocRow.clientLocationStartDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense :error="editingClientLocRowV$.orgLocationStartDateStr.$error" :error-message="editingClientLocRowV$.orgLocationStartDateStr.$errors[0]?.$message" @blur="editingClientLocRowV$.orgLocationStartDateStr.$touch">
                                <template #append>
                                  <q-icon name="o_calendar_month" class="cursor-pointer">
                                    <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                      <q-date v-model="editingClientLocRow.clientLocationStartDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                                    </q-popup-proxy>
                                  </q-icon>
                                </template>
                              </q-input>
                              <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.clientLocationStartDateStr }} </span>
                            </q-td>
                            <q-td class="text-left">
                              <q-input v-if="mode == 'editClientLocation' && editingClientLocRow && props.row.id === activeRowId" v-model="editingClientLocRow.clientLocationEndDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense :error="editingClientLocRowV$.clientLocationEndDateStr.$error" :error-message="editingClientLocRowV$.clientLocationEndDateStr.$errors[0]?.$message" @blur="editingClientLocRowV$.clientLocationEndDateStr.$touch">
                                <template #append>
                                  <q-icon name="o_calendar_month" class="cursor-pointer">
                                    <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                      <q-date v-model="editingClientLocRow.clientLocationEndDateStr" :options="disableBeforOrgStartDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                                    </q-popup-proxy>
                                  </q-icon>
                                </template>
                              </q-input>
                              <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.clientLocationEndDateStr }} </span>
                            </q-td>
                            <q-td auto-width class="text-center">
                              <template v-if="mode == 'editClientLocation' && editingClientLocRow && props.row.id === activeRowId">
                                <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                                  <q-tooltip>Save</q-tooltip>
                                </q-icon>
                                <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                                  <q-tooltip>Cancel</q-tooltip>
                                </q-icon>
                              </template>
                              <template v-else>
                                <q-icon name="o_edit" size="xs" class="cursor-pointer q-mr-lg" @click="onEditClientLocationRow(props.row)">
                                  <q-tooltip>Edit</q-tooltip>
                                </q-icon>
                                <q-icon v-if="!props.row.deleted" name="o_delete_outline" color="red" size="xs" class="cursor-pointer" @click="onDeleteClientLocation(props.row)">
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
                    </fieldset> -->
                </q-tab-panel>
                <q-tab-panel name="3_tab">
                  <fieldset v-if="!model.sameASPermanentAddress && selectedCountry === 'India' || model.sameASPermanentAddress && model.country === 'India'" class="q-mt-lg">
                    <legend>KYC Information</legend>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-3">
                        <div class="q-mb-xs text-black">Aadhar Card No</div>
                        <div>
                          <q-input
                            v-model="model.aadhaarCardNo" outlined stack-label hide-bottom-space :dense="true" :maxlength="'14'" :hint="'#### #### ####'" :mask="'#### #### ####'"
                          />
                        </div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div>
                          <div class="q-mb-xs text-black">Pan Card No</div>
                          <div><q-input v-model="model.panCardNo" outlined stack-label hide-bottom-space :dense="true" :maxlength="'10'" hint="AAAAA####A" :mask="'AAAAA####A'" /> </div>
                        </div>
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <div>
                          <div class="q-mb-xs text-black">EPF UAN No</div>
                          <div><q-input v-model="model.epfuanNo" outlined stack-label hide-bottom-space :dense="true" :maxlength="'12'" hint="############" :mask="'############'" /> </div>
                        </div>
                      </div>
                    </div>
                  </fieldset>
                  <fieldset v-if="model.personId" class="q-mt-lg">
                    <legend>Employment Period Details</legend>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-sm-6 col-md-3">
                        <formDate
                          v-model="model.joiningDateStr"
                          label="Joining Date"
                          :required="false"
                          :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                          @update:model-value="calculateOrgLocationDuration"
                        />
                      </div>
                      <div class="col-12 col-sm-6 col-md-3">
                        <formDate
                          v-model="model.releaseDateStr"
                          label="Release Date/Last Date"
                          :required="false"
                          :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                          @update:model-value="calculateOrgLocationDuration"
                          :options="disableBeforeReleaseDate"
                        />
                      </div>
                    </div>
                    <!-- <div class="row q-col-gutter-x-md q-mb-md">
                        <div class="col-3">
                          <div class="q-mb-xs text-black">Training Period Start Date</div>
                          <div>
                            <q-input v-model="model.trainingPeriodStartDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                  <q-date v-model="model.trainingPeriodStartDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()"  />
                                    </q-popup-proxy>
                                  </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                        <div class="col-3">
                          <div class="q-mb-xs text-black">Training Period End Date</div>
                          <div>
                            <q-input v-model="model.trainingPeriodEndDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                  <q-date v-model="model.trainingPeriodEndDateStr" :options="disableBeforTrainingStartDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()"  />
                                    </q-popup-proxy>
                                  </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                        <div class="col-3">
                          <div>
                            <div class="q-mb-xs text-black">Training Period Note </div>
                            <div class="form-group">
                              <q-input outlined v-model="model.trainingPeriodNote" autogrow hint="The maximum length allowed is 500."/>
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="row q-col-gutter-x-md q-mb-md">
                        <div class="col-3">
                          <div class="q-mb-xs text-black">Probation Period Start Date</div>
                          <div>
                            <q-input v-model="model.probationPeriodStartDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                  <q-date v-model="model.probationPeriodStartDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()"  />
                                    </q-popup-proxy>
                                  </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                        <div class="col-3">
                          <div class="q-mb-xs text-black">Probation Period End Date</div>
                          <div>
                            <q-input v-model="model.probationPeriodEndDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                  <q-date v-model="model.probationPeriodEndDateStr" :options="disableBeforProbationStartDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()"  />
                                    </q-popup-proxy>
                                  </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                        <div class="col-3">
                          <div>
                            <div class="q-mb-xs text-black">Probation Period Note</div>
                            <div class="form-group">
                              <q-input outlined v-model="model.probationPeriodNote" autogrow hint="The maximum length allowed is 500."/>
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="row q-col-gutter-x-md q-mb-md">
                        <div class="col-3">
                          <div class="q-mb-xs text-black">Permanent Date</div>
                          <div>
                            <q-input v-model="model.permanentDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense>
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                  <q-date v-model="model.permanentDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()"  />
                                    </q-popup-proxy>
                                  </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div> -->
                  </fieldset>
                  <fieldset v-if="model.personId" class="q-mt-lg">
                    <legend>Education Information </legend>
                    <div class="row q-col-gutter-x-md q-mb-md">
                      <div class="col-12 col-md-6">
                        <div>
                          <div class="q-mb-xs text-black">Education Details</div>
                          <div class="form-group">
                            <q-input v-model="model.educationDetail" outlined autogrow hint="The maximum length allowed is 500." maxlength="500" />
                          </div>
                        </div>
                      </div>
                    </div>
                  </fieldset>
                </q-tab-panel>
              </q-tab-panels>
            </q-card>
          </div>
        </div>
        <q-card-actions class="row wrap q-gutter-sm justify-end">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogClose" />
          <q-btn v-if="tab !== '3_tab'" label="Save & Next" type="submit" color="primary" class="actionBtn" :loading="processing" no-caps />
          <q-btn label="Save & Close" type="button" color="primary" class="actionBtn" :loading="processingClose" no-caps @click="onSubmitClose()" />
          <!-- <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
              <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps /> -->
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useQuasar, useDialogPluginComponent, uid } from "quasar";
import { required, helpers, minValue, minLength, maxLength, email } from "@vuelidate/validators";
import useFilters from "composables/useFilters";
import { ref, watch, onMounted, computed } from "vue";
import { notifySuccess, notifyError, zwConfirmLeave } from "assets/utils";
import { isDate } from "validators/zw_validators.js";
import { useAuthStore } from "stores/auth";
import _ from "lodash";
import useVuelidate from "@vuelidate/core";

import employeesService from "../employee.service";
import editPerson from "modules/person/components/addEdit.vue";
import personService from "modules/person/person.service";

// Shared Dropdowns
import departmentModule from "src/modules/department/utils/dropdowns.js";
import personModule from "src/modules/person/utils/dropdowns.js";
import commonModule from "src/modules/common/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------

const props = defineProps({ id: { type: String, default: "" }, personId: { type: String, default: "" } });

// ----------------------------------------------------------------------------------------------------------------
// Define emits
// ----------------------------------------------------------------------------------------------------------------

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

// const baseURL = process.env.API_BASE_URL;
const baseCountryId = process.env.BASE_COUNTRY_ID;
const $q = useQuasar();
const authStore = useAuthStore();
const user = authStore.user;
const roles = user?.roles || [];
const rows = ref([]);
const rowCounter = ref(0);
const statusRows = ref([]);
const deptRows = ref([]);
const desgRows = ref([]);
const orglocRows = ref([]);
const clientlocRows = ref([]);
const { toDate } = useFilters();
const tab = ref("1_tab");
const loading = ref(true);
const processing = ref(false);
const processingClose = ref(false);
const mode = ref(null);
const editingRow = ref(null);
const editingStatusRow = ref(null);
const editingDeptRow = ref(null);
const editingDesgRow = ref(null);
const editingOrgLocRow = ref(null);
const editingClientLocRow = ref(null);
const activeRowId = ref(null);
const selectedSiteId = ref(history.state?.siteId);

const readonlyEmployee = props.id ? "readonly" : "";
const pagination = ref({ sortBy: " ", descending: false, rowsPerPage: 20, page: 1 });

// ----------------------------------------------------------------------------------------------------------------
// Define columns
// ----------------------------------------------------------------------------------------------------------------

const columns = ref([
  { name: "employeeTypeId", label: "Employment Type", field: "employeeTypeId", align: "left", sortable: false },
  { name: "startDate", label: "Employment Type Start Date", field: "startDate", align: "left", sortable: false },
  { name: "endDate", label: "Employment Type End Date", field: "endDate", align: "left", sortable: false },
  { name: "duration", label: "Duration", field: "duration", align: "left", sortable: false },
  { name: "note", label: "Note", field: "note", align: "left", sortable: false }
]);
const columnsStatus = ref([
  { name: "employeeStatusId", label: "Employment Status", field: "employeeStatusId", align: "left", sortable: false },
  { name: "startDate", label: "Employment Status Start Date", field: "startDate", align: "left", sortable: false },
  { name: "endDate", label: "Employment Status End Date", field: "endDate", align: "left", sortable: false },
  { name: "duration", label: "Duration", field: "duration", align: "left", sortable: false },
  { name: "note", label: "Note", field: "note", align: "left", sortable: false }
]);
const columnsDepartment = ref([
  { name: "employeeDepartmentId", label: "Department", field: "employeeDepartmentId", align: "left", sortable: false },
  { name: "startDate", label: "Department Start Date", field: "startDate", align: "left", sortable: false },
  { name: "endDate", label: "Department End Date", field: "endDate", align: "left", sortable: false },
  { name: "duration", label: "Duration", field: "duration", align: "left", sortable: false },
  { name: "note", label: "Note", field: "note", align: "left", sortable: false }
]);
const columnsDesignation = ref([
  { name: "employeeDesignationId", label: "Designation", field: "employeeDesignationId", align: "left", sortable: false },
  { name: "shiftId", label: "Shift", field: "shiftId", align: "left", sortable: false },
  { name: "leaveApproverId", label: "Leave Approver", field: "leaveApproverId", align: "left", sortable: false },
  { name: "startDate", label: "Designation Start Date", field: "startDate", align: "left", sortable: false },
  { name: "endDate", label: "Designation End Date", field: "endDate", align: "left", sortable: false },
  { name: "duration", label: "Duration", field: "duration", align: "left", sortable: false },
  { name: "note", label: "Note", field: "note", align: "left", sortable: false }
]);
const columnsOrgLocation = ref([
  { name: "orgLocationId", label: "Org Location", field: "orgLocationId", align: "left", sortable: false },
  { name: "startDate", label: "Org Location Start Date", field: "startDate", align: "left", sortable: false },
  { name: "endDate", label: "Org Location End Date", field: "endDate", align: "left", sortable: false },
  { name: "duration", label: "Duration", field: "duration", align: "left", sortable: false },
  { name: "note", label: "Note", field: "note", align: "left", sortable: false }
]);
// const columnsClientLocation = ref([
//   { name: "clientLocationId", label: "Client Location", field: "clientLocationId", align: "left", sortable: false },
//   { name: "startDate", label: "Client Location Start Date", field: "startDate", align: "left", sortable: false },
//   { name: "endDate", label: "Client Location End Date", field: "endDate", align: "left", sortable: false }
// ]);

// On page rendering
// onMounted(() => {
//   getAllEmployeesTypeListForDropdown();
// });

let employeeId = props.id;
let disableTab = true;
if (employeeId) {
  disableTab = false;
}

// ----------------------------------------------------------------------------------------------------------------
// Validation Rules
// ----------------------------------------------------------------------------------------------------------------

const rules = {
  personId: { required: helpers.withMessage("Person name is required", required) },
  officialEmail: { email: helpers.withMessage("Invalid email", email), required: helpers.withMessage("Office Email is required", required) },
  employeeCode: { required: helpers.withMessage("Employee code required", required), minValue: helpers.withMessage("Employee code must be a positive value", minValue(0)) },
  releaseDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    releaseDateStr: helpers.withMessage("End date must occur after the joining date", (value, { joiningDateStr }) => {
      if (!value) return true;
      return new Date(value) >= new Date(joiningDateStr);
    })
  }
};

// ----------------------------------------------------------------------------------------------------------------
// Multiple row validation
// ----------------------------------------------------------------------------------------------------------------

const editingStatusRowrules = {
  employeeStatusId: { required: helpers.withMessage("Status is Required", required) },
  statusStartDateStr: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  statusEndDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    statusEndDateStr: helpers.withMessage("End date must occur after the start date", (value, { statusStartDateStr }) => {
      if (!value) return true;
      return new Date(value) >= new Date(statusStartDateStr);
    })
  }
};

const editingRowrules = {
  employeeTypeId: { required: helpers.withMessage("Type is Required", required) },
  startDateStr: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  endDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    statusEndDateStr: helpers.withMessage("End date must occur after the start date", (value, { startDateStr }) => {
      if (!value) return true;
      return new Date(value) >= new Date(startDateStr);
    })
  }
};

const editingDeptRowrules = {
  employeeDepartmentId: { required: helpers.withMessage("Department is Required", required) },
  departmentStartDateStr: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  departmentEndDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    statusEndDateStr: helpers.withMessage("End date must occur after the start date", (value, { departmentStartDateStr }) => {
      if (!value) return true;
      return new Date(value) >= new Date(departmentStartDateStr);
    })
  }
};

const editingDesgRowrules = {
  employeeDesignationId: { required: helpers.withMessage("Designation is Required", required) },
  leaveApproverId: { required: helpers.withMessage("Leave Approver is Required", required) },
  designationStartDateStr: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  designationEndDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    designationEndDateStr: helpers.withMessage("End date must occur after the start date", (value, { designationStartDateStr }) => {
      if (!value) return true;
      return new Date(value) >= new Date(designationStartDateStr);
    })
  }
};

const editingOrgLocRowrules = {
  orgLocationId: { required: helpers.withMessage("Org Location is Required", required) },
  orgLocationStartDateStr: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  orgLocationEndDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate),
    orgLocationEndDateStr: helpers.withMessage("End date must occur after the start date", (value, { orgLocationStartDateStr }) => {
      if (!value) return true;
      return new Date(value) >= new Date(orgLocationStartDateStr);
    })
  }
};

// watch(() => getEditingRowProp("startDateStr"), (newValue, oldValue) => {
//   if (newValue) {
//     calculateDuration();
//   }
// }, { immediate: true });

// ----------------------------------------------------------------------------------------------------------------
// Custom Functions
// ----------------------------------------------------------------------------------------------------------------

function calculateTypeDuration () {
  const { startDateStr, endDateStr } = editingRow.value;

  if (!startDateStr) return;
  const startDate = new Date(startDateStr);

  if (!endDateStr) {
    editingRow.value.duration = "0 month";
    return;
  }

  const endDate = new Date(endDateStr);

  if (isNaN(startDate) || isNaN(endDate)) {
    editingRow.value.duration = "Invalid date";
    return;
  }

  const startYear = startDate.getFullYear();
  const startMonth = startDate.getMonth();
  const endYear = endDate.getFullYear();
  const endMonth = endDate.getMonth();

  // Calculate the difference in months and years
  let years = endYear - startYear;
  let months = endMonth - startMonth;

  if (months < 0) {
    years -= 1;
    months += 12;
  }

  // Construct the duration string
  const yearPart = years > 0 ? `${years} year` : "";
  const monthPart = months > 0 ? `${months} month` : "";

  editingRow.value.duration =
    yearPart && monthPart ? `${yearPart}, ${monthPart}` : yearPart || monthPart || "0 month";
}

function calculateStatusDuration () {
  const { statusStartDateStr, statusEndDateStr } = editingStatusRow.value;

  if (!statusStartDateStr) return;
  const startDate = new Date(statusStartDateStr);

  if (!statusEndDateStr) {
    editingStatusRow.value.duration = "0 month";
    return;
  }

  const endDate = new Date(statusEndDateStr);

  if (isNaN(startDate) || isNaN(endDate)) {
    editingStatusRow.value.duration = "Invalid date";
    return;
  }

  const startYear = startDate.getFullYear();
  const startMonth = startDate.getMonth();
  const endYear = endDate.getFullYear();
  const endMonth = endDate.getMonth();

  // Calculate the difference in months and years
  let years = endYear - startYear;
  let months = endMonth - startMonth;

  if (months < 0) {
    years -= 1;
    months += 12;
  }

  // Construct the duration string
  const yearPart = years > 0 ? `${years} year` : "";
  const monthPart = months > 0 ? `${months} month` : "";

  editingStatusRow.value.duration =
    yearPart && monthPart ? `${yearPart}, ${monthPart}` : yearPart || monthPart || "0 month";
}

function calculateDepartmentDuration () {
  const { departmentStartDateStr, departmentEndDateStr } = editingDeptRow.value;

  if (!departmentStartDateStr) return;

  const startDate = new Date(departmentStartDateStr);
  if (!departmentEndDateStr) {
    editingDeptRow.value.duration = "0 month";
    return;
  }

  const endDate = new Date(departmentEndDateStr);

  if (isNaN(startDate) || isNaN(endDate)) {
    editingDeptRow.value.duration = "Invalid date";
    return;
  }

  const startYear = startDate.getFullYear();
  const startMonth = startDate.getMonth();
  const endYear = endDate.getFullYear();
  const endMonth = endDate.getMonth();

  // Calculate the difference in months and years
  let years = endYear - startYear;
  let months = endMonth - startMonth;

  if (months < 0) {
    years -= 1;
    months += 12;
  }

  // Construct the duration string
  const yearPart = years > 0 ? `${years} year` : "";
  const monthPart = months > 0 ? `${months} month` : "";

  editingDeptRow.value.duration =
    yearPart && monthPart ? `${yearPart}, ${monthPart}` : yearPart || monthPart || "0 month";
}

function calculateDesignationDuration () {
  const { designationStartDateStr, designationEndDateStr } = editingDesgRow.value;

  if (!designationStartDateStr) return;
  const startDate = new Date(designationStartDateStr);

  // If end date is null/empty → duration = 0 month
  if (!designationEndDateStr) {
    editingDesgRow.value.duration = "0 month";
    return;
  }

  const endDate = new Date(designationEndDateStr); // Use today's date if designationEndDateStr is null

  if (isNaN(startDate) || isNaN(endDate)) {
    editingDesgRow.value.duration = "Invalid date";
    return;
  }

  const startYear = startDate.getFullYear();
  const startMonth = startDate.getMonth();
  const endYear = endDate.getFullYear();
  const endMonth = endDate.getMonth();

  // Calculate the difference in months and years
  let years = endYear - startYear;
  let months = endMonth - startMonth;

  if (months < 0) {
    years -= 1;
    months += 12;
  }

  // Construct the duration string
  const yearPart = years > 0 ? `${years} year` : "";
  const monthPart = months > 0 ? `${months} month` : "";

  editingDesgRow.value.duration =
    yearPart && monthPart ? `${yearPart}, ${monthPart}` : yearPart || monthPart || "0 month";
}

function calculateOrgLocationDuration () {
  const { orgLocationStartDateStr, orgLocationEndDateStr } = editingOrgLocRow.value;

  if (!orgLocationStartDateStr) return;
  const startDate = new Date(orgLocationStartDateStr);

  if (!orgLocationEndDateStr) {
    editingOrgLocRow.value.duration = "0 month";
    return;
  }

  const endDate = new Date(orgLocationEndDateStr);

  if (isNaN(startDate) || isNaN(endDate)) {
    editingOrgLocRow.value.duration = "Invalid date";
    return;
  }

  const startYear = startDate.getFullYear();
  const startMonth = startDate.getMonth();
  const endYear = endDate.getFullYear();
  const endMonth = endDate.getMonth();

  // Calculate the difference in months and years
  let years = endYear - startYear;
  let months = endMonth - startMonth;

  if (months < 0) {
    years -= 1;
    months += 12;
  }

  // Construct the duration string
  const yearPart = years > 0 ? `${years} year` : "";
  const monthPart = months > 0 ? `${months} month` : "";

  editingOrgLocRow.value.duration =
    yearPart && monthPart ? `${yearPart}, ${monthPart}` : yearPart || monthPart || "0 month";
}

const editingStatusRowV$ = useVuelidate(editingStatusRowrules, editingStatusRow, { $lazy: true, $autoDirty: true });
const editingRowV$ = useVuelidate(editingRowrules, editingRow, { $lazy: true, $autoDirty: true });
const editingDeptRowV$ = useVuelidate(editingDeptRowrules, editingDeptRow, { $lazy: true, $autoDirty: true });
const editingDesgRowV$ = useVuelidate(editingDesgRowrules, editingDesgRow, { $lazy: true, $autoDirty: true });
const editingOrgLocRowV$ = useVuelidate(editingOrgLocRowrules, editingOrgLocRow, { $lazy: true, $autoDirty: true });

// ----------------------------------------------------------------------------------------------------------------
// Define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  firstName: "",
  middleName: "",
  lastName: "",
  genderId: null,
  primaryEmailAddress: "",
  primaryPhoneNumber: "",
  addressTypeId: null,
  addressLine1: "",
  addressLine2: "",
  country: null,
  countryId: null,
  stateProvinceId: null,
  city: "",
  zipCode: "",
  identifiedById: null,
  personId: null,
  identifiedDateStr: toDate(new Date()),
  identificationNote: "",
  pictureId: null,
  virtualPath: "",
  sameASPermanentAddress: false,
  aadhaarCardNo: "",
  panCardNo: "",
  epfuanNo: "",
  officialEmail: "",
  employeeCode: "",
  active: false
});

// ----------------------------------------------------------------------------------------------------------------
// Define current address model
// ----------------------------------------------------------------------------------------------------------------

const currentaddrmodel = ref({
  addressLine1: "",
  addressLine2: "",
  countryId: null,
  stateProvinceId: null,
  city: "",
  zipCode: ""
});

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

let maxLengthZip = baseCountryId === currentaddrmodel.value.countryId ? "5" : "6";
let crtaddrrules = {
  countryId: { required: helpers.withMessage("Country is required", required) },
  stateProvinceId: { required: helpers.withMessage("State is required", required) },
  addressLine1: { maxLength: maxLength(500), required: helpers.withMessage("Address Line 1 is required", required) },
  addressLine2: { maxLength: maxLength(500) },
  city: { maxLength: maxLength(500), required: helpers.withMessage("City is required", required) },
  zipCode: { minLength: minLength(maxLengthZip), maxLength: maxLength(maxLengthZip), required: helpers.withMessage(maxLengthZip === "5" ? "Zip code is required" : "Pin code is required", required) }
};

// Validate rules
let crtaddrv$ = useVuelidate(crtaddrrules, currentaddrmodel, { $lazy: true, $autoDirty: true });

// ----------------------------------------------------------------------------------------------------------------
// Get Employee details
// ----------------------------------------------------------------------------------------------------------------

const getEmployee = (employeeId) => {
  employeesService.getEmployee(employeeId).then((resp) => {
    model.value = _.cloneDeep(resp);
    if (resp.sameASPermanentAddress === false) {
      currentaddrmodel.value.addressLine1 = resp.address.addressLine1;
      currentaddrmodel.value.addressLine2 = resp.address.addressLine2;
      currentaddrmodel.value.countryId = resp.address.countryId;
      currentaddrmodel.value.stateProvinceId = resp.address.stateProvinceId;
      currentaddrmodel.value.city = resp.address.city;
      currentaddrmodel.value.zipCode = resp.address.zipCode;
    }
    getPersonbyId();
    model.value.joiningDateStr = model.value.joiningDate;
    model.value.releaseDateStr = model.value.releaseDate;
    deptRows.value = resp.employeeDepartment.map(item => ({
      ...item,
      departmentStartDateStr: item.startDate,
      departmentEndDateStr: item.endDate,
      editing: false,
      flag: "Edit"
    }));
    desgRows.value = resp.employeeDesignation.map(item => ({
      ...item,
      designationStartDateStr: item.startDate,
      designationEndDateStr: item.endDate,
      editing: false,
      flag: "Edit"
    }));
    statusRows.value = resp.employeeStatuses.map(item => ({
      ...item,
      statusStartDateStr: item.startDate,
      statusEndDateStr: item.endDate,
      editing: false,
      flag: "Edit"
    }));
    rows.value = resp.employeeType
      .map(item => ({
        ...item,
        startDateStr: item.startDate,
        endDateStr: item.endDate,
        editing: false,
        flag: "Edit"
      }));

    orglocRows.value = resp.employeeOrgLocation.map(item => ({
      ...item,
      orgLocationStartDateStr: item.startDate,
      orgLocationEndDateStr: item.endDate,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------

const { personNameDropdownSingleSelect } = personModule();
const { departmentNameDropdownSingleSelect } = departmentModule();

const {
  employeeTypeDropdownSingleSelect,
  employeeStatusDropdownSingleSelect,
  employeeDesignationBySiteIdDropdownSingleSelect,
  employeeShiftDropdownSingleSelect,
  activeEmployeesDropdownSingleSelect,
  employeeOrgLocationDropdownSingleSelect
} = employeeModule();

const {
  countryNameDropdownSingleSelect,
  stateNameDropdownSingleSelect
 } = commonModule();

const showAddButtonEndDateError = ref(false);
let isSaveDialog = false;
// let isCommonSaveDialog = false;
let isConfirmSaveTypeDialog = false;
function onAdd () {
  // Find the first row without End Date
  const rowWithoutEndDate = rows.value.find(row => !row.endDateStr);

  if (rowWithoutEndDate) {
    mode.value = "edit";
    activeRowId.value = rowWithoutEndDate.id;
    editingRow.value = { ...rowWithoutEndDate };

    showAddButtonEndDateError.value = true;
    return; // stop add
  }

  // If all rows are valid, proceed
  showAddButtonEndDateError.value = false;
  onAddConfirm();
}

function onAddConfirm () {
  isSaveDialog = true;
  // isCommonSaveDialog = true;
  isConfirmSaveTypeDialog = true;
  rowCounter.value += 1; // Increment the counter
  mode.value = "add";
  editingRow.value = {
    employeeTypeId: "",
    employmentTypeStartDate: "",
    employmentTypeEndDate: ""
  };
  activeRowId.value = null;
}

function onDialogClose () {
  if (isSaveDialog === true && isSaveStatusDialog === true && isSaveDepartmentDialog === true && isSaveDesignationDialog === true && isSaveOrgLocationDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onDialogCancel();
    }, () => {
    });
  } else {
    onDialogCancel();
  }
}

let isSaveStatusDialog = false;
let isConfirmSaveStatusDialog = false;
function onAddStatus () {
  let isAddContinue = 0;
  if (isConfirmSaveStatusDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onAddStatusConfirm();
    }, () => {
      isAddContinue = 0;
    });
  } else {
    isAddContinue = 1;
  }
  if (isAddContinue === 1) {
    onAddStatusConfirm();
  }
}

function onAddStatusConfirm () {
  isSaveStatusDialog = true;
  isConfirmSaveStatusDialog = true;
  rowCounter.value += 1; // Increment the counter
  mode.value = "addStatus";
  editingStatusRow.value = {
    employeeStatusId: "",
    employmentStatusStartDate: "",
    employmentStatusEndDate: ""
  };
  activeRowId.value = null;
}

// function onAddDepartment () {
//   mode.value = "addDepartment";
//   editingDeptRow.value = {
//     employeeDepartmentId: "",
//     departmentStartDateStr: "",
//     departmentEndDateStr: ""
//   };
//   activeRowId.value = null;
// }

let isSaveDepartmentDialog = false;
let isConfirmSaveDepartmentDialog = false;
function onAddDepartment () {
  let isAddContinue = 0;
  if (isConfirmSaveDepartmentDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onAddDepartmentConfirm();
    }, () => {
      isAddContinue = 0;
    });
  } else {
    isAddContinue = 1;
  }
  if (isAddContinue === 1) {
    onAddDepartmentConfirm();
  }
}

function onAddDepartmentConfirm () {
  isSaveDepartmentDialog = true;
  isConfirmSaveDepartmentDialog = true;
  rowCounter.value += 1; // Increment the counter
  mode.value = "addDepartment";
  editingDeptRow.value = {
    employeeDepartmentId: "",
    departmentStartDateStr: "",
    departmentEndDateStr: ""
  };
  activeRowId.value = null;
}

let isSaveDesignationDialog = false;
let isConfirmSaveDesignationDialog = false;
function onAddDesignation () {
   // Find the first designation row without End Date
  const rowWithoutEndDate = desgRows.value.find(
    row => !row.designationEndDateStr
  );

  if (rowWithoutEndDate) {
    mode.value = "editDesignation";
    activeRowId.value = rowWithoutEndDate.id;
    editingDesgRow.value = { ...rowWithoutEndDate };

    showAddButtonEndDateError.value = true;
    return;
  }

  showAddButtonEndDateError.value = false;
  let isAddContinue = 0;
  if (isConfirmSaveDesignationDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onAddDesignationConfirm();
    }, () => {
      isAddContinue = 0;
    });
  } else {
    isAddContinue = 1;
  }
  if (isAddContinue === 1) {
    onAddDesignationConfirm();
  }
}

function onAddDesignationConfirm () {
  isSaveDesignationDialog = true;
  isConfirmSaveDesignationDialog = true;
  rowCounter.value += 1; // Increment the counter
  mode.value = "addDesignation";
  editingDesgRow.value = {
    employeeDesignationId: "",
    shiftId: "",
    leaveApproverId: "",
    designationStartDateStr: "",
    designationEndDateStr: ""
  };
  activeRowId.value = null;
}

// function onAddOrgLocation () {
//   mode.value = "addOrgLocation";
//   editingOrgLocRow.value = {
//     orgLocationId: "",
//     orgLocationStartDateStr: "",
//     orgLocationEndDateStr: ""
//   };
//   activeRowId.value = null;
// }

let isSaveOrgLocationDialog = false;
let isConfirmSaveOrgLocationDialog = false;
function onAddOrgLocation () {
  let isAddContinue = 0;
  if (isConfirmSaveOrgLocationDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onAddOrgLocationConfirm();
    }, () => {
      isAddContinue = 0;
    });
  } else {
    isAddContinue = 1;
  }
  if (isAddContinue === 1) {
    onAddOrgLocationConfirm();
  }
}

function onAddOrgLocationConfirm () {
  isSaveOrgLocationDialog = true;
  isConfirmSaveOrgLocationDialog = true;
  rowCounter.value += 1; // Increment the counter
  mode.value = "addOrgLocation";
  editingOrgLocRow.value = {
    orgLocationId: "",
    orgLocationStartDateStr: "",
    orgLocationEndDateStr: ""
  };
  activeRowId.value = null;
}

// function onAddClientLocation () {
//   mode.value = "addClientLocation";
//   editingClientLocRow.value = {
//     clientLocationId: "",
//     clientLocationStartDateStr: "",
//     clientLocationEndDateStr: ""
//   };
//   activeRowId.value = null;
// }

function getPersonbyId () {
  personService.getPerson(model.value.personId).then((resp) => {
    model.value.firstName = resp.firstName;
    model.value.middleName = resp.middleName;
    model.value.lastName = resp.lastName;
    model.value.genderId = resp.gender.dropDownValue;
    model.value.primaryEmailAddress = resp.primaryEmailAddress;
    model.value.email = resp.email;
    model.value.countryId = resp.address.countryId;
    model.value.country = resp.address.addressCountry.name;
    model.value.primaryPhoneNumber = resp.primaryPhoneNumber;
    model.value.virtualPath = resp.picture.virtualPath ? resp.picture.virtualPath : "";
    model.value.addressTypeId = resp.addressType.dropDownValue;
    model.value.addressLine1 = resp.address.addressLine1;
    model.value.addressLine2 = resp.address.addressLine2;
    model.value.city = resp.address.city;
    model.value.zipCode = resp.address.zipCode;
    model.value.stateProvinceId = resp.address.addressStateProvince.name;
    model.value.employeeCode = resp.employeeCode;
  });
}

async function onSave () {
  if (mode.value === "edit") {
    if (!await editingRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveTypeDialog = false;
    const rowIndex = rows.value.findIndex((row) => row.id === editingRow.value.id);
    if (rowIndex !== -1) {
      rows.value.splice(rowIndex, 1, {
        ...rows.value[rowIndex],
        employeeTypeId: editingRow.value.employeeTypeId,
        startDateStr: editingRow.value.startDateStr,
        endDateStr: editingRow.value.endDateStr,
        duration: editingRow.value.duration,
        note: editingRow.value.note,
        flag: "Edit"
      });
      editingRow.value = null;
      mode.value = null;
      activeRowId.value = null;
    }
  } else if (mode.value === "editStatus") {
    if (!await editingStatusRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveStatusDialog = false;
    // check duplicate row
    let isDuplicate = 0;
    statusRows.value.forEach((item, index) => {
      if (item.employeeStatusId.toLowerCase() === editingStatusRow.value.employeeStatusId.toLowerCase() && item.id !== editingStatusRow.value.id) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const rowIndex = statusRows.value.findIndex((row) => row.id === editingStatusRow.value.id);
      if (rowIndex !== -1) {
        statusRows.value.splice(rowIndex, 1, {
          ...statusRows.value[rowIndex],
          employeeStatusId: editingStatusRow.value.employeeStatusId,
          statusStartDateStr: editingStatusRow.value.statusStartDateStr,
          statusEndDateStr: editingStatusRow.value.statusEndDateStr,
          duration: editingStatusRow.value.duration,
          note: editingStatusRow.value.note,
          flag: "Edit"
        });
        editingStatusRow.value = null;
        mode.value = null;
        activeRowId.value = null;
      }
    } else {
      notifyError({ message: "Duplicate Employee Status." });
    }
  } else if (mode.value === "editDepartment") {
    if (!await editingDeptRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveDepartmentDialog = false;
    // check duplicate row
    let isDuplicate = 0;
    deptRows.value.forEach((item, index) => {
      if (item.employeeDepartmentId.toLowerCase() === editingDeptRow.value.employeeDepartmentId.toLowerCase() && item.id !== editingDeptRow.value.id) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const rowIndex = deptRows.value.findIndex((row) => row.id === editingDeptRow.value.id);
      if (rowIndex !== -1) {
        deptRows.value.splice(rowIndex, 1, {
          ...deptRows.value[rowIndex],
          employeeDepartmentId: editingDeptRow.value.employeeDepartmentId,
          departmentStartDateStr: editingDeptRow.value.departmentStartDateStr,
          departmentEndDateStr: editingDeptRow.value.departmentEndDateStr,
          duration: editingDeptRow.value.duration,
          note: editingDeptRow.value.note,
          flag: "Edit"
        });
        editingDeptRow.value = null;
        mode.value = null;
        activeRowId.value = null;
      }
    } else {
      notifyError({ message: "Duplicate Employee Department." });
    }
  } else if (mode.value === "editDesignation") {
    if (!await editingDesgRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveDesignationDialog = false;
      const rowIndex = desgRows.value.findIndex((row) => row.id === editingDesgRow.value.id);
      if (rowIndex !== -1) {
        desgRows.value.splice(rowIndex, 1, {
          ...desgRows.value[rowIndex],
          employeeDesignationId: editingDesgRow.value.employeeDesignationId,
          shiftId: editingDesgRow.value.shiftId === "" ? null : editingDesgRow.value.shiftId,
          leaveApproverId: editingDesgRow.value.leaveApproverId,
          designationStartDateStr: editingDesgRow.value.designationStartDateStr,
          designationEndDateStr: editingDesgRow.value.designationEndDateStr,
          duration: editingDesgRow.value.duration,
          note: editingDesgRow.value.note,
          flag: "Edit"
        });
        editingDesgRow.value = null;
        mode.value = null;
        activeRowId.value = null;
      }
  } else if (mode.value === "editOrgLocation") {
    if (!await editingOrgLocRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveOrgLocationDialog = false;
    // check duplicate row
    let isDuplicate = 0;
    orglocRows.value.forEach((item, index) => {
      if (item.orgLocationId.toLowerCase() === editingOrgLocRow.value.orgLocationId.toLowerCase() && item.id !== editingOrgLocRow.value.id) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const rowIndex = orglocRows.value.findIndex((row) => row.id === editingOrgLocRow.value.id);
      if (rowIndex !== -1) {
        orglocRows.value.splice(rowIndex, 1, {
          ...orglocRows.value[rowIndex],
          orgLocationId: editingOrgLocRow.value.orgLocationId,
          orgLocationStartDateStr: editingOrgLocRow.value.orgLocationStartDateStr,
          orgLocationEndDateStr: editingOrgLocRow.value.orgLocationEndDateStr,
          duration: editingOrgLocRow.value.duration,
          note: editingOrgLocRow.value.note,
          flag: "Edit"
        });
        editingOrgLocRow.value = null;
        mode.value = null;
        activeRowId.value = null;
      }
    } else {
      notifyError({ message: "Duplicate Employee Org Location." });
    }
  } else if (mode.value === "editClientLocation") {
    const rowIndex = clientlocRows.value.findIndex((row) => row.id === editingClientLocRow.value.id);
    if (rowIndex !== -1) {
      clientlocRows.value.splice(rowIndex, 1, {
        ...clientlocRows.value[rowIndex],
        clientLocationId: editingClientLocRow.value.clientLocationId,
        clientLocationStartDateStr: editingClientLocRow.value.clientLocationStartDateStr,
        clientLocationEndDateStr: editingClientLocRow.value.clientLocationEndDateStr,
        flag: "Edit"
      });
      editingClientLocRow.value = null;
      mode.value = null;
      activeRowId.value = null;
    }
  } else if (mode.value === "add") {
    if (!await editingRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveTypeDialog = false;
    const newRow = {
      id: uid(),
      employeeTypeId: editingRow.value.employeeTypeId,
      startDateStr: editingRow.value.startDateStr,
      endDateStr: editingRow.value.endDateStr,
      duration: editingRow.value.duration,
      note: editingRow.value.note,
      flag: "New"
    };
    rows.value.unshift(newRow);
    mode.value = null;
    activeRowId.value = null;
  } else if (mode.value === "addStatus") {
    if (!await editingStatusRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveStatusDialog = false;
    // check duplicate row
    let isDuplicate = 0;
    statusRows.value.forEach((item, index) => {
      if (item.employeeStatusId.toLowerCase() === editingStatusRow.value.employeeStatusId.toLowerCase()) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const newRow = {
        id: uid(),
        employeeStatusId: editingStatusRow.value.employeeStatusId,
        statusStartDateStr: editingStatusRow.value.statusStartDateStr,
        statusEndDateStr: editingStatusRow.value.statusEndDateStr,
        duration: editingStatusRow.value.duration,
        note: editingStatusRow.value.note,
        flag: "New"
      };
      statusRows.value.unshift(newRow);
      mode.value = null;
      activeRowId.value = null;
    } else {
      notifyError({ message: "Duplicate Employee Status." });
    }
  } else if (mode.value === "addDepartment") {
    if (!await editingDeptRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveDepartmentDialog = false;
    // check duplicate row
    let isDuplicate = 0;
    deptRows.value.forEach((item, index) => {
      if (item.employeeDepartmentId.toLowerCase() === editingDeptRow.value.employeeDepartmentId.toLowerCase()) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const newRow = {
        id: uid(),
        employeeDepartmentId: editingDeptRow.value.employeeDepartmentId,
        departmentStartDateStr: editingDeptRow.value.departmentStartDateStr,
        departmentEndDateStr: editingDeptRow.value.departmentEndDateStr,
        duration: editingDeptRow.value.duration,
        note: editingDeptRow.value.note,
        flag: "New"
      };
      deptRows.value.unshift(newRow);
      mode.value = null;
      activeRowId.value = null;
    } else {
      notifyError({ message: "Duplicate Employee Department." });
    }
  } else if (mode.value === "addDesignation") {
    if (!await editingDesgRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveDesignationDialog = false;
      const newRow = {
        id: uid(),
        employeeDesignationId: editingDesgRow.value.employeeDesignationId,
        shiftId: editingDesgRow.value.shiftId === "" ? null : editingDesgRow.value.shiftId,
        leaveApproverId: editingDesgRow.value.leaveApproverId,
        designationStartDateStr: editingDesgRow.value.designationStartDateStr,
        designationEndDateStr: editingDesgRow.value.designationEndDateStr,
        duration: editingDesgRow.value.duration,
        note: editingDesgRow.value.note,
        flag: "New"
      };
      desgRows.value.unshift(newRow);
      mode.value = null;
      activeRowId.value = null;
  } else if (mode.value === "addOrgLocation") {
    if (!await editingOrgLocRowV$.value.$validate()) {
      return;
    }
    isConfirmSaveOrgLocationDialog = false;
    // check duplicate row
    let isDuplicate = 0;
    orglocRows.value.forEach((item, index) => {
      if (item.orgLocationId.toLowerCase() === editingOrgLocRow.value.orgLocationId.toLowerCase()) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const newRow = {
        id: uid(),
        orgLocationId: editingOrgLocRow.value.orgLocationId,
        orgLocationStartDateStr: editingOrgLocRow.value.orgLocationStartDateStr,
        orgLocationEndDateStr: editingOrgLocRow.value.orgLocationEndDateStr,
        duration: editingOrgLocRow.value.duration,
        note: editingOrgLocRow.value.note,
        flag: "New"
      };
      orglocRows.value.unshift(newRow);
      mode.value = null;
      activeRowId.value = null;
    } else {
      notifyError({ message: "Duplicate Employee Org Location." });
    }
  } else if (mode.value === "addClientLocation") {
    const newRow = {
      id: uid(),
      clientLocationId: editingClientLocRow.value.clientLocationId,
      clientLocationStartDateStr: editingClientLocRow.value.clientLocationStartDateStr,
      clientLocationEndDateStr: editingClientLocRow.value.clientLocationEndDateStr,
      flag: "New"
    };
    clientlocRows.value.unshift(newRow);
    mode.value = null;
    activeRowId.value = null;
  }
}

// Get row employee typee
function getEmployeeType (value) {
  if (value) {
    const found = employeeTypeDropdownSingleSelect.listFilter.value.find(
    item => item.value === value
  );
    return found?.text || "";
  }
}

function getEmployeeStatus (value) {
  if (value) {
    // return employeeStatusListArr.value.find((item) => item.id === value)?.dropdownValue;
    const found = employeeStatusDropdownSingleSelect.listFilter.value.find(item => item.value === value);
    return found?.text || "";
  }
}

function getEmployeeDepartment (value) {
  if (value) {
    // return employeeDepartmentListArr.value.find((item) => item.id === value)?.name;
    const found = departmentNameDropdownSingleSelect.listFilter.value.find(item => item.value === value);
    return found?.text || "";
  }
}

function getEmployeeDesignation (value) {
  if (value) {
    const found = employeeDesignationBySiteIdDropdownSingleSelect.listFilter.value.find(item => item.value === value);
    return found?.text || "";
  }
}

function getEmployeeShift (value) {
  if (value) {
    const found = employeeShiftDropdownSingleSelect.listFilter.value.find(item => item.value === value);
    return found?.text || "";
  }
}

function getEmployeeLeaveApprover (value) {
  if (value) {
    const found = activeEmployeesDropdownSingleSelect.listFilter.value.find(item => item.value === value);
    return found?.text || "";
  }
}

function getEmployeeOrgLocation (value) {
  if (value) {
    const found = employeeOrgLocationDropdownSingleSelect.listFilter.value.find(item => item.value === value);
    return found?.text || "";
  }
}

// ----------------------------------------------------------------------------------------------------------------
// Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshPersonNameDropdown = async () => {
  await personNameDropdownSingleSelect.load(selectedSiteId.value);
};

function onCancel () {
  isConfirmSaveTypeDialog = false;
  isConfirmSaveStatusDialog = false;
  isConfirmSaveDepartmentDialog = false;
  isConfirmSaveDesignationDialog = false;
  isConfirmSaveOrgLocationDialog = false;
  mode.value = null;
  editingRow.value = null;
  editingStatusRow.value = null;
  editingDeptRow.value = null;
  editingDesgRow.value = null;
  editingOrgLocRow.value = null;
  editingClientLocRow.value = null;
  activeRowId.value = null;
}

function onDelete (item) {
  isSaveDialog = true;
  item.deleted = true;
  const rowIndex = rows.value.findIndex((row) => row.id === item.id);
  if (rowIndex !== -1) {
    rows.value.splice(rowIndex, 1, {
      ...rows.value[rowIndex],
      id: item.id,
      employeeTypeId: item.employeeTypeId,
      employmentTypeStartDate: item.employmentTypeStartDate,
      employmentTypeEndDate: item.employmentTypeEndDate,
      flag: "Delete"
    });
  }
  activeRowId.value = item.id;
}

function onDeleteStatus (item) {
  isSaveStatusDialog = true;
  item.deleted = true;
  const rowIndex = statusRows.value.findIndex((row) => row.id === item.id);
  if (rowIndex !== -1) {
    statusRows.value.splice(rowIndex, 1, {
      ...statusRows.value[rowIndex],
      id: item.id,
      employeeStatusId: item.employeeStatusId,
      statusStartDate: item.statusStartDate,
      statusEndDate: item.statusEndDate,
      flag: "Delete"
    });
  }
  activeRowId.value = item.id;
}

function onDeleteDepartment (item) {
  isSaveDepartmentDialog = true;
  item.deleted = true;
  const rowIndex = deptRows.value.findIndex((row) => row.id === item.id);
  if (rowIndex !== -1) {
    deptRows.value.splice(rowIndex, 1, {
      ...deptRows.value[rowIndex],
      id: item.id,
      employeeDepartmentId: item.employeeDepartmentId,
      departmentStartDate: item.departmentStartDate,
      departmentEndDate: item.departmentEndDate,
      flag: "Delete"
    });
  }
  activeRowId.value = item.id;
}

function onDeleteDesignation (item) {
  isSaveDesignationDialog = true;
  item.deleted = true;
  const rowIndex = desgRows.value.findIndex((row) => row.id === item.id);
  if (rowIndex !== -1) {
    desgRows.value.splice(rowIndex, 1, {
      ...desgRows.value[rowIndex],
      id: item.id,
      employeeDesignationId: item.employeeDesignationId,
      shiftId: item.shiftId,
      leaveApproverId: item.leaveApproverId,
      designationStartDate: item.designationStartDate,
      designationEndDate: item.designationEndDate,
      flag: "Delete"
    });
  }
  activeRowId.value = item.id;
}

function onDeleteOrgLocation (item) {
  isSaveOrgLocationDialog = true;
  item.deleted = true;
  const rowIndex = orglocRows.value.findIndex((row) => row.id === item.id);
  if (rowIndex !== -1) {
    orglocRows.value.splice(rowIndex, 1, {
      ...orglocRows.value[rowIndex],
      id: item.id,
      orgLocationId: item.orgLocationId,
      orgLocationStartDate: item.orgLocationStartDate,
      orgLocationEndDate: item.orgLocationEndDate,
      flag: "Delete"
    });
  }
  activeRowId.value = item.id;
}

// function onDeleteClientLocation (item) {
//   item.deleted = true;
//   const rowIndex = clientlocRows.value.findIndex((row) => row.id === item.id);
//   if (rowIndex !== -1) {
//     clientlocRows.value.splice(rowIndex, 1, {
//       ...clientlocRows.value[rowIndex],
//       id: item.id,
//       clientLocationId: item.clientLocationId,
//       clientLocationStartDate: item.clientLocationStartDate,
//       clientLocationEndDate: item.clientLocationEndDate,
//       flag: "Delete"
//     });
//   }
//   activeRowId.value = item.id;
// }

function onEditRow (item) {
  showAddButtonEndDateError.value = false;
  let isContinue = 0;
  if (isConfirmSaveTypeDialog === true) {
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
  isConfirmSaveTypeDialog = true;
  mode.value = "edit";
  editingRow.value = _.cloneDeep(item);
  activeRowId.value = item.id;
}

function onEditStatusRow (item) {
  let isContinue = 0;
  if (isConfirmSaveStatusDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onEditStatusConfirm(item);
    }, () => {
      isContinue = 0;
    });
  } else {
    isContinue = 1;
  }
  if (isContinue === 1) {
    onEditStatusConfirm(item);
  }
}

function onEditStatusConfirm (item) {
  isSaveStatusDialog = true;
  isConfirmSaveStatusDialog = true;
  mode.value = "editStatus";
  editingStatusRow.value = _.cloneDeep(item);
  activeRowId.value = item.id;
}

function onEditDepartmentRow (item) {
  let isContinue = 0;
  if (isConfirmSaveDepartmentDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onEditDepartmentConfirm(item);
    }, () => {
      isContinue = 0;
    });
  } else {
    isContinue = 1;
  }
  if (isContinue === 1) {
    onEditDepartmentConfirm(item);
  }
}

function onEditDepartmentConfirm (item) {
  isSaveDepartmentDialog = true;
  isConfirmSaveDepartmentDialog = true;
  mode.value = "editDepartment";
  editingDeptRow.value = _.cloneDeep(item);
  activeRowId.value = item.id;
}

// function onEditDesignationRow (item) {
//   mode.value = "editDesignation";
//   editingDesgRow.value = _.cloneDeep(item);
//   activeRowId.value = item.id;
// }
function onEditDesignationRow (item) {
  showAddButtonEndDateError.value = false;
  let isContinue = 0;
  if (isConfirmSaveDesignationDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onEditDesignationConfirm(item);
    }, () => {
      isContinue = 0;
    });
  } else {
    isContinue = 1;
  }
  if (isContinue === 1) {
    onEditDesignationConfirm(item);
  }
}

function onEditDesignationConfirm (item) {
  isSaveDesignationDialog = true;
  isConfirmSaveDesignationDialog = true;
  mode.value = "editDesignation";
  editingDesgRow.value = _.cloneDeep(item);
  activeRowId.value = item.id;
}

// function onEditOrgLocationRow (item) {
//   mode.value = "editOrgLocation";
//   editingOrgLocRow.value = _.cloneDeep(item);
//   activeRowId.value = item.id;
// }
function onEditOrgLocationRow (item) {
  let isContinue = 0;
  if (isConfirmSaveOrgLocationDialog === true) {
    zwConfirmLeave({ data: "" }, () => {
      onEditOrgLocationConfirm(item);
    }, () => {
      isContinue = 0;
    });
  } else {
    isContinue = 1;
  }
  if (isContinue === 1) {
    onEditOrgLocationConfirm(item);
  }
}

function onEditOrgLocationConfirm (item) {
  isSaveOrgLocationDialog = true;
  isConfirmSaveOrgLocationDialog = true;
  mode.value = "editOrgLocation";
  editingOrgLocRow.value = _.cloneDeep(item);
  activeRowId.value = item.id;
}

function onUndo (item) {
  item.deleted = false;
  activeRowId.value = null;
}

function disableBeforeStartDate (date) {
  // If no Start Date is set, allow all dates
  if (!editingRow.value.startDateStr) {
    return true;
  }
  const start = new Date(editingRow.value.startDateStr);
  const current = new Date(date);

  // Disable dates before the Start Date
  return current > start;
}

function disableBeforeStatusStartDate (date) {
  // If no Start Date is set, allow all dates
  if (!editingStatusRow.value.statusStartDateStr) {
    return true;
  }
  const start = new Date(editingStatusRow.value.statusStartDateStr);
  const current = new Date(date);

  // Disable dates before the Start Date
  return current > start;
}

function disableBeforeDepStartDate (date) {
  // If no Start Date is set, allow all dates
  if (!editingDeptRow.value.departmentStartDateStr) {
    return true;
  }
  const start = new Date(editingDeptRow.value.departmentStartDateStr);
  const current = new Date(date);

  // Disable dates before the Start Date
  return current > start;
}

function disableBeforDesignationStartDate (date) {
  // If no Start Date is set, allow all dates
  if (!editingDesgRow.value.designationStartDateStr) {
    return true;
  }
  const start = new Date(editingDesgRow.value.designationStartDateStr);
  const current = new Date(date);

  // Disable dates before the Start Date
  return current > start;
}

function disableBeforOrgStartDate (date) {
  // If no Start Date is set, allow all dates
  if (!editingOrgLocRow.value.orgLocationStartDateStr) {
    return true;
  }
  const start = new Date(editingOrgLocRow.value.orgLocationStartDateStr);
  const current = new Date(date);

  // Disable dates before the Start Date
  return current > start;
}

function disableBeforeReleaseDate (date) {
  // If no Start Date is set, allow all dates
  if (!model.value.joiningDateStr) {
    return true;
  }
  const start = new Date(model.value.joiningDateStr);
  const current = new Date(date);

  // Disable dates before the Start Date
  return current >= start;
}

// Create popup

const onAddPerson = () => {
  $q.dialog({
    component: editPerson,
    componentProps: {
      siteId: selectedSiteId.value
    }
  }).onOk(async () => {
    await refreshPersonNameDropdown();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Edit popup
const onEdit = (id) => {
  $q.dialog({
    component: editPerson,
    componentProps: { id }
  }).onOk(async () => {
    await refreshPersonNameDropdown();
    getPersonbyId();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

const onSubmitClose = () => {
  onSubmit(1);
};

// const selectedCountry = computed(() => {
//   return countryList.value.find(country => country.id === currentaddrmodel.value.countryId)?.name || "";
// });

const selectedCountry = computed(() => {
  const selectedOption = countryNameDropdownSingleSelect.list.value.find(
    country => country.value === currentaddrmodel.value.countryId
  );
  return selectedOption ? selectedOption.text : null;
});

// let currAddr = null;
// function OnCheckAddress (value) {
//   if (value) {
//     currAddr = crtaddrv$.value.$validate();
//     // Do something when checked
//   } else {
//     currAddr = crtaddrv$.value.$reset();
//     // Do something when unchecked
//   }
// }
// Submit form

const onSubmit = async (isClose = 0) => {
  if (isClose === 1) {
    processingClose.value = true;
    processing.value = false;
  } else {
    processing.value = true;
  }
  try {
  // const addressValidation = currAddr ? await crtaddrv$.value.$validate() : true;
    if (model.value.sameASPermanentAddress === true && model.value.addressLine1 == null) {
      notifyError({ message: "Permanent address is blank" });
      return;
    }
    if ((!await v$.value.$validate() || (model.value.sameASPermanentAddress === false && !await crtaddrv$.value.$validate()))) {
      notifyError({ message: "Required fields are blank in Employee Info. Tab" });
      return;
    }
    if ((mode.value === "edit" || mode.value === "add" || mode.value === "editStatus" || mode.value === "addStatus" || mode.value === "editDepartment" || mode.value === "addDepartment" || mode.value === "editDesignation" || mode.value === "addDesignation" || mode.value === "editOrgLocation" || mode.value === "addOrgLocation")) {
      return;
    }
    model.value.addressLine1 = currentaddrmodel.value.addressLine1;
    model.value.addressLine2 = currentaddrmodel.value.addressLine2;
    model.value.city = currentaddrmodel.value.city;
    model.value.countryId = currentaddrmodel.value.countryId;
    model.value.stateProvinceId = currentaddrmodel.value.stateProvinceId;
    model.value.zipCode = currentaddrmodel.value.zipCode;
    model.value.tab = tab;
    model.value.employeeTypeModel = rows.value;
    model.value.employeeStatusModel = statusRows.value;
    model.value.employeeDepartmentModel = deptRows.value;
    model.value.employeeDesignationModel = desgRows.value;
    model.value.employeeOrgLocationModel = orglocRows.value;
    model.value.employeeClientLocationModel = clientlocRows.value;
    model.value.siteId = selectedSiteId.value;
    employeesService.saveEmployee(employeeId, model.value).then((resp) => {
      notifySuccess({ message: "Employee is saved successfully." });
      // employeeId.value = resp;
      employeeId = resp.id;
      disableTab = false;
      getEmployee(employeeId);
      if (isClose === 1) {
        onDialogOK();
      } else {
        const currentTab = tab.value;
        switch (currentTab) {
        case "1_tab":
          tab.value = "2_tab";
          getPersonbyId();
          break;
        case "2_tab":
          tab.value = "3_tab";
          break;
        default:
          break;
        }
      }
      getEmployee(employeeId);
    // onDialogOK();
    });
  } catch (error) {
    console.error("Error in submitting the employee:", error);
    notifyError({ message: "An error occurred while saving the employee." });
  } finally {
    if (isClose === 1) {
      processingClose.value = true;
      processing.value = false;
    } else {
      processing.value = true;
    }

    setTimeout(() => {
      processing.value = false;
      processingClose.value = false;
    }, 1500);
  }
};

watch(() => employeeId, (newValue, oldValue) => {
  if (newValue) {
    getEmployee(employeeId);
  }
}, { immediate: true });

watch(() => model.value.personId, (newValue, oldValue) => {
  if (newValue) {
    getPersonbyId();
  }
}, { immediate: false });

// let employeeCodeBase = 16151;
// watch(() => model.value.personId, (newValue, oldValue) => {
//   if (newValue) {
//     getPersonbyId();
//     // model.value.employeeCode = String(employeeCodeBase++);
//     model.value.employeeCode = employeeCodeBase++;
//   }
// }, { immediate: false });

// watches a data property with the same name i.e. immediate effect
watch(() => currentaddrmodel.value.countryId, (newValue, oldValue) => {
  currentaddrmodel.value.stateProvinceId = newValue !== oldValue && oldValue !== null ? "" : currentaddrmodel.value.stateProvinceId;
  currentaddrmodel.value.zipCode = newValue !== oldValue && oldValue !== null ? "" : currentaddrmodel.value.zipCode;
  if (newValue) {
    maxLengthZip = baseCountryId === currentaddrmodel.value.countryId ? "5" : "6";
    stateNameDropdownSingleSelect.load(newValue);
    crtaddrrules = {
      countryId: { required: helpers.withMessage("Country is required", required) },
      stateProvinceId: { required: helpers.withMessage("State is required", required) },
      addressLine1: { maxLength: maxLength(500), required: helpers.withMessage("Address Line 1 is required", required) },
      addressLine2: { maxLength: maxLength(500) },
      city: { maxLength: maxLength(500), required: helpers.withMessage("City is required", required) },
      zipCode: { minLength: minLength(maxLengthZip), maxLength: maxLength(maxLengthZip), required: helpers.withMessage(maxLengthZip === "5" ? "Zip code is required" : "Pin code is required", required) }
    };
    // Validate rules
    crtaddrv$ = useVuelidate(crtaddrrules, currentaddrmodel, { $lazy: true, $autoDirty: true });
  }
}, { immediate: false });

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  await personNameDropdownSingleSelect.load(selectedSiteId.value);
  await employeeTypeDropdownSingleSelect.load(selectedSiteId.value, "EmploymentType");
  await employeeStatusDropdownSingleSelect.load(selectedSiteId.value, "Employee Status");
  await employeeDesignationBySiteIdDropdownSingleSelect.load(selectedSiteId.value, "Employee Designation");
  await employeeOrgLocationDropdownSingleSelect.load(selectedSiteId.value, "Employee OrgLocation");
  await activeEmployeesDropdownSingleSelect.load(selectedSiteId.value);
  employeeShiftDropdownSingleSelect.load(selectedSiteId.value, "Employee Shift");
  departmentNameDropdownSingleSelect.load();
  countryNameDropdownSingleSelect.load();

  // remove "All" from main list
  employeeStatusDropdownSingleSelect.list.value = employeeStatusDropdownSingleSelect.list.value.filter(
    item => item.text !== "All"
  );
});

</script>

<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_employee .q-select__dropdown-icon{
  display: none;
}
</style>
