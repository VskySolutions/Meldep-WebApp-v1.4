<template>
  <fieldset>
    <div v-if="!props.weekEndDate" class="q-mb-sm q-gutter-sm flex justify-end">
      <q-input v-model="filterWeeklyProjectPlan" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search" dense clearable>
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </div>
    <div>
      <table
        class="WhiteTable full-width"
        :loading="loading" :filter="filterWeeklyProjectPlan"
      >
        <thead style="position: sticky; top: 55px; z-index: 9;">
          <tr class="text-center">
            <th style="font-size: 15px;">Weekend</th>
            <th width="43%">Planned</th>
            <th width="43%">Actual</th>
          </tr>
        </thead>
        <tbody
          v-for="planDate in filteredWeeklyProjectPlan"
          :key="planDate.id"
          class="weeklyDate text-grey-9"
        >
          <tr
            v-for="(line, lineIndex) in planDate.projectWeeklyPlanDatesLines"
            :key="line.id"
          >
            <td
              v-if="lineIndex === 0"
              :rowspan="planDate.projectWeeklyPlanDatesLines.length"
              class="text-center"
            >
              {{ planDate.weekDate }}
              <hr>
              <div>
                <h6>Total:- <b style="color: #1b75ab;">{{ getTotalExpectedHours(planDate.projectWeeklyPlanDatesLines) }}</b></h6>
              </div>
              <hr>
            </td>
            <td>
              <div class="row flex">
                <div class="col-12 position-relative RichTextEditor">
                  <div
                    class="plan-desc q-mt-md"
                    v-html="line.expectedDescription"
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
              </div>
            <!-- </template> -->
            </td>
            <td>
              <div v-if="line.actualDescription?.length > 0" class="plan-desc q-mt-md RichTextEditor" v-html="line.actualDescription" />
              <div v-if="(line.actualDescription?.length === 0 || line.actualDescription === '<br>')" class="fs-12 text-red">No Description Available</div>
              <template v-if="line.actualDescriptionUpdatedOnUtc">
                <div class="LastUpdatedBy text-capitalize text-end">
                  Last Updated:- {{ line?.actualDescriptionUpdatedBy?.person?.firstName + ' '+ line?.actualDescriptionUpdatedBy?.person?.lastName + '('+ line?.actualDescriptionUpdatedOnUtc +')' }}
                </div>
              </template>
            </td>
          </tr>
          <tr v-if="planDate?.projectWeeklyPlanDatesLines.length === 0">
            <td class="text-center">
              {{ planDate.weekDate }}
            </td>
            <td colspan="3" class="text-red">
              <div class="flex justify-center items-center">
                No Plan Available
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
                    <div @click="onTaskView(item.taskId)">
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
        <tbody v-if="filteredWeeklyProjectPlan?.length === 0">
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
  </fieldset>
</template>

<script setup>
import projectService from "modules/project/projects.service";
import { ref, onMounted, computed } from "vue";
import { notifyWarning } from "assets/utils";
import useFilters from "composables/useFilters";

const { toDate } = useFilters();
const props = defineProps({ projectId: { type: String, default: "" }, weekEndDate: { type: String, default: "" } });
const projectId = props.projectId;
const weekEndDate = toDate(props.weekEndDate);

const loading = ref(true);
const disableLoadMore = ref(false);
const skipIndex = ref(0);
const takeCount = ref(4);
const planTypeId = ref(null);
const projectWeeklyPlanDates = ref([]);
const filterWeeklyProjectPlan = ref("");

const columns = ref([
  { name: "Weekend", label: "Weekend Date", field: "weekDate", align: "left", sortable: true },
  { name: "expected", label: "Planned", field: "projectWeeklyPlanDatesLines[0].expectedDescription", align: "left", sortable: true },
  { name: "actual", label: "Actual", field: "projectWeeklyPlanDatesLines[0].actualDescription", align: "left", sortable: true },
  { name: "createdBy", label: "Created By", field: "createdBy.person.fullName", align: "left", sortable: false }
]);

const getProjectWeeklyPlanType = async () => {
  const resp = await projectService.getProjectWeeklyPlanTypeId("Project Weekly Target Planning", "Weekly");
  planTypeId.value = resp;
  // getProjectWeeklyTargetPlans();
};

// for weekly project plans
const getProjectWeeklyTargetPlans = () => {
  loading.value = true;
  projectService.getWeeklyProjectPlanInDetail(projectId, planTypeId.value, 0, takeCount.value, weekEndDate).then((resp) => {
    projectWeeklyPlanDates.value = resp;
  }).finally(() => {
    loading.value = false;
  });
};

const getTotalExpectedHours = (lines = []) => {
  return lines.reduce((total, line) => {
    const hours = parseFloat(line.expectedHours);
    return total + (isNaN(hours) ? 0 : hours);
  }, 0).toFixed(2); // Optional: format to 2 decimal places
};

// for static search
const filterRows = (data, searchTerm, columns) => {
  if (!searchTerm) return data;
  const lowerTerm = searchTerm.toLowerCase();

  return data.filter(row => {
    return columns.some(column => {
      const field = column.field;

      if (field.includes("projectWeeklyPlanDatesLines")) {
        const lines = row.projectWeeklyPlanDatesLines || [];
        return lines.some(line =>
          ["expectedDescription", "actualDescription"].some(key =>
            (line[key] || "").toLowerCase().includes(lowerTerm)
          )
        );
      }

      const value = field.split(".").reduce((obj, key) => obj?.[key], row);
      return String(value || "").toLowerCase().includes(lowerTerm);
    });
  });
};

const filteredWeeklyProjectPlan = computed(() =>
  filterRows(projectWeeklyPlanDates.value, filterWeeklyProjectPlan.value, columns.value)
);

const filterRequestMapping = (mappings, type) => {
  if (type.toLowerCase() === "requirement") {
    const data = (mappings || []).filter(m => m.requirementId);
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
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Plan:- Add new lines under project week.
// --------------------------------------------------------------------------------------------------------------------------------------------------

const loadProjectWeeklyPlanLines = async (index) => {
  skipIndex.value = index + 1;
  const resp = await projectService.getWeeklyProjectPlanInDetail(projectId, planTypeId.value, skipIndex.value, takeCount.value, weekEndDate);
  projectWeeklyPlanDates.value.push(...resp);
  if (resp?.length === 0) {
    notifyWarning({ message: "No Week Available" });
  }
  if (resp.length < takeCount.value) {
    disableLoadMore.value = true;
  }
  loading.value = false;
};

// On page rendering
onMounted(async () => {
  await getProjectWeeklyPlanType();
  getProjectWeeklyTargetPlans();
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
</style>
