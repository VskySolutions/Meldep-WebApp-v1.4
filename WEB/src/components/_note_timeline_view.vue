<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent position="right"  @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic column no-wrap" style="width: 50vw; max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">View Notes</div>
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
      <div v-if="AllNotes.length === 0">
          <h5 class="text-center text-grey">No Notes Available</h5>
      </div>
       <!-- Timeline Section (scrollable) -->
      <div class="col scroll q-px-xl" style="overflow-y:auto; flex-grow:1; display:flex; flex-direction:column">
        <q-timeline color="secondary">
          <template v-for="(group, date) in groupedNotes" :key="date">
            <q-timeline-entry
              v-for="note in group"
              :key="note.id"
              :side="user.userId === note.createdById ? 'right' : 'left'"
              color="primary"
              :icon="done_all"
            >
              <template v-slot:subtitle>
                <div class="text-weight-bolder text-primary">
                  {{ note.createdOnUtc }} • {{ note.user?.person?.fullName || '' }}
                </div>
              </template>
              <!-- NOTE BODY -->
              <div class="fs-14 note-row">
                  <div
                    class="note-wrapper RichTextEditor"
                    @click="user.username === note.user?.userName && isShow"
                  >
                    <span class="text-black note-text" v-html="note.note" />
                  </div>
              </div>
            </q-timeline-entry>
          </template>
        </q-timeline>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useAuthStore } from "stores/auth";
import { useDialogPluginComponent } from "quasar";
import commonService from "services/common.service";
import _ from "lodash";

defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({
  id: { type: String, default: "" },
  notesType: { type: String, default: "" },
  isShow: { type: Boolean, default: true }
});

// common variables
const loading = ref(true);
const authStore = useAuthStore();
const user = authStore.user;
const isShow = props.isShow;

// notes
const AllNotes = ref([]);
// const editingNoteId = ref(null);

// get all notes and map list
const getAllNoteByTypeAndRecord = () => {
  loading.value = true;
  commonService.getAllNoteByTypeAndRecord(props.id, props.notesType, true).then((resp) => {
    AllNotes.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// group the notes
const groupedNotes = computed(() => {
  return AllNotes.value.reduce((groups, note) => {
    // const date = new Date(note.createdDateStr).toDateString();
    const date = new Date(note.CreatedOnUtc);
    if (!groups[date]) {
      groups[date] = [];
    }
    groups[date].push(note);
    return groups;
  }, {});
});

// ======================================================================
// On page rendering
onMounted(() => {
  getAllNoteByTypeAndRecord();
});

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.note-row {
  display: flex;
  align-items: center;
  gap: 6px;
}
.note-row .label {
  font-weight: bold;
  white-space: nowrap;
}
.note-input {
  flex: 1;
  min-width: 100px;
}
.note-text {
  display: inline-block; /* shrink-wraps to text width */
}
.note-row .q-btn {
  visibility: hidden;
}
.note-row:hover .q-btn, .note-row.editing .q-btn {
  visibility: visible; /* show when row hovered */
}
.editor-locked .q-editor__toolbar {
  pointer-events: none;
  opacity: 0.6; /* optional - gives disabled look */
}

</style>
