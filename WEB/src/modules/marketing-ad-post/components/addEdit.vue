<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw !important; max-width: 60vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ id ? "Edit" : "Add" }} Ad</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <!-- <q-card-section class="card-header with-tools"> -->
      <q-form greedy @submit.prevent.stop="onSubmit">
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <!-- <q-card class="card-header with-tools headerBasic"> -->
            <fieldset>
              <legend>Ad Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="label q-mb-xs text-black">Ad Number</label>
                  <div>
                    <q-input
                      v-model="model.adNumber"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      readonly="readonly"
                    />
                  </div>
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="label q-mb-xs text-black">Ad Name<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.name"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :error="v$.name.$error"
                      :error-message="v$.name.$errors[0]?.$message"
                      @blur="v$.name.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="form-group">
                    <label class="label q-mb-xs text-black">Description</label>
                    <q-editor
                      v-model="model.description"
                      :dense="$q.screen.lt.md"
                      :toolbar="toolbar"
                      :fonts="fonts"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="label q-mb-xs text-black">Project Name<span class="required">*</span></label>
                  <div>
                    <q-select
                      v-model="model.projectId"
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="projectList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      :error="v$.projectId.$error"
                      :error-message="v$.projectId.$errors[0]?.$message"
                      @filter="projectListForFilter"
                      @blur="v$.projectId.$touch"
                      @update:model-value="getProject(model.projectId)"
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
                <div class="col-12 col-sm-6 col-md-6 edit_adModule">
                  <label class="label q-mb-xs text-black">Client Name<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.customerName"
                      outlined
                      hide-bottom-space
                      readonly="readonly"
                      :error="v$.customerName.$error"
                      :error-message="v$.customerName.$errors[0]?.$message"
                      @blur="v$.customerName.$touch"
                    />
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-lg-6 col-md-6">
                  <div class="q-mb-xs text-black">
                    Post Design<span class="required">*</span>
                  </div>

                  <!-- UPLOADER -->
                  <div v-if="!hasFile">
                    <!-- <q-uploader
                      ref="documentUploaderRef"
                      color="white"
                      text-color="dark"
                      class="prodUploader"
                      with-credentials
                      hide-upload-btn
                      field-name="postDesignfile"
                      flat
                      bordered
                      label="Drag file here or (+) to upload"
                      @uploaded="onUploaded"
                      @added="onFileAdded"
                    />
                    <div class="text-grey-7 text-caption q-mt-xs">
                      <i>Allowed Files: jpg, png, jpeg, gif, mp4, mov, webm</i>
                    </div> -->

                    <singleFileUploader
                      :allowedExtensions="[
                        '.pdf','.jpg','.png','.jpeg','.gif','.mp4','.mov','.webm'
                      ]"
                      :maxSizeInMb="25"
                      label="Drag file here or (+) to upload. (File)"
                      @file-selected="handleFile"
                      @file-valid="isFileValid = $event"
                    />
                    <div v-if="v$.pictureId.$error" class="text-negative text-caption q-mt-xs">
                      {{ v$.pictureId.$errors[0]?.$message }}
                    </div>
                  </div>

                  <!-- PREVIEW (local OR existing) -->
                  <div v-else class="column items-center">

                    <!-- Image -->
                    <img
                      v-if="isPreviewImage"
                      :src="previewSource"
                      class="preview-media"
                    >

                    <!-- Video -->
                    <video
                      v-else-if="isPreviewVideo"
                      :src="previewSource"
                      controls
                      class="preview-media"
                    />

                    <!-- PDF -->
                    <!-- <iframe
                      v-else-if="isPreviewPdf"
                      :src="previewSource"
                      class="preview-media"
                      style="height: 400px; width: 100%;"
                    /> -->

                    <div v-else-if="isPreviewPdf">
                      <q-icon name="o_picture_as_pdf" color="red" size="40px" />
                    </div>

                    <!-- REMOVE BUTTON -->
                    <q-btn
                      class="q-mt-sm"
                      color="negative"
                      label="Remove"
                      outline
                      no-caps
                      @click="handleRemove"
                    />
                  </div>
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <div class="q-mb-xs text-black">Post Link</div>
                  <q-input
                    v-model="model.url"
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    type="url"
                    :error="v$.url.$error"
                    :error-message="v$.url.$errors[0]?.$message"
                    @blur="v$.url.$touch"
                  />
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Image Provider Type<span class="required">*</span></label>
                  <q-select
                    v-model="model.imageType"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="providerTypeList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.imageType.$error"
                    :error-message="v$.imageType.$errors[0]?.$message"
                    @filter="providerTypeListForFilter"
                    @blur="v$.imageType.$touch"
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
                <div v-if="imageTypeText === 'Vsky'" class="col-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Image Provided/designed by<span class="required">*</span></label>
                  <div>
                    <q-select
                      v-model="model.imageProviderEmpId"
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
                      :error="v$.imageProviderEmpId?.$error"
                      :error-message="v$.imageProviderEmpId?.$errors[0]?.$message"
                      @filter="employeeListForFilter"
                      @blur="v$.imageProviderEmpId?.$touch"
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
                <div v-if="imageTypeText === 'Client Contact'" class="col-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Image Provided/designed by<span class="required">*</span></label>
                  <div>
                    <q-select
                      v-model="model.imageProviderClientId"
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="contactList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      :error="v$.imageProviderClientId?.$error"
                      :error-message="v$.imageProviderClientId?.$errors[0]?.$message"
                      @filter="contactListForFilter"
                      @blur="v$.imageProviderClientId?.$touch"
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
                <div class="col-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Content Provider Type<span class="required">*</span></label>
                  <q-select
                    v-model="model.contentType"
                    clearable
                    use-input
                    outlined
                    stack-label
                    hide-bottom-space
                    :dense="true"
                    :options="providerTypeList"
                    option-value="value"
                    option-label="text"
                    emit-value
                    map-options
                    :error="v$.contentType.$error"
                    :error-message="v$.contentType.$errors[0]?.$message"
                    @filter="providerTypeListForFilter"
                    @blur="v$.contentType.$touch"
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
                <div v-if="contentTypeText === 'Vsky'" class="col-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Content Provided/designed by<span class="required">*</span></label>
                  <div>
                    <q-select
                      v-model="model.contentProviderEmpId"
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
                      :error="v$.contentProviderEmpId?.$error"
                      :error-message="v$.contentProviderEmpId?.$errors[0]?.$message"
                      @filter="employeeListForFilter"
                      @blur="v$.contentProviderEmpId?.$touch"
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
                <div v-if="contentTypeText === 'Client Contact'" class="col-12 col-sm-6 col-md-4">
                  <label class="label q-mb-xs text-black">Content Provided/designed by<span class="required">*</span></label>
                  <div>
                    <q-select
                      v-model="model.contentProviderClientId"
                      use-input
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :options="contactList"
                      option-value="value"
                      option-label="text"
                      emit-value
                      map-options
                      :error="v$.contentProviderClientId?.$error"
                      :error-message="v$.contentProviderClientId?.$errors[0]?.$message"
                      @filter="contactListForFilter"
                      @blur="v$.contentProviderClientId?.$touch"
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
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="label q-mb-xs text-black">Caption<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.caption"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :error="v$.caption.$error"
                      :error-message="v$.caption.$errors[0]?.$message"
                      @blur="v$.caption.$touch"
                    />
                  </div>
                </div>
                <div class="col-12 col-sm-6 col-md-6">
                  <label class="label q-mb-xs text-black">#Tags<span class="required">*</span></label>
                  <div>
                    <q-input
                      v-model="model.tags"
                      outlined
                      stack-label
                      hide-bottom-space
                      :dense="true"
                      :error="v$.tags.$error"
                      :error-message="v$.tags.$errors[0]?.$message"
                      @blur="v$.tags.$touch"
                    />
                  </div>
                </div>
              </div>
            </fieldset>
          </div>
        </div>
        <!-- <q-separator /> -->
        <q-card-actions align="center" class="q-gutter-sm justify-center">
          <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="onDialogCancel" />
          <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" :disable="processing" no-caps />
        </q-card-actions>
      </q-form>
      <!-- </q-card-section> -->
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent, useQuasar } from "quasar";
import useVuelidate from "@vuelidate/core";
import projectService from "modules/project/projects.service";
import employeesService from "src/modules/employee/employee.service";
import commonService from "services/common.service";
import customerService from "src/modules/customer/customer.service";
import { required, helpers, minLength, maxLength, url } from "@vuelidate/validators";
import { ref, watch, onMounted, computed } from "vue";
import _ from "lodash";
import { notifySuccess, zwConfirm, notifyWarning } from "assets/utils";
import adPostService from "modules/marketing-ad-post/marketingAdPost.service";

// SOP Change :- Shared Dropdowns
import { getEditorConfig } from "src/composables/form-inputs/useEditorSettings.js";
// Shared Inputs
import singleFileUploader from "src/components/form-inputs/_singleFileUpload.vue";

// Common variables
// const baseURL = process.env.API_BASE_URL;
const loading = ref(true);
const processing = ref(false);
const $q = useQuasar();
const { fonts, toolbar } = getEditorConfig($q);
const isFileValid = ref(true);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Define model values
const model = ref({
  adNumber: "",
  name: "",
  description: "",
  projectId: "",
  customerId: "",
  url: "",
  imageType: "",
  imageProviderClientId: "",
  imageProviderEmpId: "",
  contentType: "",
  contentProviderClientId: "",
  contentProviderEmpId: "",
  caption: "",
  tags: ""
});

// get project details on edit mode
const getAdPost = () => {
  loading.value = true;
  adPostService.getAdPostDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.virtualPath = resp.picture ? resp.picture.virtualPath : "";
    model.value.description = resp.description ? resp.description : "";
  }).finally(() => {
    loading.value = false;
  });
};

// get AdPostNumber on edit mode
const getAdPostNumber = () => {
  loading.value = true;
  adPostService.getAdPostNumber().then((resp) => {
    if (model.value.id == null) {
      model.value.adNumber = resp;
    }
  }).finally(() => {
    loading.value = false;
  });
};

// get project details for customer
const getProject = (projectId) => {
  const project = projectList.value.find(x => x.value === projectId);

  if (project) {
    model.value.customerId = project.customerId;
    model.value.customerName = project.customerName;
  }
};

// -------------------------------------------------------------------------
// DropDown
// -------------------------------------------------------------------------
// Get all project list for dropdown
const projectList = ref([]);
const projectListOptions = ref([]);
function getAllProjectListForDropdown () {
  projectService.getAllProjectListForDropdown().then((resp) => {
    const responseData = resp
      .map((item) => ({ text: item.name, value: item.id, customerId: item.customer.id, customerName: item.customer.name }));
    projectList.value = responseData;
    projectListOptions.value = responseData;
    if (model.value.projectId) {
      getProject(model.value.projectId);
    }
  });
}

// Search project for dropdown
function projectListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      projectList.value = projectListOptions.value;
    } else {
      projectList.value = projectListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all Activity Owner list
const contactList = ref([]);
const contactListOptions = ref([]);
function getAllContactListForDropdown () {
  customerService.getAllContactListForDropdown("").then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName + (item.person.primaryEmailAddress ? " (" + item.person.primaryEmailAddress + ") (" + item.company.name + ")" : ""), value: item.person.id, company: item.company.name })).sort((a, b) => a.text.localeCompare(b.text));
    contactList.value = responseData;
    contactListOptions.value = responseData;
  });
}
// Search  Activity Owner for dropdown
function contactListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      contactList.value = contactListOptions.value;
    } else {
      contactList.value = contactListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all Activity Owner list
const employeeList = ref([]);
const employeeListOptions = ref([]);
function getAllEmployeesListForDropdown () {
  employeesService.getAllActiveEmployeesListForDropdown().then((resp) => {
    const responseData = resp.map((item) => ({ text: item.person.fullName, value: item.id })).sort((a, b) => a.text.localeCompare(b.text));
    employeeList.value = responseData;
    employeeListOptions.value = responseData;
  });
}
// Search  Activity Owner for dropdown
function employeeListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      employeeList.value = employeeListOptions.value;
    } else {
      employeeList.value = employeeListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all project Module Type List
const providerTypeList = ref([]);
const providerTypeListOptions = ref([]);
function getProviderType (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    providerTypeList.value = responseData;
    providerTypeListOptions.value = responseData;
  });
}
// Search project Module Type List for dropdown
function providerTypeListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      providerTypeList.value = providerTypeListOptions.value;
    } else {
      providerTypeList.value = providerTypeListOptions.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Computed property to get the imageTypeText's text
const imageTypeText = computed(() => {
  const selectedOption = providerTypeList.value.find(
    item => item.value === model.value.imageType
  );
  return selectedOption ? selectedOption.text : null;
});

// Computed property to get the contentTypeText's text
const contentTypeText = computed(() => {
  const selectedOption = providerTypeList.value.find(
    item => item.value === model.value.contentType
  );
  return selectedOption ? selectedOption.text : null;
});

// -------------------------------------------------------------------------------------------------------
// Upload Image
// -------------------------------------------------------------------------------------------------------
const documentUploaderRef = ref(null);
const previewUrl = ref(null);

const hasFile = computed(() =>
  model.value.postDesignPic || model.value.pictureId
);

function isVideo (type) {
  return type?.startsWith("video");
}

function isPdf (type) {
  return type === "application/pdf";
}

const isPreviewVideo = computed(() =>
  model.value.postDesignPic
    ? isVideo(model.value.postDesignPic.type)
    : /\.(mp4|mov|webm)$/i.test(model.value.virtualPath)
);

const isPreviewPdf = computed(() =>
  model.value.postDesignPic
    ? isPdf(model.value.postDesignPic.type)
    : /\.pdf$/i.test(model.value.virtualPath)
);

console.log("model.value.postDesignPic", model.value.postDesignPic);
const previewSource = computed(() =>
  model.value.postDesignPic
    ? previewUrl.value
    : model.value.virtualPath
);

const isPreviewImage = computed(() =>
  model.value.postDesignPic
    ? isImage(model.value.postDesignPic.type)
    : isImageFromPath(model.value.virtualPath)
);

const handleRemove = () => {
  if (model.value.postDesignPic) {
    removeSelectedFile();
  } else {
    clearImage();
  }
};

function isImage (type) {
  return type?.startsWith("image");
}

function isImageFromPath (path) {
  return /\.(jpg|jpeg|png|gif)$/i.test(path);
}

// function onFileAdded (files) {
//   const file = files[0];
//   const validFileTypes = [
//     "image/jpeg",
//     "image/png",
//     "image/gif",
//     "video/mp4",
//     "video/quicktime",
//     "video/webm"
//   ];
//   const allowedExtensions = ["jpg", "jpeg", "png", "gif", "mp4", "mov", "webm"];
//   const extension = file.name.split(".").pop().toLowerCase();
//   if (validFileTypes.includes(file.type) || allowedExtensions.includes(extension)) {
//     model.value.postDesignPic = file; // Assign the file to postDesignPic
//     model.value.postChangeFlag = "edit"; // Set the change flag
//     model.value.pictureId = "temp";
//     v$.value.pictureId.$touch();
//   } else {
//     // remove selected file
//     model.value.postDesignPic = null;
//     model.value.pictureId = null;
//     model.value.postChangeFlag = null;

//     // reset uploader UI
//     documentUploaderRef.value?.reset();
//     // Handle invalid file type
//     notifyError({ message: "Invalid file type. Please upload only Image, GIF or Video files." });
//   }
// }

// function onUploaded (info) {
//   notifySuccess({ message: "File Uploaded successfully." });
//   documentUploaderRef.value.reset();
// }

function handleFile (file) {
  model.value.postDesignPic = file;

  if (file) {
    model.value.postChangeFlag = "edit";
  } else {
    model.value.postDesignPic = null;
    model.value.pictureId = null;
    model.value.postChangeFlag = "remove";
  }
}

function removeSelectedFile () {
  model.value.postDesignPic = null;
  model.value.pictureId = null;
  model.value.postChangeFlag = "remove";

  documentUploaderRef.value?.reset();

  v$.value.pictureId.$touch();
}

function clearImage () {
  zwConfirm(
    { message: "Do you want to clear this Post ?" },
    () => {
      model.value.pictureId = null;
      model.value.postDesignPic = null;
      model.value.postChangeFlag = "remove";

      v$.value.pictureId.$touch();
    },
    () => {}
  );
}

// Validation rules
const rules = computed(() => ({
  projectId: { required: helpers.withMessage("Project name is required", required) },
  customerName: { required: helpers.withMessage("Client name is required", required) },
  name: { required: helpers.withMessage("Name is required", required), minLength: minLength(1), maxLength: maxLength(200) },
  // pictureId: { required: helpers.withMessage("Post Design is required", required) },
  pictureId: {
    required: helpers.withMessage(
      "Post Design is required",
      () => model.value.postDesignPic || model.value.pictureId
    )
  },
  imageType: { required: helpers.withMessage("Image Provider Type is required", required) },
  contentType: { required: helpers.withMessage("Content Provider Type is required", required) },
  caption: { required: helpers.withMessage("Caption is required", required) },
  tags: { required: helpers.withMessage("Tags is required", required) },
  url: { url },
  imageProviderEmpId:
    imageTypeText.value === "Vsky"
      ? {
        required: helpers.withMessage(
          "Employee is required",
          required
        )
      }
      : {},
  imageProviderClientId:
    imageTypeText.value === "Client Contact"
      ? {
        required: helpers.withMessage(
          "Client is required",
          required
        )
      }
      : {},
  contentProviderEmpId:
    contentTypeText.value === "Vsky"
      ? {
        required: helpers.withMessage(
          "Employee is required",
          required
        )
      }
      : {},
  contentProviderClientId:
    contentTypeText.value === "Client Contact"
      ? {
        required: helpers.withMessage(
          "Client is required",
          required
        )
      }
      : {}
}));
// Validate rules
const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// -------------------------------------------------------------------------------------------------------
// Submit form
// -------------------------------------------------------------------------------------------------------
const onSubmit = async () => {
  try {
    if (!isFileValid.value) {
      notifyWarning({ message: "Please upload a valid file" });
      return;
    }
    if (await v$.value.$validate()) {
      processing.value = true;
      await adPostService.saveAdPost(props.id, model.value);
      notifySuccess({ message: "Ad is saved successfully." });
      onDialogOK();
    }
  } catch (error) {
    console.error("Error in submitting:", error);
  } finally {
    processing.value = false;
  }
};

// watches a data property with the same name i.e. immediate effect
watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getAdPost();
  }
}, { immediate: true });

watch(() => model.value.postDesignPic, (file) => {
  if (previewUrl.value) {
    URL.revokeObjectURL(previewUrl.value);
  }
  previewUrl.value = file ? URL.createObjectURL(file) : null;
});

// On page rendering
onMounted(() => {
  getAllProjectListForDropdown();
  getAllContactListForDropdown();
  getAllEmployeesListForDropdown();
  getProviderType("Provider Type");
  getAdPostNumber();
});
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.edit_adModule .q-select__dropdown-icon{
  display: none;
}
.add-icon {
  border: 2px solid;
  padding: 4px;
  display: flex;
}
.preview-media {
  width: 35%;
  max-width: 100%;
  border-radius: 8px;
}
</style>
