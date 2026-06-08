<template>
  <q-page padding>
    <div class="dashboard">
      <div class="q-card q-gutter-xs q-pa-sm">
        <div class="row">
          <div class="col-12 col-md-8 col-lg-9 col-xxl-9 q-pr-sm">
            <div class="card-header with-tools bg-white text-blue-grey-5 q-mb-sm" style="border: 1px solid #1b75ab;">
              <div class="row">
                <div class="col-3 col-md-2 col-md-2 col-lg-2 bg-primary text-white flex flex-center">
                  <h1 class="q-my-xs q-mx-sm">Time In - Out</h1>
                </div>
                <div class="col-9 col-md-10 col-lg-10 q-pa-xs">
                  <div class="col-12 q-pl-xs">
                    <div class="row items-center q-col-gutter-sm">
                      <div class="col-auto">
                        <div
                          class="q-pa-sm bg-white row items-center"
                          style="border: 1px solid #1976d2; width: fit-content;"
                        >
                          <span class="text-black">
                            Current Status:
                            <strong class="text-primary">{{ employeeStatus }}</strong>
                          </span>
                        </div>
                      </div>
                      <div class="col-auto">
                        <q-btn
                          :label="model.timeInDate && !model.timeOutDate ? 'Time Out' : 'Time In'"
                          color="primary"
                          no-caps
                          :loading="processing"
                          :disable="processing"
                          @click="onSubmitTimeInOut(!!model.timeIn && !model.timeOutDate)"
                        />
                      </div>
                      <div v-if="model.timeIn" class="col-auto row items-center no-wrap">
                        <strong class="q-mr-xs text-black">Time In:</strong>
                        <span class="text-primary">
                          {{ model.timeInDate }} {{ model.timeIn }}
                        </span>
                      </div>
                      <div v-if="model.timeOutDate" class="col-auto row items-center no-wrap">
                        <strong class="q-mr-xs text-black">Time Out:</strong>
                        <span class="text-primary">
                          {{ model.timeOutDate }} {{ model.timeOut }}
                        </span>
                      </div>
                      <div v-if="model.actualHoursStr" class="col-auto row items-center no-wrap">
                        <strong class="q-mr-xs text-black">Duration:</strong>
                        <span class="text-primary">
                          {{ model.actualHoursStr }}
                        </span>
                      </div>
                      <div class="col-auto">
                        <q-btn
                          outline
                          label="View All"
                          no-caps
                          class="text-primary btnRounded"
                          @click="$router.push('time-in-time-out/list')"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="hidden">
              <div
                class="q-pa-sm row items-center no-wrap"
                style="border: 1px solid #1b75ab; width: fit-content;"
              >
                <div class="row items-center no-wrap q-mr-md">
                  <div
                    class="q-pa-sm bg-white q-mr-sm row items-center"
                    style="border: 1px solid #1976d2; width: fit-content;"
                  >
                    <span class="text-black">
                      Current Status:
                      <strong class="text-primary">{{ employeeStatus }}</strong>
                    </span>
                  </div>
                </div>
                <div class="row items-center no-wrap">
                  <q-btn
                    :label="model.timeIn && !model.timeOut ? 'Time Out' : 'Time In'"
                    color="primary"
                    no-caps
                    class="q-mr-md"
                    :loading="processing"
                    :disable="processing"
                    @click="onSubmitTimeInOut(!!model.timeIn && !model.timeOut)"
                  />
                  <div v-if="model.timeIn" class="row items-center no-wrap q-mr-md">
                    <strong class="q-mr-xs">Time In:</strong>
                    <span class="text-primary">
                      {{ model.timeIn }} {{ model.timeInDate }}
                    </span>
                  </div>
                  <div v-if="model.timeOut" class="row items-center no-wrap">
                    <strong class="q-mr-xs">Time Out:</strong>
                    <span class="text-primary">
                      {{ model.timeOut }} {{ model.timeOutDate }}
                    </span>
                  </div>
                  <q-btn
                    outline
                    label="View All"
                    no-caps
                    class="text-primary btnRounded q-ml-sm"
                    @click="$router.push('time-in-time-out/list')"
                  />
                </div>
              </div>
            </div>
            <div v-if="quickLinkRows && quickLinkRows.length > 0" class="card-header with-tools bg-white text-blue-grey-5 q-mb-sm" style="border: 1px solid #1b75ab;">
              <div class="row">
                <div class="col-3 col-md-2 col-md-2 col-lg-2 bg-primary text-white flex flex-center">
                  <h1 class="q-my-xs q-mx-sm">Quick Links</h1>
                </div>
                <div class="col-9 col-md-10 col-lg-10 q-pa-xs">
                  <div v-if="quickLinkRows && quickLinkRows.length > 0" class="q-ml-xs">
                    <div class="row q-gutter-xs items-start">
                      <div
                        v-for="(row, index) in quickLinkRows"
                        :key="index"
                        class="text-left"
                        style="min-width: auto; position: relative;"
                      >
                        <div class="row items-center text-caption text-green-9">
                          <q-btn color="primary" push outline type="button" class="" no-caps :href="row.modulesMenus.link">
                            {{ row.modulesMenus.displayName }}
                            <q-icon name="o_open_in_new" size="16px" />
                          </q-btn>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div v-if="upcomingHolidayRows && upcomingHolidayRows.length > 0" class="card-header with-tools bg-white text-blue-grey-5" style="border: 1px solid #1b75ab;">
              <div class="row">
                <div class="col-3 col-md-2 col-md-2 col-lg-2 bg-primary text-white flex flex-center">
                  <h1 class="q-my-xs q-mx-sm">Upcoming Holidays</h1>
                </div>
                <div class="col-9 col-md-10 col-lg-10 q-pa-xs">
                  <div v-if="upcomingHolidayRows && upcomingHolidayRows.length > 0" class="q-ml-xs">
                    <div class="row q-gutter-xs items-start">
                      <div
                        v-for="(row, index) in upcomingHolidayRows"
                        :key="index"
                        class="q-pa-xs text-left"
                        :style="{
                          border: row.title ? '1px solid #146c2e' : '1px solid #0f669b',
                        }"
                        style="min-width: auto;position: relative;padding-right: 2%;"
                      >
                        <div class="row items-center text-caption text-green-9">
                          <span>{{ row.title }}</span>
                        </div>
                        <div class="text-caption text-green-9">
                          {{ toDate(row.date) }}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div v-if="onLeaveRows && onLeaveRows.length > 0" class="card-header with-tools bg-white text-blue-grey-5 q-mt-md" style="border: 1px solid #1b75ab;">
              <div class="row">
                <div class="col-3 col-md-2 col-md-2 col-lg-2 bg-primary text-white flex flex-center">
                  <h1 class="q-my-xs q-mx-sm">On Leaves</h1>
                </div>
                <div class="col-9 col-md-10 col-lg-10 q-pa-xs">
                  <div class="q-ml-xs">
                    <div class="row q-gutter-xs items-start">
                      <div
                        v-for="(row, index) in onLeaveRows"
                        :key="index"
                        class="q-pa-xs text-left"
                        :style="{
                          border: row.leaveStatuses.dropDownValue === 'Approved' ? '1px solid #146c2e' : '1px solid #0f669b',
                        }"
                        style="min-width: auto;position: relative;padding-right: 2%;"
                      >
                        <div class="row items-center text-caption">
                          <span :class="{'text-green-9': row.leaveStatuses.dropDownValue === 'Approved'}">{{ row.employee.person.firstName + " " + row.employee.person.lastName }}</span>
                          <div
                            v-if="row.noofLeaves > 1"
                            :class="[
                              'flex justify-center text-center h-16 text-caption',
                              row.leaveStatuses.dropDownValue === 'Approved' ? 'bg-primary text-white' : 'bg-grey-9 text-white'
                            ]" style="border-radius: 50%;width: 9%;position: absolute; top: 0; right: 2vh; margin: 1%;z-index: 2;white-space: nowrap;"
                          >
                            {{ row.noofLeaves }}
                          </div>
                          <q-icon v-if="row.leaveStatuses.dropDownValue === 'Approved'" name="o_check_circle" color="primary" size="xs" style="position: absolute; top: 0; right: 0; margin: 1px;z-index: 1;white-space: nowrap;">
                            <q-tooltip>Approved</q-tooltip>
                          </q-icon>
                          <q-icon v-else name="o_pause_circle_filled" size="xs" color="grey-9" style="position: absolute; top: 0; right: 0; margin: 1px;z-index: 1;white-space: nowrap;">
                            <q-tooltip>{{ row.leaveStatuses.dropDownValue === 'Sent to Approver'? 'Waiting for Approval' : row.leaveStatuses.dropDownValue }}</q-tooltip>
                          </q-icon>
                        </div>
                        <div class="text-caption">
                          <span v-if="row.noofLeaves > 1" :class="{'text-green-9': row.leaveStatuses.dropDownValue === 'Approved'}">
                            {{ toDate(row.fromDate) }} to {{ toDate(row.toDate) }}
                          </span>
                          <span v-else :class="{'text-green-9': row.leaveStatuses.dropDownValue === 'Approved'}">
                            {{ toDate(row.fromDate) }} <span v-if="row.halfDayType" class="text-red"> <strong>({{ row.halfDayType }})</strong> </span>
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="row q-mt-md q-col-gutter-md justify-center">
              <div class="col-lg-6 col-md-6 col-sm-12 col-12">
                <q-page style="min-height: 15vh;">
                  <q-table ref="dailyPlannerTableRef" class="Custom-DataTable" hide-pagination flat :loading="dailyPlannerLoading" :rows="dailyPlannerRows" row-key="id" :pagination="dailyPlannerPagination" separator="cell" style="width: 100%; border: 0.5px solid #1b75ab;" no-data-label="No data available" :filter="filter" @request="getDailyPlanners">
                    <template #loading>
                      <q-inner-loading showing color="primary">
                        <q-spinner-ios size="40px" class="q-mt-xl" />
                      </q-inner-loading>
                    </template>
                    <template #header="props">
                      <q-tr>
                        <q-th colspan="3">
                          <span class="flex justify-between items-center">
                            <span class="fs-14 text-grey-9">DAILY PLANNER</span>
                            <span><q-btn outline label="View All" no-caps class="text-primary btnRounded" @click="$router.push('/my-daily-planner')" /></span>
                          </span>
                        </q-th>
                      </q-tr>
                      <q-tr :props="props" class="bg-primary text-white">
                        <q-th style="width: 12%;">ACTIVITY DATE</q-th>
                        <q-th style="width: 30%;">PROJECT NAME</q-th>
                        <q-th>ACTIVITY DETAILS</q-th>
                      </q-tr>
                    </template>
                    <template #body="props">
                      <q-tr v-for="(line) in props.row.dailyPlannerLines" :key="line.id">
                        <q-td class="text-end">{{ props.row.dailyPlannerDate }}</q-td>
                        <q-td style="white-space: normal; overflow-wrap: break-word; width: 30%; max-width: 30%">{{ line.project.name }}</q-td>
                        <q-td class="RichTextEditor" style="white-space: normal; overflow: auto; width: 50px; max-width: 50px; word-wrap: break-word;"><div v-html="line.description" /></q-td>
                      </q-tr>
                    </template>
                  </q-table>
                </q-page>
              </div>
              <div class="col-lg-6 col-md-6 col-sm-12 col-12">
                <q-page style="min-height: 15vh;">
                  <q-table ref="timesheetTableRef" class="Custom-DataTable" hide-pagination flat :loading="timesheetLoading" :rows="timesheetRows" :pagination="dailyPlannerPagination" row-key="id" separator="cell" style="width: 100%; border: 0.5px solid #1b75ab;" no-data-label="No data available" :filter="filter" @request="getTimesheets">
                    <template #loading>
                      <q-inner-loading showing color="primary">
                        <q-spinner-ios size="40px" class="q-mt-xl" />
                      </q-inner-loading>
                    </template>
                    <template #header="props">
                      <q-tr>
                        <q-th colspan="4">
                          <span class="flex justify-between items-center">
                            <span class="fs-14 text-grey-9">TIMESHEET</span>
                            <span><q-btn outline label="View All" no-caps class="text-primary btnRounded" @click="$router.push('/timesheet')" /></span>
                          </span>
                        </q-th>
                      </q-tr>
                      <q-tr :props="props" class="bg-primary text-white">
                        <q-th style="width: 12%;">ACTIVITY DATE</q-th>
                        <q-th style="width: 30%;">PROJECT NAME</q-th>
                        <q-th class="hidden">ACTIVITY DETAILS</q-th>
                        <q-th class="text-center" style="width: 12%;">HOURS</q-th>
                      </q-tr>
                    </template>
                    <template #body="props">
                      <q-tr v-for="(line, index) in getTotalHoursByProject(props.row.timesheetLines)" :key="index">
                        <q-td v-if="index === 0" :rowspan="getTotalHoursByProject(props.row.timesheetLines).length">
                          {{ props.row.timesheetDate }}
                        </q-td>
                        <q-td style="white-space: normal; overflow-wrap: break-word; width: 30%; max-width: 30%; border-left: 0.5px solid #ccc !important;">
                          <template v-if="getTotalHoursByProject(props.row.timesheetLines).length > 1">
                            {{ line.projectName }} (<b>{{ line.totalHours }}</b>)
                          </template>
                          <template v-else>
                            {{ line.projectName }}
                          </template>
                        </q-td>
                        <q-td class="hidden" style="white-space: normal; overflow: auto; width: 50px; max-width: 50px; word-wrap: break-word;">
                          <div class="RichTextEditor" v-html="line.description" />
                        </q-td>
                        <q-td v-if="getTotalHoursByProject(props.row.timesheetLines).length > 1 && index === 0" :rowspan="getTotalHoursByProject(props.row.timesheetLines).length" class="text-center">
                          <b>{{ getTotalHoursByProject(props.row.timesheetLines) .reduce((sum, l) => sum + parseFloat(l.totalHours), 0) .toFixed(2) }}</b>
                        </q-td>
                        <q-td v-else-if="getTotalHoursByProject(props.row.timesheetLines).length === 1" class="text-center">
                          <b>{{ line.totalHours }}</b>
                        </q-td>
                      </q-tr>
                    </template>
                  </q-table>
                </q-page>
              </div>
            </div>
            <div class="col-lg-12">
              <q-card class="q-mt-md bg-primary border-blue">
                <q-card-section class="card-header items-center">
                  <h1 class="text-white text-center" style="font-size: 20px;">Timesheet Hours Total</h1>
                </q-card-section>
              </q-card>
              <div class="row q-mt-md q-col-gutter-md justify-center">
                <div class="col-lg-6 col-md-6 col-sm-12 col-12">
                  <q-page style="min-height: 15vh;">
                    <q-table
                      class="equal-width-col"
                      flat
                      :loading="timesheetWeekHoursLoading"
                      hide-pagination
                      :rows="timesheetWeekHours.rows"
                      row-key="task.id"
                      separator="cell"
                      style="width: 100%; border: 0.5px solid #1b75ab;"
                    >
                      <template #loading>
                        <q-inner-loading showing color="primary">
                          <q-spinner-ios size="40px" class="q-mt-xl" />
                        </q-inner-loading>
                      </template>
                      <template #header="props">
                        <q-tr>
                          <q-th colspan="3">
                            <span class="flex justify-between items-center">
                              <span class="fs-14 text-grey-9">Current week's daily totals</span>
                            </span>
                          </q-th>
                        </q-tr>
                        <q-tr :props="props" class="bg-primary text-white">
                          <q-th
                            v-for="col in timesheetWeekHours.columns" :key="col.index"
                          >
                            {{ formatDate(col.date) }}
                          </q-th>
                        </q-tr>
                      </template>
                      <template #body>
                        <q-tr class="text-center">
                          <q-td
                            v-for="(col, index) in timesheetWeekHours.columns"
                            :key="index"
                          >
                            {{ timesheetWeekHours.rows[0][`col${col.index}`] }}
                          </q-td>
                        </q-tr><q-separator />
                      </template>
                    </q-table>
                  </q-page>
                </div>
                <div class="col-lg-6 col-md-6 col-sm-12 col-12">
                  <q-page style="min-height: 15vh;">
                    <q-table
                      class="equal-width-col"
                      flat
                      :loading="timesheetMonthLoading"
                      hide-pagination
                      :rows="timesheetMonthHours.rows"
                      row-key="task.id"
                      separator="cell"
                      style="width: 100%; border: 0.5px solid #1b75ab;"
                    >
                      <template #loading>
                        <q-inner-loading showing color="primary">
                          <q-spinner-ios size="40px" class="q-mt-xl" />
                        </q-inner-loading>
                      </template>
                      <template #header="props">
                        <q-tr>
                          <q-th colspan="3">
                            <span class="flex justify-between items-center">
                              <span class="fs-14 text-grey-9">Current month's weekly totals</span>
                            </span>
                          </q-th>
                        </q-tr>
                        <q-tr :props="props" class="bg-primary text-white">
                          <q-th
                            v-for="col in timesheetMonthHours.columns" :key="col.index"
                          >
                            {{ col.displayDateRange }}
                            <q-tooltip>{{ col.dateTooltip }}</q-tooltip>
                          </q-th>
                        </q-tr>
                      </template>
                      <template #body>
                        <q-tr class="text-center">
                          <q-td
                            v-for="(col, index) in timesheetMonthHours.columns"
                            :key="index"
                          >
                            {{ timesheetMonthHours.rows[0][`col${col.index}`] }}
                          </q-td>
                        </q-tr><q-separator />
                      </template>
                    </q-table>
                  </q-page>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-md justify-center">
              <div class="col-12 col-sm-12 col-md-4 col-lg-4 col-xxl-4">
                <q-card class="q-mt-md bg-primary border-blue">
                  <q-card-section class="card-header items-center">
                    <h1 class="text-white text-center" style="font-size: 20px;">My Projects</h1>
                  </q-card-section>
                </q-card>
                <div class="row q-mt-md">
                  <div class="col-12">
                    <q-page style="min-height: 15vh;">
                      <q-table
                        ref="projectTableRef"
                        v-model:pagination="projectPagination"
                        :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
                        :loading="projectLoading"
                        :rows="projectRows"
                        :columns="projectColumns"
                        row-key="id"
                        separator="cell"
                        no-data-label="No data available"
                        binary-state-sort
                        :rows-per-page-options="[20, 50, 100, 200, 500]"
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
                          </q-tr>
                        </template>
                        <template #body="props">
                          <q-tr :props="props">
                            <q-td>{{ props.row.name }}</q-td>
                          </q-tr>
                        </template>
                      </q-table>
                    </q-page>
                  </div>
                </div>
              </div>
              <div class="col-12 col-sm-12 col-md-8 col-lg-8 col-xxl-8">
                <q-card class="q-mt-md bg-primary border-blue">
                  <q-card-section class="card-header items-center">
                    <h1 class="text-white text-center" style="font-size: 20px;">My Open Task Activities</h1>
                  </q-card-section>
                </q-card>
                <div class="row q-mt-md">
                  <div class="col-12">
                    <q-page style="min-height: 15vh;">
                      <q-table
                        ref="activityTableRef"
                        v-model:pagination="activityPagination"
                        :class="rows.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'"
                        :rows-per-page-options="[20, 50, 100, 200, 500]"
                        :loading="taskActivityLoading"
                        :rows="activityRows"
                        :columns="activityColumns"
                        row-key="id"
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
                            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                          </q-tr>
                        </template>
                        <template #body="props">
                          <q-tr :props="props" :class="highlightedId == props.row.id ? 'highlight' : ''" :set="(preProjectName = null, preProjectModuleName = null, preProjectTaskName = null, preTaskStatus = null)">
                            <q-td style="width: 15%; white-space: normal;">
                              <div class="row no-wrap items-center justify-between">
                                <span style="flex: 1; word-break: break-word; white-space: normal;">
                                  <span v-if="preProjectName !== props.row.project.name" :set="preProjectName = props.row.project.name">
                                    {{ props.row.project.name }}
                                  </span>
                                </span>
                              </div>
                            </q-td>
                            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">
                              <span v-if="preProjectModuleName !== props.row.projectModule.name" :set="preProjectModuleName = props.row.projectModule.name">
                                {{ props.row.projectModule.name }}
                              </span>
                            </q-td>
                            <q-td style="width: 20%; white-space: normal;">
                              <div class="row no-wrap items-center justify-between">
                                <span style="flex: 1; word-break: break-word; white-space: normal;">
                                  <span v-if="preProjectTaskName !== props.row.task.name" :set="preProjectTaskName = props.row.task.name">
                                    {{ props.row.task.name }}</span>
                                </span>
                              </div>
                            </q-td>
                            <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;">
                              {{ props.row.name }}
                              <q-icon
                                v-if="props.row.activityNameDescription"
                                name="o_info"
                                size="17px"
                                class="q-ml-xs"
                              >
                                <q-tooltip v-if="props.row.activityNameDescription" class="text-wrap break-words" max-width="300px">
                                  <div v-html="props.row.activityNameDescription" />
                                </q-tooltip>
                              </q-icon>
                            </q-td>
                            <q-td style="width: 5%;">
                              <q-select
                                v-model="props.row.activityStatus.id"
                                outlined
                                stack-label
                                hide-bottom-space
                                :dense="true"
                                :options="activityStatusList"
                                class="task-activity-status-list"
                                option-value="value"
                                option-label="text"
                                emit-value
                                map-options
                                :bg-color="getStatusColor(props.row.activityStatus.dropDownValue)"
                                :disable="isClose"
                                @update:model-value="onChangeActivityStatus(props.row.id, props.row.activityStatus.id)"
                              />
                            </q-td>
                            <q-td class="text-right" style="width: 5%;">
                              {{ props.row.estimateHours }}
                            </q-td>
                          </q-tr>
                          <q-tr v-if="props.pageIndex === activityRows.length - 1">
                            <q-td colspan="5" class="text-right font-bold"><b>Total Hours:</b></q-td>
                            <q-td class="text-right"><b>{{ totalEstimateHours() }}</b></q-td>
                          </q-tr><q-separator />
                        </template>
                      </q-table>
                    </q-page>
                  </div>
                </div>
              </div>
            </div>
            <!--Tasklist-->
            <!-- <div class="col-lg-12">
          <q-card class="q-mt-md bg-primary border-blue">
            <q-card-section class="card-header items-center">
              <h1 class="text-white text-center" style="font-size: 20px;">Tasks</h1>
            </q-card-section>
          </q-card>
          <div class="row q-mt-md">
            <div class="col-lg-12">
              <q-page style="min-height: 15vh;">
                <q-table
                  ref="tableRef4" v-model:pagination="pagination5" :class="rows7.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable'" flat bordered :loading="loading" :rows="rows7" :columns="columns5" row-key="id" separator="cell"
                  binary-state-sort :rows-per-page-options="[5, 15, 30, 50, 100]" @request="getProjectTasks"
                >
                  <template #header="props">
                    <q-tr :props="props" class="text-primary">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowTaskId == props.row.id ? 'highlight' : ''" :set="(preProjectName = null, preProjectModuleName = null)">
                      <q-td style="width: 3%">#{{ props.row.projectTaskNumber }}</q-td>
                      <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;"><span v-if="preProjectName !== props.row.project.name" :set="preProjectName = props.row.project.name">{{ props.row.project.name }}</span></q-td>
                      <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 10%;"><span v-if="preProjectModuleName !== props.row.projectModule.name" :set="preProjectModuleName = props.row.projectModule.name">{{ props.row.projectModule.name }}</span></q-td>
                      <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">{{ props.row.name }}</q-td>
                      <q-td style="width: 5%;">{{ toDate(props.row.startDate) }}</q-td>
                      <q-td style="width: 5%;">{{ toDate(props.row.endDate) }}</q-td>
                      <q-td style="width: 5%;">
                        <q-badge color="red" square>{{ props.row.priority.dropDownValue }}</q-badge>
                      </q-td>
                      <q-td class="text-right" style="width: 5%;">{{ props.row.totalActivityHours }}</q-td>
                      <q-td style="width: 5%;">{{ props.row.status.dropDownValue }}</q-td>
                    </q-tr>
                    <q-tr v-if="props.pageIndex === rows7.length - 1">
                      <q-td colspan="7" class="text-right font-bold"><b>Total Hours:</b></q-td>
                      <q-td class="text-right"><b>{{ totalEstimateHours() }}</b></q-td>
                      <q-td />
                    </q-tr><q-separator />
                  </template>
                </q-table>
              </q-page>
            </div>
          </div>
        </div> -->
            <!-- <div class="row q-mt-md">
            <div class="col-lg-6 col-md-6 col-sm-12 col-xs-12">
              <q-page class="flex flex-center q-gutter-none q-pa-none" style="min-height: 15vh;">
                <q-table ref="tableRef3" class="Custom-DataTable" flat :loading="loading2" :rows="rows5" row-key="id" :pagination="pagination4" separator="cell" style="width: 100%; border: 0.5px solid #1b75ab;" @request="getProjectsAndCharterListForDashboard" no-data-label="No data available" :filter="filter">
                  <template #header="props">
                    <q-tr>
                      <q-th colspan="3">
                        <span class="flex justify-between items-center">
                          <span class="fs-14 text-grey-9">Project</span>
                        </span>
                      </q-th>
                    </q-tr>
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th style="width: 12%;">Employee Name</q-th>
                      <q-th style="width: 30%;">Role</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr v-for="(line) in props.row.projectEmployeeMappings" :key="line.id">
                      <q-td class="text-end">{{ props.row.name }}</q-td>
                      <q-td style="white-space: normal; overflow-wrap: break-word; width: 30%; max-width: 30%">{{ line.employee.person.fullName }}</q-td>
                      <q-td class="" style="white-space: normal; overflow: auto; width: 50px; max-width: 50px; word-wrap: break-word;"><div v-html="line.employeeRoleDropdown.dropDownValue"></div></q-td>
                    </q-tr>
                  </template>
                </q-table>
              </q-page>
            </div>
          </div> -->
            <div v-if="LeaveRequestRows && LeaveRequestRows.length > 0" class="col-lg-12">
              <q-card class="q-mt-md bg-primary border-blue">
                <q-card-section class="card-header items-center">
                  <h1 class="text-white text-center" style="font-size: 20px;">Leave Request</h1>
                </q-card-section>
              </q-card>
              <div v-if="leaveLoading">
                <div class="flex justify-center q-py-md">
                  <q-spinner-ios size="40px" color="grey" />
                </div>
              </div>
              <q-container class="q-mt-md">
                <div class="row q-col-gutter-md">
                  <div v-for="(leave, index) in LeaveRequestRows" :key="index" class="col-xs-12 col-sm-6 col-md-4 col-lg-3">
                    <q-card class="q-mt-md q-ml-md my-card q-mx-auto" style="box-shadow: 5px 4px 8px rgba(0, 0, 0, 0.2);">
                      <q-card-section class="bg-primary q-pt-xs q-pb-xs">
                        <div class="row items-center justify-between">
                          <div class="col-auto flex items-center">
                            <i class="q-icon text-white notranslate material-icons-outlined material-icons-outlined" aria-hidden="true" role="presentation" style="font-size: 38px;">account_circle</i>
                            <span class="text-h2 text-white employee-name q-ml-md"><strong>{{ leave.employee.person.firstName + " " + leave.employee.person.lastName }}</strong></span>
                          </div>
                          <div class="text-h2 text-white q-ml-lg">
                            <q-btn outline label="Take Action" no-caps class="text-white btnRounded q-mt-sm q-mb-sm" @click="onApprove(leave.id)" />
                          </div>
                        </div>
                      </q-card-section>
                      <q-card-section>
                        <!-- <div class="q-mb-xs"><strong>Name:</strong> {{ leave.employee.person.firstName + " " + leave.employee.person.lastName }} </div> -->
                        <div class="flex">
                          <div class="q-mb-xs q-mr-xl">
                            <strong>Leave On: </strong>
                            <span v-if="leave.noofLeaves > 1">
                              {{ leave.fromDate }} <strong>to</strong> {{ leave.toDate }}
                              <strong> ({{ leave.noofLeaves }})</strong>
                            </span>
                            <span v-else>
                              {{ leave.fromDate }} <span v-if="leave.halfDayType"> <strong>({{ leave.halfDayType }})</strong> </span>
                            </span>
                          </div>
                        <!-- <div class="q-mb-xs"><strong>To:</strong> {{ leave.toDate }} </div> -->
                        </div>
                        <!-- <div class="q-mb-xs"><strong>No. of Leave:</strong> {{ leave.noofLeaves }} </div> -->
                        <div class="q-mb-xs"><strong>Status:</strong> {{ leave.leaveStatuses.dropDownValue === 'Sent to Approver' ? 'Waiting for Approval' : leave.leaveStatuses.dropDownValue }} </div>
                        <div class="q-mb-xs RichTextEditor"><strong>Reason:</strong> <span class="fs-13" style="font-family: Poppins, sans-serif !important;" v-html="stripHTML(truncate(leave.reason, 100))" /></div>
                        <div v-if="leave.hrNote" class="q-mb-xs RichTextEditor"><strong>HR Note:</strong> <span class="fs-13" style="font-family: Poppins, sans-serif !important;" v-html="stripHTML(truncate(leave.hrNote, 100))" /></div>
                      </q-card-section>
                    </q-card>
                  </div>
                </div>
              </q-container>
            </div>
          </div>
          <div class="col-12 col-md-4 col-lg-3 col-xxl-3 flex-row-reverse-xs">
            <div class="card-header with-tools bg-white text-blue-grey-5" style="border: 1px solid #1b75ab;">
              <div class="row items-center q-py-xs q-gutter-xs flex q-ml-xs">
                <q-icon
                  v-if="movementRegDate && allDates.indexOf(movementRegDate) < allDates.length - 1"
                  name="o_arrow_back"
                  size="sm"
                  class="text-grey-9 cursor-pointer"
                  @click="changeDate('prev')"
                />
                <q-icon
                  v-if="movementRegDate
                    && allDates.indexOf(movementRegDate) > 0"
                  name="o_arrow_forward"
                  size="sm"
                  class="text-grey-9 cursor-pointer"
                  @click="changeDate('next')"
                />
                <strong class="fs-14 text-primary  q-ml-none">{{ formatMovementRegDate(movementRegDate) }}
                  <q-icon name="o_calendar_month" class="cursor-pointer  text-grey-7" size="sm">
                    <q-popup-proxy
                      ref="qDateProxy"
                      transition-show="scale"
                      transition-hide="scale"
                      @show="getMovementRegisterDateRange"
                    >
                      <q-date
                        v-model="movementRegDate"
                        :options="isMovementDateAllowed"
                        mask="MM/DD/YYYY"
                        @update:model-value="onDateSelected"
                      />
                    </q-popup-proxy>
                  </q-icon></strong>
                <strong class="fs-14 text-grey-9 q-ml-none">Movement Register
                  <q-icon
                    name="o_info"
                    size="16px"
                    class="q-ml-xs cursor-pointer text-grey-7"
                    @click.stop="showInfoTooltip = true"
                  >
                    <q-tooltip v-if="!showInfoTooltip">
                      Click to view info
                    </q-tooltip>
                    <q-tooltip
                      v-model="showInfoTooltip"
                      persistent
                      no-parent-event
                      anchor="top middle"
                      self="bottom middle"
                      max-width="420px"
                    >
                      <div class="q-pa-sm column q-gutter-sm">
                        <div class="text-body2">
                          <div class="text-red-10 text-weight-bold">
                            Important: You must read and understand all instructions before using the Movement Register.
                            Any discrepancies caused by not following these instructions will be your responsibility.
                          </div>
                          <br>
                          <b>Purpose</b>
                          <p>
                            The Movement Register is implemented to track employee attendance, availability, and presence during working hours.
                            It helps HR, Project Leads, and team members understand whether an employee is in the office or temporarily unavailable,
                            ensuring smooth coordination and accountability.
                          </p>

                          <b>When to Use</b>
                          <p>Use Movement when stepping out of the office during working hours for:</p>
                          <ul>
                            <li>Office-related work</li>
                            <li>Lunch or dinner breaks</li>
                            <li>Personal reasons</li>
                          </ul>

                          <b>How to Use</b>
                          <ul>
                            <li>Inform your Team Lead before using Movement.
                              <ul>
                                <li><i>Exception:</i> Team Lead intimation is not mandatory for a leisure break up to 15 minutes.</li>
                              </ul>
                            </li>
                            <li>Select <b>Movement</b> on the dashboard before stepping out.</li>
                            <li>Choose the correct option based on the reason:
                              <ul>
                                <li><b>Break</b> – For short breaks</li>
                                <li><b>Time Adjustment</b> – For late arrival to the office</li>
                                <li><b>WFH</b> – For work from home</li>
                              </ul>
                            </li>
                          </ul>
                        </div>
                        <div class="row justify-end">
                          <q-btn
                            outline
                            dense
                            label="Close"
                            @click.stop="showInfoTooltip = false"
                          />
                        </div>
                      </div>
                    </q-tooltip>
                  </q-icon>
                </strong>
              </div>

              <div class="row justify-end q-py-xs q-gutter-xs flex">
                <q-btn
                  icon="o_add"
                  label="Break"
                  class="bg-indigo-11 text-dark q-ml-sm"
                  no-caps
                  @click="onAddBreak()"
                >
                  <q-tooltip>
                    Add Break
                  </q-tooltip>
                </q-btn>
                <q-btn
                  icon="o_add"
                  label="Time Adjustment"
                  class="bg-purple-3 text-dark"
                  no-caps
                  @click="onAddTimeAdjustment()"
                >
                  <q-tooltip>
                    Add Time Adjustment
                  </q-tooltip>
                </q-btn>
                <q-btn
                  icon="o_add"
                  label="WFH"
                  class="bg-cyan-3 text-dark"
                  no-caps
                  @click="onAddWorkFromHome()"
                >
                  <q-tooltip>
                    Add Work From Home
                  </q-tooltip>
                </q-btn>
                <q-btn
                  outline
                  label="View All"
                  no-caps
                  class="text-primary btnRounded q-mr-sm"
                  @click="$router.push('movement-register/list')"
                />
              </div>
              <div v-if="movementRegisterLoading" class="flex justify-center">
                <q-spinner-ios size="40px" color="grey" />
              </div>
              <q-separator />
              <div
                v-if="displayRows.length === 0 && movRegDateWiseLeave.length === 0"
                class="q-pa-xs flex items-left text-grey-7"
              >
                <div>No Data Available</div>
              </div>
              <div v-for="row in displayRows" :key="row.date" class="col-12">

                <div
                  v-for="(line, i) in getVisibleRows(row)"
                  :key="i"
                  class="q-my-sm"
                  style="border:1px solid #0000001E;"
                >
                  <template v-if="line.rowType === 'MOV'">
                    <div class="row q-pa-xs items-start actions">
                      <span
                        class="Person cursor-pointer q-mr-sm"
                        :class="{
                          'bg-indigo-11 text-black': line.type.dropDownValue === 'Break',
                          'bg-purple-3 text-white': line.type.dropDownValue === 'Time Adjustment',
                          'bg-cyan-3 text-black': line.type.dropDownValue === 'Work From Home'
                        }"
                      >
                        {{ line.type.dropDownValue === 'Break'
                          ? 'B'
                          : line.type.dropDownValue === 'Time Adjustment'
                            ? 'TA'
                            : line.type.dropDownValue === 'Work From Home'
                              ? 'WFH'
                              : ''
                        }}
                        <q-tooltip>
                          {{ line.type.dropDownValue }}
                        </q-tooltip>
                      </span>
                      <div class="col">
                        <div class="text-dark">
                          {{ formatMinutesToHours(line.timeInMinutes) }}
                          <span v-if="line.timeInMinutes === 0">
                            (Cancelled)
                          </span> - {{ line.message }}
                          <span v-if="line.wfhDuration?.dropDownText" class="text-primary">
                            ({{ line.wfhDuration?.dropDownText }})
                          </span>
                          <span v-else-if="line.timeInMinutes === 480" class="text-primary">
                            (Full Day)
                          </span>
                        </div>
                        <div class="text-grey-7 text-caption text-italic">
                          {{ line.employees.person.fullName }} –
                          {{ line.createdTimeStr }}
                          <span v-if="line.siteModifiedLogCount > 0" class="q-ml-xs text-caption text-weight-bold"> Edited</span>
                        </div>
                      </div>
                      <q-icon
                        v-if="line.employeeId === storedUser.employeeId"
                        name="o_edit"
                        class="actions cursor-pointer q-ml-sm"
                        size="xs"
                        @click="onEdit(line.type.dropDownValue, row.id, line.id)"
                      >
                        <q-tooltip>Edit</q-tooltip>
                      </q-icon>
                      <q-icon
                        name="o_visibility"
                        class="actions cursor-pointer q-ml-sm"
                        size="xs"
                        @click="onView(row.id, line.id)"
                      >
                        <q-tooltip>View</q-tooltip>
                      </q-icon>
                    </div>
                  </template>
                  <template v-else>
                    <div class="row q-pa-xs items-start">
                      <span class="Person bg-red-3 text-black q-mr-sm">L
                        <q-tooltip>
                          <!-- Leave {{ line.leaveStatuses.dropDownValue === 'Sent to Approver'? 'Waiting for Approval' : line.leaveStatuses.dropDownValue }} -->
                          On Leave
                        </q-tooltip>
                      </span>

                      <div class="col">
                        <div class="row items-center q-gutter-sm">
                          <span class="text-dark">
                            {{ line.employee.person.firstName }} {{ line.employee.person.lastName }}
                          </span>

                          <span v-if="line.noofLeaves === 0.5" class="text-primary">
                            ({{ line.halfDayType }})
                          </span>
                          <q-icon v-if="line.leaveStatuses.dropDownValue === 'Approved'" name="o_check_circle" color="primary" size="xs">
                            <q-tooltip>Approved</q-tooltip>
                          </q-icon>
                          <q-icon v-else name="o_pause_circle_filled" size="xs" color="grey-9">
                            <q-tooltip>{{ line.leaveStatuses.dropDownValue === 'Sent to Approver'? 'Waiting for Approval' : line.leaveStatuses.dropDownValue }}</q-tooltip>
                          </q-icon>
                        </div>

                        <div class="text-grey-7 text-caption text-italic">
                          {{ line.reason }}
                        </div>
                      </div>
                      <div class="actions">
                        <q-icon
                          name="o_visibility"
                          class="actions cursor-pointer q-ml-sm"
                          size="xs"
                          @click="onViewEmployeeLeave(line.id)"
                        >
                          <q-tooltip>View</q-tooltip>
                        </q-icon>
                      </div>
                    </div>
                  </template>
                </div>
                <!-- <div v-if="row.movementRegisterDetails.length > 15" class="text-center">
                  <q-btn
                    flat
                    no-caps
                    color="primary"
                    :label="row.showAll ? 'View Less' : 'View More'"
                    @click="toggleShow(row)"
                  />
                </div> -->
                <div
                  v-if="getCombinedRows(row).length > 15"
                  class="text-center"
                >
                  <q-btn
                    flat
                    no-caps
                    color="primary"
                    :label="row.showAll ? 'View Less' : 'View More'"
                    @click="toggleShow(row)"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div></q-page>
</template>

<script setup>
import { ref, onMounted, computed, onBeforeUnmount } from "vue";
import { date, useQuasar } from "quasar";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";
import { notifySuccess, getLocalStorage } from "assets/utils";
import _ from "lodash";
import { format } from "date-fns"; // Standard TimeZone Conversion

import dailyplannerService from "modules/my-daily-planner/myDailyPlanner.service";
import timesheetService from "modules/timesheet/timesheet.service";
import leaveService from "modules/leave/leave.service";
import eventLeaveService from "modules/leave-yearly-schedule/leaveYearlySchedule.service";
import projectService from "modules/project/projects.service";
import projectActivitiesService from "modules/project-tasks-activities/projectTasksActivities.service";
import commonService from "services/common.service";
import timeInTimeOutService from "modules/timeInTimeOut/timeInTimeOut.service";
import approveLeave from "modules/leave/components/_approveOrDeclineLeave.vue";
import movementRegisterService from "src/modules/movementRegister/movementRegister.service";
import moduleService from "modules/module/module.service";

import viewMovementRegister from "src/modules/movementRegister/components/view.vue";
import editBreak from "src/modules/movementRegister/components/_addBreak.vue";
import editTimeAdjustment from "src/modules/movementRegister/components/_addTimeAdjustment.vue";
import editWorkFromHome from "src/modules/movementRegister/components/_addWorkFromHome.vue";
import viewEmployeeLeave from "modules/leave/components/view.vue";

const { truncate, stripHTML, toDate } = useFilters();
const authStore = useAuthStore();
const user = authStore.user;
const storedUser = getLocalStorage("user");
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

const dailyPlannerLoading = ref(false);
const timesheetLoading = ref(false);
const timesheetWeekHoursLoading = ref(false);
const projectLoading = ref(false);
const processing = ref(false);
const loading = ref(true);
const leaveLoading = ref(false);
const taskActivityLoading = ref(false);
const movementRegisterLoading = ref(false);
const movementRegisterRows = ref([]);
const dailyPlannerRows = ref([]);
const timesheetRows = ref([]);
const LeaveRequestRows = ref([]);
const onLeaveRows = ref([]);
const rows = ref([]);
const upcomingHolidayRows = ref([]);
const dailyPlannerTableRef = ref();
const timesheetTableRef = ref();
const movRegDateWiseLeave = ref([]);
const showInfoTooltip = ref(false);
const quickLinkRows = ref([]);

const dailyPlannerPagination = ref({ page1: 1, dailyPlannerRowsPerPage: 2, descending: false, sortBy: "createdOnUtc" });
const timesheetPagination = ref({ page2: 1, timesheetRowsPerPage: 2, descending: false, sortBy: "createdOnUtc" });
const employeeLeavePagination = ref({ page3: 1, employeeLeaveRowsPerPage: 5, descending: false, sortBy: "createdOnUtc" });
const projectPagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });

const $q = useQuasar();
const activeRowId = ref(null);
let movementInterval = null;
const search = ref({
  createdBy: "Created By Me"
});

const activityStatusIds = [];
const activitySearch = ref({
  activityStatusIds
});

const model = ref({
  timeIn: "",
  timeOut: "",
  timeInDate: "",
  timeOutDate: ""
});

const projectTableRef = ref();
const projectRows = ref([]);
const projectColumns = ref([
  { name: "name", label: "Project Name", field: "name", align: "center", sortable: true }
]);

const activityTableRef = ref();
const activityRows = ref([]);
const activityColumns = ref([
  { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true },
  { name: "projectModule.name", label: "Project Module", field: "projectModule.name", align: "left", sortable: true },
  { name: "task.name", label: "Task Name", field: "task.name", align: "left", sortable: true },
  { name: "name", label: "Activity Name", field: "name", align: "left", sortable: true },
  { name: "activityStatus.dropDownValue", label: "Activity Status", field: "activityStatus.dropDownValue", align: "left", sortable: true },
  { name: "estimateHours", label: "Est. Hrs", field: "estimateHours", align: "right", sortable: true }
]);

const movementRegDate = ref(null);
// Computed: all dates in descending order
const allDates = computed(() => {
  if (!dateRange.value.start || !dateRange.value.end) return [];

  const dates = [];
  const dayMs = 24 * 60 * 60 * 1000; // milliseconds in a day
  let currentTime = dateRange.value.end.getTime(); // start from end date
  const startTime = dateRange.value.start.getTime();

  while (currentTime >= startTime) {
    const date = new Date(currentTime);
    // Format MM/DD/YYYY
    const formatted = `${(date.getMonth() + 1)
      .toString()
      .padStart(2, "0")}/${date.getDate().toString().padStart(2, "0")}/${date.getFullYear()}`;
    dates.push(formatted);
    currentTime -= dayMs;
  }
  const todayFormatted = toDate(new Date());
  const endDateFormatted = toDate(dateRange.value.end);

  if (todayFormatted !== endDateFormatted) {
    dates.unshift(todayFormatted);
  }
  return dates;
});

const formatMovementRegDate = (date) => {
  const d = new Date(date);
  return `${String(d.getMonth() + 1).padStart(2, "0")}/${String(
    d.getDate()
  ).padStart(2, "0")}/${d.getFullYear()}`;
};

// Fetch movement registers
const getMovementRegisters = async (showLoader = true) => {
  if (showLoader) movementRegisterLoading.value = true;
  try {
    const today = toDate(new Date());
    if (!movementRegDate.value) {
      movementRegDate.value = today;
    }
    const isViewMore = localStorage.getItem("showMore") === "true";
    const filters = {
      ...search.value,
      fromDate: movementRegDate.value,
      ToDate: movementRegDate.value,
      isViewMore
    };
    const resp = await movementRegisterService.getMovementRegistersForDashboard(filters);
    movementRegisterRows.value = (resp.moveRegisterList || []).map(row => ({
      ...row,
      showAll: isViewMore
    }));
  } finally {
    if (showLoader) movementRegisterLoading.value = false;
  }
};

// Filter rows by selected date
const filteredRows = computed(() =>
  movementRegisterRows.value.filter(r => r.date === movementRegDate.value)
);

const displayRows = computed(() => {
  if (filteredRows.value.length > 0) {
    return filteredRows.value;
  }

  if (movRegDateWiseLeave.value.length > 0) {
    return [
      {
        id: "leave-only",
        date: movementRegDate.value,
        movementRegisterDetails: [],
        showAll: false
      }
    ];
  }

  return [];
});

// Arrow navigation
const changeDate = (direction) => {
  const idx = allDates.value.indexOf(movementRegDate.value);
  if (direction === "prev" && idx < allDates.value.length - 1) movementRegDate.value = allDates.value[idx + 1];
  else if (direction === "next" && idx > 0) movementRegDate.value = allDates.value[idx - 1];
  getMovementRegisters();
  getEmployeeLeaveListForMovReg();
};

const timesheetWeekHours = ref({ columns: [], rows: [] });
const timesheetMonthHours = ref({ columns: [], rows: [] });
const formatDate = (d) => date.formatDate(d, "ddd MM/DD");
const getTimesheetTotalHoursByWeekAndMonth = (viewType) => {
  // const { page, rowsPerPage, sortBy, descending } = props.pagination;
  timesheetWeekHoursLoading.value = true;
  search.value.viewType = viewType;
  const payload = { ...search.value };
  timesheetService.getTimesheetTotalHoursByWeekAndMonth(payload).then((resp) => {
    if (viewType === "weekly") {
      timesheetWeekHours.value = resp;
    } else if (viewType === "monthly") {
      timesheetMonthHours.value = resp;
    }
  }).finally(() => {
    timesheetWeekHoursLoading.value = false;
  });
};

const getProjects = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  projectLoading.value = true;
  projectService.getAllProjectListForDropdown().then((resp) => {
    projectRows.value = resp;
    projectPagination.value.page = page;
    projectPagination.value.rowsPerPage = rowsPerPage;
    projectPagination.value.sortBy = sortBy;
    projectPagination.value.descending = descending;
    projectPagination.value.rowsNumber = resp.total;
  }).finally(() => {
    projectLoading.value = false;
  });
};

const activityPagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const getProjectActivities = async (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  taskActivityLoading.value = true;
  await getAllActivityStatusListForDropDown("Activity Status");
  activitySearch.value.flag = "DS";
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...activitySearch.value };
  projectActivitiesService.getAllProjectActivities(payload).then((resp) => {
    activityRows.value = resp.data;
    activityRows.value = resp.data.map(activity => {
      const hasFullAccess = activity?.project?.projectUserMappings[0]?.fullAccess ?? false;
      return {
        ...activity,
        isNotes: activity?.project?.projectUserMappings[0]?.notes ?? false,
        isEditable: role === "admin" || hasFullAccess
      };
    });
    activityPagination.value.page = page;
    activityPagination.value.rowsPerPage = rowsPerPage;
    activityPagination.value.sortBy = sortBy;
    activityPagination.value.descending = descending;
    activityPagination.value.rowsNumber = resp.total;
  }).finally(() => {
    taskActivityLoading.value = false;
  });
};

// get daily planner
const getDailyPlanners = (props) => {
  const { page1, dailyPlannerRowsPerPage } = props.dailyPlannerPagination || {};
  dailyPlannerLoading.value = true;
  const payload = { page1, pageSize: dailyPlannerRowsPerPage, ...search.value };
  dailyplannerService.getDailyPlannersForDashboard(payload).then((resp) => {
    dailyPlannerRows.value = resp.data;
    dailyPlannerPagination.value.page1 = page1;
    dailyPlannerPagination.value.dailyPlannerRowsPerPage = dailyPlannerRowsPerPage;
  }).finally(() => {
    dailyPlannerLoading.value = false;
  });
};

// Get/Map timesheet list to table
const getTimesheets = (props) => {
  const { page2, timesheetRowsPerPage } = props.timesheetPagination || {};
  timesheetLoading.value = true;
  const payload = { page2, pageSize: timesheetRowsPerPage, ...search.value };
  timesheetService.getTimesheetsForDashboard(payload).then((resp) => {
    timesheetRows.value = resp.data;
    timesheetPagination.value.page2 = page2;
    timesheetPagination.value.timesheetRowsPerPage = timesheetRowsPerPage;
  }).finally(() => {
    timesheetLoading.value = false;
  });
};

// Get/Map forward leaves list to table
const getFiveEmployeeLeaveForApprove = () => {
  try {
    leaveLoading.value = true;
    leaveService.getFiveEmployeeLeaveForApprove().then((resp) => {
      LeaveRequestRows.value = resp;
    });
  } catch (error) {
    console.error("Error loading leave request:", error);
  } finally {
    setTimeout(() => {
      leaveLoading.value = false;
    }, 1500);
  }
};

// Get/Map forward leaves list to table
const getEmployeeLeaveListForDashboard = () => {
  loading.value = true;
  leaveService.getEmployeeLeaveListForDashboard().then((resp) => {
    onLeaveRows.value = resp;
  }).finally(() => {
    loading.value = false;
  });
};

const getEmployeeLeaveListForMovReg = () => {
  loading.value = true;
  const today = toDate(new Date());
  // console.log(today);
  if (!movementRegDate.value) {
    movementRegDate.value = today;
  }
  leaveService.getEmployeeLeaveListForMovReg(movementRegDate.value).then((resp) => {
    movRegDateWiseLeave.value = resp;
  }).finally(() => {
    loading.value = false;
  });
};

// Get/Map yearly leaves list
const getYearlyLeaveListForDashboard = () => {
  loading.value = true;
  eventLeaveService.getYearlyLeaveListForDashboard().then((resp) => {
    upcomingHolidayRows.value = resp;
  }).finally(() => {
    loading.value = false;
  });
};

// get quick links
const getAllModuleMenusForDashboard = () => {
  loading.value = true;
  moduleService.getAllModuleMenusForDashboard().then((resp) => {
    quickLinkRows.value = resp;
    // console.log("quickLinkRows-resp", resp);
    // console.log("quickLinkRows", quickLinkRows.value);
  }).finally(() => {
    loading.value = false;
  });
};

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
    case "New":
      return "blue-4";
    default:
      return "#ffffff";
    }
  }
}
// Get all activity status list
const activityStatusList = ref([]);
function getAllActivityStatusListForDropDown (typeName) {
  return commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp
      .map((item) => ({ text: item.dropdownValue, value: item.id }));
    activityStatusList.value = responseData
      .filter(item => item.text.toLowerCase() !== "new"
      );
    activitySearch.value.activityStatusIds = [
      activityStatusList.value.find(
        status => status.text?.toLowerCase() === "open"
      )?.value
    ].filter(Boolean);
  });
}
// onChangeActivityStatus
function onChangeActivityStatus (id, activityStatusId) {
  const payload = {
    activityIds: [id],
    activityStatusId
  };
  setTimeout(function () {
    projectActivitiesService.updateTaskActivityStatus(payload).then(resp => {
      notifySuccess({ message: "Activity status is saved successfully." });
      getProjectActivities({ pagination: activityPagination.value });
    });
  });
}
// View leaves details popup
const onApprove = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: approveLeave,
    componentProps: { id }
  }).onOk(() => {
    getFiveEmployeeLeaveForApprove({ employeeLeavePagination: employeeLeavePagination.value });
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};
function totalEstimateHours () {
  const total = activityRows.value.reduce((total, row) => total + (row.estimateHours || 0), 0);
  return total.toFixed(2);
}

function getTotalHoursByProject (lines) {
  const projectMap = {};
  for (const line of lines) {
    const name = line.project?.name;
    projectMap[name] = (projectMap[name] || 0) + (line.hours || 0);
  }

  return Object.entries(projectMap).map(([name, total]) => ({
    projectName: name,
    totalHours: parseFloat(total.toFixed(2)).toString()
  }));
}

// Fetch Time In / Time Out for a specific employee
const getTimeInTimeOutDetailsByEmployeeId = (employeeId) => {
  timeInTimeOutService.getTimeInTimeOutDetailsByEmployeeId(employeeId)
    .then((resp) => {
      model.value = _.cloneDeep(resp);
    }).finally(() => {
      loading.value = false;
    });
};

async function onSubmitTimeInOut (isTimeOutAction = false) {
  const id = isTimeOutAction ? model.value.id : null;
  if (isTimeOutAction) {
    $q.dialog({
      title: "Confirmation",
      message: "Are you sure you want to Time Out?",
      ok: { label: "Yes", color: "primary" },
      cancel: { label: "No", color: "negative" }
    }).onOk(() => save(id, isTimeOutAction));
  } else {
    save(id, isTimeOutAction);
  }
}
async function save (id, isTimeOutAction) {
  try {
    processing.value = true;
    await timeInTimeOutService.saveTimeInTimeOut(id);
    // if (!isTimeOutAction) {
    //   // Store time when user clicks Time In and it is saved
    //   localStorage.setItem("timeIn", new Date().getTime());
    // } else {
    //   // Remove time when user Time Out
    //   localStorage.removeItem("timeIn");
    // }
    notifySuccess({
      message: isTimeOutAction
        ? "Time Out saved successfully."
        : "Time In saved successfully."
    });

    getTimeInTimeOutDetailsByEmployeeId(storedUser.employeeId);
  } catch (error) {
    console.error(error);
  } finally {
    setTimeout(() => (processing.value = false), 1000);
  }
}

const employeeStatus = computed(() =>
  model.value.timeInDate && !model.value.timeOutDate ? "IN" : "OUT"
);

const formatMinutesToHours = (minutes) => {
  if (minutes === 0) return "0 mins";
  if (!minutes || isNaN(minutes)) return "";
  const hrs = Math.floor(minutes / 60);
  const mins = minutes % 60;
  const hrText = hrs > 0 ? `${hrs} hr${hrs > 1 ? "s" : ""}` : "";
  const minText = mins > 0 ? `${mins} min` : "";
  return [hrText, minText].filter(Boolean).join(" ");
};

const dateRange = ref({ start: null, end: null });

const getMovementRegisterDateRange = async () => {
  const { startDate, endDate } = await movementRegisterService.getMovementRegisterDateRange();
  dateRange.value.start = startDate ? new Date(startDate) : format(new Date(), "MM/dd/yyyy");
  dateRange.value.end = endDate ? new Date(endDate) : format(new Date(), "MM/dd/yyyy");
  const endDateFormatted = toDate(dateRange.value.end); // movement register end date
  const selectedDateFormatted = toDate(movementRegDate.value);
  if (selectedDateFormatted !== endDateFormatted) {
    const endDateExists = movementRegisterRows.value?.some(
      row => row.date === endDateFormatted
    );
    if (!endDateExists) {
      movementRegisterRows.value.unshift({
        date: endDateFormatted,
        movementRegisterDetails: [] // empty array since no data
      });
    }
  }
};

const qDateProxy = ref(null);
const onDateSelected = async () => {
  if (qDateProxy.value) qDateProxy.value.hide();
  await getMovementRegisters();
  await getEmployeeLeaveListForMovReg();
};

const isMovementDateAllowed = (date) => {
  const day = new Date(date);
  if (
    day.getFullYear() === new Date().getFullYear() &&
    day.getMonth() === new Date().getMonth() &&
    day.getDate() === new Date().getDate()
  ) {
    return true;
  }
  if (dateRange.value.start && dateRange.value.end) {
    return day >= dateRange.value.start && day <= dateRange.value.end;
  }
  return false;
};

const onViewEmployeeLeave = (id) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewEmployeeLeave,
    componentProps: { id, leaveId: id }
  }).onOk(() => {
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = null;
  });
};

const getCombinedRows = (row) => {
  const mov = row.movementRegisterDetails.map(m => ({
    ...m,
    rowType: "MOV"
  }));

  const leaves = movRegDateWiseLeave.value.map(l => ({
    ...l,
    rowType: "LEAVE"
  }));

  return [...mov, ...leaves];
};

const getVisibleRows = (row) => {
  const combined = getCombinedRows(row);
  return row.showAll ? combined : combined.slice(0, 15);
};

const toggleShow = (row) => {
  localStorage.setItem(
    "showMore",
    localStorage.getItem("showMore") !== "true"
  );
  row.showAll = !row.showAll;

  // Call API ONLY if movement register itself is more than 15
  if (
    row.showAll &&
    row.movementRegisterDetails.length > 15 &&
    !row.isLoadedMore
  ) {
    row.isLoadedMore = true;
    getMovementRegisters(row.date);
  }
};

// View popup
const onView = (id, detailId) => {
  activeRowId.value = id;
  $q.dialog({
    component: viewMovementRegister,
    componentProps: { id, detailId }
  }).onOk(() => {
    getMovementRegisters();
  }).onCancel(() => {
  }).onDismiss(() => {
    activeRowId.value = id;
  });
};

// Edit Break popup
const onEdit = (buttonType, id, detailId) => {
  const component =
    buttonType === "Break" ? editBreak
      : buttonType === "Time Adjustment" ? editTimeAdjustment
        : editWorkFromHome;

  $q.dialog({
    component,
    componentProps: { id, detailId }
  }).onOk(() => {
    getMovementRegisters();
  });
};

// Add Break popup
const onAddBreak = () => {
  $q.dialog({
    component: editBreak,
    componentProps: { }
  }).onOk(() => {
    getMovementRegisters();
  }).onCancel(() => {
  }).onDismiss(() => {
    // activeRowId.value = id;
  });
};

// Add Time Adjustment popup
const onAddTimeAdjustment = () => {
  $q.dialog({
    component: editTimeAdjustment,
    componentProps: { }
  }).onOk(() => {
    getMovementRegisters();
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Add Work From Home popup
const onAddWorkFromHome = () => {
  $q.dialog({
    component: editWorkFromHome,
    componentProps: { }
  }).onOk(() => {
    getMovementRegisters();
  }).onCancel(() => {
  }).onDismiss(() => {
    // activeRowId.value = id;
  });
};

onMounted(() => {
  // timeInOutTableRef.value.requestServerInteraction();
  dailyPlannerTableRef.value.requestServerInteraction();
  timesheetTableRef.value.requestServerInteraction();
  projectTableRef.value.requestServerInteraction();
  activityTableRef.value.requestServerInteraction();
  getFiveEmployeeLeaveForApprove();
  getEmployeeLeaveListForDashboard();
  getYearlyLeaveListForDashboard();
  getAllModuleMenusForDashboard();
  getAllActivityStatusListForDropDown("Activity Status");
  getTimesheetTotalHoursByWeekAndMonth("weekly");
  getTimesheetTotalHoursByWeekAndMonth("monthly");
  getTimeInTimeOutDetailsByEmployeeId(storedUser.employeeId);
  getMovementRegisterDateRange();
  getMovementRegisters();
  getEmployeeLeaveListForMovReg();
  movementInterval = setInterval(() => {
    getMovementRegisters(false);
  }, 20000);
});

onBeforeUnmount(() => {
  clearInterval(movementInterval);
});

</script>
<style>
.leaveCard .q-card{
  height: 100%;
  width: 100%;
}

.Person {
    border-radius: 50%;
    background-color: #5d5d5d;
    color: white;
    font-size: 12px;
    font-weight: 600;
    padding: 2px 7px;
    transition: 0.5s all ease-in-out;
    text-align: center;
    width: auto;
}

.actions i{
    font-size: 18px;
    border: 1px solid var(--q-secondary);
    padding: 3px;
    border-radius: 5px;
  }
  .actions i:hover{
    border-color: var(--q-primary);
    color: white;
    background-color: var(--q-primary);
  }
  .actions i.text-negative:hover {
    border-color: var(--q-negative);
    color: white !important;
    background-color: var(--q-negative);
  }
</style>
