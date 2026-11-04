<template>
  <div class="app-field">
    <app-field-label :label="label" :required="required" :info="info" />
    <q-input
      v-model="display"
      mask="##/##/####"
      placeholder="mm/dd/yyyy"
      :rules="modelRules"
      :error="error"
      :error-message="errorMessage"
      :disable="disable"
      :readonly="readonly"
      :hint="hint"
      :clearable="clearable"
      :autocomplete="autocomplete"
      :aria-label="ariaLabel"
      outlined
      :dense="dense"
      hide-bottom-space
      class="app-date"
      @blur="onBlur"
    >
      <template #append>
        <q-icon
          name="o_calendar_month"
          size="21px"
          :class="['app-date__icon', { 'app-date__icon--locked': locked }]"
        >
          <q-tooltip v-if="!locked">Pick a date</q-tooltip>
          <q-popup-proxy
            v-if="!locked"
            ref="popupRef"
            cover
            transition-show="jump-down"
            transition-hide="jump-up"
          >
            <q-date
              v-model="isoModel"
              mask="YYYY-MM-DD"
              :options="dateOptions"
              :default-year-month="defaultYearMonth"
              today-btn
              minimal
              color="primary"
              class="app-date__calendar"
              @update:model-value="onPicked"
            >
              <div class="row items-center justify-end q-gutter-sm">
                <q-btn v-if="isoModel" flat dense no-caps size="sm" color="grey-8" label="Clear" @click="clear" />
                <q-btn v-close-popup flat dense no-caps size="sm" color="primary" label="Done" />
              </div>
            </q-date>
          </q-popup-proxy>
        </q-icon>
      </template>
    </q-input>
  </div>
</template>

<script setup>
// Standard date field. The box READS and accepts MM/DD/YYYY (the app's display format) while the model
// holds a calendar date in `modelFormat`: ISO "YYYY-MM-DD" by default, or "MM/DD/YYYY" for the Meldep
// endpoints that exchange *DateStr fields. Never a timestamp — this field has no time and no time zone.
import { ref, computed, toRef, watch } from "vue";
import AppFieldLabel from "components/common/AppFieldLabel.vue";
import { useFieldLabel } from "composables/useFieldLabel";
import { formatDateOnly, toIsoDate, isIsoDate } from "composables/useDateFormat";

const props = defineProps({
  modelValue: { type: String, default: "" },
  // "YYYY-MM-DD" (the standard) or "MM/DD/YYYY" (what the Meldep API stores in its *Str fields).
  modelFormat: { type: String, default: "YYYY-MM-DD" },
  label: { type: String, default: "" },
  required: { type: Boolean, default: false },
  info: { type: String, default: "" },
  // Run against the model value, not the displayed string.
  rules: { type: Array, default: () => [] },
  error: { type: Boolean, default: false },
  errorMessage: { type: String, default: "" },
  disable: { type: Boolean, default: false },
  readonly: { type: Boolean, default: false },
  hint: { type: String, default: "" },
  dense: { type: Boolean, default: true },
  clearable: { type: Boolean, default: true },
  autocomplete: { type: String, default: "off" },
  // A Start/End pair: each side is handed the other (in the model format), so the calendar offers only
  // days the pair can hold. Either may be empty.
  minDate: { type: String, default: "" },
  maxDate: { type: String, default: "" }
});

const emit = defineEmits(["update:modelValue", "blur"]);

const popupRef = ref(null);

// Neither readonly nor disabled opens the calendar.
const locked = computed(() => props.readonly || props.disable);

// ---- model ⇄ ISO ⇄ MM/DD/YYYY, all by string reformatting ----
const displayModel = computed(() => props.modelFormat === "MM/DD/YYYY");
const toIso = (value) => {
  if (!value) return "";
  if (isIsoDate(value)) return String(value).slice(0, 10);
  return toIsoDate(value) || "";
};
const fromIso = (iso) => (!iso ? "" : (displayModel.value ? formatDateOnly(iso, "") : iso));
const isoValue = computed(() => toIso(props.modelValue));

// What the box shows.
const display = ref(formatDateOnly(isoValue.value, ""));

watch(isoValue, (iso) => {
  const next = formatDateOnly(iso, "");
  if (next !== display.value) display.value = next;
});

watch(display, (text) => {
  const trimmed = String(text || "").trim();
  if (!trimmed) {
    if (props.modelValue) emit("update:modelValue", "");
    return;
  }
  const iso = toIsoDate(trimmed);
  if (iso && iso !== isoValue.value) emit("update:modelValue", fromIso(iso));
});

// Leaving the field with something that is not a date puts back what IS stored, so the box never sits
// there showing "06/2" as though it had been accepted.
const onBlur = (event) => {
  const trimmed = String(display.value || "").trim();
  if (trimmed && !toIsoDate(trimmed)) display.value = formatDateOnly(isoValue.value, "");
  emit("blur", event);
};

// The calendar binds the ISO value directly.
const isoModel = computed({
  get: () => isoValue.value,
  set: (val) => emit("update:modelValue", fromIso(val || ""))
});

// Picking a day is the whole interaction — the panel closes on it.
const onPicked = () => popupRef.value?.hide();

const minIso = computed(() => toIso(props.minDate));
const maxIso = computed(() => toIso(props.maxDate));

// q-date hands the options function "YYYY/MM/DD"; the bounds are ISO, so the slashes are swapped first.
const dateOptions = (date) => {
  const d = date.replace(/\//g, "-");
  return !(minIso.value && d < minIso.value) && !(maxIso.value && d > maxIso.value);
};

// An empty field opens on the month its bound sits in rather than on today's.
const defaultYearMonth = computed(() => {
  if (isoValue.value) return undefined;
  const anchor = minIso.value || maxIso.value;
  return anchor ? anchor.slice(0, 7).replace("-", "/") : undefined;
});

const clear = () => {
  emit("update:modelValue", "");
  popupRef.value?.hide();
};

// A caller's rules are written against the v-model, so they are handed that rather than the text shown.
const modelRules = computed(() => props.rules.map((rule) => () => rule(props.modelValue)));

const { text: ariaLabel } = useFieldLabel(toRef(props, "label"), toRef(props, "required"));
</script>

<style scoped>
.app-date :deep(.q-field__control) {
  min-height: 40px !important;
}
.app-date__icon {
  color: var(--q-primary);
  cursor: pointer;
}
.app-date__icon--locked {
  color: #9aa5b1;
  cursor: default;
}
.app-date__calendar {
  box-shadow: none;
  border-radius: 10px;
}
</style>
