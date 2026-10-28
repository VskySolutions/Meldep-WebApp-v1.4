<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card  class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1600px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Add/Edit Weekly Plan</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense  />
      </q-card-section>
      <q-separator />
       <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <q-inner-loading
                :showing="loadingWeeklyPlan"
                color="primary"
                label-class="text-primary"
              />
              <legend>
                <q-breadcrumbs class="text-brown text-weight-bold text-h3">
                  <template #separator>
                    <q-icon size="1.5em" name="o_chevron_right" color="primary" />
                  </template>
                  <q-breadcrumbs-el :label="'Project - ' + props.projectName" />
                  <q-breadcrumbs-el class="text-primary" :label="'Task - ' + props.taskName" />
                </q-breadcrumbs>
              </legend>
              <div class="row full-width">
                <div class="col-12 flex justify-end q-px-sm q-my-sm">
                  <q-btn icon="o_add" outline label="Add Lines" size="sm" no-caps class="text-primary btnRounded" @click="onAddProjectPlanLines()" />
                </div>
              </div>
              <div class="row full-width q-px-sm">
                <div v-if="model.weekDateLines?.length > 0" class="col-12 full-width">
                  <table class="WhiteTable full-width">
                    <thead>
                      <tr>
                        <th style="font-size: 15px;">Weekend<span class="required">*</span></th>
                        <th width="43%">Planned<span class="required">*</span></th>
                        <th width="43%">Actual</th>
                        <th width="50px">Action</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="(line, lineIndex) in model.weekDateLines" :key="line.id" :class="line.deleted ? 'hidden' : ''" >
                        <td>
                          <formDate
                            v-model="line.weekDate"
                            :dateOptions="getWeekDateOptions(line)"
                            :navigationMinYearMonth="minMonth"
                            :minimal="true"
                            :firstDayOfWeek="1"
                            :disable="line.isApproved"
                          />
                          <div class="q-mt-sm flex flex-center">
                            <q-btn
                              icon="o_add"
                              size="sm"
                              class="bg-grey-6 text-white hidden"
                              flat
                              dense
                              @click="addWeeklyPlanDateLines(line)"
                            >
                              <q-tooltip>Add Line</q-tooltip>
                            </q-btn>
                            <q-btn
                              v-if="isFullAccess && showApprovePlanAction && !line.isApproved"
                              icon="o_lock"
                              color="primary"
                              size="sm"
                              class="q-mr-xs q-mb-xs"
                              @click="approveThisWeeklyPlan(line.projectWeeklyPlanDatesId, true)"
                            >
                              <q-tooltip>Click Here To Lock This Plan</q-tooltip>
                            </q-btn>
                            <q-btn
                              v-if="!line.isApproved && !showApprovePlanAction && line.id != null"
                              icon="o_lock_open"
                              color="grey"
                              size="sm"
                              class="q-mr-xs q-mb-xs cursor-inherit"
                            >
                              <q-tooltip>Plan Unlocked</q-tooltip>
                            </q-btn>
                            <q-btn
                              v-if="isFullAccess && showApprovePlanAction && line.isApproved && !line.isCompleted"
                              icon="o_lock_open"
                              color="secondary"
                              size="sm"
                              class="q-mr-xs q-mb-xs"
                              @click="approveThisWeeklyPlan(line.projectWeeklyPlanDatesId, false)"
                            >
                              <q-tooltip>Click Here To Unlock This Plan</q-tooltip>
                            </q-btn>
                            <q-btn
                              v-if="(showApprovePlanAction && line.isApproved && line.isCompleted) || (!showApprovePlanAction && line.isApproved)"
                              icon="o_lock"
                              color="primary"
                              size="sm"
                              class="q-mr-xs q-mb-xs cursor-inherit"
                            >
                              <q-tooltip>
                                {{ "Plan Locked By - " + line.approvedBy?.person?.firstName + " " + line.approvedBy?.person?.lastName + " (" + line?.approvedOnUtc + ")" }}
                              </q-tooltip>
                            </q-btn>
                            <q-btn
                              v-if="line.isApproved && !showApprovePlanAction && !line.isCompleted"
                              icon="o_pause_circle"
                              color="grey"
                              size="sm"
                              class="q-mr-xs q-mb-xs cursor-inherit"
                            >
                              <q-tooltip>Waiting For Completion</q-tooltip>
                            </q-btn>
                            <q-btn
                              v-if="isFullAccess && line.isApproved && showApprovePlanAction && !line.isCompleted"
                              icon="o_percent"
                              size="sm"
                              color="green"
                              class="q-mr-xs q-mb-xs"
                              @click="showCompletionDialog = true; selectedProjectPlanDateId = line.projectWeeklyPlanDatesId;"
                            >
                              <q-tooltip>Click Here To Mark As Completed</q-tooltip>
                            </q-btn>
                            <q-btn
                              v-if="line.isApproved && line.isCompleted"
                              size="sm"
                              :color="
                                line.completionPercentage <= 25 ? 'red' :
                                line.completionPercentage <= 50 ? 'orange' :
                                line.completionPercentage <= 75 ? 'primary' :
                                'green'
                              "
                              class="q-mr-xs q-mb-xs cursor-inherit fs-13"
                            >
                              {{ line.completionPercentage }}%
                              <q-tooltip>
                                {{ "Marked Completed By - " + line.completedBy?.person?.firstName + " " + line.completedBy?.person?.lastName + " (" + line?.completedOnUtc + ")" }}
                              </q-tooltip>
                            </q-btn>
                          </div>
                        </td>
                        <td>
                          <div class="row q-mb-sm">
                            <div
                              v-if="line.isApproved"
                              class="col-12 flex justify-end"
                              style="height: 30px; padding-right: 2px;"
                            >
                              <q-icon name="o_lock" size="sm" color="grey-7" />
                            </div>
                            <q-editor
                              v-model="line.description"
                              class="full-width"
                              :dense="$q.screen.lt.md"
                              :toolbar="toolbar"
                              :fonts="fonts"
                              :readonly="line.isApproved && !line.isFullAccess"
                              :class="{ 'readonly-editor': line.isApproved && !line.isFullAccess }"
                            />
                            <div v-if='line.id != null'class="col-12 text-right">
                              <div class="LastUpdatedBy text-capitalize q-md-mt-xs">
                                <div>
                                  Created By:-
                                    {{ line?.expectedDescriptionCreatedBy?.person?.firstName + ' ' +
                                          line?.expectedDescriptionCreatedBy?.person?.lastName +
                                          '(' + line?.expectedDescriptionCreatedOnUtc + ')' }}
                                </div>

                                <div>
                                  Last Updated:-
                                    {{ line?.expectedDescriptionUpdatedBy?.person?.firstName + ' ' +
                                      line?.expectedDescriptionUpdatedBy?.person?.lastName +
                                        '(' + line?.expectedDescriptionUpdatedOnUtc + ')' }}
                                </div>
                              </div>
                            </div>
                          </div>
                          <div v-if="line.weekDate" class="row">
                            <div class="col-xxl-2 col-xl-2 col-lg-3 col-md-3 col-sm-3 col-xs-3 flex items-center">
                              <q-input v-model="line.expectedHours" outlined hide-bottom-space :dense="true" :readonly="true" label="Est. Hrs*" class="q-mr-sm" />
                            </div>
                            <div class="col-xxl-10 col-xl-10 col-lg-9 col-md-9 col-sm-9 col-xs-9 flex items-center">
                              <div class="TaskActivity flex items-center">
                                <!-- Task Assignment - Action Button -->
                                <div class="q-mr-sm" >
                                  <q-icon
                                    v-if="line.isApproved"
                                    name="o_group"
                                    color="grey-8"
                                    size="sm"
                                    @click.stop.prevent
                                  />
                                  <q-icon
                                    v-else
                                    name="o_group_add"
                                    color="grey-8"
                                    size="sm"
                                    class="cursor-pointer"
                                    @click="resetResourceModel"
                                  >
                                    <q-tooltip>Assign Resource?</q-tooltip>
                                  </q-icon>
                                  <q-popup-edit
                                    class="small-popup-title"
                                    style="width: 280px;"
                                    @before-show="() => {
                                      resourceModel.value = { id: uid(), employeeId: null, estimatedHours: ''};
                                      addResourceBtn = true;
                                      weeklyEmployeeDropdownSingleSelect.load(props.projectId, planTypeId, line.weekDate)
                                    }"
                                  >
                                    <div class="row justify-between items-center q-mb-sm">
                                      <h3>Add Resource</h3>
                                      <q-btn v-close-popup icon="o_close" size="sm" color="black" flat round dense />
                                    </div>
                                    <formSingleSelectDropdown
                                      v-model="selectedResourcesOptions"
                                      label="Select Employee"
                                      :options="weeklyEmployeeDropdownSingleSelect.list.value"
                                      :filter="weeklyEmployeeDropdownSingleSelect.filter"
                                      @update:model-value="(val) => checkIfEmployeeAlreadyExistsInPlanLine(line.saveWeeklyLinesAssignTos, val)"
                                    />
                                    <label class="q-mt-sm">Est. Hrs <span class="text-red">*</span></label>
                                    <q-input
                                      v-model="resourceModel.estimatedHours"
                                      outlined
                                      hide-bottom-space
                                      dense
                                      mask="##:##"
                                      maxlength="5"
                                      :rules="[validateHours]"
                                      class="full-width"
                                      @keyup="checkAddResourceFields(resourceModel)"
                                      @blur="resourceModel.estimatedHours = formatHoursValue(resourceModel.estimatedHours);"
                                    >
                                    <template #hint>
                                      <span
                                        v-if="resourceModel.estimatedHours && validateHours(resourceModel.estimatedHours) === true && resourceModel.estimatedHours !== '00:00'""
                                        class="text-caption text-primary"
                                      >
                                        {{ getHoursMinutesText(resourceModel.estimatedHours) }}
                                      </span>
                                      <span v-else>
                                        hh:mm
                                      </span>
                                    </template>
                                    </q-input>
                                    <div class="row justify-end q-gutter-sm q-mt-sm">
                                      <q-btn v-close-popup label="Cancel" color="grey" flat dense />
                                      <q-btn v-close-popup label="Set" color="primary" dense :disable="addResourceBtn" :loading="resourceProcessing" @click="onAddResourceToLine(line, resourceModel)" />
                                    </div>
                                  </q-popup-edit>
                                </div>
                                <!-- Task Assignment - Labels -->
                               <template
                                  v-for="(resource, resourceIndex) in line.saveWeeklyLinesAssignTos"
                                  :key="resource.id"
                                >
                                  <div
                                    v-show="!resource.deleted"
                                    class="Person position-relative q-mr-xs"
                                  >
                                    {{ (resource?.firstName?.trim()?.[0] || '') + (resource?.lastName?.trim()?.[0] || '') + "(" + resource.estimatedHours + ")" }}

                                    <q-icon
                                      v-if="!line.isApproved"
                                      class="delete"
                                      name="o_close"
                                      color="red"
                                      size="xs"
                                      @click="onDeleteResourceToLine(line, resource, resourceIndex)"
                                    />

                                    <q-tooltip>
                                      <div>
                                        <q-icon name="o_person" color="white" size="xs" class="q-mr-xs" />
                                        <span>{{ resource.firstName + ' ' + resource?.lastName }}</span>
                                      </div>
                                      <div v-if="resource.primaryEmailAddress">
                                        <q-icon name="o_mail" color="white" size="xs" class="q-mr-xs" />
                                        <span>{{ resource.primaryEmailAddress }}</span>
                                      </div>
                                    </q-tooltip>
                                  </div>
                                </template>
                              </div>
                            </div>
                          </div>
                        </td>
                        <td>
                          <div
                            v-if="line.isApproved"
                            class="col-12 flex justify-end"
                            style="height: 30px; padding-right: 2px;"
                          >
                            <q-icon name="o_lock" size="sm" color="grey-7" />
                          </div>
                          <q-editor
                            v-model="line.actualDescription"
                            class="full-width"
                            :dense="$q.screen.lt.md"
                            :toolbar="toolbar"
                            :fonts="fonts"
                            :readonly="line.isApproved && !line.isFullAccess"
                            :class="{ 'readonly-editor': line.isApproved && !line.isFullAccess }"
                          />
                          <div v-if="line.id != null && line.actualDescriptionUpdatedOnUtc" class="LastUpdatedBy text-capitalize text-end">
                            <div>
                              Created By:-
                              {{ line?.actualDescriptionCreatedBy?.person?.firstName + ' ' +
                                line?.actualDescriptionCreatedBy?.person?.lastName +
                                '(' + line?.actualDescriptionCreatedOnUtc + ')' }}
                            </div>

                            <div>
                              Last Updated:-
                              {{ line?.actualDescriptionUpdatedBy?.person?.firstName + ' ' +
                                line?.actualDescriptionUpdatedBy?.person?.lastName +
                                '(' + line?.actualDescriptionUpdatedOnUtc + ')' }}
                            </div>
                          </div>
                        </td>
                        <td class="text-center">
                          <q-btn :disable="model.weekDateLines?.length <= 1 || line.isApproved" icon="o_delete" outline size="sm" color="red" no-caps class="q-ml-sm" @click="onDeleteProjectPlanLines(lineIndex)" />
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
                <div v-else class="col-12 full-width q-my-md">
                  <h6 class="text-red text-center">No plan added</h6>
                </div>
              </div>
              <div v-if="model.weekDateLines?.length > 0" class="row full-width flex justify-center q-px-sm q-mt-sm">
                <q-btn size="md" color="grey-4" outline label="Close" type="button" class="text-grey-9 q-mr-xs" style="width: 120px;" no-caps @click="onDialogCancel"  />
                <q-btn size="md" color="primary" outline label="Save & Close" type="button" :loading="processing" :disable="disableActionButton || processing" style="width: 120px;" no-caps @click="onSaveProjectPlan()" />
              </div>
            </fieldset>
          </div>
        </div>
      </q-form>
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
import { ref, onMounted, watch } from "vue";
import { useDialogPluginComponent, useQuasar, uid, date } from "quasar";
import { notifySuccess, notifyError } from "assets/utils";
import { useAuthStore } from "stores/auth";

import projectService from "modules/project/projects.service";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

// SOP Change :- Shared Dropdowns
import projectModule from "src/modules/project/utils/dropdowns.js";
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import projectTargetPlanModule from "src/modules/project-targetplan/utils/dropdowns.js";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const resourceProcessing = ref(false);
const processing = ref(false);
const loadingWeeklyPlan = ref(false);
const disableActionButton = ref(true);
const planTypeId = ref(null);
const authStore = useAuthStore();
const user = authStore.user;
const employeeId = user.employeeId ?? user.userId;

const isFullAccess = ref(true);
const showApprovePlanAction = ref(false);
const showCompletionDialog = ref(false);
const selectedCompletionPercentage = ref(0);
const selectedProjectPlanDateId = ref(null);

const props = defineProps({
  id: { type: String, default: "" },
  projectId: { type: String, default: "" },
  projectName: { type: String, default: "" },
  taskName: { type: String, default: "" }
});

// ----------------------------------------------------------------------------------------------------------------
// Define emits
// ----------------------------------------------------------------------------------------------------------------

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const model = ref({
  projectId: props?.projectId ? props.projectId : null,
  taskId: props?.id ?? null,
  planTypeId: null,
  weekDateLines: []
});

const getProjectWeeklyPlanType = async () => {
  const resp = await projectService.getProjectWeeklyPlanTypeId("Project Weekly Target Planning", "Weekly");
  planTypeId.value = resp;
};

const getWeeklyProjectPlanInDetail = async () => {
  loadingWeeklyPlan.value = true;
  try {
    const resp = await projectService.getWeeklyProjectPlanInDetail(
      model.value.projectId,
      planTypeId.value,
      0,
      4,
      null,
      model.value.taskId
    );
    const planDates = Array.isArray(resp) ? resp : resp?.data ?? [];
    isFullAccess.value = planDates[0]?.projectWeeklyPlan?.project?.currentUserManage ?? false;
    showApprovePlanAction.value = employeeId === planDates[0]?.projectWeeklyPlan?.project?.planApproverId;
    model.value.weekDateLines = planDates.flatMap(plan =>
      (plan.projectWeeklyPlanDatesLines ?? []).map(line => ({
        id: line.id,
        projectWeeklyPlanDatesId: line.projectWeeklyPlanDatesId ?? plan.id,
        taskId: line.taskId,
        weekDate: plan.weekDate,
        description: line.expectedDescription ?? "",
        expectedHours: line.expectedHours ?? "",
        actualDescription: line.actualDescription ?? "",
        completedBy: plan.completedBy ?? null,
        approvedBy: plan.approvedBy ?? null,
        approvedOnUtc: plan.approvedOnUtc ?? null,
        isApproved: plan.isApproved ?? false,
        isCompleted: plan.isCompleted ?? false,
        completionPercentage: plan.completionPercentage ?? 0,
        completedOnUtc: plan.completedOnUtc ?? null,

        // Expected Description audit details
        expectedDescriptionCreatedBy: line.expectedDescriptionCreatedBy ?? null,
        expectedDescriptionCreatedOnUtc: line.expectedDescriptionCreatedOnUtc ?? null,
        expectedDescriptionUpdatedBy: line.expectedDescriptionUpdatedBy ?? null,
        expectedDescriptionUpdatedOnUtc: line.expectedDescriptionUpdatedOnUtc ?? null,

        // Actual Description audit details
        actualDescriptionCreatedBy: line.actualDescriptionCreatedBy ?? null,
        actualDescriptionCreatedOnUtc: line.actualDescriptionCreatedOnUtc ?? null,
        actualDescriptionUpdatedBy: line.actualDescriptionUpdatedBy ?? null,
        actualDescriptionUpdatedOnUtc: line.actualDescriptionUpdatedOnUtc ?? null,

        // isFullAccess: plan.projectWeeklyPlan?.project?.currentUserManage ?? false,
        // showApprovePlanAction: employeeId === plan.projectWeeklyPlan?.project?.planApproverId,
        saveWeeklyLinesAssignTos:
          (line.projectWeeklyPlanDatesLinesAssignedTo ?? []).map(resource => ({
            id: resource.id,
            employeeId: resource.employeeId,
            firstName: resource.employee?.person?.firstName ?? "",
            lastName: resource.employee?.person?.lastName ?? "",
            estimatedHours: resource.estimatedHours ?? "",
            primaryEmailAddress: resource.employee?.person?.primaryEmailAddress ?? ""
          })),
        editing: false,
        flag: "Edit"
      }))
    );
  } catch (error) {
    console.error("Error loading weekly plan details:", error);
  } finally {
    loadingWeeklyPlan.value = false;
  }
};

const getResourceSummaryForWeekPlanById = async (line) => {
  const resp = await projectService.getResourceSummaryForWeekPlanById(planTypeId.value, line.projectWeeklyPlanDatesId);
  line.employeeEstimateHoursForWeekSummaryList = resp;
};

function getHoursMinutesText(value) {
  if (!value) return "";
  value = String(value).trim();
  // Hours only: 45 or 45:
  if (/^\d{1,2}:?$/.test(value)) {
    const hrs = parseInt(value, 10);

    return hrs > 0
      ? `${hrs} hr${hrs > 1 ? "s" : ""}`
      : "";
  }
  const match = value.match(/^(\d{1,2}):(\d{1,2})$/);

  if (!match) return "";

  const hrs = parseInt(match[1], 10);
  let mins = parseInt(match[2], 10);

  if (match[2].length === 1) {
    mins *= 10;
  }

  if (mins > 59 || (hrs === 0 && mins === 0)) {
    return "";
  }
  const result = [];
  if (hrs > 0) {
    result.push(`${hrs} hr${hrs > 1 ? "s" : ""}`);
  }
  if (mins > 0) {
    result.push(`${mins} min`);
  }
  return result.join(" ");
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Plan:- Add new project week.
// --------------------------------------------------------------------------------------------------------------------------------------------------
const today = new Date();

// // Navigation min month to block past month navigation
const minMonth = date.formatDate(today, "YYYY/MM");

// Only allow Sundays and today/future
const isSunday = (dateStr) => {
  const day = new Date(dateStr);
  const todayStart = new Date();
  todayStart.setHours(0, 0, 0, 0);

  return (
    day.getDay() === 0 // && day >= todayStart
  );
};

const getWeekDateOptions = (currentRow) => {
  const selectedDates = new Set(
    model.value.weekDateLines
      .filter(line => !line.deleted && line !== currentRow && line.weekDate)
      .map(line => new Date(line.weekDate).toDateString())
  );

  return (dateStr) =>
    isSunday(dateStr) &&
    !selectedDates.has(new Date(dateStr).toDateString());
};
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Plan Lines
// --------------------------------------------------------------------------------------------------------------------------------------------------
const onAddProjectPlanLines = () => {
  const lineModel = ref({
    id: null,
    weekDate: null,
    description: "",
    actualDescription: "",
    saveWeeklyLinesAssignTos: [],
    deleted: false
  });
  model.value.weekDateLines.unshift(lineModel.value);
};

const addWeeklyPlanDateLines = async (planDates) => {
  if (
    !planDates.description ||
    planDates.description.trim() === "" ||
    planDates.description.trim().toLowerCase() === "<br>"
  ) {
    notifyError({ message: "Please Enter Existing Plan" });
    return;
  }

  try {
    const newLine = {
      id: uid(),
      projectId: model.value.projectId,
      planTypeId: planTypeId.value,
      weekDate: planDates.weekDate,
      taskId: model.value.taskId,
      projectWeeklyPlanDatesId: planDates.id,
      expectedDescription: "",
      actualDescription: "",
      expectedHours: "00:00",
      isEditExpectedDescription: true
    };

    const resp = await projectService.saveProjectWeeklyPlanDatesLine(newLine);

    const mappedLine = {
      ...resp,
      isEditExpectedDescription: false,
      description: resp.expectedDescription || ""
    };

    model.value.weekDateLines.unshift(mappedLine);

  } catch (error) {
    console.error("Error adding weekly plan line:", error);
  }
};

const onDeleteProjectPlanLines = (lineIndex) => {
  $q.dialog({
    title: "Delete Line?",
    message: "Are you sure you want to delete weekly plan line?",
    cancel: true,
    persistent: true
  }).onOk(() => {
    if (model.value.weekDateLines.filter(line => line.deleted !== true).length > 1) {
      model.value.weekDateLines[lineIndex].deleted = true;
    } else {
      notifyError({ message: "Please add at least one row." });
    }
  });
};

const onSaveProjectPlan = async () => {
  processing.value = true;
  try {
    processing.value = true;
    model.value.planTypeId = planTypeId.value;
    await projectService.addUpdateProjectTaskWeeklyPlan(model.value);
  } catch (error) {
    console.error("Error while saving project to weekly plan:", error);
    notifyError({ message: "An error occurred while saving the project weekly plan." });
  } finally {
    setTimeout(() => {
      processing.value = false;
    }, 1500);
    onDialogOK();
  }
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
          notifySuccess({ message: `${isLock ? "Plan locked" : "Plan Unlocked"}` });
          getWeeklyProjectPlanInDetail();
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
      showCompletionDialog.value = false;
      notifySuccess({ message: "Weekly Plan Completed" });
      getWeeklyProjectPlanInDetail();
    } else {
      notifyError({ message: "Failed To Mark As Completed Plan" });
    }
  } else {
    notifyError({ message: "Something Went Wrong...." });
  }
};

function validateHours(value) {
  const strValue = String(value ?? "").trim();
  if (!strValue) {
    return true;
  }
  if (/^\d{1,2}$/.test(strValue)) {
    return true;
  }
  if (/^\d{1,2}:$/.test(strValue)) {
    return true;
  }
  const match = strValue.match(/^(\d{1,2}):(\d{1,2})$/);

  if (!match) {
    return "Invalid hours format.";
  }

  const minutes = Number(match[2]);
  if (minutes > 59) {
    return "Minutes can't exceed 59.";
  }

  return true;
}

function formatHoursValue(value) {
  if (!value) return value;
  value = String(value).trim();

  if (!value.includes(":")) {
    return `${value.padStart(2, "0")}:00`;
  }
  const [hours, minutes = "00"] = value.split(":");
  return `${hours.padStart(2, "0")}:${minutes.padEnd(2, "0")}`;
}

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Plan:- Add resource to line under project weeklines.
// --------------------------------------------------------------------------------------------------------------------------------------------------
const addResourceBtn = ref(true);
const selectedResourcesOptions = ref([]);

const resourceModel = ref({
  id: uid(),
  projectWeeklyPlanDatesLineId: "",
  employeeId: null,
  estimatedHours: "",
  firstName: "",
  lastName: ""
});

const resetResourceModel = () => {
  resourceModel.value = {
    id: uid(),
    projectWeeklyPlanDatesLineId: "",
    employeeId: null,
    estimatedHours: "",
    firstName: "",
    lastName: ""
  };
  selectedResourcesOptions.value = [];
};

// const checkIfEmployeeAlreadyExistsInPlanLine = async (existingResources, selectOption) => {
//   debugger;
//   const isEmployeeAdded = existingResources?.some(r => r.employeeId === selectOption.value);
//   if (isEmployeeAdded) {
//     resourceModel.value.employeeId = null;
//     resourceModel.value.firstName = "";
//     resourceModel.value.lastName = "";
//     selectedResourcesOptions.value = [];
//     notifyError({ message: "Cannot Add Duplicate Employee" });
//   } else {
//     const selectedEmployee = activeEmployeesDropdownSingleSelect.list.value.find(
//       (employee) => employee.value === selectOption
//     );

//     if (!selectedEmployee) {
//       notifyError({ message: "Employee not found" });
//       return;
//     }

//     const fullName = selectedEmployee.text?.trim?.() || "";
//     const nameParts = fullName.trim().split(" ");
//     resourceModel.value.employeeId = selectedResourcesOptions.value;
//     resourceModel.value.firstName = nameParts[0];
//     resourceModel.value.lastName = nameParts.length > 1 ? nameParts.slice(1).join(" ") : "";
//     checkAddResourceFields(resourceModel.value);
//   }
// };

const checkIfEmployeeAlreadyExistsInPlanLine = (existingResources, selectOption) => {
  const employeeId = selectOption?.value ?? selectOption;

  if (!employeeId) return;

  const isEmployeeAdded = (existingResources ?? []).some(
    resource => resource.employeeId === employeeId
  );

  if (isEmployeeAdded) {
    resourceModel.value.employeeId = null;
    resourceModel.value.firstName = "";
    resourceModel.value.lastName = "";
    selectedResourcesOptions.value = [];
    notifyError({ message: "Cannot Add Duplicate Employee" });
    return;
  }

  const selectedEmployee = weeklyEmployeeDropdownSingleSelect.list.value.find(
    employee => employee.value === employeeId
  );

  if (!selectedEmployee) return;

  const nameParts = (selectedEmployee.text?.trim() || "").split(/\s+/);

  resourceModel.value.employeeId = employeeId;
  resourceModel.value.firstName = nameParts[0] || "";
  resourceModel.value.lastName = nameParts.slice(1).join(" ");

  checkAddResourceFields(resourceModel.value);
};

const checkAddResourceFields = (resource) => {
  const hasMissingData =
    !resource?.employeeId ||
    !resource?.estimatedHours ||
    validateHours(resource.estimatedHours) !== true;
  addResourceBtn.value = hasMissingData;
};

// const isPositiveNumberOrDecimal = (value) => {
//   const regex = /^\d+(\.\d+)?$/;
//   return regex.test(String(value));
// };

// const onAddResourceToLine = async (line, resource) => {
//   debugger;
//   resourceProcessing.value = true;
//   try {
//     if (!line.saveWeeklyLinesAssignTos) {
//       line.saveWeeklyLinesAssignTos = [];
//     }
//     resource.projectWeeklyPlanDatesLineId = line.id;
//     line.saveWeeklyLinesAssignTos.push({ ...resource });
//     line.expectedHours = getTotalWeekLineExpectedHours(line);
//   } catch (error) {
//     console.error("Error in submitting the resource:", error);
//   } finally {
//     resourceProcessing.value = true;
//     setTimeout(() => {
//       resourceProcessing.value = false;
//     }, 1500);
//   }
// };

const onAddResourceToLine = async (line, resource) => {
  resourceProcessing.value = true;

  try {
    let newResource = { ...resource };

    if (line.id) {
      resource.projectWeeklyPlanDatesLineId = line.id;

      newResource = await projectService.addResourceToWeeklyPlanLine(resource);
    }

    line.saveWeeklyLinesAssignTos ??= [];
    line.saveWeeklyLinesAssignTos.push({
      ...newResource,
      firstName: newResource.firstName ?? newResource.employee?.person?.firstName ?? "",
      lastName: newResource.lastName ?? newResource.employee?.person?.lastName ?? ""
    });

    line.expectedHours = getTotalWeekLineExpectedHours(line);

    if (line.projectWeeklyPlanDatesId) {
      await getResourceSummaryForWeekPlanById(line);
    }
  } catch (error) {
    console.error("Error in submitting the resource:", error);
  } finally {
    resourceProcessing.value = false;
  }
};

// const onDeleteResourceToLine = (line, resource, resourceIndex) => {
//   $q.dialog({
//     title: "Remove Employee From Plan?",
//     message: "Are you sure you want to remove this employee from plan?",
//     cancel: true,
//     persistent: true
//   }).onOk(() => {
//     line.saveWeeklyLinesAssignTos[resourceIndex].deleted = true;
//     getTotalWeekLineExpectedHours(line);
//     notifyError({ message: "Resource Deleted" });
//   });
// };

const onDeleteResourceToLine = (line, resource, resourceIndex) => {
  $q.dialog({
    title: "Remove Employee From Plan?",
    message: "Are you sure you want to remove this employee from plan?",
    cancel: true,
    persistent: true
  }).onOk(async () => {
    if (resource.id) {
      // Existing saved resource: call delete API
      await projectService.deleteResourceToWeeklyPlanLine(resource.id);
      line.saveWeeklyLinesAssignTos.splice(resourceIndex, 1);
    } else {
      // New unsaved resource: remove only from UI
      line.saveWeeklyLinesAssignTos.splice(resourceIndex, 1);
    }

    line.expectedHours = getTotalWeekLineExpectedHours(line);
    notifyError({ message: "Resource Deleted" });
  });
};

const getTotalWeekLineExpectedHours = (line = {}) => {
  let totalMinutes = 0;

  line.saveWeeklyLinesAssignTos?.forEach(assignTo => {
    const value = String(assignTo.estimatedHours ?? '').trim();

    if (/^\d{1,2}:\d{2}$/.test(value)) {
      const [hours, minutes] = value.split(':').map(Number);
      totalMinutes += hours * 60 + minutes;
    }
  });

  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;

  return `${String(hours).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`;
};
// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { projectNameDropdownSingleSelect } = projectModule();
const { weeklyEmployeeDropdownSingleSelect } = projectTargetPlanModule();
const { activeEmployeesDropdownSingleSelect } = employeeModule();

// --------------------------------------------------------------------------------------------------------------------------------------------------
// Search: When user types in search
// --------------------------------------------------------------------------------------------------------------------------------------------------
watch(
  () => model.value,
  (newModelValue) => {
    const activeLines = newModelValue?.weekDateLines?.filter(
      line => !line.deleted
    ) ?? [];

    const hasMissingData =
      !newModelValue?.projectId?.length ||
      activeLines.some(line => !line?.weekDate || line.weekDate.trim() === "") ||
      activeLines.some(line => line?.expectedHours == null || line.expectedHours === "") ||
      activeLines.some(
        line =>
          !line?.description ||
          line.description.trim() === "" ||
          line.description.trim() === "<br>"
      );

    disableActionButton.value = hasMissingData;
  },
  { deep: true }
);

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On Page Load
// --------------------------------------------------------------------------------------------------------------------------------------------------

onMounted(async () => {
  projectNameDropdownSingleSelect.load();
  onAddProjectPlanLines();
  await getProjectWeeklyPlanType();
  getWeeklyProjectPlanInDetail();
  activeEmployeesDropdownSingleSelect.load(user.siteId);
});

</script>
<style scoped>
.TaskActivity .Person {
  border-radius: 10%;
  background-color: #5d5d5d;
  color: white;
  font-size: 12px;
  padding: 2px 3px;
}
.readonly-editor {
  border: 1px dashed #9e9e9e !important;
  background: #fdfcfc;
  opacity: 0.8;
  cursor: not-allowed;
}

.readonly-editor :deep(.q-editor__toolbar) {
  border-bottom: 1px dashed #9e9e9e !important;
}

.readonly-editor :deep(.q-editor__toolbar *) {
  pointer-events: none !important;
  cursor: not-allowed !important;
}
</style>
