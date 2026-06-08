<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 60vw !important; max-width: 60vw !important;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">Email Preview</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />

      <!-- Subject Section -->
      <q-card-section class="q-ml-xl">
        <div class="text-h3 text-black text-bold">Subject :
        <span class="text-h2 text-primary">{{ previewSubject }}</span>
        </div>
      </q-card-section>

      <!-- Email Body -->
      <q-card-section class="q-pa-none RichTextEditor">
        <div class="q-pa-none RichTextEditor" v-html="previewHtml"></div>
      </q-card-section>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { ref, onMounted } from "vue";
import { useDialogPluginComponent } from "quasar";
import notificationsService from "modules/notification/notifications.service";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Common variables
const previewDialog = ref(false);
const previewHtml = ref("");
const previewSubject = ref("");
const loading = ref(true);

// preview email
const openPreview = () => {
  loading.value = true;
  notificationsService.getEmailPreview(props.id)
    .then((resp) => {
      previewHtml.value = resp.html; // API should return built HTML
      previewSubject.value = resp.subject;
      previewDialog.value = true;
    })
    .finally(() => {
      loading.value = false;
    });
};

// On page rendering
onMounted(() => {
  openPreview();
});
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
