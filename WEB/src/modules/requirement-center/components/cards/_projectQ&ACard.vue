<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <!-- Header -->
    <q-card-section class="row items-center justify-between q-pb-sm">
      <div class="row items-center">
        <q-avatar
          rounded
          color="blue-1"
          text-color="primary"
          icon="o_task_alt"
          size="36px"
        />

        <div class="q-ml-md">
          <div class="row items-center">
            <div class="text-subtitle1 text-weight-bold">
              Project Questions Answers
            </div>

            <q-badge
              color="grey-3"
              text-color="grey-8"
              rounded
              class="q-ml-sm"
            >
              {{ rows.length }}
            </q-badge>
          </div>
        </div>
      </div>

      <div class="row items-center q-gutter-sm">
        <q-btn
          v-if="projectId"
          icon="o_open_in_new"
          size="sm"
          outline
          class="text-primary"
          style="padding: 3px 7px; min-height: 30px;"
          @click="$router.push({ path: '/project-questions-answers/list',
          state: {
            projectId: projectId,
            requirementId: props.requirementId
          }})"
        >
          <q-tooltip>Open Project Questions Answers</q-tooltip>
        </q-btn>
      </div>
    </q-card-section>
    <q-separator />

    <!-- Table -->
    <q-table
      flat
      :rows="rows"
      :columns="columns"
      :loading="loading"
      v-model:pagination="pagination"
      row-key="id"
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      class="req-dashboard-table"
    >
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
        <q-tr :props="props">

          <q-td style="width:40%;">
            {{ props.row.title }}
          </q-td>

          <q-td style="width:60%;">
            <p v-html="props.row.lastAnswer ? props.row.lastAnswer : props.row.description" />
          </q-td>

        </q-tr>
      </template>
    </q-table>
  </q-card>
</template>
<script setup>
import { ref, watch } from 'vue';
import { notifyError } from "assets/utils";

import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";

const emit = defineEmits(['summary']);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

const loading = ref(false);
const rows = ref([]);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = [
  { name: "title", label: "QUESTION", field: "title", align: "left", sortable: true },
  { name: "description", label: "ANSWER", field: "description", align: "left", sortable: true }
]

const projectId = ref('');
const getProjectQAByRequirementId = async ({ pagination: p }) => {
  const { page, rowsPerPage, sortBy, descending } = p;
  const payload = {
    requirementId: props.requirementId,
    sortBy: sortBy,
    descending: descending,
    page: page,
    pageSize: rowsPerPage
  };

  try {
    loading.value = true;
    const resp = await requirementCenterService.getProjectQAByRequirementId(payload);
    rows.value = resp.projectQuestionsAnswerList || [];
    console.log("rows.value", rows.value);
    if (rows.value.length > 0) {
      projectId.value = rows.value[0].project?.id || '';
    } else {
      projectId.value = '';
    }

    emit('summary', {
      total: rows.value.length
    });
  } catch (err) {
    console.error(err);
    notifyError({ message: "Failed to load Project questions answers" });
  } finally {
    loading.value = false;
  }
};

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
  async () => {
    await getProjectQAByRequirementId({
      pagination: pagination.value
    });
  },
  { immediate: true }
);
</script>
