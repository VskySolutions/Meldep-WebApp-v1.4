<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width:1100px !important; max-width: 100vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Assign Users to {{ props.reportName }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div :class="['q-pa-md cardTable']">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>User assign Info</legend>
              <div class="col-auto q-mb-sm" align="right">
                <q-btn color="primary" icon="o_add" label="Add" no-caps @click="addUserPermissionRow" />
              </div>
              <q-table
                ref="tableRef"
                v-model:pagination="pagination"
                virtual-scroll
                bordered :loading="loading"
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
                      {{ col.label }}<span v-if="['aspNetUserId'].includes(col.name)" class="required">*</span>
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
                      <q-select
                        v-model="props.row.aspNetUserId"
                        clearable
                        use-input
                        outlined
                        stack-label
                        hide-bottom-space
                        :dense="true"
                        :options="userNameList"
                        option-value="value"
                        option-label="text"
                        emit-value
                        map-options
                        :error="rowValidations[props.rowIndex]?.value?.aspNetUserId.$error"
                        :error-message="rowValidations[props.rowIndex]?.value?.aspNetUserId.$errors[0]?.$message"
                        @filter="getAllUserListForFilter"
                        @blur="rowValidations[props.rowIndex]?.value?.aspNetUserId.$touch"
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
                    </q-td>
                    <td class="text-center" style="width: 25%; vertical-align: middle;">
                      <q-checkbox v-model="props.row.fullAccess" color="primary" />
                    </td>
                    <td class="text-center" style="width: 25%; vertical-align: middle;">
                      <q-checkbox v-model="props.row.viewOnly" color="primary" />
                    </td>
                    <q-td class="text-center" style="width: 5%;">
                      <q-icon name="o_delete" size="xs" class="cursor-pointer text-red" @click="deleteUserPermissionRow(props.rowIndex)">
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
import { notifySuccess, notifyError } from "assets/utils";

import reportService from "modules/reports/reports.service";
import usersService from "modules/user-management/userManagement.service";

// Common variables
const loading = ref(true);
const processing = ref(false);
const UserRows = ref([]);
const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel, onDialogOK } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" }, reportName: { type: String, default: "" } });
const reportSettingsDetailId = props.id;
const selectAll = ref({
  fullAccess: false,
  viewOnly: false
});

const UserColumns = ref([
  { name: "aspNetUserId", label: "User", field: "aspNetUserId", sortable: false },
  { name: "fullAccess", label: "Manage Permission", field: "fullAccess" },
  { name: "viewOnly", label: "View Permission", field: "viewOnly" }
]);

const rowRules = {
  aspNetUserId: { required: helpers.withMessage("User is required", required) }
};
const rowValidations = ref([]);

// compute invalid row indexes
const invalidRows = computed(() =>
  UserRows.value.filter(row => !row.deleted).map((row, idx) =>
    !(row.fullAccess || row.viewOnly) ? idx : null
  ).filter(i => i !== null)
);

// get report user on edit mode
const getReportUserByReportSettingsDetailId = () => {
  loading.value = true;
  reportService.getReportUserByReportSettingsDetailId(reportSettingsDetailId).then((resp) => {
    UserRows.value = resp.map(item => ({
      ...item,
      id: item.id,
      editing: false,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};

function toggleAll (field, value) {
  UserRows.value.forEach(row => {
    row[field] = value;
  });
}

function addUserPermissionRow () {
  UserRows.value.push({ id: uid(), aspNetUserId: "", fullAccess: false, viewOnly: false, deleted: false });
}

const deleteUserPermissionRow = (index) => {
  UserRows.value[index].deleted = true;
};

// Get all user List for dropdown
const userNameList = ref([]);
const userNameListFilter = ref([]);
function getAllUserListForDropdown (siteId) {
  usersService.getAllUserListForDropdown(siteId, "US").then((resp) => {
    const responseData = resp
      .map((item) => ({ text: `${item.person.firstName} ${item.person.lastName}`, value: item.id }))
      .sort((a, b) => a.text.localeCompare(b.text));
    userNameList.value = responseData;
    userNameListFilter.value = responseData;
  });
}

function getAllUserListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      userNameList.value = userNameListFilter.value;
    } else {
      userNameList.value = userNameListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Submit form
const onSubmit = async () => {
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
    notifyError({ message: "Select at least one permission (Manage, View) for each row." });
    return;
  }

  if (isValid) {
    processing.value = true;
    // Prepare the payload as a plain object
    const payload = {
      reportUserMapping: UserRows.value
    };
    // Submit the payload
    reportService.saveReportUser(reportSettingsDetailId, payload).then((resp) => {
      notifySuccess({ message: "User assigned successfully." });
      onDialogOK();
    }).finally(() => {
      processing.value = false;
    });
  }
};

// On page rendering
onMounted(() => {
  getAllUserListForDropdown();
});

// watches a data property with the same name i.e. immediate effect
watch(() => reportSettingsDetailId, (newValue, oldValue) => {
  if (newValue) {
    getReportUserByReportSettingsDetailId();
  }
}, { immediate: true });

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
