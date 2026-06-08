<template>
  <q-page padding>
    <q-card>
      <q-card-section class="card-header">
        <h1>Approve Leave</h1>
      </q-card-section>
      <q-separator />
      <q-card-section class="card-body">
        <!-- Other content goes here -->
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup>
import ApproveLeave from "modules/leave/components/_approveOrDeclineLeave.vue";
import { ref, onMounted } from "vue";
import { useQuasar } from "quasar";
import { useRoute } from "vue-router";

const $q = useQuasar();
const route = useRoute();
const activeRowId = ref(null);

// Function to open the approver popup
const openApproverPopup = (id) => {
  if (id) {
    activeRowId.value = id; // Set the active ID
    $q.dialog({
      component: ApproveLeave,
      componentProps: { id, approve: "approve" } // Pass the ID to the dialog
    });
  }
};

// Automatically open the popup with the specific ID when the component is mounted
onMounted(() => {
  const id = route.query.id; // Set the ID directly
  openApproverPopup(id); // Open the popup with the predefined ID
});
</script>
