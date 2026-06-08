<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" full-height persistent position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw !important;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.jobTitle }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <!-- <q-card class="card-header with-tools headerBasic"> -->
          <fieldset>
            <legend>Job Post Info</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Job Title</div>
                <div class="text-black">
                  {{ model.jobTitle ? model.jobTitle : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Criteria</div>
                <div class="text-black">
                  {{ model.criteria ? model.criteria : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Job Status</div>
                <div class="text-black">
                  {{ model.isActive == true ? "Active" : "In Active" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Job Reference</div>
                <div class="text-black">
                  {{ model.jobReference ? model.jobReference : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Job Created Date</div>
                <div class="text-black">
                  {{ model.jobCreatedDate ? model.jobCreatedDate : "-" }}
                </div>
              </div>
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Job Published Date</div>
                <div class="text-black">
                  {{ model.publishedJobDate ? model.publishedJobDate : "-" }}
                </div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-12 col-md-12">
                <div class="q-mb-xs">Job Description:</div>
                <div class="text-black RichTextEditor">
                  <p v-html="model.jobDescription ? model.jobDescription : '-'" />
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
import jobPostService from "modules/job-post/jobPost.service";
// import useFilters from "composables/useFilters";

// Common variables
// const { toDate } = useFilters();
const loading = ref(true);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  isActive: true,
  jobTitle: "-",
  criteria: "",
  publishedJobDate: "-",
  jobCreatedDate: "-",
  jobReference: "-",
  jobDescription: ""
});
// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// On page rendering
onMounted(() => {
  getJobPost();
});

// get job post details
const getJobPost = () => {
  loading.value = true;
  jobPostService.getJobPost(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

</script>
<style>
.q-dialog__inner--minimized > div{
  max-height: calc(100vh) !important;
}
.q-dialog__inner--minimized{
  padding: 0;
}
</style>
