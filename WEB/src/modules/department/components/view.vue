<template>
  <q-dialog ref="dialogRef" class="customDialog dialog-scrollable-content" persistent full-height position="right" @hide="onDialogHide">
    <q-card class="q-dialog-plugin PersonMain card-header with-tools headerBasic" style="width: 50vw;max-width: 50vw;">
      <q-card-section class="card-header with-tools bg-primary stickyHeader">
        <div class="text-h2 text-white">{{ model.name }}</div>
        <q-btn v-close-popup icon="o_close" class="close" color="white" flat round dense />
      </q-card-section>
      <q-separator />
      <div class="q-pa-md cardTable">
        <div class="q-gutter-y-md">
          <fieldset>
            <legend>Department Information</legend>
            <div class="row q-col-gutter-x-md q-mb-md">
              <div class="col-12 col-sm-6 col-md-6">
                <div class="q-mb-xs">Department Name</div>
                <div class="q-mb-xs text-black"> {{ model.name }}</div>
              </div>
            </div>
            <div class="row q-col-gutter-x-md">
              <div class="col-12">
                <div class="q-mb-xs">Description</div>
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
import departmentsService from "modules/department/department.service";
import { ref, onMounted } from "vue";
import _ from "lodash";

// Props values i.e. come from query string
const props = defineProps({ id: { type: String, default: "" } });

// Common variables
const tab = ref("1_tab");
const loading = ref(true);

// Define emits
defineEmits([...useDialogPluginComponent.emits]);
const { dialogRef, onDialogHide } = useDialogPluginComponent();

// Define model values
const model = ref({
  name: "",
  tab
});

// get department details on edit mode
const getDepartment = () => {
  loading.value = true;
  departmentsService.getDepartment(props.id).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

// On page rendering
onMounted(() => {
  getDepartment();
});

</script>

<style>
.q-dialog__inner--minimized>div {
  max-height: calc(100vh) !important;
}

.q-dialog__inner--minimized {
  padding: 0;
}
</style>
