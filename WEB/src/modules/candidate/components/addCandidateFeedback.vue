<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1250px; max-height: 100% !important;max-width: 250vw;height: 100%;"
    >
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Add Candidate Feedback</div>
        <q-btn v-close-popup icon="o_close" color="white" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Candidate Feedback Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-lg-3 col-md-3 col-sm-4 col-xs-4">
                  <formDate
                    v-model="model.dueDate"
                    label="Date"
                    :error="v$.dueDate.$error"
                    :error-message="v$.dueDate.$errors[0]?.$message"
                    :onBlur="() => v$.dueDate.$touch()"
                  />
                </div>
                <div class="col-lg-4 col-md-4 col-sm-4 col-xs-4">
                  <formSingleSelectDropdown
                    v-model="model.employeeOwnerId"
                    label="Owner"
                    :options="activeEmployeesDropdownSingleSelect.list.value"
                    :filter="activeEmployeesDropdownSingleSelect.filter"
                    :error="v$.employeeOwnerId.$error"
                    :error-message="v$.employeeOwnerId.$errors[0]?.$message"
                  />
                </div>
                <div class="col-lg-5 col-md-5 col-sm-4 col-xs-4">
                  <formSingleSelectDropdown
                    v-model="model.questionId"
                    label="Question"
                    :options="candidateQuestionDropdownSingleSelect.list.value"
                    :filter="candidateQuestionDropdownSingleSelect.filter"
                    :error="v$.questionId.$error"
                    :error-message="v$.questionId.$errors[0]?.$message"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-lg-12 col-md-12 col-sm-12 col-xs-12">
                  <div class="q-mb-xs text-black">Answer<span class="required">*</span></div>
                  <q-editor
                    v-model="model.answer"
                    class="full-width "
                    y:dense="$q.screen.lt.md"
                    :toolbar="toolbar"
                    :fonts="fonts"
                  />
                  <div class="fs-12 q-mt-xs">
                    <div role="alert" class="text-negative">
                      {{ answerErrorMessage }}
                    </div>
                  </div>
                </div>
              </div>
              <q-card-actions align="center" class="q-mt-md">
                <q-btn
                  color="grey-4"
                  push
                  outline
                  label="Close"
                  type="button"
                  class="text-grey-9 actionBtn"
                  no-caps
                  @click="onDialogCancel"
                />
                <q-btn
                  color="primary"
                  push
                  outline
                  label="Save"
                  type="submit"
                  class="actionBtn"
                  :loading="processing"
                  :disable="processing"
                  no-caps
                />
              </q-card-actions>
            </fieldset>
            <fieldset>
              <legend>Feedback Log</legend>
              <div class="row">
                <div class="col">
                  <q-table
                    ref="tableRef"
                    v-model:pagination="pagination"
                    virtual-scroll
                    class="border Custom-DataTable"
                    :loading="loading"
                    :rows="rows"
                    :columns="columns"
                    row-key="id"
                    separator="cell"
                    no-data-label="No data available"
                    binary-state-sort
                    :rows-per-page-options="[20, 50, 100, 200, 500]"
                    @request="getRoles"
                  >
                    <template #header="props">
                      <q-tr :props="props" class="bg-primary text-white">
                        <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                        <q-th auto-width class="text-center">Actions</q-th>
                      </q-tr>
                    </template>
                    <template #body="props">
                      <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                        <q-td style="width: 5%;">
                          {{ props.row.dueDate }}
                        </q-td>
                        <q-td style="width: 10%;">
                          {{ props.row.employee.person.fullName }}
                        </q-td>
                        <q-td style="width: 20%; white-space: break-spaces;">
                          {{ props.row.candidateQuestions.dropDownValue }}
                        </q-td>
                        <q-td style="width: 30%; white-space: break-spaces;" @click="showComment(props.row.answer, true)">
                          <div class="clamped-text" v-html="removeLeadingSpaces(props.row.answer)" />
                        </q-td>
                        <q-td style="width: 5%;" class="text-center actions">
                          <q-icon
                            name="o_edit"
                            class="cursor-pointer q-mr-sm"
                            :class="storedUser.username === props.row.user.userName ? '' : 'hidden'"
                            @click="onEdit(props.row)"
                          >
                            <q-tooltip>Edit</q-tooltip>
                          </q-icon>
                          <q-icon
                            name="o_delete_outline"
                            class="cursor-pointer"
                            color="negative"
                            :class="storedUser.username === props.row.user.userName ? '' : 'hidden'"
                            @click="onSubmitCandidateFeedbackDelete(props.row.id, props.row.candidateQuestions.dropDownValue, refreshCandidateFeedbackLog)"
                          >
                            <q-tooltip>Delete</q-tooltip>
                          </q-icon>
                        </q-td>
                      </q-tr>
                    </template>
                  </q-table>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
      </q-form>
    </q-card>
  </q-dialog>
  <q-dialog v-model="isDialogOpen">
    <q-card style="width: 700px; max-width: 80vw;">
      <q-card-section style="background-color: #1b75ab">
        <div class="text-h2 text-weight-medium text-white">
          {{ currentCommentFlag ? "Answer" : "Description" }}
        </div>
      </q-card-section>
      <q-card-section class="q-pt-sm RichTextEditor">
        <div v-html="currentComment" />
      </q-card-section>
      <q-card-actions align="right" class="bg-white text-teal">
        <q-btn
          v-close-popup
          color="grey-4"
          style="width:100px"
          push
          outline
          label="Close"
          type="button"
          class="text-grey-9 actionBtn"
          no-caps
        />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent, date, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import { zwConfirmDelete, notifySuccess, notifyError, getLocalStorage, notifyWarning } from "assets/utils";
import { isDate } from "validators/zw_validators.js";
import { useAuthStore } from "stores/auth";
import { required, helpers } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import _ from "lodash";
import { format } from "date-fns"; // Standard TimeZone Conversion

import candidateService from "../candidate.service";

// Shared Dropdowns
import employeeModule from "src/modules/employee/utils/dropdowns.js";
import candidateModule from "src/modules/candidate/utils/dropdowns.js";

// Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";
import formDate from "src/components/form-inputs/_formDate.vue";

import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";

// Shared Candidate Actions
import {
  initCandidateActions,
  onSubmitCandidateFeedbackDelete
} from "src/modules/candidate/utils/actions.js";

// ----------------------------------------------------------------------------------------------------------------
// define emits
// ----------------------------------------------------------------------------------------------------------------

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent();

// ----------------------------------------------------------------------------------------------------------------
// Props values i.e. come from query string
// ----------------------------------------------------------------------------------------------------------------

const props = defineProps({ id: { type: String, default: "" } });

// ----------------------------------------------------------------------------------------------------------------
// Common variables
// ----------------------------------------------------------------------------------------------------------------

const $q = useQuasar();
const loading = ref(true);
const processing = ref(false);
const rows = ref([]);
const activeRowId = ref(null);
// const currentDate = new Date();
const isDialogOpen = ref(false);
const currentComment = ref("");
const currentCommentFlag = ref(false);
const answerError = ref(false);
const answerErrorMessage = ref("");
const storedUser = getLocalStorage("user");
const authStore = useAuthStore();
const user = authStore.user;
const { fonts, toolbar } = getEditorConfig($q);

// ----------------------------------------------------------------------------------------------------------------
// Column variables
// ----------------------------------------------------------------------------------------------------------------

const pagination = ref({ sortBy: "updatedOnUtc", descending: true, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "dueDate", label: "Due date", field: "dueDate", align: "left", sortable: true },
  { name: "employee.person.fullName", label: "Owner", field: "employee.person.fullName", align: "left", sortable: true },
  { name: "candidateQuestions.dropDownValue", label: "Question", field: "candidateQuestions.dropDownValue", align: "left", sortable: true },
  { name: "answer", label: "Answer", field: "answer", align: "left", sortable: true }
]);

// ----------------------------------------------------------------------------------------------------------------
// define model
// ----------------------------------------------------------------------------------------------------------------

const model = ref({
  employeeOwnerId: user?.employeeId ? user.employeeId : "",
  questionId: "",
  answer: "",
  dueDate: format(new Date(), "MM/dd/yyyy")
});

// ----------------------------------------------------------------------------------------------------------------
// get Candidate Feedback Log details
// ----------------------------------------------------------------------------------------------------------------

const getCandidateFeedbackLog = () => {
  loading.value = true;
  candidateService.getCandidateFeedbackDetailsById(props.id).then((resp) => {
    rows.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

const onEdit = (item) => {
  model.value = item;
  model.value.showCheckbox = true;
  activeRowId.value = item.id;
};

// ----------------------------------------------------------------------------------------------------------------
// rule for required field as per condition
// ----------------------------------------------------------------------------------------------------------------

const rules = computed(() => {
  const baseRules = {};
  baseRules.dueDate = {
    required: helpers.withMessage("Date is required", required),
    isDate: helpers.withMessage("Date is invalid", isDate)
  };
  baseRules.employeeOwnerId = {
    required: helpers.withMessage("Owner is required", required)
  };
  baseRules.questionId = {
    required: helpers.withMessage("Question is required", required)
  };
  return baseRules;
});

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// ------------------------------------------------------------------------------------
// DataTable:- Initialization Of Dialogs, Actions
// ------------------------------------------------------------------------------------

initCandidateActions(activeRowId);

// ------------------------------------------------------------------------------------
// All Dropdowns
// ------------------------------------------------------------------------------------

const { candidateQuestionDropdownSingleSelect } = candidateModule();
const { activeEmployeesDropdownSingleSelect } = employeeModule();

// ------------------------------------------------------------------------------------
// Custom functions
// ------------------------------------------------------------------------------------

const refreshCandidateFeedbackLog = () => {
  getCandidateFeedbackLog({ pagination: pagination.value });
}

const showComment = (comment, isAnswer) => {
  currentComment.value = comment;
  isDialogOpen.value = true;
  currentCommentFlag.value = isAnswer;
};

function removeLeadingSpaces (html) {
  if (!html) return "";
  // Remove leading spaces, non-breaking spaces, and empty tags
  return html.replace(/^(?:\s|&nbsp;|<[^>]+>)*\s*/, "");
}

function answerValidation (answer) {
  const textOnly = answer?.replace(/<[^>]*>/g, "").trim();
  // answer?.replace(/<[^>]*>?/gm, "").trim();
  return !textOnly;
}

// const onDelete = (item) => {
//   activeRowId.value = item.id;
//   zwConfirmDelete({ data: `${item.candidateQuestions.dropDownValue}` }, () => {
//     candidateService.deleteCandidateFeedbacks(item.id).then(resp => {
//       notifySuccess({ message: "Feedback is deleted successfully." });
//       getCandidateFeedbackLog({ pagination: pagination.value });
//       activeRowId.value = null;
//     });
//   }, () => {
//     activeRowId.value = null;
//   });
// };

// onSubmit
async function onSubmit () {
  try {
    if (processing.value) {
      notifyWarning({ message: "Double click not allowed. Please wait..." });
      return; // stop further submit
    }
    answerError.value = false;
    answerErrorMessage.value = "";
    // manually validate answer
    if (answerValidation(model.value.answer)) {
      answerError.value = true;
      answerErrorMessage.value = "Answer is required";
    }
    v$.value.$touch();
    if (!answerError.value && await v$.value.$validate()) {
      processing.value = true;
      const payload = {
        id: activeRowId.value,
        candidateId: props.id,
        dueDate: model.value.dueDate,
        employeeOwnerId: model.value.employeeOwnerId,
        questionId: model.value.questionId,
        answer: model.value.answer
      };
      candidateService.saveCandidateFeedbacks(payload).then(resp => {
        notifySuccess({ message: "Candidate Feedback is saved successfully." });
        // Clear form
        activeRowId.value = null;
        model.value.dueDate = format(new Date(), "MM/dd/yyyy");
        model.value.employeeOwnerId = user?.employeeId ? user.employeeId : "";
        model.value.questionId = "";
        model.value.answer = "";
        // Reset validation
        v$.value.$reset();
        refreshCandidateFeedbackLog();
      });
    }
  } catch (error) {
    console.error("Error in submitting:", error);
    notifyError({ message: "An error occurred while saving." });
  } finally {
    processing.value = true;
    setTimeout(() => {
      processing.value = false;
    }, 1500);
  }
}

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    refreshCandidateFeedbackLog();
  }
}, { immediate: true });

watch(
  () => model.value.questionId,
  (newValue, oldValue) => {
    if (newValue !== oldValue && oldValue !== undefined && newValue !== "" && oldValue !== "") {
      model.value.answer = ""; // clear answer
    }
  }, { immediate: true });

watch(() => model.value.answer, (newVal) => {
  const answerData = newVal?.replace(/<[^>]*>/g, "").trim();
  if (answerData) {
    answerError.value = false;
    answerErrorMessage.value = "";
  }
});

// ------------------------------------------------------------------------------------
// On page rendering
// ------------------------------------------------------------------------------------

onMounted(() => {
  activeEmployeesDropdownSingleSelect.load();
  candidateQuestionDropdownSingleSelect.load("Candidate Questions");
});

</script>
