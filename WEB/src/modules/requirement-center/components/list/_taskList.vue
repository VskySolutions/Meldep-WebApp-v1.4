<template>
  <q-card flat bordered>
    <q-separator />

    <q-card-section>
      <q-input
        v-model="search"
        dense
        outlined
        placeholder="Search task..."
        clearable
      >
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </q-card-section>

    <q-separator />

    <q-table
      v-model:pagination="pagination"
      flat
      :loading="loading"
      :rows="filteredTasks"
      :columns="columns"
      row-key="id"
      separator="cell"
      binary-state-sort
      no-data-label="No data available"
      :rows-per-page-options="[20, 50, 100, 200]"
      style="height: calc(100vh - 220px)"
    >
      <template #loading>
        <q-inner-loading showing color="primary">
          <q-spinner size="40px" />
        </q-inner-loading>
      </template>

      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th
            v-for="col in props.cols"
            :key="col.name"
            :props="props"
          >
            {{ col.label }}
          </q-th>
        </q-tr>
      </template>

      <template #body="props">
        <q-tr
          :props="props"
          class="cursor-pointer"
          :class="{ 'bg-blue-1': selectedTask === props.row.id }"
          @click="
            selectedTask = props.row.id;
            emit('select', props.row);
          "
        >
          <q-td class="q-pa-none">
            <div class="column q-gutter-xs" style="white-space: normal; word-break: break-word;">
              <div class="row items-center justify-between">
                <div class="text-caption text-weight-bold text-black">
                  #{{ props.row.projectTaskNumber }}
                </div>
                <div>
                  <q-badge
                    rounded
                    class="q-mr-xs"
                    :style="{
                      backgroundColor: props.row.priorityBgColor,
                      color: props.row.priorityTextColor
                    }"
                  >
                    {{ props.row.priorityName }}
                    <q-tooltip>Priority</q-tooltip>
                  </q-badge>
                  <q-badge
                    rounded
                    :style="{
                      backgroundColor: props.row.statusBgColor,
                      color: props.row.statusTextColor
                    }"
                  >
                    {{ props.row.statusName }}
                  </q-badge>
                </div>
              </div>

              <div class="text-black text-weight-medium">
                {{ props.row.name }}
              </div>

              <div class="text-caption text-grey-7">
                {{ props.row.projectName }}
              </div>

              <div class="text-caption text-grey-7">
                <strong>Assigned To:</strong>
                {{ props.row.owner }}
                •
                {{ props.row.dueDate }}
              </div>
            </div>
          </q-td>
        </q-tr>
        <q-separator />
      </template>

    </q-table>
  </q-card>
</template>

<script setup>
import { computed, ref, watch } from "vue";

import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

const emit = defineEmits(["select"]);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

const loading = ref(false);
const search = ref("");
const selectedTask = ref(null);
const tasks = ref([]);

const pagination = ref({
  page: 1,
  rowsPerPage: 20,
  sortBy: "updatedOnUtc",
  descending: true
});

const columns = [
  {
    name: "task",
    label: "Tasks",
    field: "title",
    align: "left"
  }
];

const getTasksByRequirementId = async (requirementId) => {
  if (!requirementId) return;

  try {
    loading.value = true;

    const resp = await requirementCenterService.getTasksByRequirementId(requirementId);
    tasks.value = resp.map(item => ({
      ...item,
      owner: item.assignedTo?.person?.fullName ?? '-',
      projectName: item.project?.name ?? "-",
      statusName: item.status?.dropDownValue ?? '-',
      dueDate: item.endDate ?? '-',
      statusTextColor: item.status?.color ?? '-',
      statusBgColor: item.status?.bgColor ?? '-',
      priorityName: item.priority?.dropDownValue ?? '-',
      priorityTextColor: item.priority?.color ?? '#000',
      priorityBgColor: item.priority?.bgColor ?? '#e0e0e0'
    }));

    if (tasks.value.length) {
      selectedTask.value = tasks.value[0].id;
      emit("select", tasks.value[0]);
    }
  } catch (err) {
    console.error(err);
    notifyError({ message: "Failed to load Tasks" });
  } finally {
    loading.value = false;
  }
};

const filteredTasks = computed(() => {
  if (!search.value) return tasks.value;

  return tasks.value.filter(task =>
    JSON.stringify(task)
      .toLowerCase()
      .includes(search.value.toLowerCase())
  );
});


// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
    async (id) => {
      await getTasksByRequirementId(id);
    },
  {
    immediate: true
  }
);
</script>
