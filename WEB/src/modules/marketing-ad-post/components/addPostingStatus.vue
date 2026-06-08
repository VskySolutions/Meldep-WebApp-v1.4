<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1100px; height: 100% !important;max-width: 150vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Ad Posting Status</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Ad Posting Status Info</legend>
              <div class="q-pa-md cardTable">
                <div class="q-gutter-y-md" />
                <div class="flex items-center justify-end q-mb-md">
                  <q-btn color="primary" icon="o_add" label="Add Posting" no-caps @click="onAdd" />
                </div>
                <q-table
                  ref="tableRef"
                  v-model:pagination="pagination"
                  virtual-scroll
                  bordered
                  class="no-shadow"
                  :loading="loading"
                  :rows="rows"
                  :columns="columns"
                  row-key="id"
                  separator="cell"
                  :rows-per-page-options="[20, 50, 100, 200, 500]"
                  no-data-label="No data available"
                  binary-state-sort
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}<span v-if="['adPostChannelId', 'dateStr', 'likes','comments', 'shares'].includes(col.name)" class="required">*</span></q-th>
                      <q-th auto-width class="text-center">Actions</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :class="props.row.deleted ? 'hidden' : ''">
                      <q-td>
                        <div>
                          <q-select
                            v-model="props.row.adPostChannelId"
                            clearable
                            use-input
                            outlined
                            stack-label
                            hide-bottom-space
                            :dense="true"
                            :options="adPostChannelList[props.row.rowCounter]"
                            option-value="value"
                            option-label="text"
                            emit-value
                            map-options
                            :error="rowValidations[props.rowIndex]?.value?.adPostChannelId.$error"
                            :error-message="rowValidations[props.rowIndex]?.value?.adPostChannelId.$errors[0]?.$message"
                            @filter="(val, update, abort) => adPostChannelListForFilter(val, update, abort, props.row.rowCounter)"
                            @blur="rowValidations[props.rowIndex]?.value?.adPostChannelId.$touch"
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
                          </q-select>
                        </div>
                      </q-td>
                      <q-td>
                        <q-input
                          v-model="props.row.dateStr"
                          outlined
                          stack-label
                          hide-bottom-space
                          mask="##/##/####"
                          dense
                          :error="rowValidations[props.rowIndex]?.value?.dateStr.$error"
                          :error-message="rowValidations[props.rowIndex]?.value?.dateStr.$errors[0]?.$message"
                          @blur="rowValidations[props.rowIndex]?.value?.dateStr.$touch"
                        >
                          <template #append>
                            <q-icon name="o_calendar_month" class="cursor-pointer">
                              <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                                <q-date
                                  v-model="props.row.dateStr"
                                  mask="MM/DD/YYYY"
                                  @update:model-value="() => $refs.qDateProxy.hide()"
                                />
                              </q-popup-proxy>
                            </q-icon>
                          </template>
                        </q-input>
                      </q-td>
                      <q-td>
                        <q-input
                          v-model="props.row.likes"
                          outlined
                          stack-label
                          hide-bottom-space
                          input-class="text-right"
                          :rules="[validateCount]"
                          maxlength="9"
                          :error="rowValidations[props.rowIndex]?.value?.likes.$error"
                          :error-message="rowValidations[props.rowIndex]?.value?.likes.$errors[0]?.$message"
                          @blur="rowValidations[props.rowIndex]?.value?.likes.$touch"
                        />
                      </q-td>
                      <q-td>
                        <q-input
                          v-model="props.row.comments"
                          outlined
                          stack-label
                          hide-bottom-space
                          dense
                          input-class="text-right"
                          :rules="[validateCount]"
                          maxlength="9"
                          :error="rowValidations[props.rowIndex]?.value?.comments.$error"
                          :error-message="rowValidations[props.rowIndex]?.value?.comments.$errors[0]?.$message"
                          @blur="rowValidations[props.rowIndex]?.value?.comments.$touch"
                        />
                      </q-td>
                      <q-td>
                        <q-input
                          v-model="props.row.shares"
                          outlined
                          stack-label
                          hide-bottom-space
                          dense
                          input-class="text-right"
                          :rules="[validateCount]"
                          maxlength="9"
                          :error="rowValidations[props.rowIndex]?.value?.shares.$error"
                          :error-message="rowValidations[props.rowIndex]?.value?.shares.$errors[0]?.$message"
                          @blur="rowValidations[props.rowIndex]?.value?.shares.$touch"
                        />
                      </q-td>
                      <q-td class="text-center" style="width: 5%;">
                        <q-icon name="o_delete_outline" size="xs" class="cursor-pointer" color="negative" @click="deleteRow(props.rowIndex)">
                          <q-tooltip>Delete</q-tooltip>
                        </q-icon>
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
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" :disable="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, uid } from "quasar";
import useVuelidate from "@vuelidate/core";
import adPostService from "modules/marketing-ad-post/marketingAdPost.service";
import { required, helpers } from "@vuelidate/validators";
import { ref, onMounted, watch } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import { isDate } from "validators/zw_validators.js";
import adPostChannelService from "modules/marketing-ad-post-channel/marketingAdPostChannel.service";

// Common variables
const loading = ref(true);
const processing = ref(false);
const rowValidations = ref([]);

// Define model values
const model = ref({});

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

const props = defineProps({ id: { type: String, default: "" } });

// Table variables
const rows = ref([]);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "adPostChannelId", label: "Ad Channel Name", field: "adPostChannel.name", align: "left", sortable: true },
  { name: "dateStr", label: "Posted Date", field: "dateStr", align: "left", sortable: true },
  { name: "likes", label: "Likes", field: "likes", align: "left", sortable: true },
  { name: "comments", label: "Comments", field: "comments", align: "left", sortable: true },
  { name: "shares", label: "Shares", field: "shares", align: "left", sortable: true }
]);

const getAdPostingStatuses = (adId) => {
  adPostService.getAdPostingStatusesByAdId(adId).then((resp) => {
    rows.value = resp.map(item => ({
      ...item,
      adPostChannelId: item.adPostChannelId,
      dateStr: item.date,
      likes: Number(item.likes),
      comments: Number(item.comments),
      shares: Number(item.shares),
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

function onAdd () {
  rows.value.unshift({
    id: uid(),
    adPostChannelId: "",
    dateStr: "",
    likes: 0,
    comments: 0,
    shares: 0,
    deleted: false
  });
}

const deleteRow = (index) => {
  if (rows.value.filter(row => row.deleted === false).length > 1) {
    rows.value[index].deleted = true;
  } else {
    notifyError({ message: "Please add at least one row." });
  }
};

// =============================================================================
// DropDown
// =============================================================================
// Get all ad Post Channel list for dropdown
const adPostChannelList = ref([]);
const adPostChannelListOptions = ref([]);
function getAllAdPostChannelListForDropdown (counter) {
  adPostChannelService.getAllAdPostChannelListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.text, value: item.value }));
    adPostChannelList.value[counter] = responseData;
    adPostChannelListOptions.value[counter] = responseData;
  });
}

// Search ad Post Channel for dropdown
function adPostChannelListForFilter (val, update, abort, counter) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      adPostChannelList.value[counter] = adPostChannelListOptions.value[counter];
    } else {
      adPostChannelList.value[counter] = adPostChannelListOptions.value[counter].filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}
// =============================================================================

function isValidCount (value) {
  if (value === null || value === undefined) return false;

  const str = String(value).trim();

  // Only digits allowed (no dot, no minus)
  if (!/^\d+$/.test(str)) return false;

  const num = Number(str);
  return !isNaN(num);
}

function validateCount (value) {
  if (value != null && value !== undefined && String(value).trim() !== "") {
    return isValidCount(value) || "It must be a number";
  }
}
const numberOnly = helpers.withMessage(
  "It must be a valid number",
  isValidCount
);

const rules = {
  adPostChannelId: { required: helpers.withMessage("Channel is required", required) },
  dateStr: {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  },
  likes: { required: helpers.withMessage("likes count is required", required), numberOnly },
  comments: { required: helpers.withMessage("comments count is required", required), numberOnly },
  shares: { required: helpers.withMessage("shares count is required", required), numberOnly }
};

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    let isValid = true;
    const nonDeletedRows = rows.value.filter(row => !row.deleted);
    rowValidations.value = nonDeletedRows.map((row) =>
      useVuelidate(rules, row, { $lazy: true, $autoDirty: true })
    );
    // Validate each row
    for (const [index, validation] of rowValidations.value.entries()) {
      if (validation?.value) {
        await validation.value.$touch();
        const isRowValid = await validation.value.$validate();
        if (!isRowValid) {
          isValid = false;
        }
      } else {
        console.error(`Validation object for row ${index} is undefined`);
        isValid = false;
      }
    }
    if (isValid) {
      model.value.adPostingStatuses = rows.value;
      model.value.adId = props.id;
      await adPostService.saveAdPostingStatus(model.value);
      notifySuccess({ message: "Ad Posting Status is saved successfully." });
      onDialogOK();
    }
  } catch (error) {
    console.error("Error in submitting:", error);
  } finally {
    processing.value = false;
  }
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getAdPostingStatuses(props.id);
  }
}, { immediate: true });

// On page rendering
onMounted(() => {
  getAllAdPostChannelListForDropdown();
});
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
