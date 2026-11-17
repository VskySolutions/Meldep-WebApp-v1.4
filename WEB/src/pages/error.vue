<template>
  <NotAuthorized v-if="isDenied" />
  <div v-else class="fullscreen bg-blue text-white text-center q-pa-md flex flex-center">
    <div>
      <div class="text-h1 text-white q-mb-md">Page Not Found</div>
      <div class="text-h2 q-my-md text-white q-mb-md" style="opacity:.8">Oops! 😖 The requested URL was not found on this server.</div>
      <q-btn color="white" text-color="blue" unelevated label="Go Back" @click="goBack" />
    </div>
  </div>
</template>

<script setup>
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { getLocalStorage } from "assets/utils";
import { usePermissionStore } from "stores/permissions";
import NotAuthorized from "pages/not_authorized.vue";

const route = useRoute();
const router = useRouter();
const permissions = usePermissionStore();

// The guard sends denied pages here with their URL intact, and records which path it denied.
const isDenied = computed(() => permissions.deniedPath === route.path);

function goBack () {
  router.push(getLocalStorage("user")?.siteLandingPageLink || "/");
}
</script>
