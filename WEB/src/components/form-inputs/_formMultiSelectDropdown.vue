<template>
  <div :class="props.wrapperClass">
    <label v-if="props.label" class="label q-mb-xs text-black">
      {{ props.label }}
      <span v-if="props.required" class="required">*</span>
    </label>

    <div>
      <q-select
        ref="selectRef"
        :model-value="props.modelValue"
        :options="displayOptions"
        option-value="value"
        option-label="text"
        :option-disable="props.optionDisable"
        emit-value
        map-options
        multiple
        use-chips
        use-input
        fill-input
        clearable
        outlined
        dense
        hide-bottom-space
        input-debounce="0"
        :disable="props.disable"
        :readonly="props.readonly"
        :error="props.error"
        :error-message="props.errorMessage"
        :popup-content-class="props.popupContentClass"
        @update:model-value="updateValue"
        @filter="handleFilter"
        @blur="props.onBlur"
      >
        <!-- Custom Option -->
        <template #option="{ itemProps, opt, selected, toggleOption }">
          <q-item v-bind="itemProps">
            <q-item-section>
              <div
                class="row q-col-gutter-x-md items-center"
                style="white-space: normal; overflow-wrap: break-word;"
              >
                <q-checkbox
                  :model-value="selected"
                  @update:model-value="toggleOption(opt)"
                />

                <span>{{ opt.text }}</span>
              </div>
            </q-item-section>
          </q-item>
        </template>
        <!-- Selected Role / Chip -->
        <template #selected-item="scope">
          <q-chip
            removable
            dense
            :tabindex="scope.tabindex"
            class="q-ma-xs"
            @remove="scope.removeAtIndex(scope.index)"
          >
            <span>{{ scope.opt.text }}</span>

            <q-icon
              v-if="props.showRoleAccess"
              name="o_info"
              size="16px"
              class="q-ml-xs cursor-pointer text-dark"
            >
              <q-tooltip
                anchor="top middle"
                self="bottom middle"
                :offset="[0, 6]"
              >
                <div class="role-access-tooltip">
                  <div v-if="getRoleAccess(scope.opt).fullAccess">
                    <q-icon name="o_edit" color="white" size="xs" class="q-mr-xs" />
                    <span>Manage all project-related data.</span>
                  </div>

                  <div v-if="getRoleAccess(scope.opt).viewOnly">
                    <q-icon name="o_visibility" color="white" size="xs" class="q-mr-xs" />
                    <span>View all project-related data.</span>
                  </div>

                  <div v-if="getRoleAccess(scope.opt).notes">
                    <q-icon name="o_assignment" color="white" size="xs" class="q-mr-xs" />
                    <span>Manage project-related notes.</span>
                  </div>
                </div>
              </q-tooltip>
            </q-icon>
          </q-chip>
        </template>
        <template #after>
          <slot name="after" />
        </template>
      </q-select>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, computed, nextTick } from "vue";

const selectRef = ref(null);

const props = defineProps({
  label: String,

  modelValue: {
    type: Array,
    default: () => []
  },

  options: {
    type: Array,
    default: () => []
  },

  required: {
    type: Boolean,
    default: true
  },

  optionDisable: {
    type: [String, Function],
    default: undefined
  },

  disable: Boolean,
  readonly: Boolean,
  error: Boolean,
  errorMessage: String,

  filter: Function,
  onBlur: Function,
  popupContentClass: String,

  showRoleAccess: {
    type: Boolean,
    default: false
  },
  /*
   * Existing dropdowns:
   * false -> keep search text after selection
   *
   * Project Charter:
   * true -> clear search after selection
   */
  clearSearchOnSelect: {
    type: Boolean,
    default: false
  },

  wrapperClass: {
    type: String,
    default: "col-xxl-4 col-lg-4 col-md-4 col-sm-4 col-xs-12"
  }
});

const emit = defineEmits(["update:modelValue"]);

const filteredOptions = ref([]);

/**
 * Keep local options synchronized with parent options.
 */
watch(
  () => props.options,
  (options) => {
    filteredOptions.value = [...(options || [])];
  },
  {
    immediate: true,
    deep: true
  }
);

const displayOptions = computed(() => filteredOptions.value);

/**
 * Selection changed.
 */
async function updateValue(val) {
  emit("update:modelValue", val || []);

  /*
   * Only clear the search box when explicitly requested.
   *
   * This is enabled only in Project Charter:
   *
   * :clear-search-on-select="true"
   */
  if (props.clearSearchOnSelect) {
    await nextTick();

    selectRef.value?.updateInputValue("");

    /*
     * Restore all options after clearing search.
     * This ensures the next search starts from the
     * complete list.
     */
    filteredOptions.value = [...(props.options || [])];
  }
}

/**
 * Search/filter.
 */
function handleFilter(val, update, abort) {
  const needle = String(val || "")
    .toLowerCase()
    .trim();

  /*
   * If a shared dropdown filter is supplied,
   * allow it to perform the actual filtering.
   */
  if (typeof props.filter === "function") {
    props.filter(
      val,
      () => {
        update(() => {
          /*
           * IMPORTANT:
           * Do not blindly clear the search value here.
           *
           * The parent filter updates its own list.
           * We use the latest props.options.
           */
          filteredOptions.value = [...(props.options || [])];
        });
      },
      abort
    );

    return;
  }

  /*
   * Local filtering fallback.
   */
  update(() => {
    if (!needle) {
      filteredOptions.value = [...(props.options || [])];
      return;
    }

    filteredOptions.value = (props.options || []).filter(option =>
      String(option.text || "")
        .toLowerCase()
        .includes(needle)
    );
  });
}

function getRoleAccess(role) {
  const permissions =
    role?.data ||
    role?.sitesProjectRolesPermissions ||
    role?.sitesProjectRoles?.sitesProjectRolesPermissions ||
    [];

  const permissionList = Array.isArray(permissions)
    ? permissions
    : [permissions];

  return {
    fullAccess: permissionList.some(
      permission => permission?.fullAccess === true
    ),

    viewOnly: permissionList.some(
      permission => permission?.viewOnly === true
    ),

    notes: permissionList.some(
      permission => permission?.notes === true
    )
  };
}
</script>
