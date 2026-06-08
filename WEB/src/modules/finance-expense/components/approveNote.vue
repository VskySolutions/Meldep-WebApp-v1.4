<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent>
    <q-card>
      <q-card-section>
        <div class="text-h6 RichTextEditor" v-html="title" />
        <div class="text-h6 RichTextEditor" v-html="message" />
        <div v-if="approver === 'Pre-Approved' || approver === 'Approved' || approver === 'Paid' || approver === 'Declined' || approver === 'Cancelled'" class="q-mt-sm">
          <label>Note <span v-if="approver === 'Declined' || approver === 'Cancelled'" class="text-negative">*</span></label>
          <q-editor
            v-model="note"
            min-height="100px"
          />
        </div>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn v-close-popup outline label="Cancel" color="negative" />
        <q-btn outline label="OK" color="primary" @click="onOk" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>
<script setup>
import { ref } from "vue";
import { useDialogPluginComponent } from "quasar";
import { notifyError } from "src/assets/utils";

const props = defineProps({
  title: String,
  message: String,
  approver: String
});
const title = ref(props.title);
const message = ref(props.message);
const approver = ref(props.approver);
// useDialogPluginComponent must be called *inside* <script setup>
const { dialogRef, onDialogOK } = useDialogPluginComponent();

const note = ref("");

const isNoteEmpty = (val) =>
  !val || val.replace(/<[^>]*>/g, "").trim().length === 0;

function onOk () {
  if (
    (props.approver === "Declined" || props.approver === "Cancelled") &&
    isNoteEmpty(note.value)
  ) {
    notifyError({
      message: "Note is required."
    });
    return;
  }

  onDialogOK(note.value);
}

</script>
