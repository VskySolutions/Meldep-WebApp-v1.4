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
              <q-breadcrumbs-el :label="!search.isTemplate ? 'Projects' : 'Project Templates'" />
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
              <div class="row items-center q-mr-xs">
                <div class="search-container position-relative">
                  <!-- SOP Change -->
                  <searchFilterBar
                    v-model="search.searchText"
                    :loading="searchLoader"
                    :applied-filters="appliedFilters"
                    @toggle-filter="showFilter = !showFilter"
                  />
                  <q-menu v-model="showFilter" anchor="bottom left" self="top left" persistent no-parent-event style="width: 500px;" @click-outside="showFilter = false">
                    <!-- SOP Change -->
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
                        :options="statusListForSearch"
                        :filter="getProjectStatusFilter"
                        :isShowAll="true"
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
                      <singleSelectDropdown
                        v-model="search.statusId"
                        label="Active/Inactive"
                        :options="projectActiveInActiveDropdown.list.value"
                      />
                      <multiSelectDropdown
                        v-model="search.projectTagIds"
                        label="Tags"
                        :options="projectTagsDropdown.list.value"
                        :filter="projectTagsDropdown.filter"
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
              <!-- SOP Change -->
              <manageDropdownOptions
                v-model="showManageDropdownOptions"
                :manage-drop-down-types="manageDropDownTypes"
                :selected-field="selectedField"
              />
              <div class="flex items-center no-wrap">
                <q-btn
                  icon="o_add"
                  outline
                  label="Add"
                  no-caps
                  class="text-primary btnRounded q-ml-xs"
                  @click="onProjectAdd(refreshProjectList, refreshProjectNameDropdown)"
                >
                  <q-tooltip>Add Project</q-tooltip>
                </q-btn>
                <q-btn
                  icon="o_event"
                  outline
                  class="text-primary btnRounded q-ml-xs"
                  @click="$router.push({ path: `/project-planning/calendar`, state: {flag: 'all_proj_calendar' } })"
                >
                  <q-tooltip>All Project Module & Task Calendar</q-tooltip>
                </q-btn>
                <q-btn
                  icon="o_view_list"
                  outline
                  class="text-primary btnRounded q-ml-xs"
                  @click="$router.push('/all-project-planner')"
                >
                  <q-tooltip>All project planner</q-tooltip>
                </q-btn>
                <q-btn
                  v-if="role === 'admin'"
                  icon="o_lock"
                  outline
                  class="text-primary btnRounded q-ml-xs"
                  @click="$router.push('/project/assign-users-to-project')"
                >
                  <q-tooltip>Security</q-tooltip>
                </q-btn>
                <!-- SOP Change -->
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
                <!-- SOP Change -->
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
                <!-- SOP Change -->
                <!-- Column Hide/Show -->
                <columnVisibilityMenu
                  :all-column-names="allColumnNames"
                  :selected-column-names="selectedColumnNames"
                  @update:selected-column-names="selectedColumnNames = $event"
                  @select-all-columns="selectAllColumns"
                  @default-columns="defaultColumns"
                />
                <!-- SOP Change -->
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
          @request="getAllProjectList"
        >
          <template #loading>
            <q-inner-loading showing color="primary">
              <q-spinner-ios size="40px" class="q-mt-xl" />
            </q-inner-loading>
          </template>
          <template #header="props">
            <q-tr :props="props" class="bg-primary text-white">
              <q-th auto-width class="text-center" />
              <!-- SOP Change -->
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
                <q-icon v-if="col.tooltip" name="o_info" size="xs" class="q-mx-xs">
                  <q-tooltip class="text-caption">
                    {{ col.tooltip }}
                     <div class="flex flex-center" v-if="col.name === 'totalRequirementCount' || col.name === 'totalTaskCount' || col.name === 'totalIssueCount'">
                      <table class="table boarded GreyTable">
                        <thead>
                          <tr>
                            <th>Color</th>
                            <th>Percentage</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td class="text-center">
                              <q-badge color="red" rounded size="xs" />
                            </td>
                            <td>Below 26%</td>
                          </tr>
                          <tr>
                            <td class="text-center">
                              <q-badge color="yellow" rounded size="xs" />
                            </td>
                            <td>26% to 50%</td>
                          </tr>
                          <tr>
                            <td class="text-center">
                              <q-badge color="blue" rounded size="xs" />
                            </td>
                            <td>51% to 75%</td>
                          </tr>
                          <tr>
                            <td class="text-center">
                              <q-badge color="green" rounded size="xs" />
                            </td>
                            <td>76% to 100%</td>
                          </tr>
                        </tbody>
                      </table>
                      </div>
                    </q-tooltip>
                </q-icon>
              </q-th>
              <q-th auto-width class="text-center">Actions</q-th>
            </q-tr>
          </template>
          <template #body="props">
            <q-tr :props="props" :class="[
              highlightedId == props.row.id ? 'highlight' : '',
              props.row.isPinned ? 'bg-amber-1' : ''
              ]">
              <q-td
                style="width: 2%; position: relative;"
              >
              <div v-if="props.row.projectColor"
                  :style="'border-left: 5px solid '+ (props.row.projectColor ? props.row.projectColor : 'transparent') + '; width: 5px;height: 97%;position: absolute; left: -1px;top: 0px;'"
                >
                  <q-tooltip>Project Colors</q-tooltip>
                </div>
                <q-icon
                  v-if="props.row.isPinned && props.row.isEditable"
                  name="o_push_pin"
                  size="xs"
                  class="q-mr-xs hoverable-cell"
                  @click="() => { onSubmitProjectPinned(props.row.id, !props.row.isPinned, refreshProjectList); }"
                >
                  <q-tooltip>Click to unpin</q-tooltip>
                </q-icon>
                <q-icon
                  v-if="props.row.isPinned && !props.row.isEditable"
                  name="o_push_pin"
                  size="xs"
                  class="q-mr-xs"
                >
                  <q-tooltip>Project Pinned</q-tooltip>
                </q-icon>
                <div
                  v-if="props.row.isEditable"
                  :class="['dot-circle q-mr-xs hoverable-cell', props.row.active ? 'dot-active' : 'dot-inactive']"
                  @click="() => { onSubmitProjectActiveInActiveToggle(props.row.id, props.row.active, refreshProjectList) }"
                >
                  <q-tooltip v-if="!props.row.active">Set Active?</q-tooltip>
                  <q-tooltip v-else>Set Inactive?</q-tooltip>
                </div>
                <div
                  v-if="!props.row.isEditable"
                  :class="['dot-circle q-mr-xs hoverable-cell', props.row.active ? 'dot-active' : 'dot-inactive']"
                />
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('customerId')"
                class="common-q-td hoverable-cell"
              >
                <div class="row no-wrap items-center justify-between">
                  <span v-if="props.row.showCustomerName" class="hoverable-cell" @click="setActiveRowIdInLocalStorage(props.row.id); onCustomerView(props.row.customer.id)">
                    {{ props.row.customer.name }}
                  </span>
                  <div class="row items-center q-gutter-sm q-ml-sm" style="flex-shrink: 0;">
                    <q-icon
                      v-if="props.row.showCustomerName && !search.isTemplate"
                      name="o_edit"
                      size="xs"
                      class="cursor-pointer"
                      @click="
                        setActiveRowIdInLocalStorage(props.row.id);
                        onCustomerEdit(props.row.customer.id, props.row.customer.customerTypeId, props.row.customer.personId, props.row.customer.companyId, refreshProjectList)
                      "
                    >
                      <q-tooltip>Edit Customer</q-tooltip>
                    </q-icon>
                  </div>
                </div>
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('name')"
                class="common-q-td hoverable-cell"
              >
                <div class="row no-wrap items-center justify-between">
                  <span
                    class="common-q-td"
                    style="flex: 1;"
                    @click="onProjectView(props.row.id)"
                  >
                    {{ props.row.name }}
                  </span>
                  <div class="row items-center q-gutter-sm q-ml-sm" style="flex-shrink: 0;">
                    <q-icon
                      v-if="!search.isTemplate"
                      name="o_radio_button_checked"
                      size="xs"
                      class="cursor-pointer"
                      @click="setActiveRowIdInLocalStorage(props.row.id); $router.push({ path: '/project-center', state: { projectId: props.row.id } })"
                    >
                      <q-tooltip>Project Center</q-tooltip>
                    </q-icon>
                    <q-icon
                      v-if="props.row.isEditable && !search.isTemplate"
                      name="o_developer_board"
                      size="xs"
                      class="cursor-pointer"
                      @click="setActiveRowIdInLocalStorage(props.row.id); $router.push({ path: '/project-planning/workboard', state: { projectId: props.row.id } })"
                    >
                      <q-tooltip>Work Board</q-tooltip>
                    </q-icon>
                  </div>
                </div>
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('startDate')"
              >
                {{ toDate(props.row.startDate) }}
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('goLiveDate')"
                class="common-q-td"
                :class="{ 'hoverable-cell' : props.row.isEditable }"
                @click="activeEdit = { rowId: props.row.id, field: 'goLiveDate' }"
              >
                <quickEditDate
                  :row-id="props.row.id"
                  :model-value="props.row.goLiveDate"
                  :editable="props.row.isEditable"
                  :date-options="disableBeforeStartDate(props.row.startDate)"
                  :show-history="true"
                  @submit="({ rowId, value }) => onSubmitProjectEndDate(rowId, value, refreshProjectList)"
                  @history="() => onSiteModifiedLog(props.row.id, props.row.name, 'Due Date')"
                />
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('projectCoordinator.id')"
                class="common-q-td"
              >
                <div
                  v-if="props.row.projectCoordinators?.length > 0"
                  class="col-9 flex justify-center TaskActivity"
                >
                  <div v-for="(lead, index) in props.row.projectCoordinators" :key="index">
                    <span
                      class="Person"
                      :style="{ background: lead.bgColor, color: lead.color }"
                    >
                      {{ typeof lead === 'object' ? getInitials((lead.text || lead.name)) : getInitials(getNameFromId(lead)) }}
                    </span>
                    <br v-if="index !== props.row.projectCoordinators.length - 1">
                    <q-tooltip>
                      <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                      <span>{{ typeof lead === 'object' ? (lead.text || lead.name) : getNameFromId(lead) }}</span>
                    </q-tooltip>
                  </div>
                </div>
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('projectLeads')"
                class="common-q-td"
              >
                <div
                  v-if="props.row.projectLeads?.length"
                  class="col-9 flex justify-center TaskActivity"
                >
                  <div v-for="(lead, index) in props.row.projectLeads" :key="index">
                    <span
                      class="Person q-mr-xs"
                      :style="{ background: lead.bgColor, color: lead.color }"
                    >
                      {{ getInitials(typeof lead === 'object' ? lead.text || lead.name : getNameFromId(lead)) }}
                    </span>
                    <q-tooltip>
                      <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                      <span>{{ typeof lead === 'object' ? (lead.text || lead.name) : getNameFromId(lead) }}</span>
                    </q-tooltip>
                  </div>
                </div>
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('projectManager')"
                class="common-q-td"
              >
                <div
                  class="flex justify-center task-box cursor-grab"
                  :class="props.row.projectManager ? 'TaskActivity' : ''"
                >
                  <div v-if="props.row.projectManager">
                    <span
                      class="Person"
                      :style="{ background: props.row.projectManager.bgColor, color: props.row.projectManager.color }"
                    >
                      {{ getInitials(typeof props.row.projectManager === 'object' ? props.row.projectManager.text || props.row.projectManager.name : getNameFromId(props.row.projectManager)) }}
                    </span>
                    <q-tooltip>
                      <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                      <span>{{ props.row.projectManager?.text }}</span>
                    </q-tooltip>
                  </div>
                </div>
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('projectPriority.dropDownValue')"
                class="common-q-td"
                :class="{ 'hoverable-cell' : props.row.isEditable }"
                @click="activeEdit = { rowId: props.row.id, field: 'priority' }"
              >
                <quickEditSingleSelect
                  field="priority"
                  :row-id="props.row.id"
                  :value="props.row.projectPriority.id"
                  :display-value="props.row.projectPriority.dropDownValue"
                  :editable="props.row.isEditable"
                  :options="projectPrioritiesDropdown.list.value"
                  :active-edit="activeEdit"
                  :show-history="false"
                  @cancel="activeEdit = { rowId: null, field: null }"
                  @submit="({ rowId, value }) => onSubmitProjectPriority(rowId, value, refreshProjectList)"
                />
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('projectStatus.dropDownValue')"
                class="common-q-td"
                :class="{ 'hoverable-cell' : props.row.isEditable }"
                @click="activeEdit = { rowId: props.row.id, field: 'status' }"
              >
                <quickEditSingleSelect
                  field="status"
                  :row-id="props.row.id"
                  :value="props.row.projectStatus.id"
                  :display-value="props.row.projectStatus.dropDownValue"
                  :editable="props.row.isEditable"
                  :options="projectStatusList"
                  :active-edit="activeEdit"
                  :show-history="true"
                  @popup-show="handlePopupShow(props.row.projectStatus.dropDownValue)"
                  @cancel="activeEdit = { rowId: null, field: null }"
                  @submit="({ rowId, value }) => onSubmitProjectStatus(rowId, value, refreshProjectList)"
                  @history="() => onSiteModifiedLog(props.row.id, props.row.name, 'Project Status')"
                />
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('projectType.dropDownValue')"
                class="common-q-td"
              >
                {{ props.row.projectType.dropDownValue }}
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('projectCategoryId')"
                class="common-q-td"
              >
                {{ props.row.projectCategories.type }}
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('totalRequirementCount')"
                class="text-end hoverable-cell fw-bold"
                @click="setActiveRowIdInLocalStorage(props.row.id); !search.isTemplate ? $router.push({ path: '/requirement', state: {projectId: props.row.id } }) : null"
              >
                <span :class="`text-${getCountColor(props.row.totalRequirementCount, props.row.completedRequirementCount)}`">
                  {{ props.row.completedRequirementCount }}
                </span>
                /
                {{ props.row.totalRequirementCount }}
                <q-tooltip v-if="props.row.isEditable && !search.isTemplate">View Project Requirements</q-tooltip>
                <q-icon v-if="props.row.totalRequirementCount > 0 && !search.isTemplate" name="o_info" size="xs" class="text-primary">
                  <q-tooltip anchor="bottom middle" self="top middle" class="bg-grey-8 text-white shadow-2">
                    <div class="text-caption">
                      <table class="table boarded GreyTable">
                        <thead>
                          <tr>
                            <th v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.noStatus > 0">No Status</th>
                            <th v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.new > 0">New</th>
                            <th v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.open > 0">Open</th>
                            <th v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.inProgress > 0">In Progress</th>
                            <th v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.close > 0">Close</th>
                            <th v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.onHold > 0">On Hold</th>
                            <th v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.total > 0">Total</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.noStatus > 0" class="text-center">{{ requirementStatusSummary.find(x => x.projectId === props.row.id)?.noStatus }}</td>
                            <td v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.new > 0" class="text-center">{{ requirementStatusSummary.find(x => x.projectId === props.row.id)?.new }}</td>
                            <td v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.open > 0" class="text-center">{{ requirementStatusSummary.find(x => x.projectId === props.row.id)?.open }}</td>
                            <td v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.inProgress > 0" class="text-center">{{ requirementStatusSummary.find(x => x.projectId === props.row.id)?.inProgress }}</td>
                            <td v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.close > 0" class="text-center">{{ requirementStatusSummary.find(x => x.projectId === props.row.id)?.close }}</td>
                            <td v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.onHold > 0" class="text-center">{{ requirementStatusSummary.find(x => x.projectId === props.row.id)?.onHold }}</td>
                            <td v-if="requirementStatusSummary.find(x => x.projectId === props.row.id)?.total > 0" class="text-center">{{ requirementStatusSummary.find(x => x.projectId === props.row.id)?.total }}</td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </q-tooltip>
                </q-icon>
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('totalTaskCount')"
                class="text-end hoverable-cell fw-bold"
                @click="setActiveRowIdInLocalStorage(props.row.id); !search.isTemplate ? $router.push({ path: '/project-tasks', state: { projectId: props.row.id } }) : null"
              >
                <span :class="`text-${getCountColor(props.row.totalTaskCount, props.row.completedTaskCount)}`">
                  {{ props.row.completedTaskCount }}
                </span>
                /
                {{ props.row.totalTaskCount }}
                <q-tooltip v-if="props.row.isEditable && !search.isTemplate">View Project Tasks</q-tooltip>
                <q-icon v-if="props.row.totalTaskCount > 0 && !search.isTemplate" name="o_info" size="xs" class="text-primary">
                  <q-tooltip anchor="bottom middle" self="top middle" class="bg-grey-8 text-white shadow-2">
                    <div class="text-caption">
                      <table class="table boarded GreyTable">
                        <thead>
                          <tr>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.noStatus > 0">No Status</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.new > 0">New</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.open > 0">Open</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.inDevelopment > 0">In Development</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.developmentCompleted > 0">Dev. Comtd</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.underDeployment > 0">In Deply</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.testSite > 0">Test Site</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.inQA > 0">In QA</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.inUAT > 0">In UAT</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.uatPassed > 0">UAT passed</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.close > 0">Close</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.onHold > 0">On Hold</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.completed > 0">Completed</th>
                            <th v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.total > 0">Total</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.noStatus > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.noStatus }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.new > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.new }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.open > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.open }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.inDevelopment > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.inDevelopment }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.developmentCompleted > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.developmentCompleted }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.underDeployment > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.underDeployment }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.testSite > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.testSite }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.inQA > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.inQA }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.inUAT > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.inUAT }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.uatPassed > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.uatPassed }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.close > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.close }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.onHold > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.onHold }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.completed > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.completed }}</td>
                            <td v-if="taskStatusSummary.find(x => x.projectId === props.row.id)?.total > 0" class="text-center">{{ taskStatusSummary.find(x => x.projectId === props.row.id)?.total }}</td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </q-tooltip>
                </q-icon>
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('estimateTime')"
                class="text-end"
              >
                <span
                  :class="props.row.totalActivityHours > props.row.totalTaskEstimateHours ? 'text-red' : ''"
                >
                  {{ props.row.totalActivityHours }}
                </span> / {{ props.row.totalTaskEstimateHours }}
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('totalIssueCount')"
                class="text-end hoverable-cell fw-bold"
                @click="setActiveRowIdInLocalStorage(props.row.id); !search.isTemplate ? $router.push({ path: '/issue', state: {projectId: props.row.id } }) : null"
              >
                <span :class="`text-${getCountColor(props.row.totalIssueCount, props.row.completedIssueCount)}`">
                  {{ props.row.completedIssueCount }}
                </span>
                /
                {{ props.row.totalIssueCount }}
                <q-tooltip v-if="props.row.isEditable && !search.isTemplate">View Project Issues</q-tooltip>
                <q-icon v-if="props.row.totalIssueCount > 0 && !search.isTemplate" name="o_info" size="xs" class="cursor-pointer text-primary">
                  <q-tooltip anchor="bottom middle" self="top middle" class="bg-grey-8 text-white shadow-2">
                    <div class="text-caption">
                      <table class="table boarded GreyTable">
                        <thead>
                          <tr>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.noStatus > 0">No Status</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.new > 0">New</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.inTesting > 0">In Testing</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.toDo > 0">To Do</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.reopen > 0">Reopen</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.inUAT > 0">In UAT</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.uatPassed > 0">UAT Passed</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.newFromTestPlan > 0">Test Plan</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.convertedToTask > 0">Converted To Task</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.inReview > 0">InReview</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.done > 0">Done</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.onHold > 0">OnHold</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.inDevelopment > 0">In Development</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.closed > 0">Closed</th>
                            <th v-if="statusSummary.find(x => x.projectId === props.row.id)?.total > 0">Total</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.noStatus > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.noStatus }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.new > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.new }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.inTesting > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.inTesting }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.toDo > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.toDo }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.reopen > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.reopen }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.inUAT > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.inUAT }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.uatPassed > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.uatPassed }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.newFromTestPlan > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.newFromTestPlan }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.convertedToTask > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.convertedToTask }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.inReview > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.inReview }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.done > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.done }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.onHold > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.onHold }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.inDevelopment > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.inDevelopment }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.closed > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.closed }}</td>
                            <td v-if="statusSummary.find(x => x.projectId === props.row.id)?.total > 0" class="text-center">{{ statusSummary.find(x => x.projectId === props.row.id)?.total }}</td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </q-tooltip>
                </q-icon>
              </q-td>
              <!-- SOP Change -->
              <q-td
                v-if="selectedColumnNames.includes('projectTags')"
                class="common-q-td"
                :class="{ 'hoverable-cell' : props.row.isEditable }"
              >
                <div v-if="props.row.projectTags?.length">
                  <div class="row items-center q-gutter-xs">
                    <q-chip
                      v-for="(tag, i) in showAllTagsRowId === props.row.id ? props.row.projectTags : props.row.projectTags.slice(0, 5)"
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
                      @remove="onDeleteProjectTag(props.row, tag)"
                    >
                      {{ tag.text }}
                    </q-chip>
                    <q-btn v-if="props.row.projectTags.length > 5" dense flat size="sm" @click.stop="toggleShowAllTags(props.row.id)">
                      <template v-if="showAllTagsRowId === props.row.id">
                        <q-chip color="gray" size="sm" text-color="black" class="q-pa-xs text-caption" style="height: 16px; min-width: 16px;">
                          -{{ props.row.projectTags.length - 5 }}
                        </q-chip>
                      </template>
                      <template v-else>
                        <div class="row items-center no-wrap">
                          <span class="">...</span>
                          <q-chip color="gray" size="sm" text-color="black" class="q-pa-xs text-caption" style="height: 16px; min-width: 16px;">
                            +{{ props.row.projectTags.length - 5 }}
                          </q-chip>
                        </div>
                      </template>
                    </q-btn>
                  </div>
                </div>
                <q-popup-edit
                  v-if="props.row.isEditable"
                  v-slot="scope"
                  v-model="props.row.projectTags"
                  class="small-popup-title common-q-td"
                  style="width: 300px;"
                  @save="val => { props.row.projectTags = val; onSubmitProjectTags( props.row.id, val, refreshProjectList, refreshProjectTagsDropdown); }"
                >
                  <div class="row justify-between items-center q-mb-sm">
                    <div class="text-subtitle2">Update Tags</div>
                    <q-btn v-close-popup icon="o_close" size="sm" color="black" flat round dense />
                  </div>
                  <!-- SOP Change -->
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
              <q-td style="width: 5%;" class="text-center actions">
                <a
                  v-if="!props.row.isTemplate && (props.row.isEditable || props.row.isNotes)"
                  style="position: relative;" class="q-icon notranslate cursor-pointer q-mr-lg q-ml-sm"
                  @click="setActiveRowIdInLocalStorage(props.row.id); $router.push({ path: `/project/projectTaskNotes`, state: {projectId: props.row.id } })"
                >
                  <q-tooltip anchor="bottom middle" self="top middle">
                    Note
                  </q-tooltip>
                  <q-icon name="o_assignment" />
                  <q-badge
                    v-if="props.row.projectNotesCount > 0"
                    style="position: absolute; right: -16px; top: -15px;"
                    color="green"
                    text-color="white"
                    :label="props.row.projectNotesCount"
                  />
                </a>
                <a
                  v-if="!props.row.isTemplate && (props.row.isEditable || props.row.isNotes)"
                  style="position: relative;" class="q-icon notranslate cursor-pointer q-mr-sm"
                  @click="setActiveRowIdInLocalStorage(props.row.id); onProjectMessage(props.row.id, refreshProjectList)"
                >
                  <q-tooltip anchor="bottom middle" self="top middle">
                    Message
                  </q-tooltip>
                  <q-icon name="o_message" />
                  <q-badge
                    v-if="props.row.projectMessageCount > 0"
                    style="position: absolute; right: -16px; top: -15px;"
                    color="green"
                    text-color="white"
                    :label="props.row.projectMessageCount"
                  />
                </a>
                <a
                  v-if="props.row.isTemplate && props.row.isEditable"
                  class="q-icon notranslate cursor-pointer q-mr-sm"
                  @click="convertTemplateProject(props.row.id, props.row.name, refreshProjectList)"
                >
                  <q-tooltip anchor="bottom middle" self="top middle">
                    Convert Template To Project?
                  </q-tooltip>
                  <q-icon name="o_swap_horiz" />
                </a>
                <q-btn dense flat icon="o_more_vert" color="primary">
                  <q-tooltip>More Options</q-tooltip>
                  <q-menu auto-close>
                    <q-list style="min-width: 180px">
                      <q-item
                        v-ripple
                        clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id); onProjectView(props.row.id)"
                      >
                        <q-item-section avatar><q-icon name="o_visibility" size="xs" /></q-item-section>
                        <q-item-section>View</q-item-section>
                      </q-item>
                      <q-item
                        v-if="props.row.isEditable"
                        v-ripple clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id); onProjectEdit(props.row.id, false, refreshProjectList, refreshProjectNameDropdown)"
                      >
                        <q-item-section avatar><q-icon name="o_edit" size="xs" /></q-item-section>
                        <q-item-section>Edit</q-item-section>
                      </q-item>
                      <q-item
                        v-if="props.row.isEditable && !search.isTemplate"
                        v-ripple clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id); onProjectEdit(props.row.id, true, refreshProjectList, refreshProjectNameDropdown)"
                      >
                        <q-item-section avatar><q-icon name="o_groups" size="xs" /></q-item-section>
                        <q-item-section>Charter</q-item-section>
                      </q-item>
                      <q-item
                        v-if="props.row.createdById === loggedUserId || role === 'admin' || props.row.isProjectManager"
                        v-ripple
                        clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id); onAssignUserToProject(props.row.id, props.row.name, refreshProjectList)"
                      >
                        <q-item-section avatar><q-icon name="o_assignment_ind" size="xs" /></q-item-section>
                        <q-item-section>Assign Users</q-item-section>
                      </q-item>
                      <q-item
                        v-if="!search.isTemplate"
                        v-ripple
                        clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id); $router.push({ path: `/all-project-planner`, state: {projectId: props.row.id } })"
                      >
                        <q-item-section avatar><q-icon name="o_task" size="xs" /></q-item-section>
                        <q-item-section>Planner</q-item-section>
                      </q-item>
                      <q-item
                        v-if="!search.isTemplate"
                        v-ripple
                        clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id); $router.push({ path: '/project-targetplan/weeklyplanner', state: {projectId: props.row.id } })"
                      >
                        <q-item-section avatar><q-icon name="o_calendar_view_week" size="xs" /></q-item-section>
                        <q-item-section>Weekly Planner</q-item-section>
                      </q-item>
                      <q-item
                        v-if="!search.isTemplate"
                        v-ripple
                        clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id); $router.push({ path: '/project-targetplan/monthlyplanner', state: {projectId: props.row.id } })"
                      >
                        <q-item-section avatar><q-icon name="o_calendar_view_month" size="xs" /></q-item-section>
                        <q-item-section>Monthly Planner</q-item-section>
                      </q-item>
                      <q-item
                        v-if="!search.isTemplate"
                        v-ripple
                        clickable
                        @click="$router.push({ path: `/project-planning/calendar`, state: {projectId: props.row.id, projectName: props.row.name, projectStatus: props.row.projectStatus.dropDownValue } })"
                      >
                        <q-item-section avatar><q-icon name="o_event" size="xs" /></q-item-section>
                        <q-item-section>Calendar</q-item-section>
                      </q-item>
                      <q-item
                        v-if="props.row.isEditable && !search.isTemplate"
                        v-ripple clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id); onProjectFilesView(props.row.id, props.row.name, refreshProjectList)"
                      >
                        <q-item-section avatar><q-icon name="o_description" size="xs" /></q-item-section>
                        <q-item-section>Files</q-item-section>
                      </q-item>
                      <q-item
                        v-if="!props.row.isTemplate && props.row.isEditable"
                        v-ripple
                        clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id); convertProjectTemplate(props.row.id, props.row.name, props.row.startDate, refreshProjectList)"
                      >
                        <q-item-section avatar><q-icon name="o_swap_horiz" size="xs" /></q-item-section>
                        <q-item-section>Convert To Template?</q-item-section>
                      </q-item>
                      <q-item
                        v-if="props.row.isEditable"
                        v-ripple
                        clickable
                        @click="setActiveRowIdInLocalStorage(props.row.id)"
                      >
                        <q-item-section avatar>
                          <q-icon name="o_local_offer" size="xs" />
                        </q-item-section>
                        <q-item-section>
                          <div class="cursor-pointer" @click.stop>
                            Add Tags
                            <q-popup-edit
                              v-slot="scope"
                              v-model="props.row.projectTags"
                              class="small-popup-title common-q-td"
                              style="width: 300px;"
                              @save="val => {
                                props.row.projectTags = val;
                                onSubmitProjectTags(
                                  props.row.id,
                                  val,
                                  refreshProjectList,
                                  refreshProjectTagsDropdown
                                );
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
                      <q-item v-if="props.row.isEditable">
                        <q-item-section avatar>
                        <q-icon v-if="props.row.projectColor" name="o_circle" :style="`border-radius:50%;color:${props.row.projectColor}; background-color:${props.row.projectColor};`" size="xs" />
                        <q-icon v-else name="o_question_mark" size="xs" />
                        </q-item-section>
                        <q-item-section class="row items-center" style="display: flex; align-items: center; justify-content: start; flex-direction: row;">
                          Project Color
                          <q-icon
                            name="o_colorize"
                            class="cursor-pointer q-ml-xs"
                            size="xs"
                            @click.stop="onRestorePreviousProjectColor(props.row.projectColor)"
                          >
                            <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                              <q-color
                                v-model="props.row.projectColor"
                                no-header
                                no-footer
                                default-view="palette"
                                class="my-picker"
                                @update:model-value="onStartProjectColorSelection"
                                @change="onSubmitProjectColor(props.row.id, props.row.projectColor, isSliderActive, previousColor, refreshProjectList)"
                              />
                            </q-popup-proxy>
                          </q-icon>
                        </q-item-section>
                      </q-item>
                      <q-item
                        v-if="props.row.isEditable" v-ripple clickable
                        @click="() => { onSubmitProjectPinned(props.row.id, !props.row.isPinned, refreshProjectList); }"
                      >
                        <q-item-section avatar>
                          <q-icon :name="props.row.isPinned ? 'o_push_pin' : 'o_push_pin'" size="xs" />
                        </q-item-section>
                        <q-item-section>{{ props.row.isPinned ? "Unpin" : "Pin" }}</q-item-section>
                      </q-item>
                      <q-item
                        v-if="!search.isTemplate && props.row.isEditable"
                        v-ripple
                        clickable
                        @click="onSubmitProjectActiveInActiveToggle(props.row.id, props.row.active, refreshProjectList)"
                      >
                        <q-item-section avatar>
                          <q-icon :name="props.row.active ? 'o_block' : 'o_check_circle_outline'" :color="!props.row.active ? 'positive' : 'negative'" size="xs" />
                        </q-item-section>
                        <q-item-section>{{ !props.row.active ? 'Set Active?' : 'Set Inactive?' }}</q-item-section>
                      </q-item>
                      <q-item
                        v-if="props.row.isEditable"
                        v-ripple
                        clickable
                        @click="onSubmitProjectDelete(props.row.id, props.row.name, refreshProjectList, refreshProjectNameDropdown)"
                      >
                        <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                        <q-item-section class="text-negative">Delete</q-item-section>
                      </q-item>
                    </q-list>
                  </q-menu>
                </q-btn>
              </q-td>
            </q-tr>
            <!-- SOP Change -->
            <q-tr v-if="props.pageIndex === rows.length - 1">
              <q-td
                :colspan="computedColumns.findIndex(c => c.name === 'estimateTime') + 1"
                class="text-right font-bold"
              >
                <b>Total Hours:</b>
              </q-td>
              <q-td class="text-right">
                <b>{{ totalTaskActivityHours() }} / {{ totalEstimateHours() }}</b>
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
  <!-- SOP Change -->
  <!-- Multi-Column Level Sorting -->
  <multiColumnSortingDialog
    v-model="showSortDialog"
    :columns="columns"
    :multi-sort="multiSort"
    :exclude-columns="['Req.', 'Tasks', 'Est. Hrs', 'Issues', 'Tags']"
    @add="addSortLevel"
    @remove="removeSortLevel"
    @apply="applyMultiSort"
  />
</template>
<script setup>
// Import libraries
import { useAuthStore } from "stores/auth";
import { ref, onMounted, watch, computed } from "vue";
import { zwConfirmDelete } from "assets/utils";
import { parse } from "date-fns"; // Standard TimeZone Conversion

import useFilters from "composables/useFilters";
import commonService from "services/common.service";
import projectService from "modules/project/projects.service";

import TagEditor from "src/modules/project-tasks/components/_taskTagEditor.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import customerModule from "src/modules/customer/utils/dropdowns.js";
import companyContactsModule from "src/modules/company-contacts/utils/dropdowns.js";
import manageDropdownModule from "src/modules/dropdown/utils/dropdowns.js";
import tagModule from "src/modules/tags/utils/dropdowns.js";

// SOP Change :- Shared Scripts DataTable Features
import { useColumnManager } from "composables/dataTable/useColumnManager.js";
import useColumnResize from "composables/dataTable/useColumnResize.js";
import useMultiSort from "composables/dataTable/useMultiSort.js";
import useSiteTableState from "composables/dataTable/useSiteTableState.js";

// SOP Change :- Shared DataTable Views
import quickEditSingleSelect from "src/components/dataTable/_quickEditSingleSelect.vue";
import quickEditDate from "src/components/dataTable/_quickEditDate.vue";
import multiColumnSortingDialog from "src/components/dataTable/_multiColumnSortingDialog.vue";
import manageDropdownOptions from "src/components/dataTable/_manageDropdownOptions.vue";
import columnVisibilityMenu from "src/components/dataTable/_columnVisibilityMenu.vue";
import searchFilterBar from "src/components/dataTable/_searchFilterBar.vue";

// SOP Change :- Shared Inputs
import singleSelectDropdown from "src/components/form-inputs/_singleSelectDropdown.vue";
import multiSelectDropdown from "src/components/form-inputs/_multiSelectDropdown.vue";

// SOP Change :- Shared Customer Dialogs
import {
  initCustomerDialogs,
  onCustomerView,
  onCustomerEdit
} from "src/modules/customer/utils/dialogs.js";

// SOP Change :- Shared Project Dialogs
import {
  initProjectDialogs,
  onProjectView,
  onProjectAdd,
  onProjectEdit,
  onAssignUserToProject,
  onProjectMessage,
  onProjectFilesView,
  convertProjectTemplate,
  convertTemplateProject
} from "src/modules/project/utils/dialogs.js";

// SOP Change :- Shared Site Dialogs
import {
  initSiteDialogs,
  onSiteModifiedLog
} from "src/modules/sites/utils/dialogs.js";

// SOP Change :- Shared Project Actions
import {
  initProjectActions,
  onSubmitProjectEndDate,
  onSubmitProjectPinned,
  onSubmitProjectColor,
  onSubmitProjectActiveInActiveToggle,
  onSubmitProjectStatus,
  onSubmitProjectPriority,
  onSubmitProjectDelete,
  onSubmitProjectTags
} from "src/modules/project/utils/actions.js";

// Common variables
const { toDate } = useFilters();
const authStore = useAuthStore();

const loading = ref(true);
const user = authStore.user;
const loggedUserId = user.userId;
const loginUserEmployeeId = user?.employeeId;
const adminRoles = ["admin", "site-super-admin", "system-super-admin"];
const role = user?.roles?.some(r => adminRoles.includes(r)) ? "admin" : "";

const currentSiteId = computed(() => user.siteId);

const showFilter = ref(false);
const searchLoader = ref(false);
const activeEdit = ref({ rowId: null, field: null });
const statusSummary = ref([]);
const taskStatusSummary = ref([]);
const requirementStatusSummary = ref([]);
const projectTagInputValue = ref("");
const manageDropDownTypes = ref([]);
const showManageDropdownOptions = ref(false);
const showSortDialog = ref(false);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// DataTable:- Columns (SOP Change)
// --------------------------------------------------------------------------------------------------------------------------------------------------

// Table variables
const tableRef = ref();
const rows = ref([]);
const columns = ref([
  { name: "customerId", label: "Customer", field: "customerId", align: "left", sortable: true, default: true },
  { name: "name", label: "Project Name", field: "name", align: "left", sortable: true, default: true },
  { name: "startDate", label: "Start Date", field: "startDate", align: "center", sortable: true, default: false },
  { name: "goLiveDate", label: "Due Date", field: "goLiveDate", align: "center", sortable: true, default: false },
  { name: "projectCoordinator.id", label: "PC", field: "projectCoordinator.id", align: "center", sortable: false, default: true, tooltip: "Project Coordinators" },
  { name: "projectLeads", label: "PL", field: row => row.projectLeads.join(", "), align: "center", sortable: false, default: true, tooltip: "Project Leads" },
  { name: "projectManager", label: "PM", field: "projectManager", align: "center", sortable: false, default: true, tooltip: "Project Manager" },
  { name: "projectPriority.dropDownValue", label: "Priority", field: "projectPriority.dropDownValue", align: "left", sortable: true, default: true },
  { name: "projectStatus.dropDownValue", label: "Status", field: "projectStatus.dropDownValue", align: "left", sortable: true, default: true },
  { name: "projectType.dropDownValue", label: "Type", field: "projectType.dropDownValue", align: "center", sortable: true, default: false },
  { name: "projectCategoryId", label: "Category", field: "projectCategoryId", align: "center", sortable: true, default: false },
  { name: "totalRequirementCount", label: "Req.", field: "totalRequirementCount", align: "right", sortable: false, default: true, tooltip: "Project Requirements With Status Summary" },
  { name: "totalTaskCount", label: "Tasks", field: "totalTaskCount", align: "right", sortable: false, default: true, tooltip: "Project Tasks With Status Summary" },
  { name: "estimateTime", label: "Est. Hrs", field: "estimateTime", align: "right", sortable: false, default: true, tooltip: "(Sum of Activity Hours of All Tasks) / (Sum of Est. Hrs of All Tasks)" },
  { name: "totalIssueCount", label: "Issues", field: "totalIssueCount", align: "right", sortable: false, default: true, tooltip: "Project Issue With Status Summary" },
  { name: "projectTags", label: "Tags", field: row => row.projectTags, align: "left", sortable: false, default: false, tooltip: "Project Tags" }
]);
// --------------------------------------------------------------------------------------------------------------------------------------------------
// DataTable:- Get Projects List (SOP Change)
// --------------------------------------------------------------------------------------------------------------------------------------------------

const getAllProjectList = async ({ pagination: p }) => {
  loading.value = true;

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

  const selectedIds = new Set(
    JSON.parse(localStorage.getItem("selectedProjectIds") || "[]")
  );

  try {
    const resp = await projectService.getProjects(payload);
    statusSummary.value = resp.statusSummary;
    taskStatusSummary.value = resp.taskStatusSummary;
    requirementStatusSummary.value = resp.requirementStatusSummary;

    const data = resp.data;
    const result = new Array(data.length);

    let lastCustomerName = null;

    for (let i = 0; i < data.length; i++) {
      const project = data[i];

      const mappings = project.projectEmployeeMappings || [];

      const leads = [];
      const coordinators = [];

      let manager = null;
      let isProjectManager = false;

      for (let j = 0; j < mappings.length; j++) {
        const m = mappings[j];

        const emp = m.employee;
        const person = emp?.person;
        const roleName = m.employeeRoleDropdown?.dropDownValue;

        const empObj = {
          value: String(emp?.id),
          text: person?.fullName || "Unknown",
          bgColor: person?.bgColor,
          color: person?.color
        };

        if (roleName === "Project Lead") {
          leads.push(empObj);
        } else if (roleName === "Project Coordinator") {
          coordinators.push(empObj);
        } else if (roleName === "Project Manager") {
          manager = empObj;
          if (emp?.id === loginUserEmployeeId) {
            isProjectManager = true;
          }
        }
      }

      const userMapping = project.projectUserMappings?.[0];

      const customerName = project.customer?.name || "";
      const showCustomerName =
        customerName !== "" && customerName !== lastCustomerName;

      lastCustomerName = customerName;

      const tagsSrc = project.projectTags || [];
      const tags = new Array(tagsSrc.length);

      for (let t = 0; t < tagsSrc.length; t++) {
        const tg = tagsSrc[t].tags;
        tags[t] = {
          text: tg.name,
          value: tg.id,
          color: tg.color,
          bgColor: tg.bgColor
        };
      }

      result[i] = {
        ...project,

        isNotes: userMapping?.notes ?? false,
        isEditable: role === "admin" || (userMapping?.fullAccess ?? false),
        checkboxStatus: selectedIds.has(project.id),
        projectLeads: leads,
        projectCoordinators: coordinators,
        projectManager: manager,
        isProjectManager,

        projectTags: tags,
        showCustomerName
      };
    }
    rows.value = result;
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
  storageKey: "projects-Index",
  siteId: currentSiteId,

  defaultSearch: {
    searchText: "",
    projectIds: [],
    projectCategoryIds: [],
    projectStatusIds: [],
    statusId: null,
    projectCoordinatorIds: [],
    projectLeadsIds: [],
    projectPriorityIds: [],
    projectTypeIds: [],
    customerIds: [],
    companyContactIds: [],
    isTemplate: false,
    projectTagIds: null
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

const highlightedId = computed(() => {
  return activeRowId.value;
});
// ----------------------------------------------------------------------------------------------------------------
// DataTable:- List -> Custom functions & Calculate Column Totals (SOP Change)
// ----------------------------------------------------------------------------------------------------------------

const refreshProjectList = () => {
  getAllProjectList({ pagination: pagination.value });
};

const refreshProjectNameDropdown = () => {
  loadProjectNameDropdown();
};

const refreshProjectTagsDropdown = () => {
  projectTagsDropdown.load();
};

function totalEstimateHours () {
  const total = rows.value.reduce((total, row) => total + (row.totalTaskEstimateHours || 0), 0);
  return total.toFixed(2);
}

function totalTaskActivityHours () {
  const total = rows.value.reduce((total, row) => total + (row.totalActivityHours || 0), 0);
  return total.toFixed(2);
}

function getCountColor (total, completedCount) {
  if (!total || total === 0) return "grey";

  const percent = (completedCount / total) * 100;

  if (percent <= 25) return "red";
  if (percent <= 50) return "yellow";
  if (percent <= 75) return "blue";
  if (percent <= 100) return "green";

  return "green";
}

function setActiveRowIdInLocalStorage(id) {
  activeRowId.value = id;

  saveDataTableState({
    activeRowId: id
  });
}

function getNameFromId (id) {
  const found = activeEmployeesDropdown.value.find(opt => opt.value === id);
  return found ? found.text : "--";
}

function getInitials (fullName) {
  return fullName
    .split(" ")
    .map(word => word[0])
    .join("")
    .toUpperCase();
}

function handlePopupShow (statusLabel) {
  getProjectStatus("Project Status", statusLabel);
}

const onChangeProjectOrTemplate = () => {
  search.value.projectIds = [];
  refreshProjectList();
  loadProjectNameDropdown();
};

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
    refreshProjectList();
  }
});

// ------------------------------------------------------------------------------------
// DataTable :- Quick Edit Actions
// ------------------------------------------------------------------------------------

// Project Due Date
const disableBeforeStartDate = (startDate) => {
  if (!startDate) return () => true;

  // Convert MM/dd/yyyy string to Date
  const start = parse(startDate, "MM/dd/yyyy", new Date());

  return (date) => {
    // If incoming date is yyyy/MM/dd
    const currentDate = parse(date, "yyyy/MM/dd", new Date());

    return currentDate >= start;
  };
};

// Project Color
const isSliderActive = ref(false);
const previousColor = ref("");

const onStartProjectColorSelection = () => { isSliderActive.value = true; };

// Store previous color
const onRestorePreviousProjectColor = (color) => {
  previousColor.value = color ?? "#e0e0e0"; // Store previous color before opening picker
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------

initCustomerDialogs(activeRowId);
initProjectDialogs(activeRowId);
initProjectActions(activeRowId);
initSiteDialogs(activeRowId);

// ------------------------------------------------------------------------------------
// DataTable :- Remove Project Tags & Save Form
// ------------------------------------------------------------------------------------

const onDeleteProjectTag = (row, tagToRemove) => {
  zwConfirmDelete(
    {
      message: "Are you sure you want to remove the tag?"
    },
    () => {
      const updatedTags = row.projectTags.filter(
        t => t.value !== tagToRemove.value
      );
      onSubmitProjectTags(
        row.id,
        updatedTags,
        refreshProjectList,
        refreshProjectTagsDropdown
      );
    }
  );
};

// ------------------------------------------------------------------------------------
// Advance Filter :- On Submit & Cancel (SOP Change)
// ------------------------------------------------------------------------------------

const onAdvanceSearch = () => { refreshProjectList(); };

const onAdvanceClear = () => {
  search.value = {
    searchText: "",
    projectIds: [],
    projectCategoryIds: [],
    projectStatusIds: [],
    statusId: null,
    projectCoordinatorIds: [],
    projectLeadsIds: [],
    projectPriorityIds: [],
    projectTypeIds: [],
    customerIds: [],
    companyContactIds: [],
    isTemplate: false,
    projectTagIds: null
  };

  saveDataTableState({
    search: search.value
  });

  onAdvanceSearch();
};
// ------------------------------------------------------------------------------------
// Advance Filter :- Applied Filter Labels.
// ------------------------------------------------------------------------------------

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
  ...mapFilterToLabel(search.value.customerIds, customerNameDropdown.list, "Customer"),
  ...mapFilterToLabel(search.value.companyContactIds, companyContactNameDropdown.list, "Company Contact"),
  ...mapFilterToLabel(search.value.projectIds, projectNameDropdown.list, "Name"),
  ...mapFilterToLabel(search.value.projectCategoryIds, projectCategoriesDropdown.list, "Category"),
  ...mapFilterToLabel(search.value.projectCoordinatorIds, activeEmployeesDropdown.list, "Coordinator"),
  ...mapFilterToLabel(search.value.projectLeadsIds, activeEmployeesDropdown.list, "Leads"),
  ...mapFilterToLabel(search.value.projectStatusIds, statusListForSearch, "Status"),
  ...mapFilterToLabel(search.value.projectPriorityIds, projectPrioritiesDropdown.list, "Priority"),
  ...mapFilterToLabel(search.value.projectTypeIds, projectTypesDropdown.list, "Type"),
  ...mapFilterToLabel(search.value.projectTagIds, projectTagsDropdown.list, "Tags"),
  ...mapSingleFilterToLabel(search.value.statusId, projectActiveInActiveDropdown.list, "Active/Inactive")
}));

const onClearFilters = (key) => {
  if (key === "Customer") {
    search.value.customerIds = [];
  } else if (key === "Company Contact") {
    search.value.companyContactIds = [];
  } else if (key === "Name") {
    search.value.projectIds = [];
  } else if (key === "Category") {
    search.value.projectCategoryIds = [];
  } else if (key === "Coordinator") {
    search.value.projectCoordinatorIds = [];
  } else if (key === "Leads") {
    search.value.projectLeadsIds = [];
  } else if (key === "Status") {
    search.value.projectStatusIds = [];
  } else if (key === "Active/Inactive") {
    search.value.statusId = null;
  } else if (key === "Priority") {
    search.value.projectPriorityIds = [];
  } else if (key === "Type") {
    search.value.projectTypeIds = [];
  } else if (key === "Tags") {
    search.value.projectTagIds = [];
  }
  delete appliedFilters.value[key];
  refreshProjectList();
};

const getFilterCount = (key) => {
  switch (key) {
  case "Customer": return search.value.customerIds?.length || 0;
  case "Company Contact": return search.value.companyContactIds?.length || 0;
  case "Name": return search.value.projectIds?.length || 0;
  case "Category": return search.value.projectCategoryIds?.length || 0;
  case "Coordinator": return search.value.projectCoordinatorIds?.length || 0;
  case "Leads": return search.value.projectLeadsIds?.length || 0;
  case "Status": return search.value.projectStatusIds?.length || 0;
  case "Priority": return search.value.projectPriorityIds?.length || 0;
  case "Type": return search.value.projectTypeIds?.length || 0;
  default: return null; // For single-value filters like Year, Status
  }
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------

const {
  projectNameDropdown,
  projectCategoriesDropdown,
  projectActiveInActiveDropdown,
  projectPrioritiesDropdown,
  projectTypesDropdown,
  projectTagsDropdown
} = projectModule();

const { activeEmployeesDropdown } = employeeModule();
const { customerNameDropdown } = customerModule();
const { companyContactNameDropdown } = companyContactsModule();
const { getDropdownTypesByModuleNameForDropdown } = manageDropdownModule();
const { tagsDropdown } = tagModule();

// Custom Conditional Dropdown:- Get all project status list
const projectStatusList = ref([]);
const projectStatusFilter = ref([]);
const statusListForSearch = ref([]);

const getProjectStatus = (typeName, currentStatusLabel = null) => {
  commonService.getDropDown(typeName).then((resp) => {
    const allowedTransitions = {
      New: ["Open", "On Hold", "Cancelled"],
      Open: ["On Hold", "Cancelled"],
      "In progress": ["On Hold", "Completed"],
      "On Hold": ["Open", "Cancelled", "Completed"]
    };

    const responseData = resp.map((item) => {
      const label = item.dropdownValue;
      const isAllowed = allowedTransitions[currentStatusLabel]?.includes(label);
      const shouldDisable = currentStatusLabel === "Cancelled" || currentStatusLabel === "Completed" ? true : currentStatusLabel ? !isAllowed : false;

      return {
        text: label,
        value: item.id,
        disable: shouldDisable
      };
    });

    projectStatusList.value = responseData;
    statusListForSearch.value = responseData.map(item => ({ ...item, disable: false }));
    projectStatusFilter.value = statusListForSearch.value;
  });
};

// Search project status for dropdown
const getProjectStatusFilter = (val, update, abort) => {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      statusListForSearch.value = projectStatusFilter.value;
    } else {
      statusListForSearch.value = projectStatusFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
};

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

  projectNameDropdown.load(search.value.isTemplate, isActive);
}

onMounted(async () => {
  if (search.value.isTemplate === null || search.value.isTemplate === undefined) {
    search.value.isTemplate = false;
  }
  // Advance Filter Dropdown
  projectCategoriesDropdown.load("ProjectCategory");
  projectPrioritiesDropdown.load("Project Priorities");
  projectTypesDropdown.load("Project Type");
  projectTagsDropdown.load();
  activeEmployeesDropdown.load(user.siteId);
  customerNameDropdown.load();
  companyContactNameDropdown.load();
  tagsDropdown.load();
  getProjectStatus("Project Status");

  // Admin:- Manage all Project Dropdowns and Types
  manageDropDownTypes.value = await getDropdownTypesByModuleNameForDropdown("Project Management");

  // DataTable row clicks
  localStorage.removeItem("selectedProjectIds");

  // Get Project Active/InActive and Set default to Active
  await projectActiveInActiveDropdown.load("Project Active Status");
  const activeValue = await projectActiveInActiveDropdown.getValueByLabel("Active");
  const setProjectStatus = projectStatusList.value.find(status => status.text.toLowerCase() === "in progress");
  loadProjectNameDropdown();

  // Set Default values for advance filter
  if (search.value.statusId === null || search.value.statusId === undefined) search.value.statusId = activeValue;
  if (search.value.projectStatusIds?.length === 0) search.value.projectStatusIds = [setProjectStatus.value];

  refreshProjectList();
});

// ------------------------------------------------------------------------------------
// On Change (SOP Change)
// ------------------------------------------------------------------------------------

// Project Tags
watch(projectTagInputValue, (val) => {
  const needle = val ? val.toLowerCase() : "";
  tagsDropdown.list.value = needle ? tagsDropdown.list.value.filter((v) => v.text.toLowerCase().includes(needle)) : [...tagsDropdown.list.value];
});

// Quick Search
watch(() => search.value.searchText, () => {
  searchLoader.value = true;
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

// Advance Filter:-  On Company/Customer Change
watch(() => search.value.customerIds, async (newValue, oldValue) => {
  if (newValue === oldValue) return;
  if (newValue?.length === 0) companyContactNameDropdown.load();

  companyContactNameDropdown.load(newValue);
});
</script>
<style scoped>
.Custom-DataTable {
  min-width: max-content;
}
</style>
