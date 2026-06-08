<template>
  <div class="">
    <!-- <div class="text-center conference-contact-us-page_intro-section Theme-bgcolor">
      <h1 class="text-black" :class="$q.screen.gt.md ? 'fs-60' : 'fs-30'">Feel free to get <span class="meet-clr">in touch</span></h1>
      <p class="intro-para q-mb-none fs-16 text-black text-bold q-mt-md">If you have any questions or would like more information about the event, please fill out the form below, and we'll get back to you promptly.</p>
    </div> -->
    <div class="q-pa-lg text-center demo" style="background-color: #a8cbdf">
      <div class="col-12 q-mx-auto q-mt-sm">
        <h1 class="text-black" :class="$q.screen.gt.md ? 'fs-60' : 'fs-30'">Feel free to get <span class="meet-clr">in touch</span></h1>
        <p class="intro-para q-mb-none fs-16 text-black text-bold q-mt-md">If you have any questions or would like more information about the event, please fill out the form below, and we'll get back to you promptly.</p>
      </div>
    </div>
    <div class="contact_us q-mt-md">
      <div class="row justify-center">
        <!-- Form Section -->
        <q-card bordered class="col-xxl-4 col-xl-4 col-lg-4 col-md-5 col-sm-5 col-xs-11 card-form-transform" :class="$q.screen.gt.md ? 'q-pa-xl' : 'q-pa-lg'" style="border-radius: 30px !important; box-shadow: 0px 8px 15px rgba(0, 0, 0, 0.20); /* only bottom */">
          <h2 class="text-bold q-mb-lg" :class="$q.screen.gt.md ? 'fs-24' : 'fs-20'">Drop your Message here</h2>
          <q-form class="form" @submit.prevent.stop="onSubmit">
            <!-- Name -->
            <label for="name">Full Name<span class="required">*</span></label>
            <q-input
              v-model="model.fullName"
              dense
              for="name"
              class="text-white"
              outlined
              placeholder="Your Full Name"
              :error="v$.fullName.$error"
              :error-message="v$.fullName.$errors[0]?.$message"
              @input="removeInvalidCharacters" @blur="v$.fullName.$touch"
            />
            <!-- Email -->
            <label for="email">Email Address<span class="required">*</span></label>
            <q-input
              v-model="model.email"
              dense for="email"
              type="email"
              outlined
              placeholder="Your Email"
              :error="v$.email.$error"
              :error-message="v$.email.$errors[0]?.$message"
              @blur="v$.email.$touch"
            />
            <!-- Subject -->
            <label for="subject">Subject<span class="required">*</span></label>
            <q-input
              v-model="model.title"
              dense
              for="Title"
              outlined
              placeholder="Subject"
              :error="v$.title.$error"
              :error-message="v$.title.$errors[0]?.$message"
              @blur="v$.title.$touch"
            />
            <!-- Message -->
            <label for="message">Message</label>
            <q-input
              v-model="model.message"
              dense
              for="message"
              outlined
              type="textarea"
              placeholder="Message"
              stack-label
              hide-bottom-space
            />
            <!-- Recaptcha -->
            <div class="q-mb-md q-mt-md">
              <div
                class="g-recaptcha"
                data-sitekey="6Lc4MnIsAAAAANdJ44Rq_3dqciIimtXnJzryC_WH"
                data-callback="onReCaptchaVerified"
              />
            </div>
            <!-- Submit Button -->
            <div class="flex justify-center q-mt-lg">
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
          </q-form>
        </q-card>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { required, helpers, email } from "@vuelidate/validators";
import { notifySuccess, notifyError, notifyWarning } from "assets/utils";
import { useMeta } from "quasar";
import useVuelidate from "@vuelidate/core";
import contactUsService from "modules/website/website.service";

// common variables
const processing = ref(false);

// Existing logic from your script
const model = ref({
  fullName: null,
  email: null,
  title: null,
  message: null
});

// validation for Full Name
const alphaOnly = helpers.withMessage(
  "Full Name must contain only letters",
  helpers.regex(/^[A-Za-z\s]+$/)
);

// remove invalid characters
const removeInvalidCharacters = (event) => {
  const regex = /[^a-zA-Z\s]/g;
  event.target.value = event.target.value.replace(regex, "");
  model.value.fullName = event.target.value;
};

// define rules
const rules = {
  fullName: {
    required: helpers.withMessage("Full Name is required", required),
    alphaOnly
  },
  title: {
    required: helpers.withMessage("Subject is required", required)
  },
  email: {
    required: helpers.withMessage("Email is required", required),
    email: helpers.withMessage("Invalid email", email)
  }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

// save contact data
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
    const payload = {
      fullName: model.value.fullName,
      email: model.value.email,
      message: model.value.message,
      title: model.value.title,
      RecaptchaToken: recaptchaToken.value
    };

    contactUsService.saveContactUs(payload).then((resp) => {
      notifySuccess({ message: "Thank You For Contacting Us." });
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
  const script = document.createElement("script");
  script.id = "recaptcha-script";
  script.src = "https://www.google.com/recaptcha/api.js";
  script.async = true;
  script.defer = true;
  document.head.appendChild(script);
});

useMeta({
  title: "Meldep - Contact Us | Meldep",
  meta: {
    description: {
      name: "description",
      content: "If you have any questions or would like more information about the Meldep, please fill out the form, and we'll get back to you promptly."
    }
  }
});

</script>
<style scoped>
.recaptcha-wrapper {
  position: relative;
  width: fit-content;
  margin: 1rem auto;
}

/* Target the iframe inserted by reCAPTCHA and scale it */
.recaptcha-wrapper > .g-recaptcha {
  transform: translateZ(0); /* ensures GPU rendering */
}

.recaptcha-wrapper iframe[src*="recaptcha"] {
  width: 304px;     /* Default reCAPTCHA width */
  height: 78px;     /* Default reCAPTCHA height */
  transform: scale(1.2);       /* Adjust-to-your liking */
  transform-origin: top left;
}

/* If you previously styled the container with a border:
   It will now align perfectly. */
.recaptcha-wrapper {
  border: 2px solid red;   /* Optional: just to visualize */
  padding: 4px;
}
</style>
