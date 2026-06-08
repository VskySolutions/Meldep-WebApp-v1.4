<!-- eslint-disable vue/no-v-html -->
<template>
  <q-page padding>
    <div class="row q-col-gutter-x-md">
      <div class="col">
        <q-card class="project6">
          <q-card-section class="card-header with-tools flex justify-between items-center advSearch">
            <div class="flex items-center">
              <q-breadcrumbs class="text-brown text-weight-bold text-h3">
                <template #separator>
                  <q-icon size="1.5em" name="o_chevron_right" color="primary" />
                </template>
                <q-breadcrumbs-el label="Dashboard" icon="o_home" clickable to="/dashboard" />
                <q-breadcrumbs-el label="All Project Planner" />
              </q-breadcrumbs>
            </div>
            <div>
              <h1 v-if="storedCustomerName || storedProjectName" class="q-pa-sm" style="border: 1px solid #1b75ab;">
                <span v-if="storedCustomerName">{{ storedCustomerName }}- </span>
                <span v-if="storedProjectName">{{ storedProjectName }}</span>
              </h1>
            </div>
            <div>
              <q-btn icon="o_event" size="sm" outline label="" color="primary" class="q-mr-xs" @click="$router.push({ path: `/project-planning/calendar`, state: {flag: 'all_proj_calendar', projectId: selectedProjectId, projectName: storedProjectName } })">
                <q-tooltip anchor="bottom middle" self="top middle">All Project Task Calendar</q-tooltip>
              </q-btn>
              <q-btn icon="o_visibility" size="sm" :disable="!selectedProjectId" outline no-caps color="primary" class="q-mr-xs" label="" @click="onProjectView(selectedProjectId)">
                <q-tooltip anchor="bottom middle" self="top middle">View Project</q-tooltip>
              </q-btn>
              <q-btn v-if="getProjectDetail(selectedProjectId)?.isEditable" icon="o_attach_file" size="sm" :disable="!selectedProjectId" outline no-caps color="primary" class="q-mr-xs" label="" @click="onProjectFilesView(selectedProjectId, storedProjectName, refreshProjectList)">
                <q-tooltip anchor="bottom middle" self="top middle">Project Files</q-tooltip>
              </q-btn>
              <q-btn v-if="getProjectDetail(selectedProjectId)?.isEditable || getProjectDetail(selectedProjectId)?.isNotes" icon="o_chat" size="sm" :disable="!selectedProjectId" outline no-caps color="primary" class="q-mr-xs" label="" style="position: relative;" @click="onProjectMessage(selectedProjectId)">
                <q-badge
                  v-if="selectedProjectId && getProjectDetail(selectedProjectId)?.projectMessageCount > 0"
                  style="position: absolute;right: -3px;top: -11px;"
                  color="green"
                  text-color="white"
                  :label="getProjectDetail(selectedProjectId)?.projectMessageCount"
                />
                <q-tooltip anchor="bottom middle" self="top middle">Project Messages</q-tooltip>
              </q-btn>
              <q-btn v-if="getProjectDetail(selectedProjectId)?.isEditable || getProjectDetail(selectedProjectId)?.isNotes" icon="o_assignment" size="sm" :disable="!selectedProjectId" outline no-caps color="primary" class="q-mr-xs" label="" style="position: relative;" @click="onNoteAdd(selectedProjectId, 'Projects', selectedProjectId, '', '')">
                <q-badge
                  v-if="selectedProjectId && getProjectDetail(selectedProjectId)?.projectNoteCount > 0"
                  style="position: absolute;right: -3px;top: -11px;"
                  color="green"
                  text-color="white"
                  :label="getProjectDetail(selectedProjectId)?.projectNoteCount"
                />
                <q-tooltip anchor="bottom middle" self="top middle">Project Notes</q-tooltip>
              </q-btn>
              <q-btn style="" icon="o_refresh" size="sm" outline color="primary" label="" class="q-mr-xs" no-caps @click="onClear">
                <q-tooltip anchor="bottom middle" self="top middle">Reset Filter</q-tooltip>
              </q-btn>
              <q-btn icon="o_chevron_left" size="sm" outline label="Back" no-caps class="text-primary no-space-between" @click="$router.back()">
                <q-tooltip anchor="bottom middle" self="top middle">Back To Dashboard</q-tooltip>
              </q-btn>
            </div>
          </q-card-section>
          <q-separator />
        </q-card>
      </div>
    </div>

    <q-card class="project6">
      <q-card-section class="card-header with-tools">
        <q-layout view="hHh lpR fFf">
          <q-page-container>
            <div v-if="!leftDrawerOpen">
              <q-btn size="sm" style="padding: 3px 7px; min-height: 30px;" outline icon="o_menu" @click="toggleLeftDrawer" />
            </div>
            <q-splitter v-model="customerSplit" :limits="!leftDrawerOpen ? [0, 0] : [10, 90]">
              <template #before>
                <div v-show="leftDrawerOpen" class="q-pa-none">
                  <q-toolbar class="bg-grey-2 column q-pa-xs" style="align-items:start !important">
                    <div class="row items-center justify-between q-gutter-sm full-width q-mb-sm">
                      <q-toolbar-title class="q-pa-none">
                        <h3 class="text-black q-mb-none">Projects</h3>
                      </q-toolbar-title>

                      <div class="row items-center q-gutter-sm">
                        <q-btn
                          icon="o_add"
                          outline
                          size="sm"
                          no-caps
                          class="text-primary"
                          style="padding: 3px 7px; min-height: 30px;"
                          @click="onProjectAdd(refreshProjectList, refreshProjectNameDropdown)"
                        >
                          <q-tooltip>Add Project</q-tooltip>
                        </q-btn>

                        <q-btn
                          icon="o_open_in_new"
                          size="sm"
                          outline
                          class="text-primary"
                          style="padding: 3px 7px; min-height: 30px;"
                          @click="$router.push('/project')"
                        >
                          <q-tooltip>Manage Projects</q-tooltip>
                        </q-btn>

                        <q-btn size="sm" style="padding: 3px 7px; min-height: 30px;" outline icon="o_menu" @click="leftDrawer">
                          <q-tooltip>Hide Project</q-tooltip>
                        </q-btn>
                      </div>
                    </div>

                    <div class="row items-center q-mr-xs">
                      <div class="search-container position-relative">
                        <searchFilterBar
                          v-model="search.searchText"
                          :loading="searchProjectLoader"
                          :applied-filters="appliedProjectFiltersCount"
                          class="search-bar"
                          @toggle-filter="showFilter = !showFilter"
                        />
                        <!-- Advanced Filter Menu -->
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
                              :options="statusListForSearch"
                              :filter="getProjectStatusFilter"
                              :isShowAll="true"
                            />
                            <singleSelectDropdown
                              v-model="search.statusId"
                              label="Active/Inactive"
                              :options="projectActiveInActiveDropdown.list.value"
                            />
                            <multiSelectDropdown
                              v-model="search.projectTypeIds"
                              label="Type"
                              :options="projectTypesDropdown.list.value"
                              :filter="projectTypesDropdown.filter"
                              :isShowAll="true"
                            />
                            <div class="row items-center q-mb-sm">
                              <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                                <label class="Cutomlabel q-mt-sm fs-13">Year</label>
                              </div>
                              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                                <div class="input-group q-mx-sm w-100 h-auto">
                                  <q-input v-model="search.year" fill-input dense mask="####" clearable stack-label @clear="onClearYear">
                                    <template #append>
                                      <q-icon name="o_calendar_month" class="cursor-pointer">
                                        <q-popup-proxy ref="qDateProxy" v-model="isPopupVisible" transition-show="scale" transition-hide="scale">
                                          <q-date ref="date3ref" v-model="search.year" default-view="Years" emit-immediately minimal mask="YYYY" class="myDate" :options="onlyYears" @update:model-value="onUpdateMv2" />
                                        </q-popup-proxy>
                                      </q-icon>
                                    </template>
                                  </q-input>
                                </div>
                              </div>
                            </div>
                            <!-- Search and Clear Buttons -->
                            <div class="row justify-end q-gutter-sm q-mb-sm">
                              <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showFilter = false; onSearch(); }" />
                              <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClearProject" />
                              <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showFilter = false; }" />
                            </div>
                          </q-card>
                        </q-menu>
                      </div>
                      <div class="q-ml-sm">
                        <!-- Replace your current sort menu with this -->
                        <q-btn icon="o_swap_vert" dense outline round no-caps class="text-primary btnRounded" size="sm">
                          <q-tooltip>
                            Sorted: By
                            {{ selectedSortByProject === 'startDate'
                              ? ' Start Date'
                              : selectedSortByProject === 'dueDate'
                                ? ' Due Date'
                                : selectedSortByProject === 'name'
                                  ? ' Name'
                                  : selectedSortByProject === 'status'
                                    ? ' Status'
                                    : selectedSortByProject === 'customerName'
                                      ? ' Customer'
                                      : selectedSortByProject === 'priority'
                                        ? ' Priority'
                                        : selectedSortByProject === 'projectTypeName'
                                          ? ' Type'
                                          : selectedSortByProject === 'createdOnUtc'
                                            ? ' Created Date'
                                            : selectedSortByProject
                            }} ({{ selectedSortOrderByProject ? 'Descending' : 'Ascending' }})</q-tooltip>
                          <q-menu>
                            <q-list style="min-width: 200px">
                              <q-separator />
                              <q-item-label header class="q-py-xs " style="font-size: 0.75rem;font-weight:500">Sort By</q-item-label>
                              <q-item>
                                <q-item-section>
                                  <q-radio
                                    v-model="selectedSortByProject"
                                    style="font-size:13px"
                                    size="xs"
                                    val="customerName"
                                    label="Customer"
                                    checked-icon="o_task_alt"
                                    dense
                                    @update:model-value="val => onSortByProject(val, false)"
                                  />
                                </q-item-section>
                              </q-item>
                              <q-item>
                                <q-item-section>
                                  <q-radio
                                    v-model="selectedSortByProject"
                                    style="font-size:13px"
                                    size="xs"
                                    val="name"
                                    label="Name"
                                    checked-icon="o_task_alt"
                                    dense
                                    @update:model-value="val => onSortByProject(val, false)"
                                  />
                                </q-item-section>
                              </q-item>
                              <q-item>
                                <q-item-section>
                                  <q-radio
                                    v-model="selectedSortByProject"
                                    style="font-size:13px"
                                    size="xs"
                                    val="startDate"
                                    label="Start Date"
                                    checked-icon="o_task_alt"
                                    dense
                                    @update:model-value="val => onSortByProject(val, false)"
                                  />
                                </q-item-section>
                              </q-item>
                              <q-item>
                                <q-item-section>
                                  <q-radio
                                    v-model="selectedSortByProject"
                                    style="font-size:13px"
                                    size="xs"
                                    val="endDate"
                                    label="End Date"
                                    checked-icon="o_task_alt"
                                    dense
                                    @update:model-value="val => onSortByProject(val, false)"
                                  />
                                </q-item-section>
                              </q-item>
                              <q-item>
                                <q-item-section>
                                  <q-radio
                                    v-model="selectedSortByProject"
                                    style="font-size:13px"
                                    size="xs"
                                    val="status"
                                    label="Status"
                                    checked-icon="o_task_alt"
                                    dense
                                    @update:model-value="val => onSortByProject(val, false)"
                                  />
                                </q-item-section>
                              </q-item>
                              <q-item>
                                <q-item-section>
                                  <q-radio
                                    v-model="selectedSortByProject"
                                    style="font-size:13px"
                                    size="xs"
                                    val="priority"
                                    label="Priority"
                                    checked-icon="o_task_alt"
                                    dense
                                    @update:model-value="val => onSortByProject(val, false)"
                                  />
                                </q-item-section>
                              </q-item>
                              <q-item>
                                <q-item-section>
                                  <q-radio
                                    v-model="selectedSortByProject"
                                    style="font-size:13px"
                                    size="xs"
                                    val="projectTypeName"
                                    label="Type"
                                    checked-icon="o_task_alt"
                                    dense
                                    @update:model-value="val => onSortByProject(val, false)"
                                  />
                                </q-item-section>
                              </q-item>
                              <q-item>
                                <q-item-section>
                                  <q-radio
                                    v-model="selectedSortByProject"
                                    style="font-size:13px"
                                    size="xs"
                                    val="createdOnUtc"
                                    label="Created Date"
                                    checked-icon="o_task_alt"
                                    dense
                                    @update:model-value="val => onSortByProject(val, false)"
                                  />
                                </q-item-section>
                              </q-item>
                              <q-separator />
                              <q-item-label header class="q-py-xs " style="font-size: 0.75rem;font-weight:500;">Sort Order</q-item-label>
                              <q-item>
                                <q-item-section>
                                  <q-radio
                                    v-model="selectedSortOrderByProject"
                                    style="font-size:13px"
                                    size="xs"
                                    :val="false"
                                    label="Ascending"
                                    checked-icon="o_task_alt"
                                    dense
                                    @update:model-value="val => onSortByProject(val, true)"
                                  />
                                </q-item-section>
                              </q-item>
                              <q-item>
                                <q-item-section>
                                  <q-radio
                                    v-model="selectedSortOrderByProject"
                                    style="font-size:13px"
                                    size="xs"
                                    :val="true"
                                    label="Descending"
                                    checked-icon="o_task_alt"
                                    dense
                                    @update:model-value="val => onSortByProject(val, true)"
                                  />
                                </q-item-section>
                              </q-item>
                            </q-list>
                          </q-menu>
                        </q-btn>
                      </div>
                    </div>
                  </q-toolbar>
                  <q-table
                    ref="tableRef"
                    v-model:pagination="pagination"
                    :loading="loading"
                    :rows="filteredProjectRows"
                    :columns="columns"
                    row-key="id"
                    separator="cell"
                    binary-state-sort
                    class="Custom-DataTable"
                    no-data-label="No project available"
                    table-style="max-height: 65vh"
                    virtual-scroll
                    :rows-per-page-options="[20, 50, 100, 200, 500]"
                    @request="getAllCustomerProjectsList"
                  >
                    <template #header="props">
                      <q-tr :props="props" class="bg-primary text-white">
                        <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                        <q-th auto-width class="text-center my-sticky-last-column-table" sticky />
                      </q-tr>
                    </template>
                    <template #body="props">
                      <q-tr :props="props" :class="['cursor-pointer', activeRowId == props.row.id ? 'bg-green-2' : '', props.row.isPinned ? 'bg-yellow-1' : '' ]" :set="preClientName == null">
                        <q-td class="highlight-text" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal !important; width: 30%;" @click="() => { selectProject(props.row, props.row); loadProjectModules(props.row.id, 'module'); }">
                          <div :style="'border-left: 5px solid ' + (props.row.projectColor ? props.row.projectColor : 'transparent') + '; width: 5px;height: 97%;position: absolute; left: -1px;top: 0px;'">
                            <q-tooltip>Project Color</q-tooltip>
                          </div>
                          <q-icon v-if="props.row.isPinned" name="o_push_pin" size="xs" class="q-mr-xs">
                            <q-tooltip>Project Pinned</q-tooltip>
                          </q-icon>
                          <span v-if="props.row.showCustomerName" class="hoverable-cell">
                            {{ props.row.customerName }}
                          </span>
                          <!-- <span v-if="!props.index || props.row.customerName !== filteredProjectRows[props.index - 1].customerName">
                            {{ props.props.index}}
                          </span> -->
                          <!-- <span v-if="preClientName !== props.row.customerName " :set="preClientName = props.row.customerName" class="hoverable-cell">{{ props.row.customerName }}</span> -->
                          <!-- <span v-if="!props.row.customerId">
                            <q-badge color="red-4" square outline class="q-ml-sm">No Data</q-badge>
                          </span> -->
                        </q-td>
                        <q-td class="highlight-text" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal !important; width: 30%;" @click="() => { selectProject(props.row, props.row); loadProjectModules(props.row.id, 'module'); }">
                          <div class="row items-center justify-between">
                            <div class="col-md-12">
                              {{ props.row.name }}
                            </div>
                            <div class="col-md-12">
                              <q-badge v-if="props.row.status" rounded :color="getStatusColor(props.row.status)" :label="props.row.status" class="text-dark" />
                              <!-- <q-badge
                                class="q-mx-xs text-dark"
                                rounded
                                color="deep-purple-2"
                              >
                                {{ props.row.totalDoneTaskCount + '/' + props.row.totalTaskCount }}
                                <q-tooltip>Task Progress</q-tooltip>
                              </q-badge> -->
                              <q-badge class="q-ml-xs" color="grey" rounded>
                                {{
                                  isPMChecked
                                    ? (props.row.projectModuleCloseCount || 0)
                                    : (props.row.totalProjectModuleCount || 0)
                                }}
                                <q-tooltip>
                                  {{ isTaskChecked ? 'Closed Modules Count' : 'Total Modules Count' }}
                                </q-tooltip>
                              </q-badge>
                            </div>
                          </div>
                        </q-td>
                        <!-- <q-td auto-width class="text-center actions" style="width: 15% !important;">
                              {{ props.row.completedTaskCount + '/' + props.row.totalTaskCount }}
                            </q-td> -->
                        <q-td auto-width class="text-center actions" style="width: 0%;">
                          <div class="flex" style="flex-wrap: nowrap;">
                            <q-btn icon="o_more_vert" dense flat round no-caps class="text-primary btnRounded prod-btn" size="sm">
                              <q-menu>
                                <q-list style="min-width: 200px">
                                  <q-separator />
                                  <q-item
                                    v-ripple clickable
                                    @click="onProjectView(props.row.id)"
                                  >
                                    <q-item-section avatar><q-icon name="o_visibility" color="" size="xs" /></q-item-section>
                                    <q-item-section class="">View Project</q-item-section>
                                  </q-item>
                                  <q-item v-if="props.row.isEditable" v-ripple clickable class="" @click.stop="onProjectEdit(props.row.id, false, refreshProjectList, refreshProjectNameDropdown)">
                                    <q-item-section avatar><q-icon name="o_edit" color="" size="xs" /></q-item-section>
                                    <q-item-section class="">Edit</q-item-section>
                                  </q-item>
                                  <q-item v-if="props.row.isEditable || props.row.isNotes" v-ripple clickable @click="onNoteAdd(props.row.id, 'Projects', props.row.id, props.row.name, props.row.name, refreshProjectList)">
                                    <q-item-section avatar><q-icon name="o_assignment" size="xs" /></q-item-section>
                                    <q-item-section>Note</q-item-section>
                                    <div>
                                      <q-badge
                                        v-if="props.row.projectNoteCount > 0"
                                        color="green"
                                        text-color="white"
                                        :label="props.row.projectNoteCount"
                                      />
                                    </div>
                                  </q-item>
                                  <q-item v-if="props.row.isEditable || props.row.isNotes" v-ripple clickable @click="onProjectMessage(props.row.id, refreshProjectList)">
                                    <q-item-section avatar><q-icon name="o_message" size="xs" /></q-item-section>
                                    <q-item-section>Message</q-item-section>
                                    <div>
                                      <q-badge
                                        v-if="props.row.projectMessageCount > 0"
                                        color="green"
                                        text-color="white"
                                        :label="props.row.projectMessageCount"
                                      />
                                    </div>
                                  </q-item>
                                  <q-item v-ripple clickable @click="$router.push({ path: '/project-center', state: { projectId: props.row.id } })">
                                    <q-item-section avatar><q-icon name="o_radio_button_checked" size="xs" /></q-item-section>
                                    <q-item-section>Project Center</q-item-section>
                                  </q-item>
                                  <q-item v-ripple class="hidden" clickable @click="$router.push({ path: `/project-planning/calendar`, state: {projectId: props.row.id } })">
                                    <q-item-section avatar><q-icon name="o_calendar_month" size="xs" /></q-item-section>
                                    <q-item-section>Project Calendar</q-item-section>
                                  </q-item>
                                  <q-item v-ripple clickable @click="$router.push({ path: '/project-targetplan/weeklyplanner', state: {projectId: props.row.id } })">
                                    <q-item-section avatar><q-icon name="o_calendar_view_week" size="xs" /></q-item-section>
                                    <q-item-section>Weekly Planner</q-item-section>
                                  </q-item>
                                  <q-item v-ripple clickable @click="$router.push({ path: '/project-targetplan/monthlyplanner', state: {projectId: props.row.id } })">
                                    <q-item-section avatar><q-icon name="o_calendar_view_month" size="xs" /></q-item-section>
                                    <q-item-section>Monthly Planner</q-item-section>
                                  </q-item>
                                  <q-item v-if="props.row.isEditable" v-ripple clickable @click="onProjectFilesView(props.row.id, props.row.name, refreshProjectList)">
                                    <q-item-section avatar><q-icon name="o_description" size="xs" /></q-item-section>
                                    <q-item-section>Files</q-item-section>
                                  </q-item>
                                  <!-- <q-item v-if="props.row.isEditable" v-ripple clickable>
                                    <q-item-section avatar><q-icon name="o_flag" color="secondary" size="xs" /></q-item-section>
                                    <q-item-section class="">Project Status</q-item-section>
                                    <q-menu anchor="top end" self="top start" @popup-show="() => handlePopupShow(props.row.status)">
                                      <q-item v-for="status in projectStatusList" :key="status.value" v-ripple clickable :disable="status.disable" @click="onStatusSubmit(props.row.id, status.value)">
                                        <q-item-section avatar>
                                          <q-icon v-if="status.value === props.row.projectStatusId" name="o_task_alt" color="primary" size="xs" />
                                          <q-icon v-else name="o_radio_button_unchecked" color="secondary" size="xs" />
                                        </q-item-section>
                                        <q-item-section class="">{{ status.text }}</q-item-section>
                                      </q-item>
                                    </q-menu>
                                  </q-item> -->
                                  <q-item v-if="props.row.isEditable" v-ripple clickable>
                                    <q-item-section avatar>
                                      <q-icon name="o_flag" color="secondary" size="xs" />
                                    </q-item-section>
                                    <q-item-section>Project Status</q-item-section>

                                    <q-menu
                                      anchor="top end"
                                      self="top start"
                                      @before-show="() => handlePopupShow(props.row.id, props.row.status)"
                                    >
                                      <q-item
                                        v-for="status in projectStatusMap[props.row.id] || []"
                                        :key="status.value"
                                        v-ripple
                                        clickable
                                        :disable="status.disable"
                                        @click="onStatusSubmit(props.row.id, status.value)"
                                      >
                                        <q-item-section avatar>
                                          <q-icon
                                            v-if="status.value === props.row.projectStatusId"
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
                                        <q-item-section>{{ status.text }}</q-item-section>
                                      </q-item>
                                    </q-menu>
                                  </q-item>

                                  <q-item v-if="props.row.isEditable" v-ripple clickable>
                                    <q-item-section avatar><q-icon name="o_priority_high" color="secondary" size="xs" /></q-item-section>
                                    <q-item-section class="">Project Priority</q-item-section>
                                    <q-menu anchor="top end" self="top start">
                                      <q-item v-for="priority in projectPrioritiesDropdown.list.value" :key="priority.value" v-ripple clickable @click="onPrioritySubmit(props.row.id, priority.value)">
                                        <q-item-section avatar>
                                          <q-icon v-if="priority.value === props.row.projectPriorityId" name="o_task_alt" color="primary" size="xs" />
                                          <q-icon v-else name="o_radio_button_unchecked" color="secondary" size="xs" />
                                        </q-item-section>
                                        <q-item-section class="">{{ priority.text }}</q-item-section>
                                      </q-item>
                                    </q-menu>
                                  </q-item>
                                  <q-item v-if="!props.row.isTemplate" v-ripple class="hidden" clickable @click="convertProjectTemplate(props.row.id, props.row.name, props.row.startDate)">
                                    <q-item-section avatar><q-icon name="o_swap_horiz" size="xs" /></q-item-section>
                                    <q-item-section>Convert To Template?</q-item-section>
                                  </q-item>
                                  <q-item v-if="props.row.isEditable">
                                    <q-item-section avatar>
                                      <q-icon v-if="props.row.projectColor" name="o_circle" :style="`border-radius:50%;color:${props.row.projectColor}; background-color:${props.row.projectColor};`" size="xs" />
                                      <q-icon v-else name="o_question_mark" size="xs" />
                                    </q-item-section>
                                    <q-item-section style="display: flex; align-items: center; justify-content: start; flex-direction: row;">
                                      Project Color
                                      <q-icon name="o_colorize" class="cursor-pointer q-ml-xs" size="xs" @click.stop="storePreviousColor(props.row)">
                                        <!-- <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                                          <q-color
                                            v-model="props.row.projectColor" v-ripple clickable no-header no-footer class="my-picker" @update:model-value="startColorSelection"
                                            @change="finalizeColorSelection(project, 'Project')"
                                          />
                                        </q-popup-proxy> -->
                                        <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                                          <q-color
                                            v-model="props.row.projectColor"
                                            no-header
                                            no-footer
                                            default-view="palette"
                                            class="my-picker"
                                            @update:model-value="startColorSelection"
                                            @change="finalizeColorSelection(props.row, 'Project')"
                                          />
                                        </q-popup-proxy>
                                      </q-icon>
                                    </q-item-section>
                                  </q-item>
                                  <q-item v-if="props.row.isEditable" v-ripple clickable @click="() => { isPinned(props.row.id, !props.row.isPinned); }">
                                    <q-item-section avatar>
                                      <q-icon :name="props.row.isPinned ? 'o_close' : 'o_push_pin'" size="xs" />
                                    </q-item-section>
                                    <q-item-section>{{ props.row.isPinned ? "Un Pin" : "Pin" }}</q-item-section>
                                  </q-item>
                                  <q-item v-if="props.row.isEditable" v-ripple class="" clickable @click="toggleActiveStatus(props.row)">
                                    <q-item-section avatar>
                                      <q-icon :name="props.row.active ? 'o_block' : 'o_check_circle_outline'" :color="props.row.active ? 'negative' : 'positive'" size="xs" />
                                    </q-item-section>
                                    <q-item-section>{{ props.row.active ? 'Set Inactive?' : 'Set Active?' }}</q-item-section>
                                  </q-item>
                                  <q-separator />
                                  <q-item v-if="props.row.isEditable" v-ripple v-close-popup clickable @click.stop="onDelete(props.row)">
                                    <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                                    <q-item-section class="text-negative">Delete</q-item-section>
                                  </q-item>
                                </q-list>
                              </q-menu>
                            </q-btn>
                          </div>
                        </q-td>
                      </q-tr>
                    </template>
                  </q-table>
                </div>
              </template>

              <template #after>
                <q-splitter v-model="mainSplit" class="module" :limits="(!middleDrawerOpen) ? [0, 0] : [0, 100]">
                  <!-- Module Panel -->
                  <template #before>
                    <div v-if="middleDrawerOpen && selectedProject">
                      <q-toolbar class="bg-green-2 column q-pa-sm" style="align-items:start !important">
                        <!-- First line: title + buttons -->
                        <div class="row items-center justify-between q-gutter-sm full-width q-mb-sm">
                          <!-- <q-toolbar-title class="q-pa-none"> -->
                          <div>
                            <h3 class="text-black q-mb-none">
                              Module for
                              <span v-if="selectedProject" class="text-primary fs-14"> {{ selectedProject.name }}</span>
                            </h3>
                          </div>
                          <div class="q-mb-sm">
                            <!-- </q-toolbar-title> -->
                            <q-btn
                              v-if="getProjectDetail(selectedProjectId)?.isEditable"
                              icon="o_add"
                              outline
                              size="sm"
                              style="padding: 3px 7px; min-height: 30px;" no-caps class="text-primary q-mr-xs"
                              @click="onAddProjectModule"
                            >
                              <q-tooltip>Add Module</q-tooltip>
                            </q-btn>
                            <q-btn size="sm" style="padding: 3px 7px; min-height: 30px;" outline icon="o_menu" @click="toggleMiddleDrawer">
                              <q-tooltip>Hide Modules & Project</q-tooltip>
                            </q-btn>
                          </div>
                        </div>
                        <div class="row items-center q-mr-xs">
                          <div class="search-container position-relative">
                            <searchFilterBar
                              v-model="searchProjectModule.filterModule"
                              :loading="searchProjectModuleLoader"
                              :applied-filters="appliedModuleFiltersCount"
                              class="search-bar"
                              @toggle-filter="showModulesFilter = !showModulesFilter"
                            />
                            <!-- Dropdown Content -->
                            <q-menu v-model="showModulesFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showModulesFilter = false">
                              <q-card class="q-pa-sm">
                                <multiSelectDropdown
                                  v-model="searchProjectModule.projectModuleIds"
                                  label="Project Module"
                                  :options="projectModulesByProjectIdForDropdown.list.value"
                                  :filter="projectModulesByProjectIdForDropdown.filter"
                                />
                                <multiSelectDropdown
                                  v-model="searchProjectModule.projectModuleStatusIds"
                                  label="Project Module Status"
                                  :options="projectModuleStatusForDropdown.list.value"
                                  :filter="projectModuleStatusForDropdown.filter"
                                  :isShowAll="true"
                                />
                                <div class="row items-center q-mb-sm">
                                  <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                                    <label class="Cutomlabel q-mt-sm fs-13">Show Closed</label>
                                  </div>
                                  <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                                    <q-checkbox v-model="isPMChecked" label="" color="indigo-12" class="q-mr-md" @click="ShowClosedPM()" />
                                  </div>
                                </div>
                                <!-- Search and Clear Buttons -->
                                <div class="row justify-end q-gutter-sm q-mb-sm">
                                  <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showModulesFilter = false; onModuleSearch(); }" />
                                  <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClearModule" />
                                  <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showModulesFilter = false; }" />
                                </div>
                              </q-card>
                            </q-menu>
                          </div>
                          <div class="q-ml-sm">
                            <!-- Replace your current sort menu with this -->
                            <q-btn icon="o_swap_vert" dense outline round no-caps class="text-primary btnRounded" size="sm">
                              <q-tooltip>
                                Sorted: By
                                {{ selectedSortByModule === 'startDate'
                                  ? ' Start Date'
                                  : selectedSortByModule === 'endDate'
                                    ? ' End Date'
                                    : selectedSortByModule === 'name'
                                      ? ' Name'
                                      : selectedSortByModule === 'projectModuleStatus'
                                        ? ' Status'
                                        : selectedSortByModule === 'createdOnUtc'
                                          ? ' Created Date'
                                          : selectedSortByModule
                                }} ({{ selectedSortOrderByModule ? 'Descending' : 'Ascending' }})</q-tooltip>
                              <q-menu>
                                <q-list style="min-width: 200px">
                                  <q-separator />
                                  <q-item-label header class="q-py-xs" style="font-size: 0.75rem;font-weight:500;">Sort By</q-item-label>
                                  <q-item>
                                    <q-item-section>
                                      <q-radio
                                        v-model="selectedSortByModule"
                                        size="xs"
                                        val="startDate"
                                        label="Start Date"
                                        checked-icon="o_task_alt"
                                        dense
                                        @update:model-value="val => onSortByModule(val, false)"
                                      />
                                    </q-item-section>
                                  </q-item>
                                  <q-item>
                                    <q-item-section>
                                      <q-radio
                                        v-model="selectedSortByModule"
                                        style="font-size:13px"
                                        size="xs"
                                        val="endDate"
                                        label="End Date"
                                        checked-icon="o_task_alt"
                                        dense
                                        @update:model-value="val => onSortByModule(val, false)"
                                      />
                                    </q-item-section>
                                  </q-item>
                                  <q-item>
                                    <q-item-section>
                                      <q-radio
                                        v-model="selectedSortByModule"
                                        style="font-size:13px"
                                        size="xs"
                                        val="name"
                                        label="Name"
                                        checked-icon="o_task_alt"
                                        dense
                                        @update:model-value="val => onSortByModule(val, false)"
                                      />
                                    </q-item-section>
                                  </q-item>
                                  <q-item>
                                    <q-item-section>
                                      <q-radio
                                        v-model="selectedSortByModule"
                                        style="font-size:13px"
                                        size="xs"
                                        val="projectModuleStatus"
                                        label="Status"
                                        checked-icon="o_task_alt"
                                        dense
                                        @update:model-value="val => onSortByModule(val, false)"
                                      />
                                    </q-item-section>
                                  </q-item>
                                  <q-item>
                                    <q-item-section>
                                      <q-radio
                                        v-model="selectedSortByModule"
                                        style="font-size:13px"
                                        size="xs"
                                        val="createdOnUtc"
                                        label="Created Date"
                                        checked-icon="o_task_alt"
                                        dense
                                        @update:model-value="val => onSortByModule(val, false)"
                                      />
                                    </q-item-section>
                                  </q-item>
                                  <q-separator />
                                  <q-item-label header class="q-py-xs " style="font-size: 0.75rem;font-weight:500;">Sort Order</q-item-label>
                                  <q-item>
                                    <q-item-section>
                                      <q-radio
                                        v-model="selectedSortOrderByModule"
                                        style="font-size:13px"
                                        size="xs"
                                        :val="false"
                                        label="Ascending"
                                        checked-icon="o_task_alt"
                                        dense
                                        @update:model-value="val => onSortByModule(val, true)"
                                      />
                                    </q-item-section>
                                  </q-item>
                                  <q-item>
                                    <q-item-section>
                                      <q-radio
                                        v-model="selectedSortOrderByModule"
                                        style="font-size:13px"
                                        size="xs"
                                        :val="true"
                                        label="Descending"
                                        checked-icon="o_task_alt"
                                        dense
                                        @update:model-value="val => onSortByModule(val, true)"
                                      />
                                    </q-item-section>
                                  </q-item>
                                </q-list>
                              </q-menu>
                            </q-btn>
                          </div>
                        </div>
                      </q-toolbar>
                      <q-table
                        v-model:pagination="projectModulesPagination"
                        :loading="loading"
                        :rows="filteredProjectModuleRows"
                        :columns="projectModuleColumns"
                        row-key="id"
                        separator="cell"
                        binary-state-sort
                        class="Custom-DataTable"
                        no-data-label="No module available"
                        :rows-per-page-options="[20, 50, 100, 200, 500]"
                        table-style="max-height: 65vh"
                        virtual-scroll
                        @request="getAllProjectsModulesByProjectId"
                      >
                        <template #header="props">
                          <q-tr :props="props" class="bg-primary text-white">
                            <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                          </q-tr>
                        </template>
                        <template #body="props">
                          <q-tr :props="props" :class="['cursor-pointer', activeModuleRowId == props.row.id ? 'bg-green-2' : '']">
                            <q-td class="highlight-text" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal !important; width: 40% !important;">

                              <div class="row items-center justify-between">
                                <div class="col-9" @click="LoadTasks(props.row.projectId, props.row.id, 'module')">
                                  <div class="" v-html="props.row.name" />
                                  <q-badge v-if="props.row.projectModuleStatus" color="grey" rounded>
                                    {{ props.row.projectModuleStatus }}
                                    <q-tooltip>Module Status</q-tooltip>
                                  </q-badge>
                                  <!-- <q-badge
                                    class="q-mx-xs text-dark"
                                    rounded
                                    color="deep-purple-2"
                                  >
                                    {{ props.row.totalDoneTaskCount + '/' + props.row.totalTaskCount }}
                                    <q-tooltip>Task Progress</q-tooltip>
                                  </q-badge> -->
                                  <q-badge class="q-ml-xs" color="grey" rounded>
                                    {{
                                      isTaskChecked
                                        ? (props.row.taskCloseCount || 0)
                                        : (props.row.totalTaskCount || 0)
                                    }}
                                    <q-tooltip>
                                      {{ isTaskChecked ? 'Closed Tasks Count' : 'Total Tasks Count' }}
                                    </q-tooltip>
                                  </q-badge>
                                </div>
                                <div>
                                  <q-btn
                                    v-if="isProject" icon="o_more_vert" flat dense no-caps class="text-primary q-ml-sm" size="sm" style="padding: 0px 3px;min-height: 25px;border: 1px solid #757575;border-radius: 5px;"
                                  >
                                    <q-menu>
                                      <q-list style="min-width: 200px">
                                        <q-separator />
                                        <q-item v-ripple clickable size="sm" icon="o_edit" label="Edit Bulk" no-caps class="q-mr-xs " @click="onProjectModuleView(props.row.id)">
                                          <q-item-section avatar><q-icon name="o_visibility" color="" size="xs" /></q-item-section>
                                          <q-item-section class="">View Module</q-item-section>
                                        </q-item>
                                        <q-item v-ripple clickable size="sm" icon="o_north_east" label="Edit Bulk" no-caps class="q-mr-xs hidden" @click="LoadTasks(props.row.projectId, props.row.id, 'module')">
                                          <q-item-section avatar><q-icon name="o_north_east" color="" size="xs" /></q-item-section>
                                          <q-item-section class="">View Task</q-item-section>
                                        </q-item>
                                        <q-item v-if="props.row.isEditable" v-ripple clickable size="sm" icon="o_edit" label="Edit Bulk" no-caps class="q-mr-xs" @click="onProjectModuleEdit(props.row.id, refreshProjectModulesList)">
                                          <q-item-section avatar><q-icon name="o_edit" color="" size="xs" /></q-item-section>
                                          <q-item-section class="">Edit Module</q-item-section>
                                        </q-item>
                                        <q-item v-if="props.row.isEditable" v-ripple clickable @click="onProjectModuleCopy(props.row.id, props.row.name, refreshProjectModulesList)">
                                          <q-item-section avatar><q-icon name="o_content_copy" size="xs" /></q-item-section>
                                          <q-item-section>Copy to Project</q-item-section>
                                        </q-item>
                                        <q-item v-if="role === 'admin'" v-ripple clickable @click="onProjectModuleMoveAsProject(props.row.id, props.row.name, props.row.project.id, refreshProjectModulesList)">
                                          <q-item-section avatar><q-icon name="o_arrow_forward" size="xs" /></q-item-section>
                                          <q-item-section>Move Module As Project</q-item-section>
                                        </q-item>
                                        <q-item v-if="props.row.isEditable" v-ripple clickable @click="onDeleteModule(props.row)">
                                          <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                                          <q-item-section class="text-negative">Delete</q-item-section>
                                        </q-item>
                                      </q-list>
                                    </q-menu>
                                  </q-btn>
                                </div>
                              </div>
                            </q-td>
                          </q-tr>
                        </template>
                      </q-table>
                    </div>
                    <div v-else class="text-grey text-caption q-pa-sm">Select a project to view modules</div>
                  </template>

                  <!-- Task and Activities View -->
                  <template #after>
                    <q-splitter v-model="ActivitySplit" class="">
                      <template #before>
                        <div v-if="selectedModule">
                          <q-toolbar class="bg-green-2 column q-pa-sm" style="align-items:start !important">
                            <!-- First line: title + actions -->
                            <div class="row items-center justify-between q-gutter-sm full-width q-mb-sm">
                              <!-- <q-toolbar-title class="q-pa-none"> -->
                              <div>
                                <h3 class="text-black q-mb-none">
                                  Task for
                                  <span v-if="storedModuleName && isProjectModule" class="text-primary" style="font-size: 14px;">
                                    {{ ProjectName + ' & ' + storedModuleName }}
                                  </span>
                                </h3>
                              </div>
                              <div class="q-mb-sm">
                                <!-- </q-toolbar-title> -->
                                <q-btn v-if="selectedModuleId && getProjectDetail(selectedProjectId)?.isEditable" size="sm" icon="o_add" outline label="" no-caps class="text-primary q-mr-xs" style="padding: 3px 7px; min-height: 30px;" @click="onAddProjectTask(selectedProjectId)" />
                                <q-btn v-if="selectedModuleId && getProjectDetail(selectedProjectId)?.isEditable" :class="isPastMonth ? 'pointer-disbled' : ''" size="sm" icon="o_add" outline label="" no-caps class="text-primary q-mr-xs" style="padding: 3px 7px; min-height: 30px;" @click="onAddBulkTask">
                                  <q-icon size="xs" name="o_view_list" class="cursor-pointer q-mr-xs" />
                                  <q-tooltip>Add Bulk Tasks</q-tooltip>
                                </q-btn>
                                <q-btn v-if="selectedTaskId!=null && getProjectDetail(selectedProjectId)?.isEditable" :class="isPastMonth ? 'pointer-disbled' : ''" size="sm" icon="o_edit" outline label="" no-caps class="text-primary q-mr-xs" style="padding: 3px 7px; min-height: 30px;" @click="onEditBulkTasks">
                                  <q-icon size="xs" name="o_view_list" class="cursor-pointer q-mr-xs" />
                                  <q-tooltip>Edit Bulk Tasks</q-tooltip>
                                </q-btn>
                                <q-btn icon="o_open_in_new" size="sm" outline class="text-primary q-mr-xs" style="padding: 3px 7px; min-height: 30px;" @click="$router.push('/project-tasks')">
                                  <q-tooltip>Manage Tasks</q-tooltip>
                                </q-btn>
                              </div>
                            </div>
                            <div class="row items-center q-mr-xs">
                              <div class="search-container position-relative">
                                <searchFilterBar
                                  v-model="searchProjectTask.filterTask"
                                  :loading="searchProjectTaskLoader"
                                  :applied-filters="appliedTaskFiltersCount"
                                  class="search-bar"
                                  @toggle-filter="showTasksFilter = !showTasksFilter"
                                />
                                <!-- Dropdown Content -->
                                <q-menu v-model="showTasksFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showTasksFilter = false">
                                  <q-card class="q-pa-sm">
                                    <div class="row items-center q-mb-sm">
                                      <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                                        <label class="Cutomlabel q-mt-sm fs-13">Task Name</label>
                                      </div>
                                      <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                                        <div>
                                          <q-input v-model="searchProjectTask.name" fill-input :dense="true" class="q-mx-sm" />
                                        </div>
                                      </div>
                                    </div>
                                    <multiSelectDropdown
                                      v-model="searchProjectTask.statusIds"
                                      label="Status"
                                      :options="projectTaskStatusForDropdown.list.value"
                                      :filter="projectTaskStatusForDropdown.filter"
                                      :isShowAll="true"
                                    />
                                    <multiSelectDropdown
                                      v-model="searchProjectTask.priorityIds"
                                      label="Task Priority"
                                      :options="projectTaskPrioritiesForDropdown.list.value"
                                      :filter="projectTaskPrioritiesForDropdown.filter"
                                      :isShowAll="true"
                                    />
                                    <multiSelectDropdown
                                      v-model="searchProjectTask.assignedToIds"
                                      label="Task Owner"
                                      :options="projectEmployeesForDropdown.list.value"
                                      :filter="projectEmployeesForDropdown.filter"
                                    />
                                    <multiSelectDropdown
                                      v-model="searchProjectTask.taskTagsIds"
                                      label="Task Tags"
                                      :options="projectTaskTagsDropdown.list.value"
                                      :filter="projectTaskTagsDropdown.filter"
                                      :show-bg-color="true"
                                    />
                                    <div class="row items-center q-mb-sm">
                                      <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                                        <label class="Cutomlabel q-mt-sm fs-13">Show Closed</label>
                                      </div>
                                      <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                                        <q-checkbox v-model="isTaskChecked" label="" color="indigo-12" class="q-mr-md" @click="ShowClosedTask()" />
                                      </div>
                                    </div>
                                    <!-- Search and Clear Buttons -->
                                    <div class="row justify-end q-gutter-sm q-mb-sm">
                                      <q-btn style="width: 20%;" outline color="primary" label="Search" class="btnRounded" no-caps @click="() => { showTasksFilter = false; onTaskSearch(); }" />
                                      <q-btn style="width: 20%;" outline color="grey-4" label="Clear" class="text-grey-9 btnRounded" no-caps @click="onClearTask" />
                                      <q-btn style="width: 20%;" outline color="negative" label="Close" class="btnRounded" no-caps @click="() => { showTasksFilter = false; }" />
                                    </div>
                                  </q-card>
                                </q-menu>
                              </div>
                              <div class="q-ml-sm">
                                <!-- Replace your current sort menu with this -->
                                <q-btn icon="o_swap_vert" dense outline round no-caps class="text-primary btnRounded" size="sm">
                                  <q-tooltip>
                                    Sorted: By
                                    {{ selectedSortByTask === 'startDate'
                                      ? ' Start Date'
                                      : selectedSortByTask === 'endDate'
                                        ? ' End Date'
                                        : selectedSortByTask === 'taskName'
                                          ? ' Name'
                                          : selectedSortByTask === 'status'
                                            ? ' Status'
                                            : selectedSortByTask === 'priority'
                                              ? ' Priority'
                                              : selectedSortByTask === 'assignTo'
                                                ? ' Owner'
                                                : selectedSortByTask === 'createdOnUtc'
                                                  ? ' Created Date'
                                                  : selectedSortByTask
                                    }} ({{ selectedSortOrderByTask ? 'Descending' : 'Ascending' }})</q-tooltip>
                                  <q-menu>
                                    <q-list style="min-width: 200px">
                                      <q-separator />
                                      <q-item-label header class="q-py-xs" style="font-size: 0.75rem;font-weight:500;">Sort By</q-item-label>
                                      <q-item>
                                        <q-item-section>
                                          <q-radio
                                            v-model="selectedSortByTask"
                                            size="xs"
                                            val="taskName"
                                            label="Name"
                                            checked-icon="o_task_alt"
                                            dense
                                            @update:model-value="val => onSortByTask(val, false)"
                                          />
                                        </q-item-section>
                                      </q-item>
                                      <q-item>
                                        <q-item-section>
                                          <q-radio
                                            v-model="selectedSortByTask"
                                            style="font-size:13px"
                                            size="xs"
                                            val="startDate"
                                            label="Start Date"
                                            checked-icon="o_task_alt"
                                            dense
                                            @update:model-value="val => onSortByTask(val, false)"
                                          />
                                        </q-item-section>
                                      </q-item>
                                      <q-item>
                                        <q-item-section>
                                          <q-radio
                                            v-model="selectedSortByTask"
                                            style="font-size:13px"
                                            size="xs"
                                            val="endDate"
                                            label="End Date"
                                            checked-icon="o_task_alt"
                                            dense
                                            @update:model-value="val => onSortByTask(val, false)"
                                          />
                                        </q-item-section>
                                      </q-item>
                                      <q-item>
                                        <q-item-section>
                                          <q-radio
                                            v-model="selectedSortByTask"
                                            style="font-size:13px"
                                            size="xs"
                                            val="status"
                                            label="Status"
                                            checked-icon="o_task_alt"
                                            dense
                                            @update:model-value="val => onSortByTask(val, false)"
                                          />
                                        </q-item-section>
                                      </q-item>
                                      <q-item>
                                        <q-item-section>
                                          <q-radio
                                            v-model="selectedSortByTask"
                                            style="font-size:13px"
                                            size="xs"
                                            val="priority"
                                            label="Priority"
                                            checked-icon="o_task_alt"
                                            dense
                                            @update:model-value="val => onSortByTask(val, false)"
                                          />
                                        </q-item-section>
                                      </q-item>
                                      <q-item>
                                        <q-item-section>
                                          <q-radio
                                            v-model="selectedSortByTask"
                                            style="font-size:13px"
                                            size="xs"
                                            val="assignTo"
                                            label="Owner"
                                            checked-icon="o_task_alt"
                                            dense
                                            @update:model-value="val => onSortByTask(val, false)"
                                          />
                                        </q-item-section>
                                      </q-item>
                                      <q-item>
                                        <q-item-section>
                                          <q-radio
                                            v-model="selectedSortByTask"
                                            style="font-size:13px"
                                            size="xs"
                                            val="createdOnUtc"
                                            label="Created Date"
                                            checked-icon="o_task_alt"
                                            dense
                                            @update:model-value="val => onSortByTask(val, false)"
                                          />
                                        </q-item-section>
                                      </q-item>
                                      <q-separator />
                                      <q-item-label header class="q-py-xs " style="font-size: 0.75rem;font-weight:500;">Sort Order</q-item-label>
                                      <q-item>
                                        <q-item-section>
                                          <q-radio
                                            v-model="selectedSortOrderByTask"
                                            style="font-size:13px"
                                            size="xs"
                                            :val="false"
                                            label="Ascending"
                                            checked-icon="o_task_alt"
                                            dense
                                            @update:model-value="val => onSortByTask(val, true)"
                                          />
                                        </q-item-section>
                                      </q-item>
                                      <q-item>
                                        <q-item-section>
                                          <q-radio
                                            v-model="selectedSortOrderByTask"
                                            style="font-size:13px"
                                            size="xs"
                                            :val="true"
                                            label="Descending"
                                            checked-icon="o_task_alt"
                                            dense
                                            @update:model-value="val => onSortByTask(val, true)"
                                          />
                                        </q-item-section>
                                      </q-item>
                                    </q-list>
                                  </q-menu>
                                </q-btn>
                              </div>
                            </div>
                          </q-toolbar>
                          <q-table
                            v-model:pagination="projectTasksPagination"
                            :loading="loading"
                            :rows="filteredTaskRows"
                            :columns="projectTaskColumns"
                            row-key="id"
                            separator="cell"
                            binary-state-sort
                            class="Custom-DataTable"
                            no-data-label="No task available"
                            :rows-per-page-options="[20, 50, 100, 200, 500]"
                            table-style="max-height: 65vh"
                            virtual-scroll
                            @request="getTaskListByModuleId"
                          >
                            <template #header="props">
                              <q-tr :props="props" class="bg-primary text-white">
                                <!-- <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th> -->
                                <q-th v-for="col in props.cols" :key="col.name" :props="props">
                                  <template v-if="col.name === 'estimateTime'">
                                    <div class="" style="text-align:right !important">
                                      <span>{{ col.label }}</span>
                                      <q-icon name="o_info" size="xs" class="text-black q-ml-xs">
                                        <q-tooltip anchor="bottom middle" self="top middle" class="bg-grey-8 text-white shadow-2">
                                          <div class="text-caption">
                                            <span>{{ "(Task Est. Hrs / Sum Of Activity hours)" }}</span>
                                          </div>
                                        </q-tooltip>
                                      </q-icon>
                                    </div>
                                  </template>
                                  <template v-else>
                                    {{ col.label }}
                                  </template>
                                </q-th>

                                <q-th auto-width class="text-center" />
                              </q-tr>
                            </template>
                            <template #body="props">
                              <q-tr :props="props" :class="['cursor-pointer', activeTaskRowId == props.row.id ? 'bg-green-2' : '']">
                                <q-td class="highlight-text" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal !important; width: 40% !important;" @click="LoadTaskActivities(props.row.id)">
                                  <div :style="'border-left: 5px solid ' + (props.row.color ? props.row.color : 'transparent') + '; width: 5px;height: 97%;position: absolute; left: -1px;top: 0px;'">
                                    <q-tooltip>Project Color</q-tooltip>
                                  </div>
                                  <div class="row items-center justify-between">
                                    <div class="col-md-12">
                                      <div class="" v-html="props.row.name" />
                                    </div>
                                    <div class="col-md-12">
                                      <q-badge v-if="props.row.status" rounded color="grey" :label="props.row.status">
                                        <q-tooltip>Task Status</q-tooltip>
                                      </q-badge>
                                      <!-- <q-badge class="q-ml-xs" color="grey" rounded>
                                        {{
                                          isPAChecked
                                            ? (props.row.activityCloseCount || 0)
                                            : (props.row.totalActivitiesCount || 0)
                                        }}
                                        <q-tooltip>
                                          {{ isPAChecked ? 'Closed Activities Count' : 'Total Activities Count' }}
                                        </q-tooltip>
                                      </q-badge> -->
                                      <q-badge class="q-ml-xs" color="grey" rounded>
                                        {{
                                          (() => {
                                            const total = props.row.totalActivitiesCount || 0
                                            const close = props.row.activityCloseCount || 0
                                            const completed = props.row.activityCompletedCount || 0
                                            const open = Math.max(0, total - close - completed)

                                            if (isPAChecked && isPAComChecked) {
                                              return open + close + completed
                                            }

                                            if (isPAChecked) {
                                              return open + close
                                            }

                                            if (isPAComChecked) {
                                              return open + completed
                                            }

                                            return open
                                          })()
                                        }}

                                        <q-tooltip>
                                          {{
                                            isPAChecked && isPAComChecked
                                              ? 'Total Activities Count'
                                              : isPAChecked
                                                ? 'Open + Closed Activities Count'
                                                : isPAComChecked
                                                  ? 'Open + Completed Activities Count'
                                                  : 'Open Activities Count'
                                          }}
                                        </q-tooltip>
                                      </q-badge>
                                    </div>
                                  </div>
                                </q-td>
                                <q-td class="text-center" style="width: 5% !important" @click="LoadTaskActivities(props.row.id)">{{ props.row.startDate ? toDate(props.row.startDate) : '--' }} <span v-if="props.row.endDate" class=""> <p class="q-mb-none">To</p> </span> {{ props.row?.endDate || toDate(props.row?.endDate) }}</q-td>
                                <q-td class="" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal !important; width: 10px !important;" @click="LoadTaskActivities(props.row.id)">
                                  <div class="row items-center">
                                    <!-- Task Owner - Label -->
                                    <div class="col-3">
                                      <div v-if="props.row.assignTo">
                                        <q-badge rounded color="primary">
                                          <q-icon name="o_person" size="xs" color="white" class="q-mr-xs">
                                            <q-tooltip>Task Owner?</q-tooltip>
                                          </q-icon>
                                          <span v-if="props.row.firstName && props.row.lastName">{{ props.row.firstName[0] + props.row.lastName[0] }}</span>
                                          <q-tooltip>
                                            <div>
                                              <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                                              <span>{{ props.row.firstName + ' ' + props.row.lastName }}</span>
                                            </div>
                                            <div>
                                              <q-icon name="o_mail" color="white" size="xs" class="q-mr-xs" />
                                              <span>{{ props.row.primaryEmailAddress }}</span>
                                            </div>
                                          </q-tooltip>
                                        </q-badge>
                                      </div>
                                    </div>
                                    <div class="col-9 flex justify-end q-pa-xs TaskActivity hidden">
                                      <div
                                        v-for="activity in getUniqueActivities(props.row.projectActivities)"
                                        :key="activity.id"
                                        class="Person"
                                      >
                                        <span>{{ getInitials(activity) }}</span>
                                        <q-tooltip>
                                          <div>
                                            <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                                            <span>{{ activity.firstName + ' ' + activity.lastName }}</span>
                                          </div>
                                          <div>
                                            <q-icon name="o_mail" color="white" size="xs" class="q-mr-xs" />
                                            <span>{{ activity.primaryEmailAddress }}</span>
                                          </div>
                                        </q-tooltip>
                                      </div>
                                    </div>
                                  </div>
                                </q-td>
                                <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal !important; width: 25% !important;" @click="LoadTaskActivities(props.row.id)">
                                  <div v-if="props.row.taskTags?.length">
                                    <q-chip
                                      v-for="(tag, i) in props.row.taskTags"
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
                                  </div>
                                  <q-popup-edit
                                    v-if="props.row.isEditable"
                                    v-slot="scope"
                                    v-model="props.row.taskTags"
                                    class="common-q-td small-popup-title"
                                    style="width: 300px;"
                                    @save="val => onSubmitProjectTaskTags(val, props.row.id)"
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
                                </q-td>
                                <q-td class="text-right" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal !important; width: 10px !important;" @click="LoadTaskActivities(props.row.id)">
                                  <span v-if="!isPAChecked">{{ props.row.estimateTime }} / {{ props.row.totalActivityHours }}</span>
                                  <span v-else>{{ props.row.estimateTime }} / {{ props.row.totalCloseActivityHours }} / </span>
                                </q-td>
                                <q-td auto-width class="text-center actions" style="width: 3% !important;">
                                  <div class="flex items-center justify-center" style="flex-wrap: nowrap;">
                                    <div class="">
                                      <q-btn icon="o_more_vert" dense flat round no-caps class="text-primary btnRounded" size="sm">
                                        <q-menu>
                                          <q-list style="min-width: 200px">
                                            <q-separator />
                                            <q-item v-ripple clickable class="" @click.stop="onProjectTaskView(props.row.id)">
                                              <q-item-section avatar><q-icon name="o_visibility" color="" size="xs" /></q-item-section>
                                              <q-item-section class="">View Task</q-item-section>
                                            </q-item>
                                            <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple clickable class="" @click.stop="onProjectTaskEdit(props.row.id, refreshProjectTaskList)">
                                              <q-item-section avatar><q-icon name="o_edit" color="" size="xs" /></q-item-section>
                                              <q-item-section class="">Edit</q-item-section>
                                            </q-item>
                                            <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple clickable @click="onProjectTaskFiles(props.row.id, props.row.name, props.row.projectName, props.row.projectListName)">
                                              <q-item-section avatar><q-icon name="o_description" size="xs" /></q-item-section>
                                              <q-item-section>Files</q-item-section>
                                            </q-item>
                                            <!-- <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple clickable>
                                              <q-item-section avatar><q-icon name="o_flag" color="secondary" size="xs" /></q-item-section>
                                              <q-item-section class="">Task Status</q-item-section>
                                              <q-menu anchor="top end" self="top start">
                                                <q-item
                                                  v-for="taskStatus in projectTaskStatusList" :key="taskStatus.value" v-ripple clickable :style="{
                                                    cursor: shouldDisableSelect(props.row) ? 'not-allowed' : 'auto',
                                                    pointerEvents: shouldDisableSelect(props.row) ? 'none' : 'auto',
                                                    opacity: shouldDisableSelect(props.row) ? 0.6 : 1
                                                  }" @click="onSubmitTaskStatus(props.row.id, taskStatus.value)"
                                                >
                                                  <q-item-section
                                                    avatar
                                                  >
                                                    <q-icon v-if="taskStatus.value === props.row.statusId" name="o_task_alt" color="primary" size="xs" />
                                                    <q-icon v-else name="o_radio_button_unchecked" color="secondary" size="xs" />
                                                  </q-item-section>
                                                  <q-item-section class="" style="white-space: nowrap !important;">{{ taskStatus.text }}</q-item-section>
                                                </q-item>
                                              </q-menu>
                                            </q-item> -->
                                            <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple clickable>
                                              <q-item-section avatar>
                                                <q-icon name="o_flag" color="secondary" size="xs" />
                                              </q-item-section>
                                              <q-item-section class="">Task Status</q-item-section>

                                              <q-menu
                                                anchor="top end"
                                                self="top start"
                                                @before-show="() => handleTaskPopupShow(props.row.id, props.row.status, props.row.project.status)"
                                              >
                                                <q-item
                                                  v-for="taskStatus in projectTaskStatusMap[props.row.id] || []"
                                                  :key="taskStatus.value"
                                                  v-ripple
                                                  clickable
                                                  :disable="taskStatus.disable"
                                                  :style="{
                                                    cursor: shouldDisableSelect(props.row) ? 'not-allowed' : 'auto',
                                                    pointerEvents: shouldDisableSelect(props.row) ? 'none' : 'auto',
                                                    opacity: shouldDisableSelect(props.row) ? 0.6 : 1
                                                  }"
                                                  @click="onSubmitTaskStatus(props.row.id, taskStatus.value)"
                                                >
                                                  <q-item-section avatar>
                                                    <q-icon
                                                      v-if="taskStatus.value === props.row.statusId"
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

                                            <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple clickable>
                                              <q-item-section avatar><q-icon name="o_priority_high" color="secondary" size="xs" /></q-item-section>
                                              <q-item-section class="">Task Priority</q-item-section>
                                              <q-menu anchor="top end" self="top start">
                                                <q-item v-for="priority in projectTaskPrioritiesForDropdown.list.value" :key="priority.value" v-ripple clickable @click="onTaskPrioritySubmit(props.row.id, priority.value)">
                                                  <q-item-section avatar>
                                                    <q-icon v-if="priority.value === props.row.priorityId" name="o_task_alt" color="primary" size="xs" />
                                                    <q-icon v-else name="o_radio_button_unchecked" color="secondary" size="xs" />
                                                  </q-item-section>
                                                  <q-item-section class="">{{ priority.text }}</q-item-section>
                                                </q-item>
                                              </q-menu>
                                            </q-item>
                                            <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple clickable class="">
                                              <q-item-section avatar><q-icon name="o_person_add" color="secondary" size="xs" /></q-item-section>
                                              <q-item-section class="">Task Owner?</q-item-section>
                                              <q-menu anchor="top end" self="top start">
                                                <q-item v-ripple clickable class="flex column">
                                                  <div class="q-pa-xs">
                                                    <q-select
                                                      v-model="props.row.assignedToId"
                                                      class="" use-input outlined stack-label hide-bottom-space :dense="true"
                                                      :options="taskOwnerList" option-value="value" option-label="text" emit-value map-options @filter="getTaskOwnerFilterForDropdown"
                                                      @update:model-value="onSubmitOwnerData(props.row.id, props.row.assignedToId)"
                                                    >
                                                      <template #prepend>
                                                        <q-icon name="o_person" size="xs" style="" />
                                                      </template>
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
                                                </q-item>
                                              </q-menu>
                                            </q-item>
                                            <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple clickable>
                                              <q-item-section avatar><q-icon name="o_local_offer" size="xs" /></q-item-section>
                                              <q-item-section>Add Tags
                                                 <q-popup-edit
                                                  v-if="props.row.isEditable"
                                                  v-slot="scope"
                                                  v-model="props.row.taskTags"
                                                  class="common-q-td small-popup-title"
                                                  style="width: 300px;"
                                                  @save="val => onSubmitProjectTaskTags(val, props.row.id)"
                                                >
                                                  <div class="row justify-between items-center q-mb-sm">
                                                    <div class="text-subtitle2">Add Tags</div>
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
                                              </q-item-section>
                                            </q-item>
                                            <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple clickable @click="onProjectTaskStatusChangeLog(props.row.id)">
                                              <q-item-section avatar><q-icon name="o_flag" size="xs" /></q-item-section>
                                              <q-item-section>Task Status Change Log</q-item-section>
                                            </q-item>
                                            <q-item v-ripple clickable @click="onProjectTaskLevelTimeSheetView(props.row.id)">
                                              <q-item-section avatar><q-icon name="o_notes" size="xs" /></q-item-section>
                                              <q-item-section>Task Level Timesheet</q-item-section>
                                            </q-item>
                                            <q-item v-if="(props.row.isEditable || props.row.isNotes) && props.row.status != 'Close'" v-ripple clickable @click="onNoteAdd(props.row.id, 'Project Task', props.row.projectId, props.row.projectName, props.row.name, refreshProjectList)">
                                              <q-item-section avatar><q-icon name="o_note" size="xs" /></q-item-section>
                                              <q-item-section>Note</q-item-section>
                                            </q-item><q-separator />
                                            <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple class="" clickable @click="onProjectTaskCopy(props.row.id, props.row.name, props.row.projectModuleId, 'isCopy', refreshProjectTaskList)">
                                              <q-item-section avatar><q-icon name="o_copy" size="xs" /></q-item-section>
                                              <q-item-section>Copy</q-item-section>
                                            </q-item>
                                            <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple class="" clickable @click="onProjectTaskMove(props.row.id, props.row.name, props.row.projectModuleId, 'isMove', refreshProjectTaskList)">
                                              <q-item-section avatar><q-icon name="o_arrow_forward" size="xs" /></q-item-section>
                                              <q-item-section>Move</q-item-section>
                                            </q-item>
                                            <q-item v-if="props.row.isEditable && props.row.status != 'Close'">
                                              <q-item-section avatar>
                                                <q-icon v-if="props.row.color" name="o_circle" :style="`border-radius:50%;color:${props.row.color}; background-color:${props.row.color};`" size="xs" />
                                                <q-icon v-else name="o_question_mark" size="xs" />
                                              </q-item-section>
                                              <q-item-section style="display: flex; align-items: center; justify-content: start; flex-direction: row;">
                                                Task Color
                                                <q-icon name="o_colorize" class="cursor-pointer q-ml-xs" size="xs" @click.stop="storePreviousColor(props.row)">
                                                  <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                                                    <q-color
                                                      v-model="props.row.color" v-ripple clickable no-header no-footer class="my-picker" @update:model-value="startColorSelection"
                                                      @change="finalizeColorSelection(props.row, 'Task')"
                                                    />
                                                  </q-popup-proxy>
                                                </q-icon>
                                              </q-item-section>
                                            </q-item>
                                            <q-separator />
                                            <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple v-close-popup clickable @click.stop="onDeleteTask(props.row)">
                                              <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                                              <q-item-section class="text-negative">Delete</q-item-section>
                                            </q-item>
                                          </q-list>
                                        </q-menu>
                                      </q-btn>
                                    </div>
                                  </div>
                                </q-td>
                              </q-tr>
                            </template>
                          </q-table>
                        </div>
                        <div v-else class="text-grey text-caption q-pa-sm">Select a module to view tasks</div>
                      </template>
                      <template #after>
                        <div v-if="selectedTask">
                          <q-toolbar class="bg-green-2 column q-pa-sm">
                            <div class="row items-center justify-between q-gutter-sm full-width">
                              <div>
                                <h3 class="text-black q-mb-none"><span>Activity for </span><span v-if="storedTaskName" class="text-primary"> {{ storedTaskName }}</span></h3>
                              </div>
                              <div class="q-mb-sm">
                                <q-btn v-if="selectedTaskId!=null" size="sm" icon="o_add" outline label="" no-caps class="text-primary q-mr-xs" style="padding: 3px 7px; min-height: 30px;" @click="onAddInlineTaskActivity(selectedTaskId)">
                                  <q-tooltip>Add Task Activity</q-tooltip>
                                </q-btn>
                                <q-btn v-if="selectedTaskId!=null" size="sm" icon="o_add" outline label="" no-caps class="text-primary q-mr-xs" style="padding: 3px 7px; min-height: 30px;" @click="onAddBulkTaskActivity">
                                  <q-icon size="xs" name="o_view_list" class="cursor-pointer q-mr-xs" />
                                  <q-tooltip>Add/Edit Bulk Activities</q-tooltip>
                                </q-btn>
                              </div>
                            </div>
                            <div class="row full-width justify-between items-center">
                              <div class="col-5">
                                <q-input
                                  v-model="searchProjectActivity.filterActivity"
                                  :loading="searchProjectActivityLoader"
                                  outlined
                                  class="bg-white "
                                  debounce="300"
                                  placeholder="Search Activities"
                                  dense
                                  clearable
                                >
                                  <template #prepend>
                                    <q-icon name="o_search" />
                                  </template>
                                </q-input>
                              </div>
                              <!-- <div class="col-5">
                                <div class="input-group">
                                  <q-input v-model="searchProjectActivity.activityTargetMonthStr" clearable outlined fill-input dense class="q-ml-sm">
                                    <template #append>
                                      <q-icon name="o_calendar_month" class="cursor-pointer">
                                        <q-popup-proxy ref="qDateProxy" v-model="isPopupActivityTargetMonthVisible" transition-show="scale" transition-hide="scale">
                                          <q-date ref="date4ref" v-model="searchProjectActivity.activityTargetMonthStr" default-view="Years" emit-immediately minimal mask="MMMM-YYYY" class="myDate" @update:model-value="onUpdateMv4" />
                                        </q-popup-proxy>
                                      </q-icon>
                                    </template>
                                  </q-input>
                                </div>
                              </div> -->
                              <div class="col-7">
                                <q-checkbox v-model="isPAChecked" label="Show Closed" color="indigo-12" class="q-mr-sm" @click="ShowClosedPA()" />
                                <q-checkbox v-model="isPAComChecked" label="Show Completed" color="indigo-12" class="q-mr-sm" @click="ShowCompletedPA()" />
                              </div>
                            </div>
                          </q-toolbar>
                          <q-table
                            v-model:pagination="projectActivitiesPagination"
                            :loading="loading"
                            :rows="filteredActivities"
                            :columns="projectActivityColumns"
                            row-key="id"
                            separator="cell"
                            binary-state-sort
                            class="Custom-DataTable"
                            no-data-label="No task assignment available"
                            :rows-per-page-options="[20, 50, 100, 200, 500]"
                            table-style="max-height: 65vh"
                            virtual-scroll
                            @request="getActivitiesByTask"
                          >
                            <template #header="props">
                              <q-tr :props="props" class="bg-primary text-white">
                                <!-- <q-th auto-width class="text-center" /> -->
                                <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}<span v-if="['activityOwner'].includes(col.name) || ['activityName'].includes(col.name)" class="required">*</span></q-th>
                                <q-th auto-width class="text-center" />
                              </q-tr>
                            </template>
                            <template #body="props">
                              <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight-row' : ''">
                                <!-- <q-td class="text-center" style="width: 0%; padding:0px">
                                  <q-checkbox
                                    v-model="props.row.checkboxStatus"
                                    :disable="props.row.status == 'Close'"
                                    size="sm"
                                    @update:model-value="onSelectCheckbox(props.row.id, $event)"
                                  />
                                </q-td> -->
                                <q-td style="width: 0% !important;">
                                  <q-tooltip v-if="props.row.activityOwner">
                                    <div>
                                      <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                                      <span>{{ activityOwnerList.find(e => e.value === props.row.activityOwner)?.text || props.row.activityOwner }}</span>
                                    </div>
                                  </q-tooltip>
                                  <span v-if="props.row.activityOwner" class="q-mr-md" style="font-size: 12px;">
                                    <span class="Person">{{ getInitialsOwner(activityOwnerList.find(e => e.value === props.row.activityOwner)?.name || props.row.activityOwner) }}
                                    </span>
                                  </span>
                                  <q-btn v-else size="xs" round icon="o_add" color="white" style="padding: 5px 5px;min-height: 15px;background-color: gray !important">
                                    <q-tooltip>Click to Add</q-tooltip>
                                  </q-btn>
                                  <q-popup-edit
                                    v-slot="scope"
                                    v-model="props.row.activityOwner"
                                    class="small-popup-title"
                                    style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 320px;"
                                    @save="newVal => onSaveInlineActivity('owner', newVal, props.row)"
                                  >
                                    <div class="row justify-between items-center">
                                      <div class="text-subtitle2 q-mb-sm">Activity Owner</div>
                                      <q-btn v-close-popup icon="o_close" size="sm" flat round dense />
                                    </div>

                                    <q-select
                                      v-model="scope.value"
                                      :options="activityOwnerList"
                                      class="w-100 h-auto"
                                      use-input
                                      clearable
                                      outlined
                                      dense
                                      push
                                      emit-value
                                      map-options
                                      option-value="value"
                                      option-label="text"
                                      dropdown-icon="o_arrow_drop_down"
                                      @filter="getownerfilter"
                                    >
                                      <template #option="{ itemProps, opt }">
                                        <q-item v-bind="itemProps">
                                          <q-item-section>
                                            <div class="row q-col-gutter-x-md items-center">
                                              <span>{{ opt.text }}</span>
                                              <q-icon
                                                v-if="opt.description"
                                                name="o_info"
                                                size="17px"
                                                class="q-ml-xs"
                                              >
                                                <q-tooltip v-if="opt.description" class="text-wrap break-words q-pa-sm" max-width="300px">
                                                  <div v-html="opt.description" />
                                                </q-tooltip>
                                              </q-icon>
                                            </div>
                                          </q-item-section>
                                        </q-item>
                                      </template>
                                    </q-select>

                                    <div class="row justify-end q-gutter-sm q-mt-sm">
                                      <q-btn v-close-popup label="Cancel" color="grey" flat dense />
                                      <q-btn
                                        label="Set"
                                        color="primary"
                                        dense
                                        @click="scope.set()"
                                      />
                                    </div>
                                  </q-popup-edit>
                                </q-td>
                                <q-td style="width: 5% !important;">
                                  <span v-if="props.row.activityName" class="q-mr-md row inline items-center no-wrap" style="font-size: 12px;">
                                    <span>
                                    {{ projectActivityList.find(e => e.value === props.row.activityName)?.text || props.row.activityName }}
                                    </span>
                                    <q-icon
                                      v-if="props.row.activityNameDescription"
                                      name="o_info"
                                      size="17px"
                                      class="q-ml-xs"
                                    >
                                      <q-tooltip v-if="props.row.activityNameDescription" class="text-wrap break-words q-pa-sm" max-width="300px">
                                        <div v-html="props.row.activityNameDescription" />
                                      </q-tooltip>
                                    </q-icon>
                                    <q-badge
                                      v-if="props.row.status"
                                      rounded
                                      color="grey"
                                      :label="props.row.status"
                                      class="q-ml-sm"
                                    >
                                      <q-tooltip>Activity Status</q-tooltip>
                                    </q-badge>
                                  </span>
                                  <q-btn v-else size="xs" round icon="o_add" color="white" style="padding: 5px 5px;min-height: 15px;background-color: gray !important">
                                    <q-tooltip>Click to Add</q-tooltip>
                                  </q-btn>
                                  <q-popup-edit
                                    v-slot="scope" v-model="props.row.activityName" class="small-popup-title" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 320px;"
                                    @save="newVal => onSaveInlineActivity('activityName', newVal, props.row)"
                                  >
                                    <div class="row justify-between items-center">
                                      <div class="text-subtitle2 q-mb-sm">Activity Name</div>
                                      <q-btn
                                        v-close-popup
                                        icon="o_close" size="sm"
                                        color="black"
                                        flat
                                        round
                                        dense
                                      />
                                    </div>
                                    <q-select
                                      v-model="scope.value"
                                      :options="projectActivityList"
                                      class="w-100 h-auto"
                                      use-input
                                      clearable
                                      outlined
                                      dense
                                      push
                                      emit-value
                                      map-options
                                      option-value="value"
                                      option-label="text"
                                      dropdown-icon="o_arrow_drop_down"
                                      @filter="getActivitiesDropDownFilter"
                                    >
                                      <template #option="{ itemProps, opt }">
                                        <q-item v-bind="itemProps">
                                          <q-item-section>
                                            <div class="row q-col-gutter-x-md items-center">
                                              <span>{{ opt.text }}</span>
                                              <q-icon
                                                v-if="opt.description"
                                                name="o_info"
                                                size="17px"
                                                class="q-ml-xs"
                                              >
                                                <q-tooltip v-if="opt.description" class="text-wrap break-words q-pa-sm" max-width="300px">
                                                  <div v-html="opt.description" />
                                                </q-tooltip>
                                              </q-icon>
                                            </div>
                                          </q-item-section>
                                        </q-item>
                                      </template>
                                    </q-select>
                                    <!-- Custom buttons -->
                                    <div class="row justify-end q-gutter-sm q-mt-sm">
                                      <q-btn
                                        v-close-popup
                                        label="Cancel"
                                        color="grey"
                                        flat
                                        dense
                                      />
                                      <q-btn
                                        label="Set"
                                        color="primary"
                                        dense
                                        @click="scope.set()"
                                      />
                                    </div>
                                  </q-popup-edit>
                                </q-td>
                                <q-td style="width: 0% !important;" class="text-right">
                                  <span class="" style="font-size: 12px;">{{ props.row.estimateHours }}</span>
                                  <q-popup-edit
                                    v-slot="scope" v-model="props.row.estimateHours" class="small-popup-title" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 270px;"
                                    @save="newVal => onSaveInlineActivity('estimateHours', newVal, props.row)"
                                  >
                                    <div class="row justify-between items-center">
                                      <div class="text-subtitle2 q-mb-sm">Estimate Hrs.</div>
                                      <q-btn
                                        v-close-popup
                                        icon="o_close" size="sm"
                                        color="black"
                                        flat
                                        round
                                        dense
                                      />
                                    </div>
                                    <q-input v-model="scope.value" outlined="" stack-label hide-bottom-space :dense="true" class="w-100 h-auto estimateHours assignto" :rules="[validatePositiveDecimal]" maxlength="5">
                                      <template #prepend>
                                        <q-icon name="o_schedule" size="xs" />
                                      </template>
                                    </q-input>
                                    <div class="row justify-end q-gutter-sm q-mt-sm">
                                      <q-btn
                                        v-close-popup
                                        label="Cancel"
                                        color="grey"
                                        flat
                                        dense
                                      />
                                      <!-- <q-btn
                                        label="Set"
                                        color="primary"
                                        dense
                                        @click="async () => {
                                          const valid = await scope.validate();
                                          if (valid) scope.set();
                                        }"
                                      /> -->
                                      <q-btn
                                        label="Set"
                                        color="primary"
                                        dense
                                        :disable="validatePositiveDecimal(scope.value) !== true"
                                        @click="scope.set()"
                                      />
                                    </div>
                                  </q-popup-edit>
                                  <q-tooltip>Click to edit</q-tooltip>
                                </q-td>
                                <q-td auto-width class="text-center actions" style="width: 0% !important;">
                                  <q-btn v-if="!props.row.newProjectActivity" icon="o_more_vert" dense flat round no-caps class="text-primary btnRounded" size="sm">
                                    <q-menu>
                                      <q-list style="min-width: 130px">
                                        <q-separator />
                                        <q-item v-if="!props.row.newProjectActivity" v-ripple clickable class="" @click="onProjectTaskActivityView(props.row.id)">
                                          <q-item-section avatar><q-icon name="o_visibility" color="" size="xs" /></q-item-section>
                                          <q-item-section class="">View</q-item-section>
                                        </q-item>
                                        <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple clickable class="" @click.stop="onEditTaskActivity(props.row.id)">
                                          <q-item-section avatar><q-icon name="o_edit" color="" size="xs" /></q-item-section>
                                          <q-item-section class="">Edit</q-item-section>
                                        </q-item>
                                        <q-item v-if="props.row.isEditable && props.row.taskStatus != 'Close'" v-ripple clickable>
                                          <q-item-section avatar>
                                            <q-icon name="o_flag" color="secondary" size="xs" />
                                          </q-item-section>
                                          <q-item-section class="">Activity Status</q-item-section>
                                          <q-menu
                                            anchor="top end"
                                            self="top start"
                                            @before-show="() => handleActivityPopupShow(props.row.id, props.row.status, props.row.project.status)"
                                          >
                                            <q-item
                                              v-for="activityStatus in projectActivityStatusMap[props.row.id] || []"
                                              :key="activityStatus.value"
                                              v-ripple
                                              clickable
                                              :disable="activityStatus.text === 'Open' && isDescriptionEmpty(props.row.activityDescription)"
                                              @click="onSubmitActivityStatus(props.row.id, activityStatus.value)"
                                            >
                                              <q-item-section avatar>
                                                <q-icon
                                                  v-if="activityStatus.value === props.row.activityStatusId"
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
                                                {{ activityStatus.text }}
                                              </q-item-section>
                                            </q-item>
                                          </q-menu>
                                        </q-item>
                                        <q-separator />
                                        <q-item v-if="props.row.isEditable && props.row.status != 'Close'" v-ripple v-close-popup clickable @click="onDeleteTaskActivity(props.row)">
                                          <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                                          <q-item-section class="text-negative">Delete</q-item-section>
                                        </q-item>
                                      </q-list>
                                    </q-menu>
                                  </q-btn>
                                  <q-btn v-else size="xs" flat round icon="o_delete_outline" color="negative" class="text-primary btnRounded" @click="onRemoveTaskActivity(props.row)">
                                    <q-tooltip>Delete Row</q-tooltip>
                                  </q-btn>
                                </q-td>
                              </q-tr>
                              <q-tr v-if="props.pageIndex === projectActivities.length - 1">
                                <q-td colspan="2" class="text-right font-bold"><b>Total Hours:</b></q-td>
                                <q-td class="text-right">{{ totalEstimateHours }}</q-td>
                                <q-td />
                              </q-tr>
                            </template>
                          </q-table>
                        </div>
                        <div v-else class="text-grey text-caption q-pa-sm">Select a task to view activities</div>
                      </template>
                    </q-splitter>
                  </template>
                </q-splitter>
              </template>
            </q-splitter>
          </q-page-container>
        </q-layout>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup>
import { ref, onMounted, watch, computed } from "vue";
import { notifySuccess, zwConfirmDelete, zwConfirm, notifyError } from "assets/utils";
import { useQuasar, uid } from "quasar";
import { useAuthStore } from "stores/auth";
import useFilters from "composables/useFilters";

import commonService from "services/common.service";
import allProjectPlannerService from "../allProjectPlanner.service";

import projectService from "modules/project/projects.service";
import TagEditor from "src/modules/project-tasks/components/_taskTagEditor.vue";

import projectModulesService from "modules/project-modules/projectModules.service";

import taskService from "modules/project-tasks/projectTasks.service";

import activitiesService from "modules/project-tasks-activities/projectTasksActivities.service";

// SOP Change :- Shared DataTable Views
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// SOP Change :- Shared Scripts DataTable Features
import useSiteTableState from "composables/datatable/useSiteTableState.js";

// SOP Change :- Shared Dropdowns
import customerModule from "src/modules/customer/utils/dropdowns.js";
import companyContactsModule from "src/modules/company-contacts/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import tagModule from "src/modules/tags/utils/dropdowns.js";

// SOP Change :- Shared Inputs
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectAdd,
  onProjectEdit,
  onProjectView,
  onProjectFilesView,
  onProjectMessage
} from "src/modules/project/utils/dialogs.js";

// Shared Project Module Dialogs
import {
  initProjectModuleDialogs,
  onProjectModuleAdd,
  onProjectModuleEdit,
  onProjectModuleView,
  onProjectModuleCopy,
  onProjectModuleMoveAsProject
} from "src/modules/project-modules/utils/dialogs.js";

// Shared Common Dialogs
import {
  initCommonDialogs,
  onNoteAdd
} from "src/modules/common/utils/dialogs.js";

// Shared Project Task Dialogs
import {
  initProjectTaskDialogs,
  onProjectTaskAdd,
  onProjectTaskEdit,
  onProjectTaskView,
  onProjectTaskFiles,
  onProjectTaskStatusChangeLog,
  onProjectTaskLevelTimeSheetView,
  onProjectTaskCopy,
  onProjectTaskMove,
  onProjectTaskAddBulk,
  onProjectTaskEditBulk
} from "src/modules/project-tasks/utils/dialogs.js";

// Shared Project Task Activity
import {
  initProjectTaskActivityDialogs,
  onProjectTaskActivityView,
  onProjectTaskActivityEdit,
  onProjectTaskAssignment
} from "src/modules/project-tasks-activities/utils/dialogs.js";

const authStore = useAuthStore();
const { toDate } = useFilters();
const $q = useQuasar();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Variable Declarations
// --------------------------------------------------------------------------------------------------------------------------------------------------
const user = authStore.user;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

const currentSiteId = computed(() => user.siteId);
const loading = ref(true);
const leftDrawerOpen = ref(true);
const middleDrawerOpen = ref(true);
const customerSplit = ref(22);
const mainSplit = ref(20);
const ActivitySplit = ref(55);

const selectedModule = ref(null);
const selectedCustomer = ref(null);
const selectedProject = ref(null);
const selectedTask = ref(null);

const showFilter = ref(false);
const showModulesFilter = ref(false);
const showTasksFilter = ref(false);
const searchProjectLoader = ref(false);
const searchProjectModuleLoader = ref(false);
const searchProjectTaskLoader = ref(false);
const searchProjectActivityLoader = ref(false);

let ProjectName;
let ModuleName;
let TaskName;
let ModuleStartDate;
let ModuleEndDate;

const defaultOutlookState = {
  isProject: true,
  isProjectModule: false,
  isProjectTask: false,
  isProjectActivity: false,

  projectId: null,
  projectModuleId: null,
  projectTaskId: null,

  projectName: "",
  moduleName: "",
  taskName: "",
  expandedRowId: null,

  leftDrawerOpen: true,
  middleDrawerOpen: true,

  customerSplit: 22,
  mainSplit: 20,
  ActivitySplit: 55
};

const {
  getTableState: getOutlookState,
  saveState: saveOutlookState
} = useSiteTableState({
  storageKey: "projects-AllProjectPlanner",
  siteId: currentSiteId,
  tableKey: "outlook-View",

  defaultState: defaultOutlookState
});

const filterLocalStorage = getOutlookState();
const expandedRowId = ref(filterLocalStorage ? filterLocalStorage.expandedRowId : null);
const projectId = ref(filterLocalStorage?.projectId || "");
const projectModuleId = ref(filterLocalStorage?.projectModuleId || "");
const projectTaskId = ref(filterLocalStorage?.projectTaskId || "");

const storedProjectName = ref(filterLocalStorage?.projectName || "");
const storedModuleName = ref(filterLocalStorage?.moduleName || "");
const storedTaskName = ref(filterLocalStorage?.taskName || "");

const isProject = ref(filterLocalStorage?.isProject ?? true);
const isProjectModule = ref(filterLocalStorage?.isProjectModule ?? false);
const isProjectTask = ref(filterLocalStorage?.isProjectTask ?? false);
const isProjectActivity = ref(filterLocalStorage?.isProjectActivity ?? false);

const selectedSortByProject = ref(
  filterLocalStorage?.selectedSortByProject || "name"
);

const selectedSortOrderByProject = ref(
  filterLocalStorage?.selectedSortOrderByProject ?? false
);

const selectedSortByModule = ref(
  filterLocalStorage?.selectedSortByModule || "name"
);

const selectedSortOrderByModule = ref(
  filterLocalStorage?.selectedSortOrderByModule ?? false
);

const selectedSortByTask = ref(
  filterLocalStorage?.selectedSortByTask || "taskName"
);

const selectedSortOrderByTask = ref(
  filterLocalStorage?.selectedSortOrderByTask ?? false
);
const selectedProjectId = ref(projectId.value || null);
const selectedModuleId = ref(projectModuleId.value || null);
const selectedTaskId = ref(projectTaskId.value || null);

const routeProjectId = history.state?.projectId;
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Customer Project List
// --------------------------------------------------------------------------------------------------------------------------------------------------
const rows = ref([]);
const filteredProjectRows = ref([]); // Initially, show all rows
const columns = ref([
  { name: "customerName", label: "Customer Name", field: "customerName", align: "left", sortable: true },
  { name: "name", label: "Project Name", field: "name", align: "left", sortable: true }
]);

const {
  search,
  pagination,
  activeRowId,
  saveDataTableState
} = useSiteTableState({
  storageKey: "projects-AllProjectPlanner",
  siteId: currentSiteId,
  tableKey: "dataTable-Projects",

  defaultSearch: {
    searchText: "",
    projectId: "",
    projectIds: [],
    customerIds: [],
    projectTypeIds: [],
    projectStatusIds: [],
    projectCoordinatorIds: [],
    projectLeadsIds: [],
    projectPriorityIds: [],
    companyContactIds: [],
    statusId: null,
    year: ""
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  },

  defaultSorts: {
    selectedSortByProject: "",
    selectedSortOrderByProject: true
  }
});

const getAllCustomerProjectsList = async (props) => {
  loading.value = true;
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...search.value };
  try {
    saveDataTableState({
      search: search.value,

      pagination: {
        ...pagination.value,
        page,
        rowsPerPage,
        sortBy,
        descending
      },

      sorts: {
        selectedSortByProject: selectedSortByProject.value,
        selectedSortOrderByProject: selectedSortOrderByProject.value
      }
    });
    const resp = await allProjectPlannerService.getAllProjectsPlannerList(payload);

    const mappedRows = resp.data.map(project => {
      const hasFullAccess = project?.projectUserMappings[0]?.fullAccess ?? false;
      return {
        ...project,
        isViewOnly: project?.projectUserMappings[0]?.viewOnly ?? false,
        isNotes: project?.projectUserMappings[0]?.notes ?? false,
        isEditable: role === "admin" || hasFullAccess
      };
    });

    // Add showCustomerName flag
    let lastCustomerName = null;
    const rowsWithFlags = mappedRows.map(row => {
      const showCustomerName = row.customerName !== lastCustomerName;
      lastCustomerName = row.customerName;
      return {
        ...row,
        showCustomerName
      };
    });
    rows.value = rowsWithFlags;
    filteredProjectRows.value = rowsWithFlags;
    pagination.value = {
      ...pagination.value,
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    };
    searchProjectLoader.value = false;
  } catch (error) {
    console.error("Error fetching modules:", error);
  } finally {
    loading.value = false;
  }
};

// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const refreshProjectList = () => {
  getAllCustomerProjectsList({ pagination: pagination.value });
};

const refreshProjectModulesList = () => {
  getAllProjectsModulesByProjectId({ pagination: projectModulesPagination.value });
};

const refreshProjectTaskList = () => {
  getTaskListByModuleId({ pagination: projectTasksPagination.value });
};

const refreshProjectTaskActivityList = () => {
  getActivitiesByTask({ pagination: projectActivitiesPagination.value });
};

const refreshProjectNameDropdown = () => {
  loadProjectNameDropdown();
};

const refreshProjectTaskTagsDropdown = () => {
  projectTaskTagsDropdown.load();
};

// Search records as per parameters
const onSearch = () => {
  ProjectModuleRows.value = [];
  projectActivities.value = [];
  projectTasks.value = [];
  selectedProjectId.value = null;
  selectedModuleId.value = null;
  selectedTaskId.value = null;
  refreshProjectList();
};

// Clear search
const onClearProject = () => {
  search.value.searchText = "";
  search.value.projectIds = [];
  search.value.projectTypeIds = [];
  search.value.customerIds = [];
  search.value.year = null;
  search.value.projectTypeIds = [];
  search.value.projectStatusIds = [];
  search.value.projectCoordinatorIds = [];
  search.value.projectLeadsIds = [];
  search.value.projectPriorityIds = [];
  search.value.companyContactIds = [];
  onSearch();
};

function onSortByProject (SortBy, order) {
  pagination.value.sortBy = selectedSortByProject.value;
  pagination.value.descending = selectedSortOrderByProject.value;
  refreshProjectList();
  const updated = {
    ...getOutlookState(),
    selectedSortByProject: selectedSortByProject.value,
    selectedSortOrderByProject: selectedSortOrderByProject.value
  };
  saveOutlookState(updated);
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Module List
// --------------------------------------------------------------------------------------------------------------------------------------------------
const ProjectModuleRows = ref([]);
const filteredProjectModuleRows = ref([]);
const projectModuleColumns = ref([
  { name: "name", label: "Module", field: "name", align: "left", sortable: true }
]);

const {
  search: searchProjectModule,
  pagination: projectModulesPagination,
  activeRowId: activeModuleRowId,

  saveDataTableState: saveProjectModuleTableState
} = useSiteTableState({
  storageKey: "projects-AllProjectPlanner",
  siteId: currentSiteId,

  tableKey: "dataTable-ProjectModules",

  defaultSearch: {
    projectModuleIds: [],
    projectModuleStatusIds: [],

    filterModule: "",
    isShowCloseStatus: false,

    projectId: projectId.value
  },

  defaultPagination: {
    sortBy: "name",
    descending: true,
    rowsPerPage: 20,
    page: 1
  },

  defaultSorts: {
    selectedSortByModule: "",
    selectedSortOrderByModule: true
  }
});
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Checkbox Computed Bindings
// --------------------------------------------------------------------------------------------------------------------------------------------------

const isPMChecked = computed({
  get: () => searchProjectModule.value.isShowCloseStatus,
  set: (value) => {
    searchProjectModule.value.isShowCloseStatus = value;
  }
});

const getAllProjectsModulesByProjectId = async (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...searchProjectModule.value };

  try {
    saveProjectModuleTableState({
      search: searchProjectModule.value,

      pagination: {
        ...projectModulesPagination.value,
        page,
        rowsPerPage,
        sortBy,
        descending
      },

      activeRowId: activeModuleRowId.value,

      sorts: {
        selectedSortByModule: selectedSortByModule.value,
        selectedSortOrderByModule: selectedSortOrderByModule.value
      }
    });
    const resp = await allProjectPlannerService.getAllProjectsModulesPlannerList(payload);
    ProjectModuleRows.value = resp.data.map(projectModule => {
      const hasFullAccess = projectModule?.project?.projectUserMappings[0]?.fullAccess ?? false;
      return {
        ...projectModule,
        isViewOnly: projectModule?.project?.projectUserMappings[0]?.viewOnly ?? false,
        isNotes: projectModule?.project?.projectUserMappings[0]?.notes ?? false,
        isEditable: role === "admin" || hasFullAccess
      };
    });
    filteredProjectModuleRows.value = ProjectModuleRows.value; // Set initial value to all rows
    projectModulesPagination.value = {
      ...projectModulesPagination.value,

      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    };
    searchProjectModuleLoader.value = false;
  } catch (error) {
    console.error("Error fetching modules:", error);
  } finally {
    loading.value = false;
  }
};

async function loadProjectModules (projectId) {
  selectedModule.value = null;
  selectedTask.value = null;

  selectedModuleId.value = null;
  selectedTaskId.value = null;

  projectTasks.value = [];
  projectActivities.value = [];
  filteredActivities.value = [];
  filteredTaskRows.value = [];

  searchProjectModule.value.projectModuleIds = [];
  projectModulesByProjectIdForDropdown.load(false, false, projectId);
  if (showModulesFilter.value === true) {
    showModulesFilter.value = false;
  }
  if (showTasksFilter.value === true) {
    showTasksFilter.value = false;
  }

  isProjectModule.value = true;
  ProjectName = getProjectProjectSingleRow(projectId)?.name;
  selectedProjectId.value = projectId;
  if (projectId) {
    activeRowId.value = projectId;
    searchProjectModule.value.projectId = projectId;
    activityOwnerList.value = [];
    getAllProjectTaskOwnersListForDropdown(selectedProjectId.value);
    projectEmployeesForDropdown.load(selectedProjectId.value);
    try {
      await refreshProjectModulesList();
      storedProjectName.value = ProjectName;
      const updatedLocalStorage = {
        ...getOutlookState(),
        isProjectModule: isProjectModule.value,
        projectId,
        projectName: storedProjectName.value
      };

      saveOutlookState(updatedLocalStorage);
    } catch (error) {
      console.error("Error loading project modules:", error);
    }
  }
}

function onSortByModule (SortBy, order) {
  projectModulesPagination.value.sortBy = selectedSortByModule.value;
  projectModulesPagination.value.descending = selectedSortOrderByModule.value;
  refreshProjectModulesList();
  const updated = {
    ...getOutlookState(),
    selectedSortByModule: selectedSortByModule.value,
    selectedSortOrderByModule: selectedSortOrderByModule.value
  };
  saveOutlookState(updated);
}

const onModuleSearch = () => {
  filteredTaskRows.value = [];
  projectTasks.value = [];
  projectActivities.value = [];
  refreshProjectModulesList();
};

const onClearModule = () => {
  searchProjectModule.value.projectModuleIds = [];
  searchProjectModule.value.projectModuleStatusIds = [];
  onModuleSearch();
};

//  --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Module Task List
// --------------------------------------------------------------------------------------------------------------------------------------------------
const projectTasks = ref([]);
const filteredTaskRows = ref([]);
const projectTaskColumns = ref([
  { name: "taskName", label: "Name", field: "taskName", align: "left", sortable: true },
  { name: "startDate", label: "Start/End Date", field: "startDate", align: "left", sortable: true, headerClasses: "text-center" },
  { name: "assignTo", label: "Owner", field: "assignTo", align: "left", sortable: true },
  { name: "projectTask_Tags", label: "Tags", field: row => row.taskTags, align: "left", sortable: false },
  { name: "estimateTime", label: "Est. Hrs", field: "estimateTime", align: "left", sortable: false }
]);

const {
  search: searchProjectTask,
  pagination: projectTasksPagination,
  activeRowId: activeTaskRowId,
  sorts: taskSorts,

  saveDataTableState: saveProjectTaskTableState
} = useSiteTableState({
  storageKey: "projects-AllProjectPlanner",
  siteId: currentSiteId,
  tableKey: "dataTable-ProjectTasks",

  defaultSearch: {
    name: "",
    statusIds: [],
    priorityIds: [],
    assignedToIds: [],
    taskTagsIds: [],
    filterTask: "",
    isShowCloseStatus: false,
    projectId: projectId.value,
    projectModuleId: projectModuleId.value
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  },

  defaultSorts: {
    selectedSortByTask: "",
    selectedSortOrderByTask: true
  }
});

const isTaskChecked = computed({
  get: () => searchProjectTask.value.isShowCloseStatus,
  set: (value) => {
    searchProjectTask.value.isShowCloseStatus = value;
  }
});

const getTaskListByModuleId = async (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...searchProjectTask.value };

  try {
    saveProjectTaskTableState({
      search: searchProjectTask.value,

      pagination: {
        ...projectTasksPagination.value,
        page,
        rowsPerPage,
        sortBy,
        descending
      },

      activeRowId: activeTaskRowId.value,
      sorts: {
        selectedSortByTask: selectedSortByTask.value,
        selectedSortOrderByTask: selectedSortOrderByTask.value
      }
    });
    const resp = await allProjectPlannerService.getAllProjectsTaskPlannerList(payload);
    projectTasks.value = resp.data.map(task => {
      const hasFullAccess = task?.project?.projectUserMappings[0]?.fullAccess ?? false;
      return {
        ...task,
        id: task.id,
        name: task.taskName,
        // checkboxStatus: false, // Initialize checkboxStatus for each row
        estimateTime: task.estimateTime,
        isViewOnly: task?.project?.projectUserMappings[0]?.viewOnly ?? false,
        isNotes: task?.project?.projectUserMappings[0]?.notes ?? false,
        isEditable: role === "admin" || hasFullAccess,
        startDate: task.startDate ? toDate(task.startDate) : "",
        endDate: task.endDate ? toDate(task.endDate) : "",
        assignedToId: task.assignedToId,
        color: task.color,
        assignedTo: task.assignedTo,
        projectId: task.projectId,
        projectModuleId: task.projectModuleId,
        projectActivities: task.projectActivities ? task.projectActivities.map(activity => ({ ...activity })) : [],
        totalActivitiesCount: task.totalActivitiesCount,
        totalActivityHours: task.totalActivityHours,
        totalCloseActivityHours: task.totalCloseActivityHours,
        taskTags: typeof task.projectTask_Tags === "string"
        ? task.projectTask_Tags
            .split(",")
            .filter(tag => tag.includes(":"))
            .map(tag => {
              const [id, name, color, bgColor] = tag.split(":");

              return {
                text: name?.trim() || "",
                value: id?.trim() || "",
                color: color || "#000000",
                bgColor: bgColor || "#e0e0e0"
              };
            })
        : []
      };
    });
    filteredTaskRows.value = projectTasks.value; // Set initial value to all rows
    projectTasksPagination.value = {
      ...projectTasksPagination.value,
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    };
    searchProjectTaskLoader.value = false;
  } catch (error) {
    console.error("Error fetching tasks:", error);
  } finally {
    loading.value = false;
  }
};

async function LoadTasks (projectId, moduleId, type) {
  if (showModulesFilter.value === true) {
    showModulesFilter.value = false;
  }
  if (showTasksFilter.value === true) {
    showTasksFilter.value = false;
  }
  isProjectTask.value = true;
  projectActivities.value = [];
  filteredActivities.value = [];
  if (moduleId && projectId) {
    activeModuleRowId.value = moduleId;
    ModuleName = ProjectModuleRows.value.find(item => item.id === moduleId)?.name;
    ModuleStartDate = ProjectModuleRows.value.find(item => item.id === moduleId)?.startDate;
    ModuleEndDate = ProjectModuleRows.value.find(item => item.id === moduleId)?.endDate;
    storedModuleName.value = ModuleName;
    ProjectName = rows.value.find(item => item.id === projectId)?.name;
    storedProjectName.value = ProjectName;
    searchProjectTask.value.projectModuleId = moduleId;
    searchProjectTask.value.projectId = projectId;
    selectedModuleId.value = moduleId;
    selectedModule.value = ProjectModuleRows.value.find(item => item.id === selectedModuleId.value);
    selectedProjectId.value = projectId;
  } else if (projectId) {
    ModuleName = "";
    ProjectName = rows.value.find(item => item.id === projectId)?.name;
    storedProjectName.value = ProjectName;
    searchProjectTask.value.projectModuleId = "";
    searchProjectTask.value.projectId = projectId;
  }
  selectedProjectId.value = projectId;

  const updated = {
    ...getOutlookState(),
    isProject: isProject.value,
    isProjectModule: isProjectModule.value,
    isProjectTask: isProjectTask.value,
    isProjectActivity: isProjectActivity.value,
    projectId: selectedProjectId.value,
    projectModuleId: selectedModuleId.value,
    projectTaskId: selectedTaskId.value,
    moduleName: ModuleName,
    expandedRowId: expandedRowId.value
  };
  saveOutlookState(updated);

  try {
    await refreshProjectTaskList();
  } catch (error) {
    console.error("Error loading tasks:", error);
  }
}

const onTaskSearch = () => {
  projectActivities.value = [];
  refreshProjectTaskList();
};

const onClearTask = () => {
  searchProjectTask.value.statusIds = [];
  searchProjectTask.value.priorityIds = [];
  searchProjectTask.value.name = "";
  searchProjectTask.value.taskTagsIds = [];
  searchProjectTask.value.assignedToIds = [];
  onTaskSearch();
};

function onSortByTask (SortBy, order) {
  projectTasksPagination.value.sortBy = selectedSortByTask.value;
  projectTasksPagination.value.descending = selectedSortOrderByTask.value;
  refreshProjectTaskList();
  const updated = {
    ...getOutlookState(),
    selectedSortByTask: selectedSortByTask.value,
    selectedSortOrderByTask: selectedSortOrderByTask.value
  };
  saveOutlookState(updated);
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Task Activity List
// --------------------------------------------------------------------------------------------------------------------------------------------------

// const activityTargetMonthStr = ref(null);
// activityTargetMonthStr.value = getCurrentMonthYear();
const filteredActivities = ref([]);
const projectActivities = ref([]);
const projectActivityColumns = ref([
  { name: "activityOwner", label: "Owner", field: "activityOwner", align: "left", sortable: true },
  { name: "activityName", label: "Name", field: "activityName", align: "left", sortable: true },
  { name: "estimateHours", label: "Est. Hrs.", field: "estimateHours", align: "left", sortable: true, headerClasses: "text-right" }
]);

const {
  search: searchProjectActivity,
  pagination: projectActivitiesPagination,

  saveDataTableState: saveProjectActivityTableState
} = useSiteTableState({
  storageKey: "projects-AllProjectPlanner",
  siteId: currentSiteId,
  tableKey: "dataTable-ProjectActivities",

  defaultSearch: {
    filterActivity: "",
    isShowCloseStatus: false,
    isShowCompleteStatus: false,
    projectId: projectId.value,
    projectModuleId: projectModuleId.value,
    projectTaskId: projectTaskId.value
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

const isPAChecked = computed({
  get: () => searchProjectActivity.value.isShowCloseStatus,
  set: (value) => {
    searchProjectActivity.value.isShowCloseStatus = value;
  }
});

const isPAComChecked = computed({
  get: () => searchProjectActivity.value.isShowCompleteStatus,
  set: (value) => {
    searchProjectActivity.value.isShowCompleteStatus = value;
  }
});

const getActivitiesByTask = async (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, ...searchProjectActivity.value };

  try {
    saveProjectActivityTableState({
      search: searchProjectActivity.value,
      pagination: {
        ...projectActivitiesPagination.value,
        page,
        rowsPerPage,
        sortBy,
        descending
      }
    });
    const resp = await allProjectPlannerService.getAllProjectsActivityPlannerList(payload);
    projectActivities.value = resp.data;
    projectActivities.value = resp.data.map(activity => {
      const hasFullAccess = activity?.project?.projectUserMappings[0]?.fullAccess ?? false;
      return {
        ...activity,
        // checkboxStatus: false, // Initialize checkbox state
        isViewOnly: activity?.project?.projectUserMappings[0]?.viewOnly ?? false,
        isNotes: activity?.project?.projectUserMappings[0]?.notes ?? false,
        isEditable: role === "admin" || hasFullAccess
      };
    });
    isDisabled = false; // Reset the isDisabled flag
    filteredActivities.value = projectActivities.value; // Set initial value to all rows

    projectActivitiesPagination.value = {
      ...projectActivitiesPagination.value,
      page,
      rowsPerPage,
      sortBy,
      descending,
      rowsNumber: resp.total
    };
    searchProjectActivityLoader.value = false;
  } catch (error) {
    console.error("Error fetching activities:", error);
  } finally {
    loading.value = false;
  }
};

let isDisabled;
const taskIds = ref([]);

async function LoadTaskActivities (projectTaskId) {
  isDisabled = false;
  isProjectActivity.value = true;

  const projectTaskRow = getProjectTaskSingleRow(projectTaskId);
  TaskName = projectTaskRow?.name;

  if (projectTaskId) {
    activeTaskRowId.value = projectTaskId;
    selectedTaskId.value = projectTaskId;
    selectedTask.value = projectTaskRow;
    searchProjectActivity.value.projectId = selectedProjectId.value;
    searchProjectActivity.value.projectModuleId = selectedModuleId.value;
    searchProjectActivity.value.projectTaskId = projectTaskId;
    storedTaskName.value = TaskName;
    try {
      await refreshProjectTaskActivityList();
      // Save the updated state to local storage
      const updated = {
        ...getOutlookState(),
        isProject: isProject.value,
        isProjectModule: isProjectModule.value,
        isProjectTask: isProjectTask.value,
        isProjectActivity: isProjectActivity.value,
        projectId: selectedProjectId.value,
        projectModuleId: selectedModuleId.value,
        projectTaskId: selectedTaskId.value,
        taskName: TaskName,
        expandedRowId: expandedRowId.value

      };
      saveOutlookState(updated);

      taskIds.value = [];
    } catch (error) {
      console.error("Error loading task activities:", error);
    }
  }
}

const totalEstimateHours = computed(() => {
  return filteredActivities.value.reduce((sum, row) => {
    const hrs = Number(row.estimateHours) || 0;
    return sum + hrs;
  }, 0);
});

const isDescriptionEmpty = (desc) => {
  if (!desc) return true;
  // remove HTML tags
  let plainText = desc.replace(/<[^>]*>/g, "");
  // decode HTML entities like &nbsp;
  plainText = plainText.replace(/&nbsp;/gi, " ");
  return plainText.trim() === "";
};

const onAddInlineTaskActivity = (selectedTaskId) => {
  // if (isEmpty(activityTargetMonthStr.value)) {
  //   notifyError({ message: "Please select a target month." });
  //   return;
  // }
  if (selectedTaskId === "") {
    notifyError({ message: "Please select a task." });
    return;
  }
  if (!isDisabled) {
    const newActivty = {
      // checkboxStatus: false,
      id: uid(),
      activityName: null,
      projectId: selectedProjectId.value,
      projectModuleId: selectedModuleId.value,
      estimateHours: 0.00,
      assignedToId: null,
      // targetMonthStr: activityTargetMonthStr.value,
      activityStatus: { dropDownValue: "New" },
      taskId: selectedTaskId,
      newProjectActivity: true,
      isDisabled: true
    };
    isDisabled = newActivty.isDisabled;
    projectActivities.value.unshift(newActivty);
  } else {
    notifyError({ message: "Please check required field." });
  }
};

function onSaveInlineActivity (type, value, row) {
  if (type === "activityName") {
    row.activityName = value;
  } else if (type === "owner") {
    row.assignedToId = value;
  } else if (type === "estimateHours") {
    row.estimateHours = value;
  }
  if (isEmpty(row.activityName) || isEmpty(row.assignedToId)) {
    // notifyError({ message: "Please check required fields: Activity Name and Owner are required." });
  } else {
    activitiesService.addProjectActivity(row).then(resp => {
      refreshProjectTaskList();
      refreshProjectTaskActivityList();
      loading.value = false;
      notifySuccess({ message: "Activity saved." });
    });
  }
}

const onRemoveTaskActivity = (row) => {
  isDisabled = false; // Reset the isDisabled flag
  projectActivities.value.find((item, index) => {
    if (item.id === row.id) {
      projectActivities.value.splice(index, 1);
      return true;
    }
    return false;
  });
};

// update activity status
function onSubmitActivityStatus (id, activityStatusId) {
  const payload = {
    activityIds: [id],
    activityStatusId
  };
  setTimeout(function () {
    activitiesService.updateTaskActivityStatus(payload).then(resp => {
      notifySuccess({ message: "Activity status is saved successfully." });
      refreshProjectTaskActivityList();
    });
  });
}

const projectActivityStatusMap = ref({});
async function handleActivityPopupShow (rowId, activityStatusLabel, projectStatusLabel) {
  if (projectActivityStatusMap.value[rowId]) return;

  if (!dropdownMasterCache.value["Activity Status"]) {
    await getDropDownTaskStatus("Activity Status");
  }

  projectActivityStatusMap.value[rowId] =
    buildStatusList("Activity Status", activityStatusLabel, projectStatusLabel);
}

const prevState = ref({
  selectedCustomer: null,
  selectedProject: null,
  selectedModule: null,
  selectedTask: null
});

const isPopupVisible = ref(false);
const onlyYears = (date) => { return true; };
const onUpdateMv2 = (val) => {
  search.value.year = val; // Update the reactive property with the selected year
  isPopupVisible.value = false; // Close the popup
  refreshProjectList();
  filteredProjectModuleRows.value = [];
  ProjectModuleRows.value = [];
  filteredProjectModuleRows.value = [];
  projectTasks.value = [];
  filteredTaskRows.value = [];
  projectActivities.value = [];
  filteredActivities.value = [];
  saveOutlookState({
    ...getOutlookState(),
    year: val
  });
};

const onClearYear = () => {
  search.value.year = ""; // Clear the year in the search model
  refreshProjectList(); // Reload the project planner list
  filteredProjectModuleRows.value = [];
  ProjectModuleRows.value = [];
  filteredProjectModuleRows.value = [];
  projectTasks.value = [];
  filteredTaskRows.value = [];
  projectActivities.value = [];
  filteredActivities.value = [];
  saveOutlookState({
    ...getOutlookState(),
    year: ""
  });
};

function getInitials (activity) {
  if (!activity.firstName || !activity.lastName) return "";
  return activity.firstName[0].toUpperCase() + activity.lastName[0].toUpperCase();
}

function getUniqueActivities (activities) {
  const seen = new Set();
  return activities.filter(activity => {
    const initials = getInitials(activity);
    if (seen.has(initials)) {
      return false;
    }
    seen.add(initials);
    return true;
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// filter task by month
// --------------------------------------------------------------------------------------------------------------------------------------------------

const date3ref = ref(null);
// --------------------------------------------------------------------------------------------------------------------------------------------------
// filter task activity by month
// --------------------------------------------------------------------------------------------------------------------------------------------------
function getProjectProjectSingleRow (id) {
  const projectRow = filteredProjectRows.value.find(item => item.id === id);
  if (projectRow) {
    return projectRow;
  }
  return null;
}

function getProjectTaskSingleRow (id) {
  const projectTaskRow = projectTasks.value.find(item => item.id === id);
  if (projectTaskRow) {
    return projectTaskRow;
  }
  return null;
}

// ------------------------------------------------------------------------------------
// DataTable :- Add Project Task Tags & Save Form
// ------------------------------------------------------------------------------------
const onSubmitProjectTaskTags = async (names, rowId) => {
  model.value.taskIds = [rowId];
  model.value.flag = null;
  model.value.tagsNameList = names.map(tag => tag.text);

  try {
    await taskService.saveTags(model.value);
    notifySuccess({ message: "Tag saved successfully." });
    getAllTagsForDropdown();
    refreshProjectTaskTagsDropdown();
    tagValues.value[rowId] = [];
  } finally {
    loading.value = false;
  }
};

// ------------------------------------------------------------------------------------
// DataTable :- Remove Project Task Tags & Save Form
// ------------------------------------------------------------------------------------

const onDeleteProjectTaskTag = (row, tagToRemove) => {
  zwConfirmDelete(
    {
      message: "Are you sure you want to remove the tag?"
    },
    () => {
      const updatedTags = row.taskTags.filter(
        t => t.value !== tagToRemove.value
      );
      row.taskTags = updatedTags;
      onSubmitProjectTaskTags(updatedTags, row.id);
    }
  );
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Popups
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onDelete = (item) => {
  zwConfirmDelete({ data: `${item.name}` }, () => {
    projectService.deleteProject(item.id).then(resp => {
      notifySuccess({ message: "Project is deleted successfully." });
      refreshProjectList();
    });
  }, () => {
  });
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Module Popups
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onAddProjectModule = () => {
  const projectId = searchProjectModule.value?.projectId;
  const projectName = getProjectProjectSingleRow(projectId)?.name;

  onProjectModuleAdd(
    projectId,
    projectName,
    refreshProjectModulesList
  );
};

const onDeleteModule = async (item) => {
  activeRowId.value = item.id;
  try {
    const resp = await projectModulesService.checkModuleCanBeDeleted(item.id);
    const canDelete = resp?.canDelete;
    if (canDelete) {
      // Normal delete confirmation
      zwConfirmDelete({ data: `${item.name}, ${item.project.name}` }, () => {
        projectModulesService.deleteProjectModule(item.id).then(() => {
          notifySuccess({ message: "Project module deleted successfully." });
          refreshProjectModulesList();
        });
      });
    } else {
      // Warning confirmation
      zwConfirm({
        title: "Active Tasks or Activities Found",
        message: "This module has active tasks or activities. You cannot delete it.",
        data: `${item.name}`,
        okLabel: "OK",
        cancel: false
      }, () => {
        refreshProjectModulesList();
      });
    }
  } catch (error) {
    console.error("Error checking module:", error);
  } finally {
    activeRowId.value = null;
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Task Popups
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onAddProjectTask = (selectedProjectId) => {
  const projectId = selectedProjectId;
  const moduleId = selectedModuleId.value;
  const startDate = ModuleStartDate;
  const endDate = ModuleEndDate;

  onProjectTaskAdd(
    refreshProjectTaskList,
    refreshProjectModulesList,
    projectId,
    moduleId,
    startDate,
    endDate
  );
};

const onDeleteTask = async (item) => {
  activeRowId.value = item.id;
  try {
    const resp = await taskService.checkTaskCanBeDeleted(item.id);
    const canDelete = resp?.canDelete;
    if (canDelete) {
      // Normal delete confirmation
      zwConfirmDelete({ data: `${item.name}` }, () => {
        loading.value = true;
        taskService.deleteProjectTask(item.id).then(resp => {
          notifySuccess({ message: "Task is deleted successfully." });
          refreshProjectTaskList();
          loading.value = false;
        });
      });
    } else {
      // Warning confirmation
      zwConfirm({
        title: "Active Activities Found",
        message: "This task has active activities. You cannot delete it.",
        data: `${item.name}`,
        okLabel: "OK",
        cancel: false
      }, () => {
        refreshProjectTaskList();
      });
    }
  } catch (error) {
    console.error("Error checking module:", error);
  } finally {
    activeRowId.value = null;
  }
};

const onAddBulkTask = () => {
  const projectId = selectedProjectId.value;
  const moduleId = selectedModuleId.value;
  const startDate = ModuleStartDate;
  const endDate = ModuleEndDate;

  onProjectTaskAddBulk(
    projectId,
    moduleId,
    startDate,
    endDate,
    refreshProjectTaskList,
    refreshProjectModulesList
  );
};

const onEditBulkTasks = () => {
  const projectId = selectedProjectId.value;
  const moduleId = selectedModuleId.value;

  onProjectTaskEditBulk(
    projectId,
    moduleId,
    refreshProjectTaskList,
    refreshProjectModulesList
  );
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Activity Popups
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onEditTaskActivity = (id) => {
  onProjectTaskActivityEdit(
    id,
    selectedProjectId.value,
    selectedModuleId.value,
    TaskName,
    ProjectName,
    ModuleName,
    false,
    refreshProjectTaskActivityList
  );
};

const onAddBulkTaskActivity = (id) => {
  onProjectTaskAssignment(
    selectedTaskId.value,
    selectedProjectId.value,
    selectedModuleId.value,
    TaskName,
    ProjectName,
    ModuleName,
    "Add Bulk Activities",
    refreshProjectTaskActivityList
  );
};

const onDeleteTaskActivity = (item) => {
  zwConfirmDelete({ data: `${item.activityName}` }, () => {
    loading.value = true;
    activitiesService.deleteProjectActivity(item.id).then(resp => {
      notifySuccess({ message: "Activity is deleted successfully." });
      LoadTaskActivities(selectedTaskId.value);
      loading.value = false;
    });
  }, () => {
    loading.value = false;
  });
};

// const onSelectCheckbox = (itemId, flag) => {
//   if (flag === true) {
//     // Add the itemId to the ProjectIds array if it's not already present
//     if (!taskIds.value.includes(itemId)) {
//       taskIds.value.push(itemId);
//     }
//   } else {
//     // Find the index of the itemId in the ProjectIds array and remove it
//     const index = taskIds.value.indexOf(itemId);
//     if (index !== -1) {
//       taskIds.value.splice(index, 1); // Remove the item at the found index
//     }
//   }
// };

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update Project Status
// --------------------------------------------------------------------------------------------------------------------------------------------------
function onStatusSubmit (id, statusId) {
  setTimeout(function () {
    projectService.updateProjectStatus(id, statusId).then(resp => {
      notifySuccess({ message: "Project status is saved successfully." });
      refreshProjectList();
    });
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update Project Priority
// --------------------------------------------------------------------------------------------------------------------------------------------------
function onPrioritySubmit (id, statusId) {
  setTimeout(function () {
    projectService.updateProjectPriority(id, statusId).then(resp => {
      notifySuccess({ message: "Project priority is saved successfully." });
      refreshProjectList();
    });
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update Project Pinned
// --------------------------------------------------------------------------------------------------------------------------------------------------
function isPinned (id, status) {
  setTimeout(function () {
    projectService.updateProjectIsPinned(id, status).then(resp => {
      notifySuccess({ message: `Project is ${status ? "pinned" : "unpinned"}.` });
      refreshProjectList();
    });
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update Project Color
// --------------------------------------------------------------------------------------------------------------------------------------------------

let isSliderActive = false;

const startColorSelection = () => {
  isSliderActive = true; // The user is select with the slider
};

const finalizeColorSelection = (row, type) => {
  if (!isSliderActive) return false;
  // isSliderActive = true; // Reset the flag
  isSliderActive = false;

  // Check if the color has changed before submitting
  // if (isSliderActive) {
    if (type === "Project") {
      if (row.projectColor !== previousColor.value) {
        onSubmitColor(row.id, row.projectColor);
        return true;
      }
    } else if (type === "Task") {
      if (row.color !== previousColor.value) {
        onSubmitTaskColor(row.id, row.color);
        return true;
      }
    }
  // }
  return false;
};

const onSubmitColor = (id, projectColor) => {
  const payload = { id, projectColor };
  setTimeout(() => {
    projectService.updateProjectColor(id, payload).then(resp => {
      notifySuccess({ message: "Color updated successfully." });
      refreshProjectList();
    });
  });
};

const previousColor = ref(""); // Store previous color
const storePreviousColor = (row) => {
  previousColor.value = row.projectColor ? row.projectColor : "#e0e0e0"; // Store previous color before opening picker
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update Project Active/Inactive Status
// --------------------------------------------------------------------------------------------------------------------------------------------------
const toggleActiveStatus = (row) => {
  const isCurrentlyActive = row.active === true;
  const newStatus = !isCurrentlyActive; // Toggle the status
  const payload = { id: row.id, activeStatus: newStatus ? "Active" : "Inactive" };

  $q.dialog({
    title: "Confirmation",
    message: `Are you sure you want to ${isCurrentlyActive ? "deactivate" : "activate"} this project?`,
    ok: { label: "Yes", color: "primary" },
    cancel: { label: "No", color: "negative" }
  }).onOk(() => {
    projectService.updateProjectColor(row.id, payload)
      .then(() => {
        notifySuccess({ message: `Project has been ${newStatus ? "activated" : "deactivated"} successfully.` });
        row.active = newStatus ? "Active" : "Inactive";
        refreshProjectList();
      })
      .catch(() => {
        $q.notify({
          type: "negative",
          message: `Failed to ${newStatus ? "activate" : "deactivate"} the project.`
        });
      });
  });
};

const getProjectDetail = (projectId) => {
  return rows.value.find(item => item.id === projectId);
};

function selectProject (customer, project) {
  selectedCustomer.value = customer;
  selectedProject.value = project;
  selectedModule.value = null;
  selectedTask.value = null;
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update Project Task Color
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onSubmitTaskColor = (id, color) => {
  const payload = { id, color };
  setTimeout(() => {
    taskService.updateTaskColor(id, payload).then(resp => {
      notifySuccess({ message: "Color updated successfully." });
      refreshProjectList();
    });
  });
};

// function shouldDisableSelect (row) {
//   const selectedStatus = projectTaskStatusList.value.find(item => item.value === row.status.id);
//   return selectedStatus?.text === "Done";
// }
function shouldDisableSelect (row) {
  const taskList = projectTaskStatusMap.value[row.id] || [];
  const selectedStatus = taskList.find(item => item.value === row.status?.id);
  return selectedStatus?.text === "Done";
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update Project Task Status
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onSubmitTaskStatus = async (id, statusId) => {
   const selected = projectTaskStatusForDropdown.list?.value?.find(
    item => item.value === statusId
  );
  try {
    if (selected?.text.toLowerCase() === "close") {
      const resp = await taskService.checkTaskCanBeDeleted(id);
      const canDelete = resp?.canDelete;
      if (canDelete) {
        const payload = {
          taskIds: [id],
          statusId
        };
        setTimeout(function () {
          taskService.updateProjectTaskStatus(payload).then(resp => {
            notifySuccess({ message: "Task status is saved successfully." });
            refreshProjectTaskList();
          });
        });
      } else {
      // Warning confirmation
        zwConfirm({
          title: "Active Activities Found",
          message: "This task has active activities. You cannot close it.",
          okLabel: "OK",
          cancel: false
        }, () => {
          refreshProjectTaskList();
        });
      }
    } else {
      const payload = {
        taskIds: [id],
        statusId
      };
      setTimeout(function () {
        taskService.updateProjectTaskStatus(payload).then(resp => {
          notifySuccess({ message: "Task status is saved successfully." });
          refreshProjectTaskList();
        });
      });
    }
  } catch (error) {
    console.error("Error checking module:", error);
  }
};
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update Project Task Priority
// --------------------------------------------------------------------------------------------------------------------------------------------------
function onTaskPrioritySubmit (id, priorityId) {
  const payload = {
    taskIds: [id],
    priorityId
  };
  setTimeout(function () {
    taskService.updateProjectTaskPriority(payload).then(resp => {
      notifySuccess({ message: "Task priority is saved successfully." });
      refreshProjectTaskList();
    });
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Update Project Task Owner
// --------------------------------------------------------------------------------------------------------------------------------------------------
function onSubmitOwnerData (id, assignedToId) {
  loading.value = true;
  taskService.updateTaskOwner(id, assignedToId).then(resp => {
    notifySuccess({ message: "Task Owner is saved successfully." });
    loading.value = false;
    refreshProjectTaskList();
  });
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Outlook view drawers
// --------------------------------------------------------------------------------------------------------------------------------------------------
function leftDrawer () {
  // Save current state before closing
  prevState.value.selectedCustomer = selectedCustomer.value;
  prevState.value.selectedProject = selectedProject.value;
  prevState.value.selectedModule = selectedModule.value;
  prevState.value.selectedTask = selectedTask.value;

  leftDrawerOpen.value = false;
}

function toggleLeftDrawer () {
  if (leftDrawerOpen.value) {
    // Save current state before closing
    prevState.value.selectedCustomer = selectedCustomer.value;
    prevState.value.selectedProject = selectedProject.value;
    prevState.value.selectedModule = selectedModule.value;
    prevState.value.selectedTask = selectedTask.value;

    leftDrawerOpen.value = false;
    middleDrawerOpen.value = false;
    customerSplit.value = 0;
    mainSplit.value = 0;
  } else {
    // Reopen and restore previous state
    leftDrawerOpen.value = true;
    middleDrawerOpen.value = true;
    customerSplit.value = 20; // reset to default
    mainSplit.value = 30;

    selectedCustomer.value = prevState.value.selectedCustomer;
    selectedProject.value = prevState.value.selectedProject;
    selectedModule.value = prevState.value.selectedModule;
    selectedTask.value = prevState.value.selectedTask;
  }
}

function toggleMiddleDrawer () {
  if (middleDrawerOpen.value) {
    prevState.value.selectedCustomer = selectedCustomer.value;
    prevState.value.selectedProject = selectedProject.value;
    prevState.value.selectedModule = selectedModule.value;
    prevState.value.selectedTask = selectedTask.value;

    leftDrawerOpen.value = false;
    // selectedProject.value = null;
    customerSplit.value = 0;
    mainSplit.value = 0;
  } else {
    leftDrawerOpen.value = true;
    selectedCustomer.value = prevState.value.selectedCustomer;
    selectedProject.value = prevState.value.selectedProject;
    selectedModule.value = prevState.value.selectedModule;
    selectedTask.value = prevState.value.selectedTask;
    customerSplit.value = 20;
    mainSplit.value = 30;
  }
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// GET Status Color
// --------------------------------------------------------------------------------------------------------------------------------------------------
function getStatusColor (statusText) {
  if (statusText) {
    switch (statusText) {
    case "Open":
      return "purple-4";
    case "Cancelled":
      return "grey-4";
    case "Completed":
      return "green-4";
    case "In progress":
      return "yellow-4";
    case "In Support":
      return "amber-4";
    case "Loss":
      return "deep-orange-4";
    case "New":
      return "blue-4";
    case "On Hold":
      return "brown-4";
    case "Presales":
      return "deep-orange-4";
    case "Sales-POC":
      return "blue-grey-4";
    case "Won":
      return "light-green-13";
    default:
      return "#ffffff";
    }
  }
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// GET Initials of Activtiy Owner
// --------------------------------------------------------------------------------------------------------------------------------------------------

function getInitialsOwner (fullName) {
  return fullName
    .split(" ")
    .map(word => word[0])
    .join("")
    .toUpperCase();
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Validation for empty field
// --------------------------------------------------------------------------------------------------------------------------------------------------
function isEmpty (value) {
  return value === undefined || value === null || String(value).trim() === "";
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Functions to check closed Module/Task/Activity
// --------------------------------------------------------------------------------------------------------------------------------------------------
const ShowClosedPM = () => {
  refreshProjectModulesList();
};

const ShowClosedTask = () => {
  refreshProjectTaskList();
};

const ShowClosedPA = () => {
  refreshProjectTaskActivityList();
};

const ShowCompletedPA = () => {
  refreshProjectTaskActivityList();
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// All Dropdowns
// --------------------------------------------------------------------------------------------------------------------------------------------------

const projectTaskStatusMap = ref({});
async function handleTaskPopupShow (rowId, taskStatusLabel, projectStatusLabel) {
  if (projectTaskStatusMap.value[rowId]) return;

  if (!dropdownMasterCache.value["Task Status"]) {
    await getDropDownTaskStatus("Task Status");
  }

  projectTaskStatusMap.value[rowId] =
    buildStatusList("Task Status", taskStatusLabel, projectStatusLabel);
}

const dropdownMasterCache = ref({});
async function getDropDownTaskStatus (typeName) {
  if (dropdownMasterCache.value[typeName]) return;

  const resp = await commonService.getDropDown(typeName);

  dropdownMasterCache.value[typeName] = resp
    .map(item => ({
      text: item.dropdownValue,
      value: item.id
    }))
    .sort((a, b) => a.text.localeCompare(b.text));
}

function buildStatusList (typeName, rowStatusLabel, projectStatusLabel) {
  const masterList = dropdownMasterCache.value[typeName] || [];

  const lockedStatuses = ["Cancelled", "Completed", "On Hold"];

  return masterList.map(item => {
    let shouldDisable = false;

    if (lockedStatuses.includes(projectStatusLabel) && rowStatusLabel === "New") {
      shouldDisable = item.text === "Open";
    }

    if (projectStatusLabel === "New") {
      shouldDisable = item.text === "Open";
    }

    return {
      ...item,
      disable: shouldDisable
    };
  });
}

const projectActivityList = ref([]);
const projectActivityListFilter = ref([]);
function getActivitiesDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.dropdownValue, description: item.description }));
    projectActivityList.value = responseData;
    projectActivityListFilter.value = responseData;
  });
}
function getActivitiesDropDownFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectActivityList.value = projectActivityListFilter.value;
    } else {
      projectActivityList.value = projectActivityListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all Activity Owner list
const activityOwnerList = ref([]);
const activityOwnerListFilter = ref([]);
function getAllProjectEmployeesListForDropdown (projectId) {
  projectService.getProjectCharterEmployeesWithWeeklyPlanHoursByProjectId(projectId).then((resp) => {
    const responseData = resp.map((item) => {
      // Build weekend breakdown string
      const weekendHours = item.employee.employeeAssignedHours
        ? item.employee.employeeAssignedHours
          .map(h => {
            const date = new Date(h.weekendDate).toLocaleDateString("en-US", {
              month: "2-digit",
              day: "2-digit"
            });
            return `${date}-${h.totalHours}`;
          })
          .join("; ")
        : "0";

      return {
        text: `${item.employee.person.fullName} (${weekendHours})`,
        value: item.employee.id,
        name: `${item.employee.person.fullName}`
      };
    });
    activityOwnerList.value = responseData;
    activityOwnerListFilter.value = responseData;

    // const responseData1 = resp.map((item) => ({ text: item.employee.person.fullName, value: item.employee.id }));
    // taskOwnerList.value = responseData1;
    // taskOwnerListFilter.value = responseData1;
  });
}
// Search  Activity Owner for dropdown
function getownerfilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      activityOwnerList.value = activityOwnerListFilter.value;
      taskOwnerList.value = taskOwnerListFilter.value;
    } else {
      activityOwnerList.value = activityOwnerListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
      taskOwnerList.value = taskOwnerListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all Task Owner list
const taskOwnerList = ref([]);
const taskOwnerListFilter = ref([]);
function getAllProjectTaskOwnersListForDropdown (projectId) {
  projectService.getProjectEmployees(projectId).then((resp) => {
    const responseData1 = resp.map((item) => ({ text: item.text, value: item.value }));
    taskOwnerList.value = responseData1;
    taskOwnerListFilter.value = responseData1;
  });
}

// Search Task Owner for dropdown
function getTaskOwnerFilterForDropdown (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      taskOwnerList.value = taskOwnerListFilter.value;
    } else {
      taskOwnerList.value = taskOwnerListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const availableTags = ref([]);
const availableTagsOptiobns = ref([]);
const tagValues = ref({});
const model = ref({ taskIds: [], tagsNameList: [], color: "" });

async function getAllTagsForDropdown () {
  taskService.getAllTagsListForDropdown().then((resp) => {
    const responseData = resp.filter(item => item && item.name).map((item) => ({ text: item.name, value: item.id, bgColor: item.bgColor || "primary", color: item.color || "#191919" })).sort((a, b) => a.text.localeCompare(b.text));
    availableTags.value = responseData;
    availableTagsOptiobns.value = responseData;
  });
}

const statusListForSearch = ref([]);
function getProjectStatus (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    statusListForSearch.value = responseData;
  });
}

const projectStatusMap = ref({});
function handlePopupShow (rowId, currentStatusLabel) {
  getDropDown("Project Status", currentStatusLabel).then((list) => {
    projectStatusMap.value[rowId] = list;
  });
}

function getDropDown (typeName, currentStatusLabel = null) {
  return commonService.getDropDown(typeName).then((resp) => {
    const allowedTransitions = {
      New: ["Open", "On Hold", "Cancelled"],
      Open: ["On Hold", "Cancelled"],
      "In progress": ["On Hold", "Completed"],
      "On Hold": ["Open", "Cancelled", "Completed"]
    };

    return resp.map((item) => {
      const label = item.dropdownValue;
      const isAllowed = allowedTransitions[currentStatusLabel]?.includes(label);
      const shouldDisable = currentStatusLabel === "Cancelled" || currentStatusLabel === "Completed"
        ? true
        : currentStatusLabel
          ? !isAllowed
          : false;

      return {
        text: label,
        value: item.id,
        disable: shouldDisable
      };
    });
  });
}

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------

initProjectDialogs(activeRowId);
initProjectModuleDialogs(activeRowId);
initCommonDialogs(activeRowId);
initProjectTaskDialogs(activeRowId);
initProjectTaskActivityDialogs(activeRowId);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Run functions sequentially
// --------------------------------------------------------------------------------------------------------------------------------------------------

async function runSequentially () {
  try {
    const savedState = getOutlookState() || {};

    // --------------------------------------------------------------------------
    // Route Project
    // --------------------------------------------------------------------------
    if (routeProjectId) {
      search.value.projectIds = [routeProjectId];

      searchProjectModule.value.projectId =
        routeProjectId;

      selectedProjectId.value =
        routeProjectId;
    }

    // --------------------------------------------------------------------------
    // Load Project Status Dropdown
    // --------------------------------------------------------------------------
    await projectActiveInActiveDropdown.load(
      "Project Active Status"
    );

    const activeValue =
      await projectActiveInActiveDropdown
        .getValueByLabel("Active");

    if (
      activeValue &&
      search.value.statusId === null
    ) {
      search.value.statusId = activeValue;
    }

    // --------------------------------------------------------------------------
    // Restore Layout State
    // --------------------------------------------------------------------------
    leftDrawerOpen.value =
      savedState?.leftDrawerOpen ?? true;

    middleDrawerOpen.value =
      savedState?.middleDrawerOpen ?? true;

    customerSplit.value =
      savedState?.customerSplit ?? 22;

    mainSplit.value =
      savedState?.mainSplit ?? 20;

    ActivitySplit.value =
      savedState?.ActivitySplit ?? 55;

    expandedRowId.value =
      savedState?.expandedRowId ?? null;

    // --------------------------------------------------------------------------
    // Load Projects
    // --------------------------------------------------------------------------
    await getAllCustomerProjectsList({
      pagination: pagination.value
    });

    // --------------------------------------------------------------------------
    // Restore Project
    // --------------------------------------------------------------------------
    const restoredProjectId =
      routeProjectId ||
      savedState?.projectId ||
      null;

    if (restoredProjectId) {
      selectedProjectId.value =
        restoredProjectId;

      activeRowId.value =
        restoredProjectId;

      searchProjectModule.value.projectId =
        restoredProjectId;

      selectedProject.value =
        rows.value.find(
          item => item.id === restoredProjectId
        ) || null;

      // ----------------------------------------------------------------------
      // Load Modules
      // ----------------------------------------------------------------------
      await getAllProjectsModulesByProjectId({
        pagination: projectModulesPagination.value
      });
    }

    // --------------------------------------------------------------------------
    // Restore Module
    // --------------------------------------------------------------------------
    const restoredModuleId =
      savedState?.projectModuleId || null;

    if (
      restoredProjectId &&
      restoredModuleId
    ) {
      selectedModuleId.value =
        restoredModuleId;

      activeModuleRowId.value =
        restoredModuleId;

      selectedModule.value =
        ProjectModuleRows.value.find(
          item => item.id === restoredModuleId
        ) || null;

      await LoadTasks(
        restoredProjectId,
        restoredModuleId
      );
    }

    // --------------------------------------------------------------------------
    // Restore Task
    // --------------------------------------------------------------------------
    const restoredTaskId =
      savedState?.projectTaskId || null;

    if (
      restoredProjectId &&
      restoredModuleId &&
      restoredTaskId
    ) {
      selectedTaskId.value =
        restoredTaskId;

      activeTaskRowId.value =
        restoredTaskId;

      selectedTask.value =
        projectTasks.value.find(
          item => item.id === restoredTaskId
        ) || null;

      await LoadTaskActivities(
        restoredTaskId
      );
    }

    // --------------------------------------------------------------------------
    // Restore Names
    // --------------------------------------------------------------------------
    storedProjectName.value =
      savedState?.projectName || "";

    storedModuleName.value =
      savedState?.moduleName || "";

    storedTaskName.value =
      savedState?.taskName || "";

    // --------------------------------------------------------------------------
    // Restore View Flags
    // --------------------------------------------------------------------------
    isProject.value =
      savedState?.isProject ?? true;

    isProjectModule.value =
      savedState?.isProjectModule ?? false;

    isProjectTask.value =
      savedState?.isProjectTask ?? false;

    isProjectActivity.value =
      savedState?.isProjectActivity ?? false;

    // --------------------------------------------------------------------------
    // Load Employees
    // --------------------------------------------------------------------------
    activityOwnerList.value = [];

    if (selectedProjectId.value) {
      getAllProjectEmployeesListForDropdown(
        selectedProjectId.value
      );

      getAllProjectTaskOwnersListForDropdown(
        selectedProjectId.value
      );

      projectEmployeesForDropdown.load(
        selectedProjectId.value
      );
    }

    // --------------------------------------------------------------------------
    // Persist Final State
    // --------------------------------------------------------------------------
    saveOutlookState({
      ...savedState,

      isProject: isProject.value,
      isProjectModule: isProjectModule.value,
      isProjectTask: isProjectTask.value,
      isProjectActivity: isProjectActivity.value,

      projectId: selectedProjectId.value,
      projectModuleId: selectedModuleId.value,
      projectTaskId: selectedTaskId.value,

      projectName: storedProjectName.value,
      moduleName: storedModuleName.value,
      taskName: storedTaskName.value,

      expandedRowId: expandedRowId.value,

      leftDrawerOpen: leftDrawerOpen.value,
      middleDrawerOpen: middleDrawerOpen.value,

      customerSplit: customerSplit.value,
      mainSplit: mainSplit.value,
      ActivitySplit: ActivitySplit.value,
    });
  } catch (error) {
    console.error(
      "Error during sequential execution:",
      error
    );
  }
}
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Clear all search filters/ LocalStorage
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onClear = () => {
  if (routeProjectId) {
    search.value.projectIds = [];
    history.replaceState({}, "", location.href);
  }

  saveOutlookState(defaultOutlookState);
  clearSearchModelsData();
  refreshProjectList();
};

function clearSearchModelsData () {
  search.value.projectIds = [];
  search.value.projectTypeIds = [];
  search.value.customerIds = [];
  search.value.projectCoordinatorIds = [];
  search.value.projectLeadsIds = [];
  search.value.projectPriorityIds = [];
  search.value.companyContactIds = [];
  search.value.statusId = null;

  expandedRowId.value = null;

  selectedCustomer.value = null;
  selectedProject.value = null;
  selectedModule.value = null;
  selectedTask.value = null;

  searchProjectModule.value.projectId = "";
  searchProjectTask.value.projectId = "";
  searchProjectTask.value.projectModuleId = "";
  searchProjectActivity.value.projectId = "";
  searchProjectActivity.value.projectModuleId = "";
  searchProjectActivity.value.projectTaskId = "";

  isProject.value = true;
  isProjectModule.value = false;
  isProjectTask.value = false;
  isProjectActivity.value = false;

  storedProjectName.value = "";
  storedModuleName.value = "";
  storedTaskName.value = "";

  selectedTaskId.value = null;
  selectedProjectId.value = null;
  selectedModuleId.value = null;

  ProjectName = "";
  ModuleName = "";
  TaskName = "";

  activeRowId.value = null;
  activeModuleRowId.value = null;
  activeTaskRowId.value = null;
}

const appliedProjectFiltersCount = computed(() => {
  let count = 0;
  const filters = search.value;

  // List all filter fields you want to count
  if (filters.searchText && filters.searchText.trim() !== "") count++;
  if (filters.projectIds && filters.projectIds.length > 0) count++;
  if (filters.customerIds && filters.customerIds.length > 0) count++;
  if (filters.projectTypeIds && filters.projectTypeIds.length > 0) count++;
  if (filters.projectStatusIds && filters.projectStatusIds.length > 0) count++;
  if (filters.projectCoordinatorIds && filters.projectCoordinatorIds.length > 0) count++;
  if (filters.projectLeadsIds && filters.projectLeadsIds.length > 0) count++;
  if (filters.projectPriorityIds && filters.projectPriorityIds.length > 0) count++;
  if (filters.companyContactIds && filters.companyContactIds.length > 0) count++;
  if (filters.statusId) count++;
  if (filters.year && filters.year !== "") count++;
  return count;
});

// Applied Module Filters Count
const appliedModuleFiltersCount = computed(() => {
  let count = 0;
  const filters = searchProjectModule.value;

  if (filters.projectModuleIds && filters.projectModuleIds.length > 0) count++;
  if (filters.projectModuleStatusIds && filters.projectModuleStatusIds.length > 0) count++;
  if (filters.filterModule && filters.filterModule.trim() !== "") count++;
  if (filters.isShowCloseStatus) count++;
  // Add more fields as needed

  return count;
});

// Applied Task Filters Count
const appliedTaskFiltersCount = computed(() => {
  let count = 0;
  const filters = searchProjectTask.value;

  if (filters.name && filters.name.trim() !== "") count++;
  if (filters.statusIds && filters.statusIds.length > 0) count++;
  if (filters.priorityIds && filters.priorityIds.length > 0) count++;
  if (filters.assignedToIds && filters.assignedToIds.length > 0) count++;
  if (filters.taskTagsIds && filters.taskTagsIds.length > 0) count++;
  if (filters.filterTask && filters.filterTask.trim() !== "") count++;
  if (filters.isShowCloseStatus) count++;
  // Add more fields as needed

  return count;
});

function validatePositiveDecimal (value) {
  const strValue = String(value).trim();
  // Accepts 0, positive integers, and positive decimals
  const regex = /^(0|[1-9]\d*)(\.\d+)?$/;
  if (regex.test(strValue)) return true;
  return "Please enter proper estimate hrs.";
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectNameDropdown,
  projectActiveInActiveDropdown,
  projectTypesDropdown,
  projectPrioritiesDropdown,
  projectEmployeesForDropdown
} = projectModule();

const { activeEmployeesDropdown } = employeeModule();
const { customerNameDropdown } = customerModule();
const { companyContactNameDropdown } = companyContactsModule();
const { projectModulesByProjectIdForDropdown, projectModuleStatusForDropdown } = projectModuleOfProjectModule();
const { projectTaskPrioritiesForDropdown, projectTaskStatusForDropdown, projectTaskTagsDropdown } = projectTaskModule();
const { tagsDropdown } = tagModule();

// ------------------------------------------------------------------------------------
// On page rendering (SOP Change)
// ------------------------------------------------------------------------------------
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
  customerNameDropdown.load();
  companyContactNameDropdown.load();
  projectTypesDropdown.load("Project Type");
  activeEmployeesDropdown.load(user.siteId);

  // Get Project Active/InActive and Set default to Active
  await projectActiveInActiveDropdown.load("Project Active Status");
  const activeValue = await projectActiveInActiveDropdown.getValueByLabel("Active");
  if (activeValue && search.value.statusId === null) search.value.statusId = activeValue;
  loadProjectNameDropdown();

  projectTaskTagsDropdown.load();
  tagsDropdown.load();

  if (selectedProjectId.value) projectModulesByProjectIdForDropdown.load(false, false, selectedProjectId.value);
  projectModuleStatusForDropdown.load("WO Status");

  projectTaskStatusForDropdown.load("Task Status");
  projectTaskPrioritiesForDropdown.load("Task Priorities");

  getDropDown("Project Status");
  getProjectStatus("Project Status");
  getActivitiesDropDown("Project Activities");
  getDropDownTaskStatus("Task Status");

  projectPrioritiesDropdown.load("Project Priorities");
  await getAllTagsForDropdown();

  if (isProject.value) await runSequentially();
});

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Static Filter on change
// --------------------------------------------------------------------------------------------------------------------------------------------------
watch(() => search.value.searchText, () => {
  if (search.value.searchText) searchProjectLoader.value = true;
  refreshProjectList();
});

watch(
  () => search.value.statusId,
  (newVal, oldVal) => {
    if (newVal === oldVal) return;

    search.value.projectIds = [];
    loadProjectNameDropdown();
  }
);

watch(() => search.value.customerIds, async (newValue, oldValue) => {
  if (newValue === oldValue) return;
  if (newValue?.length === 0) companyContactNameDropdown.load();

  companyContactNameDropdown.load(newValue);
});

watch(() => searchProjectModule.value.filterModule, () => {
  if (searchProjectModule.value.filterModule) searchProjectModuleLoader.value = true;
  refreshProjectModulesList();
});

watch(() => searchProjectTask.value.filterTask, () => {
  if (searchProjectTask.value.filterTask) searchProjectTaskLoader.value = true;
  refreshProjectTaskList();
});

watch(() => searchProjectActivity.value.filterActivity, () => {
  if (searchProjectActivity.value.filterActivity) searchProjectActivityLoader.value = true;
  refreshProjectTaskActivityList();
});

watch(
  [
    leftDrawerOpen,
    middleDrawerOpen,
    customerSplit,
    mainSplit,
    ActivitySplit
  ],
  () => {
    saveOutlookState({
      ...getOutlookState(),
      leftDrawerOpen: leftDrawerOpen.value,
      middleDrawerOpen: middleDrawerOpen.value,
      customerSplit: customerSplit.value,
      mainSplit: mainSplit.value,
      ActivitySplit: ActivitySplit.value
    });
  },
  {
    deep: true
  }
);

</script>

<style scoped>
  .pointer-disbled{
    pointer-events: none;
    cursor: not-allowed;
    opacity: 0.6;
  }

  ul {
    padding-left: 1.25rem;
  }

  .rightDialog {
    position: absolute;
    right: 2%;
    top: 30%;
    height: 45vh;
    border-left: 1px solid #ccc;
    border-radius: 0;
  }

  .Person {
    border-radius: 50%;
    background-color: #5d5d5d;
    color: white;
    font-size: 12px;
    font-weight: 600;
    padding: 3px 3px;
    margin-right: 2px;
    transition: 0.5s all ease-in-out;
  }

  .project-items{
    background: #efefef;
      margin-bottom: 2px;
      border-bottom: 1px solid #939393;
  }

  .project-actions .prod-btn {
    border: 1px solid var(--q-secondary);
    border-radius: 5px;
  }

  .tooltiptbl th{
    font-weight: 500;
    color: white;
    font-size: 10px;
  }

  .tooltiptbl th, .tooltiptbl td{
    border: 1px solid #d1d1d1  !important;
    padding: 3px 5px;
  }

  .tooltiptbl td{
    font-size: 10px;
  }
  .search-bar {
  display: flex;
  max-width: 250px;
}

  .search-bar ::v-deep(.search-box) {
  flex: 1;
  border-top-right-radius: 0 !important;
  border-bottom-right-radius: 0 !important;
}
.search-bar ::v-deep(.filter-btn) {
  height: 40px;
  border-top-left-radius: 0 !important;
  border-bottom-left-radius: 0 !important;
}
</style>
