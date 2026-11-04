<template>
  <q-card class="app-side-panel column no-wrap" :style="panelStyle">
    <!-- Fixed header: the primary band with the title (and the record it is about), then Close. -->
    <div class="app-side-panel__head row items-center no-wrap q-px-md q-py-sm bg-primary text-white">
      <div class="col app-side-panel__titles">
        <div class="text-h6 text-white ellipsis app-side-panel__title">
          {{ title }}
          <app-info-tip v-if="note" :text="note" color="white" size="16px" class="app-side-panel__note" />
        </div>
        <div v-if="subtitle" class="text-caption ellipsis app-side-panel__subtitle">{{ subtitle }}</div>
      </div>
      <slot name="head-actions" />
      <q-btn flat round dense color="white" icon="o_close" aria-label="Close" @click="$emit('close')">
        <q-tooltip>Close</q-tooltip>
      </q-btn>
    </div>

    <!-- Optional tab strip under the header. -->
    <slot name="tabs" />
    <q-separator />

    <!-- Scrollable body. A plain scrolling div rather than q-scroll-area: app.scss forces every
         q-scrollarea to calc(100% - 80px), which would cut the footer off. -->
    <div :class="['app-side-panel__body col scroll', { 'q-pa-md': !noPadding }]">
      <div v-if="loading" class="row flex-center q-pa-xl">
        <q-spinner color="primary" size="40px" />
      </div>
      <slot v-else />
    </div>

    <!-- Fixed footer: Cancel · extra actions · Save. -->
    <template v-if="!hideFooter">
      <q-separator />
      <div class="app-side-panel__foot row justify-end items-center q-pa-md bg-grey-1">
        <q-btn flat no-caps color="grey-8" :label="cancelLabel" @click="$emit('close')" />
        <slot name="footer-actions" />
        <q-btn
          v-if="!hideSave"
          unelevated
          no-caps
          color="primary"
          :label="saveLabel"
          :loading="saving"
          :disable="saving || saveDisable"
          @click="$emit('submit')"
        />
      </div>
    </template>
  </q-card>
</template>

<script setup>
// The card inside a right-side, full-height q-dialog: the form-drawer anatomy (primary header, scrolling
// body, Cancel + Save footer) for the dialogs this app opens through $q.dialog({ component }). The host
// keeps the q-dialog and its dialogRef; this is only the shell.
import { computed } from "vue";
import { useQuasar } from "quasar";
import AppInfoTip from "components/common/AppInfoTip.vue";

const props = defineProps({
  title: { type: String, default: "" },
  subtitle: { type: String, default: "" },
  // What this panel is for, on an info icon beside the title.
  note: { type: String, default: "" },
  // Desktop width; below md the panel takes the whole viewport.
  width: { type: String, default: "60vw" },
  minWidth: { type: String, default: "720px" },
  loading: { type: Boolean, default: false },
  saving: { type: Boolean, default: false },
  saveLabel: { type: String, default: "Save" },
  saveDisable: { type: Boolean, default: false },
  cancelLabel: { type: String, default: "Cancel" },
  hideSave: { type: Boolean, default: false },
  hideFooter: { type: Boolean, default: false },
  noPadding: { type: Boolean, default: false }
});

defineEmits(["close", "submit"]);

const $q = useQuasar();

const panelStyle = computed(() => ({
  width: $q.screen.lt.md ? "100vw" : props.width,
  minWidth: $q.screen.lt.md ? "0" : `min(${props.minWidth}, 100vw)`,
  maxWidth: "100vw"
}));
</script>

<style scoped>
.app-side-panel {
  height: 100vh;
  max-height: 100vh;
  border-radius: 0;
  box-shadow: none;
}
.app-side-panel__head {
  min-height: 56px;
  flex: 0 0 auto;
}
.app-side-panel__titles {
  min-width: 0;
}
.app-side-panel__title {
  font-size: 17px;
  font-weight: 600;
  line-height: 1.3;
}
.app-side-panel__note {
  opacity: 0.9;
}
.app-side-panel__subtitle {
  opacity: 0.85;
}
.app-side-panel__body {
  min-height: 0;
  overflow: auto;
  background: #fff;
}
.app-side-panel__foot {
  flex: 0 0 auto;
  gap: 8px;
}
</style>
