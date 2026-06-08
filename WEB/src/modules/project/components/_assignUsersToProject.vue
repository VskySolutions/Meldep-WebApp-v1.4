<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1100px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Assign Users to {{ props.projectName }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable']">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>User assign Info</legend>
              <div class="col-auto q-mb-sm" align="right">
                <q-btn color="primary" icon="o_add" label="Add" no-caps @click="onAdd" />
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
                      {{ col.label }}<span v-if="['userName'].includes(col.name)" class="required">*</span>
                      <template v-if="['fullAccess', 'viewOnly', 'notes'].includes(col.name)">
                        <q-checkbox
                          v-model="selectAll[col.name]"
                          dense
                          color="primary"
                          @update:model-value="toggleAll(col.name, $event)"
                        />
                      </template>
                    </q-th>
                    <q-th class="text-center">Actions</q-th>
                  </q-tr>
                </template>
                <template #body="props">
                  <q-tr :class="props.row.deleted ? 'hidden' : ''">
                    <q-td style="width: 5%;">
                      <formSingleSelectDropdown
                        v-model="props.row.aspNetUserId"
                        :options="userDropdownSingleSelect.list.value"
                        :filter="userDropdownSingleSelect.filter"
                        :error="rowValidations[props.rowIndex]?.value?.aspNetUserId.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.aspNetUserId.$errors[0]?.$message"
                      />
                    </q-td>
                    <td class="text-center" style="width: 25%; vertical-align: middle;">
                      <q-checkbox v-model="props.row.fullAccess" color="primary" />
                    </td>
                    <td class="text-center" style="width: 25%; vertical-align: middle;">
                      <q-checkbox v-model="props.row.viewOnly" color="primary" />
                    </td>
                    <td class="text-center" style="width: 25%; vertical-align: middle;">
                      <q-checkbox v-model="props.row.notes" color="primary" />
                    </td>
                    <q-td class="text-center" style="width: 5%;">
                      <q-icon name="o_delete" size="xs" class="cursor-pointer text-red" @click="deleteRow(props.rowIndex)">
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
import { useAuthStore } from "stores/auth";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import { notifySuccess, notifyError } from "assets/utils";
import projectService from "modules/project/projects.service";
import userModule from "src/modules/user-management/utils/dropdowns.js";
// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

// Common variables
const loading = ref(true);
const processing = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
const UserRows = ref([]);
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel, onDialogOK } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, projectName: { type: String, default: "" } });
const ProjectId = props.id;
const selectAll = ref({
  fullAccess: false,
  viewOnly: false,
  notes: false
});
// const selectAllPermissions = ref(false);

function toggleAll (field, value) {
  UserRows.value.forEach(row => {
    row[field] = value;
  });
}

// function toggleAllPermissions (value) {
//   UserRows.value.forEach(row => {
//     row.fullAccess = value;
//     row.viewOnly = value;
//     row.notes = value;
//   });
//   selectAll.value.fullAccess = value;
//   selectAll.value.viewOnly = value;
//   selectAll.value.notes = value;
// }
// get project user on edit mode
const getProjectUserByProjectId = () => {
  loading.value = true;
  projectService.getProjectUserByProjectId(ProjectId).then((resp) => {
    UserRows.value = resp.map(item => ({
      ...item,
      id: item.id,
      userName: item.user.person.firstName,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};
// watches a data property with the same name i.e. immediate effect
watch(() => ProjectId, (newValue, oldValue) => {
  if (newValue) {
    getProjectUserByProjectId();
  }
}, { immediate: true });

// Tab Task Activities
const UserColumns = ref([
  { name: "userName", label: "User", field: "userName", align: "center", sortable: true },
  { name: "fullAccess", label: "Manage Permission", field: "fullAccess", align: "center", sortable: true },
  { name: "viewOnly", label: "View Permission", field: "viewOnly", align: "center", sortable: true },
  { name: "notes", label: "Notes Permission", field: "notes", align: "center", sortable: true }
]);

function onAdd () {
  UserRows.value.unshift({ id: uid(), aspNetUserId: "", fullAccess: false, viewOnly: false, notes: false, deleted: false });
}
const deleteRow = (index) => {
  UserRows.value[index].deleted = true;
};

// ----------------------------------------------------------------------------------------------------------------
// Advance Filter:- Initialization Of All DropDowns
// ----------------------------------------------------------------------------------------------------------------
const { userDropdownSingleSelect } = userModule();

const rowRules = {
  aspNetUserId: { required: helpers.withMessage("User is required", required) }
};
const rowValidations = ref([]);

// compute invalid row indexes
const invalidRows = computed(() =>
  UserRows.value.filter(row => !row.deleted).map((row, idx) =>
    !(row.fullAccess || row.viewOnly || row.notes) ? idx : null
  ).filter(i => i !== null)
);

// Submit form
const onSubmit = async () => {
  processing.value = true;
  try {
    let isValid = true;
    if (UserRows.value.length === 0) {
      notifyError({ message: "Add at least one user." });
      return;
    }
    const nonDeletedRows = UserRows.value.filter(row => !row.deleted);
    // Initialize validations for all rows
    rowValidations.value = nonDeletedRows.map((row) =>
      useVuelidate(rowRules, row, { $lazy: true, $autoDirty: true })
    );

    // Validate each row
    for (const [index, validation] of rowValidations.value.entries()) {
      if (validation?.value) {
        await validation.value.$touch(); // Mark the row as touched
        const isRowValid = await validation.value.$validate(); // Validate the row
        if (!isRowValid) {
          isValid = false; // If any row is invalid, set isValid to false
        }
      } else {
        console.error(`Validation object for row ${index} is undefined`);
        isValid = false;
      }
    }

    if (!isValid) {
      return; // Prevent submission
    }

    if (invalidRows.value.length > 0) {
      notifyError({ message: "Select at least one permission (Manage, View, or Notes) for each row." });
      return;
    }

    if (isValid) {
      processing.value = true;
      // Prepare the payload as a plain object
      const payload = {
        projectUserMappings: UserRows.value
      };
      // Submit the payload
      projectService.saveProjectUser(ProjectId, payload).then((resp) => {
        notifySuccess({ message: "User assigned successfully." });
        onDialogOK();
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

// On page rendering
onMounted(() => {
  userDropdownSingleSelect.load(user.siteId, "US");
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
