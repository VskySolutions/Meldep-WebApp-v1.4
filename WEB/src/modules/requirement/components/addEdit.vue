<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw !important;max-width: 60vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Requirement</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable', readonlyRequirement != '' ? 'edit_requirement' : '']">
          <div class="q-gutter-y-md">
            <input ref="saveAndContinueStatus" type="hidden" :value="saveStatus">
            <fieldset>
              <legend>Requirement Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.projectId"
                    label="Project Name"
                    :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement' && readonlyProject != ''"
                    :options="projectNameDropdownSingleSelect.list.value"
                    :filter="projectNameDropdownSingleSelect.filter"
                    :error="v$.projectId.$error"
                    :error-message="v$.projectId.$errors[0]?.$message"
                  />
                </div>
                <!-- <div class="col-12 col-sm-6 col-md-6 hidden">
                  <label class="label q-mb-xs text-black">Requirement Group Name<span class="required">*</span></label>
                  <div>
                    <q-select
                      v-model="model.requirementGroupId" clearable use-input outlined stack-label hide-bottom-space :dense="true" :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement' && readonlyRequirementsName !== ''"
                      :options="requirementGroupList" option-value="value" option-label="text" emit-value map-options :error="v$.requirementGroupId.$error"
                      :error-message="v$.requirementGroupId.$errors[0]?.$message" @filter="filterFn2" @blur="v$.requirementGroupId.$touch"
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
                </div> -->
                <div class="col-12 col-sm-6 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.projectModuleId"
                    label="Project Module"
                    :class="readonlyProjectModule !== '' ? 'edit_tasks' : ''"
                    :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement' && readonlyProjectModule != ''"
                    :disable="!model.projectId"
                    :options="projectModulesByProjectIdForDropdownSingleSelect.list.value"
                    :filter="projectModulesByProjectIdForDropdownSingleSelect.filter"
                    :error="v$.projectModuleId.$error"
                    :error-message="v$.projectModuleId.$errors[0]?.$message"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-12 col-md-12">
                  <label class="q-mb-xs text-black">Requirement Title<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.title"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement'"
                      autogrow
                      :error="v$.title.$error"
                      :error-message="v$.title.$errors[0]?.$message"
                      @blur="v$.title.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                  <formSingleSelectDropdown
                    v-model="model.requirementTypeId"
                    label="Type"
                    :required="false"
                    :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement'"
                    :options="requirementTypeDropdownSingleSelect.list.value"
                    :filter="requirementTypeDropdownSingleSelect.filter"
                  />
                </div>
                <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                  <formSingleSelectDropdown
                    v-model="model.areaId"
                    label="Area"
                    :required="false"
                    :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement'"
                    :options="areaForDropdownSingleSelect.list.value"
                    :filter="areaForDropdownSingleSelect.filter"
                  />
                </div>
                <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                  <formSingleSelectDropdown
                    v-model="model.workspaceId"
                    label="Workspace"
                    :required="false"
                    :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement'"
                    :options="workspaceForDropdownSingleSelect.list.value"
                    :filter="workspaceForDropdownSingleSelect.filter"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.identifiedUserType"
                    label="Requirement Identifier"
                    :required="false"
                    :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement'"
                    :options="requirementIdentifiedUserTypeDropdownSingleSelect.list.value"
                    :filter="requirementIdentifiedUserTypeDropdownSingleSelect.filter"
                  />
                </div>
                <div v-if="identifiedUserTypeText === 'Employee'" class="col-12 col-sm-6 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.identifiedEmployeeId"
                    label="Employee Name"
                    :required="false"
                    :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement'"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                  />
                </div>
                <div v-if="identifiedUserTypeText === 'Customer'" class="col-12 col-sm-6 col-md-6">
                  <formSingleSelectDropdown
                    v-model="model.identifiedCustomerId"
                    label="Customer Name"
                    :required="false"
                    :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement'"
                    :options="customerContactDropdownSingleSelect.list.value"
                    :filter="customerContactDropdownSingleSelect.filter"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <formDate
                  v-model="model.IdentifiedDateStr"
                  label="Requirement Identified Date"
                  :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement'"
                  :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                  :error="v$.IdentifiedDateStr.$error"
                  :error-message="v$.IdentifiedDateStr.$errors[0]?.$message"
                  :onBlur="() => v$.IdentifiedDateStr.$touch()"
                />
                <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                  <formSingleSelectDropdown
                    v-model="model.requirementEnteredBy"
                    label="Requirement Entered By"
                    :required="false"
                    :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement'"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                  />
                </div>
                <div class="col-12 col-sm-6 col-md-3 col-lg-3 hidden">
                  <formSingleSelectDropdown
                    v-model="model.statusId"
                    label="Requirement Status"
                    :required="false"
                    :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement'"
                    :options="requirementStatusDropdownSingleSelect.list.value"
                    :filter="requirementStatusDropdownSingleSelect.filter"
                    :error="v$.statusId.$error"
                    :error-message="v$.statusId.$errors[0]?.$message"
                  />
                </div>
                <div v-if="statusText === 'Close'" class="col-12 col-sm-6 col-md-3 col-lg-3">
                  <formDate
                    v-model="model.closeDateStr"
                    label="Requirement Close Date"
                    :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement'"
                    :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                    :error="v$.closeDateStr.$error"
                    :error-message="v$.closeDateStr.$errors[0]?.$message"
                    :onBlur="() => v$.closeDateStr.$touch()"
                  />
                </div>
                <div class="col-12 col-sm-6 col-md-6 col-lg-4">
                  <formSingleSelectDropdown
                    v-model="model.approvalStatus"
                    label="Approval Status"
                    :required="false"
                    :readonly="readonlyRequirement != '' ? '' : 'readonlyRequirement'"
                    :options="requirementApprovalStatusDropdownSingleSelect.list.value"
                    :filter="requirementApprovalStatusDropdownSingleSelect.filter"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="form-group">
                    <label class="q-mb-xs text-black">Description</label>
                    <div
                      v-if="readonlyRequirement !== ''"
                      class="readonly-description q-pa-sm q-mb-sm q-field__native q-input__control bg-grey-2 text-black RichTextEditor"
                    >
                      <!-- Description text -->
                      <span v-html="model.description" />
                    </div>
                    <q-editor
                      v-else
                      v-model="model.description"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
                <div class="col-12 col-sm-6 col-md-6 hidden">
                  <div class="form-group">
                    <label class="q-mb-xs text-black">Note</label>
                    <q-input
                      v-model="model.notes"
                      outlined
                      stack-label
                      type="textarea"
                      hide-bottom-space
                      :dense="true"
                      :readonly="readonlyRequirement != ''"
                      maxlength="128"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset class="q-mb-lg">
              <legend>Document Reference List</legend>
              <div class="flex items-center justify-end q-mb-md">
                <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAddDocumentReference" />
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
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th
                      v-for="col in props.cols"
                      :key="col.name" :props="props"
                    >
                      {{ col.label }}
                      <span v-if="['filePath','fileName'].includes(col.name)" class="required">*</span>
                    </q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #top-row>
                  <q-tr v-if="mode == 'addDocumentReference' && editingRow" class="row-highlight">
                    <q-td>
                      <div>
                        <q-input
                          v-model="editingRow.filePath"
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          :error="editingRowV$.filePath.$error"
                          :error-message="editingRowV$.filePath.$errors[0]?.$message"
                          @blur="editingRowV$.filePath.$touch"
                        />
                      </div>
                    </q-td>
                    <q-td>
                      <div>
                        <q-input
                          v-model="editingRow.fileName"
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          :error="editingRowV$.fileName.$error"
                          :error-message="editingRowV$.fileName.$errors[0]?.$message"
                          @blur="editingRowV$.fileName.$touch"
                        />
                      </div>
                    </q-td>
                    <q-td style="width: 350px;">
                      <div>
                        <q-input
                          v-model="editingRow.note"
                          outlined
                          stack-label
                          type="textarea"
                          hide-bottom-space
                          :dense="true"
                          maxlength="128"
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
                    <q-td class="text-left" style="width: 40%;">
                      <q-input
                        v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                        v-model="editingRow.filePath"
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        :error="editingRowV$.filePath.$error"
                        :error-message="editingRowV$.filePath.$errors[0]?.$message"
                        @blur="editingRowV$.filePath.$touch"
                      />
                      <span
                        v-else :class="props.row.deleted ? 'text-delete' : ''"
                        style="white-space: normal; word-break: break-word;"
                      >
                        {{ props.row.filePath }}
                      </span>
                    </q-td>
                    <q-td class="text-left" style="width: 25%;">
                      <q-input
                        v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                        v-model="editingRow.fileName"
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        :error="editingRowV$.fileName.$error"
                        :error-message="editingRowV$.fileName.$errors[0]?.$message"
                        @blur="editingRowV$.fileName.$touch"
                      />
                      <span
                        v-else :class="props.row.deleted ? 'text-delete' : ''"
                        style="white-space: normal; word-break: break-word;"
                      >
                        {{ props.row.fileName }}
                      </span>
                    </q-td>
                    <q-td class="text-left">
                      <q-input
                        v-if="mode == 'edit' && editingRow && props.row.id === activeRowId"
                        v-model="editingRow.note"
                        outlined
                        stack-label
                        type="textarea"
                        hide-bottom-space
                        :dense="true"
                        maxlength="128"
                      />
                      <span
                        v-else :class="props.row.deleted ? 'text-delete' : ''"
                        style="display: block; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;"
                        v-html="props.row.note"
                      />
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
                        <q-icon v-if="!props.row.deleted" name="o_delete_outline" color="red" size="xs" class="cursor-pointer" @click="onDeleteDocumentReference(props.row)">
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
            <fieldset v-if="props.id" class="q-mb-lg">
              <legend>Requirement Change Log</legend>
              <div class="flex items-center justify-end q-mb-md">
                <q-btn color="primary" icon="o_add" label="Add" no-caps :disable="readonlyRequirement === ''" @click="onAddChangeLog" />
              </div>
              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                bordered class="no-shadow"
                :loading="loading"
                :rows="logrows"
                :columns="logcolumns"
                row-key="id"
                separator="cell"
                no-data-label="No data available"
                binary-state-sort
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th
                      v-for="col in props.cols"
                      :key="col.name" :props="props"
                    >{{ col.label }}
                      <span v-if="['requirementLogDate','employeeId','requirementName'].includes(col.name)" class="required">*</span>
                    </q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #top-row>
                  <q-tr v-if="mode == 'addChangeLog' && editingLogRow" class="row-highlight">
                    <q-td>
                      <formDate
                        v-model="editingLogRow.requirementLogDateStr"
                        :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                        :error="editingLogRowV$.requirementLogDateStr.$error"
                        :error-message="editingLogRowV$.requirementLogDateStr.$errors[0]?.$message"
                        :onBlur="() => editingLogRowV$.requirementLogDateStr.$touch()"
                      />
                      <!-- <q-input
                        v-model="editingLogRow.requirementLogDateStr"
                        outlined
                        stack-label
                        hide-bottom-space
                        mask="##/##/####"
                        dense
                        :error="editingLogRowV$.requirementLogDateStr.$error"
                        :error-message="editingLogRowV$.requirementLogDateStr.$errors[0]?.$message"
                        @blur="editingLogRowV$.requirementLogDateStr.$touch"
                      >
                        <template #append>
                          <q-icon name="o_calendar_month" class="cursor-pointer">
                            <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                              <q-date v-model="editingLogRow.requirementLogDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                            </q-popup-proxy>
                          </q-icon>
                        </template>
                      </q-input> -->
                    </q-td>
                    <q-td>
                      <div>
                        <formSingleSelectDropdown
                          v-model="editingLogRow.employeeId"
                          :options="activeEmployeesDropdownSingleSelect.list.value"
                          :filter="activeEmployeesDropdownSingleSelect.filter"
                          :error="editingLogRowV$.employeeId.$error"
                          :error-message="editingLogRowV$.employeeId.$errors[0]?.$message"
                        />
                      </div>
                    </q-td>
                    <q-td>
                      <div>
                        <q-input
                          v-model="editingLogRow.requirementName"
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          :error="editingLogRowV$.requirementName.$error"
                          :error-message="editingLogRowV$.requirementName.$errors[0]?.$message"
                          @blur="editingLogRowV$.requirementName.$touch"
                        />
                      </div>
                    </q-td>
                    <q-td>
                      <q-editor
                        v-model="editingLogRow.description"
                        :dense="$q.screen.lt.md"
                        :toolbar="toolbar"
                        :fonts="fonts"
                        style="width: 350px;"
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
                    <q-td class="text-left">
                      <formDate
                        v-if="mode == 'editLog' && editingLogRow && props.row.id === activeRowId"
                        v-model="editingLogRow.requirementLogDateStr"
                        :wrapperClass="'col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12'"
                        :error="editingLogRowV$.requirementLogDateStr.$error"
                        :error-message="editingLogRowV$.requirementLogDateStr.$errors[0]?.$message"
                        :onBlur="() => editingLogRowV$.requirementLogDateStr.$touch()"
                      />
                      <!-- <q-input
                        v-if="mode == 'editLog' && editingLogRow && props.row.id === activeRowId"
                        v-model="editingLogRow.requirementLogDateStr"
                        outlined
                        stack-label
                        hide-bottom-space
                        mask="##/##/####"
                        dense
                        :error="editingLogRowV$.requirementLogDateStr.$error"
                        :error-message="editingLogRowV$.requirementLogDateStr.$errors[0]?.$message"
                        @blur="editingLogRowV$.requirementLogDateStr.$touch"
                      >
                        <template #append>
                          <q-icon name="o_calendar_month" class="cursor-pointer">
                            <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                              <q-date v-model="editingLogRow.requirementLogDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                            </q-popup-proxy>
                          </q-icon>
                        </template>
                      </q-input> -->
                      <span
                        v-else :class="props.row.deleted ? 'text-delete' : ''"
                      >
                        <span> {{ props.row.createdOnUtc }}
                        </span>
                      </span>
                    </q-td>
                    <q-td class="text-left">
                      <formSingleSelectDropdown
                        v-if="mode == 'editLog' && editingLogRow && props.row.id === activeRowId"
                        v-model="editingLogRow.employeeId"
                        :options="activeEmployeesDropdownSingleSelect.list.value"
                        :filter="activeEmployeesDropdownSingleSelect.filter"
                        :error="editingLogRowV$.employeeId.$error"
                        :error-message="editingLogRowV$.employeeId.$errors[0]?.$message"
                      />
                      <span
                        v-else :class="props.row.deleted ? 'text-delete' : ''"
                      >
                        {{ getEmployee(props.row.employeeId) }}
                      </span>
                    </q-td>
                    <q-td class="text-left">
                      <q-input
                        v-if="mode == 'editLog' && editingLogRow && props.row.id === activeRowId"
                        v-model="editingLogRow.requirementName"
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        :error="editingLogRowV$.requirementName.$error"
                        :error-message="editingLogRowV$.requirementName.$errors[0]?.$message"
                        @blur="editingLogRowV$.requirementName.$touch"
                      />
                      <span
                        v-else :class="props.row.deleted ? 'text-delete' : ''"
                        style="display: block; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;"
                      >
                        {{ props.row.requirementName }}
                      </span>
                    </q-td>
                    <q-td class="text-left">
                      <q-editor
                        v-if="mode == 'editLog' && editingLogRow && props.row.id === activeRowId"
                        v-model="editingLogRow.description"
                        :dense="$q.screen.lt.md"
                        :toolbar="toolbar"
                        :fonts="fonts"
                      />
                      <span
                        v-else :class="props.row.deleted ? 'text-delete RichTextEditor' : 'RichTextEditor'"
                        style="display: block; overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 350px;"
                        v-html="props.row.description"
                      />
                    </q-td>
                    <q-td auto-width class="text-center">
                      <template v-if="mode == 'editLog' && editingLogRow && props.row.id === activeRowId">
                        <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                          <q-tooltip>Save</q-tooltip>
                        </q-icon>
                        <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                          <q-tooltip>Cancel</q-tooltip>
                        </q-icon>
                      </template>
                      <template v-else>
                        <q-icon v-if="!props.row.deleted" name="o_delete_outline" color="red" size="xs" class="cursor-pointer" @click="onDeleteLog(props.row)">
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
        <q-separator />
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn same-size-btn" no-caps @click="onDialogCancel" />
          <q-btn v-if="model.editingStatus === 1 || !props.id" color="primary" push outline label="Save as Draft and Continue" class="actionBtn same-size-btn" :loading="processing && activeButton === 'saveDraftAndContinue'" no-caps @click="handleSave('saveDraftAndContinue')" />
          <q-btn v-if="model.editingStatus === 1 || !props.id" color="primary" push outline label="Save as Draft and Close" class="actionBtn same-size-btn" :loading="processing && activeButton === 'saveAndDraft'" no-caps @click="handleSave('saveAndDraft')" />
          <q-btn color="primary" push outline :label="model.editingStatus === 2 ? 'Update & Close' : 'Confirm and Close'" class="actionBtn same-size-btn" :loading="processing && activeButton === 'confirmAndClose'" no-caps @click="handleSave('confirmAndClose')" />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent, useQuasar, uid } from "quasar";
import { required, helpers, minLength, maxLength } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import { notifySuccess, notifyError, getLocalStorage } from "assets/utils";
import { isDate } from "validators/zw_validators.js";
import { useAuthStore } from "stores/auth";
import _ from "lodash";
import useVuelidate from "@vuelidate/core";
import { format } from "date-fns"; // Standard TimeZone Conversion

import requirementService from "../requirement.service";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// Shared Dropdowns
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------

const props = defineProps({ id: { type: String, default: "" }, projectIdAttr: { type: String, default: "" }, projectIdValue: { type: String, default: "" }, moduleIdAttr: { type: String, default: "" } });
const readonlyProject = props.projectIdAttr ? "readonly" : "";
const readonlyProjectModule = props.moduleIdAttr ? "readonly" : "";
let requirementId = props.id;

// ----------------------------------------------------------------------------------------------------------------
// Define emits
// ----------------------------------------------------------------------------------------------------------------

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const loading = ref(true);
const saveStatus = ref(0);
const rows = ref([]);
const logrows = ref([]);
const mode = ref(null);
const activeRowId = ref(null);
const editingRow = ref(null);
const editingLogRow = ref(null);
const processing = ref(false);
const activeButton = ref("");
const $q = useQuasar();
const authStore = useAuthStore();
const user = authStore.user;
const { fonts, toolbar } = getEditorConfig($q);

// ----------------------------------------------------------------------------------------------------------------
// Define columns for File path and Requirement Change log
// ----------------------------------------------------------------------------------------------------------------

const columns = ref([
  { name: "filePath", label: "File Path", field: "filePath", align: "left", sortable: true },
  { name: "fileName", label: "File Name", field: "fileName", align: "left", sortable: true },
  { name: "note", label: "Notes", field: "note", align: "left", sortable: true }
]);

const logcolumns = ref([
  { name: "requirementLogDate", label: "Change Date", field: "requirementLogDate", align: "left", sortable: true },
  { name: "employeeId", label: "Change By", field: "employeeId", align: "left", sortable: true },
  { name: "requirementName", label: "Requirement Change", field: "requirementName", align: "left", sortable: true },
  { name: "description", label: "Description", field: "description", align: "left", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const localStorageKey = "Requirement";
const filterLocalStorage = getLocalStorage(localStorageKey);
const projectIds = filterLocalStorage ? filterLocalStorage.projectIds[0] : [];
const projectModuleIds = filterLocalStorage ? filterLocalStorage.projectModuleIds[0] : [];
// const requirementGroupIds = filterLocalStorage ? filterLocalStorage.requirementGroupIds[0] : [];

// ----------------------------------------------------------------------------------------------------------------
// Define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  projectId: props.projectIdAttr !== "" ? props.projectIdAttr : (props.projectIdValue !== "" ? props.projectIdValue : (projectIds !== "" ? projectIds : null)),
  projectModuleId: props.moduleIdAttr !== "" ? props.moduleIdAttr : (projectModuleIds !== "" ? projectModuleIds : null),
  title: "",
  areaId: null,
  workspaceId: null,
  notes: "",
  identifiedUserType: "",
  employeeId: "",
  identifiedEmployeeId: user?.employeeId ? user.employeeId : "",
  IdentifiedDateStr: format(new Date(), "MM/dd/yyyy"),
  requirementEnteredBy: user?.employeeId ? user.employeeId : "",
  statusId: "",
  approvalStatus: "",
  closeDateStr: format(new Date(), "MM/dd/yyyy"),
  description: "",
  editingStatus: 0,
  status: {
    dropDownValue: ""
  }
});

// ----------------------------------------------------------------------------------------------------------------
// Requirement Info - Validation Rules
// ----------------------------------------------------------------------------------------------------------------

const rules = {
  projectId: { required: helpers.withMessage("Project name is required", required) },
  projectModuleId: { required: helpers.withMessage("Project module is required", required) },
  // identifiedUserType: { required: helpers.withMessage("Requirement Identifier is required", required) },
  // identifiedEmployeeId: model.value.identifiedUserType === "Employee"
  //   ? { required: helpers.withMessage("Employee name is required", required) }
  //   : {},
  // identifiedCustomerId: model.value.identifiedUserType === "Customer"
  //   ? { required: helpers.withMessage("Employee name is required", required) }
  //   : {},
  statusId: { required: helpers.withMessage("Requirement Status is required", required) },
  title: { required: helpers.withMessage("Title is required", required), minLength: minLength(1), maxLength: maxLength(200) },
  IdentifiedDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  closeDateStr: {
    isDate: helpers.withMessage("Date is invalid", isDate)
  }
};

// ----------------------------------------------------------------------------------------------------------------
// Document Reference List - Validation Rules
// ----------------------------------------------------------------------------------------------------------------

const editingRowrules = {
  filePath: { required: helpers.withMessage("File Path is required", required), minLength: minLength(1), maxLength: maxLength(200) },
  fileName: { required: helpers.withMessage("File Name is required", required) }
};

// ----------------------------------------------------------------------------------------------------------------
// Requirement Change Log - Validation Rules
// ----------------------------------------------------------------------------------------------------------------

const editingLogRowrules = {
  employeeId: { required: helpers.withMessage("Change By Name is required", required), minLength: minLength(1), maxLength: maxLength(200) },
  requirementName: { required: helpers.withMessage("Requirement Name is required", required) },
  requirementLogDateStr: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  }
};

// ----------------------------------------------------------------------------------------------------------------
// Computed property to get the imageTypeText's text
// ----------------------------------------------------------------------------------------------------------------

const statusText = computed(() => {
  const selectedOption = requirementStatusDropdownSingleSelect.list.value.find(
    item => item.value === model.value.statusId
  );
  return selectedOption ? selectedOption.text : null;
});

// ----------------------------------------------------------------------------------------------------------------
// Get Requirement Details
// ----------------------------------------------------------------------------------------------------------------

const getRequirement = () => {
  loading.value = true;
  requirementService.getRequirementDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.IdentifiedDateStr = resp.identifiedDate ? format(resp.identifiedDate, "MM/dd/yyyy") : "";
    model.value.closeDateStr = resp.closeDate ? format(resp.closeDate, "MM/dd/yyyy") : "";
    rows.value = resp.filePathDetails.map(item => ({
      ...item,
      editing: false,
      flag: "Edit"
    }));
    logrows.value = resp.requirementChangeLog.map(item => ({
      ...item,
      requirementLogDateStr: format(item.requirementLogDate, "MM/dd/yyyy"),
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

const readonlyRequirement = computed(() => {
  return model.value.editingStatus === 2 ? "readonly" : "";
});

// ----------------------------------------------------------------------------------------------------------------
// Custom Functions
// ----------------------------------------------------------------------------------------------------------------

function onAddDocumentReference () {
  mode.value = "addDocumentReference";
  editingRow.value = {
    filePath: "",
    fileName: "",
    note: ""
  };
  activeRowId.value = null;
}

function onAddChangeLog () {
  mode.value = "addChangeLog";
  editingLogRow.value = {
    requirementLogDateStr: format(new Date(), "MM/dd/yyyy"),
    employeeId: user?.employeeId ? user.employeeId : "",
    requirementName: "",
    description: ""
  };
  activeRowId.value = null;
}

function onCancel () {
  mode.value = null;
  editingRow.value = null;
  editingLogRow.value = null;
  activeRowId.value = null;
}

function onUndo (item) {
  item.deleted = false;
  activeRowId.value = null;
}

function onDeleteDocumentReference (item) {
  item.deleted = true;
  const rowIndex = rows.value.findIndex((row) => row.id === item.id);
  if (rowIndex !== -1) {
    rows.value.splice(rowIndex, 1, {
      ...rows.value[rowIndex],
      id: item.id,
      fileName: item.fileName,
      filePath: item.filePath,
      note: item.note,
      flag: "Delete"
    });
  }
  activeRowId.value = item.id;
}

function onDeleteLog (item) {
  item.deleted = true;
  const rowIndex = logrows.value.findIndex((row) => row.id === item.id);
  if (rowIndex !== -1) {
    logrows.value.splice(rowIndex, 1, {
      ...logrows.value[rowIndex],
      id: item.id,
      employeeId: item.employeeId,
      requirementLogDate: item.requirementLogDate,
      description: item.description,
      requirementName: item.requirementName,
      flag: "Delete"
    });
  }
  activeRowId.value = item.id;
}

function getEmployee (value) {
  if (value) {
    const found = activeEmployeesDropdownSingleSelect.list.value.find(item => item.value === value);
    return found?.text || "";
  }
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns
// ------------------------------------------------------------------------------------

const { projectNameDropdownSingleSelect } = projectModule();
const { projectModulesByProjectIdForDropdownSingleSelect } = projectModuleOfProjectModule();
const { activeEmployeesDropdownSingleSelect } = employeeModule();
const { customerContactDropdownSingleSelect } = customerModule();
const {
  requirementIdentifiedUserTypeDropdownSingleSelect,
  requirementStatusDropdownSingleSelect,
  requirementApprovalStatusDropdownSingleSelect,
  requirementTypeDropdownSingleSelect
 } = requirementModule();

const {
  areaForDropdownSingleSelect,
  workspaceForDropdownSingleSelect
} = projectTaskModule();

// Computed property to get the identifiedUserType's text
const identifiedUserTypeText = computed(() => {
  const selectedOption = requirementIdentifiedUserTypeDropdownSingleSelect.list.value.find(
    item => item.value === model.value.identifiedUserType
  );
  return selectedOption ? selectedOption.text : null;
});

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });
const editingRowV$ = useVuelidate(editingRowrules, editingRow, { $lazy: true, $autoDirty: true });
const editingLogRowV$ = useVuelidate(editingLogRowrules, editingLogRow, { $lazy: true, $autoDirty: true });

async function onSave () {
  if (mode.value === "addDocumentReference") {
    if (!await editingRowV$.value.$validate()) {
      return;
    }
    // check duplicate row
    let isDuplicate = 0;
    rows.value.forEach((item, index) => {
      if (item.fileName.toLowerCase() === editingRow.value.fileName.toLowerCase()) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const newRow = {
        id: uid(),
        fileName: editingRow.value.fileName,
        filePath: editingRow.value.filePath,
        note: editingRow.value.note,
        flag: "New"
      };
      rows.value.unshift(newRow);
      mode.value = null;
      activeRowId.value = null;
    } else {
      notifyError({ message: "Duplicate File Name." });
    }
  } else if (mode.value === "addChangeLog") {
    if (!await editingLogRowV$.value.$validate()) {
      return;
    }
    // check duplicate row
    let isDuplicate = 0;
    logrows.value.forEach((item, index) => {
      if (item.requirementName.toLowerCase() === editingLogRow.value.requirementName.toLowerCase()) {
        isDuplicate = 1;
      }
    });
    if (isDuplicate === 0) {
      const newRow = {
        id: uid(),
        employeeId: editingLogRow.value.employeeId,
        requirementLogDateStr: editingLogRow.value.requirementLogDateStr,
        requirementName: editingLogRow.value.requirementName,
        description: editingLogRow.value.description,
        flag: "New"
      };
      logrows.value.unshift(newRow);
      mode.value = null;
      activeRowId.value = null;
    } else {
      notifyError({ message: "Duplicate Requirement Name." });
    }
  }
}

function handleSave (action) {
  // track clicked button
  activeButton.value = action;
  processing.value = true;
  // Update save status based on the button clicked
  if (action === "saveDraftAndContinue") {
    saveStatus.value = 1; // Save as Draft and Continue
  } else if (action === "saveAndDraft") {
    saveStatus.value = 1; // Save as Draft and Close
  } else if (action === "confirmAndClose") {
    saveStatus.value = 2; // Save as New and Close
  }
  onSubmit(action);
}

// Submit form
const onSubmit = async (action) => {
  processing.value = true;
  try {
    if (mode.value === "addDocumentReference") {
      const isValid = await editingRowV$.value.$validate();

      if (isValid) {
        notifyError({ message: "Please save document reference first." });
      }

      return;
    }
    if ((mode.value === "addChangeLog")) {
      const isValid = await editingLogRowV$.value.$validate();

      if (isValid) {
        notifyError({ message: "Please save requirement change log first." });
      }
      return;
    }
    if (await v$.value.$validate()) {
      processing.value = true;
      model.value.filePathDetailsModel = rows.value;
      model.value.requirementChangeLogModel = logrows.value;
      model.value.editingStatus = saveStatus.value;
      requirementService.saveRequirement(requirementId, model.value).then((resp) => {
        requirementId = resp;
        notifySuccess({ message: "Requirement is saved successfully." });
        if (action !== "saveDraftAndContinue") {
          onDialogOK();
        }
      });
    }
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    // processing.value = true;
    setTimeout(() => {
      processing.value = false;
      activeButton.value = "";
    }, 1500);
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load - If changed
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(() => props.id, (newValue) => {
  if (newValue) {
    getRequirement();
  } else {
    loading.value = false;
  }
}, { immediate: true });

watch(() => model.value.projectId, (newValue, oldValue) => {
  if (newValue) {
    projectModulesByProjectIdForDropdownSingleSelect.load(false, false, model.value.projectId);
  }
}, { immediate: true });

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  projectNameDropdownSingleSelect.load();
  areaForDropdownSingleSelect.load("Area");
  workspaceForDropdownSingleSelect.load("Workspace");
  requirementTypeDropdownSingleSelect.load("Requirement Type");
  activeEmployeesDropdownSingleSelect.load();
  customerContactDropdownSingleSelect.load();

  await requirementStatusDropdownSingleSelect.load("Requirement Status");
  await requirementIdentifiedUserTypeDropdownSingleSelect.load("Requirement Identifier");
  await requirementApprovalStatusDropdownSingleSelect.load("Approval Status");

  // Set "Employee" employeeType as the default if it exists
  const employeeType = requirementIdentifiedUserTypeDropdownSingleSelect.getValueByLabel("Employee");
  if (employeeType && props.id === "") {
    model.value.identifiedUserType = employeeType;
  }

  // Set "New" status as the default if it exists
  const newStatus = requirementStatusDropdownSingleSelect.getValueByLabel("New");
  if (newStatus && props.id === "") {
    model.value.statusId = newStatus;
  }

  // Set "Unapproved" status as the default if it exists
  const unapprovedStatus = requirementApprovalStatusDropdownSingleSelect.getValueByLabel("Unapproved");
  if (unapprovedStatus && props.id === "") {
    model.value.approvalStatus = unapprovedStatus;
  }
});

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_requirement .q-select__dropdown-icon{
  display: none;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
.same-size-btn {
  min-width: 150px;
  height: 50px;
}
.readonly-description {
  border: 1px dashed #ccc;
  border-radius: 4px;
  min-height: 60px;
}
.q-editor__content img {
  max-width: 100%;
  height: auto;
  display: block;
}

</style>
