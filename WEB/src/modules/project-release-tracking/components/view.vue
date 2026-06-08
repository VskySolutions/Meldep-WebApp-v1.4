<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader justify-between">
        <div class="text-h2 text-white q-mr-lg">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <q-tabs v-model="tab" dense class="text-primary" active-color="primary" indicator-color="primary" active-class="bg-blue-1 borderRadiusTabs" align="left" narrow-indicator inline-label mobile-arrows>
            <q-tab name="1_tab" label="Release Info." class="q-px-lg q-mr-md" />
            <q-tab name="2_tab" label="Release Tracking Items" class="q-px-lg" :disable="disableTab" />
          </q-tabs>
          <q-separator />
          <q-tab-panels v-model="tab" animated>
            <q-tab-panel name="1_tab">
              <!-- <q-card class="card-header with-tools headerBasic"> -->
              <fieldset>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Project Name</div>
                    <div class="text-black q-mb-sm">{{ model.project.name }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Infra Instance</div>
                    <div class="text-black q-mb-sm">{{ model.infraInstance.url }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Deployment Owner</div>
                    <div class="text-black q-mb-sm">{{ model.deploymentOwner.person.fullName }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Approver</div>
                    <div class="text-black q-mb-sm">{{ model.approver.person.fullName }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Tester</div>
                    <div class="text-black q-mb-sm">{{ model.tester.person.fullName }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Release Type</div>
                    <div class="text-black q-mb-sm">{{ model.releaseType.dropDownValue }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Version Number</div>
                    <div class="text-black q-mb-sm">{{ model.versionNumber }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Planned Release Date</div>
                    <div class="text-black q-mb-sm">{{ model.plannedReleaseDate }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-12 col-md-12">
                    <div class="q-mb-xs">Name</div>
                    <div class="text-black q-mb-sm">{{ model.name }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Created By</div>
                    <div class="text-black q-mb-sm"> {{ model.createdBy.person.fullName }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Created Date</div>
                    <div class="text-black q-mb-sm">{{ model.createdOnUtc }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Updated By</div>
                    <div class="text-black q-mb-sm"> {{ model.updatedBy.person.fullName }}</div>
                  </div>
                  <div class="col-12 col-sm-6 col-md-6">
                    <div class="q-mb-xs">Updated Date</div>
                    <div class="text-black q-mb-sm">{{ model.updatedOnUtc }}</div>
                  </div>
                </div>
                <div class="row q-col-gutter-x-md q-mb-md">
                  <div class="col-12">
                    <div class="q-mb-xs">Description</div>
                    <p class="q-pt-md text-black RichTextEditor" v-html="model.description ? model.description : '-'" />
                  </div>
                </div>
              </fieldset>
            </q-tab-panel>
            <q-tab-panel name="2_tab">
              <div>
                <div class="q-mb-sm q-gutter-sm flex justify-end">
                  <q-input
                    v-model="filterItems" outlined class="bg-white q-mr-sm search-box" debounce="300" placeholder="Search"
                    dense clearable
                  >
                    <template #prepend>
                      <q-icon name="o_search" />
                    </template>
                  </q-input>
                </div>
                <q-table
                  ref="tableRef"
                  v-model:pagination="pagination"
                  bordered
                  class="no-shadow"
                  virtual-scroll
                  :loading="loading"
                  :rows="filteredRows"
                  :columns="columns"
                  row-key="id"
                  separator="cell"
                  binary-state-sort
                  :rows-per-page-options="[20, 50, 100, 200, 500]"
                >
                  <template #header="props">
                    <q-tr :props="props" class="bg-primary text-white">
                      <q-th
                        v-for="col in props.cols"
                        :key="col.name"
                        :props="props"
                      >
                        {{ col.label }}
                      </q-th>
                    </q-tr>
                  </template>
                  <template #body="props">
                    <q-tr>
                      <q-td style="width: 12%;">{{ props.row.type }}</q-td>
                      <q-td class="text-right" style="width: 8%;">#{{ props.row.number }}</q-td>
                      <q-td class="ellipsis-cell" style="overflow-wrap: break-word; word-wrap: break-word; white-space: normal; width: 60%;">
                        <div>
                          {{ isExpanded(props.row.id) ? props.row.name : truncateText(props.row.name) }}

                          <span
                            v-if="props.row.name?.length > 80"
                            class="text-primary cursor-pointer q-ml-xs"
                            @click="toggleExpand(props.row.id)"
                          >
                            {{ isExpanded(props.row.id) ? 'less' : '...more' }}
                          </span>
                        </div>
                      </q-td>
                      <q-td style="width: 10%;">{{ props.row.date }}</q-td>
                    </q-tr>
                  </template>
                </q-table>
              </div>
            </q-tab-panel>
          </q-tab-panels>
        </div>
      </div>
    </q-card>
  </q-dialog>
</template>

<script setup>
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted, watch, computed } from "vue";
import _ from "lodash";
import useFilters from "composables/useFilters";
import releaseTrackingService from "modules/project-release-tracking/projectReleaseTracking.service";

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Common variables
const loading = ref(true);
const { toDate } = useFilters();
const filterItems = ref("");

// Define model values
const model = ref({
  name: "-",
  description: "",
  createdOnUtc: "",
  project: {
    name: ""
  },
  infraInstance: {
    url: ""
  },
  deploymentOwner: {
    person: {
      fullName: ""
    }
  },
  tester: {
    person: {
      fullName: ""
    }
  },
  approver: {
    person: {
      fullName: ""
    }
  },
  releaseType: {
    dropDownValue: ""
  },
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

const releaseTrackingId = props.id;
const tab = ref("1_tab");
const expandedRows = ref(new Set());

const toggleExpand = (id) => {
  if (expandedRows.value.has(id)) {
    expandedRows.value.delete(id);
  } else {
    expandedRows.value.add(id);
  }
};

const isExpanded = (id) => {
  return expandedRows.value.has(id);
};
const rows = ref([]);
const pagination = ref({ sortBy: "", descending: false, rowsPerPage: 20, page: 1 });
const columns = ref([
  { name: "type", label: "Type", field: "type", align: "left" },
  { name: "number", label: "Number", field: "number", align: "left" },
  { name: "name", label: "Name", field: "name", align: "left" },
  { name: "date", label: "Date", field: "date", align: "left" }
]);

const truncateText = (text, length = 80) =>
  text?.length > length ? text.slice(0, length) + "..." : text || "";

// get release tracking details
const getReleaseTrackingInDetailsById = () => {
  loading.value = true;
  releaseTrackingService.getReleaseTrackingInDetailsById(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
    model.value.plannedReleaseDateStr = resp.plannedReleaseDate ? toDate(resp.plannedReleaseDate) : "";
    model.value.description = resp.description ? resp.description : "";
    rows.value = resp.mappingItems || [];
  }).finally(() => {
    loading.value = false;
  });
};

const loadMappedItems = async () => {
  try {
    const mappedList = await releaseTrackingService.getMappingByReleaseTrackingId(releaseTrackingId);

    rows.value = (mappedList || []).map(x => ({
      id: x.refId,
      type: x.type,
      name: x.name,
      number: x.number,
      date: x.date
    }));
  } catch (err) {
    console.error(err);
  }
};

const filteredRows = computed(() => {
  if (!filterItems.value) return rows.value;

  const search = filterItems.value.toLowerCase();

  return rows.value.filter(row =>
    row.type?.toLowerCase().includes(search) ||
    row.name?.toLowerCase().includes(search) ||
    row.number?.toString().includes(search) ||
    row.date?.toLowerCase().includes(search)
  );
});

watch(() => tab.value, async (newTab) => {
  if (newTab !== "2_tab") return;

  if (releaseTrackingId) {
    await loadMappedItems();
  }
});

// On page rendering
onMounted(() => {
  getReleaseTrackingInDetailsById();
});

</script>
