<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1100px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Assign Bulk</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable']">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>User assign Info</legend>
              <div class="row q-col-gutter-md items-center q-my-md">
                <formMultiSelectDropdown
                  v-model="model.employeeIds"
                  label="Employee Name"
                  :options="activeEmployeesDropdown.list.value"
                  :filter="activeEmployeesDropdown.filter"
                  :error="v$.employeeIds.$error"
                  :error-message="v$.employeeIds.$errors[0]?.$message"
                  :onBlur="() => v$.employeeIds.$touch()"
                  popup-content-class="customPopupContentClass"
                />
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
                <div class="col-3 q-mb-sm justify-end" align="right">
                  <label class="q-mb-xs q-ml-sm text-black">Select All</label>
                  <q-toggle
                    v-model="selectAllPermissions"
                    color="primary"
                    no-caps :disable="!model.projectIds || model.projectIds.length === 0"
                    @update:model-value="toggleAllPermissions"
                  />
                </div>
              </div>
              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                virtual-scroll
                bordered
                :loading="loading"
                :rows="UserRows"
                :columns="UserColumns"
                row-key="id"
                separator="cell"
                :rows-per-page-options="[20, 50, 100, 200, 500]"
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th
                      v-for="col in props.cols"
                      :key="col.name"
                      class="text-center"
                    >
                      {{ col.label }}
                      <template v-if="['fullAccess', 'viewOnly', 'notes'].includes(col.name)">
                        <q-checkbox
                          v-model="selectAll[col.name]"
                          dense
                          color="primary"
                          @update:model-value="toggleAll(col.name, $event)"
                        />
                      </template>
                    </q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :class="props.row.deleted ? 'hidden' : 'edit_tasks'">
                    <q-td style="width: 35%;">
                      <formSingleSelectDropdown
                        v-model="props.row.projectId"
                        :options="projectNameDropdownSingleSelect.list.value"
                        :filter="projectNameDropdownSingleSelect.filter"
                        :readonly="true"
                        :error="rowValidations[props.rowIndex]?.value?.projectId.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.projectId.$errors[0]?.$message"
                      />
                    </q-td>
                    <td class="text-center" style="width: 20%; vertical-align: middle;">
                      <q-checkbox v-model="props.row.fullAccess" color="primary" />
                    </td>
                    <td class="text-center" style="width: 20%; vertical-align: middle;">
                      <q-checkbox v-model="props.row.viewOnly" color="primary" />
                    </td>
                    <td class="text-center" style="width: 20%; vertical-align: middle;">
                      <q-checkbox v-model="props.row.notes" color="primary" />
                    </td>
                  </q-tr>
                </template>
              </q-table>
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
import { uid, useDialogPluginComponent } from "quasar";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import { useAuthStore } from "stores/auth";
import { notifySuccess, notifyError } from "assets/utils";
import projectService from "modules/project/projects.service";
import formMultiSelectDropdown from "src/components/form-inputs/_formMultiSelectDropdown.vue";
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

// Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel, onDialogOK } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const ProjectId = props.id;

// Common variables
const loading = ref(true);
const processing = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const UserRows = ref([]);
const model = ref({});
const selectAllPermissions = ref(false);
const previousProjectIds = ref([]);
const rowValidations = ref([]);
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });

const selectAll = ref({
  fullAccess: false,
  viewOnly: false,
  notes: false
});

// Tab Task Activities
const UserColumns = ref([
  { name: "aspNetUserId", label: "Project", field: "aspNetUserId", sortable: false },
  { name: "fullAccess", label: "Manage Permission", field: "fullAccess" },
  { name: "viewOnly", label: "View Permission", field: "viewOnly" },
  { name: "notes", label: "Notes Permission", field: "notes" }
]);

// get project user on edit mode
const getProjectUserByProjectId = async () => {
  loading.value = true;
  try {
    const resp = await projectService.getProjectUserByProjectId(ProjectId);

    UserRows.value = resp.map(item => ({
      ...item,
      id: item.id,
      editing: false,
      flag: "Edit"
    }));
  } catch (err) {
    notifyError({ message: "Failed to load data" });
  } finally {
    loading.value = false;
  }
};

function toggleAll (field, value) {
  UserRows.value.forEach(row => {
    row[field] = value;
  });
}

function toggleAllPermissions (value) {
  UserRows.value.forEach(row => {
    row.fullAccess = value;
    row.viewOnly = value;
    row.notes = value;
  });
  selectAll.value.fullAccess = value;
  selectAll.value.viewOnly = value;
  selectAll.value.notes = value;
}

// ------------------------------------------------------------------------------------
// Advance Filter :- All Dropdowns (SOP Change)
// ------------------------------------------------------------------------------------
const { activeEmployeesDropdown } = employeeModule();
const {
  projectNameDropdownSingleSelect
} = projectModule();

// ------------------------------------------------------------------------------------
// Validation rules
// ------------------------------------------------------------------------------------
const rules = {
  employeeIds: { required: helpers.withMessage("Employee is required", required) },
  projectIds: { required: helpers.withMessage("Project is required", required) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// compute invalid row indexes
const invalidRows = computed(() =>
  UserRows.value.map((row, idx) =>
    !(row.fullAccess || row.viewOnly || row.notes) ? idx : null
  ).filter(i => i !== null)
);

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    if (invalidRows.value.length > 0) {
      notifyError({
        message: "Select at least one permission (Manage, View, or Notes) for each row."
      });
      processing.value = false;
      return;
    }

    const isValid = await v$.value.$validate();
    if (!isValid) {
      processing.value = false;
      return;
    }

    const payload = {
      projectUserMappings: UserRows.value
    };

    // Submit the payload
    await projectService.assignBulk(model.value.employeeIds, payload);

    notifySuccess({ message: "User assigned successfully." });
    onDialogOK();
  } catch (error) {
    console.error("Error in submitting the project:", error);
    notifyError({ message: "An error occurred while saving the project." });
  } finally {
    processing.value = false;
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => ProjectId, (newValue, oldValue) => {
  if (newValue) {
    getProjectUserByProjectId();
  } else {
    loading.value = false;
  }
}, { immediate: true });

watch(
  () => model.value.projectIds || [],
  (newIds = []) => {
    // Add new projects
    const added = newIds.filter(id => !previousProjectIds.value.includes(id));
    for (const projectId of added) {
      const project = projectNameDropdownSingleSelect.list.value.find(p => p.value === projectId);
      if (!project) continue;

      const alreadyExists = UserRows.value.some(r => r.projectId === projectId);
      if (alreadyExists) continue;

      UserRows.value.push({
        id: uid(),
        projectId,
        projectName: project.text,
        aspNetUserId: "",
        fullAccess: false,
        viewOnly: false,
        notes: false,
        deleted: false
      });
    }

    // Remove deselected projects
    const removed = previousProjectIds.value.filter(id => !newIds.includes(id));
    if (removed.length > 0) {
      UserRows.value = UserRows.value.filter(row => !removed.includes(row.projectId));
    }

    // Update tracking
    previousProjectIds.value = [...newIds];
  },
  { immediate: true }
);

// On page rendering
onMounted(() => {
  projectNameDropdownSingleSelect.load();
  activeEmployeesDropdown.load(user.siteId);
});
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_tasks .q-select__dropdown-icon{
  display: none;
}
</style>
