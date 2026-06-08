<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none  dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card
      class="q-dialog-plugin PersonMain card-header with-tools headerBasic"
      style="width: 55vw !important;max-width: 55vw;"
    >
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Add Lead Activity</div>
        <q-btn v-close-popup icon="o_close" color="white" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Lead Activity Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-md-4 col-lg-4">
                  <formDate
                    v-model="model.activityDate"
                    label="Activity Date"
                    :error="v$.activityDate.$error"
                    :error-message="v$.activityDate.$errors[0]?.$message"
                    :onBlur="() => v$.activityDate.$touch()"
                  />
                  <!-- <div class="q-my-sm text-black">Activity Date<span class="required">*</span></div> -->
                  <!-- <q-input v-model="model.activityDate" :dense="true" outlined stack-label hide-bottom-space mask="##/##/####" :error="v$.activityDate.$error" :error-message="v$.activityDate.$errors[0]?.$message" @blur="v$.activityDate.$touch">
                    <template #append>
                      <q-icon name="o_calendar_month" class="cursor-pointer">
                        <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                          <q-date v-model="model.activityDate" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input> -->
                </div>
                <div class="col-12 col-md-4 col-lg-4">
                  <formSingleSelectDropdown
                    v-model="model.leadStageId"
                    label="Lead Stage"
                    :options="leadStageDropdown.list.value"
                    :filter="leadStageDropdown.filter"
                    :error="v$.leadStageId.$error"
                    :error-message="v$.leadStageId.$errors[0]?.$message"
                  />
                  <!-- <div class="q-my-sm text-black">Lead Stage<span class="required">*</span></div>
                  <q-select
                    v-model="model.leadStageId" clearable use-input outlined stack-label hide-bottom-space :dense="true"
                    :options="leadStages" option-value="value" option-label="text" emit-value map-options :error="v$.leadStageId.$error" :error-message="v$.leadStageId.$errors[0]?.$message" @filter="filterFn1" @blur="v$.leadStageId.$touch"
                  >
                    <template #option="{ itemProps, opt }">
                      <q-item v-bind="itemProps">
                        <q-item-section>
                          <div class="row q-col-gutter-x-md items-center">
                            <span>{{ opt.text }}</span>
                          </div>
                        </q-item-section>
                      </q-item>
                    </template>
                  </q-select> -->
                </div>
                <div class="col-12 col-md-4 col-lg-4">
                  <formSingleSelectDropdown
                    v-model="model.leadActivityId"
                    label="Lead Activity"
                    :options="leadActivityDropdown.list.value"
                    :filter="leadActivityDropdown.filter"
                    :error="v$.leadActivityId.$error"
                    :error-message="v$.leadActivityId.$errors[0]?.$message"
                  />
                  <!-- <div class="q-my-sm text-black">Lead Activity<span class="required">*</span></div>
                  <q-select
                    v-model="model.leadActivityId" clearable use-input outlined stack-label hide-bottom-space :dense="true"
                    :options="leadActivityList" option-value="value" option-label="text" emit-value map-options :error="v$.leadActivityId.$error" :error-message="v$.leadActivityId.$errors[0]?.$message" @filter="filterFn2" @blur="v$.leadActivityId.$touch"
                  >
                    <template #option="{ itemProps, opt }">
                      <q-item v-bind="itemProps">
                        <q-item-section>
                          <div class="row q-col-gutter-x-md items-center">
                            <span>{{ opt.text }}</span>
                          </div>
                        </q-item-section>
                      </q-item>
                    </template>
                  </q-select> -->
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="q-mb-xs text-black">Activity Note</div>
                  <q-input v-model="model.activityNote" autogrow="" outlined stack-label hide-bottom-space :dense="false" maxlength="450" />
                </div>
              </div>
              <div class="q-gutter-md">
                <q-radio v-model="model.isFutureActivity" dense val="false" label="Completed" />
                <q-radio v-model="model.isFutureActivity" dense val="true" label="Future Activity" :class="{ hidden: model.showCheckbox }" />
                <q-checkbox v-if="model.isFutureActivity === &quot;true&quot;" v-model="model.setRemainder" label="Set Remainder" />
              </div>
              <div v-if="model.setRemainder == true" id="SetReminderForm" class="col-md-12">
                <div class="row q-pa-md">
                  <div class="col-12 col-md-4">
                    <label>Reminder After</label>
                  </div>
                  <div class="col-12 col-md-4">
                    <div class="form-group">
                      <q-input v-model="futureActivityRows.reminderAfterDays" type="number" outlined stack-label hide-bottom-space :dense="true" maxlength="128" autofocus :error="reminderv$.reminderAfterDays.$error" :error-message="reminderv$.reminderAfterDays.$errors[0]?.$message" @blur="reminderv$.reminderAfterDays.$touch" @input="validatePositiveInput" />
                    </div>
                  </div>
                </div>
                <div class="row q-pa-md">
                  <div class="col-12 col-md-4">
                    <label>Reminder Time</label>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <q-input v-model="futureActivityRows.time" outlined stack-label hide-bottom-space hint="hh:mm" mask="##:##" :error="reminderv$.time.$error" :error-message="reminderv$.time.$errors[0]?.$message" :dense="true" @blur="reminderv$.time.$touch">
                        <template #append>
                          <q-icon name="o_access_time" class="cursor-pointer">
                            <q-popup-proxy ref="qTimeProxy" transition-show="scale" transition-hide="scale">
                              <q-time v-model="futureActivityRows.time" @update:model-value="() => $refs.qTimeProxy.hide()" />
                            </q-popup-proxy>
                          </q-icon>
                          <!-- <q-icon name="o_access_time" class="cursor-pointer">
                            <q-popup-proxy ref="qTimeProxy" transition-show="scale" transition-hide="scale">
                              <q-time v-model="futureActivityRows.time" format24h @update:model-value="() => $refs.qTimeProxy.hide()" />
                            </q-popup-proxy>
                          </q-icon> -->
                        </template>
                      </q-input>
                    </div>
                  </div>
                </div>
                <div class="row q-pa-md">
                  <div class="col-12 col-md-4">
                    <label>Reminder Note</label>
                  </div>
                  <div class="col-12 col-md-6">
                    <div class="form-group">
                      <q-input v-model="futureActivityRows.note" autogrow="" outlined stack-label hide-bottom-space :dense="false" maxlength="450" />
                    </div>
                  </div>
                </div>
              </div>
              <q-card-actions align="center" class="q-mt-md q-gutter-sm justify-center">
                <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
                <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
              </q-card-actions>
            </fieldset>
            <fieldset>
              <legend>Activity Log</legend>
              <div class="row">
                <div class="col">
                  <q-table 
                    ref="tableRef" 
                    v-model:pagination="pagination" 
                    virtual-scroll 
                    class="border Custom-DataTable" 
                    :loading="loading" 
                    :rows="rows" 
                    :columns="columns" 
                    row-key="id" 
                    sseparator="cell" 
                    no-data-label="No data available" 
                    binary-state-sort
                  >
                    <template #header="props">
                      <q-tr :props="props" class="bg-primary text-white">
                        <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                        <q-th auto-width class="text-center">Actions</q-th>
                      </q-tr>
                    </template>
                    <template #body="props">
                      <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                        <q-td style="width: 5%;">{{ props.row.leadStage.stageName }}</q-td>
                        <q-td style="width: 5%;">{{ props.row.leadActivity.activityName }}</q-td>
                        <q-td style="width: 5%;">{{ props.row.activityDate }}</q-td>
                        <!-- <q-td>{{ truncateText(props.row.activityNote) }}</q-td> -->
                        <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 20%;" class="RichTextEditor">
                          <div class="text-black" v-html="props.row.activityNote ? props.row.activityNote : '-'" />
                        </q-td>
                        <q-td style="width: 5%;" class="text-center actions">
                          <q-icon 
                            name="o_edit" 
                            class="cursor-pointer q-mr-sm" 
                            :class="storedUser.username === props.row.user.userName ? '' : 'hidden'" 
                            @click="onEdit(props.row)"
                          >
                            <q-tooltip>Edit</q-tooltip>
                          </q-icon>
                          <q-icon
                            name="o_delete_outline" 
                            class="cursor-pointer" 
                            color="negative" 
                            :class="storedUser.username === props.row.user.userName ? '' : 'hidden'"
                            @click="onSubmitLeadActivityDelete(props.row.id, props.row.leadActivity.activityName, refreshLeadActivityLogList)"
                          >
                            <q-tooltip>Delete</q-tooltip>
                          </q-icon>
                        </q-td>
                      </q-tr>
                    </template>
                  </q-table>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent, date } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers, minValue } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import _ from "lodash";
import { notifySuccess, notifyError, getLocalStorage } from "assets/utils";
import { format } from "date-fns"; // Standard TimeZone Conversion

import leadService from "modules/lead/lead.service";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

import leadModule from "src/modules/lead/utils/dropdowns.js";

// Shared Lead Actions
import {
  initLeadActions,
  onSubmitLeadActivityDelete
} from "src/modules/lead/utils/actions.js";

// const $q = useDialogPluginComponent();
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const storedUser = getLocalStorage("user");
const loading = ref(true);
const processing = ref(false);
const rows = ref([]);
// const futureActivityRows = ref([]);
const activeRowId = ref(null);

const pagination = ref({ sortBy: "firstName", descending: false, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "leadStage.stageName", label: "Stage", field: "leadStage.stageName", align: "left", sortable: true },
  { name: "leadActivity.activityName", label: "Activity", field: "leadActivity.activityName", align: "left", sortable: true },
  { name: "activityDate", label: "Date", field: "activityDate", align: "left", sortable: true },
  { name: "activityNote", label: "Note", field: "activityNote", align: "left", sortable: true }
]);

const model = ref({
  description: ""
});

const futureActivityRows = ref({
  reminderAfterDays: "",
  note: "",
  time: ""
});

const props = defineProps({ id: { type: String, default: "" } });

const rules = {
  activityDate: { required: helpers.withMessage("Activity Date is required", required) },
  leadStageId: { required: helpers.withMessage("Lead Stage is required", required) },
  leadActivityId: { required: helpers.withMessage("Lead Activity is required", required) }
};
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const reminderRules = {
  reminderAfterDays: { required: helpers.withMessage("Day's are required", required), minValue: helpers.withMessage("Reminder days must be a positive value", minValue(0)) },
  time: { required: helpers.withMessage("Time is required", required) }
};
const reminderv$ = useVuelidate(reminderRules, futureActivityRows, { $lazy: true, $autoDirty: true });

const getLeadActivityLog = () => {
  loading.value = true;
  leadService.getActivityLog(props.id).then((resp) => {
    rows.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

const getLeadActivity = () => {
  loading.value = true;
  leadService.getActivityLog(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.activityDate = format(new Date(), "MM/dd/yyyy");
  }).finally(() => {
    loading.value = false;
  });
};

const onEdit = (item) => {
  model.value = item;
  model.value.showCheckbox = true;
  activeRowId.value = item.id;
};

// ----------------------------------------------------------------------------------------------------------------
// Custom functions
// ----------------------------------------------------------------------------------------------------------------

const refreshLeadActivityLogList = () => {
  getLeadActivityLog();
};

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initLeadActions(activeRowId);

async function onSubmit () {
  try {
    if (!await v$.value.$validate() || (model.value.setRemainder === true && !await reminderv$.value.$validate())) {
      return;
    }
    processing.value = true;

    const payload = {
      id: activeRowId.value,
      leadsId: props.id,
      activityDate: model.value.activityDate,
      leadStageId: model.value.leadStageId,
      leadActivityId: model.value.leadActivityId,
      activityNote: model.value.activityNote,
      isFutureActivity: model.value.isFutureActivity === "true" || model.value.isFutureActivity === true || model.value.isFutureActivity === "false" || model.value.isFutureActivity === false,
      setReminderModels: futureActivityRows.value[0]
    };
    leadService.saveLeadActivityLogs(payload).then(resp => {
      notifySuccess({ message: "Lead Activity is saved successfully." });
      onDialogOK();
    });
  } catch (error) {
    console.error("Error in submitting the activity:", error);
    notifyError({ message: "An error occurred while saving the activity." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

// ------------------------------------------------------------------------------------
// All Dropdowns
// ------------------------------------------------------------------------------------
const { 
  leadStageDropdown,
  leadActivityDropdown
} = leadModule();

// const options1 = ref([]);
// function getLeadStages () {
//   leadService.getLeadStages().then((resp) => {
//     const responseData = resp.map((item) => ({ text: item.stageName, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
//     leadStages.value = responseData;
//     options1.value = responseData;
//   });
// }

// function filterFn1 (val, update, abort) {
//   update(() => {
//     const needle = val ? val.toLowerCase() : "";
//     if (needle === "") {
//       leadStages.value = options1.value;
//     } else {
//       leadStages.value = options1.value.filter(v => v.text.toLowerCase().includes(needle));
//     }
//   });
// }

// const leadActivityList = ref([]);
// const options2 = ref([]);
// function getAllLeadActivityListForDropdown () {
//   leadService.getAllLeadActivityListForDropdown().then((resp) => {
//     const responseData = resp.map((item) => ({ text: item.text, value: item.value }));
//     leadActivityList.value = responseData;
//     options2.value = responseData;
//   });
// }

// function filterFn2 (val, update, abort) {
//   update(() => {
//     const needle = val ? val.toLowerCase() : "";
//     if (needle === "") {
//       leadActivityList.value = options2.value;
//     } else {
//       leadActivityList.value = options2.value.filter(v => v.text.toLowerCase().includes(needle));
//     }
//   });
// }

// function truncateText (text) {
//   if (!text) return "NA";
//   const maxLength = 50;
//   return text.length > maxLength ? text.substring(0, maxLength) + "..." : text;
// }

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    refreshLeadActivityLogList();
  }
}, { immediate: true });

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getLeadActivity();
  }
}, { immediate: true });

watch(() => model.value.isFutureActivity, (newValue, oldValue) => {
  if (newValue === "false") {
    model.value.setRemainder = false;
  }
}, { immediate: false });

onMounted(() => {
  leadStageDropdown.load();
  leadActivityDropdown.load();
  refreshLeadActivityLogList();
});

</script>
<style>
.hidden {
  display: none;
}
</style>
