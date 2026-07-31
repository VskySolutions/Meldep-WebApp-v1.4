<template>
  <q-card flat bordered>
    <q-separator />
    <q-card-section>
      <q-input
        v-model="search"
        dense
        outlined
        placeholder="Search test case..."
        clearable
      >
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </q-card-section>
    <q-separator />
    <q-table
      ref="tableRef"
      v-model:pagination="pagination"
      :class="(filteredTestCases.length > 0 ? 'my-sticky-header-table' : '') + 'Custom-DataTable TicketTable'"
      :loading="loading"
      :rows="filteredTestCases"
      :columns="columns"
      row-key="id"
      separator="cell"
      no-data-label="No data available"
      binary-state-sort
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      :filter="searchText"
      style="height: 100vh;"
      @request="getAllHelpDesks"
    >
      <template #loading>
        <q-inner-loading showing color="primary">
          <q-spinner size="40px" />
        </q-inner-loading>
      </template>
      <template #header="props">
        <q-tr :props="props" class="bg-primary text-white">
          <q-th v-for="col in props.cols" :key="col.name" :props="props">
            {{ col.label }}
          </q-th>
        </q-tr>
      </template>
      <template #body="props">
        <q-tr
          :props="props"
          class="cursor-pointer"
          :class="{ 'bg-blue-1': selectedTestCase === props.row.id }"
          @click="
            selectedTestCase = props.row.id;
            emit('select', props.row);
          "
        >
          <q-td class="q-pa-none">
            <div class="column q-gutter-xs" style="white-space: normal; word-break: break-word;">
                <div class="row items-center justify-between">
                  <div class="text-caption text-weight-bold text-black">
                    #{{ props.row.testCaseNumber }}
                  </div>
                  <div>
                    <q-badge
                      rounded
                      class="q-mr-xs"
                      :style="{
                        backgroundColor: props.row.statusBgColor,
                        color: props.row.statusTextColor
                      }"
                    >
                      {{ props.row.statusName }}
                      <q-tooltip>Status</q-tooltip>
                    </q-badge>
                  </div>
                </div>
              <div class="text-black fs-14">
                {{ props.row.name }}
              </div>

              <div class="text-caption text-grey-7">
                {{ props.row.projectName }}
              </div>

              <div class="text-caption text-grey-7 q-mt-xs">
                <strong>Tested By:</strong>
                {{ props.row.testedBy }}
                •
                {{ props.row.testedDate }}
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
import { computed, ref, watch } from 'vue';
import { format } from 'date-fns';
import requirementCenterService from "src/modules/requirement-center/requirementCenter.service";
const emit = defineEmits(['select']);
const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

const search = ref('');
const selectedTestCase = ref(null);
const testCases = ref([]);
const loading = ref(true);

const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = [{ name: "testcase", label: "Test Cases", align: "left", field: "name", sortable: true }];

const getTestCasesByRequirementId = async (requirementId) => {
  if (!requirementId) return;

  try {
    loading.value = true;

    const resp = await requirementCenterService.getTestCasesByRequirementId(requirementId);

    testCases.value = resp.map(item => ({
      ...item,
      testPlanId: item.planId ?? null,
      owner: item.testedByEmployee?.person?.fullName ?? '-',
      projectName: item.project?.name ?? '-',
      testedBy: item.testedByEmployee?.person?.fullName ?? '-',
      testedDate: format(item.testedDate, "MM/dd/yyyy") ?? '-',
      statusName: item.status?.dropDownValue ?? '-',
      statusTextColor: item.status?.color ?? '#000',
      statusBgColor: item.status?.bgColor ?? '#e0e0e0'
    }));

    if (testCases.value.length) {
      selectedTestCase.value = testCases.value[0].id;
      emit('select', testCases.value[0]);
    }

  } catch (err) {
    console.error(err);
    notifyError({ message: 'Failed to load Test Cases' });
  } finally {
    loading.value = false;
  }
};

const filteredTestCases = computed(() => {
  if (!search.value) return testCases.value

  return testCases.value.filter(testCase =>
    JSON.stringify(testCase)
      .toLowerCase()
      .includes(search.value.toLowerCase())
  )
})

// --------------------------------------------------------------------------------------------------------------------------------------------------
// On load
// --------------------------------------------------------------------------------------------------------------------------------------------------

watch(
  () => props.requirementId,
    async (id) => {
      await getTestCasesByRequirementId(id);
    },
  {
    immediate: true
  }
);
</script>

<style scoped>
.q-item--active {
  border-left: 4px solid #1976d2;
}
</style>
