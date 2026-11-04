<template>
  <div class="tsr-filters">
    <div class="tsr-filters__field">
      <app-field-label label="Period" />
      <app-segmented-control
        :model-value="preset"
        :options="presetOptions"
        aria-label="Period preset"
        @update:model-value="$emit('update:preset', $event)"
      />
    </div>

    <register-week-picker
      v-if="preset === 'byWeek'"
      :model-value="weekEnd"
      @update:model-value="$emit('update:weekEnd', $event)"
    />

    <app-date-field
      v-if="preset === 'custom'"
      :model-value="from"
      label="From"
      :clearable="false"
      :max-date="to || ''"
      :error="!!rangeError"
      class="tsr-filters__date"
      @update:model-value="pickDate('from', $event)"
    />
    <app-date-field
      v-if="preset === 'custom'"
      :model-value="to"
      label="To"
      :clearable="false"
      :min-date="from || ''"
      :error="!!rangeError"
      :error-message="rangeError"
      class="tsr-filters__date"
      @update:model-value="pickDate('to', $event)"
    />
    <div v-if="preset === 'custom'" class="tsr-filters__field">
      <app-field-label label="&nbsp;" />
      <q-btn
        unelevated
        no-caps
        color="primary"
        label="Apply"
        class="tsr-filters__apply"
        :disable="!!rangeError"
        :loading="loading"
        @click="$emit('apply')"
      />
    </div>

    <app-select
      :model-value="filters.departments"
      label="Department"
      multiple
      use-input
      :placeholder="filters.departments.length ? '' : 'All departments'"
      :options="options.departments"
      class="tsr-filters__select"
      @update:model-value="set('departments', $event)"
    />
    <app-select
      :model-value="filters.employees"
      label="Employee"
      multiple
      use-input
      :placeholder="filters.employees.length ? '' : 'All employees'"
      :options="options.employees"
      class="tsr-filters__select tsr-filters__select--wide"
      @update:model-value="set('employees', $event)"
    />
    <app-select
      :model-value="filters.shifts"
      label="Shift"
      multiple
      use-input
      :placeholder="filters.shifts.length ? '' : 'All shifts'"
      info="The shift on the employee's designation record. Employees with no shift are judged with the default shift."
      :options="options.shifts"
      class="tsr-filters__select tsr-filters__select--narrow"
      @update:model-value="set('shifts', $event)"
    />
    <app-select
      :model-value="filters.states"
      label="Day state"
      multiple
      use-input
      :placeholder="filters.states.length ? '' : 'All states'"
      info="Keeps the employees who have at least one day in any of the chosen states within the period."
      :options="options.states"
      class="tsr-filters__select"
      @update:model-value="set('states', $event)"
    />
    <app-text-field
      :model-value="search"
      label="Search"
      placeholder="Name, code or department"
      clearable
      class="tsr-filters__search"
      @update:model-value="$emit('update:search', $event || '')"
    />

    <div class="tsr-filters__field">
      <app-field-label label="Density" />
      <app-segmented-control
        :model-value="density"
        :options="densityOptions"
        aria-label="Density"
        @update:model-value="$emit('update:density', $event)"
      />
    </div>
    <div class="tsr-filters__field">
      <app-field-label label="Columns" />
      <q-toggle
        :model-value="filters.hideHolidays"
        dense
        label="Hide Sundays & holidays"
        class="tsr-filters__toggle"
        @update:model-value="set('hideHolidays', $event)"
      />
    </div>
    <div v-if="activeCount" class="tsr-filters__field">
      <app-field-label label="&nbsp;" />
      <q-btn
        flat
        no-caps
        dense
        color="grey-8"
        icon="o_filter_alt_off"
        :label="activeCount === 1 ? 'Clear 1 filter' : `Clear ${activeCount} filters`"
        class="tsr-filters__clear"
        @click="$emit('clear')"
      />
    </div>
  </div>
</template>

<script setup>
// The register's filters: one wrapping row of labelled controls, aligned to the top, under the title bar.
// Everything but the period is applied in the browser to the rows already loaded, so a change shows at
// once; the period reloads (presets at once, Custom on Apply). The From/To pickers appear only for Custom,
// pre-filled with the range of the preset that was active.
import AppSegmentedControl from "components/common/AppSegmentedControl.vue";
import AppDateField from "components/common/AppDateField.vue";
import AppSelect from "components/common/AppSelect.vue";
import AppTextField from "components/common/AppTextField.vue";
import AppFieldLabel from "components/common/AppFieldLabel.vue";
import RegisterWeekPicker from "modules/analytics/components/register/_registerWeekPicker.vue";
import { PRESETS } from "modules/analytics/composables/useDateRangePresets";

const props = defineProps({
  preset: { type: String, default: "thisWeek" },
  from: { type: String, default: "" },
  to: { type: String, default: "" },
  // ISO Sunday that ends the week shown by the "By weeks" preset.
  weekEnd: { type: String, default: "" },
  loading: { type: Boolean, default: false },
  rangeError: { type: String, default: "" },
  // { departments: [], employees: [], shifts: [], states: [], hideHolidays: false }
  filters: { type: Object, required: true },
  // { departments: [{label,value}], employees: [...], shifts: [...], states: [...] }
  options: { type: Object, required: true },
  search: { type: String, default: "" },
  density: { type: String, default: "auto" },
  // How many filters are active, for the Clear button.
  activeCount: { type: Number, default: 0 }
});

const emit = defineEmits([
  "update:preset", "update:from", "update:to", "update:weekEnd", "apply",
  "update:filters", "update:search", "update:density", "clear"
]);

const presetOptions = PRESETS.map((p) => ({ value: p.value, label: p.label, icon: p.icon }));

const densityOptions = [
  { value: "auto", label: "Auto", tooltip: "Hours for up to two weeks, compact beyond" },
  { value: "hours", label: "Hours", icon: "o_view_comfy", tooltip: "Show the logged hours in every cell" },
  { value: "compact", label: "Compact", icon: "o_view_compact", tooltip: "Icons only; fits a whole month" }
];

function pickDate (which, value) {
  emit(`update:${which}`, value || "");
  if (props.preset !== "custom") emit("update:preset", "custom");
}

function set (key, value) {
  emit("update:filters", { ...props.filters, [key]: Array.isArray(props.filters[key]) ? (value || []) : value });
}
</script>

<style scoped>
/* One row, wrapping, every control top-aligned with its label above it. */
.tsr-filters {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-start;
  gap: 8px 12px;
  padding: 8px 14px 10px;
}
.tsr-filters__field {
  display: flex;
  flex-direction: column;
}
.tsr-filters__field :deep(.app-segmented) {
  height: 40px;
}
.tsr-filters__field :deep(.app-segmented__item) {
  min-height: 38px;
}
.tsr-filters__toggle {
  height: 40px;
}
.tsr-filters__date {
  width: 140px;
}
.tsr-filters__apply,
.tsr-filters__clear {
  height: 40px;
}
.tsr-filters__select {
  width: 190px;
}
.tsr-filters__select--wide {
  width: 230px;
}
.tsr-filters__select--narrow {
  width: 160px;
}
.tsr-filters__search {
  width: 210px;
}
</style>
