<template>
  <q-icon
    name="o_account_balance_wallet"
    size="xs"
    class="cursor-pointer q-mr-sm"
  >
    <q-tooltip>Manage Wallet</q-tooltip>
    <q-popup-edit
      v-model="localWalletNumber"
      anchor="center middle"
      self="center middle"
      persistent
      class="wallet-popup"
    >
      <template #default="scope">
        <div style="min-width: 260px;" class="q-pa-sm">
          <!-- Header -->
          <div class="row justify-between items-center">
            <span class="text-subtitle2 q-mb-sm">
              Wallet Details
              <q-icon name="o_info" size="16px" class="q-ml-xs text-grey">
                <q-tooltip class="text-caption">
                  Either fill both Wallet Type and Wallet Number or leave both empty.
                </q-tooltip>
              </q-icon>
            </span>
            <q-btn icon="o_close" flat dense round size="sm" @click="scope.cancel" />
          </div>
          <!-- Wallet Type -->
          <formSingleSelectDropdown
            v-model="localRow.walletTypeId"
            label="Wallet Type"
            :required="!!scope.value"
            :options="walletOptions"
            :error="!!localRow.walletTypeError"
            :error-message="localRow.walletTypeError"
            @update:model-value="val => onWalletTypeChange(val, scope.value)"
          />

          <!-- Wallet Number -->
          <div class="q-mt-sm">
            <div class="field-label">
              Wallet Number
              <span v-if="localRow.walletTypeId" class="required">*</span>
            </div>

            <q-input
              v-model="scope.value"
              :type="showWallet ? 'text' : 'password'"
              outlined
              dense
              :error="!!localRow.walletNumberError"
              :error-message="localRow.walletNumberError"
              @update:model-value="val => onWalletNumberChange(val)"
            >
              <template #append>
                <q-icon
                  :name="showWallet ? 'o_visibility' : 'o_visibility_off'"
                  class="cursor-pointer"
                  @click="toggleWalletVisibility"
                />
              </template>
            </q-input>
          </div>
          <!-- Buttons -->
          <div class="row justify-end q-gutter-sm q-mt-md">
            <q-btn
              flat
              dense
              label="Cancel"
              color="primary"
              @click="scope.cancel"
            />
            <q-btn
              unelevated
              dense
              label="Save"
              color="primary"
              @click="() => onSave(scope)"
            />
          </div>
        </div>
      </template>
    </q-popup-edit>
  </q-icon>
</template>

<script setup>
import { ref, reactive, watch } from "vue";
// SOP Change :- Shared Inputs
import formSingleSelectDropdown from "src/components/form-inputs/_formSingleSelectDropdown.vue";

const props = defineProps({
  row: { type: Object, required: true },
  walletOptions: { type: Array, default: () => [] },
  onSaveApi: { type: Function, required: true }
});

const emit = defineEmits(["success"]);

const showWallet = ref(false);
const localWalletNumber = ref("");

const localRow = reactive({
  walletTypeId: null,
  walletTypeError: "",
  walletNumberError: ""
});

watch(
  () => props.row,
  (val) => {
    localRow.walletTypeId = val?.walletTypeId || null;
    localWalletNumber.value = val?.walletNumber || "";
  },
  { immediate: true }
);

// =================================================================================
// Validation
// =================================================================================
const walletHint = "Field is required";

function validateWallet (walletNumber) {
  const number = walletNumber?.trim();
  const isTypeFilled = !!localRow.walletTypeId;
  const isNumberFilled = !!number;

  localRow.walletTypeError = "";
  localRow.walletNumberError = "";

  if (!isTypeFilled && !isNumberFilled) return true;

  if (isTypeFilled && !isNumberFilled) {
    localRow.walletNumberError = walletHint;
    return false;
  }

  if (!isTypeFilled && isNumberFilled) {
    localRow.walletTypeError = walletHint;
    return false;
  }

  return true;
}

// =================================================================================
// Handlers
// =================================================================================
function onWalletTypeChange (val, walletNumber) {
  localRow.walletTypeId = val;
  validateWallet(walletNumber);
}

function onWalletNumberChange (val) {
  validateWallet(val);
}

function toggleWalletVisibility () {
  showWallet.value = !showWallet.value;
}

// =================================================================================
// Save
// =================================================================================
function onSave (scope) {
  if (!validateWallet(scope.value)) return;

  const payload = {
    walletTypeId: localRow.walletTypeId,
    walletNumber: scope.value?.trim(),
    isInstruction: false
  };

  props.onSaveApi(props.row, payload)
    .then((shouldRefresh) => {
      scope.set();

      if (shouldRefresh) {
        emit("success");
      }
    })
    .catch(() => {
    });
}
</script>
