<template>
  <fieldset>
    <legend>{{ model.person ? 'Customer Notes' : 'Company Notes' }}</legend>
    <div class="q-px-md">
      <q-timeline color="secondary">
        <q-timeline-entry
          v-for="(notes, index) in noteList"
          :key="index"
          :subtitle="`${notes.createdOnUtc} - ${notes.user.person.firstName} ${notes.user.person.lastName}`"
          :icon="done_all"
          :color="'primary'"
        ><div class="fs-14">Notes: <span class="text-black RichTextEditor" v-html="notes.note" /></div>
          <div class="q-mt-xs fs-13"><i>{{ notes.type }}: <span class="text-black">{{ notes.sub_Module }}</span></i></div>
        </q-timeline-entry>
      </q-timeline>
    </div>
  </fieldset>
</template>
<script setup>
import { ref, onMounted } from "vue";
import _ from "lodash";

import commonService from "services/common.service";

const props = defineProps({ customerId: { type: String, default: "" }, isPerson: { type: Boolean, default: false } });
const customerId = props.customerId;

const model = ref({
  description: "",
  person: props.isPerson
});

const loading = ref(true);
const noteList = ref([]);
const getAllNoteByCustomerId = () => {
  commonService.getAllNoteByTypeAndRecord(customerId, "customer", true).then((resp) => {
    noteList.value = _.cloneDeep(resp);
  }).finally(() => {
    loading.value = false;
  });
};
onMounted(() => {
  getAllNoteByCustomerId();
});
</script>
