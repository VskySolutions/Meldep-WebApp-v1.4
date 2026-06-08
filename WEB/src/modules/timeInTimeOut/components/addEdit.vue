<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1300px; height: 100% !important;max-width: 150vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "View" : "Add" }} Time In & Time Out</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable addBulkTasks">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Time Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <!-- Time In -->
                <div class="col-3">
                  <label class="label q-mb-xs text-black">Time In Date</label>
                  <div>
                    <q-input v-model="model.timeInDateStr" outlined stack-label hide-bottom-space mask="##/##/####" readonly="readonly" dense >
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date
                              v-model="model.timeInDateStr"
                              mask="MM/DD/YYYY"
                              @update:model-value="() => $refs.qDateProxy.hide()"
                              :options="optionsDate"
                            />
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
                <div class="col-4">
                  <label class="label q-mb-xs text-black">Time In<span class="required">*</span></label>
                  <div class="form-group">
                    <!-- <q-input v-model="model.timeInStr" outlined stack-label hide-bottom-space :dense="true" mask="time" :rules="['time']" hint="hh:mm" placeholder="hh:mm"
                    :error="v$.timeInStr.$error" :error-message="v$.timeInStr.$errors[0]?.$message" @blur="v$.timeInStr.$touch"/> -->
                    <q-input v-model="model.timeInStr" outlined stack-label hide-bottom-space :dense="true" mask="fulltime"
                    :rules="['fulltime']" hint="hh:mm" placeholder="hh:mm">
                      <template v-slot:append>
                        <q-icon name="o_access_time" class="cursor-pointer">
                          <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                            <q-time
                              v-model="model.timeInStr"
                              with-seconds
                              mask="HH*mm**ss"
                            >
                              <div class="row items-center justify-end">
                                <q-btn v-close-popup label="Close" color="primary" flat />
                              </div>
                            </q-time>
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <!-- Time Out -->
                <div class="col-3">
                  <label class="label q-mb-xs text-black">Time Out Date</label>
                  <div>
                    <q-input v-model="model.timeOutDateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense>
                      <template #append>
                        <q-icon name="o_calendar_month" class="cursor-pointer">
                          <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                            <q-date v-model="model.timeOutDateStr" mask="MM/DD/YYYY" @update:model-value="() => $refs.qDateProxy.hide()" :options="optionsDate"/>
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
                <div class="col-4">
                  <label class="label q-mb-xs text-black">Time Out</label>
                  <div class="form-group">
                    <!-- <q-input v-model="model.timeOutStr" outlined stack-label hide-bottom-space :dense="true" mask="time" :rules="['time']" hint="hh:mm" placeholder="hh:mm"/> -->
                    <q-input v-model="model.timeOutStr" outlined stack-label hide-bottom-space :dense="true" mask="fulltime" :rules="['fulltime']" hint="hh:mm" placeholder="hh:mm">
                      <template v-slot:append>
                        <q-icon name="o_access_time" class="cursor-pointer">
                          <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                            <q-time
                              v-model="model.timeOutStr"
                              with-seconds
                              mask="HH*mm**ss"
                            >
                              <div class="row items-center justify-end">
                                <q-btn v-close-popup label="Close" color="primary" flat />
                              </div>
                            </q-time>
                          </q-popup-proxy>
                        </q-icon>
                      </template>
                    </q-input>
                  </div>
                </div>
              </div>
            </fieldset>
            <fieldset>
              <legend>Break Info</legend>
              <div class="q-pa-md cardTable">
                <div class="q-gutter-y-md"></div>
                  <div class="flex items-center justify-end q-mb-md">
                    <q-btn color="primary" icon="o_add" label="Add Break" no-caps @click="onAdd" />
                  </div>
                <q-table
                  ref="tableRef" v-model:pagination="pagination" bordered class="no-shadow" :loading="loading" :rows="rows" :columns="columns" row-key="id" separator="cell"
                  no-data-label="I didn't find anything for you" binary-state-sort
                >
                  <template #header="props">
                    <q-tr :props="props">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                      <q-th auto-width class="text-center">Actions</q-th>
                    </q-tr>
                  </template>
                  <template #top-row>
                    <q-tr v-if="mode == 'add' && editingRow" class="row-highlight">
                      <q-td>
                        <!-- <q-input v-model="editingRow.breakInStr" outlined stack-label hide-bottom-space :dense="true" mask="time" :rules="['time']" hint="hh:mm" placeholder="hh:mm"
                          :error="editingRowV$.breakInStr.$error" :error-message="editingRowV$.breakInStr.$errors[0]?.$message" @blur="editingRowV$.breakInStr.$touch">
                        </q-input> -->
                        <q-input v-model="editingRow.breakInStr" outlined stack-label hide-bottom-space :dense="true" mask="fulltime" :rules="['fulltime']" hint="hh:mm" placeholder="hh:mm">
                          <template v-slot:append>
                            <q-icon name="o_access_time" class="cursor-pointer">
                              <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                                <q-time
                                  v-model="editingRow.breakInStr"
                                  with-seconds
                                  mask="HH*mm**ss"
                                >
                                  <div class="row items-center justify-end">
                                    <q-btn v-close-popup label="Close" color="primary" flat />
                                  </div>
                                </q-time>
                              </q-popup-proxy>
                            </q-icon>
                          </template>
                        </q-input>
                      </q-td>
                      <q-td>
                        <!-- <q-input v-model="editingRow.breakOutStr" outlined stack-label hide-bottom-space :dense="true" mask="time" :rules="['time']" hint="hh:mm" placeholder="hh:mm"
                          :error="editingRowV$.breakOutStr.$error" :error-message="editingRowV$.breakOutStr.$errors[0]?.$message" @blur="editingRowV$.breakOutStr.$touch">
                        </q-input> -->
                        <q-input v-model="editingRow.breakOutStr" outlined stack-label hide-bottom-space :dense="true" mask="fulltime" :rules="['fulltime']" hint="hh:mm" placeholder="hh:mm">
                          <template v-slot:append>
                            <q-icon name="o_access_time" class="cursor-pointer">
                              <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                                <q-time
                                  v-model="editingRow.breakOutStr"
                                  with-seconds
                                  mask="HH*mm**ss"
                                >
                                  <div class="row items-center justify-end">
                                    <q-btn v-close-popup label="Close" color="primary" flat />
                                  </div>
                                </q-time>
                              </q-popup-proxy>
                            </q-icon>
                          </template>
                        </q-input>
                      </q-td>
                      <q-td>
                        <q-input v-model="editingRow.breakReason" outlined stack-label hide-bottom-space dense
                        :error="editingRowV$.breakReason.$error" :error-message="editingRowV$.breakReason.$errors[0]?.$message" @blur="editingRowV$.breakReason.$touch">
                        </q-input>
                      </q-td>
                      <q-td auto-width class="text-center">
                        <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                          <q-tooltip>Save</q-tooltip>
                        </q-icon>
                        <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                          <q-tooltip>Cancel</q-tooltip>
                        </q-icon>
                      </q-td>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                      <q-td class="text-left">
                        <!-- <q-input v-if="mode == 'edit' && editingRow && props.row.id === activeRowId" v-model="editingRow.breakInStr" :dense="true" mask="time" :rules="['time']" hint="hh:mm" placeholder="hh:mm" outlined stack-label hide-bottom-space
                        :error="editingRowV$.breakInStr.$error" :error-message="editingRowV$.breakInStr.$errors[0]?.$message" @blur="editingRowV$.breakInStr.$touch">
                        </q-input> -->
                        <q-input v-if="mode == 'edit' && editingRow && props.row.id === activeRowId" v-model="editingRow.breakInStr" outlined stack-label hide-bottom-space :dense="true" mask="fulltime" :rules="['fulltime']" hint="hh:mm" placeholder="hh:mm"
                        :error="editingRowV$.breakInStr.$error" :error-message="editingRowV$.breakInStr.$errors[0]?.$message" @blur="editingRowV$.breakInStr.$touch">
                          <template v-slot:append>
                            <q-icon name="o_access_time" class="cursor-pointer">
                              <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                                <q-time
                                  v-model="editingRow.breakInStr"
                                  with-seconds
                                  mask="HH*mm**ss"
                                >
                                  <div class="row items-center justify-end">
                                    <q-btn v-close-popup label="Close" color="primary" flat />
                                  </div>
                                </q-time>
                              </q-popup-proxy>
                            </q-icon>
                          </template>
                        </q-input>
                        <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.breakInStr }} </span>
                      </q-td>
                      <q-td class="text-left">
                        <!-- <q-input v-if="mode == 'edit' && editingRow && props.row.id === activeRowId" v-model="editingRow.breakOutStr" :dense="true" mask="time" :rules="['time']" hint="hh:mm" placeholder="hh:mm" outlined stack-label hide-bottom-space
                        :error="editingRowV$.breakOutStr.$error" :error-message="editingRowV$.breakOutStr.$errors[0]?.$message" @blur="editingRowV$.breakOutStr.$touch">
                        </q-input> -->
                        <q-input v-if="mode == 'edit' && editingRow && props.row.id === activeRowId" v-model="editingRow.breakOutStr" outlined stack-label hide-bottom-space :dense="true" mask="fulltime" :rules="['fulltime']" hint="hh:mm" placeholder="hh:mm"
                        :error="editingRowV$.breakOutStr.$error" :error-message="editingRowV$.breakOutStr.$errors[0]?.$message" @blur="editingRowV$.breakOutStr.$touch">
                          <template v-slot:append>
                            <q-icon name="o_access_time" class="cursor-pointer">
                              <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                                <q-time
                                  v-model="editingRow.breakOutStr"
                                  with-seconds
                                  mask="HH*mm**ss"
                                >
                                  <div class="row items-center justify-end">
                                    <q-btn v-close-popup label="Close" color="primary" flat />
                                  </div>
                                </q-time>
                              </q-popup-proxy>
                            </q-icon>
                          </template>
                        </q-input>
                        <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.breakOutStr }} </span>
                      </q-td>
                      <q-td class="text-left">
                        <q-input v-if="mode == 'edit' && editingRow && props.row.id === activeRowId" v-model="editingRow.breakReason" outlined stack-label hide-bottom-space dense
                        :error="editingRowV$.breakReason.$error" :error-message="editingRowV$.breakReason.$errors[0]?.$message" @blur="editingRowV$.breakReason.$touch">
                        </q-input>
                        <span v-else :class="props.row.deleted ? 'text-delete' : ''">{{ props.row.breakReason }} </span>
                      </q-td>
                      <q-td auto-width class="text-center">
                        <template v-if="mode == 'edit' && editingRow && props.row.id === activeRowId">
                          <q-icon name="o_save" size="xs" class="cursor-pointer q-mr-lg" @click="onSave()">
                            <q-tooltip>Save</q-tooltip>
                          </q-icon>
                          <q-icon name="o_cancel" size="xs" color="red" class="cursor-pointer" @click="onCancel">
                            <q-tooltip>Cancel</q-tooltip>
                          </q-icon>
                        </template>
                        <template v-else>
                          <q-icon name="o_edit" size="xs" class="cursor-pointer q-mr-lg" @click="onEdit(props.row)">
                            <q-tooltip>Edit</q-tooltip>
                          </q-icon>
                          <q-icon v-if="!props.row.deleted" name="o_delete_outline" color="red" size="xs" class="cursor-pointer" @click="onDelete(props.row)">
                            <q-tooltip>Delete</q-tooltip>
                          </q-icon>
                          <q-icon v-if="props.row.deleted" name="o_redo" size="xs" class="cursor-pointer" @click="onUndo(props.row)">
                            <q-tooltip>Undo</q-tooltip>
                          </q-icon>
                        </template>
                      </q-td>
                    </q-tr>
                  </template>
                </q-table>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
        <!-- <q-separator /> -->
      </q-form>
      <!-- </q-card-section> -->
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, uid } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted } from "vue";
import _ from "lodash";
import { notifySuccess } from "assets/utils";
import useFilters from "composables/useFilters";
import timeInTimeOutService from "../timeInTimeOut.service";

// Common variables
const loading = ref(true);
const processing = ref(false);
const currentDate = new Date();

const tableRef = ref();
const rows = ref([]);
const mode = ref(null);
const { toDate } = useFilters();

const editingRow = ref(null);
const activeRowId = ref(null);
const pagination = ref({ sortBy: "activityName", descending: false, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "name", label: "Break Start", field: "activityName", align: "left", sortable: true },
  { name: "startDate", label: "Break End", field: "startDate", align: "left", sortable: true },
  { name: "endDate", label: "Reason", field: "endDate", align: "left", sortable: true }
]);

// Define model values
const model = ref({
  timeInStr: ref("00:00:00"),
  timeOutStr: ref("00:00:00"),
  timeInDateStr: toDate(currentDate),
  timeOutDateStr: toDate(currentDate)
});

// Validation rules
const rules = {
  timeInStr: { required: helpers.withMessage("Time In is required", required) }
};
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Multiple row validation
const editingRowrules = {
  breakInStr: { required: helpers.withMessage("Required", required) },
  breakOutStr: { required: helpers.withMessage("Required", required) },
  breakReason: { required: helpers.withMessage("Required", required) }
};
const editingRowV$ = useVuelidate(editingRowrules, editingRow, { $lazy: true, $autoDirty: true });

// Add new row
const allEditingRows = ref([]);
function onAdd () {
  mode.value = "add";
  editingRow.value = {
    breakInStr: ref("00:00:00"),
    breakOutStr: ref("00:00:00"),
    breakReason: ""
  };
  activeRowId.value = null;
  allEditingRows.value.unshift(editingRow.value);
}

// Edit row
function onEdit (item) {
  mode.value = "edit";
  editingRow.value = _.cloneDeep(item);
  activeRowId.value = item.id;
}

// Remove row
function onCancel () {
  mode.value = null;
  editingRow.value = null;
  activeRowId.value = null;
}

function onDelete (item) {
  item.deleted = true;
  const rowIndex = rows.value.findIndex((row) => row.id === item.id);
  if (rowIndex !== -1) {
    rows.value.splice(rowIndex, 1, {
      ...rows.value[rowIndex],
      id: item.id,
      breakIn: item.breakIn,
      breakOut: item.breakOut,
      breakReason: item.breakReason,
      flag: "Delete"
    });
  }
  activeRowId.value = item.id;
}

// Save row
async function onSave (editingRowId = null) {
  if (!await editingRowV$.value.$validate()) {
    return;
  }

  if (mode.value === "edit") {
    if (editingRowId != null) {
      editingRow.value.id = editingRowId;
    }
    const rowIndex = rows.value.findIndex((row) => row.id === editingRow.value.id);

    if (rowIndex !== -1) {
      rows.value.splice(rowIndex, 1, {
        ...rows.value[rowIndex],
        breakInStr: editingRow.value.breakInStr,
        breakOutStr: editingRow.value.breakOutStr,
        breakReason: editingRow.value.breakReason,
        flag: "Edit"
      });
      editingRow.value = null;
      mode.value = null;
      activeRowId.value = null;
    }
  } else if (mode.value === "add") {
    const newRow = {
      id: uid(),
      breakInStr: editingRow.value.breakInStr,
      breakOutStr: editingRow.value.breakOutStr,
      breakReason: editingRow.value.breakReason,
      flag: "New"
    };

    rows.value.unshift(newRow);
    mode.value = null;
    activeRowId.value = null;
  }
}

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, projectIdAttr: { type: String, default: "" }, moduleIdAttr: { type: String, default: "" } });

// On page rendering
onMounted(() => {
});

// get project details on edit mode
const getTimeInTimeOutDetails = () => {
  loading.value = true;
  timeInTimeOutService.getTimeInTimeOutDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.timeInDateStr = resp.timeInDate ? toDate(resp.timeInDate) : "";
    model.value.timeOutDateStr = resp.timeOutDate ? toDate(resp.timeOutDate) : "";
    rows.value = resp.timeInTimeOutBreakDetailList.map((lines, index) => {
      return {
        ...lines,
        editing: false,
        breakInStr: convertDecimalToTimeStr(lines.breakIn),
        breakOutStr: convertDecimalToTimeStr(lines.breakOut),
        breakReason: lines.breakReason,
        flag: "Edit"
      };
    });
  }).finally(() => {
    loading.value = false;
  });
};

// Convert decimal to HH.mm.ss for display
const convertDecimalToTimeStr = (decimalTime) => {
  if (decimalTime == null) return "";
  const hours = Math.floor(decimalTime);
  const minutesDecimal = (decimalTime - hours) * 60;
  const minutes = Math.floor(minutesDecimal);
  const seconds = Math.round((minutesDecimal - minutes) * 60);

  const pad = (num) => num.toString().padStart(2, "0");
  return `${pad(hours)}.${pad(minutes)}.${pad(seconds)}`;
};

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getTimeInTimeOutDetails();
  }
}, { immediate: true });

// Submit form
async function onSubmit () {
  if (!await v$.value.$validate() && !await editingRowV$.value.$validate()) {
    return;
  }
  processing.value = true;
  const payload = {
    timeInDate: model.value.timeInDateStr,
    timeOutDate: model.value.timeOutDateStr,
    timeInStr: model.value.timeInStr,
    timeOutStr: model.value.timeOutStr,
    timeInTimeOutBreakDetailModel: rows.value
  };
  if (rows.value.length > 0) {
    timeInTimeOutService.saveBreak(props.id, payload).then(resp => {
      notifySuccess({ message: "Break saved successfully." });
      onDialogOK();
      processing.value = false;
      // $emit("ok");
      // $emit("hide");
    });
  } else {
    timeInTimeOutService.saveTimeInTimeOut(props.id, payload).then(resp => {
      notifySuccess({ message: "Time In saved successfully." });
      onDialogOK();
      processing.value = false;
      // $emit("ok");
      // $emit("hide");
    });
  }
}
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.addBulkTasks .q-select__dropdown-icon{
  display: none;
}
</style>
