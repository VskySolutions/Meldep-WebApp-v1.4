<template>
  <q-page class="q-pa-lg">
    <div>
      <div class="column items-center">
        <q-img
          src="~assets/meldep_logo.png"
          style="width: 120px"
          class="q-mb-md"
        />
        <div class="text-h1 text-weight-bold text-primary">
          Privacy Policy
        </div>
      </div>
      <div>
        <PrivacyPolicyContent />
      </div>
    </div>
  </q-page>
</template>

<script setup>
import PrivacyPolicyContent from "src/components/_privacy_policy_content.vue";
</script>
