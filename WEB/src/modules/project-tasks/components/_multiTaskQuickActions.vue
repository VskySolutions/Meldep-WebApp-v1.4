<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:25vw !important; max-width: 25vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Multi Tasks {{ selectedField }} Change</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <div v-if="selectedField == 'Status'">
                <div class="row items-center q-mb-sm">
                  <div class="col-lg-3 col-md-3 col-sm-6 col-xs-6">
                    <label class="label q-mb-xs text-black">Status</label>
                  </div>
                  <div class="col-lg-9 col-md-9 col-sm-6 col-xs-6">
                    <formSingleSelectDropdown
                      v-model="model.statusId"
                      :options="projectTaskStatusList"
                      :error="v$.statusId.$error"
                      :error-message="v$.statusId.$errors[0]?.$message"
                    />
                  </div>
                </div>
              </div>
              <div v-if="selectedField == 'Priority'">
                <div class="row items-center q-mb-sm">
                  <div class="col-lg-3 col-md-3 col-sm-6 col-xs-6">
                    <label class="Cutomlabel q-mt-sm fs-13">Priority</label>
                  </div>
                  <div class="col-lg-9 col-md-9 col-sm-6 col-xs-6">
                    <formSingleSelectDropdown
                      v-model="model.priorityId"
                      :options="projectTaskPriorityForDropdownSingleSelect.list.value"
                      :filter="projectTaskPriorityForDropdownSingleSelect.filter"
                      :error="v$.priorityId.$error"
                      :error-message="v$.priorityId.$errors[0]?.$message"
                    />
                  </div>
                </div>
              </div>
              <div v-if="selectedField == 'Tags'">
                <div class="row items-center q-mb-sm">
                  <div class="col-lg-3 col-md-3 col-sm-6 col-xs-6">
                    <label class="Cutomlabel q-mt-sm fs-13">Add tags</label>
                  </div>
                  <div class="col-lg-9 col-md-9 col-sm-6 col-xs-6">
                    <formMultiSelectDropdown
                      v-model="model.taskTags"
                      :options="tagsDropdown.list.value"
                      :filter="tagsDropdown.filter"
                      :error="v$.taskTags.$error"
                      :error-message="v$.taskTags.$errors[0]?.$message"
                      :onBlur="() => v$.taskTags.$touch()"
                    />
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
import { ref, onMounted, computed, watch } from "vue";
import { notifyError } from "assets/utils";
import commonService from "services/common.service";
import projectTaskModule from "src/modules/project-tasks/utils/dropdowns.js";
import tagModule from "src/modules/tags/utils/dropdowns.js";

// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formMultiSelectDropdown from "src/components/form-inputs/_formMultiSelectDropdown.vue";

// Shared Project Task Actions
import {
  onSubmitProjectTaskStatus,
  onSubmitProjectTaskPriority,
  onSubmitProjectTaskTags
} from "src/modules/project-tasks/utils/actions.js";

// Common variables
const processing = ref(false);

// Props values i.e. come from query string
const props = defineProps({ taskIds: { type: Array, required: true }, selectedField: { type: String, default: "" }, status: { type: String, default: "" } });
const selectedField = props.selectedField;
const taskIds = props.taskIds;
const inputValue = ref("");
// Define emits
const $emit = defineEmits(["hide", "ok"]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();
const model = ref({
  statusId: null,
  priorityId: null,
  taskTags: []
});

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectTaskPriorityForDropdownSingleSelect
} = projectTaskModule();

const { tagsDropdown } = tagModule();
// Get all project task status List
const projectTaskStatusList = ref([]);
function getTaskStatuses (typeName, projectStatusLabel = null) {
  commonService.getDropDown(typeName).then((resp) => {
    const lockedStatuses = ["Cancelled", "Completed", "On Hold"];
    // const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    const responseData = resp.map((item) => {
      const label = item.dropdownValue;
      let shouldDisable = false;

      if (lockedStatuses.includes(projectStatusLabel)) {
        shouldDisable = label === "Open";
      }
      if (projectStatusLabel === "New") { shouldDisable = label === "Open"; }

      return {
        text: item.dropdownValue,
        value: item.id,
        disable: shouldDisable
      };
    });

    projectTaskStatusList.value = responseData;
  });
}

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

  if (selectedField === "Tags") {
    baseRules.taskTags = {
      required: helpers.withMessage("Tags is required", required)
    };
  }

  return baseRules;
});

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// --------------------------------------
async function onSubmit () {
  processing.value = true;
  try {
    if (!await v$.value.$validate()) {
      return;
    }

    if (selectedField === "Status") {
      await onSubmitProjectTaskStatus(
        projectTaskStatusList.value,
        taskIds,
        model.value.statusId,
        () => {
          $emit("ok");
          $emit("hide");
        }
      );
    }

    if (selectedField === "Priority") {
      await onSubmitProjectTaskPriority(
        taskIds,
        model.value.priorityId,
        () => {
          $emit("ok");
          $emit("hide");
        }
      );
    }

    if (selectedField === "Tags") {
      const tagInput = model.value.taskTags;

      const selectedTags = tagsDropdown.list.value.filter(tag =>
        tagInput.includes(tag.value)
      );

      await onSubmitProjectTaskTags(
        taskIds,
        selectedTags,
        () => {
          $emit("ok");
          $emit("hide");
        },
        () => tagsDropdown.load(),
        "isMultiSelectTask"
      );
    }
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while submitting." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

watch(inputValue, (val) => {
  const needle = val ? val.toLowerCase() : "";
  tagsDropdown.list.value = needle
    ? options9.value.filter((v) => v.text.toLowerCase().includes(needle))
    : [...options9.value];
});

// On page rendering
onMounted(() => {
  getTaskStatuses("Task Status", props.status);
  projectTaskPriorityForDropdownSingleSelect.load("Task Priorities");
  tagsDropdown.load()
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
