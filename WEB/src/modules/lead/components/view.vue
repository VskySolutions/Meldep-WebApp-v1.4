<template>
  <q-dialog ref="dialogRef" class="customDialog q-pa-none dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">View Lead</div>
        <q-btn v-close-popup icon="o_close" color="white" class="close" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Lead Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6 col-sm-6">
                <div class="q-mb-xs">Lead</div>
                <div class="text-black">{{ model.fullName }}</div>
              </div>
              <div class="col-12 col-md-6 col-sm-6">
                <div class="q-mb-xs">Company</div>
                <div class="text-black">{{ model.companyName }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6 col-sm-6">
                <div class="q-mb-xs">Lead Group</div>
                <div class="text-black">{{ model.leadGroupName }}</div>
              </div>
              <div class="col-12 col-md-6 col-sm-6">
                <div class="q-mb-xs">Lead Source</div>
                <div class="text-black">{{ model.leadSource }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-md-6 col-sm-6">
                <div class="q-mb-xs">Lead Reference</div>
                <div class="text-black">{{ model.leadReference ? model.leadReference : '-'}}</div>
              </div>
              <div class="col-12 col-md-6 col-sm-6">
                <div class="q-mb-xs">Lead Arrival Date</div>
                <div class="text-black">{{ model.leadArrivalDate }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12">
                <div>Lead Note</div>
                <div class="text-black RichTextEditor" v-html="model.leadNote ? model.leadNote : '-'"></div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created By</div>
                <div class="text-black">
                  {{ model.createdBy ? model.createdBy : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Created Date</div>
                <div class="text-black">
                  {{ model.createdOnUtc ? model.createdOnUtc : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Updated By</div>
                <div class="text-black">
                  {{ model.updatedBy ? model.updatedBy : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Updated Date</div>
                <div class="text-black">
                  {{ model.updatedOnUtc ? model.updatedOnUtc : "-" }}
                </div>
              </div>
            </div>
          </fieldset>
          <fieldset>
            <legend>Activity Logs</legend>
            <q-table virtual-scroll :rows="rows" :columns="contactColumns" row-key="id">
              <template #header="contactProps">
                <q-tr :props="contactProps" class="bg-primary text-white">
                  <q-th v-for="col in contactProps.cols" :key="col.name" :props="contactProps">{{ col.label }}</q-th>
                </q-tr>
              </template>
              <template #body="contactProps">
                <q-tr :props="contactProps">
                  <q-td>{{ contactProps.row.leadStage?.stageName || '' }}</q-td>
                  <q-td>{{ contactProps.row.leadActivity?.activityName || '' }}</q-td>
                  <q-td>{{ contactProps.row.activityDate || '' }}</q-td>
                  <q-td style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 40%;">{{ contactProps.row.activityNote || '' }}</q-td>
                </q-tr>
              </template>
            </q-table>
          </fieldset>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
import { useDialogPluginComponent } from "quasar";
import { ref, watch } from "vue";
import _ from "lodash";
import leadService from "modules/lead/lead.service";

defineEmits([...useDialogPluginComponent.emits]);

const contactColumns = ref([
  { name: "leadStage.stageName", label: "Stage", field: "leadStage.stageName", align: "left", sortable: true },
  { name: "leadActivity.activityName", label: "Activity", field: "leadActivity.activityName", align: "left", sortable: true, style: "width: 10px" },
  { name: "activityDate", label: "Date", field: "activityDate", align: "left" },
  { name: "activityNote", label: "Note", field: "activityNote", align: "left" }
]);

const loading = ref(true);
const model = ref({
  fullName: "",
  clientName: "",
  leadSource: "",
  leadGroupName: "",
  leadReference: "",
  leadArrivalDate: "",
  leadNote: "",
  createdBy: "",
  updatedBy: ""
});
const rows = ref([]);
const props = defineProps({ id: { type: String, default: "" } });

const getLeadDetails = () => {
  loading.value = true;
  leadService.getLeadDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.fullName = resp.person.firstName + " " + resp.person.lastName;
    model.value.clientName = resp.client.company.name;
    model.value.companyName = resp.company.name;
    model.value.leadSource = resp.leadSources.dropDownValue;
    model.value.leadGroupName = resp.leadGroup.dropDownValue;
    model.value.createdBy = resp.createdBy.person.fullName;
    model.value.updatedBy = resp.updatedBy.person.fullName;
    rows.value = resp.leadActivityLogs.map(contact => ({
      ...contact,
      editing: false,
      companyId: model.value.companyId,
      flag: "Edit"
    }));
  }).finally(() => {
    loading.value = false;
  });
};
watch(() => props.id, (newValue) => {
  if (newValue) {
    getLeadDetails();
  }
}, { immediate: true });

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
