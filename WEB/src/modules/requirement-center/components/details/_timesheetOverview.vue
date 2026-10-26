<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <div class="col scroll q-px-sm" style="overflow-y: auto; flex-grow: 1; height: 64vh; display: flex; flex-direction: column-reverse;">
      <q-timeline color="primary" class="timesheet-timeline">
        <q-timeline-entry
          v-for="item in allTimesheetDescriptions"
          :key="item.id"
          :title="`Task: ${ item.taskName }, Hours: ${ item.hours }`"
          :subtitle="`${formatDate(item.timesheetDate)} - ${item.employeeName}`"
          color="primary"
        >
          <div
            class="fs-14 note-row text-black note-text"
            v-html="item.description || ''"
          >
          </div>
        </q-timeline-entry>

        <div
          v-if="!loading && allTimesheetDescriptions.length === 0"
          class="text-center text-grey q-pa-md"
        >
          No Timesheet Details Available
        </div>

        <div v-if="loading" class="text-center q-pa-md">
          <q-spinner color="primary" size="2em" />
        </div>
      </q-timeline>
    </div>
  </q-card>
</template>

<script setup>
import { ref, watch } from "vue";
import timesheetService from "modules/timesheet/timesheet.service";

const props = defineProps({
  requirementId: {
    type: String,
    default: ""
  }
});

const loading = ref(false);
const allTimesheetDescriptions = ref([]);

const formatDate = (date) => {
  if (!date) return "";

  const parsedDate = new Date(date);

  if (Number.isNaN(parsedDate.getTime())) return "";

  return parsedDate.toLocaleDateString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric"
  });
};

const getTimesheetDescriptionsByRequirementId = async () => {
  if (!props.requirementId) {
    allTimesheetDescriptions.value = [];
    return;
  }

  loading.value = true;

  try {
    const resp =
      await timesheetService.getTimesheetDescriptionsByRequirementId(
        props.requirementId,
        false
      );

    // Adjust this line if your service returns a different response shape.
    const timesheets = Array.isArray(resp)
      ? resp
      : resp?.data ?? resp?.timesheets ?? [];

    const descriptions = [];

    timesheets.forEach((timesheet) => {
      const lines = timesheet.timesheetLines ?? [];

      lines.forEach((line) => {
        const description = line.description?.trim();

        // Skip empty descriptions.
        if (!description) return;

        descriptions.push({
          id: line.id,
          timesheetDate: timesheet.timesheetDate,
          employeeName:
            timesheet.user?.person?.fullName ?? "Unknown Employee",
          description,
          hours: line.hours,
          taskName: line.task?.name ?? ""
        });
      });
    });

    // Newest timesheet date first.
    descriptions.sort(
      (a, b) =>
        new Date(b.timesheetDate).getTime() -
        new Date(a.timesheetDate).getTime()
    );

    allTimesheetDescriptions.value = descriptions;
  } catch (error) {
    console.error(
      "Error while loading timesheet descriptions:",
      error
    );

    allTimesheetDescriptions.value = [];
  } finally {
    loading.value = false;
  }
};

watch(
  () => props.requirementId,
  () => getTimesheetDescriptionsByRequirementId(),
  { immediate: true }
);
</script>

<style scoped>
.timesheet-timeline :deep(.q-timeline__dot) {
  color: #1b75ab !important;
}

.timesheet-timeline :deep(.q-timeline__title) {
  color: #1b75ab !important;
}

/* .timesheet-timeline :deep(.q-timeline__subtitle) {
  color: #1b75ab !important;
} */

.timesheet-timeline :deep(.q-timeline__entry:before) {
  background: #1b75ab !important;
}

/* Description styling */
.note-row {
  display: flex;
  align-items: center;
  gap: 6px;
}

.note-text {
  display: inline-block;
}
</style>
