<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <div class="row q-col-gutter-lg q-mt-sm">
      <div class="col-10 q-ml-md">
        <!-- <div class="text-caption text-grey q-mb-sm">Description</div> -->
          <q-table
            v-if="changeLogRows && changeLogRows.length > 0"
            ref="tableRef"
            v-model:pagination="changeLogPagination"
            bordered
            class="no-shadow"
            :loading="loading"
            :rows="changeLogRows"
            :columns="chnageLogColumns"
            row-key="id"
            separator="cell"
            no-data-label="No data available"
            binary-state-sort
            :rows-per-page-options="[20, 50, 100, 200, 500]"
          >
            <template #header="props">
              <q-tr :props="props" class="bg-primary text-white">
                <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
              </q-tr>
            </template>

            <template #body="props">
              <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                <q-td>
                  <div class="row items-center">
                    <div>{{ props.row.createdOnUtc }}</div>
                  </div>
                </q-td>
                <q-td>{{ props.row.employee.person.fullName }}</q-td>
                <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;">{{ props.row.requirementName }}</q-td>
                <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 40%;"><div class="RichTextEditor" v-html="props.row.description" /></q-td>
              </q-tr>
            </template>
          </q-table>
          <div class="text-black RichTextEditor">
            <span v-html="model.description" />
          </div>
      </div>
    </div>
  </q-card>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import _ from "lodash";
import requirementService from "modules/requirement/requirement.service";

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

const loading = ref(false);
const changeLogRows = ref([]);
const changeLogPagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const chnageLogColumns = [
  { name: "requirementLogDate", label: "Change Date", field: "requirementLogDate", align: "left", sortable: true },
  { name: "employee.person.fullName", label: "Changed By", field: "employee.person.fullName", align: "left", sortable: true },
  { name: "requirementName", label: "Requirement", field: "requirementName", align: "left", sortable: true },
  { name: "description", label: "Description", field: "description", align: "left", sortable: false }
];


const model = ref({
  description: {}
});


// get get Requirement on edit mode
const getRequirement = () => {
  loading.value = true;
  requirementService.getRequirementDetails(props.requirementId).then((resp) => {
    model.value = _.cloneDeep(resp);

    changeLogRows.value = resp.requirementChangeLog.map(item => ({
      ...item,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

watch(
  () => props.requirementId,
  async () => {
    await getRequirement();
  },
  {
    immediate: true
  }
);

onMounted(async () => {
  await getRequirement();
});
</script>
