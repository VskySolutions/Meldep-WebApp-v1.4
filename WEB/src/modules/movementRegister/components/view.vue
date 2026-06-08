<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader justify-between">
        <div class="text-h2 text-white q-mr-lg">View Movement Register</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset class="q-pa-md">
            <legend>Movement Register Info</legend>
            <!-- Loop through movement register details -->
            <div v-for="line in model.movementRegisterDetails" :key="line.id">
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Date</div>
                  <div class="text-black">{{ model.date ? model.date : "-" }}</div>
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Employee</div>
                  <div class="text-black">{{ line.employees.person.fullName ? line.employees.person.fullName : "-" }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">  {{ line.type.dropDownValue === 'Break' ? 'Break Time' :
                    line.type.dropDownValue === 'Time Adjustment' ? 'Time Adjustment Time' :
                    line.type.dropDownValue === 'Work From Home' ? 'Duration' : '' }}</div>
                  <div class="text-black">{{ line.timeInMinutes ? formatMinutesToHours(line.timeInMinutes) : "-" }}</div>
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Message</div>
                  <div class="text-black">{{ line.message ? line.message : "-" }}
                    <span v-if="line.type.dropDownValue === 'Work From Home' && line.wfhDuration?.dropDownText" class="text-primary">
                      ({{ line.wfhDuration?.dropDownText }})
                    </span>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Type</div>
                  <div class="text-black">
                    {{ line.type.dropDownValue ? line.type.dropDownValue : "-" }}
                  </div>
                </div>
                <div v-if="line.approvers?.person?.fullName && line.approvers.person.fullName.trim() !== ''" class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Approved By</div>
                  <div class="text-black">{{ line.approvers.person.fullName }}</div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Created By</div>
                  <div class="text-black">{{ line.createdBy?.person?.fullName ? line.createdBy.person.fullName : "-" }}</div>
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs">Created On</div>
                  <div class="text-black">{{ line.createdOnUtc ? line.createdOnUtc : "-" }}</div>
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset v-if="rows.length > 0" class="q-pa-md">
            <legend>Change log</legend>
            <q-table
              ref="tableRef"
              bordered
              class="no-shadow"
              :loading="loading"
              :rows="rows"
              :columns="columns"
              row-key="id"
              separator="cell"
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
                  <q-td style="width: 5%;">
                    {{ props.row.columnName === "Time In Minutes" && props.row.subModule === 'Break' ? 'Break Time' :
                      props.row.columnName === "Time In Minutes" && props.row.subModule === 'Time Adjustment' ? 'Time Adjustment Time' :
                      (props.row.columnName === "Time In Minutes" || props.row.columnName === "WFH Duration") && props.row.subModule === 'Work From Home' ? 'Duration' :
                      props.row.columnName
                    }}
                  </q-td>
                  <q-td style="width: 15%; overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                    <span v-if="props.row.columnName === 'Time In Minutes'">
                      {{ formatMinutesToHours(props.row.columnValue) }}
                    </span>
                    <span v-else>
                      {{ props.row.columnValue }}
                    </span></q-td>
                  <q-td style="width: 5%;">{{ props.row.lastModifiedBy }}</q-td>
                  <q-td style="width: 5%;">{{ props.row.lastModifiedOnUtc }}</q-td>
                </q-tr>
              </template>
            </q-table>
          </fieldset>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";

import movementRegisterService from "../movementRegister.service";
import siteService from "modules/sites/site.service";

// Common variables
const loading = ref(true);
const rows = ref([]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  id: "",
  date: "",
  deleted: false,
  movementRegisterDetails: [
    {
      id: "",
      employeeId: "",
      employees: {
        person: {
          fullName: ""
        }
      },
      approvers: {
        person: {
          fullName: ""
        }
      },
      timeInMinutes: "",
      message: "",
      type: {
        dropDownValue: ""
      },
      createdBy: {
        person: {
          fullName: ""
        }
      },
      createdOnUtc: ""
    }
  ]
});

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, detailId: { type: String, default: "" } });

const columns = ref([
  { name: "columnName", label: "Field Name", field: "columnName", align: "left", sortable: false },
  { name: "columnValue", label: "Log", field: "columnValue", align: "left", sortable: false },
  { name: "lastModifiedBy", label: "Modified By", field: "lastModifiedBy", align: "left", sortable: false },
  { name: "lastModifiedOnUtc", label: "Modified On", field: "lastModifiedOnUtc", align: "left", sortable: false }
]);

// get Movement Register details
const getMovementRegisterDetails = () => {
  loading.value = true;
  movementRegisterService.getMovementRegisterDetails(props.id, props.detailId).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// get Site Modified Logs
const getSiteModifiedLogs = () => {
  loading.value = true;
  const columnNames = ["Message", "Time In Minutes", "WFH Duration"];
  siteService.getSiteModifiedLogs(props.detailId, columnNames).then((resp) => {
    const mappedLogs = resp.map(item => ({
      ...item,
      id: item.id,
      subModule: item.subModule,
      columnValue: item.columnValue,
      columnName: item.columnName,
      lastModifiedBy: item.user?.person?.fullName,
      lastModifiedOnUtc: item.lastModifiedOnUtc
    }));

    // Sort by lastModifiedOnUtc descending → newest first
    rows.value = mappedLogs.sort((a, b) => new Date(b.lastModifiedOnUtc) - new Date(a.lastModifiedOnUtc));
  }).finally(() => {
    loading.value = false;
  });
};

const formatMinutesToHours = (minutes) => {
  if (minutes === "0") return "0 mins";
  if (!minutes || isNaN(minutes)) return "";
  const hrs = Math.floor(minutes / 60);
  const mins = minutes % 60;
  const hrText = hrs > 0 ? `${hrs} hr${hrs > 1 ? "s" : ""}` : "";
  const minText = mins > 0 ? `${mins} min` : "";
  return [hrText, minText].filter(Boolean).join(" ");
};

// On page rendering
onMounted(() => {
  getMovementRegisterDetails();
  getSiteModifiedLogs();
});

</script>
