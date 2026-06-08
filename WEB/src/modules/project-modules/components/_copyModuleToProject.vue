<template>
  <q-dialog ref="dialogRef" class="customDialog DialogContainer" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Copy {{ moduleName }} </div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable']">
          <div class="q-gutter-y-md">
            <fieldset>
              <div class="row q-col-gutter-x-md q-mb-md">
                <formMultiSelectDropdown
                  v-model="model.projectIds"
                  label="Project Name"
                  :options="projectNameDropdown.list.value"
                  :filter="projectNameDropdown.filter"
                  :error="v$.projectIds.$error"
                  :error-message="v$.projectIds.$errors[0]?.$message"
                  :onBlur="() => v$.projectIds.$touch()"
                  popup-content-class="customPopupContentClass"
                />
              </div>
              <!--select checkbox for task list-->
              <div v-if="model.projectIds.length !== 0" class="row q-gutter-x-md q-mb-md">
                <q-checkbox v-model="model.selectedTaskList" label="Select Tasks" color="primary" @update:model-value="onTaskListSelection" />
              </div>
              <!-- Task List Table -->
              <div v-if="model.selectedTaskList && model.projectIds.length > 0">
                <q-table v-model:pagination="pagination" :class="projectTasks.length === 0 ? 'Custom-DataTable' : 'Custom-DataTable'" :loading="loading" :rows="projectTasks" :columns="columns" row-key="id" separator="cell" binary-state-sort no-data-label="No data available" :filter="filter" :rows-per-page-options="[20, 50, 100, 200, 500]">
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th auto-width>
                        <q-checkbox v-model="selectAllTasks" @update:model-value="toggleAllTasks" />
                      </q-th>
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props">
                      <q-td auto-width>
                        <q-checkbox v-model="model.taskIds" :val="props.row.id" />
                      </q-td>
                      <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 100%;" class="RichTextEditor">
                        <div class="text-black RichTextEditor" v-html="props.row.name" />
                        <span style="font-size: 12px;">{{ "[" + props.row.status.dropDownValue + "]" }}</span>
                      </q-td>
                      <q-td>{{ toDate(props.row.startDate) }}</q-td>
                      <q-td>{{ toDate(props.row.endDate) }}</q-td>
                    </q-tr>
                  </template>
                </q-table>
              </div>
            </fieldset>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
      <!-- </q-card-section> -->
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import { ref, onMounted, watch } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import taskService from "modules/project-tasks/projectTasks.service";
import useFilters from "composables/useFilters";
import { required, helpers } from "@vuelidate/validators";

import projectModule from "src/modules/project/utils/dropdowns.js";

import formMultiSelectDropdown from "src/components/form-inputs/_formMultiSelectDropdown.vue";

// Common variables
const processing = ref(false);
const { toDate } = useFilters();
const props = defineProps({ id: { type: String, default: "" }, name: { type: String, default: "" } });
const projectModuleId = props.id;
const moduleName = props.name;
const selectAllTasks = ref(false);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Define model values
const model = ref({
  projectIds: [],
  taskIds: [],
  projectModuleId,
  selectedTaskList: false
});

const projectTasks = ref([]);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "name", label: "Task Name", field: "name", align: "left", sortable: true },
  { name: "startDate", label: "Start Date", field: "startDate", align: "left", sortable: true },
  { name: "endDate", label: "End Date", field: "endDate", align: "left", sortable: true }
]);

// Function to fetch tasks when Task List is selected
function onTaskListSelection (val) {
  if (val) {
    getTasksByProjectModule(projectModuleId);
  } else {
    projectTasks.value = []; // Clear tasks when checkbox is unchecked
  }
}

// Function to fetch tasks by module ID
function getTasksByProjectModule (projectModuleId) {
  taskService.getTasksByProjectModule(projectModuleId).then((resp) => {
    projectTasks.value = resp;
  });
}

function toggleAllTasks () {
  if (selectAllTasks.value) {
    model.value.taskIds = projectTasks.value.map(task => task.id);
  } else {
    model.value.taskIds = [];
  }
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectNameDropdown
} = projectModule();

// ------------------------------------------------------------------------------------
// Validate rules
// ------------------------------------------------------------------------------------
const rule = {
  projectIds: { required: helpers.withMessage("Project is required", required) }
};

const v$ = useVuelidate(rule, model, { $lazy: true, $autoDirty: true });

const onSubmit = async () => {
  processing.value = true;

  try {
    const isValid = await v$.value.$validate();
    if (!isValid) {
      processing.value = false;
      return;
    }

    await taskService.copyModuleToProjects(model.value);

    notifySuccess({
      message: "Project module & tasks copied successfully."
    });

    onDialogOK();

  } catch (error) {
    console.error("Error while copying project module and tasks:", error);

    notifyError({
      message: "An error occurred while copying project module and tasks."
    });
  } finally {
    processing.value = false;
  }
};

watch(
  () => projectTasks.value,
  (newTasks) => {
    model.value.taskIds = newTasks.map(task => task.id);
  },
  { immediate: true }
);

// On page rendering
onMounted(() => {
  projectNameDropdown.load();
});
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_projectModule .q-select__dropdown-icon{
  display: none;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
</style>
