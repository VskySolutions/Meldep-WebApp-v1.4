<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 70vw; max-width: 70vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Ticket</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <!-- <q-card class="card-header with-tools headerBasic"> -->
            <fieldset>
              <legend>Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-4 col-md-4 hidden">
                  <div class="q-mb-xs text-black">Ticket Id</div>
                  <q-input v-model="model.ticketNo" outlined stack-label hide-bottom-space :dense="true" maxlength="128" readonly />
                </div>
                <!-- <div class="col-12 col-sm-4 col-md-4 hidden">
                  <div class="label q-mb-xs text-black">
                    Date<span class="required">*</span>
                    <span class="text-grey-6 fs-12"> (Select the relevant date for the ticket)</span>
                  </div>
                  <q-input
                    v-model="model.dateStr" outlined stack-label hide-bottom-space mask="##/##/####" dense :error="v$.dateStr.$error" :error-message="v$.dateStr.$errors[0]?.$message" disable
                    @click="v$.dateStr.$touch"
                  >
                    <template #append>
                      <q-icon name="o_calendar_month" class="cursor-pointer">
                        <q-popup-proxy ref="qDateProxy" transition-show="scale" transition-hide="scale">
                          <q-date v-model="model.dateStr" mask="MM/DD/YYYY" :options="disableFutureDates" @update:model-value="() => $refs.qDateProxy.hide()" />
                        </q-popup-proxy>
                      </q-icon>
                    </template>
                  </q-input>
                </div> -->
                <div class="col-12 col-sm-12 col-md-12">
                  <div class="label q-mb-xs text-black">
                    Title<span class="required">*</span>
                    <span class="text-grey-6 fs-12"> (Enter a brief and clear title for your ticket (e.g., Login issue on dashboard))</span>
                  </div>
                  <q-input v-model="model.title" outlined stack-label hide-bottom-space :dense="true" maxlength="128" autofocus :error="v$.title.$error" :error-message="v$.title.$errors[0]?.$message" @click="v$.title.$touch" />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="label q-mb-xs text-black">Workspaces</label>
                  <span class="text-grey-6 fs-12"> (Select the workspace related to this ticket)</span>
                  <div class="row items-center no-wrap">
                    <q-select
                      class="col"
                      v-model="model.topicId"
                      clearable
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="topicList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      @filter="getAllHelpDeskTopicListForFilter"
                      @update:model-value="getQuestions(model.topicId)"
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
                    <q-icon
                      name="fa-solid fa-search"
                      color="primary"
                      class="cursor-pointer q-ml-sm"
                      size="xs"
                      @click="onQuickSelectWorkspaceAndMenus"
                    >
                    <q-tooltip>Workspaces and Menus</q-tooltip>
                  </q-icon>
                  </div>
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="label q-mb-xs text-black">Menus</label>
                  <span class="text-grey-6 fs-12"> (Select the menu inside the workspace)</span>
                  <div class="row items-center no-wrap">
                    <q-select
                      class="col"
                      v-model="model.questionId"
                      clearable
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="questionList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      @filter="getAllHelpDeskTopicQuestionsListForFilter"
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
                    <q-icon
                      name="fa-solid fa-search"
                      color="primary"
                      class="cursor-pointer q-ml-sm"
                      size="xs"
                      @click="onQuickSelectWorkspaceAndMenus"
                    >
                    <q-tooltip>Workspaces and Menus</q-tooltip>
                  </q-icon>
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-sm">
                <div class="col-12 col-sm-4 col-md-4">
                  <label class="label q-mb-xs text-black">Priority<span class="required">*</span>
                    <span class="text-grey-6 fs-12"> (Choose the priority based on urgency)</span>
                  </label>
                  <q-select
                    v-model="model.priorityId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="priorityList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.priorityId.$error"
                    :error-message="v$.priorityId.$errors[0]?.$message"
                    @blur="v$.priorityId.$touch"
                    @filter="getAllPriorityListDropdownForFilter"
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
                <div class="col-12 col-sm-4 col-md-4">
                  <label class="label q-mb-xs text-black">Ticket Category<span class="required">*</span>
                  </label>
                  <q-select
                    v-model="model.categoryId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="categoryList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.categoryId.$error"
                    :error-message="v$.categoryId.$errors[0]?.$message"
                    @blur="v$.categoryId.$touch"
                    @filter="getAllCategoryListDropdownForFilter"
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
                <div v-if="!roleSupportTeamOrAdmin"
                  class="col-12 col-sm-4 col-md-4"
                >
                  <div class="q-mb-xs text-black">Requester<span class="required">*</span></div>
                  <!-- <q-select
                    v-model="model.requesterId"
                    clearable
                    :disable="!roleSupportTeamOrAdmin"
                    use-input
                    outlined
                    dense
                    :options="employeeList"
                    option-value="value"
                    option-label="primaryEmailAddress"
                    emit-value
                    map-options
                    @filter="getAllEmployeesListDropdownForFilter"
                  /> -->
                  <q-select
                    v-model="model.requesterId"
                    clearable
                    use-input
                    outlined
                    dense
                    :options="employeeList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    @filter="getAllEmployeesListDropdownForFilter"
                  />
                </div>
                <div v-if="roleSupportTeamOrAdmin" class="col-12 col-sm-4 col-md-4 hidden">
                  <label class="label q-mb-xs text-black">Company<span class="required">*</span></label>
                  <div>
                    <q-select
                      v-model="model.companyId"
                      clearable
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="customerList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      :disable="isCompanyReadonly"
                      @filter="getAllCustomerListForFilter"
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
              <div v-if="roleSupportTeamOrAdmin" class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-12 col-md-12 flex items-center">
                  <q-option-group
                    v-model="requesterType"
                    :options="requesterTypeList"
                    type="radio"
                    inline
                  />
                  <span class="text-grey-6 fs-12 q-ml-sm"> (Select New to enter a new requester email address, or select Existing to choose a requester from the dropdown list.)</span>
                </div>
                <!-- Existing Requester -->
                <div
                  class="col-12 col-sm-4 col-md-4 q-mt-sm"
                  v-if="requesterType === 'existing'"
                >
                  <div class="q-mb-xs text-black">Requester<span class="required">*</span></div>
                  <q-select
                    v-model="model.requesterId"
                    clearable
                    use-input
                    outlined
                    dense
                    :options="employeeList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.requesterId.$error"
                    :error-message="v$.requesterId.$errors[0]?.$message"
                    @blur="v$.requesterId.$touch"
                    @filter="getAllEmployeesListDropdownForFilter"
                  />
                </div>
                <!-- New Requester -->
                <div
                  class="col-12 col-sm-4 col-md-4 q-mt-sm"
                  v-else
                >
                  <div class="q-mb-xs text-black">Requester Emails<span class="required">*</span></div>
                  <q-input
                    v-model="model.requesterEmail"
                    type="email"
                    outlined
                    dense
                    :error="v$.requesterEmail.$error"
                    :error-message="v$.requesterEmail.$errors[0]?.$message"
                    @blur="v$.requesterEmail.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md hidden">
                <div class="col-12 col-sm-4 col-md-4">
                  <label class="label q-mb-xs text-black">Status</label>
                  <q-select
                    v-model="model.statusId"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="statusList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
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
                <div class="col-12 col-sm-4 col-md-4">
                  <label class="label q-mb-xs text-black">Assigned To</label>
                  <div>
                    <q-select
                      v-model="model.assignedToId"
                      clearable
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="employeeList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      @filter="getAllEmployeesListDropdownForFilter"
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
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-lg-12">
                  <div class="q-mb-xs text-black">
                    <label>Description<span class="required">*</span>
                    </label>
                    <span class="text-grey-6 fs-12"> (Add details related to this ticket and paste screenshots if needed)</span>
                  </div>
                  <div class="form-group">
                    <q-editor
                      v-model="model.description" :dense="$q.screen.lt.md"
                      :toolbar="[
                        [
                          {
                            label: $q.lang.editor.align,
                            icon: $q.iconSet.editor.align,
                            fixedLabel: true,
                            list: 'only-icons',
                            options: ['left', 'center', 'right', 'justify']
                          },
                        ],
                        ['bold', 'italic', 'strike', 'underline'],
                        ['token', 'hr', 'link', 'custom_btn'],
                        [
                          {
                            label: $q.lang.editor.formatting,
                            icon: $q.iconSet.editor.formatting,
                            list: 'no-icons',
                            options: [
                              'p',
                              'h1',
                              'h2',
                              'h3',
                              'h4',
                              'h5',
                              'h6',
                              'code'
                            ]
                          },
                          'removeFormat'
                        ],
                        ['quote', 'unordered', 'ordered', 'outdent', 'indent'],

                        ['undo', 'redo'],
                        ['viewsource']
                      ]"

                      :fonts="{
                        arial: 'Arial',
                        arial_black: 'Arial Black',
                        comic_sans: 'Comic Sans MS',
                        courier_new: 'Courier New',
                        impact: 'Impact',
                        lucida_grande: 'Lucida Grande',
                        times_new_roman: 'Times New Roman',
                        verdana: 'Verdana'
                      }" :error="v$.description.$error" :error-message="v$.description.$errors[0]?.$message" @blur="v$.description.$touch"
                    />
                  </div>
                </div>
              </div>
              <!-- <div><p class="text-grey-6 fs-12">Note:- If you do not find related topics or questions, Please choose "Other" and submit your help request.</p></div> -->
              <div class="row q-mt-xs" @paste="handlePaste">
                <!-- File Uploader -->
                <div class="col-xxl-3 col-lg-3 col-md-3 col-sm-3 col-xs-12">
                  <div class="q-mb-xs text-black">
                    <label>Files</label>
                    <span class="text-grey-6 fs-12"> (Attach files or screenshots)</span>
                  </div>
                  <div class="form-group">
                    <!-- <q-uploader
                      ref="documentUploaderRef"
                      v-model="model.helpDeskFiles"
                      class="prodUploader"
                      color="white"
                      text-color="dark"
                      with-credentials
                      hide-upload-btn
                      multiple
                      field-name="HelpDeskFiles"
                      flat
                      bordered
                      label="Drag files here or (+) to upload."
                      @added="onFileAdded"
                      @removed="onFileRemoved"
                    />
                    <div class="text-grey-7 text-caption q-mt-xs">
                      <i>Allowed Files: jpg, png, jpeg, pdf, excel, doc, ppt</i>
                    </div> -->
                    <multiFileUploader
                      :initialFiles="model.helpDeskFiles"
                      :allowedExtensions="[
                        '.pdf','.xls','.xlsx','.doc','.docx','.jpeg','.jpg','.png','.ppt','.pptx'
                      ]"
                      :maxSizeInMb="25"
                      label="Drag files here or (+) to upload."
                      @files-selected="handleFiles"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <!-- Display Files in Square Boxes with File Name Below -->
                <div v-if="model.helpDeskFiles && model.helpDeskFiles.length > 0" class="row q-gutter-md">
                  <div
                    v-for="(file, index) in model.helpDeskFiles"
                    :key="index"
                    class="col-3 position-relative file-card text-center"
                    style="max-width: 140px; min-width: 140px;"
                  >
                    <div class="file-preview square-box">
                      <template v-if="isImageFile(file)">
                        <img
                          :src="file.file.virtualPath ? file.file.virtualPath : getFilePreview(file.file)"
                          alt="File Preview"
                          class="square-content centered-image"
                        >
                      </template>
                      <template v-else>
                        <q-icon
                          :name="getFileIcon(file.file?.mimeType)"
                          class="file-icon square-content"
                          size="70px"
                        />
                      </template>
                      <div class="file-name q-mt-sm">
                        <q-btn
                          v-if="file.file?.virtualPath || file?.name"
                          class="bg-primary text-white q-pa-xs"
                          no-caps
                          @click="viewFile(file)"
                        >
                          <span class="truncate-text">
                            {{ file.file?.name || file.name || extractFileName(file.file?.seoFilename) }}
                          </span>
                        </q-btn>
                      </div>
                    </div>
                    <!-- Remove Button -->
                    <q-btn
                      color="negative"
                      flat
                      round
                      dense
                      icon="o_close"
                      class="remove-file-icon"
                      @click="removeFile(index)"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions class="stickyFooter q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Send" type="submit" class="actionBtn" :loading="processing" :disable="processing" no-caps />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import helpDeskService from "modules/helpdesk/helpDesk.service";
import { required, helpers, requiredIf } from "@vuelidate/validators";
import { ref, watch, onMounted, computed, toRaw } from "vue";
import _ from "lodash";
import { notifySuccess, notifyError, notifyWarning } from "assets/utils";
import { useAuthStore } from "stores/auth";
import commonService from "services/common.service";
import employeesService from "src/modules/employee/employee.service";
import customerService from "src/modules/customer/customer.service";
import quickSelectWorkspaceAndMenus from "modules/helpdesk/components/_quickSelectWorkspaceAndMenus.vue";

// Shared Inputs
import multiFileUploader from "src/components/form-inputs/_multiFileUpload.vue";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Common variables
const loading = ref(true);
const processing = ref(false);
const authStore = useAuthStore();
const user = authStore.user;
// const baseURL = process.env.API_BASE_URL;
const documentUploaderRef = ref(null);
const requesterType = ref("existing");
// const previousRequesterId = ref(null);
const roleSupportTeamOrAdmin = user?.roles?.includes("admin") || user?.roles?.includes("support team");
const isCompanyReadonly = ref(false);
const $q = useQuasar();

// console.log("authStore.user", authStore.user);

// Define model values
const model = ref({
  topicId: "",
  questionId: "",
  title: "",
  description: "",
  priorityId: "",
  categoryId: "",
  statusId: "",
  assignedToId: "",
  requesterEmail: "",
  // requesterId: user?.employeeId ?? null,
  requesterId: "",
  companyId: "",
  isActive: true,
  name: user.firstName + " " + user.lastName
});

const requesterTypeList = ref([
  { label: "Existing", value: "existing" },
  { label: "New", value: "new" }
]);

const requiredEditor = helpers.withMessage(
  "Description is required",

  (value) => {
    if (!value) return false;

    const text = stripHtml(value);
    const imageExists = hasImage(value);
    // return stripHtml(value).length > 0;
    return text.length > 0 || imageExists;
  }
);

const rules = computed(() => ({
  title: { required: helpers.withMessage("Title is required", required) },
  description: { requiredEditor },
  priorityId: { required: helpers.withMessage("Priority is required", required) },
  categoryId: { required: helpers.withMessage("Category is required", required) },
  requesterId: {
    required: helpers.withMessage(
      "Requester is required",
      requiredIf(() => {
        // requester logged in
        if (!roleSupportTeamOrAdmin) {
          return true;
        }

        // support/admin + existing
        return (
          roleSupportTeamOrAdmin &&
          requesterType.value === "existing"
        );
      })
    )
  },
  requesterEmail: {
    required: helpers.withMessage(
      "Requester email is required",
      requiredIf(() => {
        return (
          roleSupportTeamOrAdmin &&
          requesterType.value === "new"
        );
      })
    )
  }
  // companyId: {
  //   required: helpers.withMessage(
  //     "Company is required",
  //     requiredIf(() => {
  //       return (
  //         roleSupportTeamOrAdmin
  //       );
  //     })
  //   )
  // }
  // companyId: { required: helpers.withMessage("Company is required", required) }
}));

// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// ---------------------------------------------------------------------
// DROPDOWN LISTS
// ---------------------------------------------------------------------

// Get all topic list for dropdown
const topicList = ref([]);
const topicFilter = ref([]);
function getAllHelpDeskTopicListForDropdown () {
  helpDeskService.getAllHelpDeskTopicListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.text, value: item.value }));
    topicList.value = responseData;
    topicFilter.value = responseData;
  });
}

// Search project for dropdown
function getAllHelpDeskTopicListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      topicList.value = topicFilter.value;
    } else {
      topicList.value = topicFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all question List for dropdown
const questionList = ref([]);
const questionFilter = ref([]);
function getAllHelpDeskTopicQuestionsListForDropdown (topicId) {
  helpDeskService.getAllHelpDeskTopicQuestionsListForDropdown(topicId).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.text, value: item.value }));
    questionList.value = responseData;
    questionFilter.value = responseData;
  });
}

// Search project module for dropdown
function getAllHelpDeskTopicQuestionsListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      questionList.value = questionFilter.value;
    } else {
      questionList.value = questionFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all Assign To list
const employeeList = ref([]);
const employeeListFilter = ref([]);
function getAllEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id, primaryEmailAddress: item.person.primaryEmailAddress })).sort((a, b) => a.text.localeCompare(b.text));
    // console.log("emp", resp);
    employeeList.value = responseData;
    employeeListFilter.value = responseData;
  });
}

// Search Assign To for dropdown
function getAllEmployeesListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = employeeListFilter.value;
    } else {
      employeeList.value = employeeListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

const customerList = ref([]);
const customerListOptions = ref([]);
function getAllCustomerListForDropdown () {
  customerService.getAllCustomerListForDropdown().then((resp) => {
    const responseData = resp
      .map((item) => ({ text: item.company ? item.company.name : `${item.person.firstName} ${item.person.lastName}`, value: item.id }))
      .sort((a, b) => a.text.localeCompare(b.text));
    customerList.value = responseData;
    customerListOptions.value = responseData;
    // If only one company -> auto select & readonly
    // if (responseData.length === 1) {
    //   model.value.companyId = responseData[0].value;
    //   isCompanyReadonly.value = true;
    // } else {
    //   // multiple companies -> editable & no readonly
    //   model.value.companyId = "";
    //   isCompanyReadonly.value = false;
    // }
  });
}

function getAllCustomerListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      customerList.value = customerListOptions.value;
    } else {
      customerList.value = customerListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all help desk status List
const statusList = ref([]);
const statusFilter = ref([]);
function getHelpDeskStatus (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    statusList.value = responseData;
    statusFilter.value = responseData;
  });
}

const priorityList = ref([]);
const priorityFilter = ref([]);
function getHelpDeskPriority (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    priorityList.value = responseData;
    priorityFilter.value = responseData;
    const mediumPriority = responseData.find(priority => priority.text.toLowerCase() === "medium");
    if (mediumPriority) {
      model.value.priorityId = mediumPriority.value;
    }
  });
}

// Search priority for dropdown
function getAllPriorityListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      priorityList.value = priorityFilter.value;
    } else {
      priorityList.value = priorityFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all help desk category List
const categoryList = ref([]);
const categoryFilter = ref([]);
function getHelpDeskCategory (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    categoryList.value = responseData;
    categoryFilter.value = responseData;
  });
}

// Search category for dropdown
function getAllCategoryListDropdownForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      categoryList.value = categoryFilter.value;
    } else {
      categoryList.value = categoryFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// get project details on edit mode
const getHelpDesk = () => {
  loading.value = true;
  helpDeskService.getHelpDesk(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.description = resp.description ? resp.description : "";
  }).finally(() => {
    loading.value = false;
  });
};

function getQuestions (topicId) {
  model.value.questionId = "";
  getAllHelpDeskTopicQuestionsListForDropdown(topicId);
}

// quick Select Workspace And Menus
const onQuickSelectWorkspaceAndMenus = () => {
  $q.dialog({
    component: quickSelectWorkspaceAndMenus,
    componentProps: {}
  }).onOk((data) => {
    model.value.topicId = data.topicId;
    getAllHelpDeskTopicQuestionsListForDropdown(model.value.topicId);
    model.value.questionId = data.questionId;
  }).onCancel(() => {
  }).onDismiss(() => {
  });
};
// =================================================================================
// File Upload
// =================================================================================

// const allowedExtensions = [".pdf", ".xls", ".xlsx", ".doc", ".docx", ".jpeg", ".jpg", ".png", ".gif", ".ppt", ".pptx"];
// const allowedFileTypes = [
//   "application/pdf", // PDF
//   "application/vnd.ms-excel", // Excel (old format)
//   "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // Excel (new format)
//   "application/msword", // Word (old format)
//   "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // Word (new format)
//   "image/jpeg", "image/png", "image/gif", // Images
//   "application/vnd.ms-powerpoint", // PowerPoint (old format)
//   "application/vnd.openxmlformats-officedocument.presentationml.presentation" // PowerPoint (new format)
// ];

// const isValidFile = (file) => {
//   // Normalize type by trimming
//   const mimeType = file.type ? file.type.trim() : "";
//   const fileName = file.name ? file.name.toLowerCase() : "";

//   // Check MIME type
//   const fileTypeValid = mimeType && allowedFileTypes.includes(mimeType);

//   // Check file extension as a fallback (for edge cases)
//   const fileExtensionValid = fileName && allowedExtensions.some(ext => fileName.endsWith(ext));

//   return fileTypeValid || fileExtensionValid; // Pass if either check succeeds
// };

function handlePaste (event) {
  const clipboardItems = event.clipboardData.items;
  const files = [];
  for (const item of clipboardItems) {
    if (item.kind === "file") {
      const file = item.getAsFile();
      if (file) {
        files.push(file);
      }
    }
  }

  if (files.length > 0 && documentUploaderRef.value) {
    // Add files to uploader
    documentUploaderRef.value.addFiles(files);
  }
}

// const onFileAdded = (files) => {
//   if (!files || files.length === 0) return;

//   if (!model.value.helpDeskFiles) {
//     model.value.helpDeskFiles = [];
//   }

//   const invalidFiles = files.filter(file => !isValidFile(file));
//   const validFiles = files.filter(isValidFile);
//   // Show an alert if there are invalid files
//   if (invalidFiles.length > 0) {
//     const invalidFileNames = invalidFiles.map(file => file.name).join(", ");
//     notifyError({ message: `The following file type is not allowed: ${invalidFileNames}` });
//   }

//   // Add a "new" flag to the newly added files
//   validFiles.forEach(file => {
//     file.flag = "new"; // Mark file as "new"
//   });
//   invalidFiles.forEach((file) => {
//     documentUploaderRef.value?.removeFile(file);
//   });

//   model.value.helpDeskFiles.push(...validFiles);
//   model.value.helpDeskFileFlag = "edit"; // Set the overall flag for tracking
// };

// function handleFiles (files) {
//   const existingFiles = model.value.helpDeskFiles || [];

//   // Merge without losing old files
//   model.value.helpDeskFiles = [...existingFiles, ...files];

//   model.value.helpDeskFileFlag = "edit";
// }

function handleFiles (files) {
  model.value.helpDeskFiles = files;
  model.value.helpDeskFileFlag = "edit";
}

function getFilePreview (file) {
  return file && file instanceof File ? URL.createObjectURL(file) : "";
}

function isImageFile (file) {
  if (file.file instanceof File) {
    return file.file.type.startsWith("image/");
  } else if (file.file && file.file.mimeType) {
    return file.file.mimeType.startsWith("image/");
  }
  return false;
}

function getFileIcon (mimeType) {
  const mimeToIconMap = {
    "application/pdf": "o_picture_as_pdf",
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet": "o_insert_chart",
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document": "o_description",
    "application/vnd.openxmlformats-officedocument.presentationml.presentation": "o_slideshow", // PPTX MIME type
    "application/vnd.ms-powerpoint": "o_slideshow", // PPT MIME type
    "application/zip": "o_folder_zip",
    "text/plain": "o_article",
    "image/png": "o_image",
    "image/jpeg": "o_image",
    "image/gif": "o_image",
    // Default icon for unknown MIME types
    default: "o_insert_drive_file"
  };

  return mimeToIconMap[mimeType] || mimeToIconMap.default;
}

function viewFile (file) {
  let fileUrl = "";
  let fileName = "";
  // let mimeType = "";

  if (typeof file === "string") {
    fileUrl = file;
    fileName = file.split("/").pop();
    // isServerFile = true;
  } else if (file?.file?.virtualPath) {
    fileUrl = file.file.virtualPath;
    fileName = file.file.name || file.file.virtualPath.split("/").pop();
    // isServerFile = true;
  } else if (file instanceof File || file?.name) {
    const rawFile = file.file ?? file;
    fileUrl = URL.createObjectURL(rawFile);
    fileName = rawFile.name;
  } else {
    return;
  }

  // const fileUrl = new URL(file, baseURL).href;
  const fileExtension = fileUrl.split(".").pop().toLowerCase();
  const supportedFormats = ["pdf", "docx", "xlsx", "pptx"];
  const imageFormats = ["jpg", "jpeg", "png", "gif", "svg"];

  let viewerUrl = fileUrl;

  // Use Google Docs Viewer for Documents
  if (supportedFormats.includes(fileExtension)) {
    viewerUrl = `https://docs.google.com/gview?url=${encodeURIComponent(fileUrl)}&embedded=true`;
  }

  // Open new window
  const newWindow = window.open("", "_blank");

  // Check if the popup is blocked
  if (!newWindow) {
    alert("Popup blocked! Please allow popups for this site.");
    return;
  }

  // Delay to avoid null reference issues
  setTimeout(() => {
    newWindow.document.write(`
<html>
<head>
<title>${fileName}</title>
<style>
              * { margin: 0; padding: 0; box-sizing: border-box; }
              body, html { width: 100vw; height: 100vh; display: flex; align-items: center; justify-content: center; background-color: #f4f4f4; overflow: hidden; }
              .top-right {
                position: fixed;
                top: 10px;
                right: 10px;
                background: #007bff;
                color: white;
                padding: 10px 15px;
                border-radius: 5px;
                font-size: 16px;
                text-decoration: none;
                z-index: 10;
              }
              .top-right:hover {
                background: #0056b3;
              }

              iframe, img {
                width: 100%;
                height: 100%;
                border: none;
                display: block;
                object-fit: contain; /* Ensures images fit properly */
              }

              /* Responsive Fixes */
              @media (max-width: 768px) {
                .top-right {
                  top: 5px;
                  right: 5px;
                  padding: 8px 12px;
                  font-size: 14px;
                }
              }
</style>
</head>
<body>
<a class="top-right" href="${fileUrl}" download>Download</a>
            ${
  imageFormats.includes(fileExtension)
    ? `<img src="${fileUrl}" alt="Image Preview">` // Show image directly
    : `<iframe src="${viewerUrl}"></iframe>` // Show document using iframe
}
</body>
</html>
        `);
  }, 100);
}

function extractFileName (path) {
  return path ? path.split("/").pop() : "Unknown File";
}

function removeFile (index) {
  const file = model.value.helpDeskFiles[index];
  if (!file) return;

  // REMOVE FROM UPLOADER ALSO
  const uploader = documentUploaderRef.value;
  if (uploader) {
    const uploaderFile = uploader.files.find(
      f =>
        f.name === file.name ||
        f.name === file.file?.name
    );
    if (uploaderFile) {
      uploader.removeFile(uploaderFile);
    }
  }

  // Existing file (from DB)
  if (file.file?.virtualPath) {
    file.flag = "remove";
  }

  model.value.helpDeskFiles.splice(index, 1);

  if (model.value.helpDeskFiles.length === 0) {
    model.value.helpDeskFlag = "remove";
  }
}

// const onFileRemoved = (removedFiles) => {
//   removedFiles.forEach((removedFile) => {
//     model.value.helpDeskFiles = model.value.helpDeskFiles.filter(
//       file =>
//         file.name !== removedFile.name &&
//         file.file?.name !== removedFile.name
//     );
//   });
// };
// =================================================================================

const stripHtml = (html) => {
  if (!html) return "";
  return html.replace(/<[^>]*>/g, "").replace(/&nbsp;/g, " ").trim();
};

const hasImage = (html) => {
  if (!html) return false;
  return /<img\s+[^>]*src=/i.test(html);
};

const sanitizeValue = (value) => {
  if (typeof value === "string") {
    return value.replace(/\s+/g, " ").trim();
  }
  return value;
};

const sanitizeEditorHtml = (html) => {
  if (!html) return "";

  return html
    // remove multiple spaces
    .replace(/\s+/g, " ")
    // remove nbsp
    .replace(/&nbsp;/g, " ")
    // remove empty paragraphs
    .replace(/<p>\s*<\/p>/gi, "")
    .replace(/<p><br><\/p>/gi, "")
    // trim start/end
    .trim();
};

// Submit form
const onSubmit = async () => {
  try {
    if (processing.value) {
      notifyWarning({ message: "Double click not allowed. Please wait..." });
      return; // stop further submit
    }
    const formData = new FormData();
    const isValid = await v$.value.$validate();
    v$.value.$touch();
    if (!isValid) {
      notifyError({ message: "Please fill all required fields." });
      return; // stop submit here
    }
    const descriptionHtmlRaw = model.value.description;
    const descriptionHtml = sanitizeEditorHtml(descriptionHtmlRaw);
    const descriptionText = stripHtml(descriptionHtml);
    const containsImage = hasImage(descriptionHtml);
    if (!descriptionText && !containsImage) {
      v$.value.description.$touch();
      notifyError({ message: "Description is required" });
      return;
    }

    processing.value = true;
    formData.append("topicId", model.value.topicId ? model.value.topicId : "");
    formData.append("questionId", model.value.questionId ? model.value.questionId : "");
    formData.append("title", sanitizeValue(model.value.title));
    formData.append("description", model.value.description ? descriptionHtml : "");
    formData.append("priorityId", model.value.priorityId);
    formData.append("categoryId", model.value.categoryId);
    // formData.append("companyId", model.value.companyId ? model.value.companyId : "");
    if (requesterType.value === "existing") {
      formData.append("requesterId", model.value.requesterId || "");
      formData.append("requesterEmail", "");
    } else {
      formData.append("requesterId", "");
      formData.append("requesterEmail", model.value.requesterEmail || "");
    }

    toRaw(model.value.helpDeskFiles || []).forEach((file) => {
      if (file.file && file.file.virtualPath) {
        // For existing files, append metadata instead of the file itself
        formData.append("ExistingFiles", JSON.stringify({
          id: file.id,
          virtualPath: file.file.virtualPath
          // flag: file.flag || "new"
        }));
      } else {
        // For new files, append as raw file objects (IFormFile)
        formData.append("helpDeskFiles", file);
      }
    });

    // Also pass the helpDeskFlag for general status tracking
    formData.append("helpDeskFlag", model.value.helpDeskFlag || "no_change");
    // console.log(formData);

    await helpDeskService.saveHelpDesk(props.id, formData);

    notifySuccess({ message: "Ticket is saved successfully." });
    onDialogOK();
  } catch (error) {
    console.error("Error in submitting the ticket:", error);
    notifyError({ message: "An error occurred while saving the ticket." });
  } finally {
    processing.value = false;
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getHelpDesk();
  }
}, { immediate: true });

watch(requesterType, (val) => {
  if (val === "new") {
    // store old value before clearing
    // previousRequesterId.value = model.value.requesterId;
    model.value.requesterId = "";
  } else {
    // restore previous requester
    // model.value.requesterId = previousRequesterId.value || user?.employeeId || null;
    model.value.requesterEmail = "";
  }
});

// On page rendering
onMounted(() => {
  getAllHelpDeskTopicListForDropdown();
  getHelpDeskStatus("HelpDesk Status");
  getHelpDeskPriority("HelpDesk Priority");
  getHelpDeskCategory("HelpDesk Category");
  getAllEmployeesListForDropdown();
  getAllCustomerListForDropdown();
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
