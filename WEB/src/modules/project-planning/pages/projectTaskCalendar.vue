<template>
  <q-page class="q-pa-md">
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-12 col-md-3">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Projects" clickable to="/project" />
              <q-breadcrumbs-el label="Task Calendar" />
              <q-breadcrumbs-el v-if="projectName" :label="`${projectName}`" />
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
              <div class="row items-center justify-end no-wrap">
                <div class="search-container position-relative">
                  <searchFilterBar
                    v-model="searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <!-- Dropdown Content -->
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <q-card class="q-pa-sm">
                      <div v-if="search.calendarType.toLowerCase() === 'task'" class="row items-center q-mb-sm">
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
                        v-if="redirectFrom != null"
                        v-model="search.projectIds"
                        label="Project Name"
                        :options="projectNameDropdown.list.value"
                        :filter="projectNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-if="search.calendarType.toLowerCase() === 'module'"
                        v-model="search.projectModuleStatusIds"
                        label="Project Module Status"
                        :options="projectModuleStatusForDropdown.list.value"
                        :filter="projectModuleStatusForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-if="search.calendarType.toLowerCase() === 'task'"
                        v-model="search.projectModuleIds"
                        label="Project Module"
                        :options="projectModulesByProjectIdForDropdown.list.value"
                        :filter="projectModulesByProjectIdForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-if="search.calendarType.toLowerCase() === 'task'"
                        v-model="search.projectLeadsIds"
                        label="Project Leads"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <div v-if="search.calendarType.toLowerCase() === 'task'" class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Task Name</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div>
                            <q-input v-model="search.name" fill-input :dense="true" class="q-mx-sm" />
                          </div>
                        </div>
                      </div>
                      <multiSelectDropdown
                        v-if="search.calendarType.toLowerCase() === 'task'"
                        v-model="search.activityOwners"
                        label="Activity Owners"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-if="search.calendarType.toLowerCase() === 'task'"
                        v-model="search.statusIds"
                        label="Task Status"
                        :options="taskStatusList"
                        :filter="getTaskStatusListfilter"
                      />
                      <multiSelectDropdown
                        v-if="search.calendarType.toLowerCase() === 'task'"
                        v-model="search.priorityIds"
                        label="Task Priority"
                        :options="projectTaskPrioritiesForDropdown.list.value"
                        :filter="projectTaskPrioritiesForDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-if="search.calendarType.toLowerCase() === 'task'"
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
              </div>
              <div class="q-gutter-sm">
                <q-btn icon="o_chevron_left" outline label="Back" no-caps class="text-primary q-ml-md" @click="$router.back()">
                  <q-tooltip anchor="bottom middle" self="top middle">Back To List</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-card-section class="q-pa-sm">
        <div class="row items-center">
          <!-- Project Names and Colors -->
          <div class="col-lg-8 col-md-7 col-sm-12 col-xs-12">
            <div class="row items-center">
              <template v-if="selectedProjects.length > 1">
                <div
                  v-for="(project, index) in selectedProjects"
                  :key="project.value"
                  class="row items-center"
                >
                  <div class="text-bold q-mr-xs">{{ project.text }}</div>
                  <div
                    :class="['square-color q-ml-xs cursor-pointer', { blank: !project.color }]"
                    :style="project.color ? { backgroundColor: project.color } : {}"
                  />
                  <q-popup-proxy
                    cover
                    :offset="[0, 0]"
                    transition-show="scale"
                    transition-hide="scale"
                    anchor="top left"
                    self="top left"
                    @before-show="storePreviousColor(project, 'Project')"
                  >
                    <q-color
                      v-model="project.color"
                      no-header
                      no-footer
                      default-view="palette"
                      class="my-picker"
                      @update:model-value="startColorSelection"
                      @change="finalizeColorSelection(project, 'Project')"
                    />
                  </q-popup-proxy>

                  <span v-if="index < selectedProjects.length - 1" class="q-ml-xs">||</span>
                </div>
              </template>
            </div>
          </div>
          <div class="col-lg-4 col-md-5 col-sm-12 col-xs-12">
            <div class="row items-center justify-end no-wrap">
              <q-btn-toggle
                v-model="search.viewType"
                :options="viewTypeOptions"
                color="grey"
                toggle-color="primary"
                push
                spread
                no-caps
                class="q-mr-md"
                @update:model-value="onSearch"
              />
              <q-btn-toggle
                v-model="search.calendarType"
                :options="calendarTypeList"
                color="grey"
                toggle-color="primary"
                push
                spread
                no-caps
                class="q-mr-md"
                @update:model-value="onSearch"
              />
              <q-btn
                icon="o_chevron_left"
                class="text-primary"
                outline
                label="Previous"
                @click="onPreviousOffset"
              />
              <q-input
                v-model="pageOffset"
                class="text-primary q-mx-sm"
                outlined
                input-class="text-center"
                stack-label
                placeholder="1 to 7"
                min="1"
                max="7"
                style="width: 60px;"
                @blur="validatePageOffset"
              />
              <q-btn
                class="text-primary"
                outline
                @click="onNextOffset"
              >
                <span>Next</span>
                <q-icon name="o_chevron_right" class="q-ml-xs" />
              </q-btn>
            </div>
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-table
        v-model:pagination="pagination"
        class="equal-width-col"
        flat
        :loading="loading"
        :rows="calendar.rows"
        row-key="task.id"
        separator="cell"
        :rows-per-page-options="[15, 20, 30, 50, 100]"
        @request="getAllTaskByProjectId"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="text-white">
            <q-th
              v-for="col in calendar.columns" :key="col.index" :class="isNextOrPreviousDate(col.date) ? 'bg-grey' : 'bg-primary'"
            >
              <span v-if="col.filterType === 'week'">
                {{ col.displayDateRange }}
              </span>
              <span v-else-if="col.filterType === 'month'">
                {{ col.index }}
              </span>
              <span v-else>
                {{ String(new Date(col.date).getMonth() + 1).padStart(2, '0') }}/{{ String(new Date(col.date).getDate()).padStart(2, '0') }}
              </span>
              <q-tooltip v-if="col.dateTooltip">{{ col.dateTooltip }}</q-tooltip>
              <q-tooltip v-else>{{ formatDate(col.date) }}</q-tooltip>
            </q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr :props="props">
            <template v-if="search.calendarType.toLowerCase() === 'task'">
              <q-td
                v-for="(taskRow, colIndex) in props.row.tasks"
                :key="taskRow.id || 'empty-' + colIndex"
                :colspan="taskRow.colspan || 1"
                :class="[taskRow.type === 'task' ? 'cursor-move' : '']"
                :style="getRowStyle(taskRow)"
                @click="() => {
                  if (taskRow.type !== 'task') {
                    onAddTask(getAccurateColIndex(props.row.tasks, colIndex), props.row.tasks);
                  }
                }"
              >
                <div v-if="taskRow.type === 'task'" class="ellipsis-text">
                  <q-btn dense flat icon="o_more_vert" size="sm" :color="getTextColor(taskRow.task.color)" auto-close="true" style="padding: 0px !important;" @click.stop>
                    <q-menu>
                      <q-list style="min-width: 200px">
                        <q-separator />
                        <q-item v-if="props.row.status != 'Close'" v-ripple clickable class="" @click.stop="onProjectTaskEdit(taskRow.task.id, refreshProjectTaskList)">
                          <q-item-section avatar><q-icon name="o_edit" color="" size="xs" /></q-item-section>
                          <q-item-section class="">Edit</q-item-section>
                        </q-item>
                        <q-item v-if="taskRow.task.status.dropDownValue != 'Close'" v-ripple clickable @click="onProjectTaskFiles(taskRow.task.id, taskRow.task.name, taskRow.task.project.name, taskRow.task.projectModule.name)">
                          <q-item-section avatar><q-icon name="o_description" size="xs" /></q-item-section>
                          <q-item-section>Files</q-item-section>
                        </q-item>
                        <q-item v-ripple clickable>
                          <q-item-section avatar><q-icon name="o_calendar_month" size="xs" /></q-item-section>
                          <q-item-section>Change Task Dates</q-item-section>
                          <q-menu anchor="top end" self="top start">
                            <q-date
                              :model-value="taskRow.calendarModel"
                              range
                              mask="MM/DD/YYYY"
                              color="secondary"
                              :options="date => isDateEnabled(date, taskRow.task.project.startDate, taskRow.task.project.goLiveDate)"
                              @update:model-value="val => {
                                taskRow.calendarModel = normalizeCalendarInput(val)
                                onUpdateCalendar(taskRow)
                                $refs['qDateProxy' + taskRow.task.id]?.hide()
                              }"
                            />
                          </q-menu>
                        </q-item>
                        <q-item v-if="taskRow.task.status.dropDownValue != 'Close'" v-ripple clickable>
                          <q-item-section avatar>
                            <q-icon name="o_flag" color="secondary" size="xs" />
                          </q-item-section>
                          <q-item-section class="">Task Status</q-item-section>
                          <q-menu
                            anchor="top end"
                            self="top start"
                            @before-show="() => handleTaskPopupShow(taskRow.id, taskRow.task.status.dropDownValue, projectStatus)"
                          >
                            <q-item
                              v-for="taskStatus in projectTaskStatusMap[taskRow.id] || []"
                              :key="taskStatus.value"
                              v-ripple
                              clickable
                              :disable="taskStatus.disable"
                              :style="{
                                cursor: shouldDisableSelect(taskRow.task) ? 'not-allowed' : 'auto',
                                pointerEvents: shouldDisableSelect(taskRow.task) ? 'none' : 'auto',
                                opacity: shouldDisableSelect(taskRow.task) ? 0.6 : 1
                              }"
                              @click="onSubmitProjectTaskStatus(taskStatusList, taskRow.task.id, taskStatus.value, refreshProjectTaskList)"
                            >
                              <q-item-section avatar>
                                <q-icon
                                  v-if="taskStatus.value === taskRow.task.status.id"
                                  name="o_task_alt"
                                  color="primary"
                                  size="xs"
                                />
                                <q-icon
                                  v-else
                                  name="o_radio_button_unchecked"
                                  color="secondary"
                                  size="xs"
                                />
                              </q-item-section>
                              <q-item-section style="white-space: nowrap !important;">
                                {{ taskStatus.text }}
                              </q-item-section>
                            </q-item>
                          </q-menu>
                        </q-item>
                        <q-item v-if="taskRow.task.status.dropDownValue != 'Close'" v-ripple clickable>
                          <q-item-section avatar><q-icon name="o_priority_high" color="secondary" size="xs" /></q-item-section>
                          <q-item-section class="">Task Priority</q-item-section>
                          <q-menu anchor="top end" self="top start">
                            <q-item v-for="priority in projectTaskPrioritiesForDropdown.list.value" :key="priority.value" v-ripple clickable
                             @click="onSubmitProjectTaskPriority(taskRow.task.id, priority.value, refreshProjectTaskList)"
                             >
                              <q-item-section avatar>
                                <q-icon v-if="priority.value === taskRow.task.priority.id" name="o_task_alt" color="primary" size="xs" />
                                <q-icon v-else name="o_radio_button_unchecked" color="secondary" size="xs" />
                              </q-item-section>
                              <q-item-section class="">{{ priority.text }}</q-item-section>
                            </q-item>
                          </q-menu>
                        </q-item>
                        <q-item v-if="taskRow.task.status.dropDownValue != 'Close'" v-ripple clickable class="">
                          <q-item-section avatar><q-icon name="o_person_add" color="secondary" size="xs" /></q-item-section>
                          <q-item-section class="">Task Owner?</q-item-section>
                          <q-menu anchor="top end" self="top start" @before-show="() => onDropdownOpen(taskRow.task.projectId)">
                            <q-item v-ripple clickable class="flex column">
                              <div class="q-pa-xs">
                                <formSingleSelectDropdown
                                  v-model="taskRow.task.assignedToId"
                                  :options="projectEmployeeDropdownSingleSelect.list.value"
                                  :filter="projectEmployeeDropdownSingleSelect.filter"
                                  @update:model-value="onSubmitProjectTaskOwner(taskRow.task.id, taskRow.task.assignedToId, refreshProjectTaskList)"
                                />
                              </div>
                            </q-item>
                          </q-menu>
                        </q-item>
                        <q-item v-if="taskRow.task.status.dropDownValue != 'Close'" v-ripple clickable @click="openTagDialog(taskRow.task.id)">
                          <q-item-section avatar><q-icon name="o_local_offer" size="xs" /></q-item-section>
                          <q-item-section>Add Tags</q-item-section>
                        </q-item>
                        <q-item v-if="taskRow.task.status.dropDownValue != 'Close'" v-ripple clickable @click="onProjectTaskStatusChangeLog(taskRow.task.id)">
                          <q-item-section avatar><q-icon name="o_flag" size="xs" /></q-item-section>
                          <q-item-section>Task Status Change Log</q-item-section>
                        </q-item>
                        <q-item v-ripple clickable @click="onProjectTaskLevelTimeSheetView(taskRow.task.id)">
                          <q-item-section avatar><q-icon name="o_notes" size="xs" /></q-item-section>
                          <q-item-section>Task Level Timesheet</q-item-section>
                        </q-item>
                        <q-item v-if="taskRow.task.status.dropDownValue != 'Close'" v-ripple clickable @click="onNoteAdd(taskRow.task.id, 'Project Task', taskRow.task.projectId, taskRow.task.project.name, taskRow.task.name, refreshProjectTaskList)">
                          <q-item-section avatar><q-icon name="o_note" size="xs" /></q-item-section>
                          <q-item-section>Note</q-item-section>
                        </q-item><q-separator />
                        <q-item v-if="taskRow.task.status.dropDownValue != 'Close'" v-ripple class="" clickable @click="onProjectTaskCopy(taskRow.task.id, taskRow.task.name, taskRow.task.projectModuleId, 'isCopy', refreshProjectTaskList)">
                          <q-item-section avatar><q-icon name="o_copy" size="xs" /></q-item-section>
                          <q-item-section>Copy</q-item-section>
                        </q-item>
                        <q-item v-if="taskRow.task.status.dropDownValue != 'Close'" v-ripple class="" clickable @click="onProjectTaskMove(taskRow.task.id, taskRow.task.name, taskRow.task.projectModuleId, 'isMove', refreshProjectTaskList)">
                          <q-item-section avatar><q-icon name="o_arrow_forward" size="xs" /></q-item-section>
                          <q-item-section>Move</q-item-section>
                        </q-item>
                        <q-item v-if="taskRow.task.status.dropDownValue != 'Close'">
                          <q-item-section avatar>
                            <q-icon v-if="taskRow.task.color" name="o_circle" :style="`border-radius:50%;color:${taskRow.task.color}; background-color:${taskRow.task.color};`" size="xs" />
                            <q-icon v-else name="o_question_mark" size="xs" />
                          </q-item-section>
                          <q-item-section class="row items-center" style="display: flex; align-items: center; justify-content: start; flex-direction: row;">
                            Task Color
                            <q-icon
                              name="o_colorize"
                              class="cursor-pointer q-ml-xs"
                              size="xs"
                              @click.stop="storePreviousColor(taskRow.task, 'Task')"
                            >
                              <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                                <q-color
                                  v-model="taskRow.task.color"
                                  no-header
                                  no-footer
                                  default-view="palette"
                                  class="my-picker"
                                  @update:model-value="startColorSelection"
                                  @change="finalizeColorSelection(taskRow.task, 'Task')"
                                />
                              </q-popup-proxy>
                            </q-icon>
                          </q-item-section>
                        </q-item>
                        <q-separator />
                        <q-item v-if="taskRow.task.status.dropDownValue != 'Close'" v-ripple v-close-popup clickable
                        @click="onSubmitProjectTaskDelete(taskRow.task.id, taskRow.task.name, taskRow.task.project.name, refreshProjectTaskList)"
                        >
                          <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                          <q-item-section class="text-negative">Delete</q-item-section>
                        </q-item>
                      </q-list>
                    </q-menu>
                    <q-tooltip>View More</q-tooltip>
                  </q-btn>
                  <span class="hoverable-cell q-ml-xs" @click="onProjectTaskView(taskRow.task.id)">{{ taskRow.task.name }} ({{ taskRow.task.status.dropDownValue }})
                    <q-tooltip>{{ taskRow.task.name }} ({{ taskRow.task.status.dropDownValue }}) {{ taskRow.task.startDate && taskRow.task.endDate ? '(' + computeDateDiff(taskRow.task.startDate, taskRow.task.endDate) + ')' : "" }}</q-tooltip>
                  </span>
                  <span
                    v-if="taskRow.task.startDate && taskRow.task.endDate"
                    class="justify-end q-ml-auto" :color="getTextColor(taskRow.task.color)"
                  >
                    {{ computeDateDiff(taskRow.task.startDate, taskRow.task.endDate) }}
                    <q-tooltip>Duration in Days</q-tooltip>
                  </span>
                </div>
                <q-dialog v-model="showTagDialog[taskRow.id]">
                  <q-card class="rightDialog" style="width: 300px; max-width: 90vw; height: 310px; max-height: 80vh; white-space: normal; overflow-wrap: break-word;">
                    <q-card-section>
                      <TagEditor
                        v-model="taskRow.taskTags"
                        :row-id="taskRow.id"
                        :available-tags="projectTaskTagsDropdown.list.value"
                        @save="({ tags, rowId }) => saveTagsData(tags, rowId)"
                        @filter="filterTags"
                        @close="() => showTagDialog[taskRow.id] = false"
                      />
                    </q-card-section>
                  </q-card>
                </q-dialog>
              </q-td>
            </template>
            <template v-else>
              <q-td
                v-for="(moduleRow, colIndex) in props.row.modules"
                :key="moduleRow.id || 'empty-' + colIndex"
                :colspan="moduleRow.colspan || 1"
                :class="[moduleRow.type === 'module' ? 'cursor-move' : '']"
                :style="getRowStyle(moduleRow)"
                @click="() => {
                  if (moduleRow.type !== 'module') {
                    onAddModule(getAccurateColIndex(props.row.modules, colIndex), props.row.modules);
                  }
                }"
              >
                <div v-if="moduleRow.type === 'module'" class="ellipsis-text">
                  <q-btn dense flat icon="o_more_vert" size="sm" :color="getTextColor(moduleRow.module.color)" auto-close="true" style="padding: 0px !important;" @click.stop>
                    <q-menu auto-close>
                      <q-list style="min-width: 180px">
                        <q-item v-ripple clickable @click="onProjectFilesAdd(moduleRow.module.id, moduleRow.module.name, moduleRow.module.project.name)">
                          <q-item-section avatar><q-icon name="o_description" size="xs" /></q-item-section>
                          <q-item-section>Files</q-item-section>
                        </q-item>
                        <q-item v-ripple clickable @click="onProjectModuleEdit(moduleRow.module.id, refreshProjectTaskList)">
                          <q-item-section avatar><q-icon name="o_edit" size="xs" /></q-item-section>
                          <q-item-section>Edit</q-item-section>
                        </q-item>
                        <q-item v-ripple clickable @click="onProjectModuleCopy(moduleRow.module.id, moduleRow.module.name, refreshProjectTaskList)">
                          <q-item-section avatar><q-icon name="o_content_copy" size="xs" /></q-item-section>
                          <q-item-section>Copy to Project</q-item-section>
                        </q-item>
                        <q-item>
                          <q-item-section avatar>
                            <q-icon v-if="moduleRow.module.color" name="o_circle" :style="`border-radius:50%;color:${moduleRow.module.color}; background-color:${moduleRow.module.color};`" size="xs" />
                            <q-icon v-else name="o_question_mark" size="xs" />
                          </q-item-section>
                          <q-item-section class="row items-center" style="display: flex; align-items: center; justify-content: start; flex-direction: row;">
                            Project Module Color
                            <q-icon
                              name="o_colorize"
                              class="cursor-pointer q-ml-xs"
                              size="xs"
                              @click.stop="storePreviousColor(moduleRow.module, 'Module')"
                            >
                              <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                                <q-color
                                  v-model="moduleRow.module.color"
                                  no-header
                                  no-footer
                                  default-view="palette"
                                  class="my-picker"
                                  @update:model-value="startColorSelection"
                                  @change="finalizeColorSelection(moduleRow.module, 'Module')"
                                />
                              </q-popup-proxy>
                            </q-icon>
                          </q-item-section>
                        </q-item>
                        <q-separator />
                        <q-item v-ripple clickable @click="onSubmitProjectModuleDelete(moduleRow.module.id, moduleRow.module.name, moduleRow.module.project.name, refreshProjectTaskList)">
                          <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                          <q-item-section class="text-negative">Delete</q-item-section>
                        </q-item>
                      </q-list>
                    </q-menu>
                    <q-tooltip>View More</q-tooltip>
                  </q-btn>
                  <span class="hoverable-cell q-ml-xs" @click="onProjectModuleView(moduleRow.module.id)">{{ moduleRow.module.name }} ({{ moduleRow.module.projectModuleStatus.dropDownValue }})
                    <q-tooltip>{{ moduleRow.module.name }} ({{ moduleRow.module.projectModuleStatus.dropDownValue }}) {{ moduleRow.module.startDate && moduleRow.module.endDate ? '(' + computeDateDiff(moduleRow.module.startDate, moduleRow.module.endDate) + ')' : "" }}</q-tooltip>
                  </span>
                  <span
                    v-if="moduleRow.module.startDate && moduleRow.module.endDate"
                    class="justify-end q-ml-auto" :color="getTextColor(moduleRow.module.color)"
                  >
                    {{ computeDateDiff(moduleRow.module.startDate, moduleRow.module.endDate) }}
                    <q-tooltip>Duration in Days</q-tooltip>
                  </span>
                </div>
              </q-td>
            </template>
          </q-tr>
        </template>
      </q-table>
      <q-card-actions v-if="selectedTypeLabel !== 'month'" align="right">
        <div class="col-auto q-ml-md">
          <q-btn class="text-primary" outline icon="o_keyboard_double_arrow_left" label="Previous" @click="onPreviousPage()">
            <q-tooltip>Previous Page</q-tooltip>
          </q-btn>
          <q-btn class="text-primary q-ml-md" outline @click="onNextPage()">
            <span>Next</span>
            <q-icon name="o_keyboard_double_arrow_right" class="q-ml-xs" />
            <q-tooltip>Next Page</q-tooltip>
          </q-btn>
        </div>
      </q-card-actions>
    </q-card>
  </q-page>
</template>

<script setup>
import { ref, onMounted, computed, watch } from "vue";
import { useQuasar, date } from "quasar";
import { notifySuccess, setLocalStorage, clearLocalStorage, getLocalStorage } from "assets/utils";
import { useAuthStore } from "stores/auth";

import commonService from "services/common.service";
import TagEditor from "src/modules/project-tasks/components/_taskTagEditor.vue";
import useFilters from "composables/useFilters";

import projectTaskService from "modules/project-tasks/projectTasks.service";
import projectService from "modules/project/projects.service";
import taskService from "src/modules/project-tasks/projectTasks.service";
import projectModulesService from "modules/project-modules/projectModules.service";

import editProjectTask from "modules/project-tasks/components/addEdit.vue";
import editProjectModule from "modules/project-modules/components/addEdit.vue";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";
import companyContactsModule from "src/modules/company-contacts/utils/dropdowns.js";
import projectPlanningModule from "src/modules/project-planning/utils/dropdowns.js";

// Shared Project Module Dialogs
import {
  initProjectModuleDialogs,
  onProjectModuleView,
  onProjectModuleEdit,
  onProjectModuleCopy,
  onProjectFilesAdd
} from "src/modules/project-modules/utils/dialogs.js";

// Shared Project Task Dialogs
import {
  initProjectTaskDialogs,
  onProjectTaskView,
  onProjectTaskEdit,
  onProjectTaskCopy,
  onProjectTaskMove,
  onProjectTaskLevelTimeSheetView,
  onProjectTaskFiles,
  onProjectTaskStatusChangeLog
} from "src/modules/project-tasks/utils/dialogs.js";

// Shared Notes Dialogs
import {
  initCommonDialogs,
  onNoteAdd
} from "src/modules/common/utils/dialogs.js";

// Shared Project Task Actions
import {
  initProjectTaskActions,
  onSubmitProjectTaskOwner,
  onSubmitProjectTaskStatus,
  onSubmitProjectTaskPriority,
  onSubmitProjectTaskDelete
} from "src/modules/project-tasks/utils/actions.js";

// SOP Change :- Shared Project Actions
import {
  initProjectModuleActions,
  onSubmitProjectModuleDelete
} from "src/modules/project-modules/utils/actions.js";

const authStore = useAuthStore();
const user = authStore.user;
const { toDate } = useFilters();
const $q = useQuasar();
const loading = ref(true);
const selectedProjectId = history.state?.projectId ?? null;
const projectName = history.state?.projectName;
const redirectFrom = history.state?.flag || null;
let selectedTypeLabel = null;
const showFilter = ref(false);
const searchLoader = ref(false);
const showTagDialog = ref({}); // To control popup per row
const openTagDialog = (rowId) => {
  showTagDialog.value[rowId] = true;
};

const calendar = ref({ columns: [], rows: [] });
const formatDate = (d) => date.formatDate(d, "MM/DD/YYYY (ddd)");

// local storage values
const localStorageKey = "Project Tasks Calendar";
const filterLocalStorage = getLocalStorage(localStorageKey);
const searchText = ref(filterLocalStorage?.searchText || "");
const projectTaskNumber = filterLocalStorage ? filterLocalStorage.projectTaskNumber : 0;
const projectIds = selectedProjectId != null ? [selectedProjectId] : filterLocalStorage?.projectIds ?? [];
const projectModuleIds = filterLocalStorage ? filterLocalStorage.projectModuleIds : [];
const statusIds = filterLocalStorage ? filterLocalStorage.statusIds : [];
const priorityIds = filterLocalStorage ? filterLocalStorage.priorityIds : [];
const name = filterLocalStorage ? filterLocalStorage.name : "";
const customerIds = filterLocalStorage ? filterLocalStorage.customerIds : [];
const companyContactIds = filterLocalStorage ? filterLocalStorage.companyContactIds : [];
const taskTagsIds = filterLocalStorage ? filterLocalStorage.taskTagsIds : [];
const pagination = ref(filterLocalStorage?.pagination || { sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const highlightProjectId = filterLocalStorage?.activeRowId || null;
const projectLeadsIds = filterLocalStorage ? filterLocalStorage.projectLeadsIds : [];
const projectModuleStatusIds = filterLocalStorage ? filterLocalStorage.projectModuleStatusIds : [];
const viewType = filterLocalStorage ? filterLocalStorage.viewType : "";
const calendarType = filterLocalStorage ? filterLocalStorage.calendarType : "Task";
const calendarMonthStr = getCurrentMonthYear();

const activeRowId = ref(highlightProjectId);
function getCurrentMonthYear () {
  const today = new Date();
  const year = today.getFullYear();
  const month = String(today.getMonth() + 1).padStart(2, "0"); // 2-digit month
  return `${year}-${month}`; // Format as 'YYYY-MM'
}
const selectedProjects = computed(() =>
  projectNameDropdown.list.value.filter(p => search.value.projectIds.includes(p.value))
);

// Search variables
const search = ref({
  projectIds,
  viewType,
  calendarType,
  searchText,
  projectTaskNumber,
  name,
  projectModuleIds,
  projectLeadsIds,
  statusIds,
  priorityIds,
  customerIds,
  companyContactIds,
  taskTagsIds,
  projectModuleStatusIds,
  calendarMonthStr,
  activityOwners: user?.employeeId ? [user.employeeId] : [],
  offset: 0,
  startDateStr: null
});

const viewTypeOptions = computed(() =>
  Array.isArray(calendarFilterForDropdownSingleSelect.list.value)
    ? calendarFilterForDropdownSingleSelect.list.value.map(item => ({
      label: item.text || item.label,
      value: item.value
    }))
    : []
);

const getAllTaskByProjectId = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const taskNumber = search.value.projectTaskNumber ? search.value.projectTaskNumber.replace(/[^0-9]/g, "").replace(/^0+(?!$)/, "") : "";
  search.value.projectTaskNumber = taskNumber || "0";
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  setLocalStorage(localStorageKey, { ...search.value, pagination: props.pagination, activeRowId: activeRowId.value });
  projectTaskService.getAllTaskByProjectId(payload).then((resp) => {
    calendar.value = resp;
    resp.rows.forEach(row => {
      row.tasks = [];
      row.modules = [];

      for (let i = 0; i < row.beforeStartDateCount; i++) {
        row.tasks.push({ type: "empty", key: `before-${i}` });
        row.modules.push({ type: "empty", key: `before-${i}` });
      }
      if (search.value.calendarType === "Task") {
        row.tasks.push({
          type: "task",
          colspan: row.taskDurationColspan,
          id: row.task.id,
          task: row.task,
          taskTags: typeof row.task.projectTask_Tags === "string"
            ? row.task.projectTask_Tags.split(",").map(tagName => ({
              text: tagName,
              value: getTagbyId(tagName.trim()).value !== undefined && getTagbyId(tagName.trim()).value !== null ? getTagbyId(tagName.trim()).value : "",
              color: getTagbyId(tagName.trim()).color !== undefined && getTagbyId(tagName.trim()).color !== null ? getTagbyId(tagName.trim()).color : "#000000",
              bgColor: getTagbyId(tagName.trim()).bgColor
            }))
            : Array.isArray(row.task.projectTask_Tags)
              ? row.task.projectTask_Tags.map(tag => ({
                text: tag.tags?.name ?? tag.name ?? "",
                value: tag.tags?.id ?? tag.id ?? "",
                color: tag.tags?.color ?? tag.color ?? "",
                bgColor: tag.tags?.bgColor ?? tag.bgColor ?? ""
              }))
              : [],
          calendarModel: {
            from: toDate(row.task.startDate),
            to: toDate(row.task.endDate)
          }
        });
      } else {
        row.modules.push({
          type: "module",
          colspan: row.moduleDurationColspan,
          id: row.module.id,
          module: row.module,
          calendarModel: {
            from: toDate(row.module.startDate),
            to: toDate(row.module.endDate)
          }
        });
      }
      pagination.value.page = page;
      pagination.value.rowsPerPage = rowsPerPage;
      pagination.value.sortBy = sortBy;
      pagination.value.descending = descending;
      pagination.value.rowsNumber = resp.total;

      for (let i = 0; i < row.afterEndDateCount; i++) {
        row.tasks.push({ type: "empty", key: `after-${i}` });
        row.modules.push({ type: "empty", key: `after-${i}` });
      }
    });
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

const onSearch = () => {
  const propps = { pagination: pagination.value };
  getAllTaskByProjectId(propps);
};

// Clear search
const onClear = () => {
  // Always cleared fields
  search.value.projectIds = selectedProjectId != null ? [selectedProjectId] : [];
  search.value.customerIds = [];
  search.value.companyContactIds = [];

  const clearTaskFields = () => {
    search.value.projectTaskNumber = "";
    search.value.name = "";
    search.value.projectModuleIds = [];
    search.value.projectLeadsIds = [];
    search.value.statusIds = [];
    search.value.priorityIds = [];
    search.value.taskTagsIds = [];
  };

  const clearModuleFields = () => {
    search.value.projectModuleStatusIds = [];
  };

  const type = search.value.calendarType?.toLowerCase();
  if (type === "task") {
    clearTaskFields();
  } else if (type === "module") {
    clearModuleFields();
  }

  clearLocalStorage(localStorageKey);
  onSearch();
};

function isDateEnabled (dateStr, startDateStr, endDateStr) {
  const [y, m, d] = dateStr.split("/").map(Number);
  const date = new Date(y, m - 1, d);

  const [sm, sd, sy] = startDateStr.split("/").map(Number);
  const start = new Date(sy, sm - 1, sd);

  if (!endDateStr) {
    return date >= start;
  }

  const [em, ed, ey] = endDateStr.split("/").map(Number);
  const end = new Date(ey, em - 1, ed);

  return date >= start && date <= end;
}

function normalizeCalendarInput (val) {
  if (typeof val === "string") {
    return { from: val, to: val };
  }
  if (val && val.from && !val.to) {
    return { from: val.from, to: val.from };
  }
  return val;
}

function computeDateDiff (start, end) {
  const startDate = new Date(start);
  const endDate = new Date(end);
  if (isNaN(startDate) || isNaN(endDate)) return "";
  const diffTime = endDate - startDate;
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  return diffDays >= 0 ? diffDays : 0;
}

function onUpdateCalendar (taskRow) {
  setTimeout(function () {
    if (taskRow.calendarModel?.to) {
      projectTaskService.updateProjectTaskEndDate(taskRow.task.id, taskRow.calendarModel.to, taskRow.calendarModel.from).then(resp => {
        notifySuccess({ message: "Task dates is saved successfully." });
        getAllTaskByProjectId({ pagination: pagination.value });
      });
    }
  });
}

// ==============================================================================================
// Next and Previous button
// ==============================================================================================
let offsetCounter = 0;
const pageOffset = ref(1);

const validatePageOffset = () => {
  const parsed = parseInt(pageOffset.value, 10);
  if (isNaN(parsed) || parsed < 1 || parsed > 7) {
    pageOffset.value = 1;
  } else {
    pageOffset.value = parsed;
  }
};

function onPreviousOffset () {
  if (pageOffset.value !== offsetCounter) {
    offsetCounter = Number(offsetCounter) - Number(pageOffset.value);
  } else {
    offsetCounter -= 1;
  }
  search.value.offset = offsetCounter;
  search.value.startDateStr = null;
  getAllTaskByProjectId({ pagination: pagination.value });
}

function onNextOffset () {
  if (pageOffset.value !== offsetCounter) {
    offsetCounter = Number(offsetCounter) + Number(pageOffset.value);
  } else {
    offsetCounter += 1;
  }
  // pageOffset.value = offsetCounter;
  search.value.offset = offsetCounter;
  search.value.startDateStr = null;
  getAllTaskByProjectId({ pagination: pagination.value });
}

const getWeekEndDate = (date) => {
  const weekEnd = new Date(date);
  // Assuming week ends on Saturday (6)
  weekEnd.setDate(weekEnd.getDate() + (6 - weekEnd.getDay()));
  return weekEnd;
};

const onNextPage = () => {
  // Get last date from current columns
  const columns = calendar.value.columns;
  if (!columns.length) return;

  const lastDate = new Date(columns[columns.length - 1].date);
  let newStartDate;

  if (selectedTypeLabel === "week") {
    const lastWeekEndDate = getWeekEndDate(lastDate);
    newStartDate = new Date(lastWeekEndDate);
    newStartDate.setDate(lastWeekEndDate.getDate() + 1);
  } else {
    newStartDate = new Date(lastDate);
    newStartDate.setDate(newStartDate.getDate() + 1);
  }

  // Fetch tasks based on new start date
  search.value.startDateStr = toDate(newStartDate);
  search.value.offset = 0;
  getAllTaskByProjectId({ pagination: pagination.value });
};

const onPreviousPage = () => {
  const columns = calendar.value.columns;
  if (!columns.length) return;

  if (selectedTypeLabel === "week") {
    // Send the first date (current page's start)
    search.value.startDateStr = toDate(new Date(columns[0].date));
    search.value.offset = -1;
  } else {
    const firstDate = new Date(columns[0].date);
    const newEndDate = new Date(firstDate);
    newEndDate.setDate(newEndDate.getDate() - 1);

    // Move back by the same number of columns (page size)
    const totalColumns = columns.length;
    const newStartDate = new Date(newEndDate);
    newStartDate.setDate(newEndDate.getDate() - (totalColumns - 1));
    search.value.startDateStr = toDate(newStartDate);
    search.value.offset = 0;
  }
  getAllTaskByProjectId({ pagination: pagination.value });
};

function isNextOrPreviousDate (colDate) {
  // return current;
  const date = new Date(colDate);
  const today = new Date();

  // Strip time portions
  const current = new Date(date.toDateString());
  const now = new Date(today.toDateString());

  const selectedType = calendarFilterForDropdownSingleSelect.list.value.find(item => item.value === search.value.viewType);

  if (selectedType?.text.toLowerCase() === "day") {
    // Check if it's the current day
    return current.getTime() !== now.getTime();
  } else if (selectedType?.text.toLowerCase() === "week") {
    // Check if current date falls within the week
    const weekStart = new Date(current);
    weekStart.setDate(weekStart.getDate() - weekStart.getDay()); // Sunday
    const weekEnd = new Date(weekStart);
    weekEnd.setDate(weekEnd.getDate() + 6); // Saturday

    return !(now >= weekStart && now <= weekEnd);
  } else if (selectedType?.text.toLowerCase() === "month") {
    // Check if it's the current month
    return (
      current.getFullYear() !== now.getFullYear() ||
      current.getMonth() !== now.getMonth()
    );
  }
  return true;
}

// =========================================================================
// Task and Module colors
// =========================================================================
const getRowStyle = (row) => {
  if (row.type !== "task" && row.type !== "module") return {};

  const defaultColor = "#f3f3f3";
  let bgColor = defaultColor;
  if (search.value.projectIds.length !== 0 && search.value.projectIds !== null && search.value.projectIds.length > 1) {
    if (row.type.toLowerCase() === "task") {
      bgColor = row.task.project.projectColor || defaultColor;
    } else if (row.type.toLowerCase() === "module") {
      bgColor = row.module.project.projectColor || defaultColor;
    }
  } else {
    if (row.type.toLowerCase() === "task") {
      bgColor = row.task.color || defaultColor;
    } else if (row.type.toLowerCase() === "module") {
      bgColor = row.module.color || defaultColor;
    }
  }

  const textColor = getTextColor(bgColor);
  return {
    backgroundColor: bgColor,
    color: textColor
  };
};

const getTextColor = (colorStr) => {
  if (!colorStr) return "#000";

  const rgb = parseColorToRgb(colorStr);
  if (!rgb) return "#000";

  const brightness = (rgb.r * 299 + rgb.g * 587 + rgb.b * 114) / 1000;
  return brightness > 128 ? "#000" : "#fff";
};

const parseColorToRgb = (colorStr) => {
  // Handle rgb(r, g, b)
  if (colorStr.startsWith("rgb")) {
    const match = colorStr.match(/\d+/g);
    if (!match || match.length < 3) return null;
    return {
      r: parseInt(match[0], 10),
      g: parseInt(match[1], 10),
      b: parseInt(match[2], 10)
    };
  }

  // Handle hex via canvas fallback (supports named colors too)
  const ctx = document.createElement("canvas").getContext("2d");
  ctx.fillStyle = colorStr;
  const hex = ctx.fillStyle.replace("#", "");

  if (hex.length !== 6) return null;
  return {
    r: parseInt(hex.substring(0, 2), 16),
    g: parseInt(hex.substring(2, 4), 16),
    b: parseInt(hex.substring(4, 6), 16)
  };
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals
// ----------------------------------------------------------------------------------------------------------------

const refreshProjectTaskList = () => {
  getAllTaskByProjectId({ pagination: pagination.value });
};

function getAccurateColIndex (tasks, currentIndex) {
  let index = 0;
  for (let i = 0; i < currentIndex; i++) {
    index += tasks[i].colspan || 1;
  }
  return index;
}

const onAddTask = (colIndex, row) => {
  let projectId = null;

  const found = row.find(t => t.type === "task");
  if (found?.task?.projectId) {
    projectId = found.task.projectId;
  }
  const selectedType = calendarFilterForDropdownSingleSelect.list.value.find(item => item.value === search.value.viewType);
  if (selectedType?.text.toLowerCase() === "day") {
    const column = calendar.value.columns[colIndex];
    if (!column) return;

    // Ensure column.date is a Date object
    const columnDate = new Date(column.date);
    $q.dialog({
      component: editProjectTask,
      componentProps: { startDate: toDate(columnDate), endDate: toDate(columnDate), projectIdAttr: projectId }
    }).onOk(() => {
      getAllTaskByProjectId({ pagination: pagination.value });
    }).onCancel(() => {
    }).onDismiss(() => {
    });
  }
};

const onAddModule = (colIndex, row) => {
  let projectId = null;

  const found = row.find(t => t.type === "module");
  if (found?.module?.projectId) {
    projectId = found.module.projectId;
  }
  const selectedType = calendarFilterForDropdownSingleSelect.list.value.find(item => item.value === search.value.viewType);
  if (selectedType?.text.toLowerCase() === "day") {
    const column = calendar.value.columns[colIndex];
    if (!column) return;

    // Ensure column.date is a Date object
    const columnDate = new Date(column.date);
    $q.dialog({
      component: editProjectModule,
      componentProps: { startDate: toDate(columnDate), endDate: toDate(columnDate), projectIdAttr: projectId }
    }).onOk(() => {
      getAllTaskByProjectId({ pagination: pagination.value });
    }).onCancel(() => {
    }).onDismiss(() => {
    });
  }
};
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Add Tags
// --------------------------------------------------------------------------------------------------------------------------------------------------
function getTagbyId (id) {
  const tag = projectTaskTagsDropdown.list.value.find(tag => tag.text === id);
  return tag;
}
const inputValue = ref("");
function filterTags (val, update, abort) {
  const needle = val?.toLowerCase() || "";
  inputValue.value = val;
  const filtered = needle
    ? projectTaskTagsDropdown.list.value.filter(v => v.text.toLowerCase().includes(needle))
    : [...projectTaskTagsDropdown.list.value];

  if (typeof update === "function") {
    update(() => {
      projectTaskTagsDropdown.list.value = filtered;
    });
  } else {
    projectTaskTagsDropdown.list.value = filtered;
  }
  // mark typed flag
  // typedFlags.taskTag = val && val.length > 0;
}

const saveTagsData = async (names, rowId) => {
  const tagInput = names;
  model.value.taskIds = [rowId];
  model.value.flag = null;
  model.value.tagsNameList = tagInput.map(tag => tag.text);

  try {
    await taskService.saveTags(model.value);
    notifySuccess({ message: "Tag saved successfully." });
    projectTaskTagsDropdown.load();
    tagValues.value[rowId] = [];
  } finally {
    loading.value = false;
  }
};

const projectTaskStatusMap = ref({});

function handleTaskPopupShow (rowId, taskStatusLabel, projectStatusLabel) {
  getTaskStatusList("Task Status", taskStatusLabel, projectStatusLabel).then((list) => {
    projectTaskStatusMap.value[rowId] = list;
  });
}

function shouldDisableSelect (row) {
  const taskList = projectTaskStatusMap.value[row.id] || [];
  const selectedStatus = taskList.find(item => item.value === row.statusId);
  return selectedStatus?.text === "Done";
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update Project, Module and Task Color
// --------------------------------------------------------------------------------------------------------------------------------------------------

let isSliderActive = false;

const previousColor = ref(""); // Store previous color
const storePreviousColor = (row, type) => {
  if (type === "Project") {
    previousColor.value = row.color ? row.color : "#e0e0e0"; // Store previous color before opening picker
  } else if (type === "Task") {
    previousColor.value = row.color ? row.color : "#e0e0e0"; // Store previous color before opening picker
  }
};

const startColorSelection = () => {
  isSliderActive = true; // The user is select with the slider
};

const finalizeColorSelection = (row, type) => {
  if (!isSliderActive) return false;
  isSliderActive = true; // Reset the flag

  // Check if the color has changed before submitting
  if (isSliderActive) {
    if (row.color !== previousColor.value) {
      if (type === "Project") {
        onSubmitColor(row.value, row.color, "Project");
      } else if (type === "Task") {
        onSubmitColor(row.id, row.color, "Task");
      } else if (type === "Module") {
        onSubmitColor(row.id, row.color, "Module");
      }
      return true;
    }
  }
  return false;
};

const onSubmitColor = (id, color, type) => {
  const payload = { id, color };
  const projectPayload = { id, projectColor: color };
  setTimeout(() => {
    if (type === "Project") {
      projectService.updateProjectColor(id, projectPayload).then(resp => {
        notifySuccess({ message: "Color updated successfully." });
        getAllTaskByProjectId({ pagination: pagination.value });
      });
    } else if (type === "Task") {
      taskService.updateTaskColor(id, payload).then(resp => {
        notifySuccess({ message: "Color updated successfully." });
        getAllTaskByProjectId({ pagination: pagination.value });
      });
    } else if (type === "Module") {
      projectModulesService.updateModuleColor(id, payload).then(resp => {
        notifySuccess({ message: "Color updated successfully." });
        getAllTaskByProjectId({ pagination: pagination.value });
      });
    }
  });
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable :- Initialization Of Dialogs, Actions
// ----------------------------------------------------------------------------------------------------------------
initProjectModuleDialogs(activeRowId);
initProjectTaskDialogs(activeRowId);
initCommonDialogs(activeRowId);
initProjectTaskActions(activeRowId);
initProjectModuleActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------

const { projectNameDropdown, projectEmployeeDropdownSingleSelect } = projectModule();
const { projectModuleStatusForDropdown, projectModulesByProjectIdForDropdown } = projectModuleOfProjectModule();
const { projectTaskPrioritiesForDropdown, projectTaskTagsDropdown } = projectTaskModule();
const { activeEmployeesDropdown } = employeeModule();
const { customerNameDropdown } = customerModule();
const { companyContactNameDropdown } = companyContactsModule();
const { calendarFilterForDropdownSingleSelect } = projectPlanningModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Get All Dropdowns
// --------------------------------------------------------------------------------------------------------------------------------------------------
const calendarTypeList = [
  { label: "Module", value: "Module" },
  { label: "Task", value: "Task" }
];

const taskStatusList = ref([]);
const taskStatusListoptions = ref([]);
function getTaskStatusList (typeName, taskStatusLabel = null, projectStatusLabel = null) {
  return commonService.getDropDown(typeName).then((resp) => {
    const lockedStatuses = ["Cancelled", "Completed", "On Hold"];

    const responseData = resp.map((item) => {
      const label = item.dropdownValue;
      let shouldDisable = false;

      if (lockedStatuses.includes(projectStatusLabel) && taskStatusLabel === "New") {
        shouldDisable = label === "Open";
      }

      if (projectStatusLabel === "New") {
        shouldDisable = label === "Open";
      }

      return {
        text: label,
        value: item.id,
        disable: shouldDisable
      };
    });

    taskStatusList.value = responseData.map(item => ({ ...item, disable: false }));
    taskStatusListoptions.value = taskStatusList.value;

    return responseData;
  });
}
// Search task status List for dropdown
function getTaskStatusListfilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      taskStatusList.value = taskStatusListoptions.value;
    } else {
      taskStatusList.value = taskStatusListoptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

function onDropdownOpen (projectId) {
  if (projectId) {
    projectEmployeeDropdownSingleSelect.load(projectId);
  }
}

const tagValues = ref({});
const model = ref({ taskIds: [], tagsNameList: [], color: "" });

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

const appliedFilters = computed(() => {
  const baseFilters = {
    ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name"),
    ...mapFilterToLabel(search.value.customerIds, customerNameDropdown.list, "Customer"),
    ...mapFilterToLabel(search.value.companyContactIds, companyContactNameDropdown.list, "Company Contact")
  };

  if (search.value.calendarType === "Task") {
    Object.assign(baseFilters,
      search.value.projectTaskNumber > 0
        ? { "Task Number": search.value.projectTaskNumber }
        : {},
      mapFilterToLabel(search.value.projectModuleIds, projectModulesByProjectIdForDropdown.list, "Project Module"),
      mapFilterToLabel(search.value.projectLeadsIds, activeEmployeesDropdown.list, "Project Leads"),
      search.value.name ? { "Task Name": search.value.name } : {},
      mapFilterToLabel(search.value.activityOwners, activeEmployeesDropdown.list, "Activity Owner"),
      mapFilterToLabel(search.value.statusIds, taskStatusList, "Task Status"),
      mapFilterToLabel(search.value.priorityIds, projectTaskPrioritiesForDropdown.list, "Task Priority"),
      mapFilterToLabel(search.value.taskTagsIds, projectTaskTagsDropdown.list, "Task Tags")
    );
  } else {
    Object.assign(baseFilters,
      mapFilterToLabel(search.value.projectModuleStatusIds, projectModuleStatusForDropdown.list, "Project Module Status")
    );
  }

  return baseFilters;
});

function onClearFilters (key) {
  const baseClearMap = {
    "Project Name": () => {
      search.value.projectIds = selectedProjectId != null ? [selectedProjectId] : [];
    },
    Customer: () => {
      search.value.customerIds = [];
    },
    "Company Contact": () => {
      search.value.companyContactIds = [];
    }
  };

  const taskClearMap = {
    "Task Number": () => { search.value.projectTaskNumber = ""; },
    "Project Module": () => { search.value.projectModuleIds = []; },
    "Project Leads": () => { search.value.projectLeadsIds = []; },
    "Task Name": () => { search.value.name = ""; },
    "Task Status": () => { search.value.statusIds = []; },
    "Activity Owner": () => { search.value.activityOwners = []; },
    "Task Priority": () => { search.value.priorityIds = []; },
    "Task Tags": () => { search.value.taskTagsIds = []; }
  };

  const nonTaskClearMap = {
    "Project Module Status": () => { search.value.projectModuleStatusIds = []; }
  };

  // Run appropriate clear function
  if (baseClearMap[key]) {
    baseClearMap[key]();
  }

  if (search.value.calendarType === "Task" && taskClearMap[key]) {
    taskClearMap[key]();
  } else if (search.value.calendarType.toLowerCase() === "module" && nonTaskClearMap[key]) {
    nonTaskClearMap[key]();
  }

  delete appliedFilters.value[key];
  getAllTaskByProjectId({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Project Module": return search.value.projectModuleIds?.length || 0;
  case "Project Leads": return search.value.projectLeadsIds?.length || 0;
  case "Task Status": return search.value.statusIds?.length || 0;
  case "Activity Owner": return search.value.activityOwners?.length || 0;
  case "Task Priority": return search.value.priorityIds?.length || 0;
  case "Customer": return search.value.customerIds?.length || 0;
  case "Company Contact": return search.value.companyContactIds?.length || 0;
  case "Task Tags": return search.value.taskTagsIds?.length || 0;
  case "Project Module Status": return search.value.projectModuleStatusIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => searchText.value, () => {
  if (searchText.value) searchLoader.value = true;
  getAllTaskByProjectId({ pagination: pagination.value });
});

watch(() => search.value.customerIds, (newValue, oldValue) => {
  if (search.value?.customerIds?.length === 0 || newValue === oldValue) return;

  companyContactNameDropdown.load(newValue);
}, { immediate: true });

watch(inputValue, (val) => {
  const needle = val ? val.toLowerCase() : "";
  projectTaskTagsDropdown.list.value = needle
    ? projectTaskTagsDropdown.list.value.filter((v) => v.text.toLowerCase().includes(needle))
    : [...projectTaskTagsDropdown.list.value];
});

watch(() => search.value.projectIds, async (newValue, oldValue) => {
  if (search.value?.projectIds?.length === 0 || newValue === oldValue) return;

  search.value.projectModuleIds = [];
  await projectModulesByProjectIdForDropdown.load(search.value.isTemplate, false, search.value.projectIds);
}, { immediate: true });

watch(() => search.value.viewType, (newValue) => {
  const selectedType = calendarFilterForDropdownSingleSelect.list.value.find(
    item => item.value === newValue
  );

  selectedTypeLabel = (selectedType?.text || selectedType?.label || "day").toLowerCase();
}, { immediate: true });

onMounted(async () => {
  await calendarFilterForDropdownSingleSelect.load("Calendar View Type");
  const defaultViewType = calendarFilterForDropdownSingleSelect.list.value.find(
  filter => (filter.text || filter.label || "").toLowerCase() === "day"
);

if (defaultViewType) {
  search.value.viewType = defaultViewType.value;
}

  const propps = { pagination: pagination.value };
  getAllTaskByProjectId(propps);
  projectNameDropdown.load();
  projectTaskPrioritiesForDropdown.load("Task Priorities");
  await projectTaskTagsDropdown.load();
  getTaskStatusList("Task Status");
  customerNameDropdown.load();
  if (search.value.projectIds.length > 0) projectModulesByProjectIdForDropdown.load(search.value.isTemplate, false, search.value.projectIds);
  activeEmployeesDropdown.load(user.siteId);
  projectModuleStatusForDropdown.load("WO Status");
});
</script>
<style scoped>
.equal-width-col td,
.equal-width-col th {
  width: 100px;
  max-width: 60px;
  min-width: 60px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.ellipsis-text {
  max-width: 100%;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
  /* display: inline-block; */
}
.q-btn-toggle .q-btn {
  min-height: 32px; /* or adjust */
}
/* .blur-column {
  background-color: rgba(200, 200, 200, 0.2); // Light grey with transparency
  filter: blur(0.5px);
} */
.square-color {
  width: 16px;
  height: 16px;
  border-radius: 4px;
}
.square-color.blank {
  background-color: #f3f3f3;
  border: 1px solid #ccc;
}
</style>
