<template>
  <q-page style="padding-left: 18px; padding-top: 10px; padding-bottom: 15px;">
    <div style="width: 97vw;">
      <div class="row" style="width: 90vw;">
        <div class="col-12 col-md-10">
          <q-card style="width: 97vw">
            <q-card-section class="card-header">
              <div class="text-h2 text-primary">Profile Details</div>
            </q-card-section>
            <q-separator />
            <q-form greedy @submit.prevent.stop="onSubmit">
              <div class="card-body">
                <q-card-section>
                  <div class="row justify-center q-col-gutter-x-md q-mb-md">
                    <div class="col-xs-12 col-sm-6 col-md-4 col-lg-3">
                      <div class="q-mb-xs text-black">First Name<span class="required">*</span></div>
                      <q-input
                        v-model="model.firstName" outlined stack-label hide-bottom-space :dense="true"
                        :error="v$.firstName.$error" :error-message="v$.firstName.$errors[0]?.$message" @click="v$.firstName.$touch"
                      />
                    </div>
                    <div class="col-xs-12 col-sm-6 col-md-4 col-lg-3">
                      <div class="q-mb-xs text-black">Middle Name</div>
                      <q-input v-model="model.middleName" outlined stack-label hide-bottom-space :dense="true" />
                    </div>
                    <div class="col-xs-12 col-sm-6 col-md-4 col-lg-3">
                      <div class="q-mb-xs text-black">Last Name<span class="required">*</span></div>
                      <q-input
                        v-model="model.lastName" outlined stack-label hide-bottom-space :dense="true"
                        :error="v$.lastName.$error" :error-message="v$.lastName.$errors[0]?.$message" @click="v$.lastName.$touch"
                      />
                    </div>
                  </div>
                  <div class="row justify-center q-col-gutter-x-md q-mb-md">
                    <div class="col-xs-12 col-sm-6 col-md-4 col-lg-3">
                      <div class="q-mb-xs text-black">Email Address<span class="required">*</span></div>
                      <q-input
                        v-model="model.primaryEmailAddress" outlined stack-label hide-bottom-space :dense="true" type="email"
                        :error="v$.primaryEmailAddress.$error" :error-message="v$.primaryEmailAddress.$errors[0]?.$message" @click="v$.primaryEmailAddress.$touch"
                      />
                    </div>
                    <div class="col-xs-12 col-sm-6 col-md-4 col-lg-3">
                      <div class="q-mb-xs text-black">Background Color</div>
                      <q-input v-model="model.bgColor" filled>
                        <template #append>
                          <q-icon name="o_colorize" class="cursor-pointer">
                            <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                              <q-color v-model="model.bgColor" no-header no-footer default-view="palette" />
                            </q-popup-proxy>
                          </q-icon>
                        </template>
                      </q-input>
                    </div>
                    <div class="col-xs-12 col-sm-6 col-md-4 col-lg-3">
                      <div class="q-mb-xs text-black">Text Color</div>
                      <q-input v-model="model.color" filled>
                        <template #append>
                          <q-icon name="o_colorize" class="cursor-pointer">
                            <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                              <q-color v-model="model.color" no-header no-footer default-view="palette" />
                            </q-popup-proxy>
                          </q-icon>
                        </template>
                      </q-input>
                    </div>
                  </div>
                  <div class="row justify-center q-col-gutter-x-md q-mb-md">
                    <div class="col-xs-4 col-sm-4 col-md-4 col-lg-3">
                      <div class="q-mb-xs text-black">Profile Picture</div>
                      <!-- <div v-if="!model.pictureId">
                        <q-uploader
                          ref="documentUploaderRef"
                          color="white"
                          text-color="dark"
                          with-credentials
                          hide-upload-btn
                          field-name="personfile"
                          flat
                          bordered
                          label="Drag file here or (+) to upload. (image)"
                          @uploaded="onUploaded"
                          @added="onFileAdded"
                          style="min-height: 128px; width: 100%"
                        />
                        <div class="text-grey-7 text-caption q-mt-xs">
                          <i>Allowed Files: jpg, png, jpeg</i><br>
                          <i>500 * 500 below 1mb. </i>
                        </div>
                      </div> -->
                      <!-- <div v-if="model.pictureId" class="column items-center">
                          <img :src="model.virtualPath" alt="" style="width: 150px;">
                          <q-btn
                            color="negative"
                            label="Remove"
                            outline
                            no-caps
                            class="q-mt-sm"
                            @click="clearImage"
                          />
                        </div> -->
                      <singleFileUploader
                        :allowedTypes="['image/jpeg','image/png','image/jpg']"
                        :maxSizeInMb="25"
                        :imageSize="500"
                        :imageHeight="500"
                        :isImage="true"
                        label="Upload Profile Image"
                        @file-selected="handleFile"
                        @file-valid="isFileValid = $event"
                        :initialUrl="model.virtualPath"
                      />
                    </div>
                    <div class="col-xs-12 col-sm-4 col-md-4 col-lg-3">
                      <div v-if="shouldShowPreview" class="q-mt-md">
                        <div class="text-subtitle1 q-mb-sm">Preview:</div>
                        <div class="q-pa-sm q-mb-sm"
                          :style="{
                            backgroundColor: model.bgColor,
                            color: model.color,
                            borderRadius: '50%',
                            width: '60px',
                            display: 'inline-block',
                            width: 'auto',
                            whiteSpace: 'nowrap',
                            maxWidth: '100%'
                          }">
                          {{ initialsName }}
                        </div>
                      </div>
                    </div>
                    <div class="col-xs-12 col-sm-4 col-md-4 col-lg-3">
                    </div>
                  </div>
                </q-card-section>
                <div class="row justify-center q-gutter-sm q-pb-md">
                  <q-btn color="grey-4" push outline label="Close" type="button" class="text-grey-9 actionBtn" no-caps @click="$router.push(storedUser.siteLandingPageLink)" />
                  <q-btn color="primary" push outline label="Save" type="submit" class="actionBtn" :loading="processing" no-caps />
                </div>
              </div>
            </q-form>
          </q-card>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import useVuelidate from "@vuelidate/core";
import { required, helpers, email } from "@vuelidate/validators";
import accountService from "modules/account/account.service";
import _ from "lodash";
import { notifySuccess, getLocalStorage, notifyWarning } from "assets/utils";
import { useAuthStore } from "stores/auth";

// Shared Inputs
import singleFileUploader from "src/components/form-inputs/_singleFileUpload.vue";

const authStore = useAuthStore();
const isFileValid = ref(true);
const storedUser = getLocalStorage("user");

const model = ref({
  firstName: "",
  lastName: "",
  primaryEmailAddress: "",
  pictureId: null,
  bgColor: "",
  color: ""
});

const rules = {
  firstName: { required: helpers.withMessage("First name is required", required) },
  lastName: { required: helpers.withMessage("Last Name is required", required) },
  primaryEmailAddress: {
    required: helpers.withMessage("Email is required", required),
    email: helpers.withMessage("Invalid email", email)
  }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

function getProfile () {
  accountService.getProfile().then(resp => {
    model.value = _.cloneDeep(resp);
    model.value.id = resp.id;
    model.value.virtualPath = resp.picture ? resp.picture.virtualPath : "";
  });
}

// Upload Image
// -------------------------------------------------------------------------------------------------------
// const documentUploaderRef = ref(null);

// function onFileAdded (files) {
//   if (files[0]) {
//     model.value.personPic = files[0];
//     model.value.personChangeFlag = "edit";
//   }
// }

// function onFileAdded (files) {
//   const file = files[0];
//   if (!file) return;

//   // Allowed types
//   const allowedTypes = ["image/jpeg", "image/png", "image/jpg"];
//   if (!allowedTypes.includes(file.type)) {
//     notifyWarning({ message: "Only JPG, JPEG, and PNG files are allowed." });
//     documentUploaderRef.value.reset();
//     return;
//   }

//   // File size (1MB = 1048576 bytes)
//   if (file.size > 1048576) {
//     notifyWarning({ message: "File size must be below 1MB." });
//     documentUploaderRef.value.reset();
//     return;
//   }

//   // Image dimension check (500x500)
//   const img = new Image();
//   const objectUrl = URL.createObjectURL(file);

//   img.onload = function () {
//     if (img.width !== 500 || img.height !== 500) {
//       notifyWarning({ message: "Image must be exactly 500 x 500 pixels." });
//       documentUploaderRef.value.reset();
//       return;
//     }

//     // If all validations pass
//     model.value.personPic = file;
//     model.value.personChangeFlag = "edit";
//   };

//   img.onerror = function () {
//     notifyError({ message: "Invalid image file." });
//     documentUploaderRef.value.reset();
//   };

//   img.src = objectUrl;
// }

// function onUploaded (info) {
//   notifySuccess({ message: "File Uploaded successfully." });
//   documentUploaderRef.value.reset();
// }

// function clearImage () {
//   zwConfirm({ message: "Do you want to clear this Picture ?" }, () => {
//     model.value.pictureId = null;
//     model.value.personChangeFlag = "remove";
//   }, () => {
//   });
// }

function handleFile (file) {
  model.value.personPic = file;

  if (file) {
    model.value.personChangeFlag = "edit";
  } else {
    model.value.personPic = null;
    model.value.pictureId = null;
    model.value.personChangeFlag = "remove";
  }
}

const onSubmit = async () => {
  const isValid = await v$.value.$validate();

  if (!isFileValid.value) {
    notifyWarning({ message: "Please upload a valid file" });
    return;
  }

  if (isValid) {
    accountService.saveProfile(model.value).then(resp => {
      const user = {
        firstName: resp.firstName,
        lastName: resp.lastName,
        email: resp.email
      };
      authStore.setUserInfo(user);
      notifySuccess({ message: "Your profile has been successfully updated." });
      getProfile();
      window.location.reload();
    });
  }
};

// Only show preview if all required fields are filled
const shouldShowPreview = computed(() => {
  return model.value.firstName &&
        model.value.lastName &&
         model.value.bgColor &&
         model.value.color;
});

const initialsName = computed(() => {
  const first = model.value.firstName?.charAt(0) || "";
  const last = model.value.lastName?.charAt(0) || "";
  return (first + last).toUpperCase();
});

onMounted(() => {
  getProfile();
});

</script>
