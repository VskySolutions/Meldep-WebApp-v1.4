<template>
  <div class="design-div">
    <q-card class="my-card" style="margin-top: 200px;">
      <q-card-section>
        <div style="text-align: center;">
          <q-img class="login-logo" src="/src/assets/logo.png" /><span class="fs-20 text-grey-14 text-bold q-ml-md">Vsky Solutions</span>
        </div>
        <div class="flex justify-center items-center">
          <div class="login-content q-pa-md">
            <h1 class="q-mb-md text-bold text-grey-14 fs-18 letter-space">Forgot Password</h1>
            <p class="letter-space">
              Please enter your registered email address below.
              We will send you an email with instructions on how to reset your password.
            </p>
            <q-form greedy @submit.prevent.stop="submit">
              <div class="row">
                <div class="col-12 form-group q-pb-sm">
                  <label class="Cutomlabel text-black fs-14 letter-space">Email Address<span class="required">*</span></label>
                  <q-input
                    v-model="model.email"
                    class="form-control" outlined placeholder="Enter email address" stack-label hide-bottom-space :dense="false"
                    :error="v$.email.$error" :error-message="v$.email.$errors[0]?.$message" @change="v$.email.$touch()"
                  />
                </div>
                <div class="col-12 q-mt-sm">
                  <q-btn label="Submit" type="submit" color="indigo-12" :loading="loading" class="q-ml-sm" style="width: 440px;" />
                  <div style="text-align: center">
                    <q-btn flat class="q-mt-md text-bold" color="indigo-12" label="Back to Login" @click="$router.push('/auth/login')" />
                  </div>

                </div>
              </div>
            </q-form>
          </div>
        </div>
      </q-card-section>
    </q-card>
    <q-card class="card-1">
      <q-card-section />
    </q-card>
    <q-card class="card-4">
      <q-card-section />
    </q-card>
    <q-card class="card-2">
      <q-card-section />
    </q-card>
    <q-card class="card-3">
      <q-card-section />
    </q-card>
  </div>
</template>

<script setup>
import { ref } from "vue";
import useVuelidate from "@vuelidate/core";
import { helpers, email } from "@vuelidate/validators";
import authService from "modules/auth/auth.service";
import { notifyError, notifySuccess, notifyWarning } from "assets/utils";

const loading = ref(false);
const model = ref({
  email: ""
});

const rules = {
  email: {
    // required: helpers.withMessage("Email is required", required),
    email: helpers.withMessage("Invalid email address", email)
  }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const submit = async () => {
  if (await v$.value.$validate()) {
    if (model.value.email === "") {
      notifyError({ message: "Email address is required" });
      return false;
    }
    loading.value = true;
    authService.forgotPassword(model.value).then((resp) => {
      // notifySuccess({ message: "Your password has been reset, and a new password has been sent to your email." });
      // notifySuccess({ message: "Reset password link sent on your email. Please check your email." });
      const { success, message } = resp;
      if (success) {
        notifySuccess({ message: message });
        model.value.email = "";
      } else {
        notifyWarning({ message: message });
      }
      // Clear the form values
      // model.value.email = "";
    }).finally(() => {
      loading.value = false;
    });
  }
};
</script>
<style scoped>

.login-logo{
  height: 40px;
  width: 40px;
}
.my-card{
    position: relative;
    z-index: 3;
  }
  .card-1{
    height: 230px;
    width: 230px;
    background-color: rgb(131, 164, 214);
    filter: opacity(0.1);
    overflow: auto;
    position: absolute;
    top: 165px;
    right: 920px;
    z-index: 2;
    border-radius: 20px;
  }
  .card-4{
    height: 280px;
    width: 170px;
    /* background-color: rgb(129, 226, 230); */
    border: 1px solid rgb(154, 181, 221);
    filter: opacity(0.2);
    overflow: auto;
    position: absolute;
    top: 120px;
    right: 880px;
    z-index: 1;
    border-radius: 30px;
  }
  .card-2{
    height: 150px;
    width: 150px;
    background-color: rgb(131, 164, 214);
    filter: opacity(0.2);
    overflow: auto;
    position: absolute;
    right: 530px;
    bottom: 220px;
    z-index: 2;
    border-radius: 10px;
  }
  .card-3{
    height: 200px;
    width: 200px;
    /* background-color: rgb(78, 109, 155); */
    border:5px dashed rgb(119, 199, 236);
    filter: opacity(0.2);
    overflow: auto;
    position: absolute;
    right: 500px;
    bottom: 200px;
    z-index: 1;
    border-radius: 10px;
  }
  </style>
