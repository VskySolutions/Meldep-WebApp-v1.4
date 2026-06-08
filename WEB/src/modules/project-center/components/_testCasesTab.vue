<template>
  <fieldset>
    <legend>Project Test Cases</legend>
    <div class="q-mb-sm q-gutter-sm flex justify-end">
      <q-input v-model="filterTestCase" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search" dense clearable>
        <template #prepend>
          <q-icon name="o_search" />
        </template>
      </q-input>
    </div>
    <q-table
      ref="tableRef6" v-model:pagination="paginationTestCases" :class="rowsTestCases.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable my-sticky-header-table'" :loading="loading" :rows="filteredTestCase" :columns="columnsTestCases" row-key="id" separator="cell"
      no-data-label="No data available" binary-state-sort :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getAllTestCase"
    >
      <template #header="propsTestCase">
        <q-tr :props="propsTestCase" class="bg-primary text-white">
          <q-th v-for="col in propsTestCase.cols" :key="col.name" :props="propsTestCase">{{ col.label }}</q-th>
        <!-- <q-th auto-width class="text-center">Actions</q-th> -->
        </q-tr>
      </template>
      <template #body="propsTestCase">
        <q-tr :props="propsTestCase" :class="activeRowIdTestCases == propsTestCase.row.id ? 'highlight' : ''" :set="(preProjectName = null, preTestPlanName = null)">
          <!-- <q-td auto-width class="text-center hidden">
          <q-icon :name="isExpanded(propsTestCase.row.id) ? '-' : '+'" class="cursor-pointer custom-plus-minus-icon" @click="toggleExpand(propsTestCase.row.id)">
            <q-tooltip>{{ isExpanded(propsTestCase.row.id) ? 'Collapse' : 'Expand' }}</q-tooltip>
          </q-icon>
        </q-td> -->
          <q-td style="width: 3%;" class="hidden">{{ propsTestCase.row.testCaseNumber }}</q-td>
          <!-- <q-td>{{ propsTestCase.row.project.name }}</q-td> -->
          <!-- <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;"><span v-if="preProjectName !== propsTestCase.row.project.name" :set="preProjectName = propsTestCase.row.project.name">{{ propsTestCase.row.project.name }}</span></q-td> -->
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;"><span v-if="preTestPlanName !== propsTestCase.row.testPlan.name" :set="preTestPlanName = propsTestCase.row.testPlan.name">{{ propsTestCase.row.testPlan.name }}</span></q-td>
          <!-- <q-td>{{ truncateText(propsTestCase.row.testPlan.name) }}</q-td> -->
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">{{ propsTestCase.row.name }}</q-td>
          <!-- <q-td>{{ propsTestCase.row.status.dropDownValue }}</q-td> -->
          <q-td style="width: 5%;">
            <q-select
              v-model="propsTestCase.row.status.id" outlined stack-label hide-bottom-space :dense="true" :bg-color="getStatusColorForTestCase(propsTestCase.row.status.dropDownValue)"
              :options="testCaseStatusList" class="company-list" option-value="value" option-label="text" emit-value map-options @update:model-value="onSubmitTestCaseStatus(propsTestCase.row.id, propsTestCase.row.status.id)"
            />
          </q-td>
          <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 8%;">{{ propsTestCase.row.testedByEmployee.person.fullName }}</q-td>
          <q-td style="width: 5%;">{{ propsTestCase.row.createdOnUtc }}</q-td>
        <!-- <q-td style="width: 5%;" class="text-center actions">
          <q-icon name="o_visibility" class="cursor-pointer q-mr-sm" @click="onView(propsTestCase.row.id)" size="xs">
            <q-tooltip>View</q-tooltip>
          </q-icon>
          <q-icon name="o_edit" class="cursor-pointer q-mr-sm" @click="onEdit(propsTestCase.row.id)" size="xs">
            <q-tooltip>Edit</q-tooltip>
          </q-icon>
          <q-icon name="o_delete_outline" class="cursor-pointer" color="negative" size="xs" @click="onDelete(propsTestCase.row)">
            <q-tooltip>Delete</q-tooltip>
          </q-icon>
        </q-td> -->
        </q-tr><q-separator />
      </template>
    </q-table>
  </fieldset>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import projectService from "modules/project/projects.service";
import commonService from "services/common.service";
import testcasesService from "modules/test-case/testCase.service";
import { notifySuccess } from "assets/utils";

const props = defineProps({ projectId: { type: String, default: "" } });
const projectId = props.projectId;
const loading = ref(true);
const tableRef6 = ref();
const rowsTestCases = ref([]);
const activeRowIdTestCases = ref(null);
const filterTestCase = ref("");
const paginationTestCases = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columnsTestCases = ref([
  // { name: "testCaseNumber", label: "Id", field: "testCaseNumber", align: "left", sortable: true },
  // { name: "project.name", label: "Project Name", field: "project.name", align: "left", sortable: true },
  { name: "testPlan.name", label: "Test Plan", field: "testPlan.name", align: "left", sortable: true },
  { name: "name", label: "Name", field: "name", align: "left", sortable: true },
  { name: "status.dropDownValue", label: "Status", field: "status.dropDownValue", align: "left", sortable: true },
  { name: "testedByEmployee.person.fullName", label: "Tested By", field: "testedByEmployee.person.fullName", align: "left", sortable: false },
  { name: "createdOnUtc", label: "Created Date", field: "createdOnUtc", align: "left", sortable: true }
]);

// On page rendering
onMounted(() => {
  getDropDownTestCaseStatus("Test Case Status");
  const propsTestCase = { pagination: paginationTestCases.value };
  getAllTestCase(propsTestCase);
});

const getAllTestCase = (propsTestCase) => {
  const { page, rowsPerPage, sortBy, descending } = propsTestCase.pagination;
  loading.value = true;
  const payloadTestCase = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    descending,
    projectId
  };
  projectService.getAllTestCasesForDashboard(payloadTestCase).then((resp) => {
    rowsTestCases.value = resp.data;
    paginationTestCases.value.page = page;
    paginationTestCases.value.rowsPerPage = rowsPerPage;
    paginationTestCases.value.sortBy = sortBy;
    paginationTestCases.value.descending = descending;
    paginationTestCases.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

const testCaseStatusList = ref([]);
function getDropDownTestCaseStatus (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    testCaseStatusList.value = responseData;
  });
}
// Added colors for dropdown list
function getStatusColorForTestCase (statusText) {
  if (statusText) {
    switch (statusText) {
    case "Reopen":
      return "purple-4";
    case "Pass":
      return "green-4";
    case "Fail":
      return "red-4";
    case "Testing":
      return "deep-orange-4";
    case "New":
      return "blue-4";
    default:
      return "#ffffff";
    }
  }
}

function onSubmitTestCaseStatus (id, statusId) {
  const model = rowsTestCases.value?.find(x => x.id === id);
  setTimeout(function () {
    const localModel = {
      projectId: model.projectId,
      planId: model.planId,
      name: model.name,
      statusId,
      testedBy: model.testedBy,
      testedDateStr: model.testedDate,
      description: model.description,
      steps: model.steps,
      expectedResult: model.expectedResult,
      actualResult: model.actualResult
    };
    testcasesService.saveTestCase(id, localModel).then(resp => {
      notifySuccess({ message: "Status is saved successfully." });
      getAllTestCase({ pagination: paginationTestCases.value });
    });
  });
}

// for static filter
const filterRows = (data, searchTerm, columns) => {
  if (!searchTerm) return data; // If no filter, return all data
  const lowerCaseTerm = searchTerm.toLowerCase();
  return data.filter(row =>
    columns.some(column => {
      const value = column.field.split(".").reduce((obj, key) => obj?.[key], row); // Handle nested fields
      return String(value || "").toLowerCase().includes(lowerCaseTerm);
    })
  );
};

const teseCaseColumns = columnsTestCases.value;
const filteredTestCase = computed(() => filterRows(rowsTestCases.value, filterTestCase.value, teseCaseColumns));
</script>
