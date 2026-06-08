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
              <q-breadcrumbs-el label="My Work" />
              <q-breadcrumbs-el label="My Task and Activities" />
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
              <div class="row items-center" style="flex-wrap: nowrap;">
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
                        v-model="search.assignedToIds"
                        label="Activity Owner"
                        :options="activeEmployeesDropdown.list.value"
                        :filter="activeEmployeesDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.activityNameIds"
                        label="Activity Name"
                        :options="projectTaskActivityNameDropdown.list.value"
                        :filter="projectTaskActivityNameDropdown.filter"
                      />
                      <multiSelectDropdown
                        v-model="search.activityStatusIds"
                        label="Activity Status"
                        :options="projectTaskActivityStatusDropdown.list.value"
                        :filter="projectTaskActivityStatusDropdown.filter"
                        :isShowAll="true"
                      />
                      <multiSelectDropdown
                        v-model="search.statusIds"
                        label="Task Status"
                        :options="projectTaskStatusForDropdown.list.value"
                        :filter="projectTaskStatusForDropdown.filter"
                        :isShowAll="true"
                      />
                      <!-- <singleSelectDropdown
                        v-model="search.activeStatus"
                        label="Active/Inactive"
                        :options="projectTaskActivityActiveInActiveDropdown.list.value"
                      /> -->
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Active/Inactive</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <q-select
                            v-model="search.activeStatus"
                            clearable
                            class="q-mx-sm w-100 h-auto"
                            stack-label
                            hide-bottom-space
                            use-input
                            :dense="true"
                            :options="statusList"
                            emit-value
                            map-options
                            :popup-content-class="customPopupContentClass"
                          />
                        </div>
                      </div>
                      <div class="row items-center q-mb-sm">
                        <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                          <label class="Cutomlabel q-mt-sm fs-13">Sprint Week End Date</label>
                        </div>
                        <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                          <div class="input-group q-mx-sm w-100 h-auto">
                            <q-input v-model="search.sprintWeekEndDate" fill-input dense mask="##/##/####">
                              <template #append>
                                <q-icon name="o_calendar_month" class="cursor-pointer">
                                  <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                    <q-date
                                      v-model="search.sprintWeekEndDate"
                                      :options="isSunday"
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
                      <!-- Search and Clear Buttons -->
                      <div class="row justify-end q-gutter-sm q-mb-sm">
                        <q-btn
                          style="width: 20%;"
                          outline color="primary"
                          label="Search"
                          class="btnRounded"
                          no-caps
                          @click="() => { showFilter = false; onSearch(); }"
                        />
                        <q-btn
                          style="width: 20%;"
                          outline
                          color="grey-4"
                          label="Clear"
                          class="text-grey-9 btnRounded"
                          no-caps
                          @click="onAdvanceClear"
                        />
                        <q-btn
                          style="width: 20%;"
                          outline
                          color="negative"
                          label="Close"
                          class="btnRounded"
                          no-caps
                          @click="() => { showFilter = false; }"
                        />
                      </div>
                    </q-card>
                  </q-menu>
                </div>
              </div>
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
              <div class="q-ml-xs">
                <q-btn
                  color="primary"
                  :class="routeName === 'project-tast-activities'? 'hidden' : ''"
                  outline
                  label="Fill Daily Plan"
                  type="button"
                  no-caps
                  class="btnRounded q-mr-sm q-px-lg"
                  :disabled="ActivityIds.length === 0 || rows.some(project => project.activities.some(r => ActivityIds.includes(r.id) && !r.active))"
                  @click="onSendDailyPlan"
                />
                <q-btn
                  color="primary"
                  :class="routeName === 'project-tast-activities'? 'hidden' : ''"
                  outline
                  label="Fill Timesheet"
                  type="button"
                  no-caps
                  class="btnRounded q-px-lg"
                  :disabled="ActivityIds.length === 0 || rows.some(project => project.activities.some(r => ActivityIds.includes(r.id) && !r.active))"
                  @click="onSendTimesheet"
                />
                <q-btn
                  icon="o_checklist"
                  outline
                  no-caps
                  class="text-primary btnRounded q-ml-sm"
                  :disabled="ActivityIds.length === 0" @click.stop="showMultiSelectOptions = !showMultiSelectOptions"
                >
                  <q-badge
                    v-if="ActivityIds?.length > 0"
                    :label="ActivityIds.length"
                    class="primary"
                    floating
                  />
                  <q-tooltip>Multi Actions</q-tooltip>
                </q-btn>
                <!-- Button to Open Sorting Dialog -->
                <q-btn
                  color="primary"
                  icon="o_sort"
                  class="btnRounded q-ml-sm"
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
      <q-separator />
      <q-table
        ref="tableRef"
        v-model:pagination="pagination"
        :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
        :rows-per-page-options="[20, 50, 100, 200, 500]"
        :loading="loading"
        :rows="rows"
        :columns="columns.filter(col => col.hidden)"
        row-key="project.id"
        separator="cell"
        no-data-label="No data available"
        binary-state-sort
        @request="getProjectActivities"
      >
        <template #loading>
          <q-inner-loading showing color="primary">
            <q-spinner-ios size="40px" class="q-mt-xl" />
          </q-inner-loading>
        </template>
        <template #header="props">
          <q-tr :props="props" class="bg-primary text-white">
            <q-th v-for="col in props.cols" :key="col.name" :props="props">
              {{ col.label }}
            </q-th>
          </q-tr>
        </template>
        <template #body="props">
          <q-tr
            :props="props"
          >
            <q-td style="width: 100%;" class="hoverable-cell">
              <div class="row items-center">
                <!-- Expand / Collapse Icon -->
                <q-icon
                  :name="isExpanded(props.row.project.id) ? '-' : '+'"
                  class="cursor-pointer custom-plus-minus-icon q-mr-sm"
                  @click.stop="toggleExpand(props.row.project.id)"
                >
                  <q-tooltip>{{ isExpanded(props.row.project.id) ? 'Collapse' : 'Expand' }}</q-tooltip>
                </q-icon>

                <!-- Project Name -->
                <span v-if="props.row.project?.name" @click="onProjectView(props.row.project.id)">
                  {{ props.row.project.name }}
                </span>
                <div class="row items-center q-gutter-sm q-ml-sm" style="flex-shrink: 0;">
                  <q-icon
                    name="o_task" size="xs"
                    class="cursor-pointer"
                    @click="setActiveRowIdInLocalStorage(props.row.project.id); $router.push({ path: `/all-project-planner`, state: {projectId: props.row.project.id } })"
                  >
                    <q-tooltip>Project Planner</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_radio_button_checked" size="xs"
                    class="cursor-pointer"
                    @click="setActiveRowIdInLocalStorage(props.row.id); $router.push({ path: '/project-center', state: { projectId: props.row.project.id } })"
                  >
                    <q-tooltip>Project Center</q-tooltip>
                  </q-icon>
                  <q-icon
                    name="o_developer_board" size="xs"
                    class="cursor-pointer"
                    @click="setActiveRowIdInLocalStorage(props.row.id); $router.push({ path: '/project-planning/workboard', state: {projectId: props.row.project.id } })"
                  >
                    <q-tooltip>Work Board</q-tooltip>
                  </q-icon>
                  <q-badge
                    v-if="getSelectedActivitiesCount(props.row.project.id) > 0"
                    color="primary"
                    class="rounded-full text-center flex items-center justify-center q-pa-none"
                    style="width: 20px; height: 20px;"
                  >
                    {{ getSelectedActivitiesCount(props.row.project.id) }}
                  </q-badge>
                </div>
              </div>
            </q-td>
          </q-tr>

          <!-- Expanded Row (all other columns) -->
          <q-tr v-if="isExpanded(props.row.project.id)" class="expanded-row">
            <q-td colspan="100%" style="padding: 0;">
              <div class="q-table__expanded-row bg-primary">
                <q-table
                  :pagination="projectActivityPagination[props.row.project.id]"
                  :rows="props.row.activities"
                  :rows-per-page-options="[20, 50, 100, 200, 500]"
                  :columns="columns.filter(col => !col.hidden)"
                  flat
                  bordered
                  class="q-pa-sm"
                  @update:pagination="val => {
                    projectActivityPagination = {
                      ...projectActivityPagination,
                      [props.row.project.id]: { ...val }
                    }
                  }"
                >
                  <!-- @update:pagination="val => projectActivityPagination[props.row.project.id] = val" -->
                  <template #header="props">
                    <q-tr :props="props" class="bg-grey-4 text-black">
                      <q-th auto-width class="text-center" :class="routeName === 'project-tast-activities'? 'hidden' : ''" />
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                      <q-th auto-width class="text-center">Actions</q-th>
                    </q-tr>
                  </template>
                  <template #body="activityProps">
                    <q-tr
                      :props="activityProps"
                      :class="[
                        highlightedId == activityProps.row.id ? 'highlight' : '',
                        (activityProps.row.activityStatus?.dropDownValue?.toLowerCase() !== 'open' &&
                          activityProps.row.activityStatus?.dropDownValue?.toLowerCase() !== 'completed'|| activityProps.row.isDescription )
                          ? 'bg-light-red'
                          : ''
                      ]"
                    >
                      <q-tooltip v-if="activityProps.row.activityStatus?.dropDownValue?.toLowerCase() !== 'open' || activityProps.row.isDescription ">
                        Please add activity details and open the task activity to start filling the timesheet and daily plan for this activity.
                      </q-tooltip>
                      <!-- Active / Checkbox -->
                      <q-td class="text-center" style="width: 5%;">
                        <div
                          :class="['dot-circle q-mr-xs hoverable-cell', activityProps.row.active ? 'dot-active' : 'dot-inactive']"
                          @click="() => { onSubmitProjectTaskActivityStatus(activityProps.row, refreshProjectTaskActivityList) }"
                        >
                          <q-tooltip v-if="!activityProps.row.active">Set Active?</q-tooltip>
                          <q-tooltip v-else>Set Inactive?</q-tooltip>
                        </div>
                        <q-checkbox
                          v-model="activityProps.row.checkboxStatus"
                          @update:model-value="onSelectCheckbox(activityProps.row.id, activityProps.row.active, activityProps.row.activityStatus.dropDownValue, $event)"
                        />
                      </q-td>

                      <!-- Project Module -->
                      <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;" class="hoverable-cell" @click="onProjectModuleView(activityProps.row.projectModule.id)">
                        <span
                          v-if="activityProps.row.showModuleName"
                        >
                          {{ activityProps.row.projectModule.name }}
                        </span>
                      </q-td>

                      <!-- Task Name -->
                      <q-td style="width: 20%; white-space: normal;" class="hoverable-cell">
                        <div class="row no-wrap items-center justify-between">
                          <span style="flex: 1; word-break: break-word; white-space: normal;">
                            <!-- <span v-if="preProjectTaskName !== props.row.task.name" :set="preProjectTaskName = props.row.task.name" @click="onViewTask(props.row.task.id)"> -->
                            <span
                              v-if="activityProps.row.showTaskNameAndIcons" @click="onProjectTaskView(activityProps.row.task.id, refreshProjectTaskActivityList)"
                            >
                              {{ activityProps.row.task.name }}</span>
                          </span>
                        </div>
                      </q-td>

                      <!-- Week Dates -->
                      <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 5%;">
                        <div v-if="activityProps.row.weekDates?.length" class="col-9 flex justify-center TaskActivity">
                          <span
                            v-for="(d, idx) in activityProps.row.weekDates"
                            :key="idx"
                            class="hoverable-cell"
                            style="cursor: pointer;"
                            @click="openPlan(d, activityProps.row.id)"
                          >
                            {{ d.text }}
                          </span>
                        </div>
                      </q-td>
                      <!-- Activity Name -->
                      <q-td style="width: 10%;">
                        {{ activityProps.row.name }}
                        <q-icon
                          v-if="activityProps.row.activityNameDescription"
                          name="o_info"
                          size="17px"
                          class="q-ml-xs"
                        >
                          <q-tooltip v-if="activityProps.row.activityNameDescription" class="text-wrap break-words" max-width="300px">
                            <div v-html="activityProps.row.activityNameDescription" />
                          </q-tooltip>
                        </q-icon>
                      </q-td>

                      <!-- Assigned To -->
                      <q-td style="width: 10%;">
                        {{ activityProps.row.assignedTo.person.fullName || " " }}
                      </q-td>

                      <!-- Activity Status -->
                      <q-td style="width: 5%;">
                        <q-select
                          v-model="activityProps.row.activityStatus.id"
                          outlined
                          stack-label
                          hide-bottom-space
                          :dense="true"
                          :options="statusOptions(activityProps.row)"
                          class="task-activity-status-list"
                          option-value="value"
                          option-label="text"
                          option-disable="disable"
                          emit-value
                          map-options
                          :bg-color="getStatusColor(activityProps.row.activityStatus.dropDownValue)"
                          :disable="isClose"
                          @update:model-value="onChangeActivityStatus(activityProps.row.id, activityProps.row.activityStatus.id)"
                        />
                      </q-td>

                      <!-- Estimate Hours -->
                      <q-td style="width: 5%;" class="text-right">{{ activityProps.row.estimateHours }}</q-td>

                      <!-- Actions -->
                      <q-td style="width: 5%;" class="text-center actions">
                        <q-icon
                          :name="activityProps.row.isDescription ? 'o_lock' : 'o_article'"
                          size="xs"
                          :class="[
                            'cursor-pointer q-mr-sm'
                          ]"
                          @click="handleDescriptionClick(activityProps.row)"
                        >
                          <q-tooltip>{{ activityProps.row.isDescription ? 'Add Description' : 'Update Activity Description' }}</q-tooltip>
                          <q-popup-edit
                            v-model="activityProps.row.description"
                            anchor="center middle"
                            self="center middle"
                            persistent
                            class="instruction-popup"
                            :buttons="false"
                          >
                            <template #default="scope">
                              <div class="popup-container q-pa-sm" @mouseenter="activeScope = scope">
                                <q-btn
                                  icon="o_close"
                                  flat
                                  round
                                  dense
                                  size="sm"
                                  class="absolute-top-right"
                                  @click="scope.cancel"
                                />
                                <div class="text-subtitle2 q-mb-xs">Activity Description<span class="required">*</span></div>
                                <div class="editor-wrapper relative-position">
                                  <q-editor
                                    v-model="activityProps.row.description"
                                    :dense="$q.screen.lt.md"
                                    :toolbar="toolbar"
                                    :fonts="fonts"
                                    class="fixed-editor"
                                  />
                                  <q-inner-loading
                                    v-if="activityProps.row.isLoadingDescription"
                                    showing
                                    class="absolute-full"
                                    color="primary"
                                    size="30px"
                                  />
                                </div>
                                <div class="q-mt-md flex justify-center">
                                  <q-btn
                                    label="Cancel"
                                    flat
                                    class="q-ml-sm"
                                    @click="scope.cancel"
                                  />
                                  <q-btn
                                    label="Save"
                                    text-color="blue"
                                    unelevated
                                    :loading="processing"
                                    :disable="processing"
                                    @click="async () => {
                                      const success = await handleDescriptionSave(
                                        activityProps.row,
                                        activityProps.row.description
                                      );
                                      if (success) {
                                        scope.cancel(); // Close ONLY if successful
                                      }
                                    }"
                                  />
                                </div>
                              </div>
                            </template>
                          </q-popup-edit>
                        </q-icon>
                        <q-icon
                          name="o_visibility"
                          class="cursor-pointer q-mr-sm"
                          size="xs"
                          :class="activityProps.row.isDescription ? 'text-grey-5 cursor-not-allowed' : ''"
                          @click="!activityProps.row.isDescription && onProjectTaskActivityView(activityProps.row.id)"
                        >
                          <q-tooltip>
                            {{ activityProps.row.isDescription ? 'Please Add Description First' : 'View' }}
                          </q-tooltip>
                        </q-icon>
                        <q-icon
                          name="o_edit"
                          class="cursor-pointer q-mr-sm"
                          size="xs"
                          :class="activityProps.row.isDescription ? 'text-grey-5 cursor-not-allowed' : ''"
                          @click="!activityProps.row.isDescription && onProjectTaskActivityEdit(activityProps.row.id, activityProps.row.project.id, activityProps.row.projectModule.id, activityProps.row.task.name, activityProps.row.project.name, activityProps.row.projectModule.name, true, refreshProjectTaskActivityList)"
                        >
                          <q-tooltip>{{ activityProps.row.isDescription ? 'Please Add Description First' : 'Edit' }}</q-tooltip>
                        </q-icon>
                        <q-icon
                          name="o_timer"
                          class="cursor-pointer q-mr-sm ss"
                          size="xs"
                          :class="activityProps.row.isDescription ? 'text-grey-5 cursor-not-allowed' : ''"
                          @click="!activityProps.row.isDescription && onStartProjectTaskActivityTimer(activityProps.row, startNewTask)"
                        >
                          <q-tooltip>{{ activityProps.row.isDescription ? 'Please Add Description First' : 'Start Task Timer' }}</q-tooltip>
                        </q-icon>
                        <a
                          style="position: relative;"
                          class="q-icon notranslate cursor-pointer q-ml-sm q-mr-md"
                          :class="activityProps.row.isDescription ? 'text-grey-5 cursor-not-allowed' : ''"
                          @click="!activityProps.row.isDescription && onNoteAdd(activityProps.row.id, 'project Activities', activityProps.row.project.id, activityProps.row.project.name, activityProps.row.name, refreshProjectTaskActivityList)"
                        >
                          <q-tooltip anchor="bottom middle" self="top middle">{{ activityProps.row.isDescription ? 'Please Add Description First' : 'Note' }}</q-tooltip>
                          <q-icon name="o_assignment" />
                          <q-badge
                            v-if="activityProps.row.activitiesCount > 0"
                            style="position: absolute; right: -16px; top: -15px;"
                            color="green"
                            text-color="white"
                            :label="activityProps.row.activitiesCount"
                          />
                        </a>
                        <q-icon
                          name="o_delete_outline"
                          class="cursor-pointer q-mr-sm hidden"
                          color="negative"
                          size="xs"
                          @click="onSubmitProjectTaskActivityDelete(activityProps.row.id, activityProps.row.name, activityProps.row.project.name, refreshProjectTaskActivityList)"
                        >
                          <q-tooltip>Delete</q-tooltip>
                        </q-icon>
                        <q-icon
                          :name="activityProps.row.active ? 'o_block' : 'o_check_circle_outline'"
                          :color="activityProps.row.active ? 'negative' : 'positive'" class="cursor-pointer"
                          :class="activityProps.row.isDescription ? 'text-grey-5 cursor-not-allowed' : ''"
                          @click="!activityProps.row.isDescription && onSubmitProjectTaskActivityStatus(activityProps.row, refreshProjectTaskActivityList)"
                        >
                          <q-tooltip>
                            {{
                              activityProps.row.isDescription
                                ? 'Please Add Description First'
                                : (activityProps.row.active ? 'Make Inactive' : 'Set Active')
                            }}
                          </q-tooltip>
                        </q-icon>
                      </q-td>
                      <q-dialog v-model="showPopup[activityProps.row.id]" class="customDialog" persistent full-height position="right" @hide="onDialogHide">
                        <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 100vw !important;">
                          <q-card-section class="card-header with-tools bg-primary stickyHeader">
                            <div class="text-h2 text-white">Weekly Project Plan</div>
                            <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
                          </q-card-section>
                          <q-separator />
                          <q-card-section v-if="selectedDate">
                            <weeklyProjectPlan
                              :project-id="activityProps.row.project.id"
                              :week-end-date="selectedDate"
                            />
                          </q-card-section>
                        </q-card>
                      </q-dialog>
                    </q-tr>
                  </template>
                  <template #bottom-row>
                    <q-tr class="bg-grey-2 text-black">
                      <q-td colspan="7" class="text-right text-weight-bold">
                        Total Hours:
                      </q-td>

                      <q-td class="text-right text-weight-bold">
                        {{
                          paginatedTotalHours(
                            props.row.activities,
                            projectActivityPagination[props.row.project.id]
                          )
                        }}
                      </q-td>
                      <q-td />
                    </q-tr>
                  </template>
                </q-table>
              </div>
            </q-td>
          </q-tr>
        </template>
        <template #bottom-row>
          <q-tr class="bg-grey-3 text-black">
            <q-td colspan="100%">
              <div class="row items-center">
                <div
                  class="col-xxl-10 col-lg-10 col-md-9 col-sm-10 col-xs-10
                 text-right text-weight-bold"
                >
                  Total Hours:
                </div>
                <div
                  class="col-xxl-1 col-lg-1 col-md-2 col-sm-1 col-xs-1
                 q-pr-lg text-right text-weight-bold"
                >
                  {{ outerTotalHours || 0 }}
                </div>
                <div class="col-xxl-1 col-lg-1 col-md-1 col-sm-1 col-xs-1 q-pl-lg" />
              </div>
            </q-td>
          </q-tr>
        </template>
      </q-table>
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
import { ref, onMounted, computed, watch, inject, onBeforeUnmount } from "vue";
import { useQuasar, useDialogPluginComponent } from "quasar";
import { useRoute } from "vue-router";
import { useAuthStore } from "stores/auth";
import { notifySuccess, notifyError, notifyWarning } from "assets/utils";

import useFilters from "composables/useFilters";

import projectActivitiesService from "modules/project-tasks-activities/projectTasksActivities.service";
import editDailyplanner from "modules/my-daily-planner/components/addEdit.vue";
import editTimesheet from "modules/timesheet/components/addEdit.vue";
import weeklyProjectPlan from "modules/project-center/components/_weeklyPlannerTab.vue";
import selectMultiTaskActivity from "modules/project-tasks-activities/components/_multiTaskActivityQuickActions.vue";

// SOP Change :- Shared Dropdowns
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import projectTasksActivities from "src/modules/project-tasks-activities/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";
import useMultiSort from "composables/dataTable/useMultiSort.js";

// Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";
import multiColumnSortingDialog from "src/components/dataTable/_multiColumnSortingDialog.vue";

// Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView
} from "src/modules/project/utils/dialogs.js";

// Shared Project Module Dialogs
import {
  initProjectModuleDialogs,
  onProjectModuleView
} from "src/modules/project-modules/utils/dialogs.js";

// Shared Project Task Dialogs
import {
  initProjectTaskDialogs,
  onProjectTaskView
} from "src/modules/project-tasks/utils/dialogs.js";

// Shared Project Task Activity
import {
  initProjectTaskActivityDialogs,
  onProjectTaskActivityView,
  onProjectTaskActivityEdit
} from "src/modules/project-tasks-activities/utils/dialogs.js";

// Shared Notes Dialogs
import {
  initCommonDialogs,
  onNoteAdd
} from "src/modules/common/utils/dialogs.js";

// Shared Project Task Activity Actions
import {
  initProjectTaskActivityActions,
  onSubmitProjectTaskActivityDelete,
  onSubmitProjectTaskActivityStatus,
  onStartProjectTaskActivityTimer
} from "src/modules/project-tasks-activities/utils/actions.js";

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Common variables
// --------------------------------------------------------------------------------------------------------------------------------------------------

const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const showFilter = ref(false);
const searchLoader = ref(false);
const route = useRoute();
const routeName = route.params.type;
const authStore = useAuthStore();
const user = authStore.user;
const startNewTask = inject("startNewTask");
const { toDate } = useFilters();
const showPopup = ref({});
const selectedDate = ref("");
const { onDialogHide } = useDialogPluginComponent();
const showMultiSelectOptions = ref(false);
const selectedField = ref(null);
const showSortDialog = ref(false);
const activeScope = ref(null);
const processing = ref(false);

function openPlan (d, rowId) {
  selectedDate.value = d.text || "";
  showPopup.value[rowId] = true;
}

const siteId = computed(() => authStore.user?.siteId);

const defaultSearch = {
  searchText: "",
  projectIds: [],
  projectModuleIds: [],
  assignedToIds: user?.employeeId ? [user.employeeId] : [],
  activityNameIds: [],
  activityStatusIds: [],
  statusIds: [],
  activeStatus: "Active",
  sprintWeekEndDate: ""
};

const defaultPagination = {
  sortBy: "project.name",
  descending: false,
  rowsPerPage: 20,
  page: 1
};

const {
  search,
  pagination,
  activeRowId,
  sorts,
  saveDataTableState,
  getTableState
} = useSiteTableState({
  storageKey: "my-Task-And-Activities-Index",
  siteId,
  defaultSearch,
  defaultPagination,
  defaultSorts: {}
});

const tableState = getTableState();
const lsSorts = sorts.value || null;
const expandedRows = ref(
  Array.isArray(tableState?.expandedRows)
    ? [...tableState.expandedRows]
    : []
);

const projectActivityPagination = ref(
  typeof tableState?.rowPagination === "object" &&
  !Array.isArray(tableState?.rowPagination)
    ? { ...tableState.rowPagination }
    : {}
);

const highlightedId = computed(() => activeRowId.value);

function setActiveRowIdInLocalStorage(id) {
  saveDataTableState({
    activeRowId: id
  });
}
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Columns
// ----------------------------------------------------------------------------------------------------------------
const loading = ref(true);
const rows = ref([]);
const columns = ref([
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true, hidden: true },
  { name: "projectModule.name", label: "Project Module", field: "projectModule.name", align: "left", sortable: true },
  { name: "task.name", label: "Task Name", field: "task.name", align: "left", sortable: true },
  { name: "weekDates", label: "Week", field: row => row.weekDates.join(", "), align: "center", sortable: false },
  { name: "task.status.dropDownValue", label: "Task Status", field: "task.status.dropDownValue", align: "left", sortable: true, style: "display: none", headerStyle: "display: none" },
  { name: "name", label: "Activity Name", field: "name", align: "left", sortable: true },
  { name: "assignedTo.person.firstname", label: "Activity Owner", field: "assignedTo.person.firstname", align: "left", sortable: true },
  { name: "activityStatus.dropDownValue", label: "Activity Status", field: "activityStatus.dropDownValue", align: "left", sortable: true },
  { name: "estimateHours", label: "Est. Hrs", field: "estimateHours", align: "right", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- Get All Project Tasks Activities
// ----------------------------------------------------------------------------------------------------------------

const getProjectActivities = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  if (search.value.sprintWeekEndDate === "" || search.value.sprintWeekEndDate === null || search.value.sprintWeekEndDate === undefined) {
    search.value.sprintWeekEndDate = null;
  } else {
    search.value.sprintWeekEndDate = toDate(search.value.sprintWeekEndDate);
  }
  const validSorts = multiSort.value.filter(s => s.column && s.direction);
  // convert multi-sort array into the key-value object in dictionary
  const sortsObj = Object.fromEntries(validSorts.map(s => [s.column, s.direction]));
  const payload = { page, pageSize: rowsPerPage, sortBy, sorts: sortsObj, descending, ...search.value };

  const storedActivityIds = localStorage.getItem("selectedActivityIds");
  ActivityIds.value = storedActivityIds ? storedActivityIds.split(",") : [];
  projectActivitiesService.getAllProjectActivitiesForExpandCollapse(payload).then((resp) => {
    rows.value = resp.data;
    rows.value = resp.data.map(project => {
      return {
        ...project,
        activities: project.activities.map((activity, idx, arr) => {
          const prevActivity = arr[idx - 1];
          return {
            ...activity,
            description: activity.description || "",
            checkboxStatus: ActivityIds.value.includes(activity.id),
            weekDates: activity.task?.projectWeeklyPlanDatesReqTaskIssueMappingList
              ? activity.task.projectWeeklyPlanDatesReqTaskIssueMappingList.map(m => ({
                value: String(m.projectWeeklyPlanDates?.id),
                text: toDate(m.projectWeeklyPlanDates?.weekDate) || null
              }))
              : [],
            showModuleName:
          idx === 0 ||
          !prevActivity ||
          prevActivity.projectModule.id !== activity.projectModule.id,

            showTaskNameAndIcons:
          idx === 0 ||
          !prevActivity ||
          prevActivity.projectModule.id !== activity.projectModule.id ||
          prevActivity.task.id !== activity.task.id
          };
        })
      };
    });
    Object.assign(pagination.value, {
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    });
    saveDataTableState({
      search: search.value,

      pagination: {
        ...pagination.value,
        page,
        rowsPerPage,
        sortBy,
        descending,
        rowsNumber: resp.total
      },

      activeRowId: activeRowId.value,
      sorts: sortsObj
    });
  }).finally(() => {
    loading.value = false;
    searchLoader.value = false;
  });
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------

initProjectDialogs(activeRowId);
initProjectModuleDialogs(activeRowId);
initProjectTaskDialogs(activeRowId);
initProjectTaskActivityDialogs(activeRowId);
initCommonDialogs(activeRowId);

initProjectTaskActivityActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// ----------------------------------------------------------------------------------------------------------------

// Search records as per parameters
const onSearch = () => {
  refreshProjectTaskActivityList();
  ActivityIds.value = [];
};

// Clear search
const onAdvanceClear = () => {
  search.value.activeStatus = defaultSearch.activeStatus;
  search.value.projectIds = [];
  search.value.projectModuleIds = [];
  search.value.activityNameIds = [];
  search.value.activityStatusIds = [];
  search.value.statusIds = [];
  search.value.assignedToIds = user?.employeeId ? [user.employeeId] : [];
  search.value.sprintWeekEndDate = "";
  saveDataTableState({
    search: {
      ...defaultSearch
    },

    pagination: {
      ...defaultPagination
    },

    activeRowId: null,
    sorts: {}
  });
  onSearch();
};

const refreshProjectTaskActivityList = () => {
  getProjectActivities({ pagination: pagination.value });
};

// Only allow Sundays and today/future
const isSunday = (dateStr) => {
  const day = new Date(dateStr);
  const todayStart = new Date();
  todayStart.setHours(0, 0, 0, 0);

  return (
    day.getDay() === 0 // && day >= todayStart
  );
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { projectNameDropdown } = projectModule();
const {
  // projectTaskActivityActiveInActiveDropdown,
  projectTaskActivityNameDropdown,
  projectTaskActivityStatusDropdown
} = projectTasksActivities();
const { projectModulesByProjectIdForDropdown } = projectModuleOfProjectModule();
const { activeEmployeesDropdown } = employeeModule();
const { projectTaskStatusForDropdown } = projectTaskModule();

//  --------------------------------------------------------------------------------------------------------------------------------------------------
// Dropdowns
// --------------------------------------------------------------------------------------------------------------------------------------------------

// Active/Inactive statusList
const statusList = ref(["Active", "Inactive"]);

// // Get all activity status list
// const activityStatusList = ref([]);
// // const activityStatusListForFilter = ref([]);
// const activityStatusFilter = ref([]);
// function getAllActivityStatusListForDropDown (typeName) {
//   commonService.getDropDown(typeName).then((resp) => {
//     const responseData = resp
//       .map((item) => ({ text: item.dropdownValue, value: item.id }))
//       .sort((a, b) => a.text.localeCompare(b.text));
//     // activityStatusListForFilter.value = responseData;
//     activityStatusFilter.value = responseData;

//     // disable only "Close"
//     // activityStatusList.value = responseData
//     //   .map(item => ({
//     //     ...item,
//     //     disable: item.text.toLowerCase() === "close"
//     //   }));
//     activityStatusList.value = responseData
//       .filter(item => item.text.toLowerCase() !== "close"
//       );
//   });
// }

// Added colors for task status dropdown list
function getStatusColor (statusText) {
  if (statusText) {
    switch (statusText) {
    case "Open":
      return "purple-4";
    case "Close":
      return "grey-4";
    case "Completed":
      return "green-4";
    case "In Development":
      return "yellow-4";
    case "In QA":
      return "cyan-4";
    case "In UAT":
      return "deep-orange-4";
    case "New":
      return "blue-4";
    case "On Hold":
      return "brown-4";
    case "Test Site":
      return "blue-grey-4";
    case "UAT passed":
      return "green-9";
    default:
      return "#ffffff";
    }
  }
}

// Returns count of selected activities for a given project
const getSelectedActivitiesCount = (projectId) => {
  return ActivityIds.value.filter(id => {
    const allActivities = rows.value.flatMap(r => r.activities || []);
    const activity = allActivities.find(a => a.id === id);
    return activity?.projectId === projectId;
  }).length;
};

// const isExpanded = (projectId) => expandedRows.value.includes(projectId);
const expandedRowsMap = computed(() => {
  return new Set(expandedRows.value);
});

const toggleExpand = (projectId) => {
  const rows = [...expandedRows.value];

  const index = rows.indexOf(projectId);

  if (index > -1) {
    rows.splice(index, 1);
  } else {
    rows.push(projectId);
  }

  expandedRows.value = rows;

  // saveState({
  //   expandedRows: rows
  // });
};

const isExpanded = (projectId) => {
  return expandedRowsMap.value.has(projectId);
};

// // const startTaskActivityTimer = (activity) => {
// //   const timer = ref({
// //     taskId: activity.task.id,
// //     taskName: activity.task.name,
// //     activityId: activity.id,
// //     activityName: activity.name
// //   });

// //   // Start timer
// //   startNewTask(timer.value);
// // };

const handleDocumentClick = (event) => {
  const highlightElement = document.querySelector(".highlight");
  // Check if clicked inside the highlighted row or icons
  if (highlightElement && !highlightElement.contains(event.target)) {
    activeRowId.value = null;
    // const storedData = getLocalStorage(localStorageKey) || {};
    // setLocalStorage(localStorageKey, { ...storedData, activeRowId: null });
    saveDataTableState({
      activeRowId: null
    });
  }
};

const handleDescriptionClick = async (row) => {
  // Cancel only if switching rows
  if (activeScope.value && activeRowId.value !== row.id) {
    activeScope.value.cancel();
  }
  activeRowId.value = row.id;
  row.isLoadingDescription = true;
  // Fetch the description
  try {
    const resp = await projectActivitiesService.getProjectActivityDescriptionById(row.id);
    row.description = resp?.description || "";
  } catch (error) {
    console.error("Failed to fetch activity description");
    row.description = "";
  } finally {
    row.isLoadingDescription = false;
  }
};

// add description action
async function handleDescriptionSave (row, description) {
  row.description = description;
  return await onSaveDescription(row.id, description);
}

const onSaveDescription = async (id, description) => {
  if (!stripHtml(description) && !hasImage(description)) {
    notifyError({ message: "Description is required." });
    return false;
  }
  const payload = { description };
  processing.value = true;
  try {
    await projectActivitiesService.updateDescription(id, payload);
    notifySuccess({ message: "Description is saved successfully." });
    refreshProjectTaskActivityList();
    return true;
  } catch (err) {
    notifyError({ message: "Failed to save description." });
  } finally {
    setTimeout(() => (processing.value = false), 1000);
  }
};

// onChangeActivityStatus
function onChangeActivityStatus (id, activityStatusId) {
  const payload = {
    activityIds: [id],
    activityStatusId
  };
  setTimeout(function () {
    projectActivitiesService.updateTaskActivityStatus(payload).then(resp => {
      notifySuccess({ message: "Activity status is saved successfully." });
      getProjectActivities({ pagination: pagination.value });
    });
  });
}

const ActivityIds = ref([]);
const multiSelectTaskActivityStatusMap = ref({});
const multiSelectTaskActivityActiveMap = ref({});
const onSelectCheckbox = (itemId, isActive, activityStatus, flag) => {
  if (flag === true) {
    // Add the itemId to the ProjectIds array if it's not already present
    if (!ActivityIds.value.includes(itemId)) {
      ActivityIds.value.push(itemId);
      multiSelectTaskActivityStatusMap.value[itemId] = activityStatus;
      multiSelectTaskActivityActiveMap.value[itemId] = isActive;
    }
  } else {
    // Find the index of the itemId in the ProjectIds array and remove it
    const index = ActivityIds.value.indexOf(itemId);
    if (index !== -1) {
      ActivityIds.value.splice(index, 1); // Remove the item at the found index
    }
    delete multiSelectTaskActivityStatusMap.value[itemId];
    delete multiSelectTaskActivityActiveMap.value[itemId];
  }
  localStorage.setItem("selectedActivityIds", ActivityIds.value);
};

const onSendDailyPlan = () => {
  activeRowId.value = ActivityIds.value;
  const selectedActivities = rows.value.flatMap(project => project.activities).filter(activity => ActivityIds.value.includes(activity.id));
  if (!selectedActivities.length) return;

  const hasDisallowedStatus = selectedActivities.some(activity =>
    activity.activityStatus.dropDownValue.toLowerCase() !== "open" || activity.isDescription
  );

  if (hasDisallowedStatus) {
    notifyWarning({ message: "Please add activity details and open the task activity before filling the daily plan." });
    return;
  }

  $q.dialog({
    component: editDailyplanner,
    componentProps: { activityIds: ActivityIds.value, isMyTaskActivity: true }
  }).onOk(() => {
    // Clear ActivityIds after the dialog closes successfully
    ActivityIds.value = [];
    localStorage.removeItem("selectedActivityIds");
    getProjectActivities({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

const onSendTimesheet = () => {
  activeRowId.value = ActivityIds.value;
  const selectedActivities = rows.value.flatMap(project => project.activities).filter(activity => ActivityIds.value.includes(activity.id));
  if (!selectedActivities.length) return;

  const hasDisallowedStatus = selectedActivities.some(activity =>
    activity.activityStatus.dropDownValue.toLowerCase() !== "open" || activity.isDescription
  );

  if (hasDisallowedStatus) {
    notifyWarning({ message: "Please add activity details and open the task activity before filling the timesheet." });
    return;
  }
  $q.dialog({
    component: editTimesheet,
    componentProps: { activityIds: ActivityIds.value, isMyTaskActivity: true }
  }).onOk(() => {
    // Clear ActivityIds after the dialog closes successfully
    ActivityIds.value = [];
    localStorage.removeItem("selectedActivityIds");
    getProjectActivities({ pagination: pagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

const selectedFieldOptions = [
  { label: "Change Status", value: "Status", icon: "o_flag" },
  { label: "Active/Inactive", value: "Active/Inactive Status", icon: "o_toggle_on" }
];

const onSelectMultiOptions = () => {
  activeRowId.value = ActivityIds.value;

  const selectedActivities = rows.value
    .flatMap(project => project.activities)
    .filter(activity => ActivityIds.value.includes(activity.id));

  if (!selectedActivities.length) return;

  const missingDescriptionOrNotOpenCount = selectedActivities.filter(
    activity =>
      activity.isDescription ||
    activity.activityStatus?.dropDownValue?.toLowerCase() !== "open"
  ).length;
  if (selectedField.value === "Status" && missingDescriptionOrNotOpenCount > 0) {
    notifyWarning({
      message: `${missingDescriptionOrNotOpenCount} ${
        missingDescriptionOrNotOpenCount > 1 ? "activities" : "activity"
      } missing description or not in Open status. Please add description and update status to Open.`
    });
    selectedField.value = null;
    return;
  }

  // Open the multi-select dialog
  $q.dialog({
    component: selectMultiTaskActivity,
    componentProps: { activityIds: ActivityIds.value, selectedField: selectedField.value }
  }).onOk(() => {
    setDefaultsForMultiSelects();
    getProjectActivities({ pagination: pagination.value });
  }).onCancel(() => {
    selectedField.value = null;
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

function setDefaultsForMultiSelects () {
  ActivityIds.value = [];
  multiSelectTaskActivityStatusMap.value = [];
  multiSelectTaskActivityActiveMap.value = [];
  selectedField.value = null;
  localStorage.removeItem("selectedActivityIds");
}

function paginatedTotalHours (activities, pagination) {
  if (!activities?.length || !pagination) return "0.00";

  const { page, rowsPerPage } = pagination;

  const start = (page - 1) * rowsPerPage;
  const end = rowsPerPage === 0
    ? activities.length
    : start + rowsPerPage;

  const pageRows = activities.slice(start, end);

  const total = pageRows.reduce(
    (sum, row) => sum + (Number(row.estimateHours) || 0),
    0
  );

  return total.toFixed(2);
}

const outerTotalHours = computed(() => {
  const total = rows.value.reduce((projectSum, project) => {
    const activityTotal = project.activities?.reduce(
      (sum, act) => sum + (Number(act.estimateHours) || 0),
      0
    ) || 0;

    return projectSum + activityTotal;
  }, 0);

  return total.toFixed(2);
});

const stripHtml = (html) => {
  if (!html) return "";
  return html.replace(/<[^>]*>/g, "").replace(/&nbsp;/g, " ").trim();
};

const hasImage = (html) => {
  if (!html) return false;
  return /<img\s+[^>]*src=/i.test(html);
};

// updateTaskStatus
// function onSubmit (id, taskId, statusId) {
//   setTimeout(function () {
//     projectTaskService.updateTaskStatus(id, taskId, statusId).then(resp => {
//       notifySuccess({ message: "Task status is saved successfully." });
//       getProjectActivities({ pagination: pagination.value });
//     });
//   });
// }

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
  // ...mapSingleFilterToLabel(search.value.activeStatus, projectTaskActivityActiveInActiveDropdown.list, "Active/Inactive"),
  ...mapSingleFilterToLabel(search.value.activeStatus, statusList, "Active/Inactive"),
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Project Name"),
  ...mapFilterToLabel(search.value.projectModuleIds, projectModulesByProjectIdForDropdown.list, "Project Module"),
  ...mapFilterToLabel(search.value.assignedToIds, activeEmployeesDropdown.list, "Activity Owner"),
  ...mapFilterToLabel(search.value.activityNameIds, projectTaskActivityNameDropdown.list, "Activity Name"),
  ...mapFilterToLabel(search.value.activityStatusIds, projectTaskActivityStatusDropdown.list, "Activity Status"),
  ...mapFilterToLabel(search.value.statusIds, projectTaskStatusForDropdown.list, "Task Status"),
  ...(search.value.sprintWeekEndDate ? { "Sprint Week End Date": search.value.sprintWeekEndDate } : {})
}));

function onClearFilters (key) {
  if (key === "Active/Inactive") {
    search.value.activeStatus = null;
  } else if (key === "Project Name") {
    search.value.projectIds = [];
  } else if (key === "Project Module") {
    search.value.projectModuleIds = [];
  } else if (key === "Activity Owner") {
    search.value.assignedToIds = [];
  } else if (key === "Activity Name") {
    search.value.activityNameIds = [];
  } else if (key === "Activity Status") {
    search.value.activityStatusIds = [];
  } else if (key === "Task Status") {
    search.value.statusIds = [];
  } else if (key === "Sprint Week End Date") {
    search.value.sprintWeekEndDate = "";
  }
  delete appliedFilters.value[key];
  getProjectActivities({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Project Module": return search.value.projectModuleIds?.length || 0;
  case "Activity Owner": return search.value.assignedToIds?.length || 0;
  case "Activity Name": return search.value.activityNameIds?.length || 0;
  case "Activity Status": return search.value.activityStatusIds?.length || 0;
  case "Task Status": return search.value.statusIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

// if the status is Close then disable dropdown
const isClose = computed(() => {
  return (projectTaskStatusForDropdown.list.value.find(item => item.value === rows.value.statusId)?.text === "Close") || false;
});

const statusOptions = (row) => {
  return projectTaskActivityStatusDropdown.list.value.map(option => {
    if (option.text === "Open" && row.isDescription) {
      return { ...option, disable: true };
    }
    if (option.text === "New" && row.activityStatus.dropDownValue === "Open") {
      return { ...option, disable: true };
    }
    return { ...option, disable: false };
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Sort Filter:- Search and Clear
// --------------------------------------------------------------------------------------------------------------------------------------------------

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
    refreshProjectTaskActivityList();
  }
});

// ----------------------------
// Save static search into localstorage.
// ----------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  refreshProjectTaskActivityList();
});

watch(() => search.value.projectIds, async (newValue, oldValue) => {
  if (search.value?.projectIds?.length === 0 || newValue === oldValue) return;

  search.value.projectModuleIds = [];
  await projectModulesByProjectIdForDropdown.load(false, false, search.value.projectIds);
}, { immediate: true });

watch(selectedField, (newVal) => {
  if (newVal) {
    showMultiSelectOptions.value = false;
    onSelectMultiOptions(); // This opens the dialog for the selected action
  }
});

watch(ActivityIds, () => {
  if (ActivityIds.value.length === 0) {
    showMultiSelectOptions.value = false;
  }
}, { deep: true });

watch(
  projectActivityPagination,
  (val) => {
    saveDataTableState({
      rowPagination: JSON.parse(JSON.stringify(val))
    });
  },
  { deep: true }
);

onBeforeUnmount(() => {
  document.removeEventListener("click", handleDocumentClick);
});

// ----------------------------------------------------------------------------------------------------------------
// Expanded Rows Save
// ----------------------------------------------------------------------------------------------------------------
watch(
  expandedRows,
  (val) => {
    saveDataTableState({
      expandedRows: [...val]
    });
  },
  { deep: true }
);
// ----------------------------------------------------------------------------------------------------------------
// Expanded Rows Load
// ----------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  projectNameDropdown.load();
  if (search.value.projectIds?.length > 0) projectModulesByProjectIdForDropdown.load(false, false, search.value.projectIds);

  activeEmployeesDropdown.load(user.siteId);
  projectTaskActivityNameDropdown.load("Project Activities");
  // projectTaskActivityStatusDropdown.load("Activity Status");
  projectTaskStatusForDropdown.load("Task Status");

  // Get Active/InActive and Set default to Active
  // await projectTaskActivityActiveInActiveDropdown.load("Project Active Status");
  // const activeValue = await projectTaskActivityActiveInActiveDropdown.getValueByLabel("Active");

  if (search.value.assignedToIds?.length === 0) search.value.assignedToIds = [user.employeeId];

  await projectTaskActivityStatusDropdown.load("Activity Status");

  const activityStatus = projectTaskActivityStatusDropdown.getValuesByLabels(["New", "Open", "On Hold"]);
  if (activityStatus.length && !search.value.activityStatusIds?.length) {
    search.value.activityStatusIds = activityStatus;
  }

  localStorage.removeItem("selectedActivityIds");

  document.addEventListener("click", handleDocumentClick);
  refreshProjectTaskActivityList();
});

</script>
<style>
.bg-light-red {
  background-color: #FDECEA !important; /* Bootstrap-like light red */
}
.edges-bordered-table {
  border: 1px solid #ccc;
}
.my-sticky-header-table thead tr {
  z-index: 5;
}

.q-table__expanded-row .q-table thead tr {
  z-index: 1;
}
</style>
