<template>
  <div class="fullscreen bg-blue text-white text-center q-pa-md flex flex-center">
    <div>
      <div class="text-h1 text-white q-mb-md">Page Not Found</div>
      <div class="text-h2 q-my-md text-white q-mb-md" style="opacity:.8">Oops! 😖 The requested URL was not found on this server.</div>
      <q-btn color="white" text-color="blue" unelevated label="Go Back" @click="$router.push(storedUser.siteLandingPageLink)" />
    </div>
  </div>
</template>

<script setup>
import { getLocalStorage } from "assets/utils";
const storedUser = getLocalStorage("user");
</script>
