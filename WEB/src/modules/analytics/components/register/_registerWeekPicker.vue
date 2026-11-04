<template>
  <div class="tsr-week">
    <app-field-label label="Week ending (Sunday)" />
    <div class="tsr-week__row">
      <q-btn flat round dense color="grey-8" icon="o_chevron_left" aria-label="Previous week" @click="step(-7)">
        <q-tooltip>Previous week</q-tooltip>
      </q-btn>
      <q-input
        :model-value="display"
        readonly
        dense
        outlined
        class="tsr-week__input"
        aria-label="Week ending Sunday"
        @click="popup?.show()"
      >
        <template #append>
          <q-icon name="o_calendar_month" size="21px" class="cursor-pointer">
            <q-tooltip>Pick any Sunday; the register shows that Monday to Sunday</q-tooltip>
            <q-popup-proxy ref="popup" cover transition-show="jump-down" transition-hide="jump-up">
              <q-date
                :model-value="modelValue"
                mask="YYYY-MM-DD"
                :first-day-of-week="1"
                :options="isPickable"
                :default-year-month="yearMonth"
                minimal
                color="primary"
                @update:model-value="pick"
              >
                <div class="row items-center justify-end q-gutter-sm">
                  <q-btn flat dense no-caps size="sm" color="primary" label="This week" @click="pick(thisSunday)" />
                  <q-btn v-close-popup flat dense no-caps size="sm" color="primary" label="Done" />
                </div>
              </q-date>
            </q-popup-proxy>
          </q-icon>
        </template>
      </q-input>
      <q-btn flat round dense color="grey-8" icon="o_chevron_right" aria-label="Next week" :disable="atLatest" @click="step(7)">
        <q-tooltip>{{ atLatest ? "This is the current week" : "Next week" }}</q-tooltip>
      </q-btn>
    </div>
  </div>
</template>

<script setup>
// Week picker for the "By weeks" period: the week is named by the Sunday that ends it (Meldep's weekly
// timesheets work the same way), only Sundays can be picked in the calendar, and the arrows step a week at
// a time up to the current week.
import { ref, computed } from "vue";
import AppFieldLabel from "components/common/AppFieldLabel.vue";
import { todayIso, addDays, sundayOf, formatDayLabel } from "modules/analytics/composables/useDateRangePresets";

const props = defineProps({
  // ISO "YYYY-MM-DD" of a Sunday.
  modelValue: { type: String, default: "" }
});

const emit = defineEmits(["update:modelValue"]);

const popup = ref(null);
const thisSunday = sundayOf(todayIso());

const display = computed(() => (props.modelValue ? formatDayLabel(props.modelValue) : ""));
const yearMonth = computed(() => (props.modelValue || thisSunday).slice(0, 7).replace("-", "/"));
const atLatest = computed(() => !props.modelValue || props.modelValue >= thisSunday);

// q-date hands the calendar day as "YYYY/MM/DD".
function isPickable (dateText) {
  const [y, m, d] = String(dateText).split("/").map(Number);
  if (!y || !m || !d) return false;
  const iso = `${y}-${String(m).padStart(2, "0")}-${String(d).padStart(2, "0")}`;
  return new Date(y, m - 1, d).getDay() === 0 && iso <= thisSunday;
}

function pick (value) {
  if (!value) return; // clicking the selected day again unselects it in q-date
  emit("update:modelValue", sundayOf(value));
  popup.value?.hide();
}

function step (days) {
  const base = props.modelValue || thisSunday;
  const next = addDays(base, days);
  if (next > thisSunday) return;
  emit("update:modelValue", next);
}
</script>

<style scoped>
.tsr-week {
  display: flex;
  flex-direction: column;
}
.tsr-week__row {
  display: flex;
  align-items: center;
  gap: 2px;
}
.tsr-week__input {
  width: 170px;
}
.tsr-week__input :deep(input) {
  cursor: pointer;
}
.tsr-week__input :deep(.q-field__control) {
  min-height: 40px !important;
}
</style>
