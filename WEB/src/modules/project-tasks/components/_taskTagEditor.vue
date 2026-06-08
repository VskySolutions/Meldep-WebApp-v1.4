<template>
  <q-select
    v-model="localTags"
    push
    class="w-100 h-auto"
    clearable
    use-input
    use-chips
    transition-show="jump-up"
    transition-hide="jump-up"
    hide-bottom-space
    :dense="true"
    multiple
    fill-input
    input-debounce="0"
    outlined
    emit-value
    map-options
    :options="availableTags"
    option-label="text"
    option-value="value"
    @update:model-value="val => emitUpdate(val)"
    @popup-hide="() => emit('close')"
    :popup-content-class="customPopupContentClass"
    @filter="(val, update, abort) => emit('filter', val, update, abort)"
  >
    <template #option="{ itemProps, opt, selected, toggleOption }">
      <q-item v-bind="itemProps">
        <q-item-section>
          <div class="row items-center q-col-gutter-x-md selection-drodown-item">
            <q-checkbox :model-value="selected" @update:model-value="toggleOption(opt)" />
            <span
              :style="{
                backgroundColor: opt.bgColor || '#e0e0e0',
                color: opt.color || '#191919',
                padding: '2px 6px',
                borderRadius: '4px',
                whiteSpace: 'normal',
                overflowWrap: 'break-word',
                maxWidth: '250px'
              }"
            >
              {{ opt.text }}
            </span>
          </div>
        </q-item-section>
      </q-item>
    </template>
    <template #selected-item="{ opt }">
      <div
        class="q-mx-xs q-mb-xs"
        :style="{
          backgroundColor: opt.bgColor || '#e0e0e0',
          color: opt.color || '#191919',
          padding: '4px 8px',
          borderRadius: '12px',
          display: 'inline-flex',
          alignItems: 'center'
        }"
      >
        <span>{{ opt.text }}</span>
      </div>
    </template>
  </q-select>
</template>

<script setup>
import { ref, watch } from "vue";

const props = defineProps({
  modelValue: Array,
  rowId: [String, Number],
  availableTags: Array
});

const emit = defineEmits(["update:modelValue", "save", "filter", "close"]);
const localTags = ref([...props.modelValue]);

watch(() => props.modelValue, val => {
  localTags.value = [...val];
});

function emitUpdate (val) {
  const normalized = normalizeValues(val);
  emit("update:modelValue", normalized);
  emit("save", { tags: normalized, rowId: props.rowId });
}

// function normalizeValues (val) {
//   return val.map(v => typeof v === "object" ? v : props.availableTags.find(t => t.value === v));
// }
function normalizeValues (val) {
  return val.map(v => {
    if (typeof v === "object") return v;

    // Lookup tag by value and copy full info (color, etc.)
    const match = props.availableTags.find(t => t.value === v);
    return match || { text: v, value: v, bgColor: "primary", color: "#191919" };
  });
}

</script>
