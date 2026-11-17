<template>
  <div class="fullscreen bg-grey-1 flex flex-center q-pa-md">
    <q-card flat bordered class="no-site-card text-center q-pa-lg">
      <q-icon name="o_domain_disabled" color="grey-7" size="48px" />
      <div class="text-h5 q-mt-md">No site access</div>
      <div class="text-body2 text-grey-8 q-mt-sm q-mb-lg">
        Your account isn't assigned to an active site. Please contact your administrator.
      </div>
      <q-btn unelevated no-caps color="primary" label="Log out" @click="onLogout" />
    </q-card>
  </div>
</template>

<script setup>
import { useRouter } from "vue-router";
import { useAuthStore } from "stores/auth";

const router = useRouter();
const authStore = useAuthStore();

function onLogout () {
  authStore.logout();
  router.replace({ name: "login" });
}
</script>

<style scoped>
.no-site-card {
  width: 100%;
  max-width: 440px;
  border-radius: 12px;
}
</style>
