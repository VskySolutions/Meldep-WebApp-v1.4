<template>
  <div class="q-pa-xl text-center demo" style="background-color: #a8cbdf">
    <div class="col-12 q-mx-auto">
      <div
        class="text-h4 text-weight-bold q-my-xl q-mb-md label q-mx-auto text-center"
        style="max-width: 900px;  margin: 0 auto;"
      >
        Book a Demo
      </div>
      <div class="q-mb-md q-px-md q-px-lg-md text-weight-medium text-subtitle1 text">
        MeldEp is built by business and development experts with 30+ years of experience. More than
        just a tool, it was created by our team, for our team, and has been successfully in use for
        over 5 years.
      </div>
      <div class="q-px-md q-px-lg-md text-subtitle1 text-weight-medium text">
        Our experts will personally guide you through the setup, process, and training, showing you
        exactly how we use it, so you can do the same.
      </div>
    </div>
  </div>
  <div class="q-pa-md">
    <q-form greedy @submit.prevent.stop="onSubmit">
      <div class="form-wrapper">
        <div class="row justify-start">
          <div class="col-12 col-md-12">
            <div class="q-mb-md q-py-sm">
              <label class="text-subtitle1 text-h6 text-weight-bold" style="font-size: 20px">
                Full Name <span class="required">*</span>
              </label>
              <q-input
                v-model="model.fullName"
                outlined
                stack-label
                hide-bottom-space
                placeholder="Enter Here"
                class="q-mt-sm"
                dense="false"
                :error="v$.fullName.$error"
                :error-message="v$.fullName.$errors[0]?.$message"
                @blur="v$.fullName.$touch"
              />
            </div>
            <div class="row q-col-gutter-xl q-mb-md q-py-sm">
              <div class="col-12 col-md-6">
                <label class="text-subtitle1 text-weight-regular" style="font-size: 20px">
                  Email Address <span class="required">*</span>
                </label>
                <q-input
                  v-model="model.emailAddress"
                  outlined
                  stack-label
                  hide-bottom-space
                  type="email"
                  placeholder="Enter Here"
                  class="q-mt-sm" dense="false"
                  :error="v$.emailAddress.$error"
                  :error-message="v$.emailAddress.$errors[0]?.$message"
                  @blur="v$.emailAddress.$touch"
                />
              </div>
              <div class="col-12 col-md-6">
                <label class="text-subtitle1 text-weight-regular" style="font-size: 20px">
                  Company Name
                </label>
                <q-input
                  v-model="model.companyName"
                  outlined
                  dense="false"
                  placeholder="Enter Here"
                  class="q-mt-sm"
                />
              </div>
            </div>
            <div class="row q-col-gutter-xl q-mb-md q-py-sm">
              <div class="col-12 col-md-6">
                <label class="text-subtitle1 text-weight-regular" style="font-size: 20px">
                  Business Size <span class="required">*</span>
                </label>
                <q-select
                  v-model="model.businessSizeId"
                  outlined
                  use-input
                  hide-bottom-space
                  label="---Select---"
                  label-color="grey"
                  :options="businessSizeList"
                  option-value="value"
                  option-label="text"
                  emit-value
                  map-options
                  class="q-mt-sm"
                  :dense="true"
                  :error="v$.businessSizeId.$error"
                  :error-message="v$.businessSizeId.$errors[0]?.$message"
                  @filter="getAllBusinessSizeListForFilter"
                  @blur="v$.businessSizeId.$touch"
                />
              </div>
              <div class="col-12 col-md-6">
                <label class="text-subtitle1 text-weight-regular" style="font-size: 20px">
                  Select Required Modules<span class="required">*</span>
                </label>
                <q-select
                  v-model="model.modulesIds"
                  outlined
                  use-input
                  use-chips=""
                  hide-bottom-space
                  label="---Select---"
                  label-color="grey"
                  :options="modulesList"
                  option-value="value"
                  option-label="text"
                  emit-value
                  map-options
                  multiple
                  fill-input
                  input-debounce="0"
                  class="q-mt-sm"
                  :dense="true"
                  :error="v$.modulesIds.$error"
                  :error-message="v$.modulesIds.$errors[0]?.$message"
                  @filter="getAllModuleListForFilter"
                  @blur="v$.modulesIds.$touch"
                >
                  <template #option="{ itemProps, opt, selected, toggleOption }">
                    <q-item v-bind="itemProps">
                      <q-item-section>
                        <div class="row q-col-gutter-x-md items-center text-black">
                          <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
                          <span>{{ opt.text }}</span>
                        </div>
                      </q-item-section>
                    </q-item>
                  </template>
                </q-select>
              </div>
            </div>
            <div class="row q-col-gutter-xl q-mb-md q-py-sm">
              <!-- Recaptcha -->
              <div class="q-mb-md q-mt-md">
                <div
                  class="g-recaptcha"
                  data-sitekey="6Lc4MnIsAAAAANdJ44Rq_3dqciIimtXnJzryC_WH"
                  data-callback="onReCaptchaVerified"
                />
              </div>
            </div>
            <div class="q-mt-lg flex justify-center">
              <q-btn
                label="Submit"
                type="submit"
                size="xl"
                color="primary"
                class="q-px-xl q-py-md text-white rounded-borders"
                :loading="processing"
                style="text-transform: none; font-size: 18px;"
              />
            </div>
          </div>
        </div>
      </div>
    </q-form>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useDialogPluginComponent } from "quasar";
import { notifySuccess, notifyWarning, notifyError } from "assets/utils";
import useVuelidate from "@vuelidate/core";
import { required, helpers, email } from "@vuelidate/validators";

import commonService from "services/common.service";
import moduleService from "modules/module/module.service";
import websiteDemoService from "modules/website/website.service";

const processing = ref(false);

defineEmits([...useDialogPluginComponent.emits]);
// const { onDialogOK } = useDialogPluginComponent();

const model = ref({
  fullName: "",
  emailAddress: "",
  companyName: "",
  businessSizeId: "",
  modulesIds: [],
  recaptchaToken: ""
});

const rules = {
  fullName: { required: helpers.withMessage("Full Name is required", required) },
  emailAddress: {
    required: helpers.withMessage("Email Address is required", required),
    email: helpers.withMessage("Invalid email", email)
  },
  businessSizeId: { required: helpers.withMessage("Business Size is required", required) },
  modulesIds: { required: helpers.withMessage("Modules are required", required) }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// Get all business size list
const businessSizeList = ref([]);
const businessSizeListFilter = ref([]);
function getAllBusinessSizeListForDropDown (typeName) {
  commonService.getDropDown(typeName).then((resp) => {
    const responseData = resp.map((item) => ({ text: item.dropdownValue, value: item.id }));
    businessSizeList.value = responseData;
    businessSizeListFilter.value = responseData;
  });
}

function getAllBusinessSizeListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      businessSizeList.value = businessSizeListFilter.value;
    } else {
      businessSizeList.value = businessSizeListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Get all module list
const modulesList = ref([]);
const modulesListFilter = ref([]);
function getAllModuleListForDropDown () {
  moduleService.getModulesList().then((resp) => {
    const responseData = resp.filter(item => item.name.toLowerCase() !== "settings").map((item) => ({ text: item.name, value: item.id }));
    modulesList.value = responseData;
    modulesListFilter.value = responseData;
  });
}

function getAllModuleListForFilter (val, update, abort) {
  update(() => {
    const needle = val ? val.toLowerCase() : "";
    if (needle === "") {
      modulesList.value = modulesListFilter.value;
    } else {
      modulesList.value = modulesListFilter.value.filter(v => v.text.toLowerCase().includes(needle));
    }
  });
}

// Submit form
// const onSubmit = async () => {
//   if (await v$.value.$validate()) {
//     processing.value = true;
//     websiteDemoService.saveWebsiteDemo(model.value).then((resp) => {
//       notifySuccess({ message: "Demo is saved successfully." });
//       model.value = {
//         fullName: "",
//         email: "",
//         companyName: "",
//         businessSizeId: "",
//         modulesIds: []
//       };
//       v$.value.$reset();
//       onDialogOK();
//     }).finally(() => {
//       processing.value = false;
//     });
//   }
// };

async function onSubmit () {
  processing.value = true;
  try {
    if (!recaptchaToken.value) {
      notifyWarning({ message: "Please complete reCAPTCHA verification." });
      return;
    }
    if (!await v$.value.$validate()) {
      return;
    }
    model.value.recaptchaToken = recaptchaToken.value;

    websiteDemoService.saveWebsiteDemo(model.value).then((resp) => {
      notifySuccess({ message: "Demo is saved successfully." });
      recaptchaToken.value = "";
      setTimeout(() => { window.location.reload(); }, 1000);
    }).finally(() => {
      processing.value = false;
    });
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
// recaptcha Token
const recaptchaToken = ref("");
// Google reCAPTCHA success callback
window.onReCaptchaVerified = function (token) {
  recaptchaToken.value = token;
};

onMounted(() => {
  getAllBusinessSizeListForDropDown("Business Size");
  getAllModuleListForDropDown();

  // for captcha
  const script = document.createElement("script");
  script.id = "recaptcha-script";
  script.src = "https://www.google.com/recaptcha/api.js";
  script.async = true;
  script.defer = true;
  document.head.appendChild(script);
});
</script>
<style scoped>
.demo {
  height: 20%;
}
.q-form {
  max-width: 900px;
  margin: auto;
}
.text {
  font-size: 20px;
  line-height: 1.6;
  margin: 0 auto;
}
@media (min-width: 1440px) {
  .label {
    font-size: 60px;
  }
  .text {
    font-size: 24px;
    width: 95%;
  }
}
.form-wrapper {
  width: 100%;
  max-width: 100%;
  margin: 0 auto;
  padding: 0 20px;
}

@media (min-width: 1440px) {
  .form-wrapper {
    width: 100%; /* Or 100% if you want true full-width */
    /* margin-left: -250px; */
    margin-top: 50px;
  }
}

.full-width {
  width: 100%;
}

/* hello */
.image {
  position: relative;
  display: flex;
  justify-content: flex-end;
  align-items: center;
  /* margin: 20px; */
}
.q-img {
  display: block;
  width: 750px;
  height: auto;
  box-shadow: 30px 30px 0px #317873;
}
@media (max-width: 768px) {
  .content-header p {
      text-align: justify;
  }
  .q-img {
    border: 5px solid #317873;
    box-shadow: 5px 5px 0px #317873;
  }
}
@media (max-width: 425px) {
  .content-header h1 {
      font-size: 30px;
      text-align: center;
  }
}
</style>
