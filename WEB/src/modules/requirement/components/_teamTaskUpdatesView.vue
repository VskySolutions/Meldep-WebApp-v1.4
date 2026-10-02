<template>
  <div
    class="col scroll"
    style="overflow-y: auto; flex-grow: 1; display: flex; flex-direction: column;"
  >
    <div
      v-if="taskGroups.length"
      class="q-pa-sm"
    >
      <!-- Task -->
      <q-card
        v-for="task in taskGroups"
        :key="task.taskId"
        flat
        bordered
        class="q-mb-md"
      >
        <!-- Task Header -->
        <q-card-section class="q-pb-sm">
          <div class="row items-center no-wrap">
            <q-icon
              name="o_task"
              size="20px"
              class="q-mr-sm"
            />

            <div class="col">
              <div class="text-subtitle1 text-weight-medium">
                {{ task.taskName }}
              </div>
            </div>
          </div>
        </q-card-section>

        <q-separator />

        <!-- Task Notes -->
        <q-card-section
          v-if="task.taskNotes.length"
          class="q-py-sm"
        >
          <div class="text-caption text-weight-medium text-grey-8 q-mb-sm">
            Task Updates
          </div>

          <q-timeline
            color="secondary"
            class="q-mt-none"
          >
            <q-timeline-entry
              v-for="note in task.taskNotes"
              :key="note.id"
              :subtitle="`${note.createdOnUtc} - ${note.user?.person?.fullName || ''}`"
              icon="o_note_alt"
              color="primary"
            >
              <div
                class="text-black note-text"
                v-html="note.note || ''"
              />
            </q-timeline-entry>
          </q-timeline>
        </q-card-section>

        <!-- Activities -->
        <q-card-section
          v-if="task.activities.length"
          class="q-pt-sm"
        >
          <div class="text-caption text-weight-medium text-grey-8 q-mb-sm">
            Activities Updates
          </div>

          <div
            v-for="activity in task.activities"
            :key="activity.activityId"
            class="q-mb-md"
          >
            <!-- Activity Notes -->
            <q-timeline
              v-if="activity.notes.length"
              color="secondary"
              class="q-ml-md q-mt-none"
            >
              <q-timeline-entry
                v-for="note in activity.notes"
                :key="note.id"
                :subtitle="
                  `${note.createdOnUtc} - ${
                    note.user?.person?.fullName || ''
                  }`
                "
                icon="o_note_alt"
                color="grey-7"
              >
                <div
                  class="text-black note-text"
                  v-html="note.note || ''"
                />
              </q-timeline-entry>
            </q-timeline>
          </div>
        </q-card-section>
      </q-card>
    </div>

    <div v-else>
      <h5 class="text-center text-grey">
        No Work & Progress Updates Available
      </h5>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from "vue";
import _ from "lodash";
import requirementService from "src/modules/requirement/requirement.service";

const props = defineProps({
  id: {
    type: String,
    default: ""
  }
});

const loading = ref(true);
const allResponseLogDescriptions = ref([]);

const getRequirementWorkProgressNotes = () => {
  if (!props.id) return;

  loading.value = true;

  requirementService
    .getRequirementWorkProgressNotes(props.id)
    .then((resp) => {
      allResponseLogDescriptions.value = _.cloneDeep(resp || []);
    })
    .finally(() => {
      loading.value = false;
    });
};

const taskGroups = computed(() => {
  const tasks = {};

  allResponseLogDescriptions.value.forEach((note) => {
    const taskId = note.taskId || note.recordId;

    if (!tasks[taskId]) {
      tasks[taskId] = {
        taskId,
        taskName: note.taskName || note.recordName,
        taskUser:
          note.noteType === "Task"
            ? note.user?.person?.fullName
            : null,
        taskNotes: [],
        activities: {}
      };
    }

    // Task Note
    if (note.noteType === "Task") {
      tasks[taskId].taskNotes.push(note);

      if (!tasks[taskId].taskUser) {
        tasks[taskId].taskUser =
          note.user?.person?.fullName || null;
      }
    }

    // Activity Note
    if (note.noteType === "Activity") {
      const activityId = note.activityId || note.recordId;

      if (!tasks[taskId].activities[activityId]) {
        tasks[taskId].activities[activityId] = {
          activityId,
          activityName: note.activityName || note.recordName,
          notes: []
        };
      }

      tasks[taskId].activities[activityId].notes.push(note);
    }
  });

  return Object.values(tasks).map((task) => ({
    ...task,
    activities: Object.values(task.activities)
  }));
});

watch(
  () => props.id,
  (newId) => {
    if (newId) {
      getRequirementWorkProgressNotes();
    }
  },
  { immediate: true }
);
</script>

<style scoped>
:deep(.q-timeline__content) {
  padding-bottom: 0 !important;
}

.note-text {
  word-break: break-word;
}

.q-card {
  border-radius: 8px;
}
</style>
