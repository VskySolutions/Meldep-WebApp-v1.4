<template>
  <div class="flex flex-center q-pa-md">
    <q-card class="my-card" style="margin-top: 100px;">
      <q-card-section class="q-pa-xl">
        <div class="bg-div bg-white">
          <div class="login-content">
            <div style="text-align: center;">
              <img src="~assets/meldep_logo.png" alt="logo" class="logo" style="width: 20%;">
              <!-- <q-img class="login-logo" src="/src/assets/logo.png" /><span class="fs-20 text-grey-14 text-bold q-ml-md">Meldep</span> -->
              <h1 class="q-mt-lg text-left text-grey-14 fs-18 letter-space">Welcome To Vsky Solutions!  👋</h1>
            </div>
            <div class="q-mt-sm fs-14">
              <p class="letter-space text-grey-7">Please sign-in to your account and start the adventure</p>
            </div>
            <q-form ref="zwform" class="q-mt-lg" greedy @submit.prevent.stop="login">
              <div class="row flex flex-center">
                <div class="col-12 form-group q-pb-lg">
                  <label class="Cutomlabel text-black fs-15">Username</label>
                  <q-input v-model="model.username" class="form-control input-width input-size" outlined placeholder="Enter username" stack-label hide-bottom-space :dense="false" maxlength="128" autofocus :error="v$.username.$error" :error-message="v$.username.$errors[0]?.$message" @click="v$.username.$touch" />
                </div>
                <div class="col-12  form-group q-pb-sm">
                  <label class="Cutomlabel text-black fs-15">Password</label>
                  <q-input
                    v-model="model.password" class="form-control input-width" outlined placeholder="Enter password" autocomplete="off" stack-label hide-bottom-space :dense="false" maxlength="28" :type="isPassword ? 'password' : 'text'"
                    :error="v$.password.$error" :error-message="v$.password.$errors[0]?.$message" @click="v$.password.$touch"
                  >
                    <template #prepend>
                      <q-icon :name="isPassword ? 'o_visibility_off' : 'o_visibility'" class="cursor-pointer" @click="isPassword = !isPassword" />
                    </template>
                  </q-input>
                </div>
                <div class="row items-center justify-between q-mt-sm" style="width: 400px;">
                  <q-checkbox
                    v-model="model.isRememberMeChecked"
                    label="Remember Me"
                    color="indigo-12"
                  />
                  <router-link
                    :to="{ name: 'forgot_password', params: {} }"
                    class="q-link text-indigo-12 text-bold"
                  >
                    Forgot Password?
                  </router-link>
                </div>
                <div class="row col-12 justify-center q-mt-md">
                  <q-btn label="Login" type="submit" color="indigo-12" :loading="loading" style="width: 400px;" />
                </div>
                <!-- <q-btn label=" Back To Home" flat class="q-btn bg-dark text-white q-mt-md q-mx-md cursor-pointer q-mr-sm" @click="$router.push('/')" /> -->
                <!-- <q-btn flat class="q-mt-md text-bold" color="indigo-12" label="Back to Home" @click="$router.push('/')" /> -->
              </div>
            </q-form>
          </div>
        </div>
        <div class="row justify-center q-mt-lg">
          <div class="col-md-8">
            <a :class="isMsLoginDisabled ? '' : ''" href="javascript:void(0)" class="text-dark mslogin-btn" label="Login with Microsoft" @click="handleLogin">
              <div>
                <svg width="20" height="20" viewBox="0 0 1024 1024" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path d="M44.522 44.5217H489.739V489.739H44.522V44.5217Z" fill="#F35325" />
                  <path d="M534.261 44.5217H979.478V489.739H534.261V44.5217Z" fill="#81BC06" />
                  <path d="M44.522 534.261H489.739V979.478H44.522V534.261Z" fill="#05A6F0" />
                  <path d="M534.261 534.261H979.478V979.478H534.261V534.261Z" fill="#FFBA08" />
                </svg>
              </div>
              <div class="q-px-md text-weight-medium">
                Sign in with Microsoft
              </div>
            </a>
          </div>
        </div>
      </q-card-section>
    </q-card></div>
</template>

<script setup>
import { ref, computed } from "vue";
import useVuelidate from "@vuelidate/core";
import { required, helpers } from "@vuelidate/validators";
import { useRouter } from "vue-router";
import { useAuthStore } from "stores/auth";
import { setLocalStorage, getLocalStorage, clearLocalStorage } from "assets/utils";

import moduleService from "modules/module/module.service";
// import { msalInstancePromise } from "modules/auth/auth.service";
// import authService from "modules/auth/auth.service";
const router = useRouter();
const authStore = useAuthStore();

const loading = ref(false);
const isPassword = ref(true);

// Set Filters to local storage
const localStorageKey = "Login";
const filterLocalStorage = getLocalStorage(localStorageKey);

const username = filterLocalStorage ? filterLocalStorage.username : "";
const password = filterLocalStorage ? filterLocalStorage.password : "";
const isRememberMeChecked = filterLocalStorage ? filterLocalStorage.isRememberMeChecked : ref(false);

const model = ref({
  username,
  password,
  isRememberMeChecked
});

const Loginmodel = ref({
  token: ""
});

const rules = {
  username: { required: helpers.withMessage("Username is required", required) },
  password: { required: helpers.withMessage("Password is required", required) }
};

const v$ = useVuelidate(rules, model, { $lazy: true, $autoDirty: true });

const login = async () => {
  if (await v$.value.$validate()) {
    loading.value = true;
    authStore.login(model.value).then((resp) => {
      if (model.value.isRememberMeChecked === true) {
        setLocalStorage(localStorageKey, model.value);
      } else {
        clearLocalStorage(localStorageKey);
      }
      if (resp?.token) {
        localStorage.setItem("access_token", resp.token);
      }
      if (resp?.roles?.includes("system-super-admin")) {
        router.push({ name: "settings", params: {} });
      } else {
        redirectToLanding();
        // router.push({ name: "projects", params: {} });
      }
    }).finally(() => {
      loading.value = false;
    });
  }
};

async function handleLogin () {
  try {
    const { msalInstancePromise } = await import("../../auth/auth.service");
    const msalInstance = await msalInstancePromise;
    msalInstance.clearCache();
    // Check if an interaction is already in progress
    if (msalInstance.getActiveAccount() === null && msalInstance.getAllAccounts().length === 0) {
      const loginResponse = await msalInstance.loginPopup({
        scopes: ["user.read"] // Replace with your app's scopes
      });
      // const loginResponse = await msalInstance.loginPopup({
      //   scopes: ["user.read", "openid", "profile", "email", "api://5f80e284-2b73-4227-88cd-3f4843f0e80a/api.read"] // Replace with your app's scopes
      // });
      const accessToken = loginResponse.accessToken;
      // Send the token to the backend
      Loginmodel.value.token = `Bearer ${accessToken}`;
      loading.value = true;
      authStore.mslogin(Loginmodel.value).then((resp) => {
        if (resp.loginErrorMessage) {
          router.push({ name: "not_authorized", params: {} });
        } else {
          if (resp?.token) {
            localStorage.setItem("access_token", resp.token);
          }
          if (resp?.roles?.includes("system-super-admin")) {
            router.push({ name: "settings", params: {} });
          } else {
            redirectToLanding();
            // router.push({ name: "projects", params: {} });
          }
        }
      }).finally(() => {
        loading.value = false;
      });

      // Handle the response, e.g., save the token or user details
    } else {
      console.warn("Interaction is already in progress or user is already logged in.");
    }
  } catch (error) {
    console.error("Login failed:", error);
  }
}

const isMsLoginDisabled = computed(() => {
  return model.value.username !== "" || model.value.password !== "";
});

const redirectToLanding = async () => {
  const allModules = await moduleService.getSiteActiveModulesMenus();

  const landingMenu = allModules
    .flatMap(module => module.customSiteModuleMenuList  || [])
    .find(menu => menu.setAsLanding > 0);

  if (landingMenu && landingMenu?.link) {
    localStorage.setItem("last_route", landingMenu.link);
    router.push(`${landingMenu.link}`);
  } else {
    localStorage.setItem("last_route", "/dashboard");
    router.push("/dashboard");
  }
};
</script>

<!-- For checkbox  -->
  <style scoped>
  .login-logo{
    height: 40px;
    width: 40px;
  }
  /* For design */

  .my-card{
    position: relative;
    width: 500px;
    z-index: 3;
  }
  .pointer-disbled{
    pointer-events: none;
    cursor: not-allowed;
    opacity: 0.6;
  }
  .mslogin-btn {
    text-decoration: blink;
    display: flex;
    justify-content: center;
    align-items: center;
    border: 1px solid #b3b3b3;
    padding: 10px;
    border-radius: 5px
}
  </style>
