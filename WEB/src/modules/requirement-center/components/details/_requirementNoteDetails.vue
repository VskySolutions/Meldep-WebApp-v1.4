<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <div class="col scroll q-px-sm" style="overflow-y: auto; flex-grow: 1; height: 64vh; display: flex; flex-direction: column-reverse;">
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
              <div class="note-wrapper RichTextEditor">
                <span class="text-black note-text" v-html="note.note" />
              </div>
            </div>
          </q-timeline-entry>
        </template>
      </q-timeline>
      <div v-if="allNotes.length === 0">
        <h5 class="text-center text-grey">No Notes Available</h5>
      </div>
    </div>
  </q-card>
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
  notesType: { type: String, default: "" }
});

// common variables
const loading = ref(true);
const authStore = useAuthStore();
const user = authStore.user;

// notes
const allNotes = ref([]);

// get all notes and map list
const getAllNoteByTypeAndRecord = () => {
  loading.value = true;
  commonService.getAllNoteByTypeAndRecord(props.id, props.notesType, true).then((resp) => {
    allNotes.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// group the notes
const groupedNotes = computed(() => {
  return allNotes.value.reduce((groups, note) => {
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
.note-row {
  display: flex;
  align-items: center;
  gap: 6px;
}

.note-row .label {
  font-weight: bold;
  white-space: nowrap;
}

.note-text {
  display: inline-block; /* shrink-wraps to text width */
}
.note-row .q-btn {
  visibility: hidden; /* hide by default */
}

.note-row:hover .q-btn {
  visibility: visible; /* show when row hovered */
}
.notes-box-shadow {
  box-shadow: 0 1px 5px rgba(0, 0, 0, 0.2), 0 2px 2px rgba(0, 0, 0, 0.14), 0 3px 1px -2px rgba(0, 0, 0, 0.12) !important;
  background-color: #fff;
  border-radius: 4px 4px 4px 4px !important;
}
</style>
