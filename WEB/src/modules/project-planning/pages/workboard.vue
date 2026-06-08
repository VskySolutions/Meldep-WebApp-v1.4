<template>
  <q-page padding>
    <q-card class="project6 fs-15">
      <q-card-section class="card-header with-tools flex justify-between items-center">
        <q-breadcrumbs class="text-brown text-weight-bold text-h3">
          <template #separator>
            <q-icon size="1.5em" name="o_chevron_right" color="primary" />
          </template>
          <q-breadcrumbs-el label="Dashboard" icon="o_home" clickable to="/dashboard" />
          <q-breadcrumbs-el label="Projects" clickable to="/project" />
          <q-breadcrumbs-el :label="`WorkBoard : ${projectData.name != null ? projectData.name : ''}`" />
        </q-breadcrumbs>
        <div class="flex" float="right">
          <q-btn icon="o_visibility" outline no-caps color="primary" class="q-mr-xs" label="View" @click="openProjectView(projectId)" />
          <q-btn icon="o_attach_file" outline no-caps color="primary" class="q-mr-xs" label="Files" @click="openProjectFiles(projectId)" />
          <q-btn icon="o_chat" outline no-caps color="primary" class="q-mr-xs" label="Chat" @click="openProjectChatBox(projectId)" />
          <q-btn icon="o_assignment" outline no-caps color="primary" class="q-mr-xs" label="Notes" @click="openProjectNotes(projectId, 'Projects', projectId, '', '')" />
          <q-btn icon="o_developer_board" outline no-caps :color="!viewAsWorkBoardOrList ? 'secondary' : 'primary'" class="q-mr-xs" @click="viewAsWorkBoardOrList=true"><q-tooltip>WorkBoard View</q-tooltip></q-btn>
          <q-btn icon="o_format_list_bulleted" outline no-caps :color="viewAsWorkBoardOrList ? 'secondary' : 'primary'" class="q-mr-xs" @click="viewAsWorkBoardOrList = false"><q-tooltip>WorkBoard as List</q-tooltip></q-btn>
          <q-btn icon="o_add" outline no-caps color="primary" label="Add Swimlane" class="q-mr-xs" @click="showSwimLaneTypeModal = true" />
          <q-btn icon="o_chevron_left" outline no-caps color="secondary" :label="'Back'" @click="$router.back()" />
        </div>
      </q-card-section>
      <q-separator />
      <q-card-section class="card-body">
        <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
        <!-- View As WorkBoard -->
        <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
        <div v-if="viewAsWorkBoardOrList" id="sortable-swimlanes" class="row inline full-width">
          <div
            v-for="(lane, swimLaneIndex) in projectData.projectSwimLanes"
            :id="`swimlane-${lane.sortOrder}`"
            :key="lane.id"
            class="swimlane q-mb-md full-width relative-position" :class="lane?.deleted ? 'hidden' : ''"
          >
            <div class="row swimlane-header cursor-grab">
              <div class="col-xxl-11 col-lg-11 col-md-10 col-sm-10 col-xs-9 flex items-center">
                <div class="row full-width items-center">
                  <div class="q-mr-xs">
                    <q-icon
                      :name="collapsedSwimlane.includes(lane.id) ? 'o_expand_more' : 'o_expand_less'"
                      class="cursor-pointer"
                      size="md"
                      @click.stop="toggleCollapse(projectId, lane.id)"
                    >
                      <q-tooltip>{{ collapsedSwimlane.includes(lane.id) ? 'Close' : 'Open' }} SwimLane</q-tooltip>
                    </q-icon>
                  </div>
                  <div class="cursor-pointer min-width-200">
                    <!-- SwimLane Name -->
                    <div v-if="editingSwimlaneName[lane.id] === lane.id">
                      <q-input
                        v-model="lane.name"
                        class="min-width-200"
                        @blur="editingSwimlaneName[lane.id] = null"
                        @keyup.enter="editingSwimlaneName[lane.id] = null"
                        @change="handleLaneNameChange(lane)"
                      />
                    </div>
                    <div v-else class="flex items-center full-width fs-18 fs-bold min-width-200" @dblclick="editingSwimlaneName[lane.id] = lane.id">
                      <q-icon v-if="lane.color" name="o_circle" class="q-mr-xs" :style="`border-radius:50%; color:${lane.color}; background-color:${lane.color};`" size="xs" />
                      <div class="min-width-200">
                        <span>{{ lane.name === '' ? 'Untitled' : lane.name }} </span>
                        <span style="font-size: 11px; color: #898989;">{{ ' - ('+ lane.swimlaneType.dropDownValue + ')' }}</span>
                        <q-tooltip>Double Click To Edit</q-tooltip>
                      </div>
                    </div>
                    <q-tooltip>Double Click To Edit</q-tooltip>
                  </div>
                </div>
              </div>
              <div class="col-xxl-1 col-lg-1 col-md-2 col-sm-2 col-xs-3 text-end">
                <!-- SwimLane View More -->
                <q-btn v-if="lane.swimlaneType.dropDownValue !== defaultSwimlaneTypeName" dense flat icon="o_add" size="sm" color="primary" @click="addList(swimLaneIndex)">
                  <q-tooltip>Add List</q-tooltip>
                </q-btn>
                <q-btn dense flat icon="o_more_vert" size="sm" color="primary" class="q-pa-none" @click.stop>
                  <q-menu>
                    <q-list>
                      <q-separator />
                      <q-item v-if="lane.swimlaneType.dropDownValue !== defaultSwimlaneTypeName" v-ripple clickable @click="showComingSoon()">
                        <q-item-section avatar><q-icon name="o_content_copy" color="secondary" size="xs" /></q-item-section>
                        <q-item-section class="">Copy Swimlane To Other Project?</q-item-section>
                      </q-item>
                      <q-item v-if="lane.swimlaneType.dropDownValue !== defaultSwimlaneTypeName" v-ripple v-close-popup clickable @click="showComingSoon()">
                        <q-item-section avatar><q-icon name="o_folder_copy" color="secondary" size="xs" /></q-item-section>
                        <q-item-section class="">Duplicate Swimlane...</q-item-section>
                      </q-item>
                      <q-separator />
                      <q-item v-ripple clickable>
                        <q-item-section avatar><q-icon name="o_colorize" color="primary" size="xs" /></q-item-section>
                        <q-item-section class="">List Color</q-item-section>
                        <q-menu anchor="top end" self="top start" auto-close>
                          <q-color v-model="lane.color" v-ripple clickable no-header no-footer class="my-picker" @change="saveBoardToStorage(false)" />
                        </q-menu>
                      </q-item>
                      <q-separator />
                      <q-item v-ripple clickable @click="showComingSoon()">
                        <q-item-section avatar><q-icon name="o_sort" color="secondary" size="xs" /></q-item-section>
                        <q-item-section class="">Sort By</q-item-section>
                      </q-item>
                      <q-separator />
                      <q-item v-if="lane.swimlaneType.dropDownValue !== defaultSwimlaneTypeName" v-ripple v-close-popup clickable @click="removeSwimLane(swimLaneIndex)">
                        <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                        <q-item-section class="text-negative">Delete SwimLane?</q-item-section>
                      </q-item>
                    </q-list>
                    <q-tooltip>View More</q-tooltip>
                  </q-menu>
                </q-btn>
              </div>
            </div>
            <q-slide-transition>
              <div v-show="collapsedSwimlane.includes(lane.id)">
                <!-- Sub-Swim Lanes as Columns -->
                <div v-if="lane?.projectSwimLanesList?.length > 0" class="row">
                  <!-- Left and Right Arrow Buttons -->
                  <q-btn dense flat icon="o_arrow_left" size="xl" color="primary" class="absolute-left arrows" @click="scrollSwimlane(lane.id, 'left')" />
                  <q-btn dense flat icon="o_arrow_right" size="xl" color="primary" class="absolute-right arrows" @click="scrollSwimlane(lane.id, 'right')" />
                </div>
                <div v-if="lane?.projectSwimLanesList?.length === 0" class="text-center text-red q-mt-md">
                  <span>No list available. <b icon="o_add" class="cursor-pointer text-grey-8" @click="addList(swimLaneIndex)">Add Now</b></span>
                </div>
                <div :id="'Sortable-Sublanes-' + lane.id" class="sublanesBody row inline no-wrap overflow-auto full-width q-mt-sm q-px-xl">
                  <div
                    v-for="(list, listIndex) in lane?.projectSwimLanesList"
                    :id="'sublanes-' + list.id"
                    :key="list.id"
                    class="col-xxl-2 col-lg-2 col-md-3 col-sm-4 col-xs-12 q-pa-xs SubSwimlane cursor-grab SubSwimlane-drag-handle "
                    :class="list?.deleted ? 'hidden' : ''"
                    :data-item="JSON.stringify(list)"
                    @mouseenter="projectListHovered = list.id"
                    @mouseleave="projectListHovered = null"
                  >
                    <div class="SubSwimlane-body full-height">
                      <div class="row SubSwimlane-header flex theme-blue full-width q-pa-none">
                        <div class="col-11 flex items-center cursor-pointer">
                          <!-- List Name -->
                          <div v-if="editingListName[list.id] === list.id" class="full-width min-width-200">
                            <q-input
                              v-model="list.name"
                              class="full-width min-width-200"
                              autogrow
                              @change="handleListNameChange(list)"
                              @blur="editingListName[list.id] = null"
                              @keyup.enter="editingListName[list.id] = null"
                            />
                          </div>
                          <div v-else class="flex items-center full-width fs-bold" @dblclick="editingListName[list.id] = list.id">
                            <q-icon v-if="list.color" name="o_circle" class="q-mr-xs" :style="`border-radius:50%; color:${list.color}; background-color:${list.color};`" size="xs" />
                            <div class="min-width-200">
                              {{ list.name === '' ? 'Untitled' : list.name }}
                              <q-tooltip>Double Click To Edit</q-tooltip>
                            </div>
                          </div>
                        </div>
                        <div class="col-1">
                          <!-- List View More -->
                          <q-btn dense flat icon="o_more_vert" size="sm" color="primary" style="padding: 0px !important;" @click.stop>
                            <q-menu>
                              <q-list>
                                <!-- <q-item v-ripple clickable @click="openListView(list.id)">
                                  <q-item-section avatar><q-icon name="o_visibility" color="secondary" size="xs" /></q-item-section>
                                  <q-item-section class="">View List</q-item-section>
                                </q-item>
                                <q-separator /> -->
                                <!-- <q-item v-ripple clickable>
                                  <q-item-section avatar><q-icon name="o_flag" color="secondary" size="xs" /></q-item-section>
                                  <q-item-section class="">Change Status?</q-item-section>
                                  <q-menu anchor="top end" self="top start" auto-close>
                                    <q-item v-for="status in projectListStatusList" :key="status.value" v-ripple clickable @click="onListDropDownChange(list, status.value, status.text)">
                                      <q-item-section avatar>
                                        <q-icon v-if="status.value === list.projectModuleStatusId" name="o_task_alt" color="primary" size="xs" />
                                        <q-icon v-else name="o_radio_button_unchecked" color="secondary" size="xs" />
                                      </q-item-section>
                                      <q-item-section class="">{{ status.text }}</q-item-section>
                                    </q-item>
                                  </q-menu>
                                </q-item>
                                <q-separator /> -->
                                <q-item v-ripple clickable @click="showComingSoon()">
                                  <q-item-section avatar><q-icon name="o_folder_copy" color="secondary" size="xs" /></q-item-section>
                                  <q-item-section class="">Copy List To Other Project...</q-item-section>
                                </q-item>
                                <q-item v-ripple v-close-popup clickable @click="showComingSoon()">
                                  <q-item-section avatar><q-icon name="o_folder_copy" color="secondary" size="xs" /></q-item-section>
                                  <q-item-section class="">Duplicate List?</q-item-section>
                                </q-item>
                                <q-separator />
                                <q-item v-ripple clickable>
                                  <q-item-section avatar><q-icon name="o_colorize" color="primary" size="xs" /></q-item-section>
                                  <q-item-section class="">List Color</q-item-section>
                                  <q-menu anchor="top end" self="top start" auto-close>
                                    <q-color v-model="list.color" v-ripple clickable no-header no-footer class="my-picker" @change="saveBoardToStorage(false)" />
                                  </q-menu>
                                </q-item>
                                <q-separator />
                                <q-item v-ripple clickable @click="showComingSoon()">
                                  <q-item-section avatar><q-icon name="o_sort" color="secondary" size="xs" /></q-item-section>
                                  <q-item-section class="">Sort By</q-item-section>
                                </q-item>
                                <q-separator />
                                <q-item v-ripple v-close-popup clickable @click="removeList(swimLaneIndex, listIndex)">
                                  <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                                  <q-item-section class="text-negative">Delete List?</q-item-section>
                                </q-item>
                              </q-list>
                            </q-menu>
                            <q-tooltip>View More</q-tooltip>
                          </q-btn>
                        </div>
                      </div>
                      <div class="row">
                        <q-list :id="'sortable-tasks-' + list.id" :data-list-id="list.id" class="full-width q-pa-sm">
                          <q-item v-for="(listTask, taskIndex) in list.projectSwimLanesListsTasks" :key="listTask.id" class="task-box col-12 cursor-grab" :class="listTask?.deleted ? 'hidden' : ''" draggable="true" :data-item="JSON.stringify(listTask)">
                            <q-item-section>
                              <div :class="getPriorityBorderClass(listTask.projectTask.priority?.dropDownValue, false)" class="row text-center" />
                              <div v-if="lane.swimlaneType.dropDownValue !== defaultSwimlaneTypeName" class="row justify-center" style="background-color: #f5f5f5;">{{ listTask.projectTask.status?.dropDownValue }}</div>
                              <div class="row items-center q-px-sm q-py-sm">
                                <div class="col-11 cursor-pointer">
                                  <!-- Task Name Edit -->
                                  <div v-if="editingTaskName[listTask.projectTask.id] === listTask.projectTask.id" class="min-width-200">
                                    <q-input
                                      v-model="listTask.projectTask.name"
                                      outlined stack-label hide-bottom-space autogrow
                                      class="min-width-200"
                                      :data-item="JSON.stringify(listTask.projectTask)"
                                      @change="handleTaskNameChange(listTask.projectTask)"
                                      @blur="editingTaskName[listTask.projectTask.id] = null"
                                      @keyup.enter="editingTaskName[listTask.projectTask.id] = null"
                                    />
                                  </div>
                                  <div v-else class="min-width-200" @dblclick="editingTaskName[listTask.projectTask.id] = listTask.projectTask.id">
                                    <q-icon v-if="listTask.projectTask.color" name="o_circle" class="q-mr-xs" :style="`border-radius:50%;color:${listTask.projectTask.color}; background-color:${listTask.projectTask.color};`" size="xs" />
                                    {{ listTask.projectTask.name === '' ? 'Untitled' : listTask.projectTask.name }}
                                  </div>
                                  <q-tooltip>Double Click To Edit</q-tooltip>
                                </div>
                                <div class="col-1">
                                  <q-btn dense flat icon="o_more_vert" size="sm" color="secondary" auto-close="true" style="padding: 0px !important;" @click.stop>
                                    <q-menu>
                                      <q-list>
                                        <q-item v-ripple clickable @click="onProjectTaskView(listTask.projectTask.id)">
                                          <q-item-section avatar><q-icon name="o_visibility" color="secondary" size="xs" /></q-item-section>
                                          <q-item-section class="">View</q-item-section>
                                        </q-item>
                                        <q-item v-ripple clickable @click="onProjectTaskEdit(projectId, listTask.projectTask.id)">
                                          <q-item-section avatar><q-icon name="o_edit" size="xs" /></q-item-section>
                                          <q-item-section>Edit</q-item-section>
                                        </q-item>
                                        <q-separator />
                                        <q-item
                                          v-ripple
                                          v-close-popup
                                          clickable
                                          @click="{
                                            getAllActivityFromTask(projectId, listTask.projectTask.projectModuleId, listTask.projectTask.id);
                                            selectedTimerTask.taskId = listTask.projectTask.id;
                                            selectedTimerTask.taskName = listTask.projectTask.name;
                                            selectedTimerTask.activityId = '';
                                            selectedTimerTask.activityName = '';
                                            showTaskDetailsForTimerModal = true;
                                          }"
                                        >
                                          <q-item-section avatar><q-icon name="o_play_circle" color="secondary" size="xs" /></q-item-section>
                                          <q-item-section class="">Start Timer?</q-item-section>
                                        </q-item>
                                        <q-separator />
                                        <q-item v-ripple clickable>
                                          <q-item-section avatar><q-icon name="o_person_add" color="secondary" size="xs" /></q-item-section>
                                          <q-item-section class="">Change Module?</q-item-section>
                                          <q-menu anchor="top end" self="top start">
                                            <q-item v-ripple clickable class="flex column">
                                              <div class="q-pa-xs">
                                                <q-select
                                                  v-model="listTask.projectTask.projectModuleId"
                                                  use-input
                                                  outlined
                                                  stack-label
                                                  :dense="true"
                                                  :options="projectModuleList"
                                                  option-value="value"
                                                  option-label="text"
                                                  emit-value
                                                  map-options
                                                  @filter="getAllProjectModuleListForFilter"
                                                  @update:model-value="saveBoardToStorage(true)"
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
                                                <q-btn v-close-popup outline icon="o_close" size="sm" color="secondary" label="Close" class="q-mt-xs" />
                                              </div>
                                            </q-item>
                                          </q-menu>
                                        </q-item>
                                        <q-item v-if="lane.swimlaneType.dropDownValue !== defaultSwimlaneTypeName" v-ripple clickable>
                                          <q-item-section avatar><q-icon name="o_flag" color="secondary" size="xs" /></q-item-section>
                                          <q-item-section class="">Change Status?</q-item-section>
                                          <q-menu anchor="top end" self="top start" auto-close>
                                            <q-item v-for="status in projectTaskStatusList" :key="status.value" v-ripple clickable @click="onTaskDropDownChange(listTask.projectTask, status.value, status.text,'Task-Status')">
                                              <q-item-section avatar>
                                                <q-icon v-if="status.value === listTask.projectTask.statusId" name="o_task_alt" color="primary" size="xs" />
                                                <q-icon v-else name="o_radio_button_unchecked" color="secondary" size="xs" />
                                              </q-item-section>
                                              <q-item-section class="">{{ status.text }}</q-item-section>
                                            </q-item>
                                          </q-menu>
                                        </q-item>
                                        <q-item v-ripple clickable>
                                          <q-item-section avatar><q-icon name="o_priority_high" color="red" size="xs" /></q-item-section>
                                          <q-item-section class="">Change Priority?</q-item-section>
                                          <q-menu anchor="top end" self="top start" auto-close>
                                            <q-item v-for="priority in projectTaskPriorityList" :key="priority.value" v-ripple clickable @click="onTaskDropDownChange(listTask.projectTask, priority.value, priority.text,'Task-Priority')">
                                              <q-item-section avatar>
                                                <q-icon v-if="priority.value === listTask.projectTask.priorityId" name="o_task_alt" color="primary" size="xs" />
                                                <q-icon v-else name="o_radio_button_unchecked" color="secondary" size="xs" />
                                              </q-item-section>
                                              <q-item-section class="">{{ priority.text }}</q-item-section>
                                            </q-item>
                                          </q-menu>
                                        </q-item>
                                        <q-item v-ripple clickable>
                                          <q-item-section avatar><q-icon name="o_local_offer" size="xs" /></q-item-section>
                                          <q-item-section>Add Tags?</q-item-section>
                                          <q-menu anchor="top end" self="top start" class="q-pa-sm">
                                            <TagEditor
                                              v-model="listTask.projectTask.taskTags"
                                              :row-id="listTask.projectTask.id"
                                              :available-tags="availableTags"
                                              @save="({ tags, rowId }) => saveTagsData(tags, rowId)"
                                              @filter="filterTags"
                                            />
                                            <q-btn v-close-popup outline icon="o_close" size="sm" color="secondary" label="Close" class="q-mt-xs" />
                                          </q-menu>
                                        </q-item>
                                        <q-separator />
                                        <q-item v-ripple clickable>
                                          <q-item-section avatar><q-icon name="o_schedule" color="secondary" size="xs" /></q-item-section>
                                          <q-item-section class="">Total Est. Hrs?</q-item-section>
                                          <q-menu anchor="top end" self="top start" class="q-pa-sm">
                                            <q-input v-model="listTask.projectTask.estimateTime" outlined stack-label hide-bottom-space :dense="true" :rules="[validateTaskEstimatedHours]" hint="hh.mm" maxlength="5" @change="saveBoardToStorage(false)" />
                                            <q-btn v-close-popup outline icon="o_close" size="sm" color="secondary" label="Close" class="q-mt-xs" />
                                          </q-menu>
                                        </q-item>
                                        <q-item v-ripple clickable>
                                          <q-item-section avatar><q-icon name="o_person_add" color="secondary" size="xs" /></q-item-section>
                                          <q-item-section class="">Task Owner?</q-item-section>
                                          <q-menu anchor="top end" self="top start">
                                            <q-item v-ripple clickable class="flex column">
                                              <div class="q-pa-xs">
                                                <q-select
                                                  v-model="listTask.projectTask.assignedToId"
                                                  use-input
                                                  outlined
                                                  stack-label
                                                  :dense="true"
                                                  :options="employeeList"
                                                  option-value="value"
                                                  option-label="text"
                                                  emit-value
                                                  map-options
                                                  @filter="getAllActiveEmployeesListFilter"
                                                  @update:model-value="saveBoardToStorage(false)"
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
                                                <q-btn v-close-popup outline icon="o_close" size="sm" color="secondary" label="Close" class="q-mt-xs" />
                                              </div>
                                            </q-item>
                                          </q-menu>
                                        </q-item>
                                        <q-item v-ripple v-close-popup clickable @click="openTaskAssignment(listTask.projectTask.id, projectId, listTask.projectTask.projectModuleId, listTask.projectTask.name, projectData.name, listTask.projectTask.projectModule.name)">
                                          <q-item-section avatar><q-icon name="o_list_alt" color="secondary" size="xs" /></q-item-section>
                                          <q-item-section class="">Task Assignment?</q-item-section>
                                        </q-item>
                                        <q-separator />
                                        <q-item v-ripple v-close-popup clickable @click="duplicateTask(listTask.projectTask.id)">
                                          <q-item-section avatar><q-icon name="o_folder_copy" color="secondary" size="xs" /></q-item-section>
                                          <q-item-section class="">Duplicate Task?</q-item-section>
                                        </q-item>
                                        <q-item v-ripple clickable @click="onCopyTask(listTask.projectTask.id, listTask.projectTask.name, listTask.projectTask.projectModuleId, 'isCopy')">
                                          <q-item-section avatar><q-icon name="o_copy" color="secondary" size="xs" /></q-item-section>
                                          <q-item-section class="">Copy Tasks To...</q-item-section>
                                        </q-item>
                                        <q-item v-ripple clickable @click="onMoveTask(listTask.projectTask.id, listTask.projectTask.name, listTask.projectTask.projectModuleId, 'isMove')">
                                          <q-item-section avatar><q-icon name="o_arrow_forward" color="secondary" size="xs" /></q-item-section>
                                          <q-item-section class="">Move Tasks To...</q-item-section>
                                        </q-item>
                                        <q-separator />
                                        <q-item v-ripple clickable>
                                          <q-item-section avatar><q-icon name="o_colorize" color="primary" size="xs" /></q-item-section>
                                          <q-item-section class="">Task Color</q-item-section>
                                          <q-menu anchor="top end" self="top start" auto-close>
                                            <q-color v-model="listTask.projectTask.color" v-ripple clickable no-header no-footer class="my-picker" @change="saveBoardToStorage(false)" />
                                          </q-menu>
                                        </q-item>
                                        <q-separator />
                                        <q-item v-ripple v-close-popup clickable @click="removeTask(swimLaneIndex, listIndex, taskIndex)">
                                          <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                                          <q-item-section class="text-negative">Delete Task</q-item-section>
                                        </q-item>
                                      </q-list>
                                    </q-menu>
                                    <q-tooltip>View More</q-tooltip>
                                  </q-btn>
                                </div>
                              </div>
                              <div class="row items-center">
                                <div class="col-5 flex justify-center q-pa-xs">
                                  <q-badge rounded color="grey">
                                    <q-icon name="o_calendar_month" color="white" size="xs" class="q-mr-xs" />
                                    {{ shortDate(listTask.projectTask.startDate) }}
                                  </q-badge>
                                  <q-tooltip>Task StartDate</q-tooltip>
                                </div>
                                <div v-if="listTask.projectTask.priority" class="col-4 flex justify-center">
                                  <q-badge rounded :color="getPriorityBadgeClass(listTask.projectTask.priority?.dropDownValue)">
                                    <q-icon name="o_priority_high" size="xs" class="q-mr-xs" />
                                    {{ listTask.projectTask.priority?.dropDownValue }}
                                  </q-badge>
                                  <q-tooltip>Task Priority</q-tooltip>
                                </div>
                                <div class="col-3 flex justify-center">
                                  <q-badge v-if="listTask.projectTask.estimateTime" rounded>
                                    <q-icon name="o_schedule" color="white" size="xs" class="q-mr-xs" />
                                    {{ listTask.projectTask.estimateTime }}
                                  </q-badge>
                                  <q-badge v-else rounded color="grey">
                                    <q-icon name="o_schedule" color="white" size="xs" class="q-mr-xs" />
                                    0.00
                                  </q-badge>
                                  <q-tooltip>Total Est. Hrs</q-tooltip>
                                </div>
                              </div>
                              <q-separator />
                              <div class="row items-center justify-center">
                                <span class="fs-12">{{ listTask.projectTask.projectModule.name }}</span>
                                <q-tooltip>Task Module</q-tooltip>
                              </div>
                              <q-separator />
                              <div class="row items-center">
                                <!-- Already saved tags for the task -->
                                <div v-if="listTask.projectTask.projectTask_Tags?.length > 0" class="col-12 flex items-center q-pa-xs">
                                  <q-badge
                                    v-for="taskTag in listTask.projectTask.projectTask_Tags"
                                    :key="taskTag.id"
                                    :style="{
                                      backgroundColor: taskTag.tags?.bgColor || 'primary',
                                      color: taskTag.tags?.color || '#191919'
                                    }"
                                    class="q-mr-xs q-mb-xs"
                                  >
                                    {{ taskTag.tags?.name }}
                                  </q-badge>
                                  <q-tooltip>Task Tags</q-tooltip>
                                </div>
                              </div>
                              <q-separator />
                              <div class="row items-center">
                                <!-- Task Owner - Label -->
                                <div class="col-3">
                                  <div v-if="listTask.projectTask.assignedTo?.id">
                                    <q-icon name="o_person" size="xs" color="primary" class="q-mr-xs">
                                      <q-tooltip>Task Owner?</q-tooltip>
                                    </q-icon>
                                    <q-badge rounded color="primary">
                                      {{ listTask.projectTask.assignedTo?.person?.firstName[0] + listTask.projectTask.assignedTo?.person?.lastName[0] }}
                                      <q-tooltip>
                                        <div>
                                          <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                                          <span>{{ listTask.projectTask.assignedTo?.person?.firstName + ' ' + listTask.projectTask.assignedTo?.person?.lastName }}</span>
                                        </div>
                                        <div>
                                          <q-icon name="o_mail" color="white" size="xs" class="q-mr-xs" />
                                          <span>{{ listTask.projectTask.assignedTo?.person?.primaryEmailAddress }}</span>
                                        </div>
                                      </q-tooltip>
                                    </q-badge>
                                  </div>
                                  <div v-else>
                                    <q-icon name="o_person" size="xs" color="primary" />
                                    <q-icon name="o_question_mark" size="xs" color="secondary" class="q-mr-xs" />
                                    <q-tooltip>Task Owner</q-tooltip>
                                  </div>
                                </div>
                                <div class="col-9 flex justify-end q-pa-xs TaskActivity">
                                  <div class="cursor-pointer" @click="openTaskAssignment(listTask.projectTask.id, projectId, listTask.projectTask.projectModuleId, listTask.projectTask.name, projectData.name, listTask.projectTask.projectModule.name)">
                                    <q-icon name="o_group_add" color="secondary" size="sm" class="q-mr-xs" />
                                    <q-tooltip>Manage Task Assignments?</q-tooltip>
                                  </div>
                                  <div v-if="listTask.projectTask.projectActivities?.length > 0" class="flex">
                                    <div v-for="activity in listTask.projectTask.projectActivities" :key="activity.id" class="Person">
                                      {{ (activity?.assignedTo?.person?.firstName?.trim()?.[0] || '') + (activity?.assignedTo?.person?.lastName?.trim()?.[0] || '') }}
                                      <q-tooltip>
                                        <div>
                                          <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                                          <span>{{ activity.assignedTo?.person?.firstName + ' ' + activity.assignedTo?.person?.lastName }}</span>
                                        </div>
                                        <div>
                                          <q-icon name="o_mail" color="white" size="xs" class="q-mr-xs" />
                                          <span>{{ activity.assignedTo?.person?.primaryEmailAddress }}</span>
                                        </div>
                                        <div v-if="activity.estimateHours > 0">
                                          <q-icon name="o_timer" color="white" size="xs" class="q-mr-xs" />
                                          <span>{{ activity.estimateHours }} hrs</span>
                                        </div>
                                      </q-tooltip>
                                    </div>
                                  </div>
                                </div>
                              </div>
                            </q-item-section>
                          </q-item>
                          <q-item class="q-pa-none table text-end" style="min-height: 48px;">
                            <!-- Add Project Task -->
                            <q-btn v-show="projectListHovered === list.id" color="primary" icon="o_add" outline no-caps label="Add Task" class="full-width q-mt-sm" style="height: 30px; min-height: 30px;" @click="addTask(swimLaneIndex, listIndex)" />
                          </q-item>
                        </q-list>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </q-slide-transition>
          </div>
        </div>
        <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
        <!-- View As List WorkBoard -->
        <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
        <div v-else>
          <div
            v-for="(lane, swimLaneIndex) in projectData.projectSwimLanes"
            :id="`swimlane-${lane.sortOrder}`"
            :key="lane.id"
            class="swimlane-list full-width cursor-grab relative-position" :class="lane?.deleted ? 'hidden' : ''"
          >
            <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
            <!-- SwimLane Header -->
            <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
            <div class="row swimlane-header">
              <div class="col-xxl-11 col-lg-11 col-md-10 col-sm-10 col-xs-9 flex items-center">
                <div class="row full-width items-center">
                  <!-- SwimLane Toggle Action -->
                  <div class="q-mr-xs">
                    <q-icon
                      :name="collapsedSwimlane.includes(lane.id) ? 'o_expand_more' : 'o_expand_less'"
                      class="cursor-pointer"
                      size="md"
                      @click.stop="toggleCollapse(projectId, lane.id)"
                    >
                      <q-tooltip>{{ collapsedSwimlane.includes(lane.id) ? 'Close' : 'Open' }} SwimLane</q-tooltip>
                    </q-icon>
                  </div>
                  <!-- SwimLane Name -->
                  <div class="cursor-pointer min-width-200">
                    <!-- SwimLane Name - Input -->
                    <div v-if="editingSwimlaneName[lane.id] === lane.id">
                      <q-input
                        v-model="lane.name"
                        class="min-width-200"
                        @blur="editingSwimlaneName[lane.id] = null"
                        @keyup.enter="editingSwimlaneName[lane.id] = null"
                        @change="handleLaneNameChange(lane)"
                      />
                    </div>
                    <!-- SwimLane Name - Label -->
                    <div v-else class="flex items-center full-width fs-20 fs-bold min-width-200" @dblclick="editingSwimlaneName[lane.id] = lane.id">
                      <q-icon v-if="lane.color" name="o_circle" class="q-mr-xs" :style="`border-radius:50%; color:${lane.color}; background-color:${lane.color};`" size="xs" />
                      <div class="min-width-200">
                        {{ lane.name === '' ? 'Untitled' : lane.name }}
                        <q-tooltip>Double Click To Edit</q-tooltip>
                      </div>
                    </div>
                    <q-tooltip>Double Click To Edit</q-tooltip>
                  </div>
                </div>
              </div>
              <!-- SwimLane Actions -->
              <div class="col-xxl-1 col-lg-1 col-md-2 col-sm-2 col-xs-3 text-end">
                <!-- SwimLane - Add -->
                <q-btn v-if="lane.swimlaneType.dropDownValue !== defaultSwimlaneTypeName" dense flat icon="o_add" size="sm" color="primary" @click="addList(swimLaneIndex)">
                  <q-tooltip>Add List</q-tooltip>
                </q-btn>
                <!-- SwimLane - View More -->
                <q-btn dense flat icon="o_more_vert" size="sm" color="primary" class="q-pa-none" @click.stop>
                  <q-menu>
                    <q-list>
                      <q-separator />
                      <q-item v-if="lane.swimlaneType.dropDownValue !== defaultSwimlaneTypeName" v-ripple clickable @click="showComingSoon()">
                        <q-item-section avatar><q-icon name="o_content_copy" color="secondary" size="xs" /></q-item-section>
                        <q-item-section class="">Copy Swimlane To Other Project?</q-item-section>
                      </q-item>
                      <q-item v-if="lane.swimlaneType.dropDownValue !== defaultSwimlaneTypeName" v-ripple v-close-popup clickable @click="showComingSoon()">
                        <q-item-section avatar><q-icon name="o_folder_copy" color="secondary" size="xs" /></q-item-section>
                        <q-item-section class="">Duplicate Swimlane...</q-item-section>
                      </q-item>
                      <q-separator />
                      <q-item v-ripple clickable>
                        <q-item-section avatar><q-icon name="o_colorize" color="primary" size="xs" /></q-item-section>
                        <q-item-section class="">List Color</q-item-section>
                        <q-menu anchor="top end" self="top start" auto-close>
                          <q-color v-model="lane.color" v-ripple clickable no-header no-footer class="my-picker" @change="saveBoardToStorage(false)" />
                        </q-menu>
                      </q-item>
                      <q-separator />
                      <q-item v-ripple clickable @click="showComingSoon()">
                        <q-item-section avatar><q-icon name="o_sort" color="secondary" size="xs" /></q-item-section>
                        <q-item-section class="">Sort By</q-item-section>
                      </q-item>
                      <q-separator />
                      <q-item v-if="lane.swimlaneType.dropDownValue !== defaultSwimlaneTypeName" v-ripple v-close-popup clickable @click="removeSwimLane(swimLaneIndex)">
                        <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                        <q-item-section class="text-negative">Delete SwimLane?</q-item-section>
                      </q-item>
                    </q-list>
                    <q-tooltip>View More</q-tooltip>
                  </q-menu>
                </q-btn>
              </div>
            </div>
            <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
            <!-- SwimLane Body -->
            <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
            <q-slide-transition>
              <div v-show="collapsedSwimlane.includes(lane.id)" class="row swimlane-body">
                <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                <!-- SwimLane -- List -- Default Message -->
                <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                <div v-if="lane?.projectSwimLanesList?.length === 0" class="col-12 text-center text-red q-mt-md">
                  <span>No list available. <b icon="o_add" class="cursor-pointer text-grey-8" @click="addList(swimLaneIndex)">Add Now</b></span>
                </div>
                <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                <!-- SwimLane -- List -->
                <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                <div :id="'Sortable-Sublanes-' + lane.id" class="col-12 sublanesBody">
                  <div class="row">
                    <div
                      v-for="(list, listIndex) in lane?.projectSwimLanesList"
                      :id="'sublanes-' + list.id"
                      :key="list.id"
                      class="col-12 SubSwimlane-drag-handle "
                      :class="list?.deleted ? 'hidden' : ''"
                      :data-item="JSON.stringify(list)"
                      @mouseenter="projectListHovered = list.id"
                      @mouseleave="projectListHovered = null"
                    >
                      <div class="SubSwimlane full-height">
                        <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                        <!-- SwimLane -- List -- Header -->
                        <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                        <div class="row SubSwimlane-header flex items-center theme-blue full-width q-pa-none">
                          <!-- List Right Arrow -->
                          <div>
                            <q-icon name="o_keyboard_double_arrow_right" size="sm" />
                          </div>
                          <!-- List Actions -->
                          <div class="ListViewMore">
                            <q-btn dense flat icon="o_more_vert" size="sm" color="primary" class="q-mr-xs" style="padding: 0px !important;min-height: auto;" @click.stop>
                              <q-menu>
                                <q-list>
                                  <!-- <q-item v-ripple clickable @click="openListView(list.id)">
                                    <q-item-section avatar><q-icon name="o_visibility" color="secondary" size="xs" /></q-item-section>
                                    <q-item-section class="">View List</q-item-section>
                                  </q-item>
                                  <q-separator />
                                  <q-item v-ripple clickable>
                                    <q-item-section avatar><q-icon name="o_flag" color="secondary" size="xs" /></q-item-section>
                                    <q-item-section class="">Change Status?</q-item-section>
                                    <q-menu anchor="top end" self="top start" auto-close>
                                      <q-item v-for="status in projectListStatusList" :key="status.value" v-ripple clickable @click="onListDropDownChange(list, status.value, status.text)">
                                        <q-item-section avatar>
                                          <q-icon v-if="status.value === list.projectModuleStatusId" name="o_task_alt" color="primary" size="xs" />
                                          <q-icon v-else name="o_radio_button_unchecked" color="secondary" size="xs" />
                                        </q-item-section>
                                        <q-item-section class="">{{ status.text }}</q-item-section>
                                      </q-item>
                                    </q-menu>
                                  </q-item>
                                  <q-separator />-->
                                  <q-item v-ripple clickable @click="showComingSoon()">
                                    <q-item-section avatar><q-icon name="o_folder_copy" color="secondary" size="xs" /></q-item-section>
                                    <q-item-section class="">Copy List To Other Project...</q-item-section>
                                  </q-item>
                                  <q-item v-ripple v-close-popup clickable @click="showComingSoon()">
                                    <q-item-section avatar><q-icon name="o_folder_copy" color="secondary" size="xs" /></q-item-section>
                                    <q-item-section class="">Duplicate List?</q-item-section>
                                  </q-item>
                                  <q-separator />
                                  <q-item v-ripple clickable>
                                    <q-item-section avatar><q-icon name="o_colorize" color="primary" size="xs" /></q-item-section>
                                    <q-item-section class="">List Color</q-item-section>
                                    <q-menu anchor="top end" self="top start" auto-close>
                                      <q-color v-model="list.color" v-ripple clickable no-header no-footer class="my-picker" @change="saveBoardToStorage(false)" />
                                    </q-menu>
                                  </q-item>
                                  <q-separator />
                                  <q-item v-ripple clickable @click="showComingSoon()">
                                    <q-item-section avatar><q-icon name="o_sort" color="secondary" size="xs" /></q-item-section>
                                    <q-item-section class="">Sort By</q-item-section>
                                  </q-item>
                                  <q-separator />
                                  <q-item v-ripple v-close-popup clickable @click="removeList(swimLaneIndex, listIndex)">
                                    <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                                    <q-item-section class="text-negative">Delete List?</q-item-section>
                                  </q-item>
                                </q-list>
                              </q-menu>
                              <q-tooltip>View More</q-tooltip>
                            </q-btn>
                          </div>
                          <!-- List Name -->
                          <div class="flex items-center cursor-pointer">
                            <!-- Input Field -->
                            <div v-if="editingListName[list.id] === list.id" class="full-width min-width-200">
                              <q-input
                                v-model="list.name"
                                class="full-width min-width-200"
                                @change="handleListNameChange(list)"
                                @blur="editingListName[list.id] = null"
                                @keyup.enter="editingListName[list.id] = null"
                              />
                            </div>
                            <!-- Label -->
                            <div v-else class="flex items-center full-width fs-bold" @dblclick="editingListName[list.id] = list.id">
                              <q-icon v-if="list.color" name="o_circle" class="q-mr-xs" :style="`border-radius:50%; color:${list.color}; background-color:${list.color};`" size="xs" />
                              <div class="min-width-200 fs-18">
                                {{ list.name === '' ? 'Untitled' : list.name }}
                                <q-tooltip>Double Click To Edit</q-tooltip>
                              </div>
                            </div>
                          </div>
                        </div>
                        <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                        <!-- SwimLane -- List -- Body -->
                        <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                        <div class="row SubSwimlane-body">
                          <div class="col-12">
                            <table class="full-width table bordered">
                              <tbody :id="'sortable-tasks-' + list.id" :data-list-id="list.id">
                                <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                                <!-- SwimLane -- List -- Task -->
                                <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                                <tr
                                  v-for="(listTask, taskIndex) in list.projectSwimLanesListsTasks"
                                  :key="listTask.id"
                                  :class="listTask?.deleted ? 'hidden' : ''"
                                  draggable="true"
                                  :data-item="JSON.stringify(listTask.projectTask)"
                                >
                                  <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                                  <!-- SwimLane -- List -- Task -- View More -->
                                  <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                                  <td :class="getPriorityBorderClass(listTask.projectTask.priority?.dropDownValue, true)" style="width:20px;">
                                    <div class="TaskViewMore">
                                      <q-btn dense flat icon="o_more_vert" size="sm" color="primary" style="padding: 0px !important;min-height: auto;" @click.stop>
                                        <q-menu>
                                          <q-list>
                                            <q-item v-ripple clickable @click="onProjectTaskView(listTask.projectTask.id)">
                                              <q-item-section avatar><q-icon name="o_visibility" color="secondary" size="xs" /></q-item-section>
                                              <q-item-section class="">View</q-item-section>
                                            </q-item>
                                            <q-item v-ripple clickable @click="onProjectTaskEdit(projectId, listTask.projectTask.id)">
                                              <q-item-section avatar><q-icon name="o_edit" size="xs" /></q-item-section>
                                              <q-item-section>Edit</q-item-section>
                                            </q-item>
                                            <q-separator />
                                            <q-item v-ripple clickable>
                                              <q-item-section avatar><q-icon name="o_person_add" color="secondary" size="xs" /></q-item-section>
                                              <q-item-section class="">Change Module?</q-item-section>
                                              <q-menu anchor="top end" self="top start">
                                                <q-item v-ripple clickable class="flex column">
                                                  <div class="q-pa-xs">
                                                    <q-select
                                                      v-model="listTask.projectTask.projectModuleId"
                                                      use-input
                                                      outlined
                                                      stack-label
                                                      :dense="true"
                                                      :options="projectModuleList"
                                                      option-value="value"
                                                      option-label="text"
                                                      emit-value
                                                      map-options
                                                      @filter="getAllProjectModuleListForFilter"
                                                      @update:model-value="saveBoardToStorage(false)"
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
                                                    <q-btn v-close-popup outline icon="o_close" size="sm" color="secondary" label="Close" class="q-mt-xs" />
                                                  </div>
                                                </q-item>
                                              </q-menu>
                                            </q-item>
                                            <q-separator />
                                            <q-item v-ripple v-close-popup clickable @click="duplicateTask(listTask.projectTask.id)">
                                              <q-item-section avatar><q-icon name="o_folder_copy" color="secondary" size="xs" /></q-item-section>
                                              <q-item-section class="">Duplicate Task?</q-item-section>
                                            </q-item>
                                            <q-item v-ripple clickable @click="onCopyTask(listTask.projectTask.id, listTask.projectTask.name, listTask.projectTask.projectModuleId, 'isCopy')">
                                              <q-item-section avatar><q-icon name="o_copy" color="secondary" size="xs" /></q-item-section>
                                              <q-item-section class="">Copy Tasks To...</q-item-section>
                                            </q-item>
                                            <q-item v-ripple clickable @click="onMoveTask(listTask.projectTask.id, listTask.projectTask.name, listTask.projectTask.projectModuleId, 'isMove')">
                                              <q-item-section avatar><q-icon name="o_arrow_forward" color="secondary" size="xs" /></q-item-section>
                                              <q-item-section class="">Move Tasks To...</q-item-section>
                                            </q-item>
                                            <q-separator />
                                            <q-item v-ripple clickable>
                                              <q-item-section avatar><q-icon name="o_colorize" color="primary" size="xs" /></q-item-section>
                                              <q-item-section class="">Task Color</q-item-section>
                                              <q-menu anchor="top end" self="top start" auto-close>
                                                <q-color v-model="listTask.projectTask.color" v-ripple clickable no-header no-footer class="my-picker" @change="saveBoardToStorage(false)" />
                                              </q-menu>
                                            </q-item>
                                            <q-separator />
                                            <q-item v-ripple v-close-popup clickable @click="removeTask(swimLaneIndex, listIndex, taskIndex)">
                                              <q-item-section avatar><q-icon name="o_delete_outline" color="negative" size="xs" /></q-item-section>
                                              <q-item-section class="text-negative">Delete Task</q-item-section>
                                            </q-item>
                                          </q-list>
                                        </q-menu>
                                        <q-tooltip>View More</q-tooltip>
                                      </q-btn>
                                    </div>
                                  </td>
                                  <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                                  <!-- SwimLane -- List -- Task -- Task Name -->
                                  <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                                  <td style="width:40%;">
                                    <!-- Task Name Input -->
                                    <div v-if="editingTaskName[listTask.projectTask.id] === listTask.projectTask.id" class="min-width-200">
                                      <q-input
                                        v-model="listTask.projectTask.name"
                                        outlined stack-label hide-bottom-space autogrow
                                        class="min-width-200"
                                        :data-item="JSON.stringify(listTask.projectTask)"
                                        @change="handleTaskNameChange(listTask.projectTask)"
                                        @blur="editingTaskName[listTask.projectTask.id] = null"
                                        @keyup.enter="editingTaskName[listTask.projectTask.id] = null"
                                      />
                                    </div>
                                    <!-- Task Name Label -->
                                    <div v-else class="min-width-200" @dblclick="editingTaskName[listTask.projectTask.id] = listTask.projectTask.id">
                                      <q-icon v-if="listTask.projectTask.color" name="o_circle" class="q-mr-xs" :style="`border-radius:50%;color:${listTask.projectTask.color}; background-color:${listTask.projectTask.color};`" size="xs" />
                                      {{ listTask.projectTask.name === '' ? 'Untitled' : listTask.projectTask.name }}
                                    </div>
                                  </td>
                                  <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                                  <!-- SwimLane -- List -- Task -- Actions -->
                                  <!-- ----------------------------------------------------------------------------------------------------------------------------------------------------------- -->
                                  <!-- Task View -->
                                  <td class="text-center cursor-pointer" style="width:30px;">
                                    <a @click="onProjectTaskView(listTask.projectTask.id)">
                                      <q-icon name="o_visibility" color="grey-8" size="xs" />
                                      <q-tooltip>View</q-tooltip>
                                    </a>
                                  </td>
                                  <!-- Task Timer -->
                                  <td class="text-center cursor-pointer" style="width:30px;">
                                    <a
                                      @click="{
                                        getAllActivityFromTask(projectId, listTask.projectTask.projectModuleId, listTask.projectTask.id);
                                        selectedTimerTask.taskId = listTask.projectTask.id;
                                        selectedTimerTask.taskName = listTask.projectTask.name;
                                        selectedTimerTask.activityId = '';
                                        selectedTimerTask.activityName = '';
                                        showTaskDetailsForTimerModal = true;
                                      }"
                                    >
                                      <q-icon name="o_play_circle" color="grey-8" size="xs" />
                                      <q-tooltip>Start Timer?</q-tooltip>
                                    </a>
                                  </td>
                                  <!-- Task StartDate -->
                                  <td class="text-center cursor-pointer" style="width:100px;">
                                    <q-badge rounded color="grey">
                                      <q-icon name="o_calendar_month" color="white" size="xs" />
                                      {{ shortDate(listTask.projectTask.startDate) }}
                                    </q-badge>
                                  </td>
                                  <!-- Task Owner - Inout -->
                                  <td class="cursor-pointer" style="width:60px;">
                                    <div v-if="editingTaskOwner[listTask.projectTask.id] === listTask.projectTask.id">
                                      <q-select
                                        v-model="listTask.projectTask.assignedToId"
                                        use-input
                                        outlined
                                        stack-label
                                        :dense="true"
                                        :options="employeeList"
                                        option-value="value"
                                        option-label="text"
                                        emit-value
                                        map-options
                                        @filter="getAllActiveEmployeesListFilter"
                                        @update:model-value="saveBoardToStorage(false)"
                                        @popup-hide="editingTaskOwner[listTask.projectTask.id] = null"
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
                                    <!-- Task Owner - Label -->
                                    <div v-else class="flex items-center justify-center text-center" @dblclick="editingTaskOwner[listTask.projectTask.id] = listTask.projectTask.id">
                                      <q-icon name="o_person_add" color="grey-8" size="xs" class="q-mr-xs" />
                                      <q-badge v-if="listTask.projectTask.assignedTo?.person?.firstName" rounded>
                                        {{ listTask.projectTask?.assignedTo?.person?.firstName[0] + ' ' + listTask.projectTask?.assignedTo?.person?.lastName[0] }}
                                      </q-badge>
                                      <q-tooltip>Assign Owner?</q-tooltip>
                                    </div>
                                  </td>
                                  <!-- Task Assignment -->
                                  <td class="q-pl-xs cursor-pointer" style="min-width:100px; max-width:200px;">
                                    <div class="TaskActivity flex items-center">
                                      <!-- Task Assignment - Action Button -->
                                      <a class="q-mr-xs" @click="openTaskAssignment(listTask.projectTask.id, projectId, listTask.projectTask.projectModuleId, listTask.projectTask.name, projectData.name, listTask.projectTask.projectModule.name)">
                                        <q-icon name="o_group_add" color="grey-8" size="xs" />
                                        <q-tooltip>Manage Assignments?</q-tooltip>
                                      </a>
                                      <!-- Task Assignment - Labels -->
                                      <div v-for="activity in listTask.projectTask.projectActivities" :key="activity.id" class="Person">
                                        {{ (activity?.assignedTo?.person?.firstName?.trim()?.[0] || '') + (activity?.assignedTo?.person?.lastName?.trim()?.[0] || '') }}
                                        <q-tooltip>
                                          <div>
                                            <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                                            <span>{{ activity.assignedTo?.person?.firstName + ' ' + activity.assignedTo?.person?.lastName }}</span>
                                          </div>
                                          <div>
                                            <q-icon name="o_mail" color="white" size="xs" class="q-mr-xs" />
                                            <span>{{ activity.assignedTo?.person?.primaryEmailAddress }}</span>
                                          </div>
                                          <div v-if="activity.estimateHours > 0">
                                            <q-icon name="o_timer" color="white" size="xs" class="q-mr-xs" />
                                            <span>{{ activity.estimateHours }} hrs</span>
                                          </div>
                                        </q-tooltip>
                                      </div>
                                    </div>
                                  </td>
                                  <!-- Task Estimated Time -->
                                  <td class="cursor-pointer" style="width:100px;">
                                    <!-- Task Estimated Time - Inout -->
                                    <div v-if="editingTaskEstimatedTime[listTask.projectTask.id] === listTask.projectTask.id">
                                      <q-input
                                        v-model="listTask.projectTask.estimateTime"
                                        outlined
                                        stack-label
                                        hide-bottom-space
                                        :dense="true"
                                        :rules="[validateTaskEstimatedHours]"
                                        hint="hh.mm"
                                        maxlength="5"
                                        @change="saveBoardToStorage(false)"
                                        @blur="editingTaskEstimatedTime[listTask.projectTask.id] = null"
                                        @keyup.enter="editingTaskEstimatedTime[listTask.projectTask.id] = null"
                                      />
                                    </div>
                                    <!-- Task Estimated Time - Label -->
                                    <div v-else class="text-center" @dblclick="editingTaskEstimatedTime[listTask.projectTask.id] = listTask.projectTask.id">
                                      <q-icon name="o_schedule" color="grey-8" size="xs" />
                                      {{ listTask.projectTask.estimateTime }}
                                    </div>
                                  </td>
                                  <!-- Task Status -->
                                  <td class="cursor-pointer" style="width:100px;">
                                    <!-- Task Status - Select -->
                                    <div v-if="editingTaskStatus[listTask.projectTask.id] === listTask.projectTask.id">
                                      <q-select
                                        v-model="listTask.projectTask.statusId"
                                        use-input
                                        outlined
                                        stack-label
                                        :dense="true"
                                        :options="projectTaskStatusList"
                                        option-value="value"
                                        option-label="text"
                                        map-options
                                        @update:model-value="val => onTaskDropDownChange(listTask.projectTask, val.value, val.text, 'Task-Status')"
                                        @popup-hide="editingTaskStatus[listTask.projectTask.id] = null"
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
                                    <!-- Task Status - Label -->
                                    <div v-else class="text-center fs-12" @dblclick="editingTaskStatus[listTask.projectTask.id] = listTask.projectTask.id">
                                      <q-badge :style="{ backgroundColor: listTask.projectTask.status.bgColor || 'grey-8', color: listTask.projectTask.status.color || 'black-8' }" rounded>
                                        <q-icon name="o_flag" size="xs" />
                                        {{ listTask.projectTask.status.dropDownValue }}
                                      </q-badge>
                                    </div>
                                  </td>
                                  <!-- Task Priority -->
                                  <td class="cursor-pointer" style="width:100px;">
                                    <!-- Task Priority - Select -->
                                    <div v-if="editingTaskPriorityId[listTask.projectTask.id] === listTask.projectTask.id">
                                      <q-select
                                        v-model="listTask.projectTask.priorityId"
                                        use-input
                                        outlined
                                        stack-label
                                        :dense="true"
                                        :options="projectTaskPriorityList"
                                        option-value="value"
                                        option-label="text"
                                        map-options
                                        @update:model-value="val => onTaskDropDownChange(listTask.projectTask, val.value, val.text, 'Task-Priority')"
                                        @popup-hide="editingTaskPriorityId[listTask.projectTask.id] = null"
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
                                    <!-- Task Priority - Label -->
                                    <div v-else class="text-center fs-12" @dblclick="editingTaskPriorityId[listTask.projectTask.id] = listTask.projectTask.id">
                                      <q-badge :style="{ backgroundColor: listTask.projectTask.priority.bgColor || 'grey-8', color: listTask.projectTask.priority.color || 'black-8' }" rounded>
                                        <q-icon name="o_priority_high" size="xs" />
                                        {{ listTask.projectTask.priority.dropDownValue }}
                                      </q-badge>
                                    </div>
                                  </td>
                                  <!-- Task Tags -->
                                  <td
                                    class="text-left hoverable-cell"
                                    style="width: 250px; white-space: normal; overflow-wrap: break-word;"
                                    @click="activeEdit = { rowId: listTask.projectTask.id, field: 'tags' }"
                                  >
                                    <TagEditor
                                      v-if="activeEdit.rowId === listTask.projectTask.id && activeEdit.field === 'tags'"
                                      v-model="listTask.projectTask.taskTags"
                                      :row-id="listTask.projectTask.id"
                                      :available-tags="availableTags"
                                      @save="({ tags, rowId }) => saveTagsData(tags, rowId)"
                                      @filter="filterTags"
                                    />

                                    <template v-else>
                                      <div v-if="listTask.projectTask.taskTags?.length">
                                        <q-chip
                                          v-for="(tag, i) in listTask.projectTask.taskTags"
                                          :key="i"
                                          dense
                                          :style="{
                                            backgroundColor: tag.bgColor,
                                            color: tag.color,
                                            padding: '4px 8px',
                                            maxWidth: '100%',
                                            wordBreak: 'break-word'
                                          }"
                                        >
                                          {{ tag.text }}
                                        </q-chip>
                                      </div>
                                      <div v-else>
                                        <q-badge color="red-4" square outline class="justify-center">No Tags</q-badge>
                                      </div>
                                    </template>

                                    <q-tooltip>Click To Edit Tags</q-tooltip>
                                  </td>
                                </tr>
                              </tbody>
                            </table>
                          </div>
                          <div class="col-xxl-1 col-md-2 flex" style="min-height: 48px;">
                            <!-- Add Project Task -->
                            <q-btn v-show="projectListHovered === list.id" color="primary" icon="o_add" outline no-caps label="Add Task" class="full-width q-mt-sm" style="height: 30px; min-height: 30px;" @click="addTask(swimLaneIndex, listIndex)" />
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </q-slide-transition>
          </div>
        </div>
      </q-card-section>
    </q-card>
  </q-page>
  <!-- SwimLane Type Modal Dialog -->
  <q-dialog v-model="showUnderDevelopmentDialog" persistent>
    <q-card style="width: 25%;">
      <q-card-section>
        <div class="text-h3 text-primary">Workboard Beta Version - Under Development</div>
      </q-card-section>
      <q-card-section class="q-py-sm">
        <p>
          This feature is currently under <b>Development and Testing</b>.<br>
          Some actions may not work as expected.<br>
          We’ll notify you once it’s fully available.
        </p>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="Okay" color="secondary" @click="showUnderDevelopmentDialog = false" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <!-- SwimLane Type Modal Dialog -->
  <q-dialog v-model="showSwimLaneTypeModal" persistent>
    <q-card style="width: 25%;">
      <q-card-section>
        <div class="text-h3">Add Swimlane</div>
      </q-card-section>
      <q-card-section class="q-py-sm">
        <q-select
          v-model="selectedTypeId"
          label="Swimlane Type*"
          use-input
          outlined
          stack-label
          :dense="true"
          :options="projectSwimlaneTypeList"
          option-value="value"
          option-label="text"
          map-options
          :option-disable="opt => opt.disable"
        >
          <template #option="{ itemProps, opt }">
            <q-item v-bind="itemProps">
              <q-item-section>
                <div class="row q-col-gutter-x-md items-center">
                  <span>{{ opt.text }}</span>
                  <q-icon
                    v-if="opt.disable"
                    name="o_info"
                    size="12px"
                    color="grey"
                    class="q-ml-xs"
                  >
                    <q-tooltip>
                      Only One In A Project Allowed
                    </q-tooltip>
                  </q-icon>
                </div>
              </q-item-section>
            </q-item>
          </template>
        </q-select>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn v-close-popup flat label="Cancel" color="primary" />
        <q-btn flat label="Add" color="primary" :disable="!selectedTypeId" @click="confirmAddSwimLane" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <q-dialog v-model="showTaskDetailsForTimerModal" persistent>
    <q-card style="width: 25%;">
      <q-card-section>
        <div class="text-h3">Select Activity To Start Timer?</div>
      </q-card-section>
      <q-card-section class="q-py-sm">
        <q-select
          v-model="selectedActivity"
          label="Select Activity*"
          use-input
          outlined
          stack-label
          :dense="true"
          :options="projectTaskActivityList"
          option-value="value"
          option-label="text"
          map-options
          @filter="getAllActivityFromTaskFilter"
          @update:model-value="val => { selectedTimerTask.activityId = val.value; selectedTimerTask.activityName = val.text;}"
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
      </q-card-section>
      <q-card-actions align="right">
        <q-btn v-close-popup flat label="Cancel" color="primary" />
        <q-btn flat label="Add" color="primary" :disable="!selectedTimerTask.activityId" @click="startNewTask(selectedTimerTask), showTaskDetailsForTimerModal = false" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted, nextTick, inject } from "vue";
import { useQuasar, uid, date } from "quasar";
import { notifySuccess, setLocalStorage, getLocalStorage, notifyError } from "assets/utils";
import { Sortable } from "sortablejs";
// import "vue-select/dist/vue-select.css";
import { format } from "date-fns"; // Standard TimeZone Conversion

import commonService from "services/common.service";
import employeesService from "src/modules/employee/employee.service";
import projectService from "modules/project/projects.service";
import projectTaskService from "modules/project-tasks/projectTasks.service";

import projectView from "modules/project/components/view.vue";
import projectChatBox from "modules/project/components/_projectChat.vue";
import projectFiles from "modules/project/components/_uploadFilesToProject.vue";
import projectNotes from "modules/common/components/addNote.vue";

import projectModuleService from "modules/project-modules/projectModules.service";
import viewProjectTask from "modules/project-tasks/components/view.vue";
import copyProjectTask from "modules/project-tasks/components/_copyTaskToProject.vue";
import taskAssignment from "modules/project-tasks-activities/components/addEditBulk.vue";
import projectActivitiesService from "modules/project-tasks-activities/projectTasksActivities.service";
import TagEditor from "src/modules/project-tasks/components/_taskTagEditor.vue";
import editProjectTask from "modules/project-tasks/components/addEdit.vue";

const startNewTask = inject("startNewTask");
const activeEdit = ref({ rowId: null, field: null });

const $q = useQuasar();

const projectId = history.state?.projectId;
const viewAsWorkBoardOrList = ref(true); // false -> WorkBoard & True -> WorkBoard As List View.

// SwimLane Variables
const projectData = ref([]);
const editingSwimlaneName = ref([]);
const showSwimLaneTypeModal = ref(false);
const projectSwimlaneTypeList = ref([]);
const selectedTypeId = ref("");
const defaultSwimlaneTypeName = "Task Status As Lists";

const showUnderDevelopmentDialog = ref(true);

// List Variables
const projectListHovered = ref(null);
// const defaultListStatusId = ref(null);
const editingListName = ref([]);

// Task Variables
const defaultTaskStatusId = ref(null);
const defaultTaskStatus = "New";
const defaultTaskPriorityId = ref(null);
const defaultTaskPriority = "Medium";

const editingTaskName = ref([]);
const editingTaskOwner = ref([]);
const editingTaskEstimatedTime = ref([]);
const editingTaskStatus = ref([]);
const editingTaskPriorityId = ref([]);
const showTaskDetailsForTimerModal = ref(false);
const selectedActivity = ref(null);
const selectedTimerTask = ref({
  taskId: "",
  taskName: "",
  activityId: "",
  activityName: ""
});

// ----------------------------------------------------------------------------------------------------
// Common--> Projects
// ----------------------------------------------------------------------------------------------------

const showComingSoon = () => {
  notifySuccess({ message: "Coming Soon" });
};

// Load saved board from localStorage
const getWorkBoardByProjectId = (projectId, loading, collapsed) => {
  // Start Loading
  if (loading) $q.loading.show();
  projectService.getWorkBoardByProjectId(projectId).then((resp) => {
    projectData.value = resp;
    projectData.value.projectSwimLanes?.forEach(lane => {
      lane.projectSwimLanesList?.forEach(module => {
        module.projectSwimLanesListsTasks?.forEach(listTask => {
          listTask.projectTask.taskTags = listTask.projectTask.projectTask_Tags?.map(tagItem => ({
            text: tagItem.tags.name,
            value: tagItem.tags.id,
            bgColor: tagItem.tags.bgColor || "#e0e0e0",
            color: tagItem.tags.color || "#191919"
          })) || [];
        });
      });
    });

    // Collapse Swimlanes On Project Data
    if (getCollapsedSwimlanes(projectId).length === 0) {
      collapsedSwimlane.value = projectData.value.projectSwimLanes?.map(lane => lane.id);
      setCollapsedSwimlanes(projectId, collapsedSwimlane.value);
    } else {
      collapsedSwimlane.value = getCollapsedSwimlanes(projectId);
    }

    // Get Swimlane Type
    getProjectSwimlaneTypeDropDown("Project Swimlane Types");

    // Re-initialize Sortable Features
    nextTick(makeSortable);
  }).finally(() => {
    // Stop Loading
    if (loading) $q.loading.hide();
  });
};

// // Get Project List Default Status Id
// const getDefaultListStatusId = () => {
//   commonService.getDropDownByTypeNameAndName("WO Status", "New").then((resp) => {
//     defaultListStatusId.value = resp;
//   }).finally(() => {
//   });
// };

// Get Project Task Default Status Id
const getDefaultTaskStatusId = () => {
  commonService.getDropDownByTypeNameAndName("Task Status", defaultTaskStatus).then((resp) => {
    defaultTaskStatusId.value = resp;
  }).finally(() => {
  });
};

// Get Project Task Priority Id
const getDefaultTaskPriorityId = () => {
  commonService.getDropDownByTypeNameAndName("Task Priorities", defaultTaskPriority).then((resp) => {
    defaultTaskPriorityId.value = resp;
  }).finally(() => {
  });
};

// ----------------------------------------------------------------------------------------------------
// WorkBoard--> Collapse
// ----------------------------------------------------------------------------------------------------

const localStorageKey = "ProjectWorkBoard";
const collapsedSwimlane = ref([]);

// Get from local storage
function getCollapsedSwimlanes (projectId) {
  const boards = getLocalStorage(localStorageKey);
  return boards?.find(b => b.projectId === projectId)?.swimlaneIds || [];
}

// Set projectId and SwimlaneId to local storage
function setCollapsedSwimlanes (projectId, swimlaneIds) {
  let boards = getLocalStorage(localStorageKey) || [];
  if (!Array.isArray(boards)) boards = [];

  const index = boards.findIndex(b => b.projectId === projectId);
  if (index !== -1) {
    boards[index].swimlaneIds = swimlaneIds;
  } else {
    boards.push({ projectId, swimlaneIds });
  }
  // Set values to local storage
  setLocalStorage(localStorageKey, boards);
}

// Toggle Function
function toggleCollapse (projectId, laneId) {
  const current = [...getCollapsedSwimlanes(projectId)];

  const updated = current.includes(laneId)
    ? current.filter(id => id !== laneId)
    : [...current, laneId];

  if (updated.length === 0) updated.push(uid().toString());

  collapsedSwimlane.value = updated;
  setCollapsedSwimlanes(projectId, updated);
}

// ----------------------------------------------------------------------------------------------------
// WorkBoard--> Draggable & Sort Order & Save Data
// ----------------------------------------------------------------------------------------------------
const sortableInstances = ref([]);
const makeSortable = () => {
  nextTick(() => {
    sortableInstances.value.forEach(instance => instance.destroy());
    sortableInstances.value = [];

    const board = document.getElementById("sortable-swimlanes");
    if (board) {
      sortableInstances.value.push(
        new Sortable(board, {
          animation: 150,
          ghostClass: "sortable-ghost",
          onEnd (event) {
            const movedLane = projectData.value.projectSwimLanes.splice(event.oldIndex, 1)[0];
            projectData.value.projectSwimLanes.splice(event.newIndex, 0, movedLane);
            updateSwimlaneSortOrder();
          }
        })
      );
    }

    projectData.value.projectSwimLanes?.forEach((swimlane, swimLaneIndex) => {
      const swimLaneContainer = document.getElementById(`Sortable-Sublanes-${swimlane.id}`);
      if (swimLaneContainer) {
        sortableInstances.value.push(
          new Sortable(swimLaneContainer, {
            group: "lists",
            animation: 150,
            ghostClass: "sortable-ghost",
            handle: ".SubSwimlane-drag-handle",
            onAdd (event) {
              const newList = JSON.parse(event.item.dataset.item);
              const fromListIndex = projectData.value.projectSwimLanes.findIndex(sw => sw.projectSwimLanesList?.some(l => l.id === newList.id));
              const fromList = projectData.value.projectSwimLanes[fromListIndex];
              const listIndex = fromList.projectSwimLanesList.findIndex(l => l.id === newList.id);
              if (listIndex > -1) fromList.projectSwimLanesList.splice(listIndex, 1);

              // Add key if not present
              if (!Object.prototype.hasOwnProperty.call(projectData.value.projectSwimLanes[swimLaneIndex], "projectSwimLanesList")) {
                projectData.value.projectSwimLanes[swimLaneIndex].projectSwimLanesList = [];
              }
              projectData.value.projectSwimLanes[swimLaneIndex].projectSwimLanesList.splice(event.newIndex, 0, newList);
              updateSwimlaneSortOrder();
            },
            onEnd (event) {
              const movedSubLane = projectData.value.projectSwimLanes[swimLaneIndex].projectSwimLanesList.splice(event.oldIndex, 1)[0];
              projectData.value.projectSwimLanes[swimLaneIndex].projectSwimLanesList.splice(event.newIndex, 0, movedSubLane);
              updateSwimlaneSortOrder();
            }
          })
        );
      }

      swimlane.projectSwimLanesList?.forEach((list, listIndex) => {
        const el = document.getElementById(`sortable-tasks-${list.id}`);
        if (el) {
          sortableInstances.value.push(
            new Sortable(el, {
              group: "tasks",
              animation: 150,
              ghostClass: "sortable-ghost",
              fallbackOnBody: true,
              emptyInsertThreshold: 5,
              onAdd (event) {
                const newTask = JSON.parse(event.item.dataset.item);

                const fromLane = projectData.value.projectSwimLanes.find(sw => sw.projectSwimLanesList?.some(l => l.projectSwimLanesListsTasks?.some(t => t.id === newTask.id)));
                const fromList = fromLane?.projectSwimLanesList.find(l => l.projectSwimLanesListsTasks?.some(t => t.id === newTask.id));
                const taskIndex = fromList?.projectSwimLanesListsTasks.findIndex(t => t.id === newTask.id);

                if (taskIndex > -1) {
                  const [movedTask] = fromList.projectSwimLanesListsTasks.splice(taskIndex, 1);
                  // Ensure destination has projectSwimLanesListsTasks initialized
                  if (!list.projectSwimLanesListsTasks) list.projectSwimLanesListsTasks = [];
                  list.projectSwimLanesListsTasks.splice(event.newIndex, 0, movedTask);
                }
                updateSwimlaneSortOrder();
              },
              onEnd (event) {
                if (event.from === event.to) {
                  const movedTask = list.projectSwimLanesListsTasks.splice(event.oldIndex, 1)[0];
                  list.projectSwimLanesListsTasks.splice(event.newIndex, 0, movedTask);
                  updateSwimlaneSortOrder();
                }
              }
            })
          );
        }
      });
    });
  });
};

// Update SortOrder to Swimlane array
const updateSwimlaneSortOrder = () => {
  projectData.value.projectSwimLanes?.forEach((lane, laneIndex) => {
    if (lane !== undefined) {
      lane.sortOrder = laneIndex + 1;
      lane.projectSwimLanesList?.forEach((list, listIndex) => {
        if (list !== undefined) {
          list.sortOrder = listIndex + 1;
          list.projectSwimLanesListsTasks?.forEach((listTask, taskIndex) => {
            if (listTask !== undefined) {
              listTask.sortOrder = taskIndex + 1;
            }
          });
        }
      });
    }
  });
  saveBoardToStorage(false);
};

// Save the board state to localStorage
const saveBoardToStorage = (refresh) => {
  projectService.saveWorkboard(projectData.value).then((resp) => {
  }).finally(() => {
    if (refresh) getWorkBoardByProjectId(projectId, refresh, false);
  });
};

// ----------------------------------------------------------------------------------------------------
// Common--> Project SwimLanes
// ----------------------------------------------------------------------------------------------------
// Handle Swimlane Name Change
const handleLaneNameChange = (lane) => {
  if (!lane.name || lane.name.trim() === "") {
    lane.name = "Untitled";
  }
  saveBoardToStorage(false);
};

// Triggered when "Add" is confirmed in modal
const confirmAddSwimLane = () => {
  const selectedId = selectedTypeId.value.value;
  const selectedText = selectedTypeId.value.text;
  const selectedType = projectSwimlaneTypeList.value.find(opt => opt.value === selectedId);
  if (!selectedType) return;
  addSwimLane(selectedId, selectedText);
  showSwimLaneTypeModal.value = false;
};

// Add Swim Lane
const addSwimLane = (selectedId, selectedText) => {
  if (selectedTypeId.value) {
    const model = {
      id: uid(),
      projectId,
      swimlaneTypeId: selectedId,
      name: "Untitled",
      color: "",
      sortOrder: projectData.value.projectSwimLanes.length + 1,
      deleted: false,
      swimlaneType: {
        id: selectedId,
        dropDownValue: selectedText
      },
      projectSwimLanesList: []
    };
    projectService.addProjectSwimlane(model).then((resp) => {
      projectData.value.projectSwimLanes.push(resp);
      nextTick(makeSortable);
    });
  } else {
    notifyError({ message: "Please Select Swimlane Type" });
  }
};

// Delete Swim Lane (mark as deleted)
const removeSwimLane = (index) => {
  $q.dialog({
    title: "Delete Swimlane?",
    message: "Are you sure you want to delete Swimlane?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    $q.loading.show();
    projectData.value.projectSwimLanes[index].deleted = true;
    saveBoardToStorage(true);
    $q.loading.hide();
  }).onCancel(() => {
    // notifyError({ message: "Cancelled" });
  });
};

// // Duplicate Swimlane
// const duplicateSwimLane = (projectId, projectSwimLaneId) => {
//   $q.dialog({
//     title: "Duplicate Swimlane?",
//     message: "Are you sure you want to duplicate this Swimlane?",
//     cancel: true,
//     persistent: true
//   }).onOk(() => {
//     $q.loading.show();
//     projectService.duplicateSwimLane(projectId, projectSwimLaneId).then((resp) => {
//     }).finally(() => {
//       notifySuccess({ message: "SwimLane Duplicated" });
//       getWorkBoardByProjectId(projectId, true, false);
//       setTimeout(() => {
//         const elements = document.querySelectorAll(".swimlane");
//         const lastElement = elements[elements.length - 1];
//         lastElement?.scrollIntoView({ behavior: "smooth", block: "start" });
//       }, 1500);
//     });
//   }).onCancel(() => {
//     // notifyError({ message: "Cancelled" });
//   });
// };

// List Status
function getProjectSwimlaneTypeDropDown (typeName) {
  const isAlreadyAdded = hasTaskStatusAsLists(projectData);
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({
      text: item.dropdownValue,
      value: item.id,
      disable: item.dropdownValue === defaultSwimlaneTypeName && isAlreadyAdded
    }));
    projectSwimlaneTypeList.value = responseData;
  });
}

const hasTaskStatusAsLists = (project) => {
  return project.value.projectSwimLanes?.some((lane) => lane.swimlaneType?.dropDownValue === defaultSwimlaneTypeName);
};

// ----------------------------------------------------------------------------------------------------
// Common--> Project Lists
// ----------------------------------------------------------------------------------------------------
// Handle List Name Change
const handleListNameChange = (list) => {
  if (!list.name || list.name.trim() === "") {
    list.name = "Untitled";
  }
  saveBoardToStorage(false);
};

// const showTagDialog = ref({}); // To control popup per row
// const openTagDialog = (rowId) => {
//   showTagDialog.value[rowId] = true;
// };

// List:- Add
const addList = (swimLaneIndex) => {
  const swimLane = projectData.value.projectSwimLanes[swimLaneIndex];
  if (!Array.isArray(swimLane.projectSwimLanesList)) {
    swimLane.projectSwimLanesList = [];
  }
  swimLane.projectSwimLanesList.push({
    id: uid(),
    projectId,
    projectSwimLaneId: projectData.value.projectSwimLanes[swimLaneIndex].id,
    projectModuleNumber: 0,
    name: "Untitled",
    color: "",
    sortOrder: projectData.value.projectSwimLanes[swimLaneIndex].projectSwimLanesList.length + 1,
    deleted: false,
    projectSwimLanesListsTasks: []
  });
  nextTick(makeSortable);
  saveBoardToStorage(true);
};

// List:- Remove (mark as deleted)
const removeList = (swimLaneIndex, listIndex) => {
  $q.dialog({
    title: "Delete List?",
    message: "Are you sure you want to delete List?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    $q.loading.show();
    const listData = projectData.value.projectSwimLanes[swimLaneIndex].projectSwimLanesList[listIndex];
    listData.deleted = true;
    listData.projectSwimLanesListsTasks.forEach((task) => {
      task.deleted = true;
    });
    saveBoardToStorage(true);
    $q.loading.hide();
  }).onCancel(() => {
    // notifyError({ message: "Cancelled" });
  });
};

// Duplicate list
// const duplicateList = (projectId, projectSwimLaneId, listId) => {
//   $q.dialog({
//     title: "Duplicate List?",
//     message: "Are you sure you want to duplicate this List?",
//     cancel: true,
//     persistent: true
//   }).onOk(() => {
//     $q.loading.show();
//     projectService.duplicateList(projectId, projectSwimLaneId, listId).then((resp) => {
//     }).finally(() => {
//       notifySuccess({ message: "List Duplicated" });
//       getWorkBoardByProjectId(projectId, true, false);
//     });
//   }).onCancel(() => {
//     // notifyError({ message: "Cancelled" });
//   });
// };

// List Status
// const projectListStatusList = ref([]);
// function getProjectListStatusDropDownList (typeName) {
//   commonService.getDropDown(typeName).then((resp) => {
//     const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
//     projectListStatusList.value = responseData;
//   });
// }

// const onListDropDownChange = (data, id, text) => {
//   // Add key if not present
//   if (!Object.prototype.hasOwnProperty.call(data, "projectModuleStatusId")) {
//     data.projectModuleStatusId = "";
//   }
//   // Add Array if not present
//   if (!Array.isArray(data.projectModuleStatus)) {
//     data.projectModuleStatus = [];
//   }
//   // Assign Status
//   data.projectModuleStatusId = id;
//   data.projectModuleStatus = { id, dropDownValue: text };
//   saveBoardToStorage(false);
// };

// ----------------------------------------------------------------------------------------------------
// Common--> Project Tasks
// ----------------------------------------------------------------------------------------------------
// Handle Task Name Change
const handleTaskNameChange = (task) => {
  if (!task.name || task.name.trim() === "") {
    task.name = "Untitled";
  }
  saveBoardToStorage(false);
};

// Tasks- Add Task
const addTask = (swimLaneIndex, listIndex) => {
  const taskId = uid();
  const SwimlaneType = projectData.value.projectSwimLanes[swimLaneIndex]?.swimlaneType?.dropDownValue === defaultSwimlaneTypeName;
  const list = projectData.value.projectSwimLanes[swimLaneIndex].projectSwimLanesList[listIndex];
  const taskStatusId = projectTaskStatusList.value.find(x => x.text === list.name);

  if (!Array.isArray(list.projectSwimLanesListsTasks)) {
    list.projectSwimLanesListsTasks = [];
  }
  list.projectSwimLanesListsTasks.push({
    id: uid(),
    projectSwimlaneListId: projectData.value.projectSwimLanes[swimLaneIndex].projectSwimLanesList[listIndex].id,
    projectTaskId: taskId.value,
    projectTask: {
      id: taskId.value,
      projectId,
      statusId: SwimlaneType ? taskStatusId.value : defaultTaskStatusId,
      status: { id: SwimlaneType ? taskStatusId.value : defaultTaskStatusId, dropDownValue: SwimlaneType ? list.name : defaultTaskStatus },
      priorityId: defaultTaskPriorityId.value,
      priority: { id: defaultTaskPriorityId.value, dropDownValue: defaultTaskPriority },
      assignedToId: "",
      projectTaskNumber: 0,
      name: "Untitled",
      estimateTime: 0,
      projectTask_Tags: [],
      startDate: format(new Date(), "MM/dd/yyyy"),
      color: ""
    },
    deleted: false,
    sortOrder: projectData.value.projectSwimLanes[swimLaneIndex].projectSwimLanesList[listIndex].projectSwimLanesListsTasks?.length + 1
  });
};

// Task- Remove task (mark as deleted)
const removeTask = (swimLaneIndex, listIndex, taskIndex, taskId) => {
  $q.dialog({
    title: "Delete Task?",
    message: "Are you sure you want to delete task?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    $q.loading.show();
    const taskData = projectData.value.projectSwimLanes[swimLaneIndex].projectSwimLanesList[listIndex].projectSwimLanesListsTasks[taskIndex];
    taskData.deleted = true;
    saveBoardToStorage(true);
    $q.loading.hide();
  }).onCancel(() => {
    // notifyError({ message: "Cancelled" });
  });
};

// Duplicate list
const duplicateTask = (taskId) => {
  $q.dialog({
    title: "Duplicate Task?",
    message: "Are you sure you want to duplicate this task?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    $q.loading.show();
    projectService.duplicateTask(taskId).then((resp) => {
    }).finally(() => {
      notifySuccess({ message: "Task Duplicated" });
      getWorkBoardByProjectId(projectId, true, false);
    });
  }).onCancel(() => {
    // notifyError({ message: "Cancelled" });
  });
};

// Task- Get Start Date
const shortDate = (rawDate) => {
  return date.formatDate(rawDate, "MMM D, YY"); // "March 23, 2025"
};

// Task- Get Priority Border-color
const getPriorityBorderClass = (tag, right) => {
  if (right) {
    return {
      "border-left-grey": tag === "Low",
      "border-left-orange": tag === "Medium",
      "border-left-red": tag === "High"
    };
  } else {
    return {
      "border-top-grey": tag === "Low",
      "border-top-orange": tag === "Medium",
      "border-top-red": tag === "High"
    };
  }
};

// Task- Get Priority Badge Color
const getPriorityBadgeClass = (priority) => {
  switch (priority) {
  case "Low":
    return "grey";
  case "Medium":
    return "orange";
  case "High":
    return "red";
  default:
    return "secondary";
  }
};

// Task - Copy
const onCopyTask = (id, name, projectModuleId, isCopy) => {
  $q.dialog({
    component: copyProjectTask,
    componentProps: { id, name, projectModuleId, isCopy }
  }).onOk(() => {
    getWorkBoardByProjectId(projectId, true, false);
  }).onCancel(() => {
  });
};

// Task - Move
const onMoveTask = (id, name, projectModuleId, isMove) => {
  $q.dialog({
    component: copyProjectTask,
    componentProps: { id, name, projectModuleId, isMove }
  }).onOk(() => {
    getWorkBoardByProjectId(projectId, true, false);
  }).onCancel(() => {
  });
};

// Task - Get Project Task Status List
const projectTaskStatusList = ref([]);
function getDropdownForTaskStatus (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    projectTaskStatusList.value = responseData;
  });
}

// Task - Get Project Priority List
const projectTaskPriorityList = ref([]);
function getDropDownForTaskPriorities (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    projectTaskPriorityList.value = responseData;
  });
}

// Task - Dropdown Change Event
const onTaskDropDownChange = (data, id, text, type) => {
  if (type === "Task-Status") {
    // Add key if not present
    if (!Object.prototype.hasOwnProperty.call(data, "statusId")) {
      data.statusId = "";
    }
    // Add Array if not present
    if (!Array.isArray(data.status)) {
      data.status = [];
    }

    data.statusId = id;
    data.status = { id, dropDownValue: text };
  } else if (type === "Task-Priority") {
    // Add key if not present
    if (!Object.prototype.hasOwnProperty.call(data, "priorityId")) {
      data.priorityId = "";
    }
    // Add Array if not present
    if (!Array.isArray(data.priority)) {
      data.priority = [];
    }

    data.priorityId = id;
    data.priority = { id, dropDownValue: text };
  }
  saveBoardToStorage(false);
};

// Task - Get All Active Employee List
const employeeList = ref([]);
const employeeListFilter = ref([]);
function getAllActiveEmployeesList () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id }));
    employeeList.value = responseData;
    employeeListFilter.value = responseData;
  });
}

function getAllActiveEmployeesListFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = employeeListFilter.value;
    } else {
      employeeList.value = employeeListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Task - Change Module Action
const projectModuleList = ref([]);
const projectModuleFilter = ref([]);

function getAllProjectModuleListForDropdown () {
  projectModuleService.getAllProjectModuleListForDropdown(false, false, projectId).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    projectModuleList.value = responseData;
    projectModuleFilter.value = responseData;
  });
}

// Search project module for dropdown
function getAllProjectModuleListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectModuleList.value = projectModuleFilter.value;
    } else {
      projectModuleList.value = projectModuleFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const projectTaskActivityList = ref([]);
const projectTaskActivityFilter = ref([]);

function getAllActivityFromTask (projectId, moduleId, taskId) {
  projectActivitiesService.getProjectTaskActivityListForDropdown(projectId, moduleId, taskId).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.name, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    projectTaskActivityList.value = responseData;
    projectTaskActivityFilter.value = responseData;
  });
}

// Search project activity for dropdown
function getAllActivityFromTaskFilter (val, update, abort, counter) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectTaskActivityList.value = projectTaskActivityFilter.value;
    } else {
      projectTaskActivityList.value = projectTaskActivityFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// ----------------------------------------------------------------------------------------------------
// Common--> Project Task Tags (VERSION 2 - List)
// ----------------------------------------------------------------------------------------------------

const availableTags = ref([]); // Existing categories
const availableTagsFilter = ref([]);
const tagValues = ref({}); // Stores selected tags per row
const model = ref({ taskId: "", tagsNameList: [], color: "" });

// Fetch available tags
function getAllTagsForDropdown () {
  projectTaskService.getAllTagsListForDropdown().then((resp) => {
    const responseData = resp.filter(item => item && item.name).map((item) => ({ text: item.name, value: item.id, bgColor: item.bgColor || "primary", color: item.color || "#191919" })).sort((a, b) => a.text.localeCompare(b.text));
    availableTags.value = responseData;
    availableTagsFilter.value = responseData;
  });
}

function filterTags (val, update, abort) {
  // inputValue.value = val;
  const needle = val?.toLowerCase() || "";

  const filtered = needle ? availableTagsFilter.value.filter(v => v.text.toLowerCase().includes(needle)) : [...availableTagsFilter.value];
  if (typeof update === "function") {
    update(() => {
      availableTags.value = filtered;
    });
  } else {
    availableTags.value = filtered;
  }
}

const saveTagsData = async (names, rowId) => {
  const tagInput = names;
  model.value.taskId = rowId;
  model.value.flag = null;
  model.value.tagsNameList = tagInput.map(tag => tag.text);

  try {
    await projectTaskService.saveTags(model.value);
    // notifySuccess({ message: "Tag saved successfully." });
    getAllTagsForDropdown();
    getWorkBoardByProjectId(projectId, false, true);
    tagValues.value[rowId] = [];
  } finally {
    // processing.value = false;
  }
};

// ----------------------------------------------------------------------------------------------------
// Task - Estimated Time Validations
// ----------------------------------------------------------------------------------------------------

// Task - Estimated Hours
function validateTaskEstimatedHours (value) {
  const regex = /^(\d+(\.\d{1,2})?)?$/;
  if (!value || (regex.test(value) && value.length <= 5)) {
    return true; // Valid input
  }
  return "Invalid activity hours format.";
}

// ----------------------------------------------------------------------------------------------------
// WorkBoard--> Swinlane List Scroll
// ----------------------------------------------------------------------------------------------------

const scrollSwimlane = (laneId, direction) => {
  const container = document.getElementById(`Sortable-Sublanes-${laneId}`);
  if (container) {
    const scrollAmount = document.getElementById(`sublanes-${laneId}`)?.offsetWidth || 400; // Adjust scroll amount as needed
    if (direction === "left") {
      container.scrollBy({ left: -scrollAmount, behavior: "smooth" });
    } else if (direction === "right") {
      container.scrollBy({ left: scrollAmount, behavior: "smooth" });
    }
  }
};

// ----------------------------------------------------------------------------------------------------
// Common--> Components
// ----------------------------------------------------------------------------------------------------

// Project View
const openProjectView = (id) => {
  $q.dialog({ component: projectView, componentProps: { id } });
};

// Project ChatBox
const openProjectChatBox = (id) => {
  $q.dialog({ component: projectChatBox, componentProps: { id } });
};

// Project Files
const openProjectFiles = (id) => {
  $q.dialog({ component: projectFiles, componentProps: { id } });
};

// Project Notes
const openProjectNotes = (id, type, moduleId, module, name) => {
  $q.dialog({ component: projectNotes, componentProps: { id, type, moduleId, module, name } });
};

// // Project List View
// const openListView = (id) => {
//   $q.dialog({ component: viewProjectModule, componentProps: { id } });
// };

// Project Task View
const onProjectTaskView = (id) => {
  $q.dialog({ component: viewProjectTask, componentProps: { id } });
};

// Edit popup
const onProjectTaskEdit = (projectId, taskId) => {
  $q.dialog({
    component: editProjectTask,
    componentProps: { id: taskId }
  }).onOk(() => {
    getWorkBoardByProjectId(projectId, true, false);
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};

// Project Task Assignments
const openTaskAssignment = (taskId, projectId, moduleId, taskName, projectName, moduleName) => {
  $q.dialog({
    component: taskAssignment,
    componentProps: { id: taskId, projectIdAttr: projectId, moduleIdAttr: moduleId, taskIdAttr: taskId, projectName, moduleName, taskName }
  }).onOk(() => {
    getWorkBoardByProjectId(projectId, true, false);
  });
};

// ----------------------------------------------------------------------------------------------------
// Common--> On Load
// ----------------------------------------------------------------------------------------------------

onMounted(() => {
  // Get Project Data
  getWorkBoardByProjectId(projectId, true, true);
  // Get All Active Employees
  getAllActiveEmployeesList();
  // Get Project List Status
  // getDefaultListStatusId();
  // getProjectListStatusDropDownList("WO Status");
  // Get Project Task Status
  getDefaultTaskStatusId();
  getDropdownForTaskStatus("Task Status");
  // Get Project Priority Status
  getDefaultTaskPriorityId();
  getDropDownForTaskPriorities("Task Priorities");
  // Get All Task Tags
  getAllTagsForDropdown();
  // Get Project Modules
  getAllProjectModuleListForDropdown();
});

</script>
<style scoped>
.TaskActivity{
  padding-right: 10px;
}
.task-box:hover .Person{
  margin-right: 3px;
}
.TaskActivity .Person {
    border-radius: 50%;
    background-color: #5d5d5d;
    color: white;
    font-size: 12px;
    font-weight: 600;
    padding: 2px 3px;
    margin-right: -6px;
    transition: 0.5s all ease-in-out;
}

.arrows .q-icon {
    height: 100%;
}
.sublanesBody{
  scroll-behavior: smooth;
  -ms-overflow-style: none; /* IE and Edge */
  scrollbar-width: none; /* Firefox */
}
.sublanesBody::-webkit-scrollbar{
  display: none;
}

.min-width-200 {
  min-width: 200px;
}
/* .TagDropDown {
    max-height: 200px !important;
} */
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.4s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
.cursor-grab{
  cursor: grab;
}
.fs-bold{
  font-weight: 600;
}
.sortable-ghost {
  opacity: 0.5;
  background-color: rgb(216, 216, 216);
}
.highlighted {
  background-color: rgba(255, 255, 0, 0.3); /* Light yellow highlight */
}
.swimlane {
  border-top-right-radius: 5px;
  border-top-left-radius: 5px;
  margin-bottom: 15px;
  background-color: rgb(245 245 245 / 50%);
  border-bottom: 2px solid rgb(197 197 197);
}

.swimlane-header {
  display: flex;
  justify-content: space-between;
  padding: 10px;
  cursor: grab;
  background-color: rgb(237 237 237 / 50%);
  border-bottom: 1px solid rgb(201 201 201);
}

.SubSwimlane {
  padding: 0 10px 10px;
}

.SubSwimlane-body{
  border-right: 1px solid rgb(150 150 150);
  border-bottom: 1px solid rgb(150 150 150);
  border-left: 1px solid transparent;
  border-top: 1px solid transparent;
  background-color: white;
}

.SubSwimlane-header {
  border-radius: 0;
  padding: 5px;
  cursor: grab;
}

.task-box {
  background: rgb(243 243 243 / 50%);
  padding: 0;
  margin: 0px 0px 10px;
  border-radius: 0px;
}
.task-box h3 {
  cursor: grab;
}

.fit-content {
  width: fit-content;
}
.border-top-grey {
  border-top: 4px solid #808080;
}

.border-top-orange {
  border-top: 4px solid #ffa500;
}

.border-top-red {
  border-top: 4px solid #ff0000;
}
.border-left-grey {
  border-left: 4px solid grey !important;
}

.border-left-orange {
  border-left: 4px solid orange !important;
}

.border-left-red {
  border-left: 4px solid red !important;
}
.projectchatbox i {
    font-size: 20px;
    width: 32px;
    height: 32px;
    text-align: center;
    padding: 0px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.projectchatbox {
    min-width: auto;
    min-height: auto;
}
.projectNotes {
  font-size: 22px;
  width: 32px;
  height: 35px;
  text-align: center;
  padding: 0px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #1b75ab;
  color: white;
  border-radius: 50%;
}
/* Arrow Scroll */
.scroll-area {
  overflow-x: auto;
  scroll-behavior: smooth;
}

.gradient-overlay {
  position: absolute;
  top: 0;
  bottom: 0;
  width: 40px;
  pointer-events: none;
  z-index: 5;
}

.gradient-left {
  left: 0;
  background: linear-gradient(to right, rgba(255, 255, 255, 0.95), transparent);
}

.gradient-right {
  right: 0;
  background: linear-gradient(to left, rgba(255, 255, 255, 0.95), transparent);
}

.absolute-left,
.absolute-right {
  top: 50%;
  transform: translateY(-50%);
}

.q-scrollarea__scroll {
  display: none;
}

.swimlane-list {
  margin-bottom: 20px;
}
.swimlane-list .swimlane-header {
  display: flex;
  justify-content: space-between;
  padding: 10px;
  cursor: grab;
  background-color: rgb(237 237 237 / 50%);
  border: 1px solid #1b75ab;
  border-bottom: none;
}
.swimlane-list .swimlane-body {
  border: 1px solid #1b75ab;
  border-top: none;
}
.swimlane-list .SubSwimlane-body {
  border:none;
  margin-left: 35px;
}
.swimlane-list .task-box{
  background: transparent;
}
.swimlane-list .bordered tr td {
    border: 1px solid #bfbfbf;
}
</style>
