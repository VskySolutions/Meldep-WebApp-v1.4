<template>
  <div class="design-div">
    <q-card class="my-card" style="margin-top: 200px;">
      <q-card-section>
        <div style="text-align: center;">
          <q-img class="login-logo" src="/src/assets/logo.png" /><span class="fs-20 text-grey-14 text-bold q-ml-md">Vsky Solutions</span>
        </div>
        <div class="flex justify-center items-center">
          <div class="login-content q-pa-md">
            <h1 class="q-mb-md text-bold text-grey-14 fs-18 letter-space">Set/Reset Password</h1>
            <q-form ref="zwform" greedy @submit.prevent.stop="submit">
              <div class="row">
                <div class="col-12 form-group q-pb-sm">
                  <label class="Cutomlabel text-black fs-13 letter-space">Email Address<span class="required">*</span></label>
                  <q-input
                    v-model="model.email"
                    class="form-control" outlined placeholder="Enter email" stack-label hide-bottom-space :dense="true" maxlength="128" autofocus readonly
                  />
                </div>
                <div class="col-md-12 form-group q-pb-sm">
                  <label class="Cutomlabel text-black fs-13 letter-space">New Password<span class="required">*</span></label>
                  <q-input
                    v-model="model.newPassword" outlined stack-label hide-bottom-space :dense="true" maxlength="20"
                    :type="isPassword ? 'password' : 'text'" autofocus
                    :error="v$.newPassword.$error" :error-message="v$.newPassword.$errors[0]?.$message" @blur="v$.newPassword.$touch" class="form-control"
                  >
                    <template #append>
                      <q-icon :name="isPassword ? 'o_visibility_off' : 'o_visibility'" class="cursor-pointer" @click="isPassword = !isPassword" />
                    </template>
                  </q-input>
                </div>
                <div class="col-md-12 form-group q-pb-sm" >
                  <label class="Cutomlabel text-black fs-13 letter-space">Confirm New Password<span class="required">*</span></label>
                  <q-input
                    v-model="model.confirmPassword" outlined stack-label hide-bottom-space :dense="true" maxlength="20" :type="isPassword2 ? 'password' : 'text'"
                    :error="v$.confirmPassword.$error" :error-message="v$.confirmPassword.$errors[0]?.$message" @blur="v$.confirmPassword.$touch" class="form-control"
                  >
                    <template #append>
                      <q-icon :name="isPassword2 ? 'o_visibility_off' : 'o_visibility'" class="cursor-pointer" @click="isPassword2 = !isPassword2" />
                    </template>
                  </q-input>
                </div>
                <div class="col-12 q-mt-sm">
                  <div class="col-12 text-center q-mt-md">
                    <q-btn label="Submit" type="submit" color="indigo-12" :loading="loading" style="width: 440px;" />
                  </div>
                  <div class="text-center">
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
import { ref, onMounted } from "vue";
import useVuelidate from "@vuelidate/core";
import { required, helpers, email, minLength } from "@vuelidate/validators";
import authService from "modules/auth/auth.service";
import { notifyError, notifySuccess } from "assets/utils";
import { useRoute } from "vue-router";
// import commonService from "services/common.service";

const isPassword = ref(true);
const isPassword2 = ref(true);
const loading = ref(false);
const model = ref({
  email: "",
  newPassword: "",
  confirmPassword: ""
});

const rules = {
  email: {
    required: helpers.withMessage("Email is required", required),
    email: helpers.withMessage("Invalid email", email)
  },
  newPassword: { required: helpers.withMessage("New password is required", required), minLength: minLength(8), containsLowerCase: helpers.withMessage(() => "The password must contain a lowercase character", (value) => /[a-z]/.test(value)), containsUppercase: helpers.withMessage(() => "The password must contain an uppercase character", (value) => /[A-Z]/.test(value)), containsNumber: helpers.withMessage(() => "The password must contain a number", (value) => /[0-9]/.test(value)), containsSpecialCharacter: helpers.withMessage(() => "The password must contain special character", (value) => /[#?!@$%^&*-]/.test(value)) },
  confirmPassword: { required: helpers.withMessage("Confirm password is required", required) }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });
const route = useRoute();
const userid = route.params.userid;
// const expires = route.params.token;
const submit = async () => {
  if (await v$.value.$validate()) {
    if (model.value.newPassword !== model.value.confirmPassword) {
      return notifyError({ message: "New password & Confirm Password are different." });
    }

    loading.value = true;
    // model.value.token = expires;
    authService.resetPassword(model.value).then((resp) => {
      notifySuccess({ message: "Your password has been reset successfully. Log in with your new password." });
    }).finally(() => {
      loading.value = false;
      // const router = useRouter();
      // router.push({ name: "reset_password", params: {} });
    });
  }
};

function getUser () {
  authService.getUser(userid).then((resp) => {
    model.value.email = resp.email;
  });
}

onMounted(() => {
  getUser();
});
</script>

<style>
.login-logo{
  height: 50px;
  width: 50px;
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
