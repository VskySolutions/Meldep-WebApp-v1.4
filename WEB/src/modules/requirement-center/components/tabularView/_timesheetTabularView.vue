<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <!-- Header -->
    <q-card-section class="row items-center justify-end q-pb-sm">
      <div class="search-container position-relative">
        <searchFilterBar
          v-model="search.searchText"
          :loading="searchLoader"
          :applied-filters="appliedFilters"
          @toggle-filter="showFilter = !showFilter"
        />

        <!-- Dropdown Content -->
        <q-menu
          v-model="showFilter"
          anchor="bottom left"
          self="top left"
          persistent
          no-parent-event
          style="width: 500px;"
          @click-outside="showFilter = false"
        >
          <q-card class="q-pa-sm">
            <div class="row items-center q-mb-sm">
              <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                <label class="Cutomlabel q-mt-sm fs-13">
                  Created By
                </label>
              </div>

              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                <q-select
                  v-model="search.createdBy"
                  class="q-mx-sm w-100 h-auto"
                  stack-label
                  hide-bottom-space
                  use-input
                  :dense="true"
                  :options="createdByList"
                  emit-value
                  map-options
                  :popup-content-class="customPopupContentClass"
                />
              </div>
            </div>

            <singleSelectDropdown
              v-model="search.employeeId"
              label="Employee Name"
              :options="activeEmployeesDropdownSingleSelect.list.value"
              :filter="activeEmployeesDropdownSingleSelect.filter"
              :disable="search.createdBy === 'Created By Me'"
            />

            <singleSelectDropdown
              v-model="search.projectTaskId"
              label="Project Tasks"
              :options="projectTasksByProjectIdAndModuleIdForDropdownSingleSelect.list.value"
              :filter="projectTasksByProjectIdAndModuleIdForDropdownSingleSelect.filter"
            />

            <div class="row items-center q-mb-sm">
              <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                <label class="Cutomlabel q-mt-sm fs-13">
                  Activity Date
                </label>
              </div>

              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                <div class="input-group q-mx-sm w-100 h-auto">
                  <q-input
                    v-model="search.activityDate"
                    fill-input
                    dense
                  >
                    <template #append>
                      <q-icon
                        name="o_calendar_month"
                        class="cursor-pointer"
                      >
                        <q-popup-proxy
                          ref="qDateProxy"
                          transition-show="scale"
                          transition-hide="scale"
                        >
                          <q-date
                            v-model="search.activityDate"
                            mask="MM/DD/YYYY"
                            @update:model-value="() => $refs.qDateProxy.hide()"
                          />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
              </div>
            </div>

            <div class="row items-center q-mb-sm">
              <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                <label class="Cutomlabel q-mt-sm fs-13">
                  Week Filter
                </label>
              </div>

              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                <q-select
                  v-model="search.weekFilter"
                  class="q-mx-sm w-100 h-auto"
                  stack-label
                  clearable
                  hide-bottom-space
                  use-input
                  :dense="true"
                  :options="weekFilterList"
                  emit-value
                  map-options
                  :popup-content-class="customPopupContentClass"
                  @update:model-value="updateDates"
                />
              </div>
            </div>

            <div class="row items-center q-mb-sm">
              <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                <label class="Cutomlabel q-mt-sm fs-13">
                  From Date
                </label>
              </div>

              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                <div class="input-group q-mx-sm w-100 h-auto">
                  <q-input
                    v-model="search.fromDate"
                    fill-input
                    dense
                  >
                    <template #append>
                      <q-icon
                        name="o_calendar_month"
                        class="cursor-pointer"
                      >
                        <q-popup-proxy
                          ref="qDateProxy"
                          transition-show="scale"
                          transition-hide="scale"
                        >
                          <q-date
                            v-model="search.fromDate"
                            mask="MM/DD/YYYY"
                            @update:model-value="() => $refs.qDateProxy.hide()"
                          />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
              </div>
            </div>

            <div class="row items-center q-mb-sm">
              <div class="col-lg-5 col-md-5 col-sm-12 col-xs-12">
                <label class="Cutomlabel q-mt-sm fs-13">
                  To Date
                </label>
              </div>

              <div class="col-lg-7 col-md-7 col-sm-12 col-xs-12">
                <div class="input-group q-mx-sm w-100 h-auto">
                  <q-input
                    v-model="search.toDate"
                    fill-input
                    dense
                  >
                    <template #append>
                      <q-icon
                        name="o_calendar_month"
                        class="cursor-pointer"
                      >
                        <q-popup-proxy
                          ref="qDateProxy"
                          transition-show="scale"
                          transition-hide="scale"
                        >
                          <q-date
                            v-model="search.toDate"
                            mask="MM/DD/YYYY"
                            @update:model-value="() => $refs.qDateProxy.hide()"
                          />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div>
              </div>
            </div>

            <!-- Search and Clear Buttons -->
            <div class="row justify-end q-gutter-sm q-mb-sm">
              <q-btn
                style="width: 20%;"
                outline
                color="primary"
                label="Search"
                class="btnRounded"
                no-caps
                @click="() => {
                  showFilter = false;
                  onAdvanceSearch();
                }"
              />

              <q-btn
                style="width: 20%;"
                outline
                color="grey-4"
                label="Clear"
                class="text-grey-9 btnRounded"
                no-caps
                @click="onAdvanceClear"
              />

              <q-btn
                style="width: 20%;"
                outline
                color="negative"
                label="Close"
                class="btnRounded"
                no-caps
                @click="() => {
                  showFilter = false;
                }"
              />
            </div>
          </q-card>
        </q-menu>
      </div>

      <div class="row items-center q-gutter-sm">
        <q-btn
          v-if="projectId"
          icon="o_open_in_new"
          size="sm"
          outline
          class="text-primary"
          style="padding: 3px 7px; min-height: 30px;"
          @click="$router.push({
            path: '/timesheet',
            state: {
              projectId: projectId
            }
          })"
        >
          <q-tooltip>Open Timesheet List</q-tooltip>
        </q-btn>
      </div>
    </q-card-section>

    <q-separator />

    <!-- Table -->
    <q-table
      flat
      :rows="filteredRows"
      :columns="columns"
      :loading="loading"
      v-model:pagination="pagination"
      row-key="id"
      :rows-per-page-options="[20, 50, 100, 200, 500]"
      class="req-dashboard-table"
      separator="cell"
      no-data-label="No data available"
      @request="getAllTimesheetByRequirementId"
    >
      <template #header="props">
        <q-tr
          :props="props"
          class="bg-primary text-white"
        >
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
          <!-- Date -->
          <q-td>
            <span v-if="props.row._showDate">
              {{ getTimesheetDate(props.row) }}
            </span>
          </q-td>

          <!-- Employee -->
          <q-td>
            <span v-if="props.row._showEmployee">
              {{ getEmployeeName(props.row) }}
            </span>
          </q-td>

          <!-- Task -->
          <q-td>
            {{ props.row?.task?.name ?? "-" }}
          </q-td>

          <!-- Hours -->
          <q-td align="right">
            {{ props.row?.hours ?? "00:00" }}
          </q-td>
        </q-tr>
      </template>

      <template #bottom-row>
        <q-tr class="bg-grey-2 text-weight-bold">
          <q-td
            colspan="3"
            class="text-right"
          >
            Total Hours:
          </q-td>

          <q-td class="text-right">
            {{ totalHours }}
          </q-td>
        </q-tr>
      </template>
    </q-table>
  </q-card>
</template>

<script setup>
import {
  computed,
  ref,
  watch,
  onMounted
} from "vue";

import { useAuthStore } from "stores/auth";

import requirementCenterService
  from "src/modules/requirement-center/requirementCenter.service";

import singleSelectDropdown
  from "src/components/form-inputs/_singleSelectDropdown.vue";

import employeeModule
  from "src/modules/employee/utils/dropdowns.js";

import projectTaskModule
  from "src/modules/project-tasks/utils/dropdowns.js";

import searchFilterBar
  from "src/components/dataTable/_searchFilterBar.vue";

import useSiteTableState
  from "composables/dataTable/useSiteTableState.js";

const emit = defineEmits(["summary"]);

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

const loading = ref(false);
const searchLoader = ref(false);
const showFilter = ref(false);

const authStore = useAuthStore();

const siteId = computed(
  () => authStore.user?.siteId
);

const rows = ref([]);

const projectId = ref("");

const columns = [
  {
    name: "timesheet.timesheetDate",
    label: "DATE",
    field: "timesheet.timesheetDate",
    align: "left",
    sortable: true
  },
  {
    name: "timesheet.user.person.firstName",
    label: "EMPLOYEE",
    field: "timesheet.user.person.firstName",
    align: "left",
    sortable: true
  },
  {
    name: "task.name",
    label: "TASK",
    field: "task.name",
    align: "left",
    sortable: true
  },
  {
    name: "hours",
    label: "HOURS",
    field: "hours",
    align: "right",
    sortable: true
  }
];

const getTimesheetDate = (row) => {
  return row?.timesheet?.timesheetDate ?? "";
};

const getEmployeeName = (row) => {
  return row?.timesheet?.user?.person?.fullName ?? "";
};

const getTaskName = (row) => {
  return row?.task?.name ?? "-";
};

const normalizeDate = (value) => {
  if (
    value === null ||
    value === undefined ||
    value === ""
  ) {
    return "";
  }

  const stringValue =
    String(value).trim();

  const isoMatch =
    stringValue.match(
      /^(\d{4})-(\d{2})-(\d{2})/
    );

  if (isoMatch) {
    return [
      isoMatch[1],
      isoMatch[2],
      isoMatch[3]
    ].join("-");
  }

  const slashMatch =
    stringValue.match(
      /^(\d{1,2})\/(\d{1,2})\/(\d{4})/
    );

  if (slashMatch) {
    return [
      slashMatch[3],
      String(slashMatch[1]).padStart(2, "0"),
      String(slashMatch[2]).padStart(2, "0")
    ].join("-");
  }

  const parsedDate =
    new Date(value);

  if (!Number.isNaN(
    parsedDate.getTime()
  )) {
    return [
      parsedDate.getFullYear(),
      String(
        parsedDate.getMonth() + 1
      ).padStart(2, "0"),
      String(
        parsedDate.getDate()
      ).padStart(2, "0")
    ].join("-");
  }

  return stringValue.toLowerCase();
};

const normalizeEmployee = (
  value
) => {
  return String(value ?? "")
    .trim()
    .replace(/\s+/g, " ")
    .toLowerCase();
};

const applyGrouping = (
  sourceRows
) => {
  if (!Array.isArray(sourceRows)) {
    return [];
  }

  return sourceRows.map(
    (row, index) => {
      if (index === 0) {
        return {
          ...row,
          _showDate: true,
          _showEmployee: true
        };
      }

      const previousRow =
        sourceRows[index - 1];

      const currentDate =
        normalizeDate(
          getTimesheetDate(row)
        );

      const previousDate =
        normalizeDate(
          getTimesheetDate(
            previousRow
          )
        );

      const currentEmployee =
        normalizeEmployee(
          getEmployeeName(row)
        );

      const previousEmployee =
        normalizeEmployee(
          getEmployeeName(
            previousRow
          )
        );

      const dateChanged =
        currentDate !==
        previousDate;

      const employeeChanged =
        currentEmployee !==
        previousEmployee;

      return {
        ...row,

        _showDate:
          dateChanged,

        _showEmployee:
          dateChanged ||
          employeeChanged
      };
    }
  );
};

const filteredRows = computed(() => {
  const sourceRows =
    Array.isArray(rows.value)
      ? rows.value
      : [];

  const searchText =
    String(
      search.value.searchText ?? ""
    )
      .trim()
      .toLowerCase();

  if (!searchText) {
    return applyGrouping(
      sourceRows
    );
  }

  const filtered =
    sourceRows.filter((row) => {
      const employee =
        getEmployeeName(row)
          .toLowerCase();

      const task =
        getTaskName(row)
          .toLowerCase();

      const date =
        String(
          getTimesheetDate(row)
        ).toLowerCase();

      return (
        employee.includes(
          searchText
        ) ||
        task.includes(
          searchText
        ) ||
        date.includes(
          searchText
        )
      );
    });

  return applyGrouping(
    filtered
  );
});

const getAllTimesheetByRequirementId =
  async ({
    pagination: p = pagination.value
  }) => {
    const {
      page,
      rowsPerPage,
      sortBy,
      descending
    } = p;

    try {
      loading.value = true;

      const payload = {
        requirementId:
          props.requirementId,

        searchText:
          search.value.searchText,

        createdBy:
          search.value.createdBy,

        employeeId:
          search.value.employeeId,

        projectTaskId:
          search.value.projectTaskId,

        activityDate:
          search.value.activityDate,

        fromDate:
          search.value.fromDate,

        toDate:
          search.value.toDate,

        weekFilter:
          search.value.weekFilter,

        page,

        pageSize:
          rowsPerPage,

        sortBy,

        descending,

        sorts:
          sorts.value
      };

      const resp =
        await requirementCenterService
          .getAllTimesheetByRequirementId(
            payload
          );

      rows.value =
        Array.isArray(resp?.data)
          ? resp.data
          : [];

      Object.assign(
        pagination.value,
        {
          page,
          rowsPerPage,
          sortBy,
          descending,
          rowsNumber:
            resp?.total || 0
        }
      );

      saveDataTableState({
        search:
          search.value,

        pagination:
          pagination.value,

        activeRowId:
          activeRowId.value,

        sorts
      });
    } finally {
      loading.value = false;
      searchLoader.value = false;
    }
  };

function calculateTotalHours(
  sourceRows
) {
  let totalMinutes = 0;

  if (!Array.isArray(sourceRows)) {
    return "00:00";
  }

  sourceRows.forEach((row) => {
    if (row?.hours == null) {
      return;
    }

    let hour = "0";
    let minute = "0";

    if (
      typeof row.hours ===
      "string"
    ) {
      [hour, minute] =
        row.hours.split(":");
    } else {
      const value =
        Number(row.hours);

      if (Number.isNaN(value)) {
        return;
      }

      const h =
        Math.floor(value);

      const m =
        Math.round(
          (value - h) * 100
        );

      hour =
        h.toString();

      minute =
        m.toString().padStart(
          2,
          "0"
        );
    }

    totalMinutes +=
      parseInt(hour, 10) * 60 +
      parseInt(minute, 10);
  });

  const hours =
    Math.floor(
      totalMinutes / 60
    );

  const minutes =
    totalMinutes % 60;

  return `${hours
    .toString()
    .padStart(2, "0")}:${minutes
    .toString()
    .padStart(2, "0")}`;
}

const totalHours =
  computed(() =>
    calculateTotalHours(
      rows.value
    )
  );

/* --------------------------------------------------------------------------
 * DataTable state
 * -------------------------------------------------------------------------- */

const refreshTimesheetList =
  () => {
    return getAllTimesheetByRequirementId(
      {
        pagination:
          pagination.value
      }
    );
  };

const {
  search,
  pagination,
  activeRowId,
  sorts,
  saveDataTableState
} = useSiteTableState({
  storageKey:
    "requirement-Center-Timesheet-Tabular-List",

  siteId,

  defaultSearch: {
    searchText: "",
    createdBy: "",
    employeeId: "",
    projectId: "",
    projectModuleId: "",
    projectTaskId: "",
    activityDate: null,
    fromDate: null,
    toDate: null,
    weekFilter: ""
  },

  defaultPagination: {
    sortBy: "createdOnUtc",
    descending: true,
    rowsPerPage: 20,
    page: 1
  }
});

/* --------------------------------------------------------------------------
 * Search
 * -------------------------------------------------------------------------- */

const onAdvanceSearch = () => {
  pagination.value.page = 1;

  refreshTimesheetList();
};

/* --------------------------------------------------------------------------
 * Clear
 * -------------------------------------------------------------------------- */

const onAdvanceClear = () => {
  search.value.createdBy =
    "Created By Me";

  search.value.weekFilter =
    "";

  search.value.projectTaskId =
    null;

  search.value.employeeId =
    null;

  search.value.activityDate =
    null;

  search.value.fromDate =
    null;

  search.value.toDate =
    null;

  pagination.value.page = 1;

  saveDataTableState({
    search:
      search.value
  });

  onAdvanceSearch();
};

/* --------------------------------------------------------------------------
 * Dropdowns
 * -------------------------------------------------------------------------- */

const {
  activeEmployeesDropdownSingleSelect
} = employeeModule();

const {
  projectTasksByProjectIdAndModuleIdForDropdownSingleSelect
} = projectTaskModule();

const createdByList = ref([
  "Created By Me",
  "View All"
]);

const weekFilterList = ref([
  "Last Week",
  "This Week",
  "This Month"
]);

/* --------------------------------------------------------------------------
 * Applied filters
 * -------------------------------------------------------------------------- */

const mapFilterToLabel = (
  id,
  list,
  label
) => {
  if (
    id == null ||
    id === ""
  ) {
    return {};
  }

  const match =
    list.value.find(
      (item) =>
        item.value === id
    );

  const text =
    match
      ? match.text
      : id;

  return {
    [label]: text
  };
};

const appliedFilters =
  computed(() => ({
    ...mapFilterToLabel(
      search.value.createdBy,
      createdByList,
      "Created By"
    ),

    ...mapFilterToLabel(
      search.value.employeeId,
      activeEmployeesDropdownSingleSelect.list,
      "Employee Name"
    ),

    ...mapFilterToLabel(
      search.value.projectTaskId,
      projectTasksByProjectIdAndModuleIdForDropdownSingleSelect.list,
      "Project Tasks"
    ),

    ...mapFilterToLabel(
      search.value.weekFilter,
      weekFilterList,
      "Week Filter"
    ),

    ...(search.value.activityDate
      ? {
          "Activity Date":
            search.value.activityDate
        }
      : {}),

    ...(search.value.fromDate
      ? {
          "From Date":
            search.value.fromDate
        }
      : {}),

    ...(search.value.toDate
      ? {
          "To Date":
            search.value.toDate
        }
      : {})
  }));

/* --------------------------------------------------------------------------
 * Week filter
 * -------------------------------------------------------------------------- */

const updateDates = (
  value
) => {
  if (!value) {
    return;
  }

  const today =
    new Date();

  const formatDate = (
    date
  ) => {
    const month =
      String(
        date.getMonth() + 1
      ).padStart(2, "0");

    const day =
      String(
        date.getDate()
      ).padStart(2, "0");

    const year =
      date.getFullYear();

    return `${month}/${day}/${year}`;
  };

  const getStartOfWeek = (
    date
  ) => {
    const result =
      new Date(date);

    const day =
      result.getDay();

    const difference =
      day === 0
        ? -6
        : 1 - day;

    result.setDate(
      result.getDate() +
        difference
    );

    return result;
  };

  if (
    value === "This Week"
  ) {
    const start =
      getStartOfWeek(
        today
      );

    const end =
      new Date(start);

    end.setDate(
      end.getDate() + 6
    );

    search.value.fromDate =
      formatDate(start);

    search.value.toDate =
      formatDate(end);

    return;
  }

  if (
    value === "Last Week"
  ) {
    const currentStart =
      getStartOfWeek(
        today
      );

    const start =
      new Date(
        currentStart
      );

    start.setDate(
      start.getDate() - 7
    );

    const end =
      new Date(
        currentStart
      );

    end.setDate(
      end.getDate() - 1
    );

    search.value.fromDate =
      formatDate(start);

    search.value.toDate =
      formatDate(end);

    return;
  }

  if (
    value === "This Month"
  ) {
    const start =
      new Date(
        today.getFullYear(),
        today.getMonth(),
        1
      );

    const end =
      new Date(
        today.getFullYear(),
        today.getMonth() + 1,
        0
      );

    search.value.fromDate =
      formatDate(start);

    search.value.toDate =
      formatDate(end);
  }
};

/* --------------------------------------------------------------------------
 * Requirement change
 * -------------------------------------------------------------------------- */

watch(
  () => props.requirementId,
  async () => {
    pagination.value.page = 1;

    await getAllTimesheetByRequirementId({
      pagination:
        pagination.value
    });
  },
  {
    immediate: true
  }
);

/* --------------------------------------------------------------------------
 * Search text
 * -------------------------------------------------------------------------- */

watch(
  () =>
    search.value.searchText,
  () => {
    searchLoader.value = true;

    pagination.value.page = 1;

    refreshTimesheetList();
  }
);

/* --------------------------------------------------------------------------
 * Created By
 * -------------------------------------------------------------------------- */

watch(
  () =>
    search.value.createdBy,
  (value) => {
    if (
      value === "Created By Me" ||
      value
    ) {
      search.value.employeeId =
        null;
    }
  }
);

/* --------------------------------------------------------------------------
 * Mounted
 * -------------------------------------------------------------------------- */

onMounted(async () => {
  await activeEmployeesDropdownSingleSelect.load(
    siteId.value
  );

  if (projectId.value) {
    await projectTasksByProjectIdAndModuleIdForDropdownSingleSelect.load(
      false,
      projectId.value
    );
  }
});
</script>
