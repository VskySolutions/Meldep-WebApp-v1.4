<!-- eslint-disable vue/no-v-html -->
<template>
  <q-page padding>
    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <div class="row items-center">
          <div class="col-xxl-4 col-xl-4 col-lg-4 col-md-3 col-sm-6 col-xs-12">
            <q-breadcrumbs class="text-brown text-weight-bold text-h3">
              <template #separator>
                <q-icon size="1.5em" name="o_chevron_right" color="primary" />
              </template>
              <q-breadcrumbs-el label="Project Management" />
              <q-breadcrumbs-el icon="o_calendar_view_week" label="Weekly Target Plan" />
            </q-breadcrumbs>
          </div>
          <div class="col-xxl-4 col-xl-4 col-lg-4 col-md-4 col-sm-6 col-xs-12">
            <div class="row items-center">
              <span v-if="Object.keys(appliedFilters).length > 0" class="text-grey-10 text-caption" style="font-weight: 600;">Filters On :</span>
              <q-chip v-for="(value, key) in appliedFilters" :key="key" class="bg-grey-3 text-grey-10 text-caption q-mr-xs filter-chip">
                <q-badge v-if="getFilterCount(key) > 0" color="grey-7" floating>{{ getFilterCount(key) }}</q-badge>
                {{ key }}
                <q-icon name="o_info" class="q-ml-xs" /> <q-icon name="o_clear" class="q-ml-xs" @click="onClearFilters(key)" />
                <q-tooltip>{{ value }}</q-tooltip>
              </q-chip>
            </div>
          </div>
          <div class="col-xxl-4 col-xl-4 col-lg-4 col-md-5 col-sm-6 col-xs-12">
            <div class="row items-center justify-end no-wrap">
              <div class="search-container position-relative">
                <!-- SOP Change -->
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
                      label="Name"
                      :options="projectNameDropdown.list.value"
                      :filter="projectNameDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.projectCategoryIds"
                      label="Category"
                      :options="projectCategoriesDropdown.list.value"
                      :filter="projectCategoriesDropdown.filter"
                      :isShowAll="true"
                    />
                    <multiSelectDropdown
                      v-model="search.projectCoordinatorIds"
                      label="Coordinator"
                      :options="activeEmployeesDropdown.list.value"
                      :filter="activeEmployeesDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.projectLeadsIds"
                      label="Leads"
                      :options="activeEmployeesDropdown.list.value"
                      :filter="activeEmployeesDropdown.filter"
                    />
                    <multiSelectDropdown
                      v-model="search.projectStatusIds"
                      label="Status"
                      :options="projectStatusDropdown.list.value"
                      :filter="projectStatusDropdown.filter"
                      :isShowAll="true"
                    />
                    <singleSelectDropdown
                      v-model="search.statusId"
                      label="Active/Inactive"
                      :options="projectActiveInActiveDropdown.list.value"
                    />
                    <multiSelectDropdown
                      v-model="search.projectPriorityIds"
                      label="Priority"
                      :options="projectPrioritiesDropdown.list.value"
                      :filter="projectPrioritiesDropdown.filter"
                      :isShowAll="true"
                    />
                    <multiSelectDropdown
                      v-model="search.projectTypeIds"
                      label="Type"
                      :options="projectTypesDropdown.list.value"
                      :filter="projectTypesDropdown.filter"
                      :isShowAll="true"
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
              <div class="q-ml-sm">
                <q-btn
                  v-if="isFullAccess"
                  icon="o_add"
                  outline
                  label="Add Weekly Plan"
                  no-caps
                  class="text-primary btnRounded"
                  @click="onAddWeeklyPlan(activeProjectPlanId, activeProjectId)"
                />
                <q-btn
                  v-if="showBackButton"
                  icon="o_chevron_left"
                  outline
                  label="Back"
                  no-caps
                  class="text-primary btnRounded no-space-between q-ml-sm"
                  @click="$router.back()"
                />
              </div>
            </div>
          </div>
        </div>
        <div class="row q-mt-md">
          <div class="col-xxl-2 col-lg-2 col-md-3 col-sm-4 col-xs-12">
            <q-table
              ref="tableRef"
              v-model:pagination="pagination"
              :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
              :loading="loading"
              :rows="rows"
              :columns="columns"
              row-key="id"
              separator="cell"
              no-data-label="No data available"
              binary-state-sort
              :rows-per-page-options="[15,30,50,100]"
              @request="getProjects"
            >
              <template #loading>
                <q-inner-loading showing color="primary">
                  <q-spinner-ios size="40px" class="q-mt-xl" />
                </q-inner-loading>
              </template>
              <template #header="props">
                <q-tr :props="props" class="bg-primary text-white">
                  <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                  <q-th auto-width class="text-center">Actions</q-th>
                </q-tr>
              </template>
              <template #body="props">
                <q-tr :props="props" :class="activeProjectPlanId == props.row.id ? 'highlight' : ''">
                  <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">{{ props.row.project.name }}</q-td>
                  <q-td class="text-center actions">
                    <q-btn
                      dense
                      size="1em"
                      icon="o_north_east"
                      class="no-padding q-mr-xs remove-q-btn-box-shadow"
                      @click="LoadProjectPlan(props.row)"
                    >
                      <q-badge color="black" floating>
                        {{ props.row.projectWeeklyPlanDates ? props.row.projectWeeklyPlanDates.length : 0 }}
                      </q-badge>
                      <q-tooltip>View Plan</q-tooltip>
                    </q-btn>
                    <q-btn dense flat icon="o_more_vert" color="primary">
                      <q-tooltip>More Options</q-tooltip>
                      <q-menu auto-close>
                        <q-list style="min-width: 180px">
                          <q-item v-ripple clickable @click="onProjectView(props.row.projectId)">
                            <q-item-section avatar><q-icon name="o_visibility" size="xs" /></q-item-section>
                            <q-item-section>View</q-item-section>
                          </q-item>
                          <q-item v-if="isFullAccess" v-ripple clickable @click="$router.push({ path: '/project-planning/workboard', state: {projectId: props.row.projectId } })">
                            <q-item-section avatar><q-icon name="o_developer_board" size="xs" /></q-item-section>
                            <q-item-section>Work Board</q-item-section>
                          </q-item>
                          <q-item v-ripple clickable @click="$router.push({ path: '/project-center', state: { projectId: props.row.projectId } })">
                            <q-item-section avatar><q-icon name="o_dashboard" size="xs" /></q-item-section>
                            <q-item-section>Center</q-item-section>
                          </q-item>
                          <q-item v-if="props.row.isEditable" v-ripple clickable @click="$router.push({ path: `/all-project-planner`, state: {projectId: props.row.id } })">
                            <q-item-section avatar><q-icon name="o_task" size="xs" /></q-item-section>
                            <q-item-section>Planner</q-item-section>
                          </q-item>
                          <q-item v-ripple clickable @click="$router.push({ path: '/project-targetplan/monthlyplanner', state: {projectId: props.row.projectId } })">
                            <q-item-section avatar><q-icon name="o_calendar_view_month" size="xs" /></q-item-section>
                            <q-item-section>Monthly Planner</q-item-section>
                          </q-item>
                          <q-item v-ripple clickable @click="onProjectFilesView(props.row.projectId, props.row.name)">
                            <q-item-section avatar><q-icon name="o_description" size="xs" /></q-item-section>
                            <q-item-section>Files</q-item-section>
                          </q-item>
                          <q-item v-if="isChatAccess" v-ripple clickable @click="onAddNotesToProject(props.row.projectId, 'Project Weekly Plan', props.row.projectId, props.row.name, props.row.name)">
                            <q-item-section avatar style="position: relative;">
                              <q-icon name="o_note" size="xs" />
                              <q-badge
                                v-if="props.row.project.projectNotesCount > 0"
                                style="position: absolute; right: 5px; top: -2px;"
                                color="green"
                                text-color="white"
                                :label="props.row.project.projectNotesCount"
                              />
                            </q-item-section>
                            <q-item-section>Note</q-item-section>
                          </q-item>
                          <q-item v-if="isChatAccess" v-ripple clickable @click="onProjectMessage(props.row.projectId)">
                            <q-item-section avatar style="position: relative;">
                              <q-icon name="o_message" size="xs" />
                              <q-badge
                                v-if="props.row.project.projectMessageCount > 0"
                                style="position: absolute; right: 5px; top: -2px;"
                                color="green"
                                text-color="white"
                                :label="props.row.project.projectMessageCount"
                              />
                            </q-item-section>
                            <q-item-section>Message</q-item-section>
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
          <div class="col-xxl-10 col-lg-10 col-md-9 col-sm-8 col-xs-12 q-px-sm">
            <div class="relative-position">
              <table class="WhiteTable full-width">
                <!-- <q-inner-loading :showing="projectPlanLoading">
                  <q-spinner-ios size="50px" color="grey"/>
                </q-inner-loading> -->
                <thead style="position: sticky; top: 55px; z-index: 9;">
                  <tr>
                    <th colspan="4">
                      <div class="flex items-center justify-between position-relative">
                        <div class="flex items-center">
                          <div class="q-mr-xs">
                            <h2>{{ activeProjectName ? activeProjectName : "No Project Selected" }}</h2>
                          </div>
                          <div v-if="activeProjectCharterGroupBy?.length > 0" class="q-mr-xs">
                            <q-icon name="o_groups" color="white" size="sm">
                              <q-tooltip anchor="bottom middle" self="top middle" class="bg-grey-8 text-white shadow-2">
                                <table class="table boarded GreyTable">
                                  <thead>
                                    <tr>
                                      <td colspan="2" class="text-center" style="font-size: 13px;">Project Charter</td>
                                    </tr>
                                    <tr>
                                      <th class="text-start">Role</th>
                                      <th class="text-start">Team Members</th>
                                    </tr>
                                  </thead>
                                  <tbody>
                                    <tr v-for="mapping in activeProjectCharterGroupBy" :key="mapping.groupId">
                                      <td>{{ mapping?.groupValue }}</td>
                                      <td>
                                        {{ mapping?.employeeMappingList?.map(p => `${p.employee?.person?.firstName} ${p.employee?.person?.lastName}`).join(', ') }}
                                      </td>
                                    </tr>
                                  </tbody>
                                </table>
                              </q-tooltip>
                            </q-icon>
                          </div>
                          <div v-if="(isFullAccess || isViewAccess || isChatAccess)" class="q-mr-xs">
                            <q-icon name="o_admin_panel_settings" color="white" size="sm" />
                            <q-tooltip anchor="bottom middle" self="top middle" class="bg-grey-8 text-white shadow-2">
                              <table class="table boarded GreyTable">
                                <thead>
                                  <tr>
                                    <td colspan="2" class="text-center" style="font-size: 13px;">Project Access</td>
                                  </tr>
                                  <tr>
                                    <th class="text-start">Permission</th>
                                    <th class="text-start">Access</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  <tr>
                                    <td>Full Access</td>
                                    <td class="text-center"><q-icon :name="isFullAccess ? 'o_check' : 'o_cancel'" :color="isFullAccess ? 'white' : 'red'" /></td>
                                  </tr>
                                  <tr>
                                    <td>View Only</td>
                                    <td class="text-center"><q-icon :name="isViewAccess ? 'o_check' : 'o_cancel'" :color="isViewAccess ? 'white' : 'red'" /></td>
                                  </tr>
                                  <tr>
                                    <td>Chat</td>
                                    <td class="text-center"><q-icon :name="isChatAccess ? 'o_check' : 'o_cancel'" :color="isChatAccess ? 'white' : 'red'" /></td>
                                  </tr>
                                </tbody>
                              </table>
                            </q-tooltip>
                          </div>
                        </div>
                        <div class="flex">
                          <div v-if="activeProjectPlanApprover.id" class="AddProjectPlanApprover q-mr-xs">
                            <q-icon name="o_how_to_reg" size="sm">
                              <q-tooltip anchor="bottom middle" self="top middle">
                                {{ "Plan Approver - " + activeProjectPlanApprover.person.firstName + " " + activeProjectPlanApprover.person.lastName }}
                              </q-tooltip>
                            </q-icon>
                          </div>
                          <div v-else class="AddProjectPlanApprover q-mr-xs cursor-pointer">
                            <q-icon name="o_person_add" size="sm" @click="showPlanApproverDialog = true; selectedPlanApproverId = null">
                              <q-tooltip anchor="bottom middle" self="top middle">
                                Click Here To Add Plan Approver?
                              </q-tooltip>
                            </q-icon>
                          </div>
                          <div class="">
                            <q-btn
                              v-if="activeProjectName !== 'No Project Selected' && isFullAccess"
                              :icon="showCalendar ? 'o_close' : 'o_add'"
                              class="customBTN"
                              size="sm"
                              @click="showCalendar = !showCalendar"
                            >
                              {{ showCalendar ? 'Cancel' : "Add Weekend" }}
                            </q-btn>
                            <div v-if="showCalendar" class="q-mt-md text-black" style="position: absolute;right: 8px;top: 18px;z-index: 9;">
                              <q-date
                                v-model="selectedDate"
                                :options="isSunday"
                                mask="MM/DD/YYYY"
                                :navigation-min-year-month="minMonth"
                                minimal
                                :first-day-of-week="1"
                                @update:model-value="onDateSelected"
                              />
                            </div>
                          </div>
                        </div>
                      </div>
                    </th>
                  </tr>
                  <tr class="text-center">
                    <th style="font-size: 15px;">Weekend</th>
                    <th width="43%">Planned</th>
                    <th width="43%">Actual</th>
                    <th width="50px">Action</th>
                  </tr>
                </thead>
                <tbody v-if="projectPlanLoading">
                  <tr>
                    <td colspan="100%">
                      <div class="flex justify-center q-py-md">
                        <q-spinner-ios size="40px" color="grey" />
                      </div>
                    </td>
                  </tr>
                </tbody>
                <tbody
                  v-for="planDate in projectWeeklyPlanDates"
                  v-else
                  :key="planDate.id"
                  class="weeklyDate text-grey-9"
                  @mouseenter="!planDate.isApproved ? projectWeeklyPlanDateLineHovered = planDate.id : null"
                  @mouseleave="!planDate.isApproved ? projectWeeklyPlanDateLineHovered = null : null"
                >
                  <tr
                    v-for="(line, lineIndex) in planDate.projectWeeklyPlanDatesLines"
                    :key="line.id"
                  >
                    <td
                      v-if="lineIndex === 0"
                      :rowspan="planDate.projectWeeklyPlanDatesLines.length"
                      class="weekDetails text-center"
                    >
                      {{ planDate.weekDate }}
                      <hr>
                      <div class="weekTotal">
                        <h6 class="flex items-center justify-center">
                          Total:-
                          <b style="color: #1b75ab;">
                            {{ getTotalHoursForWeekSummary(planDate.employeeEstimateHoursForWeekSummaryList) }}
                          </b>
                          <div v-if="(planDate.employeeEstimateHoursForWeekSummaryList?.length > 0 ? true : false )" class="resourceIcon position-relative">
                            <q-icon name="o_groups" size="sm" color="primary" class="q-ml-xs" />
                            <q-tooltip class="bg-grey-8 text-white shadow-2">
                              <table class="table boarded GreyTable">
                                <thead>
                                  <tr>
                                    <td colspan="2" class="text-center" style="font-size: 13px;">Resource Summary</td>
                                  </tr>
                                  <tr>
                                    <th class="text-start">Resource Name</th>
                                    <th class="text-end">Est. Hours</th>
                                  </tr>
                                </thead>
                                <tbody>
                                  <tr v-for="group in planDate?.employeeEstimateHoursForWeekSummaryList" :key="group.employeeId">
                                    <td>{{ group.employee?.person.firstName + " " + group.employee?.person.lastName }}</td>
                                    <td class="text-end">
                                      {{ group.totalEstimatedHours }}
                                    </td>
                                  </tr>
                                  <tr v-if="planDate?.employeeEstimateHoursForWeekSummaryList?.length > 0">
                                    <td class="text-end">Total Hours</td>
                                    <td class="text-end">{{ getTotalHoursForWeekSummary(planDate.employeeEstimateHoursForWeekSummaryList) }}</td>
                                  </tr>
                                </tbody>
                              </table>
                            </q-tooltip>
                            <span class="text-primary">{{ planDate.employeeEstimateHoursForWeekSummaryList.length }}</span>
                          </div>
                        </h6>
                      </div>
                      <hr>
                      <div class="planDateActions">
                        <q-btn
                          v-if="isFullAccess && !planDate.isApproved"
                          :disable="!(projectWeeklyPlanDateLineHovered === planDate.id)"
                          icon="o_add"
                          color="grey-7"
                          size="sm"
                          class="q-mr-xs q-mb-xs"
                          @click="addWeeklyPlanDateLines(planDate)"
                        >
                          <q-tooltip>Add Line</q-tooltip>
                        </q-btn>
                        <q-btn
                          v-if="isFullAccess && showApprovePlanAction && !planDate.isApproved"
                          icon="o_lock"
                          color="primary"
                          size="sm"
                          class="q-mr-xs q-mb-xs"
                          @click="approveThisWeeklyPlan(planDate.id, true)"
                        >
                          <q-tooltip>Click Here To Lock This Plan</q-tooltip>
                        </q-btn>
                        <q-btn
                          v-if="!planDate.isApproved && !showApprovePlanAction"
                          icon="o_lock_open"
                          color="grey"
                          size="sm"
                          class="q-mr-xs q-mb-xs cursor-inherit"
                        >
                          <q-tooltip>Plan Unlocked</q-tooltip>
                        </q-btn>
                        <q-btn
                          v-if="isFullAccess && showApprovePlanAction && planDate.isApproved && !planDate.isCompleted"
                          icon="o_lock_open"
                          color="secondary"
                          size="sm"
                          class="q-mr-xs q-mb-xs"
                          @click="approveThisWeeklyPlan(planDate.id, false)"
                        >
                          <q-tooltip>Click Here To Unlock This Plan</q-tooltip>
                        </q-btn>
                        <q-btn
                          v-if="(showApprovePlanAction && planDate.isApproved && planDate.isCompleted) || (!showApprovePlanAction && planDate.isApproved)"
                          icon="o_lock"
                          color="primary"
                          size="sm"
                          class="q-mr-xs q-mb-xs cursor-inherit"
                        >
                          <q-tooltip>
                            {{ "Plan Locked By - " + planDate.approvedBy?.person?.firstName + " " + planDate.approvedBy?.person?.lastName + " (" + planDate?.approvedOnUtc + ")" }}
                          </q-tooltip>
                        </q-btn>
                        <q-btn
                          v-if="planDate.isApproved && !showApprovePlanAction && !planDate.isCompleted"
                          icon="o_pause_circle"
                          color="grey"
                          size="sm"
                          class="q-mr-xs q-mb-xs cursor-inherit"
                        >
                          <q-tooltip>Waiting For Completion</q-tooltip>
                        </q-btn>
                        <q-btn
                          v-if="isFullAccess && planDate.isApproved && showApprovePlanAction && !planDate.isCompleted"
                          icon="o_percent"
                          size="sm"
                          color="green"
                          class="q-mr-xs q-mb-xs"
                          @click="showCompletionDialog = true; selectedProjectPlanDateId = planDate.id;"
                        >
                          <q-tooltip>Click Here To Mark As Completed</q-tooltip>
                        </q-btn>
                        <q-btn
                          v-if="planDate.isApproved && planDate.isCompleted"
                          size="sm"
                          :color="
                            planDate.completionPercentage <= 25 ? 'red' :
                            planDate.completionPercentage <= 50 ? 'orange' :
                            planDate.completionPercentage <= 75 ? 'primary' :
                            'green'
                          "
                          class="q-mr-xs q-mb-xs cursor-inherit fs-13"
                        >
                          {{ planDate.completionPercentage }}%
                          <q-tooltip>
                            {{ "Marked Completed By - " + planDate.completedBy?.person?.firstName + " " + planDate.completedBy?.person?.lastName + " (" + planDate?.completedOnUtc + ")" }}
                          </q-tooltip>
                        </q-btn>
                      </div>
                    </td>
                    <td
                      :class="isFullAccess && !planDate.isApproved ? 'cursor-pointer' : ''"
                      @dblclick="isFullAccess && handleExpectedEdit(planDate, line)"
                    >
                      <template v-if="activeExpected.rowId === line.id">
                        <div class="row">
                          <q-editor
                            v-model="line.expectedDescription"
                            class="cursor-auto full-width"
                            :toolbar="toolbar"
                            :fonts="fonts"
                          />
                        </div>
                        <div class="row flex items-center q-my-xs">
                          <div class="col-2">
                            <q-input v-model="line.expectedHours" outlined hide-bottom-space :dense="true" :readonly="true" label="Est. Hrs" class="q-mr-sm" />
                          </div>
                          <div class="col-5">
                            <div class="TaskActivity flex items-center">
                              <!-- Task Assignment - Action Button -->
                              <div class="q-mr-xs">
                                <q-icon name="o_group_add" color="grey-8" size="sm" />
                                <q-tooltip>Assign Resource?</q-tooltip>
                                <q-popup-edit
                                  class="small-popup-title"
                                  style="width: 280px;"
                                  @before-show="() => {
                                    resourceModel.value = { id: uid(), employeeId: null, estimatedHours: ''};
                                    addResourceBtn = true;
                                    weeklyEmployeeDropdownSingleSelect.load(activeProjectId, planTypeId, planDate.weekDate)
                                  }"
                                >
                                  <div class="row justify-between items-center q-mb-sm">
                                    <h3>Add Resource</h3>
                                    <q-btn v-close-popup icon="o_close" size="sm" color="black" flat round dense />
                                  </div>
                                  <formSingleSelectDropdown
                                    v-model="resourceModel.value.employeeId"
                                    label="Select Employee"
                                    :isClearable="false"
                                    :options="weeklyEmployeeDropdownSingleSelect.list.value"
                                    :filter="weeklyEmployeeDropdownSingleSelect.filter"
                                    @update:model-value="val => checkIfEmployeeAlreadyExistsInPlanLine(line.id, resourceModel.value)"
                                  />

                                  <label class="q-mt-sm">Est. Hrs <span class="text-red">*</span></label>
                                  <q-input
                                    v-model="resourceModel.value.estimatedHours"
                                    outlined
                                    hide-bottom-space
                                    dense
                                    :rules="[validateTaskEstimatedHours]"
                                    maxlength="5"
                                    class="full-width"
                                    @keyup="checkAddResourceFields(resourceModel.value)"
                                  />

                                  <div class="row justify-end q-gutter-sm q-mt-sm">
                                    <q-btn v-close-popup label="Cancel" color="grey" flat dense />
                                    <q-btn
                                      v-close-popup
                                      label="Set"
                                      color="primary"
                                      dense
                                      :loading="processing"
                                      :disable="addResourceBtn"
                                      @click="{onAddResourceToLine(planDate, line, resourceModel.value); lastDescriptionBeforeClose = null; }"
                                    />
                                  </div>
                                </q-popup-edit>
                              </div>
                              <!-- Task Assignment - Labels -->
                              <div
                                v-for="(resource, resourceIndex) in line.projectWeeklyPlanDatesLinesAssignedTo"
                                :key="resource.id"
                                class="Person position-relative q-mr-xs"
                              >
                                {{ (resource?.employee?.person?.firstName?.trim()?.[0] || '') + (resource?.employee?.person?.lastName?.trim()?.[0] || '') + "(" + resource.estimatedHours + ")" }}
                                <q-icon class="delete" name="o_close" color="red" size="xs" @click="onDeleteResourceToLine(planDate, line, resource, resourceIndex)" />
                                <q-tooltip>
                                  <div>
                                    <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                                    <span>{{ resource.employee?.person?.firstName + ' ' + resource.employee?.person?.lastName }}</span>
                                  </div>
                                </q-tooltip>
                              </div>
                            </div>
                          </div>
                          <div class="col-5 flex items-center justify-end">
                            <q-btn
                              v-if="activeExpected.rowId"
                              icon-right="o_close"
                              color="grey"
                              size="sm"
                              class="q-mr-xs"
                              @click="activeExpected = { rowId: null}; line.expectedDescription = lastDescriptionBeforeClose; lastDescriptionBeforeClose = null"
                            >
                              Cancel
                            </q-btn>
                            <q-btn v-if="activeExpected.rowId" icon-right="o_check" color="primary" size="sm" :loading="processing" @click="saveExpectedPlan(line)">
                              Save
                            </q-btn>
                          </div>
                        </div>
                      </template>
                      <template v-else>
                        <div class="row flex">
                          <div class="col-12 position-relative">
                            <div
                              class="plan-desc q-mt-md RichTextEditor"
                              v-html="
                                // line.expectedDescription?.length === 0 || line.expectedDescription === ' ' || line.expectedDescription === '<br>' ?
                                !line.expectedDescription ||
                                  !line.expectedDescription.trim() ||
                                  line.expectedDescription.trim().toLowerCase() === '<br>' ?
                                    '<span class=placeholder style=color:grey;font-style:italic;font-size:13px;>'+
                                    (!planDate.isApproved ? 'Double Click To Enter Your Weekly Plan' : 'No Description Available') +'</span>'
                                  : line.expectedDescription"
                            />
                            <q-icon
                              v-if="planDate.isApproved"
                              name="o_lock"
                              size="xs"
                              style="position: absolute;top: 2px;right: 2px;"
                            />
                          </div>
                          <div v-if="line.expectedDescriptionUpdatedOnUtc" class="col-12">
                            <div class="row q-py-xs" style="border-top: 1px solid #cfcfcf;">
                              <div class="col-xxl-5 col-xl-6 col-lg-12 col-md-12 col-sm-12 col-xs-12 flex">
                                <div class="flex items-center">
                                  <q-icon name="o_schedule" color="grey-8" size="sm" />
                                  <b class="fs-15 q-px-sm" style="color: #1b75ab;line-height: 25px;">{{ line.expectedHours }}</b>
                                </div>
                                <div class="TaskActivity flex items-center">
                                  <!-- Task Assignment - Label -->
                                  <div class="q-mr-xs">
                                    <q-icon name="o_group" color="grey-8" size="sm" />
                                  </div>
                                  <!-- Task Assignment - Resource Label -->
                                  <div v-for="resource in line.projectWeeklyPlanDatesLinesAssignedTo" :key="resource.id" class="Person q-mr-xs">
                                    {{ (resource?.employee?.person?.firstName?.trim()?.[0] || '') + (resource?.employee?.person?.lastName?.trim()?.[0] || '') + "(" + resource.estimatedHours + ")" }}
                                    <q-tooltip>
                                      <div>
                                        <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                                        <span>{{ resource.employee?.person?.firstName + ' ' + resource.employee?.person?.lastName }}</span>
                                      </div>
                                      <div>
                                        <q-icon name="o_mail" color="white" size="xs" class="q-mr-xs" />
                                        <span>{{ resource.employee?.person?.primaryEmailAddress }}</span>
                                      </div>
                                      <div v-if="resource.estimatedHours > 0">
                                        <q-icon name="o_timer" color="white" size="xs" class="q-mr-xs" />
                                        <span>{{ resource.estimatedHours }} Est. hrs</span>
                                      </div>
                                    </q-tooltip>
                                  </div>
                                </div>
                              </div>
                              <div class="col-xxl-7 col-xl-6 col-lg-12 col-md-12 col-sm-12 col-xs-12 text-end">
                                <div class="LastUpdatedBy text-capitalize q-md-mt-xs">
                                  Last Updated:- {{ line?.expectedDescriptionUpdatedBy?.person?.firstName + ' '+ line?.expectedDescriptionUpdatedBy?.person?.lastName + '('+ line?.expectedDescriptionUpdatedOnUtc +')' }}
                                </div>
                              </div>
                            </div>
                          </div>
                          <q-tooltip v-if="isFullAccess && !planDate.isApproved">Double click to edit</q-tooltip>
                        </div>
                      </template>
                    </td>
                    <td :class="isFullAccess && !planDate.isCompleted ? 'cursor-pointer' : ''" @dblclick="isFullAccess && handleActualEdit(line, planDate.isCompleted)">
                      <template v-if="activeActual.rowId === line.id">
                        <q-editor
                          v-model="line.actualDescription"
                          class="cursor-auto"
                          :toolbar="toolbar"
                          :fonts="fonts"
                        />
                        <div class="row flex justify-end q-mt-xs">
                          <q-btn
                            v-if="activeActual.rowId"
                            icon-right="o_close"
                            color="grey"
                            size="sm"
                            class="q-mr-xs"
                            @click="activeActual = { rowId: null}; line.actualDescription = lastDescriptionBeforeClose; lastDescriptionBeforeClose = null"
                          >
                            Cancel
                          </q-btn>
                          <q-btn
                            v-if="activeActual.rowId"
                            icon-right="o_check"
                            color="primary"
                            size="sm"
                            :loading="processing"
                            @click="saveActualLine(line)"
                          >
                            Save
                          </q-btn>
                        </div>
                      </template>
                      <template v-else>
                        <div class="position-relative">
                          <div v-if="line.actualDescription?.length > 0" class="plan-desc q-mt-md RichTextEditor" v-html="line.actualDescription" />
                          <q-tooltip v-if="line.actualDescription?.length > 0 && line.actualDescription !== '<br>' && isFullAccess && !planDate.isCompleted">Double click to edit</q-tooltip>
                          <div v-if="(line.actualDescription?.length === 0 || line.actualDescription === '<br>')" class="fs-12 text-red">No Description Available</div>
                          <q-icon
                            v-if="planDate.isApproved && planDate.isCompleted"
                            name="o_lock"
                            size="xs"
                            style="position: absolute;top: 2px;right: 2px;"
                          />
                        </div>
                        <div v-if="line.actualDescriptionUpdatedOnUtc" class="LastUpdatedBy text-capitalize text-end">
                          Last Updated:- {{ line?.actualDescriptionUpdatedBy?.person?.firstName + ' '+ line?.actualDescriptionUpdatedBy?.person?.lastName + '('+ line?.actualDescriptionUpdatedOnUtc +')' }}
                        </div>
                      </template>
                    </td>
                    <td class="text-center">
                      <q-btn
                        v-if="isFullAccess"
                        :disable="planDate.isApproved"
                        icon-right="o_delete"
                        size="sm"
                        style="font-size: 10px;border: 1px solid red;color: red;background: transparent;"
                        @click="deleteWeeklyPlanDatesLine(planDate.projectWeeklyPlanDatesLines, line, lineIndex)"
                      >
                        <q-tooltip v-if="!planDate.isApproved">Delete Line</q-tooltip>
                      </q-btn>
                    </td>
                  </tr>
                  <tr v-if="planDate?.projectWeeklyPlanDatesLines.length === 0">
                    <td class="text-center">
                      {{ planDate.weekDate }}
                      <div style="min-height: 26px;">
                        <q-btn
                          v-if="isFullAccess"
                          :disable="!(projectWeeklyPlanDateLineHovered === planDate.id)"
                          icon="o_add"
                          color="grey-7"
                          size="sm"
                          @click="addWeeklyPlanDateLines(planDate)"
                        >
                          <q-tooltip>Add Line</q-tooltip>
                        </q-btn>
                      </div>
                    </td>
                    <td colspan="3" class="text-red">
                      <div class="flex justify-center items-center">
                        No Plan Available
                        <q-icon v-if="planDate.isApproved" name="o_lock" size="xs" />
                      </div>
                    </td>
                  </tr>
                  <tr v-if="planDate.projectWeeklyPlanDatesReqTaskIssueMapping?.length > 0">
                    <td class="text-center">External Linked</td>
                    <td colspan="3">
                      <div class="row">
                        <div v-if="filterRequestMapping(planDate.projectWeeklyPlanDatesReqTaskIssueMapping, 'Requirement')?.length > 0" class="col-4 flex">
                          <h3 class="q-mr-sm">Requirements</h3>
                          <q-badge
                            v-for="(item) in filterRequestMapping(planDate.projectWeeklyPlanDatesReqTaskIssueMapping, 'Requirement')"
                            :key="item.id"
                            class="q-mr-sm cursor-pointer"
                          >
                            <div @click="onRequirementView(item.requirementId)">
                              {{ item.requirement.requirementNumber }}
                              <q-icon name="o_info"><q-tooltip>{{ item.requirement.title }}</q-tooltip></q-icon>
                            </div>
                            <q-icon v-if="isFullAccess" name="o_close" class="q-ml-xs" color="red" @click="removeMappingFromWeek(item.id, 'Requirement')"><q-tooltip>Remove Requirement?</q-tooltip></q-icon>
                          </q-badge>
                        </div>
                        <div v-if="filterRequestMapping(planDate.projectWeeklyPlanDatesReqTaskIssueMapping, 'Tasks')?.length > 0" class="col-4 flex">
                          <h3 class="q-mr-sm">Tasks</h3>
                          <q-badge
                            v-for="(item) in filterRequestMapping(planDate.projectWeeklyPlanDatesReqTaskIssueMapping, 'Tasks')"
                            :key="item.id"
                            class="q-mr-sm cursor-pointer"
                          >
                            <div @click="onProjectTaskView(item.taskId)">
                              {{ item.task.projectTaskNumber }}
                              <q-icon name="o_info"><q-tooltip>{{ item.task.name }}</q-tooltip></q-icon>
                            </div>
                            <q-icon v-if="isFullAccess" name="o_close" class="q-ml-xs" color="red" @click="removeMappingFromWeek(item.id, 'Task')"><q-tooltip>Remove Task?</q-tooltip></q-icon>
                          </q-badge>
                        </div>
                        <div v-if="filterRequestMapping(planDate.projectWeeklyPlanDatesReqTaskIssueMapping, 'Issue')?.length > 0" class="col-4 flex">
                          <h3 class="q-mr-sm">Issues</h3>
                          <q-badge
                            v-for="(item) in filterRequestMapping(planDate.projectWeeklyPlanDatesReqTaskIssueMapping, 'Issue')"
                            :key="item.id"
                            class="q-mr-sm cursor-pointer"
                          >
                            <div @click="onIssueView(item.issueId)">
                              {{ item.issue.issueNumber }}
                              <q-icon name="o_info"><q-tooltip>{{ item.issue.name }}</q-tooltip></q-icon>
                            </div>
                            <q-icon v-if="isFullAccess" name="o_close" class="q-ml-xs" color="red" @click="removeMappingFromWeek(item.id, 'Issue')"><q-tooltip>Remove Issue?</q-tooltip></q-icon>
                          </q-badge>
                        </div>
                      </div>
                    </td>
                  </tr>
                  <tr style="background-color: rgb(231 231 231);">
                    <td colspan="4" />
                  </tr>
                </tbody>
                <tbody v-if="projectWeeklyPlanDates?.length === 0">
                  <tr>
                    <td colspan="4">
                      <h5 class="text-center text-red">No Plans Available</h5>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
            <div v-if="projectWeeklyPlanDates?.length > 3" class="text-center q-pa-sm">
              <q-btn
                :disable="disableLoadMore"
                icon-right="o_refresh"
                color="secondary"
                outlined
                size="sm"
                @click="loadProjectWeeklyPlanLines(skipIndex)"
              >
                Load More
              </q-btn>
            </div>
          </div>
        </div>
      </q-card-section>
    </q-card>
  </q-page>
  <q-dialog v-model="showPlanApproverDialog" persistent>
    <q-card style="min-width: 300px;">
      <q-card-section>
        <div class="text-h3">Add "{{ activeProjectName }}" Plan Approver?</div>
      </q-card-section>
      <q-card-section class="q-py-sm">
        <formSingleSelectDropdown
          v-model="selectedPlanApproverId"
          label="Select Approver"
          :options="activeEmployeesDropdownSingleSelect.list.value"
          :filter="activeEmployeesDropdownSingleSelect.filter"
        />
      </q-card-section>
      <q-card-actions align="right">
        <q-btn v-close-popup flat label="Cancel" color="primary" @click="showPlanApproverDialog = false" />
        <q-btn flat label="Add" color="primary" :disable="!selectedPlanApproverId" :loading="processing" @click="addProjectPlanApprover(), showPlanApproverDialog = false" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <q-dialog v-model="showCompletionDialog" persistent>
    <q-card style="min-width: 300px;">
      <q-card-section>
        <h3 class="text-primary">Mark As Completed</h3>
        <hr>
        <div class="fs-16 q-mt-sm">
          Are you sure you want to mark this as completed? <br>
          <b class="text-grey-8">Note: <small>This will lock the actual plan and prevent further edits.</small></b>
        </div>
      </q-card-section>
      <q-card-section>
        <div class="q-mx-sm">
          <q-slider
            v-model="selectedCompletionPercentage"
            :min="0"
            :max="100"
            :step="5"
            label
            label-always
            color="primary"
          />
        </div>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="Cancel" @click="showCompletionDialog = false; selectedProjectPlanDateId = null" />
        <q-btn
          flat
          label="Confirm"
          color="primary"
          :disable="selectedCompletionPercentage === 0"
          @click="addPlanCompletionPercentage(selectedProjectPlanDateId, selectedCompletionPercentage)"
        />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>
<script setup>
import { useAuthStore } from "stores/auth";
import { useQuasar, uid, date } from "quasar";
import { ref, onMounted, watch, computed } from "vue";
import { notifyError, notifyWarning, notifySuccess } from "assets/utils";

import projectService from "modules/project/projects.service";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

import addWeeklyPlanner from "modules/project-targetplan/components/_addWeeklyPlanner.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";
import companyContactsModule from "src/modules/company-contacts/utils/dropdowns.js";
import projectTargetPlanModule from "src/modules/project-targetplan/utils/dropdowns.js";
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";
import useSiteTableState from "composables/datatable/useSiteTableState.js";

// SOP Change :- Shared Inputs
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView,
  onProjectMessage,
  onProjectFilesView,
  onAddNotesToProject
} from "src/modules/project/utils/dialogs.js";

// Shared Project Task Dialogs
import {
  initProjectTaskDialogs,
  onProjectTaskView
} from "src/modules/project-tasks/utils/dialogs.js";

import viewRequirement from "modules/requirement/components/view.vue";
import viewIssue from "modules/issue/components/view.vue";

const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const loading = ref(true);
const processing = ref(false);
const planTypeId = ref(null);
const projectPlanLoading = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const employeeId = user.employeeId ?? user.userId;

const siteId = computed(() => authStore.user?.siteId);
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Variable Declarations
// --------------------------------------------------------------------------------------------------------------------------------------------------

const routeProjectId = ref(history.state?.projectId);
const showBackButton = ref(routeProjectId.value !== undefined);

const showFilter = ref(false);
const searchLoader = ref(false);

const activeProjectCharterGroupBy = ref([]);
const activeProjectPlanApprover = ref([]);

const disableLoadMore = ref(false);
const skipIndex = ref(0);
const takeCount = ref(4);

const projectWeeklyPlanDateLineHovered = ref(null);
const lastDescriptionBeforeClose = ref(null);

const isFullAccess = ref(true);
const isViewAccess = ref(true);
const isChatAccess = ref(true);

const showApprovePlanAction = ref(false);
const showPlanApproverDialog = ref(false);
const selectedPlanApproverId = ref(null);

const selectedProjectPlanDateId = ref(null);
const showCompletionDialog = ref(false);
const selectedCompletionPercentage = ref(0);

const addResourceBtn = ref(true);

const defaultPlannerState = {
  search: {
    planTypeId: null,
    searchText: "",
    customerIds: [],
    companyContactIds: [],
    projectIds: [],
    projectCategoryIds: [],
    projectPriorityIds: [],
    projectStatusIds: [],
    projectCoordinatorIds: [],
    projectLeadsIds: [],
    projectTypeIds: [],
    statusId: null
  },

  pagination: {
    sortBy: "CreatedOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  },

  activeProjectPlanId: null,
  activeProjectId: "",
  projectName: "No Project Selected"
};

const {
  search,
  pagination,
  getTableState: getPlannerState,
  saveState: savePlannerState
} = useSiteTableState({
  storageKey: "weekly-Planner-Index",
  siteId,

  defaultSearch: defaultPlannerState.search,
  defaultPagination: defaultPlannerState.pagination
});

const plannerState = getPlannerState();

const activeProjectPlanId = ref(
  plannerState?.activeProjectPlanId ||
  defaultPlannerState.activeProjectPlanId
);

const activeProjectId = ref(
  plannerState?.activeProjectId ||
  defaultPlannerState.activeProjectId
);

const activeProjectName = ref(
  plannerState?.projectName ||
  defaultPlannerState.projectName
);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project List
// --------------------------------------------------------------------------------------------------------------------------------------------------

const tableRef = ref();
const rows = ref([]);
const activeExpected = ref({ rowId: null });
const activeActual = ref({ rowId: null });
const columns = ref([
  { name: "project.name", label: "Project Name", field: "name", align: "left", sortable: true }
]);

const getProjectWeeklyPlanType = async () => {
  const resp = await projectService.getProjectWeeklyPlanTypeId("Project Weekly Target Planning", "Weekly");
  planTypeId.value = resp;
  search.value.planTypeId = resp;
};

// Get/Map project list to table
const getProjects = async (props) => {
  loading.value = true;
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  const resp = await projectService.getWeeklyProjects(payload);
  rows.value = resp.weeklyPlanList;

  if (rows?.value?.length > 0) {
    pagination.value = {
      ...pagination.value,
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    };
    // Open Project Plans
    activeProjectId.value = search.value.projectIds?.length > 0 ? search.value.projectIds[0] : activeProjectId.value;
    if (activeProjectId.value) {
      const weeklyProjectPlanRow = rows.value.find(m => m.projectId === activeProjectId.value);
      if (weeklyProjectPlanRow) LoadProjectPlan(weeklyProjectPlanRow);
    }

    savePlannerState({
      search: search.value,
      pagination: {
        ...pagination.value,
        page,
        rowsPerPage,
        sortBy,
        descending,
        rowsNumber: resp.total
      },

      activeProjectPlanId: activeProjectPlanId.value,
      activeProjectId: activeProjectId.value,
      projectName: activeProjectName.value
    });
  } else {
    projectWeeklyPlanDates.value = [];
  }

  loading.value = false;
  searchLoader.value = false;
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Plan:- When click on View Plan
// --------------------------------------------------------------------------------------------------------------------------------------------------

const projectWeeklyPlanDates = ref([]);

const LoadProjectPlan = async (weeklyPlan) => {
  try {
    projectPlanLoading.value = true;
    activeProjectPlanId.value = weeklyPlan.id;
    activeProjectId.value = weeklyPlan.projectId;
    activeProjectName.value = weeklyPlan.project.name;
    activeProjectCharterGroupBy.value = weeklyPlan.project.projectCharterGroupByList;
    activeProjectPlanApprover.value = weeklyPlan.project.planApprover;

    isFullAccess.value = weeklyPlan.project?.projectUserMappings[0] === undefined ? isFullAccess.value : weeklyPlan.project?.projectUserMappings[0]?.fullAccess;
    isViewAccess.value = weeklyPlan.project?.projectUserMappings[0] === undefined ? isViewAccess.value : weeklyPlan.project?.projectUserMappings[0]?.viewOnly;
    isChatAccess.value = weeklyPlan.project?.projectUserMappings[0] === undefined ? isChatAccess.value : weeklyPlan.project?.projectUserMappings[0]?.notes;

    showApprovePlanAction.value = employeeId === weeklyPlan.project.planApproverId;
    disableLoadMore.value = false;
    projectWeeklyPlanDates.value = await projectService.getWeeklyProjectPlanInDetail(weeklyPlan.projectId, planTypeId.value, 0, takeCount.value);

    // Update Local Storage.
    savePlannerState({
      ...getPlannerState(),
      activeProjectPlanId: activeProjectPlanId.value,
      activeProjectId: activeProjectId.value,
      projectName: activeProjectName.value
    });
  } catch (error) {
    console.error("Error loading project plan:", error);
  } finally {
    setTimeout(() => {
      projectPlanLoading.value = false;
    }, 1500);
  }
};

// Add Project Approver
const addProjectPlanApprover = async () => {
  processing.value = true;
  try {
    if (activeProjectId.value && selectedPlanApproverId.value) {
      const resp = await projectService.addProjectPlanApprover(activeProjectId.value, selectedPlanApproverId.value);
      if (resp) {
        // Get All Projects
        const props = { pagination: pagination.value };
        getProjects(props);
        notifySuccess({ message: "Plan Approver Added" });
      } else {
        notifyError({ message: "Failed to Add Plan Approver" });
      }
    } else {
      notifyError({ message: "Something Went Wrong...." });
    }
  } catch (error) {
    console.error("Error Add Plan Approver:", error);
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Weekly Plan Date
// --------------------------------------------------------------------------------------------------------------------------------------------------

// On Add WeekEnd
const onAddWeeklyPlan = (id, projectId) => {
  activeProjectPlanId.value = id;
  $q.dialog({
    component: addWeeklyPlanner,
    componentProps: { projectId }
  }).onOk(() => {
  }).onCancel(() => {
    // Get All Projects
    const props = { pagination: pagination.value };
    getProjects(props);
  });
};

// On Approve This Plan
const approveThisWeeklyPlan = async (planDateId, isLock) => {
  if (planDateId) {
    $q.dialog({
      title: (isLock ? "Lock" : "Unlock") + " This Plan?",
      html: true,
      message: `
        <span class='fs-15'>Are you sure you want to ${isLock ? "lock" : "unlock"} this Weekly Plan?</span><br>
        <span class="text-grey-8">
          <b>Note:</b> <small>This will ${isLock ? "prevent further" : "unlock"} edits.</small>
        </span>
      `,
      cancel: { label: "Cancel", color: "secondary", flat: true },
      ok: { label: `${isLock ? "Lock" : "Unlock"}`, color: "primary", flat: true },
      persistent: true
    }).onOk(() => {
      projectService.approveThisWeeklyPlan(planDateId, isLock).then((resp) => {
        if (resp) {
          const weeklyProjectPlanRow = rows.value.find(m => m.projectId === activeProjectId.value);
          LoadProjectPlan(weeklyProjectPlanRow);
          notifySuccess({ message: `${isLock ? "Plan locked" : "Plan Unlocked"}` });
        } else {
          notifyError({ message: "Failed to Approve Plan" });
        }
      });
    });
  } else {
    notifyError({ message: "Something Went Wrong...." });
  }
};

// On Mark This As Completed
const addPlanCompletionPercentage = async (planDateId, completionPercentage) => {
  if (planDateId && completionPercentage > 0) {
    const resp = await projectService.addPlanCompletionPercentage(planDateId, completionPercentage);
    if (resp) {
      const weeklyProjectPlanRow = rows.value.find(m => m.projectId === activeProjectId.value);
      LoadProjectPlan(weeklyProjectPlanRow);
      showCompletionDialog.value = false;
      notifySuccess({ message: "Weekly Plan Completed" });
    } else {
      notifyError({ message: "Failed To Mark As Completed Plan" });
    }
  } else {
    notifyError({ message: "Something Went Wrong...." });
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Plan:- Add new project week.
// --------------------------------------------------------------------------------------------------------------------------------------------------

const showCalendar = ref(false);
const selectedDate = ref(null);
const today = new Date();

// Navigation min month to block past month navigation
const minMonth = date.formatDate(today, "YYYY/MM");

// Only allow Sundays and today/future
const isSunday = (dateStr) => {
  const day = new Date(dateStr);
  const todayStart = new Date();
  todayStart.setHours(0, 0, 0, 0);

  return (
    day.getDay() === 0 && day >= todayStart
  );
};

// Format selected date as MM-DD-YYYY
// const formattedDate = computed(() => {
//   return selectedDate.value ? date.formatDate(selectedDate.value, "MM/DD/YYYY") : "";
// });

const onDateSelected = (val) => {
  showCalendar.value = false;
  if (!val) return;

  addWeeklyPlanDate(val);
};

// Add new project Week
// const addWeeklyPlanDate = async (weekendDate) => {
//   if (isWeekendAlreadyPlanned(weekendDate.value, projectWeeklyPlanDates.value)) {
//     notifyError({ message: "Duplicate WeekEnd" });
//   } else {
//     const newId = uid();
//     const model = ref({
//       id: newId,
//       projectWeeklyPlanId: activeProjectPlanId,
//       planTypeId: planTypeId.value,
//       weekDate: weekendDate,
//       projectWeeklyPlanDatesLines: ref([])
//     });

//     const resp = await projectService.addProjectWeeklyPlanDates(model.value);
//     projectWeeklyPlanDates.value.unshift(resp);

//     // Get All Projects
//     const props = { pagination: pagination.value };
//     getProjects(props);
//   }
// };
const addWeeklyPlanDate = async (weekendDate) => {
  if (!weekendDate) return;

  if (isWeekendAlreadyPlanned(weekendDate, projectWeeklyPlanDates.value)) {
    notifyError({ message: "Duplicate WeekEnd" });
    return;
  }

  const newId = uid();
  const model = ref({
      id: newId,
      projectWeeklyPlanId: activeProjectPlanId.value,
      planTypeId: planTypeId.value,
      weekDate: weekendDate,
      projectWeeklyPlanDatesLines: ref([])
    });

  const resp = await projectService.addProjectWeeklyPlanDates(model.value);
  projectWeeklyPlanDates.value.unshift(resp);

  getProjects({ pagination: pagination.value });
};

function isWeekendAlreadyPlanned (weekendDate, list) {
  return list.some(item => item.weekDate === weekendDate && !item.deleted);
}

const filterRequestMapping = (mappings, type) => {
  if (type.toLowerCase() === "requirement") {
    const data = (mappings || []).filter(m => m.requirementId);
    // console.log(data);
    return data;
  }
  if (type.toLowerCase() === "tasks") {
    const data = (mappings || []).filter(m => m.taskId);
    return data;
  }
  if (type.toLowerCase() === "issue") {
    const data = (mappings || []).filter(m => m.issueId);
    return data;
  }
};

const removeMappingFromWeek = async (mappingId, type) => {
  if (mappingId) {
    $q.dialog({
      title: `Delete ${type}?`,
      message: `Are you sure you want to delete this ${type} from Weekly Plan?`,
      cancel: true,
      persistent: true
    }).onOk(async () => {
      const resp = await projectService.deleteProjectWeeklyPlanDateMapping(mappingId);
      if (resp) {
        const weeklyProjectPlanRow = rows.value.find(m => m.projectId === activeProjectId.value);
        LoadProjectPlan(weeklyProjectPlanRow);
        notifyError({ message: `${type} Deleted` });
      } else {
        notifyError({ message: `Failed to delete ${type}` });
      }
    });
  } else {
    notifyError({ message: "Plan Deleted" });
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Plan:- Add new lines under project week.
// --------------------------------------------------------------------------------------------------------------------------------------------------

const loadProjectWeeklyPlanLines = async (index) => {
  skipIndex.value = index + 1;
  const resp = await projectService.getWeeklyProjectPlanInDetail(activeProjectId.value, planTypeId.value, skipIndex.value, takeCount.value);
  projectWeeklyPlanDates.value.push(...resp);
  if (resp?.length === 0) {
    notifyWarning({ message: "No Week Available" });
  }
  if (resp.length < takeCount.value) {
    disableLoadMore.value = true;
  }
  loading.value = false;
};

const handleExpectedEdit = (planDate, line) => {
  if (!planDate.isApproved) {
    activeExpected.value = { rowId: line.id };
    lastDescriptionBeforeClose.value = line.expectedDescription;
    line.expectedDescription = line.expectedDescription || "";
  }
};

const handleActualEdit = (line, isCompleted) => {
  processing.value = false;
  if (!isCompleted) {
    activeActual.value = { rowId: line.id };
    lastDescriptionBeforeClose.value = line.actualDescription;
  }
};

const addWeeklyPlanDateLines = async (planDates) => {
  const hasEmptyDescriptions = planDates.projectWeeklyPlanDatesLines.some(
    line =>
      !line.expectedDescription ||
      line.expectedDescription.trim() === "" ||
      line.expectedDescription.trim().toLowerCase() === "<br>"
  );
  if (!hasEmptyDescriptions) {
    const model = ref({
      id: uid(),
      projectWeeklyPlanDatesId: planDates.id,
      expectedDescription: "",
      actualDescription: "",
      isEditExpectedDescription: true
    });
    const resp = await projectService.saveProjectWeeklyPlanDatesLine(model.value);
    resp.isEditExpectedDescription = false;
    planDates.projectWeeklyPlanDatesLines.unshift(resp);
    activeExpected.value.rowId = resp;
  } else {
    notifyError({ message: "Please Enter Existing Plan" });
  }
};

const saveExpectedPlan = async (line) => {
  processing.value = true;
  try {
    if (line?.expectedDescription.length === 0 || line?.expectedDescription === "<br>") {
      notifyError({ message: "Please Enter Plan" });
    } else {
      line.isEditExpectedDescription = true;
      const resp = await projectService.saveProjectWeeklyPlanDatesLine(line);
      Object.assign(line, null);
      Object.assign(line, resp);
      activeExpected.value.rowId = null;
      line.isEditExpectedDescription = false;
    }
  } catch (error) {
    console.error("Error in saving the plan:", error);
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

const saveActualLine = async (line) => {
  processing.value = true;
  try {
    if (line?.actualDescription.length === 0 || line?.actualDescription === "<br>") {
      notifyError({ message: "Please Enter Plan" });
    } else {
      line.isEditActualDescription = true;
      const resp = await projectService.saveProjectWeeklyPlanDatesLine(line);
      Object.assign(line, resp);
      activeActual.value.rowId = null;
      line.isEditActualDescription = false;
    }
  } catch (error) {
    console.error("Error in saving the plan:", error);
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

const deleteWeeklyPlanDatesLine = (weeklyDatesLines, line, lineIndex) => {
  $q.dialog({
    title: "Delete Plan?",
    message: "Are you sure you want to delete Weekly Plan?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    projectService.deleteProjectWeeklyPlanDatesLine(line);
    weeklyDatesLines.splice(lineIndex, 1);
    activeExpected.value.rowId = null;
    activeActual.value.rowId = null;
    notifyError({ message: "Plan Deleted" });
  });
};

const validateTaskEstimatedHours = (value) => {
  if (!value) return "Estimated hours are required."

  const regex = /^\d{1,3}(\.\d{1,2})?$/
  return regex.test(String(value)) ? true : "Invalid format"
}

const getTotalHoursForWeekSummary = (employees = []) => {
  return employees.reduce((total, line) => {
    const hours = parseFloat(line.totalEstimatedHours);
    return total + (isNaN(hours) ? 0 : hours);
  }, 0).toFixed(2); // Optional: format to 2 decimal places
};

const getResourceSummaryForWeekPlanById = async (weekPlan) => {
  const resp = await projectService.getResourceSummaryForWeekPlanById(planTypeId.value, weekPlan.id);
  weekPlan.employeeEstimateHoursForWeekSummaryList = resp;
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Plan:- Add resource to line under project Week Lines.
// --------------------------------------------------------------------------------------------------------------------------------------------------

const resourceModel = ref({
  id: uid(),
  projectWeeklyPlanDatesLineId: "",
  employeeId: null,
  estimatedHours: ""
});

const checkIfEmployeeAlreadyExistsInPlanLine = async (lineId, resource) => {
  if (lineId && resource?.employeeId) {
    const resp = await projectService.checkIfEmployeeAlreadyExistsInPlanLine(lineId, resource.employeeId);
    if (resp) {
      resource.employeeId = null;
      notifyError({ message: "Cannot Add Duplicate Employee" });
    } else {
      checkAddResourceFields(resource);
    }
  }
};

const checkAddResourceFields = (resource) => {
  const hasMissingData =
    !resource?.employeeId ||
    !resource?.estimatedHours ||
    !isPositiveNumberOrDecimal(resource.estimatedHours);
  addResourceBtn.value = hasMissingData;
};

const isPositiveNumberOrDecimal = (value) => {
  const regex = /^\d{1,3}(\.\d{1,2})?$/;
  return regex.test(String(value));
};

const onAddResourceToLine = async (planDate, line, resource) => {
  processing.value = true;
  try {
    if (!line.projectWeeklyPlanDatesLinesAssignedTo) {
      line.projectWeeklyPlanDatesLinesAssignedTo = [];
    }
    resource.projectWeeklyPlanDatesLineId = line.id;
    const resp = await projectService.addResourceToWeeklyPlanLine(resource);
    line.projectWeeklyPlanDatesLinesAssignedTo.push({ ...resp });
    getTotalWeekLineExpectedHours(line);
    getResourceSummaryForWeekPlanById(planDate);
  } catch (error) {
    console.error("Error in submitting the resource:", error);
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

const onDeleteResourceToLine = (planDate, line, resource, resourceIndex) => {
  $q.dialog({
    title: "Remove Employee From Plan?",
    message: "Are you sure you want to remove this employee from plan?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    projectService.deleteResourceToWeeklyPlanLine(resource.id);
    line.projectWeeklyPlanDatesLinesAssignedTo.splice(resourceIndex, 1);
    getTotalWeekLineExpectedHours(line);
    getResourceSummaryForWeekPlanById(planDate);
    notifyError({ message: "Resource Deleted" });
  });
};

const getTotalWeekLineExpectedHours = (line = []) => {
  const totalWeekHours = line.projectWeeklyPlanDatesLinesAssignedTo.reduce((total, assignTo) => {
    const hours = parseFloat(assignTo.estimatedHours);
    return total + (isNaN(hours) ? 0 : hours);
  }, 0).toFixed(2);
  line.expectedHours = totalWeekHours;
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Advance Filter:- Search and Clear
// --------------------------------------------------------------------------------------------------------------------------------------------------

const onSearch = () => {
  const props = { pagination: pagination.value };
  getProjects(props);
};

const onClear = () => {
  Object.assign(search.value, defaultPlannerState.search);
  if (routeProjectId?.value?.length > 0) { routeProjectId.value = ""; delete history?.state?.projectId; }
  activeProjectId.value = "";
  projectWeeklyPlanDates.value = [];
  search.value.planTypeId = planTypeId.value;
  savePlannerState({
    ...defaultPlannerState
  });
  onSearch();
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Advance Filter:- Applied Filter Labels.
// --------------------------------------------------------------------------------------------------------------------------------------------------

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
  ...mapFilterToLabel(search.value.projectCategoryIds, projectCategoriesDropdown.list, "Project Category"),
  ...mapFilterToLabel(search.value.projectCoordinatorIds, activeEmployeesDropdown.list, "Project Coordinator"),
  ...mapFilterToLabel(search.value.projectLeadsIds, activeEmployeesDropdown.list, "Project Leads"),
  ...mapFilterToLabel(search.value.projectStatusIds, projectStatusDropdown.list, "Project Status"),
  ...mapSingleFilterToLabel(search.value.statusId, projectActiveInActiveDropdown.list, "Status"),
  ...mapFilterToLabel(search.value.projectPriorityIds, projectPrioritiesDropdown.list, "Project Priority"),
  ...mapFilterToLabel(search.value.projectTypeIds, projectTypesDropdown.list, "Project Type"),
  ...mapFilterToLabel(search.value.customerIds, customerNameDropdown.list, "Customer"),
  ...mapFilterToLabel(search.value.companyContactIds, companyContactNameDropdown.list, "Company Contact")
}));

function onClearFilters (key) {
  if (key === "Project Name") {
    search.value.projectIds = [];
    if (routeProjectId?.value?.length > 0) { routeProjectId.value = ""; delete history?.state?.projectId; }
  } else if (key === "Project Category") {
    search.value.projectCategoryIds = [];
  } else if (key === "Project Coordinator") {
    search.value.projectCoordinatorIds = [];
  } else if (key === "Project Leads") {
    search.value.projectLeadsIds = [];
  } else if (key === "Project Status") {
    search.value.projectStatusIds = [];
  } else if (key === "Status") {
    search.value.statusId = null;
  } else if (key === "Project Priority") {
    search.value.projectPriorityIds = [];
  } else if (key === "Project Type") {
    search.value.projectTypeIds = [];
  } else if (key === "Customer") {
    search.value.customerIds = [];
  } else if (key === "Company Contact") {
    search.value.companyContactIds = [];
  }
  delete appliedFilters.value[key];
  getProjects({ pagination: pagination.value });
}

function getFilterCount (key) {
  switch (key) {
  case "Project Name": return search.value.projectIds?.length || 0;
  case "Project Category": return search.value.projectCategoryIds?.length || 0;
  case "Project Coordinator": return search.value.projectCoordinatorIds?.length || 0;
  case "Project Leads": return search.value.projectLeadsIds?.length || 0;
  case "Project Status": return search.value.projectStatusIds?.length || 0;
  case "Project Priority": return search.value.projectPriorityIds?.length || 0;
  case "Project Type": return search.value.projectTypeIds?.length || 0;
  case "Customer": return search.value.customerIds?.length || 0;
  case "Company Contact": return search.value.companyContactIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Advance Filter:- Dropdown Functions
// --------------------------------------------------------------------------------------------------------------------------------------------------

const {
  projectNameDropdown,
  projectCategoriesDropdown,
  projectActiveInActiveDropdown,
  projectPrioritiesDropdown,
  projectTypesDropdown,
  projectStatusDropdown
} = projectModule();

const {
  activeEmployeesDropdown,
  activeEmployeesDropdownSingleSelect
} = employeeModule();

const { customerNameDropdown } = customerModule();
const { companyContactNameDropdown } = companyContactsModule();
const { weeklyEmployeeDropdownSingleSelect } = projectTargetPlanModule();
// --------------------------------------------------------------------------------------------------------------------------------------------------
// View Actions
// --------------------------------------------------------------------------------------------------------------------------------------------------

initProjectDialogs(activeProjectPlanId);
initProjectTaskDialogs(activeProjectPlanId);

const onRequirementView = (id) => {
  $q.dialog({
    component: viewRequirement,
    componentProps: { id }
  });
};

const onIssueView = (id) => {
  $q.dialog({
    component: viewIssue,
    componentProps: { id }
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Page Load
// --------------------------------------------------------------------------------------------------------------------------------------------------
function loadProjectNameDropdown() {
  const statusText =
    projectActiveInActiveDropdown
      .getLabelByValue(search.value.statusId)
      ?.toLowerCase()
      ?.trim();

  let isActive;

  if (statusText === 'inactive') {
    isActive = false;
  } else if (statusText === 'active') {
    isActive = true;
  } else {
    isActive = null;
  }

  projectNameDropdown.load(false, isActive);
}

onMounted(async () => {
  await getProjectWeeklyPlanType();

  // Advance Filter Dropdown
  customerNameDropdown.load();
  companyContactNameDropdown.load();
  projectCategoriesDropdown.load("ProjectCategory");
  projectPrioritiesDropdown.load("Project Priorities");
  projectStatusDropdown.load("Project Status");
  projectTypesDropdown.load("Project Type");
  activeEmployeesDropdown.load(user.siteId);
  activeEmployeesDropdownSingleSelect.load(user.siteId);

  // Get Project Active/InActive and Set default to Active
  await projectActiveInActiveDropdown.load("Project Active Status");
  const activeValue = await projectActiveInActiveDropdown.getValueByLabel("Active");
  loadProjectNameDropdown();

  if (activeValue && (search.value.statusId === null || search.value.statusId === undefined)) {
    search.value.statusId = activeValue;
  }

  // Get All Projects
  await getProjects({ pagination: pagination.value });
});

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Search: When user types in search
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(() => search.value.searchText, () => {
  searchLoader.value = true;
  getProjects({ pagination: pagination.value });
});

watch(
  () => search.value.statusId,
  (newVal, oldVal) => {
    if (newVal === oldVal) return;

    search.value.projectIds = [];
    loadProjectNameDropdown();
  }
);
</script>
<style scoped>
.resourceIcon span {
    position: absolute;
    left: 20px;
    top: -6px;
    font-weight: bold;
}
.AddProjectPlanApprover {
    border: 1px solid;
    padding: 0 5px;
    background-color: white;
    color: #1b75ab;
    border-radius: 5px;
}
.Team .TeamMember {
    background-color: #f9f9f9;
    margin-left: 2px;
    color: #1b75ab;
    border-radius: 4px;
}
.delete {
  position: absolute;
  right: -8px;
  top: -8px;
}
.TaskActivity .Person {
  border-radius: 10%;
  background-color: #5d5d5d;
  color: white;
  font-size: 12px;
  padding: 2px 3px;
}
.customBTN{
  border: 1px solid white;
  background-color: white;
  color: #1b75ab;
}
.CommonBordered{
  color: #000;
  background-color: #fff;
  border-radius: 4px;
  box-shadow: 0 1px 5px rgba(0, 0, 0, 0.2), 0 2px 2px rgba(0, 0, 0, 0.14), 0 3px 1px -2px rgba(0, 0, 0, 0.12);
}
.PlanBody{
    display: flex;
    flex-wrap: wrap;
    justify-content: space-between;
}
.Plans {
  display: block;
  width: 100%;
}
.plan{
  position: relative;
}
.plan-icon{
  position: absolute;
  left: -25px;
  top: -7px;
}
.row.weeklyDate {
  margin-bottom: 15px;
  padding: 5px;
  background-color: #fbfbfb;
  border-bottom: 1px solid #1b75ab59;
}
.min-height-auto {
    min-height: auto;
}
</style>
