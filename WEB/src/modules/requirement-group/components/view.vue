<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 q-mr-lg text-white">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
        <div class="q-pa-md cardTable">
          <div class="q-gutter-y-md">
            <fieldset>
              <legend>Requirement Group Info</legend>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                  <div class="q-mb-xs">Project Name</div>
                  <div class="text-black">
                    {{ model.project.name }}
                  </div>
                </div>
                <div class="col-12 col-sm-6 col-md-6 col-lg-6">
                  <div class="q-mb-xs">Requirement Group Name</div>
                  <div class="text-black">
                    {{ model.name }}
                  </div>
                </div>
              </div>
              <div class="row q-col-gutter-x-md q-mb-md">
                <div class="col-12">
                  <div class="q-mb-xs">Description:</div>
                  <div class="text-black RichTextEditor">
                    <p v-html="model.description ? model.description : '-'" />
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
// Import libraries
import { useDialogPluginComponent } from "quasar";
import { ref, onMounted } from "vue";
import _ from "lodash";
import requirementGroupsService from "modules/requirement-group/requirementGroup.service";
// import useFilters from "composables/useFilters";

// Common variables
// const { toDate } = useFilters();
const loading = ref(true);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "-",
  description: "",
  project: {
    name: ""
  }
});

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// On page rendering
onMounted(() => {
  getRequirementGroupDetails();
});

// get project details
const getRequirementGroupDetails = () => {
  loading.value = true;
  requirementGroupsService.getRequirementGroupDetails(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

</script>
