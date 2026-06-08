<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card
      class="q-dialog-plugin PersonMain card-header with-tools headerBasic"
      style="width: 40vw; max-width: 40vw;"
    >
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">View Leave
          <q-icon name="o_edit" class="cursor-pointer q-mr-sm text-white q-pa-xs q-mx-md" :disable="!isFutureOrCurrentEvent(model.date)" style="border: 1px solid white; border-radius: 4px;" @click="onEdit(model.id, model.title)">
            <q-tooltip>Edit</q-tooltip>
          </q-icon>
          <q-icon name="o_delete_outline" class="cursor-pointer q-mr-sm text-white q-pa-xs q-mx-xs" color="negative" style="border: 1px solid white; border-radius: 4px;" @click="onDelete(model)">
            <q-tooltip>Delete</q-tooltip>
          </q-icon>
        </div>
        <q-btn v-close-popup icon="o_close" color="white" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Leave Info</legend>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 col-sm-4 col-md-4">
                <div class="form-group">
                  <p class="text-primary">Date</p>
                </div>
              </div>
              <div class="col-12 col-sm-8 col-md-8">
                <div class="form-group">
                  <p class="text-black">{{ model.date }}</p>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 col-sm-4 col-md-4">
                <div class="form-group">
                  <p class="text-primary">Title</p>
                </div>
              </div>
              <div class="col-12 col-sm-8 col-md-8">
                <div class="form-group">
                  <p class="text-black">{{ model.title }}</p>
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-12 col-sm-4 col-md-4">
                <div class="form-group">
                  <p class="text-primary">Description</p>
                </div>
              </div>
              <div class="col-12 col-sm-8 col-md-8">
                <div class="form-group RichTextEditor">
                  <p class="text-black" v-html="model.description ? model.description : '-'" />
                </div>
              </div>
            </div>
          </fieldset>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent, useQuasar } from "quasar";
import { ref, watch, onMounted } from "vue";
import { zwConfirmDelete, notifySuccess } from "assets/utils";
import _ from "lodash";
import yearlyLeaveService from "modules/leave-yearly-schedule/leaveYearlySchedule.service";
import addEditEvents from "modules/leave-yearly-schedule/components/addEditEvents.vue";
import addLeaveSchedule from "modules/leave-yearly-schedule/components/addEdit.vue";

defineEmits([...useDialogPluginComponent.emits]);
const loading = ref(true);
const $q = useQuasar();
const model = ref({
  id: "",
  title: "",
  description: "",
  date: "",
  leaveRuleId: ""
});

const props = defineProps({ id: { type: String, default: "" } });

// Load event details if editing
onMounted(() => {
  if (props.id) {
    getEvents();
  }
});

const isFutureOrCurrentEvent = (eventDate) => {
  const today = new Date();
  const formattedToday = `${(today.getMonth() + 1).toString().padStart(2, "0")}/${today.getDate().toString().padStart(2, "0")}/${today.getFullYear()}`;
  return eventDate >= formattedToday; // True if today or future
};

const getEvents = () => {
  if (!props.id) return; // If no ID, do nothing (Add mode)
  loading.value = true;
  yearlyLeaveService.getEvents(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.id = resp.id;
    model.value.title = resp.title;
    model.value.description = resp.description;
    model.value.leaveRuleId = resp.leaveRuleId;
    model.value.date = resp.date;
  }).finally(() => {
    loading.value = false;
  });
};

watch(() => props.id, (newValue, oldValue) => {
  if (newValue) {
    getEvents();
  }
}, { immediate: true });

// Edit popup
const onEdit = (id, title) => {
  model.value.id = id;
  let editEventPopUp = addEditEvents; // Default edit component
  if (title === "Weekly Saturday's Off") {
    editEventPopUp = addLeaveSchedule; // Open Saturday Off Edit Page
  }
  $q.dialog({
    component: editEventPopUp,
    componentProps: { id }
  }).onOk(() => {
    getEvents();
  }).onCancel(() => {
  }).onDismiss(() => {
  // model.value.id = null;
  });
};

// Delete record
const onDelete = (item) => {
  model.value.id = item.id;
  zwConfirmDelete({ data: `${item.title}` }, () => {
    yearlyLeaveService.deleteEvent(item.id).then(resp => {
      notifySuccess({ message: "Leave event is deleted successfully." });
      // getEvents();
    });
    setTimeout(() => {
      window.location.reload();
    }, 1000);
  }, () => {
    model.value.id = null;
  });
};

</script>
<style>
.q-dialog__inner--minimized>div {
  max-height: calc(100vh) !important;
}

.q-dialog__inner--minimized {
  padding: 0;
}
</style>
