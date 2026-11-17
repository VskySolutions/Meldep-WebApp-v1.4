<template>
  <q-select
    :model-value="modelValue"
    :options="options"
    :label="label"
    :multiple="multiple"
    :use-chips="multiple"
    outlined
    dense
    use-input
    emit-value
    map-options
    input-debounce="300"
    option-value="value"
    option-label="label"
    :aria-label="label"
    @filter="onFilter"
    @update:model-value="value => $emit('update:modelValue', value)"
  >
    <template #no-option>
      <q-item><q-item-section class="text-grey">No users found</q-item-section></q-item>
    </template>
  </q-select>
</template>

<script setup>
import { ref, onMounted } from "vue";
import securityAdminService from "../securityAdmin.service";

const props = defineProps({
  modelValue: { type: [String, Array], default: null },
  label: { type: String, default: "User" },
  multiple: { type: Boolean, default: false }
});
defineEmits(["update:modelValue"]);

const options = ref([]);

async function search (text) {
  const result = await securityAdminService.getUsers({ search: text, pageSize: 50 });
  return result.items.map(user => ({
    value: user.userId,
    label: user.name ? `${user.name} (${user.userName})` : user.userName
  }));
}

function onFilter (text, update, abort) {
  search(text).then(found => update(() => { options.value = found; })).catch(abort);
}

// A preselected user from a link needs a label even before the list has been opened.
onMounted(async () => {
  if (props.modelValue && !props.multiple) options.value = await search(props.modelValue).catch(() => []);
});
</script>
