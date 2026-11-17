<template>
  <div class="fullscreen bg-grey-1 flex flex-center q-pa-md">
    <q-card flat bordered class="denied-card text-center q-pa-lg">
      <q-icon :name="couldNotCheck ? 'o_cloud_off' : 'o_lock'" color="grey-7" size="48px" />
      <div class="denied-code q-mt-md">{{ couldNotCheck ? "Access not confirmed" : "404" }}</div>
      <div class="text-h5 q-mt-xs">{{ couldNotCheck ? "We couldn't check your access" : "Unauthorized Access" }}</div>
      <div class="text-body2 text-grey-8 q-mt-sm q-mb-lg">
        {{ couldNotCheck
          ? "Please check your connection and try again."
          : "This page doesn't exist, or you don't have permission to open it." }}
      </div>
      <q-btn v-if="couldNotCheck" unelevated no-caps color="primary" label="Try again" @click="retry" />
      <q-btn v-else unelevated no-caps color="primary" label="Go to home page" @click="goHome" />
    </q-card>
  </div>
</template>

<script setup>
import { computed } from "vue";
import { LocalStorage } from "quasar";
import { useRouter } from "vue-router";
import { usePermissionStore } from "stores/permissions";

const router = useRouter();
const permissions = usePermissionStore();

const couldNotCheck = computed(() => permissions.loadFailed);

function retry () {
  window.location.reload();
}

// The home page must be one this user can open: last_route may be the page that was just denied.
function goHome () {
  const user = LocalStorage.getItem("user");
  if (!LocalStorage.getItem("token")) {
    router.replace("/auth/login");
    return;
  }

  const landing = user?.siteLandingPageLink;
  const target = landing && router.resolve(landing).path !== permissions.deniedPath ? landing : "/dashboard";
  localStorage.setItem("last_route", target);
  router.replace(target);
}
</script>

<style scoped>
.denied-card {
  width: 100%;
  max-width: 440px;
  border-radius: 12px;
}

.denied-code {
  font-size: 40px;
  font-weight: 600;
  line-height: 1.2;
}
</style>
