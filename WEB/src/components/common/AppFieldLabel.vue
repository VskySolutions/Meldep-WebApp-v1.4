<template>
  <div v-if="text" class="app-field-label">
    {{ text }}<span v-if="isRequired" class="app-field-label__star" aria-hidden="true">*</span>
    <!-- Explains a rule the control itself cannot show — where the values come from, what a field is used
         for elsewhere — on an icon rather than a caption line, so the field keeps its height. -->
    <q-icon v-if="info" name="o_info" size="14px" color="grey-6" class="app-field-label__info">
      <q-tooltip anchor="top middle" self="bottom middle" max-width="280px">{{ info }}</q-tooltip>
    </q-icon>
  </div>
</template>

<script setup>
// Standard external field label: sits at the top-left above the input (not inside it).
import { toRef } from "vue";
import { useFieldLabel } from "composables/useFieldLabel";

const props = defineProps({
  label: { type: String, default: "" },
  required: { type: Boolean, default: false },
  info: { type: String, default: "" }
});

const { text, isRequired } = useFieldLabel(toRef(props, "label"), toRef(props, "required"));
</script>

<style scoped>
.app-field-label {
  font-size: 12px;
  font-weight: 500;
  color: #423939;
  line-height: 1.2;
  margin-bottom: 4px;
}
.app-field-label__info {
  margin-left: 4px;
  cursor: help;
  vertical-align: text-bottom;
}
/* Mandatory marker: bigger than the label text and red. line-height:0 keeps the 18px glyph from growing
   the 12px line, so a required field's input lines up with the optional field beside it. */
.app-field-label__star {
  margin-left: 3px;
  color: #e53935;
  font-size: 18px;
  font-weight: 700;
  line-height: 0;
  vertical-align: baseline;
}
</style>
