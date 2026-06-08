<template>
  <q-dialog ref="dialogRef" class="customDialog DialogContainer" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1200px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ copyOrMove }} Task</div>
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
                  :options="projectNameDropdownSingleSelect.list.value"
                  :filter="projectNameDropdownSingleSelect.filter"
                  :error="v$.projectIds.$error"
                  :error-message="v$.projectIds.$errors[0]?.$message"
                  :onBlur="() => v$.projectIds.$touch()"
                  popup-content-class="customPopupContentClass"
                />
              </div>
              <div v-if="model.projectIds.length > 0">
                <label class="label q-mb-xs text-black">Task<span class="required">*</span></label>
                <q-table
                  ref="tableRef" v-model:pagination="pagination" flat bordered :loading="loading" :rows="projectTasks" :columns="columns" row-key="id" separator="cell"
                  binary-state-sort hide-pagination :rows-per-page-options="[20, 50, 100, 200, 500]" @request="getProjectTasks"
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                      <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 15%;">{{ props.row.name }}</q-td>
                      <q-td style="width: 5%;">{{ toDate(props.row.startDate) }}</q-td>
                      <q-td style="width: 5%;">{{ toDate(props.row.endDate) }}</q-td>
                    </q-tr>
                  </template>
                </q-table>
              </div>
            </fieldset>
          </div>
        </div>
        <q-card-actions align="center" class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import { ref, onMounted, watch, computed } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import taskService from "modules/project-tasks/projectTasks.service";
import useFilters from "composables/useFilters";
import { required, helpers } from "@vuelidate/validators";
import formMultiSelectDropdown from "src/components/form-inputs/_formMultiSelectDropdown.vue";

import projectModule from "src/modules/project/utils/dropdowns.js";

// Common variables
const processing = ref(false);
const { toDate } = useFilters();
const props = defineProps({ id: { type: String, default: "" }, name: { type: String, default: "" }, projectModuleId: { type: String, default: "" }, isMove: { type: String, default: "" }, isCopy: { type: String, default: "" } });
const projectTaskId = props.id;
const taskIds = props.id;
const projectModuleId = props.projectModuleId;
const copyOrMove = computed(() => (props.isCopy ? "Copy" : "Move"));
const loading = ref(true);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// Define model values
const model = ref({
  projectIds: [],
  taskId: taskIds,
  projectModuleId,
  isCopyOrMove: false
});

const projectTasks = ref([]);
const pagination = ref({ sortBy: "createdOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "name", label: "Task Name", field: "name", align: "left" },
  { name: "startDate", label: "Start Date", field: "startDate", align: "left" },
  { name: "endDate", label: "End Date", field: "endDate", align: "left" }
]);

const getProjectTasks = (props) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination;
  loading.value = true;
  const payload = { page, pageSize: rowsPerPage, sortBy, descending, projectTaskId, projectModuleId };
  taskService.getProjectTaskForCopy(payload).then((resp) => {
    projectTasks.value = resp.data;
    projectTasks.value = resp.data.map(task => ({
      ...task,
      checkboxStatus: false, // Initialize checkboxStatus for each row
      activity: task.projectActivities ? task.projectActivities.map(activity => ({
        ...activity
      })) : []
    }));
    pagination.value.page = page;
    pagination.value.rowsPerPage = rowsPerPage;
    pagination.value.sortBy = sortBy;
    pagination.value.descending = descending;
    pagination.value.rowsNumber = resp.total;
  }).finally(() => {
    loading.value = false;
  });
};

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const {
  projectNameDropdownSingleSelect
} = projectModule();

const closeDialog = () => {
  if (dialogRef.value) {
    dialogRef.value.hide();
  }
};

// validation rule
const rule = {
  projectIds: { required: helpers.withMessage("Project is required", required) }
};

// Validate rules
const v$ = useVuelidate(rule, model, { $lazy: true, $autoDirty: true });

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    if (await v$.value.$validate()) {
      processing.value = true;
      model.value.taskId = taskIds;
      model.value.isCopyOrMove = props.isCopy ? props.isCopy : props.isMove;
      taskService.copyTaskToProjects(model.value).then((resp) => {
        if (props.isCopy) {
          notifySuccess({ message: "Project task copied successfully." });
        } else {
          notifySuccess({ message: "Project task moved successfully." });
        }
        closeDialog();
      });
    }
  } catch (error) {
    console.error("Error in submitting the project users:", error);
    notifyError({ message: "An error occurred while assigning users to the project." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
};

watch(
  () => projectTasks.value,
  (newTasks) => {
    model.value.taskId = newTasks ? newTasks.id : null;
  },
  { immediate: true }
);

// On page rendering
onMounted(() => {
  projectNameDropdownSingleSelect.load();
  const propps = { pagination: pagination.value };
  getProjectTasks(propps);
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
