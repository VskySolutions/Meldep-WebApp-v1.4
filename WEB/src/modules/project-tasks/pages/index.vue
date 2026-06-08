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
              <q-breadcrumbs-el label="Project Management" />
              <q-breadcrumbs-el :label="!search.isTemplate ? 'Tasks' : 'Project Templates Tasks'" />
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
          <div class="col-12 col-xs-6 col-sm-8 col-md-7 col-lg-6 col-xl-6">
            <div class="row items-center justify-end no-wrap">
              <div class="search-container position-relative">
                <searchFilterBar
                  v-model="search.searchText"
                  :loading="searchLoader"
                  :applied-filters="appliedFilters"
                  @toggle-filter="showFilter = !showFilter"
                />
                <!-- Dropdown Content -->
                <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                  <q-card class="q-pa-sm">
                    <div class="row items-center q-mb-sm">
                      <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                        <label class="Cutomlabel q-mt-sm fs-13">Search by</label>
                      </div>
                      <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                        <q-radio v-model="search.isTemplate" checked-icon="o_task_alt" unchecked-icon="o_panorama_fish_eye" :val="false" label="Projects" @click="onChangeProjectOrTemplate()" />
                        <q-radio v-model="search.isTemplate" checked-icon="o_task_alt" unchecked-icon="o_panorama_fish_eye" :val="true" label="Templates" @click="onChangeProjectOrTemplate()" />
                      </div>
                    </div>
                    <div class="row items-center q-mb-sm">
                      <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                        <label class="Cutomlabel q-mt-sm fs-13">Task Number</label>
                      </div>
                      <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                        <q-input v-model="search.projectTaskNumber" fill-input class="q-mx-sm w-100 h-auto" :dense="true" />
                      </div>
                    </div>
                    <multiSelectDropdown
                      v-model="search.customerIds"
                      label="Customer Name"
                      :options="customerNameDropdown.list.value"
                      :filter="customerNameDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.companyContactIds"
                      label="Company Contact"
                      :options="companyContactNameDropdown.list.value"
                      :filter="companyContactNameDropdown.filter"
                    />
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
                    <multiSelectDropdown
                      v-model="search.projectTaskIds"
                      label="Task Names"
                      :options="projectTasksByProjectIdAndModuleIdForDropdown.list.value"
                      :filter="projectTasksByProjectIdAndModuleIdForDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.projectLeadsIds"
                      label="Project Leads"
                      :options="activeEmployeesDropdown.list.value"
                      :filter="activeEmployeesDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.activityOwners"
                      label="Activity Owners"
                      :options="activeEmployeesDropdown.list.value"
                      :filter="activeEmployeesDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.statusIds"
                      label="Task Status"
                      :options="projectTaskStatusListWithDisables"
                      :filter="getProjectTaskStatusFilter"
                      :isShowAll="true"
                    />
                    <multiSelectDropdown
                      v-model="search.priorityIds"
                      label="Task Priority"
                      :options="projectTaskPrioritiesForDropdown.list.value"
                      :filter="projectTaskPrioritiesForDropdown.filter"
                      :isShowAll="true"
                    />
                    <multiSelectDropdown
                      v-model="search.taskTagsIds"
                      label="Task Tags"
                      :options="projectTaskTagsDropdown.list.value"
                      :filter="projectTaskTagsDropdown.filter"
                      :show-bg-color="true"
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
                          :color="selectedField === opt.value ? 'primary' : 'grey-7'"
                        />
                      </q-item-section>
                      <q-item-section>{{ opt.label }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-card>
              </q-menu>
              <div class="flex items-center">
                <q-btn
                  icon="o_grid_view"
                  class="text-white btnRounded bg-primary q-ml-xs"
                  @click="$router.push({ path: '/project-tasks/taskListDetails' })"
                >
                  <q-tooltip>Grid View</q-tooltip>
                </q-btn>
                <q-btn
                  icon="o_add"
                  outline
                  label="Add"
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="onProjectTaskAdd(refreshProjectTaskList, null, search.projectIds?.[0], search.projectModuleIds?.[0])"
                >
                  <q-tooltip>Add Project Task</q-tooltip>
                </q-btn>
                <!-- Quick Multi Task Actions -->
                <q-btn
                  icon="o_checklist"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  :disabled="multiSelectTaskIds.length === 0"
                  @click.stop="showMultiSelectOptions = !showMultiSelectOptions"
                >
                  <q-badge v-if="multiSelectTaskIds?.length > 0" :label="multiSelectTaskIds.length" class="primary" floating />
                  <q-tooltip>Multi Actions</q-tooltip>
                </q-btn>
                <!-- Admin:- Manage All Dropdowns -->
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
                <!-- Reset Column Width -->
                <q-btn
                  icon="o_refresh"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="resetColumnsWidth"
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
                <!-- Multi-Column Level Sorting -->
                <q-btn
                  color="primary"
                  icon="o_sort"
                  class="btnRounded q-ml-xs"
                  @click="showSortDialog = true"
                >
                  <q-badge v-if="selectedSortCount > 0" color="green" floating class="q-ml-xs">
                    {{ selectedSortCount }}
                  </q-badge>
                  <q-tooltip>Multi-Column Level Sorting</q-tooltip>
                </q-btn>
                <q-btn
                  v-if="selectedProjectId"
                  icon="o_chevron_left"
                  outline label="Back"
                  no-caps
                  class="text-primary btnRounded no-space-between q-ml-xs"
                  @click="$router.back()"
                />
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
          @request="getAllProjectTaskList"
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
                :class="col.headerClasses"
                :style="{
                  width: (resizeWidths?.[col.name] || 120) + 'px',
                  minWidth: '80px',
                  position: 'relative'
                }"
                @click="!isResizing && col.sortable"
              >
                <template v-if="col.name === 'estimateTime'">
                  <div>
                    <span>{{ col.label }}</span>
                    <div class="resize-handle" @mousedown="(e) => startResize(e, col.name)" />
                    <q-icon v-if="col.tooltip" name="o_info" size="xs" class="q-mx-xs">
                      <q-tooltip class="text-caption">{{ col.tooltip }}</q-tooltip>
                    </q-icon>
                  </div>
                </template>
                <template v-else>
                  {{ col.label }}
                </template>
                <div class="resize-handle" @mousedown="(e) => startResize(e, col.name)" />
              </q-th>
              <q-th auto-width class="text-center">Actions</q-th>
            </q-tr>
          </template>
          <template #body="props">
            <q-tr
              :props="props" :class="highlightedId == props.row.id ? 'highlight' : ''"
              :set="(preProjectName = null, preProjectModuleName = null)"
            >
              <q-td class="text-center">
                <q-checkbox
                  v-model="props.row.checkboxStatus"
                  @update:model-value="onSelectCheckbox(props.row.project.id, props.row.project.name, props.row.project.projectStatus.dropDownValue, props.row.id, props.row.name,$event)"
                />
              </q-td>
              <q-td v-if="selectedColumnNames.includes('project.name')" style="white-space: normal;">
                <div class="row no-wrap items-center justify-between">
                  <span style="flex: 1; word-break: break-word; white-space: normal;">
                    <span
                      v-if="preProjectName !== props.row.project.name"
                      :set="preProjectName = props.row.project.name"
                      class="hoverable-cell"
                      @click="onProjectView(props.row.project.id)"
                    >
                      {{ props.row.project.name }}
                    </span>
                  </span>
                </div>
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('projectModule.name')"
                class="common-q-td hoverable-cell"
                @click="onProjectModuleView(props.row.projectModule.id)"
              >
                <span
                  v-if="preProjectModuleName !== props.row.projectModule.name"
                  :set="preProjectModuleName = props.row.projectModule.name"
                >
                  {{ props.row.projectModule.name }}
                </span>
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('name')"
                style="white-space: normal;"
                class="hoverable-cell"
              >
                <div class="row no-wrap items-center justify-between">
                  <span
                    style="flex: 1; word-break: break-word; white-space: normal;"
                    @click="onProjectTaskView(props.row.id)"
                  >
                    {{ props.row.name }}
                  </span>
                  <!-- Change log icon -->
                  <q-icon
                    name="o_history"
                    class="cursor-pointer q-ml-sm"
                    size="xs"
                    clickable
                    @click="onSiteModifiedLog(props.row.id, props.row.name, 'Task Name', refreshProjectTaskList)"
                  >
                    <q-tooltip>Data Change Log</q-tooltip>
                  </q-icon>
                </div>
              </q-td>
              <td
                v-if="selectedColumnNames.includes('area.dropDownValue')"
                class="common-q-td"
              >
                {{ props.row.area.dropDownValue }}
              </td>
              <td
                v-if="selectedColumnNames.includes('workspace.dropDownValue')"
                class="common-q-td"
              >
                {{ props.row.workspace.dropDownValue }}
              </td>
              <td
                v-if="selectedColumnNames.includes('action.dropDownValue')"
                class="common-q-td"
              >
                {{ props.row.action.dropDownValue }}
              </td>
              <q-td
                v-if="selectedColumnNames.includes('projectTaskNumber')"
                class="text-right"
              >
                #{{ props.row.projectTaskNumber }}
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('startDate')"
                class="text-center"
              >
                {{ toDate(props.row.startDate) }}
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('endDate')"
                class="common-q-td"
                :class="{ 'hoverable-cell' : props.row.isEditable }"
                @click="activeEdit = { rowId: props.row.id, field: 'endDate' }"
              >
                <quickEditDate
                  :row-id="props.row.id"
                  :model-value="props.row.endDateStr"
                  :editable="props.row.isEditable"
                  :date-options="disableBeforeStartDate(props.row.startDateStr)"
                  :show-history="true"
                  @submit="({ rowId, value }) => onSubmitProjectTaskEndDate(rowId, value, refreshProjectTaskList)"
                  @history="() => onSiteModifiedLog(props.row.id, props.row.name, 'Due Date')"
                />
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
                  :disable="projectTaskStatusListRaw?.find(item => item.value === props.row.status.id)?.text === 'Close'"
                  :options="projectTaskStatusListRaw"
                  :active-edit="activeEdit"
                  :show-history="true"
                  @popup-show=" handlePopupShow(props.row.status.dropDownValue, props.row.project.projectStatus.dropDownValue)"
                  @cancel="activeEdit = { rowId: null, field: null }"
                  @submit="({ rowId, value }) => onSubmitProjectTaskStatus(projectTaskStatusListWithDisables, rowId, value, refreshProjectTaskList)"
                  @history="() => onSiteModifiedLog(props.row.id, props.row.status.dropDownValue, 'Task Status')"
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
                  :options="projectTaskPrioritiesForDropdown.list.value"
                  :active-edit="activeEdit"
                  :show-history="true"
                  @filter="projectTaskPrioritiesForDropdown.filter"
                  @cancel="activeEdit = { rowId: null, field: null }"
                  @submit="({ rowId, value }) => onSubmitProjectTaskPriority(rowId, value, refreshProjectTaskList)"
                  @history="() => onSiteModifiedLog(props.row.id, props.row.name, 'Task Priority')"
                />
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('assignedTo')"
                class="common-q-td"
                :class="{ 'hoverable-cell' : props.row.isEditable }"
              >
                <div
                  class="flex item-center justify-end"
                  :class="props.row.assignedTo?.person?.firstName ? 'TaskActivity' : ''"
                >
                  <div v-if="props.row.assignedTo?.person?.fullName !== '' && props.row.assignedTo?.person?.fullName !== ' '">
                    <span
                      class="Person"
                      :style="{ background: props.row.assignedTo.person.bgColor, color: props.row.assignedTo.person.color }"
                    >
                      {{ (props.row.assignedTo?.person?.firstName?.[0] || '') + (props.row.assignedTo?.person?.lastName?.[0] || '') }}
                    </span>
                    <q-tooltip v-if="props.row.assignedTo?.person">
                      <div>
                        <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                        <span>
                          {{ props.row.assignedTo?.person?.firstName }} {{ props.row.assignedTo?.person?.lastName }}
                        </span>
                      </div>
                    </q-tooltip>
                  </div>
                  <q-icon
                    v-if="props.row.assignedTo?.person?.firstName"
                    name="o_history"
                    class="cursor-pointer q-ml-sm"
                    size="xs"
                    clickable
                    @click.stop="onSiteModifiedLog(props.row.id, props.row.name, 'Task Owner', refreshProjectTaskList)"
                  >
                    <q-tooltip>Data Change Log</q-tooltip>
                  </q-icon>
                </div>

                <q-popup-edit
                  v-if="props.row.isEditable"
                  v-slot="scope"
                  v-model="props.row.assignedToId"
                  class="small-popup-title"
                  style="width: 270px;"
                  @save="val => { props.row.assignedToId = val; onSubmitProjectTaskOwner(props.row.id, val, refreshProjectTaskList); }"
                >
                  <div class="row justify-between items-center">
                    <div class="text-subtitle2 q-mb-sm">Update Task Owner</div>
                    <q-btn v-close-popup icon="o_close" size="sm" color="black" flat round dense />
                  </div>
                  <q-select
                    v-model="scope.value"
                    :options="activeEmployeesDropdown.list.value"
                    use-input
                    style="width: 100%;"
                    use-chips
                    clearable
                    outlined
                    dense
                    emit-value
                    map-options
                    option-value="value"
                    option-label="text"
                    dropdown-icon="o_arrow_drop_down"
                    @filter="activeEmployeesDropdown.filter"
                  />
                  <div class="row justify-end q-gutter-sm q-mt-sm">
                    <q-btn v-close-popup label="Cancel" color="grey" flat dense />
                    <q-btn label="Set" color="primary" dense @click="scope.set()" />
                  </div>
                </q-popup-edit>
                <q-tooltip v-if="props.row.isEditable">Click to edit</q-tooltip>
              </q-td>
              <q-td v-if="selectedColumnNames.includes('assignedTo.id')">
                <div
                  class="flex justify-end TaskActivity"
                  :class="{ TaskActivity: getGroupedActivities(props.row.projectActivities).length > 0 }"
                >
                  <div v-for="person in visiblePersons(props.row)" :key="person.fullName">
                    <div v-if="person.fullName?.length > 1">
                      <span
                        class="Person"
                        :style="{ background: person.bgColor, color: person.color }"
                      >
                        {{ person.initials }}
                      </span>
                      <q-tooltip>
                        <div>
                          <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                          <span>{{ person.firstName }} {{ person.lastName }}</span>
                        </div>
                        <div v-for="(act, i) in person.activities" :key="i">
                          <q-icon name="o_task" color="white" size="xs" class="q-mr-xs" />
                          <span>{{ act.name }} ({{ act.estimateHours }})</span>
                        </div>
                      </q-tooltip>
                    </div>
                  </div>
                  <!-- Show three dots if more than 3 persons and not expanded -->
                  <span
                    v-if="getGroupedActivities(props.row.projectActivities).length > 3"
                    class="cursor-pointer text-primary"
                    @click="toggleRow(props.row)"
                  >
                    {{ isExpandedPersons(props.row) ? '−' : '...' }}
                  </span>
                  <q-icon
                    v-if="props.row.isEditable"
                    name="o_add"
                    class="cursor-pointer"
                    size="xs" clickable
                    @click="onProjectTaskAssignment(props.row.id, props.row.project.id, props.row.projectModule.id, props.row.name, props.row.project.name, props.row.projectModule.name, 'Task Assignment', refreshProjectTaskList)"
                  >
                    <q-tooltip>Assign Task</q-tooltip>
                  </q-icon>
                </div>
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('estimateTime')"
                class="text-right"
                :class="props.row.totalActivityHours > props.row.estimateTime ? 'text-red' : ''"
              >
                {{ props.row.totalActivityHours }} / {{ props.row.estimateTime }} /
                <span
                  class="cursor-pointer hoverable-cell"
                  @click="onHandleProjectTaskLevelTimeSheetView(props.row.id)"
                >
                  {{ props.row.totalTimesheetEstHours }}
                  <q-tooltip v-if="!search.isTemplate">Timesheet Details</q-tooltip>
                </span>
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('type.dropDownValue')"
              >
                {{ props.row.type.dropDownValue }}
              </q-td>
              <q-td
                v-if="selectedColumnNames.includes('taskTags')"
                class="common-q-td"
                :class="{ 'hoverable-cell' : props.row.isEditable }"
              >
                <div v-if="props.row.taskTags?.length">
                  <div class="row items-center q-gutter-xs">
                    <q-chip
                      v-for="(tag, i) in showAllTagsRowId === props.row.id ? props.row.taskTags : props.row.taskTags.slice(0, 5)"
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
                      @remove="onDeleteProjectTaskTag(props.row, tag)"
                    >
                      {{ tag.text }}
                    </q-chip>
                    <!-- Show "more" or "less" toggle -->
                    <q-btn v-if="props.row.taskTags.length > 5" dense flat size="sm" @click.stop="toggleShowAllTags(props.row.id)">
                      <template v-if="showAllTagsRowId === props.row.id">
                        <!-- <q-icon name="o_arrow_back" /> -->
                        <q-chip color="gray" size="sm" text-color="black" class="q-pa-xs text-caption" style="height: 16px; min-width: 16px;">
                          -{{ props.row.taskTags.length - 5 }}
                        </q-chip>
                      </template>
                      <template v-else>
                        <div class="row items-center no-wrap">
                          <span class="">...</span>
                          <q-chip color="gray" size="sm" text-color="black" class="q-pa-xs text-caption" style="height: 16px; min-width: 16px;">
                            +{{ props.row.taskTags.length - 5 }}
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
                  v-model="props.row.taskTags"
                  class="common-q-td small-popup-title"
                  style="width: 300px;"
                  @save="val => { props.row.taskTags = val; onSubmitProjectTaskTags(props.row.id, val, refreshProjectTaskList, refreshProjectTaskTagsDropdown);}"
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
              <q-td v-if="selectedColumnNames.includes('sortOrder')">{{ props.row.sortOrder }}</q-td>
              <q-td v-if="selectedColumnNames.includes('color')">
                <div
                  :style="{
                    backgroundColor: props.row.color,
                    width: '24px',
                    height: '24px',
                    borderRadius: '4px',
                    border: '1px solid #ccc',
                    margin: '0 auto'
                  }"
                />
              </q-td>
              <q-td v-if="selectedColumnNames.includes('createdBy.person.firstName')">{{ props.row.createdBy.person.firstName + " " + props.row.createdBy.person.lastName }}</q-td>
              <q-td v-if="selectedColumnNames.includes('createdOnUtc')">{{ props.row.createdOnUtc }}</q-td>
              <q-td v-if="selectedColumnNames.includes('UpdatedBy.person.firstName')">{{ props.row.updatedBy.person.firstName + " " + props.row.updatedBy.person.lastName }}</q-td>
              <q-td v-if="selectedColumnNames.includes('updatedOnUtc')">{{ props.row.updatedOnUtc }}</q-td>
              <q-td class="text-center actions" style="width: 5%">
                <a
                  v-if="props.row.isEditable || props.row.isNotes"
                  style="position: relative;"
                  class="q-icon notranslate cursor-pointer q-ml-sm q-mr-sm"
                  @click="onProjectTaskNotesAdd(props.row.id, 'Project Task', props.row.projectId, props.row.project.name, props.row.name, refreshProjectTaskList)"
                >
                  <q-tooltip anchor="bottom middle" self="top middle">
                    Note
                  </q-tooltip>
                  <q-icon name="o_assignment" />
                  <q-badge
                    v-if="props.row.projectTaskNotesCount > 0"
                    style="position: absolute; right: -16px; top: -15px;"
                    color="green"
                    text-color="white"
                    :label="props.row.projectTaskNotesCount"
                  />
                </a>
                <q-btn dense flat icon="o_more_vert" size="sm" color="primary">
                  <q-tooltip>More Options</q-tooltip>
                  <q-menu auto-close>
                    <q-list style="min-width: 250px">
                      <q-item v-if="props.row.isEditable" v-ripple clickable @click="onProjectTaskEdit(props.row.id, refreshProjectTaskList)">
                        <q-item-section avatar><q-icon name="o_edit" size="xs" /></q-item-section>
                        <q-item-section>Edit</q-item-section>
                      </q-item>
                      <q-item v-if="props.row.isEditable" v-ripple clickable @click="onProjectTaskAssignment(props.row.id, props.row.project.id, props.row.projectModule.id, props.row.name, props.row.project.name, props.row.projectModule.name,'Task Assignment', refreshProjectTaskList)">
                        <q-item-section avatar><q-icon name="o_assignment_ind" size="xs" /></q-item-section>
                        <q-item-section>Task Assignment</q-item-section>
                      </q-item>
                      <q-item v-if="props.row.isEditable" v-ripple clickable @click="openTagDialog(props.row.id)">
                        <q-item-section avatar><q-icon name="o_local_offer" size="xs" /></q-item-section>
                        <q-item-section>
                          <div class="cursor-pointer" @click.stop>
                            Add Tags
                            <q-popup-edit
                              v-slot="scope"
                              v-model="props.row.taskTags"
                              class="small-popup-title common-q-td"
                              style="width: 300px;"
                              @save="val => {
                                props.row.taskTags = val;
                                onSubmitProjectTaskTags(props.row.id, val, refreshProjectTaskList, refreshProjectTaskTagsDropdown);
                              }"
                            >
                              <div class="row justify-between items-center q-mb-sm">
                                <div class="text-subtitle2">Add Tags</div>
                                <q-btn
                                  v-close-popup
                                  icon="o_close"
                                  size="sm"
                                  color="black"
                                  flat
                                  round
                                  dense
                                />
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
                          </div>
                        </q-item-section>
                      </q-item>
                      <q-item v-ripple clickable @click="onProjectTaskLevelTimeSheetView(props.row.id)">
                        <q-item-section avatar><q-icon name="o_notes" size="xs" /></q-item-section>
                        <q-item-section>Task Level Timesheet</q-item-section>
                      </q-item>
                      <q-item v-if="props.row.isEditable" v-ripple clickable @click="onProjectTaskFiles(props.row.id, props.row.name, props.row.project.name, props.row.projectModule.name)">
                        <q-item-section avatar><q-icon name="o_description" size="xs" /></q-item-section>
                        <q-item-section>Files</q-item-section>
                      </q-item>
                      <q-separator />
                      <q-item v-if="props.row.isEditable" v-ripple clickable @click="onProjectTaskCopy(props.row.id, props.row.name, props.row.projectModuleId, 'isCopy', refreshProjectTaskList)">
                        <q-item-section avatar><q-icon name="o_copy" size="xs" /></q-item-section>
                        <q-item-section>Copy</q-item-section>
                      </q-item>
                      <q-item v-if="props.row.isEditable" v-ripple clickable @click="onProjectTaskMove(props.row.id, props.row.name, props.row.projectModuleId, 'isMove', refreshProjectTaskList)">
                        <q-item-section avatar><q-icon name="o_arrow_forward" size="xs" /></q-item-section>
                        <q-item-section>Move</q-item-section>
                      </q-item>
                      <q-item v-if="props.row.isEditable" v-ripple clickable @click="onSubmitProjectTaskDelete(props.row.id, props.row.name, props.row.project.name, refreshProjectTaskList)">
                        <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                        <q-item-section class="text-negative">Delete</q-item-section>
                      </q-item>
                    </q-list>
                  </q-menu>
                </q-btn>
              </q-td>
            </q-tr>
            <q-tr v-if="selectedColumnNames.includes('estimateTime') && props.pageIndex === rows.length - 1" class="bg-grey-2">
              <!-- Label spanning all columns before estimateTime -->
              <q-td
                :colspan="computedColumns.findIndex(c => c.name === 'estimateTime') + 1"
                class="text-right text-bold"
              >
                Total Hours:
              </q-td>

              <!-- Totals in the estimateTime column -->
              <q-td class="text-right text-bold">
                {{ totalEstimateHours() }} / {{ totalTaskEstimateTimeHours() }} / {{ totalTimesheetHours() }}
              </q-td>
              <q-td
                v-for="(col, idx) in computedColumns.slice(computedColumns.findIndex(c => c.name === 'estimateTime'))"
                :key="'blank-' + idx"
              />
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
    :exclude-columns="['Task No','Task Hrs','Owner','Assigned To','Color','Tags']"
    @add="addSortLevel"
    @remove="removeSortLevel"
    @apply="applyMultiSort"
  />
</template>
<script setup>

// ----------------------------------------------------------------------------------------------------------------
// Import libraries
// ----------------------------------------------------------------------------------------------------------------
import { ref, watch, onMounted, computed } from "vue";
import { useQuasar } from "quasar";
import { notifyWarning, zwConfirmDelete } from "assets/utils";
import { useAuthStore } from "stores/auth";
import { parse } from "date-fns"; // Standard TimeZone Conversion

import selectMultiTask from "modules/project-tasks/components/_multiTaskQuickActions.vue";
import TagEditor from "src/modules/project-tasks/components/_taskTagEditor.vue";

import useFilters from "composables/useFilters";
import commonService from "services/common.service";
import projectTaskService from "modules/project-tasks/projectTasks.service";

// Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";
import companyContactsModule from "src/modules/company-contacts/utils/dropdowns.js";
import manageDropdownModule from "src/modules/dropdown/utils/dropdowns.js";
import tagModule from "src/modules/tags/utils/dropdowns.js";
import userModule from "src/modules/user-management/utils/dropdowns.js";

// Shared DataTable Features
import { useColumnManager } from "composables/dataTable/useColumnManager.js"; // useActiveRowReset
import useColumnResize from "composables/dataTable/useColumnResize.js";
import useMultiSort from "composables/dataTable/useMultiSort.js";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// Shared DataTable Views
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";
import quickEditDate from "src/components/dataTable/_quickEditDate.vue";
import multiColumnSortingDialog from "src/components/dataTable/_multiColumnSortingDialog.vue";
import manageDropdownOptions from "src/components/dataTable/_manageDropdownOptions.vue";
import columnVisibilityMenu from "src/components/dataTable/_columnVisibilityMenu.vue";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView
} from "src/modules/project/utils/dialogs.js";

// SOP Change :- Shared Site Dialogs
import {
  initSiteDialogs,
  onSiteModifiedLog
} from "src/modules/sites/utils/dialogs.js";

// Shared Project Module Dialogs
import {
  initProjectModuleDialogs,
  onProjectModuleView
} from "src/modules/project-modules/utils/dialogs.js";

// Shared Project Task Dialogs
import {
  initProjectTaskDialogs,
  onProjectTaskView,
  onProjectTaskAdd,
  onProjectTaskEdit,
  onProjectTaskLevelTimeSheetView,
  onProjectTaskFiles,
  onProjectTaskCopy,
  onProjectTaskMove,
  onProjectTaskNotesAdd
} from "src/modules/project-tasks/utils/dialogs.js";

// Shared Project Task Activity
import {
  initProjectTaskActivityDialogs,
  onProjectTaskAssignment
} from "src/modules/project-tasks-activities/utils/dialogs.js";

// Shared Project Task Actions
import {
  initProjectTaskActions,
  onSubmitProjectTaskDelete,
  onSubmitProjectTaskStatus,
  onSubmitProjectTaskOwner,
  onSubmitLinkProjectTasksToPlan,
  onSubmitProjectTaskPriority,
  onSubmitProjectTaskEndDate,
  onSubmitProjectTaskTags
} from "src/modules/project-tasks/utils/actions.js";

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const $q = useQuasar();
const { toDate } = useFilters();
const authStore = useAuthStore();

const loading = ref(true);
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

const activeEdit = ref({ rowId: null, field: null });
const selectedProjectId = ref(history.state?.projectId);

const showFilter = ref(false);
const showMultiSelectOptions = ref(false);
const selectedField = ref(null);
const searchLoader = ref(false);
const manageDropDownTypes = ref([]);
const showManageDropdownOptions = ref(false);
const showSortDialog = ref(false);
const siteId = computed(() => authStore.user?.siteId);
// ----------------------------------------------------------------------------------------------------------------
// Local Storage:- DataTable and Advance Filter Values
// ----------------------------------------------------------------------------------------------------------------

const highlightedId = computed(() => { return activeRowId.value; });

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Project Tasks
// ----------------------------------------------------------------------------------------------------------------

const getAllProjectTaskList = async ({ pagination: p }) => {
  loading.value = true;

  const { page, rowsPerPage, sortBy, descending } = p;

  // sanitize task number
  const taskNumber = (search.value.projectTaskNumber || "").replace(/[^0-9]/g, "").replace(/^0+(?!$)/, "");
  search.value.projectTaskNumber = taskNumber || "0";

  // Build sort payload
  const sorts = Object.fromEntries(multiSort.value.filter(s => s.column && s.direction).map(s => [s.column, s.direction]));

  const payload = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    sorts,
    ...search.value
  };

  const storedTaskIds = JSON.parse(localStorage.getItem("selectedTaskIds") || "[]");
  // dependent dropdown
  if (search.value.projectIds > 0 && search.value.projectModuleIds > 0) {
    const { projectIdsFiltered, projectModuleIdsFiltered } = convertProjectIdsAndModuleIds(search.value.projectIds, search.value.projectModuleIds);
    projectTasksByProjectIdAndModuleIdForDropdown(search.value.isTemplate, projectIdsFiltered, projectModuleIdsFiltered);
  }

  try {
    const resp = await projectTaskService.getProjectTasks(payload);
    const isAdmin = role === "admin";
    rows.value = resp.data.map(task => transformTaskRow(task, storedTaskIds, isAdmin));

    Object.assign(pagination.value, {
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    });
    saveDataTableState({
      search: search.value,
      pagination: pagination.value,
      activeRowId: activeRowId.value,
      sorts
    });
  } finally {
    loading.value = false;
    searchLoader.value = false;
  }
};

function transformTaskRow (task, storedTaskIds, isAdmin) {
  const projectMapping = task?.project?.projectUserMappings?.[0] || {};
  const moduleMapping = task?.projectModule?.projectModulesUserMappings?.[0];

  const projectFullAccess = projectMapping.fullAccess ?? false;
  const projectNote = projectMapping.notes ?? false;

  const combinedEditable = isAdmin || (moduleMapping ? moduleMapping.fullAccess : projectFullAccess);
  const combinedNote = isAdmin || (moduleMapping ? moduleMapping.notes : projectNote);

  const issueMapping = task.projectTaskRelatedMappings?.find(m => m.issueId);
  const requirementMapping = task.projectTaskRelatedMappings?.find(m => m.requirementId);

  return {
    ...task,

    isNotes: combinedNote,
    isEditable: combinedEditable,
    isProjectEditable: isAdmin || projectFullAccess,

    checkboxStatus: storedTaskIds.includes(task.id),

    startDateStr: task.startDate ? toDate(task.startDate) : "",
    endDateStr: task.endDate ? toDate(task.endDate) : "",

    activity: task.projectActivities ?? [],

    taskTags: task.projectTask_Tags?.map(tag => ({
      text: tag.tags.name,
      value: tag.tags.id,
      color: tag.tags.color,
      bgColor: tag.tags.bgColor
    })) ?? [],

    issueNumbersWithStatuses: issueMapping && {
      value: String(issueMapping.id),
      text: `#${issueMapping.issue?.issueNumber ?? ""} (${issueMapping.issue?.status?.dropDownValue ?? ""})`,
      issueId: issueMapping.issueId
    },

    requirementNumbersWithStatuses: requirementMapping && {
      value: String(requirementMapping.id),
      text: `#${requirementMapping.requirement?.requirementNumber ?? ""} (${requirementMapping.requirement?.status?.dropDownValue ?? ""})`,
      requirementId: requirementMapping.requirementId
    },

    totalActivityHours: totalActivityHours(task.projectActivities)
  };
}

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const refreshProjectTaskList = () => {
  getAllProjectTaskList({ pagination: pagination.value });
};

const refreshProjectTaskTagsDropdown = () => {
  projectTaskTagsDropdown.load();
};

const expandedPersons = ref([]);
function toggleRow (row) {
  const idx = expandedPersons.value.indexOf(row.id);
  if (idx > -1) {
    expandedPersons.value.splice(idx, 1); // collapse row
  } else {
    expandedPersons.value.push(row.id); // expand row
  }
}

function isExpandedPersons (row) {
  return expandedPersons.value.includes(row.id);
}

function visiblePersons (row) {
  const activities = getGroupedActivities(row.projectActivities);
  return isExpandedPersons(row) ? activities : activities.slice(0, 3);
}

function getGroupedActivities (activities) {
  const result = [];
  const uniquePersonNames = new Set();

  activities.forEach((activity) => {
    const person = activity.assignedTo?.person;
    const fullName = person?.fullName;

    if (!uniquePersonNames.has(fullName)) {
      uniquePersonNames.add(fullName);

      const personActivities = activities.filter(
        (a) => a.assignedTo?.person?.fullName === fullName
      );

      result.push({
        initials: (person?.firstName?.[0] || "") + (person?.lastName?.[0] || ""),
        firstName: person?.firstName,
        lastName: person?.lastName,
        email: person?.primaryEmailAddress,
        bgColor: person?.bgColor,
        color: person?.color,
        fullName,
        activities: personActivities.map((a) => ({
          name: a.name,
          estimateHours: a.estimateHours || 0
        }))
      });
    }
  });

  return result;
}

function totalTimesheetHours () {
  const total = rows.value.reduce(
    (sum, row) => sum + (row.totalTimesheetEstHours || 0),
    0
  );

  return Number(total.toFixed(2));
}

function totalTaskEstimateTimeHours () {
  const total = rows.value.reduce(
    (sum, row) => sum + (row.estimateTime || 0),
    0
  );

  return Number(total.toFixed(2));
}

function totalEstimateHours () {
  const total = rows.value.reduce((sum, row) => {
    const activities = row.activity;
    if (!Array.isArray(activities)) return sum;

    return sum + activities.reduce(
      (activitySum, activity) => activitySum + (activity.estimateHours || 0),
      0
    );
  }, 0);

  return Number(total.toFixed(2));
}

function totalActivityHours (activities) {
  if (!Array.isArray(activities) || activities.length === 0) return 0;

  const total = activities.reduce(
    (sum, activity) => sum + (activity.estimateHours || 0),
    0
  );

  return Number(total.toFixed(2));
}

const disableBeforeStartDate = (startDateStr) => {
  if (!startDateStr) return () => true;

  // Convert MM/dd/yyyy string to Date
  const start = parse(startDateStr, "MM/dd/yyyy", new Date());

  return (date) => {
    // If incoming date is yyyy/MM/dd
    const currentDate = parse(date, "yyyy/MM/dd", new Date());

    return currentDate >= start;
  };
};

const onChangeProjectOrTemplate = () => {
  search.value.projectIds = [];
  search.value.projectModuleIds = [];
  search.value.projectTaskIds = [];
  refreshProjectTaskList();
  projectNameDropdown.load(search.value.isTemplate);
  projectModulesByProjectIdForDropdown.load(search.value.isTemplate);
  projectTasksByProjectIdAndModuleIdForDropdown.load(search.value.isTemplate);
};

function onHandleProjectTaskLevelTimeSheetView (id) {
  if (!search.value.isTemplate) {
    onProjectTaskLevelTimeSheetView(id, "DT");
  }
}

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true, default: true },
  { name: "projectModule.name", label: "Project Module", field: "projectModule.name", align: "left", sortable: true, default: true },
  { name: "name", label: "Task Name", field: "name", align: "left", sortable: true, default: true },
  { name: "area.dropDownValue", label: "Area", field: "area.dropDownValue", align: "left", sortable: true, default: false },
  { name: "workspace.dropDownValue", label: "Workspace", field: "workspace.dropDownValue", align: "left", sortable: true, default: false },
  { name: "action.dropDownValue", label: "Action", field: "action.dropDownValue", align: "left", sortable: true, default: false },
  { name: "projectTaskNumber", label: "Task No", field: "projectTaskNumber", align: "center", sortable: false, default: true },
  { name: "startDate", label: "Start Date", field: "startDate", align: "center", sortable: true, default: true },
  { name: "endDate", label: "Due Date", field: "endDate", align: "center", sortable: true, default: true },
  { name: "status.dropDownValue", label: "Status", field: "status.dropDownValue", align: "left", sortable: true, default: true },
  { name: "priority.dropDownValue", label: "Priority", field: "priority.dropDownValue", align: "left", sortable: true, default: true },
  { name: "assignedTo", label: "Task Owner", field: "assignedTo", align: "right", sortable: true, default: true },
  { name: "assignedTo.id", label: "Assigned To", field: "assignedTo.id", align: "right", sortable: true, default: true },
  { name: "estimateTime", label: "Task Hrs", field: "estimateTime", align: "right", sortable: false, default: true, tooltip: "(Sum Of Activity hours) / Task Est. Hrs / Timesheet Hrs" },
  { name: "type.dropDownValue", label: "Type", field: "type.dropDownValue", align: "left", sortable: true, default: false },
  { name: "taskTags", label: "Tags", field: row => row.taskTags, align: "left", sortable: false, default: false },
  { name: "sortOrder", label: "SortOrder", field: "sortOrder", align: "left", sortable: true, default: false },
  { name: "color", label: "Color", field: "color", align: "left", sortable: true, default: false },
  { name: "createdBy.person.firstName", label: "Created By", field: "createdBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true, default: false },
  { name: "UpdatedBy.person.firstName", label: "Updated By", field: "UpdatedBy.person.firstName", align: "left", sortable: true, default: false },
  { name: "updatedOnUtc", label: "Updated Date", field: "updatedOnUtc", align: "left", sortable: true, default: false }
]);

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
  storageKey: "project-Tasks-Index",
  siteId,

  // defaultSearch: {
  //   searchText: "",
  //   projectIds: [],
  //   projectCategoryIds: [],
  //   projectStatusIds: [],
  //   statusId: null,
  //   projectCoordinatorIds: [],
  //   projectLeadsIds: [],
  //   projectPriorityIds: [],
  //   projectTypeIds: [],
  //   customerIds: [],
  //   companyContactIds: [],
  //   isTemplate: false,
  //   projectTagIds: null
  // },
  defaultSearch: {
    searchText: "",
    projectTaskNumber: 0,
    customerIds: [],
    companyContactIds: [],
    projectIds: selectedProjectId.value
      ? [selectedProjectId.value]
      : [],
    projectModuleIds: [],
    projectTaskIds: [],
    projectLeadsIds: [],
    activityOwners: user?.employeeId
      ? [user.employeeId]
      : [],
    statusIds: [],
    priorityIds: [],
    taskTagsIds: [],
    isTemplate: false
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
// DataTable:- Column resize functionality
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
// DataTable:- Hide/Show Columns
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
// DataTable:- Sort Filter
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
    refreshProjectTaskList();
  }
});

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Multi-select Actions
// ----------------------------------------------------------------------------------------------------------------

const multiSelectProjectIds = ref([]);
const multiSelectProjectName = ref([]);
const multiSelectTaskProjectMap = ref({});
const multiSelectTaskIds = ref([]);
const multiSelectTaskNames = ref([]);
const multiSelectTaskStatusMap = ref({});

const onSelectCheckbox = (projectId, projectName, projectStatus, taskId, taskName, flag) => {
  if (flag === true) {
    if (!multiSelectTaskIds.value.includes(taskId)) {
      // Add the taskId to the multiSelectTaskIds array if it's not already present
      multiSelectTaskIds.value.push(taskId);
      multiSelectTaskNames.value.push(taskName);
      multiSelectTaskStatusMap.value[taskId] = projectStatus;
      multiSelectTaskProjectMap.value[taskId] = projectId;

      // Add projectId only if not already present
      if (!multiSelectProjectIds.value.includes(projectId)) {
        multiSelectProjectIds.value.push(projectId);
        multiSelectProjectName.value.push(projectName);
      }
    }
  } else {
    // Remove task
    const removedProjectId = multiSelectTaskProjectMap.value[taskId];
    const taskIndex = multiSelectTaskIds.value.indexOf(taskId);

    if (taskIndex !== -1) {
      multiSelectTaskIds.value.splice(taskIndex, 1);
      multiSelectTaskNames.value.splice(taskIndex, 1);
    }
    delete multiSelectTaskStatusMap.value[taskId];
    delete multiSelectTaskProjectMap.value[taskId];

    // If no other selected task belongs to that project, remove the projectId
    const stillHasTaskForProject = Object.values(multiSelectTaskProjectMap.value).some(pid => pid === removedProjectId);
    if (!stillHasTaskForProject) {
      multiSelectProjectIds.value = multiSelectProjectIds.value.filter(x => x !== removedProjectId);
      multiSelectProjectName.value = multiSelectProjectName.value.filter(x => x !== projectName);
    }
  }

  // Persist selections properly
  localStorage.setItem("selectedTaskIds", JSON.stringify(multiSelectTaskIds.value));
};

const selectedFieldOptions = [
  { label: "Link Task To Plan", value: "linkToPlan", icon: "o_calendar_view_week" },
  { label: "Change Status", value: "Status", icon: "o_flag" },
  { label: "Change Priority", value: "Priority", icon: "o_priority_high" },
  { label: "Change Tags", value: "Tags", icon: "o_local_offer" }
];

const onSelectMultiOptions = () => {
  const selectedTasks = rows.value.filter(task => multiSelectTaskIds.value.includes(task.id));

  // check if any selected task is not manage permission
  const hasNonEditable = selectedTasks.some(task => !task.isEditable);

  if (hasNonEditable) {
    notifyWarning({ message: "Some selected tasks have only view permission." });
    return;
  }
  activeRowId.value = multiSelectTaskIds.value;
  const selectedStatuses = multiSelectTaskIds.value.map(id => multiSelectTaskStatusMap.value[id]);
  const uniqueStatuses = Array.from(new Set(selectedStatuses));
  const commonStatus = uniqueStatuses.length === 1 ? uniqueStatuses[0] : null;

  const props = {
    taskIds: multiSelectTaskIds.value,
    selectedField: selectedField.value,
    ...(selectedField.value === "Status" && commonStatus ? { status: commonStatus } : {})
  };
  $q.dialog({
    component: selectMultiTask,
    componentProps: props
  }).onOk(() => {
    setDefaultsForMultiSelects();
    refreshProjectTaskList();
  }).onCancel(() => {
    selectedField.value = null;
  }).onDismiss(() => {
    activeRowId.value = null;
    selectedField.value = null;
  });
};

const onDeleteProjectTaskTag = (row, tagToRemove) => {
  zwConfirmDelete(
    {
      message: "Are you sure you want to remove the tag?"
    },
    () => {
      const updatedTags = row.taskTags.filter(
        t => t.value !== tagToRemove.value
      );
      onSubmitProjectTaskTags(
        row.id,
        updatedTags,
        refreshProjectTaskList,
        refreshProjectTaskTagsDropdown
      );
    }
  );
};

function setDefaultsForMultiSelects () {
  multiSelectProjectIds.value = [];
  multiSelectProjectName.value = [];
  multiSelectTaskProjectMap.value = [];
  multiSelectTaskIds.value = [];
  multiSelectTaskNames.value = [];
  multiSelectTaskStatusMap.value = [];
  localStorage.removeItem("selectedTaskIds");
}

// ----------------------------------------------------------------------------------------------------------------
// DataTable :- Initialization Of Dialogs, Actions
// ----------------------------------------------------------------------------------------------------------------

initProjectDialogs(activeRowId);
initProjectModuleDialogs(activeRowId);
initProjectTaskDialogs(activeRowId);
initProjectTaskActivityDialogs(activeRowId);
initSiteDialogs(activeRowId);

initProjectTaskActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

// Search records as per parameters
const onSearch = () => { refreshProjectTaskList(); };

// Clear search
const onClear = () => {
  search.value.projectTaskNumber = "";
  search.value.customerIds = [];
  search.value.companyContactIds = [];
  search.value.projectIds = [];
  if (selectedProjectId?.value?.length > 0) { selectedProjectId.value = ""; delete history?.state?.projectId; }
  search.value.projectModuleIds = [];
  search.value.projectTaskIds = [];
  search.value.projectLeadsIds = [];
  search.value.statusIds = [];
  search.value.priorityIds = [];
  search.value.taskTagsIds = [];

  saveDataTableState({
    search: search.value
  });
  onSearch();
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Applied Filter Labels.
// ----------------------------------------------------------------------------------------------------------------
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
  ...(search.value.projectTaskNumber > 0 ? { "Task Number": search.value.projectTaskNumber } : {}),
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name"),
  ...mapFilterToLabel(search.value.projectModuleIds, projectModulesByProjectIdForDropdown.list, "Project Module"),
  ...mapFilterToLabel(search.value.projectTaskIds, projectTasksByProjectIdAndModuleIdForDropdown.list, "Project Task"),
  ...mapFilterToLabel(search.value.projectLeadsIds, activeEmployeesDropdown.list, "Project Leads"),
  ...mapFilterToLabel(search.value.activityOwners, activeEmployeesDropdown.list, "Activity Owner"),
  ...mapFilterToLabel(search.value.statusIds, projectTaskStatusListWithDisables, "Task Status"),
  ...mapFilterToLabel(search.value.priorityIds, projectTaskPrioritiesForDropdown.list, "Task Priority"),
  ...mapFilterToLabel(search.value.customerIds, customerNameDropdown.list, "Customer"),
  ...mapFilterToLabel(search.value.companyContactIds, companyContactNameDropdown.list, "Company Contact"),
  ...mapFilterToLabel(search.value.taskTagsIds, projectTaskTagsDropdown.list, "Task Tags")
}));

const onClearFilters = (key) => {
  if (key === "Task Number") {
    search.value.projectTaskNumber = "";
  } else if (key === "Project Name") {
    search.value.projectIds = [];
    if (selectedProjectId?.value?.length > 0) { selectedProjectId.value = ""; delete history?.state?.projectId; }
  } else if (key === "Project Module") {
    search.value.projectModuleIds = [];
  } else if (key === "Project Task") {
    search.value.projectTaskIds = [];
  } else if (key === "Project Leads") {
    search.value.projectLeadsIds = [];
  } else if (key === "Task Status") {
    search.value.statusIds = [];
  } else if (key === "Activity Owner") {
    search.value.activityOwners = [];
  } else if (key === "Task Priority") {
    search.value.priorityIds = [];
  } else if (key === "Customer") {
    search.value.customerIds = [];
  } else if (key === "Company Contact") {
    search.value.companyContactIds = [];
  } else if (key === "Task Tags") {
    search.value.taskTagsIds = [];
  }
  delete appliedFilters.value[key];
  refreshProjectTaskList();
};

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Project Module": return search.value.projectModuleIds?.length || 0;
  case "Project Task": return search.value.projectTaskIds?.length || 0;
  case "Project Leads": return search.value.projectLeadsIds?.length || 0;
  case "Task Status": return search.value.statusIds?.length || 0;
  case "Activity Owner": return search.value.activityOwners?.length || 0;
  case "Task Priority": return search.value.priorityIds?.length || 0;
  case "Customer": return search.value.customerIds?.length || 0;
  case "Company Contact": return search.value.companyContactIds?.length || 0;
  case "Task Tags": return search.value.taskTagsIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const { projectNameDropdown } = projectModule();
const { projectModulesByProjectIdForDropdown } = projectModuleOfProjectModule();
const { projectTasksByProjectIdAndModuleIdForDropdown, projectTaskPrioritiesForDropdown, projectTaskTagsDropdown } = projectTaskModule();
const { activeEmployeesDropdown } = employeeModule();
const { customerNameDropdown } = customerModule();
const { companyContactNameDropdown } = companyContactsModule();
const { getDropdownTypesByModuleNameForDropdown } = manageDropdownModule();
const { tagsDropdown } = tagModule();
const { allUsersForDropdown } = userModule();

const convertProjectIdsAndModuleIds = (projectIds, moduleIds) => {
  const projectId = Array.isArray(projectIds) ? projectIds.join(",") : projectIds || null;
  const projectModuleId = Array.isArray(moduleIds) ? moduleIds.join(",") : moduleIds || null;
  return {
    projectId: projectId || null,
    projectModuleId: projectModuleId || null
  };
};

const handlePopupShow = (taskStatus, projectStatusLabel) => {
  getProjectTaskStatusForDropdown("Task Status", taskStatus, projectStatusLabel);
};

// Get all project task status List
const projectTaskStatusListRaw = ref([]);
const projectTaskStatusFilters = ref([]);
const projectTaskStatusListWithDisables = ref([]);

const getProjectTaskStatusForDropdown = (typeName, taskStatusLabel = null, projectStatusLabel = null) => {
  commonService.getDropDown(typeName).then((resp) => {
    const lockedStatuses = ["Cancelled", "Completed", "On Hold"];
    const responseData = resp.map((item) => {
      const label = item.dropdownValue;
      let shouldDisable = false;

      if (lockedStatuses.includes(projectStatusLabel) && taskStatusLabel === "New") {
        shouldDisable = label === "Open";
      }
      if (projectStatusLabel === "New") { shouldDisable = label === "Open"; }

      return {
        text: item.dropdownValue,
        value: item.id,
        disable: shouldDisable
      };
    });

    projectTaskStatusListRaw.value = responseData;
    projectTaskStatusListWithDisables.value = responseData.map(item => ({ ...item, disable: false }));
    projectTaskStatusFilters.value = projectTaskStatusListWithDisables.value;
  });
};

const getProjectTaskStatusFilter = (val, update, abort) => {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectTaskStatusListWithDisables.value = projectTaskStatusFilters.value;
    } else {
      projectTaskStatusListWithDisables.value = projectTaskStatusFilters.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
};

// ----------------------------------------------------------------------------------------------------------------
// Task Tags.
// ----------------------------------------------------------------------------------------------------------------

const showAllTagsRowId = ref(null);

const toggleShowAllTags = (rowId) => {
  showAllTagsRowId.value = showAllTagsRowId.value === rowId ? null : rowId;
};

// ----------------------------------------------------------------------------------------------------------------
// Watch on change
// ----------------------------------------------------------------------------------------------------------------

watch(selectedField, (newVal) => {
  if (newVal) {
    showMultiSelectOptions.value = false;
    if (newVal === "linkToPlan") {
      if (multiSelectProjectIds.value.length > 1) {
        $q.notify({
          type: "warning",
          message: "Cannot link plan: selected tasks are of multiple projects."
        });
        selectedField.value = null;
        return;
      }
      onSubmitLinkProjectTasksToPlan(
        rows,
        multiSelectProjectIds,
        multiSelectProjectName,
        multiSelectTaskIds,
        multiSelectTaskNames,
        refreshProjectTaskList,
        setDefaultsForMultiSelects
      );
      return;
    }
    if (newVal === "Status") {
      const uniqueStatuses = new Set(multiSelectTaskIds.value.map(id => multiSelectTaskStatusMap.value[id]));

      if (uniqueStatuses.size > 1) {
        $q.notify({
          type: "warning",
          message: "Cannot change status: selected tasks are from projects with different statuses."
        });
        selectedField.value = null;
        return;
      }

      const selectedTasks = rows.value.filter(item => multiSelectTaskIds.value.includes(item.id));
      // true if any of the selected tasks have status "close"
      const hasClosedTask = selectedTasks.some(item => item.status?.dropDownValue?.toLowerCase() === "close");
      if (hasClosedTask) {
        $q.notify({
          type: "warning",
          message: "Cannot change status: one or more selected tasks are already closed."
        });
        selectedField.value = null;
        return;
      }
    }
    onSelectMultiOptions(); // This opens the dialog for the selected action
  }
});

watch(multiSelectTaskIds, () => {
  if (multiSelectTaskIds.value.length === 0) showMultiSelectOptions.value = false;
}, { deep: true });

watch(
  () => search.value.searchText,
  () => {
    searchLoader.value = true;
    refreshProjectTaskList();
  }
);

watch(() => search.value.customerIds, (newValue, oldValue) => {
  if (search.value?.customerIds?.length === 0 || newValue === oldValue) return;

  companyContactNameDropdown.load(newValue);
}, { immediate: true });

watch(() => search.value.projectIds, async (newValue, oldValue) => {
  if (search.value?.projectIds?.length === 0 || newValue === oldValue) return;

  search.value.projectModuleIds = [];
  await projectModulesByProjectIdForDropdown.load(search.value.isTemplate, false, search.value.projectIds);
}, { immediate: true });

watch(() => search.value.projectModuleIds, (newValue, oldValue) => {
  if (search.value?.projectModuleIds?.length === 0 || newValue === oldValue) return;

  search.value.projectTaskIds = [];
  const { projectId, projectModuleId } = convertProjectIdsAndModuleIds(search.value.projectIds, newValue);
  projectTasksByProjectIdAndModuleIdForDropdown.load(search.value.isTemplate, projectId, projectModuleId);
}, { immediate: true });

// ----------------------------------------------------------------------------------------------------------------
// On page load
// ----------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  tableRef.value.requestServerInteraction();

  projectNameDropdown.load();
  if (search.value.projectIds.length > 0) projectModulesByProjectIdForDropdown.load(search.value.isTemplate, false, search.value.projectIds);

  projectTaskPrioritiesForDropdown.load("Task Priorities");
  projectTaskTagsDropdown.load();
  customerNameDropdown.load();
  companyContactNameDropdown.load();
  activeEmployeesDropdown.load(user.siteId);
  allUsersForDropdown.load(user.siteId);
  tagsDropdown.load();
  getProjectTaskStatusForDropdown("Task Status");

  // Admin:- Manage all Project Dropdowns and Types
  manageDropDownTypes.value = await getDropdownTypesByModuleNameForDropdown("Project Management");

  localStorage.removeItem("selectedTaskIds");

  if (search.value.isTemplate === null || search.value.isTemplate === undefined) {
    search.value.isTemplate = false;
  }
});
</script>
<style scoped>
.Custom-DataTable {
  min-width: max-content;
}
</style>
