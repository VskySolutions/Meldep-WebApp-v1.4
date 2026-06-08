<template>
  <q-dialog ref="dialogRef" class="customDialog" persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 1120px; height: 100% !important;max-width: 150vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <!-- <q-form greedy @submit.prevent.stop="onSubmit"> -->
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator>
            <q-tab name="1_tab" label="Ad Details" class="q-px-lg q-mr-md" />
            <q-tab name="2_tab" label="Posting Status" class="q-px-lg" :disable="disableTab" />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated>
            <q-tab-panel name="1_tab">
              <fieldset>
                <legend>Ad Info</legend>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Ad Name</div>
                    <div class="text-black">
                      {{ model.name }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Customer Name</div>
                    <div class="text-black">
                      {{ model.customer.name }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Images provided / designed by</div>
                    <div class="text-black">
                      {{ model.imageTypeDropDown.dropDownValue === 'Client Contact' ? (model.imageProviderClient && model.imageProviderClient.fullName ? model.imageProviderClient.fullName : 'N/A') : (model.imageProviderEmp && model.imageProviderEmp.person && model.imageProviderEmp.person.fullName ? model.imageProviderEmp.person.fullName : 'N/A') }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Content provided / designed by</div>
                    <div class="text-black">
                      {{ model.contentTypeDropDown.dropDownValue === 'Client Contact' ? (model.contentProviderClient && model.contentProviderClient.fullName ? model.contentProviderClient.fullName : 'N/A') : (model.contentProviderEmp && model.contentProviderEmp.person && model.contentProviderEmp.person.fullName ? model.contentProviderEmp.person.fullName : 'N/A') }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Caption</div>
                    <div class="text-black" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                      {{ model.caption }}
                    </div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">#Tags</div>
                    <div class="text-black" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal;">
                      {{ model.tags }}
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-12 col-md-12">
                    <div class="q-mb-xs">Post URL</div>
                    <div class="text-black break-url">
                      <a
                        v-if="model.url"
                        :href="model.url"
                        target="_blank"
                        rel="noopener noreferrer"
                      >
                        {{ model.url }}
                      </a>
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-12 col-md-12">
                    <div class="q-mb-xs">Post Design</div>
                    <div class="text-black">
                      <!-- <div v-if="model.pictureId" class="row">
                        <img
                          v-if="isImageFromPath(model.virtualPath)"
                          :src="model.virtualPath"
                          style="width:30%"
                        >
                        <video
                          v-else
                          :src="model.virtualPath"
                          controls
                          style="width:40%"
                        />
                      </div> -->
                      <div v-if="model.pictureId" class="row">

                        <!-- Image -->
                        <img
                          v-if="isImageFromPath(model.virtualPath)"
                          :src="model.virtualPath"
                          style="width:30%"
                        >
                        <!-- Video -->
                        <video
                          v-else-if="isVideoFromPath(model.virtualPath)"
                          :src="model.virtualPath"
                          controls
                          style="width:40%"
                        />
                        <!-- PDF -->
                        <div v-else-if="isPdfFromPath(model.virtualPath)" class="q-mt-sm">
                          <a
                            :href="model.virtualPath"
                            download
                            target="_blank"
                            rel="noopener noreferrer"
                            style="text-decoration: none; text-align: center; display: inline-block;"
                          >
                            <i class="fas fa-download" />
                            Download File
                          </a>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-12 col-md-12">
                    <div class="q-mb-xs">Description:</div>
                    <div class="text-black RichTextEditor">
                      <p class="q-pt-md" v-html="model.description" />
                    </div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Created By</div>
                    <div class="text-black">
                      {{ model.createdBy.person.fullName ? model.createdBy.person.fullName : "-" }}
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
                      {{ model.updatedBy.person.fullName ? model.updatedBy.person.fullName : "-" }}
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
            </q-tab-panel>
            <q-tab-panel name="2_tab">
              <fieldset class="q-mb-lg">
                <legend>Posting Status</legend>
                <q-table
                  ref="tableRef"
                  v-model:pagination="pagination"
                  bordered
                  class="no-shadow"
                  :loading="loading"
                  :rows="rows"
                  :columns="columns"
                  row-key="id"
                  separator="cell"
                  no-data-label="No data available"
                  :rows-per-page-options="[20, 50, 100, 200, 500]"
                  binary-state-sort
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th v-for="col in props.cols" :key="col.name" :props="props">{{ col.label }}</q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr :props="props" :class="activeRowId == props.row.id ? 'highlight' : ''">
                      <q-td>{{ props.row.adPostChannel }}</q-td>
                      <q-td>{{ props.row.dateStr }}</q-td>
                      <q-td class="text-right">{{ props.row.likes }}</q-td>
                      <q-td class="text-right">{{ props.row.comments }}</q-td>
                      <q-td class="text-right">{{ props.row.shares }}</q-td>
                    </q-tr>
                  </template>
                </q-table>
              </fieldset>
            </q-tab-panel>
          </q-tab-panels>
        </div>
      </div>
      <!-- </q-form> -->
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";
import useFilters from "composables/useFilters";
import adPostService from "modules/marketing-ad-post/marketingAdPost.service";

const { toDate } = useFilters();
// Common variables
// const baseURL = process.env.API_BASE_URL;
const tab = ref("1_tab");
const loading = ref(true);
const rows = ref([]);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "-",
  caption: "",
  tags: "",
  url: "",
  description: "",
  project: {
    name: ""
  },
  customer: {
    name: ""
  },
  imageTypeDropDown: {
    dropDownValue: ""
  },
  imageProviderClient: {
    fullName: ""
  },
  imageProviderEmp: {
    person: {
      fullName: ""
    }
  },
  contentTypeDropDown: {
    dropDownValue: ""
  },
  contentProviderClient: {
    fullName: ""
  },
  contentProviderEmp: {
    person: {
      fullName: ""
    }
  },
  pictureId: null,
  virtualPath: "",
  createdBy: {
    person: {
      fullName: ""
    }
  },
  updatedBy: {
    person: {
      fullName: ""
    }
  }
});

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });
const columns = ref([
  { name: "adPostChannel", label: "Ad Channel Name", field: "adPostChannel", align: "left", sortable: true },
  { name: "dateStr", label: "Posted Date", field: "dateStr", align: "left", sortable: true },
  { name: "likes", label: "Likes", field: "likes", align: "left", sortable: true },
  { name: "comments", label: "Comments", field: "comments", align: "left", sortable: true },
  { name: "shares", label: "Shares", field: "shares", align: "left", sortable: true }
]);

// get Ad Post details
const getAdPostDetails = () => {
  loading.value = true;
  adPostService.getAdPostDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.virtualPath = resp.picture ? resp.picture.virtualPath : "";
    rows.value = resp.adPostingStatusList.map(item => ({
      ...item,
      adPostChannel: item.adPostChannel.name,
      dateStr: item.date,
      likes: item.likes,
      comments: item.comments,
      shares: item.shares
    }));
  }).finally(() => {
    loading.value = false;
  });
};

function isImageFromPath (path) {
  return /\.(jpg|jpeg|png|gif)$/i.test(path);
}

function isVideoFromPath (path) {
  return /\.(mp4|mov|webm)$/i.test(path);
}

function isPdfFromPath (path) {
  return path?.toLowerCase().includes(".pdf");
}

// On page rendering
onMounted(() => {
  getAdPostDetails();
});
</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
.break-url {
  word-break: break-word;
  overflow-wrap: anywhere;
  white-space: normal;
}
</style>
