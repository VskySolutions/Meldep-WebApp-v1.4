<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-xs-3 col-sm-2 col-md-3 col-lg-4 col-xl-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="SDLC" />
              <q-breadcrumbs-el v-if="search.projectIds?.length > 0 && search.requirementGroupIds?.length > 0" label="Requirement Groups" clickable to="/requirement-group" />
              <q-breadcrumbs-el label="Requirements" />
            </q-breadcrumbs>
          </div>
          <div class="col-12 col-xs-3 col-sm-2 col-md-2 col-lg-2 col-xl-3">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }} <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" /> <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <q-separator />
          <div class="col-12 col-xs-6 col-sm-8 col-md-7 col-lg-6 col-xl-6">
            <div class="row items-center justify-end no-wrap">
              <div class="row items-center q-mr-xs">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    class="search-bar"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 450px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Requirement No.</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.requirementNumber" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm hidden">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Requirement Group Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.requirementGroupIds"
                            push
                            class="q-mx-sm w-100 h-auto"
                            clearable
                            use-input
                            use-chips
                            transition-show="jump-up"
                            transition-hide="jump-up"
                            hide-bottom-space
                            :dense="true"
                            multiple
                            fill-input
                            input-debounce="0"
                            :options="requirementGroupList"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                            @filter="getAllRequirementGroupListForFilter"
                          >
                            <template #option="{ itemProps, opt, selected, toggleOption }">
                              <q-item v-bind="itemProps">
                                <q-item-section>
                                  <div class="row q-col-gutter-x-md items-center selection-drodown-item">
                                    <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                                    <div style="flex: 1; word-break: break-word;">{{ opt.text }}</div>
                                  </div>
                                </q-item-section>
                              </q-item>
                            </template>
                          </q-select>
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.projectIds"
                        label="Project Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.projectModuleIds"
                        label="Project Module"
                        :options="projectModulesByProjectIdForDropdown.list.value"
                        :filter="projectModulesByProjectIdForDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Requirement Title</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-input v-model="search.name" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Draft/Confirmed</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.editingStatus"
                            clearable
                            class="q-mx-sm w-100 h-auto"
                            stack-label
                            hide-bottom-space
                            use-input
                            emit-value
                            map-options
                            :dense="true"
                            :options="editingStatusList"
                            :popup-content-class="customPopupContentClass"
                          />
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.statusIds"
                        label="Requirement Status"
                        :options="requirementStatusForDropdown.list.value"
                        :filter="requirementStatusForDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="search.requirementTypeIds"
                        label="Requirement Type"
                        :options="requirementTypeForDropdown.list.value"
                        :filter="requirementTypeForDropdown.filter"
                        :isShowAll="true"
                      />
                      <singleSelectDropdown
                        v-model="search.identifiedUserTypeId"
                        label="Identifier"
                        :options="requirementIdentifiedUserTypeDropdownSingleSelect.list.value"
                        :filter="requirementIdentifiedUserTypeDropdownSingleSelect.filter"
                      />
                      <multiSelectDropdown
                        v-if="identifiedUserTypeText === 'Customer'"
                        v-model="search.identifiedCustomerIds"
                        label="Customer Name"
                        :options="customerContactDropdown.list.value"
                        :filter="customerContactDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-if="identifiedUserTypeText === 'Employee'"
                        v-model="search.identifiedEmployeeIds"
                        label="Employee Name"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Created From Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input
                              v-model="search.fromDate"
                              fill-input
                              dense
                              mask="##/##/####"
                            >
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date
                                      v-model="search.fromDate"
                                      mask="MM/DD/YYYY"
                                      @update:model-value="() => $refs.qDateProxy.hide()"
                                    />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Created To Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input
                              v-model="search.toDate"
                              fill-input
                              dense
                              mask="##/##/####"
                            >
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date
                                      v-model="search.toDate"
                                      mask="MM/DD/YYYY"
                                      @update:model-value="() => $refs.qDateProxy.hide()"
                                    />
                                  </q-popup-proxy>
                                </q-icon>
                              </template>
                            </q-input>
                          </div>
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-model="search.requirementTagIds"
                        label="Tags"
                        :options="requirementTagsDropdown.list.value"
                        :filter="requirementTagsDropdown.filter"
                        :show-bg-color="true"
                      />
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onAdvanceSearch(); }" />
                        <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onAdvanceClear" />
                        <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
              <manageDropdownOptions
                v-model="showManageDropdownOptions"
                :manage-drop-down-types="manageDropDownTypes"
                :selected-field="selectedField"
              />
              <q-menu v-model="showMultiSelectOptions" anchor="bottom right" self="top right" persistent no-parent-event style="width: 300px;">
                <q-card class="q-pa-sm">
                  <div class="text-subtitle2 q-mt-sm q-mb-sm">Click one of the options below</div><q-separator />
                  <q-list style="min-width: 200px">
                    <q-item
                      v-for="opt in selectedFieldOptions"
                      :key="opt.value"
                      clickable
                      :active="selectedField === opt.value"
                      active-class="bg-primary text-white"
                      @click="selectedField = opt.value"
                    >
                      <q-item-section avatar>
                        <q-icon
                          :name="opt.icon"
                          size="xs"
                          class="cursor-pointer"
                          :color="selectedField === opt.value ? 'white' : 'grey-7'"
                        />
                      </q-item-section>
                      <q-item-section>{{ opt.label }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-card>
              </q-menu>
              <div>
                <q-btn
                  icon="o_add"
                  outline
                  label="Create Requirement"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onRequirementAdd(refreshRequirementList)"
                />
                <q-btn
                  icon="o_checklist"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  :disabled="multiSelectRequirementIds.length === 0" @click.stop="showMultiSelectOptions = !showMultiSelectOptions"
                >
                  <q-badge
                    v-if="multiSelectRequirementIds?.length > 0"
                    :label="multiSelectRequirementIds.length"
                    class="primary"
                    floating
                  />
                  <q-tooltip>Multi Actions</q-tooltip>
                </q-btn>
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_playlist_add"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="showManageDropdownOptions = !showManageDropdownOptions"
                >
                  <q-tooltip>Manage Dropdowns</q-tooltip>
                </q-btn>
                <q-btn
                  v-if="selectedProjectId || search.projectIds?.length > 0 && search.requirementGroupIds?.length > 0"
                  icon="o_chevron_left"
                  outline
                  label="Back"
                  no-caps
                  class="text-primary btnRounded q-ml-xs no-space-between"
                  @click="$router.back()"
                />
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
          @request="getAllRequirement"
        >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th auto-width class="text-center" />
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
            :class="[
               props.row.isPinned ? 'bg-amber-1' : '',
              highlightedId == props.row.id ? 'highlight'
                : (props.row.editingStatus === 2
                    ? 'bg-cyan-1'
                    : '')
            ]"
            :set="(preProjectName = null, resetTracking())"
          >
            <q-td>
              <div
                v-if="props.row.requirementColor"
                :style="{
                  borderLeft: '5px solid ' + props.row.requirementColor,
                  width: '5px',
                  height: '100%',
                  position: 'absolute',
                  left: '0px',
                  top: '0px',
                  zIndex: 1
                }"
              >
                <q-tooltip>Requirement Color: {{ props.row.requirementColor }}</q-tooltip>
              </div>
              <q-icon
                v-if="props.row.isPinned && props.row.isEditable"
                name="o_push_pin"
                size="xs"
                class="q-mr-xs hoverable-cell"
                @click="() => { onSubmitRequirementPinned(props.row.id, !props.row.isPinned, refreshRequirementList); }"
              >
                <q-tooltip>Click to unpin</q-tooltip>
              </q-icon>
              <q-icon
                v-if="props.row.isPinned && !props.row.isEditable"
                name="o_push_pin"
                size="xs"
                class="q-mr-xs"
              >
                <q-tooltip>Requirement Pinned</q-tooltip>
              </q-icon>
              <q-checkbox
                v-model="props.row.checkboxStatus"
                size="sm"
                @update:model-value="onSelectCheckbox(props.row.projectId, props.row.project.name, props.row.id, props.row.title, $event)"
              />
            </q-td>
            <q-td v-if="selectedColumnNames.includes('requirementNumber')" class="text-right">
              #{{ props.row.requirementNumber }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('project.name')"
              class="common-q-td hoverable-cell"
            >
              <div class="row no-wrap items-center justify-between">
                <span>
                  <span
                    v-if="preProjectName !== props.row.project.name"
                    :set="preProjectName = props.row.project.name"
                    @click="onProjectView(props.row.project.id)"
                  >
                    {{ props.row.project.name }}
                  </span>
                </span>
                <div
                  v-if="shouldShowIcons(props.row.project.name, index)"
                  class="row items-center q-gutter-sm q-ml-sm"
                  style="flex-shrink: 0;"
                >
                  <q-icon
                    name="o_radio_button_checked"
                    size="xs" class="cursor-pointer"
                    @click="setActiveRowIdInLocalStorage(props.row.id);$router.push({ path: '/project-center', state: { projectId: props.row.project.id } })"
                  >
                    <q-tooltip>Project Center</q-tooltip>
                  </q-icon>
                  <q-icon v-if="props.row.isEditable" name="o_developer_board" size="xs" class="cursor-pointer hidden" @click="setActiveRowIdInLocalStorage(props.row.id); $router.push({ path: '/project-planning/workboard', state: {projectId: props.row.project.id } })">
                    <q-tooltip>Work Board</q-tooltip>
                  </q-icon>
                </div>
              </div>
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('projectModule.name')"
              class="common-q-td"
            >
              {{ props.row.projectModule.name }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('title')"
              class="common-q-td hoverable-cell"
            >
              <span
                class="cursor-pointer"
                @click="onRequirementView(props.row.id)"
              >
                {{ props.row.title }}
              </span>
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('projectTaskRelatedMappings')"
              class="common-q-td hoverable-cell"
            >
              <span v-if="props.row.projectTaskRelatedMappings?.length">
                <template v-for="(item, index) in props.row.projectTaskRelatedMappings" :key="index">
                  <span class="hoverable-cell" style="cursor: pointer;" @click="onProjectTaskView(item.taskId)">#{{ item.projectTask?.projectTaskNumber }}
                    <span v-if="item.projectTask?.status">
                      ({{ item.projectTask.status.dropDownValue }})
                    </span>
                  </span>
                  <span v-if="index < props.row.projectTaskRelatedMappings.length - 1">, </span>
                  <br>
                </template>
              </span>
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('editingStatus')"
              class="common-q-td"
            >
              {{ props.row.editingStatus === 1 ? 'Draft' : 'Confirmed' }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('status.dropDownValue')"
              class="common-q-td"
              :class="{ 'hoverable-cell' : props.row.isEditable }"
              @click="activeEdit = { rowId: props.row.id, field: 'status' }"
            >
              <quickEditSingleSelect
                field="status"
                :row-id="props.row.id"
                :value="props.row.status.id"
                :display-value="props.row.status.dropDownValue"
                :editable="props.row.isEditable"
                :options="requirementStatusDropdownSingleSelect.list.value"
                :active-edit="activeEdit"
                :show-history="true"
                :disable="props.row.editingStatus === 1"
                @filter="requirementStatusDropdownSingleSelect.filter"
                @cancel="activeEdit = { rowId: null, field: null }"
                @submit="({ rowId, value }) => onSubmitRequirementStatus(rowId, value, refreshRequirementList)"
                @history="() => onSiteModifiedLog(props.row.id, props.row.status.dropDownValue, 'Requirement Status')"
              />
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('priority.dropDownValue')"
              class="common-q-td"
              :class="{ 'hoverable-cell' : props.row.isEditable }"
              @click="activeEdit = { rowId: props.row.id, field: 'priority' }"
            >
              <quickEditSingleSelect
                field="priority"
                :row-id="props.row.id"
                :value="props.row.priority.id"
                :display-value="props.row.priority.dropDownValue"
                :editable="props.row.isEditable"
                :options="requirementPriorityDropdownSingleSelect.list.value"
                :active-edit="activeEdit"
                :show-history="true"
                @filter="requirementPriorityDropdownSingleSelect.filter"
                @cancel="activeEdit = { rowId: null, field: null }"
                @submit="({ rowId, value }) => onSubmitRequirementPriority(rowId, value, refreshRequirementList)"
                @history="() => onSiteModifiedLog(props.row.id, props.row.priority.dropDownValue, 'Requirement Priority')"
              />
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('userType.dropDownValue')"
              class="common-q-td"
            >
                {{ props.row.userType.dropDownValue }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('employee.person.firstName')"
              class="common-q-td">
              {{ props.row.userType.dropDownValue === 'Customer' ? (props.row.customer && props.row.customer.fullName ? props.row.customer.fullName : 'N/A') : (props.row.employee && props.row.employee.person && props.row.employee.person.fullName ? props.row.employee.person.fullName : 'N/A') }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('identifiedDate')"
              class="common-q-td text-center"
            >
                {{ props.row.identifiedDate }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('requirementTags')"
              class="common-q-td"
              :class="{ 'hoverable-cell' : props.row.isEditable }"
            >
              <div v-if="props.row.requirementTags?.length">
                <div class="row items-center q-gutter-xs">
                  <q-chip
                    v-for="(tag, i) in showAllTagsRowId === props.row.id ? props.row.requirementTags : props.row.requirementTags.slice(0, 5)"
                    :key="i"
                    dense
                    removable
                    :style="{
                      backgroundColor: tag.bgColor,
                      color: tag.color,
                      padding: '4px 8px',
                      maxWidth: '100%',
                      wordBreak: 'break-word'
                    }"
                    @remove="onDeleteRequirementTag(props.row, tag)"
                  >
                    {{ tag.text }}
                  </q-chip>
                  <!-- Show "more" or "less" toggle -->
                  <q-btn v-if="props.row.requirementTags.length > 5" dense flat size="sm" @click.stop="toggleShowAllTags(props.row.id)">
                    <template v-if="showAllTagsRowId === props.row.id">
                      <!-- <q-icon name="o_arrow_back" /> -->
                      <q-chip color="gray" size="sm" text-color="black" class="q-pa-xs text-caption" style="height: 16px; min-width: 16px;">
                        -{{ props.row.requirementTags.length - 5 }}
                      </q-chip>
                    </template>
                    <template v-else>
                      <div class="row items-center no-wrap">
                        <span class="">...</span>
                        <q-chip color="gray" size="sm" text-color="black" class="q-pa-xs text-caption" style="height: 16px; min-width: 16px;">
                          +{{ props.row.requirementTags.length - 5 }}
                        </q-chip>
                      </div>
                    </template>
                  </q-btn>
                </div>
              </div>
              <!-- q-popup-edit to edit tags -->
              <q-popup-edit
                v-if="props.row.isEditable"
                v-slot="scope"
                v-model="props.row.requirementTags"
                class="common-q-td small-popup-title"
                style="width: 300px;"
                @save="val => { props.row.requirementTags = val; onSubmitRequirementTags(props.row.id, val, refreshRequirementList, refreshRequirementTagDropdown);}"
              >
                <div class="row justify-between items-center q-mb-sm">
                  <div class="text-subtitle2">Update Tags</div>
                  <q-btn v-close-popup icon="o_close" size="sm" color="black" flat round dense />
                </div>
                <TagEditor
                  v-model="scope.value"
                  :row-id="props.row.id"
                  :available-tags="tagsDropdown.list.value"
                  :clearable="false"
                  @filter="tagsDropdown.filter"
                />
                <div class="row justify-end q-gutter-sm q-mt-sm">
                  <q-btn v-close-popup label="Cancel" color="grey" flat dense />
                  <q-btn label="Set" color="primary" dense @click="scope.set()" />
                </div>
              </q-popup-edit>
              <q-tooltip v-if="props.row.isEditable">Click to edit</q-tooltip>
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('requirementType.dropDownValue')"
              class="common-q-td"
            >
                {{ props.row.requirementType.dropDownValue }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('area.dropDownValue')"
              class="common-q-td"
            >
                {{ props.row.area.dropDownValue }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('workspace.dropDownValue')"
              class="common-q-td"
            >
                {{ props.row.workspace.dropDownValue }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('requirementEntered.person.firstName')"
              class="common-q-td"
            >
                {{ props.row.requirementEntered.person.fullName}}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('approvalStatusDropDown.dropDownValue')"
              class="common-q-td"
            >
                {{ props.row.approvalStatusDropDown.dropDownValue }}
            </q-td>
            <q-td
              v-if="selectedColumnNames.includes('lastNote')"
              class="common-q-td hoverable-cell"
            >
              <span
                @click="onNoteTimelineView(
                props.row.id,
                'Requirement'
                )"
              >
                {{ truncateText(props.row.lastNote) }}
                <q-tooltip>
                  View Notes
                </q-tooltip>
              </span>
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
              <a
                v-if="props.row.isEditable || props.row.isNotes"
                style="position: relative;"
                class="q-icon notranslate cursor-pointer q-ml-sm q-mr-sm"
                @click="onNoteAdd(props.row.id, 'Requirement', props.row.project.id, props.row.projectName, props.row.name, refreshRequirementList)"
              >
                <q-tooltip anchor="bottom middle" self="top middle">
                  Note
                </q-tooltip>
                <q-icon name="o_assignment" />
                <q-badge
                  v-if="props.row.requirementNotesCount > 0"
                  style="position: absolute; right: -16px; top: -15px;"
                  color="green"
                  text-color="white"
                  :label="props.row.requirementNotesCount"
                />
              </a>
              <q-btn dense flat icon="o_more_vert" color="primary">
                <q-tooltip>More Options</q-tooltip>
                <q-menu auto-close>
                  <q-list style="min-width: 180px">
                    <q-item
                      v-ripple
                      clickable
                      @click="onRequirementView(props.row.id)"
                    >
                      <q-item-section avatar><q-icon name="o_visibility" size="xs" /></q-item-section>
                      <q-item-section>View</q-item-section>
                    </q-item>
                    <q-item
                      v-if="props.row.isEditable"
                      v-ripple
                      clickable
                      @click="onRequirementEdit(props.row.id, refreshRequirementList)"
                    >
                      <q-item-section avatar><q-icon name="o_edit" size="xs" /></q-item-section>
                      <q-item-section>Edit</q-item-section>
                      </q-item>
                    <q-item
                      v-if="props.row.isEditable"
                      v-ripple clickable
                      @click="onConvertToTask(props.row.id, props.row.project.id, props.row.projectModule.id, props.row.title, true)"
                    >
                      <q-item-section avatar><q-icon name="o_add" size="xs" /></q-item-section>
                      <q-item-section>Convert into Task</q-item-section>
                    </q-item>
                    <q-item v-if="props.row.isEditable">
                      <q-item-section avatar>
                      <q-icon v-if="props.row.requirementColor" name="o_circle" :style="`border-radius:50%;color:${props.row.requirementColor}; background-color:${props.row.requirementColor};`" size="xs" />
                      <q-icon v-else name="o_question_mark" size="xs" />
                      </q-item-section>
                      <q-item-section class="row items-center" style="display: flex; align-items: center; justify-content: start; flex-direction: row;">
                        Color
                        <q-icon
                          name="o_colorize"
                          class="cursor-pointer q-ml-xs"
                          size="xs"
                          @click.stop="onRestorePreviousRequirementColor(props.row.requirementColor)"
                        >
                          <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                            <q-color
                              v-model="props.row.requirementColor"
                              no-header
                              no-footer
                              default-view="palette"
                              class="my-picker"
                              @update:model-value="onStartRequirementColorSelection"
                                @change="onSubmitRequirementColor(props.row.id, props.row.requirementColor, isSliderActive, previousColor, refreshRequirementList)"
                            />
                          </q-popup-proxy>
                        </q-icon>
                      </q-item-section>
                    </q-item>
                    <q-item
                      v-if="props.row.isEditable" v-ripple clickable
                      @click="() => { onSubmitRequirementPinned(props.row.id, !props.row.isPinned, refreshRequirementList); }"
                    >
                      <q-item-section avatar>
                        <q-icon :name="props.row.isPinned ? 'o_push_pin' : 'o_push_pin'" size="xs" />
                      </q-item-section>
                      <q-item-section>
                        {{ props.row.isPinned ? "Unpin" : "Pin" }}
                      </q-item-section>
                    </q-item>
                    <q-item
                      v-if="props.row.isEditable"
                      v-ripple
                      clickable
                      @click="onSubmitRequirementDelete(props.row.id, props.row.title, refreshRequirementList)"
                    >
                      <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                      <q-item-section class="text-negative">Delete</q-item-section>
                    </q-item>
                  </q-list>
                </q-menu>
              </q-btn>
            </q-td>
          </q-tr>
          <q-separator />
        </template>
        </q-table>
      </div>
    </q-card>
  </q-page>
  <!-- Multi-Column Level Sorting -->
  <multiColumnSortingDialog
    v-model="showSortDialog"
    :columns="columns"
    :multi-sort="multiSort"
    :exclude-columns="['Task','Tags', 'Draft/Confirmed', 'Last Note']"
    @add="addSortLevel"
    @remove="removeSortLevel"
    @apply="applyMultiSort"
  />
</template>
<script setup>
// Import libraries
import { ref, onMounted, watch, computed } from "vue";
import { useQuasar } from "quasar";
import { useRoute } from "vue-router";
import { zwConfirmDelete, notifySuccess, notifyError } from "assets/utils";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";

import editProjectTask from "modules/project-tasks/components/addEdit.vue";
import selectMultiRequirement from "modules/requirement/components/_multiRequirementQuickActions.vue";
import linkTaskToPlan from "modules/project-targetplan/components/_linkRequirementTaskIssueToWeeklyMonthlyPlan.vue";

import requirementService from "modules/requirement/requirement.service";
import requirementGroupsService from "modules/requirement-group/requirementGroup.service";
import taskService from "modules/project-tasks/projectTasks.service";

// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";
import multiColumnSortingDialog from "src/components/dataTable/_multiColumnSortingDialog.vue";
import columnVisibilityMenu from "src/components/dataTable/_columnVisibilityMenu.vue";
import manageDropdownOptions from "src/components/dataTable/_manageDropdownOptions.vue";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import requirementModule from "src/modules/requirement/utils/dropdowns.js";
import tagModule from "src/modules/tags/utils/dropdowns.js";
import manageDropdownModule from "src/modules/dropdown/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";

import TagEditor from "src/modules/project-tasks/components/_taskTagEditor.vue";

// SOP Change :- Shared Scripts DataTable Features
import { useColumnManager } from "composables/dataTable/useColumnManager.js";
import useColumnResize from "composables/dataTable/useColumnResize.js";
import useMultiSort from "composables/dataTable/useMultiSort.js";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView
} from "src/modules/project/utils/dialogs.js";

// SOP Change :- Shared Site Dialogs
import {
  initSiteDialogs,
  onSiteModifiedLog
} from "src/modules/sites/utils/dialogs.js";

import {
  initRequirementDialogs,
  onRequirementView,
  onRequirementAdd,
  onRequirementEdit
} from "src/modules/requirement/utils/dialogs.js";

// Shared Common Dialogs
import {
  initCommonDialogs,
  onNoteAdd,
  onNoteTimelineView
} from "src/modules/common/utils/dialogs.js";

// Shared Project Task Dialogs
import {
  initProjectTaskDialogs,
  onProjectTaskView
} from "src/modules/project-tasks/utils/dialogs.js";

// Shared Requirement Actions
import {
  initRequirementActions,
  onSubmitRequirementPinned,
  onSubmitRequirementColor,
  onSubmitRequirementTags,
  onSubmitRequirementStatus,
  onSubmitRequirementPriority,
  onSubmitRequirementDelete
} from "src/modules/requirement/utils/actions.js";

// Common variables
const $q = useQuasar();
const loading = ref(true);
const showFilter = ref(false);
const searchLoader = ref(false);
const showMultiSelectOptions = ref(false);
const selectedField = ref(null);
const multiSelectRequirementIds = ref([]);
const multiSelectRequirementTitles = ref([]);
const multiSelectRequirementProjectMap = ref({});
const multiSelectProjectIds = ref([]);
const multiSelectProjectName = ref([]);
const shownProjects = new Set();
const route = useRoute();
const authStore = useAuthStore();
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";
const editingStatusList = ref(["Confirmed", "Draft"]);
const selectedProjectId = history.state?.projectId;
const processing = ref(false);
const showManageDropdownOptions = ref(false);
const manageDropDownTypes = ref([]);
const { toDate } = useFilters();
const showSortDialog = ref(false);
const activeEdit = ref({ rowId: null, field: null });

const highlightedId = computed(() => { return activeRowId.value; });

function setActiveRowIdInLocalStorage(id) {
  activeRowId.value = id;

  saveDataTableState({
    activeRowId: id
  });
}
// Table variables
const tableRef = ref();
const rows = ref([]);

const columns = ref([
  { name: "requirementNumber", label: "Req. No.", field: "requirementNumber", align: "right", sortable: true, default: true },
  { name: "project.name", label: "Project", field: "project.name", align: "left", sortable: true, default: true },
  { name: "projectModule.name", label: "Module", field: "projectModule.name", align: "left", sortable: true, default: false },
  { name: "title", label: "Title", field: "title", align: "left", sortable: true, default: true },
  { name: "projectTaskRelatedMappings", label: "Task", field: "projectTaskRelatedMappings", align: "left", sortable: false, default: false },
  { name: "editingStatus", label: "Draft/Confirmed", field: "editingStatus", align: "left", sortable: false, default: true },
  { name: "status.dropDownValue", label: "Status", field: "status.dropDownValue", align: "left", sortable: true, default: true },
  { name: "priority.dropDownValue", label: "Priority", field: "priority.dropDownValue", align: "left", sortable: true, default: true },
  { name: "userType.dropDownValue", label: "Identifier", field: "userType.dropDownValue", align: "left", sortable: true, default: false },
  { name: "employee.person.firstName", label: "Identified By", field: "employee.person.firstName", align: "left", sortable: true, default: false },
  { name: "identifiedDate", label: "Identified Date", field: "identifiedDate", align: "center", sortable: true, default: true },
  { name: "requirementTags", label: "Tags", field: row => row.requirementTags, align: "left", sortable: false, default: false },
  { name: "requirementType.dropDownValue", label: "Type", field: "requirementType.dropDownValue", align: "left", sortable: true, default: false },
  { name: "area.dropDownValue", label: "Area", field: "area.dropDownValue", align: "left", sortable: true, default: false },
  { name: "workspace.dropDownValue", label: "Workspace", field: "workspace.dropDownValue", align: "left", sortable: true, default: false },
  { name: "requirementEntered.person.firstName", label: "Entered By", field: "requirementEntered.person.firstName", align: "left", sortable: true, default: false },
  { name: "approvalStatusDropDown.dropDownValue", label: "Approval Status", field: "approvalStatusDropDown.dropDownValue", align: "left", sortable: true, default: false },
  { name: "lastNote", label: "Last Note", field: "lastNote", align: "left", sortable: false, default: true },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "createdOnUtc", label: "Created On", field: "createdOnUtc", align: "left", sortable: true, default: false },
  { name: "updatedBy.person.firstName", label: "Updated By", field: "updatedBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "updatedOnUtc", label: "Updated On", field: "updatedOnUtc", align: "left", sortable: true, default: false }
]);

// Get/Map Requirement list to table
const getAllRequirement = async ({ pagination: p }) => {
  loading.value = true;
  const number = search.value.requirementNumber ? search.value.requirementNumber.replace(/[^0-9]/g, "").replace(/^0+(?!$)/, "") : "";
  search.value.requirementNumber = number || "0";
  search.value.fromDate = search.value.fromDate ? toDate(search.value.fromDate) : null;
  search.value.toDate = search.value.toDate ? toDate(search.value.toDate) : null;

  try {
    const { page, rowsPerPage, sortBy, descending } = p;

    const sorts = {};
    const multi = multiSort.value;
    for (let i = 0; i < multi.length; i++) {
      const s = multi[i];
      if (s.column && s.direction) {
        sorts[s.column] = s.direction;
      }
    }

    const payload = {
      page,
      pageSize: rowsPerPage,
      sortBy,
      descending,
      sorts,
      ...search.value
    };
    saveDataTableState({
      search: search.value,
      pagination: p,
      activeRowId: activeRowId.value,
      sorts
    });

    const storedRequirementIds = JSON.parse(
      localStorage.getItem("selectedRequirementIds") || "[]"
    );
    const resp = await requirementService.getAllRequirement(payload);

    rows.value = resp.data.map(requirement => {
      const hasFullAccess =
        requirement.project?.projectUserMappings?.[0]?.fullAccess ?? false;

      return {
        ...requirement,

        checkboxStatus: storedRequirementIds.includes(requirement.id),

        isNotes:
          requirement.project?.projectUserMappings?.[0]?.notes ?? false,

        isEditable: role === "admin" || hasFullAccess,

        requirementTags:
          requirement.requirementTags?.map(tag => ({
            text: tag.tags.name,
            value: tag.tags.id,
            color: tag.tags.color,
            bgColor: tag.tags.bgColor
          })) ?? []
      };
    });

    pagination.value = {
      ...pagination.value,
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    };
  } catch (error) {
    console.error(error);

    notifyError({
      message: "Failed to load requirements."
    });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------
const identifiedUserTypeText = computed(() => {
  const selectedOption = requirementIdentifiedUserTypeDropdownSingleSelect.list.value.find(
    item => item.value === search.value.identifiedUserTypeId
  );
  return selectedOption ? selectedOption.text : null;
});

const refreshRequirementList = () => {
  getAllRequirement({ pagination: pagination.value });
};

const refreshRequirementTagDropdown = () => {
  requirementTagsDropdown.load();
};

// Search records as per parameters
const onAdvanceSearch = () => {
  refreshRequirementList();
};

// Clear search
const onAdvanceClear = () => {
  search.value.requirementNumber = undefined;
  search.value.projectIds = [];
  search.value.projectModuleIds = [];
  search.value.requirementGroupIds = [];
  search.value.name = "";
  search.value.editingStatus = null;
  search.value.statusIds = [];
  search.value.requirementTypeIds = [];
  search.value.identifiedUserTypeId = null;
  search.value.identifiedEmployeeIds = [];
  search.value.identifiedCustomerIds = [];
  search.value.fromDate = null;
  search.value.toDate = null;
  search.value.requirementTagIds = [];
  saveDataTableState({
    search: search.value
  });
  onAdvanceSearch();
};

// Get all requirement group list for dropdown
const requirementGroupList = ref([]);
const requirementGroupListFilter = ref([]);
function getAllRequirementGroupListForDropdown () {
  requirementGroupsService.getAllRequirementGroupListForDropdown(search.value.projectIds).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.text, value: item.value })).sort((a, b) => a.text.localeCompare(b.text));
    requirementGroupList.value = responseData;
    requirementGroupListFilter.value = responseData;
  });
}

// Search project for dropdown
function getAllRequirementGroupListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      requirementGroupList.value = requirementGroupListFilter.value;
    } else {
      requirementGroupList.value = requirementGroupListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// truncate text after 60 characters
const truncateText = (htmlText, limit = 60) => {
  const plainText = htmlText?.replace(/<[^>]*>/g, '')?.replace(/&nbsp;/g, ' ') || ''

  return plainText.length > limit
    ? plainText.substring(0, limit) + '...'
    : plainText
}

// Need to update
const onConvertToTask = (id, projectId, projectModuleId, title, isRequirementConverted) => {
  activeRowId.value = id;
  // Collect created task numbers from related mappings
  const taskNumbers = [];
  rows.value.filter(row => id === row.id).forEach(req => {
    if (req.projectTaskRelatedMappings?.length) {
      req.projectTaskRelatedMappings.forEach(mapping => {
        if (mapping.projectTask?.projectTaskNumber) {
          taskNumbers.push(mapping.projectTask.projectTaskNumber);
        }
      });
    }
  });
  $q.dialog({
    component: editProjectTask,
    componentProps: { requirementId: id, projectIdAttr: projectId, moduleIdAttr: projectModuleId, name: title, isRequirementConverted, taskNumbers }
  }).onOk(() => {
    refreshRequirementList();
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = id;
  });
};

// Requirement Color
const isSliderActive = ref(false);
const previousColor = ref("");

const onStartRequirementColorSelection = () => { isSliderActive.value = true; };

// Store previous color
const onRestorePreviousRequirementColor = (requirementColor) => {
  previousColor.value = requirementColor ?? "#e0e0e0"; // Store previous color before opening picker
};

function resetTracking () {
  shownProjects.clear(); // Clear the set before rendering rows
}

function shouldShowIcons (projectName) {
  if (shownProjects.has(projectName)) {
    return false;
  } else {
    shownProjects.add(projectName);
    return true;
  }
}

const onSelectCheckbox = (projectId, projectName, requirementId, requirementTitle, flag) => {
  if (flag === true) {
    if (!multiSelectRequirementIds.value.includes(requirementId)) {
      // Add the itemId to the multiSelectRequirementIds array if it's not already present
      multiSelectRequirementIds.value.push(requirementId);
      multiSelectRequirementTitles.value.push(requirementTitle);
      multiSelectRequirementProjectMap.value[requirementId] = projectId;

      // Add projectId only if not already present
      if (!multiSelectProjectIds.value.includes(projectId)) {
        multiSelectProjectIds.value.push(projectId);
        multiSelectProjectName.value.push(projectName);
      }
    }
  } else {
    // Remove requirement
    const index = multiSelectRequirementIds.value.indexOf(requirementId);
    const removedProjectId = multiSelectRequirementProjectMap.value[requirementId];

    if (index !== -1) {
      multiSelectRequirementIds.value.splice(index, 1);
      multiSelectRequirementTitles.value.splice(index, 1);
    }

    delete multiSelectRequirementProjectMap.value[requirementId];

    // If no other selected task belongs to that project, remove the projectId
    const stillHasTaskForProject = Object.values(multiSelectRequirementProjectMap.value).some(pid => pid === removedProjectId);
    if (!stillHasTaskForProject) {
      multiSelectProjectIds.value = multiSelectProjectIds.value.filter(x => x !== removedProjectId);
      multiSelectProjectName.value = multiSelectProjectName.value.filter(x => x !== projectName);
    }
  }

  localStorage.setItem(
    "selectedRequirementIds",
    JSON.stringify(multiSelectRequirementIds.value)
  );
};

const selectedFieldOptions = [
  { label: "Link Requirement To Plan", value: "linkToPlan", icon: "o_calendar_view_week" },
  { label: "Change Status", value: "Status", icon: "o_flag" },
  { label: "Change Priority", value: "Priority", icon: "o_priority_high" },
  { label: "Convert into Task", value: "convertIntoTask", icon: "o_swap_horiz" }
];

const onSelectMultiOptions = () => {
  activeRowId.value = multiSelectRequirementIds.value;
  $q.dialog({
    component: selectMultiRequirement,
    componentProps: { requirementIds: multiSelectRequirementIds.value, selectedField: selectedField.value }
  }).onOk(() => {
    setDefaultsForMultiSelects();
    refreshRequirementList();
  }).onCancel(() => {
    selectedField.value = null;
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

const onLinkTaskToPlan = () => {
  const props = {
    projectId: multiSelectProjectIds.value[0],
    projectName: multiSelectProjectName.value[0],
    type: "Requirements",
    ids: multiSelectRequirementIds.value,
    names: multiSelectRequirementTitles.value,
    hasTaskLink: rows.value.some(
      r => multiSelectRequirementIds.value.includes(r.id) &&
  (!r.projectTaskRelatedMappings?.length))
  };
  $q.dialog({
    component: linkTaskToPlan,
    componentProps: props
  }).onOk(() => {
    setDefaultsForMultiSelects();
    refreshRequirementList();
  }).onCancel(() => {
    selectedField.value = null;
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

function onBulkRequirementsConvertToTask (requirementIds) {
  const selectedRequirements = rows.value.filter(row => requirementIds.includes(row.id));
  if (!selectedRequirements.length) return;

  const hasMissingModule = selectedRequirements.some(req => !req.projectModuleId);
  if (hasMissingModule) {
    notifyError({ message: "Please select a project module for this requirement before converting." });
    selectedField.value = null;
    return;
  }
  // const hasDraftReq = selectedRequirements.some(req => req.editingStatus === 1);
  // if (hasDraftReq) {
  //   notifyError({ message: "Some requirements are in draft; we cannot create tasks for them." });
  //   selectedField.value = null;
  //   return;
  // }
  processing.value = true;
  // Collect created tasks grouped by task name
  const createdTasks = {};

  selectedRequirements.forEach(req => {
    if (req.projectTaskRelatedMappings?.length) {
      req.projectTaskRelatedMappings.forEach(mapping => {
        const requirementName = req.title || "Unnamed Requirement";
        const taskNumber = mapping.projectTask?.projectTaskNumber;

        if (taskNumber) {
          if (!createdTasks[requirementName]) {
            createdTasks[requirementName] = [];
          }
          createdTasks[requirementName].push(taskNumber);
        }
      });
    }
  });

  const taskModels = selectedRequirements.map(requirement => {
    return {
      flag: "Add",
      requirementId: requirement.id
    };
  });

  const payload = {
    isRequirementConverted: true,
    projectTaskModel: taskModels
  };

  console.log(payload);

  let message = "<hr/>Are you sure you want to convert selected requirements to task?";
  const taskNames = Object.keys(createdTasks);

  if (taskNames.length > 0) {
    message += "<br/><br/><b>Already created task(s):</b><br/>";
    message += "<table style='width:100%; border-collapse:collapse;' border='1' cellspacing='0' cellpadding='5'>";
    message += "<tr><th>Requirement Title</th><th>Task Numbers</th></tr>";

    taskNames.forEach(name => {
      const taskNumbers = createdTasks[name].map(num => `#${num}`).join(", ");
      message += `<tr><td>${name}</td><td>${taskNumbers}</td></tr>`;
    });

    message += "</table>";
  } else {
    message += "<hr/>";
  }
  $q.dialog({
    title: "<span class='text-primary''>Confirmation</span>",
    message,
    html: true,
    ok: { label: "Yes", color: "primary" },
    cancel: { label: "No", color: "negative" }
  }).onOk(() => {
    taskService.saveBulkTasks(payload)
      .then(() => {
        notifySuccess({ message: "Tasks are saved successfully." });
        const message = "The requirements have been converted into tasks. Please go to the task list and enter the additional information to the task.";

        $q.dialog({
          title: "<span class='text-primary''>Success</span>",
          message,
          html: true,
          ok: { label: "OK", color: "primary" }
        });
        multiSelectRequirementIds.value = [];
        selectedField.value = null;
        localStorage.removeItem("selectedRequirementIds");
        refreshRequirementList();
      })
      .finally(() => {
        processing.value = false;
      });
  }).onCancel(() => {
    selectedField.value = null;
  });
}

function setDefaultsForMultiSelects () {
  multiSelectProjectIds.value = [];
  multiSelectProjectName.value = [];
  multiSelectRequirementProjectMap.value = {};
  multiSelectRequirementIds.value = [];
  multiSelectRequirementTitles.value = [];
  localStorage.removeItem("selectedRequirementIds");
}

const onDeleteRequirementTag = (row, tagToRemove) => {
  zwConfirmDelete(
    {
      message: "Are you sure you want to remove the tag?"
    },
    () => {
      const updatedTags = row.requirementTags.filter(
        t => t.value !== tagToRemove.value
      );
      onSubmitRequirementTags(
        row.id,
        updatedTags,
        refreshRequirementList,
        refreshRequirementTagDropdown
      );
    }
  );
};

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
  storageKey: "requirements-Index",
  siteId: user?.siteId,

  defaultSearch: {
    searchText: "",
    requirementNumber: "",
    projectIds: route.query.projectId && route.query.projectId !== ""
      ? (Array.isArray(route.query.projectId)
          ? route.query.projectId
          : [route.query.projectId])
      : [],
    projectModuleIds: [],
    requirementGroupIds: route.query.requirementGroupId && route.query.requirementGroupId !== ""
      ? (Array.isArray(route.query.requirementGroupId)
          ? route.query.requirementGroupId
          : [route.query.requirementGroupId])
      : [],
    name: "",
    editingStatus: null,
    identifiedUserTypeId: null,
    identifiedEmployeeIds: [],
    statusIds: [],
    requirementIds: [],
    identifiedCustomerIds: [],
    fromDate: null,
    toDate: null,
    requirementTagIds: []
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
    refreshRequirementList();
  }
});
// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initRequirementDialogs(activeRowId);
initProjectDialogs(activeRowId);
initCommonDialogs(activeRowId);
initProjectTaskDialogs(activeRowId);
initSiteDialogs(activeRowId);
initRequirementActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { projectNameDropdown } = projectModule();
const { projectModulesByProjectIdForDropdown } = projectModuleOfProjectModule();
const { customerContactDropdown } = customerModule();
const { activeEmployeesDropdown } = employeeModule();
const { tagsDropdown } = tagModule();
const { getDropdownTypesByModuleNameForDropdown } = manageDropdownModule();
const {
  requirementStatusForDropdown,
  requirementTypeForDropdown,
  requirementStatusDropdownSingleSelect,
  requirementPriorityDropdownSingleSelect,
  requirementIdentifiedUserTypeDropdownSingleSelect,
  requirementTagsDropdown
} = requirementModule();

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

const mapSingleFilterToLabel = (id, list, label) => {
  if (id == null || id === "") return {};
  const match = list.value.find(item => item.value === id);
  const text = match ? match.text : id;
  return { [label]: text };
};

const appliedFilters = computed(() => ({
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name"),
  ...mapFilterToLabel(search.value.projectModuleIds, projectModulesByProjectIdForDropdown.list, "Project Module"),
  ...mapFilterToLabel(search.value.requirementGroupIds, requirementGroupList, "Requirement Group Name"),
  ...mapSingleFilterToLabel(search.value.editingStatus, editingStatusList, "Draft/Confirmed"),
  ...mapFilterToLabel(search.value.statusIds, requirementStatusForDropdown.list, "Requirement Status"),
  ...mapFilterToLabel(search.value.requirementTypeIds, requirementTypeForDropdown.list, "Requirement Type"),
  ...mapSingleFilterToLabel(search.value.identifiedUserTypeId, requirementIdentifiedUserTypeDropdownSingleSelect.list, "Identifier"),
  ...mapFilterToLabel(search.value.identifiedCustomerIds, customerContactDropdown.list, "Customer Name"),
  ...mapFilterToLabel(search.value.identifiedEmployeeIds, activeEmployeesDropdown.list, "Employee Name"),
  ...mapFilterToLabel(search.value.requirementTagIds, requirementTagsDropdown.list, "Tags"),
  ...(search.value.requirementNumber > 0 ? { "Requirement Id": search.value.requirementNumber } : {}),
  ...(search.value.name ? { "Requirement Title": search.value.name } : {}),
  ...(search.value.fromDate ? { "Created From Date": search.value.fromDate } : {}),
  ...(search.value.toDate ? { "Created To Date": search.value.toDate } : {})
}));

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Project Module": return search.value.projectModuleIds?.length || 0;
  case "Requirement Group Name": return search.value.requirementGroupIds?.length || 0;
  case "Requirement Status": return search.value.statusIds?.length || 0;
  case "Requirement Type": return search.value.requirementTypeIds?.length || 0;
  case "Customer Name": return search.value.identifiedCustomerIds?.length || 0;
  case "Employee Name": return search.value.identifiedEmployeeIds?.length || 0;
  case "Tags": return search.value.requirementTagIds?.length || 0;
  default: return null;
  }
}

function onClearFilters (key) {
  if (key === "Requirement Id") {
    search.value.requirementNumber = "";
  } else if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Project Module") {
    search.value.projectModuleIds = [];
  } else if (key === "Requirement Group Name") {
    search.value.requirementGroupIds = [];
  } else if (key === "Draft/Confirmed") {
    search.value.editingStatus = null;
  } else if (key === "Requirement Status") {
    search.value.statusIds = [];
  }  else if (key === "Requirement Type") {
    search.value.requirementTypeIds = [];
  } else if (key === "Identifier") {
    search.value.identifiedUserTypeId = [];
  } else if (key === "Employee Name") {
    search.value.identifiedEmployeeIds = [];
  } else if (key === "Customer Name") {
    search.value.identifiedCustomerIds = [];
  } else if (key === "Requirement Title") {
    search.value.name = "";
  } else if (key === "Created From Date") {
    search.value.fromDate = "";
  } else if (key === "Created To Date") {
    search.value.toDate = "";
  } else if (key === "Tags") {
    search.value.requirementTagIds = [];
  }
  delete appliedFilters.value[key];
  refreshRequirementList();
}

// ----------------------------------------------------------------------------------------------------------------
// Task Tags.
// ----------------------------------------------------------------------------------------------------------------

const showAllTagsRowId = ref(null);

const toggleShowAllTags = (rowId) => {
  showAllTagsRowId.value = showAllTagsRowId.value === rowId ? null : rowId;
};

watch(() => search.value.projectIds, (newValue, oldValue) => {
  if (newValue) {
    getAllRequirementGroupListForDropdown();
  }
}, { immediate: true });

watch(() => search.value.projectIds, async (newValue, oldValue) => {
  if (search.value?.projectIds?.length === 0 || newValue === oldValue) return;

  search.value.projectModuleIds = [];
  await projectModulesByProjectIdForDropdown.load(false, false, search.value.projectIds);
}, { immediate: true });

watch(selectedField, (newVal) => {
  if (newVal) {
    showMultiSelectOptions.value = false;
    if (newVal === "linkToPlan") {
      if (multiSelectProjectIds.value.length > 1) {
        $q.notify({
          type: "warning",
          message: "Cannot link plan: selected requirements are of multiple projects."
        });
        selectedField.value = null;
        return;
      }
      onLinkTaskToPlan();
      return;
    }
    if (newVal === "convertIntoTask") {
      onBulkRequirementsConvertToTask(multiSelectRequirementIds.value);
      return;
    }
    onSelectMultiOptions(); // This opens the dialog for the selected action
  }
});

watch(multiSelectRequirementIds, () => {
  if (multiSelectRequirementIds.value.length === 0) {
    showMultiSelectOptions.value = false;
  }
}, { deep: true });

watch(() => search.value.identifiedUserTypeId, () => {
  search.value.identifiedByIds = [];
  search.value.identifiedCustomerIds = [];
});
// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshRequirementList();
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
// On page rendering
onMounted(async () =>  {
  activeEmployeesDropdown.load(user.siteId);
  projectNameDropdown.load();
  if (search.value.projectIds.length > 0) projectModulesByProjectIdForDropdown.load(false, false, search.value.projectIds);

  getAllRequirementGroupListForDropdown();
  requirementStatusForDropdown.load("Requirement Status");
  requirementTypeForDropdown.load("Requirement Type");
  requirementIdentifiedUserTypeDropdownSingleSelect.load("Requirement Identifier");
  customerContactDropdown.load();
  requirementTagsDropdown.load();
  tagsDropdown.load();
  requirementStatusDropdownSingleSelect.load("Requirement Status");
  requirementPriorityDropdownSingleSelect.load("Requirement Priority");
  localStorage.removeItem("selectedRequirementIds");
  if (!activeRowId.value) {
    activeRowId.value = null;
  }
   // Admin:- Manage all SDLC Dropdowns and Types
  manageDropDownTypes.value = await getDropdownTypesByModuleNameForDropdown("SDLC");
  refreshRequirementList();
});
</script>
<style scoped>
.Custom-DataTable {
  min-width: max-content;
}
</style>
