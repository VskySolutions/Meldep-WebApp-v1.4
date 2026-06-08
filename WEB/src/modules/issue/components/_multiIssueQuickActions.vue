<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic"  style="width:25vw !important; max-width: 25vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Multi Issue {{ selectedField }} Change</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <!-- <legend>Task Info</legend> -->
              <div v-if="selectedField == 'Status'">
                <div class="row items-center q-mb-sm">
                  <div class="col-lg-3 col-md-3 col-sm-6 col-xs-6">
                    <label class="Cutomlabel q-mt-sm fs-13">Status</label>
                  </div>
                  <div class="col-lg-9 col-md-9 col-sm-6 col-xs-6">
                    <q-select v-model="model.statusId" clearable use-input outlined stack-label hide-bottom-space :dense="true"
                      :options="issueStatusList" option-value="value" option-label="text" :option-disable="disableOption" emit-value map-options
                      :error="v$.statusId.$error" :error-message="v$.statusId.$errors[0]?.$message" @blur="v$.statusId.$touch"
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
                </div>
              </div>
              <div v-if="selectedField == 'Priority'">
                <div class="row items-center q-mb-sm">
                  <div class="col-lg-3 col-md-3 col-sm-6 col-xs-6">
                    <label class="Cutomlabel q-mt-sm fs-13">Priority</label>
                  </div>
                  <div class="col-lg-9 col-md-9 col-sm-6 col-xs-6">
                    <q-select
                      v-model="model.priorityId" clearable use-input outlined stack-label hide-bottom-space :dense="true" :options="priorityList" option-value="value" option-label="text" emit-value map-options :error="v$.priorityId.$error"
                      :error-message="v$.priorityId.$errors[0]?.$message" @filter="filterFn4" @blur="v$.priorityId.$touch"
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
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="stickyFooter">
          <q-btn color="grey-4" push outline label="CANCEL" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="SET" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, onMounted, computed } from "vue";
import { notifySuccess } from "assets/utils";
import commonService from "services/common.service";
import issuesService from "modules/issue/issue.service";

// Common variables
const processing = ref(false);

// Props values i.e. come from query string
const props = defineProps({ issueIds: { type: Array, required: true }, selectedField: { type: String, default: "" } });
const selectedField = props.selectedField;
const issueIds = props.issueIds;

// Define emits
const $emit = defineEmits(["hide", "ok"]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();
const model = ref({
  statusId: null,
  priorityId: null
});

const rules = computed(() => {
  const baseRules = {};

  if (selectedField === "Status") {
    baseRules.statusId = {
      required: helpers.withMessage("Status is required", required)
    };
  }
  if (selectedField === "Priority") {
    baseRules.priorityId = {
      required: helpers.withMessage("Priority is required", required)
    };
  }
  return baseRules;
});

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Get all issue status List
const issueStatusList = ref([]);
function getDropDownIssues (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    issueStatusList.value = responseData;
  });
}

// Get all issue priority List
const priorityList = ref([]);
function getDropDownPriority (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    priorityList.value = responseData;
  });
}
// --------------------------------------

async function onSubmit () {
  if (!await v$.value.$validate()) {
    return;
  }

  if (selectedField === "Status") {
    await submitStatusUpdate();
  }

  if (selectedField === "Priority") {
    await submitPriorityUpdate();
  }
}

async function submitStatusUpdate () {
  const payload = {
    issueIds: issueIds,
    statusId: model.value.statusId
  };

  await issuesService.updateIssueStatus(payload);
  notifySuccess({ message: "Status updated successfully." });
  $emit("ok");
  $emit("hide");
}

async function submitPriorityUpdate () {
  const payload = {
    issueIds: issueIds,
    priorityId: model.value.priorityId
  };

  await issuesService.updateIssuePriority(payload);
  notifySuccess({ message: "Priority updated successfully." });
  $emit("ok");
  $emit("hide");
}

// On page rendering
onMounted(() => {
  getDropDownIssues("Issue Status");
  getDropDownPriority("Issue Priority");
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
