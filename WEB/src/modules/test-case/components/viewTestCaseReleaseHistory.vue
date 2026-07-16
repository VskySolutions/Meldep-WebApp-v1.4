<template>
  <q-dialog
    ref="dialogRef"
    class="customDialog dialog-scrollable-content"
    full-height
    persistent
    position="right"
    @hide="onDialogHide"
  >
    <q-card
      class="q-dialog-plugin PersonMain card-header with-tools headerBasic"
      style="width:65vw !important;max-width:65vw;"
    >
      <!-- Header -->
      <q-card-section
        class="card-header with-tools bg-primary stickyHeader justify-between"
      >
        <div class="text-h2 text-white">
          Test Case Status Release History
        </div>

        <q-btn
          v-close-popup
          icon="o_close"
          class="close"
          color="white"
          flat
          round
          dense
        />
      </q-card-section>
      <q-separator />
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <testCaseReleaseHistoryTable
              :rows="rows"
              :loading="loading"
              :search="search"
              @update:search="search = $event"
              @refresh="loadHistory"
            />
          </div>
        </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, watch } from "vue";
import { useDialogPluginComponent } from "quasar";
import testcaseService from "modules/test-case/testCase.service";
import testCaseReleaseHistoryTable from "./_testCaseReleaseHistoryTable.vue";

defineEmits([
  ...useDialogPluginComponent.emits
]);

const {
  dialogRef,
  onDialogHide
} = useDialogPluginComponent();

const props = defineProps({
  id: {
    type: String,
    default: ""
  }
});

const loading = ref(false);
const rows = ref([]);
const search = ref("");

const loadHistory = async () => {
  if (!props.id)
    return;

  loading.value = true;
  try {
    rows.value =
      await testcaseService.getReleaseWiseTestCaseHistory(
        props.id
      );
  }
  finally {
    loading.value = false;
  }
};

watch(() => props.id, () => {
    loadHistory();
  },{ immediate: true }
);
</script>

<style>
.q-dialog__inner--minimized > div{
  max-height:calc(100vh)!important;
}

.q-dialog__inner--minimized{
  padding:0;
}
</style>
