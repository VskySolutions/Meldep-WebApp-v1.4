<template>
  <q-card flat bordered>

    <!-- Header -->
    <q-card-section class="row bg-primary text-white q-pa-sm items-center justify-between">
      <div class="row items-center">
        <q-icon
          name="o_task_alt"
          size="sm"
          class="q-mr-md"
        />
        <div class="column">
          <div class="text-caption text-blue-2">
            • Answers
          </div>
        </div>
      </div>
    </q-card-section>

    <q-separator />
    <viewQuestionsAnswersTimeLineView
      :id="selectedQAId"
      />
  </q-card>
</template>

<script setup>
import { ref, watch } from "vue";
import _ from "lodash";

import viewQuestionsAnswersTimeLineView from "src/components/_project_question_answers_timeline_view.vue";

const props = defineProps({
  id: {
    type: String,
    required: true
  },
  requirementId: String,
  projectId: String
});

const selectedQAId = ref(props.id);

watch(
  () => props.id,
  (newId) => {
    selectedQAId.value = newId;
  },
  { immediate: true }
);

</script>
