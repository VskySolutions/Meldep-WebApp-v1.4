<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1100px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Add Users to the Project Module of {{ ProjectName }}</div>
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
                  v-model="model.userIds"
                  label="Employee Name"
                  :options="projectUsersByProjectIdForDropdown.list.value"
                  :filter="projectUsersByProjectIdForDropdown.filter"
                  :error="v$.userIds.$error"
                  :error-message="v$.userIds.$errors[0]?.$message"
                  :onBlur="() => v$.userIds.$touch()"
                  popup-content-class="customPopupContentClass"
                />
                <formMultiSelectDropdown
                  v-model="model.projectModuleIds"
                  label="Project Module Name"
                  :options="projectModulesByProjectIdForDropdown.list.value"
                  :filter="projectModulesByProjectIdForDropdown.filter"
                  :error="v$.projectModuleIds.$error"
                  :error-message="v$.projectModuleIds.$errors[0]?.$message"
                  :onBlur="() => v$.projectModuleIds.$touch()"
                  popup-content-class="customPopupContentClass"
                />
                <div class="col-3 q-mb-sm justify-end" align="right">
                  <label class="q-mb-xs q-ml-sm text-black">Select All</label>
                  <q-toggle
                    v-model="selectAllPermissions"
                    color="primary"
                    no-caps :disable="!model.projectModuleIds || model.projectModuleIds.length === 0"
                    @update:model-value="toggleAllPermissions"
                  />
                </div>
              </div>
              <q-table
                ref="tableRef" v-model:pagination="pagination"
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
                      :props="props"
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
                        v-model="props.row.projectModuleId"
                        :options="projectModulesByProjectIdForDropdown.list.value"
                        :filter="projectModulesByProjectIdForDropdown.filter"
                        :readonly="true"
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
            <fieldset>
              <legend>Existing users Info</legend>
              <q-table
                ref="tableRef" v-model:pagination="pagination"
                virtual-scroll
                bordered
                :loading="loading"
                :rows="ExistingUserRows"
                :columns="ExistUserColumns"
                row-key="id"
                separator="cell"
                :rows-per-page-options="[20, 50, 100, 200, 500]"
                binary-state-sort
              >
                <template #header="props">
                  <q-tr :props="props" class="bg-primary text-white">
                    <q-th
                      v-for="col in props.cols"
                      :key="col.name"
                      class="text-center"
                      :props="props"
                    >
                      {{ col.label }}
                    </q-th>
                    <q-th auto-width class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :class="props.row.deleted ? 'hidden' : 'edit_tasks'">
                    <q-td style="width: 35%;">
                      <formSingleSelectDropdown
                        v-model="props.row.userIds"
                        :options="projectUserByProjectIdDropdownSingleSelect.list.value"
                        :filter="projectUserByProjectIdDropdownSingleSelect.filter"
                        :readonly="true"
                      />
                    </q-td>
                    <q-td style="width: 35%;">
                      <formSingleSelectDropdown
                        v-model="props.row.projectModuleId"
                        :options="projectModulesByProjectIdForDropdownSingleSelect.list.value"
                        :filter="projectModulesByProjectIdForDropdownSingleSelect.filter"
                        :readonly="true"
                      />
                    </q-td>
                    <td class="text-center" style="width: 20%; vertical-align: middle;">
                      <q-checkbox
                        v-model="props.row.fullAccess"
                        color="primary"
                        @update:model-value="() => updateUserProjectModuleAccess(props.row)"
                      />
                    </td>

                    <td class="text-center" style="width: 20%; vertical-align: middle;">
                      <q-checkbox
                        v-model="props.row.viewOnly"
                        color="primary"
                        @update:model-value="() => updateUserProjectModuleAccess(props.row)"
                      />
                    </td>

                    <td class="text-center" style="width: 20%; vertical-align: middle;">
                      <q-checkbox
                        v-model="props.row.notes"
                        color="primary"
                        @update:model-value="() => updateUserProjectModuleAccess(props.row)"
                      />
                    </td>
                    <q-td style="width: 5%;" class="text-center actions">
                      <q-icon name="o_delete_outline" size="xs" class="cursor-pointer q-ml-sm" color="negative" @click="onSubmitProjectModuleUserDelete(props.row.id, props.row.user.person.fullName, props.row.projectModule.name, refreshModuleUsersByProjectIdList)">
                        <q-tooltip>Delete</q-tooltip>
                      </q-icon>
                    </q-td>
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
import { notifySuccess, notifyError, zwConfirmDelete } from "assets/utils";
import projectModuleService from "../projectModules.service";
import formMultiSelectDropdown from "src/components/form-inputs/_formMultiSelectDropdown.vue";
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import projectModuleOfProjectModule from "src/modules/project-modules/utils/dropdowns.js";
import projectModule from "src/modules/project/utils/dropdowns.js";

// SOP Change :- Shared Project Actions
import {
  initProjectModuleActions,
  onSubmitProjectModuleUserDelete
} from "src/modules/project-modules/utils/actions.js";

// Common variables
const loading = ref(true);
const processing = ref(false);
const UserRows = ref([]);
const ExistingUserRows = ref([]);
const previousProjectModuleIds = ref([]);
const model = ref({});
const selectAllPermissions = ref(false);
const activeRowId = ref(null);
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel, onDialogOK } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, projectName: { type: String, default: "" } });
const ProjectId = props.id;
const ProjectName = props.projectName;

// Tab Task Activities
const UserColumns = ref([
  { name: "projectModuleName", label: "Project Module", field: "projectModuleName", align: "center", sortable: true },
  { name: "fullAccess", label: "Manage Permission", field: "fullAccess", align: "center", sortable: false },
  { name: "viewOnly", label: "View Permission", field: "viewOnly", align: "center", sortable: false },
  { name: "notes", label: "Notes Permission", field: "notes", align: "center", sortable: false }
]);

const ExistUserColumns = ref([
  { name: "aspNetUserId", label: "Employee Name", field: "userName", align: "center", sortable: true },
  { name: "projectModuleId", label: "Project Module", field: "moduleName", align: "center", sortable: true },
  { name: "userFullAccess", label: "Manage Permission", field: "fullAccess", align: "center", sortable: true },
  { name: "viewOnly", label: "View Permission", field: "viewOnly", align: "center", sortable: false },
  { name: "notes", label: "Notes Permission", field: "notes", align: "center", sortable: false }
]);

const selectAll = ref({
  fullAccess: false,
  viewOnly: false,
  notes: false
});

// get project module user on edit mode
const getModuleUsersByProjectId = () => {
  loading.value = true;
  projectModuleService.getModuleUsersByProjectId(ProjectId).then((resp) => {
    ExistingUserRows.value = resp.map(item => ({
      ...item,
      id: item.id,
      userIds: item.user.id,
      userName: item.user.person.firstName,
      moduleName: item.projectModule.name,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

const refreshModuleUsersByProjectIdList = () => {
  getModuleUsersByProjectId();
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
// DataTable:- Initialization Of Dialogs, Actions (SOP Change)
// ------------------------------------------------------------------------------------
initProjectModuleActions(activeRowId);

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const {
  projectModulesByProjectIdForDropdown,
  projectModulesByProjectIdForDropdownSingleSelect
} = projectModuleOfProjectModule();

const {
  projectUsersByProjectIdForDropdown,
  projectUserByProjectIdDropdownSingleSelect
} = projectModule();

// ----------------------------------------------------------------------------------------------------------------
// Validation rules
// ----------------------------------------------------------------------------------------------------------------
const rules = {
  userIds: { required: helpers.withMessage("Employee is required", required) },
  projectModuleIds: { required: helpers.withMessage("Project Module is required", required) }
};

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// compute invalid row indexes
const invalidRows = computed(() =>
  UserRows.value.map((row, idx) =>
    !(row.fullAccess || row.viewOnly || row.notes) ? idx : null
  ).filter(i => i !== null)
);

function updateUserProjectModuleAccess (row) {
  // Check if all three permissions are false
  if (!row.fullAccess && !row.viewOnly && !row.notes) {
    notifyError({ message: "Select at least one permission (Manage, View, or Notes) for each row." });
  } else {
    projectModuleService.updateUserProjectModuleAccess(row.id, row)
      .then(() => {
        notifySuccess({ message: "Permission updated successfully." });
      })
      .catch((err) => {
        if (err.response?.status !== 404) {
          notifyError({ message: "Something went wrong." });
        }
      });
  }
}

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
      projectModulesUserMappings: UserRows.value
    };

    await projectModuleService.assignBulk(model.value.userIds, payload);

    notifySuccess({ message: "User assigned successfully." });
    onDialogOK();

  } catch (err) {
    if (err.response?.status !== 404) {
      notifyError({ message: "Something went wrong." });
    }
  } finally {
    processing.value = false;
  }
};

// // watches a data property with the same name i.e. immediate effect
watch(() => ProjectId, (newValue, oldValue) => {
  if (newValue) {
    getModuleUsersByProjectId();
  }
}, { immediate: true });

watch(
  () => model.value.projectModuleIds || [],
  (newIds = []) => {
    // Add new projects
    const added = newIds.filter(id => !previousProjectModuleIds.value.includes(id));
    for (const projectModuleId of added) {
      const projectModule = projectModulesByProjectIdForDropdown.list.value.find(p => p.value === projectModuleId);
      if (!projectModule) continue;

      const alreadyExists = UserRows.value.some(r => r.projectModuleId === projectModuleId);
      if (alreadyExists) continue;

      UserRows.value.push({
        id: uid(),
        projectModuleId,
        projectModuleName: projectModule.text,
        aspNetUserId: "",
        fullAccess: false,
        viewOnly: false,
        notes: false,
        deleted: false
      });
    }

    // Remove deselected projects
    const removed = previousProjectModuleIds.value.filter(id => !newIds.includes(id));
    if (removed.length > 0) {
      UserRows.value = UserRows.value.filter(row => !removed.includes(row.projectModuleId));
    }

    // Update tracking
    previousProjectModuleIds.value = [...newIds];
  },
  { immediate: true }
);

// On page rendering
onMounted(() => {
  if (ProjectId) {
    projectModulesByProjectIdForDropdown.load(false, false, ProjectId);
    projectModulesByProjectIdForDropdownSingleSelect.load(false, false, ProjectId);
    projectUsersByProjectIdForDropdown.load(ProjectId);
    projectUserByProjectIdDropdownSingleSelect.load(ProjectId);
  }
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
