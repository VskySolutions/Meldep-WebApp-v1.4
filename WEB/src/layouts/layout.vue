<template>
  <q-layout view="lHh Lpr lFf">
    <q-header bordered class="header">
      <q-toolbar class="header-top flex-center items-center justify-between">
        <div class="lt-md"><q-btn flat dense round icon="o_menu" class="text-black" aria-label="Menu" @click="toggleLeftDrawer" /></div>
        <div v-if="$q.screen.gt.sm" class="flex flex-center q-mr-lg">
          <q-btn flat class="no-padding q-ml-md" @click="$router.push(storedUser?.siteLandingPageLink)">
            <img src="~assets/meldep_logo.png" alt="logo" class="logo" style="width: 80px; height: 40px">
          </q-btn>
        </div>
        <div v-if="$q.screen.gt.sm" class="flex items-center menus flex-wrap">
          <div v-for="(singleModule) in sitesModules" :key="singleModule.id" class=" flex flex-center items-evenly menu-items" style="max-width: 100%">
            <div v-if="singleModule.customSiteModuleMenuList.length === 1">
              <!-- Direct link if only one menu -->
              <q-item
                clickable
                v-bind="singleModule.customSiteModuleMenuList[0].link.startsWith('http') ? { href: singleModule.customSiteModuleMenuList[0].link } : { to: singleModule.customSiteModuleMenuList[0].link }"
                :target="singleModule.customSiteModuleMenuList[0].openInNewTab ? '_blank' : '_self'"
              >
                <q-item-section class="fs-13" style="color: rgb(88, 88, 88, 1) !important; font-weight: 500;">
                  {{ singleModule.customSiteModuleMenuList[0].displayName }}
                </q-item-section>
              </q-item>
            </div>
            <div v-if="singleModule.customSiteModuleMenuList.length > 1">
              <div v-if="singleModule.customSiteModuleMenuList.length > 1">
                <q-btn-dropdown flat no-caps :label="singleModule.name" class="q-mr-md fs-13" header dense menu-anchor="bottom left" menu-self="top left">
                  <q-list class="app-menu">
                    <q-item
                      v-for="menu in singleModule.customSiteModuleMenuList"
                      :key="menu.id" v-ripple clickable
                      v-bind="menu.link.startsWith('http') ? { href: menu.link } : { to: menu.link }"
                      :target="menu.openInNewTab ? '_blank' : '_self'"
                      >
                      <!-- Avatar and Icon Section -->
                      <q-item-section avatar style="min-width: auto; border-radius: 10px" class="bg-orange-1 flex justify-center q-mr-sm q-pa-sm">
                        <q-icon :name="menu.icon" size="xs" color="orange-7" class="material-icons-outlined" />
                      </q-item-section>
                      <!-- Display Menu Label -->
                      <q-item-section>
                        <q-item-label class="fs-13">{{ menu.displayName }}</q-item-label>
                      </q-item-section>
                    </q-item>
                  </q-list>
                </q-btn-dropdown>
              </div>
            </div>
          </div>
          <q-btn
            v-if="remaining.some(module => module.customSiteModuleMenuList.length > 0)"
            color="transparent"
            class="headerMenuItems fs-12"
            label="Menu"
            style="text-transform: capitalize;"
            icon-right="o_arrow_drop_down"
          >
            <q-menu class="bg-white">
              <div v-for="singleModule in remaining" :key="singleModule.id">
                <div v-if="singleModule.customSiteModuleMenuList.length === 1">
                  <!-- Direct link if only one menu -->
                  <q-item
                    clickable
                    v-bind="singleModule.customSiteModuleMenuList[0].link.startsWith('http') ? { href: singleModule.customSiteModuleMenuList[0].link } : { to: singleModule.customSiteModuleMenuList[0].link }"
                    :target="singleModule.customSiteModuleMenuList[0].openInNewTab ? '_blank' : '_self'"
                  >
                    <q-item-section class="fs-12">
                      {{ singleModule.customSiteModuleMenuList[0].displayName }}
                    </q-item-section>
                  </q-item>
                </div>
                <div v-else-if="singleModule.customSiteModuleMenuList.length > 1">
                  <!-- Dropdown if multiple menus -->
                  <q-item clickable class="innerMenus">
                    <q-item-section class="fs-12">
                      {{ singleModule.name }}
                    </q-item-section>
                    <q-item-section side>
                      <q-icon name="o_keyboard_arrow_right" />
                    </q-item-section>

                    <q-menu auto-close anchor="top end" self="top start" class="innerMenu">
                      <q-list>
                        <q-item
                          v-for="menu in singleModule.customSiteModuleMenuList"
                          :key="menu.id"
                          clickable
                          exact
                          v-bind="menu.link.startsWith('http') ? { href: menu.link } : { to: menu.link }"
                          :target="menu.openInNewTab ? '_blank' : '_self'"
                          style="width:220px"
                        >
                          <q-item-section avatar class="rounded-corners flex justify-center q-pa-sm" style="min-width: 0px">
                            <q-icon
                              :name="menu.icon"
                              size="xs"
                              color="orange"
                              class="material-icons-outlined"
                            />
                          </q-item-section>
                          <q-item-section>
                            <q-item-label class="fs-12">
                              {{ menu.displayName }}
                            </q-item-label>
                          </q-item-section>
                        </q-item>
                      </q-list>
                    </q-menu>
                  </q-item>
                </div>
              </div>
            </q-menu>
          </q-btn>
          <div
            v-if="
              ['admin', 'site-super-admin', 'system-super-admin'].some(r => roles.includes(r)) &&
                settingsModule?.customSiteModuleMenuList?.length > 0"
          >
            <q-list class="app-menu flex items-center">
              <q-item v-ripple clickable exact to="/settings">
                <q-item-section>
                  <q-item-label class="fs-13" style="color: rgb(88, 88, 88, 1) !important; font-weight: 500;">Settings</q-item-label>
                </q-item-section>
              </q-item>
            </q-list>
          </div>
        </div>
        <!-------------------------------------------------------------------------------------------------------------------------------------->
        <div class="row q-gutter-md items-center no-wrap">
          <!-- <div v-if="storedUser?.siteName === 'Vsky Solutions'" class="column">
            <q-btn href="https://play.google.com/store/apps/details?id=com.vsky.meldep" download target="_blank" color="primary" text-color="white" class="q-pa-xs" style="border-radius: 7px;">
              <q-icon name="fa-brands fa-android" size="18px" color="white" />
              <q-tooltip anchor="bottom middle" self="top middle">Download Meld-EP 4.0 (v1.0) Mobile App</q-tooltip>
            </q-btn>
          </div> -->
          <div class="column">
            <!-- <q-btn unelevated color="primary" text-color="white" class="q-pa-xs" style="border-radius: 7px;" @click="onAdd"> -->
            <q-btn unelevated color="primary" text-color="white" class="q-pa-xs" style="border-radius: 7px;" @click="$router.push('/help-desk')">
              <q-icon name="fa-solid fa-headset" size="18px" color="white" class="q-mr-xs" />
              <q-item-label class="text-xs">
                <span class="block">HELP</span>
                <span class="block">DESK</span>
              </q-item-label>
              <q-tooltip anchor="bottom middle" self="top middle">Help Desk</q-tooltip>
            </q-btn>
          </div>
          <div class="column">
            <div>
              <q-btn round avatar font-size="38px" @click="getNotificationList(); getNotificationCount()">
                <q-icon name="o_notifications" color="grey-7" class="material-icons-outlined" />
                <q-badge floating color="red" square>{{ ntfModel }}</q-badge>
                <q-tooltip anchor="bottom middle" self="top middle">Notifications</q-tooltip>
              </q-btn>
              <!-- Notification Popup (QMenu) -->
              <q-menu ref="notifyMenu" class="notification" fit auto-close anchor="bottom start" style="width: 40% !important;">
                <q-card style="min-width: 65vh;">
                  <!-- Fixed Notifications Header -->
                  <q-card-section header class="text-bold text-h2 text-primary fit" style="letter-spacing: 1px;">
                    <div class="flex items-center justify-between">
                      <div class="flex items-center">
                        <q-icon name="o_notifications" size="sm" class="q-mr-sm" />
                        Notifications
                      </div>
                      <q-btn
                        flat
                        dense
                        no-caps
                        label="Clear All"
                        class="text-bold text-primary"
                        @click.stop="getNotificationList(null, 'clearAll')"
                      />
                    </div>
                  </q-card-section>
                  <q-space />
                  <!-- Scrollable Notifications List -->
                  <q-list v-if="AllNotifications.length > 0" class="app-menu">
                    <q-item
                      v-for="notification in AllNotifications" :key="notification.ids" clickable exact :to="`${notification.redirectURL}`" style="width:100%" class=""
                    >
                      <q-item-section avatar class="rounded-corners flex justify-centerq-pa-sm" style="min-width: 0px">
                        <q-icon name="summarize" size="sm" color="orange" class="material-icons-outlined" />
                      </q-item-section>
                      <q-item-section @click="getNotificationList(notification.id, 'RN')">
                        <q-item-label class="fs-12 text-wrap truncate-text">{{ notification.title }}</q-item-label>
                        <q-item-label class="fs-10 text-grey text-wrap truncate-text">{{ notification.message }}</q-item-label>
                      </q-item-section>
                    </q-item>
                  </q-list>
                  <hr class="q-separator text-grey-1">
                  <!-- Loader -->
                  <q-inner-loading :showing="loadingNotifications" color="primary">
                    <q-spinner-ios size="30px" />
                  </q-inner-loading>
                  <!-- Fixed View More Button -->
                  <div class="text-center">
                    <q-btn flat class="q-py-sm text-bold text-primary" no-caps label="View More Notifications" style="width: 100%;" @click="morenotifications" />
                  </div>
                </q-card>
              </q-menu>
            </div>
          </div>
          <div class="column items-center" />
          <!-- Here is User-info component -->
          <user-info />
        </div>
      </q-toolbar>
    </q-header>
    <q-drawer v-if="$q.screen.lt.md" v-model="leftDrawerOpen" bordered :width="292" :breakpoint="1024" class="bg-white">
      <aside-header />
      <q-scroll-area class="fit" :thumb-style="thumbStyle" :horizontal-thumb-style="{ opacity: 0 }">
        <AppMenu />
      </q-scroll-area>
      <div div class="absolute lt-sm" style="top: 58px; right: -13px">
        <q-btn
          size="md"
          unelevated
          color="white"
          icon="fa-solid fa-chevron-left"
          @click="leftDrawerOpen = !true"
        />
      </div>
    </q-drawer>
    <q-page-container>
      <router-view />
    </q-page-container>
    <div style="background-color: white; box-shadow: 0px 4px 8px rgba(72, 72, 72, 0.97); height: 45px; display: flex; justify-content: center; align-items: center; width: 100%; bottom: 0; margin: 0; padding: 0;">
      <h6 style="margin: 0; padding: 0; text-align: center; color: black;">Copyright &copy; 2025 Vsky. Website Designed and Developed by
        <a href="https://www.vskysolutions.com/" target="_blank" style="text-decoration: none; color: #007bff;">
          VSky Solutions.
        </a>
      </h6>
    </div>
    <!-- Task Activity Timer -->
    <div v-if="userTasks.length > 0" v-touch-pan.horizontal.vertical.mouse="onDrag" :style="{ top: position.top + 'px', left: position.left + 'px'}" class="TaskActivityTimer floating-timer">
      <q-list>
        <q-item v-for="task in userTasks" :key="task.taskId" class="items-center bg-grey-3 q-mb-sm" style="border-bottom: 1px solid #1b75ab; padding: 5px;">
          <q-item-section>
            <q-item-label class="text-left" size="xs">
              <span class="text-bold q-mr-sm">{{ formattedTime(task) }}</span> - {{ task.taskName }} - <q-badge color="secondary">{{ task.activityName }}</q-badge>
            </q-item-label>
          </q-item-section>
          <q-item-section side style="flex-direction: row;">
            <q-icon v-if="!task.running" name="o_play_circle" class="cursor-pointer" size="sm" color="green" @click="startTask(task.taskId, true)">
              <q-tooltip>Resume Timer</q-tooltip>
            </q-icon>
            <q-icon v-if="task.running" name="o_pause_circle" class="cursor-pointer" size="sm" color="red" @click="pauseTask(task.taskId, true)">
              <q-tooltip>Pause Timer</q-tooltip>
            </q-icon>
            <q-icon name="o_replay_circle_filled" class="cursor-pointer" size="sm" color="secondary" @click="resetTask(task.taskId)">
              <q-tooltip>Reset Timer</q-tooltip>
            </q-icon>
            <q-icon name="o_delete" class="cursor-pointer" size="sm" color="red" @click="deleteTask(task.taskId)">
              <q-tooltip>Delete Timer</q-tooltip>
            </q-icon>
            <q-icon name="o_send" class="cursor-pointer" size="sm" color="green" @click="openTaskTimmerDialog(task)">
              <q-tooltip>Send To Timesheet</q-tooltip>
            </q-icon>
          </q-item-section>
          <q-dialog v-model="IsTaskDialogOpened[task.taskId]" persistent>
            <q-card>
              <q-card-section class="card-header with-tools position-relative">
                <div class="text-h2">Send Task Time To Timesheet?</div>
                <q-btn v-close-popup icon="o_close" class="close" flat round dense style="position: absolute; right: 5px; top: 10px;" @click="startTask(task.taskId, true)" />
              </q-card-section>
              <q-separator />
              <q-card-section class="card-body scroll">
                <div class="row">
                  <div class="col-12 q-mb-sm">
                    <label class="fw-bold q-mb-xs">Task Name<span class="required text-red">*</span></label>
                    <q-input
                      v-model="task.taskName"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="dense" readonly
                    />
                  </div>
                  <div class="col-6 q-mb-sm">
                    <label class="fw-bold q-mb-xs">Activity Name<span class="required text-red">*</span></label>
                    <q-input
                      v-model="task.activityName"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="dense" readonly
                    />
                  </div>
                  <div class="col-12 q-mb-sm">
                    <label class="fw-bold q-mb-xs">Timesheet Description</label>
                    <q-input
                      v-model="task.description"
                      outlined
                      stack-label
                      hide-bottom-space
                      autogrow
                      maxlength="500"
                    />
                  </div>
                  <div class="col-3 q-mb-sm">
                    <label class="fw-bold q-mb-xs">Est. Hours<span class="required text-red">*</span></label>
                    <q-input
                      v-model="task.hours"
                      outlined
                      hide-bottom-space
                      :dense="true"
                      maxlength="5"
                      hint="hh.mm"
                      :rules="[validateTaskEstimatedHours]"
                    >
                      <template #append>
                        <q-icon name="o_timer" color="primary" />
                      </template>
                    </q-input>
                  </div>
                </div>
              </q-card-section>
              <q-separator />
              <q-card-actions align="right">
                <q-btn v-close-popup color="grey-8" flat dense bordered label="Close" @click="startTask(task.taskId, true)" />
                <q-btn color="primary" flat bordered label="Send To Timesheet" :disabled="IsTaskSubmitDisabled" @click="onTimmerSubmit(task)" />
              </q-card-actions>
            </q-card>
          </q-dialog>
        </q-item>
      </q-list>
    </div>
  </q-layout>
</template>

<script setup>
import { ref, onMounted, watch, onUnmounted, onBeforeUnmount, computed, provide } from "vue";
import { notifySuccess, notifyError, notifyWarning, getLocalStorage } from "assets/utils";
import { useQuasar } from "quasar";
import { useRouter } from "vue-router";
import { useAuthStore } from "stores/auth";
import { format } from "date-fns"; // Standard TimeZone Conversion

import UserInfo from "shared/user_info.vue";
import AsideHeader from "shared/aside_header.vue";
import AppMenu from "src/components/app_menu.vue";

import moduleService from "modules/module/module.service";
import notificationsService from "modules/notification/notifications.service";
import projectActivitiesService from "modules/project-tasks-activities/projectTasksActivities.service";

const $q = useQuasar();
const router = useRouter();
const storedUser = getLocalStorage("user");

const screenWidth = ref(window.innerWidth);
const updateScreenWidth = () => { screenWidth.value = window.innerWidth; };
const morenotifications = () => { router.push("/notifications"); };

const leftDrawerOpen = ref(false);
const sitesModules = ref([]);
const remaining = ref([]);
const allModules = ref([]);
// const AllReports = ref([]);
const AllNotifications = ref([]);
const loadingNotifications = ref(false);
const notifyMenu = ref(null);
const ntfModel = ref({});
let notificationInterval = null;
const settingsModule = ref(null);
// ----------------------------------------------------------------------------------------------------------------------
// System Notifications
// ----------------------------------------------------------------------------------------------------------------------

const getSiteActiveModulesMenus = () => {
  moduleService.getSiteActiveModulesMenus().then((resp) => {
    allModules.value = resp;
    settingsModule.value = resp.find(
      m => m.name === "Settings"
    ) || null;
    allModules.value = resp.filter(m => !m.name.includes("Settings"));
  }).finally(() => {
  });
};

const getNotificationCount = () => {
  notificationsService.getNotificationCount().then((resp) => {
    ntfModel.value = resp;
  }).catch((error) => {
    console.error("Error fetching notification count:", error);
  });
};

const toggleLeftDrawer = () => {
  leftDrawerOpen.value = !leftDrawerOpen.value;
};

// ----------------------------------------------------------------------------------------------------------------------
// PowerBI Reports - (MD)
// ----------------------------------------------------------------------------------------------------------------------

// async function getAllReports () {
//   const resp = await reportService.getAllReport();
//   AllReports.value = resp.reportModelList;
// }

async function getNotificationList (Id, flag) {
  try {
    loadingNotifications.value = true;
    const resp = await notificationsService.getNotificationList(Id, flag);
    AllNotifications.value = resp;
    getNotificationCount();
    if (flag === "clearAll" && AllNotifications.value.length === 0) {
      notifyMenu?.value?.hide();
    }
  } catch (error) {
    console.error("Error fetching notifications:", error);
  } finally {
    loadingNotifications.value = false; // hide loader
  }
}

// ----------------------------------------------------------------------------------------------------------------------
// Help Desk - Click Action
// ----------------------------------------------------------------------------------------------------------------------

// const onAdd = () => {
//   $q.dialog({
//     component: addeditHelpDesk,
//     componentProps: {}
//   }).onOk(() => {
//   }).onCancel(() => {
//   }).onDismiss(() => {
//   });
// };

// ----------------------------------------------------------------------------------------------------------------------
// Task Activity Timer
// ----------------------------------------------------------------------------------------------------------------------
const authStore = useAuthStore();
const user = authStore.user;
const roles = user?.roles || [];

// Constants
const TASK_STORAGE_KEY = "projectTaskActivityTimer";
const POSITION_STORAGE_KEY = "projectTaskActivityTimerPosition";
const employeeId = user?.employeeId ?? user?.userId;
const DefaultTimerWidth = 470;
const RightOffTimer = 50;

// Variables
const tasks = ref([]);
const timers = ref({});
const position = ref([]);
const IsTaskDialogOpened = ref({});
const IsTaskSubmitDisabled = ref(false);

// -------------------------------------------------
// Employee Tasks Functions
// -------------------------------------------------

// Current Employee's Task
const userTasks = computed(() => tasks.value.filter(task => task.employeeId === employeeId));

// Helper Functions
function loadTasksFromStorage () {
  try {
    const storedTasks = localStorage.getItem(TASK_STORAGE_KEY);
    return storedTasks ? JSON.parse(storedTasks) : [];
  } catch (error) {
    console.error("Failed to load tasks from storage:", error);
    return [];
  }
}

function saveTasksToStorage () {
  localStorage.setItem(TASK_STORAGE_KEY, JSON.stringify(tasks.value));
}

// -------------------------------------------------
// Positioning Functions
// -------------------------------------------------

function getDefaultOrLastPosition () {
  const savedPositions = JSON.parse(localStorage.getItem(POSITION_STORAGE_KEY)) || {};
  return savedPositions[employeeId] || {
    left: window.innerWidth - DefaultTimerWidth,
    top: window.innerHeight - RightOffTimer - GetCurrentTimerHeight()
  };
}

function savePosition () {
  const savedPositions = JSON.parse(localStorage.getItem(POSITION_STORAGE_KEY)) || {};
  savedPositions[employeeId] = position.value;
  localStorage.setItem(POSITION_STORAGE_KEY, JSON.stringify(savedPositions));
}

// **Dragging Feature**
function onDrag (event) {
  position.value.top = Math.max(0, Math.min(position.value.top + event.delta.y, window.innerHeight - GetCurrentTimerHeight()));
  position.value.left = Math.max(0, Math.min(position.value.left + event.delta.x, window.innerWidth - DefaultTimerWidth));
  savePosition();
}

function GetCurrentTimerHeight () {
  return userTasks.value.length * 60;
}

// -------------------------------------------------
// Timer Task Functions
// -------------------------------------------------
// console.log(new Date());
// console.log(new Date().getTime());
// console.log(new Date().getDate());
// console.log(format(new Date(), "MM/dd/yyyy")); // Standard TimeZone Conversion

// Timer Logic
function formattedTime(task) {
  const current = new Date().getTime();
  const start = new Date(task.startDate).getTime();

  const elapsed = task.running ? Math.floor((current - start) / 1000) : (task.elapsedTime || 0);

  const hours = Math.floor(elapsed / 3600);
  const minutes = Math.floor((elapsed % 3600) / 60);
  const seconds = elapsed % 60;

  const hh = String(hours).padStart(2, "0");
  const mm = String(minutes).padStart(2, "0");
  const ss = String(seconds).padStart(2, "0");

  const time = elapsed >= 3600 ? `${hh}:${mm}:${ss}` : `${mm}:${ss}`;
  return time;
}

// To start a new task timer
const startNewTask = async (currentTask) => {
  const dateTime = new Date().getTime();

  if (userTasks.value.some(t => t.taskId === currentTask.taskId)) {
    notifyError({ message: "Task is already running!" });
    return;
  }

  if (userTasks.value.filter(t => t.running).length >= 2) {
    notifyError({ message: "You can only run 2 tasks at a time!" });
    return;
  }

  const newTask = {
    employeeId,
    taskId: currentTask.taskId,
    taskName: currentTask.taskName,
    activityId: currentTask.activityId,
    activityName: currentTask.activityName,
    startDate: dateTime,
    running: true,
    elapsedTime: 0,
    hours: 0.00,
    description: ""
  };

  tasks.value.push(newTask);
  startTask(currentTask.taskId, false);
  getDefaultOrLastPosition();
  savePosition();
  notifySuccess({ message: "Task Timer Started" });
};

// Start Timer
function startTask (taskId, IsResume) {
  const task = tasks.value.find(t => t.taskId === taskId);
  if (!task || task.running) return;

  clearInterval(timers.value[taskId]);

  // Recalculate and store startDate in ISO format
  task.startDate = new Date(Date.now() - (task.elapsedTime || 0) * 1000).toISOString();
  task.running = true;

  timers.value[taskId] = setInterval(() => {
    task.elapsedTime = Math.floor((Date.now() - new Date(task.startDate).getTime()) / 1000);
    saveTasksToStorage();
  }, 1000);

  if (IsResume) {
    notifySuccess({ message: "Task Timer Resumed" });
  }
}

// Pause Timer
function pauseTask (taskId, notify) {
  const task = tasks.value.find(t => t.taskId === taskId);
  if (!task) return;

  clearInterval(timers.value[taskId]);
  task.elapsedTime = Math.floor((Date.now() - new Date(task.startDate).getTime()) / 1000);
  task.running = false;
  task.startDate = null;
  saveTasksToStorage();

  if (notify) notifyWarning({ message: "Task Timer Paused" });
}

// Reset Task Timer
function resetTask (taskId) {
  $q.dialog({
    title: "Reset Task Time",
    message: "Do you want to reset time for this task?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    const task = tasks.value.find(t => t.taskId === taskId);
    if (!task) return;

    clearInterval(timers.value[taskId]);
    task.startDate = null;
    task.elapsedTime = 0;
    task.hours = 0.00;
    task.running = false;
    saveTasksToStorage();
  });
}

// Delete the task timer
function deleteTask (taskId) {
  $q.dialog({
    title: "Delete Task Timer",
    message: "Are you sure you want to delete task timer?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    clearInterval(timers.value[taskId]);
    delete timers.value[taskId];
    tasks.value = tasks.value.filter(t => t.taskId !== taskId);
    notifyError({ message: "Task Timer Deleted." });
    getDefaultOrLastPosition();
  });
}

function openTaskTimmerDialog (task) {
  // Calculate elapsed time correctly
  const elapsed = task.running ? Math.floor((Date.now() - new Date(task.startDate)) / 1000) : task.elapsedTime || 0;
  const hours = Math.floor(elapsed / 3600);
  const minutes = Math.floor((elapsed % 3600) / 60);

  if (hours === 0 && minutes < 1) {
    notifyWarning({ message: "Task timer cannot be less than 1 min" });
  } else {
    // Correctly format hours and minutes as 0.2, 0.3, etc.
    const hoursDecimal = `${hours}.${minutes.toString().padStart(2, "0")}`;
    task.hours = parseFloat(hoursDecimal);
    pauseTask(task.taskId, false);
    IsTaskDialogOpened.value[task.taskId] = true;
    validateTaskEstimatedHours(task.hours);
  }
}

function onTimmerSubmit (currentTask) {
  $q.dialog({
    title: "Send To Timesheet",
    message: "Do you want this task's time to be added to the Timesheet?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    // Send time to timesheet API
    const payload = {
      employeeId,
      taskId: currentTask.taskId,
      activityId: currentTask.activityId,
      hours: currentTask.hours,
      description: currentTask.description
    };
    projectActivitiesService.sendTaskTimerToTimesheet(payload)
      .then(resp => {
        // Stop the timer for the task
        clearInterval(timers.value[currentTask.taskId]);
        delete timers.value[currentTask.taskId];
        // Remove task after successful API call
        tasks.value = tasks.value.filter(t => t.taskId !== currentTask.taskId);
        notifySuccess({ message: "Timesheet Added" });
      })
      .catch(() => {
        notifyError({ message: "Failed to send task to timesheet." });
      });
  }).onCancel(() => {
    notifyWarning({ message: "Timesheet Submission Cancelled" });
  });
}

function validateTaskEstimatedHours (value) {
  if (value === null || value === undefined || value === "" || value === 0) {
    IsTaskSubmitDisabled.value = true;
    return false;
  }

  const regex = /^\d{1,3}(\.\d{1,2})?$/;
  if (regex.test(String(value))) {
    IsTaskSubmitDisabled.value = false;
    return true;
  }

  IsTaskSubmitDisabled.value = true;
  return "Invalid Hours Format.";
}

// On Load
onMounted(() => {
  getSiteActiveModulesMenus();
  // getAllReports();
  getNotificationList();
  getNotificationCount();

  // calling getNotificationCount every 10s
  notificationInterval = setInterval(() => {
    getNotificationCount();
  }, 60000);

  window.addEventListener("resize", updateScreenWidth);

  // TaskTimer
  tasks.value = loadTasksFromStorage();
  position.value = getDefaultOrLastPosition();
  userTasks.value.forEach(task => {
    if (task.running) {
      startTask(task.taskId, false);
    }
  });
  setInterval(() => {
    tasks.value = [...tasks.value];
  }, 1000);
});

onBeforeUnmount(() => {
  if (notificationInterval) {
    clearInterval(notificationInterval);
  }
  window.removeEventListener("resize", updateScreenWidth);
});

onUnmounted(() => {
  if (notificationInterval) {
    clearInterval(notificationInterval);
  }
  window.removeEventListener("resize", updateScreenWidth);
});

// On Tasks added or changed
watch(tasks, () => { saveTasksToStorage(); }, { deep: true });

// Hide & Show Menus as per screen size
watch(() => [screenWidth.value, allModules.value], (newValue, oldValue) => {
  getNotificationCount();

  if (newValue !== oldValue) {
    // Filter modules that have at least one menu
    const modulesWithMenus = allModules.value.filter(m => m.customSiteModuleMenuList.length > 0);
    if (screenWidth.value > 2000) { // New breakpoint
      // Show up to 9 modules directly
      if (modulesWithMenus.length <= 9) {
        sitesModules.value = modulesWithMenus;
        remaining.value = [];
      } else {
        sitesModules.value = modulesWithMenus.slice(0, 9);
        remaining.value = modulesWithMenus.slice(9);
      }
    } else if (screenWidth.value > 1900) {
      if (modulesWithMenus.length <= 8) {
        sitesModules.value = modulesWithMenus;
        remaining.value = [];
      } else {
        sitesModules.value = modulesWithMenus.slice(0, 8);
        remaining.value = modulesWithMenus.slice(8);
      }
    } else if (screenWidth.value > 1800) {
      if (modulesWithMenus.length <= 8) {
        sitesModules.value = modulesWithMenus;
        remaining.value = [];
      } else {
        sitesModules.value = modulesWithMenus.slice(0, 7);
        remaining.value = modulesWithMenus.slice(7);
      }
    } else if (screenWidth.value > 1700) {
      if (modulesWithMenus.length <= 7) {
        sitesModules.value = modulesWithMenus;
        remaining.value = [];
      } else {
        sitesModules.value = modulesWithMenus.slice(0, 6);
        remaining.value = modulesWithMenus.slice(6);
      }
    } else if (screenWidth.value > 1500) {
      sitesModules.value = modulesWithMenus.slice(0, 5);
      remaining.value = modulesWithMenus.slice(5);
    } else if (screenWidth.value > 1200) {
      sitesModules.value = modulesWithMenus.slice(0, 3);
      remaining.value = modulesWithMenus.slice(3);
    } else {
      sitesModules.value = modulesWithMenus.slice(0, 2);
      remaining.value = modulesWithMenus.slice(2);
    }
  }
});

// When screen size changes
window.addEventListener("resize", getDefaultOrLastPosition());

// Provide functions globally
provide("startNewTask", startNewTask);
</script>

<style scoped>
  .narbars{
    margin-right: 13%;
  }
  .q-item.q-router-link--active, .q-item--active {
    color: #3ba5e5;
    font-weight: 500;
  }
  .no-underline {
  text-decoration: none !important;
}
.TaskActivityTimer {
  position: fixed;
  width: 450px;
  border-radius: 8px;
  cursor: grab;
  z-index: 999;
}
.task-list {
  overflow-y: auto;
}
.q-dialog__title {
    color: #1b75ab;
}
</style>
