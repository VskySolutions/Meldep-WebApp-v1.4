<template>
  <q-page padding>
    <div class="row flex flex-center">
      <div class="col-md-12">
        <q-card>
          <q-card-section class="card-header">
            <h1 class=" q-mt-md text-center">Change Password</h1>
          </q-card-section>
          <q-separator />
          <q-form greedy @submit.prevent.stop="onSubmit">
            <q-card-section class="card-body">
              <div class="row q-mt-md flex-center">
                <div class="col-md-4 form-group" style="padding-left: 20px;padding-right: 50px;">
                  <q-input
                    v-model="model.oldPassword" outlined label="Current Password" stack-label hide-bottom-space :dense="false" maxlength="20"
                    :type="isPassword ? 'password' : 'text'" autofocus
                    :error="v$.oldPassword.$error" :error-message="v$.oldPassword.$errors[0]?.$message" @blur="v$.oldPassword.$touch"
                  >
                    <template #append>
                      <q-icon :name="isPassword ? 'o_visibility_off' : 'o_visibility'" class="cursor-pointer" @click="isPassword = !isPassword" />
                    </template>
                  </q-input>
                </div>
              </div>
              <div class="row flex-center">
                <div class="col-md-4 q-pr-xl q-mt-lg form-group" style="padding-left: 20px;">
                  <q-input
                    v-model="model.newPassword" outlined label="New Password" stack-label hide-bottom-space :dense="false" maxlength="20"
                    :type="isPassword2 ? 'password' : 'text'" autofocus
                    :error="v$.newPassword.$error" :error-message="v$.newPassword.$errors[0]?.$message" @blur="v$.newPassword.$touch"
                  >
                    <template #append>
                      <q-icon :name="isPassword2 ? 'o_visibility_off' : 'o_visibility'" class="cursor-pointer" @click="isPassword2 = !isPassword2" />
                    </template>
                  </q-input>
                </div>
              </div>
              <div class="row flex-center">
                <div class="col-md-4 q-pr-xl q-mt-lg form-group" style="padding-left: 20px;">
                  <q-input
                    v-model="model.confirmPassword" outlined label="Confirm Password" stack-label hide-bottom-space :dense="false" maxlength="20" :type="isconfirmPassword ? 'password' : 'text'" autofocus
                    :error="v$.confirmPassword.$error" :error-message="v$.confirmPassword.$errors[0]?.$message" @blur="v$.confirmPassword.$touch"
                  >
                    <template #append>
                      <q-icon :name="isconfirmPassword ? 'o_visibility_off' : 'o_visibility'" class="cursor-pointer" @click="isconfirmPassword = !isconfirmPassword" />
                    </template>
                  </q-input>
                </div>
              </div>
            </q-card-section>
            <!-- <q-separator /> -->
            <div class="row" style="margin-left: 30px;">
              <q-card-actions class="col" align="center">
                <q-btn label="Set New Password" type="submit" color="primary" no-caps />
                <!-- <q-btn label="Cancel" flat type="reset" color="primary" no-caps /> -->
                <q-btn label="Cancel" flat color="primary" no-caps @click="$router.push(storedUser.siteLandingPageLink)" />
              </q-card-actions>
            </div>
            <div class="q-mt-md row justify-center items-center">
              <q-card-section class="card-header">
                <h1 class="q-mb-md q-mr-xl">Password Requirements:</h1>
                <q-separator />
                <ul class="fs-15 text-grey-8" type="disc">
                  <li> Minimum 8 characters long - the more, the better </li>
                  <li class="q-mr-xl"> At least one lowercase character </li>
                  <li class="q-mr-xl"> At least one uppercase character </li>
                  <li>At least one number, symbol, or whitespace character </li>
                </ul>
              </q-card-section>
            </div>
          </q-form>
        </q-card>
      </div>
    </div>
  </q-page>
</template>

<script setup>
import { ref } from "vue";
import useVuelidate from "@vuelidate/core";
import { required, helpers, minLength } from "@vuelidate/validators";
import accountService from "modules/account/account.service";
import { notifySuccess, getLocalStorage } from "assets/utils";
import { useRouter } from "vue-router";

const router = useRouter();
const storedUser = getLocalStorage("user");

const isPassword = ref(true);
const isPassword2 = ref(true);
const isconfirmPassword = ref(true);
const loading = ref(true);

const model = ref({
  oldPassword: "",
  newPassword: "",
  confirmPassword: ""
});

const rules = {
  oldPassword: { required: helpers.withMessage("Current password is required", required) },
  newPassword: { required: helpers.withMessage("New password is required", required), minLength: minLength(8), containsLowerCase: helpers.withMessage(() => "The password must contain a lowercase character", (value) => /[a-z]/.test(value)), containsUppercase: helpers.withMessage(() => "The password must contain an uppercase character", (value) => /[A-Z]/.test(value)), containsNumber: helpers.withMessage(() => "The password must contain a number", (value) => /[0-9]/.test(value)), containsSpecialCharacter: helpers.withMessage(() => "The password must contain special character", (value) => /[#?!@$%^&*-]/.test(value)) },
  confirmPassword: { required: helpers.withMessage("Confirm password is required", required) }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const onSubmit = async () => {
  if (await v$.value.$validate()) {
    loading.value = true;
    accountService.changePassword(model.value).then((resp) => {
      notifySuccess({ message: "The password has been changed successfully." });
      router.push({ name: "login", params: {} });
    }).finally(() => {
      loading.value = false;
    });
  }
};
</script>

<style>
li{
  list-style-type: none;
}
li::before {
      content: '•'; /* Custom bullet */
      font-size: 30px; /* Adjust bullet size */
      margin-right: 10px; /* Space between bullet and text */
      color: black; /* Bullet color */
      color: #413f3f;
    }
</style>
