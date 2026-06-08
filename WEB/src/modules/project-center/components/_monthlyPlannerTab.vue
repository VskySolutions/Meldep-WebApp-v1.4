<template>
  <fieldset>
    <div class="q-mb-sm q-gutter-sm flex justify-end">
      <q-input v-model="filterMonthlyProjectPlan" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search" dense clearable>
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </div>
    <div>
      <table class="WhiteTable full-width" :filter="filterMonthlyProjectPlan">
        <thead style="position: sticky; top: 55px; z-index: 9;">
          <tr class="text-center">
            <th style="font-size: 15px;">Month</th>
            <th width="43%">Planned</th>
            <th width="43%">Actual</th>
          </tr>
        </thead>
        <tbody
          v-for="planDate in filteredMonthlyProjectPlan"
          :key="planDate.id"
          class="MonthlyDate text-grey-9"
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
              {{ convertDateToMonthLabel(planDate.weekDate) }}
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
              </div>
              <div v-if="line.expectedDescriptionUpdatedOnUtc" class="col-12">
                <div class="row q-py-xs" style="border-top: 1px solid #cfcfcf;">
                  <div class="col-xxl-5 col-xl-6 col-lg-12 col-md-12 col-sm-12 col-xs-12 flex flex">
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
                    <div class="LastUpdatedBy text-capitalize text-end">
                      Last Updated:- {{ line?.expectedDescriptionUpdatedBy?.person?.firstName + ' '+ line?.expectedDescriptionUpdatedBy?.person?.lastName + '('+ line?.expectedDescriptionUpdatedOnUtc +')' }}
                    </div>
                  </div>
                </div>
              </div>
            </td>
            <td>
              <div v-if="line.actualDescription?.length > 0" class="plan-desc q-mt-sm RichTextEditor" v-html="line.actualDescription" />
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
              {{ convertDateToMonthLabel(planDate.weekDate) }}
            </td>
            <td colspan="3" class="text-red">
              <div class="flex justify-center items-center">
                No Plan Available
              </div>
            </td>
          </tr>
          <tr style="background-color: rgb(231 231 231);">
            <td colspan="4" />
          </tr>
        </tbody>
        <tbody v-if="filteredMonthlyProjectPlan?.length === 0">
          <tr>
            <td colspan="4">
              <h5 class="text-center text-red">No Plans Available</h5>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div v-if="projectMonthlyPlanDates?.length > 3" class="text-center q-pa-sm">
      <q-btn
        :disable="disableLoadMore"
        icon-right="o_refresh"
        color="secondary"
        outlined
        size="sm"
        @click="loadProjectMonthlyPlanLines(skipIndex)"
      >
        Load More
      </q-btn>
    </div>
  </fieldset>
</template>

<script setup>
import projectService from "modules/project/projects.service";
import { ref, onMounted, computed } from "vue";
import { date } from "quasar";
import { notifyWarning } from "assets/utils";

const props = defineProps({ projectId: { type: String, default: "" } });
const projectId = props.projectId;

const loading = ref(true);
const disableLoadMore = ref(false);
const skipIndex = ref(0);
const takeCount = ref(4);
const planTypeId = ref(null);
const projectMonthlyPlanDates = ref([]);
const filterMonthlyProjectPlan = ref("");
const columns = ref([
  { name: "Weekend", label: "Weekend Date", field: "weekDateLabel", align: "left", sortable: true },
  { name: "expected", label: "Planned", field: "projectWeeklyPlanDatesLines[0].expectedDescription", align: "left", sortable: true },
  { name: "actual", label: "Actual", field: "projectWeeklyPlanDatesLines[0].actualDescription", align: "left", sortable: true },
  { name: "createdBy", label: "Created By", field: "createdBy.person.fullName", align: "left", sortable: false }
]);
const getProjectmonthlyPlanType = async () => {
  const resp = await projectService.getProjectWeeklyPlanTypeId("Project Weekly Target Planning", "Monthly");
  planTypeId.value = resp;
  getProjectMonthlyTargetPlans();
};

// for monthly project plans
const getProjectMonthlyTargetPlans = (propsmonthlyProjectPlans) => {
  loading.value = true;
  projectService.getWeeklyProjectPlanInDetail(projectId, planTypeId.value, 0, takeCount.value).then((resp) => {
    resp.forEach(item => {
      item.weekDateLabel = convertDateToMonthLabel(item.weekDate);
    });
    projectMonthlyPlanDates.value = resp;
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

// Convert Date mm/dd/yyyy to MMMM/YYYY
const convertDateToMonthLabel = (currentMonth) => {
  return date.formatDate(currentMonth, "MMMM-YYYY");
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

const filteredMonthlyProjectPlan = computed(() =>
  filterRows(projectMonthlyPlanDates.value, filterMonthlyProjectPlan.value, columns.value)
);
// --------------------------------------------------------------------------------------------------------------------------------------------------
// Project Plan:- Add new lines under project month.
// --------------------------------------------------------------------------------------------------------------------------------------------------

const loadProjectMonthlyPlanLines = async (index) => {
  skipIndex.value = index + 1;
  const resp = await projectService.getWeeklyProjectPlanInDetail(projectId, planTypeId.value, skipIndex.value, takeCount.value);
  projectMonthlyPlanDates.value.push(...resp);
  if (resp?.length === 0) {
    notifyWarning({ message: "No Month " });
  }
  if (resp.length < takeCount.value) {
    disableLoadMore.value = true;
  }
  loading.value = false;
};

// On page rendering
onMounted(async () => {
  await getProjectmonthlyPlanType();
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
