<template>
  <router-view />
</template>

<script setup>
// Cross-site SSO token handling (?token=...) now lives in the router guard
// (src/router/index.js), which runs before the auth check on the very first
// navigation. Handling it here in onMounted was racy: onMounted fires after the
// guards, so the guard would already have redirected to /auth/login.
</script>
