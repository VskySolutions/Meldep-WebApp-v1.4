<template>
  <q-card flat bordered class="dashboard-card" style="border: 0.5px solid #1b75ab;">
    <div class="row q-col-gutter-lg q-pa-sm">
      <div class="col-10 q-ml-md">
        <div v-if="model.shortDescription" class="text-black RichTextEditor">
          <span v-html="model.shortDescription" />
        </div>
        <div v-else>
          <span class="text-grey">No short description available</span>
        </div>
      </div>
    </div>
  </q-card>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import _ from "lodash";
import requirementService from "modules/requirement/requirement.service";

const props = defineProps({
  requirementId: {
    type: String,
    required: true
  }
});

const loading = ref(false);

const model = ref({
  shortDescription: {}
});


// get get Requirement on edit mode
const getRequirement = () => {
  loading.value = true;
  requirementService.getRequirementDetails(props.requirementId).then((resp) => {
    model.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};

watch(
  () => props.requirementId,
  async () => {
    await getRequirement();
  },
  {
    immediate: true
  }
);

onMounted(async () => {
  await getRequirement();
});
</script>
